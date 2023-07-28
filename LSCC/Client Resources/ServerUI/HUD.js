var global = require('./globals');
const {TranslateMessage: Translate} = require('./Translator');

const Natives = {
    IS_RADAR_HIDDEN: "0x157F93B036700462",
    IS_RADAR_ENABLED: "0xAF754F20EB5CD51A",
    SET_TEXT_OUTLINE: "0x2513DFB0FB8400FE"
};

var OnlinePlayersJSON = [];
mp.events.add("addPlayerToOnlineList", (name) => {
    if(OnlinePlayersJSON.includes(name)) return;

    OnlinePlayersJSON.push(name);
})
mp.events.add("removePlayerFromOnlineList", (name) => {
    if(!OnlinePlayersJSON.includes(name)) return;

    var idx = OnlinePlayersJSON.indexOf(name);
    OnlinePlayersJSON.splice(idx, 1);
});

mp.events.add("chat_requestAutoCompletePlayers", (query) => {
    if(!global.ServerUI) return;

    const players = OnlinePlayersJSON.filter(p => p.toLowerCase().startsWith(query.toLowerCase()));
    var playersToSet = players;
    if(playersToSet.length > 5) playersToSet = players.slice(0, 5);

    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.api.autoCompletePlayers = ${JSON.stringify(playersToSet)}`);
});

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
global.scoreboardToggled = false;
global.scoreboardCursor = false;
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
        if(global.menuToggled !== undefined && global.menuToggled) return;
        if(!global.chatStatus && 
            !(mp.players.local.getVariable("pLogged") === undefined || mp.players.local.getVariable("pLogged") == false)) 
        {
			if (mp.keys.isDown(16) === true) 
            { // Shift is pressed
                mp.storage.data.menu.nametags = !mp.storage.data.menu.nametags;
                global.renderNametags = mp.storage.data.menu.nametags;
            }
            else 
            {
                mp.events.callLocal("ToggleHud", !global.showHud);
            }
        }
    }
);

var ScreenNotifications = [];
mp.events.add("screenNotify", (text) => {
    ScreenNotifications.push({
        position:[0.5, 0.6+(ScreenNotifications.length*0.03)],
        alpha:0,
        text:text,
        scale:[0.42, 0.42],
        created:Date.now()
    });
});
mp.events.add('render', () => {
    renderScreenNotifications();
})
function renderScreenNotifications(){
    var notificationsToRemove = [];
    for(let i = 0 ; i < ScreenNotifications.length ; i++){

        mp.game.graphics.drawText(ScreenNotifications[i].text, ScreenNotifications[i].position, {
            font:4, outline:true, centre:true, 
            scale:ScreenNotifications[i].scale,
            color:[255,255,255, ScreenNotifications[i].alpha]
        });

        const currentTime = Date.now();
        const notificationLife = (currentTime - ScreenNotifications[i].created)
        if(notificationLife < 255){
            if(ScreenNotifications[i].alpha+10 < 255){
                ScreenNotifications[i].alpha += 10;
            }
            else{
                ScreenNotifications[i].alpha = 255;
            }
        }
        else{
            ScreenNotifications[i].position[1] -= 0.0002;

            if(notificationLife > 800){
                ScreenNotifications[i].alpha-=10;
            }
            if(ScreenNotifications[i].alpha <= 0){
                notificationsToRemove.push(ScreenNotifications[i].created);
            }
        }
    }
    var remIndObjects = ScreenNotifications.filter(i => notificationsToRemove.includes(i.created));
    ScreenNotifications = ScreenNotifications.filter(i => !remIndObjects.includes(i));
}



mp.events.add("actionMenu:addItem", (id, action, color, icon) => {
    if(!global.ServerUI) return;
    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.actionMenu.items.push({
        id: ${id},
        name: "${action}",
        color: "${color}",
        icon: "${icon}"
    });`);
});
mp.events.add("actionMenu:removeItem", (id) => {
    if(!global.ServerUI) return;
    if(global.ServerUI) global.ServerUI.execute(`
        if(gm.$refs.hud.$refs.base.$refs.actionMenu.items.find(i => i.id == ${id})){
            gm.$refs.hud.$refs.base.$refs.actionMenu.items.splice(gm.$refs.hud.$refs.base.$refs.actionMenu.items.indexOf(gm.$refs.hud.$refs.base.$refs.actionMenu.items.find(i => i.id == ${id})), 1);
        }
    `);
});
mp.events.add("actionMenu:removeAllItems", () => {
    if(!global.ServerUI) return;
    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.actionMenu.items = [];`);
});

mp.events.add('actionMenu:close', () => {
    if(!global.ServerUI) return;
    global.ActionMenuActive = false;
    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.actionMenu.enabled = false;`);
    mp.gui.cursor.show(false, false);
});
mp.events.add('actionMenu:open', () => {
    if(global.chatStatus == true || global.isHeliCamActive() || global.menuToggled == true || global.MDCActive == true || global.scoreboardToggled == true
        || global.VehicleSpawnerActive == true || global.WeaponEditorActive == true || global.StationPickerToggled == true || global.isHotwiring == true || global.ClothingEditorActive == true) return;

        if(!global.ServerUI) return;
    mp.game.invoke("0xFC695459D4D0E219", 0.5, 0.5); // SET CURSOR POSITION TO CENTER [x,y = 0.5,0.5]
    global.ActionMenuActive = true;
    mp.events.callLocal("actionMenu:removeAllItems");
    actionMenu_lobbyCheck();
    actionMenu_vehicleCheck();
    actionMenu_teamCheck();

    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.actionMenu.enabled = true;`);
    mp.gui.cursor.show(true, true);
});

let lastActionMenuClick = 0;

mp.events.add('actionMenu:clickItem', (actionID) => {
    if(!global.ServerUI) return;
    if(Date.now() - lastActionMenuClick < 100) return;
    lastActionMenuClick = Date.now();

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
            if(global.ServerUI) global.ServerUI.execute(`toast('${Translate("HOTKEY_HELP")}', '${Translate("MUST_HOLD_H")}', 'warning', 3000);`);
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
        case 15: // Autojoin command
        {
            mp.events.callRemote("actionMenu_SelectedItem", actionID);
            break;
        }
        case 16: // Silent Siren
        {
            if (mp.players.local.vehicle && mp.players.local.vehicle.getPedInSeat(-1) === mp.players.local.handle && mp.players.local.vehicle.getClass() === 18) {           
                mp.events.callRemote('SilentSirenSync', mp.players.local.vehicle);
            }
            break;
        }
        case 17:
        {
            mp.events.callLocal("startHeliCam");
            break;
        }
    }
});
function actionMenu_lobbyCheck(){
    let dmID = mp.players.local.getVariable("InDM");
    if(dmID != -1){
        mp.events.callLocal("actionMenu:addItem", 8, Translate("GO_TO_LOBBY"), "#f87171", "fa-door-open");  
        return;
    }
    let lobbyID = mp.players.local.getVariable("InLobby");
    if(lobbyID > -1 && mp.players.local.getVariable("Team") < 0){ // copchase lobby
        mp.events.callLocal("actionMenu:addItem", 6, Translate("EDIT_WEAPONS"), "#38bdf8", "fa-gun");   
        let color = "#a3e635";
        if(mp.players.local.getVariable("AutoJoin") == false) color = "#f87171";
        mp.events.callLocal("actionMenu:addItem", 15, Translate("AUTOJOIN"), color, "fa-joystick");   
    }
    else if(lobbyID == -4){ // derby
        mp.events.callLocal("actionMenu:addItem", 8, Translate("GO_TO_LOBBY"), "#f87171", "fa-door-open");  
    }
    else if(lobbyID == -3){ // freeroam
        mp.events.callLocal("actionMenu:addItem", 9, Translate("SPAWN_VEHICLE"), "#4ade80", "fa-car");   
        if(mp.players.local.isInAnyVehicle(true)){
            mp.events.callLocal("actionMenu:addItem", 7, Translate("VEHICLE_TUNING"), "#a78bfa", "fa-car-wrench");  
            mp.events.callLocal("actionMenu:addItem", 10, Translate("REPAIR_VEHICLE"), "#c084fc", "fa-car-bolt");  
        } 
        mp.events.callLocal("actionMenu:addItem", 8, Translate("GO_TO_LOBBY"), "#f87171", "fa-door-open");  
    }
}
// This function checks what team the player is in, and adds options for each team.
function actionMenu_teamCheck(){
    let team = mp.players.local.getVariable("Team");
    if(team >= 0){
        mp.events.callLocal("actionMenu:addItem", 3, Translate("PAINKILLER"), "#22c55e", "fa-pills");    
        
        let foundReviveTarget = false;
        mp.players.forEachInRange(mp.players.local.position, 5, (playa) => {
            if(!foundReviveTarget){
                if(playa.remoteId != mp.players.local.remoteId && playa.getVariable("Wounded") == 2 && 
                playa.getVariable("BeingRevived") == false && playa.getVariable("Team") == team){
                    foundReviveTarget = true;
                    mp.events.callLocal("actionMenu:addItem", 12, Translate("REVIVE"), "#fb923c", "fa-hand-holding-medical");  
                }
            }
        });

        if(team == 0){
            if(!mp.players.local.isInAnyVehicle(true)){
                mp.events.callLocal("actionMenu:addItem", 2, Translate("MEDKIT"), "#fb7185", "fa-kit-medical");      
                let isFound = false;
                mp.vehicles.forEachInRange(mp.players.local.position, 3.5, (vehicle) => {
                    if(!isFound)
                    {
                        if(vehicle.getVariable("Locked") == true && vehicle.getVariable("Team") != team){   
                            mp.events.callLocal("actionMenu:addItem", 4, Translate("LOCKPICK"), "#facc15", "fa-signature-lock");    
                            isFound = true;
                        }
                    }
                });
            }
        }
        else{
            mp.events.callLocal("actionMenu:addItem", 5, Translate("GPS_BEACON"), "#60a5fa", "fa-location-dot");   
            if(!mp.players.local.isInAnyVehicle(true)){
                mp.events.callLocal("actionMenu:addItem", 2, Translate("MEDKIT"), "#fb7185", "fa-kit-medical");  
                
                let foundDragoutTarget = false;
                let foundCuffTarget = false;
                mp.players.forEachInRange(mp.players.local.position, 3.5, (playa) => {
                    if(!foundDragoutTarget)
                    {
                        if(playa.getVariable("Team") == 0 && playa.isInAnyVehicle(true)){
                            mp.events.callLocal("actionMenu:addItem", 11, Translate("DRAG_OUT"), "#22d3ee", "fa-person-through-window");  
                            foundDragoutTarget = true;
                        }
                    }
                    if(!foundCuffTarget && !foundReviveTarget){
                        if(playa.getVariable("Team") == 0 && !playa.isInAnyVehicle(true)){
                            mp.events.callLocal("actionMenu:addItem", 12, Translate("ARREST"), "#fb923c", "fa-handcuffs");  
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
            mp.events.callLocal("actionMenu:addItem", 0, Translate("UNLOCK"), "#a3e635", "fa-lock-keyhole-open");
        else mp.events.callLocal("actionMenu:addItem", 0, Translate("LOCK"), "#f87171", "fa-lock-keyhole");

        mp.events.callLocal("actionMenu:addItem", 1, Translate("SEATBELT"), "#fb923c", "fa-shield-slash");
    
        if (mp.players.local.vehicle && mp.players.local.vehicle.getPedInSeat(-1) === mp.players.local.handle && mp.players.local.vehicle.getClass() === 18) {        
            mp.events.callLocal("actionMenu:addItem", 16, Translate("SILENT_SIREN"), "#818cf8", "fa-siren");
        }

        if(vehicle.getVariable("Fuel") == undefined || vehicle.getVariable("Fuel") > 0.0){
            if(vehicle.engine == true){
                mp.events.callLocal("actionMenu:addItem", 13, Translate("ENGINE"), "#60a5fa", "fa-engine");
            }
            else{
                if(vehicle.getVariable("Team") != mp.players.local.getVariable("Team")){
                    mp.events.callLocal("actionMenu:addItem", 14, Translate("HOTWIRE"), "#60a5fa", "fa-bolt");
                }
                else mp.events.callLocal("actionMenu:addItem", 13, Translate("ENGINE"), "#60a5fa", "fa-engine");
            }
        }

        if(vehicle.getClass() === 15 && mp.players.local.getVariable("Team") === 1){
            mp.events.callLocal("actionMenu:addItem", 17, "Heli Cam", "#a78bfa", "fa-helicopter");
        }
    }
    else{
        mp.vehicles.forEachInRange(mp.players.local.position, 3.5, (vehicle) => {
            if(vehicle.getVariable("Team") == mp.players.local.getVariable("Team"))
            {
                if(vehicle.getVariable("Locked") == true)
                    mp.events.callLocal("actionMenu:addItem", 0, Translate("UNLOCK"), "#a3e635", "fa-lock-keyhole-open");
                else mp.events.callLocal("actionMenu:addItem", 0, Translate("LOCK"), "#f87171", "fa-lock-keyhole");
                return;
            }
        });
    }
}

mp.events.add('ToggleNametags', (state) => {
    global.renderNametags = state;
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
    return; 
    if(chatOpacityTimeout != null){
        clearInterval(chatOpacityTimeout);
    }

    if(mp.storage.data.menu.ActiveChatOpacity !== undefined){
        if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.settings.opacity = ${mp.storage.data.menu.ActiveChatOpacity.toString()}`);
    }
    else if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.settings.opacity = 1.0`);

    // i know this is dumb but idk i cba to fix it it didnt work otherwise
    chatOpacityTimeout = setTimeout(() => {
        if(!global.chatStatus && global.ServerUI != null && global.ServerUI.active == true){
            if(mp.storage.data.menu.showInactiveChat == undefined){
                if(mp.storage.data.menu.InactiveChatOpacity == undefined){
                    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.settings.opacity = 0.5`);
                }
                else if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.settings.opacity = ${mp.storage.data.menu.InactiveChatOpacity.toString()}`);
            }
            else{
                if(mp.storage.data.menu.showInactiveChat == false){
                    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.settings.opacity = 0.0`);
                }
                else{
                    if(mp.storage.data.menu.InactiveChatOpacity == undefined){
                        if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.settings.opacity = 0.5`);
                    }
                    else if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.settings.opacity = ${mp.storage.data.menu.InactiveChatOpacity.toString()}`);                    
                }
            }
        }
    }, time);
}

