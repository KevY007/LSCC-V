const player = mp.players.local;
var currentVehicleFuel = 0.0;
var wasInVehicle = false;
var vehicleRemoteID = -1;
var fuelFrameCount = 0;

mp.events.add('render', () => {
    if(!wasInVehicle) {
        if(player.getVariable("Team") == 0){
           if(player.isInAnyVehicle(false) && player.seat == 0){ // if the player was not in any vehicle, set his current vehicle fuel to a new value
                wasInVehicle = true; // this is done so the fuel calculations are done clientside
                const vehicle = player.vehicle; // the only serverside call is to set the fuel
                currentVehicleFuel = vehicle.getVariable("Fuel");  // fuel is set e.g. when it reaches 0, or the player exits vehicle
                vehicleRemoteID = vehicle.remoteId;
           }

        }
    }
    else{
        fuelFrameCount++;
        if(fuelFrameCount >= 30){ // fuel handler is called every 30 frames
            HandleFuel();
            fuelFrameCount = 0;
        }
   }
});

function HandleFuel(){
    if(!mp.players.local || !mp.players.exists(mp.players.local) || !player.vehicle || !player.isInAnyVehicle(false) || player.seat != 0 || player.getVariable("Team") != 0){
        wasInVehicle = false;
        fuelFrameCount = 0;
        mp.events.callRemote("UpdateVehicleFuel", currentVehicleFuel, vehicleRemoteID);
        return;
    }

    var currentVehicleSpeed = (player.vehicle.getSpeed() * 3.6);

    var minimum = 69420;
    const vec1 = player.position;
    mp.players.forEach(cop => {
        if(cop.dimension == player.dimension && cop.getVariable("Team") == 1 && cop.getVariable("Wounded") == 0){ 
            const vec2 = cop.position;
            const dist = vec1.subtract(vec2).length();

            if(dist < minimum) minimum = dist;
        }
    });


    var distance = minimum;
    var distanceEffect = 1; // PLACEHOLDER!!!
    if(distance < 100) distanceEffect = 0;
    else if(distance >= 100 && distance <= 200) distanceEffect = 0.5;
    else distanceEffect = 1;

	var fuelToSubstract = (Math.pow(currentVehicleSpeed, 2)/100000)*distanceEffect;
    currentVehicleFuel -= fuelToSubstract;

    pushMessageToChat("~y~Fuel left: " + currentVehicleFuel + " ~r~Distance: " + distance);

    if(currentVehicleFuel <= 0.0){
        currentVehicleFuel = 0.0;
        wasInVehicle = false;
        pushMessageToChat("~y~Your vehicle ran out of fuel! Tugy plaky");
        mp.events.callRemote("UpdateVehicleFuel", 0, 65535); // 65535 = it takes the vehicle player is currently in
        return;
    }
}