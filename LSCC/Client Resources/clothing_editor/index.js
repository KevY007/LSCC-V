mp.events.add("ClothingEditor:exit", () => {
    ServerUI.execute(`gm.$refs.hud.enabled = ${mp.storage.data.menu.HUD.toString()};`);
    mp.gui.cursor.show(false, false);
    ClothingEditorDOM.active = false;
});
mp.events.add("ClothingEditor:show", (team, gender) => {
    ServerUI.execute(`gm.$refs.hud.enabled = false;`);
    mp.gui.cursor.show(true, true);

    ClothingEditorDOM.execute(`ShowClothingEditor(${team}, ${gender});`);
    ClothingEditorDOM.active = true;
});
mp.events.add("ClothingEditor:updateClothing", () => {

});
function InitializeClothingEditorDOM(){
    ClothingEditorDOM = mp.browsers.new("package://clothing_editor/index.html");
    ClothingEditorDOM.active = false;
}