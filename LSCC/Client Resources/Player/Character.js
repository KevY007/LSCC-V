var global = require('./globals');

mp.events.addDataHandler("CHARACTER:Appearance", (entity, value, oldValue) => {
    applyAppearanceData(entity, value);
});
mp.events.addDataHandler("CHARACTER:Outfit", (entity, value, oldValue) => {
    applyOutfitData(entity, value);
});
mp.events.add('entityStreamIn', (entity) => {
    streamIn_AppearanceCheck(entity);
    streamIn_OutfitCheck(entity);
});
function streamIn_AppearanceCheck(entity){
    if(!entity || entity.type !== 'player' || !entity.hasVariable("CHARACTER:Appearance")) return;
    applyAppearanceData(entity, entity.getVariable('CHARACTER:Appearance'));
}
function streamIn_OutfitCheck(entity){
    if(!entity || entity.type !== 'player' || !entity.hasVariable("CHARACTER:Outfit")) return;
    applyOutfitData(entity, entity.getVariable('CHARACTER:Outfit'));
}

// Hair Overlays applying
// let fuckRendering = false;
// mp.events.add("hell", () => {
//     fuckRendering = true;
// })
// mp.events.add('render', () => {
//     if(!fuckRendering) return;
//     if(mp.players.local.hasVariable("CHARACTER:Appearance")){
//         const LocalAppearance = JSON.parse(mp.players.local.getVariable("CHARACTER:Appearance"));
//         applyHairOverlays(mp.players.local, LocalAppearance);
//     }
// });
// mp.events.add("OneSecondEvent", () => {
//     if(mp.players.local.hasVariable("CHARACTER:Appearance")){
//         const LocalAppearance = JSON.parse(mp.players.local.getVariable("CHARACTER:Appearance"));
//         applyHairOverlays(mp.players.local, LocalAppearance);
//     }
//     mp.players.forEachInStreamRange(p => {
//         if(p.dimension === mp.players.local.dimension && p.hasVariable("CHARACTER:Appearance")){
//             const Appearance = JSON.parse(p.getVariable("CHARACTER:Appearance"));
//             applyHairOverlays(p, Appearance);
//         }
//     })
// });

function applyAppearanceData(entity, data){

    const AppearanceData = JSON.parse(data);
    if(AppearanceData.IsMale){
        const model = mp.game.joaat('mp_m_freemode_01');
        if(entity.model != model){
            entity.model = model;
        }
    }
    else{
        const model = mp.game.joaat('mp_f_freemode_01');
        if(entity.model != model){
            entity.model = model
        }
    }

    const HeadBlend = AppearanceData.HeadBlend;

    entity.setHeadBlendData(HeadBlend.FirstParentID, HeadBlend.SecondParentID, HeadBlend.MixParentID, 
                            HeadBlend.FirstSkinID, HeadBlend.SecondSkinID, HeadBlend.MixSkinID,
                            HeadBlend.ShapeMix, HeadBlend.SkinMix, HeadBlend.ThirdMix, HeadBlend.IsParent);

    entity.setEyeColor(AppearanceData.EyeColor)

    for(let i = 0 ; i < AppearanceData.HeadOverlays.length ; i++){
        const HeadOverlay = AppearanceData.HeadOverlays[i];

        entity.setHeadOverlay(HeadOverlay.OverlayID, HeadOverlay.Index, HeadOverlay.Opacity, HeadOverlay.FirstColor, HeadOverlay.SecondColor);
    }
    for(let i = 0 ; i < AppearanceData.FaceFeatures.length ; i++){
        const FaceFeature = AppearanceData.FaceFeatures[i];

        entity.setFaceFeature(FaceFeature.Index, FaceFeature.Scale);
    }

    entity.setComponentVariation(2, AppearanceData.Hair.Index, 0, 0);
    entity.setHairColor(AppearanceData.Hair.Color, AppearanceData.Hair.HighlightColor);

    applyHairOverlays(entity, AppearanceData);
}

function applyHairOverlays(entity, AppearanceData){

    const hairDecalIndex = (AppearanceData.IsMale ? 0 : 1);
    const HeadOverlayData = global.hairDecalsList[hairDecalIndex];
    for(let i = 0 ; i < AppearanceData.HairOverlays.length ; i++){
        const Overlay = HeadOverlayData[AppearanceData.HairOverlays[i]];
        const overlayIndex = getLastNumber(Overlay);
        if(!overlayIndex) continue;

        const HairOverlay = HeadOverlayData[overlayIndex];

        const collection = mp.game.gameplay.getHashKey(HairOverlay.collection);
        if(!collection) continue;
        const overlay = mp.game.gameplay.getHashKey(HairOverlay.overlay);
        if(!overlay) continue;
        entity.setFacialDecoration(collection, overlay);
    }
}

function getLastNumber(str) {
    if(!str) return;
    const matches = str.match(/\d+$/);
    if (matches) {
        return parseInt(matches[0], 10);
    }
    return null; // Return null or handle the case when no number is found.
}

function applyOutfitData(entity, data){
    const OutfitData = JSON.parse(data);
    for(let i = 0 ; i < OutfitData.Components.length; i++){
        if(i == 2) continue; // skip hair styles since those are set in appearance data

        const Component = OutfitData.Components[i];
        entity.setComponentVariation(i, Component.Drawable, Component.Texture, Component.Palette);

    }
    for(let i = 0 ; i < OutfitData.Props.length; i++){
        const Prop = OutfitData.Props[i];
        entity.clearProp(Prop.Index);
        entity.setPropIndex(Prop.Index, Prop.Drawable, Prop.Texture, true);
    }


}