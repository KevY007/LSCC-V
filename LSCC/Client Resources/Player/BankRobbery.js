const ROBBERY_TIME = 60;

let robTimeLeft = ROBBERY_TIME;
let robTimer = null;

let FugiCarBlips = [];
let FinishMarker = null;
let FinishColshape = null; // cuz adding OnEnterMarker is so hard... thanks ragemp devs very cool

mp.events.add("StartBankRobberyTimer", () => {
    if(mp.players.local == undefined){
        clearInterval(robTimer);
        return;
    }
    if(!mp.players.exists(mp.players.local)){
        clearInterval(robTimer);
        return;
    }

    robTimeLeft = ROBBERY_TIME;
    mp.events.callLocal("BankRob_ClearFugiCarBlips");
    robTimer = setInterval(() => {

        if(mp.players.local == undefined){
            clearInterval(robTimer);
            return;
        }
        if(!mp.players.exists(mp.players.local)){
            clearInterval(robTimer);
            return;
        }

        robTimeLeft--;
        if(robTimeLeft <= 0){
            mp.events.callRemote("BankRob_ContinueState", 1);
        }
        else if(robTimeLeft == 45){
            mp.events.callRemote("BankRob_SendMessage", 0, 0);
        }
        else if(robTimeLeft == 30){
            mp.events.callRemote("BankRob_SendMessage", 0, 1);
            mp.events.callRemote("BankRob_SendMessage", 1, 0);

        }
        else if(robTimeLeft == 15){
            mp.events.callRemote("BankRob_SendMessage", 0, 2);
        }
    }, 1000);
});

function bankRobEnterColshape(player, shape) {
    if(shape == FinishColshape) 
    {
        if(player.getVariable("HeistLeader") == true)
            mp.events.callRemote("BankRob_ContinueState", 3);
        else{
            mp.gui.chat.push("[>] Sorry, the heist leader must enter the marker by themselves in order to win!");
        }
    }
}
  
mp.events.add("playerEnterColshape", bankRobEnterColshape);

mp.events.add("BankRob_CreateFinishMarker", (position) => {
    FinishMarker = mp.markers.new(1, position, 3.0,
        {
            color: [255, 78, 51, 255],
            visible: true,
            dimension: mp.players.local.dimension
        });
    FinishColshape = mp.colshapes.newSphere(position.x, position.y, position.z, 4.0, mp.players.local.dimension);
    
});
mp.events.add("BankRob_ClearFinishMarker", () => {
    if(FinishMarker != null && FinishMarker != undefined)
    {
        FinishMarker.destroy();
        FinishMarker = null;
    }
    if(FinishColshape != null && FinishColshape != undefined)
    {
        FinishColshape.destroy();
        FinishColshape = null;
    }
});

mp.events.add("BankRob_ClearFugiCarBlips", () => {
    if(FugiCarBlips.length <= 0) return;

    for(let i = 0 ; i < FugiCarBlips.length ; i++){
        FugiCarBlips[i].destroy();
    }
    FugiCarBlips = [];
})

mp.events.add("BankRob_SetFugiCarBlip", (id, position) => {
 
    switch(id){
        case 0:{
            let _b = mp.blips.new(616, position,
                {
                    name: "Getaway #1",
                    scale: 1.0,
                    color: 79,
                    drawDistance: 100.0
                });
            FugiCarBlips.push(_b);
        }
        break;
        case 1:{
            let _b = mp.blips.new(530, position,
                {
                    name: "Getaway #2",
                    scale: 1.0,
                    color: 79,
                    drawDistance: 100.0
                });
            FugiCarBlips.push(_b);
        }
        break;
        case 2:{
            let _b = mp.blips.new(663, position,
                {
                    name: "Getaway #2",
                    scale: 1.0,
                    color: 79,
                    drawDistance: 100.0
                });
            FugiCarBlips.push(_b);
        }
        break;
    }
})