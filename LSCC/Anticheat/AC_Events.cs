using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GTANetworkAPI;

namespace RNC
{
    public class AC_Events : Script
    {
        [RemoteEvent("AC_WeaponShot:BulletTrace")]
        public void AC_OnWeaponShotBulletTrace(Player player, float tX, float tY, float tZ, bool hit)
        {
            if (!player.HasSharedData("Admin_BulletTracers")) return;

            List<int> tracerIDs = NAPI.Util.FromJson<List<int>>(player.GetSharedData<string>("Admin_BulletTracers"));
            if (tracerIDs.Count == 0) return;

            List<Player> admins = Settings.LoggedInPlayers.Where(p => tracerIDs.Contains(p.Id)).ToList();
            foreach(Player a in admins)
            {
                a.TriggerEvent("BulletTrace:AddLine", player, tX, tY, tZ, hit);
            }
        }        

        [RemoteEvent("AC_SendWeaponHackAlert")]
        public void AC_OnReceiveWeaponHackAlert(Player player, string hackedWeaponStr)
        {
            long hackedWeapon = Int64.Parse(hackedWeaponStr);
            string weaponName = "";
            bool weaponFound = Main.WeaponReasons.TryGetValue(hackedWeapon, out weaponName);
            if (weaponFound)
            {
                EPlayer.SendMessageToAllAdmins($"[ANTICHEAT] {player.Name} might be using a hacked weapon! ({weaponName.ToUpper()})");
                DiscordBot.SendAlert($"**[ANTICHEAT]** {player.Name} might be using weapon hacks! *({weaponName.ToUpper()})*", DiscordBot.EmbedType.Warning);
            }
            else
            {
                EPlayer.SendMessageToAllAdmins($"[ANTICHEAT] {player.Name} might be using a hacked weapon! ({hackedWeapon.ToString()})");
                DiscordBot.SendAlert($"**[ANTICHEAT]** {player.Name} might be using weapon hacks! *({hackedWeapon.ToString()})*", DiscordBot.EmbedType.Warning);
            }

        }
    }
}
