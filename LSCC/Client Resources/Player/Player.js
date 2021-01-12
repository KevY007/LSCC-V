"use strict"

let camera = null;
const player = mp.players.local;
const timerBarLib = require("./Player/Bar.js");

var chaseProgBar = null;
var chaseProgText = null;
var collisionText = null;
var collisionTime = -1;
var chaseRunning = false;
var chaseTimeCur = 0;
var chaseTimeMax = 390;
var chaseMode = 0; // 0= STANDARD | 1= BUSTED | and others as in SCRIPT

let derbyText = null;
let derbyTimeText = null;
let derbyTimeCur = 0;
let derbyCounting = false;

let secondCounter = 0;
let updateRate = 200;

let specCam = null;
let specState = false;
let specTarget = null;
mp.voiceChat.muted = true;
let MaxVoiceRange = 50;

let playerBlips = [];

var targetChecker = Date.now();
var updateChecker = Date.now();
var clickDisable = false;

var themeSoundID = 1;

var checker_lastHealth = 0;
var healthLossTime = Date.now();
var healthLoss = 0;

const bones = [0, 10706, 11816, 14201, 18905, 23553, 24816, 24817, 24818, 28252, 31086, 36864, 39317, 40269, 45509, 51826, 52301, 57005, 57597, 58271, 61163, 63931, 64729];
const boneDistanceThreshold = 1;

traceLastFunc("[Players] CreateVars");

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

function getPointToBoneOffset(aimX, aimY, aimZ, boneX, boneY, boneZ) {
    let xDif = (aimX - boneX) < 0 ? -(aimX - boneX) : aimX - boneX;
    let yDif = (aimY - boneY) < 0 ? -(aimY - boneY) : aimY - boneY;
    let zDif = (aimZ - boneZ) < 0 ? -(aimZ - boneZ) : aimZ - boneZ;

    return (xDif + yDif + zDif);
}

setInterval(() => {
    if(mp.players.local.getVariable("pLogged") !== undefined && mp.players.local.getVariable("pLogged") == true)
    {
        mp.events.callRemote("UpdateTabbedState", mp.system.isFocused);
        mp.events.callLocal("TwoSecondsEvent");
    }
}, 2000);

mp.keys.bind(0x5A, false, function() {
    if(chatStatus == false)
    {
        traceLastFunc(`[Players] Attempt tog GPS`);

        mp.events.callRemote("OnScriptedKeyPress", 0x5A, true);
    }
});


mp.keys.bind(0x4E, false, function() {
    traceLastFunc(`[Players] Disable Voice`);

	mp.events.callRemote("OnScriptedKeyPress", 0x4E, false);
    mp.voiceChat.muted = true;
});

mp.keys.bind(0x4E, true, function() {
	if(!chatStatus && 
		!(mp.players.local.getVariable("pLogged") === undefined || mp.players.local.getVariable("pLogged") == false)) 
	{
        traceLastFunc(`[Players] Enable Voice`);

		mp.voiceChat.muted = false;
		mp.events.callRemote("OnScriptedKeyPress", 0x4E, true);
	}
});

mp.keys.bind(0x09, true, function() {
	if(!chatStatus && mp.voiceChat.muted == false && 
		!(mp.players.local.getVariable("pLogged") === undefined || mp.players.local.getVariable("pLogged") == false)) 
	{
		mp.events.callRemote("OnScriptedKeyPress", 0x09, true);
	}
});

function createCam(x, y, z, rx, ry, rz, viewangle) {
    traceLastFunc(`[Players] Create Cam`);

    camera = mp.cameras.new("Cam", { x, y, z }, { x: rx, y: ry, z: rz }, viewangle);
    camera.setActive(true);
    mp.game.cam.renderScriptCams(true, true, 20000000000000000000000000, false, false);
}

mp.events.add("SendLastSeen", () => {
    const position = mp.players.local.position;
    let getStreet = mp.game.pathfind.getStreetNameAtCoord(position.x, position.y, position.z, 0, 0);

    zoneName = mp.game.ui.getLabelText(mp.game.zone.getNameOfZone(position.x, position.y, position.z));
    streetName = mp.game.ui.getStreetNameFromHashKey(getStreet.streetName);

    mp.events.callRemote("SentLastSeen", streetName, zoneName);
});

