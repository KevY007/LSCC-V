class Achievement{
    constructor(ID, name, description, rewardXP, rewardPoints, unlocked, background){
        this.ID = ID;
        this.name = name;
        this.description = description;
        this.rewardXP = rewardXP;
        this.rewardPoints = rewardPoints;
        this.unlocked = unlocked;
        this.background = background;
    }
}
class DailyChallenge{
    constructor(ID, description, rewardXP, rewardPoints, expireDate){
        this.ID = ID;
        this.description = description;
        this.rewardXP = rewardXP;
        this.rewardPoints = rewardPoints;          
        this.expireDate = expireDate;
    }
}
class WeeklyChallenge{
    constructor(ID, description, rewardXP, rewardPoints, expireDate){
        this.ID = ID;
        this.description = description;
        this.rewardXP = rewardXP;
        this.rewardPoints = rewardPoints;       
        this.expireDate = expireDate;
    }
}

let Achievements = [];
let DailyChallenges = [];
let WeeklyChallenges = [];

mp.events.add("addAchievement", (id, name, desc, rXP, rPts, isUnlocked, bg) => {
    let a = new Achievement(id, name, desc, rXP, rPts, isUnlocked, bg);
    Achievements.push(a);
});
mp.events.add("addDailyChallenge", (id, name, desc, rXP, rPts, expireDate) => {
    let realDate = new Date(expireDate*1000);

    let c = new DailyChallenge(id, name, desc, rXP, rPts, realDate);
    DailyChallenges.push(c);
});
mp.events.add("addWeeklyChallenge", (id, name, desc, rXP, rPts, expireDate) => {
    let realDate = new Date(expireDate*1000);

    let c = new WeeklyChallenge(id, name, desc, rXP, rPts, realDate);
    WeeklyChallenges.push(c);
});



mp.events.add("addAchievementsToMenu", () => {
    let json = `gm.$refs.profile.$refs.ProfileTabs.$refs.Achievements.achvs = [`;
    let achstr = "";
    for(let i = 0 ; i < Achievements.length; i++){
        achstr = `{
            "name":"${Achievements[i].name}", 
            "description":"${Achievements[i].description}", 
            "unlocked":${Achievements[i].unlocked},
            "background":"${Achievements[i].background}"
        },`;
        json += achstr;
    }
    json += "]";
    UIMenu.execute(json);
});
mp.events.add("unlockAchievement", (achievementName) => {
    let achunlock = Achievements.find(a => a.name == achievementName);
    if(!achunlock) return;

    achunlock.unlocked = true;
    UIMenu.execute(`gm.$refs.profile.$refs.ProfileTabs.$refs.Achievements.achvs.find(a => a.name === "${achievementName}").unlocked = true`);
});