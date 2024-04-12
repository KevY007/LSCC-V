using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GTANetworkAPI;
[assembly: MethodCallLogger(AttributeTargetTypes = "RNC.*", AttributeTargetMembers = "*")]

namespace RNC
{
    public class AC_Events : Script
    {
        [RemoteEvent("AC_WeaponShot:BulletTrace")]
        public void AC_OnWeaponShotBulletTrace(Player player, double tX, double tY, double tZ, bool hit)
        {
            if (!player.HasSharedData("Admin_BulletTracers")) return;

            List<int> tracerIDs = NAPI.Util.FromJson<List<int>>(player.GetSharedData<string>("Admin_BulletTracers"));
            if (tracerIDs.Count == 0) return;

            List<Player> admins = Settings.LoggedInPlayers.Where(p => tracerIDs.Contains(p.Id)).ToList();
            foreach (Player a in admins)
            {
                a.TriggerEvent("BulletTrace:AddLine", player, tX, tY, tZ, hit);
            }
        }        

        [RemoteEvent("AC_SendWeaponHackAlert")]
        public void AC_OnReceiveWeaponHackAlert(Player player, string hackedWeaponStr, int type)
        {
            string modeStr = "Lobby";
            if(player.GetSharedDataSafe<int>("InLobby") >= 0)
            {
                if(player.GetSharedDataSafe<int>("Team") == 1 || player.GetSharedDataSafe<int>("Team") == 0)
                {
                    modeStr = $"Copchase (Team: {player.GetSharedDataSafe<int>("Team")})";
                }
                if(player.GetSharedDataSafe<int>("Wounded") > 0)
                {
                    modeStr += " (Wounded)";
                }

                if(player.GetSharedDataSafe<int>("InDM") > -1)
                {
                    modeStr = $"Deathmatch (Arena: {player.GetSharedDataSafe<int>("InDM")})";
                }
            }
            else if(player.GetSharedDataSafe<int>("InLobby") == -1)
            {
                modeStr = "Freeroam";
            }

            if (player.Dead)
            {
                modeStr += " (Dead/Respawning)";
            }

            long hackedWeapon = Int64.Parse(hackedWeaponStr);
            string weaponName = "";
            bool weaponFound = Main.WeaponReasons.TryGetValue(hackedWeapon, out weaponName);
            if (weaponFound)
            {
                EPlayer.SendMessageToAllAdmins($"[ANTICHEAT] {player.Name} might be using a hacked weapon! ({weaponName.ToUpper()}) ({type} | {modeStr})");
                DiscordBot.SendAlert($"**[ANTICHEAT]** {player.Name} might be using weapon hacks! *({weaponName.ToUpper()})* *({type} | {modeStr})*", DiscordBot.EmbedType.Warning);
            }
            else
            {
                EPlayer.SendMessageToAllAdmins($"[ANTICHEAT] {player.Name} might be using a hacked weapon! ({hackedWeapon.ToString()}) ({type} | {modeStr})");
                DiscordBot.SendAlert($"**[ANTICHEAT]** {player.Name} might be using weapon hacks! *({hackedWeapon.ToString()})* *({type} | {modeStr})*", DiscordBot.EmbedType.Warning);
            }

        }
    }
}
