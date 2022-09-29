require('./ServerUI/CommandsList.js');

const Natives = {
    IS_RADAR_HIDDEN: "0x157F93B036700462",
    IS_RADAR_ENABLED: "0xAF754F20EB5CD51A",
    SET_TEXT_OUTLINE: "0x2513DFB0FB8400FE"
};

var wasCustomCrosshairOn = false;

let minimap = {};

let chatOpacityTimeout = null;

var resolution = mp.game.graphics.getScreenActiveResolution(0,0);
let lastSpecHPChange = Date.now();

var miniHUDVars = {
    bottom: 0, 
    left: 0,
    visible: false,
    voice: "Standby",
    direction: "N/A",
    street: "N/A",
    zone: "N/A",
    jailed: false,
    jail: "N/A",

    // SPEC VARS
    spec: false,
    ping: 0,
    packetloss: 0.0,
};

let speedo = false;
let speedoMax = 0.0;
let speedoCur = 0.0;

// Scoreboard
let updater = null;
scoreboardToggled = false;
scoreboardCursor = false;
let scoreboardBinded = false;

// StackOverflow cause I'm too lazy to do this myself and don't wanna type getvariable etc etc every time
function firstLetterUpper(string) {
    return string.charAt(0).toUpperCase() + string.slice(1);
  }

// https://github.com/glitchdetector/fivem-minimap-anchor
function getMinimapAnchor() {
    let sfX = 1.0 / 20.0;
    let sfY = 1.0 / 20.0;
    let safeZone = mp.game.graphics.getSafeZoneSize();
    let aspectRatio = mp.game.graphics.getScreenAspectRatio(false);
    let resolution = mp.game.graphics.getScreenActiveResolution(0, 0);
    let scaleX = 1.0 / resolution.x;
    let scaleY = 1.0 / resolution.y;

    let minimap = {
        width: scaleX * (resolution.x / (4 * aspectRatio)),
        height: scaleY * (resolution.y / 5.674),
        scaleX: scaleX,
        scaleY: scaleY,
        leftX: scaleX * (resolution.x * (sfX * (Math.abs(safeZone - 1.0) * 10))),
        bottomY: 1.0 - scaleY * (resolution.y * (sfY * (Math.abs(safeZone - 1.0) * 10))),
    };

    minimap.rightX = minimap.leftX + minimap.width;
    minimap.topY = minimap.bottomY - minimap.height;
    return minimap;
}



function drawText(text, drawXY, font, color, scale, alignRight = false) {
    mp.game.ui.setTextEntry("STRING");
    mp.game.ui.addTextComponentSubstringPlayerName(text);
    mp.game.ui.setTextFont(font);
    mp.game.ui.setTextScale(scale, scale);
    mp.game.ui.setTextColour(color[0], color[1], color[2], color[3]);
    mp.game.invoke(Natives.SET_TEXT_OUTLINE);

    if (alignRight) {
        mp.game.ui.setTextRightJustify(true);
        mp.game.ui.setTextWrap(0, drawXY[0]);
    }

    mp.game.ui.drawText(drawXY[0], drawXY[1]);
}

function TimeFormat(time)
{   
    var hrs = ~~(time / 3600);
    var mins = ~~((time % 3600) / 60);
    var secs = ~~time % 60;

    var ret = "";

    if (hrs > 0) {
        ret += "" + hrs + ":" + (mins < 10 ? "0" : "");
    }

    ret += "" + mins + ":" + (secs < 10 ? "0" : "");
    ret += "" + secs;
    return ret;
}

mp.keys.bind(
    0x76,
    false,
    () => {
        if(menuToggled !== undefined && menuToggled) return;
        if(!chatStatus && 
            !(mp.players.local.getVariable("pLogged") === undefined || mp.players.local.getVariable("pLogged") == false)) 
        {
			if (mp.keys.isDown(16) === true) 
            { // Shift is pressed
                mp.storage.data.menu.nametags = !mp.storage.data.menu.nametags;
                renderNametags = mp.storage.data.menu.nametags;
            }
            else 
            {
                mp.events.callLocal("ToggleHud", !showHud);
            }
        }
    }
);



mp.events.add("actionMenu:addItem", (id, action, color, icon) => {
    ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.actionMenu.items.push({
        id: ${id},
        name: "${action}",
        color: "${color}",
        icon: "${icon}"
    });`);
});
mp.events.add("actionMenu:removeItem", (id) => {
    ServerUI.execute(`
        if(gm.$refs.hud.$refs.base.$refs.actionMenu.items.find(i => i.id == ${id})){
            gm.$refs.hud.$refs.base.$refs.actionMenu.items.splice(gm.$refs.hud.$refs.base.$refs.actionMenu.items.indexOf(gm.$refs.hud.$refs.base.$refs.actionMenu.items.find(i => i.id == ${id})), 1);
        }
    `);
});
mp.events.add("actionMenu:removeAllItems", () => {
    ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.actionMenu.items = [];`);
});

mp.events.add('actionMenu:close', () => {
    ActionMenuActive = false;
    ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.actionMenu.enabled = false;`);
    mp.gui.cursor.show(false, false);
});
mp.events.add('actionMenu:open', () => {
    if(chatStatus == true || menuToggled == true || MDCActive == true || scoreboardToggled == true
        || VehicleSpawnerActive == true || WeaponEditorActive == true || isHotwiring == true || ClothingEditorActive == true) return;

    mp.game.invoke("0xFC695459D4D0E219", 0.5, 0.5); // SET CURSOR POSITION TO CENTER [x,y = 0.5,0.5]
    ActionMenuActive = true;
    mp.events.callLocal("actionMenu:removeAllItems");
    actionMenu_lobbyCheck();
    actionMenu_vehicleCheck();
    actionMenu_teamCheck();

    ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.actionMenu.enabled = true;`);
    mp.gui.cursor.show(true, true);
});
mp.events.add('actionMenu:clickItem', (actionID) => {
    mp.events.callLocal("actionMenu:close");

    // yes it's a long switch case with same action - this is for future in case some clientside stuff needs to be done too!
    // i could've done it better but eh we can easily change it if needed haha
    switch(actionID){
        case 0: // Vehicle lock toggle - invokes the /lock command simply
        {
            mp.events.callRemote("actionMenu_SelectedItem", actionID);
            break;
        }
        case 1: // Seatbelt toggle - invokes /seatbelt
        {
            mp.events.callRemote("actionMenu_SelectedItem", actionID);
            break;
        }
        case 2: // Medkit
        {
            mp.events.callRemote("actionMenu_SelectedItem", actionID);
            break;
        }
        case 3: // Painkiller
        {
            mp.events.callRemote("actionMenu_SelectedItem", actionID);
            break;
        }
        case 4: // Lockpick
        {
            mp.events.callRemote("actionMenu_SelectedItem", actionID);
            break;
        }
        case 5: // GPS Beacon
        {
            mp.events.callRemote("actionMenu_SelectedItem", actionID);
            break;
        }
        case 6: // Weapon Editor
        {
            setTimeout(() => {
                mp.events.callRemote("actionMenu_SelectedItem", actionID); 
            }, 100);
            break;
        }
        case 7: // Vehicle Tuner [freeroam only cuz copchase vtune requires param inputs]
        {
            setTimeout(() => {
                mp.events.callRemote("actionMenu_SelectedItem", actionID);                
            }, 100);
            break;
        }
        case 8: // Back To Lobby
        {
            mp.events.callRemote("actionMenu_SelectedItem", actionID);
            break;
        }
        case 9: // Freeroam vehicle spawning
        {
            setTimeout(() => {
                mp.events.callRemote("actionMenu_SelectedItem", actionID);                
            }, 100);
            break;
        }
        case 10: // Freeroam fix vehicle
        {
            mp.events.callRemote("actionMenu_SelectedItem", actionID);
            break;
        }
        case 11: // Dragout command
        {
            mp.events.callRemote("actionMenu_SelectedItem", actionID);
            break;
        }
        case 12: // Handcuff/revive message tip sends them to hold 'H'
        {
            ServerUI.execute(`toast('Hotkey Help', 'You must <strong>hold down "H"</strong> to perform that action!', 'warning', 3000);`);
            break;
        }
        case 13: // Engine command
        {
            mp.events.callRemote("actionMenu_SelectedItem", actionID);
            break;
        }
        case 14: // Hotwire command
        {
            mp.events.callRemote("actionMenu_SelectedItem", actionID);
            break;
        }
    }
});
function actionMenu_lobbyCheck(){
    let dmID = mp.players.local.getVariable("InDM");
    if(dmID != -1){
        mp.events.callLocal("actionMenu:addItem", 8, "Go to lobby", "#f87171", "fa-door-open");  
        return;
    }
    let lobbyID = mp.players.local.getVariable("InLobby");
    if(lobbyID > -1 && mp.players.local.getVariable("Team") < 0){ // copchase lobby
        mp.events.callLocal("actionMenu:addItem", 6, "Edit Weapons", "#38bdf8", "fa-gun");   
    }
    else if(lobbyID == -4){ // derby
        mp.events.callLocal("actionMenu:addItem", 8, "Go to lobby", "#f87171", "fa-door-open");  
    }
    else if(lobbyID == -3){ // freeroam
        mp.events.callLocal("actionMenu:addItem", 9, "Spawn Vehicle", "#4ade80", "fa-car");   
        if(mp.players.local.isInAnyVehicle(true)){
            mp.events.callLocal("actionMenu:addItem", 7, "Vehicle Tuning", "#a78bfa", "fa-car-wrench");  
            mp.events.callLocal("actionMenu:addItem", 10, "Repair Vehicle", "#c084fc", "fa-car-bolt");  
        } 
        mp.events.callLocal("actionMenu:addItem", 8, "Go to lobby", "#f87171", "fa-door-open");  
    }
}
// This function checks what team the player is in, and adds options for each team.
function actionMenu_teamCheck(){
    let team = mp.players.local.getVariable("Team");
    if(team >= 0){
        mp.events.callLocal("actionMenu:addItem", 3, "Painkiller", "#22c55e", "fa-pills");    
        
        let foundReviveTarget = false;
        mp.players.forEachInRange(mp.players.local.position, 5, (playa) => {
            if(!foundReviveTarget){
                if(playa.remoteId != mp.players.local.remoteId && playa.getVariable("Wounded") == 2 && 
                playa.getVariable("BeingRevived") == false && playa.getVariable("Team") == team){
                    foundReviveTarget = true;
                    mp.events.callLocal("actionMenu:addItem", 12, "Revive", "#fb923c", "fa-hand-holding-medical");  
                }
            }
        });

        if(team == 0){
            if(!mp.players.local.isInAnyVehicle(true)){
                mp.events.callLocal("actionMenu:addItem", 2, "Medkit", "#fb7185", "fa-kit-medical");      
                let isFound = false;
                mp.vehicles.forEachInRange(mp.players.local.position, 3.5, (vehicle) => {
                    if(!isFound)
                    {
                        if(vehicle.getVariable("Locked") == true && vehicle.getVariable("Team") != team){   
                            mp.events.callLocal("actionMenu:addItem", 4, "Lockpick", "#facc15", "fa-signature-lock");    
                            isFound = true;
                        }
                    }
                });
            }
        }
        else{
            mp.events.callLocal("actionMenu:addItem", 5, "GPS Beacon", "#60a5fa", "fa-location-dot");   
            if(!mp.players.local.isInAnyVehicle(true)){
                mp.events.callLocal("actionMenu:addItem", 2, "Medkit", "#fb7185", "fa-kit-medical");  
                
                let foundDragoutTarget = false;
                let foundCuffTarget = false;
                mp.players.forEachInRange(mp.players.local.position, 3.5, (playa) => {
                    if(!foundDragoutTarget)
                    {
                        if(playa.getVariable("Team") == 0 && playa.isInAnyVehicle(true)){
                            mp.events.callLocal("actionMenu:addItem", 11, "Drag out", "#22d3ee", "fa-person-through-window");  
                            foundDragoutTarget = true;
                        }
                    }
                    if(!foundCuffTarget && !foundReviveTarget){
                        if(playa.getVariable("Team") == 0 && !playa.isInAnyVehicle(true)){
                            mp.events.callLocal("actionMenu:addItem", 12, "Arrest", "#fb923c", "fa-handcuffs");  
                            foundCuffTarget = true;
                        }
                    }
                });
            }
        }
    }
}
// This function checks for nearby vehicle OR if player is in vehicle. If yes, it will add a LOCK option.
function actionMenu_vehicleCheck(){
    if(mp.players.local.isInAnyVehicle(false)){
        let vehicle = mp.players.local.vehicle;
        if(vehicle.getVariable("Locked") == true)
            mp.events.callLocal("actionMenu:addItem", 0, "Unlock", "#a3e635", "fa-lock-keyhole-open");
        else mp.events.callLocal("actionMenu:addItem", 0, "Lock", "#f87171", "fa-lock-keyhole");

        mp.events.callLocal("actionMenu:addItem", 1, "Seatbelt", "#fb923c", "fa-shield-slash");

        if(vehicle.engine == true){
            mp.events.callLocal("actionMenu:addItem", 13, "Engine", "#60a5fa", "fa-engine");
        }
        else{
            if(vehicle.getVariable("Team") != mp.players.local.getVariable("Team")){
                mp.events.callLocal("actionMenu:addItem", 14, "Hotwire", "#60a5fa", "fa-bolt");
            }
            else mp.events.callLocal("actionMenu:addItem", 13, "Engine", "#60a5fa", "fa-engine");
        }
    }
    else{
        mp.vehicles.forEachInRange(mp.players.local.position, 3.5, (vehicle) => {
            if(vehicle.getVariable("Team") == mp.players.local.getVariable("Team"))
            {
                if(vehicle.getVariable("Locked") == true)
                    mp.events.callLocal("actionMenu:addItem", 0, "Unlock", "#a3e635", "fa-lock-keyhole-open");
                else mp.events.callLocal("actionMenu:addItem", 0, "Lock", "#f87171", "fa-lock-keyhole");
                return;
            }
        });
    }
}

