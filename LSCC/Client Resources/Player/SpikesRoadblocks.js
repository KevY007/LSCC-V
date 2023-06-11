let FirstEndPosition = null;
let StingerPreviewObject = null;
let PlacingStingers = false;
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
const STINGER_OBJECT = 3420629148; // pd_ld_stinger_s

mp.events.add("Spikes_BeginPlacing", () => {
    PlacingStingers = true;
    FirstEndPosition = mp.players.local.position;

    StingerPreviewObject = mp.objects.new(STINGER_OBJECT, FirstEndPosition, {
        alpha: 255,
        dimension: mp.players.local.dimension
    });
    StingerPreviewObject.setRotation(5, 12, 69, 1, true);
    mp.gui.chat.push(`STINGER ROT: ${StingerPreviewObject.rotation.z}`);
});
/*
mp.events.add('render', () => {
    if(!PlacingStingers) return;
    if(StingerPreviewObject == null) return;
    if(mp.players.local == null || !mp.players.exists(mp.players.local)) return;

    StingerPreviewObject.destroy();
    StingerPreviewObject = mp.objects.new(STINGER_OBJECT, FirstEndPosition, {
        rotation: getStingerFacePlayerRotation(),
        alpha: 128,
        dimension: mp.players.local.dimension
    });
});*/