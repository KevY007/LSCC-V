mp.events.add("ShowLogsCEF", (toggle) => {
    if(toggle){        
        //mp.events.call("TogglePlayerChat", false);
        mp.gui.cursor.show(true, true);
    }
    else 
    {
        //mp.events.call("TogglePlayerChat", true);
        mp.gui.cursor.show(false, false);
        MainUI.execute(`gm.$refs.logsMenu.searchLoading = false;`);
    }
    MainUI.execute(`gm.displayLogs = ${toggle == true ? 'true' : 'false'};`);

});
mp.events.add("RequestAutoCompleteName", (query) => {
    mp.events.callRemote("Logs_GetAutoCompleteNames", query);
});
mp.events.add("ReturnAutoCompleteNames", (jsonShit) => {
    MainUI.execute(`gm.$refs.logsMenu.filteredPlayers = ${jsonShit};`);
});

mp.events.add("RequestLogsFromServer", (type, username) => {
    mp.gui.chat.push("Type: " + type + " ; Username: " + username);
    mp.events.callRemote("ReqLogFromSrv", username, type);
});
mp.events.add("ReturnLogsToClient", (type, jsonLogs) => {

    if(jsonLogs == '[]'){
        MainUI.execute(`gm.$refs.logsMenu.searchLoading = false;`);
        return;
    }

    switch(type){
        case 1:{
            MainUI.execute(`gm.$refs.logsMenu.columns = [
                {field: 'Description', header: 'Description'},
                {field: 'Timestamp', header: 'Timestamp'}
              ]`);
            MainUI.execute(` gm.$refs.logsMenu.logsData = ${jsonLogs}`);
            break;
        }
        case 2:{
            MainUI.execute(`gm.$refs.logsMenu.columns = [
                {field: 'TimestampJoin', header: 'Joined'},
                {field: 'TimestampLeave', header: 'Left'},
                {field: 'Duration', header: 'Duration'},
                {field: 'IP', header: 'IP'},
                {field: 'HWID', header: 'HWID'}
              ]`);
            MainUI.execute(` gm.$refs.logsMenu.logsData = ${jsonLogs}`);           
            break;
        }
        case 3:{
            MainUI.execute(`gm.$refs.logsMenu.columns = [
                {field: 'Type', header: 'Type'},
                {field: 'Reason', header: 'Reason'},
                {field: 'Timestamp', header: 'Timestamp'},
                {field: 'Duration', header: 'Duration'}
              ]`);
            MainUI.execute(` gm.$refs.logsMenu.logsData = ${jsonLogs}`);           
            break;
        }
    }
    MainUI.execute(`gm.$refs.logsMenu.searchLoading = false;`);
});


/*let LogsCEF = null;

mp.events.add("ShowLogsCEF", (toggle) => {

    if(toggle){       
        mp.gui.cursor.show(toggle, toggle);
        LogsCEF = mp.browsers.new("package://logspanel/index.html");
    }
    else{
        mp.gui.cursor.show(toggle, toggle);
        LogsCEF.destroy();
    }
});

mp.events.add("ReturnLogsToClient", (type, jsonLogs) => {

    jsonLogs = jsonLogs.replaceAll("{", "[");
    jsonLogs = jsonLogs.replaceAll("}", "]");

    switch(type){
        case 1:{
            jsonLogs = jsonLogs.replaceAll("\"Item1\":", "");
            jsonLogs = jsonLogs.replaceAll("\"Item2\":", "");    

            LogsCEF.execute(`gm.$refs.logs.updateLogs(["Description", "Timestamp"],  ${jsonLogs})`);  

            break;
        }
        case 2:{
            jsonLogs = jsonLogs.replaceAll("\"Item1\":", "");
            jsonLogs = jsonLogs.replaceAll("\"Item2\":", "");
            jsonLogs = jsonLogs.replaceAll("\"Item3\":", "");
            jsonLogs = jsonLogs.replaceAll("\"Item4\":", "");
            jsonLogs = jsonLogs.replaceAll("\"Item5\":", "");

            LogsCEF.execute(`gm.$refs.logs.updateLogs(["Joined", "Left", "Duration", "IP", "HWID"],  ${jsonLogs})`);
            break;
        }
        case 3:{
            jsonLogs = jsonLogs.replaceAll("\"Item1\":", "");
            jsonLogs = jsonLogs.replaceAll("\"Item2\":", "");
            jsonLogs = jsonLogs.replaceAll("\"Item3\":", "");
            jsonLogs = jsonLogs.replaceAll("\"Item4\":", "");
    
            LogsCEF.execute(`gm.$refs.logs.updateLogs(["Type", "Reason", "Timestamp", "Duration"],  ${jsonLogs})`);
            break;
        }
    }

});

mp.events.add("RequestLogsFromServer", (type, username) => {
    mp.events.callRemote("ReqLogFromSrv", username, type);
})*/