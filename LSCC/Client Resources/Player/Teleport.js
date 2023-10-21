var global = require('./globals');
const {TranslateMessage: Translate} = require('./Translator');

var LAST_TELEPORT_INDICATOR_DIM = -1;
var INDICATORS_CLEARED = false;

mp.events.addDataHandler("TELEPORT:Data", (entity, value, oldValue) => {
    global.TeleportData = JSON.parse(value);
    refreshTeleportIndicators();
});
mp.events.add("OneSecondEvent", () => {
    // Check for dimension changes and reload indicators if needed
    if(mp.players.local.getVariable("Team") !== 0 &&
       mp.players.local.getVariable("Team") !== 1 &&
       mp.players.local.getVariable("InLobby") !== -3)
    {
        if(INDICATORS_CLEARED) return;
        clearTeleportIndicators();
        INDICATORS_CLEARED = true;
        return;
    }
    if(mp.players.local.dimension === LAST_TELEPORT_INDICATOR_DIM) return;

    LAST_TELEPORT_INDICATOR_DIM = mp.players.local.dimension;
    INDICATORS_CLEARED = false;
    refreshTeleportIndicators();
});
function clearTeleportIndicators(){
    for(let i = 0 ; i < global.TeleportIndicators.length ; i++){
        const indicator = global.TeleportIndicators[i];
        if(indicator.blip){
            indicator.blip.destroy();
        }
        if(indicator.marker){
            indicator.marker.destroy();
        }
        if(indicator.label){
            indicator.label.destroy();
        }
    }
    global.TeleportIndicators = [];
}
function refreshTeleportIndicators(){
    clearTeleportIndicators();
    for(let i = 0 ; i < global.TeleportData.length ; i++){
        const tp = global.TeleportData[i];

        const label = mp.labels.new(`~w~${tp.Name}\n~y~Press ~g~(Y) ~y~to enter.`, new mp.Vector3(tp.FromX, tp.FromY, tp.FromZ), {
            font:4,
            los:true,
            drawDistance:6,
            dimension:mp.players.local.dimension
        });
        const marker = mp.markers.new(23, new mp.Vector3(tp.FromX, tp.FromY, tp.FromZ-0.95), 1, {
            color:[0, 0, 255, 50], 
            dimension:mp.players.local.dimension, 
            visible:true, 
        });

        var indicator = {id:tp.SQL_ID, marker:marker, label:label};

        if(tp.BlipID != -1){
            const blip = mp.blips.new(tp.BlipID, new mp.Vector3(tp.FromX, tp.FromY, tp.FromZ), {
                name:tp.Name,
                scale:0.9,
                color:0,
                alpha:255,
                dimension:mp.players.local.dimension,
                shortRange:true
            });
            indicator.blip = blip;
        }

        global.TeleportIndicators.push(indicator);
    }
}

var IsEnteringExiting = false;
var EnterExitTimeout = null;
var EnterExitFinishTimeout = null;
var CanUnfreezeEarly = false;

