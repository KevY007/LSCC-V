var SuspectsCount = 0;
var PlayerName = "codename";

const Scale = 20.1789759578;
const ZeroY = 416;
const ZeroX = 283;

var StingerScreenDOM;
var StingerArea;
var StingerStreet;
var StingerPage;
var StingerHelp;


var last104sound = 1;
var currentSpikeNumber = 1;

/*
SPIKE STAGES:
1 => White Icon => Spikes have just been placed down.
2 => Yellow Icon => 15 seconds have passed, police peds are spawned next to the spikes.
3 => Green Icon => Spikes are currently deployed at that location.
4 => Dark-red Icon => Spikes were destroyed, probably because suspects' vehicle drove over them.
*/

const SelectSound = new Audio("../audio/select.mp3"); 
const TenFourSound = new Audio("../audio/ten_four.mp3"); 
const TenFourSound2 = new Audio("../audio/ten_four2.mp3");

var MapDOM = null;
var HolderDOM = null;
var MapContentDOM = null;

var zooming = false;
var isZoomKeyDown = false;
var zoomInterval = null;

var mapViewButton = null;
var stingerButton = null;

var CurrentOption = "map";

var currentZoomNumber = 1.0;

var isPanKeyDown = false;
var panInterval = null;
var panDirection = 'none';

var MaxZoomValue = 4.0;
var ZoomSpeed = 0.02;
var PanSpeed = 3;
var CurrentMap = 'atlas';

function UpdateMapSource(){
    if(MapDOM.src.includes('hq'))
        MapDOM.src = `./img/${CurrentMap}_map_hq.png`;
    else MapDOM.src = `./img/${CurrentMap}_map.png`;

    let ScreenDOM = document.getElementsByClassName("mdc-screen-map")[0];
    if(CurrentMap == 'atlas')
        ScreenDOM.style.backgroundColor = '#0FA8D2'; 
    else ScreenDOM.style.backgroundColor = '#143D6B';  
}

document.addEventListener("DOMContentLoaded", () => {
    StingerScreenDOM = document.getElementById("mdc_stinger");
    StingerArea = document.getElementById("stinger_area");
    StingerStreet = document.getElementById("stinger_street");
    StingerPage = document.getElementById("stinger_location_pages");
    StingerHelp = document.getElementById("stinger_help");
})

function UpdateStingerDisplay(id){
    StingerScreenDOM.style.backgroundImage = `url('./img/spikes/sp${id}.png')`;
    let stinger = GetStinger(id);
    if(stinger.id != -1){
        StingerStreet.innerHTML = stinger.street;
        StingerArea.innerHTML = stinger.area;
        StingerPage.innerHTML = `${id}/${Stingers.length}`;

        StingerHelp.innerHTML = `Press <strong>[ENTER]</strong> to request deployment.`;
    }
}

