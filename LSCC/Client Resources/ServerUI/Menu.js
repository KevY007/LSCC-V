var global = require('./globals');
  const {TranslateMessage: Translate} = require('./Translator');
global.menuToggled = false;

var FirstTimeMenuOpen = true;

var ActiveCrosshairProfileUUID = "";

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
    mp.storage.data.menu.monitoring = false;
    mp.storage.data.menu.VoiceMode = 0;
    mp.storage.data.menu.InactiveChatOpacity = 0.5;
    mp.storage.data.menu.ActiveChatOpacity = 1.0;
    mp.storage.data.menu.ShowInactiveChat = true;
    mp.storage.data.menu.AutoLogin = true;
    mp.storage.data.menu.RealtimeSpeedo = false;
    mp.storage.data.menu.Speedo = true;
    mp.storage.data.menu.loadingscreens = true;    
    mp.storage.data.menu.SnowEnabled = false;
    mp.storage.data.menu.XMRAutoPlay = true;
    mp.storage.data.menu.ShowVoiceActivity = true;
    mp.storage.data.menu.ChatAccessibility = false;
    mp.storage.data.menu.ShowDamageIndicator = true;

    mp.storage.flush();
}

mp.events.add('RestartMenu', () => {
    if(global.ServerUI !== null)
    {
        if(global.ServerUI) global.ServerUI.destroy();
        global.ServerUI = null;
    }

    //global.ServerUI = mp.browsers.new("package://ServerUI/index.html");
    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.enabled = false;`);

    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.logsTab.options = [
        { name: "Sessions", value: 2 },
        { name: "Admin", value: 1 },
        { name: "Admin Records", value: 3 },
        { name: "Incoming Damages", value: 4 },
        { name: "Outgoing Damages", value: 5 },
      ];`);
    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.logsTab.filterType = {name: "Admin Records", value: 3};`);

    mp.events.callLocal("MenuToggleTyping", false);
});

mp.events.add("setSelectedProfile", (profileID, settings) => {
    //settings.enable , settings.alwaysOn , settings.onSnipers
    ActiveCrosshairProfileUUID = profileID;

    mp.events.callRemote("Crosshair_SetProfile", profileID);
});
mp.events.add("saveCrosshairProfiles", (profileData) => {
    mp.events.callRemote("ServerConsoleOutput" ,profileData);
    return;
    mp.events.callRemote("Crosshair_SaveProfiles", profileData);

    eval(`var realProfileData = ${profileData}`);

    let prData = JSON.parse(realProfileData);
    if(prData != undefined){
        let currentProfile = undefined;
        for(let i = 0 ; i < prData.length; i++){
            if(prData[i].uuid == ActiveCrosshairProfileUUID){
                currentProfile = prData[i];
                break;
            }
        }
        if(currentProfile){
            
            global.alwaysShowCustomCrosshair = currentProfile.settings.alwaysOn;
            global.showCustomCrosshairInSniper = currentProfile.settings.onSnipers;
            global.showCustomCrosshair = currentProfile.settings.enabled;
        }
    }
});

mp.events.add("LoadCrosshairProfiles", (profileData, activeProfile) => {
    //if(global.ServerUI) global.ServerUI.execute(`gm.$refs.helpers.$refs.states.setCrosshair('profiles', ${profileData});`);
    //if(global.ServerUI) global.ServerUI.execute(`gm.$refs.helpers.$refs.states.setCrosshair('selectedProfile', '${activeProfile}');`);

    eval(`var realProfileData = ${profileData}`);
    eval(`var realActiveProfile = ${activeProfile}`);
    if(global.ServerUI) global.ServerUI.call('mainmenu_settings_crosshair_set', realProfileData, realActiveProfile);
    ActiveCrosshairProfileUUID = activeProfile;
});

var MenuClans = [];
mp.events.add("clearMenuClansVariable", () => {
    mp.events.callRemote('ServerConsoleOutput', 'Cleared menu clans variable, setting new clans.');
    MenuClans = [];
});
mp.events.add("Menu_FinishAddingClans", () => {
    if(global.ServerUI) global.ServerUI.call('mainmenu_clan_search_clans_set', MenuClans);
});
mp.events.add("Menu_AddClan", (clanID, cname, tag, desc, banner, isOpen, players, maxPlayers, kills, deaths, wins, ownerName) => {
    MenuClans.push({
        id: clanID,
        name: cname,
        tag: tag,
        description: desc,
        banner:
          (banner == "default" ? "https://www.igta5.com/images/official-artwork-blitz-play.jpg" : banner),
        open: isOpen,
        players: players,
        maxplayers: maxPlayers,
        owner: ownerName,
        stats: {
          kills: kills,
          deaths: deaths,
          wins: wins,
        },
    });
    mp.events.callRemote('ServerConsoleOutput', `Menu_AddClan: Added ${clanID} - ${cname}`);
});

mp.events.add('mainmenu_clan_my_clan_delete', () => {
    mp.events.callRemote("Clan_OnRequestDelete");
});

mp.events.add("mainmenu_clan_search_join", (clanSQLID) => {
    mp.events.callRemote("Clan_OnRequestJoinClan", clanSQLID);
});
mp.events.add("mainmenu_clan_my_clan_members_kick", (memberSQLID) => {
    mp.events.callRemote("Clan_OnRequestKickPlayer", memberSQLID);
});
mp.events.add("mainmenu_clan_my_clan_members_change_rank", (memberSQLID, tier) => {
    mp.events.callRemote("Clan_OnRequestChangeTier", memberSQLID, tier);
});

mp.events.add("mainmenu_clan_my_clan_members_invite", (playerName) => {
    mp.events.callRemote("Clan_OnRequestInvitePlayer", playerName);
});

mp.events.add("mainmenu_clan_my_clan_leave", () => {
    mp.events.callRemote("OnRequestLeaveClan");
});
/*
    CLAN SETTING IDs and VALUES

    [1] => Clan Access
    (VALUES) => 0 - Invite-only ; 1 - Public
    [2] => Force Clan Tag
    (VALUES) => true/false
    [3] => Copchase Partnering
    (VALUES) => true/false
*/
mp.events.add("mainmenu_clan_my_clan_setting_update", (settingID, value) => {
    let numValue = Number(value); // be careful in the future if a setting isn't bool nor number

    mp.events.callRemote("Clan_OnRequestChangeSetting", settingID, numValue);
});

mp.events.add("Menu_RemoveClanMember", (memberID) => {
    if(global.ServerUI) global.ServerUI.execute(`
        if(gm.$refs.mainMenu.$refs.clansTab.$refs.myClan.$refs.membersTab.members.find(m => m.id == ${memberID})){
            gm.$refs.mainMenu.$refs.clansTab.$refs.myClan.$refs.membersTab.members.splice(gm.$refs.mainMenu.$refs.clansTab.$refs.myClan.$refs.membersTab.members.indexOf(gm.$refs.mainMenu.$refs.clansTab.$refs.myClan.$refs.membersTab.members.find(m => m.id == ${memberID})), 1);
        }
    `);
});
mp.events.add("Menu_UpdateClanMember", (memberID, memberName, memberTier, memberLevel) => {
    if(global.ServerUI) global.ServerUI.execute(`
        if(gm.$refs.mainMenu.$refs.clansTab.$refs.myClan.$refs.membersTab.members.find(m => m.id == ${memberID})){
            gm.$refs.mainMenu.$refs.clansTab.$refs.myClan.$refs.membersTab.members[gm.$refs.mainMenu.$refs.clansTab.$refs.myClan.$refs.membersTab.members.indexOf(gm.$refs.mainMenu.$refs.clansTab.$refs.myClan.$refs.membersTab.members.find(m => m.id == ${memberID}))] = {
                id: ${memberID},
                name: '${memberName}',
                level: ${memberLevel},
                tier: ${memberTier}
            }
        }

    `);
});

mp.events.add("Menu_InitClanSettings", (clanAccess, copchasePartnering, forceTag) => {
    if(global.ServerUI) global.ServerUI.execute(`
        gm.$refs.mainMenu.$refs.clansTab.$refs.myClan.$refs.settingsTab.copchasePartnering = ${copchasePartnering.toString()};   
        gm.$refs.mainMenu.$refs.clansTab.$refs.myClan.$refs.settingsTab.forceClanTag = ${forceTag.toString()};    
        gm.$refs.mainMenu.$refs.clansTab.$refs.myClan.$refs.settingsTab.clanAccess = "${clanAccess}";   
    `);
});
mp.events.add("Menu_UpdateClanSetting", (type, value) => {
    switch(type){
        case "access":{ 
            if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.clansTab.$refs.myClan.$refs.settingsTab.clanAccess = "${value}";`);
            break;
        }
        case "partnering":{ 
            if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.clansTab.$refs.myClan.$refs.settingsTab.copchasePartnering = ${value.toString()};`);
            break;
        }
        case "forcetag":{ 
            if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.clansTab.$refs.myClan.$refs.settingsTab.forceClanTag = ${value.toString()};`);
            break;
        }
    }
});

