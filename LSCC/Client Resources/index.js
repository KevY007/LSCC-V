let currentJailTime = 0;
let lastJailUpdate = 10;

let currentMuteTime = 0;
let lastMuteUpdate = 10;
let lastFunc = "Init";

const fs = require("fs");

function traceLastFunc(iStr)
{
    fs.writeFile('tlf_latest.txt', data, (err) => { 
      
        if (err) throw err; 
    }) 
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

mp.events.add('loginInformationToServer', (username, password) => {

    mp.events.callRemote('OnPlayerLoginAttempt', username, password);

});

mp.events.add('registerInformationToServer', (email, username, password, allcorrect, rules) => {

    mp.events.callRemote('OnPlayerRegisterAttempt', email, username, password, allcorrect, rules);

});