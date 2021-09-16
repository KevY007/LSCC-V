var currentJailTime = 0;
var lastJailUpdate = 10;

var chatStatus = false;
var lastChatToggle = Date.now();
var streetName = null;
var zoneName = null;
var showHud = true;
var renderNametags = true;
var specState = false;
var isNoClip = false;
var currentMuteTime = 0;
var lastMuteUpdate = 10;
var lastFunc = "Init";

var UIHud = null;
var UIMenu = null;
var menuToggled = false;

function traceLastFunc(iStr)
{
    
}

traceLastFunc("RequireInit");
require('mp-commands');
require("./nativeui/index.js");
require("./chat/index.js");

require('./logspanel/logsview.js');

require('./Player/Player.js');
require('./Player/Actions.js');
require("./Player/Nametags.js");
require("./UIHud/HUD.js");
require("./UIMenu/Menu.js");
require("./charcreator/clotheditor.js")
require("./charcreator/index.js");
require("./charcreator/outfitslots.js")

require('./camerasManager.js');
require('./cefBrowser.js');
require("./rappel/index.js");
require("./scoreboard/index.js");
require("./Player/Sync.js");
require("./Player/musicPlayer.js");
require("./Player/LightsMenu.js");
require("./loadoutsel/index.js");
require('./Player/OtherStuff.js');
require("./Player/Busted.js");
require("./Player/Voice.js");

mp.events.add("onOpenChatbox", (toggle) =>
{
    chatStatus = toggle;
    lastChatToggle = Date.now();
    mp.events.callRemote("UpdateChatState", toggle);
    traceLastFunc(`[Players] ChatBox Toggled ${toggle}`);
});

mp.events.add('loginInformationToServer', (username, password) => {

    mp.events.callRemote('OnPlayerLoginAttempt', username, password);
});

mp.events.add('registerInformationToServer', (email, username, password, allcorrect, rules) => {

    mp.events.callRemote('OnPlayerRegisterAttempt', email, username, password, allcorrect, rules);
});