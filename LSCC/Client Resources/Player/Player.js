let camera = null;
const player = mp.players.local;
const timerBarLib = require("timerbars");

let chaseProgBar = null;
let chaseProgText = null;
let chaseInterval = null;
let chaseTimeCur = 0;
let chaseTimeMax = 600;

function createCam(x, y, z, rx, ry, rz, viewangle) {
    camera = mp.cameras.new("Cam", { x, y, z }, { x: rx, y: ry, z: rz }, viewangle);
    camera.setActive(true);
    mp.game.cam.renderScriptCams(true, true, 20000000000000000000000000, false, false);
}

function destroyCam() {
    if (!camera) return;
    camera.setActive(false);
    mp.game.cam.renderScriptCams(false, true, 0, true, true);
    camera.destroy();
    camera = null;
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

function updateChaseTimer()
{
    if(chaseProgBar != null && chaseProgText != null)
    {
        chaseTimeCur += 1;

        chaseProgBar.progress = chaseTimeCur / chaseTimeMax;
        chaseProgText.text = TimeFormat(chaseTimeMax - chaseTimeCur);

        chaseProgText.visible = true;
        chaseProgBar.visible = true;

        if(chaseTimeCur >= chaseTimeMax)
        {
            chaseProgText.visible = false;
            chaseProgBar.visible = false;

            clearInterval(chaseInterval);
            chaseInterval = null;
        }
    }
    return;
}


mp.events.add(
    {
        "StartChaseTimer": (iTimeCur, iTimeMax) => {

            chaseTimeCur = iTimeCur - 1;
            chaseTimeMax = iTimeMax;

            updateChaseTimer();
            
            if(chaseInterval != null) clearInterval(chaseInterval);

            chaseInterval = setInterval(updateChaseTimer, 1000);
        },

        "HideChaseTimer": () => {
            chaseProgBar.visible = false;
            chaseProgText.visible = false;
            clearInterval(chaseInterval);
            chaseInterval = null;
        },

        "InitChaseTimer": () => {

            if(chaseProgBar != null)
            {
                chaseProgBar = new timerBarLib.TimerBar("PROGRESS", true);
                chaseProgBar.pbarFgColor = [64, 115, 222, 255];
                chaseProgBar.pbarBgColor = [25, 53, 112, 255];
                chaseProgBar.progress = 1.0;
                chaseProgBar.visible = false;
            }

            if(chaseProgText != null)
            {
                chaseProgText = new timerBarLib.TimerBar("TIME LEFT");
                chaseProgText.text = "10:00";
                chaseProgText.visible = false;
            }
        },

        "DestroyPlayerCamera": () => destroyCam(),

        "EnableCollisions": () => {
            mp.players.forEach(player => {
                mp.players.local.vehicle.setNoCollision(player.vehicle.handle, true);
                player.vehicle.setAlpha(255);
                player.setAlpha(255);
            });
        },

        "DisableCollisions": () => {
            mp.players.forEach(player => {
                mp.players.local.vehicle.setNoCollision(player.vehicle.handle, false);
                player.vehicle.setAlpha(102);
                player.setAlpha(255);
            });
        },

        "SetPlayerFrozen": (status) => {
            mp.players.local.freezePosition(status);
        },

        "CreatePlayerCamera": (x, y, z, rx, ry, rz, viewangle) => {
            createCam(x, y, z, rx, ry, rz, viewangle);
        },

        "TogglePlayerChat": (toggle) => {
            mp.gui.chat.activate(toggle);
        },

        "SetDiscordStatus": (serverName, status) => {
            mp.discord.update(serverName, status);
        },
    });