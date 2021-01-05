function getClosestVehicle( position ) 
{
    try {
        let closest = 50;
        let closestVeh = null;

        mp.vehicles.forEachInStreamRange( v => {
            let dist =  mp.game.system.vdist( position.x, position.y, position.z, v.position.x, v.position.y, v.position.z );

            if( dist < closest ) {
                closest = dist;
                closestVeh = v;
            }
        } );

        return { distance: closest, vehicle: closestVeh };
    } catch( e ) { }
}

// G to ENTER + SHIFT + G to OUTER ENT script from GTAWorld

mp.keys.bind(71, false, () => 
{
    if (mp.players.local.vehicle === null && !mp.gui.cursor.visible && !chatStatus)
    {
        const driverSeatId = 0; // CHANGE THIS AS THE DRIVER INDEX CHANGES
        const playerPos = mp.players.local.position;
        let closestVeh = getClosestVehicle(playerPos);

        if (closestVeh.distance > 5)
        {
            return;
        }

        let vehicle = closestVeh.vehicle;
        if (!vehicle) return;

        if (vehicle.isAVehicle()) 
        {
            if (mp.game.vehicle.isThisModelABike(vehicle.model)) 
            {
                if (vehicle.isSeatFree(0)) 
                {
                    mp.players.local.taskEnterVehicle(vehicle.handle, 5000, 0, 2.0, 1, 0);
                }
                return;
            }

            // Seat Bones (connected to the... leg bone)
            // const seatFrontDriver = vehicle.getBoneIndexByName('seat_dside_f');
            const seatRear = vehicle.getBoneIndexByName('seat_r');
            const seatFrontPassenger = vehicle.getBoneIndexByName('seat_pside_f');
            const seatRearDriver = vehicle.getBoneIndexByName('seat_dside_r');
            const seatRearDriver1 = vehicle.getBoneIndexByName('seat_dside_r1');
            const seatRearDriver2 = vehicle.getBoneIndexByName('seat_dside_r2');
            const seatRearDriver3 = vehicle.getBoneIndexByName('seat_dside_r3');
            const seatRearDriver4 = vehicle.getBoneIndexByName('seat_dside_r4');
            const seatRearDriver5 = vehicle.getBoneIndexByName('seat_dside_r5');
            const seatRearDriver6 = vehicle.getBoneIndexByName('seat_dside_r6');
            const seatRearDriver7 = vehicle.getBoneIndexByName('seat_dside_r7');
            const seatRearPassenger = vehicle.getBoneIndexByName('seat_pside_r');
            const seatRearPassenger1 = vehicle.getBoneIndexByName('seat_pside_r1');
            const seatRearPassenger2 = vehicle.getBoneIndexByName('seat_pside_r2');
            const seatRearPassenger3 = vehicle.getBoneIndexByName('seat_pside_r3');
            const seatRearPassenger4 = vehicle.getBoneIndexByName('seat_pside_r4');
            const seatRearPassenger5 = vehicle.getBoneIndexByName('seat_pside_r5');
            const seatRearPassenger6 = vehicle.getBoneIndexByName('seat_pside_r6');
            const seatRearPassenger7 = vehicle.getBoneIndexByName('seat_pside_r7');
    
            // Positions in world
            // const seatFrontDriverPosition = seatFrontDriver === -1 ? null : vehicle.getWorldPositionOfBone(seatFrontDriver);
            const seatRearPosition = seatRear === -1 ? null : vehicle.getWorldPositionOfBone(seatRear);
            const seatFrontPassengerPosition = seatFrontPassenger === -1 ? null : vehicle.getWorldPositionOfBone(seatFrontPassenger);
            const seatRearDriverPosition = seatRearDriver === -1 ? null : vehicle.getWorldPositionOfBone(seatRearDriver);
            const seatRearDriver1Position = seatRearDriver1 === -1 ? null : vehicle.getWorldPositionOfBone(seatRearDriver1);
            const seatRearDriver2Position = seatRearDriver2 === -1 ? null : vehicle.getWorldPositionOfBone(seatRearDriver2);
            const seatRearDriver3Position = seatRearDriver3 === -1 ? null : vehicle.getWorldPositionOfBone(seatRearDriver3);
            const seatRearDriver4Position = seatRearDriver4 === -1 ? null : vehicle.getWorldPositionOfBone(seatRearDriver4);
            const seatRearDriver5Position = seatRearDriver5 === -1 ? null : vehicle.getWorldPositionOfBone(seatRearDriver5);
            const seatRearDriver6Position = seatRearDriver6 === -1 ? null : vehicle.getWorldPositionOfBone(seatRearDriver6);
            const seatRearDriver7Position = seatRearDriver7 === -1 ? null : vehicle.getWorldPositionOfBone(seatRearDriver7);
            const seatRearPassengerPosition = seatRearPassenger === -1 ? null : vehicle.getWorldPositionOfBone(seatRearPassenger);
            const seatRearPassenger1Position = seatRearPassenger1 === -1 ? null : vehicle.getWorldPositionOfBone(seatRearPassenger1);
            const seatRearPassenger2Position = seatRearPassenger2 === -1 ? null : vehicle.getWorldPositionOfBone(seatRearPassenger2);
            const seatRearPassenger3Position = seatRearPassenger3 === -1 ? null : vehicle.getWorldPositionOfBone(seatRearPassenger3);
            const seatRearPassenger4Position = seatRearPassenger4 === -1 ? null : vehicle.getWorldPositionOfBone(seatRearPassenger4);
            const seatRearPassenger5Position = seatRearPassenger5 === -1 ? null : vehicle.getWorldPositionOfBone(seatRearPassenger5);
            const seatRearPassenger6Position = seatRearPassenger6 === -1 ? null : vehicle.getWorldPositionOfBone(seatRearPassenger6);
            const seatRearPassenger7Position = seatRearPassenger7 === -1 ? null : vehicle.getWorldPositionOfBone(seatRearPassenger7);
    
            // Get closest seat
            let closestFreeSeatNumber = -1;
            let seatIndex = driverSeatId;
            let closestSeatDistance = Number.MAX_SAFE_INTEGER;
            let calculatedDistance = null;
    
            // Inline Rear
            calculatedDistance = seatRearPosition === null ? null : calcDist(playerPos, seatRearPosition);
            seatIndex = seatRear === -1 ? seatIndex : seatIndex + 1;
            if (calculatedDistance !== null && vehicle.isSeatFree(seatIndex) && calculatedDistance < closestSeatDistance) 
            {
                closestSeatDistance = calculatedDistance;
                closestFreeSeatNumber = seatIndex;
            }
    
            // Side by Side vehicles
            calculatedDistance = seatFrontPassengerPosition === null ? null : calcDist(playerPos, seatFrontPassengerPosition);
            seatIndex = seatFrontPassenger === -1 ? seatIndex : seatIndex + 1;
            if (calculatedDistance !== null && vehicle.isSeatFree(seatIndex) && calculatedDistance < closestSeatDistance) 
            {
                closestSeatDistance = calculatedDistance;
                closestFreeSeatNumber = seatIndex;
            }
    
            calculatedDistance = seatRearDriverPosition === null ? null : calcDist(playerPos, seatRearDriverPosition);
            seatIndex = seatRearDriver === -1 ? seatIndex : seatIndex + 1;
            if (calculatedDistance !== null && vehicle.isSeatFree(seatIndex) && calculatedDistance < closestSeatDistance) 
            {
                closestSeatDistance = calculatedDistance;
                closestFreeSeatNumber = seatIndex;
            }
    
            calculatedDistance = seatRearPassengerPosition === null ? null : calcDist(playerPos, seatRearPassengerPosition);
            seatIndex = seatRearPassenger === -1 ? seatIndex : seatIndex + 1;
            if (calculatedDistance !== null && vehicle.isSeatFree(seatIndex) && calculatedDistance < closestSeatDistance) 
            {
                closestSeatDistance = calculatedDistance;
                closestFreeSeatNumber = seatIndex;
            }
    
            // Force inner seats before outer grab holds if shift not pressed
            calculatedDistance = seatRearDriver1Position === null ? null : calcDist(playerPos, seatRearDriver1Position);
            seatIndex = seatRearDriver1 === -1 ? seatIndex : seatIndex + 1; // 3

            if (!vehicle.isSeatFree(seatIndex - 2) || mp.keys.isDown(16)) 
            {
                if (calculatedDistance !== null && vehicle.isSeatFree(seatIndex) && calculatedDistance < closestSeatDistance) 
                {
                    closestSeatDistance = calculatedDistance;
                    closestFreeSeatNumber = seatIndex;
                }
            }
    
            // Force inner seats before outer grab holds if shift not pressed
            calculatedDistance = seatRearPassenger1Position === null ? null : calcDist(playerPos, seatRearPassenger1Position);
            seatIndex = seatRearPassenger1 === -1 ? seatIndex : seatIndex + 1; // 4
            if (!vehicle.isSeatFree(seatIndex - 2) || mp.keys.isDown(16)) 
            {
                if (calculatedDistance !== null && vehicle.isSeatFree(seatIndex) && calculatedDistance < closestSeatDistance) 
                {
                    closestSeatDistance = calculatedDistance;
                    closestFreeSeatNumber = seatIndex;
                }
            }
    
            // Force inner seats before outer grab holds if shift not pressed
            calculatedDistance = seatRearDriver2Position === null ? null : calcDist(playerPos, seatRearDriver2Position);
            seatIndex = seatRearDriver2 === -1 ? seatIndex : seatIndex + 1; // 5
            if (!vehicle.isSeatFree(seatIndex - 4) || mp.keys.isDown(16)) 
            {
                if (calculatedDistance !== null && vehicle.isSeatFree(seatIndex) && calculatedDistance < closestSeatDistance) 
                {
                    closestSeatDistance = calculatedDistance;
                    closestFreeSeatNumber = seatIndex;
                }
            }
    
            // Force inner seats before outer grab holds if shift not pressed
            calculatedDistance = seatRearPassenger2Position === null ? null : calcDist(playerPos, seatRearPassenger2Position);
            seatIndex = seatRearPassenger2 === -1 ? seatIndex : seatIndex + 1; // 6
            if (!vehicle.isSeatFree(seatIndex - 4) || mp.keys.isDown(16)) 
            {
                if (calculatedDistance !== null && vehicle.isSeatFree(seatIndex) && calculatedDistance < closestSeatDistance) {
                    closestSeatDistance = calculatedDistance;
                    closestFreeSeatNumber = seatIndex;
                }
            }
    
            calculatedDistance = seatRearDriver3Position === null ? null : calcDist(playerPos, seatRearDriver3Position);
            seatIndex = seatRearDriver3 === -1 ? seatIndex : seatIndex + 1;
            
            if (calculatedDistance !== null && vehicle.isSeatFree(seatIndex) && calculatedDistance < closestSeatDistance) {
                closestSeatDistance = calculatedDistance;
                closestFreeSeatNumber = seatIndex;
            }
    
            calculatedDistance = seatRearPassenger3Position === null ? null : calcDist(playerPos, seatRearPassenger3Position);
            seatIndex = seatRearPassenger3 === -1 ? seatIndex : seatIndex + 1;

            if (calculatedDistance !== null && vehicle.isSeatFree(seatIndex) && calculatedDistance < closestSeatDistance) {
                closestSeatDistance = calculatedDistance;
                closestFreeSeatNumber = seatIndex;
            }
    
            calculatedDistance = seatRearDriver4Position === null ? null : calcDist(playerPos, seatRearDriver4Position);
            seatIndex = seatRearDriver4 === -1 ? seatIndex : seatIndex + 1;

            if (calculatedDistance !== null && vehicle.isSeatFree(seatIndex) && calculatedDistance < closestSeatDistance) {
                closestSeatDistance = calculatedDistance;
                closestFreeSeatNumber = seatIndex;
            }
    
            calculatedDistance = seatRearPassenger4Position === null ? null : calcDist(playerPos, seatRearPassenger4Position);
            seatIndex = seatRearPassenger4 === -1 ? seatIndex : seatIndex + 1;

            if (calculatedDistance !== null && vehicle.isSeatFree(seatIndex) && calculatedDistance < closestSeatDistance) {
                closestSeatDistance = calculatedDistance;
                closestFreeSeatNumber = seatIndex;
            }
    
            calculatedDistance = seatRearDriver5Position === null ? null : calcDist(playerPos, seatRearDriver5Position);
            seatIndex = seatRearDriver5 === -1 ? seatIndex : seatIndex + 1;

            if (calculatedDistance !== null && vehicle.isSeatFree(seatIndex) && calculatedDistance < closestSeatDistance) {
                closestSeatDistance = calculatedDistance;
                closestFreeSeatNumber = seatIndex;
            }
    
            calculatedDistance = seatRearPassenger5Position === null ? null : calcDist(playerPos, seatRearPassenger5Position);
            seatIndex = seatRearPassenger5 === -1 ? seatIndex : seatIndex + 1;

            if (calculatedDistance !== null && vehicle.isSeatFree(seatIndex) && calculatedDistance < closestSeatDistance) {
                closestSeatDistance = calculatedDistance;
                closestFreeSeatNumber = seatIndex;
            }
    
            calculatedDistance = seatRearDriver6Position === null ? null : calcDist(playerPos, seatRearDriver6Position);
            seatIndex = seatRearDriver6 === -1 ? seatIndex : seatIndex + 1;

            if (calculatedDistance !== null && vehicle.isSeatFree(seatIndex) && calculatedDistance < closestSeatDistance) {
                closestSeatDistance = calculatedDistance;
                closestFreeSeatNumber = seatIndex;
            }
    
            calculatedDistance = seatRearPassenger6Position === null ? null : calcDist(playerPos, seatRearPassenger6Position);
            seatIndex = seatRearPassenger6 === -1 ? seatIndex : seatIndex + 1;

            if (calculatedDistance !== null && vehicle.isSeatFree(seatIndex) && calculatedDistance < closestSeatDistance) {
                closestSeatDistance = calculatedDistance;
                closestFreeSeatNumber = seatIndex;
            }
    
            calculatedDistance = seatRearDriver7Position === null ? null : calcDist(playerPos, seatRearDriver7Position);
            seatIndex = seatRearDriver7 === -1 ? seatIndex : seatIndex + 1;

            if (calculatedDistance !== null && vehicle.isSeatFree(seatIndex) && calculatedDistance < closestSeatDistance) {
                closestSeatDistance = calculatedDistance;
                closestFreeSeatNumber = seatIndex;
            }
    
            calculatedDistance = seatRearPassenger7Position === null ? null : calcDist(playerPos, seatRearPassenger7Position);
            seatIndex = seatRearPassenger7 === -1 ? seatIndex : seatIndex + 1;

            if (calculatedDistance !== null && vehicle.isSeatFree(seatIndex) && calculatedDistance < closestSeatDistance) {
                closestSeatDistance = calculatedDistance;
                closestFreeSeatNumber = seatIndex;
            }
    
            if (closestFreeSeatNumber === -1) {
                return; // No closest passenger seat, single seater?
            }
    
            const lastAnimatableSeatOverrides = {
                [mp.game.joaat('journey')]: driverSeatId + 1,
                [mp.game.joaat('journey2')]: driverSeatId + 1
            };
    
            let lastAnimatableSeatIndex = driverSeatId + 3;

            if (lastAnimatableSeatOverrides[vehicle.model] !== undefined) {
                lastAnimatableSeatIndex = lastAnimatableSeatOverrides[vehicle.model];
            }
    
            if (closestFreeSeatNumber <= lastAnimatableSeatIndex) {
                // Normal Enter
                mp.players.local.taskEnterVehicle(vehicle.handle, 5000, closestFreeSeatNumber, 2.0, 1, 0);
            } else {
                // Warp Enter
                mp.game.invoke('0x9A7D091411C5F684', mp.players.local.handle, vehicle.handle, closestFreeSeatNumber);
            }
        }
    }
});