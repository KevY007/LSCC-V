
const CommandsDataJSON = require('./ServerUI/CommandsList');

/* Camera object */
function isHeliCamActive(){
	if (global.heliCam === null || !global.heliCam.isActive() || !global.heliCam.isRendering()) return false;
  return true;
}

function traceLastFunc(iStr)
{
    mp.storage.data.lastFn = iStr;
    mp.storage.flush();
}

// When 'E' key is held down, open the menu
function actionMenuOpen(){
    mp.events.callLocal("actionMenu:open"); 
}
// When 'E' key is let go, close the menu - toggleable though.
function actionMenuClose(){
    if(global.chatStatus == true || global.menuToggled == true || global.MDCActive == true || global.scoreboardToggled == true
        || global.VehicleSpawnerActive == true || global.WeaponEditorActive == true || global.isHotwiring == true || global.ClothingEditorActive == true) return;

    mp.events.callLocal("actionMenu:close"); 
}
function UpdateStatsVars(entity, all = true){
    // yes im double checking the same shit cuz for some reason this shit decided to break for no reason
    if(global.ServerUI == null || global.ServerUI == undefined || global.ServerUI == undefined || global.ServerUI == null) return;
    entity = mp.players.local;
    const player = mp.players.local;
    if(entity !== undefined){
        if(mp.players.exists(entity))
        {
            if(global.ServerUI !== undefined && global.ServerUI != null){

              let muted_players = [];
              mp.players.forEach(pl => {
                if(mp.players.local.getVariable(`VoiceMute${pl.name}`) && mp.players.local.getVariable(`VoiceMute${pl.name}`) == true)
                  muted_players.push(pl.name);
              });

              const user_object = {
                serverId: player.remoteId,
                id: player.getVariable("SQL_ID"),
                name: player.name,
                level: player.getVariable("Level"),
                fugitive: (player.getVariable("Team") == 0).toString(),
                xp: player.getVariable("CurrentXP"),
                nextLevelXp: player.getVariable("NextLevelXP"),
                createdAt: player.getVariable("RegisterDate"),
                points: player.getVariable("Points"),
                admin: player.getVariable("Admin"),
                donator: player.getVariable("Donator"),
                donatorPoints: player.getVariable("DonatorPoints"),
                diamondPoints: player.getVariable("DonatorPoints"),
                api: player.getVariable("ScreenshotServerAPI"),
                lobbyId: player.getVariable("InLobby"),
                teamId: player.getVariable("Team"),
                heading: player.heading,
                voiceMutedPlayers:muted_players,
                location:{
                    x: player.position.x,
                    y: player.position.y,
                    z: player.position.z,
                },
                clan:{
                  id: player.getVariable("ClanID"),
                  tier: player.getVariable("ClanTier"),
                  name:player.getVariable("ClanName"),
                  createdAt:player.getVariable("ClanCreated"),
                  tag:player.getVariable("ClanTag")
                }
              };
              global.ServerUI.call("set_user", user_object);

              global.ServerUI.call("mainmenu_clan_my_clan_public_set", player.getVariable("ClanPublic")); // Public value 1 == public | 0 == invite-only
              global.ServerUI.call("mainmenu_clan_my_clan_force_tag_set", player.getVariable("ForceClanTag"));
              global.ServerUI.call("mainmenu_clan_my_clan_copchase_partnering_set", player.getVariable("ClanPartnering"));
                
              if(all === true){            
                    if(global.CommandsDataJSON){
                        let allowedCommands = global.CommandsDataJSON.filter(cmd => cmd.Admin <= mp.players.local.getVariable("Admin"));
                        allowedCommands.sort(function(a, b){
                            return a.CommandText.length - b.CommandText.length;
                        });
                        if(global.ServerUI) global.ServerUI.call('hud_chat_commands_set', allowedCommands);
                    }
                }
             
            }
        }
    }
}
function ToggleMDC(){
    if(mp.players.local.getVariable("Team") != 1 || mp.players.local.getVariable("Wounded") > 0) return;
    if(global.menuToggled == true || global.chatStatus == true || global.isHeliCamActive() || global.ActionMenuActive == true || global.isHotwiring == true || global.ClothingEditorActive == true) return;

    if(global.scoreboardToggled)
    {
        ToggleScoreboard();
    }
    global.MDCActive = !global.MDCActive;
    global.MDC_DOM.active = global.MDCActive;

    if(global.MDCActive){
        mp.events.callLocal("MDC:UpdateMiniscreen", "name", mp.players.local.name);
    }
}

function LogPlayerDamage(targetEntity, weaponHash, damage, bodypartIndex, extraNotes = ""){
    const targetSQLID = targetEntity.getVariable("SQL_ID");
    if(!targetSQLID) return;
  
    let gunData = global.weaponData.find(w => Number(w.Hash) == weaponHash);
    if(!gunData) gunData = {Name:"Unknown"}
  
    const damagedHP = (mp.players.local.getHealth() - damage);
  
    let bodypartData = global.boneData.find(b => b.index === bodypartIndex);
    if(!bodypartData) bodypartData = {name:`Unknown`}
  
    let notes = extraNotes;
  
    if(mp.game.player.getInvincible() === true){
      if(notes.length > 0) notes += "; ";
      notes += "Player had invincibility. "
    }
    if(mp.players.local.getVariable("Team") == -1){
      if(mp.players.local.getVariable("InLobby") === -3){
        notes += "They were in FREEROAM!";
      }
      else if(mp.players.local.getVariable("InDM") != -1){
        notes += "They were in DEATHMATCH!";
      }
      else{
        notes += "They were most likely in the LOBBY! [Server Invincibility]";
      }
    }
    else{
      if(mp.players.local.getVariable("Wounded") > 0){
        notes += "They were WOUNDED in COPCHASE! [Server Invincibility]";
      }
      else{
        notes += "They were in COPCHASE!";
      }
    }
    mp.events.callRemote("Server_RequestLogDamage", targetSQLID, gunData.Name, damage, damagedHP, `${bodypartData.name} (${bodypartIndex})`, notes);
  }