var adminVehSpawner = false;



mp.events.add("vehicleSpawner:spawnVehicle", (modelname, col1, col2) => {
    global.VehicleSpawnerActive = false;
    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.menus.$refs.vehicleSpawner.enabled = false;`);
    mp.events.callRemote("Spawner_SpawnVehicle", modelname, adminVehSpawner, col1, col2);
    mp.gui.cursor.show(false, false);
});
mp.events.add("spawner_close", () => {
    global.VehicleSpawnerActive = false;
    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.menus.$refs.vehicleSpawner.enabled = false;`);
    mp.gui.cursor.show(false, false);
});
mp.events.add("OpenVehicleSpawner", (carname, admin) => {
    global.VehicleSpawnerActive = true;
    adminVehSpawner = admin;
    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.menus.$refs.vehicleSpawner.openSpawner('${carname}')`);
    mp.gui.cursor.show(true, true);
});

mp.events.add("weaponSpawner:giveWeapon", (weaponHash, ammo, targetName, attachments) => {
    global.VehicleSpawnerActive = false;
    mp.gui.cursor.show(false, false);
    
    mp.events.callRemote("WeaponSpawner_RequestGiveWeapon", weaponHash, ammo, targetName, attachments);
    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.menus.$refs.weaponSpawner.enabled = false;`);
});
mp.events.add("weaponSpawner:close", () => {
    global.VehicleSpawnerActive = false;
    mp.gui.cursor.show(false, false);
    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.menus.$refs.weaponSpawner.enabled = false;`);
});
mp.events.add("WeaponSpawner:show", (targetName, weaponName) => {
    global.VehicleSpawnerActive = true;
    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.menus.$refs.weaponSpawner.openSpawner('${weaponName}', '${targetName}')`);
    mp.gui.cursor.show(true, true);
});

