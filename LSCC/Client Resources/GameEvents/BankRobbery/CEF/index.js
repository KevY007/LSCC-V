const DUMMY_OBJECTIVES = [
    "Two ~b~heist leaders ~w~are in the process of stealing the ~g~money~w~.",
    "Your task is to escort the ~b~heist leaders ~w~to the ~y~getaway vehicles~w~ first.",
    "Once in the vehicles, you must escort the ~b~heist leaders ~w~to the ~y~hideout location~w~.",
    "Protect the ~b~heist leaders ~w~by all means necessary - if they die, you lose!",
    "The ~r~police ~w~are on their way.",
]
const POLICE_DUMMY_OBJECTIVES = [
    "You're responding to a call regarding an ongoing robbery at the ~y~Pacific Standard Bank~w~.",
    "Excercise caution - it's reported that XX ~r~armed robbers ~w~are inside the bank.",
    "Deadly force is authorized - neutralize the ~r~armed robbers ~w~using any means necessary.",
    "It is said that two ~r~heist leaders ~w~are taking the ~g~money ~w~- prioritize eliminating them to win.",
    "Do not let the ~r~heist leaders ~w~take the ~g~money ~w~to their hideout!",
]
const DUMMY_TEAM = 0;
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

setObjectives(DUMMY_OBJECTIVES);
setTeam(DUMMY_TEAM);

// mp.events.add("set_objectives", (array) => {
//     const realArray = JSON.parse(array);
//     setObjectives(realArray);
// });