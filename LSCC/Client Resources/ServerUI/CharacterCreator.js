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

// Server -> Clientside
mp.events.add("OutfitCreator:Set", (outfitData) => { // Loading outfits
    const outfitArray = JSON.parse(outfitData);
    const outfits = outfitArray.map((outfit) => { return {id:outfit.SQL_ID, team:outfit.Team }; });

    if(global.ServerUI) global.ServerUI.call("mainmenu_inventory_outfits_set", outfits);
});
mp.events.add("OutfitCreator:SetActive", (id, team) => { // Loading defaults
    if(global.ServerUI) global.ServerUI.call("mainmenu_inventory_outfits_set_active", id, team);
});
