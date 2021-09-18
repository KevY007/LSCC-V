const updateInterval = 1000; 

const Natives = {
    IS_RADAR_HIDDEN: "0x157F93B036700462",
    IS_RADAR_ENABLED: "0xAF754F20EB5CD51A",
    SET_TEXT_OUTLINE: "0x2513DFB0FB8400FE"
};

let minimap = {};
let lastSpeed = 0;

var visibleTick = Date.now();
var visibleCount = 0;
var visibleText = "KEVYISGAY";

var resolution = mp.game.graphics.getScreenResolution(0,0);
var res_X = resolution.x;
var res_Y = resolution.y;
let bestlobbystr = "N/A";
let dirtext = "NW";

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

setInterval(() => {
    // only do stuff if radar is enabled and visible
    if (mp.game.invoke(Natives.IS_RADAR_ENABLED) && !mp.game.invoke(Natives.IS_RADAR_HIDDEN)) {
        isMetric = mp.game.gameplay.getProfileSetting(227) == 1;
        minimap = getMinimapAnchor();

        const position = mp.players.local.position;
        let getStreet = mp.game.pathfind.getStreetNameAtCoord(position.x, position.y, position.z, 0, 0);

        zoneName = mp.game.ui.getLabelText(mp.game.zone.getNameOfZone(position.x, position.y, position.z));
        streetName = mp.game.ui.getStreetNameFromHashKey(getStreet.streetName);
    } else {
        streetName = null;
        zoneName = null;
    }
}, updateInterval);

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
    UIHud.execute(`gm.topRightInfo = ${mp.storage.data.menu.topRightInfo}; gm.killFeed = ${mp.storage.data.menu.killFeed};`);
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

mp.events.add("pushToKillFeed", (killer, killerColor, weapon, victim, victimColor) => {
    if(UIHud != null) UIHud.execute(`gm.$refs.TopRight.$refs.KillFeed.add('${killer}', '${killerColor}', '${weapon}', '${victim}', '${victimColor}');`)
});

mp.events.add("pushToKillFeed2", (victim, color, cause) => {
    if(UIHud != null) UIHud.execute(`gm.$refs.TopRight.$refs.KillFeed.add("${victim}", "${color}", "${cause}");`)
});

