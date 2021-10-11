const Natives = {
    IS_RADAR_HIDDEN: "0x157F93B036700462",
    IS_RADAR_ENABLED: "0xAF754F20EB5CD51A",
    SET_TEXT_OUTLINE: "0x2513DFB0FB8400FE"
};

let minimap = {};

var resolution = mp.game.graphics.getScreenActiveResolution(0,0);
let lastSpecHPChange = Date.now();

var miniHUDVars = {
    bottom: 0, 
    left: 0,
    visible: false,
    voice: "Standby",
    direction: "N/A",
    street: "N/A",
    zone: "N/A",
    jailed: false,
    jail: "N/A",

    // SPEC VARS
    spec: false,
    ping: 0,
    packetloss: 0.0,
};

var showReticule = true;

let speedo = false;
let speedoMax = 0.0;
let speedoCur = 0.0;

// Scoreboard
let updater = null;
scoreboardToggled = false;
scoreboardCursor = false;
let scoreboardBinded = false;

// StackOverflow cause I'm too lazy to do this myself and don't wanna type getvariable etc etc every time
function firstLetterUpper(string) {
    return string.charAt(0).toUpperCase() + string.slice(1);
  }

// https://github.com/glitchdetector/fivem-minimap-anchor
function getMinimapAnchor() {
    let sfX = 1.0 / 20.0;
    let sfY = 1.0 / 20.0;
    let safeZone = mp.game.graphics.getSafeZoneSize();
    let aspectRatio = mp.game.graphics.getScreenAspectRatio(false);
    let resolution = mp.game.graphics.getScreenActiveResolution(0, 0);
    let scaleX = 1.0 / resolution.x;
    let scaleY = 1.0 / resolution.y;

    let minimap = {
        width: scaleX * (resolution.x / (4 * aspectRatio)),
        height: scaleY * (resolution.y / 5.674),
        scaleX: scaleX,
        scaleY: scaleY,
        leftX: scaleX * (resolution.x * (sfX * (Math.abs(safeZone - 1.0) * 10))),
        bottomY: 1.0 - scaleY * (resolution.y * (sfY * (Math.abs(safeZone - 1.0) * 10))),
    };

    minimap.rightX = minimap.leftX + minimap.width;
    minimap.topY = minimap.bottomY - minimap.height;
    return minimap;
}



function drawText(text, drawXY, font, color, scale, alignRight = false) {
    mp.game.ui.setTextEntry("STRING");
    mp.game.ui.addTextComponentSubstringPlayerName(text);
    mp.game.ui.setTextFont(font);
    mp.game.ui.setTextScale(scale, scale);
    mp.game.ui.setTextColour(color[0], color[1], color[2], color[3]);
    mp.game.invoke(Natives.SET_TEXT_OUTLINE);

    if (alignRight) {
        mp.game.ui.setTextRightJustify(true);
        mp.game.ui.setTextWrap(0, drawXY[0]);
    }

    mp.game.ui.drawText(drawXY[0], drawXY[1]);
}

function TimeFormat(time)
{   
    var hrs = ~~(time / 3600);
    var mins = ~~((time % 3600) / 60);
    var secs = ~~time % 60;

    var ret = "";

    if (hrs > 0) {
        ret += "" + hrs + ":" + (mins < 10 ? "0" : "");
    }

    ret += "" + mins + ":" + (secs < 10 ? "0" : "");
    ret += "" + secs;
    return ret;
}

mp.keys.bind(
    0x76,
    false,
    () => {
        if(menuToggled !== undefined && menuToggled) return;
        if(!chatStatus && 
            !(mp.players.local.LoggedIn === undefined || mp.players.local.LoggedIn == false)) 
        {
            mp.events.callLocal("ToggleHud", !showHud);
			
			if (mp.keys.isDown(16) === true) 
            { // Shift is pressed
                renderNametags = showHud;
            }
        }
    }
);

mp.events.add('ToggleNametags', (state) => {
    renderNametags = state;
});

/*
 gm.topRight = false;
      topRightInfo
      killFeed
      gameProgress
      hud
      topRight
*/

