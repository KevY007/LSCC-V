require("./ServerUI/Menu.js");

mp.events.add("RequestLogsFromServer", (type, username) => {
    ServerUI.execute('gm.$refs.mainMenu.$refs.logsTab.isLoading = true;');
    mp.events.callRemote("ReqLogFromSrv", username, type);
});
mp.events.add("Menu_SetLogsTabLoading", (toggle) => {
    ServerUI.execute(`gm.$refs.mainMenu.$refs.logsTab.isLoading = ${toggle};`);
});
mp.events.add("InitiateLogsTab", () => {
    ServerUI.execute(`gm.$refs.mainMenu.$refs.logsTab.options = [
        { name: "Sessions", value: 2 },
        { name: "Admin", value: 1 },
        { name: "Admin Records", value: 3 },
        { name: "Incoming Damages", value: 4 },
        { name: "Outgoing Damages", value: 5 },
      ];`);
    ServerUI.execute(`gm.$refs.mainMenu.$refs.logsTab.selectedOption = 3;`);
});
mp.events.add("ReturnLogsToClient", (type, jsonLogs) => {

    jsonLogs = jsonLogs.replaceAll("{", "[");
    jsonLogs = jsonLogs.replaceAll("}", "]");

    for(let i = 1 ; i < 20 ; i++){
        let strcheck = `"Item${i}":`;
        if(jsonLogs.includes(strcheck)){
            jsonLogs = jsonLogs.replaceAll(strcheck, "");
        }
        else break;
    }

    mp.events.callRemote("ServerConsoleOutput", jsonLogs);

    switch(type){
        case 1:{ // admin
            ServerUI.execute(`gm.$refs.mainMenu.$refs.logsTab.rows = ["Timestamp", "Description"];`);
            ServerUI.execute(`gm.$refs.mainMenu.$refs.logsTab.columns = ${jsonLogs}`);
            break;
        }
        case 2:{ // session
            ServerUI.execute(`gm.$refs.mainMenu.$refs.logsTab.rows = ["Join Time", "Leave Time", "Duration", "IP", "HWID"];`);
            ServerUI.execute(`gm.$refs.mainMenu.$refs.logsTab.columns = ${jsonLogs}`);
            break;
        }
        case 3:{ // admin records
            ServerUI.execute(`gm.$refs.mainMenu.$refs.logsTab.rows = ["Timestamp", "Admin", "Duration", "Reason"];`);
            ServerUI.execute(`gm.$refs.mainMenu.$refs.logsTab.columns = ${jsonLogs}`);
            break;
        }
        case 4:{ // incoming damage
            mp.events.callRemote("ServerConsoleOutput", "i got to this part lol");

            let damageLogs = JSON.parse(jsonLogs);
            mp.events.callRemote("ServerConsoleOutput", "after damage logs!! xd");
            for(let i = 0 ; i < damageLogs.length ; i++){
                var weaponID = damageLogs[i][2];
                var bodypartID = damageLogs[i][5];

                let gunData = null;
                if(weaponID > 0)
                    gunData = weaponData.find(w => Number(w.Hash) == weaponID);
                else gunData = weaponData.find(w => Number(w.Hash) == weaponID>>>0);
                if(gunData)
                    damageLogs[i][2] = gunData.Name;
                else damageLogs[i][2] = `Unknown (${weaponID})`;

                let bodypartData = boneData.find(b => b.index === bodypartID);

                if(bodypartData)
                    damageLogs[i][5] = bodypartData.name;
                else damageLogs[i][5] = `Unknown (${bodypartID})`;
            }
            jsonLogs = JSON.stringify(damageLogs);

            ServerUI.execute(`gm.$refs.mainMenu.$refs.logsTab.rows = ["Timestamp", "Dealt By", "Weapon", "Damage", "Health (after DMG)", "Bodypart", "Armor (after DMG)"];`);
            ServerUI.execute(`gm.$refs.mainMenu.$refs.logsTab.columns = ${jsonLogs}`);
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
                else gunData = weaponData.find(w => Number(w.Hash) == weaponID>>>0);
                if(gunData)
                    damageLogs[i][2] = gunData.Name;
                else damageLogs[i][2] = `Unknown (${weaponID})`;

                let bodypartData = boneData.find(b => b.index === bodypartID);

                if(bodypartData)
                    damageLogs[i][5] = bodypartData.name;
                else damageLogs[i][5] = `Unknown (${bodypartID})`;
            }
            jsonLogs = JSON.stringify(damageLogs);

            ServerUI.execute(`gm.$refs.mainMenu.$refs.logsTab.rows = ["Timestamp", "Victim", "Weapon", "Damage", "Health (after DMG)", "Bodypart", "Armor (after DMG)"];`);
            ServerUI.execute(`gm.$refs.mainMenu.$refs.logsTab.columns = ${jsonLogs}`);
            break;
        }
    }
    ServerUI.execute('gm.$refs.mainMenu.$refs.logsTab.isLoading = false;');

});