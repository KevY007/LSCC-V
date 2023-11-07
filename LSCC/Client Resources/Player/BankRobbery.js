const ROBBERY_TIME = 40;
var global = require('./globals');
  const {TranslateMessage: Translate} = require('./Translator');

mp.events.add("EVENTS:SetBankRobberyActive", (value) => {
    global.BankRobberyOn = value;
});

var ObjectiveData = {
    show:false,
    message:""
};

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