const boneData = [
    {index:0, name:"Pelvis"},
    {index:1, name:"Left Thigh"},
    {index:2, name:"Left Calf"},
    {index:3, name:"Left Foot"},
    {index:4, name:"Right Thigh"},
    {index:5, name:"Right Calf"},
    {index:6, name:"Right Foot"},
    {index:7, name:"Lower Abdomen"},
    {index:8, name:"Upper Abdomen"},
    {index:9, name:"Chest"},
    {index:10, name:"Chest"},
    {index:11, name:"Left Clavicle"},
    {index:12, name:"Left Upper Arm"},
    {index:13, name:"Left Forearm"},
    {index:14, name:"Left Hand"},
    {index:15, name:"Right Clavicle"},
    {index:16, name:"Right Upper Arm"},
    {index:17, name:"Right Forearm"},
    {index:18, name:"Right Hand"},
    {index:19, name:"Neck"},
    {index:20, name:"Head"}
  ];
  
  const weaponData = [
      {
        "id": "weapon_dagger",
        "Name": "Antique Cavalry Dagger",
        "Type": "Melee",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Antique-cavalry-dagger-icon.png",
        "Hash": "0x92A27487"
      },
      {
        "id": "weapon_bat",
        "Name": "Baseball Bat",
        "Type": "Melee",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Baseball-bat-icon.png",
        "Hash": "0x958A4A8F"
      },
      {
        "id": "weapon_bottle",
        "Name": "Broken Bottle",
        "Type": "Melee",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Broken-bottle-icon.png",
        "Hash": "0xF9E6AA4B"
      },
      {
        "id": "weapon_crowbar",
        "Name": "Crowbar",
        "Type": "Melee",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Crowbar-icon.png",
        "Hash": "0x84BD7BFD"
      },
      {
        "id": "weapon_unarmed",
        "Name": "Fist",
        "Type": "Melee",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Fist-icon.png",
        "Hash": "0xA2719263"
      },
      {
        "id": "weapon_flashlight",
        "Name": "Flashlight",
        "Type": "Melee",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Flashlight-icon.png",
        "Hash": "0x8BB05FD7"
      },
      {
        "id": "weapon_golfclub",
        "Name": "Golf Club",
        "Type": "Melee",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Golf-club-icon.png",
        "Hash": "0x440E4788"
      },
      {
        "id": "weapon_hammer",
        "Name": "Hammer",
        "Type": "Melee",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Hammer-icon.png",
        "Hash": "0x4E875F73"
      },
      {
        "id": "weapon_hatchet",
        "Name": "Hatchet",
        "Type": "Melee",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Hatchet-icon.png",
        "Hash": "0xF9DCBF2D"
      },
      {
        "id": "weapon_knuckle",
        "Name": "Brass Knuckles",
        "Type": "Melee",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Knuckles-icon.png",
        "Hash": "0xD8DF3C3C"
      },
      {
        "id": "weapon_knife",
        "Name": "Knife",
        "Type": "Melee",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Knife-icon.png",
        "Hash": "0x99B507EA"
      },
      {
        "id": "weapon_machete",
        "Name": "Machete",
        "Type": "Melee",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Machete-icon.png",
        "Hash": "0xDD5DF8D9"
      },
      {
        "id": "weapon_switchblade",
        "Name": "Switchblade",
        "Type": "Melee",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Switch-blade-icon.png",
        "Hash": "0xDFE37640"
      },
      {
        "id": "weapon_nightstick",
        "Name": "Nightstick",
        "Type": "Melee",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Nightstick-icon.png",
        "Hash": "0x678B81B1"
      },
      {
        "id": "weapon_wrench",
        "Name": "Pipe Wrench",
        "Type": "Melee",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Pipe-wrench-icon.png",
        "Hash": "0x19044EE0"
      },
      {
        "id": "weapon_battleaxe",
        "Name": "Battle Axe",
        "Type": "Melee",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Battle-axe-icon.png",
        "Hash": "0xCD274149"
      },
      {
        "id": "weapon_poolcue",
        "Name": "Pool Cue",
        "Type": "Melee",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Pool-cue-icon.png",
        "Hash": "0x94117305"
      },
      {
        "id": "weapon_stone_hatchet",
        "Name": "Stone Hatchet",
        "Type": "Melee",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Stone-hatchet-icon.png",
        "Hash": "0x3813FC08"
      },
      {
        "id": "weapon_pistol",
        "Name": "Pistol",
        "Type": "Handguns",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Pistol-icon.png",
        "Hash": "0x1B06D571"
      },
      {
        "id": "weapon_pistol_mk2",
        "Name": "Pistol Mk II",
        "Type": "Handguns",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Pistol-mk2-icon.png",
        "Hash": "0xBFE256D4"
      },
      {
        "id": "weapon_combatpistol",
        "Name": "Combat Pistol",
        "Type": "Handguns",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Combat-pistol-icon.png",
        "Hash": "0x5EF9FEC4"
      },
      {
        "id": "weapon_appistol",
        "Name": "AP Pistol",
        "Type": "Handguns",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Appistol-icon.png",
        "Hash": "0x22D8FE39"
      },
      {
        "id": "weapon_stungun",
        "Name": "Stun Gun",
        "Type": "Handguns",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Stungun-icon.png",
        "Hash": "0x3656C8C1"
      },
      {
        "id": "weapon_pistol50",
        "Name": "Pistol .50",
        "Type": "Handguns",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Pistol.50-icon.png",
        "Hash": "0x99AEEB3B"
      },
      {
        "id": "weapon_snspistol",
        "Name": "SNS Pistol",
        "Type": "Handguns",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Sns-pistol-icon.png",
        "Hash": "0xBFD21232"
      },
      {
        "id": "weapon_snspistol_mk2",
        "Name": "SNS Pistol Mk II",
        "Type": "Handguns",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Sns-pistol-mk2-icon.png",
        "Hash": "0x88374054"
      },
      {
        "id": "weapon_heavypistol",
        "Name": "Heavy Pistol",
        "Type": "Handguns",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Heavy-pistol-icon.png",
        "Hash": "0xD205520E"
      },
      {
        "id": "weapon_vintagepistol",
        "Name": "Vintage Pistol",
        "Type": "Handguns",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Vintage-pistol-icon.png",
        "Hash": "0x83839C4"
      },
      {
        "id": "weapon_flaregun",
        "Name": "Flare Gun",
        "Type": "Handguns",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Flaregun-icon.png",
        "Hash": "0x47757124"
      },
      {
        "id": "weapon_marksmanpistol",
        "Name": "Marksman Pistol",
        "Type": "Handguns",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Marksman-pistol-icon.png",
        "Hash": "0xDC4DB296"
      },
      {
        "id": "weapon_revolver",
        "Name": "Heavy Revolver",
        "Type": "Handguns",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Heavy-revolver-icon.png",
        "Hash": "0xC1B3C3D1"
      },
      {
        "id": "weapon_revolver_mk2",
        "Name": "Heavy Revolver Mk II",
        "Type": "Handguns",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Heavy-revolver-mk2-icon.png",
        "Hash": "0xCB96392F"
      },
      {
        "id": "weapon_doubleaction",
        "Name": "Snub Nose Revolver",
        "Type": "Handguns",
        "ImageSrc": "https://i.imgur.com/TY7Npph.png",
        "Hash": "0x97EA20B8"
      },
      {
        "id": "weapon_raypistol",
        "Name": "Up-n-Atomizer",
        "Type": "Handguns",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Up-n-atomizer-icon.png",
        "Hash": "0xAF3696A1"
      },
      {
        "id": "weapon_ceramicpistol",
        "Name": "Ceramic Pistol",
        "Type": "Handguns",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Ceramic-pistol-icon.png",
        "Hash": "0x2B5EF5EC"
      },
      {
        "id": "weapon_navyrevolver",
        "Name": "Navy Revolver",
        "Type": "Handguns",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Navy-revolver-icon.png",
        "Hash": "0x917F6C8C"
      },
      {
        "id": "weapon_gadgetpistol",
        "Name": "Perico Pistol",
        "Type": "Handguns",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Perico-pistol-icon.png",
        "Hash": "0x57A4368C"
      },
      {
        "id": "weapon_stungun_mp",
        "Name": "Stun Gun",
        "Type": "Handguns",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Stungun-icon.png",
        "Hash": "0x45CD9CF3"
      },
      {
        "id": "weapon_microsmg",
        "Name": "Micro SMG",
        "Type": "Submachine Guns",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Micro-smg-icon.png",
        "Hash": "0x13532244"
      },
      {
        "id": "weapon_tacticalsmg",
        "Name": "Tactical SMG",
        "Type": "Submachine Guns",
        "ImageSrc": "https://i.imgur.com/rbqgDgt.png",
        "Hash": "0x14E5AFD5"
      },
      {
        "id": "weapon_smg",
        "Name": "SMG",
        "Type": "Submachine Guns",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Smg-icon.png",
        "Hash": "0x2BE6766B"
      },
      {
        "id": "weapon_smg_mk2",
        "Name": "SMG Mk II",
        "Type": "Submachine Guns",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Smg-mk2-icon.png",
        "Hash": "0x78A97CD0"
      },
      {
        "id": "weapon_assaultsmg",
        "Name": "Assault SMG",
        "Type": "Submachine Guns",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Assault-smg-icon.png",
        "Hash": "0xEFE7E2DF"
      },
      {
        "id": "weapon_combatpdw",
        "Name": "Combat PDW",
        "Type": "Submachine Guns",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Combat-pdw-icon.png",
        "Hash": "0x0A3D4D34"
      },
      {
        "id": "weapon_machinepistol",
        "Name": "Machine Pistol",
        "Type": "Submachine Guns",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Machine-pistol-icon.png",
        "Hash": "0xDB1AA450"
      },
      {
        "id": "weapon_minismg",
        "Name": "Mini SMG",
        "Type": "Submachine Guns",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Mini-smg-icon.png",
        "Hash": "0xBD248B55"
      },
      {
        "id": "weapon_raycarbine",
        "Name": "Unholy Hellbringer",
        "Type": "Submachine Guns",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Unholy-hellbringer-icon.png",
        "Hash": "0x476BF155"
      },
      {
        "id": "weapon_pumpshotgun",
        "Name": "Pump Shotgun",
        "Type": "Shotguns",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Pump-shotgun-icon.png",
        "Hash": "0x1D073A89"
      },
      {
        "id": "weapon_pumpshotgun_mk2",
        "Name": "Pump Shotgun Mk II",
        "Type": "Shotguns",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Pump-shotgun-mk2-icon.png",
        "Hash": "0x555AF99A"
      },
      {
        "id": "weapon_sawnoffshotgun",
        "Name": "Sawed-Off Shotgun",
        "Type": "Shotguns",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Sawed-off-shotgun-icon.png",
        "Hash": "0x7846A318"
      },
      {
        "id": "weapon_assaultshotgun",
        "Name": "Assault Shotgun",
        "Type": "Shotguns",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Assault-shotgun-icon.png",
        "Hash": "0xE284C527"
      },
      {
        "id": "weapon_bullpupshotgun",
        "Name": "Bullpup Shotgun",
        "Type": "Shotguns",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Bullpup-shotgun-icon.png",
        "Hash": "0x9D61E50F"
      },
      {
        "id": "weapon_musket",
        "Name": "Musket",
        "Type": "Shotguns",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Musket-icon.png",
        "Hash": "0xA89CB99E"
      },
      {
        "id": "weapon_heavyshotgun",
        "Name": "Heavy Shotgun",
        "Type": "Shotguns",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Heavy-shotgun-icon.png",
        "Hash": "0x3AABBBAA"
      },
      {
        "id": "weapon_dbshotgun",
        "Name": "Double Barrel Shotgun",
        "Type": "Shotguns",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Double-barrel-shotgun-icon.png",
        "Hash": "0xEF951FBB"
      },
      {
        "id": "weapon_autoshotgun",
        "Name": "Sweeper Shotgun",
        "Type": "Shotguns",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Sweeper-shotgun-icon.png",
        "Hash": "0x12E82D3D"
      },
      {
        "id": "weapon_combatshotgun",
        "Name": "Combat Shotgun",
        "Type": "Shotguns",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Combat-shotgun-icon.png",
        "Hash": "0x5A96BA4"
      },
      {
        "id": "weapon_assaultrifle",
        "Name": "Assault Rifle",
        "Type": "Assault Rifles",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Assault-rifle-icon.png",
        "Hash": "0xBFEFFF6D"
      },
      {
        "id": "weapon_assaultrifle_mk2",
        "Name": "Assault Rifle Mk II",
        "Type": "Assault Rifles",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Assault-rifle-mk2-icon.png",
        "Hash": "0x394F415C"
      },
      {
        "id": "weapon_carbinerifle",
        "Name": "Carbine Rifle",
        "Type": "Assault Rifles",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Carbine-rifle-icon.png",
        "Hash": "0x83BF0278"
      },
      {
        "id": "weapon_carbinerifle_mk2",
        "Name": "Carbine Rifle Mk II",
        "Type": "Assault Rifles",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Carbine-rifle-mk2-icon.png",
        "Hash": "0xFAD1F1C9"
      },
      {
        "id": "weapon_advancedrifle",
        "Name": "Advanced Rifle",
        "Type": "Assault Rifles",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Advanced-rifle-icon.png",
        "Hash": "0xAF113F99"
      },
      {
        "id": "weapon_specialcarbine",
        "Name": "Special Carbine",
        "Type": "Assault Rifles",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Special-carbine-icon.png",
        "Hash": "0xC0A3098D"
      },
      {
        "id": "weapon_specialcarbine_mk2",
        "Name": "Special Carbine Mk II",
        "Type": "Assault Rifles",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Special-carbine-mk2-icon.png",
        "Hash": "0x969C3D67"
      },
      {
        "id": "weapon_bullpuprifle",
        "Name": "Bullpup Rifle",
        "Type": "Assault Rifles",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Bullpup-rifle-icon.png",
        "Hash": "0x7F229F94"
      },
      {
        "id": "weapon_bullpuprifle_mk2",
        "Name": "Bullpup Rifle Mk II",
        "Type": "Assault Rifles",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Bullpup-rifle-mk2-icon.png",
        "Hash": "0x84D6FAFD"
      },
      {
        "id": "weapon_compactrifle",
        "Name": "Compact Rifle",
        "Type": "Assault Rifles",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Compact-rifle-icon.png",
        "Hash": "0x624FE830"
      },
      {
        "id": "weapon_militaryrifle",
        "Name": "Military Rifle",
        "Type": "Assault Rifles",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Military-rifle-icon.png",
        "Hash": "0x9D1F17E6"
      },
      {
        "id": "weapon_heavyrifle",
        "Name": "Heavy Rifle",
        "Type": "Assault Rifles",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Heavy-rifle-icon.png",
        "Hash": "0xC78D71B4"
      },
      {
        "id": "weapon_tacticalrifle",
        "Name": "Tactical Rifle",
        "Type": "Assault Rifles",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Tactical-rifle-icon.png",
        "Hash": "0xD1D5F52B"
      },
      {
        "id": "weapon_mg",
        "Name": "MG",
        "Type": "Machine Guns",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Mg-icon.png",
        "Hash": "0x9D07F764"
      },
      {
        "id": "weapon_combatmg",
        "Name": "Combat MG",
        "Type": "Machine Guns",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Combat-mg-icon.png",
        "Hash": "0x7FD62962"
      },
      {
        "id": "weapon_combatmg_mk2",
        "Name": "Combat MG Mk II",
        "Type": "Machine Guns",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Combat-mg-mk2-icon.png",
        "Hash": "0xDBBD7280"
      },
      {
        "id": "weapon_gusenberg",
        "Name": "Gusenberg Sweeper",
        "Type": "Machine Guns",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Gusenberg-sweeper-icon.png",
        "Hash": "0x61012683"
      },
      {
        "id": "weapon_sniperrifle",
        "Name": "Sniper Rifle",
        "Type": "Sniper Rifles",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Sniper-rifle-icon.png",
        "Hash": "0x05FC3C11"
      },
      {
        "id": "weapon_heavysniper",
        "Name": "Heavy Sniper",
        "Type": "Sniper Rifles",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Heavy-sniper-icon.png",
        "Hash": "0x0C472FE2"
      },
      {
        "id": "weapon_heavysniper_mk2",
        "Name": "Heavy Sniper Mk II",
        "Type": "Sniper Rifles",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Heavy-sniper-mk2-icon.png",
        "Hash": "0xA914799"
      },
      {
        "id": "weapon_marksmanrifle",
        "Name": "Marksman Rifle",
        "Type": "Sniper Rifles",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Marksman-rifle-icon.png",
        "Hash": "0xC734385A"
      },
      {
        "id": "weapon_marksmanrifle_mk2",
        "Name": "Marksman Rifle Mk II",
        "Type": "Sniper Rifles",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Marksman-rifle-mk2-icon.png",
        "Hash": "0x6A6C02E0"
      },
      {
        "id": "weapon_precisionrifle",
        "Name": "Precision Rifle",
        "Type": "Sniper Rifles",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Precision-rifle-icon.png",
        "Hash": "0x6E7DDDEC"
      },
      {
        "id": "weapon_rpg",
        "Name": "RPG",
        "Type": "Heavy Weapons",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Rocket-launcher-icon.png",
        "Hash": "0xB1CA77B1"
      },
      {
        "id": "weapon_grenadelauncher",
        "Name": "Grenade Launcher",
        "Type": "Heavy Weapons",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Grenade-launcher-icon.png",
        "Hash": "0xA284510B"
      },
      {
        "id": "weapon_grenadelauncher_smoke",
        "Name": "Grenade Launcher Smoke",
        "Type": "Heavy Weapons",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Grenade-launcher-icon.png",
        "Hash": "0x4DD2DC56"
      },
      {
        "id": "weapon_minigun",
        "Name": "Minigun",
        "Type": "Heavy Weapons",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Minigun-icon.png",
        "Hash": "0x42BF8A85"
      },
      {
        "id": "weapon_firework",
        "Name": "Firework Launcher",
        "Type": "Heavy Weapons",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Firework-launcher-icon.png",
        "Hash": "0x7F7497E5"
      },
      {
        "id": "weapon_railgun",
        "Name": "Railgun",
        "Type": "Heavy Weapons",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Railgun-icon.png",
        "Hash": "0x6D544C99"
      },
      {
        "id": "weapon_hominglauncher",
        "Name": "Homing Launcher",
        "Type": "Heavy Weapons",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Homing-launcher-icon.png",
        "Hash": "0x63AB0442"
      },
      {
        "id": "weapon_compactlauncher",
        "Name": "Compact Grenade Launcher",
        "Type": "Heavy Weapons",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Grenade-compact-launcher-icon.png",
        "Hash": "0x0781FE4A"
      },
      {
        "id": "weapon_rayminigun",
        "Name": "Widowmaker",
        "Type": "Heavy Weapons",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Widowmaker-icon.png",
        "Hash": "0xB62D1F67"
      },
      {
        "id": "weapon_emplauncher",
        "Name": "Compact EMP Launcher",
        "Type": "Heavy Weapons",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/EMP-launcher-icon.png",
        "Hash": "0xDB26713A"
      },
      {
        "id": "weapon_grenade",
        "Name": "Grenade",
        "Type": "Thrown Weapons",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Grenade-icon.png",
        "Hash": "0x93E220BD"
      },
      {
        "id": "weapon_bzgas",
        "Name": "BZ Gas",
        "Type": "Thrown Weapons",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Bz-gas-icon.png",
        "Hash": "0xA0973D5E"
      },
      {
        "id": "weapon_molotov",
        "Name": "Molotov Cocktail",
        "Type": "Thrown Weapons",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Molotov-icon.png",
        "Hash": "0x24B17070"
      },
      {
        "id": "weapon_stickybomb",
        "Name": "Sticky Bomb",
        "Type": "Thrown Weapons",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Sticky-bomb-icon.png",
        "Hash": "0x2C3731D9"
      },
      {
        "id": "weapon_proxmine",
        "Name": "Proximity Mines",
        "Type": "Thrown Weapons",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Proximity-mines-icon.png",
        "Hash": "0xAB564B93"
      },
      {
        "id": "weapon_snowball",
        "Name": "Snowballs",
        "Type": "Thrown Weapons",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Snowball-icon.png",
        "Hash": "0x787F0BB"
      },
      {
        "id": "weapon_pipebomb",
        "Name": "Pipe Bombs",
        "Type": "Thrown Weapons",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Pipe-bomb-icon.png",
        "Hash": "0xBA45E8B8"
      },
      {
        "id": "weapon_ball",
        "Name": "Baseball",
        "Type": "Thrown Weapons",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Ball-icon.png",
        "Hash": "0x23C9F95C"
      },
      {
        "id": "weapon_smokegrenade",
        "Name": "Tear Gas",
        "Type": "Thrown Weapons",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Tear-gas-icon.png",
        "Hash": "0xFDBC8A50"
      },
      {
        "id": "weapon_flare",
        "Name": "Flare",
        "Type": "Thrown Weapons",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Flare-icon.png",
        "Hash": "0x497FACC3"
      },
      {
        "id": "weapon_petrolcan",
        "Name": "Jerry Can",
        "Type": "Thrown Weapons",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Petrolcan-icon.png",
        "Hash": "0x34A67B97"
      },
      {
        "id": "gadget_parachute",
        "Name": "Parachute",
        "Type": "Thrown Weapons",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Parachute-icon.png",
        "Hash": "0xFBAB5776"
      },
      {
        "id": "weapon_fireextinguisher",
        "Name": "Fire Extinguisher",
        "Type": "Thrown Weapons",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Fire2.png",
        "Hash": "0x060EC506"
      },
      {
        "id": "weapon_hazardcan",
        "Name": "Hazardous Jerry Can",
        "Type": "Thrown Weapons",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Petrolcan-icon.png",
        "Hash": "0xBA536372"
      },
      {
        "id": "weapon_fertilizercan",
        "Name": "Fertilizer Can",
        "Type": "Thrown Weapons",
        "ImageSrc": "https://assets.gm.miami/gtav/weapons/Petrolcan-icon.png",
        "Hash": "0x184140A1"
      }
    ]

    
