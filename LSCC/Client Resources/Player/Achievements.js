var global = require('./globals');
  const {TranslateMessage: Translate} = require('./Translator');
class Achievement{
    constructor(ID, name, description, rewardXP, rewardPoints, unlocked, background, time){
        this.ID = ID;
        this.name = name;
        this.description = description;
        this.rewardXP = rewardXP;
        this.rewardPoints = rewardPoints;
        this.unlocked = unlocked;
        this.background = background;
        this.time = time;
    }
}
class DailyChallenge{
    constructor(ID, description, rewardXP, rewardPoints, expireDate, completed, target, progress, type, item){
        this.ID = ID;
        this.description = description;
        this.rewardXP = rewardXP;
        this.rewardPoints = rewardPoints;          
        this.expireDate = expireDate;
        this.completed = completed;
        this.target = target;
        this.progress = progress;
        this.type = type;
        this.item = item;
    }
}
/*class WeeklyChallenge{
    constructor(ID, description, rewardXP, rewardPoints, expireDate){
        this.ID = ID;
        this.description = description;
        this.rewardXP = rewardXP;
        this.rewardPoints = rewardPoints;       
        this.expireDate = expireDate;
    }
}*/

let Achievements = [];
let DailyChallenges = [];
//let WeeklyChallenges = [];

