var global = require('./globals');
  const {TranslateMessage: Translate} = require('./Translator');
const maxDistance = 25*25;
const width = 0.03;
const height = 0.0065;
const border = 0.001;
const defaultcolor = [255,255,255,255];

let timerValue = Date.now();
let timerCount = 0;

let showHPStatus = 0;

mp.nametags.enabled = false;

var NAMETAG_MESSAGES = {
    "PLAYER_SHOT": "(( THIS PLAYER HAS SHOT ))",
    "TASE_PROTECTION": "TASE PROTECTION",
    "CLIMB_PROTECTION": "CLIMB PROTECTION",
    "PLAYER_INJURED": "(( Player is wounded; hold [H] to help them up, or press [E] to carry them. ))",
    "ENEMY_INJURED": "(( Player is wounded ))"
}
mp.events.add("Translator:UpdateNametagMessages", (object) => {
    NAMETAG_MESSAGES = JSON.parse(object);
});

function isPlayerInLOS(target) {
    if(target === undefined) return false;

    const camera = mp.cameras.new("gameplay");

    if(camera === undefined)
    {
        camera = mp.cameras.new("default");
        if(camera === undefined)
        {
            return false;
        }
    }

    let position = camera.getCoord();

    let targetPos = target.getBoneCoords(12844, 0.0, 0, 0.25);

    let result = mp.raycasting.testPointToPoint(position, targetPos, [1, 16, 256, 128, 64, 32]); // now test point to point
    
    if(result === undefined)
    {
        return true;
    }
    else if(result !== undefined)
    {
        if(result.entity !== undefined) // Entity is not undefined and might be ped or veh.
        {
            if(typeof result.entity.isAPed === 'function' && result.entity.isAPed())
            {
                return true;
            }
            if(typeof result.entity.isAVehicle === 'function' && result.entity.isAVehicle())
            {
                return true;
            }
        }
        else if(result.entity === undefined) // There's no entity, can be a object or building anything.
        {
            return false;
        }
    }
    return false;
}

mp.events.add('playerNametagSet', (target, r, g, b, a, armour) => {
    if(!(target != null && target !== undefined)) return;

    target.col = true;
    target.col_r = r;
    target.col_g = g;
    target.col_b = b;
    target.col_a = a;

    if(armour)
    {
        target.setArmour(armour);
    }
});

// HPBarStatus (0) := Never show HP & AR bars. (Lobby)
// HPBarStatus (1) := Always show HP & AR bars. (Copchase)
// HPBarStatus (2) := Show only on aim. (DM/Freeroam)
// HPBarStatus (3) := Show busted progress var. (Busted)

mp.events.add('showHPBars', (status) => {
    showHPStatus = status;
});

var NT_SCALE = 0.8;
var NT_OFFSET = 0;
var NT_MULTIPLIER = -0.07;
mp.events.add("nt_adjust", (type, value) => {
    switch(type){
        case "scale": NT_SCALE = value;
        break;
        case "offset": NT_OFFSET = value;
        break;
        case "multiplier": NT_MULTIPLIER = value;
        break;
    }
});

