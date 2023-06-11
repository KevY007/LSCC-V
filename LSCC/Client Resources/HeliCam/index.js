var {
    currentJailTime,
    lastJailUpdate,
    chaseRunning,
    chaseTimeCur,
    chaseTimeMax,
    chaseMode,
    collisionTime,
    updateRate,
    radioToggle,
    carryTarget,
    chatStatus,
    chatStatusLate,
    menuToggledLate,
    lastChatToggle,
    streetName,
    zoneName,
    showHud,
    renderNametags,
    currentMuteTime,
    lastMuteUpdate,
    lastFunc,
    curMDCBind,
    lastMDCBind,
    curActionMenuBind,
    lastActionMenuBind,
    IsInTutorial,
    BankRobberyOn,
    ShowVehicleDL,
    specState, 
    lastGPSBind,
    curGPSBind,
    lastMapBind,
    curMapBind,
    specTarget,
    isNoClip,
    LastKeyPress,
    AFKStage,
    SecondsSinceLastKeyPress,
    antiAFKActive,
    showReticule,
    showCustomCrosshair,
    showCustomCrosshairInSniper,
    alwaysShowCustomCrosshair,
    customCrosshairOn,
    ServerUI,
    menuToggled,
    scoreboardToggled,
    scoreboardCursor,
    StationPickerToggled,
    MDC_DOM,
    MDCActive,
    isLockpicking,
    isHotwiring,
    HotwireDOM,
    ClothingEditorDOM,
    ClothingEditorActive,
    ActionMenuActive,
    VehicleSpawnerActive,
    WeaponEditorActive,
    FireModeDisplayText,
    cuffTarget,
    cuffInterval,
    boneData,
    weaponData,
    loginSceneEntities,
    chatStatusTimeout,
    heliCam,
    LogPlayerDamage,
    ToggleMDC,
    actionMenuClose,
    actionMenuOpen,
    traceLastFunc,
    isHeliCamActive
} = require('./globals');
  const {TranslateMessage: Translate} = require('./Translator');
/* Camera settings */
const mouseSensitivity = 4;
const zoomSpeed = 1.5;
const minZoom = 5.0;
const maxZoom = 60.0;

/* Misc */
let targetVehicleHandle = null;
let focusedVehicle = null;
let HeliCamCEF = null;
let LightToggled = true;
let CurrentView = 1;
let playerTarget = null;
let focusedPlayer = null;

/* Sync & Updates */
let lastHeliCamUpdate = 0;

