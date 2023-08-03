var global = require('./globals');
const {TranslateMessage: Translate} = require('./Translator');

var WeaponEditorCamera;

var editor_MoveFromCam;

const editorCoords = {
    camera: new mp.Vector3(460.24365, -981.17096, 31.161589),
    cameraRotation: new mp.Vector3(0, 0, -90),
};
const localPlayer = mp.players.local;



function moveCam(x, y, z, rx, ry, rz, time, newviewangle){
    global.traceLastFunc(`[Players] Move Cam`);
    if(!WeaponEditorCamera) return;
    if(editor_MoveFromCam != null){
        editor_MoveFromCam.setActive(false);
        editor_MoveFromCam.destroy();
        editor_MoveFromCam = null;
    }
    editor_MoveFromCam = mp.cameras.new("MoveFromCam", WeaponEditorCamera.getCoord(), WeaponEditorCamera.getRot(2), WeaponEditorCamera.getFov());

    setTimeout(() => {
        WeaponEditorCamera.setCoord(x, y, z);
        WeaponEditorCamera.setRot(rx, ry, rz, 2);
        WeaponEditorCamera.setFov(newviewangle);
        WeaponEditorCamera.setActiveWithInterp(editor_MoveFromCam.handle, time, 1, 1);
    }, 100);
    
}

mp.events.add("WeaponEditor:exit", () => {
    global.WeaponEditorActive = false;
    global.WeaponEditorDOM.execute(`resetWeaponEditor();`);
    mp.events.callRemote("WeaponEditor:onExit");

    global.WeaponEditorDOM.active = false;
    if(global.ServerUI) global.ServerUI.call('hud_toggle', mp.storage.data.menu.HUD);
    mp.game.ui.displayRadar(true);
    mp.game.ui.displayHud(true);
    mp.players.local.clearTasksImmediately();
    mp.players.local.setAlpha(255);
    mp.players.local.freezePosition(false);
    mp.gui.cursor.show(false, false);

    mp.game.cam.renderScriptCams(false, false, 0, true, false);
    if(WeaponEditorCamera != null){
      WeaponEditorCamera.setActive(false);
      WeaponEditorCamera.destroy();
      WeaponEditorCamera = null;
    }
    if(editor_MoveFromCam != null){
      editor_MoveFromCam.setActive(false);
      editor_MoveFromCam.destroy();
      editor_MoveFromCam = null;
  }
});

mp.events.add("WeaponEditor:playSound", (type) => {
  switch(type){
    case "back":
    mp.game.audio.playSoundFrontend(-1, 'BACK', 'HUD_FREEMODE_SOUNDSET', true);
    break;
    case "select":
    mp.game.audio.playSoundFrontend(-1, 'Apt_Style_Purchase', 'DLC_APT_Apartment_SoundSet', true);   
    break;
    case "buy":
    mp.game.audio.playSoundFrontend(-1, 'PICKUP_WEAPON_SMOKEGRENADE', 'HUD_FRONTEND_WEAPONS_PICKUPS_SOUNDSET', true);
    break;
    case "sell":
    mp.game.audio.playSoundFrontend(-1, 'PICK_UP_WEAPON', 'HUD_FRONTEND_CUSTOM_SOUNDSET', true);
    break;
    default: break;
  }
});