mp.events.add("addAchievement", (id, name, desc, rXP, rPts, isUnlocked, bg, time) => {
    let a = new Achievement(id, name, desc, rXP, rPts, isUnlocked, bg, (isUnlocked ? time : undefined));
    Achievements.push(a);
});
mp.events.add("addDailyChallenge", (id, desc, rXP, rPts, expireDate, completed, target, progress, type, item) => {
    let c = new DailyChallenge(id,  desc, rXP, rPts, expireDate, completed, target, progress, type, item);
    DailyChallenges.push(c);
});
mp.events.add("clearDailyChallenges", () => {
    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.profileTab.$refs.challengesTab.categories = []`);
    DailyChallenges = [];
})

/*
mp.events.add("addWeeklyChallenge", (id, name, desc, rXP, rPts, expireDate) => {
    let realDate = new Date(expireDate*1000);

    let c = new WeeklyChallenge(id, name, desc, rXP, rPts, realDate);
    WeeklyChallenges.push(c);
});
*/

function getchalbackgroundpic(cht){
    let bgshit = "package://Artwork/Achievements/no_shooting_win.png";
    switch(cht){
        case 0:{
            bgshit = "package://Artwork/Achievements/first_kill.png";
        }
        break;
        case 1:{
            bgshit = "package://Artwork/Achievements/first_vehicle.png";
        }
        break;
        case 2:{
            bgshit = "package://Artwork/Achievements/serial_killer.png";
        }
        break;
        case 3:{
            bgshit = "package://Artwork/Achievements/first_bankrob_cop.png";
        }
        break;
        case 4:{
            bgshit = "package://Artwork/Achievements/first_loadout.png";
        }
        break;
    }
    return bgshit;
}

let weekChalProg = 0;
let weekChalUnix = 0;

mp.events.add("addChallengesToMenu", (weekChallengeProgress, weekChallengeUnix) => {
    let json = `gm.$refs.mainMenu.$refs.profileTab.$refs.challengesTab.categories = [`;
    let chstr = "";   
    weekChalProg = Number(weekChallengeProgress);
    weekChalUnix = Number(weekChallengeUnix);

    var daily_challenges = [];
    for(let i = 0 ; i < DailyChallenges.length; i++){

        daily_challenges.push({
            name: "",
            description: DailyChallenges[i].description,
            progress: DailyChallenges[i].progress,
            goal: DailyChallenges[i].target,
            //background: getchalbackgroundpic(DailyChallenges[i].type)
        });
    }
    var challenges_object = [];
    challenges_object.push({
        name: "Weekly",
        expires: Number(new Date(Number(Date.now())+Number(weekChallengeUnix))),
        challenges: 
        [
            {
                name: "7-day Streak",
                description: 'Complete at least one daily challenge per day for 7 days in a row.',
                progress: weekChallengeProgress,
                goal: 7,
                //background: "package://Artwork/Achievements/reach_level_100.png"
            }
        ]
    },
    {
        name: "Daily",
        expires: Number(new Date(Number(Date.now()) + (DailyChallenges[i] != undefined ? Number(DailyChallenges[i].expireDate) : 3600))),
        challenges:daily_challenges
    }
    );
    mp.events.callRemote('ServerConsoleOutput', JSON.stringify(challenges_object));
    if(global.ServerUI) global.ServerUI.call('mainmenu_profile_challenges_set', challenges_object);
});
mp.events.add("advanceChallenge", (arrayID, cht, chi, chp) => {
    
    if(arrayID == 1)
    {
        let chunlock = DailyChallenges.find(a => a.type == cht && a.item == chi);
        if(!chunlock) return;

        DailyChallenges.find(a => a.type == cht && a.item == chi).progress = chp;
        
        if(global.ServerUI) global.ServerUI.execute(`if(gm.$refs.mainMenu.$refs.profileTab.$refs.challengesTab.categories[${arrayID}].challenges.find(chNig => chNig.description == "${chunlock.description}") != null){
            gm.$refs.mainMenu.$refs.profileTab.$refs.challengesTab.categories[${arrayID}].challenges.find(chNig => chNig.description == "${chunlock.description}").progress = ${chp};
        }`);
    }
    else{ // weekly
        if(global.ServerUI) global.ServerUI.execute(`if(gm.$refs.mainMenu.$refs.profileTab.$refs.challengesTab.categories[${arrayID}].challenges.find(chNig => chNig.name == "Daily Challenge 7-day streak") != null){
            gm.$refs.mainMenu.$refs.profileTab.$refs.challengesTab.categories[${arrayID}].challenges.find(chNig => chNig.name == "Daily Challenge 7-day streak").progress = ${chp};
        }`);
    }
});

mp.events.add("unlockChallenge", (cht, chi) => {
    let chunlock = DailyChallenges.find(a => a.type == cht && a.item == chi);
    if(!chunlock) return;

    DailyChallenges.find(a => a.type == cht && a.item == chi).completed = true;

    let rewardstr = "";
    if(chunlock.rewardXP > 0) rewardstr += `+${chunlock.rewardXP} XP`;
    if(chunlock.rewardPoints > 0){
        if(rewardstr == "") rewardstr += `+${chunlock.rewardPoints} points`;
        else rewardstr += `& +${chunlock.rewardPoints} points`;
    }

    let bgshit = getchalbackgroundpic(cht);

    if(global.ServerUI != null && global.ServerUI != undefined){
        
        if(global.ServerUI) global.ServerUI.execute(`toast('', '${chunlock.description}', 'info', 3000);`);
        //if(global.ServerUI) global.ServerUI.execute(`gm.sendAch("Challenge completed!", "${chunlock.description}", "${bgshit}", "${rewardstr}", 6000);`);
    }
    mp.game.audio.playSoundFrontend(-1, "PROPERTY_PURCHASE", "HUD_AWARDS", true);

    mp.events.callLocal("addChallengesToMenu", weekChalProg, weekChalUnix);
});

mp.events.add("sendAchievementNotify", (title, description, background, reward, time, playSound) => {
    if(global.ServerUI != null && global.ServerUI != undefined){
        
        if(global.ServerUI) global.ServerUI.execute(`toast('', '${title}', 'info', 3000);`);
        //if(global.ServerUI) global.ServerUI.execute(`gm.sendAch("${title}", "${description}", "${background}", "${reward}", ${time});`);
    }    
    if(playSound){
        mp.game.audio.playSoundFrontend(-1, "PROPERTY_PURCHASE", "HUD_AWARDS", true);
    }
});



mp.events.add("addAchievementsToMenu", () => {

    var achievements_object = [];
    for(let i = 0 ; i < Achievements.length; i++){

        var achObject = {
            "name":Achievements[i].name, 
            "description":Achievements[i].description, 
            "unlocked":Achievements[i].unlocked,
            "background":Achievements[i].background,
        };
        if(Achievements[i].time){
            achObject.unlockDate = Achievements[i].time;
        }
        achievements_object.push(achObject);
    }
    mp.events.callRemote("ServerConsoleOutput", JSON.stringify(achievements_object));
    if(global.ServerUI) global.ServerUI.call('mainmenu_profile_achievements_set', achievements_object);
});
mp.events.add("unlockAchievement", (achievementName) => {
    let achunlock = Achievements.find(a => a.name == achievementName);
    if(!achunlock) return;

    achunlock.unlocked = true;
    if(global.ServerUI) global.ServerUI.execute(`gm.$refs.mainMenu.$refs.profileTab.$refs.achievementsTab.achievements.find(a => a.name === "${achievementName}").unlocked = true`);

    let rewardstr = "";
    if(achunlock.rewardXP > 0) rewardstr += `+${achunlock.rewardXP} XP`;
    if(achunlock.rewardPoints > 0){
        if(rewardstr == "") rewardstr += `+${achunlock.rewardPoints} points`;
        else rewardstr += `& +${achunlock.rewardPoints} points`;
    }

    if(global.ServerUI != null && global.ServerUI != undefined){
        if(global.ServerUI) global.ServerUI.execute(`toast('', '${achunlock.name}', 'info', 3000);`);
        //TO-DO LATER: if(global.ServerUI) global.ServerUI.execute(`gm.sendAch("${achunlock.name}", "${achunlock.description}", "${achunlock.background}", "${rewardstr}", 6000);`);
    }
    mp.game.audio.playSoundFrontend(-1, "PROPERTY_PURCHASE", "HUD_AWARDS", true);

});

mp.events.add("updateAllAchievements", (unlockedAchievementsData) => {

    const real_achievements = JSON.parse(unlockedAchievementsData).map(ach => {
        return {...ach, unlockDate:Number(ach.unlockDate)};
    });
    if(global.ServerUI) global.ServerUI.call('mainmenu_profile_achievements_set', real_achievements);
});