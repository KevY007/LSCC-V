var lastWaypointStatus = false;


mp.events.add("playerCreateWaypoint", (position) => {

    if(!lastWaypointStatus) // Only if NEWLY created waypoint. Not like if it was force synced before.
    {
        mp.events.callRemote("SetWaypoint", position.x, position.y, position.z);

        lastWaypointStatus = true;
    }
});

mp.events.add("playerDisableWaypoint", () => {

    if(lastWaypointStatus) // So this does not trigger on FORCESYNCED calls.
    {
        mp.events.callRemote("DisableWaypoint");

        lastWaypointStatus = false;
    }
});

mp.events.add("gameSetWaypoint", function (x, y) {
    lastWaypointStatus = true;
    mp.game.ui.setNewWaypoint(x, y);
});

mp.events.add("gameDisableWaypoint", function () {
    lastWaypointStatus = false;
    mp.game.invoke('0xA7E4E2D361C2627F'); // VOID SET_WAYPOINT_OFF()
});

mp.events.add("OneSecondEvent", () => {
    if(!mp.game.invoke('0x1DD1F58F493F1DA5') && lastWaypointStatus) { // BOOL IS_WAYPOINT_ACTIVE() 
        mp.events.callLocal("playerDisableWaypoint");
    }
});

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

mp.events.addDataHandler('EntAlpha', function (entity, value, oldValue) 
{
    entity.setAlpha(value);
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
        entity.setEnginePowerMultiplier(value);
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
        if(entity.getVariable("EntAlpha") !== undefined) entity.setAlpha(entity.getVariable("EntAlpha"));
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
            entity.setEnginePowerMultiplier(entity.getVariable("vInitialDriveForce"));
        }
    }
});


// Silent Siren
let localPlayer = mp.players.local;

mp.events.add('entityStreamIn', (entity) => {
    if (entity.type === 'vehicle' && entity.getClass() === 18 && entity.hasVariable('SilentSiren')) entity.getVariable('SilentSiren') ? entity.setSirenSound(true) : entity.setSirenSound(false);
    
});

mp.events.addDataHandler("SilentSiren", (entity, value) => {
    if (entity.type === "vehicle") entity.setSirenSound(value);
});