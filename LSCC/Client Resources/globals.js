
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
     "id":"weapon_dagger",
     "Name":"Antique Cavalry Dagger",
     "Type":"Melee",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x92A27487.png?raw=true",
     "Hash":"0x92A27487"
  },
  {
     "id":"weapon_bat",
     "Name":"Baseball Bat",
     "Type":"Melee",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x958A4A8F.png?raw=true",
     "Hash":"0x958A4A8F"
  },
  {
     "id":"weapon_bottle",
     "Name":"Broken Bottle",
     "Type":"Melee",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0xF9E6AA4B.png?raw=true",
     "Hash":"0xF9E6AA4B"
  },
  {
     "id":"weapon_crowbar",
     "Name":"Crowbar",
     "Type":"Melee",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x84BD7BFD.png?raw=true",
     "Hash":"0x84BD7BFD"
  },
  {
     "id":"weapon_unarmed",
     "Name":"Fist",
     "Type":"Melee",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0xA2719263.png?raw=true",
     "Hash":"0xA2719263"
  },
  {
     "id":"weapon_flashlight",
     "Name":"Flashlight",
     "Type":"Melee",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x8BB05FD7.png?raw=true",
     "Hash":"0x8BB05FD7"
  },
  {
     "id":"weapon_golfclub",
     "Name":"Golf Club",
     "Type":"Melee",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x440E4788.png?raw=true",
     "Hash":"0x440E4788"
  },
  {
     "id":"weapon_hammer",
     "Name":"Hammer",
     "Type":"Melee",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x4E875F73.png?raw=true",
     "Hash":"0x4E875F73"
  },
  {
     "id":"weapon_hatchet",
     "Name":"Hatchet",
     "Type":"Melee",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0xF9DCBF2D.png?raw=true",
     "Hash":"0xF9DCBF2D"
  },
  {
     "id":"weapon_knuckle",
     "Name":"Brass Knuckles",
     "Type":"Melee",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0xD8DF3C3C.png?raw=true",
     "Hash":"0xD8DF3C3C"
  },
  {
     "id":"weapon_knife",
     "Name":"Knife",
     "Type":"Melee",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x99B507EA.png?raw=true",
     "Hash":"0x99B507EA"
  },
  {
     "id":"weapon_machete",
     "Name":"Machete",
     "Type":"Melee",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0xDD5DF8D9.png?raw=true",
     "Hash":"0xDD5DF8D9"
  },
  {
     "id":"weapon_switchblade",
     "Name":"Switchblade",
     "Type":"Melee",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0xDFE37640.png?raw=true",
     "Hash":"0xDFE37640"
  },
  {
     "id":"weapon_nightstick",
     "Name":"Nightstick",
     "Type":"Melee",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x678B81B1.png?raw=true",
     "Hash":"0x678B81B1"
  },
  {
     "id":"weapon_wrench",
     "Name":"Pipe Wrench",
     "Type":"Melee",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x19044EE0.png?raw=true",
     "Hash":"0x19044EE0"
  },
  {
     "id":"weapon_battleaxe",
     "Name":"Battle Axe",
     "Type":"Melee",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0xCD274149.png?raw=true",
     "Hash":"0xCD274149"
  },
  {
     "id":"weapon_poolcue",
     "Name":"Pool Cue",
     "Type":"Melee",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x94117305.png?raw=true",
     "Hash":"0x94117305"
  },
  {
     "id":"weapon_stone_hatchet",
     "Name":"Stone Hatchet",
     "Type":"Melee",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x3813FC08.png?raw=true",
     "Hash":"0x3813FC08"
  },
  {
     "id":"weapon_pistol",
     "Name":"Pistol",
     "Type":"Handguns",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x1B06D571.png?raw=true",
     "Hash":"0x1B06D571"
  },
  {
     "id":"weapon_pistol_mk2",
     "Name":"Pistol Mk II",
     "Type":"Handguns",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0xBFE256D4.png?raw=true",
     "Hash":"0xBFE256D4"
  },
  {
     "id":"weapon_combatpistol",
     "Name":"Combat Pistol",
     "Type":"Handguns",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x5EF9FEC4.png?raw=true",
     "Hash":"0x5EF9FEC4"
  },
  {
     "id":"weapon_appistol",
     "Name":"AP Pistol",
     "Type":"Handguns",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x22D8FE39.png?raw=true",
     "Hash":"0x22D8FE39"
  },
  {
     "id":"weapon_stungun",
     "Name":"Stun Gun",
     "Type":"Handguns",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x3656C8C1.png?raw=true",
     "Hash":"0x3656C8C1"
  },
  {
     "id":"weapon_pistol50",
     "Name":"Pistol .50",
     "Type":"Handguns",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x99AEEB3B.png?raw=true",
     "Hash":"0x99AEEB3B"
  },
  {
     "id":"weapon_snspistol",
     "Name":"SNS Pistol",
     "Type":"Handguns",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0xBFD21232.png?raw=true",
     "Hash":"0xBFD21232"
  },
  {
     "id":"weapon_snspistol_mk2",
     "Name":"SNS Pistol Mk II",
     "Type":"Handguns",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x88374054.png?raw=true",
     "Hash":"0x88374054"
  },
  {
     "id":"weapon_heavypistol",
     "Name":"Heavy Pistol",
     "Type":"Handguns",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0xD205520E.png?raw=true",
     "Hash":"0xD205520E"
  },
  {
     "id":"weapon_vintagepistol",
     "Name":"Vintage Pistol",
     "Type":"Handguns",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x83839C4.png?raw=true",
     "Hash":"0x83839C4"
  },
  {
     "id":"weapon_flaregun",
     "Name":"Flare Gun",
     "Type":"Handguns",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x47757124.png?raw=true",
     "Hash":"0x47757124"
  },
  {
     "id":"weapon_marksmanpistol",
     "Name":"Marksman Pistol",
     "Type":"Handguns",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0xDC4DB296.png?raw=true",
     "Hash":"0xDC4DB296"
  },
  {
     "id":"weapon_revolver",
     "Name":"Heavy Revolver",
     "Type":"Handguns",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0xC1B3C3D1.png?raw=true",
     "Hash":"0xC1B3C3D1"
  },
  {
     "id":"weapon_revolver_mk2",
     "Name":"Heavy Revolver Mk II",
     "Type":"Handguns",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0xCB96392F.png?raw=true",
     "Hash":"0xCB96392F"
  },
  {
     "id":"weapon_doubleaction",
     "Name":"Snub Nose Revolver",
     "Type":"Handguns",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x97EA20B8.png?raw=true",
     "Hash":"0x97EA20B8"
  },
  {
     "id":"weapon_raypistol",
     "Name":"Up-n-Atomizer",
     "Type":"Handguns",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0xAF3696A1.png?raw=true",
     "Hash":"0xAF3696A1"
  },
  {
     "id":"weapon_ceramicpistol",
     "Name":"Ceramic Pistol",
     "Type":"Handguns",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x2B5EF5EC.png?raw=true",
     "Hash":"0x2B5EF5EC"
  },
  {
     "id":"weapon_navyrevolver",
     "Name":"Navy Revolver",
     "Type":"Handguns",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x917F6C8C.png?raw=true",
     "Hash":"0x917F6C8C"
  },
  {
     "id":"weapon_gadgetpistol",
     "Name":"Perico Pistol",
     "Type":"Handguns",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x57A4368C.png?raw=true",
     "Hash":"0x57A4368C"
  },
  {
     "id":"weapon_stungun_mp",
     "Name":"Stun Gun",
     "Type":"Handguns",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x45CD9CF3.png?raw=true",
     "Hash":"0x45CD9CF3"
  },
  {
     "id":"weapon_microsmg",
     "Name":"Micro SMG",
     "Type":"Submachine Guns",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x13532244.png?raw=true",
     "Hash":"0x13532244"
  },
  {
     "id":"weapon_tacticalsmg",
     "Name":"Tactical SMG",
     "Type":"Submachine Guns",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x14E5AFD5.png?raw=true",
     "Hash":"0x14E5AFD5"
  },
  {
     "id":"weapon_smg",
     "Name":"SMG",
     "Type":"Submachine Guns",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x2BE6766B.png?raw=true",
     "Hash":"0x2BE6766B"
  },
  {
     "id":"weapon_smg_mk2",
     "Name":"SMG Mk II",
     "Type":"Submachine Guns",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x78A97CD0.png?raw=true",
     "Hash":"0x78A97CD0"
  },
  {
     "id":"weapon_assaultsmg",
     "Name":"Assault SMG",
     "Type":"Submachine Guns",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0xEFE7E2DF.png?raw=true",
     "Hash":"0xEFE7E2DF"
  },
  {
     "id":"weapon_combatpdw",
     "Name":"Combat PDW",
     "Type":"Submachine Guns",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x0A3D4D34.png?raw=true",
     "Hash":"0x0A3D4D34"
  },
  {
     "id":"weapon_machinepistol",
     "Name":"Machine Pistol",
     "Type":"Submachine Guns",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0xDB1AA450.png?raw=true",
     "Hash":"0xDB1AA450"
  },
  {
     "id":"weapon_minismg",
     "Name":"Mini SMG",
     "Type":"Submachine Guns",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0xBD248B55.png?raw=true",
     "Hash":"0xBD248B55"
  },
  {
     "id":"weapon_raycarbine",
     "Name":"Unholy Hellbringer",
     "Type":"Submachine Guns",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x476BF155.png?raw=true",
     "Hash":"0x476BF155"
  },
  {
     "id":"weapon_pumpshotgun",
     "Name":"Pump Shotgun",
     "Type":"Shotguns",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x1D073A89.png?raw=true",
     "Hash":"0x1D073A89"
  },
  {
     "id":"weapon_pumpshotgun_mk2",
     "Name":"Pump Shotgun Mk II",
     "Type":"Shotguns",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x555AF99A.png?raw=true",
     "Hash":"0x555AF99A"
  },
  {
     "id":"weapon_sawnoffshotgun",
     "Name":"Sawed-Off Shotgun",
     "Type":"Shotguns",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x7846A318.png?raw=true",
     "Hash":"0x7846A318"
  },
  {
     "id":"weapon_assaultshotgun",
     "Name":"Assault Shotgun",
     "Type":"Shotguns",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0xE284C527.png?raw=true",
     "Hash":"0xE284C527"
  },
  {
     "id":"weapon_bullpupshotgun",
     "Name":"Bullpup Shotgun",
     "Type":"Shotguns",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x9D61E50F.png?raw=true",
     "Hash":"0x9D61E50F"
  },
  {
     "id":"weapon_musket",
     "Name":"Musket",
     "Type":"Shotguns",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0xA89CB99E.png?raw=true",
     "Hash":"0xA89CB99E"
  },
  {
     "id":"weapon_heavyshotgun",
     "Name":"Heavy Shotgun",
     "Type":"Shotguns",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x3AABBBAA.png?raw=true",
     "Hash":"0x3AABBBAA"
  },
  {
     "id":"weapon_dbshotgun",
     "Name":"Double Barrel Shotgun",
     "Type":"Shotguns",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0xEF951FBB.png?raw=true",
     "Hash":"0xEF951FBB"
  },
  {
     "id":"weapon_autoshotgun",
     "Name":"Sweeper Shotgun",
     "Type":"Shotguns",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x12E82D3D.png?raw=true",
     "Hash":"0x12E82D3D"
  },
  {
     "id":"weapon_combatshotgun",
     "Name":"Combat Shotgun",
     "Type":"Shotguns",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x5A96BA4.png?raw=true",
     "Hash":"0x5A96BA4"
  },
  {
     "id":"weapon_assaultrifle",
     "Name":"Assault Rifle",
     "Type":"Assault Rifles",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0xBFEFFF6D.png?raw=true",
     "Hash":"0xBFEFFF6D"
  },
  {
     "id":"weapon_assaultrifle_mk2",
     "Name":"Assault Rifle Mk II",
     "Type":"Assault Rifles",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x394F415C.png?raw=true",
     "Hash":"0x394F415C"
  },
  {
     "id":"weapon_carbinerifle",
     "Name":"Carbine Rifle",
     "Type":"Assault Rifles",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x83BF0278.png?raw=true",
     "Hash":"0x83BF0278"
  },
  {
     "id":"weapon_carbinerifle_mk2",
     "Name":"Carbine Rifle Mk II",
     "Type":"Assault Rifles",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0xFAD1F1C9.png?raw=true",
     "Hash":"0xFAD1F1C9"
  },
  {
     "id":"weapon_advancedrifle",
     "Name":"Advanced Rifle",
     "Type":"Assault Rifles",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0xAF113F99.png?raw=true",
     "Hash":"0xAF113F99"
  },
  {
     "id":"weapon_specialcarbine",
     "Name":"Special Carbine",
     "Type":"Assault Rifles",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0xC0A3098D.png?raw=true",
     "Hash":"0xC0A3098D"
  },
  {
     "id":"weapon_specialcarbine_mk2",
     "Name":"Special Carbine Mk II",
     "Type":"Assault Rifles",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x969C3D67.png?raw=true",
     "Hash":"0x969C3D67"
  },
  {
     "id":"weapon_bullpuprifle",
     "Name":"Bullpup Rifle",
     "Type":"Assault Rifles",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x7F229F94.png?raw=true",
     "Hash":"0x7F229F94"
  },
  {
     "id":"weapon_bullpuprifle_mk2",
     "Name":"Bullpup Rifle Mk II",
     "Type":"Assault Rifles",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x84D6FAFD.png?raw=true",
     "Hash":"0x84D6FAFD"
  },
  {
     "id":"weapon_compactrifle",
     "Name":"Compact Rifle",
     "Type":"Assault Rifles",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x624FE830.png?raw=true",
     "Hash":"0x624FE830"
  },
  {
     "id":"weapon_militaryrifle",
     "Name":"Military Rifle",
     "Type":"Assault Rifles",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x9D1F17E6.png?raw=true",
     "Hash":"0x9D1F17E6"
  },
  {
     "id":"weapon_heavyrifle",
     "Name":"Heavy Rifle",
     "Type":"Assault Rifles",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0xC78D71B4.png?raw=true",
     "Hash":"0xC78D71B4"
  },
  {
     "id":"weapon_tacticalrifle",
     "Name":"Tactical Rifle",
     "Type":"Assault Rifles",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0xD1D5F52B.png?raw=true",
     "Hash":"0xD1D5F52B"
  },
  {
     "id":"weapon_mg",
     "Name":"MG",
     "Type":"Machine Guns",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x9D07F764.png?raw=true",
     "Hash":"0x9D07F764"
  },
  {
     "id":"weapon_combatmg",
     "Name":"Combat MG",
     "Type":"Machine Guns",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x7FD62962.png?raw=true",
     "Hash":"0x7FD62962"
  },
  {
     "id":"weapon_combatmg_mk2",
     "Name":"Combat MG Mk II",
     "Type":"Machine Guns",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0xDBBD7280.png?raw=true",
     "Hash":"0xDBBD7280"
  },
  {
     "id":"weapon_gusenberg",
     "Name":"Gusenberg Sweeper",
     "Type":"Machine Guns",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x61012683.png?raw=true",
     "Hash":"0x61012683"
  },
  {
     "id":"weapon_sniperrifle",
     "Name":"Sniper Rifle",
     "Type":"Sniper Rifles",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x05FC3C11.png?raw=true",
     "Hash":"0x05FC3C11"
  },
  {
     "id":"weapon_heavysniper",
     "Name":"Heavy Sniper",
     "Type":"Sniper Rifles",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x0C472FE2.png?raw=true",
     "Hash":"0x0C472FE2"
  },
  {
     "id":"weapon_heavysniper_mk2",
     "Name":"Heavy Sniper Mk II",
     "Type":"Sniper Rifles",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0xA914799.png?raw=true",
     "Hash":"0xA914799"
  },
  {
     "id":"weapon_marksmanrifle",
     "Name":"Marksman Rifle",
     "Type":"Sniper Rifles",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0xC734385A.png?raw=true",
     "Hash":"0xC734385A"
  },
  {
     "id":"weapon_marksmanrifle_mk2",
     "Name":"Marksman Rifle Mk II",
     "Type":"Sniper Rifles",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x6A6C02E0.png?raw=true",
     "Hash":"0x6A6C02E0"
  },
  {
     "id":"weapon_precisionrifle",
     "Name":"Precision Rifle",
     "Type":"Sniper Rifles",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x6E7DDDEC.png?raw=true",
     "Hash":"0x6E7DDDEC"
  },
  {
     "id":"weapon_rpg",
     "Name":"RPG",
     "Type":"Heavy Weapons",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0xB1CA77B1.png?raw=true",
     "Hash":"0xB1CA77B1"
  },
  {
     "id":"weapon_grenadelauncher",
     "Name":"Grenade Launcher",
     "Type":"Heavy Weapons",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0xA284510B.png?raw=true",
     "Hash":"0xA284510B"
  },
  {
     "id":"weapon_grenadelauncher_smoke",
     "Name":"Grenade Launcher Smoke",
     "Type":"Heavy Weapons",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x4DD2DC56.png?raw=true",
     "Hash":"0x4DD2DC56"
  },
  {
     "id":"weapon_minigun",
     "Name":"Minigun",
     "Type":"Heavy Weapons",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x42BF8A85.png?raw=true",
     "Hash":"0x42BF8A85"
  },
  {
     "id":"weapon_firework",
     "Name":"Firework Launcher",
     "Type":"Heavy Weapons",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x7F7497E5.png?raw=true",
     "Hash":"0x7F7497E5"
  },
  {
     "id":"weapon_railgun",
     "Name":"Railgun",
     "Type":"Heavy Weapons",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x6D544C99.png?raw=true",
     "Hash":"0x6D544C99"
  },
  {
     "id":"weapon_hominglauncher",
     "Name":"Homing Launcher",
     "Type":"Heavy Weapons",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x63AB0442.png?raw=true",
     "Hash":"0x63AB0442"
  },
  {
     "id":"weapon_compactlauncher",
     "Name":"Compact Grenade Launcher",
     "Type":"Heavy Weapons",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x0781FE4A.png?raw=true",
     "Hash":"0x0781FE4A"
  },
  {
     "id":"weapon_rayminigun",
     "Name":"Widowmaker",
     "Type":"Heavy Weapons",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0xB62D1F67.png?raw=true",
     "Hash":"0xB62D1F67"
  },
  {
     "id":"weapon_emplauncher",
     "Name":"Compact EMP Launcher",
     "Type":"Heavy Weapons",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0xDB26713A.png?raw=true",
     "Hash":"0xDB26713A"
  },
  {
     "id":"weapon_grenade",
     "Name":"Grenade",
     "Type":"Thrown Weapons",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x93E220BD.png?raw=true",
     "Hash":"0x93E220BD"
  },
  {
     "id":"weapon_bzgas",
     "Name":"BZ Gas",
     "Type":"Thrown Weapons",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0xA0973D5E.png?raw=true",
     "Hash":"0xA0973D5E"
  },
  {
     "id":"weapon_molotov",
     "Name":"Molotov Cocktail",
     "Type":"Thrown Weapons",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x24B17070.png?raw=true",
     "Hash":"0x24B17070"
  },
  {
     "id":"weapon_stickybomb",
     "Name":"Sticky Bomb",
     "Type":"Thrown Weapons",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x2C3731D9.png?raw=true",
     "Hash":"0x2C3731D9"
  },
  {
     "id":"weapon_proxmine",
     "Name":"Proximity Mines",
     "Type":"Thrown Weapons",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0xAB564B93.png?raw=true",
     "Hash":"0xAB564B93"
  },
  {
     "id":"weapon_snowball",
     "Name":"Snowballs",
     "Type":"Thrown Weapons",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x787F0BB.png?raw=true",
     "Hash":"0x787F0BB"
  },
  {
     "id":"weapon_pipebomb",
     "Name":"Pipe Bombs",
     "Type":"Thrown Weapons",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0xBA45E8B8.png?raw=true",
     "Hash":"0xBA45E8B8"
  },
  {
     "id":"weapon_ball",
     "Name":"Baseball",
     "Type":"Thrown Weapons",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x23C9F95C.png?raw=true",
     "Hash":"0x23C9F95C"
  },
  {
     "id":"weapon_smokegrenade",
     "Name":"Tear Gas",
     "Type":"Thrown Weapons",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0xFDBC8A50.png?raw=true",
     "Hash":"0xFDBC8A50"
  },
  {
     "id":"weapon_flare",
     "Name":"Flare",
     "Type":"Thrown Weapons",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x497FACC3.png?raw=true",
     "Hash":"0x497FACC3"
  },
  {
     "id":"weapon_petrolcan",
     "Name":"Jerry Can",
     "Type":"Thrown Weapons",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x34A67B97.png?raw=true",
     "Hash":"0x34A67B97"
  },
  {
     "id":"gadget_parachute",
     "Name":"Parachute",
     "Type":"Thrown Weapons",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0xFBAB5776.png?raw=true",
     "Hash":"0xFBAB5776"
  },
  {
     "id":"weapon_fireextinguisher",
     "Name":"Fire Extinguisher",
     "Type":"Thrown Weapons",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x060EC506.png?raw=true",
     "Hash":"0x060EC506"
  },
  {
     "id":"weapon_hazardcan",
     "Name":"Hazardous Jerry Can",
     "Type":"Thrown Weapons",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0xBA536372.png?raw=true",
     "Hash":"0xBA536372"
  },
  {
     "id":"weapon_fertilizercan",
     "Name":"Fertilizer Can",
     "Type":"Thrown Weapons",
     "ImageSrc":"https://github.com/Rojta/gtav-weapon-images/blob/main/icons/0x184140A1.png?raw=true",
     "Hash":"0x184140A1"
  }
];
    
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
  
  //if(ServerUI) ServerUI.call('hud_toggle', false);
  //if(ServerUI) ServerUI.call('loadingscreen_start', 1000);
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

