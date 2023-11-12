
const DUMMY_TEAM = 1;
var OBJECTIVES = [];

function setObjectives(objective_array){
    var objectiveList = document.getElementById("objective_list");
    if(!objectiveList) return;

    objectiveList.innerHTML = "";
    OBJECTIVES = [];
    for(let i = 0 ; i < objective_array.length ; i++){
        const obj = objective_array[i];
        if(i === 2)
            objectiveList.innerHTML += `<li id="objective_${i}" class="objective-item active">${formatColors(obj)}</li>`
        else 
            objectiveList.innerHTML += `<li id="objective_${i}" class="objective-item">${formatColors(obj)}</li>`
        OBJECTIVES.push(obj);
    }
}


function formatColors(string){
    var str = string;
    str = str.replaceAll("~w~", '<span style="color:white; font-weight:400;">');
    str = str.replaceAll("~y~", '<span style="color:yellow; font-weight:500;">');
    str = str.replaceAll("~g~", '<span style="color:green; font-weight:500;">');
    str = str.replaceAll("~b~", '<span style="color:dodgerblue; font-weight:500;">');
    str = str.replaceAll("~r~", '<span style="color:red; font-weight:500;">');
    return str;
}

function setTeam(team){
    if(team == 0){
        document.getElementById("team_display").innerHTML = `<span class="fugitive">FUGITIVE TEAM</span>`
    }
    else document.getElementById("team_display").innerHTML = `<span class="police">POLICE TEAM</span>`
}

function showObjectives(){
    const teamDisplay = document.getElementById("team_display");
    const objectiveDisplay = document.getElementById("objective_list");

    teamDisplay.classList.add("fadeIn");
    objectiveDisplay.classList.add("fadeIn");
    teamDisplay.classList.remove("invisible");
    objectiveDisplay.classList.remove("invisible");
}
function hideObjectives(){
    const teamDisplay = document.getElementById("team_display");
    const objectiveDisplay = document.getElementById("objective_list");

    teamDisplay.classList.remove("fadeIn");
    objectiveDisplay.classList.remove("fadeIn");    

    teamDisplay.classList.add("fadeOut");
    objectiveDisplay.classList.add("fadeOut");
    setTimeout(() => {
        teamDisplay.classList.add("invisible");
        objectiveDisplay.classList.add("invisible");    
        teamDisplay.classList.remove("fadeOut");
        objectiveDisplay.classList.remove("fadeOut"); 
    }, 1001);
    teamDisplay.classList.remove("invisible");
    objectiveDisplay.classList.remove("invisible");    
}

function toggleBlackBG(toggle){
    var wrapper = document.getElementById("wrapper");
    if(toggle == true){
        wrapper.classList.remove("fadeOut");
        wrapper.classList.add("fadeIn");
        setTimeout(() => {
            wrapper.style.opacity = 1.0;
        }, 1001);
    }
    else{
        wrapper.classList.remove("fadeIn");
        wrapper.classList.add("fadeOut");
        setTimeout(() => {
            wrapper.style.opacity = 0.0;
        }, 1001);
    }
}

mp.events.add("set_objectives", (array) => {
    const realArray = JSON.parse(array);
    setObjectives(realArray);
});
mp.events.add("set_team", (team) => {
    setTeam(team);
});
mp.events.add("fadeoutGameScreen", (toggle) => {
    toggleBlackBG(true);
})
mp.events.add("toggle_objectives", (toggle) => {
    if(toggle == true) showObjectives();
    else hideObjectives();

    console.log("Toggle objectives set to " + toggle.toString());
});