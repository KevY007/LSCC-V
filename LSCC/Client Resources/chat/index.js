// Disables default RageMP Chat
mp.gui.chat.show(false);

// Initialize chatbox CEF, mark it as default server chat

const chatbox = mp.browsers.new('package://chat/chat-ui/index.html');
chatbox.markAsChat();

mp.events.add("TogglePlayerChat", (toggle) => {
    if(toggle) chatbox.execute(`chatAPI.activate(true);`);
    else chatbox.execute(`chatAPI.activate(false);`);
});