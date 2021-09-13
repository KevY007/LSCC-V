var menuToggled = false;

UIMenu = mp.browsers.new("package://UI/Menu/index.html");
UIMenu.execute(`gm.mainMenu = false;`);
UIMenu.active = false;

mp.keys.bind(0x4D, false, function() {
    if(Date.now() - lastChatToggle >= 500 && chatStatus == false && mp.players.local.getVariable("pLogged") !== undefined && mp.players.local.getVariable("pLogged") == true)
    {
        traceLastFunc(`[Menu] Attempt open/M key`);

        menuToggled = !menuToggled;
        UIMenu.active = menuToggled;
        UIMenu.execute(`gm.mainMenu = ${menuToggled};`);
        mp.events.callRemote("OnToggleMenu", menuToggled);
        mp.gui.cursor.show(menuToggled, menuToggled);
        mp.gui.chat.activate(!menuToggled);
    }
});

mp.events.add("PutPlayerInLobby", (lobbyID) => { // Add 'password' here in future for server side confirmation.
    mp.events.callRemote("RequestLobbySwitch", lobbyID, "");
});

mp.events.add("UpdateMenuArenas", (arenas) => {
    UIMenu.execute(`gm.$refs.mainMenu.$refs.dmarenas.arenas = [];`);
    
    let menuItems = JSON.parse(arenas);
    for(let i = 0; i < menuItems.length; i++)
    {
        let insertString = `gm.$refs.mainMenu.$refs.dmarenas.arenas.push({`;
        insertString += `"id": ${menuItems[i][0]}, `;
        insertString += `"name": "${menuItems[i][1]}", `;
        insertString += `"background": "${menuItems[i][2]}", `;
        insertString += `"players": ${menuItems[i][3]}, `;
        insertString += `"maxplayers": ${menuItems[i][4]}`;
        if(menuItems[i][5].length > 1) insertString += `, "gamemode": "${menuItems[i][5]}"`;
        
        insertString += "});";

        UIMenu.execute(insertString);
    }
});

mp.events.add("UpdateMenuVehicles", (vehicles) => {
    UIMenu.execute(`gm.$refs.mainMenu.$refs.vehicles.$refs.police.vehicles = [];`);
});

mp.events.add("UpdateMenuLobbies", (lobbies) => {
    UIMenu.execute(`gm.$refs.mainMenu.$refs.lobbies.lobbies = [];`);

    // Lobby Data
    // ID, Name, Background, Gamemode, Password, Players, MaxPlayers, Description
    let menuItems = JSON.parse(lobbies);
    for(let i = 0; i < menuItems.length; i++)
    {
        let insertString = `gm.$refs.mainMenu.$refs.lobbies.lobbies.push({`;
        insertString += `"id": ${menuItems[i][0]}, `;
        insertString += `"name": "${menuItems[i][1]}", `;
        insertString += `"background": "${menuItems[i][2]}", `;
        insertString += `"gamemode": "${menuItems[i][3]}", `;
        if(menuItems[i][4].length > 1) insertString += `"password": "${menuItems[i][4]}", `;
        insertString += `"players": ${menuItems[i][5]}, `;
        insertString += `"maxplayers": ${menuItems[i][6]}`;
        if(menuItems[i][7].length > 1) insertString += `, "description": "${menuItems[i][7]}"`;
        insertString += "});";

        UIMenu.execute(insertString);
    }
});