function enterExitCheck(){
  if(global.chatStatus) return;
  
  mp.events.callLocal("CS_OnPlayerTeleportCheck"); // Checked in Teleport.js
}

const hairDecalsList = [

  [ // male

      { name : "Close Shave",         collection :  "mpbeach_overlays",       overlay :  "FM_Hair_Fuzz"},

      { name : "Buzzcut",             collection :  "multiplayer_overlays",   overlay :  "NG_M_Hair_001"},

      { name : "Faux Hawk",           collection :  "multiplayer_overlays",   overlay :  "NG_M_Hair_002"},

      { name : "Hipster",             collection :  "multiplayer_overlays",   overlay :  "NG_M_Hair_003"},

      { name : "Side Parting",        collection :  "multiplayer_overlays",   overlay :  "NG_M_Hair_004"},

      { name : "Shorter Cut",         collection :  "multiplayer_overlays",   overlay :  "NG_M_Hair_005"},

      { name : "Biker",               collection :  "multiplayer_overlays",   overlay :  "NG_M_Hair_006"},

      { name : "Ponytail",            collection :  "multiplayer_overlays",   overlay :  "NG_M_Hair_007"},

      { name : "Cornrows",            collection :  "multiplayer_overlays",   overlay :  "NG_M_Hair_008"},

      { name : "Slicked",             collection :  "multiplayer_overlays",   overlay :  "NG_M_Hair_009"},

      { name : "Short Brushed",       collection :  "multiplayer_overlays",   overlay :  "NG_M_Hair_013"},

      { name : "Spikey",              collection :  "multiplayer_overlays",   overlay :  "NG_M_Hair_002"},

      { name : "Caesar",              collection :  "multiplayer_overlays",   overlay :  "NG_M_Hair_011"},

      { name : "Chopped",             collection :  "multiplayer_overlays",   overlay :  "NG_M_Hair_012"},

      { name : "Dreads",              collection :  "multiplayer_overlays",   overlay :  "NG_M_Hair_014"},

      { name : "Long Hair",           collection :  "multiplayer_overlays",   overlay :  "NG_M_Hair_015"},

      { name : "Shaggy Curls",        collection :  "multiplayer_overlays",   overlay :  "NGBea_M_Hair_000"},

      { name : "Surfer Dude",         collection :  "multiplayer_overlays",   overlay :  "NGBea_M_Hair_001"},

      { name : "Short Side Part",     collection :  "multiplayer_overlays",   overlay :  "NGBus_M_Hair_000"},

      { name : "High Slicked Sides",  collection :  "multiplayer_overlays",   overlay :  "NGBus_M_Hair_001"},

      { name : "Long Slicked",        collection :  "multiplayer_overlays",   overlay :  "NGHip_M_Hair_000"},

      { name : "Hipster Youth",       collection :  "multiplayer_overlays",   overlay :  "NGHip_M_Hair_001"},

      { name : "Mullet",              collection :  "multiplayer_overlays",   overlay :  "NGInd_M_Hair_000"},

      { name : "Classic Cornrows",    collection :  "mplowrider_overlays",    overlay :  "LR_M_Hair_000"},

      { name : "Palm Cornrows",       collection :  "mplowrider_overlays",    overlay :  "LR_M_Hair_001"},

      { name : "Lightning Cornrows",  collection :  "mplowrider_overlays",    overlay :  "LR_M_Hair_002"},

      { name : "Whipped Cornrows",    collection :  "mplowrider_overlays",    overlay :  "LR_M_Hair_003"},

      { name : "Zig Zag Cornrows",    collection :  "mplowrider2_overlays",   overlay :  "LR_M_Hair_004"},

      { name : "Snail Cornrows",      collection :  "mplowrider2_overlays",   overlay :  "LR_M_Hair_005"},

      { name : "Hightop",             collection :  "mplowrider2_overlays",   overlay :  "LR_M_Hair_006"},

      { name : "Loose Swept Back",    collection :  "mpbiker_overlays",       overlay :  "MP_Biker_Hair_000_M"},

      { name : "Undercut Swept Back", collection :  "mpbiker_overlays",       overlay :  "MP_Biker_Hair_001_M"},

      { name : "Undercut Swept Side", collection :  "mpbiker_overlays",       overlay :  "MP_Biker_Hair_002_M"},

      { name : "Spiked Mohawk",       collection :  "mpbiker_overlays",       overlay :  "MP_Biker_Hair_003_M"},

      { name : "Mod",                 collection :  "mpbiker_overlays",       overlay :  "MP_Biker_Hair_004_M"},

      { name : "Layered Mod",         collection :  "mpbiker_overlays",       overlay :  "MP_Biker_Hair_005_M"},

      { name : "Flattop",             collection :  "mpgunrunning_overlays",  overlay :  "MP_Gunrunning_Hair_M_000_M"},

      { name : "Military Buzzcut",    collection :  "mpgunrunning_overlays",  overlay :  "MP_Gunrunning_Hair_M_001_M"}

  ],

  [ // female

      { name : "Close Shave",         collection :  "mpbeach_overlays",       overlay :  "FM_Hair_Fuzz"},

      { name : "Short",               collection :  "multiplayer_overlays",   overlay :  "NG_F_Hair_001"},

      { name : "Layered Bob",         collection :  "multiplayer_overlays",   overlay :  "NG_F_Hair_002"},

      { name : "Pigtails",            collection :  "multiplayer_overlays",   overlay :  "NG_F_Hair_003"},

      { name : "Ponytail",            collection :  "multiplayer_overlays",   overlay :  "NG_F_Hair_004"},

      { name : "Braided Mohawk",      collection :  "multiplayer_overlays",   overlay :  "NG_F_Hair_005"},

      { name : "Braids",              collection :  "multiplayer_overlays",   overlay :  "NG_F_Hair_006"},

      { name : "Bob",                 collection :  "multiplayer_overlays",   overlay :  "NG_F_Hair_007"},

      { name : "Faux Hawk",           collection :  "multiplayer_overlays",   overlay :  "NG_F_Hair_008"},

      { name : "French Twist",        collection :  "multiplayer_overlays",   overlay :  "NG_F_Hair_009"},

      { name : "Long Bob",            collection :  "multiplayer_overlays",   overlay :  "NG_F_Hair_010"},

      { name : "Loose Tied",          collection :  "multiplayer_overlays",   overlay :  "NG_F_Hair_011"},

      { name : "Pixie",               collection :  "multiplayer_overlays",   overlay :  "NG_F_Hair_012"},

      { name : "Shaved Bangs",        collection :  "multiplayer_overlays",   overlay :  "NG_F_Hair_013"},

      { name : "Top Knot",            collection :  "multiplayer_overlays",   overlay :  "NG_M_Hair_014"},

      { name : "Wavy Bob",            collection :  "multiplayer_overlays",   overlay :  "NG_M_Hair_015"},

      { name : "Messy Bun",           collection :  "multiplayer_overlays",   overlay :  "NGBea_F_Hair_000"},

      { name : "Pin Up Girl",         collection :  "multiplayer_overlays",   overlay :  "NGBea_F_Hair_001"},

      { name : "Tight Bun",           collection :  "multiplayer_overlays",   overlay :  "NG_F_Hair_007"},

      { name : "Twisted Bob",         collection :  "multiplayer_overlays",   overlay :  "NGBus_F_Hair_000"},

      { name : "Flapper Bob",         collection :  "multiplayer_overlays",   overlay :  "NGBus_F_Hair_001"},

      { name : "Big Bangs",           collection :  "multiplayer_overlays",   overlay :  "NGBea_F_Hair_001"},

      { name : "Braided Top Knot",    collection :  "multiplayer_overlays",   overlay :  "NGHip_F_Hair_000"},

      { name : "Mullet",              collection :  "multiplayer_overlays",   overlay :  "NGInd_F_Hair_000"},

      { name : "Pinched Cornrows",    collection :  "mplowrider_overlays",    overlay :  "LR_F_Hair_000"},

      { name : "Leaf Cornrows",       collection :  "mplowrider_overlays",    overlay :  "LR_F_Hair_001"},

      { name : "Zig Zag Cornrows",    collection :  "mplowrider_overlays",    overlay :  "LR_F_Hair_002"},

      { name : "Pigtail Bangs",       collection :  "mplowrider2_overlays",   overlay :  "LR_F_Hair_003"},

      { name : "Wave Braids",         collection :  "mplowrider2_overlays",   overlay :  "LR_F_Hair_003"},

      { name : "Coil Braids",         collection :  "mplowrider2_overlays",   overlay :  "LR_F_Hair_004"},

      { name : "Rolled Quiff",        collection :  "mplowrider2_overlays",   overlay :  "LR_F_Hair_006"},

      { name : "Loose Swept Back",    collection :  "mpbiker_overlays",       overlay :  "MP_Biker_Hair_000_F"},

      { name : "Undercut Swept Back", collection :  "mpbiker_overlays",       overlay :  "MP_Biker_Hair_001_F"},

      { name : "Undercut Swept Side", collection :  "mpbiker_overlays",       overlay :  "MP_Biker_Hair_002_F"},

      { name : "Spiked Mohawk",       collection :  "mpbiker_overlays",       overlay :  "MP_Biker_Hair_003_F"},

      { name : "Bandana and Braid",   collection :  "multiplayer_overlays",   overlay :  "NG_F_Hair_003"},

      { name : "Layered Mod",         collection :  "mpbiker_overlays",       overlay :  "MP_Biker_Hair_006_F"},

      { name : "Skinbyrd",            collection :  "mpbiker_overlays",       overlay :  "MP_Biker_Hair_004_F"},

      { name : "Neat Bun",            collection :  "mpgunrunning_overlays",  overlay :  "MP_Gunrunning_Hair_F_000_F"},

      { name : "Short Bob",           collection :  "mpgunrunning_overlays",  overlay :  "MP_Gunrunning_Hair_F_001_F"}

  ]

];

