let browser = null;
let updater = null;
let forceOff = false;
let serverName = "Los Santos Copchase";

mp.events.add("onOpenChatbox", (toggle) =>
{
	forceOff = toggle;
});

mp.events.add("updateSBServerName", (eUpdatedName) =>
{
	serverName = eUpdatedName;
});

mp.keys.bind(0x4F, false, function () {

    if(mp.players.local.getVariable("pLogged") === undefined || mp.players.local.getVariable("pLogged") == false || forceOff == true) return;
    
    if (browser != null) {
        browser.destroy();
        browser = null;
        clearInterval(updater);
        updater = null;
        mp.gui.cursor.show(false, false);
        return;
    }

    browser = mp.browsers.new("package://scoreboard/cef/index.html");

    mp.gui.cursor.show(true, true);

    mp.players.forEach(
        (player) => {
            let lobbystr = "N/A";
            if(player.getVariable("pLogged") === undefined || player.getVariable("pLogged") == false) lobbystr = "LOGIN";
            if(player.getVariable("InLobby") !== undefined && player.getVariable("InLobby") != -1) lobbystr = player.getVariable("InLobby").toString();
                
            browser.execute('createUser("' + lobbystr + '", "' + player.name + '", "' + player.getVariable("Level") + '", "' + player.getVariable("Ping") +
             '", ' + player.getVariable("Color_R") + ', ' + player.getVariable("Color_G") + ', ' + player.getVariable("Color_B") + ')');
        }
    );

    browser.execute('updateServerName("' + serverName + '")');

    updater = setInterval(function()
    { 
        mp.players.forEach(
            (player) => {
                let lobbystr = "N/A";
                if(player.getVariable("pLogged") === undefined || player.getVariable("pLogged") == false) lobbystr = "LOGIN";
                if(player.getVariable("InLobby") !== undefined && player.getVariable("InLobby") != -1) lobbystr = player.getVariable("InLobby").toString();
                
                browser.execute('updateUser("' + lobbystr + '", "' + player.name + '", "' + player.getVariable("Level") + '", "' + player.getVariable("Ping") +
                '", ' + player.getVariable("Color_R") + ', ' + player.getVariable("Color_G") + ', ' + player.getVariable("Color_B") + ')');
            }
        );

        browser.execute('updateServerName("' + serverName + '")');
        
    }, 1000);
})