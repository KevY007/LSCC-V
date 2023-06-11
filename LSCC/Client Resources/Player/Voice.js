var {
    currentJailTime,
    lastJailUpdate,
    chaseRunning,
    chaseTimeCur,
    chaseTimeMax,
    chaseMode,
    collisionTime,
    updateRate,
    radioToggle,
    carryTarget,
    chatStatus,
    chatStatusLate,
    menuToggledLate,
    lastChatToggle,
    streetName,
    zoneName,
    showHud,
    renderNametags,
    currentMuteTime,
    lastMuteUpdate,
    lastFunc,
    curMDCBind,
    lastMDCBind,
    curActionMenuBind,
    lastActionMenuBind,
    BankRobberyOn,
    ShowVehicleDL,
    specState,
    specTarget,
    isNoClip,
    LastKeyPress,
    AFKStage,
    SecondsSinceLastKeyPress,
    antiAFKActive,
    showReticule,
    showCustomCrosshair,
    showCustomCrosshairInSniper,
    alwaysShowCustomCrosshair,
    customCrosshairOn,
    ServerUI,
    menuToggled,
    scoreboardToggled,
    scoreboardCursor,
    StationPickerToggled,
    MDC_DOM,
    MDCActive,
    isLockpicking,
    isHotwiring,
    HotwireDOM,
    ClothingEditorDOM,
    ClothingEditorActive,
    ActionMenuActive,
    VehicleSpawnerActive,
    WeaponEditorActive,
    FireModeDisplayText,
    cuffTarget,
    cuffInterval,
    boneData,
    weaponData,
    loginSceneEntities,
    chatStatusTimeout,
    LogPlayerDamage,
    ToggleMDC,
    actionMenuClose,
    actionMenuOpen,
    traceLastFunc,
    isHeliCamActive,
    lastVoiceBind,
    curVoiceBind
   } = require('./globals');
  const {TranslateMessage: Translate} = require('./Translator');
var MaxVoiceRange = 50;
mp.voiceChat.muted = true;



function DisableVoice()
{
    traceLastFunc(`[Players] Disable Voice`);

    mp.events.callRemote("OnScriptedKeyPress", 0x4E, false);
    mp.voiceChat.muted = true;
}

function EnableVoice()
{
    if(!chatStatus && 
		!(mp.players.local.getVariable("pLogged") === undefined || mp.players.local.getVariable("pLogged") == false)) 
	{
        traceLastFunc(`[Players] Enable Voice`);

		mp.voiceChat.muted = false;
		mp.events.callRemote("OnScriptedKeyPress", 0x4E, true);
	}
}

function TogVCMode()
{
    if(!chatStatus && mp.voiceChat.muted == false && 
		!(mp.players.local.getVariable("pLogged") === undefined || mp.players.local.getVariable("pLogged") == false)) 
	{
		mp.events.callRemote("OnScriptedKeyPress", 0x09, true);
	}
}

if(mp.storage.data.voicebind !== undefined)
{
    curVoiceBind = mp.storage.data.voicebind;
    lastVoiceBind = mp.storage.data.voicebind;
}

mp.keys.bind(lastVoiceBind, false, DisableVoice);
mp.keys.bind(lastVoiceBind, true, EnableVoice);
mp.keys.bind(0x09, true, TogVCMode);

mp.events.addCommand("voicebind", function (bind) {
    bind = Number(bind);

    if (!Number.isInteger(bind)) {
        mp.gui.chat.push("USAGE: voicebind (keycode) --Input a virtual keycode e.g. 0x4E is 'N' (default)");
        mp.gui.chat.push("https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes");
        return;
    }

    mp.storage.data.voicebind = bind;

    mp.storage.flush();

    lastVoiceBind = curVoiceBind;
    curVoiceBind = bind;

    mp.keys.unbind(lastVoiceBind, false, DisableVoice);
    mp.keys.unbind(lastVoiceBind, true, DisableVoice);

    DisableVoice();
    mp.keys.bind(curVoiceBind, false, DisableVoice);
    mp.keys.bind(curVoiceBind, true, EnableVoice);

    mp.events.callLocal("HUDNotify", `Push to talk is now binded to <strong>${String.fromCharCode(curVoiceBind)}</strong>.`, "blue", "fas fa-check", false, 
        1000, "bottomCenter", "replace");
        
    mp.events.callLocal("RemoveTooltip", String.fromCharCode(lastVoiceBind));
    mp.events.callLocal("AddTooltip", String.fromCharCode(curVoiceBind), "Voice Chat", -1);
});

mp.events.add("playerStopTalking", (player) => {
    if(!ServerUI) return;
    if(ServerUI) ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.voiceActivity.removeVoice(${player.remoteId})`);
});
mp.events.add("playerStartTalking", (player) => {
    if(!ServerUI) return;
    if(ServerUI) ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.voiceActivity.addVoice(${player.remoteId}, "${player.name}")`);
});