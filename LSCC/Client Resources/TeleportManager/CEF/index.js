var TELEPORTS_LIST = []; // set by client
const DEFAULT_TP_SETTINGS = {
    Name:"",
    FromPosition:{x:0, y:0, z:0, h:0},
    ToPosition:{x:0, y:0, z:0, h:0},
    BlipID:-1,
    SideEntrance:-1,
    Dimension:"Outside",
    FreezeTime:0,
    IPLArray:[]
};
var TP_SETTINGS = DEFAULT_TP_SETTINGS;

function clearSelection()
{
 if (window.getSelection) {window.getSelection().removeAllRanges();}
 else if (document.selection) {document.selection.empty();}
}

var IsNewTeleport = true;
var ExistingTeleportSearch = document.getElementById("new_teleport_name_input");
var ExistingSearchResults = document.getElementById("teleport_existing_search_results");

function updateExistingTeleportsList(){

    ExistingSearchResults.innerHTML = "";
    const teleportsToSet = TELEPORTS_LIST;
    for(let i = 0 ; i < teleportsToSet.length ; i++){
        ExistingSearchResults.innerHTML += `<div class="main-menu-result">
                                                <p>${teleportsToSet[i].Name}</p>
                                                <div>
                                                    <button onclick="existingTeleportClick(false, ${teleportsToSet[i].SQL_ID})" id="teleport_teleport_ID">Teleport</button>
                                                    <button onclick="existingTeleportClick(true, ${teleportsToSet[i].SQL_ID})" id="teleport_edit_ID">Edit</button>
                                                </div>
                                            </div>`
    }
}

ExistingTeleportSearch.addEventListener("input", (e) => {
    if(ExistingTeleportSearch.value.length == 0){
        updateExistingTeleportsList();
        return;
    }

    ExistingSearchResults.innerHTML = "";
    const teleportsToSet = TELEPORTS_LIST.filter(t => t.Name.toLowerCase().startsWith(ExistingTeleportSearch.value.toLowerCase()));
    for(let i = 0 ; i < teleportsToSet.length ; i++){
        ExistingSearchResults.innerHTML += `<div class="main-menu-result">
                                                <p>${teleportsToSet[i].Name}</p>
                                                <div>
                                                    <button onclick="existingTeleportClick(false, ${teleportsToSet[i].SQL_ID})" id="teleport_teleport_ID">Teleport</button>
                                                    <button onclick="existingTeleportClick(true, ${teleportsToSet[i].SQL_ID})" id="teleport_edit_ID">Edit</button>
                                                </div>
                                            </div>`
    }
});


var FromDisplayPos = document.getElementById("fromPosDisplay");
var ToDisplayPos = document.getElementById("toPosDisplay");

var CloseButton = document.getElementById("close_button");
var UtilityButton = document.getElementById("utility_button");
var UtilityPanel = document.getElementById("utility_pannel");
var InputTPX = document.getElementById("tp_X");
var InputTPY = document.getElementById("tp_Y");
var InputTPZ = document.getElementById("tp_Z");
var TPButton = document.getElementById("tp_button");
var TPButtonFrom = document.getElementById("tp_button_from");
var TPButtonTo = document.getElementById("tp_button_to");
var NoclipButton = document.getElementById("noclip_button");

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

var TeleportList = document.getElementById("teleport_list_wrapper");
var TeleportListItems = document.getElementById("teleport_list_id");
function toggleTeleportList(toggle){
    TeleportList.style.visibility = (toggle ? "visible" : "hidden");

    TeleportSearch.value = "";
    updateTPList(TELEPORTS_LIST);

}
var TeleportSearch = document.getElementById("teleport_list_search");
TeleportSearch.addEventListener("input", (e) => {
    const tpArray = TELEPORTS_LIST.filter(t => t.Name.toLowerCase().startsWith(TeleportSearch.value.toLowerCase()));
    updateTPList(tpArray);
});
function updateTPList(tpArray){
    TeleportListItems.innerHTML = "";
    for(let i = 0 ; i < tpArray.length ; i++){
        TeleportListItems.innerHTML += `<div>
                                            <h1>${tpArray[i].Name} | ID: ${tpArray[i].SQL_ID}</h1>
                                        </div>`
    }
}


var CreateTeleportButton = document.getElementById("new_teleport_button");
CreateTeleportButton.addEventListener("click", (e) => {
    EDITING_TELEPORT_ID = -1;
    const name = document.getElementById("teleport_name_input").value;
    if(!name || name.length === 0 || name.length > 32){
        mp.trigger("TeleportEditor:Error", "Teleport name must be between 0 and 32 characters!");
        return;
    }

    TP_SETTINGS = DEFAULT_TP_SETTINGS;
    TP_SETTINGS.Name = name;
    openTeleportEditor(TP_SETTINGS);
});

