const NativeUI = require("nativeui");
const Menu = NativeUI.Menu;
const UIMenuItem = NativeUI.UIMenuItem;
const UIMenuListItem = NativeUI.UIMenuListItem;
const UIMenuCheckboxItem = NativeUI.UIMenuCheckboxItem;
const UIMenuSliderItem = NativeUI.UIMenuSliderItem;
const BadgeStyle = NativeUI.BadgeStyle;
const Point = NativeUI.Point;
const ItemsCollection = NativeUI.ItemsCollection;
const Color = NativeUI.Color;
const ListItem = NativeUI.ListItem;

const modTypes = {0: "Spoilers", 
    1: "Front Bumper", 
    2: "Rear Bumper", 
    3: "Side Skirt", 
    4: "Exhaust", 
    5: "Frame", 
    6: "Grille", 
    7: "Hood", 
    8: "Fender", 
    9: "Right Fender", 
    10: "Roof", 
    11: "Engine", 
    12: "Brakes", 
    13: "Transmission", 
    14: "Horns", // 0 to 57
    15: "Suspension", 
    16: "Armor", 
    18: "Turbo", 
    22: "Xenon", 
    23: "Wheels", 
    24: "Back Wheels", // Only for motorcycles
    25: "Plate holders", 
    27: "Trim Design", 
    28: "Ornaments", 
    30: "Dial Design", 
    33: "Steering Wheel", 
    34: "Shift Lever", 
    35: "Plaques", 
    38: "Hydraulics", 
    40: "Boost", 
    55: "Window Tint", 
    48: "Livery", 
    53: "Plate", 
    66: "Primary Colour", // 0-159
    67: "Secondary Colour",  // 0-159
    // UNOFFICIAL (CUSTOM ADDED, SCRIPT MAPPED):
    68: "Pearlescent Colour", // 0-159
    69: "Wheel Colour", // 0-159
};


var vTuner_Items = [];
var vTuner_Main = new Menu('Tune Vehicle', 'Start modding', new Point(50, 50));
var vTuner_Buy = null;
var vTuner_Exit = null;

mp.events.add('OpenTuner', () => {
    if(!mp.players.local.isInAnyVehicle(false)) return;

    for(let i = 0; i < vTuner_Items.length; i++)
    {
        vTuner_Main.RemoveItem(vTuner_Items[i]);
        delete vTuner_Items[i].Trigger;
        delete vTuner_Items[i].ItemNames;
        delete vTuner_Items[i].SlotID;
        vTuner_Items[i] = null;
    }
    delete vTuner_Items;
    vTuner_Items = [];

    let vehicle = mp.players.local.vehicle;
    for(let slots in modTypes)
    {
        let num = vehicle.getNumMods(Number(slots));
        if(Number(slots) >= 66) num = 160;

        if(num > 0)
        {
            let catItems = [];
            for(let i = -1; i < num; i++)
            {
                let name = "None";
                if(i != -1)
                {
                    if(vehicle.getModTextLabel(Number(slots), i) != undefined)
                    {
                        name = mp.game.ui.getLabelText(vehicle.getModTextLabel(Number(slots), i));
                    }
                    else name = i.toString();
                }
                else if(Number(slots) >= 66) continue; // Don't want -1 aka "None" in any colours since it does nothing.

                catItems.push(name);
            }

            let cursor = vehicle.getMod(Number(slots))+1;

            if (Number(slots) == 66) cursor = vehicle.getColours(1, 1).colorPrimary;
            else if (Number(slots) == 67) cursor = vehicle.getColours(1, 1).colorSecondary;
            else if (Number(slots) == 68) cursor = vehicle.getExtraColours(1, 1).pearlescentColor;
            else if (Number(slots) == 69) cursor = vehicle.getExtraColours(1, 1).wheelColor;
            
            let vTuner_Category = new UIMenuListItem(modTypes[slots], "", new ItemsCollection(catItems), cursor);
            
            vTuner_Category.ItemNames = JSON.parse(JSON.stringify(catItems));

            vTuner_Category.SlotID = JSON.parse(JSON.stringify(Number(slots)));
            
            vTuner_Category.Trigger = (function () {
                let selection = -1;
                for(let i = 0; i < vTuner_Category.ItemNames.length; i++)
                {
                    if(vTuner_Category.ItemNames[i] == vTuner_Category.SelectedValue)
                    {
                        selection = i-1;
                        break;
                    }
                }
                if(vTuner_Category.SlotID >= 66) selection += 1;
                mp.events.callRemote('RequestVehicleMod', vTuner_Category.SlotID, selection);
            });

            vTuner_Main.AddItem(vTuner_Category);
            vTuner_Items.push(vTuner_Category);
        }
    }

    vTuner_Buy = new UIMenuItem("Buy", "Buy for ingame team.");
    vTuner_Buy.ForeColor = new Color(220, 10, 10);
    vTuner_Main.AddItem(vTuner_Buy);
    vTuner_Items.push(vTuner_Buy);

    vTuner_Exit = new UIMenuItem("Exit", "Leave the editor.");
    vTuner_Exit.ForeColor = new Color(127, 237, 28);
    vTuner_Main.AddItem(vTuner_Exit);
    vTuner_Items.push(vTuner_Exit);

    mp.gui.chat.show(false);

    vTuner_Main.Open();
});

vTuner_Main.ListChange.on((item, listIndex) => {
    item.Trigger(); // Cuz we can, OOP magick
});

vTuner_Main.ItemSelect.on((item, index) => {
    switch (item.Text) {

        case "Exit":
            vTuner_Main.Close();
        break;

        case "Buy":
            let modItems = {};
            let vehicle = mp.players.local.vehicle;
            for(let slots in modTypes)
            {
                let mod = vehicle.getMod(Number(slots));
                if(mod != -1) modItems[slots] = mod;
            }
            mp.events.callRemote("RequestSaveVehicleMod", JSON.stringify(modItems), vehicle.getColours(1, 1).colorPrimary, 
                vehicle.getColours(1, 1).colorSecondary, vehicle.getExtraColours(1, 1).pearlescentColor, 
                vehicle.getExtraColours(1, 1).wheelColor);

        break;
    }
});

mp.events.add('playerLeaveVehicle', (vehicle, seat) => {
    if(vTuner_Main.Visible) vTuner_Main.Close();
});

vTuner_Main.MenuOpen.on(() => {
    
});

vTuner_Main.MenuClose.on(() => {
    for(let i = 0; i < vTuner_Items.length; i++)
    {
        vTuner_Main.RemoveItem(vTuner_Items[i]);
        delete vTuner_Items[i].Trigger;
        delete vTuner_Items[i].ItemNames;
        delete vTuner_Items[i].SlotID;
        vTuner_Items[i] = null;
    }
    delete vTuner_Items;
    vTuner_Items = [];

    mp.gui.chat.show(true);
});