mp.events.add('ToggleNametags', (state) => {
    renderNametags = state;
});

/*
 gm.topRight = false;
      topRightInfo
      killFeed
      gameProgress
      hud
      topRight
*/

function increaseChatOpacity(time = 4000){
    if(chatOpacityTimeout != null){
        clearInterval(chatOpacityTimeout);
    }

    if(mp.storage.data.menu.ActiveChatOpacity !== undefined){
        ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.settings.opacity = ${mp.storage.data.menu.ActiveChatOpacity.toString()}`);
    }
    else ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.settings.opacity = 1.0`);

    // i know this is dumb but idk i cba to fix it it didnt work otherwise
    chatOpacityTimeout = setTimeout(() => {
        if(!chatStatus && ServerUI != null && ServerUI.active == true){
            if(mp.storage.data.menu.showInactiveChat == undefined){
                if(mp.storage.data.menu.InactiveChatOpacity == undefined){
                    ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.settings.opacity = 0.5`);
                }
                else ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.settings.opacity = ${mp.storage.data.menu.InactiveChatOpacity.toString()}`);
            }
            else{
                if(mp.storage.data.menu.showInactiveChat == false){
                    ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.settings.opacity = 0.0`);
                }
                else{
                    if(mp.storage.data.menu.InactiveChatOpacity == undefined){
                        ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.settings.opacity = 0.5`);
                    }
                    else ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.settings.opacity = ${mp.storage.data.menu.InactiveChatOpacity.toString()}`);                    
                }
            }
        }
    }, time);
}

mp.events.add("spawner_spawnVehicle", (modelname) => {
    VehicleSpawnerActive = false;
    ServerUI.execute(`gm.$refs.menus.$refs.vehicleSpawner.enabled = false;`);
    mp.events.callRemote("Spawner_SpawnVehicle", modelname);
    mp.gui.cursor.show(false, false);
});
mp.events.add("spawner_close", () => {
    VehicleSpawnerActive = false;
    ServerUI.execute(`gm.$refs.menus.$refs.vehicleSpawner.enabled = false;`);
    mp.gui.cursor.show(false, false);
});
mp.events.add("OpenVehicleSpawner", (carname) => {
    VehicleSpawnerActive = true;
    ServerUI.execute(`gm.$refs.menus.$refs.vehicleSpawner.openSpawner('${carname}')`);
    mp.gui.cursor.show(true, true);
});

mp.events.add("SendClanInvite", (clanName, clanID, senderName) => {
    ServerUI.execute(`clanInvite("Clan Invitation", "You have been invited to <strong>${clanName}</strong> by ${senderName}.", 20000, ${clanID});`);
});

mp.events.add("startLoadingScreen", (time, message) => {
    if(time == -1){
        ServerUI.execute('gm.$refs.loadingScreen.show(true);');
        if(message != "none")
            ServerUI.execute(`gm.$refs.loadingScreen.loadingText = "${message}"`);
        else ServerUI.execute(`gm.$refs.loadingScreen.loadingText = ""`);
    }
    else{
        ServerUI.execute(`gm.$refs.loadingScreen.show(${time});`);
        if(message != "none")
            ServerUI.execute(`gm.$refs.loadingScreen.loadingText = "${message}"`);
        else ServerUI.execute(`gm.$refs.loadingScreen.loadingText = ""`);
    }
});
mp.events.add("stopLoadingScreen", () => {
    ServerUI.execute('gm.$refs.loadingScreen.hide();');
});

function activateChat(toggle){
    if(ServerUI == null) return;
    if(menuToggled){
        ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.enabled = false;`);
        return;
    }

    ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.enabled = ${toggle.toString()}`);
};
function focusChat(toggle){
    if(ServerUI == null) return;
    if(menuToggled){
        ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.active = false;`);
        return;
    }
    if((VehicleSpawnerActive !== undefined && VehicleSpawnerActive == true) ||
    (WeaponEditorActive !== undefined && WeaponEditorActive == true) ||
    (ActionMenuActive !== undefined && ActionMenuActive == true) || 
    (ClothingEditorActive !== undefined && ClothingEditorActive == true) ||
    (isHotwiring !== undefined && isHotwiring == true)){
        ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.active = false;`);
        return;
    }
    ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.active = ${toggle.toString()}`);

    if(toggle){
        if(mp.storage.data.menu.ActiveChatOpacity !== undefined){
            ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.settings.opacity = ${mp.storage.data.menu.ActiveChatOpacity.toString()}`);
        }
        else ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.settings.opacity = 1.0`);
    }
    else{
        increaseChatOpacity(3000);
    }

    mp.gui.cursor.show(toggle, toggle);
    mp.events.callRemote("onFocusChat", toggle);
}
function pushMessageToChat(message){
    if(ServerUI == null){return;}

    ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.messages.push("${message}")`);
    increaseChatOpacity(4000);
}
function clearChat(){
    if(ServerUI == null) return;
    ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.messages = [];`);
}
let api = {"chat:push": pushMessageToChat, "chat:clear": clearChat, "chat:activate": activateChat, "chat:show": focusChat}; 

mp.events.add("pushMsgToChat", (msg) => {
    pushMessageToChat(msg);
})

mp.events.add("chat:push", (msg) => {
    pushMessageToChat(msg);
});
mp.events.add("chat:clear", (msg) => {
    clearChat();
});
mp.events.add("chat:activate", (toggle) => {
    activateChat(toggle);
});
mp.events.add("chat:show", (toggle) => {
    focusChat(toggle);
});

mp.events.add("HideAllElementsExceptChat", (value) => {
    if(value == false){
        ServerUI.execute(
        `
            gm.$refs.hud.$refs.topRight.$refs.info.enabled = false; 
            gm.$refs.hud.$refs.topRight.$refs.killFeed.enabled = false;
            gm.$refs.hud.$refs.base.$refs.chat.enabled = ${showHud.toString()};
        `);
    }
    else{
        ServerUI.execute(
            `
            gm.$refs.hud.$refs.topRight.$refs.info.enabled = ${mp.storage.data.menu.topRightInfo}; 
            gm.$refs.hud.$refs.topRight.$refs.killFeed.enabled = ${mp.storage.data.menu.killFeed};
            gm.$refs.hud.$refs.base.$refs.chat.enabled = ${showHud.toString()};
            `);       
    }
});

/*
for(let fn in api)
{
    mp.events.add(fn, api[fn]);
}
*/
mp.events.add("onChatPressEnter", () => {
    focusChat(false);
	mp.events.call("onOpenChatbox", false);
});

mp.events.add("InitiateCustomChat", () => {
    /*
    setTimeout(() =>{
        mp.events.callLocal('RestartMenu');
    }, 250);
    setTimeout(() => {
        mp.events.callLocal('RestartHUD');
    }, 250);
    */
});

mp.events.add("render", () => {
    if(chatStatusLate){
        mp.game.controls.disableControlAction(0, 200, true); // prevent menu from opening when pressing ESC for chat 
    }    
});
mp.keys.bind(0x1B, true, function() { // ESC key
    if(chatStatus){
        focusChat(false);
        mp.events.call("onOpenChatbox", false);       
    }
});

mp.keys.bind(0x54, true, function() { // T key
    if(ServerUI == null) return;
    if(menuToggled){
        ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.active = false;`);
        return;
    }
    if(mp.storage.data.menu.HUD !== undefined && mp.storage.data.menu.HUD == false){
        return;
    }
    if((VehicleSpawnerActive !== undefined && VehicleSpawnerActive == true) ||
    (WeaponEditorActive !== undefined && WeaponEditorActive == true) ||
    (ClothingEditorActive !== undefined && ClothingEditorActive == true) ||
    (ActionMenuActive !== undefined && ActionMenuActive == true) || 
    (isHotwiring !== undefined && isHotwiring == true)){
        ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.active = false;`);
        return;
    }

    if(!chatStatus)
    {
        focusChat(true);
        mp.events.call("onOpenChatbox", true);
    }
});
mp.events.add("closeChat", () => {         
    focusChat(false);
    mp.events.call("onOpenChatbox", false);  
});
mp.events.add("TogglePlayerChat", (toggle) => {
    activateChat(toggle);
});

