let actionInterval = null;
let actionTime = 0;
let actionType = "none";
let player = mp.players.local;


mp.events.add("StartPlayerAction", (action, time) => {
    actionTime = time;
    actionType = action;
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
    actionInterval = null;
});

function PainkillerAction(){
    if(mp.players.local == null || mp.players.local.handle == null) return;

    if(!player.isInAnyVehicle(true))
        mp.events.callRemote("play_anim", "mini@sprunk", "plyr_buy_drink_pt2", 0);

    actionInterval = setInterval(() => {

        if(mp.players.local == null || mp.players.local.handle == null) return;

        try{
            mp.events.callRemote("OnPlayerActionTick");
            if(actionTime <= 0){
                mp.events.callRemote("FinishPlayerAction", actionType);
                clearInterval(actionInterval);
            }
            else{
                actionTime--;

                if(!player.isInAnyVehicle(true))
                {
                    if(actionTime == 39){
                        mp.events.callRemote("play_anim", "mini@sprunk", "plyr_buy_drink_pt3", 0);
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
    mp.events.callRemote("play_anim", "amb@medic@standing@kneel@enter", "enter", 0);

    actionInterval = setInterval(() => {

        if(mp.players.local == null || mp.players.local.handle == null) return;
        try{
            mp.events.callRemote("OnPlayerActionTick");
            if(actionTime <= 0){
                mp.events.callRemote("clear_anim");
                mp.events.callRemote("FinishPlayerAction", actionType);
                clearInterval(actionInterval);
            }
            else{
                actionTime--;
                if(actionTime == 1){
                    mp.events.callRemote("play_anim", "amb@medic@standing@kneel@exit", "exit", 0);
                }
            }
        } finally { }
    }, 1000);
}



// ------ CUFFING SYSTEM ------ //
let isCuffing = false;
let cuffTimeLeft = 5;
cuffTarget = null;
cuffInterval = null;

function requestCuffTarget(){
    if(player.Team != 1) return;

    mp.events.callRemote("RequestCuffTarget");
}

mp.keys.bind(0x48 , true, requestCuffTarget);

mp.events.add("StopCuffing", (reason) => { stopCuffing(reason); });

mp.events.add("BeginCuffing", (target) => {

    if(!mp.players.exists(target)) return;
    if(target.Team != 0 || player.Team != 1) return;

    isCuffing = true;
    cuffTarget = target;
    cuffTimeLeft = 5;

    UIHud.execute(`gm.cuffProgress = true;`);
    UIHud.execute(`gm.$refs.cuffProgress.time = ${cuffTimeLeft}`);
    
    if(cuffInterval != null) clearInterval(cuffInterval);

    cuffInterval = setInterval(() => {

        if(!mp.players.exists(cuffTarget) || !mp.players.exists(player) || cuffTarget == null) {
            stopCuffing(0);
            return;
        }
        if(cuffTarget.Team != 0 ){ 
            stopCuffing(1);
            return;
        }
        if(player.Team != 1){
            stopCuffing(3);
            return;
        }
        if(!mp.keys.isDown(72)){ 
            stopCuffing(5);
            return;
        }

        let distance = player.position.subtract(cuffTarget.position).length();
        if(distance > 4.0){
            stopCuffing(2);
            return;
        }

        cuffTimeLeft--;
        if(cuffTimeLeft <= 0){
            UIHud.execute(`gm.cuffProgress = false;`);
            finishCuffing();
        }
        else{
            UIHud.execute(`gm.$refs.cuffProgress.time = ${cuffTimeLeft}`);
        }
    }, 1000);
});
function stopCuffing(reason){ // if unsuccessful / force stopped

    clearInterval(cuffInterval);
    isCuffing = false;
    cuffTimeLeft = 5;
    cuffInterval = null;
    cuffTarget = null;

    UIHud.execute(`gm.cuffProgress = false;`);

    mp.events.callRemote("StopCuffing");

    switch(reason){
        case 1:
            mp.gui.chat.push("!{#FF6347}[!] !{#FFFFFF}Cuffing failed - the target is no longer a fugitive.");
            break;       
        case 2:
            mp.gui.chat.push("!{#FF6347}[!] !{#FFFFFF}Cuffing failed as you moved too far from the fugitive.");
            break;
    }
}
function finishCuffing(){ // if successful
    clearInterval(cuffInterval);

    isCuffing = false;
    cuffTimeLeft = 5;
    cuffInterval = null;

    if(!mp.players.exists(cuffTarget) || !mp.players.exists(player) || cuffTarget == null){ cuffTarget = null; return;} // better safe than sorry :)
    mp.events.callRemote("FinishCuffing", cuffTarget);
    cuffTarget = null;
}