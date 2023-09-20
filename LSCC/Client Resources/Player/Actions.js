let actionInterval = null;
let actionTime = 0;
let actionType = "none";
var global = require('./globals');
  const {TranslateMessage: Translate} = require('./Translator');
let maxActionTime = 0;

function initiateActionTimer(startTime){
    maxActionTime = startTime;
}

function startActionTimer(message, timeInSeconds){
    if(global.ServerUI) global.ServerUI.call('notification_send', message, 'info', time*1000, 'action_timer');
}
function stopActionTimer(){
    if(global.ServerUI) global.ServerUI.call('notification_dismiss', 'action_timer');
}


mp.events.add("StartPlayerAction", (action, time) => {
    if(mp.players.local.getVariable("Wounded") > 0){
        return;
    }
    actionTime = time;
    actionType = action;
    maxActionTime = time;
    switch(action){
        case "medkit":
            MedkitAction();
            break;
        case "painkiller":
            PainkillerAction();
            break;
        default: return;
    }
});

mp.events.add("StopPlayerActions", () => {
    switch(actionType){
        case "medkit":
            mp.events.callRemote("clear_anim");
            clearInterval(actionInterval);
            break;
        case "painkiller":
            clearInterval(actionInterval);
            break;
        default: return;
    }
    actionType = "none";
    actionTime = 0;
    maxActionTime = 0;
    actionInterval = null;
    stopActionTimer();
})

function PainkillerAction(){
    if(mp.players.local == null || mp.players.local.handle == 0) return;

    if(mp.players.local.getVariable("Wounded") > 0) return;

    if(!mp.players.local.isInAnyVehicle(true))
        mp.events.callRemote("play_anim", "mini@sprunk", "plyr_buy_drink_pt2", 0, 8.0, 0.0, -1, 0.0, false, false, false);

    actionInterval = setInterval(() => {

        if(mp.players.local == null || mp.players.local.handle == 0){
            clearInterval(actionInterval);
            return;
        }
        if(mp.players.local.getVariable("Wounded") > 0){
            clearInterval(actionInterval);
            return;
        }

        try{
            mp.events.callRemote("OnPlayerActionTick");
            if(actionTime <= 0){
                mp.events.callRemote("FinishPlayerAction", actionType);
                stopActionTimer();
                clearInterval(actionInterval);
            }
            else{
                actionTime--;

                if(!mp.players.local.isInAnyVehicle(true))
                {
                    if(actionTime == 39){
                        mp.events.callRemote("play_anim", "mini@sprunk", "plyr_buy_drink_pt3", 0, 8.0, 0.0, -1, 0.0, false, false, false);
                    }
                    if(actionTime == 38){            
                        mp.events.callRemote("clear_anim");
                    }
                }
            }
        } finally { }
    }, 1000);   
}

function MedkitAction(){
    if(mp.players.local == null || mp.players.local.handle == null) return;

    if(mp.players.local.getVariable("Wounded") > 0){
        return;
    }
    mp.events.callRemote("play_anim", "amb@medic@standing@kneel@enter", "enter", 0, 8.0, 0.0, -1, 0.0, false, false, false);
    initiateActionTimer(actionTime);
    startActionTimer("Using medkit...", actionTime);

    actionInterval = setInterval(() => {

        if(mp.players.local == null || mp.players.local.handle == null) return;
        if(mp.players.local.getVariable("Wounded") > 0){
            clearInterval(actionInterval);
            return;
        }
        try{
            mp.events.callRemote("OnPlayerActionTick");
            if(actionTime <= 0){
                mp.events.callRemote("clear_anim");
                mp.events.callRemote("FinishPlayerAction", actionType);
                stopActionTimer();
                clearInterval(actionInterval);
            }
            else{
                actionTime--;           
                //if(global.ServerUI) global.ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.actionBar.time = ${((maxActionTime - actionTime) == 0 ? 0 : (maxActionTime - actionTime))}`);
                if(actionTime == 1){
                    mp.events.callRemote("play_anim", "amb@medic@standing@kneel@exit", "exit", 0, 8.0, 0.0, -1, 0.0, false, false, false);
                }
            }
        } finally { }
    }, 1000);
}

// ------ E KEY ------ //

mp.keys.bind(0x45, true, () => {carryCheck();});

let isCurrentWeaponAllowedForCarry = false;
let carry_lastWeaponCarried = undefined;

