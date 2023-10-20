var global = require('./globals');
const player = mp.players.local;

var TeleportEditorCEF = null;

mp.events.add("TeleportEditor:Open", () => {
    TeleportEditorCEF = mp.browsers.new("package://TeleportManager/CEF/index.html");
    TeleportEditorCEF.active = true;

    mp.gui.cursor.show(true, true);
    global.TeleportEditorOpen = true;
    mp.events.callLocal("ToggleHud", false);
});
mp.events.add("TeleportEditor:Close", () => {
    if(!TeleportEditorCEF) return;

    TeleportEditorCEF.destroy();
    mp.gui.cursor.show(false, false);
    global.TeleportEditorOpen = false;
    mp.events.callLocal("ToggleHud", true);
});
mp.events.add("TeleportEditor:RequestIPL", (iplName) => {
    mp.game.streaming.requestIpl(iplName);
});
mp.events.add("TeleportEditor:UnloadIPL", (name) => {
    mp.game.streaming.removeIpl(name);
});
mp.events.add("TeleportEditor:TeleportToPosition", (x, y, z, h) => {
    mp.players.local.position = new mp.Vector3(Number(x), Number(y), Number(z));
    mp.players.local.setHeading(h);
});
mp.events.add("TeleportEditor:ToggleNoclip", () => {
    mp.events.callRemote("TPEditor:RequestNoclip");
});

mp.events.add("TeleportEditor:RequestSetFromPosition", () => {
    if(!TeleportEditorCEF) return;

    TeleportEditorCEF.call("SetFromPosition", mp.players.local.position.x.toString(), mp.players.local.position.y.toString(), mp.players.local.position.z.toString(), mp.players.local.getHeading().toString());
});
mp.events.add("TeleportEditor:RequestSetToPosition", () => {
    if(!TeleportEditorCEF) return;

    TeleportEditorCEF.call("SetToPosition", mp.players.local.position.x.toString(), mp.players.local.position.y.toString(), mp.players.local.position.z.toString(), mp.players.local.getHeading().toString());
});