mp.events.add('RestartHUD', () => {
    if(ServerUI !== null)
    {
        ServerUI.destroy();
        ServerUI = null;
    }

    ServerUI = mp.browsers.new("package://ServerUI/index.html");
    setTimeout(() => {
        ServerUI.markAsChat();
        mp.gui.chat.activate(true);
        ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.commands = ${JSON.stringify(CommandsDataJSON)}`);
        ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.active = false;`)
        ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.settings = {
            opacity: 1,
            autoScroll: true,
        }`);
    }, 1000);
    ServerUI.execute(`gm.$refs.hud.enabled = false;`);
});

let createdTooltips = []; // to keep track of created tooltips / preventing duplicates

mp.events.add('AddTooltip', (key, action, team) => {
    
    if(ServerUI == null) return;
    if(createdTooltips.indexOf(key) != -1) return;
    if(mp.storage.data.menu.tooltips !== true) return;

    let color = "#383838FF";
    switch(team){
        case 0: color = "#ffae00ff";
        break;
        case 1: color = "#0097ffff";
        break;
        default: color = "#383838FF";
    }

    createdTooltips.push(key);

    ServerUI.execute(`gm.$refs.hud.$refs.topRight.$refs.tooltips.tips.push({
        key: "${key}",
        action: "${action}",
        color: "${color}"
    })`);
});
mp.events.add('RemoveTooltip', (key) => {
    if(ServerUI == null) return;
    if(createdTooltips.indexOf(key) == -1) return;

    ServerUI.execute(`if(gm.$refs.hud.$refs.topRight.$refs.tooltips.tips.indexOf(gm.$refs.hud.$refs.topRight.$refs.tooltips.tips.filter(function(el){return el.key == "${key}"})[0]) != -1){
        gm.$refs.hud.$refs.topRight.$refs.tooltips.tips.splice(gm.$refs.hud.$refs.topRight.$refs.tooltips.tips.indexOf(gm.$refs.hud.$refs.topRight.$refs.tooltips.tips.filter(function(el){return el.key == "${key}"})[0]), 1);        
    }`);
    createdTooltips.splice(createdTooltips.indexOf(key), 1);
});

let keyTrunk = true; // true = key trunk, false = key seatbelt displayed

mp.events.add('ToggleTooltips', (status) =>
{
    mp.storage.data.menu.tooltips = status;
    ServerUI.execute(`gm.$refs.hud.$refs.topRight.$refs.tooltips.enabled = ${status.toString()}`);

    if(status)
    {
        mp.events.callLocal("AddTooltip", "M", "Menu", -1);
        mp.events.callLocal("AddTooltip", String.fromCharCode(curActionMenuBind), "Action Menu", -1);
        mp.events.callLocal("AddTooltip", String.fromCharCode(curVoiceBind), "Voice Chat", -1);
    }
    else {

        createdTooltips.forEach(key => {
            ServerUI.execute(`if(gm.$refs.hud.$refs.topRight.$refs.tooltips.tips.indexOf(gm.$refs.hud.$refs.topRight.$refs.tooltips.tips.filter(function(el){return el.key == "${key}"})[0]) != -1){
                gm.$refs.hud.$refs.topRight.$refs.tooltips.tips.splice(gm.$refs.hud.$refs.topRight.$refs.tooltips.tips.indexOf(gm.$refs.hud.$refs.topRight.$refs.tooltips.tips.filter(function(el){return el.key == "${key}"})[0]), 1);        
            }`);
        });

        createdTooltips = [];
    }
});

mp.events.add("TwoSecondsEvent", () => {

    if(mp.storage.data.menu.RealtimeSpeedo == false){
        if (specState == true && specTarget != null && mp.players.exists(specTarget)) 
        {
            if(specTarget.isInAnyVehicle(false))
            {
                if(!speedo)
                {
                    ServerUI.execute(`gm.$refs.hud.$refs.bottomRight.$refs.speedo.enabled = true;`);
                    speedo = true;
                }
                if(speedoMax != ((mp.game.vehicle.getVehicleModelMaxSpeed(specTarget.vehicle.model) * 3.6) + 25).toFixed(0))
                {
                    speedoMax = ((mp.game.vehicle.getVehicleModelMaxSpeed(specTarget.vehicle.model) * 3.6) + 25).toFixed(0);
                    ServerUI.execute(`gm.$refs.hud.$refs.bottomRight.$refs.speedo.maxspeed = ${speedoMax};`);
                }
                if(speedoCur != (specTarget.vehicle.getSpeed() * 3.6).toFixed(0))
                {
                    speedoCur = (specTarget.vehicle.getSpeed() * 3.6).toFixed(0);
                    ServerUI.execute(`gm.$refs.BottomRight.$refs.speedo.speed = ${speedoCur};`);
                }
            }
            else if(speedo)
            {
                speedo = false;
                speedoMax = 0;
                speedoCur = 0;
                ServerUI.execute(`gm.$refs.hud.$refs.bottomRight.$refs.speedo.enabled = false;`);
            }
        }
        else {
            if(mp.players.local.isInAnyVehicle(false))
            {
                if(!speedo)
                {
                    ServerUI.execute(`gm.$refs.hud.$refs.bottomRight.$refs.speedo.enabled = true;`);
                    speedo = true;
                }
                if(speedoMax != ((mp.game.vehicle.getVehicleModelMaxSpeed(mp.players.local.vehicle.model) * 3.6) + 25).toFixed(0))
                {
                    speedoMax = ((mp.game.vehicle.getVehicleModelMaxSpeed(mp.players.local.vehicle.model) * 3.6) + 25).toFixed(0);
                    ServerUI.execute(`gm.$refs.hud.$refs.bottomRight.$refs.speedo.maxspeed = ${speedoMax};`);
                }
                if(speedoCur != (mp.players.local.vehicle.getSpeed() * 3.6).toFixed(0))
                {
                    speedoCur = (mp.players.local.vehicle.getSpeed() * 3.6).toFixed(0);
                    ServerUI.execute(`gm.$refs.hud.$refs.bottomRight.$refs.speedo.speed = ${speedoCur};`);
                }
            }
            else if(speedo)
            {
                speedo = false;
                speedoMax = 0;
                speedoCur = 0;
                ServerUI.execute(`gm.$refs.hud.$refs.bottomRight.$refs.speedo.enabled = false;`);
            }
        }
    }
    
    if(mp.storage.data.menu.tooltips == true)
    {
        if(mp.players.local.Team != 0 && mp.players.local.Team != 1){
            if(createdTooltips.indexOf("H") != -1) mp.events.callLocal("RemoveTooltip", "H");
        }

        if(mp.players.local.vehicle){
            if(mp.players.local.getVariable("Team") == 1 && chaseRunning)
                if(createdTooltips.indexOf("B") == -1) mp.events.callLocal("AddTooltip", "B", "Silent Siren", 1);
                
            if(createdTooltips.indexOf("L") == -1) mp.events.callLocal("AddTooltip", "L", "Lock", -1);
        }
        else{
            if(createdTooltips.indexOf("L") != -1) mp.events.callLocal("RemoveTooltip", "L");
            if(createdTooltips.indexOf("B") != -1) mp.events.callLocal("RemoveTooltip", "B");     
        }
        if(mp.players.local.getVariable("Team") == 1 && chaseRunning){
            if(createdTooltips.indexOf(String.fromCharCode(curGPSBind)) == -1) 
            {
                mp.events.callLocal("AddTooltip", String.fromCharCode(curGPSBind), "GPS", 1);
            }
            if(createdTooltips.indexOf(String.fromCharCode(curMDCBind)) == -1) 
            {
                mp.events.callLocal("AddTooltip", String.fromCharCode(curMDCBind), "MDC", 1);
            }
        }
        else{
            if(createdTooltips.indexOf(String.fromCharCode(curGPSBind)) != -1) mp.events.callLocal("RemoveTooltip", String.fromCharCode(curGPSBind));
            if(createdTooltips.indexOf(String.fromCharCode(curMDCBind)) != -1) mp.events.callLocal("RemoveTooltip", String.fromCharCode(curMDCBind));
            if(createdTooltips.indexOf("B") != -1) mp.events.callLocal("RemoveTooltip", "B");     
        }

        if(!mp.players.local.isInAnyVehicle(false)) 
        {
            if(createdTooltips.indexOf("J") != -1 && !keyTrunk)
            {
                mp.events.callLocal("RemoveTooltip", "J");
                keyTrunk = true;
            }

            let veh = getClosestVehicle(mp.players.local.position);

            if(veh !== undefined && mp.vehicles.exists(veh.vehicle) && veh.vehicle.handle !== 0)
            {
                let sidebones = [
                    "bonnet",
                    "boot"
                ];
          
                let closestHandle = "";
                let closestDist = 100.0;
                sidebones.forEach((side) => {
                    let bone = veh.vehicle.getBoneIndexByName(side);
                    if(bone != -1)
                    {
                        let dist = calcDist(mp.players.local.position, veh.vehicle.getWorldPositionOfBone(bone));
                        if(dist < closestDist)
                        {
                            closestDist = dist;
                            closestHandle = side;
                        }
                    }
                });
        
                if(closestDist <= 3.5)
                {
                    if((veh.vehicle.getVariable("Locked") === undefined || veh.vehicle.getVariable("Locked") == false) || 
                    (veh.vehicle.getVariable("Locked") == true && (mp.players.local.weapon == 0x84BD7BFD || mp.players.local.weapon == 2227010557)))
                    {
                        if(createdTooltips.indexOf("J") == -1) 
                        {
                            if(closestHandle == "boot") mp.events.callLocal("AddTooltip", "J", "Trunk", -1);
                            else if(closestHandle == "bonnet") mp.events.callLocal("AddTooltip", "J", "Bonnet", -1);
                        }
                    }
                    else if(createdTooltips.indexOf("J") != -1) mp.events.callLocal("RemoveTooltip", "J");
                }
                else if(createdTooltips.indexOf("J") != -1) mp.events.callLocal("RemoveTooltip", "J");
            }
            else if(createdTooltips.indexOf("J") != -1) mp.events.callLocal("RemoveTooltip", "J");
        }
        else
        {
            if((createdTooltips.indexOf("J") != -1 && keyTrunk) || createdTooltips.indexOf("J") == -1)
            {
                mp.events.callLocal("RemoveTooltip", "J");
                
                if(mp.players.local.vehicle.getClass() != 8 && mp.players.local.vehicle.model != mp.game.joaat('policeb'))
                {
                    mp.events.callLocal("AddTooltip", "J", "Seatbelt", -1);
                }

                keyTrunk = false;
            }
        }    
    }

    if(mp.storage.data.menu.HUD == false)
    {
        ServerUI.execute(`gm.$refs.hud.enabled = false;`);
    }
    renderNametags = mp.storage.data.menu.nametags;
});



mp.events.add('browserDomReady', (browser) => {
    if(ServerUI != null && browser == ServerUI)
    {
        // Set vars that aren't reset on login and shit. Basically init().
        ServerUI.execute(`gm.$refs.hud.$refs.topRight.$refs.info.enabled = ${mp.storage.data.menu.topRightInfo}; gm.$refs.hud.$refs.topRight.$refs.killFeed.enabled = ${mp.storage.data.menu.killFeed};`);
        ServerUI.execute(`gm.$refs.hud.$refs.miniHud.enabled = true;`);
        ServerUI.execute(`gm.$refs.hud.$refs.miniHud.items = [];`);

        // Initialize miniHUD
        miniHUDVars["bottom"] = ((resolution.y - (getMinimapAnchor().bottomY * resolution.y)) + 0);
        miniHUDVars["left"] = ((getMinimapAnchor().rightX * resolution.x) + 10) + ((mp.storage.data.menu.alwaysExpandedMap == true) ? 162 : 0);

        ServerUI.execute(`gm.$refs.hud.$refs.miniHud.left = "${miniHUDVars["left"]}px";`);
        ServerUI.execute(`gm.$refs.hud.$refs.miniHud.bottom = "${miniHUDVars["bottom"]}px";`);  

        // SPECTATE VARS
        ServerUI.execute(`gm.$refs.hud.$refs.miniHud.items.push({ 
            name: "ping",
            icon: "fa-stopwatch",
            init: "0 ms",
            color: "#fbbf24"
        });`);

        ServerUI.execute(`gm.$refs.hud.$refs.miniHud.items.push({ 
            name: "packetloss",
            icon: "fa-minus",
            init: "0.0%",
            color: "#fb923c"
        });`);
        //
        
        ServerUI.execute(`gm.$refs.hud.$refs.miniHud.items.push({
            name: "jail",
            icon: "fa-ban",
            init: "N/A",
            color: "#fb7185"
        });`);

        ServerUI.execute(`gm.$refs.hud.$refs.miniHud.items.push({
            name: "visible",
            icon: "fa-globe",
            init: "Visible",
            color: "#60a5fa"
        });`);

        ServerUI.execute(`gm.$refs.hud.$refs.miniHud.items.push({
            name: "voice",
            icon: "fa-microphone",
            init: "Standby",
            color: "#818cf8"
        });`);

        ServerUI.execute(`gm.$refs.hud.$refs.miniHud.items.push({
            name: "direction",
            icon: "fa-compass",
            init: "N/A",
            color: "#a3e635"
        });`);

        ServerUI.execute(`gm.$refs.hud.$refs.miniHud.items.push({
            name: "street",
            icon: "fa-location-dot",
            init: "N/A",
            color: "#4ade80"
        });`);

        ServerUI.execute(`gm.$refs.hud.$refs.miniHud.items.push({
            name: "zone",
            icon: "fa-location-crosshairs",
            init: "N/A",
            color: "#34d399"
        });`);

        ServerUI.execute(`gm.$refs.hud.$refs.miniHud.$refs.jail[0].show = ${miniHUDVars["jailed"]};`);
        ServerUI.execute(`gm.$refs.hud.$refs.miniHud.$refs.visible[0].show = false;`);
        ServerUI.execute(`gm.$refs.hud.$refs.miniHud.$refs.ping[0].show = false;`);
        ServerUI.execute(`gm.$refs.hud.$refs.miniHud.$refs.packetloss[0].show = false;`);

        mp.events.callLocal("ToggleTooltips", mp.storage.data.menu.tooltips);

        if(mp.storage.data.menu.HUD == false)
        {
            ServerUI.execute(`gm.$refs.hud.enabled = false`);
        }
    }
});

mp.events.add('ToggleHud', (toggle) => {
    showHud = toggle;
    
    if(ServerUI != null)
    {
        if(mp.storage.data.menu.HUD == false)
        {
            ServerUI.execute(`gm.$refs.hud.enabled = false`);
            mp.storage.data.menu.HUD = false;
        }

        ServerUI.execute(`gm.$refs.hud.enabled = ${toggle};`);
        mp.storage.data.menu.HUD = toggle;
    }

    activateChat(showHud);
    mp.game.ui.displayRadar(showHud);
});

mp.keys.bind(0x75, false, () => {
    showReticule = !showReticule;
});

mp.keys.bind(0x74, false, () => {
    if(menuToggled !== undefined && menuToggled) return;
    if(scoreboardToggled == true) return;
    if(Date.now() - lastChatToggle >= 500 && chatStatus == false && 
        (mp.players.local.getVariable("pLogged") !== undefined && mp.players.local.getVariable("pLogged") == true))
    {
        traceLastFunc(`[Players] Attempt tog /DL`);
        
        mp.events.callRemote("OnScriptedKeyPress", 0x74, true);
    }
});

mp.events.add('ResetKillFeed', () => {
    if(ServerUI != null)
    {
        ServerUI.execute(`gm.$refs.hud.$refs.topRight.$refs.killFeed.enabled = false;`);
        ServerUI.execute(`gm.$refs.hud.$refs.topRight.$refs.killFeed.enabled = ${mp.storage.data.menu.killFeed};`);
    }
});

mp.events.add("pushToKillFeed", (killer, killerColor, weapon, victim, victimColor) => {
    if(ServerUI != null) ServerUI.execute(`gm.$refs.hud.$refs.topRight.$refs.killFeed.add('${killer}', '${killerColor}', '${weapon}', '${victim}', '${victimColor}');`)
});

mp.events.add("pushToKillFeed2", (victim, color, cause) => {
    if(ServerUI != null) ServerUI.execute(`gm.$refs.hud.$refs.topRight.$refs.killFeed.add("${victim}", "${color}", "${cause}");`)
});

mp.events.add("UpdateRateEvent", () => {

    // only do stuff if radar is enabled and visible
    let streetName = "N/A";
    let zoneName = "N/A";
    if (mp.game.invoke(Natives.IS_RADAR_ENABLED) && !mp.game.invoke(Natives.IS_RADAR_HIDDEN)) {
        minimap = getMinimapAnchor();

        const position = mp.players.local.position;
        let getStreet = mp.game.pathfind.getStreetNameAtCoord(position.x, position.y, position.z, 0, 0);

        zoneName = mp.game.ui.getLabelText(mp.game.zone.getNameOfZone(position.x, position.y, position.z));
        streetName = mp.game.ui.getStreetNameFromHashKey(getStreet.streetName);
    }
    
    if(ServerUI != null && (!(mp.players.local.getVariable("pLogged") === undefined || mp.players.local.getVariable("pLogged") == false || mp.players.local.getVariable("InLobby") === undefined)))
    {
        ServerUI.execute(`gm.$refs.hud.$refs.topRight.$refs.info.points = ${mp.players.local.getVariable("Points")};`);
        
        let lobbyStr = "";
        
        let lobbyPlayers = 0;
        mp.players.forEach((pl) => {
            if(!(pl.getVariable("pLogged") === undefined || pl.getVariable("pLogged") == false || pl.getVariable("InLobby") === undefined))
            {
                if(pl.getVariable("InLobby") == mp.players.local.getVariable("InLobby")) lobbyPlayers++;
            }
        });

        if(mp.players.local.getVariable("InLobby") == -2) lobbyStr = "Jail";
        if(mp.players.local.getVariable("InLobby") == -3) lobbyStr = "Freeroam";
        if(mp.players.local.getVariable("InLobby") == -4) lobbyStr = "Derby";
        if(mp.players.local.getVariable("InLobby") == -5) lobbyStr = "Customizer";
        if(mp.players.local.getVariable("InLobby") > -1) lobbyStr = mp.players.local.getVariable("InLobby").toString();
        
        ServerUI.execute(`gm.$refs.hud.$refs.topRight.$refs.info.lobbyId = "${lobbyStr}";`);

        let camera = mp.cameras.new("gameplay");
        let cameraDirection = camera.getDirection();
        let dirtext = "N/A";

        if (0.3 < cameraDirection.x && 0.3 < cameraDirection.y) {
            dirtext = "NE";
        }
        else if (cameraDirection.x < -0.3 && 0.3 < cameraDirection.y) {
            dirtext = "NW";
        }
        else if (0.3 < cameraDirection.x && cameraDirection.y < -0.3) {
            dirtext = "SE";
        }
        else if (cameraDirection.x < -0.3 && cameraDirection.y < -0.3) {
            dirtext = "SW";
        }
        else if (-0.3 < cameraDirection.x && cameraDirection.x < 0.3 && cameraDirection.y < -0.3) {
            dirtext = "S";
        }
        else if (cameraDirection.x < -0.3 && -0.3 < cameraDirection.y && cameraDirection.y < 0.3) {
            dirtext = "W";
        }
        else if (0.3 < cameraDirection.x && -0.3 < cameraDirection.y && cameraDirection.y < 0.3) {
            dirtext = "E";
        }
        else if (-0.3 < cameraDirection.x && cameraDirection.x < 0.3 && cameraDirection.y > 0.3) {
            dirtext = "N";
        }
        camera.destroy(true);

        let voiceString = "Disabled";
                
        if(mp.players.local.getVariable("VoiceMode") == 0)
        {
            voiceString = mp.voiceChat.muted ? "Standby" : "Speaking";
        }
        else if(mp.players.local.getVariable("VoiceMode") == 1)
        {
            voiceString = mp.voiceChat.muted ? "Standby (Team)" : "Speaking (Team)";
        }
        else if(mp.players.local.getVariable("VoiceMode") == 2)
        {
            voiceString = mp.voiceChat.muted ? "Standby (Squad)" : "Speaking (Squad)";
        }

        if(currentMuteTime >= 1)
        {
            voiceString = `Muted for ${TimeFormat(currentMuteTime)}`;
        }

        let jailString = "N/A";
        if(currentJailTime >= 1)
        {
            jailString = `Jailed for ${TimeFormat(currentJailTime)}`;
        }

        if(mp.players.local.getVariable("VisibleBlip") != miniHUDVars["visible"])
        {
            miniHUDVars["visible"] = mp.players.local.getVariable("VisibleBlip");
            ServerUI.execute(`gm.$refs.hud.$refs.miniHud.$refs.visible[0].show = ${miniHUDVars["visible"]};`);
        }
        if(voiceString != miniHUDVars["voice"])
        {
            miniHUDVars["voice"] = voiceString;
            ServerUI.execute(`gm.$refs.hud.$refs.miniHud.$refs.voice[0].value = "${miniHUDVars["voice"]}";`);
        }
        if(dirtext != miniHUDVars["direction"])
        {
            miniHUDVars["direction"] = dirtext;
            ServerUI.execute(`gm.$refs.hud.$refs.miniHud.$refs.direction[0].value = "${miniHUDVars["direction"]}";`);
        }
        if(streetName != miniHUDVars["street"])
        {
            miniHUDVars["street"] = streetName;
            ServerUI.execute(`gm.$refs.hud.$refs.miniHud.$refs.street[0].value = "${miniHUDVars["street"]}";`);
        }
        if(zoneName != miniHUDVars["zone"])
        {
            miniHUDVars["zone"] = zoneName;
            ServerUI.execute(`gm.$refs.hud.$refs.miniHud.$refs.zone[0].value = "${miniHUDVars["zone"]}";`);
        }

        if(currentJailTime >= 1)
        {
            if(miniHUDVars["jailed"] != true)
            {
                miniHUDVars["jailed"] = true;
                ServerUI.execute(`gm.$refs.hud.$refs.miniHud.$refs.jail[0].show = ${miniHUDVars["jailed"]};`);
            }
            miniHUDVars["jail"] = jailString;
            ServerUI.execute(`gm.$refs.hud.$refs.miniHud.$refs.jail[0].value = "${miniHUDVars["jail"]}";`);
        }
        else if(miniHUDVars["jailed"] == true)
        {
            miniHUDVars["jailed"] = false;
            ServerUI.execute(`gm.$refs.hud.$refs.miniHud.$refs.jail[0].show = ${miniHUDVars["jailed"]};`);
        }

        if (specState == true && specTarget != null && mp.players.exists(specTarget)) 
        {
            if(miniHUDVars["spec"] == false)
            {
                miniHUDVars["spec"] = true;
                ServerUI.execute(`gm.$refs.hud.$refs.miniHud.$refs.ping[0].show = true;`);
                ServerUI.execute(`gm.$refs.hud.$refs.miniHud.$refs.packetloss[0].show = true;`);

                ServerUI.execute(`gm.$refs.hud.$refs.miniHud.$refs.direction[0].show = false;`);
            }
        
            if(miniHUDVars["packetloss"] != specTarget.getVariable("Packetloss").toFixed(2))
            {
                miniHUDVars["packetloss"] = specTarget.getVariable("Packetloss").toFixed(2);
                ServerUI.execute(`gm.$refs.hud.$refs.miniHud.$refs.packetloss[0].value = "${miniHUDVars["packetloss"]}%"`);
            }
            if(miniHUDVars["ping"] != specTarget.getVariable("Ping"))
            {
                miniHUDVars["ping"] = specTarget.getVariable("Ping");
                ServerUI.execute(`gm.$refs.hud.$refs.miniHud.$refs.ping[0].value = "${miniHUDVars["ping"]} ms"`);
            }
        }
        else if(miniHUDVars["spec"] == true) 
        {
            miniHUDVars["spec"] = false;
            ServerUI.execute(`gm.$refs.hud.$refs.miniHud.$refs.ping[0].show = false;`);
            ServerUI.execute(`gm.$refs.hud.$refs.miniHud.$refs.packetloss[0].show = false;`);

            ServerUI.execute(`gm.$refs.hud.$refs.miniHud.$refs.direction[0].show = true;`);
        }
    }
});

function ToggleScoreboard()
{
    if (ServerUI == null) return;

    if(scoreboardToggled)
    {
        scoreboardToggled = false;

        if(updater != null) clearInterval(updater);
        updater = null;

        mp.events.call("TogglePlayerChat", true);
        ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.scoreboard.disableInput = true;`);
		// ServerUI.execute(`gm.$refs.scoreboard.setPlayers([]);`);
        ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.scoreboard.enabled = false;`);

        if(scoreboardCursor)
        {
            scoreboardCursor = false;
            mp.gui.cursor.show(false, false);
        }
    }
    else
    {
        scoreboardToggled = true;
        mp.events.call("TogglePlayerChat", false);

        ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.scoreboard.enabled = true;`);
        ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.scoreboard.disableInput = true;`);
		// ServerUI.execute(`gm.$refs.scoreboard.setPlayers([]);`);
		
        updatePlayers();

        updater = setInterval(function()
        {
            if(chatStatus && scoreboardToggled)
            {
                ToggleScoreboard();
            }
            else if(!chatStatus && scoreboardToggled)
            {
                updatePlayers();
            }
        }, 1000);
    }
    return;
}

mp.events.add("scoreboard:clickAction", (actionID, playerName) => {
    mp.events.callRemote("ServerConsoleOutput", `scoreboard action ${actionID} on player ${playerName}`);
    mp.events.callRemote("Scoreboard_OnAction", actionID, playerName);
});

function updatePlayers()
{
    if(mp.players.local !== undefined && mp.players.exists(mp.players.local))
        UpdateStatsVars(mp.players.local);

    let updateString = `gm.$refs.hud.$refs.base.$refs.scoreboard.setPlayers([`;

    let lobbyStr = '0';
	let colr = 255;
	let colg = 255;
	let colb = 255;
	let ping = 65535;
	let level = 0;
    let squad = -1;
	
    mp.players.forEach(sb => {
        lobbyStr = "-";
        colr = 255;
        colg = 255;
        colb = 255;
        ping = 0;
        level = 0;
        squad = -1;

        lobbyStr = sb.getVariable("InLobby");		
        squad = sb.getVariable("PlayerSquadID");	
		if(sb.getVariable("Color_R") !== undefined) colr = sb.getVariable("Color_R");
		if(sb.getVariable("Color_G") !== undefined) colg = sb.getVariable("Color_G");
		if(sb.getVariable("Color_B") !== undefined) colb = sb.getVariable("Color_B");
		if(sb.getVariable("Level") !== undefined) level = sb.getVariable("Level");
		if(sb.getVariable("Ping") !== undefined) ping = sb.getVariable("Ping");

        let mutedVar = `VoiceMute${sb.name}`;
        let mutedTog = mp.players.local.getVariable(mutedVar);
        if(mutedTog == undefined) mutedTog = false;

        updateString += `{lid: ${lobbyStr}, squad: ${squad},
        color: 'rgb(${colr}, ${colg}, ${colb})', 
        name: '${sb.name}', level: ${level}, ping: ${ping}, 
        showMute:true, muted: ${mutedTog.toString()}},`;
    });

    updateString += `]);`;

    ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.scoreboard.$refs.global.actions = [
        {
            id: 1,
            name: "Mute",
            icon: "fa-volume-xmark",
            disabledName: "Unmute",
            disabledIcon: "fa-volume",
            disableKey: "muted",
            showKey: "showMute",
        },
    ]`);
    if(ServerUI != null) ServerUI.execute(updateString);
    return;
}

