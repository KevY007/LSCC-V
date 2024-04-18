var global = require('./globals');

// Weapon Hacks
let lastHackedWeaponMessageSent = 0;
function checkHackedWeapons(){
    const weaponsJSON = mp.players.local.getVariable("AC_WeaponData");
    if(!weaponsJSON){
        if(mp.players.local.weapon !== 2725352035 && mp.players.local.weapon !== 0){
            if(Date.now() - lastHackedWeaponMessageSent > 15000){
                mp.events.callRemote("AC_SendWeaponHackAlert", mp.players.local.weapon.toString(), 0);
                mp.events.callRemote("ServerConsoleOutput", `(${new Date().toUTCString()}) [AntiCheat - Weapon Hacks] ${mp.players.local.name} might be hacking. Weapon data: ${mp.players.local.getVariable("AC_WeaponData")} | Hacked gun: ${mp.players.local.weapon.toString()} | Type: 0`);
                lastHackedWeaponMessageSent = Date.now();
            }
        }
        return;
    }
    const weaponsArray = JSON.parse(weaponsJSON);

    if(!weaponsArray.includes(mp.players.local.weapon)){
        if(mp.players.local.weapon !== 2725352035 && mp.players.local.weapon !== 0){
            if(Date.now() - lastHackedWeaponMessageSent > 15000){
                mp.events.callRemote("AC_SendWeaponHackAlert", mp.players.local.weapon.toString(), 1);
                mp.events.callRemote("ServerConsoleOutput", `(${new Date().toUTCString()}) [AntiCheat - Weapon Hacks] ${mp.players.local.name} might be hacking. Weapon data: ${mp.players.local.getVariable("AC_WeaponData")} | Hacked gun: ${mp.players.local.weapon.toString()} | Type: 1`);
                lastHackedWeaponMessageSent = Date.now();
            }
        }
      
    }
}
mp.events.add('TwoSecondsEvent', () => {
    checkHackedWeapons();
});

mp.events.addDataHandler('AC_WeaponData', function(entity, value, oldValue){
    mp.events.callRemote("ServerConsoleOutput", `(${new Date().toUTCString()}) [AntiCheat - Weapon Hacks] ${mp.players.local.name}'s weapon data is now: ${value} | Old data: ${oldValue}`);
});


// Bullet Tracing for Aimbot checking
var bulletLineData = [];
mp.events.add('render', () => {
    for(let i = 0 ; i < bulletLineData.length ; i++){
        const line = bulletLineData[i];
        if(!line.end || !line.start) continue;

        if(line.hit === true)
            mp.game.graphics.drawLine(line.start.x, line.start.y, line.start.z, line.end.x, line.end.y, line.end.z, 0, 255, 0, 200);
        else
            mp.game.graphics.drawLine(line.start.x, line.start.y, line.start.z, line.end.x, line.end.y, line.end.z, 255, 0, 0, 200);
    }
});
mp.events.add('BulletTrace:ClearLines', () => {
    bulletLineData = [];
})
mp.events.add("BulletTrace:AddLine", (entity, x, y, z, hit) => {
    const targetPosition = new mp.Vector3(x, y, z);

    const playerPosition = entity.getWorldPositionOfBone(entity.getBoneIndexByName("BONETAG_R_HAND"));  
    bulletLineData.push({start:playerPosition, end:targetPosition, hit:hit});
});