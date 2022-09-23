var hScreen;
var hintImg;
var container;

let lastDirection = "down";
let lastX, lastY;

let directions = [];
let directionIdx = 0;
let zIdx = 69;

const ElectricSound = new Audio("./js/electric.mp3");
const SuccessSound = new Audio("./js/success_electric.mp3");

document.addEventListener("keyup", function(e) {
    if(e.which == 40) // down
    {
        TryAdvance("down");
    }
    else if(e.which == 37) // left
    {
        TryAdvance("left");
    }
    else if(e.which == 39) // right
    {
        TryAdvance("right");
    }
});
function TryAdvance(direction){
    let stepSound = ElectricSound.cloneNode();
    stepSound.play();
    if(direction == directions[directionIdx]){
        if(direction == "down"){
            hScreen.children[directionIdx].style.zIndex = zIdx;
            
            if(directionIdx == 0)
                hScreen.children[directionIdx].style.height = "120px";
            else if(directionIdx > 0 && directionIdx < directions.length-1)
                hScreen.children[directionIdx].style.height = "125px";
        }
        else{
            hScreen.children[directionIdx].style.zIndex = zIdx;
        }

        hScreen.children[directionIdx].style.backgroundColor = "yellow";
        directionIdx++;
        if(directionIdx < 7)
            hintImg.src = `./js/${directions[directionIdx]}.png`;
        else{
            SuccessSound.play();
            container.style.display = "none";
            mp.trigger("CS_HotwireResult", true);
        }
    }
    else{
        mp.trigger("CS_HotwireResult", false);
    }
}

document.addEventListener("DOMContentLoaded", () => {
    hScreen = document.getElementById("screen");
    container = document.getElementById("container");
    hintImg = document.getElementById("hint-img");
    generateHotwire();
});
function generateHotwire(){
    container.style.display = "flex";
    hScreen.innerHTML = "";
    directions = [];
    directionIdx = 0;
    let offsetX = getRandomInt(150, 350);
    let offsetY = 100;
    lastX = offsetX;

    let lastDir = "NULL";
    hScreen.innerHTML += `<div class="wall-vert" style="left:${offsetX}px;"></div>`;
    directions.push("down");

    hintImg.src = "./js/down.png";

    for(let i = 0; i < 6 ; i++){
        if(lastDir == "NULL"){
            let dir = (getRandomInt(0, 100) > 50 ? "left" : "right");
            if(dir == "left"){
                lastDir = dir;
                hScreen.innerHTML += `<div class="wall-hor" style="left:${offsetX-75}px;top:${offsetY}px;"></div>`;
                offsetY += 25;
                offsetX -= 75;
                directions.push(lastDir);
            }
            else{
                lastDir = dir;
                hScreen.innerHTML += `<div class="wall-hor" style="left:${offsetX}px;top:${offsetY}px;"></div>`;
                offsetY += 25;
                offsetX += 75;
                directions.push(lastDir);
            }
        }
        else{
            if(i % 2 != 0){
                if(i != 5){
                    hScreen.innerHTML += `<div class="wall-vert" style="left:${offsetX}px;top:${offsetY}px;"></div>`;
                    offsetY += 100;
                    directions.push("down");
                }
                else{
                    hScreen.innerHTML += `<div class="wall-vert" style="left:${offsetX}px;top:${offsetY}px;height:130px;"></div>`;
                    offsetY += 100;
                    directions.push("down");
                }
              
            }
            else{
                let dir = (getRandomInt(0, 100) > 50 ? "left" : "right");
                if(dir == "left"){
                    if(offsetX-75 < 20) {
                        lastDir = "right";
                        hScreen.innerHTML += `<div class="wall-hor" style="left:${offsetX}px;top:${offsetY}px;"></div>`;
                        offsetY += 25;
                        offsetX += 75;
                        directions.push(lastDir);
                    }
                    else{
                        lastDir = dir;
                        hScreen.innerHTML += `<div class="wall-hor" style="left:${offsetX-75}px;top:${offsetY}px;"></div>`;
                        offsetY += 25;
                        offsetX -= 75;
                        directions.push(lastDir);
                    }
       
                }
                else{
                    if(offsetX + 75 > 480){
                        lastDir = "left";
                        hScreen.innerHTML += `<div class="wall-hor" style="left:${offsetX-75}px;top:${offsetY}px;"></div>`;
                        offsetY += 25;
                        offsetX -= 75;
                        directions.push(lastDir);
                    }
                    else{
                        lastDir = dir;
                        hScreen.innerHTML += `<div class="wall-hor" style="left:${offsetX}px;top:${offsetY}px;"></div>`;
                        offsetY += 25;
                        offsetX += 75;
                        directions.push(lastDir);
                    }
                
                }
            }
        }
    }
}

function getRandomInt(min, max) {
    min = Math.ceil(min);
    max = Math.floor(max);
    return Math.floor(Math.random() * (max - min) + min); // The maximum is exclusive and the minimum is inclusive
  }