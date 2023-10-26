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

    entity.clearFacialDecorations();

    const hairDecalIndex = (AppearanceData.IsMale ? 0 : 1);
    const HeadOverlayData = hairDecalsList[hairDecalIndex];
    for(let i = 0 ; i < AppearanceData.HairOverlays.length ; i++){
        const Overlay = HeadOverlayData[AppearanceData.HairOverlays[i]];

        const collection = mp.game.gameplay.getHashKey(Overlay.collection);
        const overlay = mp.game.gameplay.getHashKey(Overlay.overlay);
        entity.setFacialDecoration(collection, overlay);
    }
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




var hairDecalsList = [

    [ // male

        { name : "Close Shave",         collection :  "mpbeach_overlays",       overlay :  "FM_Hair_Fuzz"},

        { name : "Buzzcut",             collection :  "multiplayer_overlays",   overlay :  "NG_M_Hair_001"},

        { name : "Faux Hawk",           collection :  "multiplayer_overlays",   overlay :  "NG_M_Hair_002"},

        { name : "Hipster",             collection :  "multiplayer_overlays",   overlay :  "NG_M_Hair_003"},

        { name : "Side Parting",        collection :  "multiplayer_overlays",   overlay :  "NG_M_Hair_004"},

        { name : "Shorter Cut",         collection :  "multiplayer_overlays",   overlay :  "NG_M_Hair_005"},

        { name : "Biker",               collection :  "multiplayer_overlays",   overlay :  "NG_M_Hair_006"},

        { name : "Ponytail",            collection :  "multiplayer_overlays",   overlay :  "NG_M_Hair_007"},

        { name : "Cornrows",            collection :  "multiplayer_overlays",   overlay :  "NG_M_Hair_008"},

        { name : "Slicked",             collection :  "multiplayer_overlays",   overlay :  "NG_M_Hair_009"},

        { name : "Short Brushed",       collection :  "multiplayer_overlays",   overlay :  "NG_M_Hair_013"},

        { name : "Spikey",              collection :  "multiplayer_overlays",   overlay :  "NG_M_Hair_002"},

        { name : "Caesar",              collection :  "multiplayer_overlays",   overlay :  "NG_M_Hair_011"},

        { name : "Chopped",             collection :  "multiplayer_overlays",   overlay :  "NG_M_Hair_012"},

        { name : "Dreads",              collection :  "multiplayer_overlays",   overlay :  "NG_M_Hair_014"},

        { name : "Long Hair",           collection :  "multiplayer_overlays",   overlay :  "NG_M_Hair_015"},

        { name : "Shaggy Curls",        collection :  "multiplayer_overlays",   overlay :  "NGBea_M_Hair_000"},

        { name : "Surfer Dude",         collection :  "multiplayer_overlays",   overlay :  "NGBea_M_Hair_001"},

        { name : "Short Side Part",     collection :  "multiplayer_overlays",   overlay :  "NGBus_M_Hair_000"},

        { name : "High Slicked Sides",  collection :  "multiplayer_overlays",   overlay :  "NGBus_M_Hair_001"},

        { name : "Long Slicked",        collection :  "multiplayer_overlays",   overlay :  "NGHip_M_Hair_000"},

        { name : "Hipster Youth",       collection :  "multiplayer_overlays",   overlay :  "NGHip_M_Hair_001"},

        { name : "Mullet",              collection :  "multiplayer_overlays",   overlay :  "NGInd_M_Hair_000"},

        { name : "Classic Cornrows",    collection :  "mplowrider_overlays",    overlay :  "LR_M_Hair_000"},

        { name : "Palm Cornrows",       collection :  "mplowrider_overlays",    overlay :  "LR_M_Hair_001"},

        { name : "Lightning Cornrows",  collection :  "mplowrider_overlays",    overlay :  "LR_M_Hair_002"},

        { name : "Whipped Cornrows",    collection :  "mplowrider_overlays",    overlay :  "LR_M_Hair_003"},

        { name : "Zig Zag Cornrows",    collection :  "mplowrider2_overlays",   overlay :  "LR_M_Hair_004"},

        { name : "Snail Cornrows",      collection :  "mplowrider2_overlays",   overlay :  "LR_M_Hair_005"},

        { name : "Hightop",             collection :  "mplowrider2_overlays",   overlay :  "LR_M_Hair_006"},

        { name : "Loose Swept Back",    collection :  "mpbiker_overlays",       overlay :  "MP_Biker_Hair_000_M"},

        { name : "Undercut Swept Back", collection :  "mpbiker_overlays",       overlay :  "MP_Biker_Hair_001_M"},

        { name : "Undercut Swept Side", collection :  "mpbiker_overlays",       overlay :  "MP_Biker_Hair_002_M"},

        { name : "Spiked Mohawk",       collection :  "mpbiker_overlays",       overlay :  "MP_Biker_Hair_003_M"},

        { name : "Mod",                 collection :  "mpbiker_overlays",       overlay :  "MP_Biker_Hair_004_M"},

        { name : "Layered Mod",         collection :  "mpbiker_overlays",       overlay :  "MP_Biker_Hair_005_M"},

        { name : "Flattop",             collection :  "mpgunrunning_overlays",  overlay :  "MP_Gunrunning_Hair_M_000_M"},

        { name : "Military Buzzcut",    collection :  "mpgunrunning_overlays",  overlay :  "MP_Gunrunning_Hair_M_001_M"}

    ],

    [ // female

        { name : "Close Shave",         collection :  "mpbeach_overlays",       overlay :  "FM_Hair_Fuzz"},

        { name : "Short",               collection :  "multiplayer_overlays",   overlay :  "NG_F_Hair_001"},

        { name : "Layered Bob",         collection :  "multiplayer_overlays",   overlay :  "NG_F_Hair_002"},

        { name : "Pigtails",            collection :  "multiplayer_overlays",   overlay :  "NG_F_Hair_003"},

        { name : "Ponytail",            collection :  "multiplayer_overlays",   overlay :  "NG_F_Hair_004"},

        { name : "Braided Mohawk",      collection :  "multiplayer_overlays",   overlay :  "NG_F_Hair_005"},

        { name : "Braids",              collection :  "multiplayer_overlays",   overlay :  "NG_F_Hair_006"},

        { name : "Bob",                 collection :  "multiplayer_overlays",   overlay :  "NG_F_Hair_007"},

        { name : "Faux Hawk",           collection :  "multiplayer_overlays",   overlay :  "NG_F_Hair_008"},

        { name : "French Twist",        collection :  "multiplayer_overlays",   overlay :  "NG_F_Hair_009"},

        { name : "Long Bob",            collection :  "multiplayer_overlays",   overlay :  "NG_F_Hair_010"},

        { name : "Loose Tied",          collection :  "multiplayer_overlays",   overlay :  "NG_F_Hair_011"},

        { name : "Pixie",               collection :  "multiplayer_overlays",   overlay :  "NG_F_Hair_012"},

        { name : "Shaved Bangs",        collection :  "multiplayer_overlays",   overlay :  "NG_F_Hair_013"},

        { name : "Top Knot",            collection :  "multiplayer_overlays",   overlay :  "NG_M_Hair_014"},

        { name : "Wavy Bob",            collection :  "multiplayer_overlays",   overlay :  "NG_M_Hair_015"},

        { name : "Messy Bun",           collection :  "multiplayer_overlays",   overlay :  "NGBea_F_Hair_000"},

        { name : "Pin Up Girl",         collection :  "multiplayer_overlays",   overlay :  "NGBea_F_Hair_001"},

        { name : "Tight Bun",           collection :  "multiplayer_overlays",   overlay :  "NG_F_Hair_007"},

        { name : "Twisted Bob",         collection :  "multiplayer_overlays",   overlay :  "NGBus_F_Hair_000"},

        { name : "Flapper Bob",         collection :  "multiplayer_overlays",   overlay :  "NGBus_F_Hair_001"},

        { name : "Big Bangs",           collection :  "multiplayer_overlays",   overlay :  "NGBea_F_Hair_001"},

        { name : "Braided Top Knot",    collection :  "multiplayer_overlays",   overlay :  "NGHip_F_Hair_000"},

        { name : "Mullet",              collection :  "multiplayer_overlays",   overlay :  "NGInd_F_Hair_000"},

        { name : "Pinched Cornrows",    collection :  "mplowrider_overlays",    overlay :  "LR_F_Hair_000"},

        { name : "Leaf Cornrows",       collection :  "mplowrider_overlays",    overlay :  "LR_F_Hair_001"},

        { name : "Zig Zag Cornrows",    collection :  "mplowrider_overlays",    overlay :  "LR_F_Hair_002"},

        { name : "Pigtail Bangs",       collection :  "mplowrider2_overlays",   overlay :  "LR_F_Hair_003"},

        { name : "Wave Braids",         collection :  "mplowrider2_overlays",   overlay :  "LR_F_Hair_003"},

        { name : "Coil Braids",         collection :  "mplowrider2_overlays",   overlay :  "LR_F_Hair_004"},

        { name : "Rolled Quiff",        collection :  "mplowrider2_overlays",   overlay :  "LR_F_Hair_006"},

        { name : "Loose Swept Back",    collection :  "mpbiker_overlays",       overlay :  "MP_Biker_Hair_000_F"},

        { name : "Undercut Swept Back", collection :  "mpbiker_overlays",       overlay :  "MP_Biker_Hair_001_F"},

        { name : "Undercut Swept Side", collection :  "mpbiker_overlays",       overlay :  "MP_Biker_Hair_002_F"},

        { name : "Spiked Mohawk",       collection :  "mpbiker_overlays",       overlay :  "MP_Biker_Hair_003_F"},

        { name : "Bandana and Braid",   collection :  "multiplayer_overlays",   overlay :  "NG_F_Hair_003"},

        { name : "Layered Mod",         collection :  "mpbiker_overlays",       overlay :  "MP_Biker_Hair_006_F"},

        { name : "Skinbyrd",            collection :  "mpbiker_overlays",       overlay :  "MP_Biker_Hair_004_F"},

        { name : "Neat Bun",            collection :  "mpgunrunning_overlays",  overlay :  "MP_Gunrunning_Hair_F_000_F"},

        { name : "Short Bob",           collection :  "mpgunrunning_overlays",  overlay :  "MP_Gunrunning_Hair_F_001_F"}

    ]

];