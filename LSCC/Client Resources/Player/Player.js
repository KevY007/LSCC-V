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
let chaseTimeMax = 600;
let chatStatus = false;

let secondUpdater = null;

mp.events.add("onOpenChatbox", (toggle) =>
{
	chatStatus = toggle;
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
    71,
    false,
    () => {
        if (mp.players.local.vehicle === null && chatStatus == false) {
            let playerPos = mp.players.local.position;
            let vehHandle = mp.game.vehicle.getClosestVehicle(playerPos.x, playerPos.y, playerPos.z, 30, 0, 70);
            let vehicle = mp.vehicles.atHandle(vehHandle);

            if (vehicle !== null) {
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
        mp.events.callRemote("OnScriptedKeyPress", 0x5A);
    }
});

function createCam(x, y, z, rx, ry, rz, viewangle) {
    camera = mp.cameras.new("Cam", { x, y, z }, { x: rx, y: ry, z: rz }, viewangle);
    camera.setActive(true);
    mp.game.cam.renderScriptCams(true, true, 20000000000000000000000000, false, false);
}

function destroyCam() {
    if (!camera) return;
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

function SecondTimer()
{
    if(mp.players.local.getVariable("pLogged") !== undefined && mp.players.local.getVariable("pLogged") == true)
    {
        
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

        if(mp.players.local.getVariable("JailTime") >= 1)
        {
            if(currentJailTime > 0) currentJailTime--;
            lastJailUpdate--;

            if(lastJailUpdate <= 0 || currentJailTime <= 0)
            {
                mp.events.callRemote("RequestJailUpdate", currentJailTime);
                lastJailUpdate = 20;
            }
        }
    }
    return;
}

secondUpdater = setInterval(SecondTimer, 1000);

function OnPlayerWeaponShot(targetPosition, targetEntity)
{
    if(targetEntity !== null && targetEntity !== undefined)
    {
        if(typeof targetEntity.getVariable === 'function')
        {
            if(targetEntity.getVariable("Team") !== undefined && targetEntity.getVariable("HasShot") !== undefined && 
                targetEntity.getVariable("Team") != -1) // Cops have HasShot too, cars don't, this gives us a 100% PLAYER entity handle.
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
    if(mp.players.local.getVariable("HasShot") == false && player.getVariable("Team") == 0)
    {
        mp.events.callRemote("GivePlayerShotTag");
    }
}

function OnPlayerEnterVehicle(vehicle, seat) {
    if(seat == -1)
    {
        mp.game.invoke(0x6E8834B52EC20C77, mp.players.local, false); // Disable driveby as DRIVER, (invoked SetPlayerCanDoDriveby) (doesnt work yet)
    }
    else
    {
        mp.game.invoke(0x6E8834B52EC20C77, mp.players.local, true);
    }
 }

 mp.events.add("playerEnterVehicle", OnPlayerEnterVehicle);
 mp.events.add("playerWeaponShot", OnPlayerWeaponShot);
 

mp.events.add(
    {
        "SetJailCounter": (time) => {
            currentJailTime = time;
            lastJailUpdate = 10;
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

                        target2.setInvincible(true);
                        target.setInvincible(true);
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
            updateChaseTimer();
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
                collisionText.text = "0:30";
                collisionText.visible = false;
            }

            if(chaseProgText == null)
            {
                chaseProgText = new timerBarLib.TimerBar("TIME LEFT");
                chaseProgText.text = "10:00";
                chaseProgText.visible = false;
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

        "SpectatePlayerCamera": (target) => {
            if (!camera) return;

            camera.attachTo(target.handle, 0, 0, 2.0, false);
            mp.game.cam.renderScriptCams(true, false, 0, true, false);

            camera.setActive(true);
        },

        "CreatePlayerCamera": (x, y, z, rx, ry, rz, viewangle) => {
            createCam(x, y, z, rx, ry, rz, viewangle);
        },

        "SetDiscordStatus": (serverName, status) => {
            mp.discord.update(serverName, status);
        },
    });