var TraceLines = [];

mp.events.add("showKillTrace", (fromX, fromY, fromZ) => {
    TraceLines.push({
        from:new mp.Vector3(fromX, fromY, fromZ),
        to:mp.players.local.position,
        alpha:255,
        id:Date.now()
    });
});
mp.events.add("clearKillTraces", () => {
    TraceLines = [];
});

var traceLineFrames = 0;
mp.events.add('render', () => {
    traceLineFrames++;

    const LinesToRemove = [];
    for(let i = 0 ; i < TraceLines.length ; i++){
        const TraceLine = TraceLines[i];

        mp.game.graphics.drawLine(TraceLine.from.x, TraceLine.from.y, TraceLine.from.z,
            TraceLine.to.x, TraceLine.to.y, TraceLine.to.z, 255, 255, 0, TraceLine.alpha);

        if(traceLineFrames >= 1){
            TraceLine.alpha = (TraceLine.alpha-1);
            traceLineFrames = 0;
        }

        if(TraceLine.alpha <= 0)
            LinesToRemove.push(TraceLine.id);
    }

    if(TraceLines && TraceLines.length > 0)
        TraceLines = TraceLines.filter(t => !LinesToRemove.includes(t.id));
});