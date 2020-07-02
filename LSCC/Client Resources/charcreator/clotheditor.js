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

let player = mp.players.local;

let todosMenus = [];

let menuRoupas = new Menu("Clothes", "", new Point(50, 50));
menuRoupasExit = null;
menuRoupas.AddItem(menuRoupasItem2 = new UIMenuItem("Hats", "Select your hat."));
menuRoupas.AddItem(menuRoupasItem3 = new UIMenuItem("Shirts", "Select your shirt."));
menuRoupas.AddItem(menuRoupasItem7 = new UIMenuItem("Torso", "Change arms and torso."));
menuRoupas.AddItem(menuRoupasItem4 = new UIMenuItem("Undershirts", "Select your undershirt."));
menuRoupas.AddItem(menuRoupasItem5 = new UIMenuItem("Pants", "Select your pant."));
menuRoupas.AddItem(menuRoupasItem6 = new UIMenuItem("Shoes", "Select your shoes."));
menuRoupas.AddItem(menuRoupasItem8 = new UIMenuItem("Glasses", "Select your glasses."));
menuRoupas.AddItem(menuRoupasItem10 = new UIMenuItem("Bags", "Select your bag."));
menuRoupas.AddItem(menuRoupasItem1 = new UIMenuItem("Masks", "Select your mask."));
menuRoupas.AddItem(menuRoupasItem9 = new UIMenuItem("Accessories", "Select your accessories."));

menuRoupas.Visible = false;
todosMenus.push(menuRoupas);

///////////////////////////////////////////////////////
// MENU MASCARAS COMPONENT 1
///////////////////////////////////////////////////////

let mascarasDrawable = [];
for (i = 0; i < player.getNumberOfDrawableVariations(1) + 1; i++) mascarasDrawable.push(i.toString());

let mascarasTextureArray = [];
let mascarasTextureLimite = player.getNumberOfTextureVariations(1, player.getDrawableVariation(1));
for (i = 0; i < mascarasTextureLimite + 1; i++) mascarasTextureArray.push(i.toString());

const menuRoupasMascaras = new Menu("Masks", "", new Point(50, 50));
menuRoupasMascarasExit = null;
let mascarasItem = new UIMenuListItem("Masks", "Select your mask.", new ItemsCollection(mascarasDrawable), player.getDrawableVariation(1));
let mascarasTextureItem = new UIMenuListItem("Color", "Select your mask's color.", new ItemsCollection(mascarasTextureArray), player.getTextureVariation(1));
menuRoupasMascaras.AddItem(mascarasItem);
menuRoupasMascaras.AddItem(mascarasTextureItem);
menuRoupasMascaras.ParentMenu = menuRoupas;
menuRoupasMascaras.Visible = false;
todosMenus.push(menuRoupasMascaras);

menuRoupasMascaras.ListChange.on((item, listIndex) => {
    let drawable = parseInt(mascarasItem.SelectedItem.DisplayText);
    let texture = parseInt(mascarasTextureItem.SelectedItem.DisplayText);
    switch (item) {
        case mascarasItem:
            mp.events.callRemote('RequestSetClothes', 1, drawable, 0);
            mascarasTextureNewArray = [];
            for (i = 0; i < player.getNumberOfTextureVariations(1, player.getDrawableVariation(1)) + 1; i++) mascarasTextureNewArray.push(i.toString());
            mascarasTextureItem.Collection = new NativeUI.ItemsCollection(mascarasTextureNewArray).getListItems();
            mascarasTextureItem.Index = 0;
        break

        case mascarasTextureItem:
            mp.events.callRemote('RequestSetClothes', 1, drawable, texture);
    }
});


menuRoupasMascaras.ItemSelect.on((item, index) => {
    switch (item.Text) {

        case "Exit":
            menuRoupasMascaras.RemoveItem(menuRoupasMascarasExit);
            menuRoupasMascarasExit = null;

            menuRoupasMascaras.Visible = false;
            menuRoupas.Visible = true;
        break;
    }
});

///////////////////////////////////////////////////////
// MENU CHAPÉUS PROP 0
///////////////////////////////////////////////////////

