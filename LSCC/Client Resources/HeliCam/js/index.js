var CrosshairDOM = undefined;
const TargetSound = new Audio("./audio/target.mp3");
const SelectSound = new Audio("./audio/select.mp3");

var CurrentView = 1;
var LightToggle = true;

document.addEventListener("DOMContentLoaded", () => {
    CrosshairDOM = document.getElementById("crosshair");
});

function SetView(value){
    document.getElementById(`view_${CurrentView}`).classList = "panel-item-unselected";
    CurrentView = value;
    document.getElementById(`view_${CurrentView}`).classList = "panel-item-selected";

    if(CurrentView === 1){
        document.getElementsByTagName("body")[0].style.backdropFilter = `saturate(50%);`;
    }
    else{
        document.getElementsByTagName("body")[0].style.backdropFilter = `saturate(0%);`;
    }
}

function ToggleLight(value){
    if(value){
        document.getElementById("light_enabled").className = "panel-item-selected";
        document.getElementById("light_disabled").className = "panel-item-unselected";
    }
    else{
        document.getElementById("light_enabled").className = "panel-item-unselected";
        document.getElementById("light_disabled").className = "panel-item-selected";
    }
}

function UpdateDataPanel(name, passengers, location, speed){
    document.getElementById("vehicleName").innerHTML = name;
    document.getElementById("vehiclePassengers").innerHTML = passengers.toString();
    //document.getElementById("vehicleFuel").innerHTML = fuel;
    document.getElementById("vehicleSpeed").innerHTML = speed;
    document.getElementById("vehicleLocation").innerHTML = location;
}

function ToggleFocusPanel(value){
    if(value){
        document.getElementById("focusPanel").className = "focused-panel";
    }
    else{
        document.getElementById("focusPanel").className = "invisible";
    }
}

function logToConsole(text){
    console.log(text);
}

function TogglePlayerFocusPanel(value){
    if(value){
        document.getElementById("focusPanelPlayer").className = "focused-panel";
    }
    else{
        document.getElementById("focusPanelPlayer").className = "invisible";
    }
}
function UpdatePlayerDataPanel(health, name){
    document.getElementById("playerHealth").innerHTML = `${health.toString()}BPM`;
    document.getElementById("playerName").innerHTML = name;

    document.getElementById("heartbeatGif").className = (health > 0 ? "heartbeat-gif" : "heartbeat-none");
}

function SetZoomValue(value){
    document.getElementById("knob").style.left = `${value}%`;
    document.getElementById("zoomValue").innerHTML = `${value}%`;
}

function SetCrosshair(type){
    switch(type){
        case 'crosshair':
            TargetSound.pause();
            TargetSound.currentTime = 0.0;

            document.getElementById("lmb_tip").className = "invisible";
            CrosshairDOM.className = 'crosshair';
            CrosshairDOM.src = './img/crosshair.png';
            break;
        case 'target':
            TargetSound.playbackRate = 0.6;
            TargetSound.loop = true;
            TargetSound.volume = 0.5;
            TargetSound.play();

            document.getElementById("lmb_tip").className = "lmb-tip";
            CrosshairDOM.className = 'target';
            CrosshairDOM.src = './img/target.png';
            break;
        case 'lock':
            TargetSound.pause();
            TargetSound.currentTime = 0.0;

            SelectSound.volume = 0.5;
            SelectSound.play();

            document.getElementById("lmb_tip").className = "invisible";
            CrosshairDOM.className = 'target-locked';
            CrosshairDOM.src = './img/target.png';
            break;
    }
}