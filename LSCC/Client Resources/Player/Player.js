let camera = null;
const player = mp.players.local;

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

mp.events.add(
    {
        "DestroyPlayerCamera": () => destroyCam(),

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