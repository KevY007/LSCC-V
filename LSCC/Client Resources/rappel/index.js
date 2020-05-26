const localPlayer = mp.players.local;
const maxSpeed = 20.0;
const minHeight = 15.0;
const maxHeight = 85.0;
const maxAngle = 15.0;
let chatStatus = false;

mp.events.add("onOpenChatbox", (toggle) =>
{
	chatStatus = toggle;
});

function rappelPlayer(warningLevel)
{
    if(warningLevel === undefined) warningLevel = 1;

    const vehicle = localPlayer.vehicle;
    if (!vehicle) {
        return;
    }

    if (!mp.game.invoke("0x4E417C547182C84D", vehicle.handle)) {
        if(warningLevel == 1) mp.gui.chat.push("You cannot rappel from this vehicle.");
        return;
    }

    if (vehicle.getSpeed() > maxSpeed) {
        if(warningLevel == 1) mp.gui.chat.push("The vehicle is too fast for rappelling.");
        return;
    }

    if (vehicle.getPedInSeat(-1) === localPlayer.handle || vehicle.getPedInSeat(0) === localPlayer.handle) {
        if(warningLevel == 1) mp.gui.chat.push("You cannot rappel from your seat.");
        return;
    }

    const taskStatus = localPlayer.getScriptTaskStatus(-275944640);
    if (taskStatus === 0 || taskStatus === 1) {
        return;
    }

    const curHeight = vehicle.getHeightAboveGround();
    if (curHeight < minHeight || curHeight > maxHeight) {
        mp.gui.chat.push("The vehicle is too low/too high for rappelling.");
        return;
    }

    if (!vehicle.isUpright(maxAngle) || vehicle.isUpsidedown()) {
        mp.gui.chat.push("The vehicle needs to be stable for rappelling.");
        return;
    }

    localPlayer.clearTasks();
    localPlayer.taskRappelFromHeli(10.0);
}

mp.keys.bind(0x58, false, function() {
    if(!chatStatus)
    {
        rappelPlayer(0);
    }
});

mp.events.add("playerCommand", (command) => {
    if (command.toLowerCase() === "rappel") {
        rappelPlayer(1);
    }
});