// Drawable
let chapeusDrawable = [];
for (i = 0; i < player.getNumberOfPropDrawableVariations(0) + 1; i++) chapeusDrawable.push(i.toString())
// Texture
let chapeusTexture = [];
let chapeusTextureLimite = player.getNumberOfPropTextureVariations(0, player.getPropIndex(0));
for (i = 0; i < chapeusTextureLimite + 1; i++) chapeusTexture.push(i.toString());

const menuRoupasChapeus = new Menu("Hats", "", new Point(50, 50));
menuRoupasChapeusExit = null;
let chapeusItem = new UIMenuListItem("Hats", "Select your hat.", new ItemsCollection(chapeusDrawable), player.getPropIndex(0));
let chapeusTextureItem = new UIMenuListItem("Color", "Select your hat's color.", new ItemsCollection(chapeusTexture), player.getPropTextureIndex(0));
menuRoupasChapeus.AddItem(chapeusItem);
menuRoupasChapeus.AddItem(chapeusTextureItem);
menuRoupasChapeus.ParentMenu = menuRoupas;
menuRoupasChapeus.Visible = false;
todosMenus.push(menuRoupasChapeus);

menuRoupasChapeus.ListChange.on((item, listIndex) => {
	let drawable = parseInt(chapeusItem.SelectedItem.DisplayText);
	let texture = parseInt(chapeusTextureItem.SelectedItem.DisplayText);
    switch (item) {
    	case chapeusItem:
    		mp.events.callRemote('RequestSetHat', drawable, 0);
    		chapeusTextureNewArray = [];
			for (i = 0; i < player.getNumberOfPropTextureVariations(0, player.getPropIndex(0)) + 1; i++) chapeusTextureNewArray.push(i.toString());
    		chapeusTextureItem.Collection = new NativeUI.ItemsCollection(chapeusTextureNewArray).getListItems();
    		chapeusTextureItem.Index = 0;
    	break

    	case chapeusTextureItem:
    		mp.events.callRemote('RequestSetHat', drawable, texture);
    }
});

menuRoupasChapeus.ItemSelect.on((item, index) => {
    switch (item.Text) {

        case "Exit":
            menuRoupasChapeus.RemoveItem(menuRoupasChapeusExit);
            menuRoupasChapeusExit = null;

            menuRoupasChapeus.Visible = false;
            menuRoupas.Visible = true;
        break;
    }
});
///////////////////////////////////////////////////////
// MENU JAQUETAS COMPONENT 11
///////////////////////////////////////////////////////

// Drawable
let jaquetasDrawable = [];
for (i = 0; i < player.getNumberOfDrawableVariations(11) + 1; i++) jaquetasDrawable.push(i.toString());
// Texture
let jaquetasTextureArray = [];
let jaquetasTextureLimite = player.getNumberOfTextureVariations(11, player.getDrawableVariation(11));
for (i = 0; i < jaquetasTextureLimite + 1; i++) jaquetasTextureArray.push(i.toString());

const menuRoupasJaquetas = new Menu("Shirts", "", new Point(50, 50));
menuRoupasJaquetasExit = null;
let jaquetasItem = new UIMenuListItem("Shirts", "Select your shirt.", new ItemsCollection(jaquetasDrawable), player.getDrawableVariation(11));
let jaquetasTextureItem = new UIMenuListItem("Color", "Select your shirt's color.", new ItemsCollection(jaquetasTextureArray), player.getTextureVariation(11));
menuRoupasJaquetas.AddItem(jaquetasItem);
menuRoupasJaquetas.AddItem(jaquetasTextureItem);
menuRoupasJaquetas.ParentMenu = menuRoupas;
menuRoupasJaquetas.Visible = false;
todosMenus.push(menuRoupasJaquetas);