document.addEventListener("keydown", function(e){    
    if(CurrentOption != "stinger") return;
    switch(e.keyCode){
        case 13: 

        //mp.trigger("mdcRequestDeployStinger", currentSpikeNumber);

        break;   
        case 37:
            {
                (currentSpikeNumber > 1 ? currentSpikeNumber-- : currentSpikeNumber = 15);
                UpdateStingerDisplay(currentSpikeNumber);
                SelectSound.play();
            }
        break;
        case 39:
            {
                (currentSpikeNumber < 15 ? currentSpikeNumber++ : currentSpikeNumber = 1);
                UpdateStingerDisplay(currentSpikeNumber);
                SelectSound.play();
            }
        break;
    }
});
document.addEventListener("keydown", function(e){    
    if(CurrentOption != "map") return;
    switch(e.keyCode){
        case 13: 
                let oldSelected = document.getElementById(`view-${CurrentMap}`);

                if(CurrentMap == 'atlas') CurrentMap = 'satellite';
                else CurrentMap = 'atlas';

                let newSelected = document.getElementById(`view-${CurrentMap}`);

                newSelected.className = "map-view-selected";
                oldSelected.className = "map-view";
                SelectSound.play();

                UpdateMapSource();       
            break;
        case 16:
            if(!isZoomKeyDown)
            {
                zooming = true;
                zoomInterval = setInterval(() => {zoomHandler(ZoomSpeed)}, 5);
                isZoomKeyDown = true;
            }
            break;
        case 17:
            if(!isZoomKeyDown)
            {
                zooming = false;
                zoomInterval = setInterval(() => {zoomHandler(ZoomSpeed*(-1))}, 5);
                isZoomKeyDown = true;
            }
            break;  
        case 37:
            if(!isPanKeyDown){
                panDirection = 'left';
                panInterval = setInterval(() => {panHandler(panDirection)}, 5);
                isPanKeyDown = true;
            }
            break;
        case 38:
            if(!isPanKeyDown){
                panDirection = 'up';
                panInterval = setInterval(() => {panHandler(panDirection)}, 5);
                isPanKeyDown = true;
            }
            break;
        case 39:
            if(!isPanKeyDown){
                panDirection = 'right';
                panInterval = setInterval(() => {panHandler(panDirection)}, 5);
                isPanKeyDown = true;
            }
            break;
        case 40:
            if(!isPanKeyDown){
                panDirection = 'down';
                panInterval = setInterval(() => {panHandler(panDirection)}, 5);
                isPanKeyDown = true;
            }
            break;
    }
});

// Most northern point: X 80, Y -212
// South: X 28 Y 372

function getPictureCoords(x, y){
    x = x/Scale;
    y = y/Scale;
    return {x: (ZeroX + x)-500 , y: (ZeroY - y)-500};
}

function setBlipPosition(element, x, y){ 
    var coords = getPictureCoords(x, y);
    element.style.top = (coords.y) + 'px';
    element.style.left = (coords.x) + 'px';
}

function updateMapBlipScales(zoom_level){
    // Push any blips into the `blips` variable if they should be scaled with map zoom.
    // This is done to keep a constant scale of some blips. Some other blips [e.g. fugitive zones] don't need to be scaled.
    var blips = document.getElementsByClassName("blip-police");
    let scale = ((5/(zoom_level * 8)/70)+0.001);
    for(var i = 0 ; i < blips.length ; i++){
        blips[i].style.transform = `scale(${scale})`;
    }

    blips = document.getElementsByClassName("blip-spike");
    scale = ((5/(zoom_level * 8)/70)+0.01);
    for(var i = 0 ; i < blips.length ; i++){
        blips[i].style.transform = `scale(${scale})`;
    }   
}

document.addEventListener("DOMContentLoaded", () => {
    mapViewButton = document.getElementById("option_map_view");
    stingerButton = document.getElementById("option_stinger");
});

document.addEventListener("keypress", function(e){    
    let key = e.key.toLowerCase();
    switch(key){
        case "r": // Roadblock View
        if(CurrentOption != "stinger"){
            stingerButton.className = "program-tab-pressed";
            mapViewButton.className = "program-tab";
            CurrentOption = "stinger";

            document.getElementById("miniscreen_desc").innerHTML = "Request stingers to be deployed at a location.\
            Takes 30s to deploy.";

            document.getElementById("mdc_map").style.display = "none";
            document.getElementById("mdc_stinger").style.display = "flex";
            UpdateStingerDisplay(currentSpikeNumber);
            SelectSound.play();
        }
        break;
        case "m": // Map View
        if(CurrentOption != "map"){
            mapViewButton.className = "program-tab-pressed";
            stingerButton.className = "program-tab";
            CurrentOption = "map";

            document.getElementById("miniscreen_desc").innerHTML = "Approximate suspects' location & active stingers are displayed."; 
            document.getElementById("mdc_stinger").style.display = "none";          
            document.getElementById("mdc_map").style.display = "flex";
            SelectSound.play();
        }
        break;
    }
});
document.addEventListener("keyup", function(e){
    if(CurrentOption != "map") return;
    switch(e.keyCode){
        case 16:
        case 17:
            clearInterval(zoomInterval);
            isZoomKeyDown = false;
            break;    
        case 37:
        case 38:
        case 39:
        case 40:
            clearInterval(panInterval);
            isPanKeyDown = false;
            break;
    }   
})

