let currentJailTime = 0;
let lastJailUpdate = 10;

let currentMuteTime = 0;
let lastMuteUpdate = 10;

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

mp.events.add('loginInformationToServer', (username, password) => {

    mp.events.callRemote('OnPlayerLoginAttempt', username, password);

});

mp.events.add('registerInformationToServer', (email, username, password, allcorrect, rules) => {

    mp.events.callRemote('OnPlayerRegisterAttempt', email, username, password, allcorrect, rules);

});