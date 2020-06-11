"use strict"

let camera = null;
const player = mp.players.local;
const timerBarLib = require("./Player/Bar.js");

let chaseProgBar = null;
let chaseProgText = null;
let collisionText = null;
let collisionTime = 0;
let chaseRunning = false;
let chaseTimeCur = 0;
let chaseTimeMax = 390;

let chatStatus = false;
let chatState = true;

let derbyText = null;
let derbyTimeText = null;
let derbyTimeCur = 0;
let derbyCounting = false;

let secondCounter = 0;
let updateRate = 200;
let updateInterval = null;

let specCam = null;
let specState = false;
let specTarget = null;
mp.voiceChat.muted = true;
let MaxVoiceRange = 50;

let playerBlips = [];

traceLastFunc("[Players] CreateVars");

mp.events.add("onOpenChatbox", (toggle) =>
{
    chatStatus = toggle;
    mp.events.callRemote("UpdateChatState", toggle);
    traceLastFunc(`[Players] ChatBox Toggled ${toggle}`);
});

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

mp.keys.bind(
    0x76,
    false,
    () => {
        if(!chatStatus && 
            !(mp.players.local.getVariable("pLogged") === undefined || mp.players.local.getVariable("pLogged") == false)) 
        {
            chatState = !chatState;
            mp.gui.chat.show(chatState);
        }
    }
);

mp.keys.bind(
    71,
    false,
    () => {
        if (mp.players.local.vehicle === null && chatStatus == false) {
            let playerPos = mp.players.local.position;
            let vehHandle = mp.game.vehicle.getClosestVehicle(playerPos.x, playerPos.y, playerPos.z, 30, 0, 70);
            let vehicle = mp.vehicles.atHandle(vehHandle);

            if (vehicle !== null) {
                traceLastFunc(`[Players] Attempt passenger-enter on veh`);

                let seat_pside_r = vehicle.getWorldPositionOfBone(vehicle.getBoneIndexByName("seat_pside_r"));
                let seat_pside_f = vehicle.getWorldPositionOfBone(vehicle.getBoneIndexByName("seat_pside_f"));
                let seat_dside_r = vehicle.getWorldPositionOfBone(vehicle.getBoneIndexByName("seat_dside_r"));
                let seat_r = vehicle.getWorldPositionOfBone(vehicle.getBoneIndexByName("seat_r"));

                let distance = calcDist(playerPos, seat_pside_r);
                let seat = 2;
                if (vehicle.isSeatFree(0) && calcDist(playerPos, seat_pside_f) < distance) {
                    distance = calcDist(playerPos, seat_pside_f);
                    seat = 0;
                }
                if (vehicle.isSeatFree(1) && calcDist(playerPos, seat_dside_r) < distance) {
                    distance = calcDist(playerPos, seat_dside_r);
                    seat = 1;
                }
                if (vehicle.isSeatFree(3) && calcDist(playerPos, seat_r) < distance) {
                    seat = 3;
                }

                if (vehicle.isSeatFree(seat))
                    mp.players.local.taskEnterVehicle(vehHandle, 5000, seat, 2.0, 1, 0);
            }
        }
    }
);

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

