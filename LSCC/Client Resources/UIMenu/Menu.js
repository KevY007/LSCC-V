menuToggled = false;
let lastMenuToggle = Date.now();

if(typeof mp.storage.data.menu == "undefined" || typeof mp.storage.data.menu.HUD == "undefined")
{
    mp.storage.data.menu = {};
    mp.storage.data.menu.topRightInfo = true;
    mp.storage.data.menu.gameProgress = true;
    mp.storage.data.menu.killFeed = true;
    mp.storage.data.menu.alwaysExpandedMap = false;
    mp.storage.data.menu.updateRate = 200;
    mp.storage.data.menu.myweather = -1;
    mp.storage.data.menu.tooltips = true;
    mp.storage.data.menu.nametags = true;
    mp.storage.data.menu.HUD = true;
    mp.storage.data.menu.OOCauto = false;
    mp.storage.data.menu.EventMusic = true;

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

    UIMenu.execute(`gm.$refs.logs.options = [
        { name: "Sessions", value: "sessions" },
        { name: "Admin", value: "admin" },
        { name: "Records", value: "records" },
      ];`);

    mp.events.callLocal("MenuToggleTyping", false);
});

function ToggleMenu()
{
    if(scoreboardToggled)
    {
        ToggleScoreboard();
    }
    
    if(UIMenu != null && Date.now() - lastChatToggle >= 500 && !chatStatus && mp.players.local.getVariable("pLogged") !== undefined 
        && mp.players.local.getVariable("pLogged") == true && Date.now() - lastMenuToggle >= 1000)
    {
        lastMenuToggle = Date.now();
        traceLastFunc(`[Menu] Attempt open/M key`);

        menuToggled = !menuToggled;

        if(menuToggled == false)
        {
            setTimeout(() => {
                UIMenu.active = menuToggled;
            }, 750);
        }
        else UIMenu.active = menuToggled;


        //if(UIHud != null) UIHud.execute(`gm.miniHud = ${!menuToggled};`);
        UIMenu.execute(`gm.mainMenu = ${menuToggled};`);
        mp.events.callRemote("OnToggleMenu", menuToggled);
        mp.gui.cursor.show(menuToggled, menuToggled);
        mp.gui.chat.activate(!menuToggled);
        //mp.gui.chat.show(!menuToggled);

        UIMenu.execute(`gm.$refs.settings.categories = [
            {
                name: "Game",
                settings:[
                    {
                        id: 12,
                        type: 4,
                        name: "myweather",
                        label: "Weather",
                        preSelect: "Dynamic",
                        description: "Changes the weather displayed only on your screen.",
                        options: [
                          {
                              name: "Dynamic",
                              value: "-1",
                          },
                          {
                            name: "Sunny",
                            value: "0",
                          },
                          {
                            name: "Cloudy",
                            value: "2",
                          },
                          {
                            name: "Foggy",
                            value: "4",
                          },
                          {
                              name: "Rain",
                              value: "6",
                          },
                          {
                              name: "Thunderstorm",
                              value: "7",
                          },
                          {
                              name: "Snowfall",
                              value: "9",
                          },
                        ]
                    },
                    {
                        id: 14,
                        type: 1,
                        name: "autojoinopt",
                        label: "Autojoin",
                        description: "Toggles whether you will be put in copchase rounds or not."
                    },
                    {
                        id: 15,
                        type: 1,
                        name: "eventmusic",
                        label: "Event Music",
                        description: "Toggles whether background music will play in events or not."
                    }
                ]
            },
            {
                name: "Keybinds",
                settings: [
                    {
                        id: 7,
                        type: 3,
                        name: "vcbind",
                        label: "Push To Talk",
                        description: "Click to change the push to talk key."
                      },
                      {
                        id: 8,
                        type: 3,
                        name: "gpsbind",
                        label: "GPS Key",
                        description: "This is the key used to toggle GPS."
                      },
                      {
                        id: 11,
                        type: 3,
                        name: "bigmapbind",
                        label: "Expanded Minimap",
                        description: "Toggles the larger minimap view."
                      }
                ]
            },
            {
                name: "Display",
                settings: [
                    {
                        id: 9,
                        type: 1,
                        name: "hud",
                        label: "User Interface",
                        description: "Completely disable/enable the HUD."
                    },
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
                        type: 1,
                        name: "tooltips",
                        label: "Tooltips",
                        description: "Enable/disable tooltips."
                    },
                    {
                        id: 10,
                        type: 1,
                        name: "nametags",
                        label: "Nametags",
                        description: "Enable/disable nametags."
                    },
                    {
                        id: 6,
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
                    }
                ]
            },
            {
                name: "Chat",
                settings: [
                    {
                        id: 13,
                        type: 1,
                        name: "oocauto",
                        label: "Auto OOC",
                        description: "Automatically send all text messages to the global OOC chat, so you don't have to type /o every time."
                    },
                ]
            }
        ]`);
          
        UIMenu.execute(`gm.$refs.settings.$refs.Display.$refs.tri.value = ${mp.storage.data.menu.topRightInfo};`);
        UIMenu.execute(`gm.$refs.settings.$refs.Display.$refs.gp.value = ${mp.storage.data.menu.gameProgress};`);
        UIMenu.execute(`gm.$refs.settings.$refs.Display.$refs.tooltips.value = ${mp.storage.data.menu.tooltips};`);
        UIMenu.execute(`gm.$refs.settings.$refs.Display.$refs.kf.value = ${mp.storage.data.menu.killFeed};`);
        UIMenu.execute(`gm.$refs.settings.$refs.Display.$refs.nametags.value = ${mp.storage.data.menu.nametags};`);
        UIMenu.execute(`gm.$refs.settings.$refs.Display.$refs.hud.value = ${mp.storage.data.menu.HUD};`);
        UIMenu.execute(`gm.$refs.settings.$refs.Display.$refs.bigmap.value = ${mp.storage.data.menu.alwaysExpandedMap};`);
        UIMenu.execute(`gm.$refs.settings.$refs.Display.$refs.updr.value = ${mp.storage.data.menu.updateRate};`);
        UIMenu.execute(`gm.$refs.settings.$refs.Keybinds.$refs.vcbind.value = "${String.fromCharCode(curVoiceBind)}";`);
        UIMenu.execute(`gm.$refs.settings.$refs.Keybinds.$refs.gpsbind.value = "${String.fromCharCode(curGPSBind)}";`);
        UIMenu.execute(`gm.$refs.settings.$refs.Keybinds.$refs.bigmapbind.value = "${String.fromCharCode(curMapBind)}";`);
        UIMenu.execute(`gm.$refs.settings.$refs.Game.$refs.myweather.value = ${mp.storage.data.menu.myweather};`);
        UIMenu.execute(`gm.$refs.settings.$refs.Game.$refs.myweather.preSelect = ${mp.storage.data.menu.myweather};`);
        UIMenu.execute(`gm.$refs.settings.$refs.Game.$refs.autojoinopt.value = ${mp.storage.data.menu.AutoJoin};`);
        UIMenu.execute(`gm.$refs.settings.$refs.Chat.$refs.oocauto.value = ${mp.storage.data.menu.OOCauto};`);
        UIMenu.execute(`gm.$refs.settings.$refs.Chat.$refs.eventmusic.value = ${mp.storage.data.menu.EventMusic};`);
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
    if(id == 1)
    {
        mp.storage.data.menu.topRightInfo = value;
    }
    else if(id == 9)
    {
        mp.storage.data.menu.HUD = value;
        UIHud.active = value;
    }
    else if(id == 10)
    {
        mp.storage.data.menu.nametags = value;
        renderNametags = value;
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
        mp.events.callLocal("playerCommand", `bigmap ${value}`);
        //mp.storage.data.menu.alwaysExpandedMap = value;
    }
    else if(id == 5)
    {
        mp.storage.data.menu.tooltips = value;
        mp.events.callLocal("ToggleTooltips", mp.storage.data.menu.tooltips);
    }
    else if(id == 6)
    {
        mp.events.callLocal("playerCommand", `updaterate ${value}`);
        //mp.storage.data.menu.updateRate = value;
        //updateRate = value;
    }
    else if(id == 7)
    {
        mp.events.callLocal("playerCommand", `voicebind ${value}`);
    }
    else if(id == 8)
    {
        mp.events.callLocal("playerCommand", `gpsbind ${value}`);
    }
    else if(id == 11)
    {
        mp.events.callLocal("playerCommand", `bigmapbind ${value}`);
    }
    else if(id == 12)
    {
        mp.events.callRemote("SetPlayerPrivateWeather", value);
    }
    else if(id == 13)
    {
        mp.storage.data.menu.OOCauto = value;
        autoOOC = mp.storage.data.menu.OOCauto;
        mp.events.callRemote("SetAutoOOC", autoOOC);
    }
    else if(id == 14)
    {
        mp.storage.data.menu.AutoJoin = value;
        mp.players.local.AutoJoin = value
        mp.events.callRemote("SetAutoJoin", mp.players.local.AutoJoin, true);
    }
    else if(id == 15)
    {
        mp.storage.data.menu.EventMusic = value;
        mp.players.local.EventMusic = value;
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
    UIMenu.execute(`gm.$refs.minigames.games = [];`);
    UIMenu.execute(`gm.$refs.minigames.currentMinigame = ${currentMinigame};`);
    
    let menuItems = JSON.parse(arenas);
    for(let i = 0; i < menuItems.length; i++)
    {
        let insertString = `gm.$refs.minigames.games.push({`;
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
    UIMenu.execute(`gm.$refs.vehicles.$refs.police.vehicles = [];`);
    UIMenu.execute(`gm.$refs.vehicles.$refs.fugitive.vehicles = [];`);
    UIMenu.execute(`gm.$refs.vehicles.$refs.police.selectedModel = "${currentCopCar}";`);
    UIMenu.execute(`gm.$refs.vehicles.$refs.fugitive.selectedModel = "${currentFugiCar}";`);

    // Vehicle Data
    // Model, Name, Background, Energy, Price, Locked
    let menuItems = JSON.parse(vehicles);
    for(let i = 0; i < menuItems.length; i++)
    {
        let insertString = "";
        if(menuItems[i][0] == 0) insertString = `gm.$refs.vehicles.$refs.fugitive.vehicles.push({`;
        else if(menuItems[i][0] == 1) insertString = `gm.$refs.vehicles.$refs.police.vehicles.push({`;
        
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
    UIMenu.execute(`gm.$refs.loadouts.$refs.police.loadouts = [];`);
    UIMenu.execute(`gm.$refs.loadouts.$refs.fugitive.loadouts = [];`);
    UIMenu.execute(`gm.$refs.loadouts.$refs.police.selectedLoadout = "${(currentCopLoadout == "?" ? "Default" : currentCopLoadout)}";`);
    UIMenu.execute(`gm.$refs.loadouts.$refs.fugitive.selectedLoadout = "${(currentFugiLoadout == "?" ? "Default" : currentFugiLoadout)}";`);

    // Loadout Data
    // Name, Locked, Background, Requirements, Weapons
    let menuItems = JSON.parse(loadouts);
    for(let i = 0; i < menuItems.length; i++)
    {
        let insertString = "";
        if(menuItems[i][0] == 0) insertString = `gm.$refs.loadouts.$refs.fugitive.loadouts.push({`;
        else if(menuItems[i][0] == 1) insertString = `gm.$refs.loadouts.$refs.police.loadouts.push({`;
        
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
    UIMenu.execute(`gm.$refs.lobbies.lobbies = [];`);
    UIMenu.execute(`gm.$refs.lobbies.currentLobby = ${currentLobby};`);

    // Lobby Data
    // ID, Name, Background, Gamemode, Password, Players, MaxPlayers, Description
    let menuItems = JSON.parse(lobbies);
    for(let i = 0; i < menuItems.length; i++)
    {
        let insertString = `gm.$refs.lobbies.lobbies.push({`;
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