mp.events.add("Menu_SetClanMembers", (membersData) => {
    var membersDataJSON = JSON.parse(membersData);
    for(let i = 0 ; i < membersDataJSON.length ; i++){
        let clanPlayer = mp.players.toArray().find(p => p.name.toLowerCase() === membersDataJSON[i].name.toLowerCase());
        let isOnline = (clanPlayer !== undefined);
        
        membersDataJSON[i].online = isOnline;
        if(isOnline)
            membersDataJSON[i].badges = JSON.parse(clanPlayer.getVariable("BadgeData"));
    }
    mp.events.callRemote('ServerConsoleOutput', JSON.stringify(membersDataJSON));
    if(global.ServerUI) global.ServerUI.call('mainmenu_my_clan_members_set', membersDataJSON);
});

mp.events.add("Menu_AddClanMember", (memberID, memberName, memberTier, memberLevel, isOnline) => {

    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.clansTab.$refs.myClan.$refs.membersTab.members.push(
        {
            id: ${memberID},
            name: '${memberName}',
            level: ${memberLevel},
            tier: ${memberTier},
            online: ${isOnline.toString()}
        });`);
});
mp.events.add("Menu_ClearClanMembers", () => {
    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.clansTab.$refs.myClan.$refs.membersTab.members = [];`);
});

mp.events.add("Menu_DisableClanCreatorLoading", () => {
    if(global.ServerUI) global.ServerUI.execute()
});

mp.events.add("Menu_UpdateClanStats", (weekKills, totalKills, weekWins) => {
    if(global.ServerUI) global.ServerUI.call('mainmenu_clan_my_clan_stats_set',[
        {
            icon: "fa-crosshairs",
            title: "Week Kills",
            value: weekKills
        },
        {
            icon: "fa-skull",
            title: "Total Kills",
            value: totalKills
        },
        {
            icon: "fa-trophy",
            title: "COTW Wins",
            value: weekWins
        }
    ]);
});

mp.events.add("mainmenu_clan_create", (cname, tag, desc) => {
    mp.events.callRemote("OnRequestCreateClan", cname, tag, desc);
});
mp.events.add("UpdateMenuClanTab", () => {
    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.clansTab.$refs.clansList.clans = [];`);
    setTimeout(() => {
        mp.events.callRemote("Menu_RequestClansList");
        if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.clansTab.$refs.myClan.name = "${mp.players.local.getVariable("ClanName")}"`);
        if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.clansTab.$refs.myClan.createdAt = "${mp.players.local.getVariable("ClanCreated")}"`);
        mp.events.callRemote("Menu_InitializeMyClanTab");  
    }, 100);

});


mp.events.add("render", () => {
    if(global.menuToggledLate == true){
        mp.game.controls.disableControlAction(0, 200, true); // prevent menu from opening when pressing ESC for chat 
    }    
});
mp.keys.bind(0x1B, true, function() { // ESC key
    if(global.menuToggled == true){
        ToggleMenu();     
    }
});

mp.events.add("ForceCloseMenu", () => {
    if(global.menuToggled == true){
        ToggleMenu();     
    }
    if(global.ServerUI) global.ServerUI.execute(`gm._pStores.globals.isMainMenuOpen = false;`);
    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.enabled = false;`);
});

function menuSetting_getOptionNameFromValue(type, value){
    switch(type){
        case "weather":{
            switch(Number(value)){
                case -1: return "Dynamic";
                case 0: return "Sunny";
                case 2: return "Cloudy";
                case 3: return "Smog";
                case 4: return "Foggy";
                case 5: return "Overcast";
                case 6: return "Rain";
                case 7: return "Thunderstorm";
                case 9: return "Neutral";
                case 10: return "Snowfall";
                case 11: return "Blizzard";
                case 12: return "Snowlight";
                default: return "Unknown";
            }
            break;
        }
        case "voicemode":{
            switch(Number(value)){
                case 0: return "Nearby";
                case 1: return "Team";
                case 2: return "Squad";
                case 3: return "Disabled";
            }         
            break;   
        }
        case "activechatopacity":{
            return `${Number(value)*100}`;      
            break;   
        }
        case "inactivechatopacity":{
            return `${Number(value)*100}`;      
            break;   
        }
        case "language":{
            switch(Number(value)){
                case 0: return "English";
                case 1: return "Spanish/Español";
                case 2: return "French/Français";
                case 3: return "Croatian/Hrvatski";
                case 4: return "Vietnamese/Tiếng Việt";
            }        
        }
    }
}

