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
require('./Player/Character.js');
require('./Player/Achievements.js');
require('./Player/BankRobbery.js');
require('./Player/Actions.js');
require("./Player/Nametags.js");
require("./Player/SyncScripts/Aim");
require("./Player/SyncScripts/Flashlight");
require("./Player/SyncScripts/Laser");
require('./Player/DamageIndicator/damage_indicator');
require('./Player/DamageIndicator/anti_peek');
require('./Player/DamageIndicator/kill_trace');
require('./Player/Anticheat/Weapons');
require('./Player/Teleport.js');
require("./ServerUI/HUD.js");
require("./ServerUI/Menu.js");
require("./ServerUI/Logs.js");
require("./ServerUI/CharacterCreator.js");
require("./ServerUI/Tutorial.js");
require("./ServerUI/XMR.js");
require("./TeleportManager/index.js");
require("./WeaponFiringMode/index.js");
require("./charcreator/index.js");
require("./charcreator/outfitslots.js");
require("./charcreator/clotheditor.js");
require("./charcreator/tattoo/data.js");
require("./charcreator/tattoo/menu.js");
require("./charcreator/char_import/character_import.js");
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
      mp.game.time.setClockTime(20, 30, 0);
      mp.game.gameplay.setWeatherTypeNow('EXTRASUNNY');
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

mp.events.add('finishAccountHwidCheck', (status) => {
  if(status == 0){

  }
  else{

  }
});

mp.events.add('showLoginBrowser', (state) => {
    setTimeout(() => {
        if(global.ServerUI != null)
        {      
            if(global.ServerUI.active) global.ServerUI.active = true;
            if(global.ServerUI) global.ServerUI.call('hud_toggle', false);
            if(global.ServerUI) global.ServerUI.call(`mainmenu_toggle`, false); 
            if(state){
                mp.game.graphics.transitionToBlurred(1000);
                mp.players.local.freezePosition(true);

                mp.events.callRemote('Auth_ConfirmAccountBySerial');
                if(global.ServerUI) global.ServerUI.call(`auth_toggle`, true);
                global.isLoginScreenActive = true;
                global.ServerUI.call('auth_quick_login_toggle', false);
                toggleLoginScene(true);

                if(mp.storage.data.menu.rememberMe !== undefined && mp.storage.data.menu.rememberMe === true){
                  mp.events.callRemote("requestQuickLoginForgotUsernameData"); // If 'Remember Me' is checked, auto-login the user.
                }

                mp.events.callLocal("actionMenu:removeAllItems");
            }
            else{
                if(!mp.players.local || !mp.players.exists(mp.players.local)) return;
                mp.game.graphics.transitionFromBlurred(1000);

                if(global.ServerUI) global.ServerUI.call(`auth_toggle`, false);
                global.isLoginScreenActive = false;
                if(!global.IsInTutorial){
                    if(global.ServerUI) global.ServerUI.call('hud_toggle', true);
                    if(global.ServerUI) global.ServerUI.call('hud_minilist', true);
                    if(global.ServerUI) global.ServerUI.call("hud_minilist_toggle", true);
                }
                if(global.ServerUI) global.ServerUI.markAsChat();
            }
            if(!global.IsInTutorial)
                mp.gui.cursor.show(state, state);
        }
        else{
            //mp.events.callRemote("ServerConsoleOutput", `Attempted to show login browser to ${mp.players.local.name} while it's null! Trying again...`);
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
        mp.events.callLocal("HUDError", "The login credentials are invalid!");
        mp.gui.cursor.show(true, true);
    }
});
mp.events.add("login_ShowAccountExists", (toggle) => {
    if(global.ServerUI && global.ServerUI.active){
        mp.events.callLocal("HUDError", "That username is already taken!");
        mp.gui.cursor.show(true, true);
    }
});
mp.events.add("login_MaxAccountLimit", (toggle) => {
    if(global.ServerUI && global.ServerUI.active){
        mp.events.callLocal("HUDError", "You've reached the maximum accounts limit! (2 accounts)");
        mp.gui.cursor.show(true, true);
        // TO-DO: Add a message that max account limit has been reached!
    }
});
mp.events.add("login_ForceStopLoading", () => {
  if(global.ServerUI && global.ServerUI.active){
    mp.gui.cursor.show(true, true);
  }
});

mp.events.add('auth_reset_password', (code, newPassword) => {
  mp.events.callRemote('RequestResetPassword', code, newPassword);
});
mp.events.add("PasswordResetResult", (result) => {
  if(result === 0)
    global.ServerUI.call('auth_reset_password_invalid_code');
  else
    global.ServerUI.call('auth_reset_password_success');
});

mp.events.add("browser_ServerConsole", (msg) => mp.events.callRemote("ServerConsoleOutput", msg));

mp.events.add("auth_reset_rememberMe", () => {
  mp.storage.data.menu.rememberMe = false;
  mp.storage.flush();
});

mp.events.add("onOpenChatbox", (toggle) =>
{
    if(toggle === true && ((global.VehicleSpawnerActive !== undefined && global.VehicleSpawnerActive == true) ||
    (global.WeaponEditorActive !== undefined && global.WeaponEditorActive == true) ||
    (global.ActionMenuActive !== undefined && global.ActionMenuActive == true))){
        return;
    }
    if(mp.players.local.getVariable("pLogged") == undefined || mp.players.local.getVariable("pLogged") == false && toggle)
    {
        global.activateChat(false);
        return;
    }
    
    if(global.chatStatusTimeout)
      clearTimeout(global.chatStatusTimeout);

    global.chatStatusTimeout = null;
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
            global.chatStatusTimeout = null;
        }, 500);
    }

});

