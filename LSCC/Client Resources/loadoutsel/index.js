let loadoutbrowser = null;
let loadoutList = null;

mp.events.add("hideLoadoutSelector", () =>
{
    if (loadoutbrowser != null) {
        
        loadoutbrowser.destroy();
        loadoutbrowser = null;

        mp.gui.cursor.show(false, false);
        
        mp.events.call("TogglePlayerChat", true);
        return;
    }
});

mp.events.add("showLoadoutSelector", (loadouts) => {

    mp.events.callLocal("hideLoadoutSelector");

    mp.events.call("TogglePlayerChat", false);
    loadoutList = JSON.parse(loadouts);
    loadoutbrowser = mp.browsers.new("package://loadoutsel/index.html");

    mp.gui.cursor.show(true, true);
});

mp.events.add("OnSelectLoadout", (team, loadoutName) => {
    mp.events.call("hideLoadoutSelector");
    mp.events.callRemote("OnLoadoutSelectorSelect", team, loadoutName);
});

mp.keys.bind(
    0x1B,
    false,
    () => {
        if(loadoutbrowser != null) mp.events.call("hideLoadoutSelector");
    }
);

function remove_quotes(values1) // fucking savior
{
    var values = values1.toString();
    var str = "";
    var blockList = ['"','\'','\\']; // This is the list of key words to be escaped
    var flag = 0;
    for(var i = 0;i<values.length;i++)
    {
        for(var j=0;j<blockList.length;j++)
        {
            if(values[i] == blockList[j])
            {
                flag = 1;
                break;
            }
        }
        if(flag == 0)
        str += values[i];
        else
        {
            str += '\\';
            str += values[i];
            flag = 0;
        }
    }
    return str;
}

mp.events.add('browserDomReady', (ebrowser) => {
    if(loadoutbrowser != null && loadoutbrowser === ebrowser)
    {
        mp.gui.cursor.show(true, true);

        for (let i = 0; i < loadoutList.Names.length; i++) {
            loadoutbrowser.execute(`addLoadout("` + loadoutList.Names[i] + `", "` + loadoutList.Levels[i] + `", "` + remove_quotes(JSON.stringify(loadoutList.Weapons[i])) + `", "` + loadoutList.Teams[i] + `", ` + loadoutList.Locked[i] + `);`);
        }
    }
});