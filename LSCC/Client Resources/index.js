var currentJailTime = 0;
var lastJailUpdate = 10;

var chaseRunning = false;
var chaseTimeCur = 0;
var chaseTimeMax = 390;
var chaseMode = 0; // 0= STANDARD | 1= BUSTED | and others as in SCRIPT
var collisionTime = -1;
var updateRate = 200;

var radioToggle = 0;

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
var antiAFKActive = false;

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

var ClothingEditorDOM = null;
var ClothingEditorActive = false;

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
require('./clothing_editor/index.js');
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
            mp.events.callRemote("ServerConsoleOutput", `Attempted to show login browser to ${mp.players.local.name} while it's null! Trying again...`);
            ServerUI = mp.browsers.new("package://ServerUI/index.html");
            ServerUI.active = true;
            setTimeout(() => {
                mp.events.callLocal("showLoginBrowser", state);
            }, 1000);
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

    mp.gui.cursor.show(toggle, false);
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
        || VehicleSpawnerActive == true || WeaponEditorActive == true || isHotwiring == true || ClothingEditorActive == true) return;

    mp.events.callLocal("actionMenu:close"); 
}
function ToggleMDC(){
    if(mp.players.local.getVariable("Team") != 1 || mp.players.local.getVariable("Wounded") > 0) return;
    if(menuToggled == true || chatStatus == true || ActionMenuActive == true || isHotwiring == true || ClothingEditorActive == true) return;

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
    if(MDC_DOM == null) return;
    MDCActive = false;
    MDC_DOM.active = false;
});

mp.events.add("login_StopMusic", () => {
    ServerUI.execute(`gm.$refs.auth.$refs.panel.stop();`);
});


