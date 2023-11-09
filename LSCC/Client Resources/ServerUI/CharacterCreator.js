var global = require("./globals");

const PropTypeMappings = [
    {uiID:-5, realID:7},
    {uiID:-4, realID:6},
    {uiID:-3, realID:2},
    {uiID:-2, realID:1},
    {uiID:-1, realID:0},
];
var CURRENT_EDIT_ITEM = {isProp:true, index:7, drawable:-1, texture:0};
var CURRENT_OUTFIT_DATA = {};

// OUTFITS
// Menu -> Clientside
mp.events.add("mainmenu_inventory_outfits_create", (activeTeam) => {
    mp.events.callRemote("OutfitCreator:Create", activeTeam);
});
mp.events.add("mainmenu_inventory_outfits_set_active", (id, team) => {
    mp.events.callRemote("OutfitCreator:SaveActive", id, team);

});
mp.events.add("mainmenu_inventory_outfits_edit", (id, team) => {
    mp.events.callRemote("OutfitCreator:Edit", id, team);
});

function getRealPropID(uiID){
    const prop = PropTypeMappings.find(p => p.uiID == uiID);
    if(prop) return prop.realID;
    return -1;
}
function getPropUIID(realID){
    const prop = PropTypeMappings.find(p => p.realID == realID);
    if(prop) return prop.uiID;
    return -1;
}

mp.events.add("dialog_clothing_editor_change_id", (value) => {
    CURRENT_EDIT_ITEM = {...CURRENT_EDIT_ITEM, drawable:value};

    if(CURRENT_EDIT_ITEM.isProp){
        if(value !== -1)
            mp.players.local.setPropIndex(CURRENT_EDIT_ITEM.index, value, 0, true);
        else mp.players.local.clearProp(CURRENT_EDIT_ITEM.index);

        if(CURRENT_OUTFIT_DATA.Props.find(p => p.Index === CURRENT_EDIT_ITEM.index)){
            CURRENT_OUTFIT_DATA.Props.find(p => p.Index === CURRENT_EDIT_ITEM.index).Drawable = value;
            CURRENT_OUTFIT_DATA.Props.find(p => p.Index === CURRENT_EDIT_ITEM.index).Texture = 0;

            mp.events.callLocal("dialog_clothing_editor_change_type", getPropUIID(CURRENT_EDIT_ITEM.index));
        }
    }
    else{
        mp.players.local.setComponentVariation(CURRENT_EDIT_ITEM.index, value, 0, 0);
        if(CURRENT_OUTFIT_DATA.Components[CURRENT_EDIT_ITEM.index]){
            CURRENT_OUTFIT_DATA.Components[CURRENT_EDIT_ITEM.index].Drawable = value;
            CURRENT_OUTFIT_DATA.Components[CURRENT_EDIT_ITEM.index].Texture = 0;

            mp.events.callLocal("dialog_clothing_editor_change_type", CURRENT_EDIT_ITEM.index);
        }
    }
});
mp.events.add("dialog_clothing_editor_change_texture", (value) => {
    CURRENT_EDIT_ITEM = {...CURRENT_EDIT_ITEM, texture:value};

    if(CURRENT_EDIT_ITEM.isProp){
        mp.players.local.setPropIndex(CURRENT_EDIT_ITEM.index, CURRENT_EDIT_ITEM.drawable, value, true);

        if(CURRENT_OUTFIT_DATA.Props.find(p => p.Index === CURRENT_EDIT_ITEM.index)){
            CURRENT_OUTFIT_DATA.Props.find(p => p.Index === CURRENT_EDIT_ITEM.index).Texture = value;
        }
    }
    else{
        mp.players.local.setComponentVariation(CURRENT_EDIT_ITEM.index, CURRENT_EDIT_ITEM.drawable, value, 0);
        if(CURRENT_OUTFIT_DATA.Components[CURRENT_EDIT_ITEM.index]){
            CURRENT_OUTFIT_DATA.Components[CURRENT_EDIT_ITEM.index].Texture = value;
        }
    }
});
mp.events.add("dialog_clothing_editor_change_type", (value) => {
    if(value < 0){
        const realID = getRealPropID(value);

        const PropData = CURRENT_OUTFIT_DATA.Props.find(p => p.Index == realID);
        if(PropData){
            CURRENT_EDIT_ITEM = {isProp:true, index:realID, drawable:PropData.Drawable, texture:PropData.Texture}; // drawable & texture to be loaded
            onOutfitCreatorChangeType(CURRENT_EDIT_ITEM.index, CURRENT_EDIT_ITEM.drawable, CURRENT_EDIT_ITEM.texture, CURRENT_EDIT_ITEM.isProp);
            return;
        }
        CURRENT_EDIT_ITEM = {isProp:true, index:realID, drawable:0, texture:0}; // drawable & texture to be loaded
        onOutfitCreatorChangeType(CURRENT_EDIT_ITEM.index, CURRENT_EDIT_ITEM.drawable, CURRENT_EDIT_ITEM.texture, CURRENT_EDIT_ITEM.isProp);
        return;
    }

    const ComponentData = CURRENT_OUTFIT_DATA.Components[value];
    if(!ComponentData){
        CURRENT_EDIT_ITEM = {isProp:false, index:value, drawable:0, texture:0}; // drawable & texture to be loaded
        onOutfitCreatorChangeType(CURRENT_EDIT_ITEM.index, CURRENT_EDIT_ITEM.drawable, CURRENT_EDIT_ITEM.texture, CURRENT_EDIT_ITEM.isProp);
        return;
    }
    CURRENT_EDIT_ITEM = {isProp:false, index:value, drawable:ComponentData.Drawable, texture:ComponentData.Texture};
    onOutfitCreatorChangeType(CURRENT_EDIT_ITEM.index, CURRENT_EDIT_ITEM.drawable, CURRENT_EDIT_ITEM.texture, CURRENT_EDIT_ITEM.isProp);

});
mp.events.add("dialog_clothing_editor_close", (value) => {
    mp.gui.cursor.show(false, false);

    // For debugging purposes: save outfit edits
    mp.events.callRemote("OutfitCreator:SaveOutfitChanges", JSON.stringify(CURRENT_OUTFIT_DATA));

    mp.events.callRemote("CharacterEditor:CloseEvent");
});