mp.events.add('RestartHUD', () => {
    if(UIHud !== null)
    {
        UIHud.destroy();
        UIHud = null;
    }

    UIHud = mp.browsers.new("package://UIHud/index.html");
    UIHud.execute(`gm.hud = false;`);
});

let createdTooltips = []; // to keep track of created tooltips / preventing duplicates

mp.events.add('AddTooltip', (key, action, team) => {
    
    if(UIHud == null) return;
    if(createdTooltips.indexOf(key) != -1) return;
    if(mp.storage.data.menu.tooltips !== true) return;

    let color = "#383838FF";
    switch(team){
        case 0: color = "#ffae00ff";
        break;
        case 1: color = "#0097ffff";
        break;
        default: color = "#383838FF";
    }

    createdTooltips.push(key);

    UIHud.execute(`gm.$refs.TopRight.$refs.Tooltips.tips.push({
        key: "${key}",
        action: "${action}",
        color: "${color}"
    })`);
});
mp.events.add('RemoveTooltip', (key) => {
    if(UIHud == null) return;
    if(createdTooltips.indexOf(key) == -1) return;

    UIHud.execute(`if(gm.$refs.TopRight.$refs.Tooltips.tips.indexOf(gm.$refs.TopRight.$refs.Tooltips.tips.filter(function(el){return el.key == "${key}"})[0]) != -1){
        gm.$refs.TopRight.$refs.Tooltips.tips.splice(gm.$refs.TopRight.$refs.Tooltips.tips.indexOf(gm.$refs.TopRight.$refs.Tooltips.tips.filter(function(el){return el.key == "${key}"})[0]), 1);        
    }`);
    createdTooltips.splice(createdTooltips.indexOf(key), 1);
});

let keyTrunk = true; // true = key trunk, false = key seatbelt displayed

mp.events.add('ToggleTooltips', (status) =>
{
    mp.storage.data.menu.tooltips = status;

    if(status)
    {
        mp.events.callLocal("AddTooltip", "M", "Menu", -1);
        mp.events.callLocal("AddTooltip", String.fromCharCode(curVoiceBind), "Voice Chat", -1);
    }
    else {

        createdTooltips.forEach(key => {
            UIHud.execute(`if(gm.$refs.TopRight.$refs.Tooltips.tips.indexOf(gm.$refs.TopRight.$refs.Tooltips.tips.filter(function(el){return el.key == "${key}"})[0]) != -1){
                gm.$refs.TopRight.$refs.Tooltips.tips.splice(gm.$refs.TopRight.$refs.Tooltips.tips.indexOf(gm.$refs.TopRight.$refs.Tooltips.tips.filter(function(el){return el.key == "${key}"})[0]), 1);        
            }`);
        });

        createdTooltips = [];
    }
});