mp.keys.bind(
    0x1B,
    false,
    () => {
        if(ServerUI != null && scoreboardToggled) ToggleScoreboard();
    }
);

mp.keys.bind(0x71, false, function () {
    if(scoreboardToggled)
    {
        if(scoreboardCursor)
        {
            scoreboardCursor = false;
            mp.gui.cursor.show(false, false);
            ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.scoreboard.disableInput = true;`);
            
            if(!scoreboardBinded) mp.events.callLocal('ScoreboardToggleTyping', false);
        }
        else
        {
            scoreboardCursor = true;
            mp.gui.cursor.show(true, true);
            ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.scoreboard.disableInput = false;`);

            //if(scoreboardBinded) mp.events.callLocal('ScoreboardToggleTyping', true);
        }
    }
});

function ScoreboardKeyTrigger()
{
    if(mp.players.local.getVariable("pLogged") === undefined || mp.players.local.getVariable("pLogged") == false) return;
    if(Date.now() - lastChatToggle <= 500) return;
    if(menuToggled !== undefined && menuToggled) return;
    if(chatStatus && !scoreboardToggled) return;
    if(menuToggled) return;

    ToggleScoreboard();
}

mp.events.add('ScoreboardToggleTyping', (state) => {
    if(state && scoreboardBinded)
    {
        mp.keys.unbind(0x4F, false, ScoreboardKeyTrigger);
        scoreboardBinded = false;
        mp.events.callLocal("MenuToggleTyping", state);
    }
    else if(!state && !scoreboardBinded)
    {
        mp.keys.bind(0x4F, false, ScoreboardKeyTrigger);
        scoreboardBinded = true;
        mp.events.callLocal("MenuToggleTyping", state);
    }
});