function renderNametag(player, x, y, distance){
    if(Date.now() - timerValue >= 1000)
    {
        timerValue = Date.now();
        timerCount++;
        if(timerCount >= 3) timerCount = 0;
    }

    const screenRes = mp.game.graphics.getScreenResolution(0);

    if((global.specState || global.isNoClip) || (isPlayerInLOS(player) == true && player.getAlpha() > 32) || mp.players.local.getVariable("NametagHack") == true) {	   
            
        let distWithAdjust = 0 + distance;
        let maxDistWithAdjust = 0 + maxDistance;
        
        if(global.specState) {
            distWithAdjust -= 20;
            maxDistWithAdjust += 20*20;
        } 
        if(global.isNoClip) {
            distWithAdjust -= 35;
            maxDistWithAdjust = 35*35;
        }

        if(mp.players.local.getVariable("NametagHack") == true)
        {
            maxDistWithAdjust += 999999.9;
        }
        
        if((distWithAdjust <= (maxDistWithAdjust)) || (global.specState || global.isNoClip) || mp.players.local.getVariable("NametagHack") == true)
        {
            let scale = (distWithAdjust / (maxDistWithAdjust));
            if(scale < NT_SCALE) scale = NT_SCALE;

            //var health = player.getHealth() / 100;
        
            //var armour = (player.getArmour() > 100 ? 100 : player.getArmour()) / 100;
            
            y -= ((scale * (NT_MULTIPLIER   * (screenRes.y / 1080))) + NT_OFFSET);

            if(global.renderNametags == true) {
                if(player.getVariable("Team") !== undefined && player.getVariable("Team") == 0 && player.getVariable("HasShot") == true && !global.BankRobberyOn)
                {
                    mp.game.graphics.drawText(NAMETAG_MESSAGES.PLAYER_SHOT, [x, y - 0.02],
                    {
                        font: 0,
                        color: [237, 212, 52, 255],
                        scale: [0.23, 0.23],
                        outline: true
                    });
                }

                if(global.AFKStage > 0){
                    var typingStr = `(( IDLE [${SecondsSinceLastKeyPress}s/1500s] ))`;
                    if(timerCount == 0) typingStr = `(( IDLE [${SecondsSinceLastKeyPress}s/1500s] ))`;
                    else if(timerCount == 1) typingStr = `( IDLE [${SecondsSinceLastKeyPress}s/1500s] )`;
                    else if(timerCount == 2) timerCount = 0;
            
                    mp.game.graphics.drawText(typingStr, [x, y - 0.015],
                    {
                        font: 0,
                        color: [92, 255, 206, 228],
                        scale: [0.21, 0.21],
                        outline: true
                    });                        
                }
                else if(player.getVariable("Team") == 0 && player.getVariable("TaseProtection") == true)
                {
                    var typingStr = `(( ${NAMETAG_MESSAGES.TASE_PROTECTION} ))`;
                    if(timerCount == 0) typingStr = `(( ${NAMETAG_MESSAGES.TASE_PROTECTION} ))`;
                    else if(timerCount == 1) typingStr = `( ${NAMETAG_MESSAGES.TASE_PROTECTION} )`;
                    else if(timerCount == 2) timerCount = 0;
            
                    mp.game.graphics.drawText(typingStr, [x, y - 0.015],
                    {
                        font: 0,
                        color: [20, 103, 219, 228],
                        scale: [0.21, 0.21],
                        outline: true
                    });
                }
                else if(player.getVariable("ClimbProtection") == true)
                {
                    var typingStr = `(( ${NAMETAG_MESSAGES.LADDER_PROTECTION} ))`;
                    if(timerCount == 0) typingStr = `(( ${NAMETAG_MESSAGES.LADDER_PROTECTION} ))`;
                    else if(timerCount == 1) typingStr = `( ${NAMETAG_MESSAGES.LADDER_PROTECTION} )`;
                    else if(timerCount == 2) timerCount = 0;
            
                    mp.game.graphics.drawText(typingStr, [x, y - 0.015],
                    {
                        font: 0,
                        color: [20, 103, 219, 228],
                        scale: [0.23, 0.23],
                        outline: true
                    });
                }                   
                else if((player.getVariable("Wounded") == 1 || player.getVariable("Wounded") == 2))
                {
                    var typingStr = "";
                    if(player.getVariable("Team") === mp.players.local.getVariable("Team")){
                        typingStr = NAMETAG_MESSAGES.PLAYER_INJURED;

                        mp.game.graphics.drawText(typingStr, [x, y - 0.015],
                            {
                                font: 0,
                                color: [255, 99, 71, 228],
                                scale: [0.245, 0.245],
                                outline: true
                            });  
                    }
                    else{
                        typingStr = NAMETAG_MESSAGES.ENEMY_INJURED;
                        mp.game.graphics.drawText(typingStr, [x, y - 0.015],
                            {
                                font: 0,
                                color: [255, 25, 25, 200],
                                scale: [0.2, 0.2],
                                outline: true
                            });  
                    }
                                        
                }
                else if((player.isChatActive !== undefined && player.isChatActive == true) || player.isVoiceActive == true)
                {
                    var typingStr = "[...]";
                    if(timerCount == 0) typingStr = "[.]";
                    else if(timerCount == 1) typingStr = "[..]";
                    else if(timerCount == 2) typingStr = "[...]";

                    mp.game.graphics.drawText(typingStr, [x, y - 0.03],
                    {
                        font: 0,
                        color: [210, 133, 31, 200],
                        scale: [0.26, 0.26],
                        outline: true
                    });
                }
                else if((player.getVariable("Team") == 0 || player.getVariable("Team") == 1)  && player.getVariable("Invincible") === true){
                    var typingStr = "(( DAMAGE PROTECTION ))";
                    if(timerCount == 0) typingStr = "(( DAMAGE PROTECTION ))";
                    else if(timerCount == 1) typingStr = "( DAMAGE PROTECTION )";
                    else if(timerCount == 2) timerCount = 0;
            
                    mp.game.graphics.drawText(typingStr, [x, y - 0.015],
                    {
                        font: 0,
                        color: [100, 255, 0, 228],
                        scale: [0.21, 0.21],
                        outline: true
                    });
                }
                    
                var nameToDisplay = player.name.replace('_');
                if(player.getVariable("FakeNameOn") === true && player.getVariable("FakeName") && player.getVariable("FakeName").length > 0)
                    nameToDisplay = player.getVariable("FakeName");

                if(!global.isInInterior){
                    if(player.getVariable("ShowDamaged") === false){
                        if(player.col)
                        {
                            mp.game.graphics.drawText(nameToDisplay, [x, y],
                            {
                            font: 4,
                            color: [player.col_r, player.col_g, player.col_b, player.col_a],
                            scale: [0.395, 0.395],
                            outline: true
                            });
                        }
                        else
                        {
                            mp.game.graphics.drawText(nameToDisplay, [x, y],
                            {
                            font: 4,
                            color: defaultcolor,
                            scale: [0.395, 0.395],
                            outline: true
                            });
                        }
                    }  
                    else{
                        mp.game.graphics.drawText(nameToDisplay, [x, y],
                        {
                        font: 4,
                        color: [248, 113, 113, 255],
                        scale: [0.395, 0.395],
                        outline: true
                        });
                    }     
                }
                           

                /*
                if(showHPStatus == 1 || (mp.game.player.isFreeAimingAtEntity(player.handle) && showHPStatus == 2))
                {
                    let y2 = y + 0.056;
                    
                    graphics.drawRect(x, y2, width + border * 2, height + border * 2, 0, 0, 0, 200);
                    graphics.drawRect(x, y2, width, height, 176, 12, 12, 255);
                    graphics.drawRect(x - width / 2 * (1 - health), y2, width * health, height, 245, 73, 73, 200);
                    
                    if(armour > 0) 
                    {
                        y2 -= 0.012;
                        graphics.drawRect(x, y2, width + border * 2, height + border * 2, 0, 0, 0, 200);
                        graphics.drawRect(x, y2, width, height, 143, 143, 143, 255);
                        graphics.drawRect(x - width / 2 * (1 - armour), y2, width * armour, height, 255, 255, 255, 200);
                    }
                }
                if(showHPStatus == 3)
                {
                    if(player.getVariable("BustedProgress") !== undefined && player.getVariable("Team") != undefined && player.getVariable("Team") == 0 && 
                        player.getVariable("BustedProgress") >= 0.0 && global.chaseRunning) // -1.0 is used if busted.
                    {
                        health = player.getVariable("BustedProgress") / 100;
                        let y2 = y + 0.056;
                        
                        graphics.drawRect(x, y2, (0.02 + width) + border * 2, (0.003 + height) + border * 2, 0, 0, 0, 200);
                        graphics.drawRect(x, y2, (0.02 + width), (0.003 + height), 0, 13, 128, 255);
                        graphics.drawRect(x - (width + 0.02) / 2 * (1 - health), y2, (0.02 + width) * health, (0.003 + height), 92, 95, 242, 200);
                    }
                }
                */
            }

            if(mp.game.player.isFreeAimingAtEntity(player.handle) && mp.players.local.weaponAmmo > 1 && 
                mp.players.local.weapon != 2725352035 && mp.players.local.weapon != 0xA2719263) // 2725352035/hex = fist 
            {
                if(mp.players.local.getVariable("HasShot") == false && mp.players.local.getVariable("Team") == 0 && player.getVariable("Team") == 1 && global.chaseRunning)
                {
                    mp.events.callRemote("GivePlayerShotTag");
                }
            }
        }
        else
        {
            if(player.getVariable("Team") == 0 && player.getVariable("HasShot") == true && global.renderNametags == true && global.chaseRunning && !global.BankRobberyOn)
            {
                // They out of distance but still try?
                mp.game.graphics.drawText(NAMETAG_MESSAGES.PLAYER_SHOT, [x, y],
                {
                    font: 0,
                    color: [237, 212, 52, 255],
                    scale: [0.2, 0.2],
                    outline: true
                });
            }
        }
    }
}

