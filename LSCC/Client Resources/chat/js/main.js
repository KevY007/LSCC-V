let chat =
{
	size: 0,
	history_limit: 128,
	container: null,
	input: null,
	enabled: false,
	active: true,
	historyMsgs: [],
	currentIndex: 0,
	loaded: false,
	scrollStyle: null
};
var chatOpacityTimeout;

let antiDoubleSend = Date.now();

const MAX_MSG_HISTORY = 64;

function toggleScroll(enable) { 

	if(enable) 
	{
		if(chat.scrollStyle !== null) document.querySelector('#chat ul#chat_messages').style = chat.scrollStyle;
	}
	else 
	{
		chat.scrollStyle = document.querySelector('#chat ul#chat_messages');
		document.querySelector('#chat ul#chat_messages').style.overflow = 'hidden';
	}
} 

function toggleChatOpacity(enable){

	if(chat.active == false){
		$("#chat").opacity = 0.0;
		return;
	}

	if(enable){
		$("#chat").fadeTo("fast", 1.0);
	}
	else{
		if(chat.input == null){
			$(("#chat")).fadeTo("slow", 0.4);		
		}
		else{ // If the player is typing to prevent hiding the chat while they're doing so.
			clearTimeout(chatOpacityTimeout);
			chatOpacityTimeout = setTimeout(() =>{
				toggleChatOpacity(false);
			}, 5000);			
		}
	}
}

function enableChatInput(enable)
{
	if(chat.active == false
		&& enable == true)
	{
		//toggleScroll(false);
		return;
	}
	
	//toggleScroll(enable);
	mp.events.call("onOpenChatbox", enable);
    if (enable != (chat.input != null))
	{
		mp.invoke("focus", enable);
		mp.invoke("setTypingInChatState", enable);

        if (enable)
		{
            chat.input = $("#chat").append('<div><input id="chat_msg" type="text" maxlength="128" /></div>').children(":last");
			chat.input.children("input").focus();			
			toggleChatOpacity(true);

        } 
		else
		{
            chat.input.fadeOut('fast', function()
			{
                chat.input.remove();
                chat.input = null;
            });

			clearTimeout(chatOpacityTimeout);
			chatOpacityTimeout = setTimeout(() =>{
				toggleChatOpacity(false);
			}, 5000);
        }
    }
}

var chatAPI =
{
	push: (text_) =>
	{
		if(!chat.loaded) return;
		clearTimeout(chatOpacityTimeout);		
		toggleChatOpacity(true);

		let colorPositions = [];
		let colors = [];
		let chatElement = "<li>";
		let text = "!{#FFFFFF}" + text_;
		for (let i = 0; i<text.length; i++) {
			let colorCheck = `${text[i]}${text[i+ 1]}${text[i + 2]}`;
			
			if (colorCheck === "!{#") {
				colorPositions.push(i);
			}
		}

		colorPositions.forEach(el => {
			let sub = text.slice(el, -1);
			colors.push(sub.slice(3, 9));
		});

		colorPositions.forEach((el, i) => {
			let sub = text.slice(colorPositions[i] + 10, colorPositions[i + 1]);
			chatElement += `<span style='color: ${colors[i]}'>${sub}</span>`;
		});

		chatElement += "</li>";

		if (chatElement === "<li></li>") {
			chat.container.prepend("<li>" + text + "</li>");
		} else {
			chat.container.prepend(chatElement);
		}

		chat.size++;

		if (chat.size >= chat.history_limit)
		{
			chat.container.children(":last").remove();
		}

		chatOpacityTimeout = setTimeout(() =>{		
			toggleChatOpacity(false);
		}, 5000);
	},
	
	clear: () =>
	{
		if(!chat.loaded) return;
		chat.container.html("");
	},
	
	activate: (toggle) =>
	{
		if (toggle == false
			&& (chat.input != null))
			enableChatInput(false);
			
		chat.active = toggle;
	},
	
	show: (toggle) =>
	{
		if (toggle == false
			&& (chat.input != null))
			enableChatInput(false);

		if(toggle)
			$("#chat").show();
		else
			$("#chat").hide();
		
		chat.active = toggle;
	}
};

let api = {"chat:push": chatAPI.push, "chat:clear": chatAPI.clear, "chat:activate": chatAPI.activate, "chat:show": chatAPI.show}; 

for(let fn in api)
{
	mp.events.add(fn, api[fn]);
}


$(document).ready(function()
{
	chat.container = $("#chat ul#chat_messages");
	chat.loaded = true;
	
    $(".ui_element").show();
	chatAPI.push("!{#A9C4E4}Connected to !{#ffae00}Race !{#FFFFFF}'n !{#0097ff}Chase!{#A9C4E4}.");

	window.addEventListener("wheel", function(event)
	{
		event.preventDefault();

		if(chat.input != null) {

			document.getElementById("chat_messages").scrollTo(0, document.getElementById("chat_messages").scrollTop - event.deltaY); 

			/*document.getElementById("chat_messages").scrollBy({ 
				top: rate,
				left: 0, 
				behavior: 'smooth' 
			});*/

			// inversely equally scroll
		}

	}, { passive: false });

	//toggleScroll(false);

	window.addEventListener("keydown", function(e) {
		// space and arrow keys
		if([32].indexOf(e.keyCode) > -1) {
			if((chat.input != null && chat.active) == false) {
				e.preventDefault();
			}
		}
	}, false);

	

    $("body").keydown(function(event)
	{
        if (event.which == 84 && chat.input == null
			&& chat.active == true)
		{
			chat.currentIndex = 0; // Reset index on open
            enableChatInput(true);
            event.preventDefault();
        } else if (event.which == 38) {
			event.preventDefault();

			if (chat.historyMsgs.length === 0)
				return;

			const previousMessages = chat.historyMsgs;

			if (chat.currentIndex >= previousMessages.length)
				return;


            chat.currentIndex = chat.currentIndex + 1;
			chat.input.children("input").val(previousMessages[chat.currentIndex-1]);
			
			document.getElementById("chat_msg").focus();
			document.getElementById("chat_msg").setSelectionRange(129, 129);
			
		} else if (event.which == 40) {
			event.preventDefault();

			if (chat.historyMsgs.length === 0)
				return;

			const previousMessages = chat.historyMsgs;

			if (chat.currentIndex === -1) {
				chat.input.children("input").val("")
				return;
			}

			chat.currentIndex = chat.currentIndex - 1;
			chat.input.children("input").val(previousMessages[chat.currentIndex]);

			document.getElementById("chat_msg").focus();
			document.getElementById("chat_msg").setSelectionRange(129, 129);
		}
		else if (event.which == 13 && chat.input != null)
		{
			var value = chat.input.children("input").val();

            if (value.length > 0 && value.length <= 128) 
			{
				if (chat.historyMsgs.length >= MAX_MSG_HISTORY) {
					chat.historyMsgs.pop();
				}

				chat.historyMsgs.unshift(value);
				chat.currentIndex = 0;


                if (value[0] == "/")
				{
                    value = value.substr(1);

                    if (value.length > 0)
					{
                        if(Date.now() - antiDoubleSend >= 200)
						{
							mp.invoke("command", value);
							antiDoubleSend = Date.now();
						}
					}
                }
				else
				{
					if(Date.now() - antiDoubleSend >= 200)
					{
	                    mp.invoke("chatMessage", value);
						antiDoubleSend = Date.now();
					}
                }
			}
			
			$("#chat ul#chat_messages").scrollTop(0); // Scroll all the way to down.
			//toggleScroll(false);
            enableChatInput(false);
        }
    });
});