mp.events.add("OneSecondEvent", () => {

    if(mp.storage.data.menu.tooltips == true)
    {
        if(mp.players.local.vehicle){
            if(mp.players.local.Team == 1)
                if(createdTooltips.indexOf("B") == -1) mp.events.callLocal("AddTooltip", "B", "Silent Siren", 1);
                
            if(createdTooltips.indexOf("Y") == -1) mp.events.callLocal("AddTooltip", "Y", "Engine", -1);
            if(createdTooltips.indexOf("L") == -1) mp.events.callLocal("AddTooltip", "L", "Lock", -1);
        }
        else{
            if(createdTooltips.indexOf("Y") != -1) mp.events.callLocal("RemoveTooltip", "Y");
            if(createdTooltips.indexOf("L") != -1) mp.events.callLocal("RemoveTooltip", "L");
            if(createdTooltips.indexOf("B") != -1) mp.events.callLocal("RemoveTooltip", "B");     
        }
        if(mp.players.local.Team == 1){
            if(createdTooltips.indexOf(String.fromCharCode(curGPSBind)) == -1) 
            {
                mp.events.callLocal("AddTooltip", String.fromCharCode(curGPSBind), "GPS", 1);
            }
        }
        else{
            if(createdTooltips.indexOf("Z") != -1) mp.events.callLocal("RemoveTooltip", "Z");
            if(createdTooltips.indexOf("B") != -1) mp.events.callLocal("RemoveTooltip", "B");     
        }

        if(!mp.players.local.isInAnyVehicle(false)) 
        {
            if(createdTooltips.indexOf("J") != -1 && !keyTrunk)
            {
                mp.events.callLocal("RemoveTooltip", "J");
                keyTrunk = true;
            }

            let veh = getClosestVehicle(mp.players.local.position);

            if(veh !== undefined && mp.vehicles.exists(veh.vehicle) && veh.vehicle.handle !== 0)
            {
                let sidebones = [
                    "bonnet",
                    "boot"
                ];
          
                let closestHandle = "";
                let closestDist = 100.0;
                sidebones.forEach((side) => {
                    let bone = veh.vehicle.getBoneIndexByName(side);
                    if(bone != -1)
                    {
                        let dist = calcDist(mp.players.local.position, veh.vehicle.getWorldPositionOfBone(bone));
                        if(dist < closestDist)
                        {
                            closestDist = dist;
                            closestHandle = side;
                        }
                    }
                });
        
                if(closestDist <= 3.5)
                {
                    if((veh.vehicle.getVariable("Locked") === undefined || veh.vehicle.getVariable("Locked") == false) || 
                    (veh.vehicle.getVariable("Locked") == true && (mp.players.local.weapon == 0x84BD7BFD || mp.players.local.weapon == 2227010557)))
                    {
                        if(createdTooltips.indexOf("J") == -1) 
                        {
                            if(closestHandle == "boot") mp.events.callLocal("AddTooltip", "J", "Trunk", -1);
                            else if(closestHandle == "bonnet") mp.events.callLocal("AddTooltip", "J", "Bonnet", -1);
                        }
                    }
                    else if(createdTooltips.indexOf("J") != -1) mp.events.callLocal("RemoveTooltip", "J");
                }
                else if(createdTooltips.indexOf("J") != -1) mp.events.callLocal("RemoveTooltip", "J");
            }
            else if(createdTooltips.indexOf("J") != -1) mp.events.callLocal("RemoveTooltip", "J");
        }
        else
        {
            if((createdTooltips.indexOf("J") != -1 && keyTrunk) || createdTooltips.indexOf("J") == -1)
            {
                mp.events.callLocal("RemoveTooltip", "J");
                
                if(mp.players.local.vehicle.getClass() != 8 && mp.players.local.vehicle.model != mp.game.joaat('policeb'))
                {
                    mp.events.callLocal("AddTooltip", "J", "Seatbelt", -1);
                }

                keyTrunk = false;
            }
        }    
    }
});

