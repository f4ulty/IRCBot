﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;
using System.IO;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Management;
using System.Reflection;

namespace IRCBot.Modules
{
    class owner : Module
    {
        System.Timers.Timer invalid_pass_timeout = new System.Timers.Timer();
        public override void control(bot ircbot, ref IRCConfig conf, int module_id, string[] line, string command, int nick_access, string nick, string channel, bool bot_command, string type)
        {
            string module_name = ircbot.conf.module_config[module_id][0];
            if ((type.Equals("channel") || type.Equals("query")) && bot_command == true)
            {
                foreach (List<string> tmp_command in conf.command_list)
                {
                    if (module_name.Equals(tmp_command[0]))
                    {
                        string[] triggers = tmp_command[3].Split('|');
                        int command_access = Convert.ToInt32(tmp_command[5]);
                        string[] blacklist = tmp_command[6].Split(',');
                        bool blocked = false;
                        bool cmd_found = false;
                        bool spam_check = Convert.ToBoolean(tmp_command[8]);
                        foreach (string bl_chan in blacklist)
                        {
                            if (bl_chan.Equals(channel))
                            {
                                blocked = true;
                                break;
                            }
                        }
                        if (spam_check == true)
                        {
                            if (ircbot.spam_activated == true)
                            {
                                blocked = true;
                            }
                        }
                        foreach (string trigger in triggers)
                        {
                            if (trigger.Equals(command))
                            {
                                cmd_found = true;
                                break;
                            }
                        }
                        if (blocked == false && cmd_found == true)
                        {
                            foreach (string trigger in triggers)
                            {
                                switch (trigger)
                                {
                                    case "owner":
                                        if (spam_check == true)
                                        {
                                            ircbot.spam_count++;
                                        }
                                        if (nick_access >= command_access)
                                        {
                                            if (line.GetUpperBound(0) > 3)
                                            {
                                                if (line[4].Equals(conf.pass))
                                                {
                                                    add_owner(nick, ircbot, ref conf);
                                                    ircbot.sendData("NOTICE", nick + " :You are now identified as an owner!");
                                                }
                                                else
                                                {
                                                    ircbot.sendData("NOTICE", nick + " :Invalid Password");
                                                }
                                            }
                                            else
                                            {
                                                ircbot.sendData("NOTICE", nick + " :" + nick + ", you need to include more info.");
                                            }
                                        }
                                        else
                                        {
                                            ircbot.sendData("NOTICE", nick + " :You do not have permission to use that command.");
                                        }
                                        break;
                                    case "addowner":
                                        if (spam_check == true)
                                        {
                                            ircbot.spam_count++;
                                        }
                                        if (nick_access >= command_access)
                                        {
                                            if (line.GetUpperBound(0) > 3)
                                            {
                                                add_owner(line[4].ToLower(), ircbot, ref conf);
                                                ircbot.sendData("NOTICE", nick + " :" + nick + " has been added as an owner.");
                                            }
                                            else
                                            {
                                                ircbot.sendData("NOTICE", nick + " :" + nick + ", you need to include more info.");
                                            }
                                        }
                                        else
                                        {
                                            ircbot.sendData("NOTICE", nick + " :You do not have permission to use that command.");
                                        }
                                        break;
                                    case "delowner":
                                        if (spam_check == true)
                                        {
                                            ircbot.spam_count++;
                                        }
                                        if (nick_access >= command_access)
                                        {
                                            if (line.GetUpperBound(0) > 3)
                                            {
                                                del_owner(line[4].ToLower(), ircbot, ref conf);
                                                ircbot.sendData("NOTICE", nick + " :" + nick + " has been removed as an owner.");
                                            }
                                            else
                                            {
                                                ircbot.sendData("NOTICE", nick + " :" + nick + ", you need to include more info.");
                                            }
                                        }
                                        else
                                        {
                                            ircbot.sendData("NOTICE", nick + " :You do not have permission to use that command.");
                                        }
                                        break;
                                    case "nick":
                                        if (spam_check == true)
                                        {
                                            ircbot.spam_count++;
                                        }
                                        if (nick_access >= command_access)
                                        {
                                            if (line.GetUpperBound(0) > 3)
                                            {
                                                ircbot.sendData("NICK", line[4]);
                                                ircbot.conf.nick = line[4];
                                            }
                                            else
                                            {
                                                ircbot.sendData("NOTICE", nick + " :" + nick + ", you need to include more info.");
                                            }
                                        }
                                        else
                                        {
                                            ircbot.sendData("NOTICE", nick + " :You do not have permission to use that command.");
                                        }
                                        break;
                                    case "id":
                                        if (spam_check == true)
                                        {
                                            ircbot.spam_count++;
                                        }
                                        if (nick_access >= command_access)
                                        {
                                            ircbot.identify();
                                            ircbot.sendData("NOTICE", nick + " :I have identified.");
                                        }
                                        else
                                        {
                                            ircbot.sendData("NOTICE", nick + " :You do not have permission to use that command.");
                                        }
                                        break;
                                    case "join":
                                        if (spam_check == true)
                                        {
                                            ircbot.spam_count++;
                                        }
                                        if (nick_access >= command_access)
                                        {
                                            if (line.GetUpperBound(0) > 3)
                                            {
                                                string[] chan_list = line[4].Replace(' ', ',').Split(',');
                                                foreach (string chan in chan_list)
                                                {
                                                    ircbot.channel_list.Add(chan);
                                                    ircbot.sendData("JOIN", chan);
                                                }
                                            }
                                            else
                                            {
                                                ircbot.sendData("NOTICE", nick + " :" + nick + ", you need to include more info.");
                                            }
                                        }
                                        else
                                        {
                                            ircbot.sendData("NOTICE", nick + " :You do not have permission to use that command.");
                                        }
                                        break;
                                    case "part":
                                        if (spam_check == true)
                                        {
                                            ircbot.spam_count++;
                                        }
                                        if (nick_access >= command_access)
                                        {
                                            if (line.GetUpperBound(0) > 3)
                                            {
                                                string[] channels = line[4].Split(' ');
                                                bool chan_found = false;
                                                foreach (string tmp_chan in channels)
                                                {
                                                    bool part_chan = false;
                                                    int index = 0;
                                                    foreach (string chan in ircbot.channel_list)
                                                    {
                                                        if (chan.Equals(tmp_chan))
                                                        {
                                                            part_chan = true;
                                                            chan_found = true;
                                                            index++;
                                                            break;
                                                        }
                                                    }
                                                    if (part_chan == true)
                                                    {
                                                        ircbot.sendData("PART", tmp_chan);
                                                        ircbot.channel_list.RemoveAt(index);
                                                    }
                                                }
                                                if (chan_found == false)
                                                {
                                                    ircbot.sendData("NOTICE", nick + " :I am not in that channel.");
                                                }
                                            }
                                            else if (type.Equals("channel"))
                                            {
                                                bool part_chan = false;
                                                int index = 0;
                                                foreach (string chan in ircbot.channel_list)
                                                {
                                                    if (chan.Equals(channel))
                                                    {
                                                        part_chan = true;
                                                        index++;
                                                        break;
                                                    }
                                                }
                                                if (part_chan == true)
                                                {
                                                    ircbot.sendData("PART", channel);
                                                    ircbot.channel_list.RemoveAt(index);
                                                }
                                                else
                                                {
                                                    ircbot.sendData("NOTICE", nick + " :I am not in that channel.");
                                                }
                                            }
                                            else
                                            {
                                                ircbot.sendData("NOTICE", nick + " :" + nick + ", you need to include more info.");
                                            }
                                        }
                                        else
                                        {
                                            ircbot.sendData("NOTICE", nick + " :You do not have permission to use that command.");
                                        }
                                        break;
                                    case "say":
                                        if (spam_check == true)
                                        {
                                            ircbot.spam_count++;
                                        }
                                        if (nick_access >= command_access)
                                        {
                                            if (line.GetUpperBound(0) > 3)
                                            {
                                                char[] charS = new char[] { ' ' };
                                                string[] new_line = line[4].Split(charS, 2);
                                                if (new_line[0].StartsWith("#") == true && new_line.GetUpperBound(0) > 0)
                                                {
                                                    ircbot.sendData("PRIVMSG", new_line[0] + " :" + new_line[1]);
                                                }
                                                else
                                                {
                                                    ircbot.sendData("PRIVMSG", channel + " :" + line[4]);
                                                }
                                            }
                                            else
                                            {
                                                ircbot.sendData("NOTICE", nick + " :" + nick + ", you need to include more info.");
                                            }
                                        }
                                        else
                                        {
                                            ircbot.sendData("NOTICE", nick + " :You do not have permission to use that command.");
                                        }
                                        break;
                                    case "action":
                                        if (spam_check == true)
                                        {
                                            ircbot.spam_count++;
                                        }
                                        if (nick_access >= command_access)
                                        {
                                            if (line.GetUpperBound(0) > 3)
                                            {
                                                char[] charS = new char[] { ' ' };
                                                string[] new_line = line[4].Split(charS, 2);
                                                if (new_line[0].StartsWith("#") == true && new_line.GetUpperBound(0) > 0)
                                                {
                                                    ircbot.sendData("PRIVMSG", new_line[0] + " :\u0001ACTION " + new_line[1] + "\u0001");
                                                }
                                                else
                                                {
                                                    ircbot.sendData("PRIVMSG", channel + " :\u0001ACTION " + line[4] + "\u0001");
                                                }
                                            }
                                            else
                                            {
                                                ircbot.sendData("NOTICE", nick + " :" + nick + ", you need to include more info.");
                                            }
                                        }
                                        else
                                        {
                                            ircbot.sendData("NOTICE", nick + " :You do not have permission to use that command.");
                                        }
                                        break;
                                    case "quit":
                                        if (spam_check == true)
                                        {
                                            ircbot.spam_count++;
                                        }
                                        if (nick_access >= command_access)
                                        {
                                            if (line.GetUpperBound(0) <= 3)
                                            {
                                                ircbot.sendData("QUIT", "Leaving");
                                            }
                                            else
                                            {
                                                ircbot.sendData("QUIT", ":" + line[4]); //if the command is quit, send the QUIT command to the server with a quit message
                                            }
                                            ircbot.worker.CancelAsync();
                                        }
                                        else
                                        {
                                            ircbot.sendData("NOTICE", nick + " :You do not have permission to use that command.");
                                        }
                                        break;
                                    case "update":
                                        if (spam_check == true)
                                        {
                                            ircbot.spam_count++;
                                        }
                                        if (nick_access >= command_access)
                                        {
                                            ircbot.ircbot.update_conf();
                                        }
                                        else
                                        {
                                            ircbot.sendData("NOTICE", nick + " :You do not have permission to use that command.");
                                        }
                                        break;
                                    case "modules":
                                        if (spam_check == true)
                                        {
                                            ircbot.spam_count++;
                                        }
                                        if (nick_access >= command_access)
                                        {
                                            string msg = "";
                                            foreach (Module module in ircbot.module_list)
                                            {
                                                msg += ", " + module.ToString().Remove(0, 15);
                                            }
                                            if (type.Equals("channel"))
                                            {
                                                if (msg != "")
                                                {
                                                    ircbot.sendData("PRIVMSG", channel + " :" + msg.TrimStart(',').Trim());
                                                }
                                                else
                                                {
                                                    ircbot.sendData("PRIVMSG", channel + " :No Modules are loaded.");
                                                }
                                            }
                                            else
                                            {
                                                if (msg != "")
                                                {
                                                    ircbot.sendData("NOTICE", nick + " :" + msg.TrimStart(',').Trim());
                                                }
                                                else
                                                {
                                                    ircbot.sendData("NOTICE", nick + " :No Modules are loaded.");
                                                }
                                            }
                                        }
                                        else
                                        {
                                            ircbot.sendData("NOTICE", nick + " :You do not have permission to use that command.");
                                        }
                                        break;
                                    case "loadmodule":
                                        if (spam_check == true)
                                        {
                                            ircbot.spam_count++;
                                        }
                                        if (nick_access >= command_access)
                                        {
                                            if (line.GetUpperBound(0) > 3)
                                            {
                                                bool module_found = ircbot.load_module(line[4]);
                                                if (module_found == true)
                                                {
                                                    if (type.Equals("channel"))
                                                    {
                                                        ircbot.sendData("PRIVMSG", channel + " :Module Loaded Successfully");
                                                    }
                                                    else
                                                    {
                                                        ircbot.sendData("NOTICE", nick + " :Module Loaded Successfully");
                                                    }
                                                }
                                                else
                                                {
                                                    if (type.Equals("channel"))
                                                    {
                                                        ircbot.sendData("PRIVMSG", channel + " :Error loading Module");
                                                    }
                                                    else
                                                    {
                                                        ircbot.sendData("NOTICE", nick + " :Error loading Module");
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (type.Equals("channel"))
                                                {
                                                    ircbot.sendData("PRIVMSG", channel + " :" + nick + ", you need to include more info.");
                                                }
                                                else
                                                {
                                                    ircbot.sendData("NOTICE", nick + " :" + nick + ", you need to include more info.");
                                                }
                                            }
                                        }
                                        else
                                        {
                                            ircbot.sendData("NOTICE", nick + " :You do not have permission to use that command.");
                                        }
                                        break;
                                    case "delmodule":
                                        if (spam_check == true)
                                        {
                                            ircbot.spam_count++;
                                        }
                                        if (nick_access >= command_access)
                                        {

                                            if (line.GetUpperBound(0) > 3)
                                            {
                                                bool module_found = ircbot.unload_module(line[4]);
                                                if (module_found == true)
                                                {
                                                    if (type.Equals("channel"))
                                                    {
                                                        ircbot.sendData("PRIVMSG", channel + " :Module Unloaded Successfully");
                                                    }
                                                    else
                                                    {
                                                        ircbot.sendData("NOTICE", nick + " :Module Unloaded Successfully");
                                                    }
                                                }
                                                else
                                                {
                                                    if (type.Equals("channel"))
                                                    {
                                                        ircbot.sendData("PRIVMSG", channel + " :No Module found");
                                                    }
                                                    else
                                                    {
                                                        ircbot.sendData("NOTICE", nick + " :No Module found");
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (type.Equals("channel"))
                                                {
                                                    ircbot.sendData("PRIVMSG", channel + " :" + nick + ", you need to include more info.");
                                                }
                                                else
                                                {
                                                    ircbot.sendData("NOTICE", nick + " :" + nick + ", you need to include more info.");
                                                }
                                            }
                                        }
                                        else
                                        {
                                            ircbot.sendData("NOTICE", nick + " :You do not have permission to use that command.");
                                        }
                                        break;
                                    case "addchan":
                                        if (spam_check == true)
                                        {
                                            ircbot.spam_count++;
                                        }
                                        if (nick_access >= command_access)
                                        {
                                            if (line.GetUpperBound(0) > 3)
                                            {
                                                bool in_chan = false;
                                                bool chan_allowed = true;
                                                foreach (string chan in ircbot.conf.chan_blacklist.Split(','))
                                                {
                                                    if (chan.Equals(line[4]))
                                                    {
                                                        chan_allowed = false;
                                                        break;
                                                    }
                                                }
                                                if (chan_allowed == true)
                                                {
                                                    foreach (string in_channel in ircbot.channel_list)
                                                    {
                                                        if (line[4].Equals(in_channel))
                                                        {
                                                            ircbot.sendData("NOTICE", nick + " :I'm already in that channel!");
                                                            in_chan = true;
                                                            break;
                                                        }
                                                    }
                                                    if (in_chan == false)
                                                    {
                                                        if (nick_access != conf.owner_level)
                                                        {
                                                            string[] owners = conf.owner.Split(',');
                                                            foreach (string owner_nick in owners)
                                                            {
                                                                ircbot.sendData("NOTICE", owner_nick + " :" + nick + " has invited me to join " + line[4]);
                                                            }
                                                        }
                                                        ircbot.channel_list.Add(line[4]);
                                                        ircbot.sendData("JOIN", line[4]);
                                                    }
                                                }
                                                else
                                                {
                                                    ircbot.sendData("NOTICE", nick + " :I am not allowed to join that channel.");
                                                }
                                            }
                                            else
                                            {
                                                ircbot.sendData("NOTICE", nick + " :" + nick + ", you need to include more info.");
                                            }
                                        }
                                        else
                                        {
                                            ircbot.sendData("NOTICE", nick + " :You do not have permission to use that command.");
                                        }
                                        break;
                                    case "channels":
                                        if (spam_check == true)
                                        {
                                            ircbot.spam_count++;
                                        }
                                        if (nick_access >= command_access)
                                        {
                                            bool chan_found = false;
                                            string chan_list = "";
                                            foreach (string in_channel in ircbot.channel_list)
                                            {
                                                if (conf.module_config[module_id][3].Equals("False") || nick_access == conf.owner_level) // if "hide 
                                                {
                                                    chan_list += in_channel + ", ";
                                                    chan_found = true;
                                                }
                                                else
                                                {
                                                    foreach (List<string> chan_types in ircbot.nick_list)
                                                    {
                                                        if (chan_types[0].Equals(in_channel))
                                                        {
                                                            if (chan_types[1].Equals("="))
                                                            {
                                                                chan_list += in_channel + ", ";
                                                                chan_found = true;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            if (chan_found == true)
                                            {
                                                if (type.Equals("channel"))
                                                {
                                                    if (nick_access == conf.owner_level)
                                                    {
                                                        ircbot.sendData("NOTICE", nick + " :I am currently in the following channels: " + chan_list.Trim().TrimEnd(','));
                                                    }
                                                    else
                                                    {
                                                        ircbot.sendData("PRIVMSG", channel + " :I am currently in the following channels: " + chan_list.Trim().TrimEnd(','));
                                                    }
                                                }
                                                else
                                                {
                                                    ircbot.sendData("NOTICE", nick + " :I am currently in the following channels: " + chan_list.Trim().TrimEnd(','));
                                                }
                                            }
                                            else
                                            {
                                                if (type.Equals("channel"))
                                                {
                                                    ircbot.sendData("PRIVMSG", channel + " :I'm in no channels.");
                                                }
                                                else
                                                {
                                                    ircbot.sendData("NOTICE", nick + " :I'm in no channels.");
                                                }
                                            }
                                        }
                                        else
                                        {
                                            ircbot.sendData("NOTICE", nick + " :You do not have permission to use that command.");
                                        }
                                        break;
                                    case "servers":
                                        if (spam_check == true)
                                        {
                                            ircbot.spam_count++;
                                        }
                                        if (nick_access >= command_access)
                                        {
                                            if (type.Equals("channel"))
                                            {
                                                ircbot.sendData("PRIVMSG", channel + " :I am currently in the following servers: " + ircbot.ircbot.full_server_list.Replace(",",", "));
                                            }
                                            else
                                            {
                                                ircbot.sendData("NOTICE", nick + " :I am currently in the following servers: " + ircbot.ircbot.full_server_list.Replace(",", ", "));
                                            }
                                        }
                                        else
                                        {
                                            ircbot.sendData("NOTICE", nick + " :You do not have permission to use that command.");
                                        }
                                        break;
                                    case "clear":
                                        if (spam_check == true)
                                        {
                                            ircbot.spam_count++;
                                        }
                                        if (nick_access >= command_access)
                                        {
                                            if (type.Equals("channel"))
                                            {
                                                foreach (List<string> tmp_nick in ircbot.nick_list)
                                                {
                                                    if (tmp_nick[0].Equals(channel))
                                                    {
                                                        for (int i = 1; i < tmp_nick.Count(); i++)
                                                        {
                                                            string[] split = tmp_nick[i].Split(':');
                                                            if (split.GetUpperBound(0) > 0)
                                                            {
                                                                if (split[1].Equals(nick) || split[1].Equals(conf.nick))
                                                                {
                                                                }
                                                                else
                                                                {
                                                                    ircbot.sendData("KICK", channel + " :" + split[1]);
                                                                }
                                                            }
                                                        }
                                                        break;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (line.GetUpperBound(0) > 3)
                                                {
                                                    foreach (List<string> tmp_nick in ircbot.nick_list)
                                                    {
                                                        if (tmp_nick[0].Equals(line[4]))
                                                        {
                                                            for (int i = 1; i < tmp_nick.Count(); i++)
                                                            {
                                                                string[] split = tmp_nick[i].Split(':');
                                                                if (split.GetUpperBound(0) > 0)
                                                                {
                                                                    if (split[1].Equals(nick) || split[1].Equals(conf.nick))
                                                                    {
                                                                    }
                                                                    else
                                                                    {
                                                                        ircbot.sendData("KICK", line[4] + " :" + split[1]);
                                                                    }
                                                                }
                                                            }
                                                            break;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    ircbot.sendData("NOTICE", nick + " :" + nick + ", you need to include more info.");
                                                }
                                            }
                                        }
                                        else
                                        {
                                            ircbot.sendData("NOTICE", nick + " :You do not have permission to use that command.");
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void add_owner(string nick, bot ircbot, ref IRCConfig conf)
        {
            XmlDocument xmlDoc = new XmlDocument();
            if (File.Exists(ircbot.cur_dir + Path.DirectorySeparatorChar + "config" + Path.DirectorySeparatorChar + "config.xml"))
            {
                xmlDoc.Load(ircbot.cur_dir + Path.DirectorySeparatorChar + "config" + Path.DirectorySeparatorChar + "config.xml");
                XmlNodeList ServerxnList = xmlDoc.SelectNodes("/bot_settings/connection_settings/server_list/server");
                foreach (XmlNode xn in ServerxnList)
                {
                    string tmp_server = xn["server_name"].InnerText;
                    if (tmp_server.Equals(conf.server))
                    {
                        string new_owner = xn["owner"].InnerText + "," + nick;
                        xn["owner"].InnerText = new_owner;
                        break;
                    }
                }
                xmlDoc.Save(ircbot.cur_dir + Path.DirectorySeparatorChar + "config" + Path.DirectorySeparatorChar + "config.xml");
                conf.owner += "," + nick;
            }
        }

        private void del_owner(string nick, bot ircbot, ref IRCConfig conf)
        {
            XmlDocument xmlDoc = new XmlDocument();
            string new_owner = "";
            if (File.Exists(ircbot.cur_dir + Path.DirectorySeparatorChar + "config" + Path.DirectorySeparatorChar + "config.xml"))
            {
                xmlDoc.Load(ircbot.cur_dir + Path.DirectorySeparatorChar + "config" + Path.DirectorySeparatorChar + "config.xml");
                XmlNodeList ServerxnList = xmlDoc.SelectNodes("/bot_settings/connection_settings/server_list/server");
                foreach (XmlNode xn in ServerxnList)
                {
                    string tmp_server = xn["server_name"].InnerText;
                    if (tmp_server.Equals(conf.server))
                    {
                        string[] new_owner_tmp = xn["owner"].InnerText.Split(',');
                        for (int x = 0; x <= new_owner_tmp.GetUpperBound(0); x++)
                        {
                            if (new_owner_tmp[x].Equals(nick))
                            {
                            }
                            else
                            {
                                new_owner += new_owner_tmp[x] + ",";
                            }
                        }
                        xn["owner"].InnerText = new_owner.TrimEnd(',');
                        break;
                    }
                }
                xmlDoc.Save(ircbot.cur_dir + Path.DirectorySeparatorChar + "config" + Path.DirectorySeparatorChar + "config.xml");
                conf.owner = new_owner.TrimEnd(',');
            }
        }
    }
}
