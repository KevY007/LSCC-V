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
    ServerSettings,
    chatStatusTimeout,
    LogPlayerDamage,
    ToggleMDC,
    actionMenuClose,
    actionMenuOpen,
    traceLastFunc,
    isHeliCamActive
   } = require('./globals');
  const {TranslateMessage: Translate} = require('./Translator');
let isAimingLastReport = false;
let cameraUpdateDisabled = false;
let lastCameraDirReport = new mp.Vector3(-1.0, -1.0, -1.0);
let lastUpdateTime = Date.now();

mp.events.add('render', () => {
    if(mp.players.local.getVariable("pLogged") !== undefined && mp.players.local.getVariable("pLogged") == true)
    {
        if(isAimingLastReport != mp.game.invoke('0x68EDDA28A5976D07'))
        {
            isAimingLastReport = mp.game.invoke('0x68EDDA28A5976D07');
            mp.events.callRemoteUnreliable("UpdateAimingState", isAimingLastReport);
        }

        if(ServerSettings.CameraUpdateTime !== undefined && ServerSettings.CameraUpdateTime >= 10 
            && mp.players.local.getVariable("UpdateCameraData") == true)
        {
            if(ServerSettings.CameraUpdateTime != 699 && Date.now() - lastUpdateTime >= ServerSettings.CameraUpdateTime)
            {
                let camera = mp.cameras.new("gameplay");
                let direction = camera.getDirection();

                if(lastCameraDirReport.x != direction.x || lastCameraDirReport.y != direction.y || lastCameraDirReport.z != direction.z)
                {
                    lastCameraDirReport = direction;
                    cameraUpdateDisabled = false;
                    mp.events.callRemoteUnreliable("UpdateCameraDir", lastCameraDirReport.x, lastCameraDirReport.y, lastCameraDirReport.z);
                }
                lastUpdateTime = Date.now();
            }
            else if(ServerSettings.CameraUpdateTime == 699 && cameraUpdateDisabled == false)
            {
                mp.events.callRemoteUnreliable("UpdateCameraDir", -1.0, -1.0, -1.0);
                cameraUpdateDisabled = true;
            }
        }
        else if(cameraUpdateDisabled == false)
        {
            mp.events.callRemoteUnreliable("UpdateCameraDir", -1.0, -1.0, -1.0);
            cameraUpdateDisabled = true;
        }
    }
});