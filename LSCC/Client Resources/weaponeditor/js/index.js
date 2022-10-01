var ItemsHolder;
var InfoTitle; // also serves as back button
var InfoText;
const blockedCategories = ["Melee", "Thrown Weapons", "Heavy Weapons"];

var CurrentPage = "home";
var CurrentCategory = "";
var CurrentWeapon = undefined;
let CurrentAttachment = undefined;

document.addEventListener("DOMContentLoaded", () => {
    
    ItemsHolder = document.getElementById("items");
    InfoTitle = document.getElementById("info_title");
    InfoText = document.getElementById("info_text");

    showCategories();
});

function resetWeaponEditor(){
    CurrentPage = "home";
    CurrentCategory = "";
    CurrentWeapon = undefined;
    CurrentAttachment = undefined;
    showCategories();
}

function exitButtonPress(){
    mp.trigger("WeaponEditor:exit");
}

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
                    <img style="height:48px; width:auto;" src="${weaponData[i].ImageSrc}"/>
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
    mp.trigger("WeaponEditor:playSound", "back");
    switch(CurrentPage){
        case "category":
            showCategories(false);
            break;
        case "weapon":
            openCategory(CurrentCategory, false);
            break;
    }
}

// class mp { DEBUG PURPOSES ONLY
//     static trigger(text){ console.log(text); }
//   }

function openCategory(type, sound=true){
    if(sound == true)
        mp.trigger("WeaponEditor:playSound", "select");

    CurrentPage = "category";
    CurrentCategory = type;
    InfoTitle.innerHTML = "CLICK HERE TO GO BACK";
    InfoTitle.className = "back-button";
    InfoTitle.addEventListener("click", backButtonPress);

    InfoText.innerHTML = "Select a weapon";
    CurrentWeapon = undefined;
    CurrentAttachment = undefined;

    ItemsHolder.innerHTML = "";

    weaponData.filter(w => w.Type == type).forEach((weapon) => {   
        let element = `
            <div class="weapon-item" id="weapon_${weapon.Name}" onclick="openWeapon('${weapon.Name}')">
                <img style="height:48px; width:auto;" src="${weapon.ImageSrc}"/>
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

function openWeapon(name, sound=true){
    if(sound == true)
        mp.trigger("WeaponEditor:playSound", "select");

    CurrentPage = "weapon";

    InfoText.innerHTML = "Select an attachment";
    ItemsHolder.innerHTML = `
        <div class="top-extension">
            <p id="extension-help">to preview it</p>
            <div class="option-buttons">
                <div class="option" id="option-button" onClick="optionClick()">
                    PURCHASE
                </div>
            </div>
        </div>
    `;
    let weapon = weaponData.find(w => w.Name == name);
    CurrentWeapon = weapon;
    CurrentAttachment = undefined;
    document.getElementById("option-button").style.display = "none";
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
    mp.trigger("WeaponEditor:playSound", "select");
    let ExtensionInfo = document.getElementById("extension-help");
    if(document.getElementsByClassName("attachment-item-selected").length > 0)
    {
        if(document.getElementsByClassName("attachment-item-selected")[0].id.endsWith("true")){
            document.getElementsByClassName("attachment-item-selected")[0].className = "attachment-item-purchased";
        }
        else document.getElementsByClassName("attachment-item-selected")[0].className = "attachment-item";
    }

    for(let i = 0 ; i < attachmentsList.length ; i++){
        if(attachmentsList[i].attached == false && attachmentsList[i].hash != hash){
             let wHash = Number(CurrentWeapon.Hash);
            mp.trigger("WeaponEditor:removePreviewAttachment", wHash, attachmentsList[i].hash);
        }
    }

    let attachment = attachmentsList.find(a => a.hash == hash);
    if(attachment){
        CurrentAttachment = attachment;
        document.getElementById(`${name}_${attachment.attached.toString()}`).className = "attachment-item-selected";
        mp.trigger("WeaponEditor:requestAttachmentPreview", hash);

        if(attachment.attached){
            document.getElementById("option-button").style.display = "flex";
            document.getElementById("option-button").innerHTML = "REMOVE";
            InfoText.innerHTML = `Removing this attachment will <strong>sell</strong> it!`;
            ExtensionInfo.innerHTML = `<br></br><br></br>You'll be refunded the full price.`;
        }
        else{
            document.getElementById("option-button").style.display = "flex";
            document.getElementById("option-button").innerHTML = "PURCHASE";
            InfoText.innerHTML = `<strong>Price:</strong> ${attachment.price} Points`;
            let nametodisp = name;
            let words = name.split(" ");
            if(words.length > 2) nametodisp = `${words[0]} ${words[1]}...`;
            ExtensionInfo.innerHTML = `Selected: ${nametodisp}`;
        }
    }

}

function setAttachmentPurchased(hash, purchased){
    let attachmentName = attachmentsList.find(a => a.hash == hash).name;
    attachmentsList.find(a => a.hash == hash).attached = purchased;

    let element = document.getElementById(`${attachmentName}_${!purchased}`);
    element.id = `${attachmentName}_${purchased}`;
    if(purchased)
    {
        if(CurrentAttachment) CurrentAttachment.attached = true;
        mp.trigger("WeaponEditor:playSound", "buy");
        element.className = "attachment-item-purchased";
    }
    else 
    {
        if(CurrentAttachment) CurrentAttachment.attached = false;
        let wHash = Number(CurrentWeapon.Hash);
        mp.trigger("WeaponEditor:removePreviewAttachment", wHash, CurrentAttachment.hash);
        mp.trigger("WeaponEditor:playSound", "sell");
        element.className = "attachment-item";
    }
    
    let ExtensionInfo = document.getElementById("extension-help");
    InfoText.innerHTML = "Select an attachment";
    ExtensionInfo.innerHTML = "to preview it";
}

function optionClick(){
    if(CurrentAttachment){
        if(CurrentAttachment.attached == false){
            mp.trigger("WeaponEditor:requestAttachmentPurchase", CurrentAttachment.hash, CurrentAttachment.price);
            document.getElementById("option-button").style.display = "none";
        }
        else{
            mp.trigger("WeaponEditor:requestAttachmentRemove", CurrentAttachment.hash, CurrentAttachment.price);
            document.getElementById("option-button").style.display = "none";
        }
    }
    else{
        mp.trigger("HUDError", "You did not select an attachment!");
    }
}

function setComponentList(array){
    attachmentsList = array;
    for(let i = 0; i < array.length; i++){
        if(array[i].name == "") continue;

        let classtoset = (array[i].attached == true ? "attachment-item-purchased" : "attachment-item");

        ItemsHolder.innerHTML += `
            <div class="${classtoset}" id="${array[i].name}_${array[i].attached.toString()}" onclick="selectAttachment('${array[i].name}', '${array[i].hash}')">
                ${array[i].name}
            </div>
        `;
    }
}

var attachmentsList = [];