mp.events.callLocal('ScoreboardToggleTyping', false);

mp.events.add("HUDError", (msg, icon) => {
    if(!showHud && mp.players.local.getVariable("pLogged") == true) return;
    /*
	let insertString = `iziToast.show({
		message: "${msg}",
		icon: "${icon}",
		color: "red",
		timeout: 5000,
		close: true		
	});`;
    */
    let insertString = `toast('', '${msg}', 'error', 5000);`;
    if(ServerUI != null) ServerUI.execute(insertString);
});

mp.events.add("HUDNotify", (msg, type, icon, close, timeout, orientation, displaymode) => {
    if(!showHud && mp.players.local.getVariable("pLogged") == true) return;
    
    /*
    let insertString = `iziToast.show({`;
    insertString += `message: \`${msg}\`, `;
    insertString += `color: "${color}", `;
    insertString += `icon: "${icon}", `;
    insertString += `close: ${close.toString()}, `;
    insertString += `timeout: ${timeout},`;
    if(orientation.length > 1) insertString += `position: \`${orientation}\`, `;
    if(displaymode.length > 1) insertString += `displayMode: \`${displaymode}\`, `;
    insertString += "});";
    */
    let insertString = `toast('', '${msg}', '${type}', ${timeout});`;
    if(ServerUI != null) ServerUI.execute(insertString);
});
// bottomRight, bottomLeft, topRight, topLeft, topCenter, bottomCenter, center
mp.events.add("HUDNotify2", (title, msg, orientation, displaymode) => {
    if(!showHud && mp.players.local.getVariable("pLogged") == true) return;
    
    /*
    let insertString = `iziToast.show({`;
    insertString += `title: \`${title}\`, `;
    insertString += `message: \`${msg}\`, `;
    if(orientation.length > 1) insertString += `position: \`${orientation}\`, `;
    if(displaymode.length > 1) insertString += `displayMode: \`${displaymode}\`, `;
    insertString += "});";
    */
    let insertString = `toast('${title}', '${msg}', 'nothing', 5000);`;
    if(ServerUI != null) ServerUI.execute(insertString);
});



function isVehicleInLOS(target, startpos) {
    if(target === undefined) return false;

    let result = mp.raycasting.testPointToPoint(startpos, target.position, [1, 16]); // now test point to point
    
    if(result === undefined)
    {
        return true;
    }
    else if(result !== undefined)
    {
        if(result.entity !== undefined) // Entity is not undefined and might be ped or veh.
        {
            if(typeof result.entity.isAPed === 'function' && result.entity.isAPed())
            {
                return true;
            }
            if(typeof result.entity.isAVehicle === 'function' && result.entity.isAVehicle())
            {
                return true;
            }
        }
        else if(result.entity === undefined) // There's no entity, can be a object or building anything.
        {
            return false;
        }
    }
    return false;
}