mp.events.add('render', (nametags) => {

    if((mp.players.local.getVariable("pLogged") !== undefined && mp.players.local.getVariable("pLogged") == true) == false)
    {
        return;
    }
    
    nametags.forEach(nametag => {
        let [player, x, y, distance] = nametag;
    
        renderNametag(player, x, y, distance);
    });

    if(mp.storage.data.menu.ownNametag == 1){
        const localHeadPos = mp.players.local.getBoneCoords(12844, 0.4, 0, 0);
        if(localHeadPos){
            let localPlayer2Dpos = mp.game.graphics.world3dToScreen2d(localHeadPos);
            if(localPlayer2Dpos && localPlayer2Dpos.x && localPlayer2Dpos.y){
                renderNametag(mp.players.local, localPlayer2Dpos.x, localPlayer2Dpos.y, 0);
            }
        } 
        return;
    }
    if(mp.storage.data.menu.ownNametag == 2){
        localPlayer2Dpos = mp.game.graphics.world3dToScreen2d(new mp.Vector3(mp.players.local.position.x,
                                                                             mp.players.local.position.y,
                                                                             mp.players.local.position.z + 1));
        if(localPlayer2Dpos && localPlayer2Dpos.x && localPlayer2Dpos.y){
            renderNametag(mp.players.local, localPlayer2Dpos.x, localPlayer2Dpos.y, 0);
        }
        return;
    }    
})