mp.events.add("UpdateMenuStatsTab", (email, admin, points, dpoints, dlevel, derbywins, escapes, arrests,
                                     kills, deaths, kdr, playtime, warns, discordlinked) => {

    if(global.ServerUI) global.ServerUI.call(`mainmenu_profile_stats_set`, [
            {
                icon: "fa-mailbox",
                title: "E-Mail",
                value: `${email}`,
            },
            {
                icon: "fa-shield-quartered",
                title: "Admin",
                value: `${admin}`,
            },
            {
                icon: "fa-coins",
                title: "Points",
                value: `${points}`,
            },
            {
                icon: "fa-treasure-chest",
                title: "Donator Points",
                value: `${dpoints}`,
            },
            {
                icon: "fa-gem",
                title: "Donator Level",
                value: `${dlevel}`,
            },
            {
                icon: "fa-truck-monster",
                title: "Derby Wins",
                value: `${derbywins}`,
            },
            {
                icon: "fa-person-to-portal",
                title: "Escapes",
                value: `${escapes}`,
            },
            {
                icon: "fa-handcuffs",
                title: "Arrests",
                value: `${arrests}`,
            },
            {
                icon: "fa-crosshairs",
                title: "Kills",
                value: `${kills}`,
            },
            {
                icon: "fa-skull",
                title: "Deaths",
                value: `${deaths}`,
            },
            {
                icon: "fa-scale-unbalanced",
                title: "K/D Ratio",
                value: `${kdr.toFixed(2)}`,
            },
            {
                icon: "fa-clock",
                title: "Play Time",
                value: `${playtime} hours`,
            },
            {
                icon: "fa-triangle-exclamation",
                title: "Warns",
                value: `${warns}`,
            },
            {
                icon: "fa-clouds",
                title: "Discord",
                value: `${(discordlinked == true ? "Your account is verified." : "Your account isn't verified! (/verify)")}`,
            }
        ]);

    // `gm.$refs.mainMenu.$refs.profileTab.$refs.statsTab.stats = {
    //     {
    //         icon: "fa-crosshairs",
    //         title: "Kills",
    //         value: 15,
    //       },
    // }`
});

/*
play
store
inventory
profile
clans
wiki
settings
*/

mp.events.add("mainmenu_tab_request_data", (tab) => {
    // store inventory play wiki settings profile clans
    switch(tab){
        case "clan":
            if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.clansTab.$refs.clansList.clans = [];`);
            if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.clansTab.$refs.myClan.name = "${mp.players.local.getVariable("ClanName")}"`);
            if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.clansTab.$refs.myClan.createdAt = "${mp.players.local.getVariable("ClanCreated")}"`);

            mp.events.callRemote("Menu_RequestClansList");
            mp.events.callRemote("Menu_InitializeMyClanTab");
            break;
        case "play":
            mp.events.callRemote("Menu_RequestTabData", tab);
            break;
        case "profile":
            mp.events.callRemote("Menu_RequestTabData", tab);
            break;
        case "inventory":
            mp.events.callRemote("Menu_RequestTabData", tab);
            break;
        case "store":
            mp.events.callRemote("Menu_RequestTabData", tab);
            break;
        case "settings":
            updateMenuSettingsTab();
            break;         
    }
});

function ToggleMenu()
{
    if(global.MDCActive !== undefined && global.MDCActive == true){
        global.ToggleMDC();
    }
    if((global.VehicleSpawnerActive !== undefined && global.VehicleSpawnerActive == true) ||
        (global.WeaponEditorActive !== undefined && global.WeaponEditorActive == true) ||
        (global.ActionMenuActive !== undefined && global.ActionMenuActive == true) ||
        (global.ClothingEditorActive !== undefined && global.ClothingEditorActive == true) ||
        (global.isHotwiring !== undefined && global.isHotwiring == true)){
        return;
    }
    if(global.scoreboardToggled)
    {
        ToggleScoreboard();
    }
    if(global.ServerUI != null && Date.now() - global.lastChatToggle >= 500 && global.chatStatus == false && mp.players.local.getVariable("pLogged") !== undefined 
        && mp.players.local.getVariable("pLogged") == true && Date.now() - lastMenuToggle >= 100)
    {
        lastMenuToggle = Date.now();
        global.traceLastFunc(`[Menu] Attempt open/M key`);

        global.menuToggled = !global.menuToggled;

        //if(global.ServerUI != null) if(global.ServerUI) global.ServerUI.execute(`gm.miniHud = ${!global.menuToggled};`);
        if(global.ServerUI) global.ServerUI.execute(`gm._pStores.globals.isMainMenuOpen = ${global.menuToggled};`);
        if(global.ServerUI) global.ServerUI.call(`mainmenu_toggle`, global.menuToggled);
        
        if(global.ServerUI) global.ServerUI.call('hud_toggle', (global.menuToggled ? false : mp.storage.data.menu.HUD));
        mp.events.callRemote("OnToggleMenu", global.menuToggled);
        mp.gui.cursor.show(global.menuToggled, global.menuToggled);
        global.activateChat(!global.menuToggled);
        mp.events.callRemote("RequestStatsForStatsTab");
        global.UpdateStatsVars(mp.players.local);

        if(global.ServerUI) global.ServerUI.call('mainmenu_settings_crosshair_request_save');

        if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.playTab.privateRoom = undefined;`);

        if(global.ServerUI) global.ServerUI.execute('gm.$refs.mainMenu.$refs.logsTab.isLoading = false;');

        if(global.menuToggled){
            //if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.clansTab.$refs.clansList.clans = [];`);
            //mp.events.callRemote("Menu_RequestClansList");
            //if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.clansTab.$refs.myClan.name = "${mp.players.local.getVariable("ClanName")}"`);
            //if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.clansTab.$refs.myClan.createdAt = "${mp.players.local.getVariable("ClanCreated")}"`);
            if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.profileTab.name = '${mp.players.local.name}'`);
            if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.profileTab.createdAt = '${mp.players.local.getVariable("RegisterDate")}'`);

            

            // Testing for now, loads all clan data such as members, clan kills, etc.

            if(FirstTimeMenuOpen){
                FirstTimeMenuOpen = false;

                mp.events.callRemote("Menu_RequestClanSettings");
            }
            global.menuToggledLate = true;
        }
        else{
            setTimeout(() => {global.menuToggledLate = false;}, 500);
        }
        
        /*
          {
            label: "Croatian/Hrvatski",
            value: "3",
          },
        */
    }
}

