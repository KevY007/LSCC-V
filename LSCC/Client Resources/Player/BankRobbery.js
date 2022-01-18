const ROBBERY_TIME = 60;

let robTimeLeft = ROBBERY_TIME;
let robTimer = null;

mp.events.add("StartBankRobberyTimer", () => {
    robTimeLeft = ROBBERY_TIME;
    robTimer = setInterval(() => {
        robTimeLeft--;
        if(robTimeLeft <= 0){
            // continue the robbery
        }
        else if(robTimeLeft == 45){

        }
        else if(robTimeLeft == 30){

        }
        else if(robTimeLeft == 15){
            
        }
    }, 1000);
});