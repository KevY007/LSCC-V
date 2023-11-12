var global = require('./globals');
var ObjectiveData = {
    show:false,
    message:""
};
const {TranslateMessage: Translate} = require('./Translator');

const BankRobHostageData = [
    {
        "animDictionary": "anim@heists@ornate_bank@hostages@hit",
        "animClip": "hit_loop_ped_a",
        "position": new mp.Vector3(243.79079, 227.17055, 106.28744),
        "heading": 76.1064,
        "skin":"ig_bankman"
    },
    {
        "animDictionary": "anim@heists@ornate_bank@hostages@hit",
        "animClip": "hit_loop_ped_c",
        "position": new mp.Vector3(248.61475, 226.6744, 106.28748),
        "heading": 166.46756,
        "skin":"u_m_m_bankman"
    },
    {
        "animDictionary": "random@arrests",
        "animClip": "kneeling_arrest_idle",
        "position": new mp.Vector3(255.31871, 228.69337, 106.28613),
        "heading": -13.687548,
        "skin":"a_f_y_business_04"
    },
    {
        "animDictionary": "amb@code_human_cower@female",
        "animClip": "idle_a",
        "position": new mp.Vector3(243.7874, 224.19652, 106.28689),
        "heading": 148.99236,
        "skin":"a_f_o_genstreet_01"
    },
    {
        "animDictionary": "amb@code_human_cower@male",
        "animClip": "idle_a",
        "position": new mp.Vector3(241.53307, 224.9041, 106.28689),
        "heading": -137.23442,
        "skin":"a_m_o_genstreet_01"
    },
    {
        "animDictionary": "anim@heists@ornate_bank@hostages@hit",
        "animClip": "hit_loop_ped_d",
        "position": new mp.Vector3(254.29602, 220.53346, 106.28684),
        "heading": 151.43887,
        "skin":"a_f_m_ktown_01"
    },
    {
        "animDictionary": "anim@heists@ornate_bank@hostages@hit",
        "animClip": "hit_loop_ped_e",
        "position": new mp.Vector3(252.74269, 221.3664, 106.286575),
        "heading": 178.74031,
        "skin":"a_m_m_socenlat_01"
    },
    {
        "animDictionary": "anim@heists@ornate_bank@hostages@hit",
        "animClip": "hit_loop_ped_f",
        "position": new mp.Vector3(250.64748, 221.58116, 106.286575),
        "heading": 178.74031,
        "skin":"a_f_m_soucentmc_01"
    },
    {
        "animDictionary": "anim@heists@ornate_bank@hostages@hit",
        "animClip": "hit_loop_ped_g",
        "position": new mp.Vector3(250.47556, 216.60675, 106.286804),
        "heading": -99.13274,
        "skin":"cs_tom"
    },
    {
        "animDictionary": "anim@heists@ornate_bank@hostages@hit",
        "animClip": "hit_loop_ped_h",
        "position": new mp.Vector3(253.66742, 215.48692, 106.286896),
        "heading": 106.908615,
        "skin":"mp_f_cardesign_01"
    },
    {
        "animDictionary": "anim@heists@ornate_bank@hostages@hit",
        "animClip": "hit_loop_ped_a",
        "position": new mp.Vector3(249.0875, 212.70743, 106.286896),
        "heading": -21.926682,
        "skin":"s_m_m_bouncer_02"
    },
    {
        "animDictionary": "anim@heists@ornate_bank@hostages@hit",
        "animClip": "hit_loop_ped_g",
        "position": new mp.Vector3(241.64333, 214.93053, 106.286896),
        "heading": -102.78821,
        "skin":"mp_f_helistaff_01"
    },
    {
        "animDictionary": "anim@heists@ornate_bank@hostages@hit",
        "animClip": "hit_loop_ped_h",
        "position": new mp.Vector3(241.45969, 214.12125, 106.286896),
        "heading": -45.291897,
        "skin":"s_m_m_highsec_04"
    }
];
var BeginningScreenCEF = null;
const FUGITIVE_OBJECTIVES = [
    "F_BANKROB_SCREEN_OBJECTIVE_1",
    "F_BANKROB_SCREEN_OBJECTIVE_2",
    "F_BANKROB_SCREEN_OBJECTIVE_3",
    "F_BANKROB_SCREEN_OBJECTIVE_4",
    "F_BANKROB_SCREEN_OBJECTIVE_5",
]
const POLICE_OBJECTIVES = [
    "C_BANKROB_SCREEN_OBJECTIVE_1",
    "C_BANKROB_SCREEN_OBJECTIVE_2",
    "C_BANKROB_SCREEN_OBJECTIVE_3",
    "C_BANKROB_SCREEN_OBJECTIVE_4",
    "C_BANKROB_SCREEN_OBJECTIVE_5",
]
/**
 * Need translating:
 * C_BANKROB_SCREEN_OBJECTIVE_2 -> fugitiveCount
 */