menuRoupasJaquetas.ListChange.on((item, listIndex) => {
	let drawable = parseInt(jaquetasItem.SelectedItem.DisplayText);
	let texture = parseInt(jaquetasTextureItem.SelectedItem.DisplayText);
    switch (item) {
    	case jaquetasItem:
    		mp.events.callRemote('RequestSetClothes', 11, drawable, 0);
    		jaquetasTextureNewArray = [];
			for (i = 0; i < player.getNumberOfTextureVariations(11, player.getDrawableVariation(11)) + 1; i++) jaquetasTextureNewArray.push(i.toString());
    		jaquetasTextureItem.Collection = new NativeUI.ItemsCollection(jaquetasTextureNewArray).getListItems();
    		jaquetasTextureItem.Index = 0;
    	break

    	case jaquetasTextureItem:
    		mp.events.callRemote('RequestSetClothes', 11, drawable, texture);
    }
});

menuRoupasJaquetas.ItemSelect.on((item, index) => {
    switch (item.Text) {

        case "Exit":
            menuRoupasJaquetas.RemoveItem(menuRoupasJaquetasExit);
            menuRoupasJaquetasExit = null;

            menuRoupasJaquetas.Visible = false;
            menuRoupas.Visible = true;
        break;
    }
});
///////////////////////////////////////////////////////
// MENU CAMISAS COMPONENT 8
///////////////////////////////////////////////////////


let camisasDrawable = [];
for (i = 0; i < player.getNumberOfDrawableVariations(8) + 1; i++) camisasDrawable.push(i.toString());

let camisasTextureArray = [];
let camisasTextureLimite = player.getNumberOfTextureVariations(8, player.getDrawableVariation(8));
for (i = 0; i < camisasTextureLimite + 1; i++) camisasTextureArray.push(i.toString());

const menuRoupasCamisas = new Menu("Undershirts", "", new Point(50, 50));
menuRoupasCamisasExit = null;
let camisasItem = new UIMenuListItem("Undershirts", "Select your undershirt.", new ItemsCollection(camisasDrawable), player.getDrawableVariation(8));
let camisasTextureItem = new UIMenuListItem("Color", "Select your undershirt's color.", new ItemsCollection(camisasTextureArray), player.getTextureVariation(8));
menuRoupasCamisas.AddItem(camisasItem);
menuRoupasCamisas.AddItem(camisasTextureItem);
menuRoupasCamisas.ParentMenu = menuRoupas;
menuRoupasCamisas.Visible = false;
todosMenus.push(menuRoupasCamisas);

menuRoupasCamisas.ListChange.on((item, listIndex) => {
	let drawable = parseInt(camisasItem.SelectedItem.DisplayText);
	let texture = parseInt(camisasTextureItem.SelectedItem.DisplayText);
    switch (item) {
    	case camisasItem:
    		mp.events.callRemote('RequestSetClothes', 8, drawable, 0);
    		camisasTextureNewArray = [];
			for (i = 0; i < player.getNumberOfTextureVariations(8, player.getDrawableVariation(8)) + 1; i++) camisasTextureNewArray.push(i.toString());
    		camisasTextureItem.Collection = new NativeUI.ItemsCollection(camisasTextureNewArray).getListItems();
    		camisasTextureItem.Index = 0;
    	break

    	case camisasTextureItem:
    		mp.events.callRemote('RequestSetClothes', 8, drawable, texture);
    }
});

menuRoupasCamisas.ItemSelect.on((item, index) => {
    switch (item.Text) {

        case "Exit":
            menuRoupasCamisas.RemoveItem(menuRoupasCamisasExit);
            menuRoupasCamisasExit = null;

            menuRoupasCamisas.Visible = false;
            menuRoupas.Visible = true;
        break;
    }
});

///////////////////////////////////////////////////////
// MENU CALÇAS COMPONENT 4
///////////////////////////////////////////////////////

let calcasDrawable = [];
for (i = 0; i < player.getNumberOfDrawableVariations(4) + 1; i++) calcasDrawable.push(i.toString());

let calcasTextureArray = [];
let calcasTextureLimite = player.getNumberOfTextureVariations(4, player.getDrawableVariation(4));
for (i = 0; i < calcasTextureLimite + 1; i++) calcasTextureArray.push(i.toString());

