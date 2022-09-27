let editorTeam = -1;

mp.events.add("ClothingEditor:exit", (save) => {
    ServerUI.execute(`gm.$refs.hud.enabled = ${mp.storage.data.menu.HUD.toString()};`);
    mp.gui.cursor.show(false, false);
    ClothingEditorDOM.active = false;
    ClothingEditorActive = false;

    mp.events.callRemote("ClothingEditor:onExit", save, editorTeam);
});
mp.events.add("ClothingEditor:show", (team, gender) => {
    editorTeam = team;
    ServerUI.execute(`gm.$refs.hud.enabled = false;`);
    mp.gui.cursor.show(true, true);

    ClothingEditorDOM.execute(`ShowClothingEditor(${team}, '${gender}');`);
    ClothingEditorDOM.active = true;
    ClothingEditorActive = true;
});
mp.events.add("ClothingEditor:updateClothing", (clothingName, drawable, texture) => {
    let isProp = false, componentIndex = 0;
    switch(clothingName){
        case "Shirt": componentIndex = 11; break;
        case "Undershirt": componentIndex = 8; break;
        case "Pants": componentIndex = 4; break;
        case "Shoes": componentIndex = 6; break;
        case "Accessory 1": componentIndex = 5; break;
        case "Accessory 2": componentIndex = 7; break;
        case "Armor": componentIndex = 9; break;
        case "Handwear": componentIndex = 3; break;
        case "Hats": componentIndex = 0; isProp = true; break;
        case "Glasses": componentIndex = 1; isProp = true; break;
    }
    if(!isProp){
        mp.events.callRemote('RequestSetClothes', componentIndex, drawable, texture);
        mp.players.local.setComponentVariation(componentIndex, drawable, texture, 0);
    }
    else{
        if(componentIndex == 1){
    		mp.events.callRemote('RequestSetGlasses', drawable, texture);
        }
        else{
    		mp.events.callRemote('RequestSetHat', drawable, texture);
        }
    }
});
function InitializeClothingEditorDOM(){
    ClothingEditorDOM = mp.browsers.new("package://clothing_editor/index.html");
    ClothingEditorDOM.active = false;
    ClothingEditorActive = false;
}