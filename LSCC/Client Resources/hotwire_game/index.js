var global = require('./globals');
  const {TranslateMessage: Translate} = require('./Translator');


mp.events.add("StartHotwire", () => {
    global.HotwireDOM.execute(`generateHotwire();`); // restarts the minigame
    global.HotwireDOM.active = true;
    global.isHotwiring = true;
});
mp.events.add("StopHotwire", () => {
    global.HotwireDOM.active = false;
    global.isHotwiring = false;
});
mp.events.add("CS_HotwireResult", (success) => {
    if(!global.isHotwiring) return;

    global.isHotwiring = false;
    if(success){
        setTimeout(() => {
            global.HotwireDOM.active = false; // to let the sound fully play
        }, 500);
    }
    mp.events.callRemote("OnHotwireResult", success);
});