mp.events.add("OneSecondEvent", () => {
    if(UIHud != null && (!(mp.players.local.LoggedIn === undefined || mp.players.local.LoggedIn == false || mp.players.local.Lobby === undefined)))
    {
        UIHud.execute(`gm.$refs.TopRight.$refs.Info.onlinePlayers = ${mp.players.length};`);

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

    mp.players.forEach(player => {
        updateString += `{id: ${player.id}, 
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

    ToggleScoreboard();
}

mp.events.add('ScoreboardToggleTyping', (state) => {
    if(state && scoreboardBinded)
    {
        mp.keys.unbind(0x4F, false, ScoreboardKeyTrigger);
        scoreboardBinded = false;
    }
    else if(!state && !scoreboardBinded)
    {
        mp.keys.bind(0x4F, false, ScoreboardKeyTrigger);
        scoreboardBinded = true;
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

    if(Date.now() - visibleTick >= 1000)
    {
        visibleTick = Date.now();
        

        visibleCount++;
        visibleText = "~r~~h~BLIP VISIBLE~o~!";

        if(visibleCount >= 2)
        {
            visibleCount = 0;
            visibleText = "~r~~h~BLIP VISIBLE";
        }

        var camera = mp.cameras.new("gameplay");
        var cameraDirection = camera.getDirection();

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
        
        bestlobbystr = "";
        if(!(mp.players.local.LoggedIn === undefined || mp.players.local.LoggedIn == false || mp.players.local.Lobby === undefined))
        {
            let lobbyPlayers = 0;
            mp.players.forEach((pl) => {
                if(!(pl.LoggedIn === undefined || pl.LoggedIn == false || pl.Lobby === undefined))
                {
                    if(pl.Lobby == mp.players.local.Lobby) lobbyPlayers++;
                }
            });

            if(mp.players.local.Lobby == -2) bestlobbystr = "~r~Jail";
            if(mp.players.local.Lobby == -3) bestlobbystr = "~g~Freeroam";
            if(mp.players.local.Lobby == -4)
            {
                bestlobbystr = "~y~Derby " + "~w~| ~w~Participants: ~y~" + lobbyPlayers;
            }
            if(mp.players.local.Lobby == -5) bestlobbystr = "~r~Player Customizer";
            if(mp.players.local.Lobby > -1) 
            {
                bestlobbystr = "~w~Lobby ~y~" + mp.players.local.Lobby.toString() + "  ~w~|  ~w~Players: ~y~" + lobbyPlayers;
            }
        }
    }

    if(mp.players.local.LoggedIn !== undefined && mp.players.local.LoggedIn == true)
    {
        if (currentJailTime >= 1)
        {
            drawText(`~y~JAILED: ~w~${TimeFormat(currentJailTime)}`, [minimap.rightX + 0.0385, minimap.bottomY - 0.130], 4, [204, 204, 204, 255], 1.3);
        }
        else
        {
            if (streetName && zoneName && showHud) {

                let vehicle = mp.players.local.vehicle;
                var pX = 0.0;
                var pY = 0.0;
                var sX = 0.0;
                var sY = 0.0;
                
                if (vehicle) 
                {
                    if(mp.players.local.getVariable("VisibleBlip") == true)
                    {
                        if(mp.players.local.getVariable("ActionType") == "none")
                        {
                            pX = minimap.rightX + 0.0875;
                            pY = minimap.topY + 0.100;
                            sX = 0.165;
                            sY = 0.15;
                        }
                        else
                        {
                            pX = minimap.rightX + 0.0875;
                            pY = minimap.topY + 0.090;
                            sX = 0.165;
                            sY = 0.17;   
                            drawText(`~y~Action: ~w~${firstLetterUpper(mp.players.local.getVariable("ActionType"))} ~b~| ~y~Time: ${mp.players.local.getVariable("ActionTime") > 3 ? "~o~" : "~g~"}${mp.players.local.getVariable("ActionTime")}`, [minimap.rightX + 0.0085, minimap.bottomY - 0.171], 0, [204, 204, 204, 255], 0.38);                        
                        }

                        drawText(visibleText, [minimap.rightX + 0.0085, minimap.bottomY - 0.152], 0, [204, 204, 204, 255], 0.40);
                    }
                    else
                    {
                        if(mp.players.local.getVariable("ActionType") == "none")
                        {
                            pX = minimap.rightX + 0.0875;
                            pY = minimap.topY + 0.110;
                            sX = 0.165;
                            sY = 0.13;
                        }
                        else
                        {
                            pX = minimap.rightX + 0.0875;
                            pY = minimap.topY + 0.100;
                            sX = 0.165;
                            sY = 0.15;      
                            
                            drawText(`~y~Action: ~w~${firstLetterUpper(mp.players.local.getVariable("ActionType"))} ~b~| ~y~Time: ${mp.players.local.getVariable("ActionTime") > 3 ? "~o~" : "~g~"}${mp.players.local.getVariable("ActionTime")}`, [minimap.rightX + 0.0085, minimap.bottomY - 0.150], 0, [204, 204, 204, 255], 0.38);
                        }
                    }

                    if(((vehicle.getSpeed() * 3.6).toFixed(0) % 2 == 0 || (vehicle.getSpeed() * 3.6).toFixed(0) % 5 == 0) || 
                        (vehicle.getSpeed() * 3.6).toFixed(0) == 0)
                    {
                        lastSpeed = (vehicle.getSpeed() * 3.6).toFixed(0);
                    }

                    drawText(`~y~${lastSpeed} ~w~KM/H`, [minimap.rightX + 0.0085, minimap.bottomY - 0.130], 4, [204, 204, 204, 255], 0.7);
                }
                else
                {
                    if(mp.players.local.getVariable("VisibleBlip") == true)
                    {
                        if(mp.players.local.getVariable("ActionType") == "none")
                        {
                            pX = minimap.rightX + 0.0875;
                            pY = minimap.topY + 0.110;
                            sX = 0.1625;
                            sY = 0.1293;
                        }
                        else
                        {
                            pX = minimap.rightX + 0.0875;
                            pY = minimap.topY + 0.100;
                            sX = 0.1625;
                            sY = 0.1486;
                            drawText(`~y~Action: ~w~${firstLetterUpper(mp.players.local.getVariable("ActionType"))} ~b~| ~y~Time Left: ${mp.players.local.getVariable("ActionTime") > 3 ? "~o~" : "~g~"}${mp.players.local.getVariable("ActionTime")}`, [minimap.rightX + 0.0085, minimap.bottomY - 0.1525], 0, [204, 204, 204, 255], 0.35);
                        }

                        drawText(visibleText, [minimap.rightX + 0.0085, minimap.bottomY - 0.1315], 0, [204, 204, 204, 255], 0.35);
                    }
                    else
                    {
                        if(mp.players.local.getVariable("ActionType") == "none")
                        {
                            pX = minimap.rightX + 0.0875;
                            pY = minimap.topY + 0.120;
                            sX = 0.1625;
                            sY = 0.110;
                        }
                        else
                        {
                            pX = minimap.rightX + 0.0875;
                            pY = minimap.topY + 0.110;
                            sX = 0.1625;
                            sY = 0.1293;
                            drawText(`~y~Action: ~w~${firstLetterUpper(mp.players.local.getVariable("ActionType"))} ~b~| ~y~Time Left: ${mp.players.local.getVariable("ActionTime") > 3 ? "~o~" : "~g~"}${mp.players.local.getVariable("ActionTime")}`, [minimap.rightX + 0.0085, minimap.bottomY - 0.1315], 0, [204, 204, 204, 255], 0.35);
                        }                        
                    }
                }

                mp.game.graphics.drawRect(pX, pY, sX, sY, 0, 0, 0, 150);

                if(!mp.players.local.isInAnyVehicle(false))
                    drawText(`~r~Fugitive Chance: ~w~${mp.players.local.getVariable("FugitiveChance").toFixed(1)}%`, [minimap.rightX + 0.0085, minimap.bottomY - 0.110], 0, [204, 204, 204, 255], 0.28);
                else
                    drawText(`~r~Fugitive Chance: ~w~${mp.players.local.getVariable("FugitiveChance").toFixed(1)}%`, [minimap.rightX + 0.072, minimap.bottomY - 0.116], 0, [204, 204, 204, 255], 0.28);

                drawText(`~y~Points: ~w~${mp.players.local.getVariable("Points")}`, [minimap.rightX + 0.087, minimap.bottomY - 0.032], 0, [204, 204, 204, 255], 0.35);

                let voiceString = "Voice: ~r~Disabled";
                
                if(mp.players.local.getVariable("VoiceMode") == 0)
                {
                    voiceString = mp.voiceChat.muted ? "Voice: ~w~Standby" : "Voice: ~g~Speaking";
                }
                else if(mp.players.local.getVariable("VoiceMode") == 1)
                {
                    voiceString = mp.voiceChat.muted ? "Voice: ~w~Standby (T)" : "Voice: ~g~Speaking (T)";
                }
                else if(mp.players.local.getVariable("VoiceMode") == 2)
                {
                    voiceString = mp.voiceChat.muted ? "Voice: ~w~Standby (S)" : "Voice: ~g~Speaking (S)";
                }

                if(currentMuteTime >= 1)
                {
                    voiceString = `~r~Muted for ${TimeFormat(currentMuteTime)}`;
                }

                drawText(voiceString, [minimap.rightX + 0.0085, minimap.bottomY - 0.095], 4, [63, 135, 240, 255], 0.50);
                drawText(streetName, [minimap.rightX + 0.0085, minimap.bottomY - 0.065], 4, [204, 204, 204, 255], 0.465);
                drawText(zoneName, [minimap.rightX + 0.0085, minimap.bottomY - 0.035], 4, [255, 255, 255, 255], 0.48);

                mp.game.graphics.drawText(bestlobbystr, [minimap.rightX + 0.121, minimap.bottomY - 0.0625], {
                    font:4, 
                    color:[255, 255, 255, 200],
                    scale: [0.40, 0.40],
                    outline: true,
                    centre: true }
                );

                mp.game.graphics.drawText("~y~" + dirtext + " ~w~| " + "Online: " + mp.players.length, [minimap.rightX + 0.119, minimap.bottomY - 0.090], {
                    font:4, 
                    color:[255, 255, 255, 200],
                    scale: [0.37, 0.37],
                    outline: true,
                    centre: true }
                );
            }
        }
    }
});