mp.keys.bind(0x4D, false, ToggleMenu);
/*
mp.keys.bind(
    0x1B,
    false,
    () => {
        if(global.ServerUI != null && global.menuToggled) ToggleMenu();
    }
);*/

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
function updateMenuSettingsTab(){
    if(global.ServerUI) global.ServerUI.call(`mainmenu_settings_set`, [
        {
            name: "Game",
            icon: "fa-duotone fa-gamepad-modern",
            settings:[
                {
                    id: 29,
                    type: 2,
                    name: "serverlanguage",
                    label: "Language",
                    description: "Changes the language in which *most* server messages are displayed. Want to help with translations? Join our Discord for more info.",
                    options: [
                      {
                        label: "English",
                        value: "0",
                      },
                      {
                        label: "Spanish/Español",
                        value: "1",
                      },
                      {
                        label: "French/Français",
                        value: "2",
                      },
                      {
                        label: "Vietnamese/Tiếng Việt",
                        value: "4",
                      }
                    ]
                },
                {
                    id: 12,
                    type: 2,
                    name: "myweather",
                    label: "Weather",
                    description: "Changes the weather displayed only on your screen.",
                    options: [
                      {
                          label: "Dynamic",
                          value: "-1",
                      },
                      {
                        label: "Sunny",
                        value: "0",
                      },
                      {
                        label: "Cloudy",
                        value: "2",
                      },    {
                        label: "Smog",
                          value: "3",
                      },
                      {
                        label: "Foggy",
                        value: "4",
                      },
                      {
                        label: "Overcast",
                          value: "5",
                      },
                      {
                        label: "Rain",
                          value: "6",
                      },
                      {
                        label: "Thunderstorm",
                        value: "7",
                      },   
                      {
                        label: "Neutral",
                          value: "9",
                      },
                      {
                        label: "Snowfall",
                          value: "10",
                      }, 
                      {
                        label: "Blizzard",
                          value: "11",
                      }, 
                      {
                        label: "Snowlight",
                          value: "12",
                      },
                    ]
                },
                {
                    id: 28,
                    type: 1,
                    name: "enablesnow",
                    label: "Snowy Map",
                    description: "Toggles snow in the map of San Andreas (on the roads, etc.)."
                },
                {
                    id: 14,
                    type: 1,
                    name: "autojoinopt",
                    label: "Autojoin",
                    description: "Toggles whether you will be put in copchase rounds or not."
                },
                {
                    id: 30,
                    type: 1,
                    name: "autoplayradio",
                    label: "XMR Auto-Play",
                    description: "If toggled on, your most recent radio station will automatically be set upon entering a car without a radio station set."
                },
                {
                    id: 15,
                    type: 1,
                    name: "eventmusic",
                    label: "Event Music",
                    description: "Toggles whether background music will play in events or not."
                },
                {
                    id: 26,
                    type: 1,
                    name: "carradio",
                    label: "GTA Car Radio",
                    description: "Toggles whether you'll hear the in-game car radio music. NOTE: Due to GTAV sync, if the driver has disabled radio, you won't hear anything too!"
                },
                {
                    id: 27,
                    type: 1,
                    name: "loadingscreens",
                    label: "Enable Loading Screens",
                    description: "Toggles the loading screens (e.g. the one at beginning of copchase) on or off. Disable this if you are experiencing issues with long loading screens."
                },
                {
                    id: 21,
                    type: 1,
                    name: "autologintog",
                    label: "Auto Login",
                    description: "Automatically log in to this account - eliminates the need to enter your login info."
                },
                {
                    id: 22,
                    type: 1,
                    name: "realtimespeedo",
                    label: "Real-time Speedometer Updates",
                    description: "If toggled on, the speedometer will update in real-time (every frame). This option might have a bad effect on your performance."
                }
            ]
        },
        {
            name: "Keybinds",
            icon: "fa-duotone fa-keyboard",
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
                  },
                  {
                    id: 23,
                    type: 3,
                    name: "actionmenubind",
                    label: "Quick-Action Menu",
                    description: "Opens the quick-action menu when held down, closes it when key is let go."
                  },
                  {
                    id: 24,
                    type: 3,
                    name: "mdcbind",
                    label: "Police MDC",
                    description: "Toggles the police Mobile Data Computer."
                  }
            ]
        },
        {
            name: "Display",
            icon: "fa-duotone fa-tv",
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
                    id: 33,
                    type: 1,
                    name: "damageindicator",
                    label: "Damage Indicators",
                    description: "Enable/disable damage indicators."
                },
                {
                    id: 25,
                    type: 1,
                    name: "togspeedo",
                    label: "Speedometer",
                    description: "Enable/disable car speedometer."
                },
                {
                    id: 6,
                    type: 2,
                    name: "updr",
                    label: "Update Rate",
                    description: "How fast should blips & UI segments update.",
                    options: [
                      {
                        label: "Slow",
                        value: "1000",
                      },
                      {
                        label: "Normal",
                        value: "200",
                      },
                      {
                        label: "Fast",
                        value: "10",
                      },
                    ]
                },
                {
                    id: 16,
                    type: 1,
                    name: "monitor",
                    label: "FPS/Ping Monitoring",
                    description: "Toggles whether your FPS & ping will be displayed in the top right corner."
                }
            ]
        },
        {
            name: "Chat",
            icon: "fa-duotone fa-messages",
            settings: [
                {
                    id: 13,
                    type: 1,
                    name: "oocauto",
                    label: "Auto OOC",
                    description: "Automatically send all text messages to the global OOC chat, so you don't have to type /o every time."
                },
                {
                    id: 31,
                    type: 1,
                    name: "showvoice",
                    label: "Voice Activity Display",
                    description: "Toggles whether to show a list of users that's currently talking or not."
                },
                {
                    id: 17,
                    type: 2,
                    name: "voicemode",
                    label: "Voice Mode",
                    description: "Sets the voice channel in which you speak. (SHORTCUT: N+TAB)",
                    default: "Nearby",
                    options: [
                      {
                        label: "Nearby",
                        value: "0",
                      },
                      {
                        label: "Team",
                        value: "1",
                      },
                      {
                        label: "Squad",
                        value: "2",
                      },
                      {
                        label: "Disabled",
                        value: "3",
                      },
                    ]
                },
                {
                    id: 32,
                    type: 1,
                    name: "chataccessibility",
                    label: "Accessibility Mode",
                    description: "Toggles accessibility mode to make chat more visible (outline on text, etc.)"
                },
            ]
        }
    ]);

    if(global.ServerUI){
        global.ServerUI.call("mainmenu_setting_value_set", 1, mp.storage.data.menu.topRightInfo);
        global.ServerUI.call("mainmenu_setting_value_set", 2, mp.storage.data.menu.killFeed);
        global.ServerUI.call("mainmenu_setting_value_set", 3, mp.storage.data.menu.gameProgress);
        global.ServerUI.call("mainmenu_setting_value_set", 4, mp.storage.data.menu.alwaysExpandedMap);
        global.ServerUI.call("mainmenu_setting_value_set", 5, mp.storage.data.menu.tooltips);
        global.ServerUI.call("mainmenu_setting_value_set", 6, mp.storage.data.menu.updateRate.toString());
        global.ServerUI.call("mainmenu_setting_value_set", 7, String.fromCharCode(global.curVoiceBind));
        global.ServerUI.call("mainmenu_setting_value_set", 8, String.fromCharCode(global.curGPSBind));
        global.ServerUI.call("mainmenu_setting_value_set", 9, mp.storage.data.menu.HUD);
        global.ServerUI.call("mainmenu_setting_value_set", 10, mp.storage.data.menu.nametags);
        global.ServerUI.call("mainmenu_setting_value_set", 11, String.fromCharCode(global.curMapBind));
        global.ServerUI.call("mainmenu_setting_value_set", 12, mp.storage.data.menu.myweather.toString());
        global.ServerUI.call("mainmenu_setting_value_set", 13, mp.storage.data.menu.autoOOC);
        global.ServerUI.call("mainmenu_setting_value_set", 14, mp.storage.data.menu.AutoJoin);
        global.ServerUI.call("mainmenu_setting_value_set", 15, mp.storage.data.menu.EventMusic);
        global.ServerUI.call("mainmenu_setting_value_set", 16, mp.storage.data.menu.monitoring);
        global.ServerUI.call("mainmenu_setting_value_set", 17, mp.storage.data.menu.VoiceMode.toString());
        global.ServerUI.call("mainmenu_setting_value_set", 18, mp.storage.data.menu.ActiveChatOpacity);
        global.ServerUI.call("mainmenu_setting_value_set", 19, mp.storage.data.menu.InactiveChatOpacity);
        global.ServerUI.call("mainmenu_setting_value_set", 20, mp.storage.data.menu.ShowInactiveChat);
        global.ServerUI.call("mainmenu_setting_value_set", 21, mp.storage.data.menu.AutoLogin);
        global.ServerUI.call("mainmenu_setting_value_set", 22, mp.storage.data.menu.RealtimeSpeedo);
        global.ServerUI.call("mainmenu_setting_value_set", 23, String.fromCharCode(global.curActionMenuBind));
        global.ServerUI.call("mainmenu_setting_value_set", 24, String.fromCharCode(global.curMDCBind));
        global.ServerUI.call("mainmenu_setting_value_set", 25, mp.storage.data.menu.Speedo);
        global.ServerUI.call("mainmenu_setting_value_set", 26, mp.storage.data.radiotoggle);
        global.ServerUI.call("mainmenu_setting_value_set", 27, mp.storage.data.menu.loadingscreens);
        global.ServerUI.call("mainmenu_setting_value_set", 28, mp.storage.data.menu.SnowEnabled);
        global.ServerUI.call("mainmenu_setting_value_set", 29, mp.storage.data.language.toString());
        global.ServerUI.call("mainmenu_setting_value_set", 30, mp.storage.data.menu.XMRAutoPlay);
        global.ServerUI.call("mainmenu_setting_value_set", 31, mp.storage.data.menu.ShowVoiceActivity);
        global.ServerUI.call("mainmenu_setting_value_set", 32, mp.storage.data.menu.ChatAccessibility);
        global.ServerUI.call("mainmenu_setting_value_set", 33, mp.storage.data.menu.ShowDamageIndicator);

        global.ServerUI.call("mainmenu_setting_value_set", 69420, global.showCustomCrosshair);
        global.ServerUI.call("mainmenu_setting_value_set", 69421, global.alwaysShowCustomCrosshair);
        global.ServerUI.call("mainmenu_setting_value_set", 69422, global.showCustomCrosshairInSniper);
    }
}

