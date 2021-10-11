"use strict"

let camera = null;
let camera_moveable = false;
let camera_lastmove = Date.now();

const player = mp.players.local;
let localPlayer = mp.players.local;
const timerBarLib = require("./Player/Bar.js");

let chaseStartVeh = -1;
let chaseStartSeat = -1;

let derbyText = null;
let derbyTimeCur = 0;
let derbyCounting = false;

let secondCounter = 0;

let dbDisabledMsg = false;
let dbDisabledMsgLast = Date.now();

if(mp.storage.data.menu !== undefined && mp.storage.data.menu.updateRate !== undefined)
{
    updateRate = mp.storage.data.menu.updateRate;
}

/* Spec Camera move settings */
let mouseSensitivity = 3.5;
let zoomSpeed = 1.5;
let minZoom = -60.0;
let maxZoom = 90.0;
let moveRotX = 0.0;
let moveRotY = 0.0;
let moveRotZ = 0.0;

let playerBlips = [];

var targetChecker = Date.now();
var updateChecker = Date.now();
var clickDisable = false;

let climbingStart = 0;
let climbTrigger = false;

var themeSoundID = 1;

var checker_lastHealth = 0;
var healthLossTime = Date.now();
var healthLoss = 0;

const bones = [0, 10706, 11816, 14201, 18905, 23553, 24816, 24817, 24818, 28252, 31086, 36864, 39317, 40269, 45509, 51826, 52301, 57005, 57597, 58271, 61163, 63931, 64729];
const boneDistanceThreshold = 1;

var lastGPSBind = 0x5A;
var curGPSBind = 0x5A;
var radioToggle = 0;

var ServerSettings = [];
var ShotStats = [];

var AFOB_Enabled = false;
var AFOB_LastF = Date.now();
var AFOB_LastMessage = Date.now();
var AFOB_LastVeh = null;
var AFOB_LastSeat = -1;
var AFOB_LastSpeed = 0;
var AFOB_SpeedLimit = 999;

var Seatbelt_Enabled = false;
var Seatbelt_LastMessage = Date.now();
var Seatbelt_LastVeh = null;
var Seatbelt_LastSeat = -1;

traceLastFunc("[Players] CreateVars");

function ResetShotStats()
{
    ShotStats.TotalFired = 0;
    ShotStats.HitPlayer = 0;
    ShotStats.HitVehicle = 0;
    ShotStats.Missed = 0;

    ShotStats.UpdateInterval = 0;

    traceLastFunc("[Players] ResetShotStats");
}

ResetShotStats();

mp.events.add("SetChaseVehicle", (vid, seat) => {
    chaseStartVeh = vid;
    chaseStartSeat = seat;
});

mp.keys.bind(0x46, true, function () {
    if(!chatStatus && !menuToggled && !scoreboardToggled) 
    {
        AFOB_LastF = Date.now();
        AFOB_LastSeat = -1;
        AFOB_LastVeh = null;
        AFOB_LastSpeed = 0;
    }
});

mp.events.add('entityStreamIn', (entity) => {
    if (chaseRunning && mp.players.local.Team >= 0 && entity.type === 'vehicle' 
        && chaseStartVeh != -1 && entity.remoteId !== undefined && entity.remoteId == chaseStartVeh)
    {
        if(chaseTimeCur <= 25) // Less than 25s passed to game start. If this doesn't make up, then get a better connection fag.
        {
            mp.events.callRemote("RequestSetInVehicle", entity, chaseStartSeat);
        }
    }
});

var HandcuffTooltipTime = 0;
var HandcuffTooltipShown = false;

mp.events.add("ShowHandcuffTooltip", () => {
    HandcuffTooltipTime = Date.now() + 19000;

    if(!HandcuffTooltipShown)
        mp.events.callLocal("AddTooltip", "H", "Handcuff", 1);

    HandcuffTooltipShown = true;
});

mp.events.add("OneSecondEvent", () => {
    ShotStats.UpdateInterval++;

    if(ShotStats.UpdateInterval >= 10)
    {
        ShotStats.UpdateInterval = 0;
        mp.events.callRemoteUnreliable("SyncShotStats", ShotStats.TotalFired, ShotStats.HitPlayer, ShotStats.HitVehicle, ShotStats.Missed);
        mp.events.callRemote("UpdatePacketloss");
    }

    if(HandcuffTooltipShown){

        if(Date.now() >= HandcuffTooltipTime || mp.players.local.Team != 1){
            mp.events.callLocal("RemoveTooltip", "H");

            HandcuffTooltipTime = 0;
            HandcuffTooltipShown = false;
        }
    }
});

mp.events.add("ResolveJS", (code) => {
    var result = "uh";

    try {
        result = eval(code);

    }
    catch(err) {
        result = err.message;
    }
	if(result === undefined) result = "undefined";
    mp.events.callRemote("JSResolved", code.toString(), result.toString());
});

mp.events.add("TwoSecondsEvent", () => {
    if(mp.players.local.Team !== undefined && mp.players.local.Team == 0)
    {
        if(mp.players.local.isSwimming() && !mp.players.local.isDead() && !mp.players.local.isInAnyVehicle(false)) {
            mp.events.callRemote("RequestSwimPenalty");
        }
    }

    switch(mp.players.local.Team){
        case 0:
            mp.game.ui.setHudColour(143, 255, 174, 0, 128);
            mp.game.ui.setHudColour(144, 255, 174, 0, 128);
            mp.game.ui.setHudColour(145, 255, 174, 0, 128);
            break;
        case 1:
            mp.game.ui.setHudColour(143, 0, 151, 255, 128);
            mp.game.ui.setHudColour(144, 0, 151, 255, 128);
            mp.game.ui.setHudColour(145, 0, 151, 255, 128);  
            break;
        default:     
            mp.game.ui.setHudColour(143, 110, 200, 200, 128);
            mp.game.ui.setHudColour(144, 110, 200, 200, 128);
            mp.game.ui.setHudColour(145, 110, 200, 200, 128);  
            break; 
    }
});

mp.events.add("ResetShotStats", ResetShotStats);

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


function TogGPSMode()
{
    if(menuToggled !== undefined && menuToggled) return;
    if(Date.now() - lastChatToggle >= 500 && chatStatus == false && 
        (mp.players.local.LoggedIn !== undefined && mp.players.local.LoggedIn == true))
    {
        traceLastFunc(`[Players] Attempt tog GPS`);
        
        mp.events.callRemote("OnScriptedKeyPress", 0x5A, true);
    }
}

if(mp.storage.data.gpsbind !== undefined)
{
    curGPSBind = mp.storage.data.gpsbind;
    lastGPSBind = mp.storage.data.gpsbind;
}

if(mp.storage.data.radiotoggle !== undefined)
{
    radioToggle = mp.storage.data.radiotoggle;
}

