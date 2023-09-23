var global = require('./globals');

let lastHackedWeaponMessageSent = 0;
function checkHackedWeapons(){
    const weaponsJSON = mp.players.local.getVariable("AC_WeaponData");
    if(!weaponsJSON) return;
    const weaponsArray = JSON.parse(weaponsJSON);

    if(!weaponsArray.includes(mp.players.local.weapon)){
        if(Date.now() - lastHackedWeaponMessageSent > 15000){
            mp.events.callRemote("AC_SendWeaponHackAlert", mp.players.local.weapon);
            lastHackedWeaponMessageSent = Date.now();
        }
    }
}
mp.events.add('TwoSecondsEvent', () => {
    checkHackedWeapons();
});