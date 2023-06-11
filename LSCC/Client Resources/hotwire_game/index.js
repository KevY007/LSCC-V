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
    isHeliCamActive
   } = require('./globals');
  const {TranslateMessage: Translate} = require('./Translator');
function InitializeHotwireDOM(){
    HotwireDOM = mp.browsers.new("package://hotwire_game/index.html");
    HotwireDOM.active = false;
}

mp.events.add("StartHotwire", () => {
    HotwireDOM.execute(`generateHotwire();`); // restarts the minigame
    HotwireDOM.active = true;
    isHotwiring = true;
});
mp.events.add("StopHotwire", () => {
    HotwireDOM.active = false;
    isHotwiring = false;
});
mp.events.add("CS_HotwireResult", (success) => {
    if(!isHotwiring) return;

    isHotwiring = false;
    if(success){
        setTimeout(() => {
            HotwireDOM.active = false; // to let the sound fully play
        }, 500);
    }
    mp.events.callRemote("OnHotwireResult", success);
});