const menuRoupasCalcas = new Menu("Pants", "", new Point(50, 50));
menuRoupasCalcasExit = null;
let calcasItem = new UIMenuListItem("Pants", "Select your pants.", new ItemsCollection(calcasDrawable), player.getDrawableVariation(4));
let calcasTextureItem = new UIMenuListItem("Color", "Select your pants color.", new ItemsCollection(calcasTextureArray), player.getTextureVariation(4));
menuRoupasCalcas.AddItem(calcasItem);
menuRoupasCalcas.AddItem(calcasTextureItem);
menuRoupasCalcas.ParentMenu = menuRoupas;
menuRoupasCalcas.Visible = false;
todosMenus.push(menuRoupasCalcas);

menuRoupasCalcas.ListChange.on((item, listIndex) => {
    let drawable = parseInt(calcasItem.SelectedItem.DisplayText);
    let texture = parseInt(calcasTextureItem.SelectedItem.DisplayText);
    switch (item) {
        case calcasItem:
            mp.events.callRemote('RequestSetClothes', 4, drawable, 0);
            calcasTextureNewArray = [];
            for (i = 0; i < player.getNumberOfTextureVariations(4, player.getDrawableVariation(4)) + 1; i++) calcasTextureNewArray.push(i.toString());
            calcasTextureItem.Collection = new NativeUI.ItemsCollection(calcasTextureNewArray).getListItems();
            calcasTextureItem.Index = 0;
        break

        case calcasTextureItem:
            mp.events.callRemote('RequestSetClothes', 4, drawable, texture);
    }
});

menuRoupasCalcas.ItemSelect.on((item, index) => {
    switch (item.Text) {

        case "Exit":
            menuRoupasCalcas.RemoveItem(menuRoupasCalcasExit);
            menuRoupasCalcasExit = null;

            menuRoupasCalcas.Visible = false;
            menuRoupas.Visible = true;
        break;
    }
});

///////////////////////////////////////////////////////
// MENU SAPATOS COMPONENT 6
///////////////////////////////////////////////////////

let sapatosDrawable = [];
for (i = 0; i < player.getNumberOfDrawableVariations(6) + 1; i++) sapatosDrawable.push(i.toString());

let sapatosTextureArray = [];
let sapatosTextureLimite = player.getNumberOfTextureVariations(6, player.getDrawableVariation(6));
for (i = 0; i < sapatosTextureLimite + 1; i++) sapatosTextureArray.push(i.toString());

const menuRoupasSapatos = new Menu("Shoes", "", new Point(50, 50));
menuRoupasSapatosExit = null;
let sapatosItem = new UIMenuListItem("Shoes", "Select your shoes.", new ItemsCollection(sapatosDrawable), player.getDrawableVariation(6));
let sapatosTextureItem = new UIMenuListItem("Color", "Select your shoes color.", new ItemsCollection(sapatosTextureArray), player.getTextureVariation(6));
menuRoupasSapatos.AddItem(sapatosItem);
menuRoupasSapatos.AddItem(sapatosTextureItem);
menuRoupasSapatos.ParentMenu = menuRoupas;
menuRoupasSapatos.Visible = false;
todosMenus.push(menuRoupasSapatos);

menuRoupasSapatos.ListChange.on((item, listIndex) => {
    let drawable = parseInt(sapatosItem.SelectedItem.DisplayText);
    let texture = parseInt(sapatosTextureItem.SelectedItem.DisplayText);
    switch (item) {
        case sapatosItem:
            mp.events.callRemote('RequestSetClothes', 6, drawable, 0);
            sapatosTextureNewArray = [];
            for (i = 0; i < player.getNumberOfTextureVariations(6, player.getDrawableVariation(6)) + 1; i++) sapatosTextureNewArray.push(i.toString());
            sapatosTextureItem.Collection = new NativeUI.ItemsCollection(sapatosTextureNewArray).getListItems();
            sapatosTextureItem.Index = 0;
        break

        case sapatosTextureItem:
            mp.events.callRemote('RequestSetClothes', 6, drawable, texture);
    }
});

menuRoupasSapatos.ItemSelect.on((item, index) => {
    switch (item.Text) {

        case "Exit":
            menuRoupasSapatos.RemoveItem(menuRoupasSapatosExit);
            menuRoupasSapatosExit = null;

            menuRoupasSapatos.Visible = false;
            menuRoupas.Visible = true;
        break;
    }
});

