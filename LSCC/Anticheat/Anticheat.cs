using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
[assembly: MethodCallLogger(AttributeTargetTypes = "RNC.*", AttributeTargetMembers = "*")]

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

        public static void AC_ForceRemoveTracer(Player target, int id)
        {
            if (!target.HasSharedData("Admin_BulletTracers"))
                target.SetSharedData("Admin_BulletTracers", NAPI.Util.ToJson(new List<int>()));

            List<int> tracerIDs = NAPI.Util.FromJson<List<int>>(target.GetSharedData<string>("Admin_BulletTracers"));
            if (tracerIDs.Contains(id)) tracerIDs.Remove(id);
            target.SetSharedData("Admin_BulletTracers", NAPI.Util.ToJson(tracerIDs));
        }
        public static void AC_ClearBulletTraces(Player player)
        {
            player.TriggerEvent("BulletTrace:ClearLines");
        }
        public static void AC_ToggleBulletTracing(Player player, Player target, bool toggle)
        {
            if (EPlayer.GetAccount(player).Admin < 1) return;

            if (!target.HasSharedData("Admin_BulletTracers"))
                target.SetSharedData("Admin_BulletTracers", NAPI.Util.ToJson(new List<int>()));


            List<int> tracerIDs = NAPI.Util.FromJson<List<int>>(target.GetSharedData<string>("Admin_BulletTracers"));
            if (toggle == true)
            {
                if (tracerIDs.Contains(player.Id))
                {
                    player.SendChatMessage("!{#ff6347}[!] !{#ffffff}Bullet tracing for this player is already on!");
                    return;
                }

                tracerIDs.Add(player.Id);
                target.SetSharedData("Admin_BulletTracers", NAPI.Util.ToJson(tracerIDs));
                AC_ClearBulletTraces(player);
                player.SetSharedData("Admin_TraceTarget", target.Id);
                player.SendChatMessage($"!{{#ff6347}}[!] !{{#ffffff}}Bullet tracing for {target.Name} has been turned !{{#00ff00}}ON!{{#ffffff}}!");
            }
            else
            {
                if (!tracerIDs.Contains(player.Id))
                {
                    player.SendChatMessage("!{#ff6347}[!] !{#ffffff}Bullet tracing for this player is already off!");
                    return;
                }


                tracerIDs.Remove(player.Id);
                target.SetSharedData("Admin_BulletTracers", NAPI.Util.ToJson(tracerIDs));
                AC_ClearBulletTraces(player);
                player.SetSharedData("Admin_TraceTarget", -1);
                player.SendChatMessage($"!{{#ff6347}}[!] !{{#ffffff}}Bullet tracing for {target.Name} has been turned !{{#ff0000}}OFF!{{#ffffff}}!");
            }
        }
    }
}
