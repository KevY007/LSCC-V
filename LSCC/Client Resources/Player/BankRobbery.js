var global = require('./globals');
var ObjectiveData = {
    show:false,
    message:""
};
  const {TranslateMessage: Translate} = require('./Translator');


var BR_BLIPS = [];
var FinishMarker = null;
mp.events.add("EVENT:AddBankRobberyFinishMarker", (posX, posY, posZ) => {
    if(FinishMarker != null) return;

    FinishMarker = mp.markers.new(1, new mp.Vector3(posX, posY, posZ), 2.0, {
        color:[255, 255, 0, 128],
        visible:true,
        dimension:mp.players.local.dimension
    });
});
mp.events.add("EVENT:DestroyBankRobberyFinishMarker", () => {
    if(FinishMarker == null) return;

    FinishMarker.destroy();
    FinishMarker = null;
});

mp.events.add("EVENT:ClearObjectiveBlips", () => {
    for(let i = 0 ; i < BR_BLIPS.length ; i++){
        if(BR_BLIPS[i])
            BR_BLIPS[i].destroy();
    }
    BR_BLIPS = [];
});
mp.events.add("EVENT:AddBankRobberyObjectiveBlip", (posX, posY, posZ) => {
    var blip = mp.blips.new(1, new mp.Vector3(posX, posY, posZ), {
        name:"Objective",
        scale:1.0,
        color:5,
        alpha:255,
        shortRange:false,
        dimension:mp.players.local.dimension
    });
    BR_BLIPS.push(blip);
});
mp.events.add("EVENT:SetBankRobberyWaypoint", (posX, posY) => {
    mp.game.ui.setNewWaypoint(posX, posY);
});
mp.events.add("EVENT:ClearBankRobberyWaypoint", () => {
    mp.game.invoke('0xA7E4E2D361C2627F'); // CLEAR WAYPOINT
});

mp.events.add("playerEnterVehicle", (vehicle, seat) => {
    const isHeistLeader = (mp.players.local.hasVariable("BR_IsHeistLeader") && mp.players.local.getVariable("BR_IsHeistLeader") == true);
    if(!isHeistLeader) return;
    if(!vehicle.hasVariable("Team")) return;

    const team = vehicle.getVariable("Team");
    mp.events.callRemote("EVENT:BankRobbery_LeaderEnterVehicle", vehicle, team);
});

mp.events.add("EVENTS:SetBankRobberyActive", (value) => {
    global.BankRobberyOn = value;
});
mp.events.add("EVENTS:SetBankRobberyObjective", (toggle, message_code, ...args) => {

    const argsArray = (args.length > 0 ? args : null);

    ObjectiveData = {
        show:toggle,
        message:Translate(message_code, argsArray)
    };
});
mp.events.add('render', () => {
   if(!ObjectiveData.show) return;

   mp.game.graphics.drawText(ObjectiveData.message, [0.5, 0.9], {
    font:2,
    color:[255, 255, 255, 255],
    outline:true,
    scale:[0.25, 0.25],
    centre:true
   });
});