///////////////////////////////////////////////////////
// MENU MÃOS COMPONENT 3
///////////////////////////////////////////////////////

let maosDrawable = [];
for (i = 0; i < player.getNumberOfDrawableVariations(3) + 1; i++) maosDrawable.push(i.toString());

let maosTextureArray = [];
let maosTextureLimite = player.getNumberOfTextureVariations(3, player.getDrawableVariation(3));
for (i = 0; i < maosTextureLimite + 1; i++) maosTextureArray.push(i.toString());

const menuRoupasMaos = new Menu("Torso", "", new Point(50, 50));
menuRoupasMaosExit = null;
let maosItem = new UIMenuListItem("Torso", "Select your torso.", new ItemsCollection(maosDrawable), player.getDrawableVariation(3));
let maosTextureItem = new UIMenuListItem("Color", "Select your torso variation.", new ItemsCollection(maosTextureArray), player.getTextureVariation(3));
menuRoupasMaos.AddItem(maosItem);
menuRoupasMaos.AddItem(maosTextureItem);
menuRoupasMaos.ParentMenu = menuRoupas;
menuRoupasMaos.Visible = false;
todosMenus.push(menuRoupasMaos);

menuRoupasMaos.ListChange.on((item, listIndex) => {
    let drawable = parseInt(maosItem.SelectedItem.DisplayText);
    let texture = parseInt(maosTextureItem.SelectedItem.DisplayText);
    switch (item) {
        case maosItem:
            mp.events.callRemote('RequestSetClothes', 3, drawable, 0);
            maosTextureNewArray = [];
            for (i = 0; i < player.getNumberOfTextureVariations(3, player.getDrawableVariation(3)) + 1; i++) maosTextureNewArray.push(i.toString());
            maosTextureItem.Collection = new NativeUI.ItemsCollection(maosTextureNewArray).getListItems();
            maosTextureItem.Index = 0;
        break

        case maosTextureItem:
            mp.events.callRemote('RequestSetClothes', 3, drawable, texture);
    }
});

menuRoupasMaos.ItemSelect.on((item, index) => {
    switch (item.Text) {

        case "Exit":
            menuRoupasMaos.RemoveItem(menuRoupasMaosExit);
            menuRoupasMaosExit = null;

            menuRoupasMaos.Visible = false;
            menuRoupas.Visible = true;
        break;
    }
});

///////////////////////////////////////////////////////
// MENU ÓCULOS PROP 1
///////////////////////////////////////////////////////

// Drawable
let oculosDrawable = [];
for (i = 0; i < player.getNumberOfPropDrawableVariations(1) + 1; i++) oculosDrawable.push(i.toString())
// Texture
let oculosTexture = [];
let oculosTextureLimite = player.getNumberOfPropTextureVariations(1, player.getPropIndex(0));
for (i = 0; i < oculosTextureLimite + 1; i++) oculosTexture.push(i.toString());

const menuRoupasOculos = new Menu("Glasses", "", new Point(50, 50));
menuRoupasOculosExit = null;
let oculosItem = new UIMenuListItem("Glasses", "Select your glasses.", new ItemsCollection(oculosDrawable), player.getPropIndex(0));
let oculosTextureItem = new UIMenuListItem("Color", "Select your glasses color.", new ItemsCollection(oculosTexture), player.getPropTextureIndex(1));
menuRoupasOculos.AddItem(oculosItem);
menuRoupasOculos.AddItem(oculosTextureItem);
menuRoupasOculos.ParentMenu = menuRoupas;
menuRoupasOculos.Visible = false;
todosMenus.push(menuRoupasOculos);

menuRoupasOculos.ListChange.on((item, listIndex) => {
	let drawable = parseInt(oculosItem.SelectedItem.DisplayText);
	let texture = parseInt(oculosTextureItem.SelectedItem.DisplayText);
    switch (item) {
    	case oculosItem:
    		mp.events.callRemote('RequestSetGlasses', drawable, 0);
    		oculosTextureNewArray = [];
			for (i = 0; i < player.getNumberOfPropTextureVariations(1, player.getPropIndex(1)) + 1; i++) oculosTextureNewArray.push(i.toString());
    		oculosTextureItem.Collection = new NativeUI.ItemsCollection(oculosTextureNewArray).getListItems();
    		oculosTextureItem.Index = 0;
    	break

    	case oculosTextureItem:
    		mp.events.callRemote('RequestSetGlasses', drawable, texture);
    }
});