function activateChat(toggle){
    if(global.ServerUI == null) return;
    if(global.menuToggled){
        global.ServerUI.call('chat:show', false);
        return;
    }

    global.ServerUI.call('chat:show', toggle);
    mp.gui.chat.show(toggle);
};
function AddCustomVehiclesToSpawner(){
    if(global.ServerUI != null){

        let modelsList = ["lspdunm", "polbufac", "huntley2", "lspdvic", "prevolter", "m3e46", "pscout", "impala",
        "shegrangerold", "shergauntlet", "polbisonf", "gresleyh", "tulip2", "golfmk6", "bcso2slick", "bcat", "police3slick",
        "fibexecutioner", "police3umk"];
        return;
    }
}
function calcDist(v1, v2) {
    return mp.game.system.vdist(
        v1.x,
        v1.y,
        v1.z,
        v2.x,
        v2.y,
        v2.z
    );
}

let emojiPickerOpen = false;
mp.keys.bind(0x74, true, () => { // F5 to open emoji picker in chat
  if(global.chatStatus){
    emojiPickerOpen = !emojiPickerOpen;
    global.ServerUI.call('hud_chat_emoji_picker_toggle', emojiPickerOpen);
  }
});


function focusChat(toggle){
  
    if(global.isInPunishmentDialog) return;
    if(global.ServerUI == null) return;

    emojiPickerOpen = false;
    var channelsObject = [
      {name:"Local", value:"text"},
      {name:"Global", value:"go"},
      {name:"Lobby", value:"lg"},
    ];

    if(mp.players.local.getVariable("ClanID") !== -1)
        channelsObject.push({name:"Clan", value:"c"});

    if(mp.players.local.getVariable("Admin") > 0)
        channelsObject.push({name:"Staff Chat", value:"a"});


    if(global.ServerUI) global.ServerUI.call('hud_chat_channels_set', channelsObject);

    if(global.menuToggled && toggle === true){
        focusChat(false);
        return;
    }
    if(toggle === true){
      if((global.VehicleSpawnerActive !== undefined && global.VehicleSpawnerActive == true) ||
      (global.WeaponEditorActive !== undefined && global.WeaponEditorActive == true) ||
      (global.ActionMenuActive !== undefined && global.ActionMenuActive == true) || 
      (global.ClothingEditorActive !== undefined && global.ClothingEditorActive == true) ||
      (global.StationPickerToggled !== undefined && global.StationPickerToggled == true) ||
      (global.isHotwiring !== undefined && global.isHotwiring == true) || global.isHeliCamActive()){
          focusChat(false);
          return;
      }
    }

    global.ServerUI.call('chat:activate', toggle); // this opens the chatbox, not mp.gui.chat.activate
    mp.events.callRemote("onFocusChat", toggle);
}
function InitializeHotwireDOM(){
    global.HotwireDOM = mp.browsers.new("package://hotwire_game/index.html");
    global.HotwireDOM.active = false;
}
function getClosestVehicle(position) {
    try {
      let closest = 50;
      let closestVeh = null;
  
      mp.vehicles.forEachInStreamRange((v) => {
        let dist = mp.game.system.vdist(
          position.x,
          position.y,
          position.z,
          v.position.x,
          v.position.y,
          v.position.z
        );
  
        if (dist < closest) {
          closest = dist;
          closestVeh = v;
        }
      });
  
      return { distance: closest, vehicle: closestVeh };
    } catch (e) {}
  }


