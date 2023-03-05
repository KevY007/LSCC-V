const TATTOO_DATA = require('./data.js');

const Menu = NativeUI.Menu;
const UIMenuItem = NativeUI.UIMenuItem;
const UIMenuListItem = NativeUI.UIMenuListItem;
const UIMenuCheckboxItem = NativeUI.UIMenuCheckboxItem;
const UIMenuSliderItem = NativeUI.UIMenuSliderItem;
const BadgeStyle = NativeUI.BadgeStyle;
const Point = NativeUI.Point;
const ItemsCollection = NativeUI.ItemsCollection;
const Color = NativeUI.Color;

const player = mp.players.local;

const TattooMenu = new Menu("Tattoos", "", new Point(50, 50));

var CategoryPickerItem = null;
var CurrentOverlayIdx = 0;

var TattooItems = [];

mp.events.add("ShowTattooMenu", () => {
    var categoryList = [];
    for(let i = 0 ; i < TATTOO_DATA.length ; i++) categoryList.push(i.toString());

    CategoryPickerItem = new UIMenuListItem("Category", "Choose a category", new ItemsCollection(categoryList));
    TattooMenu.addItem(CategoryPickerItem);

    for(let i = 0 ; i < TATTOO_DATA[0].data.length ; i++){
        TattooItems[i] = new UIMenuCheckboxItem(TATTOO_DATA[0].data[i].LocalizedName, false, "Toggle this tattoo on or off.");
        TattooMenu.addItem(attooItems[i]);
    }
    CurrentOverlayIdx = 0;

    CategoryPickerItem.ListChange.on((item, listIndex) => {

        for(let i = 0 ; i < TattooItems.length ; i++){
            TattooMenu.RemoveItem(TattooItems[i]);
        }
        TattooItems = [];

        let category = parseInt(CategoryPickerItem.SelectedItem.DisplayText);
        for(let i = 0 ; i < TATTOO_DATA[category].data.length ; i++){
            TattooItems[i] = new UIMenuCheckboxItem(TATTOO_DATA[0].data[i].LocalizedName, false, "Toggle this tattoo on or off.");
            TattooMenu.addItem(attooItems[i]);
        }
        CurrentOverlayIdx = category;
    });
});

TattooMenu.CheckboxChange.on((item, checked) => {
    let tattooName = item.DisplayText;

    const tattooData = TATTOO_DATA[CurrentOverlayIdx].data.find(t => t.LocalizedName === tattooName);

    if(checked)
        player.setDecoration(mp.game.joaat(TATTOO_DATA[CurrentOverlayIdx].overlay), mp.game.joaat(tattooData.HashNameMale));
    else
        player.clearDecorations();
});