menuRoupasOculos.ItemSelect.on((item, index) => {
    switch (item.Text) {

        case "Exit":
            menuRoupasOculos.RemoveItem(menuRoupasOculosExit);
            menuRoupasOculosExit = null;

            menuRoupasOculos.Visible = false;
            menuRoupas.Visible = true;
        break;
    }
});

///////////////////////////////////////////////////////
// MENU ACESSÓRIOS COMPONENT 7
///////////////////////////////////////////////////////

let acessoriosDrawable = [];
for (i = 0; i < player.getNumberOfDrawableVariations(7) + 1; i++) acessoriosDrawable.push(i.toString());

let acessoriosTextureArray = [];
let acessoriosTextureLimite = player.getNumberOfTextureVariations(7, player.getDrawableVariation(7));
for (i = 0; i < acessoriosTextureLimite + 1; i++) acessoriosTextureArray.push(i.toString());

const menuRoupasAcessorios = new Menu("Accessories", "", new Point(50, 50));
menuRoupasAcessoriosExit = null;
let acessoriosItem = new UIMenuListItem("Accessories", "Select your accessories.", new ItemsCollection(acessoriosDrawable), player.getDrawableVariation(7));
let acessoriosTextureItem = new UIMenuListItem("Color", "Select your accessories color.", new ItemsCollection(acessoriosTextureArray), player.getTextureVariation(7));
menuRoupasAcessorios.AddItem(acessoriosItem);
menuRoupasAcessorios.AddItem(acessoriosTextureItem);
menuRoupasAcessorios.ParentMenu = menuRoupas;
menuRoupasAcessorios.Visible = false;
todosMenus.push(menuRoupasAcessorios);

menuRoupasAcessorios.ListChange.on((item, listIndex) => {
    let drawable = parseInt(acessoriosItem.SelectedItem.DisplayText);
    let texture = parseInt(acessoriosTextureItem.SelectedItem.DisplayText);
    switch (item) {
        case acessoriosItem:
            mp.events.callRemote('RequestSetClothes', 7, drawable, 0);
            acessoriosTextureNewArray = [];
            for (i = 0; i < player.getNumberOfTextureVariations(7, player.getDrawableVariation(7)) + 1; i++) acessoriosTextureNewArray.push(i.toString());
            acessoriosTextureItem.Collection = new NativeUI.ItemsCollection(acessoriosTextureNewArray).getListItems();
            acessoriosTextureItem.Index = 0;
        break

        case acessoriosTextureItem:
            mp.events.callRemote('RequestSetClothes', 7, drawable, texture);
    }
});

menuRoupasAcessorios.ItemSelect.on((item, index) => {
    switch (item.Text) {

        case "Exit":
            menuRoupasAcessorios.RemoveItem(menuRoupasAcessoriosExit);
            menuRoupasAcessoriosExit = null;

            menuRoupasAcessorios.Visible = false;
            menuRoupas.Visible = true;
        break;
    }
});

///////////////////////////////////////////////////////
// MENU MOCHILAS COMPONENT 5
///////////////////////////////////////////////////////

let mochilasDrawable = [];
for (i = 0; i < player.getNumberOfDrawableVariations(5) + 1; i++) mochilasDrawable.push(i.toString());

let mochilasTextureArray = [];
let mochilasTextureLimite = player.getNumberOfTextureVariations(5, player.getDrawableVariation(5));;
for (i = 0; i < mochilasTextureLimite + 1; i++) mochilasTextureArray.push(i.toString());

const menuRoupasMochilas = new Menu("Bags", "", new Point(50, 50));
menuRoupasMochilasExit = null;
let mochilasItem = new UIMenuListItem("Bags", "Select your bag.", new ItemsCollection(mochilasDrawable), player.getDrawableVariation(5));
let mochilasTextureItem = new UIMenuListItem("Color", "Select your bag's color.", new ItemsCollection(mochilasTextureArray), player.getTextureVariation(5));
menuRoupasMochilas.AddItem(mochilasItem);
menuRoupasMochilas.AddItem(mochilasTextureItem);
menuRoupasMochilas.ParentMenu = menuRoupas;
menuRoupasMochilas.Visible = false;
todosMenus.push(menuRoupasMochilas);

