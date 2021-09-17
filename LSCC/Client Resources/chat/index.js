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

        mp.events.callLocal('RestartMenu');
        mp.events.callLocal('RestartHUD');
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