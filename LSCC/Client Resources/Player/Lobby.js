var global = require('./globals');

mp.events.add("LobbyCamera:Toggle", (toggle) => {
    if(toggle == true){
        mp.events.callLocal("ForceOpenMenu");
        mp.events.callLocal("ToggleHud", false);
    }
    else{
        mp.events.callLocal("ForceCloseMenu");
        mp.events.callLocal("ToggleHud", true);
    }
});