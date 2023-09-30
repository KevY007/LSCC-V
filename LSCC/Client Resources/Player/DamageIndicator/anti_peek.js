var global = require('./globals');

var isAntiPeekOn = false;
var antiPeekRealCoord = null;

let wasMouseOnBeforeAntipeek = false;

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

mp.events.add('playerTaskWeaponShot', (entity, x, y, z, accurate) => {
    mp.game.invoke("0x96A05E4FB321B1BA", entity.handle, x, y, z, accurate); // SET_PED_SHOOTS_AT_COORD   
});

function disableAntiPeekFire(){
    mp.game.controls.disableControlAction(32, 24, true);
    mp.game.controls.disableControlAction(32, 140, true);
    mp.game.controls.disableControlAction(32, 141, true);
    mp.game.controls.disableControlAction(32, 142, true);
    mp.game.controls.disableControlAction(32, 257, true);
    mp.game.controls.disableControlAction(32, 263, true);
    mp.game.controls.disableControlAction(32, 264, true);   
}

mp.events.add('render', () => {
    if(!isAntiPeekOn && wasMouseOnBeforeAntipeek == true){
        disableAntiPeekFire();
    }
});

function shootWeaponAtRealPosition(){
    if(!isAntiPeekOn || !antiPeekRealCoord) return;
    wasMouseOnBeforeAntipeek = true;

    const weaponEntityIndex = mp.game.invoke('0x3B390A939AF0B5FC', mp.players.local.handle);
    let weapCoord = mp.players.local.getWorldPositionOfBone(mp.players.local.getBoneIndexByName("BONETAG_R_HAND"));
    const weaponRaycast = RayCastGamePlayWeapon(weaponEntityIndex, 8.0, 1);
    const entityRaycast1 = mp.raycasting.testPointToPoint(weapCoord, weaponRaycast.position, mp.players.local, 4);
    const entityRaycast2 = mp.raycasting.testPointToPoint(weapCoord, weaponRaycast.position, mp.players.local, 8);
    
    mp.game.invoke("0x96A05E4FB321B1BA", mp.players.local.handle, antiPeekRealCoord.x, antiPeekRealCoord.y, antiPeekRealCoord.z, false); // SET_PED_SHOOTS_AT_COORD
    mp.events.callRemote("SyncPlayerWeaponShot", antiPeekRealCoord.x, antiPeekRealCoord.y, antiPeekRealCoord.z, false);
    if(entityRaycast1 && entityRaycast1.entity){
        mp.events.callLocal("playerWeaponShot", antiPeekRealCoord, entityRaycast1.entity);
        return;
    }
    if(entityRaycast2 && entityRaycast2.entity){
        mp.events.callLocal("playerWeaponShot", antiPeekRealCoord, entityRaycast2.entity);
        return;
    }
    mp.events.callLocal("playerWeaponShot", antiPeekRealCoord, null);
}
mp.events.add('click', (x, y, upOrDown, leftOrRight, relativeX, relativeY, worldPosition, hitEntity) => {
    if(leftOrRight == "left"){
        if(upOrDown == "down")
            shootWeaponAtRealPosition();
        else{
            if(wasMouseOnBeforeAntipeek == true) wasMouseOnBeforeAntipeek = false;
        }
    }
});

mp.events.add('render', () => {
    if(mp.players.local.getVariable("Team") != 0 && mp.players.local.getVariable("Team") != 1) return;
    if(mp.players.local.weapon === 2725352035){
        global.HideAllCrosshairs.AntiPeek = false;
        isAntiPeekOn = false;
        if(!isAntiPeekOn && wasMouseOnBeforeAntipeek == true){
            disableAntiPeekFire();
        }
        return;
    }
    if(mp.game.invoke('0x68EDDA28A5976D07') !== 1 && !mp.game.player.isFreeAiming()){
        global.HideAllCrosshairs.AntiPeek = false;
        isAntiPeekOn = false;
        if(!isAntiPeekOn && wasMouseOnBeforeAntipeek == true){
            disableAntiPeekFire();
        }
        return;
    }

    const weaponEntityIndex = mp.game.invoke('0x3B390A939AF0B5FC', mp.players.local.handle);
    if(weaponEntityIndex === undefined || weaponEntityIndex === 0){
        global.HideAllCrosshairs.AntiPeek = false;
        isAntiPeekOn = false;
        if(!isAntiPeekOn && wasMouseOnBeforeAntipeek == true){
            disableAntiPeekFire();
        }
        return;
    }
    let weapCoord = mp.players.local.getWorldPositionOfBone(mp.players.local.getBoneIndexByName("BONETAG_R_HAND"));   

    const weaponRaycast = RayCastGamePlayWeapon(weaponEntityIndex, 8.0, 1);

    if(!weaponRaycast){
        global.HideAllCrosshairs.AntiPeek = false;
        isAntiPeekOn = false;
        if(!isAntiPeekOn && wasMouseOnBeforeAntipeek == true){
            disableAntiPeekFire();
        }
        return;
    }
    const entityRaycast1 = mp.raycasting.testPointToPoint(weapCoord, weaponRaycast.position, mp.players.local, 4);
    const entityRaycast2 = mp.raycasting.testPointToPoint(weapCoord, weaponRaycast.position, mp.players.local, 8);
    if((entityRaycast1 && entityRaycast1.entity.type === 'player') || (entityRaycast2 && entityRaycast2.entity.type === 'player')){
        global.HideAllCrosshairs.AntiPeek = false;
        isAntiPeekOn = false;
        if(!isAntiPeekOn && wasMouseOnBeforeAntipeek == true){
            disableAntiPeekFire();
        }
        return;
    }

    mp.game.graphics.drawText("\+", [weaponRaycast.position.x, weaponRaycast.position.y, weaponRaycast.position.z], {
        color:[255, 255, 255, 175],
        outline:true,
        font:0,
        scale:[0.35, 0.35]
        //scale:[0.42, 0.39]
    });
    antiPeekRealCoord = {x:weaponRaycast.position.x, y:weaponRaycast.position.y, z:weaponRaycast.position.z}

    global.HideAllCrosshairs.AntiPeek = true;
    isAntiPeekOn = true;
    if(checkIfFirePressed() == true) wasMouseOnBeforeAntipeek = true;

    mp.game.ui.hideHudComponentThisFrame(14); // RETICULE
    // Disable shooting
    disableAntiPeekFire();
});

function checkIfFirePressed(){
    if(mp.game.controls.isControlPressed(32, 24)) return true;
    if(mp.game.controls.isControlPressed(32, 140)) return true;
    if(mp.game.controls.isControlPressed(32, 141)) return true;
    if(mp.game.controls.isControlPressed(32, 142)) return true;
    if(mp.game.controls.isControlPressed(32, 257)) return true;
    if(mp.game.controls.isControlPressed(32, 263)) return true;
    if(mp.game.controls.isControlPressed(32, 264)) return true;
    return false;
}