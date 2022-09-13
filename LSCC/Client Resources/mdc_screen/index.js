mp.events.add("MDC:AddBlip", (id, type) => {
    MDC_DOM.execute(`AddBlip(${id}, '${type}');`); 
});
mp.events.add("MDC:AddSpikeBlip", (id, stage) => {
    MDC_DOM.execute(`AddSpikeBlip(${id}, ${stage});`);
});
mp.events.add("MDC:UpdateSpike", (id, stage) => {
    MDC_DOM.execute(`UpdateSpikeStage(${id}, ${stage});`);
});
mp.events.add("MDC:RemoveBlip", (id, type) => {
    MDC_DOM.execute(`RemoveBlip(${id}, '${type}');`);
});
mp.events.add("MDC:RemoveAllBlips", () => {
    MDC_DOM.execute(`RemoveAllBlips();`);
});
mp.events.add("MDC:UpdateBlip", (id, type, x, y) => {
    MDC_DOM.execute(`UpdateBlip(${id}, '${type}', ${x}, ${y});`);
});
mp.events.add("MDC:UpdateMiniscreen", (key, value) => {
    MDC_DOM.execute(`UpdateMiniscreenStatus('${key}', '${value}');`);
});