mp.events.add("startHeliCam", () => {
    if(!mp.players.local.vehicle || !mp.players.local.isInAnyVehicle(false) || mp.players.local.vehicle.getClass() != 15 || heliCam !== null) return;
    ServerUI.active = false;

    HeliCamCEF = mp.browsers.new("package://HeliCam/index.html");
    HeliCamCEF.active = true;
    mp.game.ui.displayRadar(false);
    
    LightToggled = true;
    CurrentView = 1;

    heliCam = mp.cameras.new('default', mp.players.local.position, new mp.Vector3(0,0,0), 40);

    heliCam.attachTo(mp.players.local.vehicle.handle, 0, 0, -2.0, false);
    heliCam.setActive(true);
    heliCam.setFov(maxZoom);

    mp.game.cam.renderScriptCams(true, false, 0, true, false);
    mp.events.callRemote("HeliCam_OnStart");

    targetVehicleHandle = null;
    focusedVehicle = null;
    playerTarget = null;
    focusedPlayer = null;
    displayLostSignal = false;
});
mp.keys.bind(0x11, false, () => {
	if (heliCam === null && heliCam.isActive() && heliCam.isRendering()) return;

    mp.events.callLocal("stopHeliCam");
});
mp.keys.bind(0x4C, false, () => {
	if (heliCam === null && heliCam.isActive() && heliCam.isRendering()) return;

    LightToggled = !LightToggled;
    mp.events.callRemote("HeliCam_ToggleLight", LightToggled);

    HeliCamCEF.execute(`ToggleLight(${LightToggled})`);
});
mp.keys.bind(0x4B, false, () => {
	if (heliCam === null && heliCam.isActive() && heliCam.isRendering()) return;

    CurrentView++;
    if(CurrentView > 3) CurrentView = 1;

    switch(CurrentView){
        case 1:
            mp.game.graphics.setSeethrough(false);
            mp.game.graphics.setNightvision(false);
            break;
        case 2:
            mp.game.graphics.setSeethrough(false);
            mp.game.graphics.setNightvision(true);
            break;
        case 3:
            mp.game.graphics.setNightvision(false);
            mp.game.graphics.setSeethrough(true);
            break;
    }

    HeliCamCEF.execute(`SetView(${CurrentView})`);
});
mp.events.add("stopHeliCam", () => {
    if(heliCam == null) return;

    if(ServerUI)
        ServerUI.active = true;

    if(HeliCamCEF){
        HeliCamCEF.active = false;
        HeliCamCEF.destroy();
    }

    mp.events.callRemote("HeliCam_OnStop");

    mp.game.cam.renderScriptCams(false, false, 0, true, false);
    heliCam.setActive(false);
    heliCam.destroy();
    heliCam = null;    
    mp.game.ui.displayRadar(true);

    playerTarget = null;
    focusedPlayer = null;
    targetVehicleHandle = null;
    focusedVehicle = null;
    displayLostSignal = false;
    mp.game.graphics.setSeethrough(false);
    mp.game.graphics.setNightvision(false);
});

// Check if camera is pointing at vehicle
mp.events.add("getPointingAt", (distance) => {pointingAt(distance)});
function pointingAt(distance) {
	if (heliCam === null && heliCam.isActive() && heliCam.isRendering()) return;
    if(focusedPlayer !== null || focusedVehicle !== null) return;

    let ccposition = heliCam.getCoord(); // grab the position of the gameplay camera as Vector3

    let ccdirection = heliCam.getDirection(); // get the forwarding vector of the direction you aim with the gameplay camera as Vector3

    let farAway =  new mp.Vector3((ccdirection.x * distance) + (ccposition.x), (ccdirection.y * distance) + (ccposition.y), (ccdirection.z * distance) + (ccposition.z)); 

    let result = mp.raycasting.testPointToPoint(ccposition, farAway, [mp.players.local], 14); // 2 + 4 + 8 = 14 peds peds and cars

    return result; // and return the result ( undefined, if no hit )
}

// mp.events.addCommand("hdbg", () => {
//     mp.events.callLocal("startHeliCam");
// })
// mp.events.addCommand("shdbg", () => {
//     mp.events.callLocal("stopHeliCam");
// });

mp.events.add('click', (x, y, upOrDown, leftOrRight, relativeX, relativeY, worldPosition, hitEntity) => {
	if (heliCam === null || !heliCam.isActive() || !heliCam.isRendering()) return;
    displayLostSignal = false;

    if(upOrDown == "down" && leftOrRight == "left"){
        if(targetVehicleHandle == null && playerTarget == null) return;

        if(targetVehicleHandle){
            const veh = Object.values(mp.vehicles).find(v => v.handle === targetVehicleHandle);
            if(veh == undefined) return;
    
            heliCam.pointAt(veh.handle, 0, 0, 0, true);
            focusedVehicle = veh;
    
            HeliCamCEF.execute(`ToggleFocusPanel(true);`);
            UpdateHeliCamCEF(veh);
            HeliCamCEF.execute(`SetCrosshair('lock')`);
        }
        else if(playerTarget){
            focusedPlayer = playerTarget;

            heliCam.pointAt(focusedPlayer.handle, 0, 0, 0, true);
            HeliCamCEF.execute(`SetCrosshair('lock')`);
            UpdateHeliCamPlayerCEF(focusedPlayer);
            HeliCamCEF.execute(`TogglePlayerFocusPanel(true);`);
        }  
    }
    if(upOrDown == "down" && leftOrRight == "right"){
        if(focusedVehicle == null && focusedPlayer == null) return;

        HeliCamCEF.execute(`ToggleFocusPanel(false);`);
        HeliCamCEF.execute(`TogglePlayerFocusPanel(false);`);
        heliCam.stopPointing();
        focusedVehicle = null;
        focusedPlayer = null;
        targetVehicleHandle = null;
        playerTarget = null;
        HeliCamCEF.execute(`SetCrosshair('crosshair')`);
    }
});

