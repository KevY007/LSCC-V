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
        if(!chatStatus && 
            !(mp.players.local.getVariable("pLogged") === undefined || mp.players.local.getVariable("pLogged") == false)) 
        {
            if (mp.keys.isDown(16) === true) 
            { // Shift is pressed
                renderNametags = showHud;
            }

            mp.events.callLocal("ToggleHud", !showHud);
        }
    }
);

mp.events.add('ToggleHud', (toggle) => {
    showHud = toggle;

    mp.gui.chat.show(showHud);
    mp.game.ui.displayRadar(showHud);
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
    }

    if(mp.players.local.getVariable("pLogged") !== undefined && mp.players.local.getVariable("pLogged") == true)
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
                        pX = minimap.rightX + 0.0875;
                        pY = minimap.topY + 0.100;
                        sX = 0.165;
                        sY = 0.15;

                        drawText(visibleText, [minimap.rightX + 0.0085, minimap.bottomY - 0.152], 0, [204, 204, 204, 255], 0.40);
                    }
                    else
                    {
                        pX = minimap.rightX + 0.0875;
                        pY = minimap.topY + 0.110;
                        sX = 0.165;
                        sY = 0.13;
                    }

                    if((vehicle.getSpeed() * 3.6).toFixed(0) % 2 == 0 || (vehicle.getSpeed() * 3.6).toFixed(0) == 0)
                    {
                        lastSpeed = (vehicle.getSpeed() * 3.6).toFixed(0);
                    }

                    drawText(`~y~${lastSpeed} ~w~KM/H`, [minimap.rightX + 0.0085, minimap.bottomY - 0.130], 4, [204, 204, 204, 255], 0.7);
                }
                else
                {
                    if(mp.players.local.getVariable("VisibleBlip") == true)
                    {
                        pX = minimap.rightX + 0.0875;
                        pY = minimap.topY + 0.120;
                        sX = 0.1625;
                        sY = 0.110;

                        drawText(visibleText, [minimap.rightX + 0.0085, minimap.bottomY - 0.1125], 0, [204, 204, 204, 255], 0.35);
                    }
                    else
                    {
                        pX = minimap.rightX + 0.0875;
                        pY = minimap.topY + 0.125;
                        sX = 0.1625;
                        sY = 0.10;
                    }
                }

                mp.game.graphics.drawRect(pX, pY, sX, sY, 0, 0, 0, 150);

                
                let voiceString = "Voice: ~r~Disabled";
                
                if(mp.players.local.getVariable("VoiceMode") == 0)
                {
                    voiceString = mp.voiceChat.muted ? "Voice: ~w~Standby" : "Voice: ~g~Speaking";
                }
                else if(mp.players.local.getVariable("VoiceMode") == 1)
                {
                    voiceString = mp.voiceChat.muted ? "Voice: ~w~Standby (Team)" : "Voice: ~g~Speaking (Team)";
                }

                if(currentMuteTime >= 1)
                {
                    voiceString = `~r~Muted for ${TimeFormat(currentMuteTime)}`;
                }

                drawText(voiceString, [minimap.rightX + 0.0085, minimap.bottomY - 0.095], 4, [63, 135, 240, 255], 0.50);
                drawText(streetName, [minimap.rightX + 0.0085, minimap.bottomY - 0.065], 4, [204, 204, 204, 255], 0.465);
                drawText(zoneName, [minimap.rightX + 0.0085, minimap.bottomY - 0.035], 4, [255, 255, 255, 255], 0.48);
            }
        }
    }
});