using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;

using RNC.Translate.Languages;

namespace RNC
{
    public class Translator : Script
    {
        public static string Translate(Player player, string message_code, params object[] args)
        {
            object[,] LANG = English.MESSAGES;
            // Switch-case for other languages.

            for(int i = 0; i < LANG.GetLength(0); i++)
            {
                if(LANG[i,0].ToString() == message_code)
                {
                    string message = ApplyStringArguments(LANG[i,1].ToString(), args);
                    return message;
                }
            }
            return $"[ERROR] This message is not defined: {message_code} | Language: English";
        }

        private static string ApplyStringArguments(string text, params object[] args)
        {
            if (args.Length == 0) return text;

            int currentArgIdx = 0;
            bool isFormat = false;
            string toReturn = "";
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '{')
                {
                    if (text[i + 1] != '#')
                    {
                        isFormat = true;
                    }
                }

                if (!isFormat) toReturn += text[i];

                if (text[i] == '}' && isFormat)
                {
                    isFormat = false;

                    if (currentArgIdx < args.Length)
                    {
                        toReturn += args[currentArgIdx];
                        currentArgIdx++;
                    }
                    else
                    {
                        toReturn += "{MISSING TRANSLATION ARGUMENT}";
                    }

                }
            }
            return toReturn;
        }
    }
}
