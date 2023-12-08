const player = mp.players.local;
var lastFuelTickPosition = new mp.Vector3(0,0,0);
var isPlayerDriving = false;


mp.events.add("render", () => {
    // Handle the 'isPlayerDriving' variable based on whether the player is in the vehicle AND their seat is the driver's seat
    if(isPlayerDriving && (!mp.players.local.isInAnyVehicle() || mp.players.local.seat !== -1)){
        isPlayerDriving = false;
        return;
    }
    if(!isPlayerDriving && mp.players.local.isInAnyVehicle() && mp.players.local.seat == -1){
        isPlayerDriving = true;
        return;
    }
});

// Every two seconds, perform a check to see how far the vehicle went from last check
mp.events.add("TwoSecondsEvent", () => {
    if(!isPlayerDriving) return;

    const vehicle = mp.players.local.vehicle;
    if(!vehicle || !vehicle.hasVariable("Fuel")) return;

    const vehiclePosition = vehicle.position;

    const distance = mp.game.system.vdist2(vehiclePosition.x, vehiclePosition.y, vehiclePosition.z, lastFuelTickPosition.x, lastFuelTickPosition.y, lastFuelTickPosition.z);
    if(distance < 10) return;

    // If distance is more than 10 units, decrease the fuel
    const fuel = vehicle.getVariable("Fuel");
    const fuelToSet = (fuel ? (fuel-1) : 0);
    mp.gui.chat.push(`!{#ff6347}[FUEL] !{#ffffff}Fuel depleted, currently at: !{#ffff00}${fuelToSet}%!{#ffffff}.`);
    mp.events.callRemote("UpdateVehicleFuel", fuelToSet, vehicle.remoteId);
    lastFuelTickPosition = vehiclePosition;
});
