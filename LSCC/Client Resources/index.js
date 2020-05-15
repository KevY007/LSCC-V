require('./Player/Player.js');
require('./camerasManager.js');
require('./cefBrowser.js');

mp.events.add('loginInformationToServer', (username, password) => {

    mp.events.callRemote('OnPlayerLoginAttempt', username, password);

});

mp.events.add('registerInformationToServer', (email, username, password, allcorrect, rules) => {

    mp.events.callRemote('OnPlayerRegisterAttempt', email, username, password, allcorrect, rules);

});