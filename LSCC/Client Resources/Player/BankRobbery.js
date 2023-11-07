const ROBBERY_TIME = 40;
var global = require('./globals');
var ObjectiveData = {
    show:false,
    message:""
};
  const {TranslateMessage: Translate} = require('./Translator');


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

    const argsArray = (args.length > 0 ? args.toArray() : null);

    ObjectiveData = {
        show:toggle,
        message:Translate(message_code, argsArray)
    };
});
mp.events.add('render', () => {
   if(!ObjectiveData.show) return;

   mp.game.graphics.drawText(ObjectiveData.message, [0.9, 0.5], {
    font:2,
    color:[255, 255, 255, 255],
    outline:true,
    scale:[0.4, 0.4],
    centre:true
   });
});