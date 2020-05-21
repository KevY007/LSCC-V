mp.events.add("InitiateCustomChat", () => {
    mp.gui.chat.show(false); 

    const chatbox = mp.browsers.new('package://chat/index.html');
    chatbox.markAsChat();
});

mp.events.add("TogglePlayerChat", (toggle) => {
	mp.gui.chat.activate(toggle);
});