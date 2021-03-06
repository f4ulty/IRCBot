﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;
using ExtensionMethods;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IRCBot.Modules
{
    class _4chan : Module
    {
        public override void control(bot ircbot, ref IRCConfig conf, int module_id, string[] line, string command, int nick_access, string nick, string channel, bool bot_command, string type)
        {
            string module_name = ircbot.conf.module_config[module_id][0];
            string board = "";
            int thread = 0;
            int page = 0;
            if (type.Equals("channel") && bot_command == true)
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
                                    case "4chan":
                                        if (spam_check == true)
                                        {
                                            ircbot.spam_count++;
                                        }
                                        if (nick_access >= command_access)
                                        {
                                            if (line.GetUpperBound(0) > 3)
                                            {
                                                string[] args = line[4].Split(' ');
                                                board = "";
                                                thread = 0;
                                                page = 0;
                                                if (args.GetUpperBound(0) > 0)
                                                {
                                                    try
                                                    {
                                                        board = args[0];
                                                        thread = Convert.ToInt32(args[1]);
                                                        get_thread(channel, ircbot, board, 0);
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        board = args[0];
                                                        get_board(channel, ircbot, board, 0);
                                                    }
                                                }
                                                else
                                                {
                                                    board = args[0];
                                                    get_board(channel, ircbot, board, 0);
                                                }
                                            }
                                            else
                                            {
                                                ircbot.sendData("PRIVMSG", line[2] + " :" + nick + ", you need to include more info.");
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

        private void get_thread(string channel, bot ircbot, string board, int thread)
        {
        }

        private void get_board(string channel, bot ircbot, string board, int page)
        {
            string uri = "https://api.4chan.org/" + board + "/" + page.ToString();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "GET";

            Stream requestStream = request.GetRequestStream();
            requestStream.Write(null, 0, 0);
            requestStream.Close();

            string reply = "";
            try
            {
                // grab te response and print it out to the console along with the status code
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                reply = new StreamReader(response.GetResponseStream()).ReadToEnd();
                XmlDocument xmlDoc = JsonConvert.DeserializeXmlNode(reply);
            }
            catch (WebException ex)
            {
                reply = ex.Message;
            }
        }
    }
}
