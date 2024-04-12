using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
[assembly: MethodCallLogger(AttributeTargetTypes = "RNC.*", AttributeTargetMembers = "*")]

namespace RNC
{
    public class AC_Testing : Script
    {
        [Command("ac_weapontest")]
        public void cmd_anticheat_weapontest(Player player, Int64 hash, int ammo = 500)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 6)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not a level 6 admin.");
                return;
            }
            player.GiveWeapon((WeaponHash)hash, ammo);
            player.SendChatMessage("!{red}[AC-TEST] !{white}Given you a 'hacked' weapon!");
        }
    }
}
