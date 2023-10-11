mp.events.addDataHandler("CHARACTER:Appearance", (entity, value, oldValue) => {
    applyAppearanceData(entity, value);
});
mp.events.addDataHandler("CHARACTER:Outfit", (entity, value, oldValue) => {
    applyOutfitData(entity, value);
});

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

}
function applyOutfitData(entity, data){
    const OutfitData = JSON.parse(data);
    for(let i = 0 ; i < OutfitData.Components.length; i++){
        if(i == 2) continue; // skip hair styles since those are set in appearance data

        const Component = OutfitData.Components[i];
        entity.setComponentVariation(i, Component.Drawable, Component.Texture, Component.Palette);

    }
    for(let i = 0 ; i < OutfitData.Props.Length; i++){

        const Prop = OutfitData.Props[i];
        entity.setProp(Prop.Index, Prop.Drawable, Prop.Texture);
    }


}