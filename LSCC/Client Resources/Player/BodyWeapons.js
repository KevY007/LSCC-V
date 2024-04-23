var global = require('./globals');
const BODY_WEAPON_DATA = [];

const PistolAttachmentPos = new mp.Vector3(0.02, 0.06, 0.1);
const PistolAttachmentRot = new mp.Vector3(-100.0, 0.0, 0.0);
const PistolAttachmentBone = 14;

const SMGAttachmentPos = new mp.Vector3(0.08, 0.03, -0.1);
const SMGAttachmentRot = new mp.Vector3(-80.77, 0.0, 0.0);
const SMGAttachmentBone = 2;

const ShotgunAttachmentPos = new mp.Vector3(-0.1, -0.15, 0.11);
const ShotgunAttachmentRot = new mp.Vector3(-180.0, 0.0, 0.0);
const ShotgunAttachmentBone = 38;

const RifleAttachmentPos = new mp.Vector3(-0.1, -0.15, -0.13);
const RifleAttachmentRot = new mp.Vector3(0.0, 0.0, 3.5);
const RifleAttachmentBone = 38;

/*
Every weapon has its own object stored in the BODY_WEAPON_DATA array. The script will sync objects based on this array.

Whenever a change to an entity's "BODY_WEAPONS" variable happens, ONLY the updated body weapon objects should change here.
This is to prevent stupid workarounds like removing all weapons and re-adding them for the entiry, which would result in visual bugs.

The way it works is:
1) Find all weapon data belonging to the entity.
2) Go through found data and compare it to the server weapon data which has been changed.
2.1) First check for updated objects and update weapons as needed (weapon objs with "isVisible" false must be removed and vice versa)
2.2) Check for new weapons and add them to body weapon data. (create objs and sync)
2.3) Completely remove body weapons which are no longer in server data. (remove objs)

object example:
{
    entity,
    weaponHash,
    objectHash,
    isVisible,
    object, ====> the object 'entity' which can be added/removed/attached
}

*/

mp.events.addDataHandler("BODY_WEAPONS", (entity, value, oldValue) => {
    if(entity.type !== 'player') return;
    if(!value) return;

    const sWeaponData = JSON.parse(value); // server weapon data
    // weapon hash, object hash, is visible

    const weaponsToRemove = [];

    // find all weapon data belonging to this entity
    const entityData = BODY_WEAPON_DATA.filter(w => w.entity.remoteId === entity.remoteId);
    entityData.forEach(wData => {
         const thisWeapon = sWeaponData.find(w => w.WeaponHash == wData.weaponHash);
        if(thisWeapon){
            const comparisonObj = {
                entity:entity, 
                weaponHash:thisWeapon.WeaponHash, 
                objectHash:thisWeapon.ModelHash, 
                isVisible:thisWeapon.IsVisible
            };
            if(!compareObject(comparisonObj, wData)){
                updateBodyWeapon(entity, wData, comparisonObj, thisWeapon);
            }
        }
        else{
            // if this weapon no longer exists in sWeaponData, it will be removed
            weaponsToRemove.push(wData);
        }
    });

    // filter all server weapons where searching for their hash and entity remote id in body weapon data returns NULL aka not found
    const weaponsToAdd = sWeaponData.filter(w => !(BODY_WEAPON_DATA.find(wd => wd.weaponHash == w.WeaponHash && wd.entity.remoteId == entity.remoteId)));

    addBodyWeapons(entity, weaponsToAdd);
    removeBodyWeapons(weaponsToRemove);
});