function equipHandgun(){
    if(mp.players.local.getVariable("Handgun_Hash") !== undefined){
        const gunData = global.weaponData.find(w => Number(w.Hash) == mp.players.local.getVariable("Handgun_Hash"));
        if(gunData !== undefined && gunData.Type === "Handguns"){
            mp.game.invoke('0xADF692B254977C0C', mp.players.local.handle, (Number(gunData.Hash))>>0, true);
            isCurrentWeaponAllowedForCarry = true;
        }
        else{
            mp.game.invoke('0xADF692B254977C0C', mp.players.local.handle, 2725352035>>0, true);
            isCurrentWeaponAllowedForCarry = true;
        }
    }
    else{
        mp.game.invoke('0xADF692B254977C0C', mp.players.local.handle, 2725352035>>0, true);
        isCurrentWeaponAllowedForCarry = true;
    }
}

mp.events.add("render", () => {
    if(global.carryTarget === undefined) return;

    if(global.carryTarget != -1){
        if(mp.players.local.weapon !== carry_lastWeaponCarried){
            isCurrentWeaponAllowedForCarry = false;
            carry_lastWeaponCarried = mp.players.local.weapon;
        }

        if(!isCurrentWeaponAllowedForCarry){
            let weaponHash = mp.players.local.weapon;
            if(weaponHash !== undefined){
                const gunData = global.weaponData.find(w => Number(w.Hash) == weaponHash);
                if(gunData !== undefined){
                    if(gunData.Type === "Handguns" || gunData.Type === "Melee"){
                        isCurrentWeaponAllowedForCarry = true;
                    }
                    else{
                        equipHandgun();
                    }
                    
                }
                else{
                    equipHandgun();
                }
            }
            else{
                equipHandgun();
            }
        }

        disableControlsOnCarry();        
    }
});

function disableControlsOnCarry(){
    mp.game.controls.disableControlAction(27, 75, true); // disable exit vehicle
        mp.game.controls.disableControlAction(0, 23, true); // disable enter vehicle
        mp.game.controls.disableControlAction(27, 23, true); // disable enter vehicle

        mp.game.controls.disableControlAction(0, 22, true); //jump
        mp.game.controls.disableControlAction(0, 44, true); //cover

        /*
        mp.game.controls.disableControlAction(0, 37, true); //weapon wheel / switch
        mp.game.controls.disableControlAction(0, 99, true); //next weapon
        mp.game.controls.disableControlAction(0, 100, true); //prev weapon

        //mp.game.controls.disableControlAction(0, 157, true); //weapon switch
        mp.game.controls.disableControlAction(0, 158, true); //weapon switch
        //mp.game.controls.disableControlAction(0, 159, true); //weapon switch
        mp.game.controls.disableControlAction(0, 160, true); //weapon switch
        mp.game.controls.disableControlAction(0, 161, true); //weapon switch
        mp.game.controls.disableControlAction(0, 162, true); //weapon switch
        mp.game.controls.disableControlAction(0, 163, true); //weapon switch
        mp.game.controls.disableControlAction(0, 164, true); //weapon switch
        mp.game.controls.disableControlAction(0, 165, true); //weapon switch

        mp.game.controls.disableControlAction(0, 261, true); //weapon switch scroll
        mp.game.controls.disableControlAction(0, 262, true); //weapon switch scroll
        */
}

function carryCheck(){
    if(mp.players.local.getVariable("Team") == -1 || mp.players.local.getVariable("Wounded") > 0) return;
    if(mp.players.local.isInAnyVehicle(false)) return;
    if((global.VehicleSpawnerActive !== undefined && global.VehicleSpawnerActive == true) ||
        (global.WeaponEditorActive !== undefined && global.WeaponEditorActive == true) ||
        (global.ActionMenuActive !== undefined && global.ActionMenuActive == true) ||
        (global.ClothingEditorActive !== undefined && global.ClothingEditorActive == true) ||
        (global.menuToggled !== undefined && global.menuToggled == true) ||
        (global.chatStatus !== undefined && global.chatStatus == true) ||
        (global.isHotwiring !== undefined && global.isHotwiring == true)){
        return;
    }
    global.carryTarget = mp.players.local.getVariable("CarryingPlayer");
    if(global.carryTarget === -1){
        tryCarryPlayer();
    }
    else{
        stopCarryPlayer();
    }
}

function tryCarryPlayer(){
    mp.players.forEach(pl => {
        if(pl.getVariable("Team") == mp.players.local.getVariable("Team"))
        {
            if(pl.getVariable("Wounded") == 2)
            {
                if(pl.remoteId != mp.players.local.remoteId)
                {
                    if(pl.getVariable("BeingRevived") == false && pl.getVariable("CarriedBy") == -1)
                    {
                        let distance = global.calcDist(mp.players.local.position, pl.position);
                        if(distance < 5)
                        {
                            global.carryTarget = pl;
                            mp.game.player.setSprint(false);
                            mp.events.callRemote("RequestCarryPlayer", pl.remoteId);
                            return;
                        }
                    }
                }
            }
        }
    });
}
mp.events.add("onStopCarryPlayer", () => {stopCarryPlayer();});
function stopCarryPlayer(){
    if(global.carryTarget.remoteId === mp.players.local.remoteId) return;

    global.carryTarget = -1;
    mp.game.player.setSprint(true);
    isCurrentWeaponAllowedForCarry = false;
    carry_lastWeaponCarried = undefined;
    mp.events.callRemote("RequestStopCarryPlayer");
}

