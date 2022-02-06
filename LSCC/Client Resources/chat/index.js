let chatbox = null;
let fontsized = false;

mp.events.add("setFontsize", (size) => {
    if(chatbox != null) 
    {
        chatbox.execute(`document.getElementById('chat').style.fontSize = "${size}"`);
        chatbox.execute(`document.getElementById('chat_messages').style.fontSize = "${size}"`);
    }
});
mp.events.add("setPagesize", (pages) => {
    // pages = how many lines will be shown basically



    if(chatbox != null){
        chatbox.execute(`document.getElementById('chat_messages').style.height = "${pages}"`)
    }
})

function convertPageSizeToPixels(pagesize){
    return (pagesize*23.07);
}

mp.events.addCommand("pagesize", function (page) {
    page = Number(page);

    if(chatbox == null) 
    {
        pushMessageToChat("Chatbox isn't created yet.");
        return;
    }

    if (!Number.isInteger(page)) {
        pushMessageToChat("[USAGE] /pagesize (number: line count | default: 13)");
        return;
    }   
    if(page < 5 || page > 30){
        pushMessageToChat("Page size can be between 5 and 30! (default is 13)");
        return;       
    }

    let pagestr = convertPageSizeToPixels(page) + "px";

    mp.events.callLocal("setPagesize", pagestr);

    mp.storage.data.pagesize = pagestr;
    mp.storage.flush();
});

mp.events.addCommand("fontsize", function (rate) {
    rate = Number(rate);

    if(chatbox == null) 
    {
        pushMessageToChat("Chatbox isn't created yet.");
        return;
    }

    if (!Number.isInteger(rate)) {
        pushMessageToChat("[USAGE] /fontsize (number: font size | default: 20)");
        return;
    }

    if(rate < 8 || rate > 28) {
        pushMessageToChat("Fontsize can be between 8-28, default is 20.");
        return;
    }

    var sizestr = rate + "px";
    mp.events.callLocal("setFontsize", sizestr);

    mp.storage.data.fontsize = sizestr;
    mp.storage.flush();
});