function InitializeMDC(){
    global.MDC_DOM = mp.browsers.new("package://mdc_screen/index.html");

    mp.events.callLocal("MDC:AddBlip", mp.players.local.remoteId, "police");

    global.MDCActive = false;
    global.MDC_DOM.active = false;
}
function InitializeWeaponEditor(){
    global.WeaponEditorDOM = mp.browsers.new("package://weaponeditor/index.html");
    global.WeaponEditorDOM.active = false;
}
function InitializeClothingEditorDOM(){
    global.ClothingEditorDOM = mp.browsers.new("package://clothing_editor/index.html");
    global.ClothingEditorDOM.active = false;
    global.ClothingEditorActive = false;
}

const enterableBuldingsUnfiltered = [
  [new mp.Vector3(433.919, -981.88116, 30.709455), 91.243935, "Police Station", 60],
  [new mp.Vector3(1855.289, 3683.2783, 34.26754), -155.12216, "Police Station", 60],
  [new mp.Vector3(-443.17432, 6016.3916, 31.71221), -58.92256, "Police Station", 60],
  [new mp.Vector3(417.98846, -807.51514, 29.397846), 89.17556, "Clothing Store", 73],
  [new mp.Vector3(82.95728, -1391.6437, 29.411314), -91.942116, "Clothing Store", 73],
  [new mp.Vector3(127.474014, -210.47804, 54.538425), -17.514898, "Clothing Store", 73],
  [new mp.Vector3(-3168.0598, 1056.835, 20.858864), 158.76701, "Clothing Store", 73],
  [new mp.Vector3(1197.9128, 2702.575, 38.156094), -167.03397, "Clothing Store", 73],
  [new mp.Vector3(617.9219, 2750.137, 42.088955), 5.2716217, "Clothing Store", 73],
  [new mp.Vector3(1686.2122, 4820.5693, 41.9779), -3.139387, "Clothing Store", 73],
  [new mp.Vector3(-1.8723875, 6517.2583, 31.483122), -81.96693, "Clothing Store", 73],
  [new mp.Vector3(-1095.2634, 2705.6094, 19.08352), -97.42532, "Clothing Store", 73],
  [new mp.Vector3(-717.02545, -156.80402, 36.988018), -135.40097, "Clothing Store", 73],
  [new mp.Vector3(-155.85936, -305.98184, 39.289597), -13.635981, "Clothing Store", 73],
  [new mp.Vector3(-817.6786, -1079.5449, 11.132536), 108.381096, "Clothing Store", 73],
  [new mp.Vector3(-1201.8987, -778.27277, 17.337143), 36.595222, "Clothing Store", 73],
  [new mp.Vector3(-1456.3527, -231.9416, 49.34221), -108.30579, "Clothing Store", 73],
  [new mp.Vector3(151.08084, -1037.2583, 29.339083), -18.368605, "Bank", 108],
  [new mp.Vector3(230.26367, 214.54152, 105.55165), 107.37271, "Bank", 108],
  [new mp.Vector3(1175.447, 2702.9802, 38.172596), 79.56133, "Bank", 108],
  [new mp.Vector3(-111.0951, 6462.488, 31.640772), 131.73488, "Bank", 108],
  [new mp.Vector3(-2966.3823, 483.12125, 15.692721), 97.38031, "Bank", 108],
  [new mp.Vector3(-53.19084, -1757.1958, 29.439632), 138.3869, "General Store", 52],
  [new mp.Vector3(-712.0335, -917.12573, 19.214228), -165.72762, "General Store", 52],
  [new mp.Vector3(376.45834, 322.9743, 103.57246), 164.76015, "General Store", 52],
  [new mp.Vector3(29.014181, -1350.0109, 29.330671), 178.37935, "General Store", 52],
  [new mp.Vector3(-1226.6317, -902.0572, 12.284421), 34.853443, "General Store", 52],
  [new mp.Vector3(1159.586, -327.39114, 69.2069), 26.24761, "General Store", 52],
  [new mp.Vector3(1141.9082, -980.9195, 46.197273), -82.31642, "General Store", 52],
  [new mp.Vector3(-3038.0398, 589.6614, 7.8159347), -75.56125, "General Store", 52],
  [new mp.Vector3(-3239.455, 1004.35754, 12.613415), -97.95817, "General Store", 52],
  [new mp.Vector3(-2974.5254, 390.84995, 15.032467), 92.103386, "General Store", 52],
  [new mp.Vector3(-1821.9016, 787.904, 138.1819), -119.5243, "General Store", 52],
  [new mp.Vector3(2559.8208, 385.24298, 108.62105), -103.74505, "General Store", 52],
  [new mp.Vector3(543.9823, 2673.5598, 42.150967), -164.34106, "General Store", 52],
  [new mp.Vector3(1394.3987, 3599.277, 35.007645), -156.69781, "General Store", 52],
  [new mp.Vector3(1965.6084, 3740.0222, 32.324284), 145.27957, "General Store", 52],
  [new mp.Vector3(2682.9756, 3282.0818, 55.238586), -22.671528, "General Store", 52],
  [new mp.Vector3(1698.2491, 4929.7495, 42.078136), -173.00418, "General Store", 52],
  [new mp.Vector3(1730.7263, 6410.941, 35.00061), 152.38426, "General Store", 52],
  [new mp.Vector3(-1491.7097, -384.13174, 40.05992), 145.3827, "General Store", 52],
  [new mp.Vector3(1990.7975, 3053.9634, 47.214928), -179.2838, "Bar", 93],
  [new mp.Vector3(-564.56793, 275.46957, 83.036514), 116.935356, "Bar", 93],
  [new mp.Vector3(-3163.8137, 1082.067, 20.847738), 156.68716, "Weapon Store", 110],
  [new mp.Vector3(1699.7227, 3751.9797, 34.5176), 62.504383, "Weapon Store", 110],
  [new mp.Vector3(-324.67267, 6075.518, 31.243692), -20.820875, "Weapon Store", 110],
  [new mp.Vector3(17.042889, -1115.6641, 29.791164), -150.31505, "Weapon Store", 110],
  [new mp.Vector3(243.7973, -45.14607, 69.89652), 113.76118, "Weapon Store", 110],
  [new mp.Vector3(-1314.8613, -390.05228, 36.58421), 125.82767, "Weapon Store", 110],
  [new mp.Vector3(-1112.4161, 2689.9128, 18.588762), -65.482925, "Weapon Store", 110],
  [new mp.Vector3(-663.95325, -945.24023, 21.783993), 135.03511, "Weapon Store", 110],
  [new mp.Vector3(812.01636, -2147.6223, 29.50994), 35.41494, "Weapon Store", 110],
  [new mp.Vector3(844.0101, -1023.5221, 27.98121), -175.35904, "Weapon Store", 110],
  [new mp.Vector3(2569.5264, 304.4676, 108.60786), 120.68825, "Weapon Store", 110],
  [new mp.Vector3(-1149.9208, -1521.7838, 10.628015), 32.143345, "Safehouse", 40],
  [new mp.Vector3(-817.43414, 177.93681, 72.22407), 115.10763, "Safehouse", 40],
  [new mp.Vector3(2451.903, 4969.29, 46.571575), 23.83546, "Safehouse", 40],
  [new mp.Vector3(8.432799, 540.177, 176.0273), 162.03288, "Safehouse", 40],
  [new mp.Vector3(-14.277575, -1442.3053, 31.099993), -7.2073927, "Safehouse", 40],
  [new mp.Vector3(-29.886766, -146.67, 57.006653), -14.317797, "Hair Salon", 71],
  [new mp.Vector3(132.33678, -1712.7504, 29.302402), -25.616528, "Hair Salon", 71],
  [new mp.Vector3(1934.2506, 3724.4253, 32.802742), 29.181149, "Hair Salon", 71],
  [new mp.Vector3(-282.37573, 6233.015, 31.489086), -126.68627, "Hair Salon", 71],
  [new mp.Vector3(-1288.5099, -1116.5143, 6.980809), -78.46615, "Hair Salon", 71],
  [new mp.Vector3(1206.4691, -470.36188, 66.18448), -110.0705, "Hair Salon", 71],
  [new mp.Vector3(-823.9467, -188.37604, 37.62022), 16.1521, "Hair Salon", 71],
  [new mp.Vector3(128.80002, -1298.5297, 29.232729), 120.83149, "Strip Club", 121],
  [new mp.Vector3(320.9853, 177.96117, 103.54415), 157.75287, "Tattoo Shop", 75],
  [new mp.Vector3(1859.6602, 3748.8728, 33.032528), 84.75233, "Tattoo Shop", 75],
  [new mp.Vector3(1321.3888, -1649.578, 52.14558), -146.76845, "Tattoo Shop", 75],
  [new mp.Vector3(-3167.3445, 1073.9807, 20.848097), 167.05197, "Tattoo Shop", 75],
  [new mp.Vector3(-289.08853, 6200.185, 31.4653), 129.98734, "Tattoo Shop", 75],
  [new mp.Vector3(-1155.1997, -1422.9274, 4.784532), 33.00279, "Tattoo Shop", 75],
  [new mp.Vector3(-2342.5286, 3266.2368, 32.827637), -112.88373, "Fort Zancudo Tower", 181],
  [new mp.Vector3(-1082.0801, -260.31897, 37.80518), -157.7062, "Lifeinvader Offices", 475],
  [new mp.Vector3(-252.85185, -2025.1189, 29.946032), -17.70971, "Maze Bank Arena", 557],
  [new mp.Vector3(736.1084, 132.5129, 80.72056), -120.125916, "LSDWP", 769],
  [new mp.Vector3(717.91974, -976.4302, 24.92134), -102.15795, "Garment Factory", 473],
  [new mp.Vector3(1064.7393, -2004.7765, 32.06369), 29.043642, "Steel Foundry", 473],
  [new mp.Vector3(-609.8029, -1609.7666, 26.898926), -1.0767382, "Scrap Yard & Recycling Center", 473],
  [new mp.Vector3(-1589.2687, -3230.5068, 26.312695), 148.57982, "Airport Terminal", 758],
  [new mp.Vector3(-1388.5024, -586.8714, 30.218977), 35.01387, "Nightclub", 136] 
]
const EnterableBuildingData = enterableBuldingsUnfiltered.map((b) => {
  return {
    position:b[0],
    heading:b[1],
    name:b[2],
    blipID:b[3]
  }
});

