var global = require('./globals');
const player = mp.players.local;

mp.events.add("TeleportEditor:TeleportToPosition", (x, y, z, h) => {
    player.position = new mp.Vector3(x, y, z);
    player.heading = h;
});
mp.events.add("TeleportEditor:ToggleNoclip", () => {
    mp.events.callRemote("TPEditor:RequestNoclip");
});