mp.keys.bind(lastGPSBind, true, TogGPSMode);

mp.events.addCommand("gpsbind", function (bind) {
    
    bind = Number(bind);

    if (!Number.isInteger(bind)) {
        mp.gui.chat.push("USAGE: gpsbind (keycode) --Input a virtual keycode e.g. 0x5A is 'Z' (default)");
        mp.gui.chat.push("https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes");
        return;
    }

    mp.storage.data.gpsbind = bind;

    mp.storage.flush();

    lastGPSBind = curGPSBind;
    curGPSBind = bind;

    mp.keys.unbind(lastGPSBind, true, TogGPSMode);
    mp.keys.bind(curGPSBind, true, TogGPSMode);

    mp.events.callLocal("HUDNotify", `GPS key is now binded to <strong>${String.fromCharCode(curGPSBind)}</strong>.`, "blue", "fas fa-check", false, 
        1000, "bottomCenter", "replace");

    mp.events.callLocal("RemoveTooltip", String.fromCharCode(lastGPSBind));
});

mp.keys.bind(0x4C, false, function() {
    if(menuToggled !== undefined && menuToggled) return;
    if(Date.now() - lastChatToggle >= 500 && chatStatus == false)
    {
        traceLastFunc(`[Players] Attempt L key/lock`);

        mp.events.callRemote("OnScriptedKeyPress", 0x4C, true);
    }
});

mp.keys.bind(0x59, false, function() {
    if(menuToggled !== undefined && menuToggled) return;
    if(Date.now() - lastChatToggle >= 500 && chatStatus == false)
    {
        traceLastFunc(`[Players] Attempt Y key/engine`);

        mp.events.callRemote("OnScriptedKeyPress", 0x59, true);
    }
});

mp.keys.bind(0x42, true, _ => {
    if(menuToggled !== undefined && menuToggled) return;
    if(Date.now() - lastChatToggle >= 500 && chatStatus == false)
    {
        traceLastFunc(`[Players] Attempt B key/silent siren`);

        if (localPlayer.vehicle && localPlayer.vehicle.getPedInSeat(-1) === localPlayer.handle && localPlayer.vehicle.getClass() === 18) {
            mp.events.callRemote('SilentSirenSync', localPlayer.vehicle)
        }
    }
});

function createCam(x, y, z, rx, ry, rz, viewangle, moveable = false) {
    traceLastFunc(`[Players] Create Cam`);

    camera = mp.cameras.new("Cam", { x, y, z }, { x: rx, y: ry, z: rz }, viewangle);
    camera_moveable = moveable;

    camera.setActive(true);
    mp.game.cam.renderScriptCams(true, true, 20000000000000000000000000, false, false);
}

mp.events.add("SetVehicleLights", (vehicle, state) => vehicle.setLights(state));

mp.events.add("ReportLastSeen", (name, posx, posy, posz) => {
    let getStreet = mp.game.pathfind.getStreetNameAtCoord(posx, posy, posz, 0, 0);

    zoneName = mp.game.ui.getLabelText(mp.game.zone.getNameOfZone(posx, posy, posz));
    streetName = mp.game.ui.getStreetNameFromHashKey(getStreet.streetName);

    mp.gui.chat.push(`!{#FFEC8B}** [RADIO | FREQ: 911] DISPATCH: Felon ${name} last spotted at ${streetName}, ${zoneName}.`);
});

mp.events.add("AFKDriverCheck", (x,y,z) => {
    if(mp.players.local == null || mp.players.local.handle == null) return;

    try{
        let player = mp.players.local;
        setTimeout(() => {
            const oldPosition = new mp.Vector3(x,y,z);
            const currentPosition = player.position;

            const dist = new mp.Vector3(x-currentPosition.x, y-currentPosition.y, z-currentPosition.z).length();
            if(dist < 5.0){
                mp.events.callRemote("ReplaceAFKDriver");
            }
        }, 25000);
    }
    finally{ // meh it should do the trick
        return;
    }
});