mp.events.add("WeaponEditor:show", () => {
    mp.events.callRemote("WeaponEditor:onOpen");
    global.WeaponEditorActive = true;
    if(global.ServerUI) global.ServerUI.call('hud_toggle', false);
    mp.players.local.setAlpha(0);
    mp.players.local.freezePosition(true);
    mp.players.local.clearTasksImmediately();
    mp.game.ui.displayRadar(false);
    mp.game.ui.displayHud(false);
    if (WeaponEditorCamera === undefined || WeaponEditorCamera === null) {
        WeaponEditorCamera = mp.cameras.new("weaponEditorCamera", editorCoords.camera, editorCoords.cameraRotation, 30);
    }
    WeaponEditorCamera.setActive(true);
    mp.game.cam.renderScriptCams(true, false, 0, true, false);

    global.WeaponEditorDOM.active = true;
    mp.gui.cursor.show(true, true);

    mp.players.local.taskPlayAnim("weapons@pistol@stealth", "aim_med_loop", 10.0, 1.0, -1, 1, 1.0, false, false, false); 
});
mp.events.add("WeaponEditor:requestComponentList", (hash, type) => {
    let newviewangle = 30;
    if(type == "Sniper Rifles"){
        newviewangle = 40;
    }
    else if(type == "Machine Guns"){
        newviewangle = 35;
    }
    else if(type == "Handguns"){
        newviewangle = 20;
    }
    else newviewangle = 30;

    moveCam(editorCoords.camera.x, editorCoords.camera.y, editorCoords.camera.z, editorCoords.cameraRotation.x, 
        editorCoords.cameraRotation.y, editorCoords.cameraRotation.z, 1000, newviewangle);

    // SET_CURRENT_PED_WEAPON(Ped ped, Hash weaponHash, BOOL equipNow)
    mp.players.local.taskPlayAnim("weapons@pistol@stealth", "aim_med_loop", 10.0, 1.0, -1, 1, 1.0, false, false, false); 
    mp.game.invoke('0xADF692B254977C0C', mp.players.local.handle, hash, true);

    mp.events.callRemote("WeaponEditor:onRequestComponentList", hash);
});
mp.events.add("WeaponEditor:requestAttachmentPreview", (atthash) => {
    mp.events.callRemote("WeaponEditor:onRequestAttachmentPreview", atthash);
});
mp.events.add("WeaponEditor:removePreviewAttachment", (wephash, comphash) => {
    let realHash = wephash;
    mp.game.invoke('0x1E8BE90C74FB4C09', mp.players.local.handle, (realHash)>>0, (comphash)>>0);

    setTimeout(() => {
        mp.players.local.taskPlayAnim("weapons@pistol@stealth", "aim_med_loop", 10.0, 1.0, -1, 1, 1.0, false, false, false); 
        mp.game.invoke('0xADF692B254977C0C', mp.players.local.handle, (realHash)>>0, true);
        mp.game.invoke('0x1E8BE90C74FB4C09', mp.players.local.handle, (realHash)>>0, (comphash)>>0);
    }, 100);
    setTimeout(() => {
        mp.players.local.taskPlayAnim("weapons@pistol@stealth", "aim_med_loop", 10.0, 1.0, -1, 1, 1.0, false, false, false); 
        mp.game.invoke('0xADF692B254977C0C', mp.players.local.handle, (realHash)>>0, true);
    }, 500);
});

mp.events.add("WeaponEditor:sendAttachmentPreview", () => {
    // SET_CURRENT_PED_WEAPON(Ped ped, Hash weaponHash, BOOL equipNow)
    let hash = mp.players.local.weapon;
    setTimeout(() => {
      mp.players.local.taskPlayAnim("weapons@pistol@stealth", "aim_med_loop", 10.0, 1.0, -1, 1, 1.0, false, false, false); 
      mp.game.invoke('0xADF692B254977C0C', mp.players.local.handle, (hash)>>0, true);
    }, 100);
    // just in case setting the weapon again cuz sometimes the shit bugs idk
    setTimeout(() => {
      mp.players.local.taskPlayAnim("weapons@pistol@stealth", "aim_med_loop", 10.0, 1.0, -1, 1, 1.0, false, false, false); 
      mp.game.invoke('0xADF692B254977C0C', mp.players.local.handle, (hash)>>0, true);
    }, 500);
});
mp.events.add("WeaponEditor:sendComponentList", (data) => {
    let hash = mp.players.local.weapon;
    setTimeout(() => {
      mp.players.local.taskPlayAnim("weapons@pistol@stealth", "aim_med_loop", 10.0, 1.0, -1, 1, 1.0, false, false, false); 
      mp.game.invoke('0xADF692B254977C0C', mp.players.local.handle, (hash)>>0, true);
    }, 100);
    setTimeout(() => {
      mp.players.local.taskPlayAnim("weapons@pistol@stealth", "aim_med_loop", 10.0, 1.0, -1, 1, 1.0, false, false, false); 
      mp.game.invoke('0xADF692B254977C0C', mp.players.local.handle, (hash)>>0, true);
    }, 500);
    global.WeaponEditorDOM.execute(`setComponentList(${data})`);
});
mp.events.add("WeaponEditor:requestAttachmentPurchase", (comphash, price) => {
    mp.events.callRemote("WeaponEditor:onRequestPurchaseAttachment", comphash, price);
});
mp.events.add("WeaponEditor:requestAttachmentRemove", (comphash, price) => {
  mp.events.callRemote("WeaponEditor:onRequestRemoveAttachment", comphash, price);
});
mp.events.add("WeaponEditor:sendPurchaseResponse", (comphash) => {
    global.WeaponEditorDOM.execute(`setAttachmentPurchased('${comphash}', true)`);
});
mp.events.add("WeaponEditor:sendRemoveResponse", (comphash) => {  
  global.WeaponEditorDOM.execute(`setAttachmentPurchased('${comphash}', false)`);
});