mp.events.add("report_player", (username, reason) => {
    mp.events.callRemote("Menu_ReportPlayer", username, reason);
});

// ID 69420 ==> Custom Crosshair Toggle | ID 69421 ==> Always On Crosshair |  ID 69422 ==> Sniper Custom or Default crosshair
mp.events.add("mainmenu_setting_change", (id, value) => 
{
    if(id == 1)
    {
        mp.storage.data.menu.topRightInfo = value;
    }
    else if(id == 9)
    {
        mp.storage.data.menu.HUD = value;
        if(global.ServerUI) global.ServerUI.call('hud_toggle', value);
    }
    else if(id == 10)
    {
        mp.storage.data.menu.nametags = value;
        global.renderNametags = value;
    }
    else if(id == 2)
    {
        mp.storage.data.menu.killFeed = value;
        if(global.ServerUI) global.ServerUI.call('hud_info_killfeed_toggle', value);
    }
    else if(id == 3)
    {
        mp.storage.data.menu.gameProgress = value;
    }
    else if(id == 4)
    {
        //mp.events.callLocal("playerCommand", `bigmap ${value}`);
        mp.storage.data.menu.alwaysExpandedMap = value;
        mp.game.ui.setRadarBigmapEnabled(value, false);
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
        //global.updateRate = value;
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
        mp.storage.data.menu.myweather = value;
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
    else if(id == 16)
    {
        mp.storage.data.menu.monitoring = value;
        if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.topRight.$refs.info.monitorEnabled = ${value}`);
    }
    else if(id == 17)
    {
        mp.storage.data.menu.VoiceMode = value;
        mp.events.callRemote("SetVoiceMode", value, true);
    }
    else if(id == 18)
    {
        mp.storage.data.menu.ActiveChatOpacity = value;
        increaseChatOpacity(500);
    }
    else if(id == 19)
    {
        mp.storage.data.menu.InactiveChatOpacity = value;
        increaseChatOpacity(500);
    }
    else if(id == 20)
    {
        mp.storage.data.menu.ShowInactiveChat = value;
        increaseChatOpacity(500);
    }
    else if(id == 21)
    {
        mp.storage.data.menu.AutoLogin = value;
        mp.events.callRemote("Server_ToggleAutoLogin", value);
    }
    else if(id == 22)
    {
        mp.storage.data.menu.RealtimeSpeedo = value;
    }
    else if(id == 23)
    {
        changeActionMenuBind(value);
    }
    else if(id == 24)
    {
        changeMdcBind(value);
    }
    else if(id == 25){
        mp.storage.data.menu.Speedo = value;
        if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.bottomRight.$refs.speedo.enabled = ${value};`);
    }
    else if(id == 26){
        mp.storage.data.radiotoggle = value;
        global.radioToggle = value;
    }
    else if(id == 27){
        mp.storage.data.menu.loadingscreens = value;
    }
    else if(id == 28){
        mp.storage.data.menu.SnowEnabled = value;
        mp.game1.gameplay.enableSnow = value;
    }
    else if(id == 29){
        mp.events.callLocal("Translator:ChangeLanguage", value, false);
        mp.events.callLocal("Translator:UpdateNametagMessagesManual");
    }
    else if(id == 30){
        mp.storage.data.menu.XMRAutoPlay = value;
    }
    else if(id == 31){
        mp.storage.data.menu.ShowVoiceActivity = value;
        if(global.ServerUI) global.ServerUI.call('hud_voiceactivity_toggle', value.toString());
    }
    else if(id == 32){
        mp.storage.data.menu.ChatAccessibility = value;
        if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.api.accessibilityMode = ${value.toString()}`);
    }
    else if(id == 33){
        mp.storage.data.menu.ShowDamageIndicator = value;
    }

    if(global.ServerUI != null)
    {
        global.ServerUI.call('hud_info_toggle', mp.storage.data.menu.topRightInfo);
    }

    mp.storage.flush();
});


function changeActionMenuBind(bind){
    bind = Number(bind);

    if (!Number.isInteger(bind)) {
        return;
    }

    if(mp.storage.data.actionmenubind !== undefined){
        mp.keys.unbind(mp.storage.data.actionmenubind, true, global.actionMenuOpen);
        mp.keys.unbind(mp.storage.data.actionmenubind, false, global.actionMenuClose);
    }
    else{
        mp.keys.unbind(0x59, true, global.actionMenuOpen);
        mp.keys.unbind(0x59, false, global.actionMenuClose);
    }

    mp.storage.data.actionmenubind = bind;

    mp.storage.flush();

    global.lastActionMenuBind = global.curActionMenuBind;
    global.curActionMenuBind = bind;

    mp.keys.unbind(global.lastActionMenuBind, true, global.actionMenuOpen);
    mp.keys.unbind(global.lastActionMenuBind, false, global.actionMenuClose);
    mp.keys.bind(global.curActionMenuBind, true, global.actionMenuOpen);
    mp.keys.bind(global.curActionMenuBind, false, global.actionMenuClose);

    mp.events.callLocal("HUDNotify", `MDC key is now binded to <strong>${String.fromCharCode(global.curActionMenuBind)}</strong>.`, "blue", "fas fa-check", false, 
        1000, "bottomCenter", "replace");

    mp.events.callLocal("RemoveTooltip", String.fromCharCode(global.lastActionMenuBind));
}
function changeMdcBind(bind){
    bind = Number(bind);

    if (!Number.isInteger(bind)) {
        return;
    }

    if(mp.storage.data.mdcbind !== undefined){
        mp.keys.unbind(mp.storage.data.mdcbind, true, global.ToggleMDC);
    }
    else{
        mp.keys.unbind(0x4B, true, global.ToggleMDC);
    }

    mp.storage.data.mdcbind = bind;

    mp.storage.flush();

    global.lastMDCBind = global.curMDCBind;
    global.curMDCBind = bind;

    mp.keys.unbind(global.lastMDCBind, true, global.ToggleMDC);
    mp.keys.bind(global.curMDCBind, true, global.ToggleMDC);

    mp.events.callLocal("HUDNotify", `MDC key is now binded to <strong>${String.fromCharCode(global.curMDCBind)}</strong>.`, "blue", "fas fa-check", false, 
        1000, "bottomCenter", "replace");

    mp.events.callLocal("RemoveTooltip", String.fromCharCode(global.lastMDCBind));
}

mp.events.add("Menu_UpdateLevelXP", (first_time = false) => {
    setTimeout(() => {
        global.UpdateStatsVars();
    }, (first_time ? 1000 : 1));
})

mp.events.add("PutPlayerInLobby", (lobbyID, password) => {
    mp.events.callRemote("Server_RequestLobbySwitch", lobbyID, password);
    ToggleMenu();
});

mp.events.add("RequestJoinMinigame", (ID) => {

    if(ID === 6969){
        mp.events.callRemote("Server_RequestMinigameJoin", -3);
    }
    else if(ID === 6970){
        mp.events.callRemote("Server_RequestMinigameJoin", -4);
    }
    else{
        mp.events.callRemote("Server_RequestMinigameJoin", ID - 69420);
    }

    ToggleMenu();
});

mp.events.add("MenuSelectVehicle", (model, team) => {
    mp.events.callRemote("MenuSelectedVehicle", model, team);

    if(team == 1){
        if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.inventoryTab.$refs.vehiclesTab.selectedVehiclePolice = "${model}"`);
        if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.storeTab.$refs.vehiclesTab.selectedVehiclePolice = "${model}"`);
    }
    else{
        if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.inventoryTab.$refs.vehiclesTab.selectedVehicleFugitive = "${model}"`);
        if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.storeTab.$refs.vehiclesTab.selectedVehicleFugitive = "${model}"`);
    }
});

mp.events.add("MenuSelectLoadout", (name, team) => {
    mp.events.callRemote("MenuSelectedLoadout", name, team);

    if(team == 1){
        if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.inventoryTab.$refs.loadoutsTab.selectedLoadoutPolice = "${name}"`);
        if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.storeTab.$refs.loadoutsTab.selectedLoadoutPolice = "${name}"`);
    }
    else{
        if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.storeTab.$refs.loadoutsTab.selectedLoadoutFugitive = "${name}"`);
        if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.inventoryTab.$refs.loadoutsTab.selectedLoadoutFugitive = "${name}"`);
    }
});

function minigameNameToID(name){
    switch(name){
        case "Freeroam":
            return 4;
        case "Derby":
            return 3;
        case "DM":
            return 2;
        default:
            return -1;
    }
}

mp.events.add("UpdateMenuMinigames", (arenas, currentMinigame) => {
    //if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.minigamesTab.games = [];`);
    
    let menuItems = JSON.parse(arenas);
    for(let i = 0; i < menuItems.length; i++)
    {

        let insertString = `gm.$refs.mainMenu.$refs.playTab.rooms.push({`;
        insertString += `"id": ${(menuItems[i][0]+69420)}, `;
        insertString += `"name": "${menuItems[i][1]}", `;
        insertString += `"image": "${menuItems[i][2]}", `;
        insertString += `"players": ${menuItems[i][3]}, `;
        insertString += `"maxplayers": ${menuItems[i][4]}, `;
        if(menuItems[i][5].length > 1) insertString += `"type": ${minigameNameToID(menuItems[i][5])}, `;
        insertString += `"joinFunction":"RequestJoinMinigame"`;
        if(menuItems[i][6].length > 1) insertString += `"description": "${menuItems[i][6]}", `;
        
        insertString += "});";

        if(global.ServerUI) global.ServerUI.execute(insertString);
    }

    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.playTab.rooms.push({
        id: 6969,
        type: 4,
        name: "Freeroam",
        image:
        "package://Artwork/Freeroam.png",
        players: ${menuItems[0][3]},
        maxplayers: 1000,
        joinFunction: "RequestJoinMinigame",
    });`);
    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.playTab.rooms.push({
        id: 6970,
        type: 3,
        name: "Derby",
        image:
        "package://Artwork/Derby.jpg",
        players: ${menuItems[1][3]},
        maxplayers: 20,
        joinFunction: "RequestJoinMinigame",
    });`);

    if(currentMinigame !== -1){
        if(currentMinigame !== -3 && currentMinigame !== -4){
            if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.playTab.currentRoom = ${currentMinigame + 69420};`);
        }
        else{
            if(currentMinigame === -3)
                if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.playTab.currentRoom = 6969;`);

            if(currentMinigame === -4)
                if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.playTab.currentRoom = 6970;`);

        }
    }
});

mp.events.add("UpdateMenuItemsTab", (itemsData) => {
    const itemDataJSON = JSON.parse(itemsData);

    var items_array = [];
    for(let i = 0; i < itemDataJSON.length ; i++){
        const item = itemDataJSON[i];
        // {itemData, owned, max}

        var itemObjectToAdd = {
            id:item.itemData.ID,
            name:item.itemData.Name,
            description:item.itemData.Description,
            owned:item.owned,
            max:item.max,
            cost:Number(item.itemData.Price),
            price:Number(item.itemData.Price),
            teamId:item.itemData.TeamID
        }
        items_array.push(itemObjectToAdd);
    }
    if(global.ServerUI) global.ServerUI.call('mainmenu_inventory_items_set', items_array);
});

mp.events.add("AddMenuItem", (name, type, owned, price, description, max, id, image) => {
    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.inventoryTab.$refs.itemsTab.items.push({
        id: ${id},
        name: "${name}",
        image: "${image}",
        owned: ${owned},
        cost: ${price},
        max: ${max},
        description: "${description}"
    });`);
    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.storeTab.$refs.itemsTab.items.push({
        id: ${id},
        name: "${name}",
        image: "${image}",
        owned: ${owned},
        cost: ${price},
        max: ${max},
        description: "${description}"
    });`);
});
mp.events.add("UpdateMaxMenuItem", (id, max) => {
    if(global.ServerUI) global.ServerUI.execute(`
        if(gm.$refs.mainMenu.$refs.inventoryTab.$refs.itemsTab.items.find(i => i.id == ${id}) != undefined){
            gm.$refs.mainMenu.$refs.inventoryTab.$refs.itemsTab.items.find(i => i.id == ${id}).max = ${max};
        }
        else{
            console.log("Failed to find item of type ${id} in the menu!");
        }
    `);
    if(global.ServerUI) global.ServerUI.execute(`
    if(gm.$refs.mainMenu.$refs.storeTab.$refs.itemsTab.items.find(i => i.id == ${id}) != undefined){
        gm.$refs.mainMenu.$refs.storeTab.$refs.itemsTab.items.find(i => i.id == ${id}).max = ${max};
    }
    else{
        console.log("Failed to find item of type ${id} in the menu!");
    }
