var global = require('./globals');

var BLOOD_DECALS = [];
var IsLocalPlayerBleeding = false;
const BloodElements = [
    1010,
    1015,
    1017,
    1110
];

mp.events.add("Bleeding:ClearAllDecals", () => {
   for(let i = 0 ; i < BLOOD_DECALS.length ; i++){
    if(!mp.game.graphics.isDecalAlive(BLOOD_DECALS[i])) continue;

    mp.game.graphics.removeDecal(BLOOD_DECALS[i]);
   } 
   BLOOD_DECALS = [];
});
mp.events.add("Bleeding:AddBloodDecal", (x, y, z, preciseSpawn) => {
    const element = BloodElements[getRandomInt(0, BloodElements.length)];
    if(!element) return;

    const realX = (preciseSpawn ? x : (x + (Math.random()-0.5))); // spawn around player if precise spawn is off
    const realY = (preciseSpawn ? y : (y + (Math.random()-0.5))); // spawn around player if precise spawn is off
    const size = (0.4 + Math.random());

    const getGroundZ = mp.game.gameplay.getGroundZFor3dCoord(realX, realY, z, parseFloat(0), false);

    const decalID = mp.game.graphics.addDecal(element, realX, realY, getGroundZ, 0, 0, -1, 0, 1, 0, size, size*2, 255, 0.1, 0.1, 1.0, 150.0, false, false, false);
    BLOOD_DECALS.push(decalID);
});
mp.events.add("Bleeding:SetActive", (toggle) => {
    IsLocalPlayerBleeding = toggle;
});

mp.events.add("TwoSecondsEvent", () => {
    if(!IsLocalPlayerBleeding) return;

    mp.events.callRemote("Bleeding:SpawnBloodDecal");
});




function getRandomInt(min, max) {
    min = Math.ceil(min);
    max = Math.floor(max);
    return Math.floor(Math.random() * (max - min) + min); //The maximum is exclusive and the minimum is inclusive
  }