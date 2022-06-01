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
var lastChatToggle = Date.now();
var streetName = null;
var zoneName = null;
var showHud = true;
var renderNametags = true;
var currentMuteTime = 0;
var lastMuteUpdate = 10;
var lastFunc = "Init";

var BankRobberyOn = false;

var ShowVehicleDL = false;
var specCam = null;
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

var cuffTarget = null;
var cuffInterval = null;

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
require("./charcreator/clotheditor.js")
require("./charcreator/index.js");
require("./charcreator/outfitslots.js")
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
function toggleLoginScene(toggle){
    if(toggle){
        loginSceneEntities[0] = mp.vehicles.new(mp.game.joaat("police"), new mp.Vector3(240.73251, -1794.3538, 27.502863),
            {
                heading: 97.76415,
                numberPlate: 'RNC123',
                dimension: mp.players.local.dimension
            });
        loginSceneEntities[0].setDoorOpen(1, false, true);
        loginSceneEntities[0].setDoorOpen(0, false, true);
        loginSceneEntities[1] = mp.vehicles.new(mp.game.joaat("cavalcade2"), new mp.Vector3(236.4917, -1784.2609, 28.601725),
        {
            heading: 5.697903,
            numberPlate: 'RNC123',
            dimension: mp.players.local.dimension
        });
        loginSceneEntities[1].setDoorOpen(5, false, true);

        loginSceneEntities[2] = mp.peds.new(mp.game.joaat('s_m_y_cop_01'), 
        new mp.Vector3(236.14717, -1795.1011, 28.029375), -82.156, mp.players.local.dimension);
        loginSceneEntities[3] = mp.peds.new(mp.game.joaat('s_f_y_cop_01'), 
        new mp.Vector3(236.59952, -1787.1562, 28.597916), -4.26895, mp.players.local.dimension);
        loginSceneEntities[4] = mp.peds.new(mp.game.joaat('ig_ramp_gang'), 
        new mp.Vector3(237.48822, -1794.7585, 27.987871), -85.38175, mp.players.local.dimension);

    }
    else{
        for(let i = 0 ; i < loginSceneEntities.length ; i++){
            loginSceneEntities[i].destroy();
        }
    }
}

mp.events.add('showLoginBrowser', (state) => {
    setTimeout(() => {
        if(ServerUI != null)
        {
            if(!ServerUI.active) ServerUI.active = true;
            if(state){
                ServerUI.execute(`gm.$refs.auth.startLoading(5);`); // initiate the fake loading screen to allow CEF sounds and shit duhh
                toggleLoginScene(true);
                mp.events.callRemote("requestQuickLoginForgotUsernameData"); // yes
            }
            else{
                ServerUI.execute(`gm.$refs.auth.$refs.panel.authSuccess = true;`);
                setTimeout(() => {
                    if(!mp.players.local || !mp.players.exists(mp.players.local)) return;

                    ServerUI.execute(`gm.$refs.auth.authPanel = false;`);
                    toggleLoginScene(false);
                }, 1000);
            }
            mp.gui.cursor.show(state, state);
        }
        else{
            mp.events.callRemote("ServerConsoleOutput", `Attempted to show login browser when ServerUI is null!`);
        }

        /*
        loginBrowser.active = state;
    
        if(!loginBrowser.active && mp.players.local.getVariable("pLogged") == true && loginBrowser != null)
        {
            loginBrowser.destroy();
            loginBrowser = null;
        }
        */
       // [SERVERUI.TODO] FUNCTIONS TO TOGGLE THE LOGIN SCREEN ON/OFF WILL BE HERE
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

let chatStatusTimeout = null;
mp.events.add("onOpenChatbox", (toggle) =>
{
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
    mp.events.callRemote("OnAutoLoginAttempt", username);
});

mp.events.add('requestForgotUsernameServer', () => {
    mp.events.callRemote("OnRequestForgotUsername");
});
