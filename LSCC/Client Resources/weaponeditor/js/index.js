var ItemsHolder;
var InfoTitle; // also serves as back button
var InfoText;
const blockedCategories = ["Melee", "Thrown Weapons", "Heavy Weapons"];

var CurrentPage = "home";
var CurrentCategory = "";

document.addEventListener("DOMContentLoaded", () => {
    
    ItemsHolder = document.getElementById("items");
    InfoTitle = document.getElementById("info_title");
    InfoText = document.getElementById("info_text");

    showCategories();
});

function showCategories(){
    CurrentPage = "home";
    CurrentCategory = "";
    InfoTitle.innerHTML = "Weapon Customizer";
    InfoTitle.className = "";
    InfoTitle.removeEventListener("click", backButtonPress);

    InfoText.innerHTML = "Select a category"

    ItemsHolder.innerHTML = "";
    for(let i = 0 ; i < weaponData.length ; i++){
        let type = weaponData[i].Type;
        if(blockedCategories.includes(type)) continue;
        if(document.getElementById(`category_${type}`) == undefined){
            let element = `
                <div class="weapon-item" id="category_${type}" onclick="openCategory('${type}')">
                    <img src="./js/${weaponData[i].ImageSrc}"/>
                    <p class="hide-item-name">${type}</p>
                </div>
            `;
            ItemsHolder.innerHTML += element;
        }
    }
    setTimeout(() => {
        let elements = document.getElementsByTagName("p");
        for(let i = 0; i < elements.length; i++) { elements[i].className = "";}
    }, 1000);
}

function backButtonPress(){
    
    switch(CurrentPage){
        case "category":
            showCategories();
            break;
        case "weapon":
            openCategory(CurrentCategory);
            break;
    }
}

function openCategory(type){
    CurrentPage = "category";
    CurrentCategory = type;
    InfoTitle.innerHTML = "CLICK HERE TO GO BACK";
    InfoTitle.className = "back-button";
    InfoTitle.addEventListener("click", backButtonPress);

    InfoText.innerHTML = "Select a weapon"

    ItemsHolder.innerHTML = "";

    weaponData.filter(w => w.Type == type).forEach((weapon) => {   
        let element = `
            <div class="weapon-item" id="weapon_${weapon.Name}" onclick="openWeapon('${weapon.Name}')">
                <img src="./js/${weapon.ImageSrc}"/>
                <p class="hide-item-name">${weapon.Name}</p>
            </div>
        `;
        ItemsHolder.innerHTML += element;
    });
    setTimeout(() => {
        let elements = document.getElementsByTagName("p");
        for(let i = 0; i < elements.length; i++) { elements[i].className = "";}
    }, 1000);
}

function openWeapon(name){
    CurrentPage = "weapon";

    InfoText.innerHTML = "Select an attachment";
    ItemsHolder.innerHTML = `
        <div class="top-extension">
            <p>to preview it</p>
            <div class="option-buttons">
                <div class="option">
                    PURCHASE
                </div>
                <div class="option">
                    REMOVE
                </div>
            </div>
        </div>
    `;
    let weapon = weaponData.find(w => w.Name == name)
    let hashNumber = weapon.Hash;
    let numero = Number(hashNumber);
    let wtype = weapon.Type;
    mp.trigger("WeaponEditor:requestComponentList", numero, wtype);

    /* Add Weapon Attachments Here */
    /*
           
                <div class="attachment-item-selected">
                    Grip
                </div>
    */
}   

function selectAttachment(name, hash){
    if(document.getElementsByClassName("attachment-item-selected").length > 0)
        document.getElementsByClassName("attachment-item-selected")[0].className = "attachment-item";

    document.getElementById(name).className = "attachment-item-selected";
    mp.trigger("WeaponEditor:requestAttachmentPreview", hash);
}

function setComponentList(array){
    for(let i = 0; i < array.length; i++){
        if(array[i].name == "") continue;
        ItemsHolder.innerHTML += `
            <div class="attachment-item" id="${array[i].name}" onclick="selectAttachment('${array[i].name}', '${array[i].hash}')">
                ${array[i].name}
            </div>
        `;
    }
}