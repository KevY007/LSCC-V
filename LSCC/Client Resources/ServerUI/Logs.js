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
require("./ServerUI/Menu.js");

mp.events.add("RequestLogsFromServer", (type, username) => {
    if(!ServerUI) return;
    if(ServerUI) ServerUI.execute('gm.$refs.mainMenu.$refs.logsTab.isLoading = true;');
    mp.events.callRemote("ReqLogFromSrv", username, type);
});
mp.events.add("Menu_SetLogsTabLoading", (toggle) => {
    if(!ServerUI) return;
    if(ServerUI) ServerUI.execute(`gm.$refs.mainMenu.$refs.logsTab.isLoading = ${toggle};`);
});
mp.events.add("InitiateLogsTab", () => {
    if(!ServerUI) return;
    if(ServerUI) ServerUI.execute(`gm.$refs.mainMenu.$refs.logsTab.options = [
        { name: "Sessions", value: 2 },
        { name: "Admin", value: 1 },
        { name: "Admin Records", value: 3 },
        { name: "Incoming Damages", value: 4 },
        { name: "Outgoing Damages", value: 5 },
        { name: "Commands", value: 6 },
      ];`);
    if(ServerUI) ServerUI.execute(`gm.$refs.mainMenu.$refs.logsTab.selectedOption = 3;`);
});
mp.events.add("ReturnLogsToClient", (type, jsonLogs) => {
    if(!ServerUI) return;

    jsonLogs = jsonLogs.replaceAll("{", "[");
    jsonLogs = jsonLogs.replaceAll("}", "]");

    for(let i = 1 ; i < 20 ; i++){
        let strcheck = `"Item${i}":`;
        if(jsonLogs.includes(strcheck)){
            jsonLogs = jsonLogs.replaceAll(strcheck, "");
        }
        else break;
    }

    switch(type){
        case 1:{ // admin
            if(ServerUI) ServerUI.execute(`gm.$refs.mainMenu.$refs.logsTab.rows = ["Timestamp", "Description"];`);
            if(ServerUI) ServerUI.execute(`gm.$refs.mainMenu.$refs.logsTab.columns = ${jsonLogs}`);
            break;
        }
        case 2:{ // session
            if(ServerUI) ServerUI.execute(`gm.$refs.mainMenu.$refs.logsTab.rows = ["Join Time", "Leave Time", "Duration", "IP", "HWID"];`);
            if(ServerUI) ServerUI.execute(`gm.$refs.mainMenu.$refs.logsTab.columns = ${jsonLogs}`);
            break;
        }
        case 3:{ // admin records
            if(ServerUI) ServerUI.execute(`gm.$refs.mainMenu.$refs.logsTab.rows = ["Timestamp", "Admin", "Duration", "Reason"];`);
            if(ServerUI) ServerUI.execute(`gm.$refs.mainMenu.$refs.logsTab.columns = ${jsonLogs}`);
            break;
        }
        case 4:{ // incoming damage

            let damageLogs = JSON.parse(jsonLogs);
            for(let i = 0 ; i < damageLogs.length ; i++){
                var weaponID = damageLogs[i][2];
                var bodypartID = damageLogs[i][5];

                let gunData = null;
                if(weaponID > 0)
                    gunData = weaponData.find(w => Number(w.Hash) == weaponID);
                else gunData = weaponData.find(w => Number(w.Hash) == (weaponID+4294967296));
                if(gunData)
                    damageLogs[i][2] = gunData.Name;
                else damageLogs[i][2] = `Unknown (${weaponID})`;

                let bodypartData = boneData.find(b => b.index === bodypartID);

                if(bodypartData)
                    damageLogs[i][5] = bodypartData.name;
                else damageLogs[i][5] = `Unknown (${bodypartID})`;
            }
            jsonLogs = JSON.stringify(damageLogs);

            if(ServerUI) ServerUI.execute(`gm.$refs.mainMenu.$refs.logsTab.rows = ["Timestamp", "Dealt By", "Weapon", "Damage", "Health (after DMG)", "Bodypart", "Armor (after DMG)"];`);
            if(ServerUI) ServerUI.execute(`gm.$refs.mainMenu.$refs.logsTab.columns = ${jsonLogs}`);
            break;
        }
        case 5:{ // outgoing damage

            let damageLogs = JSON.parse(jsonLogs);
            for(let i = 0 ; i < damageLogs.length ; i++){
                var weaponID = damageLogs[i][2];
                var bodypartID = damageLogs[i][5];
                
                let gunData = null;
                if(weaponID > 0)
                    gunData = weaponData.find(w => Number(w.Hash) == weaponID);
                else gunData = weaponData.find(w => Number(w.Hash) == (weaponID+4294967296));
                if(gunData)
                    damageLogs[i][2] = gunData.Name;
                else damageLogs[i][2] = `Unknown (${weaponID})`;

                let bodypartData = boneData.find(b => b.index === bodypartID);

                if(bodypartData)
                    damageLogs[i][5] = bodypartData.name;
                else damageLogs[i][5] = `Unknown (${bodypartID})`;
            }
            jsonLogs = JSON.stringify(damageLogs);

            if(ServerUI) ServerUI.execute(`gm.$refs.mainMenu.$refs.logsTab.rows = ["Timestamp", "Victim", "Weapon", "Damage", "Health (after DMG)", "Bodypart", "Armor (after DMG)"];`);
            if(ServerUI) ServerUI.execute(`gm.$refs.mainMenu.$refs.logsTab.columns = ${jsonLogs}`);
            break;
        }
        case 6:{ // commands
            if(ServerUI) ServerUI.execute(`gm.$refs.mainMenu.$refs.logsTab.rows = ["Timestamp", "Command"];`);
            if(ServerUI) ServerUI.execute(`gm.$refs.mainMenu.$refs.logsTab.columns = ${jsonLogs}`);
            break;
        }
    }
    if(ServerUI) ServerUI.execute('gm.$refs.mainMenu.$refs.logsTab.isLoading = false;');

});

mp.events.add('playerCommand', (command) => {
    mp.events.callRemote("logPlayerCommand", command);
});