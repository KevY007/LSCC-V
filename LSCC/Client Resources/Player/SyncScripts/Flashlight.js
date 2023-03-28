// FLASHLIGHT AND AIMING + DIRECTION SYNC
mp.events.add('render', () => {

    if(mp.players.local.getVariable("pLogged") !== undefined && mp.players.local.getVariable("pLogged") == true)
    {
        mp.players.streamed.forEach(target => 
        {
            if(target.weapon !== 0 && target.weapon != 2725352035/* && target != mp.players.local*/)
            {
                if(target != mp.players.local){
                    if(target.getVariable("FlashlightEquipped") == true)
                    {
                        if(target.getVariable("IsAiming") == true && target.getVariable("CameraDirectionX") !== undefined && target.getVariable("CameraDirectionX") !== null)
                        {
                            let handPos = target.getWorldPositionOfBone(target.getBoneIndexByName("BONETAG_R_HAND"));    
                            let forward = target.getForwardVector();
                        
                            let origin = new mp.Vector3(handPos.x + forward.x * 3.4, handPos.y + forward.y * 3.4, (handPos.z + forward.z * 3.4));
                                                                
                            mp.game.graphics.drawSpotLightWithShadow(origin.x, origin.y, origin.z, target.getVariable("CameraDirectionX"), target.getVariable("CameraDirectionY"), target.getVariable("CameraDirectionZ"), 255, 255, 255, 15, 2, 2, 20, 3, 1);
                            mp.game.graphics.drawLightWithRange(handPos.x, handPos.y, handPos.z, 255, 255, 255, 1, 8);
                        }
                    }
                }        
            }
        });
    }
});