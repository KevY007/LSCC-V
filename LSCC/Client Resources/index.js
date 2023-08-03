var global = require('./globals');
const {TranslateMessage: Translate} = require('./Translator');
mp.players.local.Team = -1;
mp.game.vehicle.defaultEngineBehaviour = false;
mp.game.gameplay.enableMpDlcMaps(true);
mp.game.player.disableVehicleRewards();
mp.game.player.setHealthRechargeMultiplier(0.0);
mp.game.vehicle.setExperimentalAttachmentSyncEnabled(true);
mp.game.gameplay.setFadeOutAfterDeath(false);
mp.players.local.setConfigFlag(429, false);
mp.players.local.setRagdollFlag(2);



global.traceLastFunc("Main::LoadIPLs()");
// where IPL names name of your .ymap file in dlc -> mapping.rpf
if(!mp.game.streaming.isIplActive("blockheisttunnel") || !mp.game.streaming.isIplActive("derby") || 
    !mp.game.streaming.isIplActive("floydsapt") || !mp.game.streaming.isIplActive("whbig") || 
    !mp.game.streaming.isIplActive("whmed") || !mp.game.streaming.isIplActive("whsmall"))
{
	mp.game.invoke("0xD7C10C4A637992C9"); // _LOAD_SP_DLC_MAPS
	mp.game.invoke("0x0888C3502DBBEEF5"); // _LOAD_MP_DLC_MAPS
}

mp.players.local.position = new mp.Vector3(-1018.55300000, -2731.95600000, 15.70000000);

global.traceLastFunc("RequireInit");
require('mp-commands');
require('./ServerUI/CommandsList.js');
require("./nativeui/index.js");

require('./HeliCam/index.js');
require('./Player/Player.js');
require('./Player/SpikesRoadblocks.js');
require('./Player/Achievements.js');
require('./Player/BankRobbery.js');
require('./Player/Actions.js');
require("./Player/Nametags.js");
require("./Player/SyncScripts/Aim");
require("./Player/SyncScripts/Flashlight");
require("./Player/SyncScripts/Laser");
require('./Player/DamageIndicator/damage_indicator');
require('./Player/DamageIndicator/anti_peek');
require("./ServerUI/HUD.js");
require("./ServerUI/Menu.js");
require("./ServerUI/Logs.js");
require("./ServerUI/Tutorial.js");
require("./ServerUI/XMR.js");
require("./WeaponFiringMode/index.js");
require("./charcreator/index.js");
require("./charcreator/outfitslots.js");
require("./charcreator/clotheditor.js");
require("./charcreator/tattoo/data.js");
require("./charcreator/tattoo/menu.js");
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


var ReactDOM = null;
mp.events.add("testNativeEventSend", (text) => {
  mp.gui.chat.push(text);
});
mp.events.add("callReactEventNoArgs", () => {
  mp.events.callLocal("testNativeEventReceive");
});
mp.events.add("callReactEvent", () => {
  mp.events.callLocal("testNativeEventReceive", "yes");
});

mp.events.add("react_delete", () => {
  if(ReactDOM) ReactDOM.destroy();

  ReactDOM = null;
})
mp.events.add("react_new", () => {
  ReactDOM = mp.browsers.new("package://react_test/index.html");
});

global.traceLastFunc("Main::Init()");