mp.events.add("SendClanInvite", (clanName, clanID, senderName) => {
    mp.game.audio.playSoundFrontend(-1, "CHALLENGE_UNLOCKED",  "HUD_AWARDS", true);
    if(global.ServerUI) global.ServerUI.execute(`clanInvite("Clan Invitation", "You have been invited to <strong>${clanName}</strong> by ${senderName}.", 20000, ${clanID});`);
    let notifyscoreboardstr = `toast('${Translate("SHOW_CURSOR")}', '${Translate("CLAN_INVITE_TIP")}', 'nothing', 6500, 2);`;
    if(global.ServerUI) global.ServerUI.execute(notifyscoreboardstr);
});

var IsInInfiniteLoading = true;
var CanSkipLoading = false;
mp.events.add("startLoadingScreen", (time, message) => {
    if(mp.storage.data.menu.loadingscreens !== undefined && mp.storage.data.menu.loadingscreens == false){
        return;
    }

    if(time == -1){
        if(global.ServerUI) global.ServerUI.call('loadingscreen_start', 999999999);
        //if(message != "none")
        //    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.loadingScreen.loadingText = "${message}"`);
        //else if(global.ServerUI) global.ServerUI.execute(`gm.$refs.loadingScreen.loadingText = ""`);

        IsInInfiniteLoading = true;

        setTimeout(() => {
            if(mp.players.local != null && mp.players.exists(mp.players.local) && IsInInfiniteLoading == true){
                CanSkipLoading = true;
                let notifyscoreboardstr = `toast('${Translate("SKIP_LOADING")}', '${Translate("SKIP_LOADING_TIP")}', 'nothing', 10000, 2);`;
                if(global.ServerUI) global.ServerUI.execute(notifyscoreboardstr);
            }
        }, 10000);
    }
    else{
        if(global.ServerUI) global.ServerUI.call('loadingscreen_start', time);
        //if(message != "none")
        //    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.loadingScreen.loadingText = "${message}"`);
        //else if(global.ServerUI) global.ServerUI.execute(`gm.$refs.loadingScreen.loadingText = ""`);
    }
});
mp.keys.bind(0x20, false, function() {
    if(IsInInfiniteLoading == true && CanSkipLoading == true){
        mp.events.callLocal("stopLoadingScreen");

        let notifyscoreboardstr = `toast('${Translate("SKIP_LOADING")}', '${Translate("SKIP_LOADING_TIP")}', 'nothing', 1, 2);`;
        if(global.ServerUI) global.ServerUI.execute(notifyscoreboardstr);
    }
});
mp.events.add("stopLoadingScreen", () => {
    if(mp.storage.data.menu.loadingscreens !== undefined && mp.storage.data.menu.loadingscreens == false){
        return;
    }

    if(global.ServerUI) global.ServerUI.execute('gm.$refs.loadingScreen.hide();');
    IsInInfiniteLoading = false;
    CanSkipLoading = false;
});

mp.events.addCommand("fontsize", (size) => {
    
    if(size < 0.5 || size > 2.5){
        mp.gui.chat.push("!{#ff6347}[!] !{white}Font size must be between 0.5 and 2.5! (default: 1.0)");
        return;
    }
    mp.storage.data.chat.fontsizenew = size;
    mp.storage.flush();
    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.api.fontSize = ${size}`);
});
mp.events.addCommand("pagesize", (size) => {
    
    if(size < 4 || size > 24){
        mp.gui.chat.push("!{#ff6347}[!] !{white}Page size must be between 4 and 24! (default: 18)");
        return;
    }
    mp.storage.data.chat.pagesize = size;
    mp.storage.flush();
    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.api.pageSize = ${size}`);
});
mp.events.addCommand("timestamp", () => {
 
    mp.storage.data.chat.timestamp = !mp.storage.data.chat.timestamp;
    mp.storage.flush();
    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.api.timestamps = ${mp.storage.data.chat.timestamp.toString()}`);
    mp.gui.chat.push(`!{white}Chat timestamps have been turned ${(mp.storage.data.chat.timestamp == true ? `!{green}ON` : `!{red}OFF`)}`);
});

function pushMessageToChat(message){
    if(global.ServerUI == null){return;}
/*
    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.messages.push("${message}")`);
    increaseChatOpacity(4000);
*/
    mp.gui.chat.push(message);
}
function clearChat(){
    if(global.ServerUI == null) return;
    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.messages = [];`);
}

//let api = {"chat:push": mp.gui.chat.push, "chat:clear": clearChat, "chat:activate": global.activateChat, "chat:show": global.focusChat}; 

mp.events.add("pushMsgToChat", (msg) => {
    mp.gui.chat.push(msg);
})

mp.events.add("chat:push", (msg) => {
    mp.gui.chat.push(msg);
});

mp.events.add("chat:clear", (msg) => {
    clearChat();
});
mp.events.add("chat:activate", (toggle) => {
    mp.gui.chat.activate(toggle);
});
mp.events.add("chat:show", (toggle) => {
    mp.gui.chat.show(toggle);
});

mp.events.add("HideAllElementsExceptChat", (value) => {
    if(value == false){
        if(global.ServerUI) global.ServerUI.execute(
        `
            gm.$refs.hud.$refs.topRight.$refs.info.enabled = false; 
            gm.$refs.hud.$refs.topRight.$refs.killFeed.enabled = false;
            gm.$refs.hud.$refs.base.$refs.chat.api.toggle(${global.showHud.toString()});
        `);
    }
    else{
        if(global.ServerUI) global.ServerUI.execute(
            `
            gm.$refs.hud.$refs.topRight.$refs.info.enabled = ${mp.storage.data.menu.topRightInfo}; 
            gm.$refs.hud.$refs.topRight.$refs.killFeed.enabled = ${mp.storage.data.menu.killFeed};
            gm.$refs.hud.$refs.base.$refs.chat.api.toggle(${global.showHud.toString()});
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
    global.focusChat(false);
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
    if(global.chatStatusLate){
        mp.game.controls.disableControlAction(0, 200, true); // prevent menu from opening when pressing ESC for chat 
    }    
});
mp.keys.bind(0x1B, true, function() { // ESC key
    if(global.chatStatus){
        global.focusChat(false);
        mp.events.call("onOpenChatbox", false);       
    }
});
mp.keys.bind(0x0D, true, function() { // Enter Key
    if(global.chatStatus){
        global.focusChat(false);
        mp.events.call("onOpenChatbox", false);       
    }
});

mp.keys.bind(0x26, true, function() { // UP arrow
    if(global.chatStatus && global.ServerUI){
        if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.api.historyUp();`);
    }
});
mp.keys.bind(0x28, true, function() { // DOWN arrow
    if(global.chatStatus && global.ServerUI){
        if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.api.historyDown();`);
    }
});

mp.keys.bind(0x21, true, function() { // PAGE UP arrow
    if(global.chatStatus && global.ServerUI){
        if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.api.channelUp();`);
    }
});
mp.keys.bind(0x222, true, function() { // PAGE DOWN arrow
    if(global.chatStatus && global.ServerUI){
        if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.api.channelDown();`);
    }
});

