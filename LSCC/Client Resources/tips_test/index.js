var FloatTips = [];
var FloatTipsDOM = undefined;

let lastFrameUpdate = 0;
function InitializeFloatTips(){
    FloatTipsDOM = mp.browsers.new("package://tips_test/index.html");
}
/*
mp.events.add("addFloatTip", (id, key, action, posX, posY, posZ, maxDist, useDist) => {
    FloatTips.push({
        id: id,
        key:key, action:action, 
        posX:posX, 
        posY:posY, 
        posZ:posZ,
        maxDist:maxDist,
        useDist:useDist,

        triggered:false,
        currentColor:[0, 0, 0]
    });
});

mp.events.add("removeTipByID", (id) => {
    var tip = FloatTips.find(t => t.id === id);
    if(!tip) return;

    var idx = FloatTips.indexOf(tip);
    FloatTips.splice(idx, 1);
});
mp.events.add("removeTipsByKey", (key) => {
    var tips = FloatTips.filter(t => t.key === key);
    if(!tips || tips.length === 0) return;
    
    const newTips = FloatTips.filter( x => !tips.filter( y => y.id === x.id).length);
    FloatTips = newTips;
});

mp.events.add("triggerTipsByKey", (key) => {
    var tips = FloatTips.filter(t => t.key === key);
    if(!tips || tips.length === 0) return;
    for(let i = 0 ; i < tips.length ; i++){tips[i].triggered = true;}

    setTimeout(() => {
        for(let i = 0 ; i < tips.length ; i++){tips[i].triggered = false;}
    }, 1);
});

mp.events.add("triggerTipByID", (id) => {
    var tip = FloatTips.find(t => t.id === id);
    if(!tip) return;
    tip.triggered = true;

    setTimeout(() => {
        if(tip){
            tip.triggered = false;
        }
    }, 1);
});

mp.events.add('render', () => {
    if(FloatTips.length === 0) return;

    for(let i = 0 ; i < FloatTips.length ; i++){
        const tip = FloatTips[i];

        const tipPos = new mp.Vector3(tip.posX, tip.posY, tip.posZ);
        let pos2d = mp.game.graphics.world3dToScreen2d(tipPos);
        if(!pos2d) continue;

        const playerPos = mp.players.local.position;
        const distance = playerPos.subtract(tipPos).length();

        let dividor = 1;
        if(distance < 1) dividor = 1;
        else if(distance > 1 && distance < tip.maxDist) dividor = (1 - (distance/30));
        else continue;

        var scaleData = {
            keyBoxWidth:0.0165*dividor,
            keyBoxWidthHalf:0.00825*dividor,
            boxHeight:0.02933205*dividor,
            actionBoxWidthMultiplies:0.006*dividor,
            keyTextScale:0.5*dividor,
            actionTextScale:0.45*dividor,
            keyTextOffsetY:-0.017*dividor,
            actionTextOffsetY:-0.016*dividor,   
        }

        let targetColor = (distance < tip.useDist ? [0, 0, 0] : [50, 0, 0]);
        if(tip.triggered && distance < tip.useDist){
            targetColor = [250,250,250];
            tip.currentColor = [250, 250, 250];
        };

        let actColor = tip.currentColor;
        for(let i = 0 ; i < 3 ; i++){
            if(actColor[i] != targetColor[i]){
                if(targetColor[i] > actColor[i]) actColor[i]+= 10;
                else actColor[i]-= 10;
            }
        }
        tip.currentColor = actColor;

        mp.game.graphics.drawRect(pos2d.x, pos2d.y, scaleData.keyBoxWidth, scaleData.boxHeight, 0, 0, 0, 120);

        const actwidth = (tip.action.length*scaleData.actionBoxWidthMultiplies);
        mp.game.graphics.drawRect(pos2d.x+(scaleData.keyBoxWidthHalf+actwidth/2), pos2d.y, actwidth, scaleData.boxHeight, actColor[0], actColor[1], actColor[2], 90);

        mp.game.graphics.drawText(tip.key, [pos2d.x, pos2d.y+scaleData.keyTextOffsetY], {font:4, color:[255, 255, 255, 200], scale:[scaleData.keyTextScale, scaleData.keyTextScale]});
        mp.game.graphics.drawText(tip.action, [pos2d.x+(scaleData.keyBoxWidthHalf+actwidth/2), pos2d.y+scaleData.actionTextOffsetY], {font:4, centre:true, outline:false, scale:[scaleData.actionTextScale, scaleData.actionTextScale], color:[200, 200, 200, 200]});

    }
});
*/



/*======================== OLD SYSTEM WITH CEF THAT WAS LAGGY =======================================*/


mp.events.add("addFloatTip", (id, key, action, posX, posY, posZ, maxDist = 0, useDist = 0) => {
    FloatTips.push({
        id: id,
        key:key, action:action, 
        posX:posX, 
        posY:posY, 
        posZ:posZ
    });
    let pos2d = mp.game.graphics.world3dToScreen2d(new mp.Vector3(posX, posY, posZ));
    if(pos2d){
        let realPos = {x:0, y:0};

        realPos.y = ((resolution.y - (pos2d.y * resolution.y)));
        realPos.x = ((pos2d.x * resolution.x) + 10);
        FloatTipsDOM.execute(`addFloatTip('${id}', '${key}', '${action}', ${realPos.x}, ${realPos.y});`);
    }
});

mp.events.add('render', () => {
    let resolution = mp.game.graphics.getScreenActiveResolution(0,0);
    lastFrameUpdate++;
    if(lastFrameUpdate >= 1){
        for(let i = 0 ; i < FloatTips.length ; i++){
            let pos2d = mp.game.graphics.world3dToScreen2d(new mp.Vector3(FloatTips[i].posX, FloatTips[i].posY, FloatTips[i].posZ));
            if(pos2d){
                let realPos = {x:0, y:0};

                realPos.y = (pos2d.y * resolution.y);
                realPos.x = (pos2d.x * resolution.x);

                FloatTipsDOM.execute(`setScreenPosition('${FloatTips[i].id}', ${realPos.x}, ${realPos.y});`);
            }
            
        }
        lastFrameUpdate = 0;
    }
});