document.addEventListener("DOMContentLoaded", () => {
    MapDOM = document.getElementById("map_display");
    HolderDOM = document.getElementById("mdc-content");
    MapContentDOM = document.getElementById("map_content");

    updateMapBlipScales(currentZoomNumber);
});

function panHandler(direction){
    let current_left = window.getComputedStyle(MapDOM,null).getPropertyValue("left");
    let leftNumber = Number(current_left.replace('px', ''));
    let current_top = window.getComputedStyle(MapDOM,null).getPropertyValue("top");
    let topNumber = Number(current_top.replace('px', ''));

    switch(direction){
        case 'left':
            var bounds = (currentZoomNumber-1)*(300);
            if(currentZoomNumber == 1.0) bounds = 0;

            if(leftNumber < bounds){
                MapDOM.style.left = (leftNumber+PanSpeed) + 'px';
                MapContentDOM.style.left = (leftNumber+PanSpeed) + 'px';
            }
            break;
        case 'right':;
            var bounds = (currentZoomNumber-1)*(-300);
            if(currentZoomNumber == 1.0) bounds = 0;


            if(leftNumber > bounds){
                MapDOM.style.left = (leftNumber-PanSpeed) + 'px';
                MapContentDOM.style.left = (leftNumber-PanSpeed) + 'px';
            }
            break;
        case 'up':
            var bounds = (currentZoomNumber-1)*(300);
            if(currentZoomNumber == 1.0) bounds = 0;

            if(topNumber < bounds){
                MapDOM.style.top = (topNumber+PanSpeed) + 'px';
                MapContentDOM.style.top = (topNumber+PanSpeed) + 'px';
            }
            break;
        case 'down':
            var bounds = (currentZoomNumber-1)*(-300);
            if(currentZoomNumber == 1.0) bounds = 0;

            if(topNumber > bounds){
                MapDOM.style.top = (topNumber-PanSpeed) + 'px';
                MapContentDOM.style.top = (topNumber-PanSpeed) + 'px';
            }
            break;
    }
}

function zoomHandler(value){
    let currentZoom = HolderDOM.getBoundingClientRect().width / HolderDOM.offsetWidth;
    
    if((currentZoom <= 1.0 && value < 0) || (currentZoom >= MaxZoomValue && value > 0)) return;
    currentZoom += value;
    currentZoomNumber = currentZoom;

    HolderDOM.style.transform = `scale(${currentZoom})`;

    updateMapBlipScales(currentZoom);
}

function ToggleHQMap(toggle){
    if(toggle){
        MapDOM.src = `./img/${CurrentMap}_map_hq.png`;
        MaxZoomValue = 10.0;
        ZoomSpeed = 0.04;
        PanSpeed = 1;
    }
    else{ 
        MapDOM.src = `./img/${CurrentMap}_map.png`;
        MaxZoomValue = 4.0;
        ZoomSpeed = 0.02;
        PanSpeed = 2;
    }
}

function RemoveBlip(ID, type){
    let blipElement = document.getElementById(`${type}_${ID}`);
    if(blipElement){
        blipElement.remove();
    }
}
function RemoveAllBlips(){
    MapContentDOM.innerHTML = "";
}

function UpdateBlip(ID, type, xPos, yPos){
    let blipElement = document.getElementById(`${type}_${ID}`);
    if(blipElement){
        setBlipPosition(blipElement, xPos, yPos);
    }
}

function AddBlip(ID, type){
    let blip = `${type}_${ID}`;
    if(document.getElementById(blip)){
        return;
    }
    let blipData = {
        sourceName: "n/a",
        className: "n/a"
    };
    switch(type){
        case "police":
            blipData.sourceName = "police_blip.gif";
            blipData.className = "blip-police";
        break;
        case "fugitivezone":
            blipData.sourceName = "fugitive_circle.gif";
            blipData.className = "fugi-circle";
        break;
    }

    let blipElement = document.createElement("img");
    blipElement.src = `./img/${blipData.sourceName}`;
    blipElement.className = blipData.className;
    blipElement.id = blip;

    MapContentDOM.appendChild(blipElement);
    updateMapBlipScales(currentZoomNumber);
}