mp.keys.bind(0xBF, true, function(){  // /? key
    if(global.ServerUI == null) return;
    if(global.menuToggled){
        if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.api.toggleActive(false);`);
        return;
    }
    if(mp.storage.data.menu.HUD !== undefined && mp.storage.data.menu.HUD == false){
        return;
    }
    if((global.VehicleSpawnerActive !== undefined && global.VehicleSpawnerActive == true) ||
    (global.WeaponEditorActive !== undefined && global.WeaponEditorActive == true) ||
    (global.ClothingEditorActive !== undefined && global.ClothingEditorActive == true) ||
    (global.ActionMenuActive !== undefined && global.ActionMenuActive == true) || 
    (global.StationPickerToggled !== undefined && global.StationPickerToggled == true) ||
    (global.isHotwiring !== undefined && global.isHotwiring == true) || global.isHeliCamActive()){
        if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.api.toggleActive(false);`);
        return;
    }
    if(!global.chatStatus)
    {
        if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.api.input = '/'`);
        global.focusChat(true);
        mp.events.call("onOpenChatbox", true);

        // The Anti-FUCK system: try to hide cursor instantly, but if it fails, hide it afterwards! :D very cool
        setTimeout(() => {
            mp.gui.cursor.show(true, false);
        }, 5);
        setTimeout(() => {
            mp.gui.cursor.show(true, false);
        }, 10);
        setTimeout(() => {
            mp.gui.cursor.show(true, false);
        }, 50);
    }
});

mp.keys.bind(0x54, true, function() { // T key
    if(global.ServerUI == null) return;
    if(global.menuToggled){
        if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.api.toggleActive(false);`);
        return;
    }
    if(mp.storage.data.menu.HUD !== undefined && mp.storage.data.menu.HUD == false){
        return;
    }
    if((global.VehicleSpawnerActive !== undefined && global.VehicleSpawnerActive == true) ||
    (global.WeaponEditorActive !== undefined && global.WeaponEditorActive == true) ||
    (global.ClothingEditorActive !== undefined && global.ClothingEditorActive == true) ||
    (global.ActionMenuActive !== undefined && global.ActionMenuActive == true) || 
    (global.StationPickerToggled !== undefined && global.StationPickerToggled == true) ||
    (global.isHotwiring !== undefined && global.isHotwiring == true) || global.isHeliCamActive()){
        if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.api.toggleActive(false);`);
        return;
    }
    if(!global.chatStatus)
    {
        global.focusChat(true);
        mp.events.call("onOpenChatbox", true);

        // The Anti-FUCK system: try to hide cursor instantly, but if it fails, hide it afterwards! :D very cool
        setTimeout(() => {
            mp.gui.cursor.show(true, false);
        }, 5);
        setTimeout(() => {
            mp.gui.cursor.show(true, false);
        }, 10);
        setTimeout(() => {
            mp.gui.cursor.show(true, false);
        }, 50);
    }
});
mp.events.add("closeChat", () => {         
    global.focusChat(false);
    mp.events.call("onOpenChatbox", false);  
});
mp.events.add("TogglePlayerChat", (toggle) => {
    global.activateChat(toggle);
});

mp.events.add('RestartHUD', () => {
    if(global.ServerUI !== null)
    {
        if(global.ServerUI) global.ServerUI.destroy();
        global.ServerUI = null;
    }

    //global.ServerUI = mp.browsers.new("package://ServerUI/index.html");
    setTimeout(() => {
        //if(global.ServerUI) global.ServerUI.markAsChat();
        //mp.gui.chat.activate(true);
        //mp.gui.chat.show(true);
        // if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.commands = ${JSON.stringify(global.CommandsDataJSON)}`);
        // if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.api.toggleActive = false;`)
        // if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.chat.settings = {
        //     opacity: 1,
        //     autoScroll: true,
        // }`);
    }, 1000);
    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.enabled = false;`);
});

let createdTooltips = []; // to keep track of created tooltips / preventing duplicates

mp.events.add('AddTooltip', (key, action, team) => {
    
    if(global.ServerUI == null) return;
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

    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.topRight.$refs.tooltips.tips.push({
        key: "${key}",
        action: "${action}",
        color: "${color}"
    })`);
});
mp.events.add('RemoveTooltip', (key) => {
    if(global.ServerUI == null) return;
    if(createdTooltips.indexOf(key) == -1) return;

    if(global.ServerUI) global.ServerUI.execute(`if(gm.$refs.hud.$refs.topRight.$refs.tooltips.tips.indexOf(gm.$refs.hud.$refs.topRight.$refs.tooltips.tips.filter(function(el){return el.key == "${key}"})[0]) != -1){
        gm.$refs.hud.$refs.topRight.$refs.tooltips.tips.splice(gm.$refs.hud.$refs.topRight.$refs.tooltips.tips.indexOf(gm.$refs.hud.$refs.topRight.$refs.tooltips.tips.filter(function(el){return el.key == "${key}"})[0]), 1);        
    }`);
    createdTooltips.splice(createdTooltips.indexOf(key), 1);
});

let keyTrunk = true; // true = key trunk, false = key seatbelt displayed