menuRoupasMochilas.ListChange.on((item, listIndex) => {
    let drawable = parseInt(mochilasItem.SelectedItem.DisplayText);
    let texture = parseInt(mochilasTextureItem.SelectedItem.DisplayText);
    switch (item) {
        case mochilasItem:
            mp.events.callRemote('RequestSetClothes', 5, drawable, 0);
            mochilasTextureNewArray = [];
            for (i = 0; i < player.getNumberOfTextureVariations(5, player.getDrawableVariation(5)) + 1; i++) mochilasTextureNewArray.push(i.toString());
            mochilasTextureItem.Collection = new NativeUI.ItemsCollection(mochilasTextureNewArray).getListItems();
            mochilasTextureItem.Index = 0;
        break

        case mochilasTextureItem:
            mp.events.callRemote('RequestSetClothes', 5, drawable, texture);
    }
});

menuRoupasMochilas.ItemSelect.on((item, index) => {
    switch (item.Text) {

        case "Exit":
            menuRoupasMochilas.RemoveItem(menuRoupasMochilasExit);
            menuRoupasMochilasExit = null;

            menuRoupasMochilas.Visible = false;
            menuRoupas.Visible = true;
        break;
    }
});

///////////////////////////////////////////////////////////////////////////////////////////////////
/*
menuRoupas.BindMenuToItem(menuRoupasMascaras, menuRoupasItem1);
menuRoupas.BindMenuToItem(menuRoupasChapeus, menuRoupasItem2);
menuRoupas.BindMenuToItem(menuRoupasJaquetas, menuRoupasItem3);
menuRoupas.BindMenuToItem(menuRoupasCamisas, menuRoupasItem4);
menuRoupas.BindMenuToItem(menuRoupasCalcas, menuRoupasItem5);
menuRoupas.BindMenuToItem(menuRoupasSapatos, menuRoupasItem6);
menuRoupas.BindMenuToItem(menuRoupasMaos, menuRoupasItem7);
menuRoupas.BindMenuToItem(menuRoupasOculos, menuRoupasItem8);
menuRoupas.BindMenuToItem(menuRoupasAcessorios, menuRoupasItem9);
menuRoupas.BindMenuToItem(menuRoupasMochilas, menuRoupasItem10);*/