mp.events.add("screenSimulation", (team, fugitiveCount) => {
    mp.events.callLocal("EVENT:ToggleBankRobberyBeginningScreen", true, team, fugitiveCount);
    setTimeout(() => {
        mp.events.callLocal("EVENT:ToggleBankRobberyBeginningScreen", false);
    }, 13000);
});

mp.events.add("EVENT:ToggleBankRobberyBeginningScreen", (toggle, team, fugitiveCount) => {
    if(!toggle){
        if(BeginningScreenCEF){
            BeginningScreenCEF.call("toggle_objectives", false);
            setTimeout(() => {
                mp.game.cam.doScreenFadeOut(0);
                //BeginningScreenCEF.call("fadeoutGameScreen", false);
                mp.game.cam.doScreenFadeIn(1000);
                setTimeout(() => {
                    if(BeginningScreenCEF) BeginningScreenCEF.destroy();
                    BeginningScreenCEF = null;
                }, 251);
            
                mp.game.audio.playSoundFrontend(-1, "Long_Transition_Out", "PLAYER_SWITCH_CUSTOM_SOUNDSET", true);

                setTimeout(() => {
                    mp.events.callLocal('ToggleHud', true);
                }, 1000);
            }, 1001);
        }
        return;
    }
    BeginningScreenCEF = mp.browsers.new("package://GameEvents/BankRobbery/CEF/index.html");
    BeginningScreenCEF.active = true;

    var objectiveArray = [];
    if(team == 0){
        objectiveArray = FUGITIVE_OBJECTIVES.map((o) => {
            return Translate(o);
        });
    }
    if(team == 1){
        objectiveArray = POLICE_OBJECTIVES.map((o) => {
            if(o != "C_BANKROB_SCREEN_OBJECTIVE_2")
                return Translate(o);
            else return Translate(o, [fugitiveCount]);
        });
    }

    mp.events.callLocal('ToggleHud', false);
    BeginningScreenCEF.call("fadeoutGameScreen", true);
    mp.game.audio.playSoundFrontend(-1, "1st_Person_Transition", "PLAYER_SWITCH_CUSTOM_SOUNDSET", true);
    setTimeout(() => {
        mp.game.audio.playSoundFrontend(-1, "Long_Transition_In", "PLAYER_SWITCH_CUSTOM_SOUNDSET", true);
        BeginningScreenCEF.call("set_objectives", objectiveArray);
        BeginningScreenCEF.call("set_team", team);
        BeginningScreenCEF.call("toggle_objectives", true);
    }, 1000);

});


var BR_BLIPS = [];
var FinishMarker = null;
mp.events.add("EVENT:AddBankRobberyFinishMarker", (posX, posY, posZ) => {
    if(FinishMarker != null) return;

    FinishMarker = mp.markers.new(1, new mp.Vector3(posX, posY, posZ), 2.0, {
        color:[255, 255, 0, 128],
        visible:true,
        dimension:mp.players.local.dimension
    });
});
mp.events.add("EVENT:DestroyBankRobberyFinishMarker", () => {
    if(FinishMarker == null) return;

    FinishMarker.destroy();
    FinishMarker = null;
});

