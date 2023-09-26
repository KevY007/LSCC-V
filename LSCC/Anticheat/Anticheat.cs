using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;

namespace RNC
{
    public static class Anticheat
    {
        public static void GiveWeaponAC(this Player player, WeaponHash weapon, int ammo)
        {
            if (!player.HasSharedData("AC_WeaponData"))
            {
                List<WeaponHash> weapons = new List<WeaponHash>();
                weapons.Add(weapon);
                player.SetSharedData("AC_WeaponData", NAPI.Util.ToJson(weapons));
            }
            else
            {
                List<WeaponHash> weapons = NAPI.Util.FromJson<List<WeaponHash>>(player.GetSharedDataSafe<string>("AC_WeaponData"));
                if (!weapons.Contains(weapon)) { weapons.Add(weapon); }
                player.SetSharedData("AC_WeaponData", NAPI.Util.ToJson(weapons));
            }

            player.GiveWeapon(weapon, ammo);
        }
        public static void RemoveAllWeaponsAC(this Player player)
        {
            List<WeaponHash> weapons = new List<WeaponHash>();
            player.SetSharedData("AC_WeaponData", NAPI.Util.ToJson(weapons));

            player.RemoveAllWeapons();
        }
        public static void RemoveWeaponAC(this Player player, WeaponHash weapon)
        {
            if (player.HasSharedData("AC_WeaponData"))
            {
                List<WeaponHash> weapons = NAPI.Util.FromJson<List<WeaponHash>>(player.GetSharedDataSafe<string>("AC_WeaponData"));
                if (weapons.Contains(weapon)) { weapons.Remove(weapon); }
                player.SetSharedData("AC_WeaponData", NAPI.Util.ToJson(weapons));
            }

            player.RemoveWeapon(weapon);
        }
    }
}
