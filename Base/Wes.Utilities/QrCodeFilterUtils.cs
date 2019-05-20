using System;
using System.Collections.Generic;
using Wes.Utilities.Exception;

namespace Wes.Utilities
{
    public class QrCodeFilterUtils
    {

        /**
         * 解析QrCode LoadingNo
         */
        public static string ResolverCommand(String command, object currentScanTarget, bool isNecessary = false)
        {
            //qrCode
            if (command.Length > 13 && command.Contains(","))
            {
                if (command.Contains("PXT") && command.Contains("TXT") && isNecessary == false)
                {
                    return command;
                }
                else
                {
                    int scanTarget = Convert.ToInt32(currentScanTarget);
                    string[] commands = command.Split(',');
                    string commandPrefix = string.Empty;
                    switch (scanTarget)
                    {
                        case 1:
                            commandPrefix = "PXT";
                            break;
                        case 2:
                            commandPrefix = "TXT";
                            break;
                        case 11:
                            commandPrefix = "RXT";
                            break;
                        case 21:
                            commandPrefix = "SXT";
                            break;
                        default:
                            return command;
                    }

                    if (commands.Length > 0)
                    {
                        for (int i = 0; i < commands.Length; i++)
                        {
                            if (commands[i].ToUpper().StartsWith(commandPrefix))
                            {
                                return commands[i].ToUpper();
                            }
                        }
                    }
                }
            }
            else
            {
                return command;
            }

            throw new WesException("目前不支持的QrCode格式");
        }

        /// <summary>
        /// 解析QrCode中的作業單號
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static List<string> ResolverQrCode(String command)
        {
            List<string> workNos = new List<string>();
            //qrCode
            if (command.Length > 13 && command.Contains(","))
            {
                try
                {
                    string[] commands = command.Split(',');
                    HashSet<string> commandPrefix = new HashSet<string>() { "PXT", "TXT", "RXT", "SXT" };
                    if (commands.Length > 0)
                    {
                        for (int i = 0; i < commands.Length; i++)
                        {
                            foreach (var item in commandPrefix)
                            {
                                if (commands[i].ToUpper().StartsWith(item))
                                {
                                    workNos.Add(commands[i].ToUpper().Trim());
                                }
                            }
                        }
                    }
                }
                catch
                {
                    throw new WesException("目前不支持的QrCode格式");
                }
            }
            else
            {
                workNos.Add(command);
            }
            return workNos;
        }
    }
}
