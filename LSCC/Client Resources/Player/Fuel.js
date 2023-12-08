const player = mp.players.local;
var lastFuelTickPosition = new mp.Vector3(0,0,0);
var isPlayerDriving = false;


mp.events.add("render", () => {
    // Handle the 'isPlayerDriving' variable based on whether the player is in the vehicle AND their seat is the driver's seat
    if(isPlayerDriving && (!mp.players.local.isInAnyVehicle(true) || mp.players.local.seat !== -1)){
        isPlayerDriving = false;
        return;
    }
    if(!isPlayerDriving && mp.players.local.isInAnyVehicle(true) && mp.players.local.seat == -1){
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
    if(distance < 75) return;

    // If distance is more than 10 units, decrease the fuel
    const fuel = vehicle.getVariable("Fuel");
    const fuelDepletion = getFuelDepletionRate();
    const fuelToSet = (fuel ? (fuel-fuelDepletion) : 0); 
    if(global.ServerUI) global.ServerUI.call('hud_vehicle_fuel_set', fuelToSet); // needs updating/fixing
    mp.events.callRemote("UpdateVehicleFuel", fuelToSet, vehicle.remoteId);
    lastFuelTickPosition = vehiclePosition;
});

// Biggest distance = higher fuel depletion
function getFuelDepletionRate(){
    var distance = distanceToNearestCop();
    if(!distance) return 1.2;
    if(distance < 100) return 0.3;
    if(distance >= 100 && distance <= 200) return 0.5;
    return 0.75;
}
function distanceToNearestCop(){
    var closestDistance = undefined;
    mp.players.forEachInStreamRange((pl) => {
        if(pl.getVariable("Team") == 1){
            const dist = mp.game.system.vdist2(pl.position.x, pl.position.y, pl.position.z, player.position.x, player.position.y, player.position.z);
            if(!closestDistance || dist < closestDistance) closestDistance = dist;
        }
    });
    return closestDistance;
}