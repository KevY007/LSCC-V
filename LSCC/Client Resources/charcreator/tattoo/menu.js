const TATTOO_DATA = require('./charcreator/tattoo/data.js');

const NativeUI = require('./nativeui/index.js');
var global = require('./globals');
  const {TranslateMessage: Translate} = require('./Translator');
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

var Gender = "M";
var TattooList = [];

mp.events.add("ShowTattooMenu", (gender) => {
    global.activateChat(false);
    Gender = gender;
    TattooList = JSON.parse(player.getVariable("TattooList"));

    TattooMenu.Visible = true;
    var categoryList = [];
    for(let i = 0 ; i < TATTOO_DATA.length ; i++) categoryList.push(i.toString());

    CategoryPickerItem = new UIMenuListItem("Category", "Choose a category", new ItemsCollection(categoryList));
    TattooMenu.AddItem(CategoryPickerItem);

    CategoryPickerItem.Visible = true;

    for(let i = 0 ; i < TATTOO_DATA[0].data.length ; i++){

        let checked = TattooList.includes(TATTOO_DATA[0].data[i].LocalizedName);

        TattooItems[i] = new UIMenuCheckboxItem(TATTOO_DATA[0].data[i].LocalizedName, checked, `Toggle this tattoo on or off. (${i+1}/${TATTOO_DATA[0].data.length})`);
        TattooMenu.AddItem(TattooItems[i]);

        TattooItems[i].Visible = true;
    }
    CurrentOverlayIdx = 0;

    TattooMenu.ListChange.on((item, listIndex) => {

        for(let i = 0 ; i < TattooItems.length ; i++){
            TattooItems[i].Visible = false;
            TattooMenu.RemoveItem(TattooItems[i]);
        }
        TattooItems = [];
    
        let category = parseInt(CategoryPickerItem.SelectedItem.DisplayText);
        for(let i = 0 ; i < TATTOO_DATA[category].data.length ; i++){
            let checked = TattooList.includes(TATTOO_DATA[category].data[i].LocalizedName);

            TattooItems[i] = new UIMenuCheckboxItem(TATTOO_DATA[category].data[i].LocalizedName, checked, `Toggle this tattoo on or off. (${i+1}/${TATTOO_DATA[category].data.length})`);
            TattooMenu.AddItem(TattooItems[i]);
            TattooItems[i].Visible = true;
        }
        CurrentOverlayIdx = category;
    });
});

TattooMenu.CheckboxChange.on((item, checked) => {
    const tattooName = item.Text;
    mp.gui.chat.push(`${tattooName} added on you: ${checked.toString()}`);

    if(checked){
        applyTattoo(tattooName);
        TattooList.push(tattooName);

        mp.events.callRemote("UpdateTattooList", JSON.stringify(TattooList));
    }
    else{
        console.log(`Searching for ${tattooName}`);
        let remIdx = TattooList.indexOf(TattooList.find(t => t === tattooName));
        if(remIdx === -1){
            mp.gui.chat.push(`Tattoo not found in your list. Report this on discord: ${tattooName}`);
            return;
        }
        TattooList.splice(remIdx, 1);
        mp.events.callRemote("UpdateTattooList", JSON.stringify(TattooList));

        player.clearDecorations();
        setTimeout(() => {
            for(let i = 0 ; i < TattooList.length ; i++){
                applyTattoo(TattooList[i]);
            }
        }, 50);
    }
});

function applyTattooEx(tattooName){
    let success = false;
    for(let i = 0 ; i < TATTOO_DATA.length ; i++){
        const tattooData = TATTOO_DATA[i].data.find(t => t.LocalizedName === tattooName);
        if(!tattooData){
            continue;
        }
        success = true;
        player.setDecoration(mp.game.joaat(TATTOO_DATA[i].overlay), mp.game.joaat(Gender === 'M' ? tattooData.HashNameMale : tattooData.HashNameFemale));
        return;
    }
    if(!success){
        mp.gui.chat.push(`Cannot find and apply this tattoo! ${tattooName} | Report on our Discord.`);
    }
}

function applyTattoo(tattooName){
    const tattooData = TATTOO_DATA[Number(CurrentOverlayIdx)].data.find(t => t.LocalizedName === tattooName);
    if(!tattooData){
        mp.gui.chat.push(`Cannot find and apply this tattoo! ${tattooName} | Report on our Discord.`);
        return;
    }
    player.setDecoration(mp.game.joaat(TATTOO_DATA[CurrentOverlayIdx].overlay), mp.game.joaat(Gender === 'M' ? tattooData.HashNameMale : tattooData.HashNameFemale));
}
function applyTattoosOnEntity(entity, tattooList){
    return;

    const list = JSON.parse(tattooList);
    for(let j = 0 ; j < list.length ; j++){
        let tattooName = list[j];

        let success = false;
        for(let i = 0 ; i < TATTOO_DATA.length ; i++){
            const tattooData = TATTOO_DATA[i].data.find(t => t.LocalizedName === tattooName);
            if(!tattooData){
                continue;
            }
            success = true;
            entity.setDecoration(mp.game.joaat(TATTOO_DATA[i].overlay), mp.game.joaat(tattooData.HashNameMale));
            entity.setDecoration(mp.game.joaat(TATTOO_DATA[i].overlay), mp.game.joaat(tattooData.HashNameFemale));
        }
        if(!success){
            mp.gui.chat.push(`Cannot find and apply this tattoo! ${tattooName} | Report on our Discord.`);
        }
    }

}

TattooMenu.MenuClose.on(() => {
    for(let i = 0 ; i < TattooItems.length ; i++){
        TattooItems[i].Visible = false;
        TattooMenu.RemoveItem(TattooItems[i]);
    }
    TattooItems = [];
    CategoryPickerItem.Visible = false;
    TattooMenu.RemoveItem(CategoryPickerItem);
    global.activateChat(true);

    mp.events.callRemote("SaveTattoos", JSON.stringify(TattooList));
});

mp.events.add("ApplyTattoos", (tattooList) => {
    player.clearDecorations();
    if(!tattooList) return;
    const list = JSON.parse(tattooList);
    if(!list) return;
    for(let i = 0 ; i < list.length ; i++){
        applyTattooEx(list[i]);
    }
});

mp.events.add('entityStreamIn', (entity) => {
    if(entity != null && entity.type === 'player') 
    {
        entity.clearDecorations();
        if(entity.getVariable("TattooList")){
            const list = JSON.parse(entity.getVariable("TattooList"));
            for(let i = 0 ; i < list.length ; i++){
                applyTattoosOnEntity(entity, list[i]);
            }
        }
    }
});
mp.events.add('entityStreamOut', (entity) => {
    if(entity !== null && entity.type === 'player'){
        entity.clearDecorations();
    }
});

mp.events.add("TwoSecondsEvent", () => {
    mp.events.callLocal("ApplyTattoos", mp.players.local.getVariable("TattooList"));
});