mp.events.add('browserDomReady', (browser) => {
    if(UIHud != null && browser == UIHud)
    {
        // Set vars that aren't reset on login and shit. Basically init().
        UIHud.execute(`gm.topRightInfo = ${mp.storage.data.menu.topRightInfo}; gm.killFeed = ${mp.storage.data.menu.killFeed};`);
        UIHud.execute(`gm.$refs.MiniHud.items = [];`);

        // Initialize miniHUD
        miniHUDVars["bottom"] = ((resolution.y - (getMinimapAnchor().bottomY * resolution.y)) + 0);
        miniHUDVars["left"] = ((getMinimapAnchor().rightX * resolution.x) + 10) + ((mp.storage.data.menu.alwaysExpandedMap == true) ? 162 : 0);

        UIHud.execute(`gm.$refs.MiniHud.left = "${miniHUDVars["left"]}px";`);
        UIHud.execute(`gm.$refs.MiniHud.bottom = "${miniHUDVars["bottom"]}px";`);  

        // SPECTATE VARS
        UIHud.execute(`gm.$refs.MiniHud.items.push({ 
            name: "ping",
            icon: "fa-solid fa-stopwatch",
            init: "0 ms",
            color: "#ff8084"
        });`);

        UIHud.execute(`gm.$refs.MiniHud.items.push({ 
            name: "packetloss",
            icon: "fa-solid fa-minus",
            init: "0.0%",
            color: "#f7aa45"
        });`);
        //
        
        UIHud.execute(`gm.$refs.MiniHud.items.push({
            name: "jail",
            icon: "fa-solid fa-ban",
            init: "N/A",
            color: "#c73228"
        });`);

        UIHud.execute(`gm.$refs.MiniHud.items.push({
            name: "visible",
            icon: "fa-solid fa-globe",
            init: "Visible",
            color: "#65e9fa"
        });`);

        UIHud.execute(`gm.$refs.MiniHud.items.push({
            name: "voice",
            icon: "fa-solid fa-microphone",
            init: "Standby",
            color: "#c365fa"
        });`);

        UIHud.execute(`gm.$refs.MiniHud.items.push({
            name: "direction",
            icon: "fa-solid fa-compass",
            init: "N/A",
            color: "#c3fa65"
        });`);

        UIHud.execute(`gm.$refs.MiniHud.items.push({
            name: "street",
            icon: "fa-solid fa-location-dot",
            init: "N/A",
            color: "#fae165"
        });`);

        UIHud.execute(`gm.$refs.MiniHud.items.push({
            name: "zone",
            icon: "fa-solid fa-location-crosshairs",
            init: "N/A",
            color: "#fa65b0"
        });`);

        UIHud.execute(`gm.$refs.MiniHud.$refs.jail.show = ${miniHUDVars["jailed"]};`);
        UIHud.execute(`gm.$refs.MiniHud.$refs.visible.show = false;`);
        UIHud.execute(`gm.$refs.MiniHud.$refs.ping.show = false;`);
        UIHud.execute(`gm.$refs.MiniHud.$refs.packetloss.show = false;`);

        mp.events.callLocal("ToggleTooltips", mp.storage.data.menu.tooltips);
    }
});

mp.events.add('ToggleHud', (toggle) => {
    showHud = toggle;
    
    if(UIHud != null)
    {
        UIHud.execute(`gm.hud = ${toggle};`);
    }

    mp.gui.chat.show(showHud);
    mp.game.ui.displayRadar(showHud);
});

mp.keys.bind(0x75, false, () => {
    showReticule = !showReticule;
});

mp.events.add('ResetKillFeed', () => {
    if(UIHud != null)
    {
        UIHud.execute(`gm.killFeed = false;`);
        UIHud.execute(`gm.killFeed = ${mp.storage.data.menu.killFeed};`);
    }
});

mp.events.add("pushToKillFeed", (killer, killerColor, weapon, victim, victimColor) => {
    if(UIHud != null) UIHud.execute(`gm.$refs.TopRight.$refs.KillFeed.add('${killer}', '${killerColor}', '${weapon}', '${victim}', '${victimColor}');`)
});

mp.events.add("pushToKillFeed2", (victim, color, cause) => {
    if(UIHud != null) UIHud.execute(`gm.$refs.TopRight.$refs.KillFeed.add("${victim}", "${color}", "${cause}");`)
});

