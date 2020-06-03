let chatbox = null;
let fontsized = false;

mp.events.add("InitiateCustomChat", () => {
    mp.gui.chat.show(false); 

    chatbox = mp.browsers.new('package://chat/index.html');
    chatbox.markAsChat();

    setTimeout(() => {
        if(!fontsized)
        {
            if(mp.storage.data.fontsize !== undefined)
            {
                var fsize = mp.storage.data.fontsize;
                mp.events.callLocal("setFontsize", mp.storage.data.fontsize);
            }
            fontsized = true;
        }
    }, 2500);
});

mp.events.add("TogglePlayerChat", (toggle) => {
	mp.gui.chat.activate(toggle);
});

mp.events.add("setFontsize", (size) => {
    if(chatbox != null) chatbox.execute(`document.getElementById('chat').style.fontSize = "${size}"`);
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

    if(rate < 8 || rate > 24) {
        mp.gui.chat.push("Fontsize can be between 8-24, default is 16.");
        return;
    }

    var sizestr = rate + "px";
    mp.events.callLocal("setFontsize", sizestr);

    mp.storage.data.fontsize = sizestr;
    mp.storage.flush();
});