menuRoupas.ItemSelect.on((item, index) => {
    switch (index) {
        case 0:
            menuRoupas.Visible = false;
            menuRoupasChapeus.Visible = true;

            if(menuRoupasChapeusExit == null)
            {
                menuRoupasChapeusExit = new UIMenuItem("Exit", "Go back to last menu.");
                menuRoupasChapeusExit.ForeColor = new Color(127, 237, 28);
                menuRoupasChapeus.AddItem(menuRoupasChapeusExit);
            }
        break;

        case 1:
            menuRoupas.Visible = false;
            menuRoupasJaquetas.Visible = true;

            if(menuRoupasJaquetasExit == null)
            {
                menuRoupasJaquetasExit = new UIMenuItem("Exit", "Go back to last menu.");
                menuRoupasJaquetasExit.ForeColor = new Color(127, 237, 28);
                menuRoupasJaquetas.AddItem(menuRoupasJaquetasExit);
            }
        break;

        case 2:
            menuRoupas.Visible = false;
            menuRoupasMaos.Visible = true;

            if(menuRoupasMaosExit == null)
            {
                menuRoupasMaosExit = new UIMenuItem("Exit", "Go back to last menu.");
                menuRoupasMaosExit.ForeColor = new Color(127, 237, 28);
                menuRoupasMaos.AddItem(menuRoupasMaosExit);
            }
        break;

        case 3:
            menuRoupas.Visible = false;
            menuRoupasCamisas.Visible = true;

            if(menuRoupasCamisasExit == null)
            {
                menuRoupasCamisasExit = new UIMenuItem("Exit", "Go back to last menu.");
                menuRoupasCamisasExit.ForeColor = new Color(127, 237, 28);
                menuRoupasCamisas.AddItem(menuRoupasCamisasExit);
            }
        break;

        case 4:
            menuRoupas.Visible = false;
            menuRoupasCalcas.Visible = true;

            if(menuRoupasCalcasExit == null)
            {
                menuRoupasCalcasExit = new UIMenuItem("Exit", "Go back to last menu.");
                menuRoupasCalcasExit.ForeColor = new Color(127, 237, 28);
                menuRoupasCalcas.AddItem(menuRoupasCalcasExit);
            }
        break;

        case 5:
            menuRoupas.Visible = false;
            menuRoupasSapatos.Visible = true;

            if(menuRoupasSapatosExit == null)
            {
                menuRoupasSapatosExit = new UIMenuItem("Exit", "Go back to last menu.");
                menuRoupasSapatosExit.ForeColor = new Color(127, 237, 28);
                menuRoupasSapatos.AddItem(menuRoupasSapatosExit);
            }
        break;

        case 6:
            menuRoupas.Visible = false;
            menuRoupasOculos.Visible = true;

            if(menuRoupasOculosExit == null)
            {
                menuRoupasOculosExit = new UIMenuItem("Exit", "Go back to last menu.");
                menuRoupasOculosExit.ForeColor = new Color(127, 237, 28);
                menuRoupasOculos.AddItem(menuRoupasOculosExit);
            }
        break;

        case 7:
            menuRoupas.Visible = false;
            menuRoupasMochilas.Visible = true;

            if(menuRoupasMochilasExit == null)
            {
                menuRoupasMochilasExit = new UIMenuItem("Exit", "Go back to last menu.");
                menuRoupasMochilasExit.ForeColor = new Color(127, 237, 28);
                menuRoupasMochilas.AddItem(menuRoupasMochilasExit);
            }
        break;

        case 8:
            menuRoupas.Visible = false;
            menuRoupasMascaras.Visible = true;

            if(menuRoupasMascarasExit == null)
            {
                menuRoupasMascarasExit = new UIMenuItem("Exit", "Go back to last menu.");
                menuRoupasMascarasExit.ForeColor = new Color(127, 237, 28);
                menuRoupasMascaras.AddItem(menuRoupasMascarasExit);
            }
        break;

        case 9:
            menuRoupas.Visible = false;
            menuRoupasAcessorios.Visible = true;

            if(menuRoupasAcessoriosExit == null)
            {
                menuRoupasAcessoriosExit = new UIMenuItem("Exit", "Go back to last menu.");
                menuRoupasAcessoriosExit.ForeColor = new Color(127, 237, 28);
                menuRoupasAcessorios.AddItem(menuRoupasAcessoriosExit);
            }
        break;

        case 10:
            menuRoupas.Close();
        break;
    }
});

mp.events.add("ShowClothesMenu", () =>
{
    if (menuRoupas.Visible | menuRoupasMascaras.Visible | 
        menuRoupasChapeus.Visible | 
        menuRoupasJaquetas.Visible | 
        menuRoupasCamisas.Visible | 
        menuRoupasCalcas.Visible | 
        menuRoupasSapatos.Visible | 
        menuRoupasMaos.Visible | 
        menuRoupasOculos.Visible | 
        menuRoupasAcessorios.Visible | 
        menuRoupasMochilas.Visible) {
        todosMenus.forEach(function(element, index, array){element.Close()});
    } else {
        
        menuRoupas.Open();
        if(menuRoupasExit == null)
        {
            menuRoupasExit = new UIMenuItem("Exit", "Go back to last menu.");
            menuRoupasExit.ForeColor = new Color(127, 237, 28);
            menuRoupas.AddItem(menuRoupasExit);
        }
        
        mp.gui.chat.show(false);
        mp.gui.cursor.visible = false;
    }
});

menuRoupas.MenuClose.on(() => {
    menuRoupas.RemoveItem(menuRoupasExit);
    menuRoupasExit = null;
    mp.gui.chat.show(true);
    mp.gui.cursor.visible = false;
});