mp.events.add('ToggleTooltips', (status) =>
{
    mp.storage.data.menu.tooltips = status;
    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.topRight.$refs.tooltips.enabled = ${status.toString()}`);

    if(status)
    {
        mp.events.callLocal("AddTooltip", "M", Translate("MENU"), -1);
        mp.events.callLocal("AddTooltip", String.fromCharCode(global.curActionMenuBind), Translate("ACTION_MENU"), -1);
        mp.events.callLocal("AddTooltip", String.fromCharCode(global.curVoiceBind), Translate("VOICE_CHAT"), -1);
    }
    else {

        createdTooltips.forEach(key => {
            if(global.ServerUI) global.ServerUI.execute(`if(gm.$refs.hud.$refs.topRight.$refs.tooltips.tips.indexOf(gm.$refs.hud.$refs.topRight.$refs.tooltips.tips.filter(function(el){return el.key == "${key}"})[0]) != -1){
                gm.$refs.hud.$refs.topRight.$refs.tooltips.tips.splice(gm.$refs.hud.$refs.topRight.$refs.tooltips.tips.indexOf(gm.$refs.hud.$refs.topRight.$refs.tooltips.tips.filter(function(el){return el.key == "${key}"})[0]), 1);        
            }`);
        });

        createdTooltips = [];
    }
});

var lastClosestVehBoneHandle = "";

mp.events.add("TwoSecondsEvent", () => {

    if(mp.storage.data.menu.RealtimeSpeedo == false){
        if (global.specState == true && global.specTarget != null && mp.players.exists(global.specTarget)) 
        {
            if(global.specTarget.isInAnyVehicle(false) && global.specTarget.vehicle != undefined)
            {
                if(!speedo)
                {
                    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.bottomRight.$refs.speedo.enabled = true;`);
                    speedo = true;
                }
                if(speedoMax != ((mp.game.vehicle.getVehicleModelMaxSpeed(global.specTarget.vehicle.model) * 3.6) + 25).toFixed(0))
                {
                    speedoMax = ((mp.game.vehicle.getVehicleModelMaxSpeed(global.specTarget.vehicle.model) * 3.6) + 25).toFixed(0);
                    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.bottomRight.$refs.speedo.maxspeed = ${speedoMax};`);
                }
                if(speedoCur != (global.specTarget.vehicle.getSpeed() * 3.6).toFixed(0))
                {
                    speedoCur = (global.specTarget.vehicle.getSpeed() * 3.6).toFixed(0);
                    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.BottomRight.$refs.speedo.speed = ${speedoCur};`);
                }
            }
            else if(speedo)
            {
                speedo = false;
                speedoMax = 0;
                speedoCur = 0;
                if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.bottomRight.$refs.speedo.enabled = false;`);
            }
        }
        else {
            if(mp.players.local.isInAnyVehicle(false) && mp.players.local.vehicle != undefined && mp.storage.data.menu.Speedo == true)
            {
                if(!speedo)
                {
                    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.bottomRight.$refs.speedo.enabled = true;`);
                    speedo = true;
                }
                if(speedoMax != ((mp.game.vehicle.getVehicleModelMaxSpeed(mp.players.local.vehicle.model) * 3.6) + 25).toFixed(0))
                {
                    speedoMax = ((mp.game.vehicle.getVehicleModelMaxSpeed(mp.players.local.vehicle.model) * 3.6) + 25).toFixed(0);
                    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.bottomRight.$refs.speedo.maxspeed = ${speedoMax};`);
                }
                if(speedoCur != (mp.players.local.vehicle.getSpeed() * 3.6).toFixed(0))
                {
                    speedoCur = (mp.players.local.vehicle.getSpeed() * 3.6).toFixed(0);
                    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.bottomRight.$refs.speedo.speed = ${speedoCur};`);
                }
            }
            else if(speedo)
            {
                speedo = false;
                speedoMax = 0;
                speedoCur = 0;
                if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.bottomRight.$refs.speedo.enabled = false;`);
            }
        }
    }
    
    if(mp.storage.data.menu.tooltips == true)
    {
        if(mp.players.local.Team != 0 && mp.players.local.Team != 1){
            if(createdTooltips.indexOf("H") != -1) mp.events.callLocal("RemoveTooltip", "H");
        }

        if(mp.players.local.vehicle){
                
            if(mp.players.local.vehicle.getVariable("Locked") === true)
                if(createdTooltips.indexOf("L") == -1) mp.events.callLocal("AddTooltip", "L", Translate("UNLOCK"), -1);
            else 
                if(createdTooltips.indexOf("L") == -1) mp.events.callLocal("AddTooltip", "L", Translate("LOCK"), -1);
        }
        else{
            if(createdTooltips.indexOf("L") != -1) mp.events.callLocal("RemoveTooltip", "L");
            if(createdTooltips.indexOf("B") != -1) mp.events.callLocal("RemoveTooltip", "B");     
        }
        if(mp.players.local.getVariable("Team") == 1 && global.chaseRunning){
            if(createdTooltips.indexOf(String.fromCharCode(global.curGPSBind)) == -1) 
            {
                mp.events.callLocal("AddTooltip", String.fromCharCode(global.curGPSBind), "GPS", 1);
            }
            if(createdTooltips.indexOf(String.fromCharCode(global.curMDCBind)) == -1) 
            {
                mp.events.callLocal("AddTooltip", String.fromCharCode(global.curMDCBind), "MDC", 1);
            }
        }
        else{
            if(createdTooltips.indexOf(String.fromCharCode(global.curGPSBind)) != -1) mp.events.callLocal("RemoveTooltip", String.fromCharCode(global.curGPSBind));
            if(createdTooltips.indexOf(String.fromCharCode(global.curMDCBind)) != -1) mp.events.callLocal("RemoveTooltip", String.fromCharCode(global.curMDCBind));  
        }

        if(!mp.players.local.isInAnyVehicle(false)) 
        {
            if(createdTooltips.indexOf("J") != -1 && !keyTrunk)
            {
                mp.events.callLocal("RemoveTooltip", "J");
                keyTrunk = true;
            }

            let veh = global.getClosestVehicle(mp.players.local.position);

            if(veh !== undefined && mp.vehicles.exists(veh.vehicle) && veh.vehicle.handle !== 0)
            {
                let sidebones = [
                    "bonnet",
                    "boot"
                ];
          
                let closestHandle = "";
                let closestDist = 100.0;
                let bonePos = undefined;
                sidebones.forEach((side) => {
                    let bone = veh.vehicle.getBoneIndexByName(side);
                    if(bone != -1)
                    {
                        bonePos = veh.vehicle.getWorldPositionOfBone(bone);
                        let dist = global.calcDist(mp.players.local.position, bonePos);
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
                        if(closestHandle != lastClosestVehBoneHandle){
                            mp.events.callLocal("RemoveTooltip", "J");
                            mp.events.callLocal("removeTipByID", veh.handle);
                            if(closestHandle == "boot"){
                                mp.events.callLocal("addFloatTip", veh.handle, 'J', Translate("TRUNK"), bonePos.x, bonePos.y, bonePos.z, 7, 3.5);
                                mp.events.callLocal("AddTooltip", "J", Translate("TRUNK"), -1);
                            }
                            else if(closestHandle == "bonnet"){
                                mp.events.callLocal("addFloatTip", veh.handle, 'J', Translate("BONNET"), bonePos.x, bonePos.y, bonePos.z, 7, 3.5);
                                mp.events.callLocal("AddTooltip", "J", Translate("BONNET"), -1);
                            }
                            
                            lastClosestVehBoneHandle = closestHandle;
                        }
               
                      
                    }
                    else if(createdTooltips.indexOf("J") != -1){
                        mp.events.callLocal("removeTipsByKey", 'J');
                        mp.events.callLocal("RemoveTooltip", "J");
                    }
                }
                else if(createdTooltips.indexOf("J") != -1){
                    mp.events.callLocal("removeTipsByKey", 'J');
                    mp.events.callLocal("RemoveTooltip", "J");
                }
            }
            else if(createdTooltips.indexOf("J") != -1){
                mp.events.callLocal("removeTipsByKey", 'J');
                mp.events.callLocal("RemoveTooltip", "J");
            }
        }
        else
        {
            if((createdTooltips.indexOf("J") != -1 && keyTrunk) || createdTooltips.indexOf("J") == -1)
            {
                mp.events.callLocal("removeTipsByKey", 'J');
                
                /*
                if(mp.players.local.vehicle.getClass() != 8 && mp.players.local.vehicle.model != mp.game.joaat('policeb'))
                {
                    mp.events.callLocal("AddTooltip", "J", "Seatbelt", -1);
                }
                */

                keyTrunk = false;
            }
        }    
    }

    if(mp.storage.data.menu.HUD == false)
    {
        if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.enabled = false;`);
    }
    global.renderNametags = mp.storage.data.menu.nametags;
});



mp.events.add('browserDomReady', (browser) => {
    if(global.ServerUI != null && browser == global.ServerUI)
    {
        // Set vars that aren't reset on login and shit. Basically init().
        if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.topRight.$refs.info.enabled = ${mp.storage.data.menu.topRightInfo}; gm.$refs.hud.$refs.topRight.$refs.killFeed.enabled = ${mp.storage.data.menu.killFeed};`);
        if(global.ServerUI) global.ServerUI.call("hud_minilist_toggle", true);
        if(global.ServerUI) global.ServerUI.call("hud_minilist_items_set", []);

        // Initialize miniHUD
        miniHUDVars["bottom"] = ((resolution.y - (getMinimapAnchor().bottomY * resolution.y)) + 0);
        miniHUDVars["left"] = ((getMinimapAnchor().rightX * resolution.x) + 10) + ((mp.storage.data.menu.alwaysExpandedMap == true) ? 162 : 0);

        if(global.ServerUI) global.ServerUI.call("hud_minilist_position_set", `${miniHUDVars["left"]}px`, `${miniHUDVars["bottom"]}px`);

        var miniHUD_items = [];
        miniHUD_items.push({ 
            id:1,
            enabled:false,
            name: "ping",
            icon: "fa-stopwatch",
            value: "0 ms",
            color: "#fbbf24"
        });
        miniHUD_items.push({ 
            id:2,
            enabled:false,
            name: "packetloss",
            icon: "fa-minus",
            value: "0.0%",
            color: "#fb923c"
        });
        miniHUD_items.push({ 
            id:3,
            enabled:miniHUDVars["jailed"],
            name: "jail",
            icon: "fa-ban",
            value: "N/A",
            color: "#fb7185"
        });
        miniHUD_items.push({ 
            id:4,
            enabled:false,
            name: "visible",
            icon: "fa-globe",
            value: "Visible",
            color: "#60a5fa"
        });
        miniHUD_items.push({ 
            id:5,
            enabled:true,
            name: "voice",
            icon: "fa-microphone",
            value: "Standby",
            color: "#818cf8"
        });
        miniHUD_items.push({ 
            id:6,
            enabled:true,
            name: "direction",
            icon: "fa-compass",
            value: "N/A",
            color: "#a3e635"
        });
        miniHUD_items.push({ 
            id:7,
            enabled:true,
            name: "street",
            icon: "fa-location-dot",
            value: "N/A",
            color: "#4ade80"
        });
        miniHUD_items.push({ 
            id:8,
            enabled:true,
            name: "zone",
            icon: "fa-location-crosshairs",
            value: "N/A",
            color: "#34d399"
        });
        global.ServerUI.call('hud_minilist_items_set', miniHUD_items);
        mp.events.callLocal("ToggleTooltips", mp.storage.data.menu.tooltips);

        if(mp.storage.data.menu.HUD == false)
        {
            if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.enabled = false`);
        }
    }
});

mp.events.add('ToggleHud', (toggle) => {
    global.showHud = toggle;
    
    if(global.ServerUI != null)
    {
        if(mp.storage.data.menu.HUD == false)
        {
            if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.enabled = false`);
            mp.storage.data.menu.HUD = false;
        }

        if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.enabled = ${toggle};`);
        mp.storage.data.menu.HUD = toggle;
    }

    global.activateChat(global.showHud);
    mp.game.ui.displayRadar(global.showHud);
});

mp.keys.bind(0x75, false, () => {
    global.showReticule = !global.showReticule;
});

mp.keys.bind(0x74, false, () => {
    if(global.menuToggled !== undefined && global.menuToggled) return;
    if(global.scoreboardToggled == true) return;
    if(Date.now() - global.lastChatToggle >= 500 && global.chatStatus == false && 
        (mp.players.local.getVariable("pLogged") !== undefined && mp.players.local.getVariable("pLogged") == true))
    {
        global.traceLastFunc(`[Players] Attempt tog /DL`);
        
        mp.events.callRemote("OnScriptedKeyPress", 0x74, true);
    }
});

