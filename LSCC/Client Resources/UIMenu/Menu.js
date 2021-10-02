menuToggled = false;
let lastMenuToggle = Date.now();

if(typeof mp.storage.data.menu == "undefined")
{
    mp.storage.data.menu = {};
    mp.storage.data.menu.topRightInfo = true;
    mp.storage.data.menu.gameProgress = true;
    mp.storage.data.menu.killFeed = true;
    mp.storage.data.menu.alwaysExpandedMap = false;
    mp.storage.data.menu.updateRate = 200;

    mp.storage.flush();
}

mp.events.add('RestartMenu', () => {
    if(UIMenu !== null)
    {
        UIMenu.destroy();
        UIMenu = null;
    }

    UIMenu = mp.browsers.new("package://UIMenu/index.html");
    UIMenu.execute(`gm.mainMenu = false;`);
    UIMenu.active = false;

    mp.events.callLocal("MenuToggleTyping", false);
});

function ToggleMenu()
{
    if(UIMenu != null && !scoreboardToggled && Date.now() - lastChatToggle >= 500 && !chatStatus && mp.players.local.LoggedIn !== undefined 
        && mp.players.local.LoggedIn == true && Date.now() - lastMenuToggle >= 1000)
    {
        lastMenuToggle = Date.now();
        traceLastFunc(`[Menu] Attempt open/M key`);

        menuToggled = !menuToggled;
        UIMenu.active = menuToggled;
        //if(UIHud != null) UIHud.execute(`gm.miniHud = ${!menuToggled};`);
        UIMenu.execute(`gm.mainMenu = ${menuToggled};`);
        mp.events.callRemote("OnToggleMenu", menuToggled);
        mp.gui.cursor.show(menuToggled, menuToggled);
        mp.gui.chat.activate(!menuToggled);
        //mp.gui.chat.show(!menuToggled);

        UIMenu.execute(`gm.$refs.mainMenu.$refs.settings.settings = [
            {
              id: 1,
              type: 1,
              name: "tri",
              label: "Top Right Info",
              description: "This includes RnC Logo, Player Count, Current Lobby ID, Clock."
            },
            {
              id: 2,
              type: 1,
              name: "kf",
              label: "Kill Feed",
              description: "This includes Kill Feed."
            },
            {
              id: 3,
              type: 1,
              name: "gp",
              label: "Game Progress",
              description: "This includes In-Game Progress Timer."
            },
            {
              id: 4,
              type: 1,
              name: "bigmap",
              label: "Expanded Minimap",
              description: "Toggle permanently expanded minimap."
            },
            {
              id: 5,
              type: 2,
              name: "updr",
              label: "Update Rate",
              description: "How fast should blips & UI segments update.",
              options: [
                {
                  name: "Slow",
                  value: "1000",
                },
                {
                  name: "Normal",
                  value: "200",
                },
                {
                  name: "Fast",
                  value: "10",
                },
              ]
            },
            {
              id: 6,
              type: 3,
              name: "vcbind",
              label: "Push To Talk",
              description: "Click to change the push to talk key."
            },
            {
              id: 7,
              type: 3,
              name: "gpsbind",
              label: "GPS Key",
              description: "This is the key used to toggle GPS."
            },
          ]`);
          
        UIMenu.execute(`gm.$refs.mainMenu.$refs.settings.$refs.tri.value = ${mp.storage.data.menu.topRightInfo};`);
        UIMenu.execute(`gm.$refs.mainMenu.$refs.settings.$refs.gp.value = ${mp.storage.data.menu.gameProgress};`);
        UIMenu.execute(`gm.$refs.mainMenu.$refs.settings.$refs.kf.value = ${mp.storage.data.menu.killFeed};`);
        UIMenu.execute(`gm.$refs.mainMenu.$refs.settings.$refs.bigmap.value = ${mp.storage.data.menu.alwaysExpandedMap};`);
        UIMenu.execute(`gm.$refs.mainMenu.$refs.settings.$refs.updr.value = ${mp.storage.data.menu.updateRate};`);
        UIMenu.execute(`gm.$refs.mainMenu.$refs.settings.$refs.vcbind.value = "${String.fromCharCode(curVoiceBind)}";`);
        UIMenu.execute(`gm.$refs.mainMenu.$refs.settings.$refs.gpsbind.value = "${String.fromCharCode(curGPSBind)}";`);
    }
}

mp.keys.bind(
    0x1B,
    false,
    () => {
        if(UIMenu != null && menuToggled) ToggleMenu();
    }
);

mp.events.add("MenuToggleTyping", (state) => {
    if(state)
    {
        mp.keys.unbind(0x4D, false, ToggleMenu);
    }
    else
    {
        mp.keys.bind(0x4D, false, ToggleMenu);
    }
});

mp.events.add("MenuChangeSettings", (id, value) => 
{
    mp.gui.chat.push(`id ${id} => ${value}`);
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
    else if(id == 4)
    {
        mp.events.callLocal("playerCommand", "bigmap");
        //mp.storage.data.menu.alwaysExpandedMap = value;
    }
    else if(id == 5)
    {
        mp.events.callLocal("playerCommand", `updaterate ${value}`);
        //mp.storage.data.menu.updateRate = value;
        //updateRate = value;
    }
    else if(id == 6)
    {
        mp.events.callLocal("playerCommand", `voicebind ${value}`);
    }
    else if(id == 7)
    {
        mp.events.callLocal("playerCommand", `gpsbind ${value}`);
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

mp.events.add("MenuSelectLoadout", (name, team) => {
    mp.events.callRemote("MenuSelectedLoadout", name, team);
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

mp.events.add("UpdateMenuLoadouts", (loadouts, currentCopLoadout, currentFugiLoadout) => {
    UIMenu.execute(`gm.$refs.mainMenu.$refs.loadouts.$refs.police.loadouts = [];`);
    UIMenu.execute(`gm.$refs.mainMenu.$refs.loadouts.$refs.fugitive.loadouts = [];`);
    UIMenu.execute(`gm.$refs.mainMenu.$refs.loadouts.$refs.police.selectedLoadout = "${(currentCopLoadout == "?" ? "Default" : currentCopLoadout)}";`);
    UIMenu.execute(`gm.$refs.mainMenu.$refs.loadouts.$refs.fugitive.selectedLoadout = "${(currentFugiLoadout == "?" ? "Default" : currentFugiLoadout)}";`);

    // Loadout Data
    // Name, Locked, Background, Requirements, Weapons
    let menuItems = JSON.parse(loadouts);
    for(let i = 0; i < menuItems.length; i++)
    {
        let insertString = "";
        if(menuItems[i][0] == 0) insertString = `gm.$refs.mainMenu.$refs.loadouts.$refs.fugitive.loadouts.push({`;
        else if(menuItems[i][0] == 1) insertString = `gm.$refs.mainMenu.$refs.loadouts.$refs.police.loadouts.push({`;
        
        insertString += `"name": "${menuItems[i][1]}", `;
        insertString += `"background": "${menuItems[i][2]}", `;
        insertString += `"locked": ${menuItems[i][3]}, `;
        insertString += menuItems[i][4];
        insertString += menuItems[i][5];
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