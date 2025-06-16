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
mp.events.add("mainmenu_character_outfit_remove", (id, team) => {
    mp.events.callRemote("OutfitCreator:Remove", id, team);
});

mp.events.add("mainmenu_character_edit_appearance", () => {
    mp.events.callRemote("AppearanceEditor:Edit");
});
mp.events.add("mainmenu_character_reset_appearance", () => {
    mp.events.callRemote("AppearanceEditor:Reset");
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

mp.events.add("clothing_editor_change_id", (value) => {
    const isDonator = (mp.players.local.hasVariable("Donator") && mp.players.local.getVariable("Donator") >= 3);
    const isMale = OUTFIT_EDITOR_DATA.isMale;
    const team = OUTFIT_EDITOR_DATA.team;

    if(!isDonator && team === 1){
        if(isMale){
            if(!ALLOWED_POLICE_CLOTHING_MALE[`${CURRENT_EDIT_ITEM.index}`].includes(value)){
                mp.events.callLocal("screenNotify", `This clothing item (ID ${value}) is for Gold Donators only on the Police team!`, "error", "bottom-center");
                return;
            }
        }
        else{
              if(!ALLOWED_POLICE_CLOTHING_FEMALE[`${CURRENT_EDIT_ITEM.index}`].includes(value)){
                mp.events.callLocal("screenNotify", `This clothing item (ID ${value}) is for Gold Donators only on the Police team!`, "error", "bottom-center");
                return;
            }
        }
    }
    if(team === 0){
        if(isMale){
            if(ALLOWED_POLICE_CLOTHING_MALE[`${CURRENT_EDIT_ITEM.index}`].includes(value)){
                mp.events.callLocal("screenNotify", `This clothing item (ID ${value}) is only available for the Police team!`, "error", "bottom-center");
                return;
            }
        }
        else{
              if(ALLOWED_POLICE_CLOTHING_FEMALE[`${CURRENT_EDIT_ITEM.index}`].includes(value)){
                mp.events.callLocal("screenNotify", `This clothing item (ID ${value}) is only available for the Police team!`, "error", "bottom-center");
                return;
            }
        }        
    }


    CURRENT_EDIT_ITEM = {...CURRENT_EDIT_ITEM, drawable:value};

    if(CURRENT_EDIT_ITEM.isProp){
        if(value !== -1)
            mp.players.local.setPropIndex(CURRENT_EDIT_ITEM.index, value, 0, true);
        else mp.players.local.clearProp(CURRENT_EDIT_ITEM.index);

        if(CURRENT_OUTFIT_DATA.Props.find(p => p.Index === CURRENT_EDIT_ITEM.index)){
            CURRENT_OUTFIT_DATA.Props.find(p => p.Index === CURRENT_EDIT_ITEM.index).Drawable = value;
            CURRENT_OUTFIT_DATA.Props.find(p => p.Index === CURRENT_EDIT_ITEM.index).Texture = 0;

            mp.events.callLocal("clothing_editor_change_type", getPropUIID(CURRENT_EDIT_ITEM.index));
        }
    }
    else{
        mp.players.local.setComponentVariation(CURRENT_EDIT_ITEM.index, value, 0, 0);
        if(CURRENT_OUTFIT_DATA.Components[CURRENT_EDIT_ITEM.index]){
            CURRENT_OUTFIT_DATA.Components[CURRENT_EDIT_ITEM.index].Drawable = value;
            CURRENT_OUTFIT_DATA.Components[CURRENT_EDIT_ITEM.index].Texture = 0;

            mp.events.callLocal("clothing_editor_change_type", CURRENT_EDIT_ITEM.index);
        }
    }
});
mp.events.add("clothing_editor_change_texture", (value) => {
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
mp.events.add("clothing_editor_change_type", (value) => {
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
mp.events.add("clothing_editor_close", (value) => {
    mp.gui.cursor.show(false, false);

      if(global.ClothingEditorHelpDOM){
        global.ClothingEditorHelpDOM.active = false; 
    }
    mp.events.callRemote("CharacterEditor:CloseEvent");
});
mp.events.add("clothing_editor_save", (value) => {
    mp.gui.cursor.show(false, false);

    // For debugging purposes: save outfit edits
    mp.events.callRemote("OutfitCreator:SaveOutfitChanges", JSON.stringify(CURRENT_OUTFIT_DATA));
    mp.events.callRemote("CharacterEditor:CloseEvent");
});


// Server -> Clientsidelju
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

                const data = CLOTHING_EDITOR_LIMITS_FEMALE["police_donator"][idToCheck.toString()];

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

                const data = CLOTHING_EDITOR_LIMITS_FEMALE["fugitive_donator"][idToCheck.toString()];

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

    if(global.ClothingEditorHelpDOM && team == 1){
        global.ClothingEditorHelpDOM.active = true;
        global.ClothingEditorHelpDOM.call("toggle_panel", (isMale ? "male" : "female"));   
    }

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

mp.events.add("OneSecondEvent", () => {
    if(CanRotateInEditor === true) // WE'RE ALL GOING TO HELL WITH THIS ONE 🔥🔥🔥🔥🔥🔥🔥🔥🔥🔥
        mp.events.callLocal('ToggleHud', false);
});

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
    mp.players.local.taskPlayAnim("nm@hands", "hands_up", 1.0, 1.0, -1, 1, 1.0, true, true, true);
    mp.players.local.freezePosition(true);
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
    mp.players.local.freezePosition(false);
    mp.players.local.clearTasksImmediately();
});

/*
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
*/
const ALLOWED_POLICE_CLOTHING_MALE = {
    "11":[55, 316, 317, 318, 319, 320], // tops
    "10":[70, 207, 208, 209, 210], // decals
    "4":[35, 121], // pants
    "8":[15, 58, 59, 122, 129, 130, 131, 153, 154, 170], // undershirts
    "9":[53, 54, 55, 57], // armors
    "7":[125, 128, 133, 146, 147, 148], // accessories
    "3":[0, 4, 17, 18, 19, 20, 21, 22, 23, 24, 30, 31, 32, 33, 55, 56], // torso
    "0":[46, 115, 123, 124, 125, 126, 141, 142, 143], // hats
}
const ALLOWED_POLICE_CLOTHING_FEMALE = {
    "11":[48, 327, 328, 329, 330, 331],
    "10":[79, 223, 224, 225, 226],
    "4":[34, 127],
    "8":[15, 35, 36, 152, 159, 160, 161, 189, 190],
    "9":[2,3,4,5,6,7,8,9,10,11,12,13, 53, 54, 55],
    "7":[95, 98, 102, 115, 116, 117],
    "3":[0, 3, 18, 19, 20, 23, 33, 36],
    "0":[45, 122, 123, 124, 125, 140, 141, 142],
}


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
        "4":{min:0, max:217}, // pants
        "5":{min:0, max:-2}, // bags/hands
        "6":{min:25, max:25}, // shoes
        "7":{min:0, max:-2}, // accessories
        "8":{min:0, max:-2}, // undershirts
        "9":{min:-1, max:-2}, // armors
        "10":{min:-1, max:227}, // decals
        "11":{min:0, max:587}, // tops
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
        "4":{min:0, max:217}, // pants
        "5":{min:0, max:-2}, // bags/hands
        "6":{min:1, max:161}, // shoes
        "7":{min:-1, max:-2}, // accessories
        "8":{min:0, max:-2}, // undershirts
        "9":{min:-1, max:-2}, // armors
        "10":{min:-1, max:227}, // decals
        "11":{min:0, max:587}, // tops       
    },
    "fugitive_donator":{
        "-5":{min:-1, max:-2}, // bracelets
        "-4":{min:-1, max:-2}, // watches
        "-3":{min:-1, max:-2}, // ears
        "-2":{min:-1, max:-2}, // glasses
        "-1":{min:-1, max:-2}, // hats
        "0":{min:0, max:0}, // head
        "1":{min:-1, max:-2}, // masks - set to anything for donators
        "2":{min:0, max:0}, // hairs - set in character appearance editor bruv
        "3":{min:0, max:-2}, // torsos
        "4":{min:0, max:217}, // pants
        "5":{min:0, max:-2}, // bags/hands
        "6":{min:1, max:161}, // shoes
        "7":{min:-1, max:-2}, // accessories
        "8":{min:0, max:-2}, // undershirts
        "9":{min:-1, max:-2}, // armors
        "10":{min:-1, max:227}, // decals
        "11":{min:0, max:587}, // tops       
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
        "-1":{min:-1, max:-2}, // hats
        "0":{min:0, max:0}, // head
        "1":{min:-1, max:-1}, // masks - set to anything for donators
        "2":{min:0, max:0}, // hairs - set in character appearance editor bruv
        "3":{min:0, max:-2}, // torsos
        "4":{min:0, max:202}, // pants
        "5":{min:-1, max:-2}, // bags/hands
        "6":{min:25, max:25}, // shoes
        "7":{min:-1, max:-2}, // accessories
        "8":{min:0, max:-2}, // undershirts
        "9":{min:-1, max:-2}, // armors
        "10":{min:-1, max:211}, // decals
        "11":{min:0, max:543}, // tops
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
        "4":{min:0, max:202}, // pants
        "5":{min:0, max:-2}, // bags/hands
        "6":{min:1, max:154}, // shoes
        "7":{min:-1, max:-2}, // accessories
        "8":{min:0, max:-2}, // undershirts
        "9":{min:-1, max:-2}, // armors
        "10":{min:-1, max:211}, // decals
        "11":{min:0, max:543}, // tops        
    },
    "fugitive_donator":{
        "-5":{min:-1, max:-2}, // bracelets
        "-4":{min:-1, max:-2}, // watches
        "-3":{min:-1, max:-2}, // ears
        "-2":{min:-1, max:-2}, // glasses
        "-1":{min:-1, max:-2}, // hats
        "0":{min:0, max:0}, // head
        "1":{min:-1, max:-2}, // masks - set to anything for donators
        "2":{min:0, max:0}, // hairs - set in character appearance editor bruv
        "3":{min:0, max:-2}, // torsos
        "4":{min:0, max:202}, // pants
        "5":{min:0, max:-2}, // bags/hands
        "6":{min:1, max:154}, // shoes
        "7":{min:-1, max:-2}, // accessories
        "8":{min:0, max:-2}, // undershirts
        "9":{min:-1, max:-2}, // armors
        "10":{min:-1, max:211}, // decals
        "11":{min:0, max:543}, // tops      
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

mp.events.add("CharacterEditor:SetClosable", (toggle) => {
    if(global.ServerUI) global.ServerUI.call("character_editor_closable_toggle", toggle);
});

mp.events.add("CharacterEditor:ToggleAppearanceEditor", (toggle, gender, appearanceData) => {
    if(toggle == false){
        if(global.ServerUI) global.ServerUI.call('character_editor_toggle', toggle);
        return;
    }

    if(appearanceData.length !== 0) CURRENT_APPEARANCE_DATA = JSON.parse(appearanceData);
    else CURRENT_APPEARANCE_DATA = undefined;

    APPEARANCE_IsMale = gender;
    if(global.ServerUI) global.ServerUI.call('character_editor_toggle', toggle);

    setAppearanceEditorCategories();
    mp.gui.cursor.show(toggle, toggle);
    updateCategoryOptionValues("none", []);
});

function updateCategoryOptionValues(category, options){
    if(!CURRENT_APPEARANCE_DATA) return;
    if(category === "none"){
        if(global.ServerUI) global.ServerUI.call('character_editor_set_options', []);
        return;
    }

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
        DATA.push(Math.round(HeadBlendData.ShapeMix*100));
        DATA.push(Math.round(HeadBlendData.SkinMix*100));
        DATA.push(Math.round(HeadBlendData.ThirdMix*100));
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
            OptionsOBJ[i].value = Math.round((FaceFeatures[i].Scale*100));
        }

        if(global.ServerUI) global.ServerUI.call('character_editor_set_options', OptionsOBJ);
        return;
    }
    if(category === "c_tattoos"){
        const TattooList = CURRENT_APPEARANCE_DATA.Tattoos;
        
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
                OptionsOBJ.find(h => h.id == `opacity_head_overlay_${i}`).value = Math.round(Opacity*100);
   
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
        /*{
            id:"c_tattoos",
            name:"Tattoos"
        },*/
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
    if(category === "c_tattoos"){
        const tattooList = getTattoosForGender(APPEARANCE_IsMale);
        const tattoosToSet = tattooList.map((t) => {
            return {id:`tattooID_${t.Name}`, name:t.LocalizedName};
        });
        var optionToSet = options.find(o => o.id === "tattoos");
        if(optionToSet !== undefined && optionToSet.values) optionToSet.values = tattoosToSet;
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
                value: 50,
                min: 0,
                max: 100,
                step: 1,
            },
            {
                type: "slider",
                id: "skin_mix",
                name: "Skin Mix",
                value: 50,
                min: 0,
                max: 100,
                step: 1,
            },
            {
                type: "slider",
                id: "third_mix",
                name: "Third Mix",
                value: 50,
                min: 0,
                max: 100,
                step: 1,
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
                "value": 50,
                "min": -100,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "face_feature_1",
                "name": "Nose height",
                "value": 50,
                "min": -100,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "face_feature_2",
                "name": "Nose length",
                "value": 50,
                "min": -100,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "face_feature_3",
                "name": "Nose bridge",
                "value": 50,
                "min": -100,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "face_feature_4",
                "name": "Nose tip",
                "value": 50,
                "min": -100,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "face_feature_5",
                "name": "Nose bridge shift",
                "value": 50,
                "min": -100,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "face_feature_6",
                "name": "Brow height",
                "value": 50,
                "min": -100,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "face_feature_7",
                "name": "Brow width",
                "value": 50,
                "min": -100,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "face_feature_8",
                "name": "Cheekbone height",
                "value": 50,
                "min": -100,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "face_feature_9",
                "name": "Cheekbone width",
                "value": 50,
                "min": -100,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "face_feature_10",
                "name": "Cheeks width",
                "value": 50,
                "min": -100,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "face_feature_11",
                "name": "Eye size",
                "value": 50,
                "min": -100,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "face_feature_12",
                "name": "Lip size",
                "value": 50,
                "min": -100,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "face_feature_13",
                "name": "Jaw width",
                "value": 50,
                "min": -100,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "face_feature_14",
                "name": "Jaw height",
                "value": 50,
                "min": -100,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "face_feature_15",
                "name": "Chin length",
                "value": 50,
                "min": -100,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "face_feature_16",
                "name": "Chin position",
                "value": 50,
                "min": -100,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "face_feature_17",
                "name": "Chin width",
                "value": 50,
                "min": -100,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "face_feature_18",
                "name": "Chin shape",
                "value": 50,
                "min": -100,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "face_feature_19",
                "name": "Neck width",
                "value": 50,
                "min": -100,
                "max": 100,
                "step": 1
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
                "min": -1,
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
                "value": 100,
                "min": 0,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "head_overlay_1",
                "name": "Facial Hair",
                "value": 0,
                "min": -1,
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
                "value": 100,
                "min": 0,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "head_overlay_2",
                "name": "Eyebrows",
                "value": 0,
                "min": -1,
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
                "value": 100,
                "min": 0,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "head_overlay_3",
                "name": "Ageing",
                "value": 0,
                "min": -1,
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
                "value": 100,
                "min": 0,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "head_overlay_4",
                "name": "Makeup",
                "value": 0,
                "min": -1,
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
                "value": 100,
                "min": 0,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "head_overlay_5",
                "name": "Blush",
                "value": 0,
                "min": -1,
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
                "value": 100,
                "min": 0,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "head_overlay_6",
                "name": "Complexion",
                "value": 0,
                "min": -1,
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
                "value": 100,
                "min": 0,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "head_overlay_7",
                "name": "Sun Damage",
                "value": 0,
                "min": -1,
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
                "value": 100,
                "min": 0,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "head_overlay_8",
                "name": "Lipstick",
                "value": 0,
                "min": -1,
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
                "value": 100,
                "min": 0,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "head_overlay_9",
                "name": "Moles/Freckles",
                "value": 0,
                "min": -1,
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
                "value": 100,
                "min": 0,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "head_overlay_10",
                "name": "Chest Hair",
                "value": 0,
                "min": -1,
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
                "value": 100,
                "min": 0,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "head_overlay_11",
                "name": "Body Blemishes",
                "value": 0,
                "min": -1,
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
                "value": 100,
                "min": 0,
                "max": 100,
                "step": 1
            },
            {
                "type": "slider",
                "id": "head_overlay_12",
                "name": "Add Body Blemishes",
                "value": 1,
                "min": -1,
                "max": 1,
                "step": 1
            },
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
                "min": -1,
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
    },
    {
        category:"c_tattoos",
        options:[
            {
                type:"checkbox",
                id:"tattoos",
                name:"Tattoos",
                value:[],
                values:[
                    // Filled out on runtime depending on gender
                ]
            }   
        ]
    },
];


function getTattoosForGender(isMale){
    if(!global.tattoosList) return;

    var tattoos = [];
    if(isMale) tattoos = global.tattoosList.map((t) => {if(t.HashNameMale) return t;});
    else tattoos = global.tattoosList.map((t) => {if(t.HashNameFemale) return t;});
    return tattoos;
}
function getHairOverlaysForGender(isMale){
    if(!global.hairDecalsList){
        return;
    }

    var overlays = [];
    if(isMale) overlays = global.hairDecalsList[0];
    else overlays = global.hairDecalsList[1];

    return overlays;
}

mp.events.add("character_editor_set_active_category_id", (id) => {

});
mp.events.add("character_editor_change_category", (id) => {
    setAppearanceEditorOptions(id);
});
mp.events.add("character_editor_save", () => {
    mp.events.callRemote("CharacterEditor:SaveAppearance", JSON.stringify(CURRENT_APPEARANCE_DATA))
    mp.events.callRemote('CharacterEditor:CloseAppearanceEditor');
});
mp.events.add("character_editor_close", () => {
    mp.events.callRemote('CharacterEditor:CloseAppearanceEditor');
});
mp.events.add("character_editor_change_option", (id, value) => {

    if(id === "gender_option"){
        APPEARANCE_IsMale = !value; // True is female, false is male, therefore invert
        let genderHash = "mp_m_freemode_01"
        if (!APPEARANCE_IsMale)
            genderHash = "mp_f_freemode_01"
        let genderModel = mp.game.joaat(genderHash)
        mp.players.local.model = genderModel;

        CURRENT_APPEARANCE_DATA.IsMale = !value;

        return;
    }
    if(id.includes("face_feature")){
        let faceFeatureID = getLastNumber(id);

        let realValue = (value/100)

        CURRENT_APPEARANCE_DATA.FaceFeatures[faceFeatureID].Scale = realValue;
        mp.players.local.setFaceFeature(faceFeatureID, realValue);
        return;
    }
    if(id.includes("head_overlay")){
        let overlayID = getLastNumber(id);
        let realValue = value;

        if(id.startsWith("opacity")){
            realValue = (value/100);
            CURRENT_APPEARANCE_DATA.HeadOverlays[overlayID].Opacity = realValue;
        }
        else if(id.startsWith("color1")) CURRENT_APPEARANCE_DATA.HeadOverlays[overlayID].FirstColor = value;
        else if(id.startsWith("color2")) CURRENT_APPEARANCE_DATA.HeadOverlays[overlayID].SecondColor = value;
        else CURRENT_APPEARANCE_DATA.HeadOverlays[overlayID].Index = value;

        const HeadOverlay = CURRENT_APPEARANCE_DATA.HeadOverlays[overlayID];
        mp.players.local.setHeadOverlay(overlayID, HeadOverlay.Index, HeadOverlay.Opacity, HeadOverlay.FirstColor, HeadOverlay.SecondColor);

        return;
    }
    if(id == "hair_id"){
        CURRENT_APPEARANCE_DATA.Hair.Index = value;

        mp.players.local.setComponentVariation(2, value, 0, 0);
        mp.players.local.setHairColor(CURRENT_APPEARANCE_DATA.Hair.Color, CURRENT_APPEARANCE_DATA.Hair.HighlightColor);

        return;
    }
    if(id == "hair_color1"){
        CURRENT_APPEARANCE_DATA.Hair.Color = value;

        mp.players.local.setHairColor(CURRENT_APPEARANCE_DATA.Hair.Color, CURRENT_APPEARANCE_DATA.Hair.HighlightColor);

        return;
    }
    if(id == "hair_color2"){
        CURRENT_APPEARANCE_DATA.Hair.HighlightColor = value;

        mp.players.local.setHairColor(CURRENT_APPEARANCE_DATA.Hair.Color, CURRENT_APPEARANCE_DATA.Hair.HighlightColor);

        return;
    }
    if(id.startsWith("hair_overlay")){
        const hairDecals = getHairOverlaysForGender(CURRENT_APPEARANCE_DATA.IsMale);
        CURRENT_APPEARANCE_DATA.HairOverlays = JSON.parse(value);

        mp.players.local.clearFacialDecorations();
        for(let i = 0 ; i < CURRENT_APPEARANCE_DATA.HairOverlays.length ; i++){
            const Overlay = CURRENT_APPEARANCE_DATA.HairOverlays[i];
            const overlayIndex = getLastNumber(Overlay);
            if(!overlayIndex) continue;
    
            const HairOverlay = hairDecals[overlayIndex];
    
            const collection = mp.game.gameplay.getHashKey(HairOverlay.collection);
            if(!collection) continue;
            const overlay = mp.game.gameplay.getHashKey(HairOverlay.overlay);
            if(!overlay) continue;
            mp.players.local.setFacialDecoration(collection, overlay);
        }
    }
    // TATTOOS TBA.

    if(id == "shape_parent_1"){
        CURRENT_APPEARANCE_DATA.HeadBlend.FirstParentID = value;
        updateHeritageData();
        return;
    }
    if(id == "shape_parent_2"){
        CURRENT_APPEARANCE_DATA.HeadBlend.SecondParentID = value;
        updateHeritageData();
        return;
    }
    if(id == "shape_parent_3"){
        CURRENT_APPEARANCE_DATA.HeadBlend.MixParentID = value;
        updateHeritageData();
        return;
    }
    if(id == "skin_parent_1"){
        CURRENT_APPEARANCE_DATA.HeadBlend.FirstSkinID = value;
        updateHeritageData();
        return;
    }
    if(id == "skin_parent_2"){
        CURRENT_APPEARANCE_DATA.HeadBlend.SecondSkinID = value;
        updateHeritageData();
        return;
    }
    if(id == "skin_parent_3"){
        CURRENT_APPEARANCE_DATA.HeadBlend.MixSkinID = value;
        updateHeritageData();
        return;
    }
    if(id == "shape_mix"){
        let realValue = (value/100);
        CURRENT_APPEARANCE_DATA.HeadBlend.ShapeMix = realValue;
        updateHeritageData();
        return;
    }
    if(id == "skin_mix"){
        let realValue = (value/100);
        CURRENT_APPEARANCE_DATA.HeadBlend.SkinMix = realValue;
        updateHeritageData();
        return;
    }
    if(id == "third_mix"){
        let realValue = (value/100);
        CURRENT_APPEARANCE_DATA.HeadBlend.ThirdMix = realValue;
        updateHeritageData();
        return;
    }
    if(id == "eye_color"){
        CURRENT_APPEARANCE_DATA.EyeColor = value;
        mp.players.local.setEyeColor(value);
        return;
    }
});

function updateHeritageData(){
    const entity = mp.players.local;
    const HeadBlend = CURRENT_APPEARANCE_DATA.HeadBlend;
    entity.setHeadBlendData(HeadBlend.FirstParentID, HeadBlend.SecondParentID, HeadBlend.MixParentID, 
        HeadBlend.FirstSkinID, HeadBlend.SecondSkinID, HeadBlend.MixSkinID,
        HeadBlend.ShapeMix, HeadBlend.SkinMix, HeadBlend.ThirdMix, HeadBlend.IsParent);
}

function getLastNumber(str) {
    if(!str) return;
    const matches = str.match(/\d+$/);
    if (matches) {
        return parseInt(matches[0], 10);
    }
    return null; // Return null or handle the case when no number is found.
}