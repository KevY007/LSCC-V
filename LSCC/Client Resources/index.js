let currentJailTime = 0;
let lastJailUpdate = 10;

let chatStatus = false;
let streetName = null;
let zoneName = null;
let showHud = true;

let currentMuteTime = 0;
let lastMuteUpdate = 10;
let lastFunc = "Init";

function traceLastFunc(iStr)
{
    
}

traceLastFunc("RequireInit");
require('mp-commands');
require("./nativeui/index.js");
require("./chat/index.js");

require('./Player/Player.js');
require("./Player/Nametags.js");
require("./Player/HUD.js");
require("./Player/KillList.js");
require("./charcreator/clotheditor.js")
require("./charcreator/index.js");

require('./camerasManager.js');
require('./cefBrowser.js');
require("./rappel/index.js");
require("./scoreboard/index.js");
require("./Player/Sync.js");
require("./carselector/index.js");
require("./Player/musicPlayer.js");
require("./Player/LightsMenu.js");
require("./loadoutsel/index.js");
require('./Player/OtherStuff.js');
require("./Player/Busted.js");

mp.events.add("onOpenChatbox", (toggle) =>
{
    chatStatus = toggle;
    mp.events.callRemote("UpdateChatState", toggle);
    traceLastFunc(`[Players] ChatBox Toggled ${toggle}`);
});

mp.events.add('loginInformationToServer', (username, password) => {

    mp.events.callRemote('OnPlayerLoginAttempt', username, password);

});

mp.events.add('registerInformationToServer', (email, username, password, allcorrect, rules) => {

    mp.events.callRemote('OnPlayerRegisterAttempt', email, username, password, allcorrect, rules);

});