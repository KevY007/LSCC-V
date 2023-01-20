/* Camera object */
let heliCam = null;

/* Camera settings */
const mouseSensitivity = 4;
const zoomSpeed = 1.5;
const minZoom = 5.0;
const maxZoom = 60.0;

/* Sync & Updates */
let lastHeliCamUpdate = 0;

mp.events.add("startHeliCam", () => {
    if(!mp.players.local.vehicle || !mp.players.local.isInAnyVehicle(false) || mp.players.local.vehicle.getClass() != 15 || heliCam !== null) return;

    heliCam = mp.cameras.new('default', mp.players.local.position, new mp.Vector3(0,0,0), 40);

    heliCam.attachTo(mp.players.local.vehicle.handle, 0, 0, -2.0, false);
    heliCam.setActive(true);

    mp.game.cam.renderScriptCams(true, false, 0, true, false);
    mp.events.callRemote("HeliCam_OnStart");
});
mp.events.add("stopHeliCam", () => {
    if(heliCam == null) return;
    mp.events.callRemote("HeliCam_OnStop");

    mp.game.cam.renderScriptCams(false, false, 0, true, false);
    heliCam.setActive(false);
    heliCam.destroy();
    heliCam = null;    
});

// Check if camera is pointing at vehicle
mp.events.add("getPointingAt", (distance) => {pointingAt(distance)});
function pointingAt(distance) {
	if (heliCam === null && heliCam.isActive() && heliCam.isRendering()) return;

    let ccposition = heliCam.getCoord(); // grab the position of the gameplay camera as Vector3

    let ccdirection = heliCam.getDirection(); // get the forwarding vector of the direction you aim with the gameplay camera as Vector3

    let farAway =  new mp.Vector3((ccdirection.x * distance) + (ccposition.x), (ccdirection.y * distance) + (ccposition.y), (ccdirection.z * distance) + (ccposition.z)); 

    let result = mp.raycasting.testPointToPoint(ccposition, farAway, [mp.players.local], 2); // now test point to point - intersects with map and objects [1 + 16]

    return result; // and return the result ( undefined, if no hit )
}

mp.events.addCommand("hdbg", () => {
    mp.events.callLocal("startHeliCam");
})
mp.events.addCommand("shdbg", () => {
    mp.events.callLocal("stopHeliCam");
})

mp.events.add('render', () => {
	if (heliCam === null || !heliCam.isActive() || !heliCam.isRendering()) return;
    if(pointingAt(1000) === undefined) return;

    const rayData = pointingAt(1000);
    const {position:carPosition, entity:carEntity} = rayData;
    if(carEntity.type !== 'vehicle') return;

    const veh = Object.values(mp.vehicles).find(v => v.handle === carEntity.handle);
    //const dimensions = mp.game.gameplay.getModelDimensions(veh.model);
    //const minPos = veh.getOffsetFromInWorldCoords(dimensions.minimum.x, dimensions.minimum.y, dimensions.minimum.z);
    //const maxPos = veh.getOffsetFromInWorldCoords(dimensions.maximum.x, dimensions.maximum.y, dimensions.maximum.z);

    mp.game.graphics.drawMarker(
        0,
        carPosition.x, carPosition.y, carPosition.z + 2,
        0, 0, 0,
        0, 0, 0,
        1.0, 1.0, 1.0,
        200, 200, 0, 128,
        false, false, 2,
        false, null, null, false,
      );
    mp.game.graphics.drawText(`${mp.game.vehicle.getDisplayNameFromVehicleModel(veh.model).toUpperCase()}`, [carPosition.x, carPosition.y, carPosition.z],{
        font:2,
        color:[255, 255, 255, 200],
        outline:true,
        scale:[0.5, 0.5],
        centre:true
    });

});

// Sync
mp.events.add('render', () => {
    mp.players.streamed.forEach(target => {
        if(target.getVariable("HeliCam") === true && target.getVariable("HeliCamLight") === true 
            && target.getVariable("HeliCamPosition") !== undefined){

                const {x,y,z,fx,fy,fz} = JSON.parse(target.getVariable("HeliCamPosition"));
                //const dist = Math.round(mp.game.system.vdist2(x, y, z, mp.players.local.position.x, mp.players.local.position.y, mp.players.local.position.z) / 3);
                mp.game.graphics.drawSpotLightWithShadow(x,y,z,fx,fy,fz, 255, 255, 255, 500, 10, 1, 12, 1, 0);
                mp.game.graphics.drawLightWithRange(x,y,z, 255, 255, 255, 1, 10);
        }
    });
});

// Camera Movement
mp.events.add('render', () => {
	if (heliCam !== null && heliCam.isActive() && heliCam.isRendering()) {
        mp.game.controls.disableAllControlActions(2);

        let x = (mp.game.controls.getDisabledControlNormal(7, 1) * mouseSensitivity);
        let y = (mp.game.controls.getDisabledControlNormal(7, 2) * mouseSensitivity);
        let zoomIn = (mp.game.controls.getDisabledControlNormal(2, 40) * zoomSpeed);
        let zoomOut = (mp.game.controls.getDisabledControlNormal(2, 41) * zoomSpeed);

        let currentRot = heliCam.getRot(2);

        currentRot = new mp.Vector3(currentRot.x - y, 0, currentRot.z - x);

        heliCam.setRot(currentRot.x, currentRot.y, currentRot.z, 2);

        if (zoomIn > 0)
        {
            let currentFov = heliCam.getFov();
            currentFov -= zoomIn;
            if (currentFov < minZoom)
                currentFov = minZoom;
            heliCam.setFov(currentFov);
        } else if (zoomOut > 0)
        {
            let currentFov = heliCam.getFov();
            currentFov += zoomOut;
            if (currentFov > maxZoom)
                currentFov = maxZoom;
            heliCam.setFov(currentFov);
        }

        if(Date.now() - lastHeliCamUpdate > 100){
            const forward = heliCam.getDirection();
            const position = heliCam.getCoord();

            mp.events.callRemoteUnreliable("HeliCam_UpdatePosition", position.x, position.y, position.z, forward.x, forward.y, forward.z);
        }
    }
});