let HeliCamCEFFrames = 0;

function UpdateHeliCamCEF_Zoom(zoom){
    if(!HeliCamCEF) return;

    const realZoom = convertZoomToPercentage(zoom);
    HeliCamCEF.execute(`SetZoomValue(${realZoom})`);
}

mp.events.add("playerLeaveVehicle", (vehicle, seat) => {
	if (heliCam === null || !heliCam.isActive() || !heliCam.isRendering()) return;

    mp.events.callLocal("stopHeliCam");    
});

function UpdateHeliCamCEF(vehicle){
    if(!HeliCamCEF) return;
     const fuel = vehicle.getVariable("Fuel");
     const speed = (vehicle.getSpeed() * 3.6).toFixed(0);
//
     const getStreet = mp.game.pathfind.getStreetNameAtCoord(vehicle.position.x, vehicle.position.y, vehicle.position.z, 0, 0);
     const zoneName = mp.game.ui.getLabelText(mp.game.zone.getNameOfZone(vehicle.position.x, vehicle.position.y, vehicle.position.z));
     const streetName = mp.game.ui.getStreetNameFromHashKey(getStreet.streetName);
//
//
     //const fuelStr = fuel ? `${Math.floor(fuel)}%` : `∞`;
     const speedStr = speed ? `${speed}KM/H` : `0KM/H`;
     //const nameStr = name;
     const locStr = `${streetName}, ${zoneName}`;
     const name = mp.game.vehicle.getDisplayNameFromVehicleModel(vehicle.model).toUpperCase();

        let passengerCnt = 0;
        mp.players.forEachInStreamRange(player => {
            if(player.vehicle && player.vehicle.handle === vehicle.handle) passengerCnt++;
        });


     HeliCamCEF.execute(`UpdateDataPanel('${name}', ${passengerCnt}, '${locStr}', '${speedStr}');`);
}

function UpdateHeliCamPlayerCEF(target){
    const health = target.getHealth();
    HeliCamCEF.execute(`UpdatePlayerDataPanel(${health}, '${target.name}');`);
}

let displayLostSignal = false;
mp.events.add('entityStreamOut', (entity) => {
    if(focusedPlayer === entity || focusedVehicle === entity){
        HeliCamCEF.execute(`ToggleFocusPanel(false);`);
        HeliCamCEF.execute(`TogglePlayerFocusPanel(false);`);
        heliCam.stopPointing();
        focusedVehicle = null;
        focusedPlayer = null;
        HeliCamCEF.execute(`SetCrosshair('crosshair')`);

        displayLostSignal = true;
        setTimeout(() => {
            displayLostSignal = false;
        }, 1000);
    }
});

