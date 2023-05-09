var Indicators = [];
var SmartIndicators = [];

// TYPE: 0 = None | 1 = Broken | 2 = Armored

function getIndicatorColorFromEntity(entity, damage, type = 0){
    var color = [0,0,0];
    if(entity.remoteId !== mp.players.local.remoteId){
        if(damage < 40)
        switch(type){
            case 0: 
                color = [100, 255, 100, 255];
                break;
            case 1:
                color = [167, 100, 255, 255];
                break;
            case 2:
                color = [0, 162, 255, 255];
                break;

        }
        else
            switch(type){
                case 0:
                    color = [255, 200, 0, 255];
                    break;
                case 1:
                    color = [170, 59, 255, 255];
                    break;
                case 2:
                    color = [0, 128, 255, 255];
                    break;

            }
    }
    else{
        if(damage > -40)
            color = [255, 100, 100, 255];
        else 
            color = [200, 0, 0, 255];
    }
    return color;
}

function getIndicatorColorFromDamage(damage, is_me = false){
    if(is_me){
        var color = [0,0,0];
        if(damage > 0){
            if(damage < 40)
                color = [255, 100, 100, 255];
            else
                color = [200, 0, 0, 255];
        }
        return color;
    }
    else{
        var color = [0,0,0];
        if(damage > 0){
            if(damage < 40)
                color = [100, 255, 100, 255];
            else
                color = [255, 200, 0, 255];
        }
        return color;
    }
}

function getDamageText(damage){
    if(damage < 0)
        return `${damage}`;
    else
        return `-${damage}`;
}

function showDamageIndicator(damage, is_me = false){
    if(!mp.storage.data.menu.ShowDamageIndicator) return;
    if(damage === 0) return;
    if(is_me && mp.game.player.getInvincible()) return;

    Indicators.push({
        color:getIndicatorColorFromDamage(damage, is_me),
        position:[0.5, 0.8],
        alpha:255,
        text:getDamageText(damage),
        scale:[0, 0],
        created:Date.now()
    });
}

mp.events.add("showDamageIndicatorOnPosition", (damage, entity, boneIdx, type) => {showDamageIndicatorOnPosition(damage, entity, boneIdx, type); });

function showDamageIndicatorOnPosition(damage, entity, boneIdx, type){
    if(!mp.storage.data.menu.ShowDamageIndicator) return;
    if(damage === 0) return;
    if(entity.getVariable("Invincible") === true) return;

    const pos2d = mp.game.graphics.world3dToScreen2d(entity.position);
    if(!pos2d) return;
    
    SmartIndicators.push({
        color:getIndicatorColorFromEntity(entity, damage, type),
        entity:entity,
        boneIndex:boneIdx,
        position:[pos2d.x, pos2d.y],
        alpha:255,
        text:getDamageText(damage),
        scale:[0, 0],
        created:Date.now()
    });
}

mp.events.add('entityIndicatorTest', (damage, boneIndex) => {
    showDamageIndicatorOnPosition(damage, mp.players.local, boneIndex);
});
var indTestEnabled = false;
mp.events.add('indicatorTest', () => {
    indTestEnabled = !indTestEnabled;
    mp.gui.chat.push(`Damage indicator test: ${indTestEnabled.toString()}`);
});

mp._events.add("playerWeaponShot", (targetPosition, targetEntity) => {
    if(indTestEnabled){
        showDamageIndicator(getRandomInt(10, 90));
    }
});
function getRandomInt(min, max) {
    min = Math.ceil(min);
    max = Math.floor(max);
    return Math.floor(Math.random() * (max - min) + min); //The maximum is exclusive and the minimum is inclusive
  }


mp.events.add('render', () => {
    renderIndicators();
    renderSmartIndicators();
});

function renderSmartIndicators(){
    const timestamp = Date.now();
    const indicatorsToRemove = [];

    for(let i = 0 ; i < SmartIndicators.length ; i++){

        if(!SmartIndicators[i].entity || !mp.players.exists(SmartIndicators[i].entity)){
            indicatorsToRemove.push(SmartIndicators[i].created);
            continue;
        }

        //const bonePos = SmartIndicators[i].entity.getWorldPositionOfBone(SmartIndicators[i].boneIndex);

        const pos2d = mp.game.graphics.world3dToScreen2d(new mp.Vector3(
            SmartIndicators[i].entity.position.x,
            SmartIndicators[i].entity.position.y,
            SmartIndicators[i].entity.position.z + 0.4
        ));
        if(!pos2d){
            showDamageIndicator(Math.abs(Number(SmartIndicators[i].text)), (SmartIndicators[i].entity.remoteId === mp.players.local.remoteId));
            return;
        }

        mp.game.graphics.drawText(SmartIndicators[i].text, [pos2d.x, pos2d.y], {
            font:4, outline:true, centre:true, 
            scale:SmartIndicators[i].scale,
            color:SmartIndicators[i].color
        });
        const timeSinceCreation = timestamp - SmartIndicators[i].created;
        
        if(timeSinceCreation < 100){
            if(SmartIndicators[i].scale[0] < 0.5){
                SmartIndicators[i].scale[0] += 0.06;
                SmartIndicators[i].scale[1] += 0.06;
            }
        }
        else{
            if(timeSinceCreation < 250){
                SmartIndicators[i].color[3] -= 4;
            }
            else{
                indicatorsToRemove.push(SmartIndicators[i].created);
            }
        }
    }
    var remIndObjects = SmartIndicators.filter(i => indicatorsToRemove.includes(i.created));
    SmartIndicators = SmartIndicators.filter(i => !remIndObjects.includes(i));
}

function renderIndicators(){
    const timestamp = Date.now();
    const indicatorsToRemove = [];

    for(let i = 0 ; i < Indicators.length ; i++){
        mp.game.graphics.drawText(Indicators[i].text, Indicators[i].position, {
            font:4, outline:true, centre:true, 
            scale:Indicators[i].scale,
            color:Indicators[i].color
        });
        const timeSinceCreation = timestamp - Indicators[i].created;
        
        if(timeSinceCreation < 100){
            if(Indicators[i].scale[0] < 0.5){
                Indicators[i].scale[0] += 0.05;
                Indicators[i].scale[1] += 0.05;
            }
        }
        else{
            if(timeSinceCreation < 250){
                Indicators[i].position[1] -= 0.001;

                Indicators[i].color[3] -= 4;
            }
            else{
                indicatorsToRemove.push(Indicators[i].created);
            }
        }
    }
    var remIndObjects = Indicators.filter(i => indicatorsToRemove.includes(i.created));
    Indicators = Indicators.filter(i => !remIndObjects.includes(i));
}