function setEditingNewTeleport(isNewTeleport){
    isNewTeleport = isNewTeleport;

    if(isNewTeleport){
        document.getElementById("top_buttonset_create").style.visibility = "visible";
        document.getElementById("top_buttonset_create").style.position = "";
        document.getElementById("top_buttonset_edit").style.visibility = "hidden";
        document.getElementById("top_buttonset_edit").style.position = "absolute";
    }
    else{
        document.getElementById("top_buttonset_edit").style.visibility = "visible";
        document.getElementById("top_buttonset_edit").style.position = "";
        document.getElementById("top_buttonset_create").style.visibility = "hidden";
        document.getElementById("top_buttonset_create").style.position = "absolute";
    }
}

function openTeleportEditorFromClient(tpData){
    const TPData = tpData;

    var iplList = TPData.IPLs.split(',');

    TP_SETTINGS = {
        Name:TPData.Name,
        FromPosition:{x:TPData.FromX, y:TPData.FromY, z:TPData.FromZ, h:TPData.FromHeading},
        ToPosition:{x:TPData.ToX, y:TPData.ToY, z:TPData.ToZ, h:TPData.ToHeading},
        BlipID:TPData.BlipID,
        SideEntrance:TPData.SideEntranceTo,
        Dimension:(TPData.ToDimension == -1 ? "Outside" : "Inside"),
        FreezeTime:TPData.FreezeTime,
        IPLArray:((iplList && iplList.length > 0) ? iplList : [])
    };
    openTeleportEditor(TP_SETTINGS);
    setEditingNewTeleport(false);    
}
function openTeleportEditor(settings){
    TP_SETTINGS = settings;
    CreateTPButton.innerHTML = `Create ${settings.Name}`; 
    FromDisplayPos.innerHTML = `X:${settings.FromPosition.x} Y:${settings.FromPosition.y} Z:${settings.FromPosition.z} H:${settings.FromPosition.h}`;
    ToDisplayPos.innerHTML = `X:${settings.ToPosition.x} Y:${settings.ToPosition.y} Z:${settings.ToPosition.z} H:${settings.ToPosition.h}`;
    BlipIDInput.value = (settings.BlipID == -1 ? '' : settings.BlipID);
    SideEntranceInput.value = (settings.SideEntrance == -1 ? '' : settings.SideEntrance);
    if(settings.Dimension == "Outside"){
        DimOutsideButton.style.backgroundColor = "white";
        DimOutsideButton.style.color = "black";
    
        DimInsideButton.style.backgroundColor = "transparent";
        DimInsideButton.style.color = "white";
    }
    else{
        DimInsideButton.style.backgroundColor = "white";
        DimInsideButton.style.color = "black";
    
        DimOutsideButton.style.backgroundColor = "transparent";
        DimOutsideButton.style.color = "white";
    }

    FreezeTimeInput.value = (settings.FreezeTime == 0 ? "" : settings.FreezeTime);

    updateIPLList();

    document.getElementById("main_menu").style.visibility = "hidden";
    document.getElementById("main_menu").style.position = "absolute";
    document.getElementById("teleport_editor").style.visibility = "visible";
    document.getElementById("teleport_editor").style.position = "";
}

var utilityPanelOpen = false;
function toggleUtilityPanel(){
    utilityPanelOpen = !utilityPanelOpen;

    UtilityPanel.style.visibility = (utilityPanelOpen ? "visible" : "hidden");
}
UtilityButton.addEventListener("click", (e) => toggleUtilityPanel());


// const mp = {
//     trigger:function(event){
//         console.log(event);
//     },
//     events:{
//         add:function(event, params){
//             console.log(`Event registered: ${event} [${params}]`);
//         }
//     }
// }
BlipIDInput.addEventListener("change", (e) => {
    clearSelection();
    TP_SETTINGS.BlipID = BlipIDInput.value;
});
SideEntranceInput.addEventListener("change", (e) => {
    clearSelection();
    TP_SETTINGS.SideEntrance = SideEntranceInput.value;
});
FreezeTimeInput.addEventListener("change", (e) => {
    clearSelection();
    TP_SETTINGS.FreezeTime = FreezeTimeInput.value;
});
DimOutsideButton.addEventListener("click", (e) => {
    clearSelection();
    TP_SETTINGS.Dimension = "Outside";
    DimOutsideButton.style.backgroundColor = "white";
    DimOutsideButton.style.color = "black";

    DimInsideButton.style.backgroundColor = "transparent";
    DimInsideButton.style.color = "white";
});
DimInsideButton.addEventListener("click", (e) => {
    clearSelection();
    TP_SETTINGS.Dimension = "Inside";
    DimInsideButton.style.backgroundColor = "white";
    DimInsideButton.style.color = "black";

    DimOutsideButton.style.backgroundColor = "transparent";
    DimOutsideButton.style.color = "white";
});
IPLButtonAdd.addEventListener("click", (e) => {
    clearSelection();
    if(!IPLNameInput.value) return;
    if(IPLNameInput.value.includes(',')) return;
    if(TP_SETTINGS.IPLArray.includes(IPLNameInput.value)) return;

    mp.trigger("TeleportEditor:RequestIPL", IPLNameInput.value);

    TP_SETTINGS.IPLArray.push(IPLNameInput.value);
    updateIPLList();

    IPLNameInput.value = "";
});