mp.events.add("EVENT:ClearObjectiveBlips", () => {
    for(let i = 0 ; i < BR_BLIPS.length ; i++){
        if(BR_BLIPS[i])
            BR_BLIPS[i].destroy();
    }
    BR_BLIPS = [];
});
mp.events.add("EVENT:AddBankRobberyObjectiveBlip", (posX, posY, posZ) => {
    var blip = mp.blips.new(1, new mp.Vector3(posX, posY, posZ), {
        name:"Objective",
        scale:1.0,
        color:5,
        alpha:255,
        shortRange:false,
        dimension:mp.players.local.dimension
    });
    BR_BLIPS.push(blip);
});
mp.events.add("EVENT:SetBankRobberyWaypoint", (posX, posY) => {
    mp.game.ui.setNewWaypoint(posX, posY);
});
mp.events.add("EVENT:ClearBankRobberyWaypoint", () => {
    mp.game.invoke('0xA7E4E2D361C2627F'); // CLEAR WAYPOINT
});

mp.events.add("playerEnterVehicle", (vehicle, seat) => {
    const isHeistLeader = (mp.players.local.hasVariable("BR_IsHeistLeader") && mp.players.local.getVariable("BR_IsHeistLeader") == true);
    if(!isHeistLeader) return;
    if(!vehicle.hasVariable("Team")) return;

    const team = vehicle.getVariable("Team");
    mp.events.callRemote("EVENT:BankRobbery_LeaderEnterVehicle", vehicle, team);
});

var BR_PEDS = [];
function destroyBankRobberyPeds(){
    for(let i = 0 ; i < BR_PEDS.length ; i++){
        if(BR_PEDS[i] && BR_PEDS[i].ped)
            BR_PEDS[i].ped.destroy();
    }
    BR_PEDS = [];
}
function loadBankRobberyPeds(){
    for(let i = 0 ; i < BankRobHostageData.length ; i++){
        const pedData = BankRobHostageData[i];

        let ped = mp.peds.new(mp.game.joaat(pedData.skin),
                              pedData.position,
                              pedData.heading,
                              mp.players.local.dimension);

        BR_PEDS.push({
            ped:ped,
            data:pedData
        });
    }
}
mp.events.add("entityStreamIn", (entity) => {
    if(entity.type !== 'ped') return;
    const brPed = BR_PEDS.find(p => p.ped.handle === entity.handle);
    if(!brPed) return;

    mp.events.callRemote(`ServerConsoleOutput`, `[PedStream - ${mp.players.local.name}] Streaming in Bank Robbery ped.`);

    brPed.ped.setInvincible(true);
    brPed.ped.taskPlayAnim(brPed.data.animDictionary, brPed.data.animClip, 8.0, 1.0, -1, 1, 1.0, false, false, false);
});

mp.events.add("EVENTS:SetBankRobberyActive", (value) => {
    global.BankRobberyOn = value;
    if(value == true){
        mp.game.gameplay.setFakeWantedLevel(4);
        loadBankRobberyPeds();
    }
    else{
        mp.game.gameplay.setFakeWantedLevel(0);
        destroyBankRobberyPeds();
    }
});
mp.events.add("EVENTS:SetBankRobberyObjective", (toggle, message_code, ...args) => {

    const argsArray = (args.length > 0 ? args : null);

    ObjectiveData = {
        show:toggle,
        message:Translate(message_code, argsArray)
    };
});
mp.events.add('render', () => {
   if(!ObjectiveData.show) return;

   mp.game.graphics.drawText(ObjectiveData.message, [0.5, 0.95], {
    font:0,
    color:[255, 255, 255, 255],
    outline:true,
    scale:[0.35, 0.35],
    centre:true
   });
});