mp.events.add("render", () => {

    // Taken from GTA.World
    // IsPedArmed (0x475768A975D5AD17): 4 returns true if you are equipped with any weapon except Explosives weapon group AND melee weapons
    // If the player is armed with anything but a melee weapon or explosive weapon disable the lock-on functionality, otherwise enable it
    mp.game.invoke('0x475768A975D5AD17', player.handle, 4) ? mp.game.invoke('0x5C8B2F450EE4328E', player, false) :  mp.game.invoke('0x5C8B2F450EE4328E', player, true);

    if (specState == true && specCam != null && specTarget != null) {

        if (mp.players.exists(specTarget)) 
        {
            specCam.detach();
            specCam.attachTo(specTarget.handle, 0, -5, 1, true);
            specCam.setRot(0, 0, specTarget.getHeading(), 2);
        }
        else 
        {
            traceLastFunc(`[Players] Stop Spec (target handle)`);

            specCam.setActive(false); 
            specState = false;
            mp.events.callLocal("StopSpectating");
        }
    }

    if(checker_lastHealth != player.getHealth() && player.getHealth() >= 99 ) 
    {
        healthLoss = checker_lastHealth - player.getHealth();
        healthLossTime = Date.now();
        checker_lastHealth = player.getHealth();
    }
});

function destroyCam() {
    if (!camera) return;
    traceLastFunc(`[Players] Disable Cam`);

    camera.setActive(false);
    mp.game.cam.renderScriptCams(false, true, 0, true, true);
    camera.destroy();
    camera = null;
}

function TimeFormat(time)
{   
    var hrs = ~~(time / 3600);
    var mins = ~~((time % 3600) / 60);
    var secs = ~~time % 60;

    var ret = "";

    if (hrs > 0) {
        ret += "" + hrs + ":" + (mins < 10 ? "0" : "");
    }

    ret += "" + mins + ":" + (secs < 10 ? "0" : "");
    ret += "" + secs;
    return ret;
}

mp.events.addCommand("updaterate", function (rate) {
    rate = Number(rate);

    if (!Number.isInteger(rate)) {
        mp.gui.chat.push("Invalid rate (must be in numbers & milliseconds)");
        return;
    }

    if (rate < 10 || rate > 1000) {
        mp.gui.chat.push("Invalid rate, must be milliseconds between 10-1000");
        return;
    }

    updateRate = rate;

    mp.gui.chat.push("Updates now happen " + Math.round(1000 / updateRate) + " times a second.");
});