async function toggleLoginScene(toggle){
    if(toggle){

        mp.game.streaming.requestAnimDict("amb@prop_human_seat_bar@male@elbows_on_bar@idle_a");
        mp.game.streaming.requestAnimDict("amb@prop_human_seat_chair@female@arms_folded@idle_a");
        mp.game.streaming.requestAnimDict("amb@prop_human_seat_chair@male@elbows_on_knees@idle_b");
        mp.game.streaming.requestAnimDict("anim@amb@casino@brawl@fights@argue@");
        mp.game.streaming.requestAnimDict("anim@heists@ornate_bank@chat_manager");
  
        /* CREATING THE PEDS */
        setTimeout(() => {
          global.loginSceneEntities[0] = mp.peds.new(
            mp.game.joaat('a_m_y_gay_01'),
            new mp.Vector3(2055.6335, 2963.446, -67.70754),
            -125.94715,
            mp.players.local.dimension
          );

          global.loginSceneEntities[1] = mp.peds.new(
            mp.game.joaat('ig_kaylee'),
            new mp.Vector3(2057.192, 2961.257, -67.811966),
            52.981815,
            mp.players.local.dimension
          );

          global.loginSceneEntities[2] = mp.peds.new(
            mp.game.joaat('a_m_y_vindouche_01'),
            new mp.Vector3(2058.2307, 2962.797, -67.82209),
            67.54069,
            mp.players.local.dimension
          );

          global.loginSceneEntities[3] = mp.peds.new(
            mp.game.joaat('s_m_y_dealer_01'),
            new mp.Vector3(2056.2158, 2966.3682, -67.30179),
            144.91861,
            mp.players.local.dimension
          );

          global.loginSceneEntities[4] = mp.peds.new(
            mp.game.joaat('g_m_m_chicold_01'),
            new mp.Vector3(2052.9705, 2961.859, -67.30187),
            -36.38813,
            mp.players.local.dimension
          );

          global.loginSceneEntities[5] = mp.peds.new(
            mp.game.joaat('a_m_m_farmer_01'),
            new mp.Vector3(2057.881, 2961.7786, -67.30188),
            52.57501,
            mp.players.local.dimension
          );

          // Room 2
          global.loginSceneEntities[6] = mp.peds.new(
            mp.game.joaat('csb_prolsec'),
            new mp.Vector3(2063.319, 2966.2705, -67.30189),
            48.209084,
            mp.players.local.dimension
          );

          global.loginSceneEntities[7] = mp.peds.new(
            mp.game.joaat('s_m_y_swat_01'),
            new mp.Vector3(2059.623, 2971.459, -67.30181),
            -160.187,
            mp.players.local.dimension
          );

          global.loginSceneEntities[8] = mp.peds.new(
            mp.game.joaat('s_m_y_swat_01'),
            new mp.Vector3(2058.6614, 2970.3154, -67.30181),
            -129.64273,
            mp.players.local.dimension
          );

          global.loginSceneEntities[9] = mp.peds.new(
            mp.game.joaat('s_m_y_swat_01'),
            new mp.Vector3(2057.7458, 2969.198, -67.30181),
            -116.9288,
            mp.players.local.dimension
          );

          global.loginSceneEntities[10] = mp.peds.new(
            mp.game.joaat('s_m_y_swat_01'),
            new mp.Vector3(2057.7458, 2967.9717, -67.30181),
            -93.87298,
            mp.players.local.dimension
          );

        }, 100);

        /* GENERAL STUFF - TIME OF DAY, ANIM DICTS... */
        mp.game.audio.startAudioScene("CHARACTER_CHANGE_IN_SKY_SCENE");
        mp.game.time.setClockTime(21, 0, 0);
        mp.game.gameplay.setWeatherTypeNow('CLEAR');
    }
    else{
        mp.game.audio.stopAudioScene("CHARACTER_CHANGE_IN_SKY_SCENE");
        for(let i = 0 ; i < global.loginSceneEntities.length ; i++){
            global.loginSceneEntities[i].destroy();
        }
        global.loginSceneEntities = [];
    }
}
mp.events.add('entityStreamIn', (entity) => {
  if(!global.loginSceneEntities.includes(entity)) return;

  entity.freezePosition(true);
  entity.setCollision(false, false);

  const idx = global.loginSceneEntities.indexOf(entity);
  switch(idx){
    case 0:
      entity.taskPlayAnim('amb@prop_human_seat_bar@male@elbows_on_bar@idle_a', 'idle_a', 8.0, 1.0, -1, 1, 1.0, false, false, false);
      break;
    case 1:
      entity.taskPlayAnim('amb@prop_human_seat_chair@female@arms_folded@idle_a', 'idle_c', 8.0, 1.0, -1, 1, 1.0, false, false, false);
      break;
    case 2:
      entity.taskPlayAnim('amb@prop_human_seat_chair@male@elbows_on_knees@idle_b', 'idle_e', 8.0, 1.0, -1, 1, 1.0, false, false, false);
      break;
    case 3:
      mp.game.invoke('0xBF0FD6E56C964FCB', entity.handle, 2144741730>>0, 100, false, true);
      break;
    case 4:
      mp.game.invoke('0xBF0FD6E56C964FCB', entity.handle, 2144741730>>0, 100, false, true);
      break;
    case 5:
      mp.game.invoke('0xBF0FD6E56C964FCB', entity.handle, 584646201>>0, 100, false, true);
      entity.taskPlayAnim('anim@amb@casino@brawl@fights@argue@', 'arguement_loop_mp_m_brawler_01', 8.0, 1.0, -1, 1, 1.0, false, false, false);
      break;
    case 6:
      entity.taskPlayAnim('anim@heists@ornate_bank@chat_manager', 'on_foot', 8.0, 1.0, -1, 1, 1.0, false, false, false);
      break;
    case 7:
      mp.game.invoke('0xBF0FD6E56C964FCB', entity.handle, 736523883>>0, 100, false, true);
      break;
    case 8:
      mp.game.invoke('0xBF0FD6E56C964FCB', entity.handle, 984333226>>0, 100, false, true);
      break;
    case 9:
      mp.game.invoke('0xBF0FD6E56C964FCB', entity.handle, 736523883>>0, 100, false, true);
      break;
    case 10:
      mp.game.invoke('0xBF0FD6E56C964FCB', entity.handle, 984333226>>0, 100, false, true);
      break;
  }
});

