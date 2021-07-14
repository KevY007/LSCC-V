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

    if(!player.isInAnyVehicle(true))
        mp.events.callRemote("play_anim", "mini@sprunk", "plyr_buy_drink_pt2", 0);

    actionInterval = setInterval(() => {
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
    }, 1000);   
}

function MedkitAction(){
    mp.events.callRemote("play_anim", "amb@medic@standing@kneel@enter", "enter", 0);

    actionInterval = setInterval(() => {
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
    }, 1000);
}