mp.events.add('render', () => {

    if(mp.players.local.getVariable("pLogged") !== undefined && mp.players.local.getVariable("pLogged") == true)
    {
        mp.game.player.restoreStamina(100);

        if(clickDisable) 
        { 
            mp.game.controls.disableControlAction(32, 257, true); 
            mp.game.player.disableFiring(true);
        }
        else { mp.game.controls.enableControlAction(32, 257, true); }

        traceLastFunc(`[Players] render::Parachute_Dive_Check();`);

        if(player.doesExist()) {
            if(mp.game.invoke('0x7DCE8BDA0F1C1200', player.handle)) // Is Ped Parachute Free Falling?
            {
                traceLastFunc(`[Players] Parachute_Open();`);
                mp.game.invoke('0x16E42E800B472221', player.handle); // Force Open Parachute (( 1.1 Bug where you can't open ))
            }

            if(player.isInAnyVehicle(false))
            {
                if (player.seat !== undefined && player.seat == -1 && player.getVariable("Team") >= 0)
                {
                    mp.game.controls.disableControlAction(32, 25, true); 
                    mp.game.controls.disableControlAction(32, 68, true); 
                    mp.game.controls.disableControlAction(32, 70, true); 
                    mp.game.controls.disableControlAction(32, 91, true); 
                    mp.game.controls.disableControlAction(32, 92, true); 
                    mp.game.controls.disableControlAction(32, 69, true); 
                    mp.game.controls.disableControlAction(32, 24, true); 
                }
            }
        }
    }

    if(Date.now() - updateChecker >= updateRate)
    {
        updateChecker = Date.now();

        secondCounter++;

        if(secondCounter >= Math.round(1000 / updateRate))
        {
            secondCounter = 0;

            if(mp.players.local.getVariable("pLogged") !== undefined && mp.players.local.getVariable("pLogged") == true)
            {
                mp.events.callLocal("OneSecondEvent");

                traceLastFunc(`[Players] Update Stream Data`);
                mp.events.callRemote("UpdateStreamData");

                traceLastFunc(`[Players] Update VoiceData`);

                mp.players.forEach(target => {
                    target.voiceVolume = 0.0;
                    if(!target.voice3d) target.voice3d = true;

                    if(target.dimension == mp.players.local.dimension && mp.players.exists(target) && target != mp.players.local && 
                        target.handle !== 0)
                    {
                        const playerPos = target.position;
                        var localPos = mp.players.local.position;
                        let dist = calcDist(playerPos, localPos);
                        
                        if(dist > MaxVoiceRange) {
                            if(target.getVariable("VoiceMode") == 1) {
                                if(target.getVariable("Team") == mp.players.local.getVariable("Team")) target.voiceVolume = 0.5;
                                else target.voiceVolume = 0.7;
                            }
                            else target.voiceVolume = 0.0;
                        }
                        else target.voiceVolume = 1 - (dist / MaxVoiceRange);
                    }
                });

                if (specState == true && specCam != null && specTarget != null) {

                    if (mp.players.exists(specTarget)) 
                    {
                        var position = specTarget.position;

                        if(calcDist(position, mp.players.local.position) >= 200.0 
                        || (specTarget.handle !== 0) == false || specTarget.handle === null || 
                            specTarget.handle === undefined)
                        {
                            position.x = specTarget.getVariable("UnstreamedX");
                            position.y = specTarget.getVariable("UnstreamedY");
                            position.z = specTarget.getVariable("UnstreamedZ");
                        }
                        position.z -= 35;

                        mp.players.local.position = position;
                    }
                    else 
                    {
                        specCam.setActive(false); 
                        specState = false;
                        mp.events.callLocal("StopSpectating");
                    }
                }

                traceLastFunc(`[Players] Update Derby Data`);

                if(mp.players.local.getVariable("InLobby") == -4)
                {
                    derbyText.text = "N/A";

                    derbyText.visible = true;
                    derbyTimeText.visible = true;

                    derbyTimeText.text = TimeFormat(derbyTimeCur);
                    if(derbyCounting)
                    {
                        mp.events.callRemote("DerbyIncreaseCount");
                        derbyTimeCur--;
                    }

                    var TimeNow = 0;

                    mp.players.forEach(target => {

                        if(target.getVariable("pLogged") !== undefined && target.getVariable("InLobby") == -4)
                        {
                            if(target.getVariable("DerbyTime") > TimeNow)
                            {
                                TimeNow = target.getVariable("DerbyTime");

                                derbyText.text = target.name;
                            }
                        }
                    });
                }
                else
                {
                    derbyText.visible = false;
                    derbyTimeText.visible = false;
                }

                traceLastFunc(`[Players] Update Chase Data1`);

                if(chaseProgBar != null && chaseProgText != null && chaseRunning == true)
                {
                    chaseTimeCur += 1;

                    chaseProgBar.progress = chaseTimeCur / chaseTimeMax;
                    chaseProgText.text = TimeFormat(chaseTimeMax - chaseTimeCur);

                    chaseProgText.visible = true;
                    chaseProgBar.visible = true;

                    if(chaseTimeCur >= chaseTimeMax)
                    {
                        chaseProgText.visible = false;
                        chaseProgBar.visible = false;
                        chaseRunning = false;
                    }
                }

                traceLastFunc(`[Players] Update Chase Data2`);

                if(collisionText != null && chaseRunning == true)
                {
                    if(collisionTime > 0)
                    {
                        collisionTime--;
                        collisionText.text = TimeFormat(collisionTime);

                        collisionText.visible = true;

                        mp.vehicles.forEach(target => {
                            mp.vehicles.forEach(target2 => {
                                target.setNoCollision(target2.handle, false);
                                target2.setNoCollision(target.handle, false);
        
                                target2.setInvincible(false);
                                target.setInvincible(false);
                            });
                        });
                    }
                    else if(collisionTime == 0)
                    {
                        collisionText.visible = false;
                        collisionTime = -1;

                        mp.vehicles.forEach(target => {
                            mp.vehicles.forEach(target2 => {
                                target.setNoCollision(target2.handle, true);
                                target2.setNoCollision(target.handle, true);
        
                                target.setCanBeDamaged(true);
                                target2.setCanBeDamaged(true);
                                target.setCanBreak(true);
                                target2.setCanBreak(true);
        
                                target2.setInvincible(false);
                                target.setInvincible(false);
                            });
                        });
                    }
                }

                traceLastFunc(`[Players] Update Chase Data3`);

                if(mp.players.local.getVariable("JailTime") >= 1)
                {
                    if(currentJailTime > 0) currentJailTime--;
                    lastJailUpdate--;

                    if(lastJailUpdate <= 0 || currentJailTime <= 0) // add current too so it unmutes/jails the second it hits 0. aka requests update
                    {
                        mp.events.callRemote("RequestCounterUpdate", currentJailTime, 0);
                        lastJailUpdate = 20;
                    }
                }
                else {
                    if(currentJailTime > 0) currentJailTime = 0;
                }

                if(mp.players.local.getVariable("MuteTime") >= 1)
                {
                    if(currentMuteTime > 0) currentMuteTime--;
                    lastMuteUpdate--;

                    if(lastMuteUpdate <= 0 || currentMuteTime <= 0)
                    {
                        mp.events.callRemote("RequestCounterUpdate", currentMuteTime, 1);
                        lastMuteUpdate = 20;
                    }
                }
                else {
                    if(currentMuteTime > 0) currentMuteTime = 0;
                }

                traceLastFunc(`[Players] Update Chase Data Complete`);
            }
        }

        traceLastFunc(`[Players] Blips Stage1`);

        mp.blips.forEach(eblip => {
            var index = playerBlips.indexOf(eblip.id);
            if(index != -1)
            {
                eblip.destroy();
            }
        });

        playerBlips = [];

        traceLastFunc(`[Players] Blips Stage2`);

        mp.players.forEach(target => {

            if(target.dimension == mp.players.local.dimension/* && target != mp.players.local*/)
            {
                if (target.getVariable("VisibleBlip") !== undefined && target.getVariable("VisibleBlip") == true)
                {
                    var blipColor = 45;
                    var blipId = 1;
                    if(target.getVariable("Team") == 0) 
                    {
                        blipColor = 75;
                        if(target.getVariable("HasShot") == true) blipColor = 76;
                    }
                    else if(target.getVariable("Team") == 1) blipColor = 3;
                    if(target.getVariable("InLobby") == -3) blipColor = 15;
                    if(target.getVariable("InLobby") == -4) blipColor = 73;
                    if(target.getVariable("InDM") != -1) blipColor = 39;
                    if(target.isInAnyVehicle(false)) blipId = 724;

                    var position = target.position;

                    if(calcDist(position, mp.players.local.position) >= 1000.0 
                    || (target.handle !== 0) == false || target.handle === null || 
                        target.handle === undefined || target.position == null || (target.position.x == 0.0 && target.position.z == 0.0))
                    {
                        position.x = target.getVariable("UnstreamedX");
                        position.y = target.getVariable("UnstreamedY");
                        position.z = target.getVariable("UnstreamedZ");
                        if(target.getVariable("UnstreamedState") == 1) blipId = 724;
                    }

                    var targetBlip = mp.blips.new(blipId, position,
                    {
                        name: target.name,
                        color: blipColor,
                        shortRange: false,
                        dimension: mp.players.local.dimension
                    });
                    playerBlips.push(targetBlip.id);
                }
            }
        });

        traceLastFunc(`[Players] Update Completed`);
    }

    if(Date.now() - targetChecker >= 75 && player.getVariable("Team") !== undefined && player.getVariable("Team") != -1)
    {
        targetChecker = Date.now();

        var target = mp.game.player.getEntityIsFreeAimingAt();
        clickDisable = false;

        if(mp.players.local.weaponAmmo > 0 && target !== undefined)
        {
            if(typeof target.getVariable === 'function')
            {
                if(target.getVariable("Team") !== undefined && target.getVariable("Team") != -1) 
                {
                    if(target.type === 'vehicle')
                    {
                        if(target.getVariable("Team") == player.getVariable("Team"))
                        {
                            clickDisable = true; // Disable shooting team-kill car by default
                            var vehicleOccupants = 0;
                            
                            mp.players.forEach(occupant => {
                                if(occupant.isInVehicle(target.handle, false)) {
                                    vehicleOccupants++;
                                    if(occupant.getVariable("Team") != player.getVariable("Team")) 
                                    {
                                        clickDisable = false; // Don't break shot if there's a fugi in or opposite team member!
                                    }
                                }
                            });

                            if(vehicleOccupants == 0) clickDisable = false;
                        }
                        else if(target.getVariable("Team") == 0 && player.getVariable("Team") == 1)
                        {
                            clickDisable = true; // Disable innocent car shots by default.
                            var vehicleOccupants = 0;

                            mp.players.forEach(occupant => {
                                if(occupant.isInVehicle(target.handle, false)) {
                                    vehicleOccupants++;
                                    if(occupant.getVariable("Team") == 0 && occupant.getVariable("HasShot") == true) {
                                        clickDisable = false; // Don't break shot if there's a fugitive that shot in!
                                    }
                                }
                            });

                            if(vehicleOccupants == 0) clickDisable = false;
                        }
                    }
                    else if(target.type === 'player')
                    {
                        if(target.getVariable("Team") != -1 && player.getVariable("Team") != -1)
                        {
                            if(player.getVariable("Team") == target.getVariable("Team"))
                            {
                                clickDisable = true;

                                if(target.isInAnyVehicle(true) && clickDisable == true && target.vehicle) {
                                    if(target.vehicle.getVariable("Team") !== undefined && target.vehicle.getVariable("Team") != target.getVariable("Team")) {
                                        clickDisable = false;
                                    }
                                    else {
                                        mp.players.forEach(occupant => {
                                            if(occupant.isInVehicle(target.vehicle.handle, true) && occupant.getVariable("Team") != target.getVariable("Team")) { 
                                                clickDisable = false; // Member of opposite team in it? Shoot.
                                            }
                                        });
                                    }
                                }
                            }

                            if(player.getVariable("Team") == 1 && target.getVariable("Team") == 0 && 
                                target.getVariable("HasShot") == false) // Disable click for fugi with no tag
                            {
                                clickDisable = true;

                                if(target.isInAnyVehicle(true) && clickDisable == true && target.vehicle)
                                {
                                    if(target.vehicle.getVariable("Team") !== undefined && target.vehicle.getVariable("Team") != target.getVariable("Team")) {
                                        clickDisable = false;
                                    }
                                    else {
                                        mp.players.forEach(occupant => {
                                            if(occupant.isInVehicle(target.vehicle.handle, true) && occupant.getVariable("Team") == target.getVariable("Team") && 
                                            occupant.getVariable("HasShot") == true) {
                                                clickDisable = false;
                                            }
                                        });
                                    }
                                }

                                if(player.weapon == 911657153) 
                                {
                                    clickDisable = false; // Stun Gun allow if conditions down don't match.

                                    //if(target.isInAnyVehicle(false) == true || calcDist(target.position, player.position) > 7.5) clickDisable = true;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
});

mp._events.add("playerWeaponShot", (targetPosition, targetEntity) => 
{
    if(targetEntity !== null && targetEntity !== undefined)
    {
        if(targetEntity.type === 'vehicle' && player.getVariable("Team") != -1)
        {
            if(targetEntity.getVariable("Team") !== undefined)
            {
                var vehicleOccupants = 0;
                let breakShot = false;

                if(targetEntity.getVariable("Team") == player.getVariable("Team"))
                {
                    breakShot = true; // Break team-kill car by default
                    
                    mp.players.forEach(occupant => {
                        if(occupant.isInVehicle(targetEntity.handle, false)) {
                            vehicleOccupants++;
                            if(occupant.getVariable("Team") != player.getVariable("Team")) 
                            {
                                breakShot = false; // Don't break shot if there's a fugi in or opposite team member!
                            }
                        }
                    });

                    if(vehicleOccupants == 0) breakShot = false;

                    if(breakShot) mp.game.graphics.notify("~r~Do not teamkill!");
                }
                else if(targetEntity.getVariable("Team") == 0 && player.getVariable("Team") == 1 && player.weapon != 911657153)
                {
                    breakShot = true; // Break innocent car shots by default.

                    mp.players.forEach(occupant => {
                        if(occupant.isInVehicle(targetEntity.handle, false)) {
                            vehicleOccupants++;
                            if(occupant.getVariable("Team") == 0 && occupant.getVariable("HasShot") == true) {
                                breakShot = false; // Don't break shot if there's a fugitive that shot in!
                            }
                        }
                    });

                    if(vehicleOccupants == 0) breakShot = false;

                    if(breakShot) mp.game.graphics.notify("~r~Do not deathmatch!");
                }

                if(breakShot == true) return true;
            }
        }
        if(targetEntity.type === 'player')
        {
            if(targetEntity.getVariable("Team") !== undefined && targetEntity.getVariable("HasShot") !== undefined)
            {
                if(targetEntity.isInAnyVehicle(false) == false)
                {
                    if(targetEntity.getVariable("Team") != -1)
                    {
                        if(player.getVariable("Team") == targetEntity.getVariable("Team"))
                        {
                            mp.events.callRemote("PlayerShotAtWrongPlayer", 0, targetEntity);
                            return true;
                        }
                        if(player.getVariable("Team") == 1 && targetEntity.getVariable("Team") == 0 && targetEntity.getVariable("HasShot") == false && 
                            player.weapon != 911657153) // Shot a fugitive that hasn't shot.
                        {
                            mp.events.callRemote("PlayerShotAtWrongPlayer", 1, targetEntity);
                            return true;
                        }
                    }
                }
                
                if(player.weapon == 911657153 && (targetEntity.isInAnyVehicle(false) == true || 
                calcDist(targetEntity.position, player.position) > 7.5)) {
                    mp.game.graphics.notify("~r~Taser out of reach, get closer!");
                    return true; // Stun Gun no shot if more than 10 meters or target in car.
                }

                let bone = -1;
                let previousOffset = 9999;

                for (let i = 0; i < bones.length; i++) {
                    let boneCoords = targetEntity.getBoneCoords(bones[i], 0, 0, 0);
                    let newOffset = getPointToBoneOffset(targetPosition.x, targetPosition.y, targetPosition.z, boneCoords.x, boneCoords.y, boneCoords.z);

                    if (newOffset < previousOffset) {
                        bone = bones[i];
                        previousOffset = newOffset;
                    }
                }

                if (previousOffset < boneDistanceThreshold)
                    mp.events.callRemote('opsp', targetEntity, bone, mp.players.local.weapon);
            }
        }
    }

    if(mp.players.local.getVariable("HasShot") == false && player.getVariable("Team") == 0)
    {
        mp.events.callRemote("GivePlayerShotTag");
    }
});


mp._events.add('incomingDamage', (sourceEntity, sourcePlayer, targetEntity, weapon, boneIndex, damage) => {

    if(targetEntity.type === 'player' && targetEntity == player)
    {
        if(sourceEntity.type === 'player')
        {
            if(sourceEntity.getVariable("Team") != -1 && player.getVariable("Team") != -1)
            {
                if(player.getVariable("Team") == sourceEntity.getVariable("Team"))
                {
                    return true;
                }

                if(sourceEntity.getVariable("Team") == 1 && player.getVariable("Team") == 0 && 
                    player.getVariable("HasShot") == false && weapon != 911657153) // Took damage from a cop and hasn't shot.
                {
                    return true;
                }
            }
        }
    }
})

mp._events.add("outgoingDamage", (sourceEntity, targetEntity, targetPlayer, weapon, boneIndex, damage) => {

    if(sourceEntity.type === 'player' && sourceEntity == player && player.getVariable("Team") != -1)
    {
        if(targetEntity.type === 'player')
        {
            if(targetEntity.getVariable("Team") != -1 && player.getVariable("Team") != -1)
            {
                if(sourceEntity.getVariable("Team") == targetEntity.getVariable("Team"))
                {
                    return true;
                }

                if(sourceEntity.getVariable("Team") == 1 && targetEntity.getVariable("Team") == 0 && 
                    targetEntity.getVariable("HasShot") == false) // Shot a fugitive that hasn't shot.
                {
                    if(weapon != 911657153) return true;

                    if(weapon == 911657153 && (targetEntity.isInAnyVehicle(false) == true || 
                        calcDist(targetEntity.position, player.position) > 7.5)) {

                        mp.game.graphics.notify("~r~Taser out of reach, get closer!");
                        return true; // Stun Gun no shot if more than 10 meters or target in car.
                    }
                }
            }
        }
        else if(targetEntity.type === 'vehicle' && player.getVariable("Team") != -1)
        {
            if(targetEntity.getVariable("Team") !== undefined && player.getVariable("Team") != -1)
            {
                let breakShot = false;
                var vehicleOccupants = 0;

                if(targetEntity.getVariable("Team") == player.getVariable("Team"))
                {
                    breakShot = true; // Break team-kill car by default
                    
                    mp.players.forEach(occupant => {
                        if(occupant.isInVehicle(targetEntity.handle, false)) {
                            vehicleOccupants++;
                            if(occupant.getVariable("Team") != player.getVariable("Team")) {
                                breakShot = false; // Don't break shot if there's a fugi in or opposite team member!
                            }
                        }
                    });

                    if(vehicleOccupants == 0) breakShot = false;

                    if(breakShot) mp.game.graphics.notify("~r~Do not teamkill!");
                }
                else if(targetEntity.getVariable("Team") == 0 && player.getVariable("Team") == 1 && weapon != 911657153)
                {
                    breakShot = true; // Break innocent car shots by default.

                    mp.players.forEach(occupant => {
                        if(occupant.isInVehicle(targetEntity.handle, false)) {
                            vehicleOccupants++;
                            if(occupant.getVariable("Team") == 0 && occupant.getVariable("HasShot") == true) {
                               breakShot = false; // Don't break shot if there's a fugitive that shot in!
                            }
                        }
                    });

                    if(vehicleOccupants == 0) breakShot = false;

                    if(breakShot) mp.game.graphics.notify("~r~Do not deathmatch!");
                }
                if(breakShot == true) return true;
            }
        }
    }
});

mp.events.add("entplayanim", (entity, animDictionary, animName, animFlag) => {
    if(entity.id == mp.players.local.id)
    {
        mp.events.callRemote("UpdateLastAnim", animDictionary, animName);
    }
	mp.game.streaming.requestAnimDict(animDictionary);
	new Promise(() => {
	const timer = setInterval(() => {
		if(mp.game.streaming.hasAnimDictLoaded(animDictionary)) {
			clearInterval(timer);
			resolve(entity, animDictionary, animName, animFlag);
		}
	}, 100);
});
});

function resolve(entity, animDictionary, animName, animFlag)
{
	entity.taskPlayAnim(animDictionary, animName, 8.0, 0.0, -1, animFlag, 0.0, false, false, false);
}

mp.events.add("entclearanim", (entity, animDictionary, animName) => {
    
    if(animDictionary != "null" || animDictionary != null)
    {
        entity.stopAnimTask(animDictionary, animName, 1.0);
    }
    entity.clearTasks();
});

mp.events.add(
    {
        "AttachWeaponComponent": (weapon, comp) => {
            mp.game.invoke("0xD966D51AA5B28BB9", player.handle, weapon, comp);
            mp.game.invoke("0xADF692B254977C0C", player.handle, weapon, true);
        },
        "PlayThemeSong": (link, level) => {
            themeSoundID = mp.game.audio.playSoundEx(link, level);
        },
        "StopThemeSong": () => {
            mp.game.audio.stopSoundEx(themeSoundID);
        },

        "SetHandcuffs": (status) => {
            if(status == true)
            {
                mp.players.local.clearTasksImmediately();

                mp.players.local.setEnableHandcuffs(true);

                mp.events.callRemote("play_anim", "mp_prison_break", "handcuffed", 32);
            }
            else
            {
                mp.players.local.setEnableHandcuffs(false);

                mp.players.local.clearTasksImmediately();
            }
        },
        "SetHandsup": (status) =>
        {
            if(status == true)
            {
                mp.events.callRemote("play_anim", "ped", "handsup_base", 50);
            }
            else
            {
                mp.events.callRemote("play_anim", "ped", "handsup_exit", 50);
                mp.players.local.stopAnim("handsup", "ped", 8.0);

                setTimeout(() => {
                    mp.players.local.stopAnim("handsup_exit", "ped", 8.0);
                    mp.players.local.clearTasksImmediately();
                }, 500);
            }
        },
        "SetDerbyCounting": (status, time) => {
            derbyTimeCur = time;
            derbyCounting = status;
        },
        "SetTimeWeather": (hour, minute, second, weather) => {
            mp.game.time.setClockTime(hour, minute, second);

            switch(weather) {
                case 0:
                    mp.game.gameplay.setWeatherTypeNow('EXTRASUNNY');
                    break;
                case 1:
                    mp.game.gameplay.setWeatherTypeNow('CLEAR');
                    break;
                case 2:
                    mp.game.gameplay.setWeatherTypeNow('CLOUDS');
                    break;
                case 3:
                    mp.game.gameplay.setWeatherTypeNow('SMOG');
                    break;
                case 4:
                    mp.game.gameplay.setWeatherTypeNow('FOGGY');
                    break;
                case 5:
                    mp.game.gameplay.setWeatherTypeNow('OVERCAST');
                    break;
                case 6:
                    mp.game.gameplay.setWeatherTypeNow('RAIN');
                    break;
                case 7:
                    mp.game.gameplay.setWeatherTypeNow('THUNDER');
                    break;
                case 8:
                    mp.game.gameplay.setWeatherTypeNow('CLEARING');
                    break;
                case 9:
                    mp.game.gameplay.setWeatherTypeNow('SNOWLIGHT');
                    break;
                default:
                    mp.game.gameplay.setWeatherTypeNow('CLOUDS');
                    break;
            }
        },
        "SpectatePlayer": (target) => {

            if(specCam != null)
            {
                specCam.setActive(false);
                specCam.destroy();
                specCam = null;
            }

            specTarget = target;
    
            mp.game.cam.renderScriptCams(false, false, 0, true, false);

            specCam = mp.cameras.new('default', target.position, new mp.Vector3(0, 0, target.getHeading()), target.isInAnyVehicle(false) ? 80 : 60);
            specCam.attachTo(target.handle, 0, target.isInAnyVehicle(false) ? -7 : -5, target.isInAnyVehicle(false) ? 2 : 1, true);
            specCam.setActive(true);

            mp.game.cam.renderScriptCams(true, false, 0, true, false);
    
            specState = true;
        },
    
        "StopSpectating": () => {

            specTarget = null;
            specState = false;
    
            mp.game.cam.renderScriptCams(false, false, 0, true, false);

            if(specCam != null)
            {
                specCam.setActive(false);
                specCam.destroy();
                specCam = null;
            }
        },

        "setToRagdoll": (time1, time2, ragdollType, p4, p5, p6) => {
            player.clearTasksImmediately();
            player.setToRagdoll(time1, time2, ragdollType, p4, p5, p6);
        },
        "SetJailCounter": (time) => {
            currentJailTime = time;
            lastJailUpdate = 10;
        },
        "SetMuteCounter": (time) => {
            currentMuteTime = time;
            lastMuteUpdate = 10;
        },

        "SetActorInvulnerable": (toggle) => {
            if(toggle)
            {
                player.setInvincible(true);
            }
            else
            {
                player.setInvincible(false);
            }
        },

        "CanFallOutOfCar": (toggle) => {
            if(toggle)
            {
                player.canKnockOffVehicle(true);
            }
            else
            {
                player.canKnockOffVehicle(false);
            }
        },

        "StartChaseTimer": (iTimeCur, iTimeMax, collisionDuration, mode) => {

            chaseTimeCur = iTimeCur - 1;
            chaseTimeMax = iTimeMax;
            collisionTime = collisionDuration;
            chaseRunning = true;
            chaseMode = mode;
        },

        "SendNotification": (title, text, attribute, duration) => {
            mp.system.notify(title, text, attribute, duration, false);
        },

        "HideChaseTimer": () => {
            chaseProgBar.visible = false;
            chaseProgText.visible = false;
            collisionText.visible = false;
            chaseRunning = false;
        },

        "playerEnterVehicle": (vehicle, seat) => {
            player.seat = seat;
            vehicle.setInvincible(false);

            if(mp.game.vehicle.isThisModelAHeli(vehicle.model) || mp.game.vehicle.isThisModelAPlane(vehicle.model)) {
                mp.events.callRemote("RequestParachute", vehicle);
            }
        },

        "InitChaseTimer": () => {

            if(chaseProgBar == null)
            {
                chaseProgBar = new timerBarLib.TimerBar("PROGRESS", true);
                chaseProgBar.pbarFgColor = [64, 115, 222, 255];
                chaseProgBar.pbarBgColor = [25, 53, 112, 255];
                chaseProgBar.progress = 1.0;
                chaseProgBar.visible = false;
            }

            if(collisionText == null)
            {
                collisionText = new timerBarLib.TimerBar("COLLISIONS ENABLE");
                collisionText.text = "99:99";
                collisionText.visible = false;
            }

            if(chaseProgText == null)
            {
                chaseProgText = new timerBarLib.TimerBar("TIME LEFT");
                chaseProgText.text = "99:99";
                chaseProgText.visible = false;
            }

            if(derbyTimeText == null)
            {
                derbyTimeText = new timerBarLib.TimerBar("WINNER IN");
                derbyTimeText.text = "99:99";
                derbyTimeText.visible = false;
            }

            if(derbyText == null)
            {
                derbyText = new timerBarLib.TimerBar(" ", false);
                derbyText.text = "N/A";
                derbyText.textColor = 109; // HUD_COLOUR_GOLD
                derbyText.usePlayerStyle = true;
                derbyText.visible = false;
            }
        },

        "DestroyPlayerCamera": () => destroyCam(),

        "SetPlayerFrozen": (status) => {
            player.freezePosition(status);

            if(player.vehicle)
            {
                player.vehicle.freezePosition(status);
            }
        },

        "CreatePlayerCamera": (x, y, z, rx, ry, rz, viewangle) => {
            createCam(x, y, z, rx, ry, rz, viewangle);
        },

        "SetDiscordStatus": (serverName, status) => {
            mp.discord.update(serverName, status);
        },

        "playerReady": (player) => {
            mp.events.callRemote("playerReadyCon");
        },
    });