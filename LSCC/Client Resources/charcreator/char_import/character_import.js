var global = require('./globals');

var CHARACTER_APPEARANCE_DATA = null;

mp.events.add('character_import_open', (appearanceData) => {
    if(global.CharacterImportCEF){
        global.CharacterImportCEF.destroy();
        global.CharacterImportCEF = null;
    }
    CHARACTER_APPEARANCE_DATA = JSON.parse(appearanceData);

    // script to open it if needed maybe idk
    mp.gui.cursor.show(true, true);

    if(global.ServerUI) global.ServerUI.call('hud_toggle', false);
});

let importData = {genderValue:'M', data:''}
mp.events.add("character_import_set_data", (appearanceData) => {
    CHARACTER_APPEARANCE_DATA = JSON.parse(appearanceData);
    mp.events.callRemote("ServerConsoleOutput", "Yeahhhh data: " + appearanceData);
    
    setupOnImport(importData.genderValue, importData.data);
});
mp.events.add('character_import_close', () => {
    if(global.CharacterImportCEF){
        global.CharacterImportCEF.destroy();
        global.CharacterImportCEF = null;
    }

    if(global.ServerUI) global.ServerUI.call("character_editor_import_toggle", false);

    mp.gui.cursor.show(false, false);
    if(global.ServerUI) global.ServerUI.call('hud_toggle', true);
});

function setupOnImport(genderValue, data){
    let gender = (genderValue == 0 ? 'M' : 'F');

    if(data.startsWith("There was")){
        mp.events.callLocal("HUDError", data);
        return;
    }
    if(gender == "M" && (CHARACTER_APPEARANCE_DATA && !CHARACTER_APPEARANCE_DATA.IsMale)){
        if(global.ServerUI) global.ServerUI.call("character_editor_import_error_set", "Your current character's gender is set to FEMALE! Please pick a different option or use <strong>/char reset</strong> and set it to MALE.");
        return;
    }
    if(gender == "F" && (CHARACTER_APPEARANCE_DATA && CHARACTER_APPEARANCE_DATA.IsMale)){
        if(global.ServerUI) global.ServerUI.call("character_editor_import_error_set", "Your current character's gender is set to MALE! Please pick a different option or use <strong>/char reset</strong> and set it to FEMALE.");
        return;
    }
    
    const CharacterDataJSON = JSON.parse(data);
    if(CharacterDataJSON.OutfitPedData){
        if(CharacterDataJSON.OutfitPedData.PedProperties){
            if(CharacterDataJSON.OutfitPedData.PedProperties.HeadFeatures){
                if(CharacterDataJSON.OutfitPedData.PedProperties.HeadFeatures.FacialFeatures){
                    const FacialFeaturesArray = Object.values(CharacterDataJSON.OutfitPedData.PedProperties.HeadFeatures.FacialFeatures);
                    for(let i = 0; i < FacialFeaturesArray.length ; i++){
                        CHARACTER_APPEARANCE_DATA.FaceFeatures[i].Scale = Number(FacialFeaturesArray[i]);
                    }
                }
                if(CharacterDataJSON.OutfitPedData.PedProperties.HeadFeatures.EyeColour){
                    CHARACTER_APPEARANCE_DATA.EyeColor = Number(CharacterDataJSON.OutfitPedData.PedProperties.HeadFeatures.EyeColour);
                }
                if(CharacterDataJSON.OutfitPedData.PedProperties.HeadFeatures.Overlays){
                    for(let i = 0 ; i < 13 ; i++){
                        const overlayData = CharacterDataJSON.OutfitPedData.PedProperties.HeadFeatures.Overlays[`_${i}`];

                        CHARACTER_APPEARANCE_DATA.HeadOverlays[i].Index = Number(overlayData['@index']);
                        CHARACTER_APPEARANCE_DATA.HeadOverlays[i].Opacity = Number(overlayData['@opacity']);
                        CHARACTER_APPEARANCE_DATA.HeadOverlays[i].FirstColor = Number(overlayData['@colour']);
                        CHARACTER_APPEARANCE_DATA.HeadOverlays[i].SecondColor = Number(overlayData['@colourSecondary']);
                    }
                }
                if(CharacterDataJSON.OutfitPedData.PedProperties.HeadFeatures.ShapeAndSkinTone){
                    const _s = CharacterDataJSON.OutfitPedData.PedProperties.HeadFeatures.ShapeAndSkinTone;
                    CHARACTER_APPEARANCE_DATA.HeadBlend.FirstParentID = Number(_s.ShapeFatherId);
                    CHARACTER_APPEARANCE_DATA.HeadBlend.SecondParentID = Number(_s.ShapeMotherId);
                    CHARACTER_APPEARANCE_DATA.HeadBlend.MixParentID = Number(_s.ShapeOverrideId);
                    CHARACTER_APPEARANCE_DATA.HeadBlend.FirstSkinID = Number(_s.ToneFatherId);
                    CHARACTER_APPEARANCE_DATA.HeadBlend.SecondSkinID = Number(_s.ToneMotherId);
                    CHARACTER_APPEARANCE_DATA.HeadBlend.MixSkinID = Number(_s.ToneOverrideId);
                    CHARACTER_APPEARANCE_DATA.HeadBlend.ShapeMix = Number(_s.ShapeVal);
                    CHARACTER_APPEARANCE_DATA.HeadBlend.SkinMix = Number(_s.ToneVal);
                    CHARACTER_APPEARANCE_DATA.HeadBlend.ThirdMix = Number(_s.OverrideVal);
                }
            }
        }
    }
    setTimeout(() => {
        mp.events.callRemote("CharacterEditor:SaveAppearance", JSON.stringify(CHARACTER_APPEARANCE_DATA));
        setTimeout(() => {
            mp.events.callLocal('character_import_close');
            mp.events.callRemote("DebugLog_CharAppearanceImport", JSON.stringify(CHARACTER_APPEARANCE_DATA), data);
        }, 50);
    }, 50);
 
}

mp.events.add('character_editor_import', (genderValue, data) => {
    mp.events.callRemote("RequestCharImportData");
    importData = {genderValue:genderValue, data:data};
});