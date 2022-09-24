var currentJailTime = 0;
var lastJailUpdate = 10;

var chaseRunning = false;
var chaseTimeCur = 0;
var chaseTimeMax = 390;
var chaseMode = 0; // 0= STANDARD | 1= BUSTED | and others as in SCRIPT
var collisionTime = -1;
var updateRate = 200;

var chatStatus = false;
var chatStatusLate = false;
var menuToggledLate = false;
var lastChatToggle = Date.now();
var streetName = null;
var zoneName = null;
var showHud = true;
var renderNametags = true;
var currentMuteTime = 0;
var lastMuteUpdate = 10;
var lastFunc = "Init";

var curMDCBind = 0x4B;
var lastMDCBind = 0x4B;
var curActionMenuBind = 0x59;
var lastActionMenuBind = 0x59;

var BankRobberyOn = false;

var ShowVehicleDL = false;
var specState = false;
var specTarget = null;
var isNoClip = false;

var LastKeyPress = 0;
var AFKStage = 0;
var SecondsSinceLastKeyPress = 0; // using this for nametags

var showReticule = true;
var showCustomCrosshair = false;
var showCustomCrosshairInSniper = false;
var alwaysShowCustomCrosshair = false;
var customCrosshairOn = false;

var ServerUI = null;
var menuToggled = false;
var scoreboardToggled = false;
var scoreboardCursor = false;

var MDC_DOM = null;
var MDCActive = false;
var isLockpicking = false;

var isHotwiring = false;
var HotwireDOM = false;

var ActionMenuActive = false;
var VehicleSpawnerActive = false;
var WeaponEditorActive = false;

var cuffTarget = null;
var cuffInterval = null;

var IsInTutorial = false;

var disableSprint = false;

var FPS = 0; // This is the actual fps.
var FPS_Frames = 0;
var FPS_Calc = Date.now();

mp.players.local.Team = -1;
mp.game.vehicle.defaultEngineBehaviour = false;
mp.game.gameplay.enableMpDlcMaps(true);
mp.game.player.disableVehicleRewards();
mp.game.player.setHealthRechargeMultiplier(0.0);
mp.game.vehicle.setExperimentalAttachmentSyncEnabled(true);
mp.game.gameplay.setFadeOutAfterDeath(false);
mp.players.local.setConfigFlag(429, false);
mp.players.local.setRagdollFlag(2);

function traceLastFunc(iStr)
{
    mp.storage.data.lastFn = iStr;
    mp.storage.flush();
}

traceLastFunc("Main::LoadIPLs()");
// where IPL names name of your .ymap file in dlc -> mapping.rpf
if(!mp.game.streaming.isIplActive("blockheisttunnel") || !mp.game.streaming.isIplActive("derby") || 
    !mp.game.streaming.isIplActive("floydsapt") || !mp.game.streaming.isIplActive("whbig") || 
    !mp.game.streaming.isIplActive("whmed") || !mp.game.streaming.isIplActive("whsmall"))
{
	mp.game.invoke("0xD7C10C4A637992C9"); // _LOAD_SP_DLC_MAPS
	mp.game.invoke("0x0888C3502DBBEEF5"); // _LOAD_MP_DLC_MAPS
}

mp.players.local.position = new mp.Vector3(-1018.55300000, -2731.95600000, 15.70000000);

traceLastFunc("RequireInit");
require('mp-commands');
require('./ServerUI/CommandsList.js');
require("./nativeui/index.js");

require('./Player/Player.js');
require('./Player/SpikesRoadblocks.js');
require('./Player/Achievements.js');
require('./Player/BankRobbery.js');
require('./Player/Actions.js');
require("./Player/Nametags.js");
require("./ServerUI/HUD.js");
require("./ServerUI/Menu.js");
require("./ServerUI/Logs.js");
require("./ServerUI/Tutorial.js");
require("./charcreator/clotheditor.js")
require("./charcreator/index.js");
require("./charcreator/outfitslots.js");
require('./mdc_screen/index.js');
require('./hotwire_game/index.js');
require('./weaponeditor/index.js');
require('./tips_test/index.js');
require('./Player/Fuel.js');

require('./camerasManager.js');
require('./cefBrowser.js');
require("./rappel/index.js");
require("./Player/Sync.js");
require("./Player/LightsMenu.js");
require('./Player/OtherStuff.js');
require("./Player/Busted.js");
require("./Player/Voice.js");
require("./Player/VehicleTuner.js");

traceLastFunc("Main::Init()");

