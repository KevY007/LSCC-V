
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

              global.ServerUI.call("set_user", {
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
                  tier: player.getVariable("ClanTier")
                }
              })
              /*
                if(global.ServerUI) global.ServerUI.execute(`gm.$refs.helpers.$refs.states.setUser(["stats"], {
                    serverId: ${player.remoteId},
                    id: ${player.getVariable("SQL_ID")},
                    name: "${player.name}",
                    level: ${player.getVariable("Level")},
                    fugitive: ${(player.getVariable("Team") == 0).toString()},
                    xp: ${player.getVariable("CurrentXP")},
                    nextLevelXp: ${player.getVariable("NextLevelXP")},
                    createdAt: "${player.getVariable("RegisterDate")}",
                    points: ${player.getVariable("Points")},
                    admin: ${player.getVariable("Admin")},
                    donator: ${player.getVariable("Donator")},
                    donatorPoints: ${player.getVariable("DonatorPoints")},
                    api: "${player.getVariable("ScreenshotServerAPI")}",
                    lobbyId: ${player.getVariable("InLobby")},
                    teamId: ${player.getVariable("Team")},
                    heading: ${player.heading},
                    location:{
                        x: ${player.position.x},
                        y: ${player.position.y},
                        z: ${player.position.z},
                    }
                })`);
                if(global.ServerUI) global.ServerUI.execute(`gm.$refs.helpers.$refs.states.setUser(["clan"], {
                    id: ${player.getVariable("ClanID")},
                    tier: ${player.getVariable("ClanTier")}
                })`);
                */
                
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
        if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.api.toggle(false);`);
        return;
    }

    global.ServerUI.call('chat:show', toggle);
    mp.gui.chat.show(toggle);
    //if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.api.toggle(${toggle.toString()})`);
};
function AddCustomVehiclesToSpawner(){
    if(global.ServerUI != null){

        let modelsList = ["lspdunm", "polbufac", "huntley2", "lspdvic", "prevolter", "m3e46", "pscout", "impala",
        "shegrangerold", "shergauntlet", "polbisonf", "gresleyh", "tulip2", "golfmk6", "bcso2slick", "bcat", "police3slick",
        "fibexecutioner", "police3umk"];

        for(let i = 0 ; i < modelsList.length; i++){
            if(global.ServerUI) global.ServerUI.execute(`gm.$refs.menus.$refs.vehicleSpawner.vehicles.push({
                model: "${modelsList[i]}",
                hash: "${modelsList[i]}",
                type: 25,
                image: "package://Artwork/Vehicles/${modelsList[i]}.png",
            });`);
        }
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
        //if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.api.toggleActive = false;`);
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
          //if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.api.toggleActive = false;`);
          return;
      }
    }

    //if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.api.toggleActive = ${toggle.toString()}`);
    //if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.api.toggleActive(${toggle.toString()});`);

    // if(toggle){
    //     if(mp.storage.data.menu.ActiveChatOpacity !== undefined){
    //         if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.settings.opacity = ${mp.storage.data.menu.ActiveChatOpacity.toString()}`);
    //     }
    //     else if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.settings.opacity = 1.0`);
    // }
    // else{
    //     increaseChatOpacity(3000);
    // }

    //global.ServerUI.call('chat:activate', toggle);
    //global.ServerUI.call('chat:show', toggle);
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

  WeaponEditorDOM:null,

  curMDCBind: 0x4B,
  lastMDCBind: 0x4B,
  curActionMenuBind: 0x59,
  lastActionMenuBind: 0x59,

  lastVoiceBind: 0x4E,
  curVoiceBind: 0x4E,

  BankRobberyOn: false,

  ShowVehicleDL: false,
  specState: false,
  specTarget: null,
  isNoClip: false,

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
