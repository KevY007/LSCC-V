menuToggled = false;

if(typeof mp.storage.data.menu == "undefined")
{
    mp.storage.data.menu = {};
    mp.storage.data.menu.topRightInfo = true;
    mp.storage.data.menu.gameProgress = true;
    mp.storage.data.menu.killFeed = true;

    mp.storage.flush();
}

mp.events.add("MenuToggleTyping", (state) => {
    if(state)
    {
        mp.keys.unbind(0x4D, false);
    }
    else
    {
        mp.keys.bind(0x4D, false, function() {
            if(UIMenu != null && Date.now() - lastChatToggle >= 500 && chatStatus == false && mp.players.local.LoggedIn !== undefined && mp.players.local.LoggedIn == true)
            {
                traceLastFunc(`[Menu] Attempt open/M key`);
        
                menuToggled = !menuToggled;
                UIMenu.active = menuToggled;
                UIMenu.execute(`gm.mainMenu = ${menuToggled};`);
                mp.events.callRemote("OnToggleMenu", menuToggled);
                mp.gui.cursor.show(menuToggled, menuToggled);
                mp.gui.chat.activate(!menuToggled);
                //mp.gui.chat.show(!menuToggled);

                UIMenu.execute(`gm.$refs.mainMenu.$refs.settings.topRightInfo = ${mp.storage.data.menu.topRightInfo};`);
                UIMenu.execute(`gm.$refs.mainMenu.$refs.settings.gameProgress = ${mp.storage.data.menu.gameProgress};`);
                UIMenu.execute(`gm.$refs.mainMenu.$refs.settings.killFeed = ${mp.storage.data.menu.killFeed};`);
            }
        });
    }
});

mp.events.add("MenuChangeSettings", (id, value) => {
    if(id == 1)
    {
        mp.storage.data.menu.topRightInfo = value;
    }
    else if(id == 2)
    {
        mp.storage.data.menu.killFeed = value;
    }
    else if(id == 3)
    {
        mp.storage.data.menu.gameProgress = value;
    }

    if(UIHud != null)
    {
        UIHud.execute(`gm.topRightInfo = ${mp.storage.data.menu.topRightInfo}; gm.killFeed = ${mp.storage.data.menu.killFeed};`);
    }

    mp.storage.flush();
});

mp.events.add("PutPlayerInLobby", (lobbyID, password) => {
    mp.events.callRemote("RequestLobbySwitch", lobbyID, password);
});

mp.events.add("RequestJoinMinigame", (ID) => {
    mp.events.callRemote("RequestMinigameJoin", ID);
});

mp.events.add("MenuSelectVehicle", (model, team) => {
    mp.events.callRemote("MenuSelectedVehicle", model, team);
});

mp.events.add("UpdateMenuMinigames", (arenas, currentMinigame) => {
    UIMenu.execute(`gm.$refs.mainMenu.$refs.minigames.games = [];`);
    UIMenu.execute(`gm.$refs.mainMenu.$refs.minigames.currentMinigame = ${currentMinigame};`);
    
    let menuItems = JSON.parse(arenas);
    for(let i = 0; i < menuItems.length; i++)
    {
        let insertString = `gm.$refs.mainMenu.$refs.minigames.games.push({`;
        insertString += `"id": ${menuItems[i][0]}, `;
        insertString += `"name": "${menuItems[i][1]}", `;
        insertString += `"background": "${menuItems[i][2]}", `;
        insertString += `"players": ${menuItems[i][3]}, `;
        insertString += `"maxplayers": ${menuItems[i][4]}, `;
        if(menuItems[i][5].length > 1) insertString += `"gamemode": "${menuItems[i][5]}", `;
        if(menuItems[i][6].length > 1) insertString += `"description": "${menuItems[i][6]}", `;
        
        insertString += "});";

        UIMenu.execute(insertString);
    }
});

mp.events.add("UpdateMenuVehicles", (vehicles, currentCopCar, currentFugiCar) => {
    UIMenu.execute(`gm.$refs.mainMenu.$refs.vehicles.$refs.police.vehicles = [];`);
    UIMenu.execute(`gm.$refs.mainMenu.$refs.vehicles.$refs.fugitive.vehicles = [];`);
    UIMenu.execute(`gm.$refs.mainMenu.$refs.vehicles.$refs.police.selectedModel = "${currentCopCar}";`);
    UIMenu.execute(`gm.$refs.mainMenu.$refs.vehicles.$refs.fugitive.selectedModel = "${currentFugiCar}";`);

    // Vehicle Data
    // Model, Name, Background, Energy, Price, Locked
    let menuItems = JSON.parse(vehicles);
    for(let i = 0; i < menuItems.length; i++)
    {
        let insertString = "";
        if(menuItems[i][0] == 0) insertString = `gm.$refs.mainMenu.$refs.vehicles.$refs.fugitive.vehicles.push({`;
        else if(menuItems[i][0] == 1) insertString = `gm.$refs.mainMenu.$refs.vehicles.$refs.police.vehicles.push({`;
        
        insertString += `"model": "${menuItems[i][1]}", `;
        insertString += `"name": "${menuItems[i][2]}", `;
        insertString += `"background": "${menuItems[i][3]}", `;
        if(menuItems[i][4].length > 1) insertString += `"price": "${menuItems[i][4]}", `;
        insertString += `"locked": ${menuItems[i][5]}, `;
        if(menuItems[i][6].length > 1) insertString += `"energy": "${menuItems[i][6]}", `;
        insertString += "});";

        UIMenu.execute(insertString);
    }
});

mp.events.add("UpdateMenuLobbies", (lobbies, currentLobby) => {
    UIMenu.execute(`gm.$refs.mainMenu.$refs.lobbies.lobbies = [];`);
    UIMenu.execute(`gm.$refs.mainMenu.$refs.lobbies.currentLobby = ${currentLobby};`);

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