mp.events.add("DestroyLoginScene", () => toggleLoginScene(false));

mp.events.add('showLoginBrowser', (state) => {
    setTimeout(() => {
        if(global.ServerUI != null)
        {      
            if(global.ServerUI.active) global.ServerUI.active = true;
            if(global.ServerUI) global.ServerUI.call('hud_toggle', false);
            if(global.ServerUI) global.ServerUI.call(`mainmenu_toggle`, false); 
            if(state){
                if(global.ServerUI) global.ServerUI.call(`auth_toggle`, true);
                toggleLoginScene(true);
                mp.events.callRemote("requestQuickLoginForgotUsernameData"); // yes
                //if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.inventoryTab.items = [];`);
                mp.events.callLocal("actionMenu:removeAllItems");
            }
            else{
                //if(global.ServerUI) global.ServerUI.execute(`gm.$refs.auth.api.authSuccess = true;`);
                //if(global.ServerUI) global.ServerUI.execute(`gm.$refs.auth.stop();`);
                setTimeout(() => {
                    if(!mp.players.local || !mp.players.exists(mp.players.local)) return;

                    if(global.ServerUI) global.ServerUI.call(`auth_toggle`, false);
                    if(!global.IsInTutorial){
                        if(global.ServerUI) global.ServerUI.call('hud_toggle', true);
                        if(global.ServerUI) global.ServerUI.call('hud_minilist', true);
                        if(global.ServerUI) global.ServerUI.call("hud_minilist_toggle", true);
                    }
                }, 1000);
            }
            if(!global.IsInTutorial)
                mp.gui.cursor.show(state, state);
        }
        else{
            mp.events.callRemote("ServerConsoleOutput", `Attempted to show login browser to ${mp.players.local.name} while it's null! Trying again...`);
            global.ServerUI = global.InitializeServerUI();
            if(global.ServerUI) global.ServerUI.active = true;
            setTimeout(() => {
                mp.events.callLocal("showLoginBrowser", state);
            }, 1000);
        }

    }, 500);
});