`);
});
mp.events.add("UpdateMenuItem", (id, owned) => {
    if(global.ServerUI) global.ServerUI.execute(`
        if(gm.$refs.mainMenu.$refs.inventoryTab.$refs.itemsTab.items.find(i => i.id == ${id}) != undefined){
            gm.$refs.mainMenu.$refs.inventoryTab.$refs.itemsTab.items.find(i => i.id == ${id}).owned = ${owned};
        }
        else{
            console.log("Failed to find item of type ${id} in the menu!");
        }
    `);
    if(global.ServerUI) global.ServerUI.execute(`
    if(gm.$refs.mainMenu.$refs.storeTab.$refs.itemsTab.items.find(i => i.id == ${id}) != undefined){
        gm.$refs.mainMenu.$refs.storeTab.$refs.itemsTab.items.find(i => i.id == ${id}).owned = ${owned};
    }
    else{
        console.log("Failed to find item of type ${id} in the menu!");
    }
`);
});
// called by menu:
mp.events.add("menu_buyNamechange", (id) => {
    mp.events.callRemote("Menu_OnBuyNamechange", id);
});
mp.events.add("menu_buyRank", (id) => {
    mp.events.callRemote("Menu_OnBuyRank", id);
});
mp.events.add("mainmenu_inventory_items_item_buy", (id) => {
    mp.events.callRemote("Menu_RequestBuyItem", id);
});
mp.events.add("MenuCustomizeLoadout", () => {
    mp.events.callRemote("Menu_OnWeaponEditor");
});
mp.events.add("MenuCustomizeVehicle", (model, teamID) => {
    mp.events.callRemote("Menu_OnEditVehicle", model, teamID);
});
mp.events.add("menu_store_redeem", (code) => {
    mp.events.callRemote("Menu_OnCodeRedeem", code);
    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.storeTab.$refs.dpTab.isRedeeming = true;`);
});
mp.events.add("menu_store_redeem_result", (success, message) => {
    if(!global.ServerUI) return;

    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.storeTab.$refs.dpTab.redeemResponse = {
        success:${success.toString()},
        message:"${message}"
    }`);
    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.storeTab.$refs.dpTab.isRedeeming = false;`);
});