// Server -> Clientside
mp.events.add("OutfitCreator:SetOutfits", (outfitData) => { // Loading outfits
    const outfitArray = JSON.parse(outfitData);
    const outfits = outfitArray.map((outfit) => { return {id:outfit.SQL_ID, team:outfit.Team }; });


    if(global.ServerUI) global.ServerUI.call("mainmenu_inventory_outfits_set", outfits);
});
mp.events.add("OutfitCreator:SetActive", (id, team) => { // Loading defaults

    if(global.ServerUI) global.ServerUI.call("mainmenu_inventory_outfits_set_active", id, team);
});


var OUTFIT_EDITOR_DATA = {
    isMale:true,
    team:1
};
function onOutfitCreatorChangeType(componentID, inputID, inputTexture, isProp){

    const isDonator = (mp.players.local.hasVariable("Donator") && mp.players.local.getVariable("Donator") >= 3);
    const isMale = OUTFIT_EDITOR_DATA.isMale;
    const team = OUTFIT_EDITOR_DATA.team;
    

    var min_drawable = 0;
    var max_drawable = 0;

    if(isMale){
        if(isDonator){
            if(team == 1){
                var idToCheck = (isProp ? getPropUIID(componentID) : componentID);

                const data = CLOTHING_EDITOR_LIMITS_MALE["police_donator"][idToCheck.toString()];
                if(data){
                    min_drawable = data.min;

                    if(!isProp)
                        max_drawable = (data.max == -2 ? mp.players.local.getNumberOfDrawableVariations(componentID) : data.max);
                    else max_drawable = (data.max == -2 ? mp.players.local.getNumberOfPropDrawableVariations(componentID) : data.max);
                }
                else{
                    min_drawable = -1;

                    if(!isProp)
                        max_drawable = mp.players.local.getNumberOfDrawableVariations(componentID);
                    else max_drawable = mp.players.local.getNumberOfPropDrawableVariations(componentID)
                }
            }
            else{
                var idToCheck = (isProp ? getPropUIID(componentID) : componentID);

                const data = CLOTHING_EDITOR_LIMITS_MALE["fugitive_donator"][idToCheck.toString()];
                if(data){
                    min_drawable = data.min;

                    if(!isProp)
                        max_drawable = (data.max == -2 ? mp.players.local.getNumberOfDrawableVariations(componentID) : data.max);
                    else max_drawable = (data.max == -2 ? mp.players.local.getNumberOfPropDrawableVariations(componentID) : data.max);
                }
                else{
                    min_drawable = -1;

                    if(!isProp)
                        max_drawable = mp.players.local.getNumberOfDrawableVariations(componentID);
                    else max_drawable = mp.players.local.getNumberOfPropDrawableVariations(componentID)
                }
            }
        }
        else{
            if(team == 1){
                var idToCheck = (isProp ? getPropUIID(componentID) : componentID);

                const data = CLOTHING_EDITOR_LIMITS_MALE["police"][idToCheck.toString()];
                if(data){
                    min_drawable = data.min;

                    if(!isProp)
                        max_drawable = (data.max == -2 ? mp.players.local.getNumberOfDrawableVariations(componentID) : data.max);
                    else max_drawable = (data.max == -2 ? mp.players.local.getNumberOfPropDrawableVariations(componentID) : data.max);
                }
                else{
                    min_drawable = -1;

                    if(!isProp)
                        max_drawable = mp.players.local.getNumberOfDrawableVariations(componentID);
                    else max_drawable = mp.players.local.getNumberOfPropDrawableVariations(componentID)
                }
            }
            else{
                var idToCheck = (isProp ? getPropUIID(componentID) : componentID);

                const data = CLOTHING_EDITOR_LIMITS_MALE["fugitive"][idToCheck.toString()];
                if(data){
                    min_drawable = data.min;

                    if(!isProp)
                        max_drawable = (data.max == -2 ? mp.players.local.getNumberOfDrawableVariations(componentID) : data.max);
                    else max_drawable = (data.max == -2 ? mp.players.local.getNumberOfPropDrawableVariations(componentID) : data.max);
                }
                else{
                    min_drawable = -1;

                    if(!isProp)
                        max_drawable = mp.players.local.getNumberOfDrawableVariations(componentID);
                    else max_drawable = mp.players.local.getNumberOfPropDrawableVariations(componentID)
                }
            }            
        }
    }
    else{
        if(isDonator){
            if(team == 1){
                var idToCheck = (isProp ? getPropUIID(componentID) : componentID);

                const data = CLOTHING_EDITOR_LIMITS_MALE["police_donator"][idToCheck.toString()];
                if(data){
                    min_drawable = data.min;

                    if(!isProp)
                        max_drawable = (data.max == -2 ? mp.players.local.getNumberOfDrawableVariations(componentID) : data.max);
                    else max_drawable = (data.max == -2 ? mp.players.local.getNumberOfPropDrawableVariations(componentID) : data.max);
                }
                else{
                    min_drawable = -1;

                    if(!isProp)
                        max_drawable = mp.players.local.getNumberOfDrawableVariations(componentID);
                    else max_drawable = mp.players.local.getNumberOfPropDrawableVariations(componentID)
                }
            }
            else{
                var idToCheck = (isProp ? getPropUIID(componentID) : componentID);

                const data = CLOTHING_EDITOR_LIMITS_MALE["fugitive_donator"][idToCheck.toString()];
                if(data){
                    min_drawable = data.min;

                    if(!isProp)
                        max_drawable = (data.max == -2 ? mp.players.local.getNumberOfDrawableVariations(componentID) : data.max);
                    else max_drawable = (data.max == -2 ? mp.players.local.getNumberOfPropDrawableVariations(componentID) : data.max);
                }
                else{
                    min_drawable = -1;

                    if(!isProp)
                        max_drawable = mp.players.local.getNumberOfDrawableVariations(componentID);
                    else max_drawable = mp.players.local.getNumberOfPropDrawableVariations(componentID)
                }
            }
        }
        else{
            if(team == 1){
                var idToCheck = (isProp ? getPropUIID(componentID) : componentID);

                const data = CLOTHING_EDITOR_LIMITS_FEMALE["police"][idToCheck.toString()];
                if(data){
                    min_drawable = data.min;

                    if(!isProp)
                        max_drawable = (data.max == -2 ? mp.players.local.getNumberOfDrawableVariations(componentID) : data.max);
                    else max_drawable = (data.max == -2 ? mp.players.local.getNumberOfPropDrawableVariations(componentID) : data.max);
                }
                else{
                    min_drawable = -1;

                    if(!isProp)
                        max_drawable = mp.players.local.getNumberOfDrawableVariations(componentID);
                    else max_drawable = mp.players.local.getNumberOfPropDrawableVariations(componentID)
                }
            }
            else{
                var idToCheck = (isProp ? getPropUIID(componentID) : componentID);

                const data = CLOTHING_EDITOR_LIMITS_FEMALE["fugitive"][idToCheck.toString()];
                if(data){
                    min_drawable = data.min;

                    if(!isProp)
                        max_drawable = (data.max == -2 ? mp.players.local.getNumberOfDrawableVariations(componentID) : data.max);
                    else max_drawable = (data.max == -2 ? mp.players.local.getNumberOfPropDrawableVariations(componentID) : data.max);
                }
                else{
                    min_drawable = -1;

                    if(!isProp)
                        max_drawable = mp.players.local.getNumberOfDrawableVariations(componentID);
                    else max_drawable = mp.players.local.getNumberOfPropDrawableVariations(componentID)
                }
            }            
        }       
    }

    // const max_drawable = (isProp ? 
    //     mp.players.local.getNumberOfPropDrawableVariations(componentID) : 
    //     mp.players.local.getNumberOfDrawableVariations(componentID));
    const max_texture = (isProp ? 
        mp.players.local.getNumberOfPropTextureVariations(componentID, inputID) :
        mp.players.local.getNumberOfTextureVariations(componentID, inputID));

    if(global.ServerUI) global.ServerUI.call("dialog_clothing_editor_open", inputID, inputTexture, {
        id:{
            min:min_drawable,
            max:max_drawable
        },
        texture:{
            min:0,
            max:max_texture
        }
    });
}
mp.events.add("OutfitCreator:ChangeType", onOutfitCreatorChangeType);
mp.events.add("OutfitCreator:OpenEditor", (inputID, inputTexture, isMale, team) => {
    mp.events.callLocal("ForceCloseMenu");

    // the editor opens with bracelets selected by default?
    onOutfitCreatorChangeType(CURRENT_EDIT_ITEM.index, CURRENT_EDIT_ITEM.drawable, CURRENT_EDIT_ITEM.texture, CURRENT_EDIT_ITEM.isProp);
    OUTFIT_EDITOR_DATA = {
        isMale:isMale,
        team:team
    }
    mp.gui.cursor.show(true, true);

    if(!mp.players.local.hasVariable("OUTFIT:Editing")) return;
    CURRENT_OUTFIT_DATA = JSON.parse(mp.players.local.getVariable("OUTFIT:Editing"));
    if(!CURRENT_OUTFIT_DATA) return;


    if(global.ServerUI) global.ServerUI.call("dialog_clothing_editor_open", inputID, inputTexture, {
        id:{
            min:-1,
            max:-1
        },
        texture:{
            min:-1,
            max:-1
        }
    });
});


