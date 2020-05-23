const updateInterval = 1000; 

const Natives = {
    IS_RADAR_HIDDEN: "0x157F93B036700462",
    IS_RADAR_ENABLED: "0xAF754F20EB5CD51A",
    SET_TEXT_OUTLINE: "0x2513DFB0FB8400FE"
};

let streetName = null;
let zoneName = null;
let minimap = {};
let showHud = true;

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

mp.events.add('ToggleHud', (toggle) => {
    showHud = toggle;
});

mp.events.add("render", () => {
    if(mp.players.local.getVariable("pLogged") !== undefined && mp.players.local.getVariable("pLogged") == true)
    {
        if (currentJailTime >= 1)
        {
            drawText(`~y~Jailed: ~w~${TimeFormat(currentJailTime)}`, [minimap.rightX + 0.0385, minimap.bottomY - 0.130], 4, [204, 204, 204, 255], 1.3);
        }
        else
        {
            if (streetName && zoneName && showHud) {

                let vehicle = mp.players.local.vehicle;
                if (vehicle) 
                {
                    mp.game.graphics.drawRect(minimap.rightX + 0.055, minimap.topY + 0.110, 0.10, 0.13, 0, 0, 0, 150);

                    drawText(`${(vehicle.getSpeed() * 3.6).toFixed(0)} KM/H`, [minimap.rightX + 0.0085, minimap.bottomY - 0.130], 4, [204, 204, 204, 255], 0.7);
                }
                else
                {
                    mp.game.graphics.drawRect(minimap.rightX + 0.055, minimap.topY + 0.125, 0.10, 0.10, 0, 0, 0, 150);
                }
                
                drawText(mp.voiceChat.muted ? "Voice: Standby" : "Voice: Speaking", [minimap.rightX + 0.0085, minimap.bottomY - 0.095], 4, [63, 135, 240, 255], 0.50);
                drawText(streetName, [minimap.rightX + 0.0085, minimap.bottomY - 0.065], 4, [255, 255, 255, 255], 0.425);
                drawText(zoneName, [minimap.rightX + 0.0085, minimap.bottomY - 0.035], 4, [255, 255, 255, 255], 0.45);

                
            }
        }
    }
});