mp.events.add("render", () => {
    
    mp.game.ui.hideHudComponentThisFrame(7); // area name
    mp.game.ui.hideHudComponentThisFrame(9); // street name
    mp.game.ui.hideHudComponentThisFrame(6); // vehicle name
    mp.game.ui.hideHudComponentThisFrame(8); // vehicle class
    mp.game.ui.hideHudComponentThisFrame(3); // cash
    mp.game.ui.hideHudComponentThisFrame(4); // mp_cash
    mp.game.ui.displayAmmoThisFrame(false); // HIDE AMMO

    if(mp.players.local.weapon != 2725352035){
        if(ServerUI != null){
        let wepData = weaponData.find(w => w.hash == mp.players.local.weapon);
        if(!wepData) wepData = weaponData.find(w => w.hash == (mp.players.local.weapon-4294967296));
        
        if(!wepData){
            wepData = {Name:"Undefined Weapon", image:"https://assets.gm.miami/gtav/weapons/Baseball-bat-icon.png"};
        }

        let weapon_hash = mp.players.local.weapon; // returns weapon as a hash => uint
        let ammoClip = mp.players.local.getAmmoInClip(weapon_hash); // returns ur ammo in clip
        let ammoWeapon  = mp.players.local.getWeaponAmmo(weapon_hash); // returns ur wep ammo
        let clipSize = mp.game.weapon.getWeaponClipSize(weapon_hash);

        ServerUI.execute(`gm.$refs.hud.$refs.topRight.$refs.info.equippedWeapon = {
            name: "${wepData.name}",
            ammo: ${ammoClip},
            maxAmmo: ${ammoWeapon + (clipSize-ammoClip)},
            image: "${wepData.image}",
          }`);
        }
    }
    else{
        if(ServerUI != null){
            ServerUI.execute(`gm.$refs.hud.$refs.topRight.$refs.info.equippedWeapon = null;`);
        }
    }

    if(showCustomCrosshair){
        if(alwaysShowCustomCrosshair == true){
            if(!customCrosshairOn){
                if(!mp.game.player.isFreeAiming()){         
                    if(!menuToggled)                     
                    {
                        customCrosshairOn = true;
                    }
                }
                else{
                    if(isHoldingSniper()){
                        if(showCustomCrosshairInSniper){
                            customCrosshairOn = true;                           
                        }
                        else{
                            customCrosshairOn = false;                           
                        }
                    }
                    else{
                        customCrosshairOn = true;
                    }
                }
            }
            else{
                if(mp.game.player.isFreeAiming()){                              
                    if(isHoldingSniper()){
                        if(showCustomCrosshairInSniper){
                            customCrosshairOn = true;                          
                        }
                        else{
                            customCrosshairOn = false;                            
                        }
                    }
                }               
            }
        }
        else{
            if(mp.game.invoke('0x68EDDA28A5976D07') == 1){
                if(!customCrosshairOn){
                    if(!mp.game.player.isFreeAiming()){                              
                        customCrosshairOn = true;
                    }
                    else{
                        if(isHoldingSniper()){
                            if(showCustomCrosshairInSniper){
                                customCrosshairOn = true;                          
                            }
                            else{
                                customCrosshairOn = false;                                  
                            }
                        }
                        else{
                            customCrosshairOn = true;
                        }
                    }
                }
            }
            else{
                if(customCrosshairOn){
                    customCrosshairOn = false;
                }
            }   
        }
    }

    if(!showReticule)
    {
        mp.game.ui.hideHudComponentThisFrame(14); // RETICULE
        customCrosshairOn = false;
    }
    if(isLockpicking == true || ActionMenuActive == true){
        mp.game.ui.hideHudComponentThisFrame(14); // RETICULE
        customCrosshairOn = false;
    }
    if(customCrosshairOn == true){
        if(!wasCustomCrosshairOn){
            ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.crosshair.enabled = true;`);
            wasCustomCrosshairOn = true;
        }
        mp.game.ui.hideHudComponentThisFrame(14); // RETICULE
    }
    else{
        if(wasCustomCrosshairOn == true){
            ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.crosshair.enabled = false;`);
            wasCustomCrosshairOn = false;
        }
    }

    if(mp.players.local.getVariable("pLogged") !== undefined && mp.players.local.getVariable("pLogged") == true)
    {       
        if ((resolution.x != mp.game.graphics.getScreenActiveResolution(0,0).x || miniHUDVars["left"] != (((getMinimapAnchor().rightX * resolution.x) + 10)) + ((mp.storage.data.menu.alwaysExpandedMap == true) ? 162 : 0)) && ServerUI != null)
        {
            resolution = mp.game.graphics.getScreenActiveResolution(0,0);
            
            miniHUDVars["bottom"] = ((resolution.y - (getMinimapAnchor().bottomY * resolution.y)) + 0);
            miniHUDVars["left"] = ((getMinimapAnchor().rightX * resolution.x) + 10) + ((mp.storage.data.menu.alwaysExpandedMap == true) ? 162 : 0);

            ServerUI.execute(`gm.$refs.hud.$refs.miniHud.left = "${miniHUDVars["left"]}px";`);
            ServerUI.execute(`gm.$refs.hud.$refs.miniHud.bottom = "${miniHUDVars["bottom"]}px";`);  
        }
        // if (mp.storage.data.menu.alwaysExpandedMap == true)
        // {
        //     mp.game.ui.setRadarBigmapEnabled(true, false);
        // }
        
        if (specState == true && specTarget != null && mp.players.exists(specTarget)) 
        {
            if((mp.players.local.getHealth() != specTarget.getHealth() || mp.players.local.getArmour() != specTarget.getArmour()) && 
                Date.now() - lastSpecHPChange >= 500)
            {
                mp.events.callRemote("SpectateChangeHPAR", specTarget.getHealth(), specTarget.getArmour());
                lastSpecHPChange = Date.now();
            }

            if(specTarget.isInAnyVehicle(false))
            {
                if(!speedo)
                {
                    ServerUI.execute(`gm.$refs.hud.$refs.bottomRight.$refs.speedo.enabled = true;`);
                    speedo = true;
                }
                if(mp.storage.data.menu.RealtimeSpeedo == true){
                    if(speedoMax != ((mp.game.vehicle.getVehicleModelMaxSpeed(specTarget.vehicle.model) * 3.6) + 25).toFixed(0))
                    {
                        speedoMax = ((mp.game.vehicle.getVehicleModelMaxSpeed(specTarget.vehicle.model) * 3.6) + 25).toFixed(0);
                        ServerUI.execute(`gm.$refs.hud.$refs.bottomRight.$refs.speedo.maxspeed = ${speedoMax};`);
                    }
                    if(speedoCur != (specTarget.vehicle.getSpeed() * 3.6).toFixed(0))
                    {
                        speedoCur = (specTarget.vehicle.getSpeed() * 3.6).toFixed(0);
                        ServerUI.execute(`gm.$refs.BottomRight.$refs.speedo.speed = ${speedoCur};`);
                    }
                }
         
            }
            else if(speedo)
            {
                speedo = false;
                speedoMax = 0;
                speedoCur = 0;
                ServerUI.execute(`gm.$refs.hud.$refs.bottomRight.$refs.speedo.enabled = false;`);
            }
        }
        else {
            if(mp.players.local.isInAnyVehicle(false))
            {
                if(!speedo)
                {
                    ServerUI.execute(`gm.$refs.hud.$refs.bottomRight.$refs.speedo.enabled = true;`);
                    speedo = true;
                }
                if(mp.storage.data.menu.RealtimeSpeedo == true){
                    if(speedoMax != ((mp.game.vehicle.getVehicleModelMaxSpeed(mp.players.local.vehicle.model) * 3.6) + 25).toFixed(0))
                    {
                        speedoMax = ((mp.game.vehicle.getVehicleModelMaxSpeed(mp.players.local.vehicle.model) * 3.6) + 25).toFixed(0);
                        ServerUI.execute(`gm.$refs.hud.$refs.bottomRight.$refs.speedo.maxspeed = ${speedoMax};`);
                    }
                    if(speedoCur != (mp.players.local.vehicle.getSpeed() * 3.6).toFixed(0))
                    {
                        speedoCur = (mp.players.local.vehicle.getSpeed() * 3.6).toFixed(0);
                        ServerUI.execute(`gm.$refs.hud.$refs.bottomRight.$refs.speedo.speed = ${speedoCur};`);
                    }
                }
            }
            else if(speedo)
            {
                speedo = false;
                speedoMax = 0;
                speedoCur = 0;
                ServerUI.execute(`gm.$refs.hud.$refs.bottomRight.$refs.speedo.enabled = false;`);
            }
        }
    }

    if(mp.players.local.getVariable("VehicleDL") == true)
    {
        const camera = mp.cameras.new("gameplay");

        let position = mp.players.local.position;

        if(camera !== undefined) position = camera.getCoord();
        else 
        {
            camera = mp.cameras.new("default");

            if(camera === undefined) position = mp.players.local.position;
            else position = camera.getCoord();
        }
        
        mp.vehicles.streamed.forEach(veh => {

            let dist = calcDist(veh.position, position);
            if(isVehicleInLOS(veh, position) && dist <= 32.0)
            {
                let vehName = mp.game.ui.getLabelText(mp.game.vehicle.getDisplayNameFromVehicleModel(veh.model));
                let passengers = veh.getNumberOfPassengers();
                if(veh.getPedInSeat(-1) != 0) passengers++;

                let drawText = `~c~${vehName} (ID: ~w~${veh.remoteId}~c~) (~w~${passengers} Occupants~c~)\n~w~Speed: ~s~${(veh.getSpeed() * 3.6).toFixed(0)} ~w~/ ~s~${(mp.game.vehicle.getVehicleModelMaxSpeed(veh.model) * 3.6).toFixed(0)} ~w~KM/H`;

                let hp = ((veh.getHealth() / 1000) * 100).toFixed(0);
                if(hp < 35)
                {
                    drawText = drawText + `\nHP: ~r~${hp}%`;
                }
                else if(hp >= 35 && hp <= 65) 
                {
                    drawText = drawText + `\nHP: ~y~${hp}%`;
                }
                else if(hp > 65)
                {
                    drawText = drawText + `\nHP: ~g~${hp}%`;
                }
            
                let rescale = (dist / 32.0) - 0.60;
                if(rescale >= 0.40) rescale = (0.40 + (rescale / 10.0));
                if(rescale <= 0.35) rescale = (0.40 - (rescale / 10.0));

                mp.game.graphics.drawText(drawText, [veh.position.x, veh.position.y, veh.position.z], { 
                    font: 4, 
                    color: [255, 255, 255, 216], 
                    scale: [rescale, rescale], 
                    outline: true,
                    centre: true
                });
            }
        });
    }
});







