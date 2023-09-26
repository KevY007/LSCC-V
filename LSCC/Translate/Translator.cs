using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using GTANetworkAPI;

using Newtonsoft.Json;
using GTANetworkMethods;

namespace RNC
{
    public class Translator : Script
    {
        public static string GetLanguageNameByID(int ID, bool localized = false)
        {
            string LANG = "";
            if (!localized)
            {
                switch (ID)
                {
                    case 0: LANG = "English"; break;
                    case 1: LANG = "Spanish"; break;
                    case 2: LANG = "French"; break;
                    case 3: LANG = "Croatian"; break;
                    case 4: LANG = "Vietnamese"; break;
                    default: LANG = "English"; break;
                }
            }
            else
            {
                switch (ID)
                {
                    case 0: LANG = "English"; break;
                    case 1: LANG = "Español"; break;
                    case 2: LANG = "Français"; break;
                    case 3: LANG = "Hrvatski"; break;
                    case 4: LANG = "Tiếng Việt"; break;
                    default: LANG = "English"; break;
                }
            }
            return LANG;
        }

        public static string Translate(int languageID, string message_code, params object[] args)
        {
            try
            {
                string LANG = GetLanguageNameByID(languageID);
                // Switch-case for other languages.

                using (FileStream stream = new FileStream($"client_packages/Languages/Server/{LANG}.json", FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader sr = new StreamReader(stream))
                    {
                        using (JsonTextReader jsonReader = new JsonTextReader(sr))
                        {
                            var langs = new JsonSerializer().Deserialize<Dictionary<string, string>>(jsonReader);

                            KeyValuePair<string, string>? kvp = langs.FirstOrDefault(k => k.Key == message_code);
                            if (kvp != null)
                            {
                                string unformatted = kvp?.Value;

                                string message = ApplyStringArguments(unformatted, args);
                                return message;
                            }
                            return $"[ERROR] This message is not defined: {message_code} | Language: {LANG}";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput(ex.ToString());
                return $"[ERROR] Error sending message: {ex.Message} | SEND THIS TO THE DEVS.";
            }
        }

        public static string Translate(GTANetworkAPI.Player player, string message_code, params object[] args)
        {
            try
            {
                int languageID = (player.HasSharedData("Language") ? player.GetSharedDataSafe<int>("Language") : 0);
                return Translate(languageID, message_code, args);
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput(ex.ToString());
                return $"[ERROR] Error sending message: {ex.Message} | SEND THIS TO THE DEVS.";
            }
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
