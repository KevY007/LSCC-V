var global = require('./globals');
  const {TranslateMessage: Translate} = require('./Translator');
function RayCastGamePlayWeapon(weapon, distance, flag){
    let cameraRotation = mp.cameras.new('gameplay').getRot(2);

    //let weapCoord = mp.game.invoke('0x3FEF770D40960D5A', weapon, false);
    let weapCoord = mp.players.local.getWorldPositionOfBone(mp.players.local.getBoneIndexByName("BONETAG_R_HAND"));    

    let cameraCoord = mp.cameras.new('gameplay').getCoord();
    let direction = mp.cameras.new('gameplay').getDirection();

    let destination = new mp.Vector3(cameraCoord.x + direction.x * distance,
                                     cameraCoord.y + direction.y * distance, 
		                             cameraCoord.z + direction.z * distance );

    const shapeResult = mp.raycasting.testPointToPoint(new mp.Vector3(weapCoord.x, weapCoord.y, weapCoord.z),
                                                       new mp.Vector3(destination.x, destination.y, destination.z), undefined, flag);
    if(shapeResult){
        return {
            destination:destination,
            position:shapeResult.position,
            normal:shapeResult.surfaceNormal,
            entity:shapeResult.entity
        }
    }
    else{
        return undefined;
    }
}

mp.events.add('render', () => {
    if(mp.players.local.getVariable("Team") != 0 && mp.players.local.getVariable("Team") != 1) return;
    if(mp.players.local.weapon === 2725352035){
        global.HideAllCrosshairs.AntiPeek = false;
        return;
    }
    if(mp.game.invoke('0x68EDDA28A5976D07') !== 1 && !mp.game.player.isFreeAiming()){
        global.HideAllCrosshairs.AntiPeek = false;
        return;
    }

    const weaponEntityIndex = mp.game.invoke('0x3B390A939AF0B5FC', mp.players.local.handle);
    if(weaponEntityIndex === undefined || weaponEntityIndex === 0){
        global.HideAllCrosshairs.AntiPeek = false;
        return;
    }
    let weapCoord = mp.players.local.getWorldPositionOfBone(mp.players.local.getBoneIndexByName("BONETAG_R_HAND"));   

    const weaponRaycast = RayCastGamePlayWeapon(weaponEntityIndex, 8.0, 1);

    if(!weaponRaycast){
        global.HideAllCrosshairs.AntiPeek = false;
        return;
    }
    const entityRaycast1 = mp.raycasting.testPointToPoint(weapCoord, weaponRaycast.position, mp.players.local, 4);
    const entityRaycast2 = mp.raycasting.testPointToPoint(weapCoord, weaponRaycast.position, mp.players.local, 8);
    if((entityRaycast1 && entityRaycast1.entity.type === 'player') || (entityRaycast2 && entityRaycast2.entity.type === 'player')){
        global.HideAllCrosshairs.AntiPeek = false;
        return;
    }

    mp.game.graphics.drawText("X", [weaponRaycast.position.x, weaponRaycast.position.y, weaponRaycast.position.z], {
        color:[255, 0, 0, 255],
        outline:true,
        font:2,
        scale:[0.42, 0.39]
    });

    global.HideAllCrosshairs.AntiPeek = true;
    mp.game.ui.hideHudComponentThisFrame(14); // RETICULE
    // Disable shooting
    mp.game.controls.disableControlAction(32, 24, true);
    mp.game.controls.disableControlAction(32, 140, true);
    mp.game.controls.disableControlAction(32, 141, true);
    mp.game.controls.disableControlAction(32, 142, true);
    mp.game.controls.disableControlAction(32, 257, true);
    mp.game.controls.disableControlAction(32, 263, true);
    mp.game.controls.disableControlAction(32, 264, true);
});