let Constants = require("./WeaponFiringMode/constants.js");
var global = require('./globals');
  const {TranslateMessage: Translate} = require('./Translator');

let lockedData = {}
let localPlayer  = mp.players.local
let currentWeapon = localPlayer.weapon
let ignoreCurrentWeapon = Constants.isWeaponIgnored(currentWeapon)
let weaponConfig = {}
let lastWeaponConfigUpdate = 0
let curFiringMode = 0
let curBurstShots = 0


mp.events.add("render", () => {
    if (localPlayer.weapon != currentWeapon) {
        currentWeapon = localPlayer.weapon;
        ignoreCurrentWeapon = Constants.isWeaponIgnored(currentWeapon);

        curFiringMode = weaponConfig[currentWeapon] === undefined ? Constants.firingModes.Auto : weaponConfig[currentWeapon];

        if (curFiringMode == Constants.firingModes.Burst) {
            if (!Constants.canWeaponUseBurstFire(currentWeapon)) curFiringMode = Constants.firingModes.Auto;
        } else if (curFiringMode == Constants.firingModes.Single) {
            if (!Constants.canWeaponUseSingleFire(currentWeapon)) curFiringMode = Constants.firingModes.Auto;
        }

        if ((curFiringMode == Constants.firingModes.Auto || curFiringMode == Constants.firingModes.Burst) && (Constants.isWeaponSingleFireOnly(currentWeapon) && lockedData[currentWeapon])) curFiringMode = Constants.firingModes.Single;

        curBurstShots = 0;
    }

    if (ignoreCurrentWeapon) return;

    if (curFiringMode != Constants.firingModes.Auto) {
        if (curFiringMode == Constants.firingModes.Burst) {
            if (localPlayer.isShooting()) curBurstShots++;
            if (curBurstShots > 0 && curBurstShots < 3) mp.game.controls.setControlNormal(0, 24, 1.0);

            if (curBurstShots == 3) {
                mp.game.player.disableFiring(false);
                if (mp.game.controls.isDisabledControlJustReleased(0, 24)) curBurstShots = 0;
            }

            if (localPlayer.isReloading()) curBurstShots = 0;
        } else if (curFiringMode == Constants.firingModes.Single) {
            if (mp.game.controls.isDisabledControlPressed(0, 24)) mp.game.player.disableFiring(false);
        } else if (curFiringMode == Constants.firingModes.Safe) {
            mp.game.player.disableFiring(false);
            if (mp.game.controls.isDisabledControlJustPressed(0, 24)) mp.game.audio.playSoundFrontend(-1, "Faster_Click", "RESPAWN_ONLINE_SOUNDSET", true);
        }
    }
});

mp.events.add("OneSecondEvent", () => {
    if (ignoreCurrentWeapon) return;

    if (Constants.isBoltAction(currentWeapon))
        global.FireModeDisplayText = "BOLT";
    else if(Constants.isPumpAction(currentWeapon))
        global.FireModeDisplayText = "PUMP";
    else
        global.FireModeDisplayText = Constants.firingModeNames[curFiringMode];
});

mp.keys.bind(0x42, false, () => {
    if((global.vehicleManagerOpen !== undefined && global.vehicleManagerOpen == true) || 
    (global.loadoutManagerOpen !== undefined && global.loadoutManagerOpen == true)) return;
    if((global.VehicleSpawnerActive !== undefined && global.VehicleSpawnerActive == true) ||
        (global.WeaponEditorActive !== undefined && global.WeaponEditorActive == true) ||
        (global.ActionMenuActive !== undefined && global.ActionMenuActive == true) ||
        (global.ClothingEditorActive !== undefined && global.ClothingEditorActive == true) ||
        (global.isHotwiring !== undefined && global.isHotwiring == true) ||
        (global.menuToggled !== undefined && global.menuToggled == true)){
        return;
    }
    if(Date.now() - global.lastChatToggle < 500 || global.chatStatus == true){
        return;
    }
    
    if (ignoreCurrentWeapon) return;

    let newFiringMode = curFiringMode - 1;

    if (newFiringMode < Constants.firingModes.Auto) newFiringMode = Constants.firingModes.Single;


    if (newFiringMode == Constants.firingModes.Burst) {
        if (!Constants.canWeaponUseBurstFire(currentWeapon)) newFiringMode = Constants.firingModes.Auto;
    } else if (newFiringMode == Constants.firingModes.Single) {
        if (!Constants.canWeaponUseSingleFire(currentWeapon)) newFiringMode = Constants.firingModes.Auto;
    }

    if ((newFiringMode == Constants.firingModes.Auto || newFiringMode == Constants.firingModes.Burst) && (Constants.isWeaponSingleFireOnly(currentWeapon) || lockedData[currentWeapon])) newFiringMode = Constants.firingModes.Single;


    if (curFiringMode != newFiringMode) {
        mp.events.callRemote("OnPlayerFiringModeChange")
        curFiringMode = newFiringMode;
        curBurstShots = 0;
        lastWeaponConfigUpdate = Date.now();

        mp.game.audio.playSoundFrontend(-1, "Faster_Click", "RESPAWN_ONLINE_SOUNDSET", true);
        weaponConfig[currentWeapon] = curFiringMode;
        
        //let notifyscoreboardstr = `toast('${Translate("FIRE_MODE")}', '${Translate("WEAPON_FIRE_MODE_SWITCH", [Constants.firingModeNames[curFiringMode]])}', 'nothing', 500, 2);`;
        //mp.events.callLocal("screenNotify", Translate("WEAPON_FIRE_MODE_SWITCH", [`~y~${Constants.firingModeNames[curFiringMode]}`])); 
        global.ServerUI.call('hud_weapon_fire_mode_toggle');
        global.ServerUI.call('hud_weapon_fire_mode_toggle', true);
        global.ServerUI.call("hud_weapon_fire_mode_switch", Translate("WEAPON_FIRE_MODE_SWITCH", [`${Constants.firingModeNames[curFiringMode]}`])); 
        //if(global.ServerUI) global.ServerUI.execute(notifyscoreboardstr);
    }
});

mp.events.add("FiringMode::UpdateModes", function(data){
    if (data){
        let entries = data.split("|")
        entries.forEach((entry) => {
            let [weapon, mode] = entry.split("=")
            weapon = parseInt(weapon, 36)
            
            
            let unlocked = mode.includes("*")
            mode = parseInt(mode.replace("*", ""))

            lockedData[weapon] = unlocked
            weaponConfig[weapon] = mode 

            if (currentWeapon == weapon){
                curFiringMode = mode
                curBurstShots = 0;
                lastWeaponConfigUpdate = Date.now()
            }
        })
    }
})


mp.game.audio.setAudioFlag("LoadMPData", true);