var EditorCamera = null;
var CanRotateInEditor = false;
function rotatePlayer(z_value){
    const rotation = mp.players.local.getHeading();
    mp.players.local.setHeading(rotation+z_value);
}
mp.events.add('render', () => {
    if(!CanRotateInEditor) return;

    if(mp.keys.isDown(69))
        return rotatePlayer(1);

    if(mp.keys.isDown(81))
        return rotatePlayer(-1);
});

/* Character Editor Scene Setup */
mp.events.add("CharacterEditor:SceneSetup", (camX, camY, camZ, camRX, camRY, camRZ) => {
    if(EditorCamera){
        EditorCamera.destroy();
        EditorCamera = null;
    }

    EditorCamera = mp.cameras.new('default', new mp.Vector3(camX, camY, camZ), new mp.Vector3(camRX, camRY, camRZ), 40);
    EditorCamera.setActive(true);
    mp.game.cam.renderScriptCams(true, false, 0, true, false);

    CanRotateInEditor = true;
    mp.events.callLocal('ToggleHud', false);
});

mp.events.add("CharacterEditor:DestroyScene", () => {
    if(EditorCamera){
        EditorCamera.setActive(false);
        EditorCamera.destroy();
        EditorCamera = null;
    }
    mp.game.cam.renderScriptCams(false, false, 0, true, false);

    CanRotateInEditor = false;
    mp.events.callLocal('ToggleHud', true);
});