mp.events.add("mainmenu_play_custom_room_create", (name, pass, type, maxPlayers, maxPing) => {
    mp.events.callRemote("Menu_OnLobbyCreate", name, pass, type, maxPlayers, maxPing);
});
mp.events.add("mainmenu_play_custom_room_settings_update", (roomJSON) => {
    const roomData = JSON.parse(roomJSON);
    mp.events.callRemote("Menu_OnLobbyEdit", roomData.name, roomData.password, roomData.type, roomData.maxPlayers, roomData.maxPing);
});
mp.events.add("privateRoom_delete", (id) => {
    mp.events.callRemote('ServerConsoleOutput', `Deleted private room`);
    mp.events.callRemote("Menu_OnLobbyDelete", id);
});
mp.events.add("privateRoom_kickPlayer", (id) => {
    mp.events.callRemote("Menu_OnLobbyKickPlayer", id);
});

mp.events.add("privateRoom_banPlayer", (id) => {
    mp.events.callRemote("Menu_OnLobbyBanPlayer", id);
});
mp.events.add("privateRoom_unbanPlayer", (id) => {
    mp.events.callRemote("Menu_OnLobbyUnbanPlayer", id);
});
mp.events.add("removePrivateLobby", () => {
    mp.events.callRemote('ServerConsoleOutput', `Removed private room.`);
    if(global.ServerUI) global.ServerUI.call('mainmenu_play_custom_room_set', undefined);
})
mp.events.add("updateMenuPrivateLobby", (id, name, password, type, playerCount, maxPlayers, maxPing, ownerName, playerList, bannedPlayersList) => {
    eval(`var player_list = ${playerList}`);
    eval(`var banned_player_list = ${bannedPlayersList}`);
    if(global.ServerUI) global.ServerUI.call('mainmenu_play_custom_room_set', {
        id:id,
        name:name,
        password:password,
        type:type,
        players:playerCount,
        maxPlayers:maxPlayers,
        maxPing:maxPing,
        owner:ownerName,
        playerList:player_list,
        bannedPlayers:banned_player_list,
        image:"https://i.pinimg.com/originals/54/28/fa/5428faf1395c1f9c90fbefc71d88b88b.jpg"
    });
});

// --------------------
mp.events.add("UpdateMenuVehicles", (vehicles, currentCopCar, currentFugiCar) => {
    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.storeTab.$refs.vehiclesTab.vehicles = [];`);
    //if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.vehiclesTab.$refs.police.selectedModel = "${currentCopCar}";`);
    //if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.vehiclesTab.$refs.fugitive.selectedModel = "${currentFugiCar}";`);

    // Vehicle Data
    // Model, Name, Background, Energy, Price, Locked
    let menuItems = JSON.parse(vehicles);
    let vehicle_items = [];
    for(let i = 0; i < menuItems.length; i++)
    {
        //if(menuItems[i][2] == "Random") continue;

        eval(`var requirements_json = ${menuItems[i][6]}`);

        vehicle_items.push({
            id:i,
            teamId:menuItems[i][0],
            model:menuItems[i][1],
            name:menuItems[i][2],
            image:menuItems[i][3],
            owned:!menuItems[i][4], // menuItems[i][4] = "locked" | locked = !owned
            speed:(menuItems[i][2] !== "Random" ? menuItems[i][5] : "Random"),
            requirements:requirements_json
        });        
    }    
    if(global.ServerUI){
        global.ServerUI.call("mainmenu_inventory_vehicles_set", vehicle_items);   
        global.ServerUI.call("mainmenu_inventory_current_vehicles_set", currentCopCar, currentFugiCar);

    }

   
});
mp.events.add("UpdateMenuLoadouts", (loadouts, currentCopLoadout, currentFugiLoadout) => {
    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.storeTab.$refs.loadoutsTab.loadouts = [];`);
    //if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.storeTab.$refs.loadoutsTab.$refs.police.selectedLoadout = "${(currentCopLoadout == "?" ? "Default" : currentCopLoadout)}";`);
    //if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.storeTab.$refs.loadoutsTab.$refs.fugitive.selectedLoadout = "${(currentFugiLoadout == "?" ? "Default" : currentFugiLoadout)}";`);

    // Loadout Data
    // Name, Locked, Background, Requirements, Weapons
    let menuItems = JSON.parse(loadouts);
    let loadout_items = [];
    for(let i = 0; i < menuItems.length; i++)
    {

        eval(`var requirements_json_loadout = ${menuItems[i][4]}`);
        eval(`var weapons_json_loadout = ${menuItems[i][5]}`);

        loadout_items.push({
            teamId: menuItems[i][0],
            name: menuItems[i][1],
            background: menuItems[i][2],
            owned:!menuItems[i][3],
            requirements:requirements_json_loadout,
            weapons:weapons_json_loadout
        });

    }
    if(global.ServerUI){

        let copLoadout = currentCopLoadout === '?' ? "Default" : currentCopLoadout;
        let fugiLoadout = currentFugiLoadout === '?' ? "Default" : currentFugiLoadout;

        global.ServerUI.call("mainmenu_inventory_loadouts_set", loadout_items);
        global.ServerUI.call("mainmenu_inventory_current_loadouts_set", copLoadout, fugiLoadout);
    }
});