mp.events.add("login_ShowInvalidInfo", (toggle) => {
    if(global.ServerUI && global.ServerUI.active){
        if(global.ServerUI) global.ServerUI.execute(`gm.$refs.auth.api.isLoading = false;`);
        mp.events.callLocal("HUDError", "The login credentials are invalid!");
        mp.gui.cursor.show(true, true);
        //if(global.ServerUI) global.ServerUI.execute(`gm.$refs.auth.$refs.panel.$refs.login.loginError = ${toggle.toString()}`);
    }
});
mp.events.add("login_ShowAccountExists", (toggle) => {
    if(global.ServerUI && global.ServerUI.active){
        if(global.ServerUI) global.ServerUI.execute(`gm.$refs.auth.api.isLoading = false;`);
        mp.events.callLocal("HUDError", "That username is already taken!");
        mp.gui.cursor.show(true, true);
        //if(global.ServerUI) global.ServerUI.execute(`gm.$refs.auth.$refs.panel.$refs.register.usernameAlreadyExists = ${toggle.toString()}`);
    }
});
mp.events.add("login_MaxAccountLimit", (toggle) => {
    if(global.ServerUI && global.ServerUI.active){
        if(global.ServerUI) global.ServerUI.execute(`gm.$refs.auth.api.isLoading = false;`);
        mp.events.callLocal("HUDError", "You've reached the maximum accounts limit! (2 accounts)");
        mp.gui.cursor.show(true, true);
        //if(global.ServerUI) global.ServerUI.execute(`gm.$refs.auth.$refs.panel.$refs.register.usernameAlreadyExists = ${toggle.toString()}`);
        // TO-DO: Add a message that max account limit has been reached!
    }
});
mp.events.add("login_ForceStopLoading", () => {
  if(global.ServerUI && global.ServerUI.active){
    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.auth.api.isLoading = false;`);
    mp.gui.cursor.show(true, true);
  }
});

mp.events.add("browser_ServerConsole", (msg) => mp.events.callRemote("ServerConsoleOutput", msg));

mp.events.add("onOpenChatbox", (toggle) =>
{
    if((global.VehicleSpawnerActive !== undefined && global.VehicleSpawnerActive == true) ||
    (global.WeaponEditorActive !== undefined && global.WeaponEditorActive == true) ||
    (global.ActionMenuActive !== undefined && global.ActionMenuActive == true)){
        return;
    }
    if(mp.players.local.getVariable("pLogged") == undefined || mp.players.local.getVariable("pLogged") == false && toggle)
    {
        global.activateChat(false);
        return;
    }
    clearTimeout(global.chatStatusTimeout);
    global.chatStatus = toggle;
    global.lastChatToggle = Date.now();
    global.focusChat(toggle);
    mp.events.callRemote("UpdateChatState", toggle);
    global.traceLastFunc(`[Players] ChatBox Toggled ${toggle}`);

    mp.gui.cursor.show(toggle, false);
    if(toggle == true){
        global.chatStatusLate = true;
    }
    else{
        global.chatStatusTimeout = setTimeout(() =>{
            global.chatStatusLate = false;
        }, 500);
    }

});

mp.events.add('auth_login', (username, password) => {

    mp.events.callRemote('OnPlayerLoginAttempt', username, password);
});

mp.events.add('auth_register', (username, email, password, confirmPass) => {

  if(password === confirmPass)
    mp.events.callRemote('OnPlayerRegisterAttempt', email, username, password);
  else{
    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.auth.api.isLoading = false;`);
    mp.events.callLocal("HUDError", "The passwords do not match!");
  }

});

mp.events.add('requestAutoLoginData', () => {
    mp.events.callRemote("OnRequestAutoLoginData");
});

mp.events.add('sendAutoLoginDataToBrowser', (data) => {
    setTimeout(() => {
        if(data.length > 2)
            if(global.ServerUI) global.ServerUI.call(`auth_quick_login_set`, data);
    }, 500);
});
mp.events.add('sendForgotUsernameToBrowser', (data) => {
    setTimeout(() => {
        //if(global.ServerUI) global.ServerUI.execute(`gm.$refs.auth.api.login.possibleUsernames = ${data}`);
    }, 500);

});

mp.events.add('auth_quickLogin', (username) => {
    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.auth.api.isLoading = true;`);
    mp.events.callRemote("OnAutoLoginAttempt", username);
});

mp.events.add('requestForgotUsernameServer', () => {
    mp.events.callRemote("OnRequestForgotUsername");
});



mp.events.add("hideMDC", () => {
    if(global.MDC_DOM == null) return;
    global.MDCActive = false;
    global.MDC_DOM.active = false;
});

mp.events.add("login_StopMusic", () => {
    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.auth.$refs.panel.stop();`);
});