const CLOTHING_EDITOR_LIMITS_FEMALE = { // min -1 means -1 aka can be nothing, max -2 means all/limit
    "police":{ // this category's max values need updating if custom fugitive clothes are ever added.
        "-5":{min:-1, max:-2}, // bracelets
        "-4":{min:-1, max:-2}, // watches
        "-3":{min:-1, max:-2}, // ears
        "-2":{min:-1, max:-2}, // glasses
        "-1":{min:195, max:-2}, // hats
        "0":{min:0, max:0}, // head
        "1":{min:-1, max:-1}, // masks - set to anything for donators
        "2":{min:0, max:0}, // hairs - set in character appearance editor bruv
        "3":{min:0, max:-2}, // torsos
        "4":{min:191, max:221}, // pants
        "5":{min:111, max:183}, // bags/hands
        "6":{min:25, max:25}, // shoes
        "7":{min:143, max:-2}, // accessories
        "8":{min:0, max:335}, // undershirts
        "9":{min:-1, max:143}, // armors
        "10":{min:-1, max:240}, // decals
        "11":{min:535, max:654}, // tops
    },
    "police_donator":{
        "-5":{min:-1, max:-2}, // bracelets
        "-4":{min:-1, max:-2}, // watches
        "-3":{min:-1, max:-2}, // ears
        "-2":{min:-1, max:-2}, // glasses
        "-1":{min:-1, max:-2}, // hats
        "0":{min:0, max:0}, // head
        "1":{min:-1, max:-2}, // masks - set to anything for donators
        "2":{min:0, max:0}, // hairs - set in character appearance editor bruv
        "3":{min:0, max:-2}, // torsos
        "4":{min:0, max:-2}, // pants
        "5":{min:0, max:-2}, // bags/hands
        "6":{min:1, max:-2}, // shoes
        "7":{min:-1, max:-2}, // accessories
        "8":{min:0, max:-2}, // undershirts
        "9":{min:-1, max:-2}, // armors
        "10":{min:-1, max:-2}, // decals
        "11":{min:0, max:-2}, // tops        
    },
    "fugitive_donator":{
        "-5":{min:-1, max:-2}, // bracelets
        "-4":{min:-1, max:-2}, // watches
        "-3":{min:-1, max:-2}, // ears
        "-2":{min:-1, max:-2}, // glasses
        "-1":{min:-1, max:195}, // hats
        "0":{min:0, max:0}, // head
        "1":{min:-1, max:-2}, // masks - set to anything for donators
        "2":{min:0, max:0}, // hairs - set in character appearance editor bruv
        "3":{min:0, max:-2}, // torsos
        "4":{min:0, max:190}, // pants
        "5":{min:0, max:110}, // bags/hands
        "6":{min:1, max:-2}, // shoes
        "7":{min:-1, max:142}, // accessories
        "8":{min:0, max:244}, // undershirts
        "9":{min:-1, max:57}, // armors
        "10":{min:-1, max:186}, // decals
        "11":{min:0, max:533}, // tops        
    },
    "fugitive":{
        "-5":{min:-1, max:-2}, // bracelets
        "-4":{min:-1, max:-2}, // watches
        "-3":{min:-1, max:-2}, // ears
        "-2":{min:-1, max:-2}, // glasses
        "-1":{min:-1, max:10}, // hats
        "0":{min:0, max:0}, // head
        "1":{min:-1, max:-1}, // masks - set to anything for donators
        "2":{min:0, max:0}, // hairs - set in character appearance editor bruv
        "3":{min:0, max:-2}, // torsos
        "4":{min:0, max:10}, // pants
        "5":{min:0, max:10}, // bags/hands
        "6":{min:1, max:10}, // shoes
        "7":{min:-1, max:10}, // accessories
        "8":{min:0, max:240}, // undershirts
        "9":{min:-1, max:-1}, // armors
        "10":{min:-1, max:-1}, // decals
        "11":{min:0, max:10}, // tops        
    }
}
const CLOTHING_EDITOR_LIMITS_MALE = { // min -1 means -1 aka can be nothing, max -2 means all/limit
    "police":{ // this category's max values need updating if custom fugitive clothes are ever added.
        "-5":{min:-1, max:-2}, // bracelets
        "-4":{min:-1, max:-2}, // watches
        "-3":{min:-1, max:-2}, // ears
        "-2":{min:-1, max:-2}, // glasses
        "-1":{min:195, max:-2}, // hats
        "0":{min:0, max:0}, // head
        "1":{min:-1, max:-1}, // masks - set to anything for donators
        "2":{min:0, max:0}, // hairs - set in character appearance editor bruv
        "3":{min:0, max:-2}, // torsos
        "4":{min:177, max:197}, // pants
        "5":{min:111, max:156}, // bags/hands
        "6":{min:25, max:25}, // shoes
        "7":{min:175, max:197}, // accessories
        "8":{min:0, max:258}, // undershirts
        "9":{min:-1, max:104}, // armors
        "10":{min:-1, max:208}, // decals
        "11":{min:499, max:572}, // tops
    },
    "police_donator":{
        "-5":{min:-1, max:-2}, // bracelets
        "-4":{min:-1, max:-2}, // watches
        "-3":{min:-1, max:-2}, // ears
        "-2":{min:-1, max:-2}, // glasses
        "-1":{min:-1, max:-2}, // hats
        "0":{min:0, max:0}, // head
        "1":{min:-1, max:-2}, // masks - set to anything for donators
        "2":{min:0, max:0}, // hairs - set in character appearance editor bruv
        "3":{min:0, max:-2}, // torsos
        "4":{min:0, max:-2}, // pants
        "5":{min:0, max:-2}, // bags/hands
        "6":{min:1, max:-2}, // shoes
        "7":{min:-1, max:-2}, // accessories
        "8":{min:0, max:-2}, // undershirts
        "9":{min:-1, max:-2}, // armors
        "10":{min:-1, max:-2}, // decals
        "11":{min:0, max:-2}, // tops        
    },
    "fugitive_donator":{
        "-5":{min:-1, max:-2}, // bracelets
        "-4":{min:-1, max:-2}, // watches
        "-3":{min:-1, max:-2}, // ears
        "-2":{min:-1, max:-2}, // glasses
        "-1":{min:-1, max:196}, // hats
        "0":{min:0, max:0}, // head
        "1":{min:-1, max:-2}, // masks - set to anything for donators
        "2":{min:0, max:0}, // hairs - set in character appearance editor bruv
        "3":{min:0, max:-2}, // torsos
        "4":{min:0, max:176}, // pants
        "5":{min:0, max:110}, // bags/hands
        "6":{min:1, max:-2}, // shoes
        "7":{min:-1, max:174}, // accessories
        "8":{min:0, max:198}, // undershirts
        "9":{min:-1, max:57}, // armors
        "10":{min:-1, max:169}, // decals
        "11":{min:0, max:494}, // tops        
    },
    "fugitive":{
        "-5":{min:-1, max:-2}, // bracelets
        "-4":{min:-1, max:-2}, // watches
        "-3":{min:-1, max:-2}, // ears
        "-2":{min:-1, max:-2}, // glasses
        "-1":{min:-1, max:10}, // hats
        "0":{min:0, max:0}, // head
        "1":{min:-1, max:-1}, // masks - set to anything for donators
        "2":{min:0, max:0}, // hairs - set in character appearance editor bruv
        "3":{min:0, max:-2}, // torsos
        "4":{min:0, max:10}, // pants
        "5":{min:0, max:10}, // bags/hands
        "6":{min:1, max:10}, // shoes
        "7":{min:-1, max:10}, // accessories
        "8":{min:0, max:190}, // undershirts
        "9":{min:-1, max:-1}, // armors
        "10":{min:-1, max:-1}, // decals
        "11":{min:0, max:10}, // tops        
    }
}


