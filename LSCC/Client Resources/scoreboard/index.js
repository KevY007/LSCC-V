let browser = null;
let updater = null;
let forceOff = false;
let serverName = "Los Santos Copchase";
let tablistLen = 0;

mp.events.add("onOpenChatbox", (toggle) =>
{
	forceOff = toggle;
});

mp.events.add("updateSBServerName", (eUpdatedName) =>
{
	serverName = eUpdatedName;
});

function ToggleScoreboard()
{
    if (browser != null) {
        
        if(browser.active)
        {
            if(updater != null) clearInterval(updater);
            updater = null;

            if(tablistLen >= 9)
            {
                mp.gui.cursor.show(false, false);
                mp.events.call("TogglePlayerChat", true);
            }
            browser.active = false;

        } else {
            browser.active = true;

            tablistLen = mp.players.length;

            if(tablistLen >= 9)
            {
                mp.gui.cursor.show(true, true);
                mp.events.call("TogglePlayerChat", false);
            }
            mp.events.call("browserDomReady", browser);
        }
        return;
    }

    tablistLen = mp.players.length;

    if(tablistLen >= 9)
    {
        mp.gui.cursor.show(true, true);
        mp.events.call("TogglePlayerChat", false);
    }

    browser = mp.browsers.new("package://scoreboard/cef/index.html");
    
    return;
}

function updatePlayer(player)
{
    let lobbystr = "N/A";
    if(player.getVariable("pLogged") === undefined || player.getVariable("pLogged") == false || player.getVariable("InLobby") === undefined) lobbystr = "LOGIN";
    else if(player.getVariable("InLobby") == -2) lobbystr = "JAIL";
    else if(player.getVariable("InLobby") == -3) lobbystr = "FREEROAM";
    else if(player.getVariable("InLobby") == -4) lobbystr = "DERBY";
    else if(player.getVariable("InLobby") == -5) lobbystr = "CUSTOMIZER";
    else if(player.getVariable("InLobby") > -1) lobbystr = player.getVariable("InLobby").toString();
    
    browser.execute('updateUser("' + lobbystr + '", "' + player.name + '", "' + 
    (player.getVariable("Level") === undefined ? 'n/a' : player.getVariable("Level")) + '", "' + 
    (player.getVariable("Ping") === undefined ? 'n/a' : player.getVariable("Ping")) +
    '", ' + 0 + ', ' + 0 + ', ' + 0 + ');');
    //'", ' + player.getVariable("Color_R") + ', ' + player.getVariable("Color_G") + ', ' + player.getVariable("Color_B") + ');');

    return;
}

mp.events.add('browserDomReady', (ebrowser) => {
    if(browser != null && browser === ebrowser)
    {
        if(forceOff == true)
        {
            if(browser.active) ToggleScoreboard();
            return;
        }
        else if(browser.active) {
            tablistLen = mp.players.length;

            if(tablistLen >= 9)
            {
                mp.gui.cursor.show(true, true);
                mp.events.call("TogglePlayerChat", false);
            }

            mp.players.forEach(
                (player) => {
                    //browser.execute('createUser("N/A", "' + player.name + '", "N/A", "N/A", ' + player.getVariable("Color_R") + ', ' + player.getVariable("Color_G") + ', ' + player.getVariable("Color_B") + ')');
                    updatePlayer(player);
                }
            );
        
            if(serverName != "Los Santos Copchase") 
            {
                browser.execute('updateServerName("' + serverName + '");');
            }
        
            updater = setInterval(function()
            {
                if(forceOff == true && browser != null && browser.active)
                {
                    ToggleScoreboard();
                }
                else if(browser != null && forceOff != true && browser.active)
                {
                    if(mp.players.length >= 9 && tablistLen <= 8)
                    {
                        tablistLen = mp.players.length;

                        mp.gui.cursor.show(true, true);
                        mp.events.call("TogglePlayerChat", false);
                    }
                    else if(mp.players.length <= 8 && tablistLen >= 9)
                    {
                        tablistLen = mp.players.length;
                        
                        mp.gui.cursor.show(false, false);
                        mp.events.call("TogglePlayerChat", true);
                    }

                    mp.players.forEach(
                        (player) => {
                            updatePlayer(player);
                        }
                    );
                }
            }, 1000);
        }
    }
});

mp.keys.bind(
    0x1B,
    false,
    () => {
        if(browser != null && browser.active) ToggleScoreboard();
    }
);

mp.keys.bind(0x4F, false, function () {

    if(mp.players.local.getVariable("pLogged") === undefined || mp.players.local.getVariable("pLogged") == false || forceOff == true) return;
    
    ToggleScoreboard();
})