mp.events.add("UpdateRateEvent", () => {

    // only do stuff if radar is enabled and visible
    let streetName = "N/A";
    let zoneName = "N/A";
    if (mp.game.invoke(Natives.IS_RADAR_ENABLED) && !mp.game.invoke(Natives.IS_RADAR_HIDDEN)) {
        minimap = getMinimapAnchor();

        const position = mp.players.local.position;
        let getStreet = mp.game.pathfind.getStreetNameAtCoord(position.x, position.y, position.z, 0, 0);

        zoneName = mp.game.ui.getLabelText(mp.game.zone.getNameOfZone(position.x, position.y, position.z));
        streetName = mp.game.ui.getStreetNameFromHashKey(getStreet.streetName);
    }
    
    if(UIHud != null && (!(mp.players.local.LoggedIn === undefined || mp.players.local.LoggedIn == false || mp.players.local.Lobby === undefined)))
    {
        UIHud.execute(`gm.$refs.TopRight.$refs.Info.points = ${mp.players.local.getVariable("Points")};`);
        
        let lobbyStr = "";
        
        let lobbyPlayers = 0;
        mp.players.forEach((pl) => {
            if(!(pl.LoggedIn === undefined || pl.LoggedIn == false || pl.Lobby === undefined))
            {
                if(pl.Lobby == mp.players.local.Lobby) lobbyPlayers++;
            }
        });

        if(mp.players.local.Lobby == -2) lobbyStr = "Jail";
        if(mp.players.local.Lobby == -3) lobbyStr = "Freeroam";
        if(mp.players.local.Lobby == -4) lobbyStr = "Derby";
        if(mp.players.local.Lobby == -5) lobbyStr = "Customizer";
        if(mp.players.local.Lobby > -1) lobbyStr = mp.players.local.Lobby.toString();
        
        UIHud.execute(`gm.$refs.TopRight.$refs.Info.lobbyID = "${lobbyStr}";`);

        let camera = mp.cameras.new("gameplay");
        let cameraDirection = camera.getDirection();
        let dirtext = "N/A";

        if (0.3 < cameraDirection.x && 0.3 < cameraDirection.y) {
            dirtext = "NE";
        }
        else if (cameraDirection.x < -0.3 && 0.3 < cameraDirection.y) {
            dirtext = "NW";
        }
        else if (0.3 < cameraDirection.x && cameraDirection.y < -0.3) {
            dirtext = "SE";
        }
        else if (cameraDirection.x < -0.3 && cameraDirection.y < -0.3) {
            dirtext = "SW";
        }
        else if (-0.3 < cameraDirection.x && cameraDirection.x < 0.3 && cameraDirection.y < -0.3) {
            dirtext = "S";
        }
        else if (cameraDirection.x < -0.3 && -0.3 < cameraDirection.y && cameraDirection.y < 0.3) {
            dirtext = "W";
        }
        else if (0.3 < cameraDirection.x && -0.3 < cameraDirection.y && cameraDirection.y < 0.3) {
            dirtext = "E";
        }
        else if (-0.3 < cameraDirection.x && cameraDirection.x < 0.3 && cameraDirection.y > 0.3) {
            dirtext = "N";
        }
        camera.destroy(true);

        let voiceString = "Disabled";
                
        if(mp.players.local.getVariable("VoiceMode") == 0)
        {
            voiceString = mp.voiceChat.muted ? "Standby" : "Speaking";
        }
        else if(mp.players.local.getVariable("VoiceMode") == 1)
        {
            voiceString = mp.voiceChat.muted ? "Standby (Team)" : "Speaking (Team)";
        }
        else if(mp.players.local.getVariable("VoiceMode") == 2)
        {
            voiceString = mp.voiceChat.muted ? "Standby (Squad)" : "Speaking (Squad)";
        }

        if(currentMuteTime >= 1)
        {
            voiceString = `Muted for ${TimeFormat(currentMuteTime)}`;
        }

        let jailString = "N/A";
        if(currentJailTime >= 1)
        {
            jailString = `Jailed for ${TimeFormat(currentJailTime)}`;
        }

        if(mp.players.local.getVariable("VisibleBlip") != miniHUDVars["visible"])
        {
            miniHUDVars["visible"] = mp.players.local.getVariable("VisibleBlip");
            UIHud.execute(`gm.$refs.MiniHud.$refs.visible.show = ${miniHUDVars["visible"]};`);
        }
        if(voiceString != miniHUDVars["voice"])
        {
            miniHUDVars["voice"] = voiceString;
            UIHud.execute(`gm.$refs.MiniHud.$refs.voice.value = "${miniHUDVars["voice"]}";`);
        }
        if(dirtext != miniHUDVars["direction"])
        {
            miniHUDVars["direction"] = dirtext;
            UIHud.execute(`gm.$refs.MiniHud.$refs.direction.value = "${miniHUDVars["direction"]}";`);
        }
        if(streetName != miniHUDVars["street"])
        {
            miniHUDVars["street"] = streetName;
            UIHud.execute(`gm.$refs.MiniHud.$refs.street.value = "${miniHUDVars["street"]}";`);
        }
        if(zoneName != miniHUDVars["zone"])
        {
            miniHUDVars["zone"] = zoneName;
            UIHud.execute(`gm.$refs.MiniHud.$refs.zone.value = "${miniHUDVars["zone"]}";`);
        }

        if(currentJailTime >= 1)
        {
            if(miniHUDVars["jailed"] != true)
            {
                miniHUDVars["jailed"] = true;
                UIHud.execute(`gm.$refs.MiniHud.$refs.jail.show = ${miniHUDVars["jailed"]};`);
            }
            miniHUDVars["jail"] = jailString;
            UIHud.execute(`gm.$refs.MiniHud.$refs.jail.value = "${miniHUDVars["jail"]}";`);
        }
        else if(miniHUDVars["jailed"] == true)
        {
            miniHUDVars["jailed"] = false;
            UIHud.execute(`gm.$refs.MiniHud.$refs.jail.show = ${miniHUDVars["jailed"]};`);
        }

        if (specState == true && specCam !== null && specTarget != null && mp.players.exists(specTarget)) 
        {
            if(miniHUDVars["spec"] == false)
            {
                miniHUDVars["spec"] = true;
                UIHud.execute(`gm.$refs.MiniHud.$refs.ping.show = true;`);
                UIHud.execute(`gm.$refs.MiniHud.$refs.packetloss.show = true;`);

                UIHud.execute(`gm.$refs.MiniHud.$refs.direction.show = false;`);
            }
        
            if(miniHUDVars["packetloss"] != specTarget.getVariable("Packetloss").toFixed(2))
            {
                miniHUDVars["packetloss"] = specTarget.getVariable("Packetloss").toFixed(2);
                UIHud.execute(`gm.$refs.MiniHud.$refs.packetloss.value = "${miniHUDVars["packetloss"]}%"`);
            }
            if(miniHUDVars["ping"] != specTarget.getVariable("Ping"))
            {
                miniHUDVars["ping"] = specTarget.getVariable("Ping");
                UIHud.execute(`gm.$refs.MiniHud.$refs.ping.value = "${miniHUDVars["ping"]} ms"`);
            }
        }
        else if(miniHUDVars["spec"] == true) 
        {
            miniHUDVars["spec"] = false;
            UIHud.execute(`gm.$refs.MiniHud.$refs.ping.show = false;`);
            UIHud.execute(`gm.$refs.MiniHud.$refs.packetloss.show = false;`);

            UIHud.execute(`gm.$refs.MiniHud.$refs.direction.show = true;`);
        }
    }
});