// haha hex no export
const weaponData = [
    {
      "id": "weapon_dagger",
      "Name": "Antique Cavalry Dagger",
      "Type": "Melee",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Antique-cavalry-dagger-icon.png",
      "Hash": "0x92A27487"
    },
    {
      "id": "weapon_bat",
      "Name": "Baseball Bat",
      "Type": "Melee",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Baseball-bat-icon.png",
      "Hash": "0x958A4A8F"
    },
    {
      "id": "weapon_bottle",
      "Name": "Broken Bottle",
      "Type": "Melee",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Broken-bottle-icon.png",
      "Hash": "0xF9E6AA4B"
    },
    {
      "id": "weapon_crowbar",
      "Name": "Crowbar",
      "Type": "Melee",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Crowbar-icon.png",
      "Hash": "0x84BD7BFD"
    },
    {
      "id": "weapon_unarmed",
      "Name": "Fist",
      "Type": "Melee",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Fist-icon.png",
      "Hash": "0xA2719263"
    },
    {
      "id": "weapon_flashlight",
      "Name": "Flashlight",
      "Type": "Melee",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Flashlight-icon.png",
      "Hash": "0x8BB05FD7"
    },
    {
      "id": "weapon_golfclub",
      "Name": "Golf Club",
      "Type": "Melee",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Golf-club-icon.png",
      "Hash": "0x440E4788"
    },
    {
      "id": "weapon_hammer",
      "Name": "Hammer",
      "Type": "Melee",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Hammer-icon.png",
      "Hash": "0x4E875F73"
    },
    {
      "id": "weapon_hatchet",
      "Name": "Hatchet",
      "Type": "Melee",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Hatchet-icon.png",
      "Hash": "0xF9DCBF2D"
    },
    {
      "id": "weapon_knuckle",
      "Name": "Brass Knuckles",
      "Type": "Melee",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Knuckles-icon.png",
      "Hash": "0xD8DF3C3C"
    },
    {
      "id": "weapon_knife",
      "Name": "Knife",
      "Type": "Melee",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Knife-icon.png",
      "Hash": "0x99B507EA"
    },
    {
      "id": "weapon_machete",
      "Name": "Machete",
      "Type": "Melee",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Machete-icon.png",
      "Hash": "0xDD5DF8D9"
    },
    {
      "id": "weapon_switchblade",
      "Name": "Switchblade",
      "Type": "Melee",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Switch-blade-icon.png",
      "Hash": "0xDFE37640"
    },
    {
      "id": "weapon_nightstick",
      "Name": "Nightstick",
      "Type": "Melee",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Nightstick-icon.png",
      "Hash": "0x678B81B1"
    },
    {
      "id": "weapon_wrench",
      "Name": "Pipe Wrench",
      "Type": "Melee",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Pipe-wrench-icon.png",
      "Hash": "0x19044EE0"
    },
    {
      "id": "weapon_battleaxe",
      "Name": "Battle Axe",
      "Type": "Melee",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Battle-axe-icon.png",
      "Hash": "0xCD274149"
    },
    {
      "id": "weapon_poolcue",
      "Name": "Pool Cue",
      "Type": "Melee",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Pool-cue-icon.png",
      "Hash": "0x94117305"
    },
    {
      "id": "weapon_stone_hatchet",
      "Name": "Stone Hatchet",
      "Type": "Melee",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Stone-hatchet-icon.png",
      "Hash": "0x3813FC08"
    },
    {
      "id": "weapon_pistol",
      "Name": "Pistol",
      "Type": "Handguns",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Pistol-icon.png",
      "Hash": "0x1B06D571"
    },
    {
      "id": "weapon_pistol_mk2",
      "Name": "Pistol Mk II",
      "Type": "Handguns",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Pistol-mk2-icon.png",
      "Hash": "0xBFE256D4"
    },
    {
      "id": "weapon_combatpistol",
      "Name": "Combat Pistol",
      "Type": "Handguns",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Combat-pistol-icon.png",
      "Hash": "0x5EF9FEC4"
    },
    {
      "id": "weapon_appistol",
      "Name": "AP Pistol",
      "Type": "Handguns",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Appistol-icon.png",
      "Hash": "0x22D8FE39"
    },
    {
      "id": "weapon_stungun",
      "Name": "Stun Gun",
      "Type": "Handguns",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Stungun-icon.png",
      "Hash": "0x3656C8C1"
    },
    {
      "id": "weapon_pistol50",
      "Name": "Pistol .50",
      "Type": "Handguns",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Pistol.50-icon.png",
      "Hash": "0x99AEEB3B"
    },
    {
      "id": "weapon_snspistol",
      "Name": "SNS Pistol",
      "Type": "Handguns",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Sns-pistol-icon.png",
      "Hash": "0xBFD21232"
    },
    {
      "id": "weapon_snspistol_mk2",
      "Name": "SNS Pistol Mk II",
      "Type": "Handguns",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Sns-pistol-mk2-icon.png",
      "Hash": "0x88374054"
    },
    {
      "id": "weapon_heavypistol",
      "Name": "Heavy Pistol",
      "Type": "Handguns",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Heavy-pistol-icon.png",
      "Hash": "0xD205520E"
    },
    {
      "id": "weapon_vintagepistol",
      "Name": "Vintage Pistol",
      "Type": "Handguns",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Vintage-pistol-icon.png",
      "Hash": "0x83839C4"
    },
    {
      "id": "weapon_flaregun",
      "Name": "Flare Gun",
      "Type": "Handguns",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Flaregun-icon.png",
      "Hash": "0x47757124"
    },
    {
      "id": "weapon_marksmanpistol",
      "Name": "Marksman Pistol",
      "Type": "Handguns",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Marksman-pistol-icon.png",
      "Hash": "0xDC4DB296"
    },
    {
      "id": "weapon_revolver",
      "Name": "Heavy Revolver",
      "Type": "Handguns",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Heavy-revolver-icon.png",
      "Hash": "0xC1B3C3D1"
    },
    {
      "id": "weapon_revolver_mk2",
      "Name": "Heavy Revolver Mk II",
      "Type": "Handguns",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Heavy-revolver-mk2-icon.png",
      "Hash": "0xCB96392F"
    },
    {
      "id": "weapon_doubleaction",
      "Name": "Double Action Revolver",
      "Type": "Handguns",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Double-action-revolver-icon.png",
      "Hash": "0x97EA20B8"
    },
    {
      "id": "weapon_raypistol",
      "Name": "Up-n-Atomizer",
      "Type": "Handguns",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Up-n-atomizer-icon.png",
      "Hash": "0xAF3696A1"
    },
    {
      "id": "weapon_ceramicpistol",
      "Name": "Ceramic Pistol",
      "Type": "Handguns",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Ceramic-pistol-icon.png",
      "Hash": "0x2B5EF5EC"
    },
    {
      "id": "weapon_navyrevolver",
      "Name": "Navy Revolver",
      "Type": "Handguns",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Navy-revolver-icon.png",
      "Hash": "0x917F6C8C"
    },
    {
      "id": "weapon_gadgetpistol",
      "Name": "Perico Pistol",
      "Type": "Handguns",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Perico-pistol-icon.png",
      "Hash": "0x57A4368C"
    },
    {
      "id": "weapon_stungun_mp",
      "Name": "Stun Gun",
      "Type": "Handguns",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Stungun-icon.png",
      "Hash": "0x45CD9CF3"
    },
    {
      "id": "weapon_microsmg",
      "Name": "Micro SMG",
      "Type": "Submachine Guns",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Micro-smg-icon.png",
      "Hash": "0x13532244"
    },
    {
      "id": "weapon_smg",
      "Name": "SMG",
      "Type": "Submachine Guns",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Smg-icon.png",
      "Hash": "0x2BE6766B"
    },
    {
      "id": "weapon_smg_mk2",
      "Name": "SMG Mk II",
      "Type": "Submachine Guns",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Smg-mk2-icon.png",
      "Hash": "0x78A97CD0"
    },
    {
      "id": "weapon_assaultsmg",
      "Name": "Assault SMG",
      "Type": "Submachine Guns",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Assault-smg-icon.png",
      "Hash": "0xEFE7E2DF"
    },
    {
      "id": "weapon_combatpdw",
      "Name": "Combat PDW",
      "Type": "Submachine Guns",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Combat-pdw-icon.png",
      "Hash": "0x0A3D4D34"
    },
    {
      "id": "weapon_machinepistol",
      "Name": "Machine Pistol",
      "Type": "Submachine Guns",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Machine-pistol-icon.png",
      "Hash": "0xDB1AA450"
    },
    {
      "id": "weapon_minismg",
      "Name": "Mini SMG",
      "Type": "Submachine Guns",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Mini-smg-icon.png",
      "Hash": "0xBD248B55"
    },
    {
      "id": "weapon_raycarbine",
      "Name": "Unholy Hellbringer",
      "Type": "Submachine Guns",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Unholy-hellbringer-icon.png",
      "Hash": "0x476BF155"
    },
    {
      "id": "weapon_pumpshotgun",
      "Name": "Pump Shotgun",
      "Type": "Shotguns",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Pump-shotgun-icon.png",
      "Hash": "0x1D073A89"
    },
    {
      "id": "weapon_pumpshotgun_mk2",
      "Name": "Pump Shotgun Mk II",
      "Type": "Shotguns",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Pump-shotgun-mk2-icon.png",
      "Hash": "0x555AF99A"
    },
    {
      "id": "weapon_sawnoffshotgun",
      "Name": "Sawed-Off Shotgun",
      "Type": "Shotguns",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Sawed-off-shotgun-icon.png",
      "Hash": "0x7846A318"
    },
    {
      "id": "weapon_assaultshotgun",
      "Name": "Assault Shotgun",
      "Type": "Shotguns",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Assault-shotgun-icon.png",
      "Hash": "0xE284C527"
    },
    {
      "id": "weapon_bullpupshotgun",
      "Name": "Bullpup Shotgun",
      "Type": "Shotguns",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Bullpup-shotgun-icon.png",
      "Hash": "0x9D61E50F"
    },
    {
      "id": "weapon_musket",
      "Name": "Musket",
      "Type": "Shotguns",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Musket-icon.png",
      "Hash": "0xA89CB99E"
    },
    {
      "id": "weapon_heavyshotgun",
      "Name": "Heavy Shotgun",
      "Type": "Shotguns",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Heavy-shotgun-icon.png",
      "Hash": "0x3AABBBAA"
    },
    {
      "id": "weapon_dbshotgun",
      "Name": "Double Barrel Shotgun",
      "Type": "Shotguns",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Double-barrel-shotgun-icon.png",
      "Hash": "0xEF951FBB"
    },
    {
      "id": "weapon_autoshotgun",
      "Name": "Sweeper Shotgun",
      "Type": "Shotguns",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Sweeper-shotgun-icon.png",
      "Hash": "0x12E82D3D"
    },
    {
      "id": "weapon_combatshotgun",
      "Name": "Combat Shotgun",
      "Type": "Shotguns",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Combat-shotgun-icon.png",
      "Hash": "0x5A96BA4"
    },
    {
      "id": "weapon_assaultrifle",
      "Name": "Assault Rifle",
      "Type": "Assault Rifles",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Assault-rifle-icon.png",
      "Hash": "0xBFEFFF6D"
    },
    {
      "id": "weapon_assaultrifle_mk2",
      "Name": "Assault Rifle Mk II",
      "Type": "Assault Rifles",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Assault-rifle-mk2-icon.png",
      "Hash": "0x394F415C"
    },
    {
      "id": "weapon_carbinerifle",
      "Name": "Carbine Rifle",
      "Type": "Assault Rifles",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Carbine-rifle-icon.png",
      "Hash": "0x83BF0278"
    },
    {
      "id": "weapon_carbinerifle_mk2",
      "Name": "Carbine Rifle Mk II",
      "Type": "Assault Rifles",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Carbine-rifle-mk2-icon.png",
      "Hash": "0xFAD1F1C9"
    },
    {
      "id": "weapon_advancedrifle",
      "Name": "Advanced Rifle",
      "Type": "Assault Rifles",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Advanced-rifle-icon.png",
      "Hash": "0xAF113F99"
    },
    {
      "id": "weapon_specialcarbine",
      "Name": "Special Carbine",
      "Type": "Assault Rifles",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Special-carbine-icon.png",
      "Hash": "0xC0A3098D"
    },
    {
      "id": "weapon_specialcarbine_mk2",
      "Name": "Special Carbine Mk II",
      "Type": "Assault Rifles",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Special-carbine-mk2-icon.png",
      "Hash": "0x969C3D67"
    },
    {
      "id": "weapon_bullpuprifle",
      "Name": "Bullpup Rifle",
      "Type": "Assault Rifles",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Bullpup-rifle-icon.png",
      "Hash": "0x7F229F94"
    },
    {
      "id": "weapon_bullpuprifle_mk2",
      "Name": "Bullpup Rifle Mk II",
      "Type": "Assault Rifles",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Bullpup-rifle-mk2-icon.png",
      "Hash": "0x84D6FAFD"
    },
    {
      "id": "weapon_compactrifle",
      "Name": "Compact Rifle",
      "Type": "Assault Rifles",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Compact-rifle-icon.png",
      "Hash": "0x624FE830"
    },
    {
      "id": "weapon_militaryrifle",
      "Name": "Military Rifle",
      "Type": "Assault Rifles",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Military-rifle-icon.png",
      "Hash": "0x9D1F17E6"
    },
    {
      "id": "weapon_heavyrifle",
      "Name": "Heavy Rifle",
      "Type": "Assault Rifles",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Heavy-rifle-icon.png",
      "Hash": "0xC78D71B4"
    },
    {
      "id": "weapon_tacticalrifle",
      "Name": "Tactical Rifle",
      "Type": "Assault Rifles",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Tactical-rifle-icon.png",
      "Hash": "0xD1D5F52B"
    },
    {
      "id": "weapon_mg",
      "Name": "MG",
      "Type": "Machine Guns",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Mg-icon.png",
      "Hash": "0x9D07F764"
    },
    {
      "id": "weapon_combatmg",
      "Name": "Combat MG",
      "Type": "Machine Guns",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Combat-mg-icon.png",
      "Hash": "0x7FD62962"
    },
    {
      "id": "weapon_combatmg_mk2",
      "Name": "Combat MG Mk II",
      "Type": "Machine Guns",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Combat-mg-mk2-icon.png",
      "Hash": "0xDBBD7280"
    },
    {
      "id": "weapon_gusenberg",
      "Name": "Gusenberg Sweeper",
      "Type": "Machine Guns",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Gusenberg-sweeper-icon.png",
      "Hash": "0x61012683"
    },
    {
      "id": "weapon_sniperrifle",
      "Name": "Sniper Rifle",
      "Type": "Sniper Rifles",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Sniper-rifle-icon.png",
      "Hash": "0x05FC3C11"
    },
    {
      "id": "weapon_heavysniper",
      "Name": "Heavy Sniper",
      "Type": "Sniper Rifles",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Heavy-sniper-icon.png",
      "Hash": "0x0C472FE2"
    },
    {
      "id": "weapon_heavysniper_mk2",
      "Name": "Heavy Sniper Mk II",
      "Type": "Sniper Rifles",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Heavy-sniper-mk2-icon.png",
      "Hash": "0xA914799"
    },
    {
      "id": "weapon_marksmanrifle",
      "Name": "Marksman Rifle",
      "Type": "Sniper Rifles",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Marksman-rifle-icon.png",
      "Hash": "0xC734385A"
    },
    {
      "id": "weapon_marksmanrifle_mk2",
      "Name": "Marksman Rifle Mk II",
      "Type": "Sniper Rifles",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Marksman-rifle-mk2-icon.png",
      "Hash": "0x6A6C02E0"
    },
    {
      "id": "weapon_precisionrifle",
      "Name": "Precision Rifle",
      "Type": "Sniper Rifles",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Precision-rifle-icon.png",
      "Hash": "0x6E7DDDEC"
    },
    {
      "id": "weapon_rpg",
      "Name": "RPG",
      "Type": "Heavy Weapons",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Rocket-launcher-icon.png",
      "Hash": "0xB1CA77B1"
    },
    {
      "id": "weapon_grenadelauncher",
      "Name": "Grenade Launcher",
      "Type": "Heavy Weapons",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Grenade-launcher-icon.png",
      "Hash": "0xA284510B"
    },
    {
      "id": "weapon_grenadelauncher_smoke",
      "Name": "Grenade Launcher Smoke",
      "Type": "Heavy Weapons",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Grenade-launcher-icon.png",
      "Hash": "0x4DD2DC56"
    },
    {
      "id": "weapon_minigun",
      "Name": "Minigun",
      "Type": "Heavy Weapons",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Minigun-icon.png",
      "Hash": "0x42BF8A85"
    },
    {
      "id": "weapon_firework",
      "Name": "Firework Launcher",
      "Type": "Heavy Weapons",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Firework-launcher-icon.png",
      "Hash": "0x7F7497E5"
    },
    {
      "id": "weapon_railgun",
      "Name": "Railgun",
      "Type": "Heavy Weapons",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Railgun-icon.png",
      "Hash": "0x6D544C99"
    },
    {
      "id": "weapon_hominglauncher",
      "Name": "Homing Launcher",
      "Type": "Heavy Weapons",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Homing-launcher-icon.png",
      "Hash": "0x63AB0442"
    },
    {
      "id": "weapon_compactlauncher",
      "Name": "Compact Grenade Launcher",
      "Type": "Heavy Weapons",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Grenade-compact-launcher-icon.png",
      "Hash": "0x0781FE4A"
    },
    {
      "id": "weapon_rayminigun",
      "Name": "Widowmaker",
      "Type": "Heavy Weapons",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Widowmaker-icon.png",
      "Hash": "0xB62D1F67"
    },
    {
      "id": "weapon_emplauncher",
      "Name": "Compact EMP Launcher",
      "Type": "Heavy Weapons",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/EMP-launcher-icon.png",
      "Hash": "0xDB26713A"
    },
    {
      "id": "weapon_grenade",
      "Name": "Grenade",
      "Type": "Thrown Weapons",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Grenade-icon.png",
      "Hash": "0x93E220BD"
    },
    {
      "id": "weapon_bzgas",
      "Name": "BZ Gas",
      "Type": "Thrown Weapons",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Bz-gas-icon.png",
      "Hash": "0xA0973D5E"
    },
    {
      "id": "weapon_molotov",
      "Name": "Molotov Cocktail",
      "Type": "Thrown Weapons",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Molotov-icon.png",
      "Hash": "0x24B17070"
    },
    {
      "id": "weapon_stickybomb",
      "Name": "Sticky Bomb",
      "Type": "Thrown Weapons",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Sticky-bomb-icon.png",
      "Hash": "0x2C3731D9"
    },
    {
      "id": "weapon_proxmine",
      "Name": "Proximity Mines",
      "Type": "Thrown Weapons",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Proximity-mines-icon.png",
      "Hash": "0xAB564B93"
    },
    {
      "id": "weapon_snowball",
      "Name": "Snowballs",
      "Type": "Thrown Weapons",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Snowball-icon.png",
      "Hash": "0x787F0BB"
    },
    {
      "id": "weapon_pipebomb",
      "Name": "Pipe Bombs",
      "Type": "Thrown Weapons",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Pipe-bomb-icon.png",
      "Hash": "0xBA45E8B8"
    },
    {
      "id": "weapon_ball",
      "Name": "Baseball",
      "Type": "Thrown Weapons",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Ball-icon.png",
      "Hash": "0x23C9F95C"
    },
    {
      "id": "weapon_smokegrenade",
      "Name": "Tear Gas",
      "Type": "Thrown Weapons",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Tear-gas-icon.png",
      "Hash": "0xFDBC8A50"
    },
    {
      "id": "weapon_flare",
      "Name": "Flare",
      "Type": "Thrown Weapons",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Flare-icon.png",
      "Hash": "0x497FACC3"
    },
    {
      "id": "weapon_petrolcan",
      "Name": "Jerry Can",
      "Type": "Thrown Weapons",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Petrolcan-icon.png",
      "Hash": "0x34A67B97"
    },
    {
      "id": "gadget_parachute",
      "Name": "Parachute",
      "Type": "Thrown Weapons",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Parachute-icon.png",
      "Hash": "0xFBAB5776"
    },
    {
      "id": "weapon_fireextinguisher",
      "Name": "Fire Extinguisher",
      "Type": "Thrown Weapons",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Fire2.png",
      "Hash": "0x060EC506"
    },
    {
      "id": "weapon_hazardcan",
      "Name": "Hazardous Jerry Can",
      "Type": "Thrown Weapons",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Petrolcan-icon.png",
      "Hash": "0xBA536372"
    },
    {
      "id": "weapon_fertilizercan",
      "Name": "Fertilizer Can",
      "Type": "Thrown Weapons",
      "ImageSrc": "https://assets.gm.miami/gtav/weapons/Petrolcan-icon.png",
      "Hash": "0x184140A1"
    }
  ]