mp.events.add("onGetCarried", () => {
    if(mp.players.local === undefined || !mp.players.exists(mp.players.local)) return;

    if(mp.players.local.getVariable("Wounded") > 0){
        mp.events.callLocal("togglePlayerCrawl", false);
    }
});
mp.events.add("onStopGettingCarried", () => {
    if(mp.players.local === undefined || !mp.players.exists(mp.players.local)) return;

    if(mp.players.local.getVariable("Wounded") > 0){
        mp.events.callLocal("togglePlayerCrawl", true);
    }
});

// ------ H KEY ------ //
let reviveTarget = null;
let reviveTime = 7;
let reviveTimer = null;
mp.keys.bind(0x48, true, onHoldHkey);
function onHoldHkey()
{
    if(mp.players.local.getVariable("Wounded") > 0){
        return;
    }

    if(canRevive())
    {
        mp.players.forEach(pl => {
            if(pl.getVariable("Team") == mp.players.local.getVariable("Team"))
            {
                if(pl.getVariable("Wounded") == 2)
                {
                    if(pl.remoteId != mp.players.local.remoteId)
                    {
                        if(pl.getVariable("BeingRevived") == false)
                        {
                            let distance = global.calcDist(mp.players.local.position, pl.position);
                            if(distance < 5)
                            {
                                reviveTarget = pl;
                                reviveTime = 7;
                                mp.events.callRemote("AssignReviveTarget", pl.remoteId);
                                return;
                            }
                        }
                    }
                }
            }
        });
    }

    // if no revive target is found, request cuff
    requestCuffTarget();
}

// ------ REVIVAL SYSTEM ------ //
let isReviving = false;

function canRevive(){
    if(mp.players.local.getVariable("Team") == -1 || global.carryTarget !== -1 || mp.players.local.getVariable("Wounded") > 0 || global.chatStatus || global.menuToggled || global.scoreboardToggled || Date.now() - global.lastChatToggle <= 500) return false; 
    return true;
}
mp.events.add("BeginReviving", () => {
    if(mp.players.local == null || !mp.players.exists(mp.players.local)) return;
    if(reviveTarget == null || !mp.players.exists(reviveTarget)) return;
    if(global.carryTarget !== -1) return;
    if(mp.players.local.getVariable("Wounded") > 0){
        return;
    }

    reviveTime = 7;
    isReviving = true;
    initiateActionTimer(reviveTime);
    startActionTimer("Reviving player...", reviveTime);
    clearInterval(reviveTimer);
    reviveTimer = setInterval(() => {
        if(mp.players.local == null || !mp.players.exists(mp.players.local)){ stopCuffing(69); return; }
        if(reviveTarget == null || !mp.players.exists(reviveTarget)){ stopCuffing(69); return; }

        if(mp.players.local.getVariable("Team") == -1 || mp.players.local.isDead()){
            stopReviving(1);
            return;
        }       

        if(reviveTarget.getVariable("Team") == -1 || (mp.players.local.getVariable("Team") != reviveTarget.getVariable("Team")) || reviveTarget.isDead()){
            stopReviving(2);
        }

        if(!mp.keys.isDown(72)){ 
            stopReviving(4);
            return;
        }

        reviveTime--;
        if(reviveTime <= 0)
        {
            stopActionTimer();
            finishCuffing();
            mp.events.callRemote("FinishRevive", reviveTarget);
            clearInterval(reviveTimer);
            return;
        }
    }, 1000);
});

mp.events.add("StopReviving", (reason) => stopReviving(reason));
function stopReviving(reason){

    isReviving = false;
    clearInterval(reviveTimer);
    reviveTarget = null;

    mp.events.callRemote("StopReviving");
    stopActionTimer();

    switch(reason){
        case 1:{
            mp.gui.chat.push("!{#FF6347}[!] !{#FFFFFF}Reviving failed - you are no longer in copchase.");
            break;
        }        
        case 2:{
            mp.gui.chat.push("!{#FF6347}[!] !{#FFFFFF}Reviving failed - target is no longer on the same team as you or is dead.");
            break;
        }
        case 3:{
            mp.gui.chat.push("!{#FF6347}[!] !{#FFFFFF}Reviving failed - You are severely injured.");
            break;           
        }
    }
}


// ------ CUFFING SYSTEM ------ //
let isCuffing = false;
let cuffTimeLeft = 5;
global.cuffTarget = null;
global.cuffInterval = null;

