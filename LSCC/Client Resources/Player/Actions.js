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
        default: return;
    }
});

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