function AddSpikeBlip(ID, stage){
    let blip = `spike_${ID}`;
    if(document.getElementById(blip)){
        return;
    }
    let blipData = {
        sourceName: `spikes_stage${stage}.png`,
        className: "blip-spike"
    };

    let blipElement = document.createElement("img");
    blipElement.src = `./img/${blipData.sourceName}`;
    blipElement.className = blipData.className;
    blipElement.id = blip;

    MapContentDOM.appendChild(blipElement);
    updateMapBlipScales(currentZoomNumber);
}
function UpdateSpikeStage(ID, stage){
    let blip = `spike_${ID}`;
    let blipElement = document.getElementById(blip);
    if(blipElement){
        blipElement.src = `./img/spikes_stage${stage}.png`;
    }
}

function UpdateMiniscreenStatus(item, value){
    switch(item){
        case "name": PlayerName = value;
        break;
        case "suspects": SuspectsCount = value;
        break;
    }

    document.getElementById("miniscreen_status").innerHTML = 
    `
        SIGNAL: [///]<br>
        OFFICER: ${PlayerName}<br>
        SUSPECTS: ${SuspectsCount}
    `;
}

function SetStingerError(msg){
    StingerHelp.innerHTML = msg;
}
function SetStingerSuccess(id, msg){
    if(last104sound == 1){
        TenFourSound2.play();
        last104sound = 2;
    }
    else if(last104sound == 2){
        TenFourSound.play();
        last104sound = 1;            
    }
    StingerHelp.innerHTML = msg;

    AddSpikeBlip(id, 1);
}

var Stingers = [
    {
        id: 1,
        area: "Vinewood Hills",
        street: "Los Santos Freeway"
    },
    {
        id: 2,
        area: "Grand Senora Desert",
        street: "Route 68"
    }, 
    {
        id: 3,
        area: "Braddock Pass",
        street: "Senora Freeway"
    },     
    {
        id: 4,
        area: "Paleto Bay",
        street: "Procopio Drive"
    },      
    {
        id: 5,
        area: "Chiliad Mountain State Wilderness",
        street: "Freeway"
    },      
    {
        id: 6,
        area: "North Chumash",
        street: "Freeway"
    },         
    {
        id: 7,
        area: "Chumash",
        street: "Braham Canyon Drive"
    },           
    {
        id: 8,
        area: "Del Perro",
        street: "Bay City Incline"
    },            
    {
        id: 9,
        area: "Downtown",
        street: "Del Perro Freeway"
    },           
    {
        id: 10,
        area: "El Burro Heights",
        street: "Del Perro Freeway"
    },          
    {
        id: 11,
        area: "Elysian Island",
        street: "Elysian Fields Freeway"
    },       
    {
        id: 12,
        area: "La Puerta",
        street: "Olympic Freeway"
    },     
    {
        id: 13,
        area: "Downtown",
        street: "Integrity Way"
    },    
    {
        id: 14,
        area: "Palomino Highlands",
        street: "Olympic Freeway"
    },    
    {
        id: 15,
        area: "Tataviam Mountains",
        street: "Olympic Freeway"
    }
];

function AddStinger(id, areaName, streetName){
    Stingers.push({
        id: id,
        area: areaName,
        street: streetName
    });
}
function RemoveStinger(id){
    let stinger = Stingers.find(s => s.id == id);
    if(stinger){
        let idx = Stingers.indexOf(stinger);
        Stingers.splice(idx, 1);
    }
}
function GetStinger(id){
    let stinger = Stingers.find(s => s.id == id);
    if(stinger) return stinger;
    else{
        return {
            id:-1, area:"", street:""
        };
    }
}