function ToggleScoreboard()
{
    if (UIHud == null) return;

    if(scoreboardToggled)
    {
        scoreboardToggled = false;

        if(updater != null) clearInterval(updater);
        updater = null;

        mp.events.call("TogglePlayerChat", true);
        UIHud.execute(`gm.$refs.scoreboard.disableInput = true;`);
        UIHud.execute(`gm.scoreboard = false;`);

        if(scoreboardCursor)
        {
            scoreboardCursor = false;
            mp.gui.cursor.show(false, false);
        }
    }
    else
    {
        scoreboardToggled = true;
        mp.events.call("TogglePlayerChat", false);

        UIHud.execute(`gm.scoreboard = true;`);
        UIHud.execute(`gm.$refs.scoreboard.disableInput = true;`);

        updatePlayers();

        updater = setInterval(function()
        {
            if(chatStatus && scoreboardToggled)
            {
                ToggleScoreboard();
            }
            else if(!chatStatus && scoreboardToggled)
            {
                updatePlayers();
            }
        }, 1000);
    }
    return;
}

function updatePlayers()
{
    let updateString = `gm.$refs.scoreboard.setPlayers([`;

    let lobbyStr = '0';
    mp.players.forEach(player => {
        lobbyStr = player.Lobby.toString();
        if(player.Lobby == -2) lobbyStr = "J";
        if(player.Lobby == -3) lobbyStr = "F";
        if(player.Lobby == -4) lobbyStr = "D";
        if(player.Lobby == -5) lobbyStr = "C";

        updateString += `{id: '${lobbyStr}', 
        color: 'rgb(${player.getVariable("Color_R")}, ${player.getVariable("Color_G")}, ${player.getVariable("Color_B")})', 
        name: '${player.name}', level: ${player.getVariable("Level")}, ping: ${player.getVariable("Ping")}},`;
    });

    updateString += `]);`;

    if(UIHud != null) UIHud.execute(updateString);
    return;
}