mp.events.add("auth_reset_password", (code, newPassword) => {
  // Code here
});
mp.events.add('auth_login', (username, password, rememberMe) => {
    mp.events.callRemote('OnPlayerLoginAttempt', username, password);

    mp.storage.data.menu.rememberMe = rememberMe;
    mp.storage.flush();
});

mp.events.add('auth_register', (username, email, password, confirmPass) => {

  if(password === confirmPass)
    mp.events.callRemote('OnPlayerRegisterAttempt', email, username, password);
  else{
    mp.events.callLocal("HUDError", "The passwords do not match!");
  }

});

mp.events.add('requestAutoLoginData', () => {
    mp.events.callRemote("OnRequestAutoLoginData");
});

var quickLoginUsername = "";
mp.events.add('sendAutoLoginDataToBrowser', (data) => {
    if(mp.storage.data.menu.rememberMe == undefined || mp.storage.data.menu.rememberMe === false) return;

    setTimeout(() => {
        if(mp.storage.data.menu.rememberMe == undefined || mp.storage.data.menu.rememberMe === false) return;

        if(data.length > 2){
            //if(global.ServerUI) global.ServerUI.call(`auth_quick_login_set`, data);
            quickLoginUsername = data;
            global.ServerUI.call('auth_quick_login_toggle', true);
        }
    }, 500);
});

mp.events.add('auth_quick_login', () => {
    if(mp.storage.data.menu.rememberMe == undefined || mp.storage.data.menu.rememberMe === false) return;
    
    mp.events.callRemote("OnAutoLoginAttempt", quickLoginUsername);
});

mp.events.add('requestForgotUsernameServer', () => {
    mp.events.callRemote("OnRequestForgotUsername");
});



mp.events.add("hideMDC", () => {
    if(global.MDC_DOM == null) return;
    global.MDCActive = false;
    global.MDC_DOM.active = false;
});

var passwordChangeDialogInterval = null;

mp.events.add("showPasswordChangeDialog", (reason) => {
  passwordChangeDialogInterval = setInterval(() => {
    if(global.ServerUI) global.ServerUI.call('hud_toggle', false);
    if(global.ServerUI) global.ServerUI.call('hud_minilist', false);
    if(global.ServerUI) global.ServerUI.call("hud_minilist_toggle", false);
    mp.gui.cursor.show(true, true);
  }, 100);


  if(global.ServerUI) global.ServerUI.call('dialog_force_password_change_open', reason);
});
mp.events.add('force_password_change_submit', (newPassword) => {
  clearInterval(passwordChangeDialogInterval);
  passwordChangeDialogInterval = null;
  mp.events.callRemote("OnForceChangePassword", newPassword);
});
mp.events.add("showPasswordChangeDisclaimer", () => {
  var reconnectingState = '.';
  setInterval(() => {
    switch (reconnectingState) {
      case '.':
        reconnectingState = '..';
        break;
      case '..':
        reconnectingState = '...';
        break;
      default:
        reconnectingState = '.';
        break;
    }
  }, 500);
  mp.events.add('render', () => {
    mp.game.graphics.drawText(`~g~Password changed successfully!\n~y~Reconnecting${reconnectingState}`, [0.5, 0.5], {
      font:0,
      centre:true,
      scale:[0.7, 0.7],
      outline:true,
      color:[0, 255, 0, 255]
    });
  });
});