function UpdateTimer()
{
    secondCounter++;

    if(secondCounter >= Math.round(1000 / updateRate))
    {
        secondCounter = 0;

        if(mp.players.local.getVariable("pLogged") !== undefined && mp.players.local.getVariable("pLogged") == true)
        {
            traceLastFunc(`[Players] Update Stream Data`);
            mp.events.callRemote("UpdateStreamData");

            traceLastFunc(`[Players] Update VoiceData`);

            mp.players.forEachInStreamRange(target => {
                if(target.dimension == mp.players.local.dimension && mp.players.exists(target) && target != mp.players.local && 
                    target.handle !== 0)
                {
                    if(!target.voice3d) target.voice3d = true;
    
                    const playerPos = target.position;
                    var localPos = mp.players.local.position;
                    let dist = mp.game.system.vdist(playerPos.x, playerPos.y, playerPos.z, localPos.x, localPos.y, localPos.z);
                    
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
                    var targetPos = new mp.Vector3(specTarget.position.x, specTarget.position.y, specTarget.position.z);
                    targetPos.z -= 35;

                    mp.players.local.position = targetPos;
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
                }
                else
                {
                    collisionText.visible = false;
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

    for (let i = 0; i < mp.blips.length; i++)
    {
        if(mp.blips.at(i).id !== undefined)
        {
            var index = playerBlips.indexOf(mp.blips.at(i).id);
            if(index != -1)
            {
                mp.blips.at(i).destroy();
            }
        }
    }
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
                if(mp.game.system.vdist2(position.x, position.y, position.z, mp.players.local.position.x, mp.players.local.position.y, mp.players.local.position.z) >= 100 
                || (target.handle !== 0) == false || target.handle === null || 
                    target.handle === undefined)
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

    return;
}

updateInterval = setInterval(UpdateTimer, updateRate);

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

    if(updateInterval != null)
    {
        clearInterval(updateInterval);
        updateInterval = null;
    }

    updateRate = rate;

    updateInterval = setInterval(UpdateTimer, updateRate);
    mp.gui.chat.push("Updates now happen " + Math.round(1000 / updateRate) + " times a second.");
});

function OnPlayerWeaponShot(targetPosition, targetEntity)
{
    if(targetEntity !== null && targetEntity !== undefined)
    {
        if(typeof targetEntity.getVariable === 'function')
        {
            if(targetEntity.getVariable("Team") !== undefined && targetEntity.getVariable("HasShot") !== undefined) // Cops have HasShot too, cars don't, this gives us a 100% PLAYER entity handle.
            {
                if(targetEntity.isInAnyVehicle(false) == false)
                {
                    mp.events.callRemote("OnPlayerShotPlayer", targetPosition, targetEntity, mp.players.local.weapon);

                    if(targetEntity.getVariable("Team") != -1)
                    {
                        if(player.getVariable("Team") == targetEntity.getVariable("Team"))
                        {
                            mp.events.callRemote("PlayerShotAtWrongPlayer", 0);
                        }
                        if(player.getVariable("Team") == 1 && targetEntity.getVariable("Team") == 0 && targetEntity.getVariable("HasShot") == false) // Shot a fugitive that hasn't shot.
                        {
                            mp.events.callRemote("PlayerShotAtWrongPlayer", 1);
                        }
                    }
                }
            }
        }
    }
    if(mp.players.local.getVariable("HasShot") == false && player.getVariable("Team") == 0)
    {
        mp.events.callRemote("GivePlayerShotTag");
    }
}

mp.events.add("playerWeaponShot", OnPlayerWeaponShot);

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
        "ToggleChaseCollisions": (toggle) => {
            if(toggle)
            {
                mp.vehicles.forEach(target => {
                    mp.vehicles.forEach(target2 => {
                        target.setNoCollision(target2.handle, true);
                        target2.setNoCollision(target.handle, true);

                        target2.setInvincible(false);
                        target.setInvincible(false);
                    });
                });
            }
            else
            {
                mp.vehicles.forEach(target => {
                    mp.vehicles.forEach(target2 => {
                        target.setNoCollision(target2.handle, false);
                        target2.setNoCollision(target.handle, false);

                        target.setCanBeDamaged(true);
                        target2.setCanBeDamaged(true);
                        target.setCanBreak(true);
                        target2.setCanBreak(true);

                        target2.setInvincible(false);
                        target.setInvincible(false);
                    });
                });
            }
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

        "StartChaseTimer": (iTimeCur, iTimeMax, collisionDuration) => {

            chaseTimeCur = iTimeCur - 1;
            chaseTimeMax = iTimeMax;
            collisionTime = collisionDuration;
            chaseRunning = true;
        },

        "HideChaseTimer": () => {
            chaseProgBar.visible = false;
            chaseProgText.visible = false;
            collisionText.visible = false;
            chaseRunning = false;
        },

        "playerEnterVehicle": (vehicle, seat) => {
            vehicle.setInvincible(false);
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
    });