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
        mp.players.local.setPropIndex(CURRENT_EDIT_ITEM.index, value, 0, true);
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
            mp.events.callRemote(`ServerConsoleOutput`, `Set ${CURRENT_EDIT_ITEM.index} texture to ${value}`);
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
        "4":{min:191, max:214}, // pants
        "5":{min:111, max:172}, // bags/hands
        "6":{min:25, max:25}, // shoes
        "7":{min:143, max:-2}, // accessories
        "8":{min:0, max:328}, // undershirts
        "9":{min:-1, max:125}, // armors
        "10":{min:-1, max:222}, // decals
        "11":{min:535, max:633}, // tops
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