mp.events.add('ResetKillFeed', () => {
    if(global.ServerUI != null)
    {
        if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.topRight.$refs.killFeed.killFeed = [];`);
    }
});

mp.events.add("pushToKillFeed", (killer, killerColor, weapon, victim, victimColor) => {
    if(global.ServerUI != null) if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.topRight.$refs.killFeed.add('${killer}', '${killerColor}', '${weapon}', '${victim}', '${victimColor}');`)
});

mp.events.add("pushToKillFeed2", (victim, color, cause) => {
    if(global.ServerUI != null) if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.topRight.$refs.killFeed.add("${victim}", "${color}", "${cause}");`)
});

mp.events.add("UpdateRateEvent", () => {

    // only do stuff if radar is enabled and visible
    let streetName = "N/A";
    let zoneName = "N/A";
    if (mp.game.invoke(Natives.IS_RADAR_ENABLED) && !mp.game.invoke(Natives.IS_RADAR_HIDDEN)) {
        minimap = getMinimapAnchor();

        /*
        const position = mp.players.local.position;
        let getStreet = mp.game.pathfind.getStreetNameAtCoord(position.x, position.y, position.z, 0, 0);

        global.zoneName = mp.game.ui.getLabelText(mp.game.zone.getNameOfZone(position.x, position.y, position.z));
        global.streetName = mp.game.ui.getStreetNameFromHashKey(getStreet.streetName);
        */
        global.zoneName = mp.game.hud.getCurrentAreaNameString();
        global.streetName = mp.game.hud.getCurrentStreetNameString();
        if(global.streetName.length <= 0) global.streetName = global.zoneName;
    }
    
    if(global.ServerUI != null && (!(mp.players.local.getVariable("pLogged") === undefined || mp.players.local.getVariable("pLogged") == false || mp.players.local.getVariable("InLobby") === undefined)))
    {
        if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.topRight.$refs.info.points = ${mp.players.local.getVariable("Points")};`);
        if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.topRight.$refs.info.donatorPoints = ${mp.players.local.getVariable("DonatorPoints")};`);
        
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
        
        if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.topRight.$refs.info.lobbyId = "${lobbyStr}";`);

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

        if(global.currentMuteTime >= 1)
        {
            voiceString = `Muted for ${TimeFormat(global.currentMuteTime)}`;
        }

        let jailString = "N/A";
        if(global.currentJailTime >= 1)
        {
            jailString = `Jailed for ${TimeFormat(global.currentJailTime)}`;
        }

        if(mp.players.local.getVariable("VisibleBlip") != miniHUDVars["visible"])
        {
            miniHUDVars["visible"] = mp.players.local.getVariable("VisibleBlip");
            if(global.ServerUI) global.ServerUI.call("hud_minilist_item_toggle", 4, miniHUDVars["visible"]);
        }
        if(voiceString != miniHUDVars["voice"])
        {
            miniHUDVars["voice"] = voiceString;
            if(global.ServerUI) global.ServerUI.call("hud_minilist_item_value_set", 5, miniHUDVars["voice"]);
        }
        if(dirtext != miniHUDVars["direction"])
        {
            miniHUDVars["direction"] = dirtext;
            if(global.ServerUI) global.ServerUI.call("hud_minilist_item_value_set", 6, miniHUDVars["direction"]);
        }
        if(global.streetName != miniHUDVars["street"])
        {
            miniHUDVars["street"] = global.streetName;
            if(global.ServerUI) global.ServerUI.call("hud_minilist_item_value_set", 7, miniHUDVars["street"]);
        }
        if(global.zoneName != miniHUDVars["zone"])
        {
            miniHUDVars["zone"] = global.zoneName;
            if(global.ServerUI) global.ServerUI.call("hud_minilist_item_value_set", 8, miniHUDVars["zone"]);
        }

        if(global.currentJailTime >= 1)
        {
            if(miniHUDVars["jailed"] != true)
            {
                miniHUDVars["jailed"] = true;
                if(global.ServerUI) global.ServerUI.call("hud_minilist_item_toggle", 3, miniHUDVars["jailed"]);
            }
            miniHUDVars["jail"] = jailString;
            if(global.ServerUI) global.ServerUI.call("hud_minilist_item_value_set", 3, miniHUDVars["jail"]);
        }
        else if(miniHUDVars["jailed"] == true)
        {
            miniHUDVars["jailed"] = false;
            if(global.ServerUI) global.ServerUI.call("hud_minilist_item_toggle", 3, miniHUDVars["visible"]);
        }

        if (global.specState == true && global.specTarget != null && mp.players.exists(global.specTarget)) 
        {
            if(miniHUDVars["spec"] == false)
            {
                miniHUDVars["spec"] = true;
                if(global.ServerUI) global.ServerUI.call("hud_minilist_item_toggle", 1, true);
                if(global.ServerUI) global.ServerUI.call("hud_minilist_item_toggle", 2, true);

                if(global.ServerUI) global.ServerUI.call("hud_minilist_item_toggle", 6, false);
            }
        
            if(miniHUDVars["packetloss"] != global.specTarget.getVariable("Packetloss").toFixed(2))
            {
                miniHUDVars["packetloss"] = global.specTarget.getVariable("Packetloss").toFixed(2);
                if(global.ServerUI) global.ServerUI.call("hud_minilist_item_value_set", 2, `${miniHUDVars["packetloss"]}%`);
            }
            if(miniHUDVars["ping"] != global.specTarget.getVariable("Ping"))
            {
                miniHUDVars["ping"] = global.specTarget.getVariable("Ping");
                if(global.ServerUI) global.ServerUI.call("hud_minilist_item_value_set", 1, `${miniHUDVars["ping"]}ms`);
            }
        }
        else if(miniHUDVars["spec"] == true) 
        {
            miniHUDVars["spec"] = false;
            if(global.ServerUI) global.ServerUI.call("hud_minilist_item_toggle", 1, false);
            if(global.ServerUI) global.ServerUI.call("hud_minilist_item_toggle", 2, false);

            if(global.ServerUI) global.ServerUI.call("hud_minilist_item_toggle", 6, true);
        }
    }
});

mp.events.add('click', (x, y, upOrDown, leftOrRight, relativeX, relativeY, worldPosition, hitEntity) => {
    if(global.scoreboardToggled && leftOrRight === 'right' && upOrDown === 'down')
    {
        if(!global.scoreboardCursor){
            global.scoreboardCursor = true;    
            mp.game.invoke("0xFC695459D4D0E219", 0.5, 0.5); // SET CURSOR POSITION TO CENTER [x,y = 0.5,0.5]
        }
        mp.gui.cursor.show(true, true);
    }
});
mp.events.add("player_voice_mute_toggle", (username) => {
    mp.events.callRemote("Menu_OnToggleVoiceBlock", username);
    global.UpdateStatsVars(mp.players.local, false);
});
mp.events.add("menu_punishments_punish", (username, punishmentType, reason, durationMinutes) => {
    mp.events.callRemote("Menu_OnPunishPlayer", username, punishmentType, reason, durationMinutes);
});
mp.events.add("spectate_player", (username) => {
    mp.events.callRemote("Menu_OnSpectatePlayer", username);
});
mp.events.add("menu_scoreboard_closed", () => {
    global.scoreboardCursor = false;
    mp.gui.cursor.show(false, false);
    global.scoreboardToggled = false;

    if(updater != null) clearInterval(updater);
    updater = null;
    mp.events.call("TogglePlayerChat", true);

});
mp.events.add('toggle_cursor', (toggle) => { mp.gui.cursor.show(toggle, toggle) });
function ToggleScoreboard()
{
    if (global.ServerUI == null) return;

    if(global.scoreboardToggled)
    {
        global.scoreboardToggled = false;

        if(updater != null) clearInterval(updater);
        updater = null;

        mp.events.call("TogglePlayerChat", true);
        //if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.scoreboard.disableInput = true;`);
		// if(global.ServerUI) global.ServerUI.execute(`gm.$refs.scoreboard.setPlayers([]);`);
        if(global.ServerUI) global.ServerUI.call('menu_scoreboard_toggle', false);

        if(global.scoreboardCursor)
        {
            global.scoreboardCursor = false;
            mp.gui.cursor.show(false, false);
        }
    }
    else
    {
        global.scoreboardToggled = true;
        mp.events.call("TogglePlayerChat", false);

        if(global.ServerUI) global.ServerUI.call('menu_scoreboard_toggle', true);
        //if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.scoreboard.disableInput = true;`);
		// if(global.ServerUI) global.ServerUI.execute(`gm.$refs.scoreboard.setPlayers([]);`);

        let notifyscoreboardstr = `toast('${Translate("SHOW_CURSOR")}', '${Translate("SCOREBOARD_TIP")}', 'nothing', 6500, 2);`;
        if(global.ServerUI) global.ServerUI.execute(notifyscoreboardstr);

        updatePlayers();

        updater = setInterval(function()
        {
            if(global.chatStatus && global.scoreboardToggled)
            {
                ToggleScoreboard();
            }
            else if(!global.chatStatus && global.scoreboardToggled)
            {
                updatePlayers();
            }
        }, 1000);
    }
    return;
}