// Appearance Editor
var APPEARANCE_IsMale = true;
var CURRENT_APPEARANCE_DATA = undefined;


mp.events.add("CharacterEditor:ToggleAppearanceEditor", (toggle, gender, appearanceData) => {
    if(appearanceData.length !== 0) CURRENT_APPEARANCE_DATA = JSON.parse(appearanceData);
    else CURRENT_APPEARANCE_DATA = undefined;

    APPEARANCE_IsMale = gender;
    if(global.ServerUI) global.ServerUI.call('character_editor_toggle', toggle);

    setAppearanceEditorCategories();
});

function updateCategoryOptionValues(category, options){
    if(!CURRENT_APPEARANCE_DATA) return;
    var OptionsOBJ = options;
    if(category === "c_heritage"){
        const HeadBlendData = CURRENT_APPEARANCE_DATA.HeadBlend;
        // Exactly in order of the options in the category!
        const DATA = [];
        DATA.push(!CURRENT_APPEARANCE_DATA.IsMale); // This needs to be inverted because Male == false in the editor
        DATA.push(HeadBlendData.FirstParentID);
        DATA.push(HeadBlendData.SecondParentID);
        DATA.push(HeadBlendData.MixParentID);
        DATA.push(HeadBlendData.FirstSkinID);
        DATA.push(HeadBlendData.SecondSkinID);
        DATA.push(HeadBlendData.MixSkinID);
        DATA.push(HeadBlendData.ShapeMix);
        DATA.push(HeadBlendData.SkinMix);
        DATA.push(HeadBlendData.ThirdMix);
        DATA.push(CURRENT_APPEARANCE_DATA.EyeColor);
        /*
        const HeadBlendData = CURRENT_APPEARANCE_DATA.HeadBlend;
        const IsMale = CURRENT_APPEARANCE_DATA.IsMale;
        const Parent1 = HeadBlendData.FirstParentID;
        const Parent2 = HeadBlendData.SecondParentID;
        const Parent3 = HeadBlendData.MixParentID;
        const Skin1 = HeadBlendData.FirstSkinID;
        const Skin2 = HeadBlendData.SecondSkinID;
        const Skin3 = HeadBlendData.MixSkinID;
        const ShapeMix = HeadBlendData.ShapeMix;
        const SkinMix = HeadBlendData.SkinMix;
        const ThirdMix = HeadBlendData.ThirdMix;
        const EyeColor = CURRENT_APPEARANCE_DATA.EyeColor;
        */

        for(let i = 0 ; i < DATA.length ; i++){
            OptionsOBJ[i].value = DATA[i];
        }

        if(global.ServerUI) global.ServerUI.call('character_editor_set_options', OptionsOBJ);
        return;
    }
    if(category === "c_features"){
        const FaceFeatures = CURRENT_APPEARANCE_DATA.FaceFeatures;
        for(let i = 0 ; i < FaceFeatures.length ; i++){
            OptionsOBJ[i].value = FaceFeatures[i].Scale;
        }

        if(global.ServerUI) global.ServerUI.call('character_editor_set_options', OptionsOBJ);
        return;
    }

    if(category === "c_overlays"){
        const HeadOverlays = CURRENT_APPEARANCE_DATA.HeadOverlays;
        for(let i = 0 ; i < HeadOverlays.length ; i++){
            const Overlay = HeadOverlays[i];
            const Index = Overlay.Index;
            const Opacity = Overlay.Opacity;
            const Color1 = Overlay.FirstColor;
            const Color2 = Overlay.SecondColor;

            if(OptionsOBJ.find(h => h.id == `head_overlay_${i}`))
                OptionsOBJ.find(h => h.id == `head_overlay_${i}`).value = Index;

            if(OptionsOBJ.find(h => h.id == `opacity_head_overlay_${i}`))
                OptionsOBJ.find(h => h.id == `opacity_head_overlay_${i}`).value = Opacity;
   
            if(OptionsOBJ.find(h => h.id == `color1_head_overlay_${i}`))
                OptionsOBJ.find(h => h.id == `color1_head_overlay_${i}`).value = Color1;

            if(OptionsOBJ.find(h => h.id == `color2_head_overlay_${i}`))
                OptionsOBJ.find(h => h.id == `color2_head_overlay_${i}`).value = Color2;
        }

        if(global.ServerUI) global.ServerUI.call('character_editor_set_options', OptionsOBJ);
        return;
    }

    if(category === "c_hair"){
        const HairData = CURRENT_APPEARANCE_DATA.Hair;
        const HairOverlays = CURRENT_APPEARANCE_DATA.HairOverlays;

        const Index = HairData.Index;
        const Color = HairData.Color;
        const HighlightColor = HairData.HighlightColor;

        OptionsOBJ.find(o => o.id === "hair_id").value = Index;
        OptionsOBJ.find(o => o.id === "hair_color1").value = Color;
        OptionsOBJ.find(o => o.id === "hair_color2").value = HighlightColor;

        OptionsOBJ.find(o => o.id === "hair_overlay").value = HairOverlays;

        if(global.ServerUI) global.ServerUI.call('character_editor_set_options', OptionsOBJ);
        return;
    }
}

