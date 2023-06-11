var global = require('./globals');
  const {TranslateMessage: Translate} = require('./Translator');
mp.events.add('render', () => {

    if(mp.players.local.getVariable("pLogged") !== undefined && mp.players.local.getVariable("pLogged") == true)
    {
        mp.players.streamed.forEach(target => 
        {
            if(target.weapon !== 0 && target.weapon != 2725352035)
            {
                if(target.getVariable("LaserEquipped") == true)
                {
                    if(target !== mp.players.local){
                        if(target.getVariable("IsAiming") == true && target.getVariable("CameraDirectionX") !== undefined && target.getVariable("CameraDirectionX") !== null)
                        {
                            let handPos = target.getWorldPositionOfBone(target.getBoneIndexByName("BONETAG_R_PH_HAND"));  

                            mp.game.entity.get
                        
                            let farAway =  new mp.Vector3((target.getVariable("CameraDirectionX") * 300) + (handPos.x), (target.getVariable("CameraDirectionY") * 300) + (handPos.y), (target.getVariable("CameraDirectionZ") * 300) + (handPos.z)); 

                            let LOStest = mp.raycasting.testPointToPoint(handPos, farAway, [target], 512);

                            let finalPosition = undefined;
                            if(LOStest){
                                if(LOStest.position && LOStest.position.x && LOStest.position.y && LOStest.position.z){
                                    var distance = mp.game.system.vdist2(handPos.x, handPos.y, handPos.z, LOStest.position.x, LOStest.position.y, LOStest.position.z);
                                    finalPosition = new mp.Vector3((target.getVariable("CameraDirectionX") * distance) + (handPos.x), (target.getVariable("CameraDirectionY") * distance) + (handPos.y), (target.getVariable("CameraDirectionZ") * distance) + (handPos.z)); 
                                }
                                else finalPosition = farAway;
                            }
                            else finalPosition = farAway;

                            mp.game.graphics.drawLine(handPos.x, handPos.y, handPos.z, finalPosition.x, finalPosition.y, finalPosition.z, 255, 0, 0, 180);
                            //mp.game.graphics.drawSpotLightWithShadow(origin.x, origin.y, origin.z, target.getVariable("CameraDirectionX"), target.getVariable("CameraDirectionY"), target.getVariable("CameraDirectionZ"), 255, 0, 0, 300, 5, 1, 0.1, 1, 0);
                            //mp.game.graphics.drawLightWithRange(handPos.x, handPos.y, handPos.z, 255, 255, 255, 1, 8);
                        }
                    }
                    else{
                        if(target.getVariable("IsAiming") == true){
                            let handPos = target.getWorldPositionOfBone(target.getBoneIndexByName("BONETAG_R_PH_HAND")); 
                        
                            let camera = mp.cameras.new("gameplay");

                            let resolution = mp.game.graphics.getScreenActiveResolution();

                            let direction = mp.game.graphics.screen2dToWorld3d(new mp.Vector3(resolution.x/2, resolution.y/2));                            

                            mp.game.graphics.drawLine(handPos.x, handPos.y, handPos.z, direction.x, direction.y, direction.z, 255, 0, 0, 180);
                        }
                    }
                }
            }
        });
    }
});