var FloatTips = [];
var FloatTipsDOM = undefined;

let lastFrameUpdate = 0;

function InitializeFloatTips(){
    FloatTipsDOM = mp.browsers.new("package://tips_test/index.html");
}

mp.events.add("addFloatTip", (id, key, action, posX, posY, posZ) => {
    FloatTips.push({
        id: id,
        key:key, action:action, 
        posX:posX, 
        posY:posY, 
        posZ:posZ
    });
    let pos2d = mp.game.graphics.world3dToScreen2d(new mp.Vector3(posX, posY, posZ));
    if(pos2d){
        FloatTipsDOM.execute(`addFloatTip('${id}', '${key}', '${action}', ${pos2d.x*1000}, ${pos2d.y*1000});`);
    }
});

mp.events.add('render', () => {
    lastFrameUpdate++;
    if(lastFrameUpdate >= 10){
        for(let i = 0 ; i < FloatTips.length ; i++){
            let pos2d = mp.game.graphics.world3dToScreen2d(new mp.Vector3(FloatTips[i].posX, FloatTips[i].posY, FloatTips[i].posZ));
            if(pos2d){
                FloatTipsDOM.execute(`setScreenPosition('${FloatTips[i].id}', ${pos2d.x*1000}, ${pos2d.y*1000});`);
            }
            
        }
        lastFrameUpdate = 0;
    }
});