var loginSceneEntities = [];
async function toggleLoginScene(toggle){
    if(toggle){
  
        /* CREATING THE VEHICLES */
        loginSceneEntities[0] = mp.vehicles.new(mp.game.joaat("cavalcade"), new mp.Vector3(-694.26056, -1044.3917, 15.728991),
            {
                heading: -4.97181,
                numberPlate: 'RNC123',
                color: [[255, 0, 0], [255, 255, 255]],
                alpha: 255,
                engine: false,
                dimension: mp.players.local.dimension
            });
        loginSceneEntities[1] = mp.vehicles.new(mp.game.joaat("police2"), new mp.Vector3(-688.26843, -1044.1389, 15.873694),
        {
            heading: 103.21785,
            numberPlate: 'RNC123',
            color: [[0,0,0], [255, 255, 255]],
            alpha: 255,
            engine: false,
            dimension: mp.players.local.dimension
        });
        loginSceneEntities[2] = mp.vehicles.new(mp.game.joaat("police"), new mp.Vector3(-696.2687, -1050.3245, 15.12663),
        {
            heading: -29.521137,
            numberPlate: 'RNC123',
            color: [[0,0,0], [255, 255, 255]],
            alpha: 255,
            engine: false,
            dimension: mp.players.local.dimension
        });
        
        /* GENERAL STUFF - TIME OF DAY, ANIM DICTS... */

        mp.game.audio.startAudioScene("CHARACTER_CHANGE_IN_SKY_SCENE");
        mp.game.time.setClockTime(21, 0, 0);
        mp.game.gameplay.setWeatherTypeNow('CLEAR');

        mp.game.streaming.requestAnimDict("arrest");
        for(let i = 0; mp.game.streaming.hasAnimDictLoaded("arrest") === 0 && i < 15; i++){
            await mp.game.waitAsync(100);
        }
        mp.game.streaming.requestAnimDict("amb@medic@standing@kneel@base");
        for(let i = 0; mp.game.streaming.hasAnimDictLoaded("amb@medic@standing@kneel@base") === 0 && i < 15; i++){
            await mp.game.waitAsync(100);
        }
        mp.game.streaming.requestAnimDict("random@arrests@busted");
        for(let i = 0; mp.game.streaming.hasAnimDictLoaded("random@arrests@busted") === 0 && i < 15; i++){
            await mp.game.waitAsync(100);
        }
        mp.game.streaming.requestAnimDict("combat@damage@writhe");
        for(let i = 0; mp.game.streaming.hasAnimDictLoaded("combat@damage@writhe") === 0 && i < 15; i++){
            await mp.game.waitAsync(100);
        }

        /* CREATING THE PEDS */
        setTimeout(() => {
            loginSceneEntities[3] = mp.peds.new( // gangster 1 - dead
                mp.game.joaat('csb_grove_str_dlr'), 
                new mp.Vector3(-697.9484, -1043.4353, 15.86826),
                -47.113903,
                mp.players.local.dimension
            );
            loginSceneEntities[4] = mp.peds.new( // gangster 2 - surrendering
                mp.game.joaat('a_m_m_salton_02'), 
                new mp.Vector3(-693.20276, -1047.7367, 15.804193),
                34.280487,
                mp.players.local.dimension
            );
            loginSceneEntities[5] = mp.peds.new( // cop speaking into radio, aiming at gangster 2
                mp.game.joaat('s_m_y_cop_01'), 
                new mp.Vector3(-692.4914, -1048.5272, 15.750268),
                35.93633,
                mp.players.local.dimension
            );
            loginSceneEntities[6] = mp.peds.new( // cop 2 - kneeling next to ded guy
                mp.game.joaat('s_f_y_cop_01'), 
                new mp.Vector3(-696.91583, -1044.7444, 15.767713),
                67.523476,
                mp.players.local.dimension
            );
            loginSceneEntities[7] = mp.peds.new( // cop 3 - idling next to them
                mp.game.joaat('s_m_y_cop_01'), 
                new mp.Vector3(-689.9278, -1046.8384, 15.976717),
                104.24435,
                mp.players.local.dimension
            );

        }, 550);
    }
    else{
        mp.game.audio.stopAudioScene("CHARACTER_CHANGE_IN_SKY_SCENE");
        for(let i = 0 ; i < loginSceneEntities.length ; i++){
            loginSceneEntities[i].destroy();
        }
        loginSceneEntities = [];
    }
}
mp.events.add("DestroyLoginScene", () => toggleLoginScene(false));

mp.events.add('showLoginBrowser', (state) => {


    setTimeout(() => {
        if(ServerUI != null)
        {
            if(!ServerUI.active) ServerUI.active = true;
            ServerUI.execute(`gm.$refs.hud.enabled = false;`);
            ServerUI.execute(`gm.$refs.mainMenu.enabled = false;`);
            ServerUI.execute(`gm.$refs.mainMenu.enabled = false;`);
            if(state){
                ServerUI.execute(`gm.$refs.auth.startLoading(5);`); // initiate the fake loading screen to allow CEF sounds and shit duhh
                toggleLoginScene(true);
                mp.events.callRemote("requestQuickLoginForgotUsernameData"); // yes
                ServerUI.execute(`gm.$refs.mainMenu.$refs.inventoryTab.items = [];`);
                mp.events.callLocal("actionMenu:removeAllItems");
            }
            else{
                ServerUI.execute(`gm.$refs.auth.$refs.panel.authSuccess = true;`);
                setTimeout(() => {
                    if(!mp.players.local || !mp.players.exists(mp.players.local)) return;

                    ServerUI.execute(`gm.$refs.auth.authPanel = false;`);
                    if(!IsInTutorial)
                        ServerUI.execute(`gm.$refs.hud.enabled = true;`);
                }, 1000);
            }
            if(!IsInTutorial)
                mp.gui.cursor.show(state, state);
        }
        else{
            mp.events.callRemote("ServerConsoleOutput", `Attempted to show login browser when ServerUI is null!`);
        }

    }, 500);
});