let ServerUI = null;
function InitializeServerUI(){
  if(ServerUI) return ServerUI;

  ServerUI = mp.browsers.new("package://ServerUI/index.html");
  
  if(ServerUI) ServerUI.call('hud_toggle', false);
  if(ServerUI) ServerUI.call('loadingscreen_start', 1000);
  return ServerUI;
}

// Check if holding sniper for the crosshair
let sniperHashes = [3342088282, 1785463520, 205991906, 177293209, 100416529];
function isHoldingSniper() {
  let returnValue = false;
  if (mp.players.local != undefined && mp.players.exists(mp.players.local)) {
    if (mp.players.local.weapon != undefined) {
      if (sniperHashes.includes(mp.players.local.weapon)) {
        returnValue = true;
      }
    }
  }
  return returnValue;
}

let global = {
  EnterableBuildings:EnterableBuildingData,
  
  currentJailTime: 0,
  lastJailUpdate: 10,

  isLoginScreenActive:false,

  chaseRunning: false,
  chaseTimeCur: 0,
  chaseTimeMax: 390,
  chaseMode: 0, // 0= STANDARD | 1= BUSTED | and others as in SCRIPT
  collisionTime: -1,
  updateRate: 200,

  radioToggle: 0,
  carryTarget: -1,

  chatStatus: false,
  chatStatusLate: false,
  menuToggledLate: false,
  lastChatToggle: Date.now(),
  streetName: null,
  zoneName: null,
  showHud: true,
  renderNametags: true,
  currentMuteTime: 0,
  lastMuteUpdate: 10,
  lastFunc: "Init",

  isInPunishmentDialog:false,

  miniHUD_items:[],

  WeaponEditorDOM:null,
  CharacterImportCEF:null,

  curMDCBind: 0x4B,
  lastMDCBind: 0x4B,
  curActionMenuBind: 0x59,
  lastActionMenuBind: 0x59,

  lastVoiceBind: 0x4E,
  curVoiceBind: 0x4E,

  digitalHPAR: false,

  BankRobberyOn: false,

  ShowVehicleDL: false,
  specState: false,
  specTarget: null,
  isNoClip: false,

  isInInterior:false,

  heliCam:null,

  IsInTutorial:false,
  disableSprint:false,
  FPS:0, // This is the actual fps
  FPS_Frames:0,
  FPS_Calc:Date.now(),
  precisionPositionerActive:false,
  HideAllCrosshairs:{AntiPeek:false},
  loginSceneEntities:[],
  chatStatusTimeout:null,

  ServerSettings:[],

  LastKeyPress: 0,
  AFKStage: 0,
  SecondsSinceLastKeyPress: 0, // using this for nametags
  antiAFKActive: false,

  showReticule: true,
  showCustomCrosshair: false,
  showCustomCrosshairInSniper: false,
  alwaysShowCustomCrosshair: false,
  customCrosshairOn: false,

  menuToggled:false,
  scoreboardToggled:false,
  scoreboardCursor:false,
  StationPickerToggled:false,
  MDC_DOM:null,
  MDCActive:false,
  isLockpicking:false,
  isHotwiring:false,
  HotwireDOM:false,
  ClothingEditorDOM:null,
  ClothingEditorActive:false,
  ActionMenuActive:false,
  VehicleSpawnerActive:false,
  WeaponEditorActive:false,
  FireModeDisplayText:"",
  cuffTarget:null,
  cuffInterval:null,
  lastGPSBind:0x5A,
  curGPSBind:0x5A,
  lastMapBind:0x50,
  curMapBind:0x50,

  CommandsDataJSON,
  weaponData,
  boneData,
  ServerUI:ServerUI,
  getClosestVehicle,
  LogPlayerDamage,
  ToggleMDC,
  actionMenuClose,
  actionMenuOpen,
  traceLastFunc,
  isHeliCamActive,
  UpdateStatsVars,
  focusChat,
  activateChat,
  InitializeClothingEditorDOM,
  InitializeMDC,
  InitializeWeaponEditor,
  InitializeHotwireDOM,
  AddCustomVehiclesToSpawner,
  calcDist,
  InitializeServerUI,
  isHoldingSniper,
};
exports = global;
