var global = require('./globals');
  const {TranslateMessage: Translate} = require('./Translator');
var MaxVoiceRange = 50;
mp.voiceChat.muted = true;



function DisableVoice()
{
    global.traceLastFunc(`[Players] Disable Voice`);

    mp.events.callRemote("OnScriptedKeyPress", 0x4E, false);
    mp.voiceChat.muted = true;
}

function EnableVoice()
{
    if(!global.chatStatus && 
		!(mp.players.local.getVariable("pLogged") === undefined || mp.players.local.getVariable("pLogged") == false)) 
	{
        global.traceLastFunc(`[Players] Enable Voice`);

		mp.voiceChat.muted = false;
		mp.events.callRemote("OnScriptedKeyPress", 0x4E, true);
	}
}

function TogVCMode()
{
    if(!global.chatStatus && mp.voiceChat.muted == false && 
		!(mp.players.local.getVariable("pLogged") === undefined || mp.players.local.getVariable("pLogged") == false)) 
	{
		mp.events.callRemote("OnScriptedKeyPress", 0x09, true);
	}
}

if(mp.storage.data.voicebind !== undefined)
{
    global.curVoiceBind = mp.storage.data.voicebind;
    global.lastVoiceBind = mp.storage.data.voicebind;
}

mp.keys.bind(global.lastVoiceBind, false, DisableVoice);
mp.keys.bind(global.lastVoiceBind, true, EnableVoice);
mp.keys.bind(0x09, true, TogVCMode);

mp.events.addCommand("voicebind", function (bind) {
    bind = Number(bind);

    if (!Number.isInteger(bind)) {
        mp.gui.chat.push("USAGE: voicebind (keycode) --Input a virtual keycode e.g. 0x4E is 'N' (default)");
        mp.gui.chat.push("https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes");
        return;
    }

    mp.storage.data.voicebind = bind;

    mp.storage.flush();

    global.lastVoiceBind = global.curVoiceBind;
    global.curVoiceBind = bind;

    mp.keys.unbind(global.lastVoiceBind, false, DisableVoice);
    mp.keys.unbind(global.lastVoiceBind, true, DisableVoice);

    DisableVoice();
    mp.keys.bind(global.curVoiceBind, false, DisableVoice);
    mp.keys.bind(global.curVoiceBind, true, EnableVoice);

    mp.events.callLocal("HUDNotify", `Push to talk is now binded to <strong>${String.fromCharCode(global.curVoiceBind)}</strong>.`, "blue", "fas fa-check", false, 
        1000, "bottomCenter", "replace");
        
    mp.events.callLocal("RemoveTooltip", String.fromCharCode(global.lastVoiceBind));
    mp.events.callLocal("AddTooltip", String.fromCharCode(global.curVoiceBind), "Voice Chat", -1);
});

mp.events.add("playerStopTalking", (player) => {
    if(!global.ServerUI) return;

    if(global.ServerUI) global.ServerUI.call('hud_voiceactivity_voice_remove', player.remoteId);
});
mp.events.add("playerStartTalking", (player) => {
    if(!global.ServerUI) return;

    const vec1 = mp.players.local.position;
    const vec2 = player.position;

    const distance = vec1.subtract(vec2).length();

    if(global.ServerUI) global.ServerUI.call('hud_voiceactivity_voice_add', player.remoteId, player.getVariable("BadgeData"), player.name, distance.toFixed(0), player.getVariable("Team"));
});