mp.events.add("login_ShowInvalidInfo", (toggle) => {
    if(ServerUI && ServerUI.active){
        ServerUI.execute(`gm.$refs.auth.$refs.panel.authLoading = false;`);
        ServerUI.execute(`gm.$refs.auth.$refs.panel.$refs.login.loginError = ${toggle.toString()}`);
    }
});
mp.events.add("login_ShowAccountExists", (toggle) => {
    if(ServerUI && ServerUI.active){
        ServerUI.execute(`gm.$refs.auth.$refs.panel.authLoading = false;`);
        ServerUI.execute(`gm.$refs.auth.$refs.panel.$refs.register.usernameAlreadyExists = ${toggle.toString()}`);
    }
});
mp.events.add("login_MaxAccountLimit", (toggle) => {
    if(ServerUI && ServerUI.active){
        ServerUI.execute(`gm.$refs.auth.$refs.panel.authLoading = false;`);
        //ServerUI.execute(`gm.$refs.auth.$refs.panel.$refs.register.usernameAlreadyExists = ${toggle.toString()}`);
        // TO-DO: Add a message that max account limit has been reached!
    }
});

mp.events.add("browser_ServerConsole", (msg) => mp.events.callRemote("ServerConsoleOutput", msg));

let chatStatusTimeout = null;
mp.events.add("onOpenChatbox", (toggle) =>
{
    if((VehicleSpawnerActive !== undefined && VehicleSpawnerActive == true) ||
    (WeaponEditorActive !== undefined && WeaponEditorActive == true) ||
    (ActionMenuActive !== undefined && ActionMenuActive == true)){
        return;
    }
    if(mp.players.local.getVariable("pLogged") == undefined || mp.players.local.getVariable("pLogged") == false && toggle)
    {
        activateChat(false);
        return;
    }
    clearTimeout(chatStatusTimeout);
    chatStatus = toggle;
    lastChatToggle = Date.now();
    focusChat(toggle);
    mp.events.callRemote("UpdateChatState", toggle);
    traceLastFunc(`[Players] ChatBox Toggled ${toggle}`);

    if(toggle == true){
        chatStatusLate = true;
    }
    else{
        chatStatusTimeout = setTimeout(() =>{
            chatStatusLate = false;
        }, 500);
    }

});

mp.events.add('loginInformationToServer', (username, password) => {

    mp.events.callRemote('OnPlayerLoginAttempt', username, password);
});

mp.events.add('registerInformationToServer', (email, username, password) => {

    mp.events.callRemote('OnPlayerRegisterAttempt', email, username, password);
});

mp.events.add('requestAutoLoginData', () => {
    mp.events.callRemote("OnRequestAutoLoginData");
});

mp.events.add('sendAutoLoginDataToBrowser', (data) => {
    setTimeout(() => {
        if(data.length > 2)
            ServerUI.execute(`gm.$refs.auth.$refs.panel.$refs.login.quickLoginUsername = "${data}"`);
    }, 500);
});
mp.events.add('sendForgotUsernameToBrowser', (data) => {
    setTimeout(() => {
        ServerUI.execute(`gm.$refs.auth.$refs.panel.$refs.login.foundUsernames = ${data}`);
    }, 500);

});

mp.events.add('autologinToServer', (username) => {
    ServerUI.execute(`gm.$refs.auth.$refs.panel.authLoading = true;`);
    mp.events.callRemote("OnAutoLoginAttempt", username);
});

mp.events.add('requestForgotUsernameServer', () => {
    mp.events.callRemote("OnRequestForgotUsername");
});


// When 'E' key is held down, open the menu
function actionMenuOpen(){
    mp.events.callLocal("actionMenu:open"); 
}
// When 'E' key is let go, close the menu - toggleable though.
function actionMenuClose(){
    if(chatStatus == true || menuToggled == true || MDCActive == true || scoreboardToggled == true
        || VehicleSpawnerActive == true || WeaponEditorActive == true || isHotwiring == true) return;

    mp.events.callLocal("actionMenu:close"); 
}
function ToggleMDC(){
    if(mp.players.local.getVariable("Team") != 1 || mp.players.local.getVariable("Wounded") > 0) return;
    if(menuToggled == true || chatStatus == true || ActionMenuActive == true || isHotwiring == true) return;

    if(scoreboardToggled)
    {
        ToggleScoreboard();
    }
    MDCActive = !MDCActive;
    MDC_DOM.active = MDCActive;

    if(MDCActive){
        mp.events.callLocal("MDC:UpdateMiniscreen", "name", mp.players.local.name);
    }
    
}
mp.events.add("hideMDC", () => {
    MDCActive = false;
    MDC_DOM.active = false;
});