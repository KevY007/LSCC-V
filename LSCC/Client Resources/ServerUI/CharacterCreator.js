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

function onOutfitCreatorChangeType(componentID, inputID, inputTexture, isProp){

    const max_drawable = (isProp ? 
        mp.players.local.getNumberOfPropDrawableVariations(componentID) : 
        mp.players.local.getNumberOfDrawableVariations(componentID));
    const max_texture = (isProp ? 
        mp.players.local.getNumberOfPropTextureVariations(componentID, inputID) :
        mp.players.local.getNumberOfTextureVariations(componentID, inputID));

    if(global.ServerUI) global.ServerUI.call("dialog_clothing_editor_open", inputID, inputTexture, {
        id:{
            min:0,
            max:max_drawable
        },
        texture:{
            min:0,
            max:max_texture
        }
    });
}
mp.events.add("OutfitCreator:ChangeType", onOutfitCreatorChangeType);
mp.events.add("OutfitCreator:OpenEditor", (inputID, inputTexture) => {
    mp.events.callLocal("ForceCloseMenu");

    // the editor opens with bracelets selected by default?
    const max_bracelets = mp.players.local.getNumberOfPropDrawableVariations(7);
    const max_bracelet_texture = mp.players.local.getNumberOfPropTextureVariations(7, inputID);

    mp.gui.cursor.show(true, true);

    CURRENT_EDIT_ITEM = {isProp:true, index:7, drawable:inputID, texture:inputTexture};

    if(!mp.players.local.hasVariable("OUTFIT:Editing")) return;
    CURRENT_OUTFIT_DATA = JSON.parse(mp.players.local.getVariable("OUTFIT:Editing"));
    if(!CURRENT_OUTFIT_DATA) return;


    if(global.ServerUI) global.ServerUI.call("dialog_clothing_editor_open", inputID, inputTexture, {
        id:{
            min:0,
            max:max_bracelets
        },
        texture:{
            min:0,
            max:max_bracelet_texture
        }
    });
});
