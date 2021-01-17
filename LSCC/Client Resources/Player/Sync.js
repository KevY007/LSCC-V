mp.events.addDataHandler('Status_Trunk', function (entity, value, oldValue) 
{
    if (entity.type === 'vehicle') 
    {
        if (value == true){
            entity.setDoorOpen(5, false, true);
            entity.setDoorOpen(6, false, true);
            entity.setDoorOpen(7, false, true);
        } else {
            entity.setDoorShut(5, true);
            entity.setDoorShut(6, true);
            entity.setDoorShut(7, true);
        }
    }
});

mp.events.addDataHandler('ComponentSyncTrigger', function (entity, value, oldValue) 
{
    if(entity != null && entity.type === 'player') mp.events.callRemote("RequestCompSync", entity);
});

mp.events.add("OpenVehicleDoor", (vehicle, doorId) => {
    if(vehicle.type === 'vehicle') vehicle.setDoorOpen(doorId, false, true);
});

mp.events.addDataHandler('vMaxSpeed', function (entity, value, oldValue) 
{
    if (entity.type === 'vehicle') 
    {
        entity.setMaxSpeed(value);
    }
});

mp.events.addDataHandler('vInitialDriveForce', function (entity, value, oldValue) 
{
    if (entity.type === 'vehicle') 
    {
        entity.setHandling("fInitialDriveForce", value);
    }
});

mp.events.addDataHandler('Engine', function (entity, value, oldValue) 
{
    if (entity.type === 'vehicle')
    {
        entity.setEngineOn(value, true, true);
    }
});

mp.events.add("AttachWeaponComponent", (target, weapon, comp) => {
    mp.game.invoke("0xD966D51AA5B28BB9", target.handle, weapon, comp);
    //mp.game.invoke("0xADF692B254977C0C", target.handle, weapon, true);
});

mp.events.add('entityStreamIn', (entity) => {

    if(entity != null && entity.type === 'player') 
    {
        mp.events.callRemote("RequestCompSync", entity);
    }
    if (entity.type === 'vehicle') 
    {
        if(entity.getVariable("Status_Trunk") !== undefined)
        {
            if (entity.getVariable("Status_Trunk") == true)
            {
                entity.setDoorOpen(5, false, true);
                entity.setDoorOpen(6, false, true);
                entity.setDoorOpen(7, false, true);
            } 
            else 
            {
                entity.setDoorShut(5, true);
                entity.setDoorShut(6, true);
                entity.setDoorShut(7, true);
            }
        }

        if(entity.getVariable("Engine") !== undefined)
        {
            entity.setEngineOn(entity.getVariable("Engine"), true, true);
        }

        if(entity.getVariable("vMaxSpeed") !== undefined)
        {
            entity.setMaxSpeed(entity.getVariable("vMaxSpeed"));
        }

        if(entity.getVariable("vInitialDriveForce") !== undefined)
        {
            entity.setHandling("fInitialDriveForce", entity.getVariable("vInitialDriveForce"));
        }
    }
});