mp.events.add("render", () => {

    // Anti Pistol Whip?
    if (mp.game.invoke('0x475768A975D5AD17', player.handle, 6))
    {
        //Disable pistol whipping
        mp.game.controls.disableControlAction(0, 140, true); 
        mp.game.controls.disableControlAction(0, 141, true); 
        mp.game.controls.disableControlAction(0, 142, true); 
    }

    if (disableSprint) mp.game.controls.disableControlAction(0, 21, true); // Used for cuffing sys

    mp.game.invoke('0xB1906895227793F3', 3); // SET_PLAYER_TARGETTING_MODE => 3 (Free Aim)

    // Taken from GTA.World
    // IsPedArmed (0x475768A975D5AD17): 4 returns true if you are equipped with any weapon except Explosives weapon group AND melee weapons
    // If the player is armed with anything but a melee weapon or explosive weapon disable the lock-on functionality, otherwise enable it
    mp.game.invoke('0x475768A975D5AD17', player.handle, 4) ? mp.game.invoke('0x5C8B2F450EE4328E', player.handle, false) :  mp.game.invoke('0x5C8B2F450EE4328E', player.handle, true);

    // FOR VEH TUNER
    if(camera != null && camera.isActive() && camera.isRendering() && camera_moveable == true && !chatStatus && !mp.gui.cursor.visible)
    {
        mp.game.controls.disableAllControlActions(2); // DISABLE ALL CONTROLS
        mp.game.controls.enableControlAction(2, 172, true); // MENU KEYS START (FOR CONTROLLING, SELECTING ETC)
        mp.game.controls.enableControlAction(2, 173, true);
        mp.game.controls.enableControlAction(2, 174, true);
        mp.game.controls.enableControlAction(2, 175, true);
        mp.game.controls.enableControlAction(2, 176, true);
        mp.game.controls.enableControlAction(2, 177, true);
        mp.game.controls.enableControlAction(2, 178, true);
        mp.game.controls.enableControlAction(2, 187, true);
        mp.game.controls.enableControlAction(2, 188, true);
        mp.game.controls.enableControlAction(2, 189, true);
        mp.game.controls.enableControlAction(2, 190, true);
        mp.game.controls.enableControlAction(2, 191, true);
        mp.game.controls.enableControlAction(2, 194, true);
        mp.game.controls.enableControlAction(2, 201, true);
        mp.game.controls.enableControlAction(2, 202, true);
        mp.game.controls.enableControlAction(2, 215, true); // MENU KEYS END
        mp.game.controls.enableControlAction(2, 86, true); // HORN
        mp.game.controls.enableControlAction(2, 74, true); // HEADLIGHTS

        let x = (mp.game.controls.getDisabledControlNormal(7, 1) * 3.5);
        let y = (mp.game.controls.getDisabledControlNormal(7, 2) * 3.5);
        let zoomIn = (mp.game.controls.getDisabledControlNormal(2, 40) * 3.0);
        let zoomOut = (mp.game.controls.getDisabledControlNormal(2, 41) * 3.0);

        let LMB = mp.game.controls.getDisabledControlNormal(2, 24);
        let RMB = mp.game.controls.getDisabledControlNormal(2, 25);
        
        if(LMB != 0.0 && Date.now() - camera_lastmove >= 500)
        {
            mp.events.callRemote("TunerSwitch", 0);
            camera_lastmove = Date.now();
        }
        if(RMB != 0.0 && Date.now() - camera_lastmove >= 500)
        {
            mp.events.callRemote("TunerSwitch", 1);
            camera_lastmove = Date.now();
        }

        let currentRot = camera.getRot(2);
        currentRot = new mp.Vector3(currentRot.x - y, 0.0, currentRot.z - x);

        camera.setRot(currentRot.x, currentRot.y, currentRot.z, 2);

        if (zoomIn > 0)
        {
            let currentFov = camera.getFov();
            currentFov -= zoomIn;
            camera.setFov(currentFov);
        } 
        else if (zoomOut > 0)
        {
            let currentFov = camera.getFov();
            currentFov += zoomOut;
            camera.setFov(currentFov);
        }
    }

    // Spectate Things
    
    if (specState == true && specCam !== null && specTarget != null) 
    {
        // Automatically stop spec if target goes woosh
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

        // Move spectate camera
        if(specCam.isActive() && specCam.isRendering() && mp.players.exists(specTarget) && !chatStatus)
        {
            
            mp.game.controls.disableAllControlActions(2);

            let x = (mp.game.controls.getDisabledControlNormal(7, 1) * mouseSensitivity);
            let y = (mp.game.controls.getDisabledControlNormal(7, 2) * mouseSensitivity);
            let zoomIn = (mp.game.controls.getDisabledControlNormal(2, 40) * zoomSpeed);
            let zoomOut = (mp.game.controls.getDisabledControlNormal(2, 41) * zoomSpeed);

            //let currentRot = specCam.getRot(2);
            let currentRot = new mp.Vector3(0, 0, specTarget.getHeading())
            moveRotX += y;
            moveRotZ += x;
            currentRot = new mp.Vector3(currentRot.x - moveRotX, moveRotY, currentRot.z - moveRotZ);

            specCam.setRot(currentRot.x, currentRot.y, currentRot.z, 2);

            if (zoomIn > 0)
            {
                let currentFov = specCam.getFov();
                currentFov -= zoomIn;
                if (currentFov < minZoom)
                    currentFov = minZoom;
                specCam.setFov(currentFov);
            } else if (zoomOut > 0)
            {
                let currentFov = specCam.getFov();
                currentFov += zoomOut;
                if (currentFov > maxZoom)
                    currentFov = maxZoom;
                specCam.setFov(currentFov);
            }
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
    mp.storage.data.menu.updateRate = rate;
    mp.storage.flush();

    mp.events.callLocal("HUDNotify", `Updates now happen <strong>${Math.round(1000 / updateRate)}</strong> times a second.`, "blue", "fas fa-check", false, 
        1000, "bottomCenter", "replace");
});

mp.events.addCommand("bigmap", function (state) {
    state = Number(state);

    if (Number.isInteger(state) && (state == 0 || state == 1)) {
        mp.storage.data.menu.alwaysExpandedMap = Boolean(state);
    }
    else {
        if(mp.storage.data.menu.alwaysExpandedMap == true)
        {
            mp.storage.data.menu.alwaysExpandedMap = false;
        }
        else 
        {
            mp.storage.data.menu.alwaysExpandedMap = true;
        }
    }

    if(mp.storage.data.menu.alwaysExpandedMap == true)
    {
        mp.events.callLocal("HUDNotify", `Minimap will now always be <strong>expanded</strong>.`, "blue", "fas fa-check", false, 
        1000, "bottomCenter", "replace");
    }
    else 
    {
        mp.game.ui.setRadarBigmapEnabled(false, false);

        mp.events.callLocal("HUDNotify", `Minimap reverted to <strong>normal</strong>.`, "blue", "fas fa-check", false, 
        1000, "bottomCenter", "replace");
    }
    mp.storage.flush();
});

if(!radioToggle) mp.game.audio.setInitialPlayerStation('OFF');

mp.events.addDataHandler('Team', function (entity, value, oldValue) 
{
    entity.Team = value;
    
    if(entity.type === 'player' && entity.remoteId == mp.players.local.remoteId)
    {
        let r = 255;
        let g = 255;
        let b = 255;

        if(value == 1)
        {
            r = 0;
            g = 151;
            b = 255;
        }
        else if(value == 0)
        {
            r = 255;
            g = 174;
            b = 0;
        }
        else
        {
            r = 110;
            g = 200;
            b = 200;
        }
        mp.game.invoke('0xF314CF4F0211894E', 143, r, g, b, 255); // Replace Michael colour
        mp.game.invoke('0xF314CF4F0211894E', 116, r, g, b, 255); // Replace freemode colour
    }
});

mp.events.addDataHandler('pLogged', function (entity, value, oldValue) 
{
    entity.LoggedIn = value;
});

mp.events.addDataHandler('InLobby', function (entity, value, oldValue) 
{
    entity.Lobby = value;
});


mp.keys.bind(0x4A, false, () => {

    if (mp.players.local.isInAnyVehicle(false) && chatStatus == false && Date.now() - lastChatToggle >= 500 && 
        !menuToggled && !scoreboardToggled)
    {
        if(mp.players.local.vehicle.getClass() != 8 && mp.players.local.vehicle.model != mp.game.joaat('policeb')) 
        { // NO BIKE FUCKERS ALLOWED CUZ YALL GOT "ANTI FALL OFF BIKE" SMH
            mp.events.callRemote("OnScriptedKeyPress", 0x4A, true); // For toggling da sseeaattbbelltt
        }
    }
});

mp.events.add('render', () => {

    if(mp.players.local.LoggedIn !== undefined && mp.players.local.LoggedIn == true)
    {
        mp.game.player.restoreStamina(100);

        if(AFOB_Enabled === true && !mp.players.local.isDead() && !mp.players.local.isSwimming())
        {
            if(AFOB_LastF > Date.now() - 4000 && !mp.players.local.isInAnyVehicle(false))
            {
                AFOB_LastVeh = null;
            }

            if(AFOB_LastF < Date.now()-4000 && AFOB_LastSpeed <= AFOB_SpeedLimit)
            {
                if(mp.players.local.isInAnyVehicle(false) == false)
                {
                    if(AFOB_LastVeh != null && mp.vehicles.exists(AFOB_LastVeh) && AFOB_LastVeh.handle !== 0 &&
                        (AFOB_LastVeh.getClass() == 8 || AFOB_LastVeh.model == mp.game.joaat('policeb')))
                    {
                        mp.players.local.setIntoVehicle(AFOB_LastVeh.handle, AFOB_LastSeat);
                        if(Date.now() > AFOB_LastMessage)
                        {
                            AFOB_LastMessage = Date.now() + 60000;
                            mp.gui.chat.push(`!{#FF6347}[!] !{#FFFFFF}Anti Fall Off Bike has put you back in the bike. To exit, press/spam F.`);
                            //mp.gui.chat.push(`Setting into vehicle as last speed was ${AFOB_LastSpeed} under the limit ${AFOB_SpeedLimit}`);
                        }
                    }
                }
                else if(mp.players.local.vehicle != null && mp.players.local.vehicle.handle !== 0 && mp.players.local.isInAnyVehicle(false))
                {
                    if(AFOB_LastVeh == null || !mp.vehicles.exists(AFOB_LastVeh) || AFOB_LastVeh.handle === 0 || 
                        (AFOB_LastVeh.getClass() != 8 && AFOB_LastVeh.model != mp.game.joaat('policeb')))
                    {
                        AFOB_LastVeh = mp.players.local.vehicle;
                    }
                    
                    AFOB_LastSpeed = Number((mp.players.local.vehicle.getSpeed() * 3.6).toFixed(0));

                    // Till seat 8.
                    if(mp.players.local.vehicle.getPedInSeat(-1) == mp.players.local.handle) AFOB_LastSeat = -1;
                    if(mp.players.local.vehicle.getPedInSeat(0) == mp.players.local.handle) AFOB_LastSeat = 0;
                    if(mp.players.local.vehicle.getPedInSeat(1) == mp.players.local.handle) AFOB_LastSeat = 1;
                    if(mp.players.local.vehicle.getPedInSeat(2) == mp.players.local.handle) AFOB_LastSeat = 2;
                    if(mp.players.local.vehicle.getPedInSeat(3) == mp.players.local.handle) AFOB_LastSeat = 3;
                    if(mp.players.local.vehicle.getPedInSeat(4) == mp.players.local.handle) AFOB_LastSeat = 4;
                    if(mp.players.local.vehicle.getPedInSeat(5) == mp.players.local.handle) AFOB_LastSeat = 5;
                    if(mp.players.local.vehicle.getPedInSeat(6) == mp.players.local.handle) AFOB_LastSeat = 6;
                }
            }
        }

        if(Seatbelt_Enabled === true && !mp.players.local.isDead() && !mp.players.local.isSwimming())
        {
            if(mp.players.local.isInAnyVehicle(false) == false)
            {
                if(Seatbelt_LastVeh != null && mp.vehicles.exists(Seatbelt_LastVeh) && Seatbelt_LastVeh.handle !== 0)
                {
                    mp.players.local.setIntoVehicle(Seatbelt_LastVeh.handle, Seatbelt_LastSeat);
                    if(Date.now() > Seatbelt_LastMessage)
                    {
                        Seatbelt_LastMessage = Date.now() + 5000;
                        mp.gui.chat.push(`!{#FF6347}[!] !{#FFFFFF}Your seatbelt has prevented you from exiting the vehicle.`);
                        //mp.gui.chat.push(`Setting into vehicle as last speed was ${AFOB_LastSpeed} under the limit ${AFOB_SpeedLimit}`);
                    }
                }
            }
            else if(mp.players.local.vehicle != null && mp.players.local.vehicle.handle !== 0 && mp.players.local.isInAnyVehicle(false))
            {
                if(Seatbelt_LastVeh == null || !mp.vehicles.exists(Seatbelt_LastVeh) || Seatbelt_LastVeh.handle === 0)
                {
                    Seatbelt_LastVeh = mp.players.local.vehicle;
                }
                // Till seat 8.
                if(mp.players.local.vehicle.getPedInSeat(-1) == mp.players.local.handle) Seatbelt_LastSeat = -1;
                if(mp.players.local.vehicle.getPedInSeat(0) == mp.players.local.handle) Seatbelt_LastSeat = 0;
                if(mp.players.local.vehicle.getPedInSeat(1) == mp.players.local.handle) Seatbelt_LastSeat = 1;
                if(mp.players.local.vehicle.getPedInSeat(2) == mp.players.local.handle) Seatbelt_LastSeat = 2;
                if(mp.players.local.vehicle.getPedInSeat(3) == mp.players.local.handle) Seatbelt_LastSeat = 3;
                if(mp.players.local.vehicle.getPedInSeat(4) == mp.players.local.handle) Seatbelt_LastSeat = 4;
                if(mp.players.local.vehicle.getPedInSeat(5) == mp.players.local.handle) Seatbelt_LastSeat = 5;
                if(mp.players.local.vehicle.getPedInSeat(6) == mp.players.local.handle) Seatbelt_LastSeat = 6;
            }
        }

        if(clickDisable) 
        { 
            mp.game.controls.disableControlAction(32, 257, true); 
            mp.game.player.disableFiring(true);
        }
        else { mp.game.controls.enableControlAction(32, 257, true); }

        if(mp.players.local.isClimbing())
        {
            if(climbingStart == 0)
            {
                climbingStart = Date.now();
                climbTrigger = false;
            }

            if(Date.now() - climbingStart >= 2500 && !climbTrigger)
            {
                climbTrigger = true;
                mp.events.callRemote("ClimbProtection", 0); // Long time climb start
            }
        }
        else if(!mp.players.local.isClimbing() && climbingStart != 0)
        {
            //mp.gui.chat.push(`You were climbing for ${(Date.now() - climbingStart) / 1000}s`);
            
            if(climbTrigger) mp.events.callRemote("ClimbProtection", 1); // Long time climb stop

            climbingStart = 0;
            climbTrigger = false;
        }

        traceLastFunc(`[Players] render::Parachute_Dive_Check();`);

        if(player.doesExist()) {
            if(Date.now() - updateChecker >= updateRate)
            {
                // output any debug/stuff per update rate so it doesnt spam for debugging purposes
            }

            if(mp.game.invoke('0x7DCE8BDA0F1C1200', player.handle)) // Is Ped Parachute Free Falling?
            {
                traceLastFunc(`[Players] Parachute_Open();`);
                mp.game.invoke('0x16E42E800B472221', player.handle); // Force Open Parachute (( 1.1 Bug where you can't open ))
            }

            if(player.isInAnyVehicle(false))
            {
                if (player.seat !== undefined)
                {
                    let disableDriveby = false;
                    if(player.seat == -1 && player.weapon == 911657153)
                    { // DISABLE STUN GUN AIMING
                        disableDriveby = true;
                    }

                    if(player.Team == 0 /*&& player.seat != -1 */&& player.getVariable("HasShot") == false)
                    {
                        let driver = null;
                        mp.players.forEach(p => {
                            if(p.isInAnyVehicle(false) && p.vehicle.remoteId == player.vehicle.remoteId && player.vehicle.getPedInSeat(-1) == p.handle && 
                            player.vehicle.getPedInSeat(-1).handle != 0 && p.getVariable("HasShot") == false) 
                            // DRIVER HIMSELF HASN'T SHOT, THESE FACKEN HYPOCRITES SMH
                            {
                                driver = p;
                                return; // return in forEach exits the forEach loop btw.
                            }
                        });

                        if(driver != null)
                        {
                            if(driver.getVariable("WantsDriveby") == false) 
                            {
                                disableDriveby = true;

                                if(!dbDisabledMsg)
                                {
                                    if(dbDisabledMsgLast < Date.now())
                                    {
                                        mp.gui.chat.push(`!{#FF6347}[!] !{#FFFFFF}The driver does not want to engage in a driveby. Driver has to use /db to enable it again.`);
                                        dbDisabledMsgLast = Date.now() + 10000;
                                    }
                                    dbDisabledMsg = true;
                                }
                            }
                        }
                    }

                    if(disableDriveby)
                    {
                        mp.game.controls.disableControlAction(32, 25, true); 
                        mp.game.controls.disableControlAction(32, 68, true); 
                        mp.game.controls.disableControlAction(32, 70, true); 
                        mp.game.controls.disableControlAction(32, 91, true); 
                        mp.game.controls.disableControlAction(32, 92, true); 
                        mp.game.controls.disableControlAction(32, 69, true); 
                        mp.game.controls.disableControlAction(32, 24, true); 
                    }
                    // /*&& player.weapon != 2725352035 && player.Team == 0*/
                } //-- ALLOWED FUGITIVE DRIVER AIMING AGAIN!! 
            }
        }
    }

    if(Date.now() - updateChecker >= updateRate)
    {
        mp.events.callLocal("UpdateRateEvent");
        updateChecker = Date.now();

        secondCounter++;

        if(secondCounter >= Math.round(1000 / updateRate))
        {
            secondCounter = 0;

            if(mp.players.local.LoggedIn !== undefined && mp.players.local.LoggedIn == true)
            {
                if(player.isInAnyVehicle(false))
                {
                    if(!radioToggle && player.seat == -1) mp.game.invoke('0x1B9C0099CB942AC6', player.vehicle.handle, 'OFF');
                }
                if(!radioToggle && player.seat == -1) mp.game.audio.setRadioToStationName('OFF');

                mp.events.callLocal("OneSecondEvent");

                if(player.isInAnyVehicle(false))
                {
                    if (player.seat !== undefined && player.seat == -1)
                    {
                        if(!mp.game.vehicle.isThisModelAHeli(player.vehicle.model) && !mp.game.vehicle.isThisModelAPlane(player.vehicle.model))
                        {
                            //if(player.vehicle.getHealth() == 1000) player.vehicle.setHealth(800);
                            player.vehicle.setEngineHealth(player.vehicle.getHealth()); // Set eng health according to body health.
                        }
                    }
                }

                traceLastFunc(`[Players] Update Counters/JailTime`);

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

                traceLastFunc(`[Players] Update Stream Data`);
                mp.events.callRemoteUnreliable("UpdateStreamData");

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
                                if(target.Team == mp.players.local.Team) target.voiceVolume = 0.5;
                                else target.voiceVolume = 0.7;
                            }
                            else target.voiceVolume = 0.0;
                        }
                        else target.voiceVolume = 1 - (dist / MaxVoiceRange);
                    }
                });

                if (specState == true && specCam !== null && specTarget !== null) {

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
                        position.z -= 20;

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

                if(mp.players.local.Lobby == -4)
                {
                    derbyText.text = "N/A";

                    if(showHud !== undefined && showHud == false)
                    {
                        derbyText.visible = false;
                    }
                    else {
                        derbyText.visible = true;
                    }

                    if(UIHud != null)
                    {
                        UIHud.execute(`gm.gameProgress = ${mp.storage.data.menu.gameProgress};`);
                        UIHud.execute(`gm.$refs.TopRight.$refs.GameProgress.title = "Derby Time";`);
                        UIHud.execute(`gm.$refs.TopRight.$refs.GameProgress.time = ${derbyTimeCur};`);
                    }

                    if(derbyCounting)
                    {
                        mp.events.callRemoteUnreliable("DerbyIncreaseCount");
                        derbyTimeCur--;
                    }

                    var TimeNow = 0;

                    mp.players.forEach(target => {

                        if(target.LoggedIn !== undefined && target.Lobby == -4)
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
                }

                traceLastFunc(`[Players] Update Chase Data1`);

                if(UIHud != null && chaseRunning == true)
                {
                    chaseTimeCur += 1;

                    if(chaseTimeCur <= 0)
                    {
                        mp.players.local.freezePosition(true);

                        if(mp.players.local.isInAnyVehicle(false))
                        {
                            mp.players.local.vehicle.freezePosition(true);
                        }
                    }

                    if(chaseTimeCur >= chaseTimeMax || chaseTimeCur <= -1)
                    {
                        chaseRunning = false;
                        UIHud.execute(`gm.gameProgress = false;`);
                    }
                    else {
                        UIHud.execute(`gm.gameProgress = ${mp.storage.data.menu.gameProgress};`);
                        if(collisionTime > 0)
                        {
                            UIHud.execute(`gm.$refs.TopRight.$refs.GameProgress.title = "Collisions";`);
                            UIHud.execute(`gm.$refs.TopRight.$refs.GameProgress.time = ${collisionTime}`);
                        }
                        else 
                        {
                            UIHud.execute(`gm.$refs.TopRight.$refs.GameProgress.title = "Time Left";`);
                            UIHud.execute(`gm.$refs.TopRight.$refs.GameProgress.time = ${chaseTimeMax - chaseTimeCur}`);
                        }
                    }
                }

                traceLastFunc(`[Players] Update Chase Data2`);

                if(chaseRunning == true && UIHud != null)
                {
                    if(collisionTime > 0)
                    {
                        collisionTime--;
                    } 
                }

                traceLastFunc(`[Players] Update Chase Data3`);
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

            if(target.dimension == mp.players.local.dimension/* && target != mp.players.local*/ && !target.isDead())
            {
                if (target.getVariable("VisibleBlip") !== undefined && target.getVariable("VisibleBlip") == true)
                {
                    var blipColor = 45;
                    var blipId = 1;
                    if(target.Team == 0) 
                    {
                        blipColor = 70;
                        if(target.getVariable("HasShot") == true){
                            blipColor = 70;
                            blipId = 458;
                        }
                    }
                    else if(target.Team == 1) blipColor = 3;
                    if(target.Lobby == -3) blipColor = 15;
                    if(target.Lobby == -4) blipColor = 69;
                    if(target.getVariable("InDM") != -1) blipColor = 39;

                    if(target.isInAnyVehicle(false)){ 
                        blipId = 724; 
                        if(target.isInAnyHeli()){
                            blipId = 43;
                        }
                        if(target.isOnAnyBike()){
                            blipId = 226;
                        }
                    }

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

    if(Date.now() - targetChecker >= 25 && player.Team !== undefined && player.Team != -1)
    {
        targetChecker = Date.now();

        var target = mp.game.player.getEntityIsFreeAimingAt();
        clickDisable = false;

        if(mp.players.local.weaponAmmo > 0 && target !== undefined)
        {
            if(typeof target.getVariable === 'function')
            {
                if(target.Team !== undefined && target.Team != -1) 
                {
                    if(target.type === 'vehicle')
                    {
                        if(target.Team == player.Team)
                        {
                            clickDisable = true; // Disable shooting team-kill car by default
                            var vehicleOccupants = 0;
                            
                            mp.players.forEach(occupant => {
                                if(occupant.isInVehicle(target.handle, false)) {
                                    vehicleOccupants++;
                                    if(occupant.Team != player.Team) 
                                    {
                                        clickDisable = false; // Don't break shot if there's a fugi in or opposite team member!
                                    }
                                }
                            });

                            if(vehicleOccupants == 0) clickDisable = false;
                        }
                        else if(target.Team == 0 && player.Team == 1)
                        {
                            clickDisable = true; // Disable innocent car shots by default.
                            var vehicleOccupants = 0;

                            mp.players.forEach(occupant => {
                                if(occupant.isInVehicle(target.handle, false)) {
                                    vehicleOccupants++;
                                    if(occupant.Team == 0 && occupant.getVariable("HasShot") == true && !occupant.isDead()) {
                                        clickDisable = false; // Don't break shot if there's a fugitive that shot in!
                                    }
                                }
                            });

                            if(vehicleOccupants == 0) clickDisable = false;
                        }
                    }
                    else if(target.type === 'player')
                    {
                        if(target.Team != -1 && player.Team != -1)
                        {
                            if(player.Team == target.Team)
                            {
                                clickDisable = true;

                                if(target.isInAnyVehicle(true) && clickDisable == true && target.vehicle) {
                                    if(target.vehicle.Team !== undefined && target.vehicle.Team != target.Team) {
                                        clickDisable = false;
                                    }
                                    else {
                                        mp.players.forEach(occupant => {
                                            if(occupant.isInVehicle(target.vehicle.handle, true) && occupant.Team != target.Team) { 
                                                clickDisable = false; // Member of opposite team in it? Shoot.
                                            }
                                        });
                                    }
                                }
                            }

                            if(player.Team == 1 && target.Team == 0 && 
                                target.getVariable("HasShot") == false) // Disable click for fugi with no tag
                            {
                                clickDisable = true;

                                if(target.isInAnyVehicle(true) && clickDisable == true && target.vehicle)
                                {
                                    if(target.vehicle.Team !== undefined && target.vehicle.Team != target.Team) {
                                        clickDisable = false;
                                    }
                                    else {
                                        mp.players.forEach(occupant => {
                                            if(occupant.isInVehicle(target.vehicle.handle, true) && occupant.Team == target.Team && 
                                            occupant.getVariable("HasShot") == true && !occupant.isDead()) {
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
    ShotStats.TotalFired++;

    if(targetEntity !== null && targetEntity !== undefined)
    {
        if(targetEntity.type === 'vehicle') ShotStats.HitVehicle++;
        else if(targetEntity.type === 'player') ShotStats.HitPlayer++;
        else ShotStats.Missed++;

        if(targetEntity.type === 'vehicle' && player.Team != -1)
        {
            if(targetEntity.Team !== undefined)
            {
                var vehicleOccupants = 0;
                let breakShot = false;

                if(targetEntity.Team == player.Team)
                {
                    breakShot = true; // Break team-kill car by default
                    
                    mp.players.forEach(occupant => {
                        if(occupant.isInVehicle(targetEntity.handle, false)) {
                            vehicleOccupants++;
                            if(occupant.Team != player.Team) 
                            {
                                breakShot = false; // Don't break shot if there's a fugi in or opposite team member!
                            }
                        }
                    });

                    if(vehicleOccupants == 0) breakShot = false;

                    if(breakShot) mp.game.graphics.notify("~r~Do not teamkill!");
                }
                else if(targetEntity.Team == 0 && player.Team == 1 && player.weapon != 911657153)
                {
                    breakShot = true; // Break innocent car shots by default.

                    mp.players.forEach(occupant => {
                        if(occupant.isInVehicle(targetEntity.handle, false)) {
                            vehicleOccupants++;
                            if(occupant.Team == 0 && occupant.getVariable("HasShot") == true && !occupant.isDead()) {
                                breakShot = false; // Don't break shot if there's a fugitive that shot in!
                            }
                        }
                    });

                    if(vehicleOccupants == 0) breakShot = false;

                    if(breakShot) mp.game.graphics.notify("~r~Do not deathmatch!");
                }
                
                if(targetEntity.Team == 0 && player.Team == 1 && player.weapon == 911657153)
                {
                    breakShot = true;
                    mp.console.logInfo("Taser does not work for vehicles.");
                }


                if(breakShot == true) return true;
            }
        }
        if(targetEntity.type === 'player')
        {
            if(targetEntity.Team !== undefined && targetEntity.getVariable("HasShot") !== undefined)
            {
                if(targetEntity.getVariable("ClimbProtection") == true || player.getVariable("ClimbProtection") == true) return true;
                
                if(targetEntity.isInAnyVehicle(false) == false)
                {
                    if(targetEntity.Team != -1)
                    {
                        if(player.Team == targetEntity.Team)
                        {
                            mp.events.callRemote("PlayerShotAtWrongPlayer", 0, targetEntity);
                            return true;
                        }
                        if(player.Team == 1 && targetEntity.Team == 0 && targetEntity.getVariable("HasShot") == false && 
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

                if(player.weapon == 911657153 && 
                    (BigInt(Math.round((new Date()).getTime() / 1000)) - BigInt(targetEntity.getVariable("LastVehEnterExit"))) <= BigInt(7) 
                    && targetEntity.Team == 0)
                {
                    mp.game.graphics.notify("~r~Cannot tase! Target exiting vehicle!");
                    return true;
                }

                if(player.weapon == 911657153 && (targetEntity.getVariable("HasShot") == true || chaseTimeCur < 60) && targetEntity.Team == 0)
                {
                    return true;
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

    if(mp.players.local.getVariable("HasShot") == false && player.Team == 0)
    {
        mp.events.callRemote("GivePlayerShotTag");
    }
});


mp._events.add('incomingDamage', (sourceEntity, sourcePlayer, targetEntity, weapon, boneIndex, damage) => {

    if(targetEntity !== null && targetEntity !== undefined && targetEntity.type === 'player' && targetEntity == player)
    {
        if(sourceEntity !== null && sourceEntity !== undefined && sourceEntity.type === 'player')
        {
            if(weapon == 911657153 && targetEntity.isInAnyVehicle(false)) return true; // Disable stun if in car.

            if(targetEntity.getVariable("ClimbProtection") == true || sourceEntity.getVariable("ClimbProtection") == true) return true;

            if(weapon == 911657153 && (BigInt(Math.round((new Date()).getTime() / 1000)) - BigInt(targetEntity.getVariable("LastVehEnterExit"))) <= BigInt(7)) return true;

            if(weapon == 911657153 && targetEntity.getVariable("HasShot") == true && targetEntity.Team == 0) return true;

            if(sourceEntity.Team != -1 && player.Team != -1)
            {
                if(player.Team == sourceEntity.Team)
                {
                    return true;
                }

                if(sourceEntity.Team == 1 && player.Team == 0 && 
                    player.getVariable("HasShot") == false && weapon != 911657153) // Took damage from a cop and hasn't shot.
                {
                    return true;
                }
            }
        }
    }
})

function isMeleeWeapon(weapon){
    switch(weapon){
        case 2725352035:
        case 2578778090:
        case 1737195953:
        case 1317494643:
        case 2508868239:
        case 1141786504:
        case 2227010557:
        case 4192643659:
        case 2460120199:
        case 3638508604:
        case 4191993645:
        case 3713923289:
        case 3756226112:
        case 3441901897:
        case 2484171525:
        case 419712736:
            return true;
        default: return false;
    }
}

mp._events.add("outgoingDamage", (sourceEntity, targetEntity, targetPlayer, weapon, boneIndex, damage) => {
    if(sourceEntity !== undefined && sourceEntity !== null && sourceEntity.type === 'player' && sourceEntity == player && player.Team != -1)
    {
        if(targetEntity !== undefined && targetEntity !== null && targetEntity.type === 'player')
        {
            if(sourceEntity.getVariable("ClimbProtection") == true || targetEntity.getVariable("ClimbProtection") == true) return true;

            if(targetEntity.Team != -1 && player.Team != -1)
            {
                if(sourceEntity.Team == targetEntity.Team)
                {
                    return true;
                }

                if(sourceEntity.Team == 1 && targetEntity.Team == 0) 
                {
                    if(targetEntity.getVariable("HasShot") == false) // Shot a fugitive that hasn't shot.
                    {
                        if(weapon != 911657153) return true; // cant damage dem at alll if thy havent shot unless taser

                        if(chaseTimeCur < 60) return true; // nah not at da start

                        if((BigInt(Math.round((new Date()).getTime() / 1000)) - BigInt(targetEntity.getVariable("LastVehEnterExit"))) <= BigInt(7) 
                            && targetEntity.Team == 0)
                        {
                            mp.game.graphics.notify("~r~Cannot tase! Target exiting vehicle!");
                            return true;
                        }

                        if(targetEntity.Team == 0 && targetEntity.getVariable("HasShot") == true) {
                            return true;
                        }

                        if((targetEntity.isInAnyVehicle(false) == true || 
                            calcDist(targetEntity.position, player.position) > 7.5)) {

                            mp.game.graphics.notify("~r~Taser out of reach, get closer!");
                            return true; // Stun Gun no shot if more than 10 meters or target in car.
                        }
                    }
                    else{
                        if(weapon == 911657153){ // Attempt to tase a fugitive with shot tag.
                            mp.game.graphics.notify("~r~Target has shot, cannot tase them!");
                            return true;
                        }
                    }
                }
                if(sourceEntity.Team == 0 && targetEntity.Team == 1 && sourceEntity.getVariable("HasShot") == false
                    && isMeleeWeapon(weapon))
                {
                    mp.events.callRemote("GivePlayerShotTag");
                }
            }
        }
        else if(targetEntity.type === 'vehicle' && player.Team != -1)
        {
            if(targetEntity.Team !== undefined && player.Team != -1)
            {
                let breakShot = false;
                var vehicleOccupants = 0;

                if(weapon == 911657153)
                {
                    breakShot = true;
                    mp.console.logInfo("Taser does not work for vehicles. (2)");
                }

                if(targetEntity.Team == player.Team)
                {
                    breakShot = true; // Break team-kill car by default
                    
                    mp.players.forEach(occupant => {
                        if(occupant.isInVehicle(targetEntity.handle, false)) {
                            vehicleOccupants++;
                            if(occupant.Team != player.Team) {
                                breakShot = false; // Don't break shot if there's a fugi in or opposite team member!
                            }
                        }
                    });

                    if(vehicleOccupants == 0) breakShot = false;

                    if(breakShot) mp.game.graphics.notify("~r~Do not teamkill!");
                }
                else if(targetEntity.Team == 0 && player.Team == 1 && weapon != 911657153)
                {
                    breakShot = true; // Break innocent car shots by default.

                    mp.players.forEach(occupant => {
                        if(occupant.isInVehicle(targetEntity.handle, false)) {
                            vehicleOccupants++;
                            if(occupant.Team == 0 && occupant.getVariable("HasShot") == true && !occupant.isDead()) {
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

mp.events.add("entplayanim", (entity, animDictionary, animName, animFlag, speed = 8.0, speedMul = 0.0, duration = -1, playbackRate = 0.0, lockX = false, lockY = false, lockZ = false) => {
    if(entity.remoteId == mp.players.local.remoteId)
    {
        mp.events.callRemote("UpdateLastAnim", animDictionary, animName);
    }
	mp.game.streaming.requestAnimDict(animDictionary);
	new Promise(() => {
	const timer = setInterval(() => {
		if(mp.game.streaming.hasAnimDictLoaded(animDictionary)) {
			clearInterval(timer);
			resolvePlayAnim(entity, animDictionary, animName, animFlag, speed, speedMul, duration, playbackRate, lockX, lockY, lockZ);
		}
	}, 100);
});
});

function resolvePlayAnim(entity, animDictionary, animName, animFlag, speed = 8.0, speedMul = 0.0, duration = -1, playbackRate = 0.0, lockX = false, lockY = false, lockZ = false)
{
	entity.taskPlayAnim(animDictionary, animName, speed, speedMul, duration, animFlag, playbackRate, lockX, lockY, lockZ);
}

mp.events.add("entclearanim", (entity, animDictionary, animName) => {
    
    if(animDictionary != "null" || animDictionary != null)
    {
        entity.stopAnimTask(animDictionary, animName, 1.0);
    }
    if(!entity.isFalling()) entity.clearTasks();
});

mp.events.add('native:call', (nativeName, args, hasCallback) => {
    let commandString = "";
    if(args.length > 0)
    {
        commandString = nativeName + "(";
        for(var i = 0; i < args.length; i++)
        {
            if(typeof args[i] === 'string')
            {
                commandString += '"' + args[i] + '"';
            }
            else commandString += args[i];
 
            if(i != args.length - 1)
            {
                commandString += ", ";
            }
        }
        commandString += ");";
    }
    else commandString = nativeName + "();";
 
    var native = eval(commandString);
 
    if(hasCallback)
    {
        mp.events.callRemote("native:return", native);
    }
});

mp.events.add(
{
    "GetGroundZ": (px, py, pz, dimension, returnEvent) => {
        let getGroundZ = mp.game.gameplay.getGroundZFor3dCoord(px, py, pz, parseFloat(0), false);

        mp.events.callRemote(returnEvent, px, py, getGroundZ, dimension);
    },
    "ChangeVehicleHeading": (heading) => {
        if(mp.players.local.isInAnyVehicle(false)) mp.players.local.vehicle.setHeading(heading);
    },

    "ToggleAFOB": (status, speedLimit) => {
        AFOB_LastF = Date.now();
        AFOB_LastSeat = -1;
        AFOB_LastVeh = null;
        AFOB_Enabled = status;
        AFOB_LastSpeed = 0;

        if(status) AFOB_SpeedLimit = speedLimit;
    },

    "ToggleSeatbelt": (status) => {
        Seatbelt_Enabled = status;
        Seatbelt_LastMessage = Date.now() - 100000;
        Seatbelt_LastVeh = null;
        Seatbelt_LastSeat = -1;
    },

    "SyncServerSettings": (onfootcollisions, cameraUpdateTime) => {
        ServerSettings.OnfootCollisions = onfootcollisions;
        ServerSettings.CameraUpdateTime = cameraUpdateTime;
    },
    "ToggleClientRadio": () => {
        radioToggle = !radioToggle;

        mp.storage.data.radiotoggle = radioToggle;

        mp.storage.flush();

        if(!radioToggle) mp.gui.chat.push("!{#FF6347}[!] !{#FFFFFF}Vehicle radio toggled OFF.");
        else mp.gui.chat.push("!{#FF6347}[!] !{#FFFFFF}Vehicle radio toggled ON.");
    },
    "SetInVehEx": (vehicle, vehseat) => {
        mp.players.local.setIntoVehicle(vehicle.handle, vehseat);
    },
    "ToggleHud": (show) => {
        if(show)
        {
            if(mp.players.local.Lobby == -4 && derbyText != null) {
                derbyText.visible = true;
            }
        }
        else{

            if(derbyText != null) {
                derbyText.visible = false;
            }
        }
    },
    "ThrowFromVehicle": () => {
        let localVeh = mp.players.local.vehicle;
        if (localVeh) {
            mp.players.local.taskLeaveVehicle(localVeh.handle, 4160);
        }
    },
    "DisableControlActions": (actiongroup) => {
        mp.game.controls.disableAllControlActions(actiongroup);
    },

    "EnableControlActions": (actiongroup) => {
        mp.game.controls.enableAllControlActions(actiongroup);
    },

    "SetHandcuffs": (status) => {
        if(status == true)
        {
            mp.players.local.clearTasksImmediately();

            mp.players.local.setEnableHandcuffs(true);

            mp.events.callRemote("play_anim", "mp_prison_break", "handcuffed", 32, 8.0, 0.0, -1, 0.0, false, false, false);
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
            mp.events.callRemote("play_anim", "ped", "handsup_base", 50, 8.0, 0.0, -1, 0.0, false, false, false);
        }
        else
        {
            mp.events.callRemote("play_anim", "ped", "handsup_exit", 50, 8.0, 0.0, -1, 0.0, false, false, false);
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
        specCam.attachTo(target.handle, 0, target.isInAnyVehicle(false) ? -6 : -5, target.isInAnyVehicle(false) ? 3 : 1, true);
        specCam.setActive(true);

        mp.game.cam.renderScriptCams(true, false, 0, true, false);

        specState = true;
    },

    "ShakeCam": (shakeType, amplit, time) => {
        mp.game.cam.shakeGameplayCam(shakeType, amplit);
        setTimeout(() => {
            mp.game.cam.stopGameplayCamShaking(true);
        }, time);
    },

    "PlayMissionComplete": (type) => {
        mp.game.audio.playMissionCompleteAudio(type);
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
        if(UIHud != null) UIHud.execute(`gm.gameProgress = false;`);
        chaseRunning = false;
    },

    "playerEnterVehicle": (vehicle, seat) => {
        dbDisabledMsg = false;
        player.seat = seat;
        if(vehicle.Collisions == undefined || vehicle.Collisions == true) vehicle.setInvincible(false);

        if(!radioToggle && seat == -1) mp.game.invoke('0x1B9C0099CB942AC6', vehicle.handle, 'OFF');

        if(mp.game.vehicle.isThisModelAHeli(vehicle.model) || mp.game.vehicle.isThisModelAPlane(vehicle.model)) {
            mp.events.callRemote("RequestParachute", vehicle);
        }

        if (seat == -1) {
            if(vehicle.getVariable("Engine") !== undefined)
            {
                vehicle.setEngineOn(vehicle.getVariable("Engine"), true, true);
            }
            else vehicle.setEngineOn(true, true, true);
        }
    },

    "playerLeaveVehicle": (vehicle, seat) => {
        if (vehicle != null) {

            if(Seatbelt_Enabled == true)
            {
                mp.players.local.clearTasks();
                mp.players.local.setIntoVehicle(vehicle.handle, seat);
            }

            if(vehicle.getVariable("Engine") !== undefined)
                vehicle.setEngineOn(vehicle.getVariable("Engine"), true, true);
            else vehicle.setEngineOn(true, true, true);
        }
    },

    "InitChaseTimer": () => {
        
        if(UIHud != null) UIHud.execute(`gm.gameProgress = false;`);

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

    "CreatePlayerCamera": (x, y, z, rx, ry, rz, viewangle, moveable = false) => {
        createCam(x, y, z, rx, ry, rz, viewangle, moveable);
    },

    "SetDiscordStatus": (serverName, status) => {
        mp.discord.update(serverName, status);
    },

    "playerReady": (player) => {
        mp.events.callRemote("playerReadyCon");
        mp.game.gxt.set('PM_PAUSE_HDR', 'Race \'n Chase'); // Replace map title

        setInterval(() => {
            if(mp.players.local.LoggedIn !== undefined && mp.players.local.LoggedIn == true)
            {
                mp.events.callRemote("UpdateTabbedState", mp.system.isFocused);
                mp.events.callLocal("TwoSecondsEvent");
            }
        }, 2000);
    },
});