function requestCuffTarget(){
    if(mp.players.local.getVariable("Team") != 1 || global.chatStatus || global.menuToggled || global.scoreboardToggled || Date.now() - global.lastChatToggle <= 500) return;

    mp.events.callRemote("RequestCuffTarget");
}

mp.events.add("StopCuffing", (reason) => { stopCuffing(reason); });

mp.events.add("BeginCuffing", (target) => {

    if(target == null || !mp.players.exists(target)) return;
    if(target.getVariable("Team") != 0 || mp.players.local.getVariable("Team") != 1) return;
    if(mp.players.local.getVariable("Wounded") > 0){
        return;
    }

    isCuffing = true;
    global.cuffTarget = target;
    cuffTimeLeft = 5;
    global.disableSprint = true;

    initiateActionTimer(cuffTimeLeft);
    startActionTimer("Handcuffing player...", cuffTimeLeft);
    
    if(global.cuffInterval != null) clearInterval(global.cuffInterval);

    global.cuffInterval = setInterval(() => {

        if(global.cuffTarget == null || !mp.players.exists(global.cuffTarget) || !mp.players.exists(mp.players.local)) {
            stopCuffing(0);
            return;
        }
        if(global.cuffTarget.getVariable("Team") != 0 || global.cuffTarget.isDead()){ 
            stopCuffing(1);
            return;
        }
        if(mp.players.local.getVariable("Team") != 1 || mp.players.local.isDead()){
            stopCuffing(3);
            return;
        }
        if(!mp.keys.isDown(72)){ 
            stopCuffing(5);
            return;
        }

        let distance = global.calcDist(mp.players.local.position, global.cuffTarget.position);
        if(distance > 4.0){
            stopCuffing(2);
            return;
        }

        cuffTimeLeft--;
        if(cuffTimeLeft <= 0){
            stopActionTimer();
            finishCuffing();
        }
    }, 1000);
});

mp.events.add("forceStopCuffing", () => {
    stopCuffing(69);
    mp.events.callRemote("clear_anim");
    stopActionTimer();
    mp.events.callLocal("StopPlayerActions");
});

function stopCuffing(reason){ // if unsuccessful / force stopped

    global.disableSprint = false;
    clearInterval(global.cuffInterval);
    isCuffing = false;
    cuffTimeLeft = 5;
    global.cuffInterval = null;
    global.cuffTarget = null;

    stopActionTimer();

    mp.events.callRemote("StopCuffing");

    switch(reason){
        case 1:
            mp.gui.chat.push("!{#FF6347}[!] !{#FFFFFF}Cuffing failed - the target is no longer a fugitive or deceased.");
            break;       
        case 2:
            mp.gui.chat.push("!{#FF6347}[!] !{#FFFFFF}Cuffing failed as you moved too far from the fugitive.");
            break;
        case 12: // putting 12 cuz idk the ids anymore fuck this shit im braindead at this point
            mp.gui.chat.push("!{#FF6347}[!] !{#FFFFFF}Cuffing failed - you are severely injured.");
            break;
    }
}
function finishCuffing(){ // if successful
    clearInterval(global.cuffInterval);

    global.disableSprint = false;
    isCuffing = false;
    cuffTimeLeft = 5;
    global.cuffInterval = null;

    if(!mp.players.exists(global.cuffTarget) || !mp.players.exists(mp.players.local)) { global.cuffTarget = null; return;} // better safe than sorry :)
    mp.events.callRemote("FinishCuffing", global.cuffTarget);
    global.cuffTarget = null;
}


/* LOCKPICKING */
mp.events.add("startLockpicking", (donorLevel) => {
    global.isLockpicking = true;
    let rangeSize = 15;
    rangeSize += (donorLevel*8);
    let rangePos = 0;
    do{
        rangePos = getRandomInt(0, 180);
    }
    while(rangePos+rangeSize >= 180);
});

// true = success ; false = fail
mp.events.add("stopLockpicking", () => {
    global.isLockpicking = false;  
});
mp.events.add("sendLockpickResult", (result) => {
    global.isLockpicking = false;
    mp.events.callRemote("onLockpickAttempt", result); 
});
mp.events.add("lockpick_vehicleAlarm", (veh, isLockpickPlayer, isSuccess) => {
    mp.game.invoke('0xCDE5E70C1DDB954C', veh.handle, true);
    setTimeout(() => {
        mp.game.invoke('0xB8FF7AB45305C345', veh.handle);
    }, 100);

    if(isLockpickPlayer && isSuccess){
        mp.players.local.taskEnterVehicle(veh.handle, 200, -1, 2.0, 1, 0);
    }
});

function getRandomInt(min, max) {
    min = Math.ceil(min);
    max = Math.floor(max);
    return Math.floor(Math.random() * (max - min) + min); // The maximum is exclusive and the minimum is inclusive
  }