function updatePlayers()
{
    if(mp.players.local !== undefined && mp.players.exists(mp.players.local))
        global.UpdateStatsVars(mp.players.local);

    let lobbyStr = '0';
	let colr = 255;
	let colg = 255;
	let colb = 255;
	let ping = 65535;
	let level = 0;
    let squad = -1;
	
    let scoreboard_players = [];
    mp.players.forEach(sb => {
        lobbyStr = "-";
        colr = 255;
        colg = 255;
        colb = 255;
        ping = 0;
        level = 0;
        squad = -1;

        let nameToDisplay = sb.name;
        let pingToDisplay = sb.getVariable("Ping");
        let levelToDisplay = sb.getVariable("Level");
        if(sb.getVariable("FakeNameOn") === true && sb.getVariable("FakeName") && sb.getVariable("FakeName").length > 0){
            nameToDisplay = sb.getVariable("FakeName");
            pingToDisplay = sb.getVariable("FakeNamePing");
            levelToDisplay = sb.getVariable("FakeNameLevel");
        }
        

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

        let roomTeam = -1;
        if(sb.getVariable("Team") > -1) roomTeam = sb.getVariable("Team");
        if(sb.getVariable("InDM") > -1) roomTeam = -2;

        scoreboard_players.push({
            id:sb.remoteId,
            donator:sb.getVariable("Donator"),
            roomId:sb.getVariable("InLobby"),
            username:nameToDisplay,
            level:levelToDisplay,
            ping:pingToDisplay,
            roomTeam:roomTeam
        });
    });

    
    if(global.ServerUI){
        global.ServerUI.call('menu_scoreboard_players_set', scoreboard_players);
    }

    // if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.scoreboard.$refs.global.actions = [
    //     {
    //         id: 1,
    //         name: "Mute",
    //         icon: "fa-volume-xmark",
    //         disabledName: "Unmute",
    //         disabledIcon: "fa-volume",
    //         disableKey: "muted",
    //         showKey: "showMute",
    //     },
    // ]`);
    return;
}

mp.keys.bind(
    0x1B,
    false,
    () => {
        if(global.ServerUI != null && global.scoreboardToggled) ToggleScoreboard();
    }
);

let cursorF2toggled = false;
mp.keys.bind(0x71, false, function () {
    if(global.scoreboardToggled)
    {
        if(global.scoreboardCursor)
        {
            global.scoreboardCursor = false;
            mp.gui.cursor.show(false, false);
            if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.scoreboard.disableInput = true;`);
            
            if(!scoreboardBinded) mp.events.callLocal('ScoreboardToggleTyping', false);
        }
        else
        {
            global.scoreboardCursor = true;
            mp.gui.cursor.show(true, true);
            if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.scoreboard.disableInput = false;`);

            //if(scoreboardBinded) mp.events.callLocal('ScoreboardToggleTyping', true);
        }
    }
    else{
        if(!cursorF2toggled){
            mp.gui.cursor.show(true, true);
            cursorF2toggled = true;
            mp.game.invoke("0xFC695459D4D0E219", 0.5, 0.5); // SET CURSOR POSITION TO CENTER [x,y = 0.5,0.5]
        }
        else{
            mp.gui.cursor.show(false, false);
            cursorF2toggled = false;
        }
    }
});

mp.events.add("OneSecondEvent", () => {
    if(global.menuToggled == true || global.VehicleSpawnerActive == true || global.WeaponEditorActive == true || global.ClothingEditorActive == true 
    || global.ActionMenuActive == true || global.StationPickerToggled == true){
        mp.gui.cursor.show(true, true);
    }
});

function ScoreboardKeyTrigger()
{
    if(mp.players.local.getVariable("pLogged") === undefined || mp.players.local.getVariable("pLogged") == false) return;
    if(Date.now() - global.lastChatToggle <= 500) return;
    if(global.menuToggled !== undefined && global.menuToggled) return;
    if(global.chatStatus && !global.scoreboardToggled) return;
    if(global.menuToggled) return;

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
    if(!global.showHud && mp.players.local.getVariable("pLogged") == true) return;

    /*
	let insertString = `iziToast.show({
		message: "${msg}",
		icon: "${icon}",
		color: "red",
		timeout: 5000,
		close: true		
	});`;
    */
    //let insertString = `toast(null, \`${msg}\`, 'error', 5000);`;
    //if(global.ServerUI != null) if(global.ServerUI) global.ServerUI.execute(insertString);   
    if(global.ServerUI) global.ServerUI.call('notification_send', msg, "error", 5000);
});

