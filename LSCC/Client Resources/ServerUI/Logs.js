var global = require('./globals');
  const {TranslateMessage: Translate} = require('./Translator');
require("./ServerUI/Menu.js");

let currentDialogID = "";
var testArray = [];
mp.events.add("logstest", () => {
    for(let i = 0 ; i < 200 ; i++)
        testArray.push(i);
});
mp.events.add("openLogsDialog", (dialogId, username) => {
    if(!global.ServerUI) return;
    currentDialogID = dialogId;
    global.ServerUI.call('dialog_logs_open', dialogId, username);
    global.ServerUI.call(`dialog_logs_total_set_${currentDialogID}`, testArray.length);
    //mp.events.callRemote('ServerConsoleOutput', `opened logs dialog for ${dialogId} and ${username}`);

});
mp.events.add("dialog_logs_request_page", (dialogId, username, first, rows) => {
    //mp.events.callRemote('ServerConsoleOutput', `Trying to get logs for ${dialogId} from ${first} and ${rows} rows!`);
    var logsToShow = [];
    for(let i = first ; i < first+rows ; i++){
        logsToShow.push(testArray[i]);
    }
    //mp.events.callRemote('ServerConsoleOutput', `${JSON.stringify(logsToShow)}`);
    global.ServerUI.call(`dialog_logs_add_${dialogId}`, logsToShow);
});

mp.events.add("RequestLogsFromServer", (type, username) => {
    if(!global.ServerUI) return;
    mp.events.callRemote("ReqLogFromSrv", username, type);
});
mp.events.add("Menu_SetLogsTabLoading", (toggle) => {
    if(!global.ServerUI) return;
});
mp.events.add("InitiateLogsTab", () => {
    if(!global.ServerUI) return;
    // if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.logsTab.options = [
    //     { name: "Sessions", value: 2 },
    //     { name: "Admin", value: 1 },
    //     { name: "Admin Records", value: 3 },
    //     { name: "Incoming Damages", value: 4 },
    //     { name: "Outgoing Damages", value: 5 },
    //     { name: "Commands", value: 6 },
    //   ];`);
    // if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.logsTab.selectedOption = 3;`);
});
mp.events.add("ReturnLogsToClient", (type, jsonLogs) => {
    if(!global.ServerUI) return;

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
            //if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.logsTab.rows = ["Timestamp", "Description"];`);
            //if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.logsTab.columns = ${jsonLogs}`);
            break;
        }
        case 2:{ // session
            //if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.logsTab.rows = ["Join Time", "Leave Time", "Duration", "IP", "HWID"];`);
            //if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.logsTab.columns = ${jsonLogs}`);
            break;
        }
        case 3:{ // admin records
            //if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.logsTab.rows = ["Timestamp", "Admin", "Duration", "Reason"];`);
            //if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.logsTab.columns = ${jsonLogs}`);
            break;
        }
        case 4:{ // incoming damage

            let damageLogs = JSON.parse(jsonLogs);
            for(let i = 0 ; i < damageLogs.length ; i++){
                var weaponID = damageLogs[i][2];
                var bodypartID = damageLogs[i][5];

                let gunData = null;
                if(weaponID > 0)
                    gunData = global.weaponData.find(w => Number(w.Hash) == weaponID);
                else gunData = global.weaponData.find(w => Number(w.Hash) == (weaponID+4294967296));
                if(gunData)
                    damageLogs[i][2] = gunData.Name;
                else damageLogs[i][2] = `Unknown (${weaponID})`;

                let bodypartData = global.boneData.find(b => b.index === bodypartID);

                if(bodypartData)
                    damageLogs[i][5] = bodypartData.name;
                else damageLogs[i][5] = `Unknown (${bodypartID})`;
            }
            jsonLogs = JSON.stringify(damageLogs);

            if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.logsTab.rows = ["Timestamp", "Dealt By", "Weapon", "Damage", "Health (after DMG)", "Bodypart", "Armor (after DMG)"];`);
            if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.logsTab.columns = ${jsonLogs}`);
            break;
        }
        case 5:{ // outgoing damage

            let damageLogs = JSON.parse(jsonLogs);
            for(let i = 0 ; i < damageLogs.length ; i++){
                var weaponID = damageLogs[i][2];
                var bodypartID = damageLogs[i][5];
                
                let gunData = null;
                if(weaponID > 0)
                    gunData = global.weaponData.find(w => Number(w.Hash) == weaponID);
                else gunData = global.weaponData.find(w => Number(w.Hash) == (weaponID+4294967296));
                if(gunData)
                    damageLogs[i][2] = gunData.Name;
                else damageLogs[i][2] = `Unknown (${weaponID})`;

                let bodypartData = global.boneData.find(b => b.index === bodypartID);

                if(bodypartData)
                    damageLogs[i][5] = bodypartData.name;
                else damageLogs[i][5] = `Unknown (${bodypartID})`;
            }
            jsonLogs = JSON.stringify(damageLogs);

            if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.logsTab.rows = ["Timestamp", "Victim", "Weapon", "Damage", "Health (after DMG)", "Bodypart", "Armor (after DMG)"];`);
            if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.logsTab.columns = ${jsonLogs}`);
            break;
        }
        case 6:{ // commands
            if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.logsTab.rows = ["Timestamp", "Command"];`);
            if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.logsTab.columns = ${jsonLogs}`);
            break;
        }
    }
    if(global.ServerUI) global.ServerUI.execute('gm.$refs.mainMenu.$refs.logsTab.isLoading = false;');

});

mp.events.add('playerCommand', (command) => {
    mp.events.callRemote("logPlayerCommand", command);
});