function setAppearanceEditorCategories(){
    const options = [
        {
            id:"c_heritage",
            name:"Heritage"
        },
        {
            id:"c_features",
            name:"Facial Features"
        },
        {
            id:"c_overlays",
            name:"Head Overlays"
        },
        {
            id:"c_hair",
            name:"Hair"
        },  
        {
            id:"c_tattoos",
            name:"Tattoos"
        },
    ];
    if(global.ServerUI) global.ServerUI.call('character_editor_set_categories', options);
}
// /triggerjs mp.events.callLocal("CharacterEditor:ToggleAppearanceEditor", true)
function setAppearanceEditorOptions(category){
    var options = [];
    const categoryJSON = OptionsPerCategory.find(c => c.category === category);
    if(!categoryJSON) return;

    options = categoryJSON.options;
    if(category === "c_hair"){
        const overlaysToGet = getHairOverlaysForGender(APPEARANCE_IsMale);
        const overlaysToSet = overlaysToGet.map((o) => {
            return {id:`hair_overlay_${overlaysToGet.indexOf(o).toString()}`, name:o.name};
        });
        
        var optionToSet = options.find(o => o.id === "hair_overlay");
        if(optionToSet !== undefined && optionToSet.values) optionToSet.values = overlaysToSet;
    }

    if(global.ServerUI) global.ServerUI.call('character_editor_set_options', options);

    updateCategoryOptionValues(category, options);
}

