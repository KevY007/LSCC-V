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
var scoreboardToggled = false;
var scoreboardCursor = false;

var cuffTarget = null;
var cuffInterval = null;

var disableSprint = false;

mp.players.local.Team = -1;
mp.players.local.LoggedIn = false;

function traceLastFunc(iStr)
{
    
}

// where IPL names name of your .ymap file in dlc -> mapping.rpf
if(!mp.game.streaming.isIplActive("blockheisttunnel") || !mp.game.streaming.isIplActive("derby") || 
    !mp.game.streaming.isIplActive("floydsapt") || !mp.game.streaming.isIplActive("whbig") || 
    !mp.game.streaming.isIplActive("whmed") || !mp.game.streaming.isIplActive("whsmall"))
{
	mp.game.invoke("0xD7C10C4A637992C9"); // _LOAD_SP_DLC_MAPS
	mp.game.invoke("0x0888C3502DBBEEF5"); // _LOAD_MP_DLC_MAPS
}

mp.players.local.position = new mp.Vector3(-1018.55300000, -2731.95600000, 15.70000000);

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
require("./Player/Sync.js");
require("./Player/musicPlayer.js");
require("./Player/LightsMenu.js");
require("./loadoutsel/index.js");
require('./Player/OtherStuff.js');
require("./Player/Busted.js");
require("./Player/Voice.js");

mp.events.add("onOpenChatbox", (toggle) =>
{
    if(mp.players.local.LoggedIn == undefined || mp.players.local.LoggedIn == false)
    {
        mp.gui.chat.activate(false);
        return;
    }
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