mp.keys.bind(
    0x1B,
    false,
    () => {
        if(UIHud != null && scoreboardToggled) ToggleScoreboard();
    }
);

mp.keys.bind(0x71, false, function () {
    if(scoreboardToggled)
    {
        if(scoreboardCursor)
        {
            scoreboardCursor = false;
            mp.gui.cursor.show(false, false);
            UIHud.execute(`gm.$refs.scoreboard.disableInput = true;`);
            
            if(!scoreboardBinded) mp.events.callLocal('ScoreboardToggleTyping', false);
        }
        else
        {
            scoreboardCursor = true;
            mp.gui.cursor.show(true, true);
            UIHud.execute(`gm.$refs.scoreboard.disableInput = false;`);

            //if(scoreboardBinded) mp.events.callLocal('ScoreboardToggleTyping', true);
        }
    }
});

function ScoreboardKeyTrigger()
{
    if(mp.players.local.LoggedIn === undefined || mp.players.local.LoggedIn == false) return;
    if(Date.now() - lastChatToggle <= 500) return;
    if(menuToggled !== undefined && menuToggled) return;
    if(chatStatus && !scoreboardToggled) return;
    if(menuToggled) return;

    ToggleScoreboard();
}

mp.events.add('ScoreboardToggleTyping', (state) => {
    if(state && scoreboardBinded)
    {
        mp.keys.unbind(0x4F, false, ScoreboardKeyTrigger);
        scoreboardBinded = false;
        mp.events.callLocal("MenuToggleTyping", state);
    }
    else if(!state && !scoreboardBinded)
    {
        mp.keys.bind(0x4F, false, ScoreboardKeyTrigger);
        scoreboardBinded = true;
        mp.events.callLocal("MenuToggleTyping", state);
    }
});

mp.events.callLocal('ScoreboardToggleTyping', false);

mp.events.add("HUDError", (msg, icon) => {
    let insertString = `iziToast.error({`;
    insertString += `message: \`${msg}\`, `;
    insertString += `icon: "${icon}" `;
    insertString += "});";

    if(UIHud != null) UIHud.execute(insertString);
});

mp.events.add("HUDNotify", (msg, color, icon, close, timeout, orientation, displaymode) => {
    let insertString = `iziToast.show({`;
    insertString += `message: \`${msg}\`, `;
    insertString += `color: "${color}", `;
    insertString += `icon: "${icon}", `;
    insertString += `close: ${close.toString()}, `;
    insertString += `timeout: ${timeout},`;
    if(orientation.length > 1) insertString += `position: \`${orientation}\`, `;
    if(displaymode.length > 1) insertString += `displayMode: \`${displaymode}\`, `;
    insertString += "});";

    if(UIHud != null) UIHud.execute(insertString);
});
// bottomRight, bottomLeft, topRight, topLeft, topCenter, bottomCenter, center
mp.events.add("HUDNotify2", (title, msg, orientation, displaymode) => {
    let insertString = `iziToast.show({`;
    insertString += `title: \`${title}\`, `;
    insertString += `message: \`${msg}\`, `;
    if(orientation.length > 1) insertString += `position: \`${orientation}\`, `;
    if(displaymode.length > 1) insertString += `displayMode: \`${displaymode}\`, `;
    insertString += "});";

    if(UIHud != null) UIHud.execute(insertString);
});

