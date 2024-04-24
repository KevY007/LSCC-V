var global = require('./globals');
const player = mp.players.local;

var TeleportEditorCEF = null;

mp.events.add("TeleportEditor:Open", (editTp) => {
    TeleportEditorCEF = mp.browsers.new("package://TeleportManager/CEF/index.html");
    TeleportEditorCEF.active = true;
    TeleportEditorCEF.call("LoadTeleports", JSON.stringify(global.TeleportData));
    if(editTp.length === 0){
        TeleportEditorCEF.call("SetEditTeleport", editTp);
    }

    mp.gui.cursor.show(true, true);
    global.TeleportEditorOpen = true;
    mp.events.callLocal("", false);
});
mp.events.add("TeleportEditor:SearchResultAction", (isEdit, tpID) => {
    const tp = global.TeleportData.find(t => t.SQL_ID == tpID);
    if(!tp) return;

    if(!isEdit){
        player.position = new mp.Vector3(tp.FromX, tp.FromY, tp.FromZ);
        player.setHeading(tp.FromHeading);
        return;
    }
    TeleportEditorCEF.call("SetEditTeleport", JSON.stringify(tp));
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

mp.events.add("TeleportEditor:CreateNewTeleport", (tpSettings) => {
    const tpSet = JSON.parse(tpSettings);
    let isOutside = (tpSet.Dimension === "Outside");
    let iplList = '';
    tpSet.IPLArray.map((ipl) => { iplList += (ipl + ",")});
    iplList = removeTrailingComma(iplList);

    mp.events.callRemote("TPEditor:CreateTeleport", tpSet.Name, tpSet.BlipID, tpSet.SideEntrance, isOutside, iplList, tpSet.FreezeTime,
    tpSet.FromPosition.x, tpSet.FromPosition.y, tpSet.FromPosition.z, tpSet.FromPosition.h, 
    tpSet.ToPosition.x, tpSet.ToPosition.y, tpSet.ToPosition.z, tpSet.ToPosition.h);    
});
mp.events.add("TeleportEditor:EditTeleport", (tpID, tpSettings) => {
    const tpSet = JSON.parse(tpSettings);
    let isOutside = (tpSet.Dimension === "Outside");
    let iplList = '';
    tpSet.IPLArray.map((ipl) => { iplList += (ipl + ",")});
    iplList = removeTrailingComma(iplList);

    mp.events.callRemote("TPEditor:EditTeleport", tpID, tpSet.Name, tpSet.BlipID, tpSet.SideEntrance, isOutside, iplList, tpSet.FreezeTime,
    tpSet.FromPosition.x, tpSet.FromPosition.y, tpSet.FromPosition.z, tpSet.FromPosition.h, 
    tpSet.ToPosition.x, tpSet.ToPosition.y, tpSet.ToPosition.z, tpSet.ToPosition.h);    
});
mp.events.add("TeleportEditor:DeleteTeleport", (tpID) => {
    mp.events.callRemote("TPEditor:DeleteTeleport", tpID);
});

function removeTrailingComma(inputString) {
    // Use a regular expression to replace the trailing comma with an empty string
    return inputString.replace(/,$/, '');
  }