const TATTOO_DATA = [
  {
      "overlay":"multiplayer_overlays",
      "data":[
          {
            "Name": "TAT_FM_008",
            "LocalizedName": "Skull",
            "HashNameMale": "FM_Tat_Award_M_000",
            "HashNameFemale": "FM_Tat_Award_F_000",
            "Zone": "ZONE_HEAD",
            "ZoneID": 1,
            "Price": 20000
          },
          {
            "Name": "TAT_FM_009",
            "LocalizedName": "Burning Heart",
            "HashNameMale": "FM_Tat_Award_M_001",
            "HashNameFemale": "FM_Tat_Award_F_001",
            "Zone": "ZONE_LEFT_ARM",
            "ZoneID": 2,
            "Price": 1400
          },
          {
            "Name": "TAT_FM_010",
            "LocalizedName": "Grim Reaper Smoking Gun",
            "HashNameMale": "FM_Tat_Award_M_002",
            "HashNameFemale": "FM_Tat_Award_F_002",
            "Zone": "ZONE_RIGHT_ARM",
            "ZoneID": 3,
            "Price": 9750
          },
          {
            "Name": "TAT_FM_011",
            "LocalizedName": "Blackjack",
            "HashNameMale": "FM_Tat_Award_M_003",
            "HashNameFemale": "FM_Tat_Award_F_003",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 2150
          },
          {
            "Name": "TAT_FM_012",
            "LocalizedName": "Hustler",
            "HashNameMale": "FM_Tat_Award_M_004",
            "HashNameFemale": "FM_Tat_Award_F_004",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 10000
          },
          {
            "Name": "TAT_FM_013",
            "LocalizedName": "Angel",
            "HashNameMale": "FM_Tat_Award_M_005",
            "HashNameFemale": "FM_Tat_Award_F_005",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 12400
          },
          {
            "Name": "TAT_FM_014",
            "LocalizedName": "Skull and Sword",
            "HashNameMale": "FM_Tat_Award_M_006",
            "HashNameFemale": "FM_Tat_Award_F_006",
            "Zone": "ZONE_RIGHT_LEG",
            "ZoneID": 5,
            "Price": 3500
          },
          {
            "Name": "TAT_FM_015",
            "LocalizedName": "Racing Blonde",
            "HashNameMale": "FM_Tat_Award_M_007",
            "HashNameFemale": "FM_Tat_Award_F_007",
            "Zone": "ZONE_LEFT_ARM",
            "ZoneID": 2,
            "Price": 4950
          },
          {
            "Name": "TAT_FM_016",
            "LocalizedName": "Los Santos Customs",
            "HashNameMale": "FM_Tat_Award_M_008",
            "HashNameFemale": "FM_Tat_Award_F_008",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 1350
          },
          {
            "Name": "TAT_FM_017",
            "LocalizedName": "Dragon and Dagger",
            "HashNameMale": "FM_Tat_Award_M_009",
            "HashNameFemale": "FM_Tat_Award_F_009",
            "Zone": "ZONE_LEFT_LEG",
            "ZoneID": 4,
            "Price": 1450
          },
          {
            "Name": "TAT_FM_018",
            "LocalizedName": "Ride or Die",
            "HashNameMale": "FM_Tat_Award_M_010",
            "HashNameFemale": "FM_Tat_Award_F_010",
            "Zone": "ZONE_RIGHT_ARM",
            "ZoneID": 3,
            "Price": 2700
          },
          {
            "Name": "TAT_FM_019",
            "LocalizedName": "Blank Scroll",
            "HashNameMale": "FM_Tat_Award_M_011",
            "HashNameFemale": "FM_Tat_Award_F_011",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 1200
          },
          {
            "Name": "TAT_FM_020",
            "LocalizedName": "Embellished Scroll",
            "HashNameMale": "FM_Tat_Award_M_012",
            "HashNameFemale": "FM_Tat_Award_F_012",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 1500
          },
          {
            "Name": "TAT_FM_021",
            "LocalizedName": "Seven Deadly Sins",
            "HashNameMale": "FM_Tat_Award_M_013",
            "HashNameFemale": "FM_Tat_Award_F_013",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 2650
          },
          {
            "Name": "TAT_FM_022",
            "LocalizedName": "Trust No One",
            "HashNameMale": "FM_Tat_Award_M_014",
            "HashNameFemale": "FM_Tat_Award_F_014",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 1900
          },
          {
            "Name": "TAT_FM_023",
            "LocalizedName": "Racing Brunette",
            "HashNameMale": "FM_Tat_Award_M_015",
            "HashNameFemale": "FM_Tat_Award_F_015",
            "Zone": "ZONE_LEFT_ARM",
            "ZoneID": 2,
            "Price": 4950
          },
          {
            "Name": "TAT_FM_024",
            "LocalizedName": "Clown",
            "HashNameMale": "FM_Tat_Award_M_016",
            "HashNameFemale": "FM_Tat_Award_F_016",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 2400
          },
          {
            "Name": "TAT_FM_025",
            "LocalizedName": "Clown and Gun",
            "HashNameMale": "FM_Tat_Award_M_017",
            "HashNameFemale": "FM_Tat_Award_F_017",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 5100
          },
          {
            "Name": "TAT_FM_026",
            "LocalizedName": "Clown Dual Wield",
            "HashNameMale": "FM_Tat_Award_M_018",
            "HashNameFemale": "FM_Tat_Award_F_018",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 7400
          },
          {
            "Name": "TAT_FM_027",
            "LocalizedName": "Clown Dual Wield Dollars",
            "HashNameMale": "FM_Tat_Award_M_019",
            "HashNameFemale": "FM_Tat_Award_F_019",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 10000
          },
          {
            "Name": "TAT_FM_204",
            "LocalizedName": "Brotherhood",
            "HashNameMale": "FM_Tat_M_000",
            "HashNameFemale": "FM_Tat_F_000",
            "Zone": "ZONE_RIGHT_ARM",
            "ZoneID": 3,
            "Price": 10000
          },
          {
            "Name": "TAT_FM_205",
            "LocalizedName": "Dragons",
            "HashNameMale": "FM_Tat_M_001",
            "HashNameFemale": "FM_Tat_F_001",
            "Zone": "ZONE_RIGHT_ARM",
            "ZoneID": 3,
            "Price": 12500
          },
          {
            "Name": "TAT_FM_209",
            "LocalizedName": "Melting Skull",
            "HashNameMale": "FM_Tat_M_002",
            "HashNameFemale": "FM_Tat_F_002",
            "Zone": "ZONE_LEFT_LEG",
            "ZoneID": 4,
            "Price": 3750
          },
          {
            "Name": "TAT_FM_206",
            "LocalizedName": "Dragons and Skull",
            "HashNameMale": "FM_Tat_M_003",
            "HashNameFemale": "FM_Tat_F_003",
            "Zone": "ZONE_RIGHT_ARM",
            "ZoneID": 3,
            "Price": 10000
          },
          {
            "Name": "TAT_FM_219",
            "LocalizedName": "Faith",
            "HashNameMale": "FM_Tat_M_004",
            "HashNameFemale": "FM_Tat_F_004",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 10000
          },
          {
            "Name": "TAT_FM_201",
            "LocalizedName": "Serpents",
            "HashNameMale": "FM_Tat_M_005",
            "HashNameFemale": "FM_Tat_F_005",
            "Zone": "ZONE_LEFT_ARM",
            "ZoneID": 2,
            "Price": 2400
          },
          {
            "Name": "TAT_FM_202",
            "LocalizedName": "Oriental Mural",
            "HashNameMale": "FM_Tat_M_006",
            "HashNameFemale": "FM_Tat_F_006",
            "Zone": "ZONE_LEFT_ARM",
            "ZoneID": 2,
            "Price": 5100
          },
          {
            "Name": "TAT_FM_210",
            "LocalizedName": "The Warrior",
            "HashNameMale": "FM_Tat_M_007",
            "HashNameFemale": "FM_Tat_F_007",
            "Zone": "ZONE_RIGHT_LEG",
            "ZoneID": 5,
            "Price": 3750
          },
          {
            "Name": "TAT_FM_211",
            "LocalizedName": "Dragon Mural",
            "HashNameMale": "FM_Tat_M_008",
            "HashNameFemale": "FM_Tat_F_008",
            "Zone": "ZONE_LEFT_LEG",
            "ZoneID": 4,
            "Price": 4800
          },
          {
            "Name": "TAT_FM_213",
            "LocalizedName": "Skull on the Cross",
            "HashNameMale": "FM_Tat_M_009",
            "HashNameFemale": "FM_Tat_F_009",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 12350
          },
          {
            "Name": "TAT_FM_218",
            "LocalizedName": "LS Flames",
            "HashNameMale": "FM_Tat_M_010",
            "HashNameFemale": "FM_Tat_F_010",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 2500
          },
          {
            "Name": "TAT_FM_214",
            "LocalizedName": "LS Script",
            "HashNameMale": "FM_Tat_M_011",
            "HashNameFemale": "FM_Tat_F_011",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 1900
          },
          {
            "Name": "TAT_FM_220",
            "LocalizedName": "Los Santos Bills",
            "HashNameMale": "FM_Tat_M_012",
            "HashNameFemale": "FM_Tat_F_012",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 10000
          },
          {
            "Name": "TAT_FM_215",
            "LocalizedName": "Eagle and Serpent",
            "HashNameMale": "FM_Tat_M_013",
            "HashNameFemale": "FM_Tat_F_013",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 4500
          },
          {
            "Name": "TAT_FM_207",
            "LocalizedName": "Flower Mural",
            "HashNameMale": "FM_Tat_M_014",
            "HashNameFemale": "FM_Tat_F_014",
            "Zone": "ZONE_RIGHT_ARM",
            "ZoneID": 3,
            "Price": 5000
          },
          {
            "Name": "TAT_FM_203",
            "LocalizedName": "Zodiac Skull",
            "HashNameMale": "FM_Tat_M_015",
            "HashNameFemale": "FM_Tat_F_015",
            "Zone": "ZONE_LEFT_ARM",
            "ZoneID": 2,
            "Price": 3600
          },
          {
            "Name": "TAT_FM_216",
            "LocalizedName": "Evil Clown",
            "HashNameMale": "FM_Tat_M_016",
            "HashNameFemale": "FM_Tat_F_016",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 12250
          },
          {
            "Name": "TAT_FM_212",
            "LocalizedName": "Tribal",
            "HashNameMale": "FM_Tat_M_017",
            "HashNameFemale": "FM_Tat_F_017",
            "Zone": "ZONE_RIGHT_LEG",
            "ZoneID": 5,
            "Price": 3500
          },
          {
            "Name": "TAT_FM_208",
            "LocalizedName": "Serpent Skull",
            "HashNameMale": "FM_Tat_M_018",
            "HashNameFemale": "FM_Tat_F_018",
            "Zone": "ZONE_RIGHT_ARM",
            "ZoneID": 3,
            "Price": 7500
          },
          {
            "Name": "TAT_FM_217",
            "LocalizedName": "The Wages of Sin",
            "HashNameMale": "FM_Tat_M_019",
            "HashNameFemale": "FM_Tat_F_019",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 12300
          },
          {
            "Name": "TAT_FM_221",
            "LocalizedName": "Dragon",
            "HashNameMale": "FM_Tat_M_020",
            "HashNameFemale": "FM_Tat_F_020",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 7500
          },
          {
            "Name": "TAT_FM_222",
            "LocalizedName": "Serpent Skull",
            "HashNameMale": "FM_Tat_M_021",
            "HashNameFemale": "FM_Tat_F_021",
            "Zone": "ZONE_LEFT_LEG",
            "ZoneID": 4,
            "Price": 5000
          },
          {
            "Name": "TAT_FM_223",
            "LocalizedName": "Fiery Dragon",
            "HashNameMale": "FM_Tat_M_022",
            "HashNameFemale": "FM_Tat_F_022",
            "Zone": "ZONE_RIGHT_LEG",
            "ZoneID": 5,
            "Price": 7300
          },
          {
            "Name": "TAT_FM_224",
            "LocalizedName": "Hottie",
            "HashNameMale": "FM_Tat_M_023",
            "HashNameFemale": "FM_Tat_F_023",
            "Zone": "ZONE_LEFT_LEG",
            "ZoneID": 4,
            "Price": 7250
          },
          {
            "Name": "TAT_FM_225",
            "LocalizedName": "Flaming Cross",
            "HashNameMale": "FM_Tat_M_024",
            "HashNameFemale": "FM_Tat_F_024",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 11900
          },
          {
            "Name": "TAT_FM_226",
            "LocalizedName": "LS Bold",
            "HashNameMale": "FM_Tat_M_025",
            "HashNameFemale": "FM_Tat_F_025",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 2750
          },
          {
            "Name": "TAT_FM_227",
            "LocalizedName": "Smoking Dagger",
            "HashNameMale": "FM_Tat_M_026",
            "HashNameFemale": "FM_Tat_F_026",
            "Zone": "ZONE_LEFT_LEG",
            "ZoneID": 4,
            "Price": 1750
          },
          {
            "Name": "TAT_FM_228",
            "LocalizedName": "Virgin Mary",
            "HashNameMale": "FM_Tat_M_027",
            "HashNameFemale": "FM_Tat_F_027",
            "Zone": "ZONE_RIGHT_ARM",
            "ZoneID": 3,
            "Price": 7300
          },
          {
            "Name": "TAT_FM_229",
            "LocalizedName": "Mermaid",
            "HashNameMale": "FM_Tat_M_028",
            "HashNameFemale": "FM_Tat_F_028",
            "Zone": "ZONE_RIGHT_ARM",
            "ZoneID": 3,
            "Price": 3250
          },
          {
            "Name": "TAT_FM_230",
            "LocalizedName": "Trinity Knot",
            "HashNameMale": "FM_Tat_M_029",
            "HashNameFemale": "FM_Tat_F_029",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 1000
          },
          {
            "Name": "TAT_FM_231",
            "LocalizedName": "Lucky Celtic Dogs",
            "HashNameMale": "FM_Tat_M_030",
            "HashNameFemale": "FM_Tat_F_030",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 5000
          },
          {
            "Name": "TAT_FM_232",
            "LocalizedName": "Lady M",
            "HashNameMale": "FM_Tat_M_031",
            "HashNameFemale": "FM_Tat_F_031",
            "Zone": "ZONE_LEFT_ARM",
            "ZoneID": 2,
            "Price": 7500
          },
          {
            "Name": "TAT_FM_233",
            "LocalizedName": "Faith",
            "HashNameMale": "FM_Tat_M_032",
            "HashNameFemale": "FM_Tat_F_032",
            "Zone": "ZONE_LEFT_LEG",
            "ZoneID": 4,
            "Price": 5100
          },
          {
            "Name": "TAT_FM_234",
            "LocalizedName": "Chinese Dragon",
            "HashNameMale": "FM_Tat_M_033",
            "HashNameFemale": "FM_Tat_F_033",
            "Zone": "ZONE_LEFT_LEG",
            "ZoneID": 4,
            "Price": 5050
          },
          {
            "Name": "TAT_FM_235",
            "LocalizedName": "Flaming Shamrock",
            "HashNameMale": "FM_Tat_M_034",
            "HashNameFemale": "FM_Tat_F_034",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 2450
          },
          {
            "Name": "TAT_FM_236",
            "LocalizedName": "Dragon",
            "HashNameMale": "FM_Tat_M_035",
            "HashNameFemale": "FM_Tat_F_035",
            "Zone": "ZONE_LEFT_LEG",
            "ZoneID": 4,
            "Price": 4950
          },
          {
            "Name": "TAT_FM_237",
            "LocalizedName": "Way of the Gun",
            "HashNameMale": "FM_Tat_M_036",
            "HashNameFemale": "FM_Tat_F_036",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 5100
          },
          {
            "Name": "TAT_FM_238",
            "LocalizedName": "Grim Reaper",
            "HashNameMale": "FM_Tat_M_037",
            "HashNameFemale": "FM_Tat_F_037",
            "Zone": "ZONE_LEFT_LEG",
            "ZoneID": 4,
            "Price": 12250
          },
          {
            "Name": "TAT_FM_239",
            "LocalizedName": "Dagger",
            "HashNameMale": "FM_Tat_M_038",
            "HashNameFemale": "FM_Tat_F_038",
            "Zone": "ZONE_RIGHT_ARM",
            "ZoneID": 3,
            "Price": 1150
          },
          {
            "Name": "TAT_FM_240",
            "LocalizedName": "Broken Skull",
            "HashNameMale": "FM_Tat_M_039",
            "HashNameFemale": "FM_Tat_F_039",
            "Zone": "ZONE_RIGHT_LEG",
            "ZoneID": 5,
            "Price": 7500
          },
          {
            "Name": "TAT_FM_241",
            "LocalizedName": "Flaming Skull",
            "HashNameMale": "FM_Tat_M_040",
            "HashNameFemale": "FM_Tat_F_040",
            "Zone": "ZONE_RIGHT_LEG",
            "ZoneID": 5,
            "Price": 7600
          },
          {
            "Name": "TAT_FM_242",
            "LocalizedName": "Dope Skull",
            "HashNameMale": "FM_Tat_M_041",
            "HashNameFemale": "FM_Tat_F_041",
            "Zone": "ZONE_LEFT_ARM",
            "ZoneID": 2,
            "Price": 2600
          },
          {
            "Name": "TAT_FM_243",
            "LocalizedName": "Flaming Scorpion",
            "HashNameMale": "FM_Tat_M_042",
            "HashNameFemale": "FM_Tat_F_042",
            "Zone": "ZONE_RIGHT_LEG",
            "ZoneID": 5,
            "Price": 2500
          },
          {
            "Name": "TAT_FM_244",
            "LocalizedName": "Indian Ram",
            "HashNameMale": "FM_Tat_M_043",
            "HashNameFemale": "FM_Tat_F_043",
            "Zone": "ZONE_RIGHT_LEG",
            "ZoneID": 5,
            "Price": 7450
          },
          {
            "Name": "TAT_FM_245",
            "LocalizedName": "Stone Cross",
            "HashNameMale": "FM_Tat_M_044",
            "HashNameFemale": "FM_Tat_F_044",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 7500
          },
          {
            "Name": "TAT_FM_246",
            "LocalizedName": "Skulls and Rose",
            "HashNameMale": "FM_Tat_M_045",
            "HashNameFemale": "FM_Tat_F_045",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 10000
          },
          {
            "Name": "TAT_FM_247",
            "LocalizedName": "Lion",
            "HashNameMale": "FM_Tat_M_047",
            "HashNameFemale": "FM_Tat_F_047",
            "Zone": "ZONE_RIGHT_ARM",
            "ZoneID": 3,
            "Price": 2500
          }
      ]
  },
  {
      "overlay":"mpchristmas3_overlays",
      "data":[
          {
            "Name": "TAT_X6_000",
            "LocalizedName": "The Christmas Spirit",
            "HashNameMale": "MP_Christmas3_Tat_000_M",
            "HashNameFemale": "MP_Christmas3_Tat_000_F",
            "Zone": "ZONE_LEFT_ARM",
            "ZoneID": 2,
            "Price": 27600
          },
          {
            "Name": "TAT_X6_001",
            "LocalizedName": "Festive Reaper",
            "HashNameMale": "MP_Christmas3_Tat_001_M",
            "HashNameFemale": "MP_Christmas3_Tat_001_F",
            "Zone": "ZONE_LEFT_ARM",
            "ZoneID": 2,
            "Price": 29500
          },
          {
            "Name": "TAT_X6_002",
            "LocalizedName": "Skull Bauble",
            "HashNameMale": "MP_Christmas3_Tat_002_M",
            "HashNameFemale": "MP_Christmas3_Tat_002_F",
            "Zone": "ZONE_RIGHT_ARM",
            "ZoneID": 3,
            "Price": 24100
          },
          {
            "Name": "TAT_X6_003",
            "LocalizedName": "Bony Snowman",
            "HashNameMale": "MP_Christmas3_Tat_003_M",
            "HashNameFemale": "MP_Christmas3_Tat_003_F",
            "Zone": "ZONE_RIGHT_ARM",
            "ZoneID": 3,
            "Price": 28500
          },
          {
            "Name": "TAT_X6_004",
            "LocalizedName": "Herbal Bouquet",
            "HashNameMale": "MP_Christmas3_Tat_004_M",
            "HashNameFemale": "MP_Christmas3_Tat_004_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 29100
          },
          {
            "Name": "TAT_X6_005",
            "LocalizedName": "Cash Krampus",
            "HashNameMale": "MP_Christmas3_Tat_005_M",
            "HashNameFemale": "MP_Christmas3_Tat_005_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 31200
          },
          {
            "Name": "TAT_X6_006",
            "LocalizedName": "All In One Night",
            "HashNameMale": "MP_Christmas3_Tat_006_M",
            "HashNameFemale": "MP_Christmas3_Tat_006_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 44800
          },
          {
            "Name": "TAT_X6_007",
            "LocalizedName": "A Little Present For You",
            "HashNameMale": "MP_Christmas3_Tat_007_M",
            "HashNameFemale": "MP_Christmas3_Tat_007_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 52000
          },
          {
            "Name": "TAT_X6_008",
            "LocalizedName": "Gingerbread Steed",
            "HashNameMale": "MP_Christmas3_Tat_008_M",
            "HashNameFemale": "MP_Christmas3_Tat_008_F",
            "Zone": "ZONE_RIGHT_LEG",
            "ZoneID": 5,
            "Price": 31100
          },
          {
            "Name": "TAT_X6_009",
            "LocalizedName": "Naughty Snow Globe",
            "HashNameMale": "MP_Christmas3_Tat_009_M",
            "HashNameFemale": "MP_Christmas3_Tat_009_F",
            "Zone": "ZONE_LEFT_LEG",
            "ZoneID": 4,
            "Price": 32400
          },
          {
            "Name": "TAT_X6_010",
            "LocalizedName": "Dude",
            "HashNameMale": "MP_Christmas3_Tat_010_M",
            "HashNameFemale": "MP_Christmas3_Tat_010_F",
            "Zone": "ZONE_HEAD",
            "ZoneID": 1,
            "Price": 27750
          },
          {
            "Name": "TAT_X6_011",
            "LocalizedName": "Fooligan Tribal",
            "HashNameMale": "MP_Christmas3_Tat_011_M",
            "HashNameFemale": "MP_Christmas3_Tat_011_F",
            "Zone": "ZONE_HEAD",
            "ZoneID": 1,
            "Price": 16500
          },
          {
            "Name": "TAT_X6_012",
            "LocalizedName": "Skull Jester",
            "HashNameMale": "MP_Christmas3_Tat_012_M",
            "HashNameFemale": "MP_Christmas3_Tat_012_F",
            "Zone": "ZONE_HEAD",
            "ZoneID": 1,
            "Price": 24500
          },
          {
            "Name": "TAT_X6_013",
            "LocalizedName": "Budonk-adonk!",
            "HashNameMale": "MP_Christmas3_Tat_013_M",
            "HashNameFemale": "MP_Christmas3_Tat_013_F",
            "Zone": "ZONE_HEAD",
            "ZoneID": 1,
            "Price": 0
          },
          {
            "Name": "TAT_X6_014",
            "LocalizedName": "Masked Machete Killer",
            "HashNameMale": "MP_Christmas3_Tat_014_M",
            "HashNameFemale": "MP_Christmas3_Tat_014_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 62250
          },
          {
            "Name": "TAT_X6_015",
            "LocalizedName": "Killer",
            "HashNameMale": "MP_Christmas3_Tat_015_M",
            "HashNameFemale": "MP_Christmas3_Tat_015_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 42600
          },
          {
            "Name": "TAT_X6_016",
            "LocalizedName": "Powwer",
            "HashNameMale": "MP_Christmas3_Tat_016_M",
            "HashNameFemale": "MP_Christmas3_Tat_016_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 39600
          },
          {
            "Name": "TAT_X6_017",
            "LocalizedName": "Two Headed Beast",
            "HashNameMale": "MP_Christmas3_Tat_017_M",
            "HashNameFemale": "MP_Christmas3_Tat_017_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 41000
          },
          {
            "Name": "TAT_X6_018",
            "LocalizedName": "Dudes",
            "HashNameMale": "MP_Christmas3_Tat_018_M",
            "HashNameFemale": "MP_Christmas3_Tat_018_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 38000
          },
          {
            "Name": "TAT_X6_019",
            "LocalizedName": "Fooligan Jester",
            "HashNameMale": "MP_Christmas3_Tat_019_M",
            "HashNameFemale": "MP_Christmas3_Tat_019_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 44450
          },
          {
            "Name": "TAT_X6_020",
            "LocalizedName": "Vile Smile",
            "HashNameMale": "MP_Christmas3_Tat_020_M",
            "HashNameFemale": "MP_Christmas3_Tat_020_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 14450
          },
          {
            "Name": "TAT_X6_021",
            "LocalizedName": "Demon Skull Jester",
            "HashNameMale": "MP_Christmas3_Tat_021_M",
            "HashNameFemale": "MP_Christmas3_Tat_021_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 42950
          },
          {
            "Name": "TAT_X6_022",
            "LocalizedName": "Fatal Incursion Outline",
            "HashNameMale": "MP_Christmas3_Tat_022_M",
            "HashNameFemale": "MP_Christmas3_Tat_022_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 32900
          },
          {
            "Name": "TAT_X6_023",
            "LocalizedName": "Many-Headed Beast",
            "HashNameMale": "MP_Christmas3_Tat_023_M",
            "HashNameFemale": "MP_Christmas3_Tat_023_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 49950
          },
          {
            "Name": "TAT_X6_024",
            "LocalizedName": "Demon Stitches",
            "HashNameMale": "MP_Christmas3_Tat_024_M",
            "HashNameFemale": "MP_Christmas3_Tat_024_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 27900
          },
          {
            "Name": "TAT_X6_025",
            "LocalizedName": "Collector",
            "HashNameMale": "MP_Christmas3_Tat_025_M",
            "HashNameFemale": "MP_Christmas3_Tat_025_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 64900
          },
          {
            "Name": "TAT_X6_026",
            "LocalizedName": "Fooligan Clown",
            "HashNameMale": "MP_Christmas3_Tat_026_M",
            "HashNameFemale": "MP_Christmas3_Tat_026_F",
            "Zone": "ZONE_LEFT_ARM",
            "ZoneID": 2,
            "Price": 29150
          },
          {
            "Name": "TAT_X6_027",
            "LocalizedName": "Orang-O-Tang Dude",
            "HashNameMale": "MP_Christmas3_Tat_027_M",
            "HashNameFemale": "MP_Christmas3_Tat_027_F",
            "Zone": "ZONE_RIGHT_ARM",
            "ZoneID": 3,
            "Price": 23300
          },
          {
            "Name": "TAT_X6_028",
            "LocalizedName": "Dude Outline",
            "HashNameMale": "MP_Christmas3_Tat_028_M",
            "HashNameFemale": "MP_Christmas3_Tat_028_F",
            "Zone": "ZONE_LEFT_ARM",
            "ZoneID": 2,
            "Price": 14500
          },
          {
            "Name": "TAT_X6_029",
            "LocalizedName": "Orang-O-Tang Gray",
            "HashNameMale": "MP_Christmas3_Tat_029_M",
            "HashNameFemale": "MP_Christmas3_Tat_029_F",
            "Zone": "ZONE_RIGHT_ARM",
            "ZoneID": 3,
            "Price": 19850
          },
          {
            "Name": "TAT_X6_030",
            "LocalizedName": "Dude Jester",
            "HashNameMale": "MP_Christmas3_Tat_030_M",
            "HashNameFemale": "MP_Christmas3_Tat_030_F",
            "Zone": "ZONE_LEFT_ARM",
            "ZoneID": 2,
            "Price": 26550
          },
          {
            "Name": "TAT_X6_031",
            "LocalizedName": "Sailor Fuku Killer",
            "HashNameMale": "MP_Christmas3_Tat_031_M",
            "HashNameFemale": "MP_Christmas3_Tat_031_F",
            "Zone": "ZONE_RIGHT_ARM",
            "ZoneID": 3,
            "Price": 24200
          },
          {
            "Name": "TAT_X6_032",
            "LocalizedName": "Fooligan",
            "HashNameMale": "MP_Christmas3_Tat_032_M",
            "HashNameFemale": "MP_Christmas3_Tat_032_F",
            "Zone": "ZONE_RIGHT_ARM",
            "ZoneID": 3,
            "Price": 27800
          },
          {
            "Name": "TAT_X6_033",
            "LocalizedName": "Fooligan Impotent Rage",
            "HashNameMale": "MP_Christmas3_Tat_033_M",
            "HashNameFemale": "MP_Christmas3_Tat_033_F",
            "Zone": "ZONE_LEFT_ARM",
            "ZoneID": 2,
            "Price": 27950
          },
          {
            "Name": "TAT_X6_034",
            "LocalizedName": "Zombie Head",
            "HashNameMale": "MP_Christmas3_Tat_034_M",
            "HashNameFemale": "MP_Christmas3_Tat_034_F",
            "Zone": "ZONE_LEFT_LEG",
            "ZoneID": 4,
            "Price": 32450
          },
          {
            "Name": "TAT_X6_035",
            "LocalizedName": "Erupting Skeleton",
            "HashNameMale": "MP_Christmas3_Tat_035_M",
            "HashNameFemale": "MP_Christmas3_Tat_035_F",
            "Zone": "ZONE_RIGHT_LEG",
            "ZoneID": 5,
            "Price": 31650
          },
          {
            "Name": "TAT_X6_036",
            "LocalizedName": "B'Donk Now Crank It Later",
            "HashNameMale": "MP_Christmas3_Tat_036_M",
            "HashNameFemale": "MP_Christmas3_Tat_036_F",
            "Zone": "ZONE_RIGHT_LEG",
            "ZoneID": 5,
            "Price": 34400
          },
          {
            "Name": "TAT_X6_037",
            "LocalizedName": "Orang-O-Tang Grenade",
            "HashNameMale": "MP_Christmas3_Tat_037_M",
            "HashNameFemale": "MP_Christmas3_Tat_037_F",
            "Zone": "ZONE_LEFT_LEG",
            "ZoneID": 4,
            "Price": 17500
          },
          {
            "Name": "TAT_X6_038",
            "LocalizedName": "Fool",
            "HashNameMale": "MP_Christmas3_Tat_038_M",
            "HashNameFemale": "MP_Christmas3_Tat_038_F",
            "Zone": "ZONE_LEFT_LEG",
            "ZoneID": 4,
            "Price": 19950
          },
          {
            "Name": "TAT_X6_039",
            "LocalizedName": "Jack Me",
            "HashNameMale": "MP_Christmas3_Tat_039_M",
            "HashNameFemale": "MP_Christmas3_Tat_039_F",
            "Zone": "ZONE_RIGHT_LEG",
            "ZoneID": 5,
            "Price": 29900
          },
          {
            "Name": "TAT_X6_040",
            "LocalizedName": "Monkey",
            "HashNameMale": "MP_Christmas3_Tat_040_M",
            "HashNameFemale": "MP_Christmas3_Tat_040_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 30000
          },
          {
            "Name": "TAT_X6_041",
            "LocalizedName": "Dragon",
            "HashNameMale": "MP_Christmas3_Tat_041_M",
            "HashNameFemale": "MP_Christmas3_Tat_041_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 30000
          },
          {
            "Name": "TAT_X6_042",
            "LocalizedName": "Snake",
            "HashNameMale": "MP_Christmas3_Tat_042_M",
            "HashNameFemale": "MP_Christmas3_Tat_042_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 30000
          },
          {
            "Name": "TAT_X6_043",
            "LocalizedName": "Goat",
            "HashNameMale": "MP_Christmas3_Tat_043_M",
            "HashNameFemale": "MP_Christmas3_Tat_043_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 30000
          },
          {
            "Name": "TAT_X6_044",
            "LocalizedName": "Rat",
            "HashNameMale": "MP_Christmas3_Tat_044_M",
            "HashNameFemale": "MP_Christmas3_Tat_044_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 30000
          },
          {
            "Name": "TAT_X6_045",
            "LocalizedName": "Rabbit",
            "HashNameMale": "MP_Christmas3_Tat_045_M",
            "HashNameFemale": "MP_Christmas3_Tat_045_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 0
          },
          {
            "Name": "TAT_X6_046",
            "LocalizedName": "Ox",
            "HashNameMale": "MP_Christmas3_Tat_046_M",
            "HashNameFemale": "MP_Christmas3_Tat_046_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 30000
          },
          {
            "Name": "TAT_X6_047",
            "LocalizedName": "Pig",
            "HashNameMale": "MP_Christmas3_Tat_047_M",
            "HashNameFemale": "MP_Christmas3_Tat_047_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 30000
          },
          {
            "Name": "TAT_X6_048",
            "LocalizedName": "Rooster",
            "HashNameMale": "MP_Christmas3_Tat_048_M",
            "HashNameFemale": "MP_Christmas3_Tat_048_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 30000
          },
          {
            "Name": "TAT_X6_049",
            "LocalizedName": "Dog",
            "HashNameMale": "MP_Christmas3_Tat_049_M",
            "HashNameFemale": "MP_Christmas3_Tat_049_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 30000
          },
          {
            "Name": "TAT_X6_050",
            "LocalizedName": "Horse",
            "HashNameMale": "MP_Christmas3_Tat_050_M",
            "HashNameFemale": "MP_Christmas3_Tat_050_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 30000
          },
          {
            "Name": "TAT_X6_051",
            "LocalizedName": "Tiger",
            "HashNameMale": "MP_Christmas3_Tat_051_M",
            "HashNameFemale": "MP_Christmas3_Tat_051_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 30000
          }
      ]
  },
  {
      "overlay":"mpheist4_overlays",
      "data":[
          {
            "Name": "TAT_H4_000",
            "LocalizedName": "Headphone Splat",
            "HashNameMale": "MP_Heist4_Tat_000_M",
            "HashNameFemale": "MP_Heist4_Tat_000_F",
            "Zone": "ZONE_RIGHT_ARM",
            "ZoneID": 3,
            "Price": 26400
          },
          {
            "Name": "TAT_H4_001",
            "LocalizedName": "Tropical Dude",
            "HashNameMale": "MP_Heist4_Tat_001_M",
            "HashNameFemale": "MP_Heist4_Tat_001_F",
            "Zone": "ZONE_RIGHT_ARM",
            "ZoneID": 3,
            "Price": 38550
          },
          {
            "Name": "TAT_H4_002",
            "LocalizedName": "Jellyfish Shades",
            "HashNameMale": "MP_Heist4_Tat_002_M",
            "HashNameFemale": "MP_Heist4_Tat_002_F",
            "Zone": "ZONE_RIGHT_ARM",
            "ZoneID": 3,
            "Price": 28500
          },
          {
            "Name": "TAT_H4_003",
            "LocalizedName": "Lighthouse",
            "HashNameMale": "MP_Heist4_Tat_003_M",
            "HashNameFemale": "MP_Heist4_Tat_003_F",
            "Zone": "ZONE_RIGHT_ARM",
            "ZoneID": 3,
            "Price": 34650
          },
          {
            "Name": "TAT_H4_004",
            "LocalizedName": "Skeleton Breeze",
            "HashNameMale": "MP_Heist4_Tat_004_M",
            "HashNameFemale": "MP_Heist4_Tat_004_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 39150
          },
          {
            "Name": "TAT_H4_005",
            "LocalizedName": "LSUR",
            "HashNameMale": "MP_Heist4_Tat_005_M",
            "HashNameFemale": "MP_Heist4_Tat_005_F",
            "Zone": "ZONE_RIGHT_ARM",
            "ZoneID": 3,
            "Price": 27000
          },
          {
            "Name": "TAT_H4_006",
            "LocalizedName": "Music Locker",
            "HashNameMale": "MP_Heist4_Tat_006_M",
            "HashNameFemale": "MP_Heist4_Tat_006_F",
            "Zone": "ZONE_RIGHT_ARM",
            "ZoneID": 3,
            "Price": 27000
          },
          {
            "Name": "TAT_H4_007",
            "LocalizedName": "Skeleton DJ",
            "HashNameMale": "MP_Heist4_Tat_007_M",
            "HashNameFemale": "MP_Heist4_Tat_007_F",
            "Zone": "ZONE_RIGHT_ARM",
            "ZoneID": 3,
            "Price": 22450
          },
          {
            "Name": "TAT_H4_008",
            "LocalizedName": "Smiley Glitch",
            "HashNameMale": "MP_Heist4_Tat_008_M",
            "HashNameFemale": "MP_Heist4_Tat_008_F",
            "Zone": "ZONE_RIGHT_ARM",
            "ZoneID": 3,
            "Price": 18550
          },
          {
            "Name": "TAT_H4_009",
            "LocalizedName": "Scratch Panther",
            "HashNameMale": "MP_Heist4_Tat_009_M",
            "HashNameFemale": "MP_Heist4_Tat_009_F",
            "Zone": "ZONE_LEFT_ARM",
            "ZoneID": 2,
            "Price": 25650
          },
          {
            "Name": "TAT_H4_010",
            "LocalizedName": "Tropical Serpent",
            "HashNameMale": "MP_Heist4_Tat_010_M",
            "HashNameFemale": "MP_Heist4_Tat_010_F",
            "Zone": "ZONE_LEFT_LEG",
            "ZoneID": 4,
            "Price": 26550
          },
          {
            "Name": "TAT_H4_011",
            "LocalizedName": "Soulwax",
            "HashNameMale": "MP_Heist4_Tat_011_M",
            "HashNameFemale": "MP_Heist4_Tat_011_F",
            "Zone": "ZONE_RIGHT_ARM",
            "ZoneID": 3,
            "Price": 27000
          },
          {
            "Name": "TAT_H4_012",
            "LocalizedName": "Still Slipping",
            "HashNameMale": "MP_Heist4_Tat_012_M",
            "HashNameFemale": "MP_Heist4_Tat_012_F",
            "Zone": "ZONE_RIGHT_ARM",
            "ZoneID": 3,
            "Price": 27000
          },
          {
            "Name": "TAT_H4_013",
            "LocalizedName": "Wild Dancers",
            "HashNameMale": "MP_Heist4_Tat_013_M",
            "HashNameFemale": "MP_Heist4_Tat_013_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 40100
          },
          {
            "Name": "TAT_H4_014",
            "LocalizedName": "Paradise Nap",
            "HashNameMale": "MP_Heist4_Tat_014_M",
            "HashNameFemale": "MP_Heist4_Tat_014_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 42300
          },
          {
            "Name": "TAT_H4_015",
            "LocalizedName": "Paradise Ukulele",
            "HashNameMale": "MP_Heist4_Tat_015_M",
            "HashNameFemale": "MP_Heist4_Tat_015_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 61100
          },
          {
            "Name": "TAT_H4_016",
            "LocalizedName": "Rose Panther",
            "HashNameMale": "MP_Heist4_Tat_016_M",
            "HashNameFemale": "MP_Heist4_Tat_016_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 48850
          },
          {
            "Name": "TAT_H4_017",
            "LocalizedName": "Tropical Sorcerer",
            "HashNameMale": "MP_Heist4_Tat_017_M",
            "HashNameFemale": "MP_Heist4_Tat_017_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 57750
          },
          {
            "Name": "TAT_H4_018",
            "LocalizedName": "Record Head",
            "HashNameMale": "MP_Heist4_Tat_018_M",
            "HashNameFemale": "MP_Heist4_Tat_018_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 49900
          },
          {
            "Name": "TAT_H4_019",
            "LocalizedName": "Record Shot",
            "HashNameMale": "MP_Heist4_Tat_019_M",
            "HashNameFemale": "MP_Heist4_Tat_019_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 31500
          },
          {
            "Name": "TAT_H4_020",
            "LocalizedName": "Speaker Tower",
            "HashNameMale": "MP_Heist4_Tat_020_M",
            "HashNameFemale": "MP_Heist4_Tat_020_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 40500
          },
          {
            "Name": "TAT_H4_021",
            "LocalizedName": "Skull Surfer",
            "HashNameMale": "MP_Heist4_Tat_021_M",
            "HashNameFemale": "MP_Heist4_Tat_021_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 64350
          },
          {
            "Name": "TAT_H4_022",
            "LocalizedName": "Paradise Sirens",
            "HashNameMale": "MP_Heist4_Tat_022_M",
            "HashNameFemale": "MP_Heist4_Tat_022_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 46770
          },
          {
            "Name": "TAT_H4_023",
            "LocalizedName": "Techno Glitch",
            "HashNameMale": "MP_Heist4_Tat_023_M",
            "HashNameFemale": "MP_Heist4_Tat_023_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 30450
          },
          {
            "Name": "TAT_H4_024",
            "LocalizedName": "Pineapple Skull",
            "HashNameMale": "MP_Heist4_Tat_024_M",
            "HashNameFemale": "MP_Heist4_Tat_024_F",
            "Zone": "ZONE_LEFT_LEG",
            "ZoneID": 4,
            "Price": 27800
          },
          {
            "Name": "TAT_H4_025",
            "LocalizedName": "Glow Princess",
            "HashNameMale": "MP_Heist4_Tat_025_M",
            "HashNameFemale": "MP_Heist4_Tat_025_F",
            "Zone": "ZONE_LEFT_LEG",
            "ZoneID": 4,
            "Price": 26700
          },
          {
            "Name": "TAT_H4_026",
            "LocalizedName": "Shark Water",
            "HashNameMale": "MP_Heist4_Tat_026_M",
            "HashNameFemale": "MP_Heist4_Tat_026_F",
            "Zone": "ZONE_RIGHT_ARM",
            "ZoneID": 3,
            "Price": 37000
          },
          {
            "Name": "TAT_H4_027",
            "LocalizedName": "Skullphones",
            "HashNameMale": "MP_Heist4_Tat_027_M",
            "HashNameFemale": "MP_Heist4_Tat_027_F",
            "Zone": "ZONE_RIGHT_LEG",
            "ZoneID": 5,
            "Price": 24350
          },
          {
            "Name": "TAT_H4_028",
            "LocalizedName": "Skull Waters",
            "HashNameMale": "MP_Heist4_Tat_028_M",
            "HashNameFemale": "MP_Heist4_Tat_028_F",
            "Zone": "ZONE_LEFT_LEG",
            "ZoneID": 4,
            "Price": 28950
          },
          {
            "Name": "TAT_H4_029",
            "LocalizedName": "Soundwaves",
            "HashNameMale": "MP_Heist4_Tat_029_M",
            "HashNameFemale": "MP_Heist4_Tat_029_F",
            "Zone": "ZONE_LEFT_LEG",
            "ZoneID": 4,
            "Price": 17850
          },
          {
            "Name": "TAT_H4_030",
            "LocalizedName": "Radio Tape",
            "HashNameMale": "MP_Heist4_Tat_030_M",
            "HashNameFemale": "MP_Heist4_Tat_030_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 37950
          },
          {
            "Name": "TAT_H4_031",
            "LocalizedName": "Octopus Shades",
            "HashNameMale": "MP_Heist4_Tat_031_M",
            "HashNameFemale": "MP_Heist4_Tat_031_F",
            "Zone": "ZONE_RIGHT_ARM",
            "ZoneID": 3,
            "Price": 31600
          },
          {
            "Name": "TAT_H4_032",
            "LocalizedName": "K.U.L.T. 99.1 FM",
            "HashNameMale": "MP_Heist4_Tat_032_M",
            "HashNameFemale": "MP_Heist4_Tat_032_F",
            "Zone": "ZONE_RIGHT_ARM",
            "ZoneID": 3,
            "Price": 27000
          }
      ]
  },
  {
      "overlay":"mpsecurity_overlays",
      "data":[
          {
            "Name": "TAT_FX_000",
            "LocalizedName": "Hood Skeleton",
            "HashNameMale": "MP_Security_Tat_000_M",
            "HashNameFemale": "MP_Security_Tat_000_F",
            "Zone": "ZONE_RIGHT_ARM",
            "ZoneID": 3,
            "Price": 23050
          },
          {
            "Name": "TAT_FX_001",
            "LocalizedName": "Bright Diamond",
            "HashNameMale": "MP_Security_Tat_001_M",
            "HashNameFemale": "MP_Security_Tat_001_F",
            "Zone": "ZONE_HEAD",
            "ZoneID": 1,
            "Price": 21250
          },
          {
            "Name": "TAT_FX_002",
            "LocalizedName": "Hustle",
            "HashNameMale": "MP_Security_Tat_002_M",
            "HashNameFemale": "MP_Security_Tat_002_F",
            "Zone": "ZONE_HEAD",
            "ZoneID": 1,
            "Price": 23500
          },
          {
            "Name": "TAT_FX_003",
            "LocalizedName": "Bandana Knife",
            "HashNameMale": "MP_Security_Tat_003_M",
            "HashNameFemale": "MP_Security_Tat_003_F",
            "Zone": "ZONE_RIGHT_LEG",
            "ZoneID": 5,
            "Price": 16400
          },
          {
            "Name": "TAT_FX_004",
            "LocalizedName": "Hood Heart",
            "HashNameMale": "MP_Security_Tat_004_M",
            "HashNameFemale": "MP_Security_Tat_004_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 57700
          },
          {
            "Name": "TAT_FX_005",
            "LocalizedName": "Peacock",
            "HashNameMale": "MP_Security_Tat_005_M",
            "HashNameFemale": "MP_Security_Tat_005_F",
            "Zone": "ZONE_RIGHT_ARM",
            "ZoneID": 3,
            "Price": 35000
          },
          {
            "Name": "TAT_FX_006",
            "LocalizedName": "Skeleton Shot",
            "HashNameMale": "MP_Security_Tat_006_M",
            "HashNameFemale": "MP_Security_Tat_006_F",
            "Zone": "ZONE_LEFT_ARM",
            "ZoneID": 2,
            "Price": 16600
          },
          {
            "Name": "TAT_FX_007",
            "LocalizedName": "Ballas 4 Life",
            "HashNameMale": "MP_Security_Tat_007_M",
            "HashNameFemale": "MP_Security_Tat_007_F",
            "Zone": "ZONE_RIGHT_ARM",
            "ZoneID": 3,
            "Price": 21100
          },
          {
            "Name": "TAT_FX_008",
            "LocalizedName": "Los Santos Tag",
            "HashNameMale": "MP_Security_Tat_008_M",
            "HashNameFemale": "MP_Security_Tat_008_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 13700
          },
          {
            "Name": "TAT_FX_009",
            "LocalizedName": "Ascension",
            "HashNameMale": "MP_Security_Tat_009_M",
            "HashNameFemale": "MP_Security_Tat_009_F",
            "Zone": "ZONE_RIGHT_ARM",
            "ZoneID": 3,
            "Price": 28800
          },
          {
            "Name": "TAT_FX_010",
            "LocalizedName": "Music Is The Remedy",
            "HashNameMale": "MP_Security_Tat_010_M",
            "HashNameFemale": "MP_Security_Tat_010_F",
            "Zone": "ZONE_LEFT_ARM",
            "ZoneID": 2,
            "Price": 17000
          },
          {
            "Name": "TAT_FX_011",
            "LocalizedName": "Serpent Mic",
            "HashNameMale": "MP_Security_Tat_011_M",
            "HashNameFemale": "MP_Security_Tat_011_F",
            "Zone": "ZONE_LEFT_ARM",
            "ZoneID": 2,
            "Price": 16700
          },
          {
            "Name": "TAT_FX_012",
            "LocalizedName": "Zombie Rhymes",
            "HashNameMale": "MP_Security_Tat_012_M",
            "HashNameFemale": "MP_Security_Tat_012_F",
            "Zone": "ZONE_RIGHT_ARM",
            "ZoneID": 3,
            "Price": 19500
          },
          {
            "Name": "TAT_FX_013",
            "LocalizedName": "Blessed Boombox",
            "HashNameMale": "MP_Security_Tat_013_M",
            "HashNameFemale": "MP_Security_Tat_013_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 51200
          },
          {
            "Name": "TAT_FX_014",
            "LocalizedName": "Chamberlain Hills",
            "HashNameMale": "MP_Security_Tat_014_M",
            "HashNameFemale": "MP_Security_Tat_014_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 54400
          },
          {
            "Name": "TAT_FX_015",
            "LocalizedName": "Smoking Barrels",
            "HashNameMale": "MP_Security_Tat_015_M",
            "HashNameFemale": "MP_Security_Tat_015_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 43200
          },
          {
            "Name": "TAT_FX_016",
            "LocalizedName": "All From The Same Tree",
            "HashNameMale": "MP_Security_Tat_016_M",
            "HashNameFemale": "MP_Security_Tat_016_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 41800
          },
          {
            "Name": "TAT_FX_017",
            "LocalizedName": "King of the Jungle",
            "HashNameMale": "MP_Security_Tat_017_M",
            "HashNameFemale": "MP_Security_Tat_017_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 32300
          },
          {
            "Name": "TAT_FX_018",
            "LocalizedName": "Night Owl",
            "HashNameMale": "MP_Security_Tat_018_M",
            "HashNameFemale": "MP_Security_Tat_018_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 39800
          },
          {
            "Name": "TAT_FX_019",
            "LocalizedName": "Weed Knuckles",
            "HashNameMale": "MP_Security_Tat_019_M",
            "HashNameFemale": "MP_Security_Tat_019_F",
            "Zone": "ZONE_LEFT_ARM",
            "ZoneID": 2,
            "Price": 17800
          },
          {
            "Name": "TAT_FX_020",
            "LocalizedName": "Dog Fist",
            "HashNameMale": "MP_Security_Tat_020_M",
            "HashNameFemale": "MP_Security_Tat_020_F",
            "Zone": "ZONE_RIGHT_ARM",
            "ZoneID": 3,
            "Price": 10200
          },
          {
            "Name": "TAT_FX_021",
            "LocalizedName": "Graffiti Skull",
            "HashNameMale": "MP_Security_Tat_021_M",
            "HashNameFemale": "MP_Security_Tat_021_F",
            "Zone": "ZONE_RIGHT_LEG",
            "ZoneID": 5,
            "Price": 29600
          },
          {
            "Name": "TAT_FX_022",
            "LocalizedName": "LS Smoking Cartridges",
            "HashNameMale": "MP_Security_Tat_022_M",
            "HashNameFemale": "MP_Security_Tat_022_F",
            "Zone": "ZONE_LEFT_LEG",
            "ZoneID": 4,
            "Price": 16100
          },
          {
            "Name": "TAT_FX_023",
            "LocalizedName": "Trust",
            "HashNameMale": "MP_Security_Tat_023_M",
            "HashNameFemale": "MP_Security_Tat_023_F",
            "Zone": "ZONE_LEFT_LEG",
            "ZoneID": 4,
            "Price": 24300
          },
          {
            "Name": "TAT_FX_024",
            "LocalizedName": "Beatbox Silhouette",
            "HashNameMale": "MP_Security_Tat_024_M",
            "HashNameFemale": "MP_Security_Tat_024_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 31600
          },
          {
            "Name": "TAT_FX_025",
            "LocalizedName": "Davis Flames",
            "HashNameMale": "MP_Security_Tat_025_M",
            "HashNameFemale": "MP_Security_Tat_025_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 20400
          },
          {
            "Name": "TAT_FX_026",
            "LocalizedName": "Dollar Guns Crossed",
            "HashNameMale": "MP_Security_Tat_026_M",
            "HashNameFemale": "MP_Security_Tat_026_F",
            "Zone": "ZONE_TORSO",
            "ZoneID": 0,
            "Price": 27400
          },
          {
            "Name": "TAT_FX_027",
            "LocalizedName": "Black Widow",
            "HashNameMale": "MP_Security_Tat_027_M",
            "HashNameFemale": "MP_Security_Tat_027_F",
            "Zone": "ZONE_HEAD",
            "ZoneID": 1,
            "Price": 24800
          }
      ]
  }
];
function updateBiometricsPosition(){
  var x = 10; var y = 25;
  var resolution = mp.game.graphics.getScreenActiveResolution(0,0);
  var bottom = ((resolution.y - (global.getMinimapAnchor().bottomY * resolution.y)) + y);
  var left = ((global.getMinimapAnchor().leftX * resolution.x) + x);

  if(resolution.y >= 1440 && resolution.y < 2160){
      bottom += 10;
  }
  else if(resolution.y >= 2160){
      bottom += 20;
  }

  global.ServerUI.call("hud_biometrics_position_set", `${left}px`, `${bottom}px`);
}
function getMinimapAnchor() {
  let sfX = 1.0 / 20.0;
  let sfY = 1.0 / 20.0;
  let safeZone = mp.game.graphics.getSafeZoneSize();
  let aspectRatio = mp.game.graphics.getScreenAspectRatio(false);
  let resolution = mp.game.graphics.getScreenActiveResolution(0, 0);
  let scaleX = 1.0 / resolution.x;
  let scaleY = 1.0 / resolution.y;

  let minimap = {
      width: scaleX * (resolution.x / (4 * aspectRatio)),
      height: scaleY * (resolution.y / 5.674),
      scaleX: scaleX,
      scaleY: scaleY,
      leftX: scaleX * (resolution.x * (sfX * (Math.abs(safeZone - 1.0) * 10))),
      bottomY: 1.0 - scaleY * (resolution.y * (sfY * (Math.abs(safeZone - 1.0) * 10))),
  };

  minimap.rightX = minimap.leftX + minimap.width;
  minimap.topY = minimap.bottomY - minimap.height;
  return minimap;
}

let global = {
  tattoosList:TATTOO_DATA,
  hairDecalsList:hairDecalsList,
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
  curEnterExitBind:0x58,
  lastEnterExitBind:0x58,

  lastVoiceBind: 0x4E,
  curVoiceBind: 0x4E,

  digitalHPAR: false,

  BankRobberyOn: false,

  ShowVehicleDL: false,
  specState: false,
  specTarget: null,
  isNoClip: false,

  isInInterior:false,

  vehicleManagerOpen:false,
  loadoutManagerOpen:false,

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
  isAntiPeekOn:false,
  ServerSettings:[],

  TeleportData:[],
  TeleportIndicators:[],
  TeleportEditorOpen:false,

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
  enterExitCheck,
  getMinimapAnchor,
  updateBiometricsPosition
};
exports = global;
