class Achievement{
    constructor(ID, name, description, rewardXP, rewardPoints){
        this.ID = ID;
        this.name = name;
        this.description = description;
        this.rewardXP = rewardXP;
        this.rewardPoints = rewardPoints;
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

mp.events.add("addAchievement", (id, name, desc, rXP, rPts) => {
    let a = new Achievement(id, name, desc, rXP, rPts);
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