// i cba to fuck with these export/import shits right now just gonna put it here im tired its 3AM
const weaponData = [
    {
      "id": "weapon_dagger",
      "name": "Antique Cavalry Dagger",
      "type": 0,
      "image": "https://assets.gm.miami/gtav/weapons/Antique-cavalry-dagger-icon.png",
      "hash": "0x92A27487"
    },
    {
      "id": "weapon_bat",
      "name": "Baseball Bat",
      "type": 0,
      "image": "https://assets.gm.miami/gtav/weapons/Baseball-bat-icon.png",
      "hash": "0x958A4A8F"
    },
    {
      "id": "weapon_bottle",
      "name": "Broken Bottle",
      "type": 0,
      "image": "https://assets.gm.miami/gtav/weapons/Broken-bottle-icon.png",
      "hash": "0xF9E6AA4B"
    },
    {
      "id": "weapon_crowbar",
      "name": "Crowbar",
      "type": 0,
      "image": "https://assets.gm.miami/gtav/weapons/Crowbar-icon.png",
      "hash": "0x84BD7BFD"
    },
    {
      "id": "weapon_unarmed",
      "name": "Fist",
      "type": 0,
      "image": "https://assets.gm.miami/gtav/weapons/Fist-icon.png",
      "hash": "0xA2719263"
    },
    {
      "id": "weapon_flashlight",
      "name": "Flashlight",
      "type": 0,
      "image": "https://assets.gm.miami/gtav/weapons/Flashlight-icon.png",
      "hash": "0x8BB05FD7"
    },
    {
      "id": "weapon_golfclub",
      "name": "Golf Club",
      "type": 0,
      "image": "https://assets.gm.miami/gtav/weapons/Golf-club-icon.png",
      "hash": "0x440E4788"
    },
    {
      "id": "weapon_hammer",
      "name": "Hammer",
      "type": 0,
      "image": "https://assets.gm.miami/gtav/weapons/Hammer-icon.png",
      "hash": "0x4E875F73"
    },
    {
      "id": "weapon_hatchet",
      "name": "Hatchet",
      "type": 0,
      "image": "https://assets.gm.miami/gtav/weapons/Hatchet-icon.png",
      "hash": "0xF9DCBF2D"
    },
    {
      "id": "weapon_knuckle",
      "name": "Brass Knuckles",
      "type": 0,
      "image": "https://assets.gm.miami/gtav/weapons/Knuckles-icon.png",
      "hash": "0xD8DF3C3C"
    },
    {
      "id": "weapon_knife",
      "name": "Knife",
      "type": 0,
      "image": "https://assets.gm.miami/gtav/weapons/Knife-icon.png",
      "hash": "0x99B507EA"
    },
    {
      "id": "weapon_machete",
      "name": "Machete",
      "type": 0,
      "image": "https://assets.gm.miami/gtav/weapons/Machete-icon.png",
      "hash": "0xDD5DF8D9"
    },
    {
      "id": "weapon_switchblade",
      "name": "Switchblade",
      "type": 0,
      "image": "https://assets.gm.miami/gtav/weapons/Switch-blade-icon.png",
      "hash": "0xDFE37640"
    },
    {
      "id": "weapon_nightstick",
      "name": "Nightstick",
      "type": 0,
      "image": "https://assets.gm.miami/gtav/weapons/Nightstick-icon.png",
      "hash": "0x678B81B1"
    },
    {
      "id": "weapon_wrench",
      "name": "Pipe Wrench",
      "type": 0,
      "image": "https://assets.gm.miami/gtav/weapons/Pipe-wrench-icon.png",
      "hash": "0x19044EE0"
    },
    {
      "id": "weapon_battleaxe",
      "name": "Battle Axe",
      "type": 0,
      "image": "https://assets.gm.miami/gtav/weapons/Battle-axe-icon.png",
      "hash": "0xCD274149"
    },
    {
      "id": "weapon_poolcue",
      "name": "Pool Cue",
      "type": 0,
      "image": "https://assets.gm.miami/gtav/weapons/Pool-cue-icon.png",
      "hash": "0x94117305"
    },
    {
      "id": "weapon_stone_hatchet",
      "name": "Stone Hatchet",
      "type": 0,
      "image": "https://assets.gm.miami/gtav/weapons/Stone-hatchet-icon.png",
      "hash": "0x3813FC08"
    },
    {
      "id": "weapon_pistol",
      "name": "Pistol",
      "type": 1,
      "image": "https://assets.gm.miami/gtav/weapons/Pistol-icon.png",
      "hash": "0x1B06D571"
    },
    {
      "id": "weapon_pistol_mk2",
      "name": "Pistol Mk II",
      "type": 1,
      "image": "https://assets.gm.miami/gtav/weapons/Pistol-mk2-icon.png",
      "hash": "0xBFE256D4"
    },
    {
      "id": "weapon_combatpistol",
      "name": "Combat Pistol",
      "type": 1,
      "image": "https://assets.gm.miami/gtav/weapons/Combat-pistol-icon.png",
      "hash": "0x5EF9FEC4"
    },
    {
      "id": "weapon_appistol",
      "name": "AP Pistol",
      "type": 1,
      "image": "https://assets.gm.miami/gtav/weapons/Appistol-icon.png",
      "hash": "0x22D8FE39"
    },
    {
      "id": "weapon_stungun",
      "name": "Stun Gun",
      "type": 1,
      "image": "https://assets.gm.miami/gtav/weapons/Stungun-icon.png",
      "hash": "0x3656C8C1"
    },
    {
      "id": "weapon_pistol50",
      "name": "Pistol .50",
      "type": 1,
      "image": "https://assets.gm.miami/gtav/weapons/Pistol.50-icon.png",
      "hash": "0x99AEEB3B"
    },
    {
      "id": "weapon_snspistol",
      "name": "SNS Pistol",
      "type": 1,
      "image": "https://assets.gm.miami/gtav/weapons/Sns-pistol-icon.png",
      "hash": "0xBFD21232"
    },
    {
      "id": "weapon_snspistol_mk2",
      "name": "SNS Pistol Mk II",
      "type": 1,
      "image": "https://assets.gm.miami/gtav/weapons/Sns-pistol-mk2-icon.png",
      "hash": "0x88374054"
    },
    {
      "id": "weapon_heavypistol",
      "name": "Heavy Pistol",
      "type": 1,
      "image": "https://assets.gm.miami/gtav/weapons/Heavy-pistol-icon.png",
      "hash": "0xD205520E"
    },
    {
      "id": "weapon_vintagepistol",
      "name": "Vintage Pistol",
      "type": 1,
      "image": "https://assets.gm.miami/gtav/weapons/Vintage-pistol-icon.png",
      "hash": "0x83839C4"
    },
    {
      "id": "weapon_flaregun",
      "name": "Flare Gun",
      "type": 1,
      "image": "https://assets.gm.miami/gtav/weapons/Flaregun-icon.png",
      "hash": "0x47757124"
    },
    {
      "id": "weapon_marksmanpistol",
      "name": "Marksman Pistol",
      "type": 1,
      "image": "https://assets.gm.miami/gtav/weapons/Marksman-pistol-icon.png",
      "hash": "0xDC4DB296"
    },
    {
      "id": "weapon_revolver",
      "name": "Heavy Revolver",
      "type": 1,
      "image": "https://assets.gm.miami/gtav/weapons/Heavy-revolver-icon.png",
      "hash": "0xC1B3C3D1"
    },
    {
      "id": "weapon_revolver_mk2",
      "name": "Heavy Revolver Mk II",
      "type": 1,
      "image": "https://assets.gm.miami/gtav/weapons/Heavy-revolver-mk2-icon.png",
      "hash": "0xCB96392F"
    },
    {
      "id": "weapon_doubleaction",
      "name": "Double Action Revolver",
      "type": 1,
      "image": "https://assets.gm.miami/gtav/weapons/Double-action-revolver-icon.png",
      "hash": "0x97EA20B8"
    },
    {
      "id": "weapon_raypistol",
      "name": "Up-n-Atomizer",
      "type": 1,
      "image": "https://assets.gm.miami/gtav/weapons/Up-n-atomizer-icon.png",
      "hash": "0xAF3696A1"
    },
    {
      "id": "weapon_ceramicpistol",
      "name": "Ceramic Pistol",
      "type": 1,
      "image": "https://assets.gm.miami/gtav/weapons/Ceramic-pistol-icon.png",
      "hash": "0x2B5EF5EC"
    },
    {
      "id": "weapon_navyrevolver",
      "name": "Navy Revolver",
      "type": 1,
      "image": "https://assets.gm.miami/gtav/weapons/Navy-revolver-icon.png",
      "hash": "0x917F6C8C"
    },
    {
      "id": "weapon_gadgetpistol",
      "name": "Perico Pistol",
      "type": -1,
      "image": "https://assets.gm.miami/gtav/weapons/Perico-pistol-icon.png",
      "hash": "0x57A4368C"
    },
    {
      "id": "weapon_stungun_mp",
      "name": "Stun Gun",
      "type": -1,
      "image": "https://assets.gm.miami/gtav/weapons/Stungun-icon.png",
      "hash": "0x45CD9CF3"
    },
    {
      "id": "weapon_microsmg",
      "name": "Micro SMG",
      "type": 2,
      "image": "https://assets.gm.miami/gtav/weapons/Micro-smg-icon.png",
      "hash": "0x13532244"
    },
    {
      "id": "weapon_smg",
      "name": "SMG",
      "type": 2,
      "image": "https://assets.gm.miami/gtav/weapons/Smg-icon.png",
      "hash": "0x2BE6766B"
    },
    {
      "id": "weapon_smg_mk2",
      "name": "SMG Mk II",
      "type": 2,
      "image": "https://assets.gm.miami/gtav/weapons/Smg-mk2-icon.png",
      "hash": "0x78A97CD0"
    },
    {
      "id": "weapon_assaultsmg",
      "name": "Assault SMG",
      "type": 2,
      "image": "https://assets.gm.miami/gtav/weapons/Assault-smg-icon.png",
      "hash": "0xEFE7E2DF"
    },
    {
      "id": "weapon_combatpdw",
      "name": "Combat PDW",
      "type": -1,
      "image": "https://assets.gm.miami/gtav/weapons/Combat-pdw-icon.png",
      "hash": "0x0A3D4D34"
    },
    {
      "id": "weapon_machinepistol",
      "name": "Machine Pistol",
      "type": 2,
      "image": "https://assets.gm.miami/gtav/weapons/Machine-pistol-icon.png",
      "hash": "0xDB1AA450"
    },
    {
      "id": "weapon_minismg",
      "name": "Mini SMG",
      "type": 2,
      "image": "https://assets.gm.miami/gtav/weapons/Mini-smg-icon.png",
      "hash": "0xBD248B55"
    },
    {
      "id": "weapon_raycarbine",
      "name": "Unholy Hellbringer",
      "type": 2,
      "image": "https://assets.gm.miami/gtav/weapons/Unholy-hellbringer-icon.png",
      "hash": "0x476BF155"
    },
    {
      "id": "weapon_pumpshotgun",
      "name": "Pump Shotgun",
      "type": 3,
      "image": "https://assets.gm.miami/gtav/weapons/Pump-shotgun-icon.png",
      "hash": "0x1D073A89"
    },
    {
      "id": "weapon_pumpshotgun_mk2",
      "name": "Pump Shotgun Mk II",
      "type": 3,
      "image": "https://assets.gm.miami/gtav/weapons/Pump-shotgun-mk2-icon.png",
      "hash": "0x555AF99A"
    },
    {
      "id": "weapon_sawnoffshotgun",
      "name": "Sawed-Off Shotgun",
      "type": 3,
      "image": "https://assets.gm.miami/gtav/weapons/Sawed-off-shotgun-icon.png",
      "hash": "0x7846A318"
    },
    {
      "id": "weapon_assaultshotgun",
      "name": "Assault Shotgun",
      "type": 3,
      "image": "https://assets.gm.miami/gtav/weapons/Assault-shotgun-icon.png",
      "hash": "0xE284C527"
    },
    {
      "id": "weapon_bullpupshotgun",
      "name": "Bullpup Shotgun",
      "type": 3,
      "image": "https://assets.gm.miami/gtav/weapons/Bullpup-shotgun-icon.png",
      "hash": "0x9D61E50F"
    },
    {
      "id": "weapon_musket",
      "name": "Musket",
      "type": 3,
      "image": "https://assets.gm.miami/gtav/weapons/Musket-icon.png",
      "hash": "0xA89CB99E"
    },
    {
      "id": "weapon_heavyshotgun",
      "name": "Heavy Shotgun",
      "type": 3,
      "image": "https://assets.gm.miami/gtav/weapons/Heavy-shotgun-icon.png",
      "hash": "0x3AABBBAA"
    },
    {
      "id": "weapon_dbshotgun",
      "name": "Double Barrel Shotgun",
      "type": 3,
      "image": "https://assets.gm.miami/gtav/weapons/Double-barrel-shotgun-icon.png",
      "hash": "0xEF951FBB"
    },
    {
      "id": "weapon_autoshotgun",
      "name": "Sweeper Shotgun",
      "type": 3,
      "image": "https://assets.gm.miami/gtav/weapons/Sweeper-shotgun-icon.png",
      "hash": "0x12E82D3D"
    },
    {
      "id": "weapon_combatshotgun",
      "name": "Combat Shotgun",
      "type": -1,
      "image": "https://assets.gm.miami/gtav/weapons/Combat-shotgun-icon.png",
      "hash": "0x5A96BA4"
    },
    {
      "id": "weapon_assaultrifle",
      "name": "Assault Rifle",
      "type": 4,
      "image": "https://assets.gm.miami/gtav/weapons/Assault-rifle-icon.png",
      "hash": "0xBFEFFF6D"
    },
    {
      "id": "weapon_assaultrifle_mk2",
      "name": "Assault Rifle Mk II",
      "type": 4,
      "image": "https://assets.gm.miami/gtav/weapons/Assault-rifle-mk2-icon.png",
      "hash": "0x394F415C"
    },
    {
      "id": "weapon_carbinerifle",
      "name": "Carbine Rifle",
      "type": 4,
      "image": "https://assets.gm.miami/gtav/weapons/Carbine-rifle-icon.png",
      "hash": "0x83BF0278"
    },
    {
      "id": "weapon_carbinerifle_mk2",
      "name": "Carbine Rifle Mk II",
      "type": 4,
      "image": "https://assets.gm.miami/gtav/weapons/Carbine-rifle-mk2-icon.png",
      "hash": "0xFAD1F1C9"
    },
    {
      "id": "weapon_advancedrifle",
      "name": "Advanced Rifle",
      "type": 4,
      "image": "https://assets.gm.miami/gtav/weapons/Advanced-rifle-icon.png",
      "hash": "0xAF113F99"
    },
    {
      "id": "weapon_specialcarbine",
      "name": "Special Carbine",
      "type": 4,
      "image": "https://assets.gm.miami/gtav/weapons/Special-carbine-icon.png",
      "hash": "0xC0A3098D"
    },
    {
      "id": "weapon_specialcarbine_mk2",
      "name": "Special Carbine Mk II",
      "type": 4,
      "image": "https://assets.gm.miami/gtav/weapons/Special-carbine-mk2-icon.png",
      "hash": "0x969C3D67"
    },
    {
      "id": "weapon_bullpuprifle",
      "name": "Bullpup Rifle",
      "type": 4,
      "image": "https://assets.gm.miami/gtav/weapons/Bullpup-rifle-icon.png",
      "hash": "0x7F229F94"
    },
    {
      "id": "weapon_bullpuprifle_mk2",
      "name": "Bullpup Rifle Mk II",
      "type": 4,
      "image": "https://assets.gm.miami/gtav/weapons/Bullpup-rifle-mk2-icon.png",
      "hash": "0x84D6FAFD"
    },
    {
      "id": "weapon_compactrifle",
      "name": "Compact Rifle",
      "type": 4,
      "image": "https://assets.gm.miami/gtav/weapons/Compact-rifle-icon.png",
      "hash": "0x624FE830"
    },
    {
      "id": "weapon_militaryrifle",
      "name": "Military Rifle",
      "type": -1,
      "image": "https://assets.gm.miami/gtav/weapons/Military-rifle-icon.png",
      "hash": "0x9D1F17E6"
    },
    {
      "id": "weapon_heavyrifle",
      "name": "Heavy Rifle",
      "type": -1,
      "image": "https://assets.gm.miami/gtav/weapons/Heavy-rifle-icon.png",
      "hash": "0xC78D71B4"
    },
    {
      "id": "weapon_tacticalrifle",
      "name": "Tactical Rifle",
      "type": -1,
      "image": "https://assets.gm.miami/gtav/weapons/Tactical-rifle-icon.png",
      "hash": "0xD1D5F52B"
    },
    {
      "id": "weapon_mg",
      "name": "MG",
      "type": 5,
      "image": "https://assets.gm.miami/gtav/weapons/Mg-icon.png",
      "hash": "0x9D07F764"
    },
    {
      "id": "weapon_combatmg",
      "name": "Combat MG",
      "type": 5,
      "image": "https://assets.gm.miami/gtav/weapons/Combat-mg-icon.png",
      "hash": "0x7FD62962"
    },
    {
      "id": "weapon_combatmg_mk2",
      "name": "Combat MG Mk II",
      "type": 5,
      "image": "https://assets.gm.miami/gtav/weapons/Combat-mg-mk2-icon.png",
      "hash": "0xDBBD7280"
    },
    {
      "id": "weapon_gusenberg",
      "name": "Gusenberg Sweeper",
      "type": 5,
      "image": "https://assets.gm.miami/gtav/weapons/Gusenberg-sweeper-icon.png",
      "hash": "0x61012683"
    },
    {
      "id": "weapon_sniperrifle",
      "name": "Sniper Rifle",
      "type": -1,
      "image": "https://assets.gm.miami/gtav/weapons/Sniper-rifle-icon.png",
      "hash": "0x05FC3C11"
    },
    {
      "id": "weapon_heavysniper",
      "name": "Heavy Sniper",
      "type": -1,
      "image": "https://assets.gm.miami/gtav/weapons/Heavy-sniper-icon.png",
      "hash": "0x0C472FE2"
    },
    {
      "id": "weapon_heavysniper_mk2",
      "name": "Heavy Sniper Mk II",
      "type": 6,
      "image": "https://assets.gm.miami/gtav/weapons/Heavy-sniper-mk2-icon.png",
      "hash": "0xA914799"
    },
    {
      "id": "weapon_marksmanrifle",
      "name": "Marksman Rifle",
      "type": 6,
      "image": "https://assets.gm.miami/gtav/weapons/Marksman-rifle-icon.png",
      "hash": "0xC734385A"
    },
    {
      "id": "weapon_marksmanrifle_mk2",
      "name": "Marksman Rifle Mk II",
      "type": 6,
      "image": "https://assets.gm.miami/gtav/weapons/Marksman-rifle-mk2-icon.png",
      "hash": "0x6A6C02E0"
    },
    {
      "id": "weapon_precisionrifle",
      "name": "Precision Rifle",
      "type": -1,
      "image": "https://assets.gm.miami/gtav/weapons/Precision-rifle-icon.png",
      "hash": "0x6E7DDDEC"
    },
    {
      "id": "weapon_rpg",
      "name": "RPG",
      "type": 7,
      "image": "https://assets.gm.miami/gtav/weapons/Rocket-launcher-icon.png",
      "hash": "0xB1CA77B1"
    },
    {
      "id": "weapon_grenadelauncher",
      "name": "Grenade Launcher",
      "type": 7,
      "image": "https://assets.gm.miami/gtav/weapons/Grenade-launcher-icon.png",
      "hash": "0xA284510B"
    },
    {
      "id": "weapon_grenadelauncher_smoke",
      "name": "Grenade Launcher Smoke",
      "type": 7,
      "image": "https://assets.gm.miami/gtav/weapons/Grenade-launcher-icon.png",
      "hash": "0x4DD2DC56"
    },
    {
      "id": "weapon_minigun",
      "name": "Minigun",
      "type": 7,
      "image": "https://assets.gm.miami/gtav/weapons/Minigun-icon.png",
      "hash": "0x42BF8A85"
    },
    {
      "id": "weapon_firework",
      "name": "Firework Launcher",
      "type": 7,
      "image": "https://assets.gm.miami/gtav/weapons/Firework-launcher-icon.png",
      "hash": "0x7F7497E5"
    },
    {
      "id": "weapon_railgun",
      "name": "Railgun",
      "type": 7,
      "image": "https://assets.gm.miami/gtav/weapons/Railgun-icon.png",
      "hash": "0x6D544C99"
    },
    {
      "id": "weapon_hominglauncher",
      "name": "Homing Launcher",
      "type": 7,
      "image": "https://assets.gm.miami/gtav/weapons/Homing-launcher-icon.png",
      "hash": "0x63AB0442"
    },
    {
      "id": "weapon_compactlauncher",
      "name": "Compact Grenade Launcher",
      "type": -1,
      "image": "https://assets.gm.miami/gtav/weapons/Grenade-compact-launcher-icon.png",
      "hash": "0x0781FE4A"
    },
    {
      "id": "weapon_rayminigun",
      "name": "Widowmaker",
      "type": 7,
      "image": "https://assets.gm.miami/gtav/weapons/Widowmaker-icon.png",
      "hash": "0xB62D1F67"
    },
    {
      "id": "weapon_emplauncher",
      "name": "Compact EMP Launcher",
      "type": -1,
      "image": "https://assets.gm.miami/gtav/weapons/EMP-launcher-icon.png",
      "hash": "0xDB26713A"
    },
    {
      "id": "weapon_grenade",
      "name": "Grenade",
      "type": 8,
      "image": "https://assets.gm.miami/gtav/weapons/Grenade-icon.png",
      "hash": "0x93E220BD"
    },
    {
      "id": "weapon_bzgas",
      "name": "BZ Gas",
      "type": 8,
      "image": "https://assets.gm.miami/gtav/weapons/Bz-gas-icon.png",
      "hash": "0xA0973D5E"
    },
    {
      "id": "weapon_molotov",
      "name": "Molotov Cocktail",
      "type": 8,
      "image": "https://assets.gm.miami/gtav/weapons/Molotov-icon.png",
      "hash": "0x24B17070"
    },
    {
      "id": "weapon_stickybomb",
      "name": "Sticky Bomb",
      "type": 8,
      "image": "https://assets.gm.miami/gtav/weapons/Sticky-bomb-icon.png",
      "hash": "0x2C3731D9"
    },
    {
      "id": "weapon_proxmine",
      "name": "Proximity Mines",
      "type": 8,
      "image": "https://assets.gm.miami/gtav/weapons/Proximity-mines-icon.png",
      "hash": "0xAB564B93"
    },
    {
      "id": "weapon_snowball",
      "name": "Snowballs",
      "type": 8,
      "image": "https://assets.gm.miami/gtav/weapons/Snowball-icon.png",
      "hash": "0x787F0BB"
    },
    {
      "id": "weapon_pipebomb",
      "name": "Pipe Bombs",
      "type": 8,
      "image": "https://assets.gm.miami/gtav/weapons/Pipe-bomb-icon.png",
      "hash": "0xBA45E8B8"
    },
    {
      "id": "weapon_ball",
      "name": "Baseball",
      "type": 8,
      "image": "https://assets.gm.miami/gtav/weapons/Ball-icon.png",
      "hash": "0x23C9F95C"
    },
    {
      "id": "weapon_smokegrenade",
      "name": "Tear Gas",
      "type": 8,
      "image": "https://assets.gm.miami/gtav/weapons/Tear-gas-icon.png",
      "hash": "0xFDBC8A50"
    },
    {
      "id": "weapon_flare",
      "name": "Flare",
      "type": 8,
      "image": "https://assets.gm.miami/gtav/weapons/Flare-icon.png",
      "hash": "0x497FACC3"
    },
    {
      "id": "weapon_petrolcan",
      "name": "Jerry Can",
      "type": 9,
      "image": "https://assets.gm.miami/gtav/weapons/Petrolcan-icon.png",
      "hash": "0x34A67B97"
    },
    {
      "id": "gadget_parachute",
      "name": "Parachute",
      "type": 9,
      "image": "https://assets.gm.miami/gtav/weapons/Parachute-icon.png",
      "hash": "0xFBAB5776"
    },
    {
      "id": "weapon_fireextinguisher",
      "name": "Fire Extinguisher",
      "type": -1,
      "image": "https://assets.gm.miami/gtav/weapons/Fire2.png",
      "hash": "0x060EC506"
    },
    {
      "id": "weapon_hazardcan",
      "name": "Hazardous Jerry Can",
      "type": 9,
      "image": "https://assets.gm.miami/gtav/weapons/Petrolcan-icon.png",
      "hash": "0xBA536372"
    },
    {
      "id": "weapon_fertilizercan",
      "name": "Fertilizer Can",
      "type": -1,
      "image": "https://assets.gm.miami/gtav/weapons/Petrolcan-icon.png",
      "hash": "0x184140A1"
    }
  ]
  