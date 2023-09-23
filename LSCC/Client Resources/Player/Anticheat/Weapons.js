var global = require('./globals');

let lastHackedWeaponMessageSent = 0;
function checkHackedWeapons(){
    const weaponsJSON = mp.players.local.getVariable("AC_WeaponData");
    if(!weaponsJSON){
        if(mp.players.local.weapon !== 2725352035 && mp.players.local.weapon !== 0){
            if(Date.now() - lastHackedWeaponMessageSent > 15000){
                mp.events.callRemote("AC_SendWeaponHackAlert", mp.players.local.weapon.toString());
                lastHackedWeaponMessageSent = Date.now();
            }
        }
        return;
    }
    const weaponsArray = JSON.parse(weaponsJSON);

    if(!weaponsArray.includes(mp.players.local.weapon)){
        if(mp.players.local.weapon !== 2725352035 && mp.players.local.weapon !== 0){
            if(Date.now() - lastHackedWeaponMessageSent > 15000){
                mp.events.callRemote("AC_SendWeaponHackAlert", mp.players.local.weapon.toString());
                lastHackedWeaponMessageSent = Date.now();
            }
        }
      
    }
}
mp.events.add('TwoSecondsEvent', () => {
    checkHackedWeapons();
});