// LAST NUMBER IN FACE FEATURES CATEGORY = INDEX TO CHANGE
// This can also serve as the DEFAULT VALUES!!
const OptionsPerCategory = [
    {
        category:"c_heritage",
        options:[
            {
                type: "toggle",
                id: "gender_option",
                name: "Gender",
                value: false,
                labels: ["Male", "Female"]
            },
            {
                type: "slider",
                id: "shape_parent_1",
                name: "Parent 1",
                value: 0,
                min: 0,
                max: 45,
                step: 1,
            },
            {
                type: "slider",
                id: "shape_parent_2",
                name: "Parent 2",
                value: 0,
                min: 0,
                max: 45,
                step: 1,
            },
            {
                type: "slider",
                id: "shape_parent_3",
                name: "Mix Parent",
                value: 0,
                min: 0,
                max: 45,
                step: 1,
            },
            {
                type: "slider",
                id: "skin_parent_1",
                name: "Skin Parent 1",
                value: 0,
                min: 0,
                max: 45,
                step: 1,
            },
            {
                type: "slider",
                id: "skin_parent_2",
                name: "Skin Parent 2",
                value: 0,
                min: 0,
                max: 45,
                step: 1,
            },
            {
                type: "slider",
                id: "skin_parent_3",
                name: "Skin Mix Parent",
                value: 0,
                min: 0,
                max: 45,
                step: 1,
            },
            {
                type: "slider",
                id: "shape_mix",
                name: "Shape Mix",
                value: 0.5,
                min: 0,
                max: 1.0,
                step: 0.1,
            },
            {
                type: "slider",
                id: "skin_mix",
                name: "Skin Mix",
                value: 0.5,
                min: 0,
                max: 1.0,
                step: 0.1,
            },
            {
                type: "slider",
                id: "third_mix",
                name: "Third Mix",
                value: 0.5,
                min: 0,
                max: 1.0,
                step: 0.1,
            },
            {
                type: "slider",
                id: "eye_color",
                name: "Eye Color",
                value: 0,
                min: 0,
                max: 31,
                step: 1,
            },
        ]
    },
    {
        category:"c_features", 
        options:[
            {
                "type": "slider",
                "id": "face_feature_0",
                "name": "Nose width",
                "value": 0,
                "min": -1.0,
                "max": 1.0,
                "step": 0.1
            },
            {
                "type": "slider",
                "id": "face_feature_1",
                "name": "Nose height",
                "value": 0,
                "min": -1.0,
                "max": 1.0,
                "step": 0.1
            },
            {
                "type": "slider",
                "id": "face_feature_2",
                "name": "Nose length",
                "value": 0,
                "min": -1.0,
                "max": 1.0,
                "step": 0.1
            },
            {
                "type": "slider",
                "id": "face_feature_3",
                "name": "Nose bridge",
                "value": 0,
                "min": -1.0,
                "max": 1.0,
                "step": 0.1
            },
            {
                "type": "slider",
                "id": "face_feature_4",
                "name": "Nose tip",
                "value": 0,
                "min": -1.0,
                "max": 1.0,
                "step": 0.1
            },
            {
                "type": "slider",
                "id": "face_feature_5",
                "name": "Nose bridge shift",
                "value": 0,
                "min": -1.0,
                "max": 1.0,
                "step": 0.1
            },
            {
                "type": "slider",
                "id": "face_feature_6",
                "name": "Brow height",
                "value": 0,
                "min": -1.0,
                "max": 1.0,
                "step": 0.1
            },
            {
                "type": "slider",
                "id": "face_feature_7",
                "name": "Brow width",
                "value": 0,
                "min": -1.0,
                "max": 1.0,
                "step": 0.1
            },
            {
                "type": "slider",
                "id": "face_feature_8",
                "name": "Cheekbone height",
                "value": 0,
                "min": -1.0,
                "max": 1.0,
                "step": 0.1
            },
            {
                "type": "slider",
                "id": "face_feature_9",
                "name": "Cheekbone width",
                "value": 0,
                "min": -1.0,
                "max": 1.0,
                "step": 0.1
            },
            {
                "type": "slider",
                "id": "face_feature_10",
                "name": "Cheeks width",
                "value": 0,
                "min": -1.0,
                "max": 1.0,
                "step": 0.1
            },
            {
                "type": "slider",
                "id": "face_feature_11",
                "name": "Eye size",
                "value": 0,
                "min": -1.0,
                "max": 1.0,
                "step": 0.1
            },
            {
                "type": "slider",
                "id": "face_feature_12",
                "name": "Lip size",
                "value": 0,
                "min": -1.0,
                "max": 1.0,
                "step": 0.1
            },
            {
                "type": "slider",
                "id": "face_feature_13",
                "name": "Jaw width",
                "value": 0,
                "min": -1.0,
                "max": 1.0,
                "step": 0.1
            },
            {
                "type": "slider",
                "id": "face_feature_14",
                "name": "Jaw height",
                "value": 0,
                "min": -1.0,
                "max": 1.0,
                "step": 0.1
            },
            {
                "type": "slider",
                "id": "face_feature_15",
                "name": "Chin length",
                "value": 0,
                "min": -1.0,
                "max": 1.0,
                "step": 0.1
            },
            {
                "type": "slider",
                "id": "face_feature_16",
                "name": "Chin position",
                "value": 0,
                "min": -1.0,
                "max": 1.0,
                "step": 0.1
            },
            {
                "type": "slider",
                "id": "face_feature_17",
                "name": "Chin width",
                "value": 0,
                "min": -1.0,
                "max": 1.0,
                "step": 0.1
            },
            {
                "type": "slider",
                "id": "face_feature_18",
                "name": "Chin shape",
                "value": 0,
                "min": -1.0,
                "max": 1.0,
                "step": 0.1
            },
            {
                "type": "slider",
                "id": "face_feature_19",
                "name": "Neck width",
                "value": 0,
                "min": -1.0,
                "max": 1.0,
                "step": 0.1
            }
        ]
    },
    {
        category:"c_overlays",
        options:[
            {
                "type": "slider",
                "id": "head_overlay_0",
                "name": "Blemishes",
                "value": 0,
                "min": 0,
                "max": 23,
                "step": 1
            },
            {
                "type": "slider",
                "id": "color1_head_overlay_0",
                "name": "Blemishes Color 1",
                "value": 0,
                "min": 0,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "color2_head_overlay_0",
                "name": "Blemishes Color 2",
                "value": 0,
                "min": 0,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "opacity_head_overlay_0",
                "name": "Blemishes Opacity",
                "value": 1.0,
                "min": 0,
                "max": 1.0,
                "step": 0.1
            },
            {
                "type": "slider",
                "id": "head_overlay_1",
                "name": "Facial Hair",
                "value": 0,
                "min": 0,
                "max": 28,
                "step": 1
            },
            {
                "type": "slider",
                "id": "color1_head_overlay_1",
                "name": "Facial Hair Color 1",
                "value": 0,
                "min": 0,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "color2_head_overlay_1",
                "name": "Facial Hair Color 2",
                "value": 0,
                "min": 0,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "opacity_head_overlay_1",
                "name": "Facial Hair Opacity",
                "value": 1.0,
                "min": 0,
                "max": 1.0,
                "step": 0.1
            },
            {
                "type": "slider",
                "id": "head_overlay_2",
                "name": "Eyebrows",
                "value": 0,
                "min": 0,
                "max": 33,
                "step": 1
            },
            {
                "type": "slider",
                "id": "color1_head_overlay_2",
                "name": "Eyebrows Color 1",
                "value": 0,
                "min": 0,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "color2_head_overlay_2",
                "name": "Eyebrows Color 2",
                "value": 0,
                "min": 0,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "opacity_head_overlay_2",
                "name": "Eyebrows Opacity",
                "value": 1.0,
                "min": 0,
                "max": 1.0,
                "step": 0.1
            },
            {
                "type": "slider",
                "id": "head_overlay_3",
                "name": "Ageing",
                "value": 0,
                "min": 0,
                "max": 14,
                "step": 1
            },
            {
                "type": "slider",
                "id": "color1_head_overlay_3",
                "name": "Ageing Color 1",
                "value": 0,
                "min": 0,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "color2_head_overlay_3",
                "name": "Ageing Color 2",
                "value": 0,
                "min": 0,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "opacity_head_overlay_3",
                "name": "Ageing Opacity",
                "value": 1.0,
                "min": 0,
                "max": 1.0,
                "step": 0.1
            },
            {
                "type": "slider",
                "id": "head_overlay_4",
                "name": "Makeup",
                "value": 0,
                "min": 0,
                "max": 74,
                "step": 1
            },
            {
                "type": "slider",
                "id": "color1_head_overlay_4",
                "name": "Makeup Color 1",
                "value": 0,
                "min": 0,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "color2_head_overlay_4",
                "name": "Makeup Color 2",
                "value": 0,
                "min": 0,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "opacity_head_overlay_4",
                "name": "Makeup Opacity",
                "value": 1.0,
                "min": 0,
                "max": 1.0,
                "step": 0.1
            },
            {
                "type": "slider",
                "id": "head_overlay_5",
                "name": "Blush",
                "value": 0,
                "min": 0,
                "max": 32,
                "step": 1
            },
            {
                "type": "slider",
                "id": "color1_head_overlay_5",
                "name": "Blush Color 1",
                "value": 0,
                "min": 0,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "color2_head_overlay_5",
                "name": "Blush Color 2",
                "value": 0,
                "min": 0,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "opacity_head_overlay_5",
                "name": "Blush Opacity",
                "value": 1.0,
                "min": 0,
                "max": 1.0,
                "step": 0.1
            },
            {
                "type": "slider",
                "id": "head_overlay_6",
                "name": "Complexion",
                "value": 0,
                "min": 0,
                "max": 11,
                "step": 1
            },
            {
                "type": "slider",
                "id": "color1_head_overlay_6",
                "name": "Complexion Color 1",
                "value": 0,
                "min": 0,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "color2_head_overlay_6",
                "name": "Complexion Color 2",
                "value": 0,
                "min": 0,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "opacity_head_overlay_6",
                "name": "Complexion Opacity",
                "value": 1.0,
                "min": 0,
                "max": 1.0,
                "step": 0.1
            },
            {
                "type": "slider",
                "id": "head_overlay_7",
                "name": "Sun Damage",
                "value": 0,
                "min": 0,
                "max": 10,
                "step": 1
            },
            {
                "type": "slider",
                "id": "color1_head_overlay_7",
                "name": "Sun Damage Color 1",
                "value": 0,
                "min": 0,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "color2_head_overlay_7",
                "name": "Sun Damage Color 2",
                "value": 0,
                "min": 0,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "opacity_head_overlay_7",
                "name": "Sun Damage Opacity",
                "value": 1.0,
                "min": 0,
                "max": 1.0,
                "step": 0.1
            },
            {
                "type": "slider",
                "id": "head_overlay_8",
                "name": "Lipstick",
                "value": 0,
                "min": 0,
                "max": 9,
                "step": 1
            },
            {
                "type": "slider",
                "id": "color1_head_overlay_8",
                "name": "Lipstick Color 1",
                "value": 0,
                "min": 0,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "color2_head_overlay_8",
                "name": "Lipstick Color 2",
                "value": 0,
                "min": 0,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "opacity_head_overlay_8",
                "name": "Lipstick Opacity",
                "value": 1.0,
                "min": 0,
                "max": 1.0,
                "step": 0.1
            },
            {
                "type": "slider",
                "id": "head_overlay_9",
                "name": "Moles/Freckles",
                "value": 0,
                "min": 0,
                "max": 17,
                "step": 1
            },
            {
                "type": "slider",
                "id": "color1_head_overlay_9",
                "name": "Moles/Freckles Color 1",
                "value": 0,
                "min": 0,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "color2_head_overlay_9",
                "name": "Moles/Freckles Color 2",
                "value": 0,
                "min": 0,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "opacity_head_overlay_9",
                "name": "Moles/Freckles Opacity",
                "value": 1.0,
                "min": 0,
                "max": 1.0,
                "step": 0.1
            },
            {
                "type": "slider",
                "id": "head_overlay_10",
                "name": "Chest Hair",
                "value": 0,
                "min": 0,
                "max": 16,
                "step": 1
            },
            {
                "type": "slider",
                "id": "color1_head_overlay_10",
                "name": "Chest Hair Color 1",
                "value": 0,
                "min": 0,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "color2_head_overlay_10",
                "name": "Chest Hair Color 2",
                "value": 0,
                "min": 0,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "opacity_head_overlay_10",
                "name": "Chest Hair Opacity",
                "value": 1.0,
                "min": 0,
                "max": 1.0,
                "step": 0.1
            },
            {
                "type": "slider",
                "id": "head_overlay_11",
                "name": "Body Blemishes",
                "value": 0,
                "min": 0,
                "max": 11,
                "step": 1
            },
            {
                "type": "slider",
                "id": "color1_head_overlay_11",
                "name": "Body Blemishes Color 1",
                "value": 0,
                "min": 0,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "color2_head_overlay_11",
                "name": "Body Blemishes Color 2",
                "value": 0,
                "min": 0,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "opacity_head_overlay_11",
                "name": "Body Blemishes Opacity",
                "value": 1.0,
                "min": 0,
                "max": 1.0,
                "step": 0.1
            },
            {
                type: "toggle",
                id: "head_overlay_12",
                name: "Body Blemishes",
                value: false,
                labels: ["Disabled", "Enabled"]
            }
        ]
    },
    {
        category:"c_hair",
        options:[
            {
                "type": "slider",
                "id": "hair_id",
                "name": "Hair",
                "value": 0,
                "min": 0,
                "max": 140,
                "step": 1
            },      
            {
                "type": "slider",
                "id": "hair_color1",
                "name": "Color",
                "value": 0,
                "min": 0,
                "max": 100,
                "step": 1
            },  
            {
                "type": "slider",
                "id": "hair_color2",
                "name": "Highlight Color",
                "value": 0,
                "min": 0,
                "max": 100,
                "step": 1
            },
            {
                type:"checkbox",
                id:"hair_overlay",
                name:"Hair Overlays",
                value:[],
                values:[
                    // Filled out on runtime depending on gender
                ]
            }   
        ]
    }
];

function getHairOverlaysForGender(isMale){
    if(!global.hairDecalsList){
        mp.events.callRemote('ServerConsoleOutput', `Can't find hair decals list?!?!?!?!?!`);
        return;
    }

    var overlays = [];
    if(isMale) overlays = global.hairDecalsList[0];
    else overlays = global.hairDecalsList[1];

    return overlays;
}

mp.events.add("character_editor_set_active_category_id", (id) => {
    mp.events.callRemote(`ServerConsoleOutput`, `[CHAR EDITOR] Category set active to ${id}`);

});
mp.events.add("character_editor_change_category", (id) => {
    mp.events.callRemote(`ServerConsoleOutput`, `[CHAR EDITOR] Category changed to ${id}`);
    setAppearanceEditorOptions(id);
});
mp.events.add("character_editor_close", () => {
    
});
mp.events.add("character_editor_change_option", (id, value) => {
    mp.events.callRemote(`ServerConsoleOutput`, `[CHAR EDITOR] Changed option: ${id} to ${value}`);

    if(id === "gender_option"){
        APPEARANCE_IsMale = !value; // True is female, false is male, therefore invert
    }
});