mp.events.add('render', () => {
	if (heliCam === null || !heliCam.isActive() || !heliCam.isRendering()) return;

    if(displayLostSignal === true){
        mp.game.graphics.drawText("SIGNAL LOST", [0.5, 0.6], {
            font:2,
            color:[255,0,0,255],
            outline:true,
            scale:[0.5, 0.5],
            centre:true
        });
    }

    if(focusedVehicle !== null){
        HeliCamCEFFrames++;
        if(HeliCamCEFFrames > 500){
            UpdateHeliCamCEF(focusedVehicle);
            HeliCamCEFFrames = 0;
        }
        return;
    }
    if(focusedPlayer !== null){
        HeliCamCEFFrames++;
        if(HeliCamCEFFrames > 500){
            UpdateHeliCamPlayerCEF(focusedPlayer);
            HeliCamCEFFrames = 0;
        }
        return;  
    }

    if(pointingAt(1000) === undefined){    
        playerTarget = null;
        targetVehicleHandle = null;
        HeliCamCEF.execute(`SetCrosshair('crosshair')`);
        return;
    }

    const rayData = pointingAt(1000);
    const {position:carPosition, entity:carEntity} = rayData;
    if(carEntity.type !== 'vehicle'){
        targetVehicleHandle = null;
        if(carEntity.type !== 'player'){       
            playerTarget = null;
            HeliCamCEF.execute(`SetCrosshair('crosshair')`);
        }
        else{
            if(playerTarget == null){
                mp.players.forEachInStreamRange(pl => {
                    if(pl.dimension === mp.players.local.dimension && pl.handle === rayData.entity.handle){
                        mp.game.graphics.drawText(`${pl.name}`, [0.5, 0.65],{
                            font:2,
                            color:(pl.getVariable("Team") === 0 ? [255, 177, 0, 200] : [255, 255, 255, 200]),
                            outline:true,
                            scale:[0.5, 0.5],
                            centre:true
                        });
                        playerTarget = pl;
                        HeliCamCEF.execute(`SetCrosshair('target')`);
                        return;
                    }
                })
            }
            else{
                mp.game.graphics.drawText(`${playerTarget.name}`, [0.5, 0.65],{
                    font:2,
                    color:(playerTarget.getVariable("Team") === 0 ? [255, 177, 0, 200] : [255, 255, 255, 200]),
                    outline:true,
                    scale:[0.5, 0.5],
                    centre:true
                });
            }     
        }  
        return;      
    }

    if(carEntity.handle != targetVehicleHandle){
        targetVehicleHandle = carEntity.handle;
        HeliCamCEF.execute(`SetCrosshair('target')`);
    }

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
        if(target !== mp.players.local && target.getVariable("HeliCam") === true && target.getVariable("HeliCamLight") === true ){

                const x = target.getVariable("HeliPosX");
                const y = target.getVariable("HeliPosY");
                const z = target.getVariable("HeliPosZ");
                const fx = target.getVariable("HeliForX");
                const fy = target.getVariable("HeliForY");
                const fz = target.getVariable("HeliForZ");

                if(x && y && z && fx && fy && fz){
                    //const dist = Math.round(mp.game.system.vdist2(x, y, z, mp.players.local.position.x, mp.players.local.position.y, mp.players.local.position.z) / 3);
                    mp.game.graphics.drawSpotLightWithShadow(x,y,z,fx,fy,fz, 255, 255, 255, 500, 3, 1, 12, 1, 0);
                    mp.game.graphics.drawLightWithRange(x,y,z, 255, 255, 255, 1, 10);
                }
           
        }
    });
    
});

function convertZoomToPercentage(value){
    return Math.round(((maxZoom-value)/(maxZoom-minZoom)).toFixed(2)*100);
}
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

            UpdateHeliCamCEF_Zoom(currentFov);
        } else if (zoomOut > 0)
        {
            let currentFov = heliCam.getFov();
            currentFov += zoomOut;
            if (currentFov > maxZoom)
                currentFov = maxZoom;
            heliCam.setFov(currentFov);

            UpdateHeliCamCEF_Zoom(currentFov);
        }
        
        const forward = heliCam.getDirection();
        const position = heliCam.getCoord();
        if(LightToggled){
            mp.game.graphics.drawSpotLightWithShadow(position.x, position.y, position.z, forward.x, forward.y, forward.z, 255, 255, 255, 500, 3, 1, 12, 1, 0);
            mp.game.graphics.drawLightWithRange(position.x, position.y, position.z, 255, 255, 255, 1, 10);   
            if(Date.now() - lastHeliCamUpdate > 100){
                lastHeliCamUpdate = Date.now();
                mp.events.callRemoteUnreliable("HeliCam_UpdatePosition", position.x, position.y, position.z, forward.x, forward.y, forward.z);
            }  
        }      
    }
});