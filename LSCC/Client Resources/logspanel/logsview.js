let LogsCEF = null;

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
})