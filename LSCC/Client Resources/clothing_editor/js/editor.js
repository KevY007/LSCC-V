let selectedShirtType = 0;
var Content;

var ClothingIDs = [];

var Team = 1;
var Gender = "M";
var ClothingData;

var CurrentShirt = "JacketOpen";
var AcceptedItems = [];



function ShowClothingEditor(team, gender){
    Team = team;
    Gender = gender;
    selectedShirtType = 0;
    AcceptedItems = [];
    CurrentShirt = "JacketOpen";
    ClothingData = [];
    ClothingIDs = [];

    UpdateContent();
    updateCurrentClothingData();
}
function ExitClothingEditor(){
    mp.trigger("ClothingEditor:exit", false);
}
function SaveClothingEditor(){
    mp.trigger("ClothingEditor:exit", true);
}

document.addEventListener("DOMContentLoaded", () => {
    Content = document.getElementById("content");

    UpdateContent();
});
function UpdateContent(){
    AcceptedItems = [];
    Content.innerHTML = "";
    Content.innerHTML += `
    <div class="item-selector">
        <div class="item-left" onclick="scrollShirtType(false)">
            &#60;
        </div>
        <div class="item-right" onclick="scrollShirtType(true)">
            >
        </div>
        <p id="shirttype">
        ${LinkedTypes[selectedShirtType].type}
        </p>
    </div>
    `;
    CurrentShirt = LinkedTypes[selectedShirtType].type;

    if(Gender == "M") ClothingData = MaleClothing;
    else ClothingData = FemaleClothing;

    let clothingData = ClothingData.find(c => c.type == CurrentShirt);
    AcceptedItems.push({ 
        name:clothingData.clothing,
        items:clothingData.items
    });
    Content.innerHTML += `
    <div class="item-selector">
        <div class="item-left" onclick="scrollItem('Shirt', 1)">
            &#60;
        </div>
        <div class="item-right" onclick="scrollItem('Shirt', -1)">
            >
        </div>
        <p>
        Shirt:&nbsp;
        </p>
        <p id="Shirt">
        ${clothingData.items[0]}
        </p>
        <div class="texture-select">
        <div class="texture-left" onclick="scrollTexture('Shirt', -1)">&#60;</div>
        <div class="texture-right" onclick="scrollTexture('Shirt', 1)">></div>
            <p>
            Texture:&nbsp;
            </p>
            <p id="Shirt_texture">
            0
            </p>
        </div>
    </div>
    `;
    let realType = LinkedTypes.find(l => l.type == CurrentShirt);
    for(let i = 0 ; i < ClothingData.length ; i++){
        let acceptedCheck = [];
        switch(ClothingData[i].clothing){
            case "Undershirt": acceptedCheck = realType.accepted.Undershirts;
            break;
            case "Pants": acceptedCheck = realType.accepted.Pants;
            break;
            case "Shoes": acceptedCheck = realType.accepted.Shoes;
            break;
            case "Accessory 1": acceptedCheck = realType.accepted.Bags;
            break;
            case "Accessory 2": acceptedCheck = realType.accepted.Accessories;
            break;
            case "Armor": acceptedCheck = realType.accepted.Armor;
            break;
            case "Handwear": acceptedCheck = realType.accepted.Torso;
            break;
            case "Hats": acceptedCheck = realType.accepted.Hats;
            break;
        }
        if(!acceptedCheck.includes(ClothingData[i].type)) continue;

        let accepted = AcceptedItems.find(a => a.name == ClothingData[i].clothing);
        if(accepted){
            accepted.items = [...accepted.items, ...ClothingData[i].items];
        }
        else{
            AcceptedItems.push({
                name:ClothingData[i].clothing,
                items:ClothingData[i].items
            });
        }

        if(document.getElementById(ClothingData[i].clothing) == undefined){
            Content.innerHTML += `
            <div class="item-selector">
                <div class="item-left" onclick="scrollItem('${ClothingData[i].clothing}', 1)">
                    &#60;
                </div>
                <div class="item-right" onclick="scrollItem('${ClothingData[i].clothing}', -1)">
                    >
                </div>
                <p>
                ${ClothingData[i].clothing}:&nbsp;
                </p>
                <p id="${ClothingData[i].clothing}">
                ${ClothingData[i].items[0]}
                </p>
                <div class="texture-select">
                <div class="texture-left" onclick="scrollTexture('${ClothingData[i].clothing}', -1)">&#60;</div>
                <div class="texture-right" onclick="scrollTexture('${ClothingData[i].clothing}', 1)">></div>
                <p>
                Texture:&nbsp;
                </p>
                <p id="${ClothingData[i].clothing}_texture">
                0
                </p>
            </div>
            </div>
            `;
        }
    }
}

function scrollTexture(clothingName, value){
    let elementID = `${clothingName}_texture`;
    let currentID = Number(document.getElementById(elementID).innerHTML);
    
    currentID += value;
    if(currentID > 17) currentID = 0;
    if(currentID < 0) currentID = 16;
    document.getElementById(elementID).innerHTML = currentID;

    let currentClothing = Number(document.getElementById(clothingName).innerHTML);
    mp.trigger("ClothingEditor:updateClothing", clothingName, currentClothing, currentID);

    updateCurrentClothingData();
}

function scrollItem(clothingName, value){
    let currentID = Number(document.getElementById(clothingName).innerHTML);
    let acceptedItem = AcceptedItems.find(a => a.name == clothingName);

    let currentIdx = acceptedItem.items.indexOf(currentID);
    currentIdx += value;
    if(currentIdx >= acceptedItem.items.length) currentIdx = 0;
    if(currentIdx < 0) currentIdx = acceptedItem.items.length-1;

    document.getElementById(clothingName).innerHTML = acceptedItem.items[currentIdx];
    document.getElementById(clothingName + "_texture").innerHTML = "0";
    mp.trigger("ClothingEditor:updateClothing", clothingName, acceptedItem.items[currentIdx], 0);
    updateCurrentClothingData();
}

function scrollShirtType(right){
    console.log(LinkedTypes);
    if(right){
        selectedShirtType++;
        if(selectedShirtType >= LinkedTypes.length) selectedShirtType = 0;
    }
    else{
        selectedShirtType--;
        if(selectedShirtType < 0) selectedShirtType = LinkedTypes.length-1;
    }
    UpdateContent();
    updateCurrentClothingData();
}

function updateCurrentClothingData(){
    var currentClothing = "{";
    for(let i = 0 ; i < AcceptedItems.length ; i++){
        let currentIdx = Number(document.getElementById(AcceptedItems[i].name).innerHTML);
        let texture = Number(document.getElementById(AcceptedItems[i].name + "_texture").innerHTML);

        currentClothing += `"${AcceptedItems[i].name}":[${currentIdx},${texture}]`;
        if(i != AcceptedItems.length-1) currentClothing += ",";
        
    }
    currentClothing += "}";
    //mp.trigger("ClothingEditor:updateClothing", currentClothing);
}