function updateBodyWeapon(entity, oldData, newData, newServerData){

    if(oldData.isVisible == true && newData.isVisible == false){
        removeBodyWeapons([oldData]);
    }
    if(oldData.isVisible == false && newData.isVisible == true){
        addBodyWeapons(entity, [newServerData]);
    }
}
function removeBodyWeapons(array){
    for(var i = 0 ; i < array.length ; i++){
        const bWeapon = array[i];
        if(bWeapon && bWeapon.object && mp.objects.exists(bWeapon.object)){
            bWeapon.object.destroy();
        }

        const toSplice = BODY_WEAPON_DATA.findIndex(w => w.entity.remoteId == bWeapon.entity.remoteId && w.weaponHash == bWeapon.weaponHash);
        BODY_WEAPON_DATA.splice(toSplice, 1);
    }
}
function addBodyWeapons(entity, array){
    array.forEach(sData => {
        var object = undefined;
        if(sData.IsVisible){
            object = mp.objects.new(mp.game.joaat(sData.ModelHash), entity.position, {
                rotation:entity.rotation,
                alpha:0,
                dimension:entity.dimension
            });
        }
        setTimeout(() => {
            if(sData.IsVisible && object && mp.objects.exists(object) && object.handle){
                const gunData = global.weaponData.find(w => Number(w.Hash) == Number(sData.WeaponHash));
                let category = "Assault Rifles";
                if(gunData) category = gunData.Type;
    
                switch(category){
                    case "Handguns":
                        object.attachTo(entity.handle, PistolAttachmentBone, PistolAttachmentPos.x, PistolAttachmentPos.y, PistolAttachmentPos.z,
                                                       PistolAttachmentRot.x, PistolAttachmentRot.y, PistolAttachmentRot.z,
                                                       false, true, true, true, 1, true);
                        break;
                    case "Submachine Guns":
                        object.attachTo(entity.handle, SMGAttachmentBone, SMGAttachmentPos.x, SMGAttachmentPos.y, SMGAttachmentPos.z,
                            SMGAttachmentRot.x, SMGAttachmentRot.y, SMGAttachmentRot.z,
                            false, true, true, true, 1, true);
                        break;
                    case "Shotguns":
                        object.attachTo(entity.handle, ShotgunAttachmentBone, ShotgunAttachmentPos.x, ShotgunAttachmentPos.y, ShotgunAttachmentPos.z,
                            ShotgunAttachmentRot.x, ShotgunAttachmentRot.y, ShotgunAttachmentRot.z,
                            false, true, true, true, 1, true);        
                        break;
                    case "Assault Rifles":
                        object.attachTo(entity.handle, RifleAttachmentBone, RifleAttachmentPos.x, RifleAttachmentPos.y, RifleAttachmentPos.z,
                            RifleAttachmentRot.x, RifleAttachmentRot.y, RifleAttachmentRot.z,
                            false, true, true, true, 1, true);
                        break;
                    case "Machine Guns":
                        object.attachTo(entity.handle, RifleAttachmentBone, RifleAttachmentPos.x, RifleAttachmentPos.y, RifleAttachmentPos.z,
                            RifleAttachmentRot.x, RifleAttachmentRot.y, RifleAttachmentRot.z,
                            false, true, true, true, 1, true);
                        break;
                    case "Sniper Rifles":
                        object.attachTo(entity.handle, RifleAttachmentBone, RifleAttachmentPos.x, RifleAttachmentPos.y, RifleAttachmentPos.z,
                            RifleAttachmentRot.x, RifleAttachmentRot.y, RifleAttachmentRot.z,
                            false, true, true, true, 1, true);
                        break;
                    default:
                        object.attachTo(entity.handle, ShotgunAttachmentBone, ShotgunAttachmentPos.x, ShotgunAttachmentPos.y, ShotgunAttachmentPos.z,
                            ShotgunAttachmentRot.x, ShotgunAttachmentRot.y, ShotgunAttachmentRot.z,
                            false, true, true, true, 1, true); 
                        break;
                }
                object.setAlpha(255);
            }
            //(mp.players.local.handle, 36, -0.1, -0.15, 0.11, -180.0, 0.0, 0.0, false, true, true, true, 1, true);
            const obj = {
                entity:entity,
                weaponHash:sData.WeaponHash,
                objectHash:sData.ModelHash,
                isVisible:sData.IsVisible,
                object:object
            };
            BODY_WEAPON_DATA.push(obj);
        }, 200);
    });
}


function compareObject(obj1, obj2){

    var cond1 = (obj1.entity.remoteId === obj2.entity.remoteId);
    var cond2 = (obj1.weaponHash === obj2.weaponHash);
    var cond3 = (obj1.objectHash === obj2.objectHash);
    var cond4 = (obj1.isVisible === obj2.isVisible);

     return (cond1 && cond2 && cond3 && cond4);
}