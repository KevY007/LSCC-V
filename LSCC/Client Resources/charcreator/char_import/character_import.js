var global = require('./globals');

mp.events.add('character_import_open', () => {
    if(global.CharacterImportCEF){
        global.CharacterImportCEF.destroy();
        global.CharacterImportCEF = null;
    }

    global.CharacterImportCEF = mp.browsers.new('package://charcreator/char_import/CEF/index.html');
    global.CharacterImportCEF.active = true;
    mp.gui.cursor.show(true, true);

    if(global.ServerUI) global.ServerUI.call('hud_toggle', false);
});
mp.events.add('character_import_close', () => {
    if(global.CharacterImportCEF){
        global.CharacterImportCEF.destroy();
        global.CharacterImportCEF = null;
    }
    mp.gui.cursor.show(false, false);
    if(global.ServerUI) global.ServerUI.call('hud_toggle', true);
});
mp.events.add('character_import_data', (gender, data) => {
    if(data.startsWith("There was")){
        mp.events.callLocal("HUDError", data);
        return;
    }
    mp.events.callLocal("HUDError", `Applying character data! (${gender})`);
    const CharacterDataJSON = JSON.parse(data);
    if(CharacterDataJSON.OutfitPedData){
        if(CharacterDataJSON.OutfitPedData.PedProperties){
            if(CharacterDataJSON.OutfitPedData.PedProperties.HeadFeatures){
                if(CharacterDataJSON.OutfitPedData.PedProperties.HeadFeatures.FacialFeatures){
                    const FacialFeaturesArray = Object.values(CharacterDataJSON.OutfitPedData.PedProperties.HeadFeatures.FacialFeatures);
                    for(let i = 0; i < FacialFeaturesArray.length ; i++){
                        mp.players.local.setFaceFeature(i, Number(FacialFeaturesArray[i]));
                    }
                }
                if(CharacterDataJSON.OutfitPedData.PedProperties.HeadFeatures.EyeColour){
                    mp.players.local.setEyeColor(Number(CharacterDataJSON.OutfitPedData.PedProperties.HeadFeatures.EyeColour));
                }
                if(CharacterDataJSON.OutfitPedData.PedProperties.HeadFeatures.Overlays){
                    for(let i = 0 ; i < 13 ; i++){
                        const overlayData = CharacterDataJSON.OutfitPedData.PedProperties.HeadFeatures.Overlays[`_${i}`];
                        mp.players.local.setHeadOverlay(i, Number(overlayData['@index']), 
                                                           Number(overlayData['@opacity']), 
                                                           Number(overlayData['@colour']), 
                                                           Number(overlayData['@colourSecondary']));
                    }
                }
                if(CharacterDataJSON.OutfitPedData.PedProperties.HeadFeatures.ShapeAndSkinTone){
                    const _s = CharacterDataJSON.OutfitPedData.PedProperties.HeadFeatures.ShapeAndSkinTone;
                    mp.players.local.setHeadBlendData(Number(_s.ShapeFatherId), Number(_s.ShapeMotherId), Number(_s.ShapeOverrideId),
                    Number(_s.ToneFatherId), Number(_s.ToneMotherId), Number(_s.ToneOverrideId), Number(_s.ShapeVal),
                    Number(_s.ToneVal), Number(_s.OverrideVal), Boolean(_s.IsP));
                }
            }
        }
    }
    mp.events.callLocal('character_import_close');

});