require("./chat/index.js");

require('./Player/Player.js');
require("./Player/Nametags.js");
require("./Player/Voice.js");
require("./Player/HUD.js");

require('./camerasManager.js');
require('./cefBrowser.js');

require("./Mugshot/MugshotBoard.js");

global.hasMugshotBoard = false;

mp.players.local.mugshotboard = {
    show: (title, topText, midText, bottomText, rank = -1) => mp.events.call("ShowMugshotBoard", title, topText, midText, bottomText, rank),
    hide: () => mp.events.call("HideMugshotBoard")
};

mp.events.add('loginInformationToServer', (username, password) => {

    mp.events.callRemote('OnPlayerLoginAttempt', username, password);

});

mp.events.add('registerInformationToServer', (email, username, password, allcorrect, rules) => {

    mp.events.callRemote('OnPlayerRegisterAttempt', email, username, password, allcorrect, rules);

});