const Use3d = true;
const MaxRange = 50.0;
let forceOff = false;
mp.voiceChat.muted = true;


mp.keys.bind(0x4E, false, function() {
	mp.events.callRemote("OnScriptedKeyPress", 0x4E, false);
    mp.voiceChat.muted = true;
});

mp.keys.bind(0x4E, true, function() {
	if(!forceOff && 
		!(mp.players.local.getVariable("pLogged") === undefined || mp.players.local.getVariable("pLogged") == false)) 
	{
		mp.voiceChat.muted = false;
		mp.events.callRemote("OnScriptedKeyPress", 0x4E, true);
	}
});

mp.events.add("onOpenChatbox", (toggle) =>
{
	forceOff = toggle;
});

setInterval(() =>
{
	let localPlayer = mp.players.local;
	let localPos = localPlayer.position;
	
	mp.players.forEachInStreamRange(player =>
	{
		if(player != localPlayer)
		{
			if(player.handle !== 0)
			{
				player.voice3d = true;
				const playerPos = player.position;		
				let dist = mp.game.system.vdist(playerPos.x, playerPos.y, playerPos.z, localPos.x, localPos.y, localPos.z);
				
				if(dist > MaxRange)
				{
					player.voiceVolume = 0.0;
				}
				else
				{
					player.voiceVolume = 1 - (dist / MaxRange);
				}
			}
		}
	});
}, 500);