using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GTANetworkAPI;

namespace RNC
{
    public class AC_Events : Script
    {
        [RemoteEvent("AC_SendWeaponHackAlert")]
        public void AC_OnReceiveWeaponHackAlert(Player player, long hackedWeapon)
        {
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
