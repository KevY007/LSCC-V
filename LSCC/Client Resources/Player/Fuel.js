const player = mp.players.local;
var currentVehicleFuel = 0.0;
var wasInVehicle = false;
var vehicleRemoteID = -1;
var fuelFrameCount = 0;

mp.events.add('render', () => {
    fuelFrameCount++;
    if(!wasInVehicle) {
        if(mp.players.local.getVariable("Team") == 0){
           if(mp.players.local.vehicle && player.seat == -1){ // if the player was not in any vehicle, set his current vehicle fuel to a new value
                wasInVehicle = true; // this is done so the fuel calculations are done clientside
                const vehicle = mp.players.local.vehicle; // the only serverside call is to set the fuel
                currentVehicleFuel = vehicle.getVariable("Fuel");  // fuel is set e.g. when it reaches 0, or the player exits vehicle
                vehicleRemoteID = vehicle.remoteId;
                pushMessageToChat("setting wasInVehicle = true");

                if(ServerUI){
                    ServerUI.execute(`gm.fuelStatus = true;`);
                }
           }

        }
    }
    else{
        if(fuelFrameCount >= 30){ // fuel handler is called every 30 frames
            HandleFuel();
            fuelFrameCount = 0;
        }
   }
});

function HandleFuel(){
    if(!mp.players.exists(mp.players.local) || !mp.players.local.vehicle || player.seat != -1 || player.getVariable("Team") != 0){
        wasInVehicle = false;
        fuelFrameCount = 0;
        ServerUI.execute(`gm.fuelStatus = false;`);
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

    ServerUI.execute(`gm.$refs.BottomRight.$refs.FuelStatus.fuel = ${currentVehicleFuel};`);

    if(currentVehicleFuel <= 0.0){
        currentVehicleFuel = 0.0;
        wasInVehicle = false;
        pushMessageToChat("~y~Your vehicle ran out of fuel! Tugy plaky");
        mp.events.callRemote("UpdateVehicleFuel", 0, 65535); // 65535 = it takes the vehicle player is currently in
        return;
    }
}