var global = require('./globals');

mp.events.add("LobbyCamera:Toggle", (toggle) => {
    if(toggle == true){
        mp.events.callLocal("ForceOpenMenu");
        mp.events.callLocal("ToggleHud", false);
        mp.game.time.setClockTime(11, 0, 0);
        mp.game.gameplay.setWeatherTypeOverTime('EXTRASUNNY', 1);
    }
    else{
        mp.events.callLocal("ForceCloseMenu");
        mp.events.callLocal("ToggleHud", true);
    }
});