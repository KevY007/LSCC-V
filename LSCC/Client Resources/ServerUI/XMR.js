var RadioToggled = false;

mp.events.add("XMR_LoadRecentStations", (stationData) => {
    const stations = JSON.parse(stationData);
    const data = stations.map(station => ({
        id:station.SQL_ID,
        name:station.Name,
        stream:station.StreamURL,
        cover:station.IconURL,
        category:station.CategoryID
    }));

    if(ServerUI){
        ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.radio.recentStations = ${JSON.stringify(data)}`);
    }
});

mp.events.add("XMR_SetStationList", (stationData, categoryData) => {
    const stations = JSON.parse(stationData);
    const categories = JSON.parse(categoryData);
    const data = stations.map(station => ({
        id:station.SQL_ID,
        name:station.Name,
        stream:station.StreamURL,
        cover:station.IconURL,
        category:station.CategoryID
    }));
    const cData = categories.map(category => ({
        value:category.SQL_ID,
        label:category.Name,
    }));
    cData.unshift({value:-1, label:"All"});

    if(ServerUI){
        ServerUI.execute(`gm.$refs.menus.$refs.radioSelector.categories = ${JSON.stringify(cData)}`);
        ServerUI.execute(`gm.$refs.menus.$refs.radioSelector.radios = ${JSON.stringify(data)}`);
        //ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.radio.recentStations = ${JSON.stringify(data)}`);
    }
});

mp.events.add("StationPicker_Toggle", (toggle) => {
    if(!ServerUI) return;

    StationPickerToggled = toggle;
    mp.gui.cursor.show(toggle, toggle);
    mp.game.invoke("0xFC695459D4D0E219", 0.5, 0.5);
    ServerUI.execute(`gm.$refs.menus.$refs.radioSelector.enabled = ${toggle}`);
});

mp.events.add("XMR_Toggle", (toggle) => {
    if(!ServerUI) return;

    RadioToggled = toggle;
    ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.radio.enabled = ${toggle.toString()}`);

    if(mp.players.local.vehicle != undefined && mp.players.local.isInAnyVehicle(false)){
        ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.radio.recentStationsEnabled = ${(mp.players.local.seat === -1).toString()};`);
    }
    if(toggle){
        ServerUI.execute(`gm.$refs.hud.$refs.bottomRight.$refs.speedo.enabled = false;`);
    }
    else{
        if(mp.storage.data.menu.Speedo == true && mp.players.local.isInAnyVehicle(true))
        {
            ServerUI.execute(`gm.$refs.hud.$refs.bottomRight.$refs.speedo.enabled = true;`);
        }
    }
});
mp.events.add("XMR_ToggleOpposite", () => {
    RadioToggled = !RadioToggled;
    ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.radio.enabled = ${RadioToggled.toString()}`);

    if(mp.players.local.vehicle != undefined && mp.players.local.isInAnyVehicle(false)){
        ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.radio.recentStationsEnabled = ${(mp.players.local.seat === -1).toString()};`);
    }
});

mp.events.add("RequestPlayRadio", (id) => {
    mp.events.callLocal("StationPicker_Toggle", false);
    mp.events.callRemote("XMR_RequestPlayStation", id);
});

mp.events.add("AddRecentStation", (id) => {
    if(mp.storage.data.radio === undefined){
        mp.storage.data.radio = {recents:[]};
    }
    if(mp.storage.data.radio.recents === undefined){
        mp.storage.data.radio.recents = [];
    }
    if(mp.storage.data.radio.recents.includes(id)){
        mp.storage.data.radio.recents.splice(mp.storage.data.radio.recents.indexOf(id), 1);
    }
    mp.storage.data.radio.recents.unshift(id);
    if(mp.storage.data.radio.recents >= 5){
        mp.storage.data.radio.recents.pop();
    }
    mp.storage.flush();

    mp.events.callRemote("XMR_RequestRecentStations", JSON.stringify(mp.storage.data.radio.recents));
});

mp.events.add("radiopicker_close", () => {
    mp.events.callLocal("StationPicker_Toggle", false);
});

mp.events.add("XMR_StopStationForPlayer", () => {
    if(!ServerUI) return;

    ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.radio.stop();`);
    mp.events.callLocal("XMR_Toggle", false);
    mp.events.callLocal("StationPicker_Toggle", false);
});
mp.events.add("XMR_PlayStationForPlayer", (id, name, stream, icon) => {
    if(!ServerUI) return;

    mp.events.callLocal("AddRecentStation", id);
    mp.events.callLocal("XMR_Toggle", true);
    ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.radio.currentStation = {
        name:'${name}',
        cover:'${icon}'
    }`);

    if(mp.storage.data.radio !== undefined && mp.storage.data.radio.volume !== undefined)
        ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.radio.setVolumeServer(${mp.storage.data.radio.volume})`);
    else{
        ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.radio.setVolumeServer(0.1)`);
    }
    
    ServerUI.execute(`gm.$refs.hud.$refs.base.$refs.radio.play('${stream}')`);
});

mp.events.add("playerLeaveVehicle", (vehicle, seat) => {
    mp.events.callLocal("XMR_StopStationForPlayer");
}) 
mp.events.add("playerEnterVehicle", (vehicle, seat) => {
    if(vehicle.getVariable("RadioStation") !== undefined){
        mp.events.callRemote("XMR_RequestPlayStationUnsynced", vehicle.getVariable("RadioStation"));
    }    
    else{
        if(seat === -1){
            if(mp.storage.data.menu.XMRAutoPlay !== undefined && mp.storage.data.menu.XMRAutoPlay == true){
                if(mp.storage.data.radio !== undefined && mp.storage.data.radio.recents !== undefined && mp.storage.data.radio.recents.length > 0){
                    mp.events.callRemote("XMR_RequestPlayStation", mp.storage.data.radio.recents[0]);
                }
            }
        }
    }
});

mp.events.add("XMR_ChangeVolume", (volume) => {
    mp.storage.data.radio.volume = volume;
    mp.storage.flush();
});