mp.events.add("HUDNotify", (msg, type, icon, close, timeout, orientation, displaymode) => {
    if(!global.showHud && mp.players.local.getVariable("pLogged") == true) return;
    
    let displayModeNumber = 0;
    if(displaymode == "replace") displayModeNumber = 2;

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
    //let insertString = `toast(null, \`${msg}\`, '${type}', ${timeout}, ${displayModeNumber});`;
    //if(global.ServerUI != null) if(global.ServerUI) global.ServerUI.execute(insertString);
    if(global.ServerUI) global.ServerUI.call('notification_send', msg, type, timeout);
});
// bottomRight, bottomLeft, topRight, topLeft, topCenter, bottomCenter, center
mp.events.add("HUDNotify2", (title, msg, orientation, displaymode) => {
    if(!global.showHud && mp.players.local.getVariable("pLogged") == true) return;
    
    /*
    let insertString = `iziToast.show({`;
    insertString += `title: \`${title}\`, `;
    insertString += `message: \`${msg}\`, `;
    if(orientation.length > 1) insertString += `position: \`${orientation}\`, `;
    if(displaymode.length > 1) insertString += `displayMode: \`${displaymode}\`, `;
    insertString += "});";
    */
    //let insertString = `toast(\`${title}\`, \`${msg}\`, 'nothing', 5000, 2);`;
    //if(global.ServerUI != null) if(global.ServerUI) global.ServerUI.execute(insertString);
    
    if(global.ServerUI) global.ServerUI.call('notification_send', msg, "info", 4000);
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
        if(global.ServerUI != null){
        let wepData = global.weaponData.find(w => w.Hash == mp.players.local.weapon);
        if(!wepData) wepData = global.weaponData.find(w => w.Hash == (mp.players.local.weapon-4294967296));
        
        if(!wepData){
            wepData = {Name:"Undefined Weapon", ImageSrc:"https://assets.gm.miami/gtav/weapons/Baseball-bat-icon.png"};
        }

        let weapon_hash = mp.players.local.weapon; // returns weapon as a hash => uint
        let ammoClip = mp.players.local.getAmmoInClip(weapon_hash); // returns ur ammo in clip
        let ammoWeapon  = mp.players.local.getWeaponAmmo(weapon_hash); // returns ur wep ammo
        let clipSize = mp.game.weapon.getWeaponClipSize(weapon_hash);

        if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.topRight.$refs.info.equippedWeapon = {
            name: "${wepData.Name}",
            ammo: ${ammoClip},
            maxAmmo: ${ammoWeapon + (clipSize-ammoClip)},
            image: "${wepData.ImageSrc}",
            mode: "${global.FireModeDisplayText}"
          }`);
        }
    }
    else{
        if(global.ServerUI != null){
            if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.topRight.$refs.info.equippedWeapon = null;`);
        }
    }

    if(global.showCustomCrosshair){
        if(global.alwaysShowCustomCrosshair == true){
            if(!global.customCrosshairOn){
                if(!mp.game.player.isFreeAiming()){         
                    if(!global.menuToggled)                     
                    {
                        global.customCrosshairOn = true;
                    }
                }
                else{
                    if(global.isHoldingSniper()){
                        if(global.showCustomCrosshairInSniper){
                            global.customCrosshairOn = true;                           
                        }
                        else{
                            global.customCrosshairOn = false;                           
                        }
                    }
                    else{
                        global.customCrosshairOn = true;
                    }
                }
            }
            else{
                if(mp.game.player.isFreeAiming()){                              
                    if(global.isHoldingSniper()){
                        if(global.showCustomCrosshairInSniper){
                            global.customCrosshairOn = true;                          
                        }
                        else{
                            global.customCrosshairOn = false;                            
                        }
                    }
                }               
            }
        }
        else{
            if(mp.game.invoke('0x68EDDA28A5976D07') == 1){
                if(!global.customCrosshairOn){
                    if(!mp.game.player.isFreeAiming()){                              
                        global.customCrosshairOn = true;
                    }
                    else{
                        if(global.isHoldingSniper()){
                            if(global.showCustomCrosshairInSniper){
                                global.customCrosshairOn = true;                          
                            }
                            else{
                                global.customCrosshairOn = false;                                  
                            }
                        }
                        else{
                            global.customCrosshairOn = true;
                        }
                    }
                }
            }
            else{
                if(global.customCrosshairOn){
                    global.customCrosshairOn = false;
                }
            }   
        }
    }

    if(!global.HideAllCrosshairs.AntiPeek){
        if(!global.showReticule)
        {
            mp.game.ui.hideHudComponentThisFrame(14); // RETICULE
            global.customCrosshairOn = false;
        }
        if(global.isLockpicking == true || global.ActionMenuActive == true){
            mp.game.ui.hideHudComponentThisFrame(14); // RETICULE
            global.customCrosshairOn = false;
        }
        if(global.customCrosshairOn == true){
            if(!wasCustomCrosshairOn){
                if(global.ServerUI) global.ServerUI.call('crosshair_toggle', true);
                wasCustomCrosshairOn = true;
            }
            mp.game.ui.hideHudComponentThisFrame(14); // RETICULE
        }
        else{
            if(wasCustomCrosshairOn == true){
                if(global.ServerUI) global.ServerUI.call('crosshair_toggle', false);
                wasCustomCrosshairOn = false;
            }
        }
    }
    else{
        mp.game.ui.hideHudComponentThisFrame(14); // RETICULE
        if(wasCustomCrosshairOn == true){
            if(global.ServerUI) global.ServerUI.call('crosshair_toggle', false);
            wasCustomCrosshairOn = false;
            global.customCrosshairOn = false;
        }
    }
 

    if(mp.players.local.getVariable("pLogged") !== undefined && mp.players.local.getVariable("pLogged") == true)
    {       
        if ((resolution.x != mp.game.graphics.getScreenActiveResolution(0,0).x || miniHUDVars["left"] != (((getMinimapAnchor().rightX * resolution.x) + 10)) + ((mp.storage.data.menu.alwaysExpandedMap == true) ? 162 : 0)) && global.ServerUI != null)
        {
            resolution = mp.game.graphics.getScreenActiveResolution(0,0);
            
            miniHUDVars["bottom"] = ((resolution.y - (getMinimapAnchor().bottomY * resolution.y)) + 0);
            miniHUDVars["left"] = ((getMinimapAnchor().rightX * resolution.x) + 10) + ((mp.storage.data.menu.alwaysExpandedMap == true) ? 162 : 0);

            if(global.ServerUI) global.ServerUI.call("hud_minilist_position_set", `${miniHUDVars["left"]}px`, `${miniHUDVars["bottom"]}px`);
            //if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.miniHud.left = "${miniHUDVars["left"]}px";`);
            //if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.miniHud.bottom = "${miniHUDVars["bottom"]}px";`);  
        }
        // if (mp.storage.data.menu.alwaysExpandedMap == true)
        // {
        //     mp.game.ui.setRadarBigmapEnabled(true, false);
        // }
        
        if (global.specState == true && global.specTarget != null && mp.players.exists(global.specTarget)) 
        {
            if((mp.players.local.getHealth() != global.specTarget.getHealth() || mp.players.local.getArmour() != global.specTarget.getArmour()) && 
                Date.now() - lastSpecHPChange >= 500)
            {
                mp.events.callRemote("SpectateChangeHPAR", global.specTarget.getHealth(), global.specTarget.getArmour());
                lastSpecHPChange = Date.now();
            }

            if(global.specTarget.isInAnyVehicle(false))
            {
                if(!speedo)
                {
                    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.bottomRight.$refs.speedo.enabled = true;`);
                    speedo = true;
                }
                if(mp.storage.data.menu.RealtimeSpeedo == true){
                    if(speedoMax != ((mp.game.vehicle.getVehicleModelMaxSpeed(global.specTarget.vehicle.model) * 3.6) + 25).toFixed(0))
                    {
                        speedoMax = ((mp.game.vehicle.getVehicleModelMaxSpeed(global.specTarget.vehicle.model) * 3.6) + 25).toFixed(0);
                        if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.bottomRight.$refs.speedo.maxspeed = ${speedoMax};`);
                    }
                    if(speedoCur != (global.specTarget.vehicle.getSpeed() * 3.6).toFixed(0))
                    {
                        speedoCur = (global.specTarget.vehicle.getSpeed() * 3.6).toFixed(0);
                        if(global.ServerUI) global.ServerUI.execute(`gm.$refs.BottomRight.$refs.speedo.speed = ${speedoCur};`);
                    }
                }
         
            }
            else if(speedo)
            {
                speedo = false;
                speedoMax = 0;
                speedoCur = 0;
                if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.bottomRight.$refs.speedo.enabled = false;`);
            }
        }
        else {
            if(mp.players.local.isInAnyVehicle(false))
            {
                if(!speedo && mp.storage.data.menu.Speedo == true)
                {
                    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.bottomRight.$refs.speedo.enabled = true;`);
                    speedo = true;
                }
                if(mp.storage.data.menu.RealtimeSpeedo == true){
                    if(speedoMax != ((mp.game.vehicle.getVehicleModelMaxSpeed(mp.players.local.vehicle.model) * 3.6) + 25).toFixed(0))
                    {
                        speedoMax = ((mp.game.vehicle.getVehicleModelMaxSpeed(mp.players.local.vehicle.model) * 3.6) + 25).toFixed(0);
                        if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.bottomRight.$refs.speedo.maxspeed = ${speedoMax};`);
                    }
                    if(speedoCur != (mp.players.local.vehicle.getSpeed() * 3.6).toFixed(0))
                    {
                        speedoCur = (mp.players.local.vehicle.getSpeed() * 3.6).toFixed(0);
                        if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.bottomRight.$refs.speedo.speed = ${speedoCur};`);
                    }
                }
            }
            else if(speedo)
            {
                speedo = false;
                speedoMax = 0;
                speedoCur = 0;
                if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.bottomRight.$refs.speedo.enabled = false;`);
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

            let dist = global.calcDist(veh.position, position);
            if(isVehicleInLOS(veh, position) && dist <= 32.0)
            {
                let vehName = mp.game.ui.getLabelText(mp.game.vehicle.getDisplayNameFromVehicleModel(veh.model));
                let passengers = veh.getNumberOfPassengers();
                if(veh.getPedInSeat(-1) != 0) passengers++;

                let drawText = `~c~${vehName} (ID: ~w~${veh.remoteId}~c~ | HANDLE: ~w~${veh.handle}~c~) (~w~${passengers} Occupants~c~)\n~w~Speed: ~s~${(veh.getSpeed() * 3.6).toFixed(0)} ~w~/ ~s~${(mp.game.vehicle.getVehicleModelMaxSpeed(veh.model) * 3.6).toFixed(0)} ~w~KM/H`;

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