mp.events.add("CS_OnPlayerTeleportCheck", () => {
    if(mp.players.local.getVariable("InLobby") != -3 && mp.players.local.getVariable("Team") != 1 && mp.players.local.getVariable("Team") != 0) return;
    if(mp.players.local.isInAnyVehicle(true)) return;
    if(mp.players.local.getVariable("Wounded") > 0) return;
    if(IsEnteringExiting === true) return;

    const teleportsWithDistance = global.TeleportData.map((tp) => {
        const enterDistance = mp.game.system.vdist2(mp.players.local.position.x, mp.players.local.position.y, mp.players.local.position.z,
                                                    tp.FromX, tp.FromY, tp.FromZ);
        const exitDistance = mp.game.system.vdist2(mp.players.local.position.x, mp.players.local.position.y, mp.players.local.position.z,
                                                    tp.ToX, tp.ToY, tp.ToZ);

        return {...tp, entranceDistance:enterDistance, exitDistance};
    });
    const enterTP = teleportsWithDistance.find(t => t.entranceDistance < 4.0);
    if(enterTP){
        if(enterTP.ToDimension == -1 && enterTP.FromDimension == -1){
            mp.players.local.position = new mp.Vector3(enterTP.ToX, enterTP.ToY, enterTP.ToZ);
            mp.players.local.setHeading(enterTP.ToHeading);
            return;
        }
        if(enterTP.IPLs.length > 0){
            var iplList = enterTP.IPLs.split(',');
            if(iplList.length > 0){
                for(let i = 0 ; i < iplList.length ; i++){
                    mp.game.streaming.requestIpl(iplList[i]);
                }
            }
        }

        IsEnteringExiting = true;
        CanUnfreezeEarly = false;
        mp.events.callRemote("Teleport:OnPlayerEnter", enterTP.SQL_ID, enterTP.ToDimension);
        mp.events.callRemote("ChangePlayerAlpha", 0);
        mp.game.cam.doScreenFadeOut(250);
        mp.players.local.setInvincible(true);
        mp.players.local.freezePosition(true);

        EnterExitTimeout = setTimeout(() => {
            mp.players.local.freezePosition(false);
            mp.players.local.position = new mp.Vector3(enterTP.ToX, enterTP.ToY, enterTP.ToZ);
            mp.players.local.setHeading(enterTP.ToHeading);
            mp.players.local.freezePosition(true);
            mp.game.cam.doScreenFadeIn(250);

            EnterExitFinishTimeout = null;

            mp.players.local.setAlpha(255);
            CanUnfreezeEarly = true;
            global.ServerUI.call('notification_send', Translate("EARLY_UNFREEZE_TIP"), "info", enterTP.FreezeTime);
            EnterExitFinishTimeout = setTimeout(() => {
                mp.players.local.freezePosition(false);
                mp.players.local.setInvincible(false);
                mp.events.callRemote("ChangePlayerAlpha", 255);

                IsEnteringExiting = false;
                CanUnfreezeEarly = false;

                EnterExitFinishTimeout = null;
                EnterExitTimeout = null;
            }, enterTP.FreezeTime);
        }, 251);
        return;
    }
    const exitTP = teleportsWithDistance.find(t => t.exitDistance < 4.0);
    if(exitTP){
        if(exitTP.ToDimension == -1 && exitTP.FromDimension == -1){
            mp.players.local.position = new mp.Vector3(exitTP.FromX, exitTP.FromY, exitTP.FromZ);
            mp.players.local.setHeading(exitTP.FromHeading);
            return;
        }

        IsEnteringExiting = true;
        CanUnfreezeEarly = false;
        mp.events.callRemote("Teleport:OnPlayerExit", exitTP.SQL_ID, exitTP.FromDimension);
        mp.events.callRemote("ChangePlayerAlpha", 0);
        mp.game.cam.doScreenFadeOut(250);
        mp.players.local.setInvincible(true);
        mp.players.local.freezePosition(true);

        EnterExitTimeout = null;
        EnterExitTimeout = setTimeout(() => {
            mp.players.local.freezePosition(false);
            mp.players.local.position = new mp.Vector3(exitTP.FromX, exitTP.FromY, exitTP.FromZ);
            mp.players.local.setHeading(exitTP.FromHeading);
            mp.players.local.freezePosition(true);
            mp.game.cam.doScreenFadeIn(250);

            EnterExitFinishTimeout = null;

            mp.players.local.setAlpha(255);
            CanUnfreezeEarly = true;
            global.ServerUI.call('notification_send', Translate("EARLY_UNFREEZE_TIP"), "info", exitTP.FreezeTime);
            EnterExitFinishTimeout = setTimeout(() => {
                mp.players.local.freezePosition(false);
                mp.players.local.setInvincible(false);
                mp.events.callRemote("ChangePlayerAlpha", 255);

                IsEnteringExiting = false;
                CanUnfreezeEarly = false;

                EnterExitFinishTimeout = null;
                EnterExitTimeout = null;
            }, exitTP.FreezeTime);
        }, 251);
        return;
    }
});

mp.keys.bind(0xA4, true, () => {
    if(IsEnteringExiting && CanUnfreezeEarly){
        mp.players.local.freezePosition(false);
        mp.players.local.setInvincible(false);
        mp.events.callRemote("ChangePlayerAlpha", 255);

        IsEnteringExiting = false;
        CanUnfreezeEarly = false;

        EnterExitFinishTimeout = null;
        EnterExitTimeout = null;
    }
});