mp.events.add("render", () => {
    
    mp.game.ui.hideHudComponentThisFrame(7); // area name
    mp.game.ui.hideHudComponentThisFrame(9); // street name
    mp.game.ui.hideHudComponentThisFrame(6); // vehicle name
    mp.game.ui.hideHudComponentThisFrame(8); // vehicle class
    mp.game.ui.hideHudComponentThisFrame(3); // cash
    mp.game.ui.hideHudComponentThisFrame(4); // mp_cash
    mp.game.ui.displayAmmoThisFrame(false); // HIDE AMMO

    if(!showReticule)
    {
        mp.game.ui.hideHudComponentThisFrame(14); // RETICULE
    }

    if(mp.players.local.LoggedIn !== undefined && mp.players.local.LoggedIn == true)
    {       
        if ((resolution.x != mp.game.graphics.getScreenActiveResolution(0,0).x || miniHUDVars["left"] != (((getMinimapAnchor().rightX * resolution.x) + 10)) + ((mp.storage.data.menu.alwaysExpandedMap == true) ? 162 : 0)) && UIHud != null)
        {
            resolution = mp.game.graphics.getScreenActiveResolution(0,0);
            
            miniHUDVars["bottom"] = ((resolution.y - (getMinimapAnchor().bottomY * resolution.y)) + 0);
            miniHUDVars["left"] = ((getMinimapAnchor().rightX * resolution.x) + 10) + ((mp.storage.data.menu.alwaysExpandedMap == true) ? 162 : 0);

            UIHud.execute(`gm.$refs.MiniHud.left = "${miniHUDVars["left"]}px";`);
            UIHud.execute(`gm.$refs.MiniHud.bottom = "${miniHUDVars["bottom"]}px";`);  
        }
        if (mp.storage.data.menu.alwaysExpandedMap == true)
        {
            mp.game.ui.setRadarBigmapEnabled(true, false);
        }
        
        if (specState == true && specCam !== null && specTarget != null && mp.players.exists(specTarget)) 
        {
            if((mp.players.local.getHealth() != specTarget.getHealth() || mp.players.local.getArmour() != specTarget.getArmour()) && 
                Date.now() - lastSpecHPChange >= 500)
            {
                mp.events.callRemote("SpectateChangeHPAR", specTarget.getHealth(), specTarget.getArmour());
                lastSpecHPChange = Date.now();
            }

            if(specTarget.isInAnyVehicle(false))
            {
                if(!speedo)
                {
                    UIHud.execute(`gm.speedo = true;`);
                    speedo = true;
                }

                if(speedoMax != ((mp.game.vehicle.getVehicleModelMaxSpeed(specTarget.vehicle.model) * 3.6) + 25).toFixed(0))
                {
                    speedoMax = ((mp.game.vehicle.getVehicleModelMaxSpeed(specTarget.vehicle.model) * 3.6) + 25).toFixed(0);
                    UIHud.execute(`gm.$refs.BottomRight.$refs.Speedometer.maxspeed = ${speedoMax};`);
                }
                if(speedoCur != (specTarget.vehicle.getSpeed() * 3.6).toFixed(0))
                {
                    speedoCur = (specTarget.vehicle.getSpeed() * 3.6).toFixed(0);
                    UIHud.execute(`gm.$refs.BottomRight.$refs.Speedometer.speed = ${speedoCur};`);
                }
            }
            else if(speedo)
            {
                speedo = false;
                speedoMax = 0;
                speedoCur = 0;
                UIHud.execute(`gm.speedo = false;`);
            }
        }

        if(mp.players.local.isInAnyVehicle(false))
        {
            if(!speedo)
            {
                UIHud.execute(`gm.speedo = true;`);
                speedo = true;
            }

            if(speedoMax != ((mp.game.vehicle.getVehicleModelMaxSpeed(mp.players.local.vehicle.model) * 3.6) + 25).toFixed(0))
            {
                speedoMax = ((mp.game.vehicle.getVehicleModelMaxSpeed(mp.players.local.vehicle.model) * 3.6) + 25).toFixed(0);
                UIHud.execute(`gm.$refs.BottomRight.$refs.Speedo.maxspeed = ${speedoMax};`);
            }
            if(speedoCur != (mp.players.local.vehicle.getSpeed() * 3.6).toFixed(0))
            {
                speedoCur = (mp.players.local.vehicle.getSpeed() * 3.6).toFixed(0);
                UIHud.execute(`gm.$refs.BottomRight.$refs.Speedo.speed = ${speedoCur};`);
            }
        }
        else if(speedo)
        {
            speedo = false;
            speedoMax = 0;
            speedoCur = 0;
            UIHud.execute(`gm.speedo = false;`);
        }
    }
});