function updateIPLList(){
    IPLItemsList.innerHTML = ``;
    for(let i = 0 ; i < TP_SETTINGS.IPLArray.length ; i++){
        IPLItemsList.innerHTML += `<div id="${TP_SETTINGS.IPLArray[i]}" class="ipl-item">
        <p>${TP_SETTINGS.IPLArray[i]}</p>
        <button onclick="removeIPL('${TP_SETTINGS.IPLArray[i]}')">Remove</button>
        </div>`;
    }

}

function removeIPL(name){
    if(document.getElementById(name))
        document.getElementById(name).remove();

    if(TP_SETTINGS.IPLArray.includes(name))
        TP_SETTINGS.IPLArray.splice(TP_SETTINGS.IPLArray.indexOf(name), 1);

    mp.trigger("TeleportEditor:UnloadIPL", name);
}

var TopButtonsetNew = document.getElementById("top_buttonset_create");
var TopButtonsetEdit = document.getElementById("top_buttonset_edit");

var CreateTPButton = document.getElementById("teleport_finish_create_button");
var EditTPButton = document.getElementById("teleport_finish_edit_button");
var DeleteTPButton = document.getElementById("teleport_finish_delete_button");

TPButtonFrom.addEventListener("click", (e) => {
    clearSelection();
    mp.trigger("TeleportEditor:TeleportToPosition", TP_SETTINGS.FromPosition.x, TP_SETTINGS.FromPosition.y, TP_SETTINGS.FromPosition.z, TP_SETTINGS.FromPosition.h);
});
TPButtonTo.addEventListener("click", (e) => {
    clearSelection();
    mp.trigger("TeleportEditor:TeleportToPosition", TP_SETTINGS.ToPosition.x, TP_SETTINGS.ToPosition.y, TP_SETTINGS.ToPosition.z, TP_SETTINGS.ToPosition.h);
});
TPButton.addEventListener("click", (e) => {
    clearSelection();
    if(!InputTPX.value || !InputTPY.value || !InputTPZ.value) return;
    mp.trigger("TeleportEditor:TeleportToPosition", InputTPX.value, InputTPY.value, InputTPZ.value, 0);
});
NoclipButton.addEventListener("click", (e) => {
    clearSelection();
    mp.trigger("TeleportEditor:ToggleNoclip");
});
SetFromButton.addEventListener("click", (e) => {
    clearSelection();
    mp.trigger("TeleportEditor:RequestSetFromPosition");
});
SetToButton.addEventListener("click", (e) => {
    clearSelection();
    mp.trigger("TeleportEditor:RequestSetToPosition");
});
CreateTPButton.addEventListener("click", (e) => {
    clearSelection();
    mp.trigger("TeleportEditor:CreateNewTeleport", JSON.stringify(TP_SETTINGS));
});
EditTPButton.addEventListener("click", (e) => {
    clearSelection();
    mp.trigger("TeleportEditor:EditTeleport", EDITING_TELEPORT_ID, JSON.stringify(TP_SETTINGS));
});
DeleteTPButton.addEventListener("click", (e) => {
    clearSelection();
    mp.trigger("TeleportEditor:DeleteTeleport", EDITING_TELEPORT_ID);
});

var EDITING_TELEPORT_ID = -1;
function existingTeleportClick(isEdit, tpID){
    clearSelection();
    mp.trigger("TeleportEditor:SearchResultAction", isEdit, tpID);   
    EDITING_TELEPORT_ID = tpID;
}
CloseButton.addEventListener("click", (e) => {
    clearSelection();
    mp.trigger("TeleportEditor:Close");
});

mp.events.add("SetFromPosition", (x, y, z, h) => {
    TP_SETTINGS.FromPosition = {x:Number(x), y:Number(y), z:Number(z), h:Number(h)};
    FromDisplayPos.innerHTML = `X:${Number(x).toFixed(2)} Y:${Number(y).toFixed(2)} Z:${Number(z).toFixed(2)} H:${Number(h).toFixed(2)}`;
});
mp.events.add("SetToPosition", (x, y, z, h) => {
    TP_SETTINGS.ToPosition = {x:Number(x), y:Number(y), z:Number(z), h:Number(h)};
    ToDisplayPos.innerHTML = `X:${Number(x).toFixed(2)} Y:${Number(y).toFixed(2)} Z:${Number(z).toFixed(2)} H:${Number(h).toFixed(2)}`;
});
mp.events.add("LoadTeleports", (tpData) => {
    const TPData = JSON.parse(tpData);
    TELEPORTS_LIST = TPData;

    updateExistingTeleportsList();        
});
mp.events.add("SetEditTeleport", (tpData) => {
    openTeleportEditorFromClient(JSON.parse(tpData));
});