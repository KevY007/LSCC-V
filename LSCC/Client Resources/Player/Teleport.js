var global = require('./globals');

mp.events.addDataHandler("TELEPORT:Data", (entity, value, oldValue) => {
    
    for(let i = 0 ; i < global.TeleportIndicators.length ; i++){
        const indicator = global.TeleportIndicators[i];
        if(indicator.blip){
            indicator.blip.destroy();
        }
        if(indicator.marker){
            indicator.marker.destroy();
        }
        if(indicator.label){
            indicator.label.destroy();
        }
    }
    global.TeleportIndicators = [];

    global.TeleportData = JSON.parse(value);

    for(let i = 0 ; i < global.TeleportData.length ; i++){
        const tp = global.TeleportData[i];

        const label = mp.labels.new(`~w~${tp.Name}\n~y~Press ~g~(Y) ~y~to enter.`, new mp.Vector3(tp.FromX, tp.FromY, tp.FromZ), {
            font:4,
            los:true,
            drawDistance:6,
            dimension:mp.players.local.dimension
        });
        const marker = mp.markers.new(23, new mp.Vector3(tp.FromX, tp.FromY, tp.FromZ-0.95), 1, {
            color:[0, 0, 255, 50], 
            dimension:mp.players.local.dimension, 
            visible:true, 
        });

        var indicator = {id:tp.SQL_ID, marker:marker, label:label};

        if(tp.BlipID != -1){
            const blip = mp.blips.new(tp.BlipID, new mp.Vector3(tp.FromX, tp.FromY, tp.FromZ), {
                name:tp.Name,
                scale:0.9,
                color:0,
                alpha:255,
                dimension:mp.players.local.dimension,
                shortRange:true
            });
            indicator.blip = blip;
        }

        global.TeleportIndicators.push(indicator);
    }
});