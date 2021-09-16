let chatbox = null;
let fontsized = false;

mp.events.add("InitiateCustomChat", () => {

    if(chatbox == null) {
        mp.gui.chat.show(false); 

        chatbox = mp.browsers.new('package://chat/index.html');
        chatbox.markAsChat();

        setTimeout(() => {
            if(!fontsized)
            {
                if(mp.storage.data.fontsize !== undefined)
                {
                    mp.events.callLocal("setFontsize", mp.storage.data.fontsize);
                }
                fontsized = true;
            }
        }, 2500);

        if(typeof UIMenu == "undefined" || UIMenu == null)
        {
            UIMenu = mp.browsers.new("package://UIMenu/index.html");
            UIMenu.execute(`gm.mainMenu = false;`);
            UIMenu.active = false;

            mp.events.callLocal("MenuToggleTyping", false);
        }
        if(typeof UIHud == "undefined" || UIHud == null)
        {
            UIHud = mp.browsers.new("package://UIHud/index.html");
            UIHud.execute(`gm.hud = false;`);
            UIHud.execute(`gm.topRightInfo = ${mp.storage.data.menu.topRightInfo}; gm.killFeed = ${mp.storage.data.menu.killFeed};`);
        }
    }
});

mp.events.add("TogglePlayerChat", (toggle) => {
    mp.gui.chat.activate(toggle);
});

mp.events.add("setFontsize", (size) => {
    if(chatbox != null) 
    {
        chatbox.execute(`document.getElementById('chat').style.fontSize = "${size}"`);
        chatbox.execute(`document.getElementById('chat_messages').style.fontSize = "${size}"`);
    }
});

mp.events.addCommand("fontsize", function (rate) {
    rate = Number(rate);

    if(chatbox == null) 
    {
        mp.gui.chat.push("Chatbox isn't created yet.");
        return;
    }

    if (!Number.isInteger(rate)) {
        mp.gui.chat.push("Invalid size (must be in numbers) default size: 16");
        return;
    }

    if(rate < 8 || rate > 28) {
        mp.gui.chat.push("Fontsize can be between 8-28, default is 16.");
        return;
    }

    var sizestr = rate + "px";
    mp.events.callLocal("setFontsize", sizestr);

    mp.storage.data.fontsize = sizestr;
    mp.storage.flush();
});