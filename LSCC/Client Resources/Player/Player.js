var global = require('./globals');
  const {TranslateMessage: Translate} = require('./Translator');

let camera = null;
let camera_moveable = false;
let camera_lastmove = Date.now();
let moveToCam = null;

const player = mp.players.local;
let localPlayer = mp.players.local;
const timerBarLib = require("./Player/Bar.js");

player.crawl = false;

player.canExitVehicle = true;
mp.events.add("ToggleCanExitVehicle", (toggle) => {
    player.canExitVehicle = toggle;
})

const BASSFXType = {
    BASS_FX_DX8_CHORUS: 0,
    BASS_FX_DX8_COMPRESSOR: 1,
    BASS_FX_DX8_DISTORTION: 2,
    BASS_FX_DX8_ECHO: 3,
    BASS_FX_DX8_FLANGER: 4,
    BASS_FX_DX8_GARGLE: 5,
    BASS_FX_DX8_I3DL2REVERB: 6,
    BASS_FX_DX8_PARAMEQ: 7,
    BASS_FX_DX8_REVERB: 8,
    BASS_FX_VOLUME: 9,

    // bass_fx.dll
    BASS_FX_BFX_PEAKEQ: 65540,
    BASS_FX_BFX_BQF: 65555
};

let ping = 69;

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
    global.updateRate = mp.storage.data.menu.updateRate;
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

let LowHealthCameraActive = false;

var targetChecker = Date.now();
var updateChecker = Date.now();
var clickDisable = false;

var wasClimbingLastFrame = false;
var checkClimbingDurationTimeout = undefined;
var climbProtectionState = -1; // -1 = none, 0 = active on ladder, 1 = short protection after ladder
var climbProtectionTimeout = undefined;
var shortClimbProtectionTimeout = undefined;
var lastClimbProtectionTimestamp = 0;

var climbProtectionEndTimestamp = 0; // when the climb protection should end in seconds

var taseProtectionTimer = null;
var wasInCar = false;

var themeSoundID = 1;

var checker_lastHealth = 0;
var healthLossTime = Date.now();
var healthLoss = 0;

const bones = [0, 10706, 11816, 14201, 18905, 23553, 24816, 24817, 24818, 28252, 31086, 36864, 39317, 40269, 45509, 51826, 52301, 57005, 57597, 58271, 61163, 63931, 64729];
const boneDistanceThreshold = 1;




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

global.traceLastFunc("[Players] CreateVars");

function ResetShotStats()
{
    ShotStats.TotalFired = 0;
    ShotStats.HitPlayer = 0;
    ShotStats.HitVehicle = 0;
    ShotStats.Missed = 0;

    ShotStats.UpdateInterval = 0;

    global.traceLastFunc("[Players] ResetShotStats");
}

ResetShotStats();

mp.events.add("SetChaseVehicle", (vid, seat) => {
    chaseStartVeh = vid;
    chaseStartSeat = seat;
});

mp.keys.bind(0x46, true, function () {
    if(!global.chatStatus && !global.menuToggled && !global.scoreboardToggled) 
    {
        AFOB_LastF = Date.now();
        AFOB_LastSeat = -1;
        AFOB_LastVeh = null;
        AFOB_LastSpeed = 0;
    }
});

