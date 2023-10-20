function adjustTextarea(e){
    e.style.height = "1px";
    e.style.height = (e.scrollHeight-25)+"px";
  }
var HelpBoxText = document.getElementById("help_box_text");
var HelpBoxWrapper = document.getElementById("help_box_wrapper");
var HelpBox = document.getElementById("help_box");
var HelpBoxClose = document.getElementById("help_box_close");
HelpBoxClose.addEventListener("click", (e) => hideHelpBox());
function showHelpBox(text){
    HelpBoxWrapper.style.visibility = "visible";

    HelpBoxText.innerHTML = text;
    adjustTextarea(HelpBoxText);
}
function hideHelpBox(){
    HelpBoxWrapper.style.visibility = "hidden";
}
function onRequestHelp(id){
    var text = ``;
    switch(id){
        case 0:
            text = `The EXTERIOR position - this position MUST be OUTSIDE.
            
            This is where a player will be teleported when they, for example, exit a TP interior.`;
            break;
        case 1:
            text = `The position to teleport the player to; can be inside or outside.
            
            Make sure you set the correct Dimension option (Inside/Outside) based on where this TP is leading to.
            
            This is where a player will be teleported to when they, for example, enter a TP interior.`;
            break;
        case 2:
            text = `Choose a fitting Blip to show on the Map/Radar for this interior.
            
            Utilize the Blip List to get the correct Blip ID. Set to -1 to disable blips for this interior.`;
            break;  
        case 3:
            text = `Some buildings may have multiple entrances! The 'Side Entrance' option will make this interior act as a side entrance for a "main" building.
            
            For example, you've created the Diamond Casino teleport. If you wanted to create a rooftop entrance for the casino, you'd set this side entrance to the Diamond Casino teleport's ID.
             
            By default, this option is set to -1, which means it's a standalone interior.`;
            break;     
        case 4:
            text = `Choose whether this teleport sets the player into a custom dimension for this teleport, or into the "Outside" dimension.
            
            If your teleport leads to a TP interior, set it to "Inside". If it leads to another outside spot, set it to "Outside".`;
            break;     
        case 5:
            text = `How long to freeze the player for before they're auto-unfrozen. Useful for interiors that may take longer to load.
            
            It's recommended to set this to around 1000ms (1 second). Players can unfreeze themselves earlier at will.
            
            Feel free to set this to 0 if the TP leads to an Outside spot.`;
            break;           
        case 6:
            text = `An IPL, in simple terms, is a part of the game map that can be loaded & unloaded by the script.
            
            For example, Online apartments aren't loaded by default. Therefore, if you wanted to create a teleport to one, you would need to add 'apa_v_mp_h_01_a' to the IPL list to load its interior.
             
            You generally won't need to use this unless you're creating a teleport to a GTA:O DLC interior.`;
            break;                                               
    }
    showHelpBox(text);
}

var BlipsList = document.getElementById("blips_list_wrapper");
var BlipsListClose = document.getElementById("blips_list_close");
BlipsListClose.addEventListener("click", (e) => toggleBlipList(false));
function toggleBlipList(toggle){
    BlipsList.style.visibility = (toggle ? "visible" : "hidden");
}


var TP_SETTINGS = {
    Name:"",
    FromPosition:{x:0, y:0, z:0, h:0},
    ToPosition:{x:0, y:0, z:0, h:0},
    BlipID:-1,
    SideEntrance:-1,
    Dimension:"Outside",
    FreezeTime:0,
    IPLArray:[]
};

var SetFromButton = document.getElementById("set_from_position");
var SetToButton = document.getElementById("set_to_position");
var BlipIDInput = document.getElementById("blip_id");
var SideEntranceInput = document.getElementById("side_entrance_id");
var DimOutsideButton = document.getElementById("dim_outside_button");
var DimInsideButton = document.getElementById("dim_inside_button");
var FreezeTimeInput = document.getElementById("freeze_time");
var IPLNameInput = document.getElementById("ipl_name_input");
var IPLItemsList = document.getElementById("ipl_items_list");
var IPLButtonAdd = document.getElementById("ipl_name_add");

const mp = {
    trigger:function(event){
        console.log(event);
    }
}
SetFromButton.addEventListener("click", (e) => {
    mp.trigger("TeleportManager:RequestSetFromPosition");
});
SetToButton.addEventListener("click", (e) => {
    mp.trigger("TeleportManager:RequestSetToPosition");
});
BlipIDInput.addEventListener("change", (e) => {
    TP_SETTINGS.BlipID = BlipIDInput.value;
});
SideEntranceInput.addEventListener("change", (e) => {
    TP_SETTINGS.SideEntrance = SideEntranceInput.value;
});
FreezeTimeInput.addEventListener("change", (e) => {
    TP_SETTINGS.FreezeTime = FreezeTimeInput.value;
});
DimOutsideButton.addEventListener("click", (e) => {
    TP_SETTINGS.Dimension = "Outside";
    DimOutsideButton.style.backgroundColor = "white";
    DimOutsideButton.style.color = "black";

    DimInsideButton.style.backgroundColor = "transparent";
    DimInsideButton.style.color = "white";
});
DimInsideButton.addEventListener("click", (e) => {
    TP_SETTINGS.Dimension = "Inside";
    DimInsideButton.style.backgroundColor = "white";
    DimInsideButton.style.color = "black";

    DimOutsideButton.style.backgroundColor = "transparent";
    DimOutsideButton.style.color = "white";
});
IPLButtonAdd.addEventListener("click", (e) => {
    if(!IPLNameInput.value) return;
    if(TP_SETTINGS.IPLArray.includes(IPLNameInput.value)) return;

    TP_SETTINGS.IPLArray.push(IPLNameInput.value);
    IPLItemsList.innerHTML += `<div id="${IPLNameInput.value}" class="ipl-item">
    <p>${IPLNameInput.value}</p>
    <button onclick="removeIPL('${IPLNameInput.value}')">Remove</button>
    </div>`;

    IPLNameInput.value = "";
});

function removeIPL(name){
    if(document.getElementById(name))
        document.getElementById(name).remove();

    if(TP_SETTINGS.IPLArray.includes(name))
        TP_SETTINGS.IPLArray.splice(TP_SETTINGS.IPLArray.indexOf(name), 1);
}