let lastBlipEvent = 0;
let playerBlips = [];
mp.events.add('render', () => {
    if(Date.now() - lastBlipEvent < 1000){
        return;
    }
    lastBlipEvent = Date.now();
    for(let i = 0 ; i < playerBlips.length ; i++){
        if(playerBlips[i]) playerBlips[i].destroy();
    }
    playerBlips = [];
    mp.players.forEachInStreamRange(p => {
            if(p.hasVariable('ON_DUTY_ADMIN') && p.getVariable('ON_DUTY_ADMIN')){
                let blip = mp.blips.new(1, p.position, {
                    color:2, 
                    alpha:255, 
                    name:`${p.name} (ADMIN)`, 
                    shortRange:false, 
                    dimension:mp.players.local.dimension,
                    scale:1.2});
                playerBlips.push(blip);
            }
            else{
                let blip = mp.blips.new(1, p.position, {
                    color:4, 
                    alpha:255, 
                    name:p.name, 
                    shortRange:false, 
                    dimension:mp.players.local.dimension,
                    scale:1.2});
                playerBlips.push(blip);
            }
    });
});

mp.events.add('render', () => {

    let notOnScreen = [];

    mp.players.forEachInStreamRange((player) => {
        if(player.hasVariable('ON_DUTY_ADMIN') && player.getVariable('ON_DUTY_ADMIN')){

            const screenPos = mp.game.graphics.world3dToScreen2d(player.position.x, player.position.y, player.position.z+0.65)
            if(screenPos && screenPos.x && screenPos.y){
                mp.game.graphics.drawText(`${player.name} (ADMIN)`, [screenPos.x, screenPos.y],
                {
                    font: 0,
                    color: [0, 200, 0, 255],
                    scale: [0.27, 0.27],
                    outline: true
                });
            }
            else{
                const distance = mp.game.system.vdist2(mp.players.local.position.x, mp.players.local.position.y, mp.players.local.position.z,
                                                       player.position.x, player.position.y, player.position.z).toFixed(0);
                notOnScreen.push({name:player.name, admin:true, distance:distance})
            }
        
        }
        else{
            const screenPos = mp.game.graphics.world3dToScreen2d(player.position.x, player.position.y, player.position.z+0.65)
            if(screenPos && screenPos.x && screenPos.y){
                mp.game.graphics.drawText(`${player.name}`, [screenPos.x, screenPos.y],
                {
                    font: 0,
                    color: [255, 128, 128, 255],
                    scale: [0.25, 0.25],
                    outline: true
                });
            }
            else{
                const distance = mp.game.system.vdist2(mp.players.local.position.x, mp.players.local.position.y, mp.players.local.position.z,
                    player.position.x, player.position.y, player.position.z).toFixed(0);
                notOnScreen.push({name:player.name, admin:false, distance:distance})
            }
        }
    });

    let str = `~y~STREAMED PLAYERS:`
    for(let i = 0 ; i < notOnScreen ; i++){
        if(notOnScreen[i].admin == true)
            str += `\n~g~${notOnScreen[i].name} ~y~(${notOnScreen[i].distance}m)`;
        else str += `\n~w~${notOnScreen[i].name} ~y~(${notOnScreen[i].distance}m)`;
    }
    mp.game.graphics.drawText(str, [0.9, 0.5], {
        font: 0,
        color: [255, 255, 0, 255],
        scale: [0.2, 0.2],
        outline: true
    })
});