mp.events.add('entityStreamIn', (entity) => {
    if(entity == null || !entity.doesExist()) return;

    if (global.chaseRunning && mp.players.local.getVariable("Team") >= 0 && entity.type === 'vehicle' 
        && chaseStartVeh != -1 && entity.remoteId !== undefined && entity.remoteId == chaseStartVeh)
    {
        if(global.chaseTimeCur <= 25) // Less than 25s passed to game start. If this doesn't make up, then get a better connection fag.
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
        mp.events.callLocal("AddTooltip", "H", Translate("ARREST"), 1);

    HandcuffTooltipShown = true;
});

mp.events.add('setPing', (pingtoset) => {
    ping = pingtoset;
})

let lastWeapon = 2725352035;
let holsterAnimTimeout = undefined;
let holster_aimDisabled = false;

let holsterTimeoutTime = 0;
let holsterAnimData = {};

mp.events.add("holsterAnimSync_clearAnim", (entity, dict, name, speed) => {
    entity.stopAnim(dict, name, speed);
});
mp.events.add("holsterAnimSync_playAnim", (entity, animDictionary, animName, animFlag, speed = 8.0, speedMul = 0.0, duration = -1, offset = 0.0, lockX = false, lockY = false, lockZ = false) => {
    entity.taskPlayAnim(animDictionary, animName, speed, speedMul, duration, animFlag, offset, lockX, lockY, lockZ);
});

mp.events.add('render', () => {

    if(!mp.players.local || !mp.players.local.weapon) return;

    if(holster_aimDisabled === true){
        mp.game.controls.disableControlAction(0, 25, true); // disable aim
    }

    if(mp.players.local.weapon != lastWeapon){
        lastWeapon = mp.players.local.weapon;
        holster_aimDisabled = true;
        clearTimeout(holsterAnimTimeout);
        holsterAnimTimeout = undefined;

        let gunData = global.weaponData.find(w => Number(w.Hash) == mp.players.local.weapon);
        const gunType = (gunData ? (gunData.Type || "Handguns") : "Handguns");
        if(gunType === "Handguns" || gunType === "Submachine Guns"){
            if(mp.players.local.getVariable("Team") == 0){
                if(lastWeapon !== 2725352035){
                    holsterAnimData = {dict:'reaction@intimidation@1h', name:'intro', offset:0.3};
                    holsterTimeoutTime = 550;
                }
                else{
                    holsterAnimData = {dict:'reaction@intimidation@1h', name:'outro', offset:0.3};
                    holsterTimeoutTime = 500;
                }
            }
            else{
                if(lastWeapon !== 2725352035){
                    holsterAnimData = {dict:'rcmjosh4', name:'josh_leadout_cop2', offset:0};
                    holsterTimeoutTime = 550;

                    if(mp.players.local.getVariable("Team") == 1)
                        mp.events.callRemote("HolsterAnimation_PistolCheck", true);
                }
                else{
                    holsterAnimData = {dict:'rcmjosh4', name:'josh_leadout_cop2', offset:0};
                    holsterTimeoutTime = 200;

                    if(mp.players.local.getVariable("Team") == 1)
                        mp.events.callRemote("HolsterAnimation_PistolCheck", false);
                }
            }
        }
        else{
            if(mp.players.local.getVariable("Team") == 0){
                holsterAnimData = {dict:'reaction@intimidation@1h', name:'outro', offset:0.3};
                holsterTimeoutTime = 500;
            }
            else{
                holsterAnimData = {dict:'rcmjosh4', name:'josh_leadout_cop2', offset:0};
                holsterTimeoutTime = 200;

                if(mp.players.local.getVariable("Team") == 1)
                    mp.events.callRemote("HolsterAnimation_PistolCheck", false);
            }
        }
        mp.players.local.taskPlayAnim(holsterAnimData.dict, holsterAnimData.name, 24.0, 24.0, -1, 48, holsterAnimData.offset, false, false, false)
        mp.events.callRemote("HolsterAnimation_PlaySynced", holsterAnimData.dict, holsterAnimData.name, 24.0, 24.0, -1, 48, holsterAnimData.offset, false, false, false)

        holsterAnimTimeout = setTimeout(() => {
            mp.players.local.stopAnim(holsterAnimData.dict, holsterAnimData.name, 3.0);
            mp.events.callRemote("HolsterAnimation_StopSynced", holsterAnimData.dict, holsterAnimData.name, 3.0);
            holster_aimDisabled = false;
            clearTimeout(holsterAnimTimeout);
            holsterAnimTimeout = null;
        }, holsterTimeoutTime);
        
    }
});


let recoil_LastShotTime = 0;
let recoil_ShotsInBurst = 0;
let recoil_goingUp = true;

var tasePreview = false;

mp.events.add("render", () => {

    if(recoil_ShotsInBurst > 0 && (Date.now() - recoil_LastShotTime > 300)){
        recoil_ShotsInBurst = 0;
        recoil_goingUp = true;
    }

    if(player.getVariable("Team") == 1){  
        if(player.weapon == 911657153){
            tasePreview = true;
            mp.players.forEachInRange(player.position, 70.0, (fugitive) => {
                const vec1 = player.position;
                const vec2 = fugitive.position;
                const distance = vec1.subtract(vec2).length();

                if((fugitive.getVariable("Team") == 0 && !fugitive.isInAnyVehicle(false) && fugitive.getVariable("TaseProtection") == false
                    && distance < 7.5) || fugitive.remoteId == player.remoteId){
                        fugitive.setAlpha(255);
                }
                else{
                    fugitive.setAlpha(180);
                }
            });
        }
    }

});

mp.events.add("DetachPlayer", (player) => {
    player.detach(true, true);
});

// 35 0.5 0.1 0.2 0 90 0 => Best for the fireman carry animation.
mp.events.add("AttachPlayerToPlayer", (playerToAttach, target) => {
    playerToAttach.attachTo(target.handle, 35, 0.5, 0.1, 0.2, 0, 90, 0, true, false, false, true, 0, true);
});

var PoliceHeliCamActive = false;
var LastPoliceHeliCamToggle = 0;
var PoliceHeliCamFrameCount = 0;
var PoliceHeliCam_Fugitives = []; // for client-side, updates every 2 seconds
var PoliceHeliCam_Fugitives_Server = []; // for the server-side updates, every 15 seconds

mp.keys.bind(0xA4, true, () => { // Left Alt
    if(Date.now() - LastPoliceHeliCamToggle < 550) return;
    if((mp.players.local.getVariable("Team") != 1 || mp.players.local.vehicle == undefined 
    || !mp.players.local.isInAnyVehicle(false) || mp.players.local.vehicle.getClass() != 15)){
        return;
    }

    ToggleHeliCam(!PoliceHeliCamActive);
});
mp.events.add("PoliceHeliCam_ClearFugitives", () => {
    PoliceHeliCam_Fugitives_Server = [];
})
mp.events.add("PoliceHeliCam_AddFugitivePosition", (x, y) => {
    PoliceHeliCam_Fugitives_Server.push({x:x, y:y});
});
function ToggleHeliCam(toggle){

    PoliceHeliCamActive = toggle;
    LastPoliceHeliCamToggle = Date.now();

    if(PoliceHeliCamActive){
        mp.game.graphics.stopScreenEffect('FocusOut');
        mp.game.graphics.startScreenEffect('FocusIn', 500, false);
        mp.game.graphics.startScreenEffect('DeathFailNeutralIn', 500, true);
        mp.game.audio.playSoundFrontend(-1, 'Hit_In', 'PLAYER_SWITCH_CUSTOM_SOUNDSET', true);

        PoliceHeliCam_Fugitives = [];
        mp.players.forEachInStreamRange((fugi) => {
            if(fugi.dimension === mp.players.local.dimension && fugi.getVariable("Team") == 0){
                PoliceHeliCam_Fugitives.push({x:fugi.position.x, y:fugi.position.y});
            }
        })

    }
    else{
        mp.game.graphics.stopScreenEffect('FocusIn');
        mp.game.graphics.stopScreenEffect('DeathFailNeutralIn');
        mp.game.graphics.startScreenEffect('FocusOut', 500, false);
        mp.game.audio.playSoundFrontend(-1, 'Hit_Out', 'PLAYER_SWITCH_CUSTOM_SOUNDSET', true);
    }
}

mp.events.add("TwoSecondsEvent", () => {
    if(!PoliceHeliCamActive) return;

    PoliceHeliCam_Fugitives = [];
    mp.players.forEachInStreamRange((fugi) => {
        if(fugi.dimension === mp.players.local.dimension && fugi.getVariable("Team") == 0){
            PoliceHeliCam_Fugitives.push({x:fugi.position.x, y:fugi.position.y});
        }
    })
})

function Render3DMapMatrix(){
    if(!PoliceHeliCamActive) return;
    if(PoliceHeliCamActive && (mp.players.local.getVariable("Team") != 1 || mp.players.local.vehicle == undefined 
        || !mp.players.local.isInAnyVehicle(false) || mp.players.local.vehicle.getClass() != 15)){
        ToggleHeliCam(false);
        return;
    }

    PoliceHeliCamFrameCount++;
    if(PoliceHeliCamFrameCount > 60) PoliceHeliCamFrameCount = 0;

    const Z_value = (player.position.z < 325 ? 175 : (player.position.z-150));

    for(let i = -4000 ; i <= 8000; i+=250){
        mp.game.graphics.drawLine(i, -4000,Z_value, i, 6000, Z_value, 255, 255, 255, 32);
    }
    for(let i = -4000 ; i <= 6000; i+=250){
        mp.game.graphics.drawLine(-4000, i,Z_value, 8000, i, Z_value, 255, 255, 255, 32);
    }

    const playerTooLow = (player.position.z < 200);
    let currentX = -4000, currentY = -4000;
    if(playerTooLow){
        mp.game.graphics.drawBox(currentX, currentY, Z_value, (currentX+250), (currentY+250), (Z_value+0.001), 255, 0, 0, 16);
        if(PoliceHeliCamFrameCount > 30){
            mp.game.graphics.drawText("Altitude too low, gain some height!", [0.5, 0.005], { 
                font: 4, 
                color: [255, 50, 50, 185], 
                scale: [0.7, 0.7], 
                outline: true,
                centre:true
            });
        }
        for(let i = 0; i < 40 ; i++){
            currentY = -4000;
            for(let j = 0 ; j < 48 ; j++){
                mp.game.graphics.drawBox(currentX, currentY, Z_value, (currentX+250), (currentY+250), (Z_value+0.001), 255, 0, 0, 16);
                currentY += 250;
            }
            currentX += 250;
        }
        return;
    }

    mp.game.graphics.drawText(`ALT: ${Math.round(player.position.z)}m / 200m`, [0.5, 0.005], { 
        font: 4, 
        color: [255, 255, 255, 185], 
        scale: [0.7, 0.7], 
        outline: true,
        centre:true
    });

    for(let i = 0; i < 40 ; i++){
        currentY = -4000;
        for(let j = 0 ; j < 48 ; j++){

            let highlight = 0;
            for(let fug = 0 ; fug < PoliceHeliCam_Fugitives_Server.length; fug++){
                if(PoliceHeliCam_Fugitives_Server[fug].x < (currentX+250) && PoliceHeliCam_Fugitives_Server[fug].x > currentX){
                    if(PoliceHeliCam_Fugitives_Server[fug].y < (currentY+250) && PoliceHeliCam_Fugitives_Server[fug].y > currentY){
                        highlight = 2;
                        break;
                    }
                }
            }
            for(let fug = 0 ; fug < PoliceHeliCam_Fugitives.length; fug++){
                if(PoliceHeliCam_Fugitives[fug].x < (currentX+250) && PoliceHeliCam_Fugitives[fug].x > currentX){
                    if(PoliceHeliCam_Fugitives[fug].y < (currentY+250) && PoliceHeliCam_Fugitives[fug].y > currentY){
                        highlight = 1;
                        break;
                    }
                }
            }

            if(highlight > 0){
                mp.game.graphics.drawBox(currentX, currentY, Z_value, (currentX+250), (currentY+250), (Z_value+0.001), (highlight == 2 ? 255 : 0), 255, 0, 16);

                // distance calc and if player is close display street
                const vec1 = mp.players.local.position;
                const vec2 = new mp.Vector3((currentX+125), (currentY+125), Z_value);

                const distance = vec1.subtract(vec2).length();
                if(distance < 1000){
                    let getStreet = mp.game.pathfind.getStreetNameAtCoord((currentX+125), (currentY+125), Z_value, 0, 0);
                    let zoneName = mp.game.ui.getLabelText(mp.game.zone.getNameOfZone((currentX+125), (currentY+125), Z_value));
                    let realStreet = mp.game.ui.getStreetNameFromHashKey(getStreet.streetName);
        
                    mp.game.graphics.drawText(`${global.zoneName}\n${realStreet}`, [(currentX+125), (currentY+125), (Z_value+0.5)], {
                        font:4,
                        color: [255, 255, 255, 128],
                        scale: [0.45, 0.45],
                        outline:true,
                        centre:true
                    });
                }
            }
            else{
                mp.game.graphics.drawBox(currentX, currentY, Z_value, (currentX+250), (currentY+250), (Z_value+0.001), 255, 255, 255, 0);
            }

            currentY += 250;
        }
        currentX += 250;
    }

    /*
        let getStreet = mp.game.pathfind.getStreetNameAtCoord(posx, posy, posz, 0, 0);

    global.zoneName = mp.game.ui.getLabelText(mp.game.zone.getNameOfZone(posx, posy, posz));
    global.streetName = mp.game.ui.getStreetNameFromHashKey(getStreet.streetName);

    mp.gui.chat.push(`!{#FFEC8B}** [RADIO | FREQ: 911] DISPATCH: Felon ${name} last spotted at ${global.streetName}, ${global.zoneName}.`);
    */
    

    
}
mp.events.add("render", () => {
    Render3DMapMatrix();
});

mp.events.add("OneSecondEvent", () => {

    ShotStats.UpdateInterval++;

    if(player.getVariable("Team") >= 0){
        if(player.getHealth() <= 50){
            if(!LowHealthCameraActive)
            {
                mp.game.cam.shakeGameplayCam('DRUNK_SHAKE', 0.21);
                LowHealthCameraActive = true;
            }
        }
        else{
            if(LowHealthCameraActive){
                mp.game.cam.shakeGameplayCam('DRUNK_SHAKE', 0);
                LowHealthCameraActive = false;
            }
        }
    }
    else{
        if(LowHealthCameraActive){
            mp.game.cam.shakeGameplayCam('DRUNK_SHAKE', 0);
            LowHealthCameraActive = false;
        }
    }

    // taser range: 7.5m
    if(player.getVariable("Team") == 1){  
        if(player.weapon != 911657153){
            if(tasePreview == true){
                tasePreview = false;
                mp.players.forEachInStreamRange((playa, id) => {
                    playa.setAlpha(255);
                });
            }
        }
    }
    else{
        if(tasePreview == true){
            tasePreview = false;
            mp.players.forEachInStreamRange((playa, id) => {
                playa.setAlpha(255);
            });
        }
    }

    if(climbProtectionEndTimestamp > 0){
        let currentTimestamp = Date.now();
        if(climbProtectionEndTimestamp <= currentTimestamp){
            // end climb protection      
            mp.events.callRemote("ClimbProtection", -1);
            if(shortClimbProtectionTimeout){clearTimeout(shortClimbProtectionTimeout); shortClimbProtectionTimeout = undefined;}  
            if(climbProtectionTimeout){clearTimeout(climbProtectionTimeout); climbProtectionTimeout = undefined;}  
            if(checkClimbingDurationTimeout){clearTimeout(checkClimbingDurationTimeout); checkClimbingDurationTimeout = undefined;}
       
            wasClimbingLastFrame = false;
            climbProtectionState = -1;       
            climbProtectionEndTimestamp = 0;
        }
    }


    if(global.antiAFKActive == true){
        let curSecond = Math.floor(Date.now()/1000);
        let lastKeyPressSecond = Math.floor(global.LastKeyPress/1000);

        SecondsSinceLastKeyPress = (curSecond-lastKeyPressSecond);
        
        if(SecondsSinceLastKeyPress < 1500){
            if(SecondsSinceLastKeyPress == 600){ // MAKING SURE IDFK
                if(global.AFKStage == 0){ global.AFKStage = 1;}

                mp.gui.chat.push(Translate("AFK_MESSAGE_1_1"));
                mp.gui.chat.push(Translate("AFK_MESSAGE_1_2"));
                mp.gui.chat.push(Translate("AFK_MESSAGE_1_3"));

                mp.events.callRemote("SetAutoJoin", false, true);
            }
            else if(SecondsSinceLastKeyPress == 900){
                if(global.AFKStage == 0){ global.AFKStage = 1;}
                mp.gui.chat.push(Translate("AFK_MESSAGE_10_MIN"));
            }
            else if(SecondsSinceLastKeyPress == 1200){
                if(global.AFKStage == 0){ global.AFKStage = 1;}
                mp.gui.chat.push(Translate("AFK_MESSAGE_5_MIN"));
            }
            else if(SecondsSinceLastKeyPress == 1440){
                if(global.AFKStage == 0){ global.AFKStage = 1;}
                mp.gui.chat.push(Translate("AFK_MESSAGE_1_MIN"));
            }
        }
        else{
            if(global.AFKStage != 2)
            {
                global.AFKStage = 2;
                mp.gui.chat.push(Translate("AFK_MESSAGE_KICKED"));
                mp.events.callRemote("KickPlayer", "AFK", false);
            }
        }
    }
    
	

    if(ShotStats.UpdateInterval >= 10)
    {
        ShotStats.UpdateInterval = 0;
        mp.events.callRemoteUnreliable("SyncShotStats", ShotStats.TotalFired, ShotStats.HitPlayer, ShotStats.HitVehicle, ShotStats.Missed);
        mp.events.callRemoteUnreliable("UpdateFPS", global.FPS);
        mp.events.callRemote("UpdatePacketloss");

        global.UpdateStatsVars(mp.players.local);
    }
    if(mp.storage.data.menu.monitoring == true){
        mp.events.callRemote("RequestPlayerPing");
    }

    if(HandcuffTooltipShown){

        if((Date.now() >= HandcuffTooltipTime || mp.players.local.getVariable("Team") != 1) && global.chaseRunning){
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
    if(global.chaseRunning && mp.players.local.getVariable("Team") !== undefined && mp.players.local.getVariable("Team") == 0)
    {
        if(mp.players.local.isSwimming() && !mp.players.local.isDead() && !mp.players.local.isInAnyVehicle(false)) {
            mp.events.callRemote("RequestSwimPenalty");
        }
    }
    if(mp.players.local.getVariable("InDM") > -1){
        if(mp.players.local.isSwimming() && !mp.players.local.isDead() && !mp.players.local.isInAnyVehicle(false)) {
            mp.events.callRemote("RequestSwimPenalty");
        }
    }

    if(mp.players.local.getVariable("InLobby") == -6)
    {
        mp.game.ui.setHudColour(143, 255, 0, 0, 128);
        mp.game.ui.setHudColour(144, 255, 0, 0, 128);
        mp.game.ui.setHudColour(145, 255, 0, 0, 128);
    }
    else {
        switch(mp.players.local.getVariable("Team")){
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
    }
});

mp.events.add("ResetShotStats", ResetShotStats);



function getPointToBoneOffset(aimX, aimY, aimZ, boneX, boneY, boneZ) {
    let xDif = (aimX - boneX) < 0 ? -(aimX - boneX) : aimX - boneX;
    let yDif = (aimY - boneY) < 0 ? -(aimY - boneY) : aimY - boneY;
    let zDif = (aimZ - boneZ) < 0 ? -(aimZ - boneZ) : aimZ - boneZ;

    return (xDif + yDif + zDif);
}

function registerLastKeyPress()
{
  global.LastKeyPress = Date.now();

    if(global.AFKStage > 0){      
        mp.events.callRemote("SetAutoJoin", true, true);
        mp.gui.chat.push(Translate("AFK_RETURN_1"));
        mp.gui.chat.push(Translate("AFK_RETURN_2"));
    }

  SecondsSinceLastKeyPress = 0;
  global.AFKStage = 0;
}
for(let i = 1; i < 255; i++)
{
    mp.keys.bind(i, true, registerLastKeyPress);
}


function TogGPSMode()
{
    if(global.menuToggled !== undefined && global.menuToggled) return;
    if(Date.now() - global.lastChatToggle >= 500 && global.chatStatus == false && 
        (mp.players.local.getVariable("pLogged") !== undefined && mp.players.local.getVariable("pLogged") == true))
    {
        global.traceLastFunc(`[Players] Attempt tog GPS`);
        
        mp.events.callRemote("OnScriptedKeyPress", 0x5A, true);
    }
}

let autoOOC = false;

mp.events.addCommand("autoooc", () => {
 
    autoOOC = !autoOOC;
    mp.storage.data.menu.OOCauto = autoOOC;
    mp.storage.flush();

    mp.events.callRemote("SetAutoOOC", autoOOC);
    mp.events.callLocal("HUDNotify", `You will ${autoOOC ? "now" : "no longer"} automatically send messages to OOC chat.`, "blue", "fas fa-check", false, 
        1000, "bottomCenter", "replace");

});

if(mp.storage.data.gpsbind !== undefined)
{
    global.curGPSBind = mp.storage.data.gpsbind;
    global.lastGPSBind = mp.storage.data.gpsbind;
}
if(mp.storage.data.bigmapbind !== undefined)
{
    global.curMapBind = mp.storage.data.bigmapbind;
    global.lastMapBind = mp.storage.data.bigmapbind;
}

if(mp.storage.data.radiotoggle !== undefined)
{
    global.radioToggle = mp.storage.data.radiotoggle;
}

if(mp.storage.data.actionmenubind !== undefined){
    global.curActionMenuBind = mp.storage.data.actionmenubind;
    global.lastActionMenuBind = mp.storage.data.actionmenubind;
}
if(mp.storage.data.enterexitbind !== undefined){
    global.curEnterExitBind = mp.storage.data.enterexitbind;
    global.lastEnterExitBind = mp.storage.data.enterexitbind;

    mp.keys.bind(global.curEnterExitBind, true, global.enterExitCheck);
}
else{
    mp.keys.bind(0x58, true, global.enterExitCheck);
}

if(mp.storage.data.mdcbind !== undefined){
    global.curMDCBind = mp.storage.data.mdcbind;
    global.lastMDCBind = mp.storage.data.mdcbind;
}



mp.keys.bind(global.lastActionMenuBind, true, global.actionMenuOpen);
mp.keys.bind(global.lastActionMenuBind, false, global.actionMenuClose);
mp.keys.bind(global.lastMDCBind, true, global.ToggleMDC);
mp.keys.bind(global.lastGPSBind, true, TogGPSMode);



// const bassBfxBqfLowpass = 0;
// const bassBfxBqfHighpass = 1;
// const bassBfxChannelAll = -1;

// const playerTalkEffects = [];

// mp.events.add("playerStopTalking", (player) => {
//     const talker = playerTalkEffects.find(p => p.handle === player.remoteId);
//     if(!talker){
//         mp.gui.chat.push(`no talk data for ${player.name}`);
//         return;
//     }
//     player.removeVoiceFx(talker.bfq);
//     player.removeVoiceFx(talker.peak);

//     playerTalkEffects.splice(playerTalkEffects.indexOf(talker), 1);
// });
// mp.events.add("playerStartTalking", (player) => {
//     const bfqHandle = player.setVoiceFx(BASSFXType.BASS_FX_BFX_BQF, 0);
//     const peakEqHandle = player.setVoiceFx(BASSFXType.BASS_FX_BFX_PEAKEQ, 0);
//     player.setVoiceFxBQF(bfqHandle, {
//         lFilter: bassBfxBqfHighpass,
//         fCenter: 900.0,
//         fGain: 0.0, 
//         fBandwidth: 0.86, 
//         fQ: 0.0, 
//         fS: 0.0, 
//         lChannel: bassBfxChannelAll
//     });

//     player.setVoiceFxBQF(bfqHandle, {
//         lFilter: bassBfxBqfLowpass,
//         fCenter: 1400.0,
//         fGain: 0.0,
//         fBandwidth: 0.83,
//         fQ: 0.0,
//         fS: 0.0,
//         lChannel: bassBfxChannelAll
//     });

//     player.setVoiceFxPeakEq(peakEqHandle, {
//         lBand: 0,
//         fBandwidth: 13.0, 
//         fQ: 1070.0, 
//         fCenter: 0.0, 
//         fGain: 0.3, 
//         lChannel: bassBfxChannelAll
//     });

//     player.setVoiceFxPeakEq(peakEqHandle, {
//         lBand: 1,
//         fBandwidth: 13.0,
//         fQ: 1300.0,
//         fCenter: 0.0,
//         fGain: 0.4,
//         lChannel: bassBfxChannelAll
//     });

//     playerTalkEffects.push({handle:player.remoteId, bfq:bfqHandle, peak:peakEqHandle});
// });

mp.events.addCommand("gpsbind", function (bind) {
    
    bind = Number(bind);

    if (!Number.isInteger(bind)) {
        mp.gui.chat.push("USAGE: gpsbind (keycode) --Input a virtual keycode e.g. 0x5A is 'Z' (default)");
        mp.gui.chat.push("https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes");
        return;
    }

    if(mp.storage.data.gpsbind !== undefined){
        mp.keys.unbind(mp.storage.data.gpsbind, true, TogGPSMode);
    }
    else{
        mp.keys.unbind(0x5A, true, TogGPSMode);
    }

    mp.storage.data.gpsbind = bind;

    mp.storage.flush();

    global.lastGPSBind = global.curGPSBind;
    global.curGPSBind = bind;

    mp.keys.unbind(global.lastGPSBind, true, TogGPSMode);
    mp.keys.bind(global.curGPSBind, true, TogGPSMode);

    mp.events.callLocal("HUDNotify", `GPS key is now binded to <strong>${String.fromCharCode(global.curGPSBind)}</strong>.`, "blue", "fas fa-check", false, 
        1000, "bottomCenter", "replace");

    mp.events.callLocal("RemoveTooltip", String.fromCharCode(global.lastGPSBind));
});

mp.keys.bind(0x4C, false, function() {
    if(global.menuToggled !== undefined && global.menuToggled) return;
    if(Date.now() - global.lastChatToggle >= 500 && global.chatStatus == false)
    {
        global.traceLastFunc(`[Players] Attempt L key/lock`);

        mp.events.callRemote("OnScriptedKeyPress", 0x4C, true);
    }
});

// mp.keys.bind(0x59, false, function() {
//     if(global.menuToggled !== undefined && global.menuToggled) return;
//     if(Date.now() - global.lastChatToggle >= 500 && global.chatStatus == false)
//     {
//         global.traceLastFunc(`[Players] Attempt Y key/engine`);

//         mp.events.callRemote("OnScriptedKeyPress", 0x59, true);
//     }
// });

function createCam(x, y, z, rx, ry, rz, viewangle, moveable = false) {
    global.traceLastFunc(`[Players] Create Cam`);

    camera = mp.cameras.new("Cam", { x, y, z }, { x: rx, y: ry, z: rz }, viewangle);
    camera_moveable = moveable;

    camera.setActive(true);
    mp.game.cam.renderScriptCams(true, true, 20000000000000000000000000, false, false);
}
function moveCam(x, y, z, rx, ry, rz, time, newviewangle){
    global.traceLastFunc(`[Players] Move Cam`);
    if(!camera) return;
    if(moveToCam != null){
        moveToCam.setActive(false);
        moveToCam.destroy();
        moveToCam = null;
    }
    moveToCam = mp.cameras.new("MoveToCam", {x, y, z}, { x: rx, y: ry, z: rz}, newviewangle);
    camera.shake('HAND_SHAKE', 0.4);

    const {x:oX, y:oY, z:oZ} = camera.getCoord();
    const {x:oXR, y:oYR, z:oZR} = camera.getRot(2);

    setTimeout(() => {
        if(!camera)
            createCam(x,y,z,rx,ry,rz, newviewangle, true);

        if(camera)
            camera.setActiveWithInterp(moveToCam.handle, time, 1, 1);
    }, 100);

    setTimeout(() => {
        if(camera){
            destroyCam();
            createCam(x,y,z,rx,ry,rz, newviewangle, true);
            moveCam(oX, oY, oZ, oXR, oYR, oZR, time, newviewangle);
        }
    }, time-1);
    
}

mp.events.add("SetVehicleLights", (vehicle, state) => vehicle.setLights(state));

mp.events.add("ReportLastSeen", (name, posx, posy, posz) => {
    let getStreet = mp.game.pathfind.getStreetNameAtCoord(posx, posy, posz, 0, 0);

    let zoneName = mp.game.ui.getLabelText(mp.game.zone.getNameOfZone(posx, posy, posz));
    let streetName = mp.game.ui.getStreetNameFromHashKey(getStreet.streetName);

    mp.gui.chat.push(Translate("LAST_SEEN_REPORT", [name, streetName, zoneName]));
});
mp.events.add("ReportFugitiveEnterBuilding", (name, interiorName, streetName, zoneName) => {
    mp.gui.chat.push(Translate("FUGITIVE_ENTER_INTERIOR_REPORT", [name, interiorName, streetName, zoneName]));
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

    if(!player.canExitVehicle){   
        mp.game.controls.disableControlAction(0, 75, true);
    }

    // Anti Pistol Whip?
    if (mp.game.invoke('0x475768A975D5AD17', player.handle, 6))
    {
        //Disable pistol whipping
        mp.game.controls.disableControlAction(0, 140, true); 
        mp.game.controls.disableControlAction(0, 141, true); 
        mp.game.controls.disableControlAction(0, 142, true); 
    }

    if (global.disableSprint) mp.game.controls.disableControlAction(0, 21, true); // Used for cuffing sys

    mp.game.invoke('0xB1906895227793F3', 3); // SET_PLAYER_TARGETTING_MODE => 3 (Free Aim)

    // Taken from GTA.World
    // IsPedArmed (0x475768A975D5AD17): 4 returns true if you are equipped with any weapon except Explosives weapon group AND melee weapons
    // If the player is armed with anything but a melee weapon or explosive weapon disable the lock-on functionality, otherwise enable it
    mp.game.invoke('0x475768A975D5AD17', player.handle, 4) ? mp.game.invoke('0x5C8B2F450EE4328E', player.handle, false) :  mp.game.invoke('0x5C8B2F450EE4328E', player.handle, true);

    // FOR VEH TUNER
    if(camera != null && camera.isActive() && camera.isRendering() && camera_moveable == true && !global.chatStatus && !mp.gui.cursor.visible)
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
    
    if (global.specState == true && global.specTarget != null) 
    {
        // Automatically stop spec if target goes woosh
        if (mp.players.exists(global.specTarget)) 
        {
            // something if spec player is online
        }
        else 
        {
            global.traceLastFunc(`[Players] Stop Spec (target handle)`);

            //specCam.setActive(false); 
            global.specState = false;
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
    global.traceLastFunc(`[Players] Disable Cam`);

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

    global.updateRate = rate;
    mp.storage.data.menu.updateRate = rate;
    mp.storage.flush();

    mp.events.callLocal("HUDNotify", `Updates now happen <strong>${Math.round(1000 / global.updateRate)}</strong> times a second.`, "blue", "fas fa-check", false, 
        1000, "bottomCenter", "replace");
});


mp.keys.bind(global.lastMapBind, false, () => {
    ToggleBigMap(69);
});

mp.events.addCommand("bigmapbind", function (bind) {
    
    bind = Number(bind);

    if (!Number.isInteger(bind)) {
        mp.gui.chat.push("USAGE: bigmapbind (keycode) --Input a virtual keycode e.g. 0x50 is 'P' (default)");
        mp.gui.chat.push("https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes");
        mp.gui.chat.push("Or press 'M' to open the menu and head into the 'Settings' tab to change this.");
        return;
    }

    if(mp.storage.data.bigmapbind !== undefined)
    {
        mp.keys.unbind(mp.storage.data.bigmapbind, true, ToggleBigMap);
    }
    else{
        mp.keys.unbind(0x50, true, ToggleBigMap);
    }

    mp.storage.data.bigmapbind = bind;

    mp.storage.flush();

    global.lastMapBind = global.curMapBind;
    global.curMapBind = bind;

    mp.keys.unbind(global.lastMapBind, true, ToggleBigMap);
    mp.keys.bind(global.curMapBind, true, ToggleBigMap);

    mp.events.callLocal("HUDNotify", `Big Minimap key is now binded to <strong>${String.fromCharCode(global.curMapBind)}</strong>.`, "blue", "fas fa-check", false, 
        1000, "bottomCenter", "replace");

    mp.events.callLocal("RemoveTooltip", String.fromCharCode(global.lastMapBind));
});

function ToggleBigMap(state){

    if((global.vehicleManagerOpen !== undefined && global.vehicleManagerOpen == true) || 
    (global.loadoutManagerOpen !== undefined && global.loadoutManagerOpen == true)) return;
    if((global.VehicleSpawnerActive !== undefined && global.VehicleSpawnerActive == true) ||
    (global.TeleportEditorOpen !== undefined && global.TeleportEditorOpen == true) ||
    (global.WeaponEditorActive !== undefined && global.WeaponEditorActive == true) ||
    (global.ClothingEditorActive !== undefined && global.ClothingEditorActive == true) ||
    (global.ActionMenuActive !== undefined && global.ActionMenuActive == true) || 
    (global.StationPickerToggled !== undefined && global.StationPickerToggled == true) ||
    (global.isHotwiring !== undefined && global.isHotwiring == true) || global.isHeliCamActive()){
        return;
    }

    state = Number(state);

    if (Number.isInteger(state) && (state == 0 || state == 1)) {
        mp.storage.data.menu.alwaysExpandedMap = Boolean(state);
        mp.game.ui.setRadarBigmapEnabled(mp.storage.data.menu.alwaysExpandedMap, false);
    }
    else {

        if(mp.players.local.getVariable("pLogged") === undefined || mp.players.local.getVariable("pLogged") == false) return;
        if(Date.now() - global.lastChatToggle <= 500) return;
        if(global.menuToggled !== undefined && global.menuToggled) return;
        if(global.chatStatus && !global.scoreboardToggled) return;
        if(global.menuToggled) return;

        if(mp.storage.data.menu.alwaysExpandedMap == true)
        {
            mp.storage.data.menu.alwaysExpandedMap = false;
        }
        else 
        {
            mp.storage.data.menu.alwaysExpandedMap = true;
        }
        mp.game.ui.setRadarBigmapEnabled(mp.storage.data.menu.alwaysExpandedMap, false);
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
}

mp.events.addCommand("bigmap", function (state) {
    ToggleBigMap(state);
});

if(!global.radioToggle) mp.game.audio.setInitialPlayerStation('OFF');

mp.events.add("CS_UpdateStatsVars", () => {
    global.UpdateStatsVars(player);
});

mp.events.add("TwoSecondsEvent", () => {
    if(global.ServerUI == null || global.ServerUI == undefined || global.ServerUI == undefined || global.ServerUI == null) return;

    mp.events.callRemoteUnreliable("requestDonatorPointsVar");
    setTimeout(() => {
        global.UpdateStatsVars(mp.players.local, false);
    }, 500);
});

mp.events.addDataHandler('PlayerSquadID', function(entity, value, oldValue){
    entity.SquadID = value;

    setTimeout(() => {
        global.UpdateStatsVars(entity);        
    }, 500);
});

mp.events.addDataHandler('AutoJoin', function(entity, value, oldValue){
    entity.AutoJoin = value;
    
    if(entity.type === 'player' && entity.remoteId == mp.players.local.remoteId)
        mp.storage.data.menu.AutoJoin = value;
});

mp.events.addDataHandler('Team', function (entity, value, oldValue) 
{
    entity.Team = value; 

    
    if(entity.type === 'player' && entity.remoteId == mp.players.local.remoteId)
    {
        //mp.events.callRemote("SetPlayerWounded", 0, -1);
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
    setTimeout(() => {
        global.UpdateStatsVars(entity);        
    }, 500);
});

mp.events.addDataHandler('pLogged', function (entity, value, oldValue) 
{
    entity.LoggedIn = value;
});

mp.events.addDataHandler('InLobby', function (entity, value, oldValue) 
{
    entity.Lobby = value;   
    setTimeout(() => {
        global.UpdateStatsVars(entity);        
    }, 500);
});
mp.events.addDataHandler('Admin', function (entity, value, oldValue){
    entity.Admin = value;   
    setTimeout(() => {
        global.UpdateStatsVars(entity);        
    }, 500);
})

mp.events.add("ClearClimbTaseProtection", (reset_cooldown) => {
    if(shortClimbProtectionTimeout){clearTimeout(shortClimbProtectionTimeout); shortClimbProtectionTimeout = undefined;}  
    if(climbProtectionTimeout){clearTimeout(climbProtectionTimeout); climbProtectionTimeout = undefined;}  
    if(checkClimbingDurationTimeout){clearTimeout(checkClimbingDurationTimeout); checkClimbingDurationTimeout = undefined;}

    if(taseProtectionTimer !== undefined){clearTimeout(taseProtectionTimer); taseProtectionTimer = undefined;}

    mp.events.callRemote("SetTaseProtection", false);

    wasClimbingLastFrame = false;
    climbProtectionState = -1;
    if(reset_cooldown)
        lastClimbProtectionTimestamp = 0;

    climbProtectionEndTimestamp = 0;
});
// TASE PROTECTION
var exitCarTime = 0;
mp.events.add("playerEnterVehicle", (vehicle, seat) => {
    if(player.getVariable("Team") == 0){
        if(taseProtectionTimer !== undefined){
            clearTimeout(taseProtectionTimer);
            taseProtectionTimer = undefined;
        }
        if(player.getVariable("TaseProtection") == true){
            mp.events.callRemote("SetTaseProtection", false);
        }

        if(vehicle.getVariable("Team") == 10 && vehicle.getVariable("Engine") == false){
            vehicle.setEngineOn(false, true, true);
        }
    }
    if(vehicle.getVariable("Fuel") && vehicle.getVariable("Fuel") <= 0.0){
        vehicle.setEngineOn(false, true, true);
    }
});
mp.events.add("playerLeaveVehicle", (vehicle, seat) => {
    exitCarTime = Math.floor(Date.now() / 1000);
    if(player.getVariable("Team") == 0){
        if(taseProtectionTimer !== undefined){
            clearTimeout(taseProtectionTimer);
            taseProtectionTimer = undefined;
        }
        if(player.getVariable("TaseProtection") == false){
            mp.events.callRemote("SetTaseProtection", true);
            taseProtectionTimer = setTimeout(() => {
                if(mp.players.local == null || mp.players.local == undefined || !mp.players.exists(mp.players.local)) return;
           
                mp.events.callRemote("SetTaseProtection", false);
                clearTimeout(taseProtectionTimer);
                taseProtectionTimer = undefined;
            }, 5000);
        }
    }
});
// backup plan for tase protection to check 5s timer
mp.events.add("OneSecondEvent", () => {
    if(!player.isInAnyVehicle(false) && player.getVariable("Team") == 0 && player.getVariable("TaseProtection") == true){
        let currentTimestamp = Math.floor(Date.now() / 1000);
        if(currentTimestamp - exitCarTime >= 5){
            mp.events.callRemote("SetTaseProtection", false);
            clearTimeout(taseProtectionTimer);
            taseProtectionTimer = undefined;
        }
    }
});

var driveby_vehicle = undefined;
mp.events.add("car_window_test", (offsetX, offsetY, offsetZ, rotX, rotY, rotZ) => {
    if(!mp.players.local.vehicle) {mp.gui.chat.push("Not in car."); return;};
    driveby_vehicle = mp.players.local.vehicle;
    var windowIndex = driveby_vehicle.getBoneIndexByName('window_rf');
    mp.players.local.taskLeaveVehicle(driveby_vehicle.handle, 16);
    setTimeout(() => {
        mp.players.local.attachTo(driveby_vehicle.handle, windowIndex, offsetX, offsetY, offsetZ, rotX, rotY, rotZ, false, true, false, true, 0, false);
        mp.players.local.setRotation(rotX, rotY, rotZ, 2, true);
    }, 10);
});
mp.events.add("car_window_end", () => {
    if(!driveby_vehicle) return;

    mp.players.local.detach(true, true);
    setTimeout(() => {
        mp.players.local.taskEnterVehicle(driveby_vehicle.handle, 0, -1, 2.0, 16, 0);
        driveby_vehicle = undefined;
    }, 10);
});

var isInTrunkDB = false;
/*
mp.keys.bind(0xA3, false, () => {
    mp.events.callRemote("db_test");
});
mp.keys.bind(0xA1, false, () => {
    if(isInTrunkDB)
        mp.events.callRemote("db_test_2", 1);
    else
        mp.events.callRemote("db_test_2", 0);
});
mp.events.add("driveby_test_finish", () => {
    if(!mp.players.local.vehicle) return;

    driveby_vehicle = mp.players.local.vehicle;
    var windowIndex = driveby_vehicle.getBoneIndexByName('windscreen_r');
    mp.players.local.taskLeaveVehicle(driveby_vehicle.handle, 16);
    setTimeout(() => {
        mp.players.local.attachTo(driveby_vehicle.handle, windowIndex, 0, -0.55, -0.42, 0, 0, 180, false, true, false, true, 0, true);
        isInTrunkDB = true;
        mp.events.callRemote("db_sync", true, driveby_vehicle.remoteId, windowIndex);
        mp.events.callRemote('ServerConsoleOutput', `[${mp.players.local.name}] Attaching ${mp.players.local.name} to car ${driveby_vehicle.remoteId} index ${windowIndex}`);
    }, 10);

    setTimeout(() => {
        if(!isInTrunkDB) return;
        mp.players.local.attachTo(driveby_vehicle.handle, windowIndex, 0, -0.55, -0.42, 0, 0, 180, false, true, false, true, 0, true);
        mp.events.callRemote('ServerConsoleOutput', `[${mp.players.local.name}] Attaching v2 ${mp.players.local.name} to car ${driveby_vehicle.remoteId} index ${windowIndex}`);
    }, 150);
    setTimeout(() => {
        if(!isInTrunkDB) return;
        mp.players.local.attachTo(driveby_vehicle.handle, windowIndex, 0, -0.55, -0.42, 0, 0, 180, false, true, false, true, 0, true);
        mp.events.callRemote('ServerConsoleOutput', `[${mp.players.local.name}] Attaching v3 ${mp.players.local.name} to car ${driveby_vehicle.remoteId} index ${windowIndex}`);
    }, 1000);
});
mp.events.add("driveby_test_end", () => {
    isInTrunkDB = false;
    mp.game.cam.setFollowPedCamViewMode(1);
    mp.players.local.detach(true, false);
    setTimeout(() => {
        mp.players.local.taskEnterVehicle(driveby_vehicle.handle, 0, 0, 100, 16, 0);
        driveby_vehicle = undefined;
        mp.events.callRemote("db_sync", false, 0, 0);
    }, 10);
});
mp.events.add('driveby_sync_attach', (attach, target, car_id) => {
    if(!attach){
        target.detach(true, false);
        return;
    }

    var target_veh = null;
    mp.vehicles.forEach(veh => {
        if(veh.remoteId === car_id){
            target_veh = veh;
        }
    });
    if(!target_veh){
        mp.events.callRemote('ServerConsoleOutput', `${mp.players.local.name} - not found target veh with id ${car_id}`);
    }
    var windowIndex = target_veh.getBoneIndexByName('windscreen_r');
    setTimeout(() => {
        target.attachTo(target_veh.handle, windowIndex, 0, -0.55, -0.42, 0, 0, 180, false, true, false, true, 0, true);
        mp.events.callRemote('ServerConsoleOutput', `[${mp.players.local.name}] Syncing ${target.name} attach to car ${car_id} index ${windowIndex}`);
    }, 50);
    setTimeout(() => {
        target.attachTo(target_veh.handle, windowIndex, 0, -0.55, -0.42, 0, 0, 180, false, true, false, true, 0, true);
        mp.events.callRemote('ServerConsoleOutput', `[${mp.players.local.name}] Syncing v2 ${target.name} attach to car ${car_id} index ${windowIndex}`);
    }, 1000);
});
mp.events.add('render', () => {
    if(!isInTrunkDB) return;

    mp.game.cam.setFollowPedCamViewMode(4);
});
*/

// mp.keys.bind(0x4A, false, () => {

//     if (mp.players.local.isInAnyVehicle(false) && global.chatStatus == false && Date.now() - global.lastChatToggle >= 500 && 
//         !global.menuToggled && !global.scoreboardToggled)
//     {
//         if(mp.players.local.vehicle.getClass() != 8 && mp.players.local.vehicle.model != mp.game.joaat('policeb')) 
//         { // NO BIKE FUCKERS ALLOWED CUZ YALL GOT "ANTI FALL OFF BIKE" SMH
//             mp.events.callRemote("OnScriptedKeyPress", 0x4A, true); // For toggling da sseeaattbbelltt
//         }
//     }
// });

let clearTaseAnimTimer = null;
mp.events.add("clearTaseAnims", (time) => {
    clearTimeout(clearTaseAnimTimer);
    mp.gui.chat.push("[>] You've been tased, the effect will last " + (time/1000) + " seconds before you can get up!");
    clearTaseAnimTimer = setTimeout(() => {
        mp.players.local.clearTasksImmediately();
        clearTaseAnimTimer = null;
    }, time);
});

mp.events.add('render', () => {

    global.FPS_Frames++;
    if(Date.now() - global.FPS_Calc >= 1000)
    {
        global.FPS = global.FPS_Frames;
        global.FPS_Frames = 0;
        global.FPS_Calc = Date.now();
    }

    if(global.radioToggle != undefined && global.radioToggle == false){
        if(mp.players.local.vehicle != null && mp.players.local.isInAnyVehicle(false)){
            mp.game.invoke('0x1B9C0099CB942AC6', mp.players.local.vehicle.handle, 'OFF');
        }
    }

    if(mp.players.local.getVariable("Team") == 0){
        if(mp.players.local.isInAnyVehicle(false) && mp.players.local.vehicle != null
         && (mp.players.local.vehicle.getPedInSeat(-1) == mp.players.local.handle)){
            if(mp.players.local.vehicle.getVariable("Team") == 10 && mp.players.local.vehicle.getVariable("Engine") == false){
                mp.players.local.vehicle.setEngineOn(false, true, true);
            }
        }
    }

    if(global.specState == true && global.specTarget != null){
        if (mp.players.exists(global.specTarget)) 
        {
            mp.game.invoke("0x8BBACBF51DA047A8", global.specTarget.handle); //NETWORK_SET_IN_SPECTATOR_MODE
        }
        else{
            mp.events.callLocal("StopSpectating");
        }
    }

    if(mp.players.local.getVariable("pLogged") !== undefined && mp.players.local.getVariable("pLogged") == true)
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
                if(Seatbelt_LastVeh != null && mp.vehicles.exists(Seatbelt_LastVeh) && Seatbelt_LastVeh.handle !== 0 && 
                    Seatbelt_LastVeh.getClass() != 8 && Seatbelt_LastVeh.model != mp.game.joaat('policeb'))
                {
                    mp.players.local.setIntoVehicle(Seatbelt_LastVeh.handle, Seatbelt_LastSeat);
                    if(Date.now() > Seatbelt_LastMessage)
                    {
                        Seatbelt_LastMessage = Date.now() + 5000;
                        mp.game.graphics.notify("~r~Seatbelt has prevented you from falling out.");
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
            //mp.game.player.disableFiring(true);
        }
        else { mp.game.controls.enableControlAction(32, 257, true); }

        if(mp.players.local.isClimbing() && mp.players.local.getVariable("Team") >= 0){
            if(!wasClimbingLastFrame){ // if he was not climbing last frame aka just started climbing
                wasClimbingLastFrame = true; // it initiates a timeout for 2,5s
                if(climbProtectionState == -1){

                    let timestampSeconds =Math.floor(Date.now() / 1000);
                    if(timestampSeconds - lastClimbProtectionTimestamp > 45) // if more than 45 seconds passed
                    {
                        if(checkClimbingDurationTimeout){clearTimeout(checkClimbingDurationTimeout); checkClimbingDurationTimeout = undefined;}

                        checkClimbingDurationTimeout = setTimeout(() => {
                            if(!mp.players.local || !mp.players.exists(mp.players.local) || !wasClimbingLastFrame){
                                clearTimeout(checkClimbingDurationTimeout);
                                checkClimbingDurationTimeout = null;
                                return;
                            }

                            if(mp.players.local.isClimbing()){ // if still climbing after 2,5s [ladder], start protection
                                // INITIATE CLIMB PROTECTION
                                lastClimbProtectionTimestamp = timestampSeconds;
                                mp.events.callRemote("ClimbProtection", 0);
                                climbProtectionState = 0;
                                if(climbProtectionTimeout){clearTimeout(climbProtectionTimeout); climbProtectionTimeout = undefined;}    

                                let protecttime = (mp.players.local.getVariable("Team") == 0 ? 9000 : 18500);

                                climbProtectionEndTimestamp = Date.now() + protecttime;

                                climbProtectionTimeout = setTimeout(() => {
                                    if(!mp.players.local || !mp.players.exists(mp.players.local) || !wasClimbingLastFrame){
                                        clearTimeout(climbProtectionTimeout);
                                        return;
                                    }

                                    // If they camp on the ladder, finito, done, stop their godmode
                                    mp.events.callRemote("ClimbProtection", -1);
                                    // However, set this to 0 so it can still give them the short protection
                                    climbProtectionState = 0;    
                                    climbProtectionEndTimestamp = 0;

                                    if(climbProtectionTimeout){clearTimeout(climbProtectionTimeout); climbProtectionTimeout = undefined;}                          
                                }, protecttime);
                            }
                            if(checkClimbingDurationTimeout){clearTimeout(checkClimbingDurationTimeout); checkClimbingDurationTimeout = undefined;}
                        }, 2500);
                    }
                }
            }
        }
        else{ // if he stops climbing, and he was climbing last frame, clear the timeout immediately
            if(wasClimbingLastFrame){ // this should hopefully make sure they dont get climb protection for fences and if they get down too soon
                if(checkClimbingDurationTimeout){clearTimeout(checkClimbingDurationTimeout); checkClimbingDurationTimeout = undefined;}
                wasClimbingLastFrame = false;

                if(climbProtectionState == -1){ /* if no climb protection is active do ??*/
                }
                else if(climbProtectionState == 0){ // if stage 0 [long protection] is active
                    // ACTIVATING THE SHORT PROTECTION
                    // step #1 = clear the long protection timer if there is one
                    climbProtectionState = 1;
                    if(climbProtectionTimeout){clearTimeout(climbProtectionTimeout); climbProtectionTimeout = undefined;}  
                    
                    // step #2 = activating that shit
                    mp.events.callRemote("ClimbProtection", 1);

                    if(shortClimbProtectionTimeout){clearTimeout(shortClimbProtectionTimeout); shortClimbProtectionTimeout = undefined;}  

                    let protecttime = (mp.players.local.getVariable("Team") == 0 ? 2500 : 4000);
                    climbProtectionEndTimestamp = Date.now() + protecttime;
                    shortClimbProtectionTimeout = setTimeout(() => {
                        if(!mp.players.local || !mp.players.exists(mp.players.local)){
                            clearTimeout(shortClimbProtectionTimeout);
                            return;
                        }

                        mp.events.callRemote("ClimbProtection", -1);
                        climbProtectionEndTimestamp = 0;
                        climbProtectionState = -1;

                        if(shortClimbProtectionTimeout){clearTimeout(shortClimbProtectionTimeout); shortClimbProtectionTimeout = undefined;}  
                    }, protecttime);
                }
            }
        }

       /* if(mp.players.local.isClimbing())
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
        }*/

        global.traceLastFunc(`[Players] render::Parachute_Dive_Check();`);

        if(player.doesExist()) {
            if(Date.now() - updateChecker >= global.updateRate)
            {
                // output any debug/stuff per update rate so it doesnt spam for debugging purposes
            }

            /*
            if(mp.game.invoke('0x7DCE8BDA0F1C1200', player.handle)) // Is Ped Parachute Free Falling?
            {
                global.traceLastFunc(`[Players] Parachute_Open();`);
                mp.game.invoke('0x16E42E800B472221', player.handle); // Force Open Parachute (( 1.1 Bug where you can't open ))
            }
            */

            if(player.isInAnyVehicle(false) && global.chaseRunning)
            {
                if (player.seat !== undefined)
                {
                    let disableDriveby = false;
                    if(player.seat == -1 && player.weapon == 911657153)
                    { // DISABLE STUN GUN AIMING
                        disableDriveby = true;
                    }

                    if(player.getVariable("Team") == 0 /*&& player.seat != -1 */&& player.getVariable("HasShot") == false)
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
                    // /*&& player.weapon != 2725352035 && player.getVariable("Team") == 0*/
                } //-- ALLOWED FUGITIVE DRIVER AIMING AGAIN!! 
            }
        }
    }

    if(Date.now() - updateChecker >= global.updateRate)
    {
        mp.events.callLocal("UpdateRateEvent");
        updateChecker = Date.now();

        secondCounter++;

        if(secondCounter >= Math.round(1000 / global.updateRate))
        {
            secondCounter = 0;

            if(mp.players.local.getVariable("pLogged") !== undefined && mp.players.local.getVariable("pLogged") == true)
            {

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

                global.traceLastFunc(`[Players] Update Counters/JailTime`);

                if(mp.players.local.getVariable("JailTime") >= 1)
                {
                    if(global.currentJailTime > 0) global.currentJailTime--;
                    global.lastJailUpdate--;

                    if(global.lastJailUpdate <= 0 || global.currentJailTime <= 0) // add current too so it unmutes/jails the second it hits 0. aka requests update
                    {
                        mp.events.callRemote("RequestCounterUpdate", global.currentJailTime, 0);
                        global.lastJailUpdate = 20;
                    }
                }
                else {
                    if(global.currentJailTime > 0) global.currentJailTime = 0;
                }

                if(mp.players.local.getVariable("MuteTime") >= 1)
                {
                    if(global.currentMuteTime > 0) global.currentMuteTime--;
                    global.lastMuteUpdate--;

                    if(global.lastMuteUpdate <= 0 || global.currentMuteTime <= 0)
                    {
                        mp.events.callRemote("RequestCounterUpdate", global.currentMuteTime, 1);
                        global.lastMuteUpdate = 20;
                    }
                }
                else {
                    if(global.currentMuteTime > 0) global.currentMuteTime = 0;
                }

                global.traceLastFunc(`[Players] Update Stream Data`);
                mp.events.callRemoteUnreliable("UpdateStreamData");

                global.traceLastFunc(`[Players] Update VoiceData`);

                mp.players.forEach(target => {
                    target.voiceVolume = 0.0;
                    if(!target.voice3d && target.handle !== 0) target.voice3d = true;

                    if(target.dimension == mp.players.local.dimension && target != mp.players.local)
                    {
                        var playerPos = target.position;
                        if(target.handle === 0)
                        {
                            playerPos = new mp.Vector3(target.getVariable("UnstreamedX"), target.getVariable("UnstreamedY"), target.getVariable("UnstreamedZ"));
                        }
                       
                        var localPos = mp.players.local.position;
                        let dist = global.calcDist(playerPos, localPos);
                        
                        if(dist > MaxVoiceRange) 
                        {
                            if(target.getVariable("VoiceMode") == 1) // Team
                            {
                                if(target.getVariable("Team") == mp.players.local.getVariable("Team"))
                                {
                                    target.voice3d = false;
                                    target.voiceVolume = 0.5;
                                }
                                else target.voiceVolume = 0.7;
                            }
                            else if(target.getVariable("VoiceMode") == 2) // Squad
                            {
                                target.voice3d = false;
                                target.voiceVolume = 0.8;
                            }
                            else target.voiceVolume = 0.0;
                        }
                        else target.voiceVolume = 1 - (dist / MaxVoiceRange);
                    }
                });

                if (global.specState == true && global.specTarget !== null) {

                    if (mp.players.exists(global.specTarget)) 
                    {
                        var position = global.specTarget.position;

                        if(global.calcDist(position, mp.players.local.position) >= 200.0 
                        || (global.specTarget.handle !== 0) == false || global.specTarget.handle === null || 
                            global.specTarget.handle === undefined)
                        {
                            position.x = global.specTarget.getVariable("UnstreamedX");
                            position.y = global.specTarget.getVariable("UnstreamedY");
                            position.z = global.specTarget.getVariable("UnstreamedZ");
                        }
                        position.z -= 20;

                        mp.players.local.position = position;
                    }
                    else 
                    {
                        global.specState = false;
                        mp.events.callLocal("StopSpectating");
                    }
                }

                global.traceLastFunc(`[Players] Update Derby Data`);

                if(mp.players.local.getVariable("InLobby") == -4)
                {
                    derbyText.text = "N/A";

                    if(global.showHud !== undefined && global.showHud == false)
                    {
                        derbyText.visible = false;
                    }
                    else {
                        derbyText.visible = true;
                    }

                    if(global.ServerUI != null)
                    {

                        if(global.ServerUI) mp.events.callLocal("setMiniHudItemToggle", 9, true);                
                        let timeDisplay = new Date(derbyTimeCur * 1000).toISOString().substring(14, 19);
                        if(global.ServerUI) mp.events.callLocal('setMiniHudItemValue', 9, timeDisplay);
                    }

                    if(derbyCounting)
                    {
                        mp.events.callRemoteUnreliable("DerbyIncreaseCount");
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
                }

                global.traceLastFunc(`[Players] Update Chase Data1`);

                if(global.ServerUI != null && global.chaseRunning == true)
                {
                    global.chaseTimeCur += 1;

                    if(global.chaseTimeCur <= 0 && mp.players.local.getVariable("Team") >= 0)
                    {
                        mp.players.local.freezePosition(true);

                        if(mp.players.local.isInAnyVehicle(false))
                        {
                            mp.players.local.vehicle.freezePosition(true);
                        }
                    }
                    else if(global.chaseTimeCur == 1 && mp.players.local.getVariable("Team") == 0)
                    {
                        mp.players.local.freezePosition(false);

                        if(mp.players.local.isInAnyVehicle(false))
                        {
                            mp.players.local.vehicle.freezePosition(false);
                        }
                    }
                }

                global.traceLastFunc(`[Players] Update Chase Data2`);

                if(global.chaseRunning == true && global.ServerUI != null)
                {
                    if(global.collisionTime > 0)
                    {
                        global.collisionTime--;
                    } 
                }

                global.traceLastFunc(`[Players] Update Chase Data3`);
                global.traceLastFunc(`[Players] Update Chase Data Complete`);
            }
        }

        global.traceLastFunc(`[Players] Blips Stage1`);

        mp.blips.forEach(eblip => {
            var index = playerBlips.indexOf(eblip.id);
            if(index != -1)
            {
                eblip.destroy();
            }
        });

        playerBlips = [];

        global.traceLastFunc(`[Players] Blips Stage2`);
        if(global.chaseMode == 0 || global.chaseMode == 1){ // COPCHSE & BUSTED MODES
            handleCopchaseBlips();
        }
        if(global.chaseMode == 2){ // BANK ROBBERY
            handleBankRobberyBlips();
        }
        

        global.traceLastFunc(`[Players] Update Completed`);
    }

    if(Date.now() - targetChecker >= 25 && player.getVariable("Team") !== undefined && player.getVariable("Team") != -1)
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
                            
                            mp.players.streamed.forEach(occupant => {
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
                        else if(target.getVariable("Team") == 0 && player.getVariable("Team") == 1 && global.chaseRunning)
                        {
                            clickDisable = true; // Disable innocent car shots by default.
                            var vehicleOccupants = 0;

                            mp.players.streamed.forEach(occupant => {
                                if(occupant.isInVehicle(target.handle, false)) {
                                    vehicleOccupants++;
                                    if(occupant.getVariable("Team") == 0 && occupant.getVariable("HasShot") == true && !occupant.isDead()) {
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
                                        mp.players.streamed.forEach(occupant => {
                                            if(occupant.isInVehicle(target.vehicle.handle, true) && occupant.getVariable("Team") != target.getVariable("Team")) { 
                                                clickDisable = false; // Member of opposite team in it? Shoot.
                                            }
                                        });
                                    }
                                }
                            }

                            if(player.getVariable("Team") == 1 && target.getVariable("Team") == 0 && 
                                target.getVariable("HasShot") == false && global.chaseRunning) // Disable click for fugi with no tag
                            {
                                clickDisable = true;

                                if(target.isInAnyVehicle(true) && clickDisable == true && target.vehicle)
                                {
                                    if(target.vehicle.getVariable("Team") !== undefined && target.vehicle.getVariable("Team") != target.getVariable("Team")) {
                                        clickDisable = false;
                                    }
                                    else {
                                        mp.players.streamed.forEach(occupant => {
                                            if(occupant.isInVehicle(target.vehicle.handle, true) && occupant.getVariable("Team") == target.getVariable("Team") && 
                                            occupant.getVariable("HasShot") == true && !occupant.isDead()) {
                                                clickDisable = false;
                                            }
                                        });
                                    }
                                }

                                if(player.weapon == 911657153) 
                                {
                                    clickDisable = false; // Stun Gun allow if conditions down don't match.

                                    //if(target.isInAnyVehicle(false) == true || global.calcDist(target.position, player.position) > 7.5) clickDisable = true;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
});
function handleBankRobberyBlips(){
    if(!global.isInInterior){
        mp.players.forEach(target => {

            if(target.dimension == mp.players.local.dimension/* && target != mp.players.local*/ && (!target.isDead() || target.handle == 0))
            {
                // For cops, fugitives show up RED on the map in Bank Robbery. Other cops are always visible on the map no matter the GPS status.
                if(mp.players.local.getVariable("Team") == 1){ // if cop
                    var blipColor = 45;
                    var blipId = 1;

                    if(target.getVariable("Team") == 0){
                        blipColor = 1;
                    }
                    if(target.getVariable("Team") == 1){
                        blipColor = 3;
                    }
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

                    if(global.calcDist(position, mp.players.local.position) >= 1000.0 
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
                else if(mp.players.local.getVariable("Team") == 0){ // if fugitive
                    var blipColor = 45;
                    var blipId = 1;

                    // Only HEIST LEADERS are visible, marked BLUE on the map.
                    // Cops will be marked RED if their GPS status is active.
                    // Fellow fugitives are invisible to prevent confusion
                    let displayThisPlayer = false;
                    let isHeistLeader = false;
                    let isPlayerCop = false;

                    if(target.getVariable("Team") == 0){
                        displayThisPlayer = true;
                        if(target.getVariable("HeistLeader") === true){
                            blipColor = 3;
                            isHeistLeader = true;
                        }
                        else{
                            blipColor = 43;
                        }
                    }
                    if(target.getVariable("Team") == 1 && target.getVariable("VisibleBlip") == true){
                        blipColor = 1;
                        displayThisPlayer = true;
                        isPlayerCop = true;
                    }
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

                    if(global.calcDist(position, mp.players.local.position) >= 1000.0 
                    || (target.handle !== 0) == false || target.handle === null || 
                        target.handle === undefined || target.position == null || (target.position.x == 0.0 && target.position.z == 0.0))
                    {
                        position.x = target.getVariable("UnstreamedX");
                        position.y = target.getVariable("UnstreamedY");
                        position.z = target.getVariable("UnstreamedZ");
                        if(target.getVariable("UnstreamedState") == 1) blipId = 724;
                    }

                    if(displayThisPlayer == true){
                        var targetBlip = mp.blips.new(blipId, position,
                            {
                                name: target.name,
                                color: blipColor,
                                shortRange: false,
                                dimension: mp.players.local.dimension,
                                scale:((isHeistLeader == true || isPlayerCop == true) ? 1.0 : 0.5) // if cop or heist leader, normal scale,
                                                                                                   // otherwise make blip smaller
                            });
                            playerBlips.push(targetBlip.id);
                    }
             
                }

                
            }
        });
    }
    else{
        
    }
}
function handleCopchaseBlips(){
    if(!global.isInInterior){
        mp.players.forEach(target => {

            if(target.dimension == mp.players.local.dimension/* && target != mp.players.local*/ && (!target.isDead() || target.handle == 0))
            {
                if ((target.getVariable("VisibleBlip") !== undefined && target.getVariable("VisibleBlip") == true) || 
                    (target.getVariable("Team") == 0 && mp.players.local.getVariable("Team") == 0 && target.remoteId != mp.players.local.remoteId))
                {
                    var blipColor = 45;
                    var blipId = 1;
                    if(target.getVariable("InLobby") == -6 && target.getVariable("InDM") != -1)
                    {
                        blipColor = Number(target.getVariable("DMBlipColor"));
                    }
                    else 
                    {
                        if(target.getVariable("Team") == 0) 
                        {
                            blipColor = 70;
                            if(target.getVariable("HasShot") == true){
                                blipColor = 70;
                                blipId = 458;
                            }
                        }
                        else if(target.getVariable("Team") == 1) blipColor = 3;
                        if(target.getVariable("InLobby") == -3) blipColor = 15;
                        if(target.getVariable("InLobby") == -4) blipColor = 69;
                        if(target.getVariable("InDM") != -1) blipColor = 39;
                    }
                    

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

                    if(global.calcDist(position, mp.players.local.position) >= 1000.0 
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
    }
    else{
        
    }
}


// ADDING SHORT COOLDOWN TO SHOOTING AFTER THEY DO THE ROLL THING
var rollShootCooldownActive = false;
var currentRollCooldownTimestamp = 0;
mp.events.add("render", () => {
    if(mp.game.controls.isControlJustPressed(0, 22)){
        if(mp.game.invoke('0x68EDDA28A5976D07') == 1 || mp.game.player.isFreeAiming()){
            if(mp.players.local.isOnFoot())
            {
                currentRollCooldownTimestamp = (Date.now() + 1000);
                rollShootCooldownActive = true;
                if(global.ServerUI) global.ServerUI.call('notification_send', Translate("SHOOTING_COOLDOWN"), 'warning', 1000);
            }
        }
    }

    if(rollShootCooldownActive == true){
        mp.game.controls.disableControlAction(32, 24, true);
        mp.game.controls.disableControlAction(32, 140, true);
        mp.game.controls.disableControlAction(32, 141, true);
        mp.game.controls.disableControlAction(32, 142, true);
        mp.game.controls.disableControlAction(32, 257, true);
        mp.game.controls.disableControlAction(32, 263, true);
        mp.game.controls.disableControlAction(32, 264, true);
    }
    if(Date.now() > currentRollCooldownTimestamp){
        rollShootCooldownActive = false;
        currentRollCooldownTimestamp = 0;
    }


});

let shotListenData = {
    playerHitsInRow:0,
}

var recoil_smoothing = 0.65;
mp.events.add("changeRecoilSmooting", (value) => {
    recoil_smoothing = value;
})

// stop shooting while tased
mp.events.add("render", () => {
    if(mp.players.local.getVariable("TasedTill") > Date.now()){
        mp.game.controls.disableControlAction(32, 24, true);
        mp.game.controls.disableControlAction(32, 69, true);
        mp.game.controls.disableControlAction(32, 92, true);
        mp.game.controls.disableControlAction(32, 106, true);
        mp.game.controls.disableControlAction(32, 122, true);
        mp.game.controls.disableControlAction(32, 135, true);
        mp.game.controls.disableControlAction(32, 142, true);
        mp.game.controls.disableControlAction(32, 144, true);
        mp.game.controls.disableControlAction(32, 257, true);
        mp.game.controls.disableControlAction(32, 346, true);
    }
});

mp._events.add("playerWeaponShot", (targetPosition, targetEntity) => 
{
    if(mp.players.local.getVariable("TasedTill") > Date.now()) return; // if currently tased

    if(targetPosition){
        let hitPlayer = false;
        if(targetEntity !== undefined && targetEntity.type == 'player') hitPlayer = true;
        mp.events.callRemote("AC_WeaponShot:BulletTrace", targetPosition.x, targetPosition.y, targetPosition.z, hitPlayer);
    }
    if(mp.players.local.getVariable("HasShot") == false && player.getVariable("Team") == 0 && global.chaseRunning)
    {
        mp.events.callRemote("GivePlayerShotTag");
    }
    if(global.chaseRunning && mp.players.local.Team == 0 || mp.players.local.Team == 1)
        mp.events.callRemote("SetShotsFired", true);

    ShotStats.TotalFired++;

    let gunData = global.weaponData.find(w => Number(w.Hash) == mp.players.local.weapon);
    if(mp.players.local.vehicle == undefined && !mp.players.local.isInAnyVehicle(false))
    {
        const campos = mp.game.cam.getGameplayCamRot(0);
        const camhead = mp.game.cam.getGameplayCamRelativeHeading();

        var shakeValue = 0;
        if(gunData == undefined){
            shakeValue = (getRandomInt(0, 7)/10);
        }
        else{
            var gunType = gunData.Type;
            var gunName = gunData.Name;
            switch(gunType){ // first doing the types in general
                case "Handguns":
                    shakeValue = (getRandomInt(2, 6)/10);
                    break;
                case "Submachine Guns":
                    shakeValue = (getRandomInt(2, 4)/10);
                    break;
                case "Shotguns":
                    if(gunName !== "Heavy Shotgun" && gunName !== "Combat Shotgun" && gunName !== "Assault Shotgun")
                        shakeValue = (getRandomInt(55, 65)/10);
                    else shakeValue = (getRandomInt(15,25)/10);
                    break;
                case "Assault Rifles":
                    shakeValue = (getRandomInt(1, 2)/10);
                    break;
                case "Machine Guns":
                    shakeValue = (getRandomInt(3, 6)/10);
                    break;
                case "Sniper Rifles":
                    shakeValue = (getRandomInt(4, 6)/10);
                    break;
                default:
                    shakeValue = (getRandomInt(4, 9)/10);
                    break;
            }
            // and now doing some specific weapons
            if(gunName.includes("Revolver") || gunName === "Marksman Pistol" || gunName === "Pistol .50"){
                shakeValue = (getRandomInt(9, 14)/10);
            }
            if(gunName === "Stun Gun"){
                shakeValue = 0;
            }
        }

        if(gunType !== "Shotguns"){
            let isCrouching = false, isAiming = false;
            if(mp.players.local.getStealthMovement() == true){
                shakeValue = shakeValue/2.25; // slightly more accurate when crouching
                isCrouching = true;
            }
            if(mp.game.player.isFreeAiming() == true){ // if player is holding right click to aim
                shakeValue = shakeValue/3.5; // more accurate when holding right click
                isAiming = true;
            }

            if(!isAiming && !isCrouching && gunType !== "Handguns"){
                shakeValue = shakeValue * 4;
            }
        }
        else{
            if(mp.game.player.isFreeAiming() == true){ // if player is holding right click to aim
                shakeValue = shakeValue/1.25; // more accurate when holding right click
            }
        }
    

        if((Date.now() - recoil_LastShotTime < 350) && gunType === "Handguns"){
            shakeValue = shakeValue * 1.75;
        }

        recoil_ShotsInBurst++;
        if(gunType !== "Handguns" && gunType !== "Shotguns"){
            if(recoil_ShotsInBurst % 10 === 0){
                recoil_goingUp = !recoil_goingUp;
            }
        }

        if(global.isAntiPeekOn){
            shakeValue = shakeValue * 4; // double recoil if anti-peek is active
        }

        //shakeValue = shakeValue/recoil_smoothing;
        
        const val = shakeValue;
        const headval = (getRandomInt(0, 10) > 5 ? (val/3) : -(val/3));
        const pitchval = campos.x + shakeValue;
        //const pitchval = campos.x + (recoil_goingUp === true ? shakeValue : (-1-shakeValue));
        mp.game.cam.setGameplayCamRelativePitch(pitchval, recoil_smoothing);
        mp.game.cam.setGameplayCamRelativeHeading(camhead + headval);
        recoil_LastShotTime = Date.now();
        //mp.game.cam.shakeGameplayCam('JOLT_SHAKE', shakeValue/2);
    }
    else{
        if(mp.players.local?.vehicle){
            let value = 0.3;
            let carSpeed = mp.players.local.vehicle.getSpeed();
            if(gunData && gunData.Type){
                var gunType = gunData.Type;
                switch(gunType){ // first doing the types in general
                    case "Handguns":
                        value = (carSpeed * 0.013) + 0.015;
                        break;
                    case "Submachine Guns":
                        value = (carSpeed * 0.0075) + 0.015;
                        break;
                    default:
                        shakeValue = (carSpeed * 0.013) + 0.01;
                        break;
                }
            }
            mp.game.cam.shakeGameplayCam('JOLT_SHAKE', value);
        }
    }

    if(targetEntity !== null && targetEntity !== undefined)
    {
        if(targetEntity.type === 'player'){
            shotListenData.playerHitsInRow++;

        }
        else{
            shotListenData.playerHitsInRow = 0;
        }

        let gunName = (gunData ? gunData.Name : "Unknown");
        mp.events.callRemote("ShotListen_SendInfo", gunName, targetEntity.type, shotListenData.playerHitsInRow);

        if(targetEntity.type === 'vehicle') ShotStats.HitVehicle++;
        else if(targetEntity.type === 'player') ShotStats.HitPlayer++;
        else ShotStats.Missed++;

        if(targetEntity.type === 'vehicle' && player.getVariable("Team") != -1)
        {
            if(targetEntity.getVariable("Team") !== undefined)
            {
                var vehicleOccupants = 0;
                let breakShot = false;

                if(targetEntity.getVariable("Team") == player.getVariable("Team"))
                {
                    breakShot = true; // Break team-kill car by default
                    
                    mp.players.streamed.forEach(occupant => {
                        if(occupant.isInVehicle(targetEntity.handle, false)) {
                            vehicleOccupants++;
                            if(occupant.getVariable("Team") != player.getVariable("Team")) 
                            {
                                breakShot = false; // Don't break shot if there's a fugi in or opposite team member!
                            }
                        }
                    });
                    if(mp.players.local.vehicle != undefined && mp.players.local.vehicle.handle == targetEntity.handle) breakShot = false;

                    if(vehicleOccupants == 0) breakShot = false;

                    if(breakShot) mp.game.graphics.notify("~r~Do not teamkill!");
                }
                else if(targetEntity.getVariable("Team") == 0 && player.getVariable("Team") == 1 && player.weapon != 911657153 && global.chaseRunning)
                {
                    breakShot = true; // Break innocent car shots by default.

                    mp.players.streamed.forEach(occupant => {
                        if(occupant.isInVehicle(targetEntity.handle, false)) {
                            vehicleOccupants++;
                            if(occupant.getVariable("Team") == 0 && occupant.getVariable("HasShot") == true && !occupant.isDead()) {
                                breakShot = false; // Don't break shot if there's a fugitive that shot in!
                            }
                        }
                    });

                    if(vehicleOccupants == 0) breakShot = false;

                    if(breakShot) mp.game.graphics.notify("~r~Do not deathmatch!");
                }
                
                if(targetEntity.getVariable("Team") == 0 && player.getVariable("Team") == 1 && player.weapon == 911657153 && global.chaseRunning)
                {
                    breakShot = true;
                    mp.console.logInfo("Taser does not work for vehicles.");
                }


                if(breakShot == true) return true;
            }
        }
        if(targetEntity.type === 'player')
        {
            if(global.chaseRunning)
            {
                if(targetEntity.getVariable("Team") !== undefined && targetEntity.getVariable("HasShot") !== undefined)
                {
                    if(targetEntity.getVariable("ClimbProtection") == true || player.getVariable("ClimbProtection") == true) return true;
                    
                    if(player.weapon == 911657153 && (targetEntity.isInAnyVehicle(false) == true || 
                    global.calcDist(targetEntity.position, player.position) > 7.5)) {
                        mp.game.graphics.notify("~r~Taser out of reach, get closer!");
                        return true; // Stun Gun no shot if more than 10 meters or target in car.
                    }

                    if(BigInt(player.weapon) == BigInt(911657153) && targetEntity.getVariable("TaseProtection") == true
                        && targetEntity.getVariable("Team") == 0)
                    {
                        mp.game.graphics.notify("~r~Cannot tase! Target exiting vehicle!");
                        return true;
                    }

                    if(player.weapon == 911657153 && (targetEntity.getVariable("HasShot") == true || global.chaseTimeCur < 60) && targetEntity.getVariable("Team") == 0)
                    {
                        return true;
                    }
                }
            }

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
                        player.weapon != 911657153 && global.chaseRunning) // Shot a fugitive that hasn't shot.
                    {
                        mp.events.callRemote("PlayerShotAtWrongPlayer", 1, targetEntity);
                        return true;
                    }
                }
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

            if(targetEntity.getVariable("Team") != mp.players.local.getVariable("Team") && global.chaseRunning && mp.players.local.getVariable("Team") != -1)
            {
                if(targetEntity.getVariable("Wounded") == 1)
                {
                    return true;
                }
            }
        }
    }
    else{
        shotListenData.playerHitsInRow = 0;
        let gunName = (gunData ? gunData.Name : "Unknown");
        mp.events.callRemote("ShotListen_SendInfo", gunName, "nothing", shotListenData.playerHitsInRow);
    }
});

let damageNametagTimeout = null;
let isShowingDamage = false;

mp.events.add("setClearDamageNametag", () => {
    if(damageNametagTimeout){
        clearTimeout(damageNametagTimeout);
        damageNametagTimeout = null;
    }
    isShowingDamage = true;
    damageNametagTimeout = setTimeout(() => {
        if(mp.players.local == null || mp.players.local == undefined || !mp.players.exists(mp.players.local)) return;
        
        mp.events.callRemote("RemoveIsDamaged");
        damageNametagTimeout = null;
        isShowingDamage = false;
    }, 500);
});

mp.events.add('incomingDamage', (sourceEntity, sourcePlayer, targetEntity, weapon, boneIndex, damage) => {

    if(mp.players.local.getVariable("Invincible") === false && sourceEntity.type !== 'player'){
        mp.events.callRemote("SetIsDamaged");
    }

    if(targetEntity !== null && targetEntity !== undefined && targetEntity.type === 'vehicle'){
        if(sourceEntity !== null && sourceEntity !== undefined && sourceEntity.type === 'player'){
            let nerfedDmg = Math.floor(damage/2);
            damage = nerfedDmg;
            mp.game.weapon.setCurrentDamageEventAmount(nerfedDmg);
            return;
        }
    }

    damage = 0;
    mp.game.weapon.setCurrentDamageEventAmount(0);
    mp.game.weapon.cancelCurrentDamageEvent();
    return;

    if(targetEntity !== null && targetEntity !== undefined && targetEntity.type === 'player' && targetEntity == player)
    {
        if(sourceEntity !== null && sourceEntity !== undefined && sourceEntity.type === 'player')
        {
            let gunData = global.weaponData.find(w => Number(w.Hash) == weapon);
            if(gunData != undefined && (gunData.Type == "Assault Rifles" || gunData.Type == "Machine Guns")){
                let nerfedDmg = (damage-10);
                damage = nerfedDmg;
                mp.game.weapon.setCurrentDamageEventAmount(nerfedDmg);

                showDamageIndicatorOnPosition(nerfedDmg, targetEntity, boneIndex);
                ////global.LogPlayerDamage(targetEntity, weapon, nerfedDmg, boneIndex);
                damage = 0;
                mp.game.weapon.setCurrentDamageEventAmount(0);
                mp.game.weapon.cancelCurrentDamageEvent();
            }
            else{
                let nerfedDmg = (damage-5);
                if(nerfedDmg <= 0) nerfedDmg = 1;
                damage = nerfedDmg;
                mp.game.weapon.setCurrentDamageEventAmount(nerfedDmg);
                
                showDamageIndicatorOnPosition(nerfedDmg, targetEntity, boneIndex);

                ////global.LogPlayerDamage(targetEntity, weapon, nerfedDmg, boneIndex);
                damage = 0;
                mp.game.weapon.setCurrentDamageEventAmount(0);
                mp.game.weapon.cancelCurrentDamageEvent();
            }
            if(weapon == 2441047180){ // navy revolver nerf
                const vec1 = sourceEntity.position;
                const vec2 = targetEntity.position;

                const distance = vec1.subtract(vec2).length();

                if(distance < 9.0){
                    damage = 75;
                    mp.game.weapon.setCurrentDamageEventAmount(75);
                
                    showDamageIndicatorOnPosition(75, targetEntity, boneIndex);

                    ////global.LogPlayerDamage(targetEntity, weapon, 75, boneIndex);
                damage = 0;
                mp.game.weapon.setCurrentDamageEventAmount(0);
                mp.game.weapon.cancelCurrentDamageEvent();
                }
                else if(distance >= 9.0 && distance < 18.0){
                    damage = 50;
                    mp.game.weapon.setCurrentDamageEventAmount(50);
                
                    showDamageIndicatorOnPosition(50, targetEntity, boneIndex);

                    ////global.LogPlayerDamage(targetEntity, weapon, 50, boneIndex);
                damage = 0;
                mp.game.weapon.setCurrentDamageEventAmount(0);
                mp.game.weapon.cancelCurrentDamageEvent();
                }
                else{
                    damage = 25;
                    mp.game.weapon.setCurrentDamageEventAmount(25);
                
                    showDamageIndicatorOnPosition(25, targetEntity, boneIndex);

                    ////global.LogPlayerDamage(targetEntity, weapon, 25, boneIndex);
                damage = 0;
                mp.game.weapon.setCurrentDamageEventAmount(0);
                mp.game.weapon.cancelCurrentDamageEvent();
                }
            }
            if(weapon == 3800352039){ // assault shotgun nerf
                const vec1 = sourceEntity.position;
                const vec2 = targetEntity.position;

                const distance = vec1.subtract(vec2).length();
                if(distance < 9.2681523185659){
                    damage = 10;
                    mp.game.weapon.setCurrentDamageEventAmount(10);
                
                    showDamageIndicatorOnPosition(10, targetEntity, boneIndex);
                
                    ////global.LogPlayerDamage(targetEntity, weapon, 10, boneIndex);
                damage = 0;
                mp.game.weapon.setCurrentDamageEventAmount(0);
                mp.game.weapon.cancelCurrentDamageEvent();
                }
                else if(distance > 9.2681523185659 && distance < 19.267101852897){
                    damage = 7;
                    mp.game.weapon.setCurrentDamageEventAmount(7);
                
                    showDamageIndicatorOnPosition(7, targetEntity, boneIndex);
                
                    ////global.LogPlayerDamage(targetEntity, weapon, 7, boneIndex);
                damage = 0;
                mp.game.weapon.setCurrentDamageEventAmount(0);
                mp.game.weapon.cancelCurrentDamageEvent();
                }
                else{
                    damage = 0;
                    mp.game.weapon.setCurrentDamageEventAmount(0);
                
                    ////global.LogPlayerDamage(targetEntity, weapon, 0, boneIndex, "Target too far - no damage dealt.");
                damage = 0;
                mp.game.weapon.setCurrentDamageEventAmount(0);
                mp.game.weapon.cancelCurrentDamageEvent();
                }
            }
            else if(weapon == 984333226){  // heavy shotgun nerf - shoots 1 pellet so it gets more dmg
                const vec1 = sourceEntity.position;
                const vec2 = targetEntity.position;

                const distance = vec1.subtract(vec2).length();
                if(distance < 9.2681523185659){
                    damage = 37;
                    mp.game.weapon.setCurrentDamageEventAmount(37);
                
                    showDamageIndicatorOnPosition(37, targetEntity, boneIndex);
                
                    ////global.LogPlayerDamage(targetEntity, weapon, 37, boneIndex);
                damage = 0;
                mp.game.weapon.setCurrentDamageEventAmount(0);
                mp.game.weapon.cancelCurrentDamageEvent();
                }
                else if(distance > 9.2681523185659 && distance < 19.267101852897){
                    damage = 28;
                    mp.game.weapon.setCurrentDamageEventAmount(28);
                
                    showDamageIndicatorOnPosition(28, targetEntity, boneIndex);
                
                    ////global.LogPlayerDamage(targetEntity, weapon, 28, boneIndex);
                damage = 0;
                mp.game.weapon.setCurrentDamageEventAmount(0);
                mp.game.weapon.cancelCurrentDamageEvent();
                }
                else{
                    damage = 0;
                    mp.game.weapon.setCurrentDamageEventAmount(0);
                
                    ////global.LogPlayerDamage(targetEntity, weapon, 0, boneIndex, "Target too far - no damage dealt.");
                damage = 0;
                mp.game.weapon.setCurrentDamageEventAmount(0);
                mp.game.weapon.cancelCurrentDamageEvent();
                }
            }

            if(weapon == 911657153 && targetEntity.isInAnyVehicle(false)) return true; // Disable stun if in car.

            if(targetEntity.getVariable("ClimbProtection") == true || sourceEntity.getVariable("ClimbProtection") == true) return true;

            if(BigInt(weapon) == BigInt(911657153) && BigInt(targetEntity.getVariable("LastVehEnterExit")) + BigInt(5000) >= BigInt(Date.now())) return true;

            if(weapon == 911657153 && targetEntity.getVariable("HasShot") == true && targetEntity.getVariable("Team") == 0 && global.chaseRunning) return true;

            if(sourceEntity.getVariable("Team") != -1 && player.getVariable("Team") != -1)
            {
                if(player.getVariable("Team") == sourceEntity.getVariable("Team"))
                {
                    return true;
                }

                if(sourceEntity.getVariable("Team") == 1 && player.getVariable("Team") == 0 && 
                    player.getVariable("HasShot") == false && weapon != 911657153 && global.chaseRunning) // Took damage from a cop and hasn't shot.
                {
                    return true;
                }

                if(global.chaseRunning)
                {
                    let HP = mp.players.local.getHealth();

                    if(HP - damage <= 15){

                        if(mp.players.local.getVariable("Wounded") == 0) 
                        {
                            if(!mp.players.local.isDead()){
                                mp.game.player.setInvincible(true);
                                mp.events.callRemote("SetInvincible", true);
                                mp.players.local.setInvincible(true);
                                mp.events.callLocal("setWounded", 1, sourcePlayer.remoteId);
                                damage = 0;
                                mp.game.weapon.setCurrentDamageEventAmount(0);
                                mp.game.weapon.cancelCurrentDamageEvent();
                                return true;
                            }
                            else{
                                damage = 0;
                                mp.game.weapon.setCurrentDamageEventAmount(0);
                                mp.game.weapon.cancelCurrentDamageEvent();
                                return true;
                            }
                        }
                    }

                    if(mp.players.local.getVariable("Wounded") == 1)
                    {
                        damage = 0;
                        mp.game.weapon.setCurrentDamageEventAmount(0);
                        mp.game.weapon.cancelCurrentDamageEvent();
                        return true;
                    }
                }
            }
        }
    }
    else if(targetEntity !== null && targetEntity !== undefined && targetEntity.type === 'vehicle'){
        if(sourceEntity !== null && sourceEntity !== undefined && sourceEntity.type === 'player'){
            let nerfedDmg = Math.floor(damage/2);
            damage = nerfedDmg;
            mp.game.weapon.setCurrentDamageEventAmount(nerfedDmg);
        }
    }
});

mp.events.add("togglePlayerCrawl", (toggle) => {toggleCrawl(toggle)});

let crawlAnim;
let crawlAnimTimeout;

let woundedTimeLeft = 60;
let woundedTimer = null;

mp.events.add('render', () => {
    if(mp.players.local.getVariable("Wounded") > 0){
        //if (mp.players.local.isInAnyVehicle(false)) { // making sure to disable driveby and vehicle movement completely good RP
            mp.game.controls.disableControlAction(0, 61, true);
            mp.game.controls.disableControlAction(0, 62, true);
            mp.game.controls.disableControlAction(0, 63, true);
            mp.game.controls.disableControlAction(0, 64, true);
            mp.game.controls.disableControlAction(0, 71, true);
            mp.game.controls.disableControlAction(0, 72, true);
            mp.game.controls.disableControlAction(0, 73, true);
            mp.game.controls.disableControlAction(0, 74, true);
            mp.game.controls.disableControlAction(0, 75, true);
            mp.game.controls.disableControlAction(0, 278, true);
            mp.game.controls.disableControlAction(0, 279, true);

            mp.game.controls.disableControlAction(0, 76, true);
            mp.game.controls.disableControlAction(0, 91, true);
            mp.game.controls.disableControlAction(0, 92, true);
            mp.game.controls.disableControlAction(0, 24, true);
            mp.game.controls.disableControlAction(0, 25, true);
            mp.game.controls.disableControlAction(0, 69, true);
            mp.game.controls.disableControlAction(0, 70, true);
            mp.game.controls.disableControlAction(0, 140, true);
            mp.game.controls.disableControlAction(0, 141, true);
            mp.game.controls.disableControlAction(0, 142, true);
            mp.game.controls.disableControlAction(0, 257, true);
            mp.game.controls.disableControlAction(0, 263, true);
            mp.game.controls.disableControlAction(0, 265, true);
            mp.game.controls.disableControlAction(32, 68, true);
            mp.game.controls.disableControlAction(32, 70, true);

            // MOVEMENT CONTROLS JUMP AND SPRINT
            mp.game.controls.disableControlAction(0, 22, true);
            mp.game.controls.disableControlAction(0, 102, true);
            mp.game.controls.disableControlAction(0, 258, true);
            mp.game.controls.disableControlAction(0, 259, true);
            mp.game.controls.disableControlAction(0, 350, true);

            mp.game.controls.disableControlAction(0, 137, true);
        
    }
});

mp.events.add("ResetWounded", () => {
    mp.game.graphics.stopScreenEffect('DrugsDrivingIn');
    toggleCrawl(false);

    if(global.chaseRunning && mp.players.local.getVariable("Team") >= 0) 
    {
        mp.players.local.setInvincible(false);
        mp.events.callRemote("SetInvincible", false);
        mp.game.player.setInvincible(false);
    }

    if(woundedTimer != null) 
    {
        clearInterval(woundedTimer);
        woundedTimer = null;
    }
});

mp.events.add('setWounded', (state, idToPass) => 
{
    // if its trying to set the same state but the state isnt 0 [revive or clearing vars]
    if (state == mp.players.local.getVariable("Wounded") && state != 0){
        return;
    }
    if (!global.chaseRunning && state != 0){
        return;
    }
    // if the player is no longer in a team and it tries to set them wounded [but not 0]
    if ((mp.players.local.getVariable("Team") != 1 && mp.players.local.getVariable("Team") != 0) && state != 0){
        return;
    }
    // if the player is already in crawl mode and it tries to set them to state 1 [ragdoll] return
    if(mp.players.local.getVariable("Wounded") == 2 && state == 1){
        return;
    }
    if(mp.players.local.isDead()){
        return;
    }

    if(state == 1) // Pre-wounded (beginning of wounded mode) --ragdolling the player and waiting 3 secs before giving ability to crawl
    {
        if(woundedTimer != null)
        {
            clearInterval(woundedTimer);
            woundedTimer = null;
        }

        woundedTimeLeft = 60;

        woundedTimer = setInterval(() => {

            if(!global.chaseRunning || mp.players.local.getVariable("Wounded") == 0 || mp.players.local.getVariable("Team") < 0) 
            {
                clearInterval(woundedTimer);
                woundedTimer = null;
                return;
            }

            if(woundedTimeLeft > 0) 
            {
                woundedTimeLeft--;
                if(woundedTimeLeft % 5 == 0) // Every 5 seconds. Please. No event spams.
                {
                    mp.events.callRemote("WoundedTimerTick", woundedTimeLeft);
                }
            }
            else
            {
                mp.events.callRemote("WoundedTimerTick", 0);
            }
        }, 1000);

        if(!mp.players.local.isInAnyVehicle(false))
        {
            mp.players.local.setToRagdoll(4000, 4000 * 2, 0, true, true, true);

            setTimeout(() => {
                if(global.chaseRunning && mp.players.local.getVariable("Team") >= 0 && mp.players.local.getVariable("Wounded") != 0 && !mp.players.local.isDead())
                {
                    mp.events.callLocal("setWounded", 2, -1);
                }
            }, 4030);
        }
        else 
        {
            mp.events.callRemote("play_anim", 'anim@veh@lowrider@std@ds@arm@base', 'die', 2, 8.0, 0, -1, 0, false, false, false);
        }
        mp.game.player.setInvincible(true);
        mp.events.callRemote("SetInvincible", true);
        mp.game.player.setInvincible(true);
        setTimeout(() => {
            if(global.chaseRunning && mp.players.local.getVariable("Team") >= 0 && mp.players.local.getVariable("Wounded") != 0 && !mp.players.local.isDead())
            {
                mp.events.callLocal("setWounded", 2, -1);
            }
        }, 2000);
        mp.events.callRemote("play_anim", "amb@world_human_push_ups@male@exit", "exit", 2, 8.0, 0, -1, 0, false, false, false);
        //mp.game.graphics.startScreenEffect('DrugsDrivingIn', -1, true);
    }
    else if(state == 2) // As wounded player goes into wounded state
    {
        if(mp.players.local.getVariable("Wounded") != 1 || !global.chaseRunning || mp.players.local.getVariable("Team") < 0 || mp.players.local.isDead()) return;
        
        mp.players.local.setInvincible(false);
        mp.events.callRemote("SetInvincible", false);
        mp.game.player.setInvincible(false);
        if(!mp.players.local.isInAnyVehicle(false))
        {
            mp.players.local.clearDrivebyTaskUnderneathDrivingTask();
            mp.events.callRemote("clear_tasks", 1);
            mp.events.callRemote("play_anim", 'move_crawlprone2crawlfront', 'front', 2, 8.0, 0, -1, 0, false, false, false);
            toggleCrawl(true);   
        }   
        else{
            mp.events.callRemote("play_anim", 'anim@veh@lowrider@std@ds@arm@base', 'die', 2, 8.0, 0, -1, 0, false, false, false);
        }
    }
    else if(state == 0) // DISABLE WOUNDED
    {
        mp.events.callLocal("ResetWounded");
    }

    mp.events.callRemote("SetPlayerWounded", state, idToPass);
});

function handleCrawlControls() 
{
    if(!mp.players.local) return;
    if (!mp.players.local.crawl) return;
    if(global.chatStatus) return;
    let dict = 'move_crawl';
    let rotation = mp.players.local.getRotation(2);
    mp.game.controls.disableControlAction(0, 32, true);
    mp.game.controls.disableControlAction(0, 33, true);
    mp.game.controls.disableControlAction(0, 34, true);
    mp.game.controls.disableControlAction(0, 35, true);
    if(mp.game.controls.isDisabledControlPressed(0, 34))
    {
        mp.players.local.setRotation(rotation.x, rotation.y, rotation.z + 0.5, 2, true);
    }
    if(mp.game.controls.isDisabledControlPressed(0, 35))
    {
        mp.players.local.setRotation(rotation.x, rotation.y, rotation.z - 0.5, 2, true);
    }
    if(mp.game.controls.isDisabledControlPressed(0, 32))
    {
        if (crawlAnim === ('onfront_fwd' || 'onfront_bwd') || crawlAnimTimeout) return;

        crawlAnim = 'onfront_fwd';

        let timer = mp.game.entity.getEntityAnimDuration('move_crawl', crawlAnim);

        mp.game.streaming.requestAnimDict(dict);

        mp.players.local.taskPlayAnim(dict, crawlAnim, 8.0, 1000, -1, 2, 0, false, false, false);

        crawlAnimTimeout = setTimeout(() => {
            crawlAnim = undefined;
            crawlAnimTimeout = undefined;
        }, (timer - 0.1) * 1000);
    }
    if(mp.game.controls.isDisabledControlPressed(0, 33))
    {
        if (crawlAnim === ('onfront_fwd' || 'onfront_bwd') || crawlAnimTimeout) return;
        crawlAnim = 'onfront_bwd';

        let timer = mp.game.entity.getEntityAnimDuration('move_crawl', crawlAnim);

        mp.game.streaming.requestAnimDict(dict);

        mp.players.local.taskPlayAnim(dict, crawlAnim, 8.0, 1000, -1, 2, 0, false, false, false);

        crawlAnimTimeout = setTimeout(() => {
            crawlAnim = undefined;
            crawlAnimTimeout = undefined;
        }, (timer - 0.1) * 1000);
    }
}

function toggleCrawl(option){
    if(option == true){
        if(mp.players.local.crawl){
            mp.players.local.crawl = false;
            clearInterval(crawlInterval);
            crawlInterval = null;
            mp.players.local.clearTasks();
            //player.clearSecondaryTask()

            toggleCrawl(option);
        }
        else{
            crawlInterval = setInterval(handleCrawlControls, 0);
            mp.players.local.crawl = true;
            mp.game.streaming.requestAnimDict('move_crawlprone2crawlfront');

            mp.players.local.taskPlayAnim(
                'move_crawlprone2crawlfront',
                'front',
                8.0,
                1000,
                -1,
                2,
                0,
                false,
                false,
                false
            );
        }
    }
    else{
        if(mp.players.local.crawl){
            mp.players.local.crawl = false;
            clearInterval(crawlInterval);
            crawlInterval = null;
            mp.players.local.clearTasks();
            //player.clearSecondaryTask()
        }       
    }
}

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

let lastRegisteredShotTime = 0;
let shotsToRegister = [];
let triggerMultipleShotsStamp = 0;
mp.events.add("render", () => {
    if(triggerMultipleShotsStamp === 0) return;
    if(Date.now() < triggerMultipleShotsStamp) return;
    if(shotsToRegister.length <= 0) return;

    var weapon = shotsToRegister[0].weapon;
    var totalDmg = 0;
    var target = shotsToRegister[0].target;
    var bone = shotsToRegister[0].bone;
    for(var i = 0 ; i < shotsToRegister.length ; i++){
        totalDmg += shotsToRegister[i].damage;
    }

    mp.events.callRemoteUnreliable("PlayerShot_SyncHealth", target, target.getHealth(), totalDmg, target.getArmour(), weapon.toString(), bone);  
    //mp.events.callRemoteUnreliable("ServerConsoleOutput", `${mp.players.local.name} | Added up total damage: ${totalDmg} from the fast shots (${shotsToRegister.length} total)`);           

    shotsToRegister = [];
    triggerMultipleShotsStamp = 0;
    lastRegisteredShotTime = Date.now();
});

mp.events.add("fakeDeathToRespawn", () => {
    mp.game.player.setInvincible(true);
    mp.players.local.setInvincible(true);
    mp.events.callRemote("SetInvincible", true);
    mp.players.local.setToRagdoll(10000, 10000, 0, false, false, false);
});

const WeaponDamageData = [
    {
        categoryId:0,
        category:"Less-Lethal",
        torsoDamage:5,
        limbDamage:5,
        headDamage:5,
        weapons:[1432025498, 911657153] // Reserved for weapons that don't deal / barely deal damage, like taser and beanbag
        // Pump Shotgun Mk2, Stun Gun
    },
    {
        categoryId:1,
        category:"Handguns",
        torsoDamage:26,
        limbDamage:20,
        headDamage:52,
        weapons:[453432689, 3219281620, 1593441988, 584646201, 3218215474, 2285322324, 3523564046, 137902532, 465894841, 727643628] 
        // Pistol, Pistol MK2, Combat Pistol, AP Pistol, SNS Pistol, SNS Pistol MK2, Heavy Pistol, Vintage Pistol, Pistol XM3, Ceramic Pistol
    },
    {
        categoryId:1,
        category:"Handguns",
        torsoDamage:40,
        limbDamage:35,
        headDamage:70,
        weapons:[3696079510, 3249783761, 3415619887, 2548703416, 2578377531, 2441047180] 
        // Marksman Pistol, Heavy Revolver, Heavy Revolver MK2, Double Action, Pistol .50, Navy Revolver
    },    
    {
        categoryId:2,
        category:"Submachine Guns",
        torsoDamage:18,
        limbDamage:15,
        headDamage:33,
        weapons:[324215364, 736523883, 171789620, 3173288789, 3675956304, 584646201, 350597077] 
        // Micro SMG, SMG, Combat PDW, Mini SMG, Machine Pistol, AP Pistol, Tactical SMG
    },   
    {
        categoryId:2,
        category:"Submachine Guns",
        torsoDamage:22,
        limbDamage:18,
        headDamage:38,
        weapons:[4024951519, 2024373456] 
        // Assault SMG, SMG MK2
    },  
    {
        categoryId:3,
        category:"Assault Rifles",
        torsoDamage:30,
        limbDamage:27,
        headDamage:60,
        weapons:[3220176749, 2210333304, 3231910285, 2132975508, 1649403952, 2634544996] 
        // Assault Rifle, Carbine Rifle, Special Carbine, Bullpup Rifle, Compact Rifle, MG
    }, 
    {
        categoryId:3,
        category:"Assault Rifles",
        torsoDamage:34,
        limbDamage:30,
        headDamage:63,
        weapons:[961495388, 4208062921, 2937143193, 2526821735, 2228681469, 2144741730, 3520460075, 1627465347] 
        // Assault Rifle MK2, Carbine Rifle MK2, Advanced Rifle, Special Carbine MK2, Bullpup Rifle MK2, Combat MG, Tactical Rifle, Gusenberg Sweeper
    }, 
    {
        categoryId:3,
        category:"Assault Rifles",
        torsoDamage:37,
        limbDamage:33,
        headDamage:63,
        weapons:[3686625920, 2636060646, 3347935668] 
        // Combat MG MK2, Military Rifle, Heavy Rifle
    },    
    {
        categoryId:4,
        category:"Shotguns",
        torsoDamage:60,
        limbDamage:50,
        headDamage:100,
        weapons:[487013001, 2017895192, 4019527611, 2640438543] 
        // Pump Shotgun, Sawed-Off Shotgun, Double Barrel Shotgun, Bullpup Shotgun
    },
    {
        categoryId:4,
        category:"Shotguns",
        torsoDamage:24,
        limbDamage:20,
        headDamage:40,
        weapons:[3800352039, 984333226, 317205821, 94989220] 
        // Assault Shotgun, Heavy Shotgun, Sweeper Shotgun, Combat Shotgun
    },
    {
        categoryId:5,
        category:"Rifles",
        torsoDamage:100,
        limbDamage:90,
        headDamage:200,
        weapons:[100416529, 205991906, 1853742572, 2828843422] 
        // Sniper Rifle, Heavy Sniper, Precision Rifle, Musket
    },
    {
        categoryId:5,
        category:"Rifles",
        torsoDamage:200,
        limbDamage:150,
        headDamage:300,
        weapons:[177293209] 
        // Heavy Sniper MK2
    },
    {
        categoryId:5,
        category:"Rifles",
        torsoDamage:80,
        limbDamage:70,
        headDamage:150,
        weapons:[3342088282, 1785463520] 
        // Marksman Rifle, Marksman Rifle MK2
    },
]

mp.events.add("outgoingDamage", (sourceEntity, targetEntity, targetPlayer, weapon, boneIndex, damage) => {
    if(sourceEntity === undefined || sourceEntity === null || sourceEntity.type !== 'player' || sourceEntity !== player) return;
    if(mp.players.local.getVariable("TasedTill") > Date.now()) return; // if currently tased

    const KEY = Date.now();
    // In case of the target being another player
    if(targetEntity !== undefined && targetEntity.type === 'player' && targetPlayer !== player){
        if(sourceEntity.getVariable("ClimbProtection") == true || targetEntity.getVariable("ClimbProtection") == true) return;
        if(targetEntity.getVariable("Invincible") == true) return;
        if(targetEntity.getHealth() <= 0) return;

        var DamageToDeal = 0;
        const damageInfo = WeaponDamageData.find(w => w.weapons.includes(weapon));
        const distance = sourceEntity.position.subtract(targetEntity.position).length();
        if(!damageInfo) DamageToDeal = 0;
        else{
            switch(boneIndex){
                case 0: DamageToDeal = damageInfo.torsoDamage;
                break;
                case 7: DamageToDeal = damageInfo.torsoDamage;
                break;
                case 8: DamageToDeal = damageInfo.torsoDamage;
                break;
                case 9: DamageToDeal = damageInfo.torsoDamage;
                break;
                case 10: DamageToDeal = damageInfo.torsoDamage;
                break;
                case 19: DamageToDeal = damageInfo.headDamage;
                break;
                case 20: DamageToDeal = 1000;
                break;
                default: DamageToDeal = damageInfo.limbDamage;
                break;
            }
            if(damageInfo.categoryId == 4){
                if(boneIndex === 20){
                    if(distance <= 10.0)
                        DamageToDeal = 1000.0;
                    else DamageToDeal = (damageInfo.headDamage*1.2);
                }    

                if(distance > 10.0 && distance < 20.0)
                    DamageToDeal *= 0.8;
                else if(distance > 20.0 && distance < 30.0)
                    DamageToDeal *= 0.5;
                else DamageToDeal *= 0.2;
            }
        }

        
        if(boneIndex === 20 && distance > 50.0){ // no instant headshots if more than 50m away from target
            DamageToDeal = 50;

            if(damageInfo)
                DamageToDeal = damageInfo.headDamage;
        }

        if(global.isAntiPeekOn){
            if(boneIndex === 20){
                DamageToDeal = 50;
                if(damageInfo)
                    DamageToDeal = damageInfo.headDamage;

                if(distance < 7.5){
                    DamageToDeal = 1000;
                }
            }
            else{
                DamageToDeal *= 0.5; // lower damage by 50% if anti-peek is active
            }


        }
        

        if(DamageToDeal > 0){
            if(Date.now() - lastRegisteredShotTime > 20 && damageInfo.categoryId !== 4){
                mp.events.callRemoteUnreliable("PlayerShot_SyncHealth", targetPlayer, targetPlayer.getHealth(), DamageToDeal, targetPlayer.getArmour(), weapon.toString(), boneIndex);            
                lastRegisteredShotTime = Date.now();
            }
            else{
                shotsToRegister.push({weapon:weapon, damage:DamageToDeal, target:targetPlayer, bone:boneIndex});
                triggerMultipleShotsStamp = Date.now()+30;
            }
        }

        /* Other checks to prevent/reduce visual bugs such as players dying on shooter's screen */
        
        if(targetEntity.getVariable("IsBeingCuffed") === true || targetEntity.getVariable("IsCuffed") === true)
        {
            mp.events.callRemote(`ServerConsoleOutput`, `[K:${KEY}] Target was cuffed, cancelling damage`);
            damage = 0;
            mp.game.weapon.setCurrentDamageEventAmount(0);
            mp.game.weapon.cancelCurrentDamageEvent();
            return true;
        }
        if(targetEntity.getVariable("Team") != -1 && player.getVariable("Team") != -1)
            {
                if(sourceEntity.getVariable("Team") == targetEntity.getVariable("Team"))
                {
                    mp.events.callRemote(`ServerConsoleOutput`, `[K:${KEY}] Same team, cancelling damage`);
                    damage = 0;
                    mp.game.weapon.setCurrentDamageEventAmount(0);
                    mp.game.weapon.cancelCurrentDamageEvent();
                    return true;
                }

                if(sourceEntity.getVariable("Team") == 1 && targetEntity.getVariable("Team") == 0 && global.chaseRunning)
                {
                    if(targetEntity.getVariable("HasShot") == false) // Shot a fugitive that hasn't shot.
                    {
                        mp.events.callRemote(`ServerConsoleOutput`, `[K:${KEY}] Fugi hasnt shot, shot by cop`);
                        if(weapon != 911657153){
                           
                            mp.events.callRemote(`ServerConsoleOutput`, `[K:${KEY}] Weapon wasnt taser, cancelling`);
                            damage = 0;
                            mp.game.weapon.setCurrentDamageEventAmount(0);
                            mp.game.weapon.cancelCurrentDamageEvent();
                            return true; // cant damage dem at alll if thy havent shot unless taser
                        }

                        if(global.chaseTimeCur < 60){  
                            mp.events.callRemote(`ServerConsoleOutput`, `[K:${KEY}] Chase time current was less than 60 cancelling and tase protect`);                         
                            damage = 0;
                            mp.game.weapon.setCurrentDamageEventAmount(0);
                            mp.game.weapon.cancelCurrentDamageEvent();
                            return true;
                        }

                        if(targetEntity.getVariable("TaseProtection") == true && targetEntity.getVariable("Team") == 0)
                        {        
                            mp.events.callRemote(`ServerConsoleOutput`, `[K:${KEY}] Tase protection active`);           
                            damage = 0;
                            mp.game.weapon.setCurrentDamageEventAmount(0);
                            mp.game.weapon.cancelCurrentDamageEvent();
                            mp.events.callLocal("HUDError", "Target has active Tase Protection! You can't tase them yet.");
                            return true;
                        }

                        if(targetEntity.getVariable("Team") == 0 && targetEntity.getVariable("HasShot") == true) {
                            damage = 0;
                            mp.game.weapon.setCurrentDamageEventAmount(0);
                            mp.game.weapon.cancelCurrentDamageEvent();
                            mp.events.callRemote(`ServerConsoleOutput`, `[K:${KEY}] target entity was 0 and their shot was true, wtf, taser`);
                            return true;
                        }

                        if((targetEntity.isInAnyVehicle(false) == true || 
                            global.calcDist(targetEntity.position, player.position) > 7.5)) {
                            damage = 0;
                            mp.game.weapon.setCurrentDamageEventAmount(0);
                            mp.game.weapon.cancelCurrentDamageEvent();
                            mp.events.callLocal("HUDError", "You're too far! Must be less than 7.5m away from target.");
                            return true; // Stun Gun no shot if more than 10 meters or target in car.
                        }
                    }
                    else{
                        if(weapon == 911657153){ // Attempt to tase a fugitive with shot tag.
                            damage = 0;
                            mp.game.weapon.setCurrentDamageEventAmount(0);
                            mp.game.weapon.cancelCurrentDamageEvent();
                            mp.events.callLocal("HUDError", "You can't tase players who have the Shot Tag!");
                            mp.events.callRemote(`ServerConsoleOutput`, `[K:${KEY}] tased with shot tag`);
                            return true;
                        }
                    }
                }
                if(sourceEntity.getVariable("Team") == 0 && targetEntity.getVariable("Team") == 1 && sourceEntity.getVariable("HasShot") == false
                    && isMeleeWeapon(weapon) && global.chaseRunning)
                {
                    mp.events.callRemote("GivePlayerShotTag");
                }

                if(sourceEntity.getVariable("Team") != targetEntity.getVariable("Team"))
                {
                    if(targetEntity.getVariable("Wounded") == 1)
                    {
                        damage = 0;
                        mp.game.weapon.setCurrentDamageEventAmount(0);
                        mp.game.weapon.cancelCurrentDamageEvent();
                        mp.events.callRemote(`ServerConsoleOutput`, `[K:${KEY}] wounded is 1`);
                        return true;
                    }
                    if(targetEntity.getVariable("Wounded") == 2)
                    {
                        damage = 0;
                        mp.game.weapon.setCurrentDamageEventAmount(0);
                        mp.game.weapon.cancelCurrentDamageEvent();
                        mp.events.callRemote("opsp", targetPlayer, boneIndex, mp.players.local.weapon);
                        mp.events.callRemote(`ServerConsoleOutput`, `[K:${KEY}] wounded is 2`);
                        return true;
                    }
                }
            }
    }
    else if(targetEntity !== undefined && targetEntity.type === 'vehicle'){
        if(targetEntity.getVariable("Team") !== undefined && player.getVariable("Team") != -1)
        {
            let breakShot = false;
            var vehicleOccupants = 0;

            if(weapon == 911657153)
            {
                breakShot = true;
                mp.events.callLocal("HUDError", "Taser does not work for vehicles!!");
            }

            if(targetEntity.getVariable("Team") == player.getVariable("Team"))
            {
                breakShot = true; // Break team-kill car by default
                
                mp.players.streamed.forEach(occupant => {
                    if(occupant.isInVehicle(targetEntity.handle, false)) {
                        vehicleOccupants++;
                        if(occupant.getVariable("Team") != player.getVariable("Team")) {
                            breakShot = false; // Don't break shot if there's a fugi in or opposite team member!
                        }
                    }
                });

                if(mp.players.local.vehicle != undefined && mp.players.local.vehicle.handle == targetEntity.handle) breakShot = false;

                if(vehicleOccupants == 0) breakShot = false;

                if(breakShot) mp.events.callLocal("HUDError", "Team-killing is not allowed!");
            }
            else if(targetEntity.getVariable("Team") == 0 && player.getVariable("Team") == 1 && weapon != 911657153 && global.chaseRunning)
            {
                breakShot = true; // Break innocent car shots by default.

                mp.players.streamed.forEach(occupant => {
                    if(occupant.isInVehicle(targetEntity.handle, false)) {
                        vehicleOccupants++;
                        if(occupant.getVariable("Team") == 0 && occupant.getVariable("HasShot") == true && !occupant.isDead()) {
                           breakShot = false; // Don't break shot if there's a fugitive that shot in!
                        }
                    }
                });

                if(vehicleOccupants == 0) breakShot = false;

                if(breakShot) mp.events.callLocal("HUDError", "Deathmatching is not allowed - fugitive doesn't have the Shot Tag!");
            }

            if(targetEntity.getVariable("Team") == 0 && player.getVariable("Team") == 1 && weapon == 911657153)
            {
                breakShot = true; 
                mp.events.callLocal("HUDError", "Taser does not work for vehicles!");
            }

            if(breakShot == true){
                damage = 0;
                mp.game.weapon.setCurrentDamageEventAmount(0);
                mp.game.weapon.cancelCurrentDamageEvent();
                return true;
            }
        }
    }
});
/*
mp.events.add("outgoingDamage", (sourceEntity, targetEntity, targetPlayer, weapon, boneIndex, damage) => {
    if(sourceEntity !== undefined && sourceEntity !== null && sourceEntity.type === 'player' && sourceEntity == player)
    {
        if(targetEntity !== undefined && targetEntity !== null && targetEntity.type === 'player')
        {
            if(sourceEntity.getVariable("ClimbProtection") == true || targetEntity.getVariable("ClimbProtection") == true) return true;
            if(targetPlayer.getVariable("Invincible") === true) return;
            if(targetPlayer.getHealth() <= 0) return;

            let DAMAGE_TO_DEAL = 0;

            let gunData = global.weaponData.find(w => Number(w.Hash) == weapon);
            if(gunData != undefined && (gunData.Type == "Assault Rifles" || gunData.Type == "Machine Guns")){
                let nerfedDmg = (damage-10);
                damage = nerfedDmg;
                mp.game.weapon.setCurrentDamageEventAmount(nerfedDmg);
                //global.LogPlayerDamage(targetEntity, weapon, nerfedDmg, boneIndex);
                
                //showDamageIndicatorOnPosition(nerfedDmg, targetEntity, boneIndex);

                DAMAGE_TO_DEAL = nerfedDmg;
                damage = 0;
                mp.game.weapon.setCurrentDamageEventAmount(0);
                mp.game.weapon.cancelCurrentDamageEvent();
            }
            else{
                let nerfedDmg = (damage-5);
                if(nerfedDmg <= 0) nerfedDmg = 1;
                damage = nerfedDmg;
                mp.game.weapon.setCurrentDamageEventAmount(nerfedDmg);
                //global.LogPlayerDamage(targetEntity, weapon, nerfedDmg, boneIndex);
                
                //showDamageIndicatorOnPosition(nerfedDmg, targetEntity, boneIndex);

                DAMAGE_TO_DEAL = nerfedDmg;
                damage = 0;
                mp.game.weapon.setCurrentDamageEventAmount(0);
                mp.game.weapon.cancelCurrentDamageEvent();
            }
            if(weapon == 2441047180){ // navy revolver nerf
                const vec1 = sourceEntity.position;
                const vec2 = targetEntity.position;

                const distance = vec1.subtract(vec2).length();

                if(distance < 9.0){
                    damage = 75;
                    mp.game.weapon.setCurrentDamageEventAmount(75);
                    //global.LogPlayerDamage(targetEntity, weapon, 75, boneIndex);
                
                    //showDamageIndicatorOnPosition(75, targetEntity, boneIndex);

                    DAMAGE_TO_DEAL = 75;
                    damage = 0;
                    mp.game.weapon.setCurrentDamageEventAmount(0);
                    mp.game.weapon.cancelCurrentDamageEvent();
                }
                else if(distance >= 9.0 && distance < 18.0){
                    damage = 50;
                    mp.game.weapon.setCurrentDamageEventAmount(50);
                    //global.LogPlayerDamage(targetEntity, weapon, 50, boneIndex);
                
                    //showDamageIndicatorOnPosition(50, targetEntity, boneIndex);

                    DAMAGE_TO_DEAL = 50;
                    damage = 0;
                    mp.game.weapon.setCurrentDamageEventAmount(0);
                    mp.game.weapon.cancelCurrentDamageEvent();
                }
                else{
                    damage = 25;
                    mp.game.weapon.setCurrentDamageEventAmount(25);
                    //global.LogPlayerDamage(targetEntity, weapon, 25, boneIndex);
                
                    //showDamageIndicatorOnPosition(25, targetEntity, boneIndex);

                    DAMAGE_TO_DEAL = 25;
                    damage = 0;
                    mp.game.weapon.setCurrentDamageEventAmount(0);
                    mp.game.weapon.cancelCurrentDamageEvent();
                }
            }
            if(weapon == 3800352039){ // assault shotgun nerf
                const vec1 = sourceEntity.position;
                const vec2 = targetEntity.position;

                const distance = vec1.subtract(vec2).length();
                if(distance < 9.2681523185659){
                    damage = 10;
                    mp.game.weapon.setCurrentDamageEventAmount(10);
                    //global.LogPlayerDamage(targetEntity, weapon, 10, boneIndex);
                
                    //showDamageIndicatorOnPosition(10, targetEntity, boneIndex);
                    DAMAGE_TO_DEAL = 10;
                    damage = 0;
                    mp.game.weapon.setCurrentDamageEventAmount(0);
                    mp.game.weapon.cancelCurrentDamageEvent();
                }
                else if(distance > 9.2681523185659 && distance < 19.267101852897){
                    damage = 7;
                    mp.game.weapon.setCurrentDamageEventAmount(7);
                    //global.LogPlayerDamage(targetEntity, weapon, 7, boneIndex);
                
                    //showDamageIndicatorOnPosition(7, targetEntity, boneIndex);
                    DAMAGE_TO_DEAL = 7;
                    damage = 0;
                    mp.game.weapon.setCurrentDamageEventAmount(0);
                    mp.game.weapon.cancelCurrentDamageEvent();
                }
                else{
                    damage = 0;
                    mp.game.weapon.setCurrentDamageEventAmount(0);
                    //global.LogPlayerDamage(targetEntity, weapon, 0, boneIndex);
                    damage = 0;
                    mp.game.weapon.setCurrentDamageEventAmount(0);
                    mp.game.weapon.cancelCurrentDamageEvent();
                }
            }
            else if(weapon == 984333226){  // heavy shotgun nerf - shoots 1 pellet so it gets more dmg
                const vec1 = sourceEntity.position;
                const vec2 = targetEntity.position;

                const distance = vec1.subtract(vec2).length();
                if(distance < 9.2681523185659){
                    damage = 37;
                    mp.game.weapon.setCurrentDamageEventAmount(37);
                    //global.LogPlayerDamage(targetEntity, weapon, 37, boneIndex);
                
                    //showDamageIndicatorOnPosition(37, targetEntity, boneIndex);
                    DAMAGE_TO_DEAL = 37;
                    damage = 0;
                    mp.game.weapon.setCurrentDamageEventAmount(0);
                    mp.game.weapon.cancelCurrentDamageEvent();
                }
                else if(distance > 9.2681523185659 && distance < 19.267101852897){
                    damage = 28;
                    mp.game.weapon.setCurrentDamageEventAmount(28);
                    //global.LogPlayerDamage(targetEntity, weapon, 28, boneIndex);
                
                    //showDamageIndicatorOnPosition(28, targetEntity, boneIndex);
                    DAMAGE_TO_DEAL = 28;
                    damage = 0;
                    mp.game.weapon.setCurrentDamageEventAmount(0);
                    mp.game.weapon.cancelCurrentDamageEvent();
                }
                else{
                    damage = 0;
                    mp.game.weapon.setCurrentDamageEventAmount(0);
                    //global.LogPlayerDamage(targetEntity, weapon, 0, boneIndex);
                    damage = 0;
                    mp.game.weapon.setCurrentDamageEventAmount(0);
                    mp.game.weapon.cancelCurrentDamageEvent();
                }
            }

            if(DAMAGE_TO_DEAL > 0){
                if(Date.now() - lastRegisteredShotTime > 20 && gunData.Type !== "Shotguns"){
                    mp.events.callRemoteUnreliable("PlayerShot_SyncHealth", targetPlayer, targetPlayer.getHealth(), DAMAGE_TO_DEAL, targetPlayer.getArmour(), weapon.toString(), boneIndex);            
                    lastRegisteredShotTime = Date.now();
                }
                else{
                    shotsToRegister.push({weapon:weapon, damage:DAMAGE_TO_DEAL, target:targetPlayer, bone:boneIndex});

                    triggerMultipleShotsStamp = Date.now()+30;
                    //mp.events.callRemoteUnreliable("ServerConsoleOutput", `${mp.players.local.name} is dealing damage too fast! Adding to shotsToRegister (${shotsToRegister.length})`);
                }
            }
            
            if(targetEntity.getVariable("Team") != -1 && player.getVariable("Team") != -1)
            {
                if(sourceEntity.getVariable("Team") == targetEntity.getVariable("Team"))
                {
                    damage = 0;
                    mp.game.weapon.setCurrentDamageEventAmount(0);
                    mp.game.weapon.cancelCurrentDamageEvent();
                    return true;
                }

                if(sourceEntity.getVariable("Team") == 1 && targetEntity.getVariable("Team") == 0 && global.chaseRunning)
                {
                    if(targetEntity.getVariable("HasShot") == false) // Shot a fugitive that hasn't shot.
                    {
                        if(weapon != 911657153){
                           
                            damage = 0;
                            mp.game.weapon.setCurrentDamageEventAmount(0);
                            mp.game.weapon.cancelCurrentDamageEvent();
                            return true; // cant damage dem at alll if thy havent shot unless taser
                        }

                        if(global.chaseTimeCur < 60){                           
                            damage = 0;
                            mp.game.weapon.setCurrentDamageEventAmount(0);
                            mp.game.weapon.cancelCurrentDamageEvent();
                            return true;
                        }

                        if(targetEntity.getVariable("TaseProtection") == true && targetEntity.getVariable("Team") == 0)
                        {                   
                            damage = 0;
                            mp.game.weapon.setCurrentDamageEventAmount(0);
                            mp.game.weapon.cancelCurrentDamageEvent();
                            mp.game.graphics.notify("~r~Cannot tase! Target exiting vehicle!");
                            return true;
                        }

                        if(targetEntity.getVariable("Team") == 0 && targetEntity.getVariable("HasShot") == true) {
                            damage = 0;
                            mp.game.weapon.setCurrentDamageEventAmount(0);
                            mp.game.weapon.cancelCurrentDamageEvent();
                            return true;
                        }

                        if((targetEntity.isInAnyVehicle(false) == true || 
                            global.calcDist(targetEntity.position, player.position) > 7.5)) {
                            damage = 0;
                            mp.game.weapon.setCurrentDamageEventAmount(0);
                            mp.game.weapon.cancelCurrentDamageEvent();
                            mp.game.graphics.notify("~r~Taser out of reach, get closer!");
                            return true; // Stun Gun no shot if more than 10 meters or target in car.
                        }
                    }
                    else{
                        if(weapon == 911657153){ // Attempt to tase a fugitive with shot tag.
                            damage = 0;
                            mp.game.weapon.setCurrentDamageEventAmount(0);
                            mp.game.weapon.cancelCurrentDamageEvent();
                            mp.game.graphics.notify("~r~Target has shot, cannot tase them!");
                            return true;
                        }
                    }
                }
                if(sourceEntity.getVariable("Team") == 0 && targetEntity.getVariable("Team") == 1 && sourceEntity.getVariable("HasShot") == false
                    && isMeleeWeapon(weapon) && global.chaseRunning)
                {
                    mp.events.callRemote("GivePlayerShotTag");
                }

                if(sourceEntity.getVariable("Team") != targetEntity.getVariable("Team"))
                {
                    if(targetEntity.getVariable("Wounded") == 1)
                    {
                        damage = 0;
                        mp.game.weapon.setCurrentDamageEventAmount(0);
                        mp.game.weapon.cancelCurrentDamageEvent();
                        return true;
                    }
                    if(targetEntity.getVariable("Wounded") == 2)
                    {
                        damage = 0;
                        mp.game.weapon.setCurrentDamageEventAmount(0);
                        mp.game.weapon.cancelCurrentDamageEvent();
                        mp.events.callRemote("opsp", targetPlayer, boneIndex, mp.players.local.weapon);
                        return true;
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

                if(weapon == 911657153)
                {
                    breakShot = true;
                    mp.console.logInfo("Taser does not work for vehicles. (2)");
                }

                if(targetEntity.getVariable("Team") == player.getVariable("Team"))
                {
                    breakShot = true; // Break team-kill car by default
                    
                    mp.players.streamed.forEach(occupant => {
                        if(occupant.isInVehicle(targetEntity.handle, false)) {
                            vehicleOccupants++;
                            if(occupant.getVariable("Team") != player.getVariable("Team")) {
                                breakShot = false; // Don't break shot if there's a fugi in or opposite team member!
                            }
                        }
                    });

                    if(mp.players.local.vehicle != undefined && mp.players.local.vehicle.handle == targetEntity.handle) breakShot = false;

                    if(vehicleOccupants == 0) breakShot = false;

                    if(breakShot) mp.game.graphics.notify("~r~Do not teamkill!");
                }
                else if(targetEntity.getVariable("Team") == 0 && player.getVariable("Team") == 1 && weapon != 911657153 && global.chaseRunning)
                {
                    breakShot = true; // Break innocent car shots by default.

                    mp.players.streamed.forEach(occupant => {
                        if(occupant.isInVehicle(targetEntity.handle, false)) {
                            vehicleOccupants++;
                            if(occupant.getVariable("Team") == 0 && occupant.getVariable("HasShot") == true && !occupant.isDead()) {
                               breakShot = false; // Don't break shot if there's a fugitive that shot in!
                            }
                        }
                    });

                    if(vehicleOccupants == 0) breakShot = false;

                    if(breakShot) mp.game.graphics.notify("~r~Do not deathmatch!");
                }

                if(targetEntity.getVariable("Team") == 0 && player.getVariable("Team") == 1 && weapon == 911657153)
                {
                    breakShot = true;
                    mp.console.logInfo("Taser does not work for vehicles.");
                }

                if(breakShot == true){
                    damage = 0;
                    mp.game.weapon.setCurrentDamageEventAmount(0);
                    mp.game.weapon.cancelCurrentDamageEvent();
                    return true;
                }
            }
        }
    }
    else if(targetEntity !== null && targetEntity !== undefined && targetEntity.type === 'vehicle'){
        if(sourceEntity !== null && sourceEntity !== undefined && sourceEntity.type === 'player' && sourceEntity == player){
            let nerfedDmg = Math.floor(damage/2);
            damage = nerfedDmg;
            mp.game.weapon.setCurrentDamageEventAmount(nerfedDmg);
        }
    }
});
*/
mp.events.add("entplayanim", (entity, animDictionary, animName, animFlag, speed = 8.0, speedMul = 0.0, duration = -1, playbackRate = 0.0, lockX = false, lockY = false, lockZ = false) => {
    if(entity.handle === 0) return;

    if(entity.remoteId == mp.players.local.remoteId)
    {
        mp.events.callRemote("UpdateLastAnim", animDictionary, animName);
    }

	mp.game.streaming.requestAnimDict(animDictionary);
	new Promise(() => {
	var timer = setInterval(() => {
		if(mp.game.streaming.hasAnimDictLoaded(animDictionary)) {
			clearInterval(timer);
            timer = null;
			resolvePlayAnim(entity, animDictionary, animName, animFlag, speed, speedMul, duration, playbackRate, lockX, lockY, lockZ);
		}
	}, 100);
});
});

mp.events.add("entcleartasks", (entity, immediately) => {
    if(entity.handle === 0) return;

    if(immediately == 1) entity.clearTasksImmediately();
    else entity.clearTasks();
});

function resolvePlayAnim(entity, animDictionary, animName, animFlag, speed = 8.0, speedMul = 0.0, duration = -1, playbackRate = 0.0, lockX = false, lockY = false, lockZ = false)
{
    if(entity == null || !entity.doesExist()) return;
	entity.taskPlayAnim(animDictionary, animName, speed, speedMul, duration, animFlag, playbackRate, lockX, lockY, lockZ);
}

function getRandomInt(min, max) {
    min = Math.ceil(min);
    max = Math.floor(max);
    return Math.floor(Math.random() * (max - min) + min); //The maximum is exclusive and the minimum is inclusive
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


mp.events.add("PopVehicleTires", () => {
    let vehicle = mp.players.local.vehicle;
    if(!vehicle) return;
    vehicle.setTyreBurst(0, false, 1000);
    vehicle.setTyreBurst(1, false, 1000);
    vehicle.setTyreBurst(4, false, 1000);
    vehicle.setTyreBurst(5, false, 1000);
    mp.game.audio.playSoundFrontend(-1, 'Drill_Pin_Break', 'DLC_HEIST_FLEECA_SOUNDSET', true);
    // vehicle.setBurnout(true);
    // setTimeout(_ => {
    //     if(vehicle)
    //         vehicle.setBurnout(false);
    // }, 500);
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
    "Syncglobal.ServerSettings": (onfootcollisions, cameraUpdateTime) => {
        global.ServerSettings.OnfootCollisions = onfootcollisions;
        global.ServerSettings.CameraUpdateTime = cameraUpdateTime;
    },
    "ToggleClientRadio": () => {
        global.radioToggle = !global.radioToggle;

        mp.storage.data.radiotoggle = global.radioToggle;

        mp.storage.flush();

        if(!global.radioToggle) mp.gui.chat.push("!{#FF6347}[!] !{#FFFFFF}Vehicle radio toggled OFF.");
        else mp.gui.chat.push("!{#FF6347}[!] !{#FFFFFF}Vehicle radio toggled ON.");
    },
    "SetInVehEx": (vehicle, vehseat) => {
        mp.players.local.setIntoVehicle(vehicle.handle, vehseat);
    },
    "ToggleHud": (show) => {
        if(show)
        {
            if(mp.players.local.getVariable("InLobby") == -4 && derbyText != null) {
                derbyText.visible = true;
            }
        }
        else{

            if(derbyText != null) {
                derbyText.visible = false;
            }
        }
    },
    "ClearTaskNotFalling": () => {
        if(!mp.players.local.isFalling()) mp.players.local.clearTasksImmediately();
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
     
        global.specTarget = target;   
        global.specState = true;
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
    "PlaySoundFrontend": (soundName, setName) => {
        mp.game.audio.playSoundFrontend(-1, soundName, setName, true);
    },

    "StopSpectating": () => {
        mp.game.invoke("0x423DE3854BB50894", false, mp.players.local.handle); //NETWORK_SET_IN_SPECTATOR_MODE
        global.specTarget = null;
        global.specState = false;
    },

    "setToRagdoll": (time1, time2, ragdollType, p4, p5, p6) => {
        player.clearTasksImmediately();
        player.setToRagdoll(time1, time2, ragdollType, p4, p5, p6);
    },
    "SetJailCounter": (time) => {
        global.currentJailTime = time;
        global.lastJailUpdate = 10;
    },
    "SetMuteCounter": (time) => {
        global.currentMuteTime = time;
        global.lastMuteUpdate = 10;
    },

    "SetActorInvulnerable": (toggle) => {
        if(toggle)
        {
            player.setInvincible(true);
            mp.events.callRemote("SetInvincible", true);
        }
        else
        {
            player.setInvincible(false);
            mp.events.callRemote("SetInvincible", false);
        }
    },

    "StartChaseTimer": (iTimeCur, iTimeMax, collisionDuration, mode) => {

        global.chaseTimeCur = iTimeCur - 1;
        global.chaseTimeMax = iTimeMax;
        global.collisionTime = collisionDuration;
        global.chaseRunning = true;
        global.chaseMode = mode;
    },

    "SendNotification": (title, text, attribute, duration) => {
        mp.system.notify(title, text, attribute, duration, false);
    },

    "HideChaseTimer": () => {
        if(global.ServerUI) mp.events.callLocal("setMiniHudItemToggle", 9, false);
        global.chaseRunning = false;
    },

    "playerEnterVehicle": (vehicle, seat) => {
        dbDisabledMsg = false;
        player.seat = seat;
        mp.events.callRemote("UpdateLastVehEnterExit", Date.now());
        if(vehicle.Collisions == undefined || vehicle.Collisions == true) vehicle.setInvincible(false);

        if(!global.radioToggle && seat == -1) mp.game.invoke('0x1B9C0099CB942AC6', vehicle.handle, 'OFF');

        if(mp.game.vehicle.isThisModelAHeli(vehicle.model) || mp.game.vehicle.isThisModelAPlane(vehicle.model)) {
            mp.events.callRemote("RequestParachute", vehicle);

            if(mp.players.local.getVariable("Team") === 1){
                mp.events.callLocal("AddTooltip", "Alt", "Police Camera™", 1);
            }
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
            mp.events.callRemote("UpdateLastVehEnterExit", Date.now());
            if(Seatbelt_Enabled == true && vehicle.getClass() != 8 && vehicle.model != mp.game.joaat('policeb'))
            {
                mp.game.graphics.notify("~r~You cannot leave with seatbelt on.");
                mp.players.local.clearTasks();
                mp.players.local.setIntoVehicle(vehicle.handle, seat);
            }

            mp.events.callLocal("RemoveTooltip", "Alt");

            if(vehicle.getVariable("Engine") !== undefined)
                vehicle.setEngineOn(vehicle.getVariable("Engine"), true, true);
            else vehicle.setEngineOn(true, true, true);
        }
    },

    "InitChaseTimer": () => {
          
        if(global.ServerUI) mp.events.callLocal("setMiniHudItemToggle", 9, true);

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

    "MovePlayerCamera": (x, y, z, rx, ry, rz, time, hz1, hz2) => {
        moveCam(x, y, z, rx, ry, rz, time, hz1, hz2);
    },

    "SetDiscordStatus": (serverName, status) => {
        mp.discord.update(serverName, status);
    },

    "playerReady": (player) => {
        setTimeout(() => {            /*
            loginBrowser = mp.browsers.new("package://LoginRegister/index.html"); // Right on connect so it doesn't overlay the notifications.
            loginBrowser.active = false;
            */
            //global.ServerUI = mp.browsers.new("package://ServerUI/index.html");
            if(global.ServerUI) global.ServerUI.active = false;
            global.LastKeyPress = Date.now();
            global.AFKStage = 0;
            global.antiAFKActive = false;
            setTimeout(() => {
                if(global.ServerUI) global.ServerUI.markAsChat();
            }, 250);
            setTimeout(() => {
                if(mp.storage.data.language){
                    mp.events.callLocal("Translator:ChangeLanguage", mp.storage.data.language, true);
                    mp.events.callLocal("Translator:UpdateNametagMessagesManual");
                }
                else{
                    mp.events.callLocal("Translator:ChangeLanguage", 0, true);
                    mp.events.callLocal("Translator:UpdateNametagMessagesManual");
                }
            }, 100);
            setTimeout(() => {
                if(mp.storage.data.radio === undefined){
                    mp.storage.data.radio = {recents:[]};
                }
                if(mp.storage.data.radio.recents === undefined){
                    mp.storage.data.radio.recents = [];
                }
                mp.storage.flush();
                mp.events.callRemote("XMR_RequestStationList", JSON.stringify(mp.storage.data.radio.recents), true);
            }, 1000);
        }, 1000);


        mp.events.callRemote("playerReadyCon");
        mp.game.gxt.set('PM_PAUSE_HDR', 'Race \'N\' Chase'); // Replace map title

        setInterval(() => {
            if(mp.players.local.getVariable("pLogged") !== undefined && mp.players.local.getVariable("pLogged") == true)
            {
                mp.events.callRemote("UpdateTabbedState", mp.system.isFocused);
                mp.events.callLocal("TwoSecondsEvent");
            }
        }, 2000);
    },
});



var precisionEditorKeys = {"W":false, "A":false, "S":false, "D":false, "E":false, "Q":false, "SHIFT":false, "CTRL":false};
function precisionPositioner_handleInput(key, isDown){
    if(!global.precisionPositionerActive) return; 
    if(global.chatStatus) return;

    precisionEditorKeys = {...precisionEditorKeys, [key]:isDown};
}

mp.keys.bind(0x57, true, () => {precisionPositioner_handleInput("W", true)});
mp.keys.bind(0x57, false, () => {precisionPositioner_handleInput("W", false)});
mp.keys.bind(0x41, true, () => {precisionPositioner_handleInput("A", true)});
mp.keys.bind(0x41, false, () => {precisionPositioner_handleInput("A", false)});
mp.keys.bind(0x53, true, () => {precisionPositioner_handleInput("S", true)});
mp.keys.bind(0x53, false, () => {precisionPositioner_handleInput("S", false)});
mp.keys.bind(0x44, true, () => {precisionPositioner_handleInput("D", true)});
mp.keys.bind(0x44, false, () => {precisionPositioner_handleInput("D", false)});
mp.keys.bind(0x45, true, () => {precisionPositioner_handleInput("E", true)});
mp.keys.bind(0x45, false, () => {precisionPositioner_handleInput("E", false)});
mp.keys.bind(0x51, true, () => {precisionPositioner_handleInput("Q", true)});
mp.keys.bind(0x51, false, () => {precisionPositioner_handleInput("Q", false)});
mp.keys.bind(0x10, true, () => {precisionPositioner_handleInput("SHIFT", true)});
mp.keys.bind(0x10, false, () => {precisionPositioner_handleInput("SHIFT", false)});
mp.keys.bind(0x11, true, () => {precisionPositioner_handleInput("CTRL", true)});
mp.keys.bind(0x11, false, () => {precisionPositioner_handleInput("CTRL", false)});

mp.keys.bind(0x46, false, () => {
    if(global.precisionPositionerActive && !global.chatStatus){
        mp.events.callRemote("StopAnimPositioner");
    }
});

var precisionPositioner_animData = {};
mp.events.add("togglePrecisionPositioner", (toggle) => {
    mp.players.local.freezePosition(toggle);
    mp.players.local.setCollision(!toggle, !toggle);
    global.precisionPositionerActive = toggle;

    if(toggle && mp.players.local.getVariable("PlayedAnimation") !== "NULL"){
        const ANIMOBJECT = JSON.parse(mp.players.local.getVariable("PlayedAnimation"));
        precisionPositioner_animData = {flags:ANIMOBJECT.flags, name:ANIMOBJECT.name, library:ANIMOBJECT.library};
        mp.gui.chat.push(`[X] Library: ${precisionPositioner_animData.library} | Name: ${precisionPositioner_animData.name}`);
    }
});

var wasKeyPressedLastFrame = false;
mp.events.add('render', () => {
    if(!global.precisionPositionerActive) return;

    mp.game.controls.disableControlAction(32, 21, true);
    mp.game.controls.disableControlAction(32, 22, true);
    mp.game.controls.disableControlAction(32, 23, true);
    mp.game.controls.disableControlAction(32, 24, true);
    mp.game.controls.disableControlAction(32, 25, true);
    mp.game.controls.disableControlAction(32, 49, true);
    mp.game.controls.disableControlAction(32, 75, true);
    mp.game.controls.disableControlAction(32, 30, true);
    mp.game.controls.disableControlAction(32, 31, true);
    mp.game.controls.disableControlAction(32, 32, true);
    mp.game.controls.disableControlAction(32, 33, true);
    mp.game.controls.disableControlAction(32, 34, true);
    mp.game.controls.disableControlAction(32, 35, true);
    mp.game.controls.disableControlAction(32, 44, true);
    mp.game.controls.disableControlAction(32, 36, true);

    var position = mp.players.local.position;
    var heading = mp.players.local.getHeading();

    var positionToSet = position;
    var headingToSet = heading;

    var wasAnyKeyPressed = false;

    if(precisionEditorKeys["W"] === true){
        let radians = -mp.players.local.getHeading()*Math.PI/180;
        positionToSet.x = position.x + (0.01 * Math.sin(radians));
        positionToSet.y = position.y + (0.01 * Math.cos(radians));
        wasAnyKeyPressed = true;
    }
    if(precisionEditorKeys["S"] === true){
        let radians = -mp.players.local.getHeading()*Math.PI/180;
        positionToSet.x = position.x + (-0.01 * Math.sin(radians));
        positionToSet.y = position.y + (-0.01 * Math.cos(radians));
        wasAnyKeyPressed = true;
    }
    if(precisionEditorKeys["A"] === true){
        let radians = -(mp.players.local.getHeading()+90)*Math.PI/180;
        positionToSet.x = position.x + (0.01 * Math.sin(radians));
        positionToSet.y = position.y + (0.01 * Math.cos(radians));
        wasAnyKeyPressed = true;
    }
    if(precisionEditorKeys["D"] === true){
        let radians = -(mp.players.local.getHeading()-90)*Math.PI/180;
        positionToSet.x = position.x + (0.01 * Math.sin(radians));
        positionToSet.y = position.y + (0.01 * Math.cos(radians));
        wasAnyKeyPressed = true;
    }
    if(precisionEditorKeys["E"] === true){
        headingToSet = heading - 0.25;
        wasAnyKeyPressed = true;
    }
    if(precisionEditorKeys["Q"] === true){
        headingToSet = heading + 0.25;
        wasAnyKeyPressed = true;
    }
    if(precisionEditorKeys["CTRL"] === true){
        positionToSet.z = position.z - 0.01;
        wasAnyKeyPressed = true;
    }
    if(precisionEditorKeys["SHIFT"] === true){
        positionToSet.z = position.z + 0.01;
        wasAnyKeyPressed = true;
    }

    if(wasAnyKeyPressed){
        mp.players.local.position = positionToSet;
        mp.players.local.setHeading(headingToSet);
    }
    else{
        if(wasKeyPressedLastFrame === true){
            mp.events.callLocal("entplayanim", mp.players.local, precisionPositioner_animData.library, precisionPositioner_animData.name, precisionPositioner_animData.flags)
        }
    }

    wasKeyPressedLastFrame = wasAnyKeyPressed;
});

mp.events.add("OnPlayerLoginRegister", () => {

    setTimeout(() => {
        global.focusChat(false);
    }, 300);
    setTimeout(() => {
        setStorageDataVars();
    }, 400);
    setTimeout(() => {
        global.InitializeMDC();
        mp.game.streaming.requestAnimDict("rcmjosh4");
    }, 500);
    setTimeout(() => {
        global.InitializeWeaponEditor();
        mp.game.streaming.requestAnimDict("reaction@intimidation@1h");
    }, 600);
    setTimeout(() => {
        global.InitializeHotwireDOM();
        global.AddCustomVehiclesToSpawner()
    }, 700);
    setTimeout(() => {
        global.InitializeClothingEditorDOM();

        if(mp.storage.data.menu !== undefined && mp.storage.data.menu.myweather !== undefined)
        {
            mp.events.callRemote("SetPlayerPrivateWeather", mp.storage.data.menu.myweather);
        }

        mp.game.object.doorControl(4147641866, 2055.52, 2969.68, -67.3, true, 0, 0, 0.2);
    }, 800);

    if(global.ServerUI){
        global.ServerUI.call("hud_watermark_toggle", true);
    }
});


mp.events.add("setVoiceModeMenu", (value) => {
    mp.storage.data.menu.VoiceMode = value;
});

const animDict = "MP_SUICIDE";
const fireActionHash = mp.game.joaat("Fire");

let animCheckerHandle = undefined;
let shotFired = false;

function destroyAnimChecker() {
    animCheckerHandle.destroy();
    animCheckerHandle = undefined;
}

mp.events.add("Suicide_ApplyAnimation", (animName, animTime) => {
    if (player.handle) {
        mp.game.streaming.requestAnimDict(animDict);
        while (!mp.game.streaming.hasAnimDictLoaded(animDict)) mp.game.wait(0);
        player.taskPlayAnim(animDict, animName, 8.0, 0.0, -1, 0, 0.0, false, false, false);
  
        shotFired = false;
        if (animCheckerHandle) destroyAnimChecker();

        animCheckerHandle = new mp.Event("render", () => {
            if (player.isPlayingAnim(animDict, animName, 3)) {
                if (animName === "PISTOL" && !shotFired && player.hasAnimEventFired(fireActionHash)) {
                    shotFired = true;
                    mp.game.invoke("0x96A05E4FB321B1BA", player.handle, 0.0, 0.0, 0.0, false); // SET_PED_SHOOTS_AT_COORD
                }

                if (player.getAnimCurrentTime(animDict, animName) >= animTime) {
                    destroyAnimChecker();
                    mp.events.callRemote("Suicide_Kill");
                }
            } else {
                destroyAnimChecker();
            }
        });
        
    }
});

mp.events.add("TwoSecondsEvent", () => {
    mp.players.local.setConfigFlag(149, true);
    mp.players.local.setConfigFlag(438, true);

    mp.events.callRemote("RemoveIsDamaged");
});

function setStorageDataVars(){

    if(mp.storage.data.menu == undefined) mp.storage.data.menu = {};

    global.LastKeyPress = Date.now();
    global.AFKStage = 0;
    global.antiAFKActive = true;

    if(mp.storage.data.chat == undefined){
        mp.storage.data.chat = {};
        mp.storage.data.chat.timestamp = false;
        mp.storage.data.chat.pagesize = 18;
        mp.storage.data.chat.fontsizenew = 1.0;
    }

    if(mp.storage.data.menu.chatSize === undefined) mp.storage.data.menu.chatSize = 1.0;
    if(mp.storage.data.menu.chatHeight === undefined) mp.storage.data.menu.chatHeight = 1.0;
    if(mp.storage.data.menu.chatWidth === undefined) mp.storage.data.menu.chatWidth = 1.0;
    global.ServerUI.call("hud_chat_size_multiplier", mp.storage.data.menu.chatSize);
    global.ServerUI.call("hud_chat_height_multiplier", mp.storage.data.menu.chatHeight);
    global.ServerUI.call("hud_chat_width_multiplier", mp.storage.data.menu.chatWidth);

    if(mp.storage.data.menu.Speedo == undefined){
        mp.storage.data.menu.Speedo = true;
    }
    if(mp.storage.data.menu.OOCauto !== undefined){
        autoOOC = mp.storage.data.menu.OOCauto;
        mp.events.callRemote("SetAutoOOC", autoOOC);
    }
    else{
        mp.storage.data.menu.OOCauto = autoOOC;
        mp.events.callRemote("SetAutoOOC", autoOOC);
    }

    if(mp.storage.data.menu.ShowDamageIndicator === undefined){
        mp.storage.data.menu.ShowDamageIndicator = true;
    }

    if(mp.storage.data.menu.ChatAccessibility == undefined){
        mp.storage.data.menu.ChatAccessibility = false;
    }

    if(mp.storage.data.menu.XMRAutoPlay == undefined){
        mp.storage.data.menu.XMRAutoPlay = true;
    }

    if(mp.storage.data.menu.AutoJoin !== undefined){
        mp.players.local.AutoJoin = mp.storage.data.menu.AutoJoin;
        mp.events.callRemote("SetAutoJoin", mp.players.local.AutoJoin, false);
    }
    else{
        mp.storage.data.menu.AutoJoin = ((mp.players.local.AutoJoin) ? mp.players.local.AutoJoin : true);
        mp.events.callRemote("SetAutoJoin", mp.storage.data.menu.AutoJoin, false);
    }

    if(mp.storage.data.menu.SnowEnabled !== undefined){
        mp.game1.gameplay.enableSnow = mp.storage.data.menu.SnowEnabled;
    }
    else{
        mp.game1.gameplay.enableSnow = false;
        mp.storage.data.menu.SnowEnabled = false;
    }
    
    if(mp.storage.data.menu.ShowVoiceActivity == undefined){
        mp.storage.data.menu.ShowVoiceActivity = true;
    }
    if(global.ServerUI) global.ServerUI.call('hud_voiceactivity_toggle', mp.storage.data.menu.ShowVoiceActivity.toString());

    if(mp.storage.data.menu.InactiveChatOpacity == undefined){
        mp.storage.data.menu.InactiveChatOpacity = 0.5;
    }
    if(mp.storage.data.menu.ActiveChatOpacity == undefined){
        mp.storage.data.menu.InactiveChatOpacity = 1.0;
    }
    if(mp.storage.data.menu.ShowInactiveChat == undefined){
        mp.storage.data.menu.ShowInactiveChat = true;
    }
    if(mp.storage.data.menu.ownNametag == undefined){
        mp.storage.data.menu.ownNametag = 0;
    }
    if(mp.storage.data.menu.AutoLogin == undefined){
        mp.storage.data.menu.AutoLogin = true;
    }
    if(mp.storage.data.menu.RealtimeSpeedo == undefined){
        mp.storage.data.menu.RealtimeSpeedo = false;
    }

    if(mp.storage.data.menu.EventMusic !== undefined){
        mp.players.local.EventMusic = mp.storage.data.menu.EventMusic;
    }
    else{
        mp.storage.data.menu.EventMusic = true;
        mp.players.local.EventMusic = true;
    }

    if(mp.storage.data.menu.digitalHPAR == undefined){
        mp.storage.data.menu.digitalHPAR = true;
        global.digitalHPAR = true;
    }
    else{
        global.digitalHPAR = mp.storage.data.menu.digitalHPAR;
    }


    if(mp.storage.data.menu.VoiceMode !== undefined){
        //mp.storage.data.menu.VoiceMode = false;
        mp.events.callRemote("SetVoiceMode", mp.storage.data.menu.VoiceMode, false);
    }
    else{
        mp.storage.data.menu.VoiceMode = 0;
        mp.events.callRemote("SetVoiceMode", 0, false);
    }


    mp.events.callLocal("LoadCrosshair");

    mp.storage.flush();
}