mp.events.add("UpdateMenuPlayRooms", (lobbies, minigames, currentMinigame) => {
    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.playTab.rooms = [];`);

    // Lobby Data
    // ID, Name, Background, Gamemode, Password, Players, MaxPlayers, Description
    let menuItems = JSON.parse(lobbies);
    let play_rooms = [];
    for(let i = 0; i < menuItems.length; i++)
    {
        let ownerName = "null";
        if(menuItems[i][8].length > 0) ownerName = `"${menuItems[i][8]}"`

        let roomType = 0;
        switch(menuItems[i][3]){
            case "Standard": roomType = 0; break;
            case "Busted": roomType = 1; break;
            default: roomType = 0;
        }

        play_rooms.push({
            id: menuItems[i][0],
            name: menuItems[i][1],
            image: menuItems[i][2],
            type: roomType,
            password:(menuItems[i][4].length > 1 ? menuItems[i][4] : ""),
            players:menuItems[i][5],
            maxplayers:menuItems[i][6],
            joinFunction:"PutPlayerInLobby",
            owner:(ownerName === "null" ? "" : ownerName)
        });
    }
    menuItems = JSON.parse(minigames);
    for(let i = 0; i < menuItems.length; i++)
    {
        let roomType = minigameNameToID(menuItems[i][5]);
        if(roomType == -1) continue;

        play_rooms.push({
            id: (menuItems[i][0]+69420),
            name:menuItems[i][1],
            image: menuItems[i][2],
            players: menuItems[i][3],
            maxplayers: menuItems[i][4],
            type: roomType,
            joinFunction:"RequestJoinMinigame",
            description:menuItems[i][6]
        });
    }
    play_rooms.push({
        id: 6969,
        type: 4,
        name: "Freeroam",
        image:
        "package://Artwork/Freeroam.png",
        players: menuItems[0][3],
        maxplayers: 1000,
        joinFunction: "RequestJoinMinigame",
    });
    play_rooms.push({
        id: 6970,
        type: 3,
        name: "Derby",
        image:
        "package://Artwork/Derby.jpg",
        players: menuItems[1][3],
        maxplayers: 20,
        joinFunction: "RequestJoinMinigame",
    });

    var current_room = -1;

    if(mp.players.local.getVariable("InLobby") >= 0){
        if(mp.players.local.getVariable("InDM") > -1)
            current_room = (mp.players.local.getVariable("InDM")+69420);
        else current_room = mp.players.local.getVariable("InLobby");
    }
    else{
        if(mp.players.local.getVariable("InLobby") === -3)
            current_room = 6969;
        else if(mp.players.local.getVariable("InLobby") === -4)
            current_room = 6970;
    }

    if(global.ServerUI) global.ServerUI.call('mainmenu_play_rooms_set', play_rooms);
    if(global.ServerUI) global.ServerUI.call('mainmenu_play_rooms_current_room_set', current_room);
    
});

mp.events.add("UpdateMenuLobbies", (lobbies, currentLobby) => {
    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.playTab.rooms = [];`);

    // Lobby Data
    // ID, Name, Background, Gamemode, Password, Players, MaxPlayers, Description
    let menuItems = JSON.parse(lobbies);
    let play_rooms = [];
    for(let i = 0; i < menuItems.length; i++)
    {
        let ownerName = "null";
        if(menuItems[i][8].length > 0) ownerName = `"${menuItems[i][8]}"`

        play_rooms.push({
            id: menuItems[i][0],
            name: menuItems[i][1],
            image: menuItems[i][2],
            type: (menuItems[i][3] === "Standard" ? 0 : 1),
            password:(menuItems[i][4].length > 1 ? menuItems[i][4] : ""),
            players:menuItems[i][5],
            maxplayers:menuItems[i][6],
            joinFunction:"PutPlayerInLobby",
            owner:(ownerName === "null" ? "" : ownerName)
        });

        // let insertString = `gm.$refs.mainMenu.$refs.playTab.rooms.push({`;
        // insertString += `"id": ${menuItems[i][0]}, `;
        // insertString += `"name": "${menuItems[i][1]}", `;
        // insertString += `"image": "${menuItems[i][2]}", `;
        // insertString += `"type": ${(menuItems[i][3] === "Standard" ? 0 : 1)}, `;
        // if(menuItems[i][4].length > 1) insertString += `"password": "${menuItems[i][4]}", `;
        // insertString += `"players": ${menuItems[i][5]}, `;
        // insertString += `"maxplayers": ${menuItems[i][6]},`;
        // insertString += `"joinFunction":"PutPlayerInLobby",`,
        // insertString += `"owner":${ownerName}`;
        // //if(menuItems[i][7].length > 1) insertString += `, "description": "${menuItems[i][7]}"`;
        // insertString += "});";

        //if(global.ServerUI) global.ServerUI.execute(insertString);
    }
    if(global.ServerUI) global.ServerUI.call('mainmenu_play_rooms_set', play_rooms);
    
    if(currentLobby >= 1)
        if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.playTab.currentRoom = ${currentLobby};`);
});
/*
mp.events.add("UpdateMenuLobbies", (lobbies, currentLobby) => {
    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.lobbiesTab.lobbies = [];`);
    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.lobbiesTab.currentLobby = ${currentLobby};`);

    // Lobby Data
    // ID, Name, Background, Gamemode, Password, Players, MaxPlayers, Description
    let menuItems = JSON.parse(lobbies);
    for(let i = 0; i < menuItems.length; i++)
    {
        let insertString = `gm.$refs.mainMenu.$refs.lobbiesTab.lobbies.push({`;
        insertString += `"id": ${menuItems[i][0]}, `;
        insertString += `"name": "${menuItems[i][1]}", `;
        insertString += `"background": "${menuItems[i][2]}", `;
        insertString += `"gamemode": "${menuItems[i][3]}", `;
        if(menuItems[i][4].length > 1) insertString += `"password": "${menuItems[i][4]}", `;
        insertString += `"players": ${menuItems[i][5]}, `;
        insertString += `"maxplayers": ${menuItems[i][6]}`;
        if(menuItems[i][7].length > 1) insertString += `, "description": "${menuItems[i][7]}"`;
        insertString += "});";

        if(global.ServerUI) global.ServerUI.execute(insertString);
    }
});
*/

// mp.events.add("SaveCrosshair", (crData) => {
//     mp.storage.data.menu.CrosshairData = crData;
//     if(global.ServerUI) global.ServerUI.execute(`gm.crosshair.layers = ${crData}`);

//     mp.storage.flush();
// });
// mp.events.add("LoadCrosshair", () => {
//     if (mp.storage.data.menu.CrosshairData !== undefined) {
//         if(global.ServerUI) global.ServerUI.execute(`gm.crosshair.layers = ${mp.storage.data.menu.CrosshairData}`);
//         if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.settingsTab.settings.$refs.crosshair.layers = ${mp.storage.data.menu.CrosshairData}`);
//     }
//     else {
//         mp.storage.data.menu.CrosshairData = "[]";
//         if(global.ServerUI) global.ServerUI.execute(`gm.crosshair.layers = []`);
//         if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.settingsTab.settings.$refs.crosshair.layers = []`);
//     }
// });