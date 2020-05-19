const Use3d = true;
const UseAutoVolume = false;
const MaxRange = 50.0;
mp.voiceChat.muted = true;


mp.keys.bind(0x4E, false, function() {
    mp.voiceChat.muted = true;
});

mp.keys.bind(0x4E, true, function() {
    mp.voiceChat.muted = false;
});

mp.events.add('render', () => {
    if(!mp.voiceChat.muted)
    {
        mp.game.graphics.drawText("Voice: Speaking", [0.22, 0.83], { 
            font: 4, 
            color: [63, 135, 240, 255], 
            scale: [0.5, 0.5], 
            outline: true
          });
    }
    else
    {
        mp.game.graphics.drawText("Voice: Idle (Hold N)", [0.22, 0.83], { 
            font: 4, 
            color: [63, 135, 240, 255], 
            scale: [0.5, 0.5], 
            outline: true
          });
    }
});


let g_voiceMgr =
{
	listeners: [],
	
	add: function(player)
	{
		this.listeners.push(player);
		
		player.isListening = true;		
		mp.events.callRemote("AddVoiceListener", player);
		
		if(UseAutoVolume)
		{
			player.voiceAutoVolume = true;
		}
		else
		{
			player.voiceVolume = 1.0;
		}
		
		if(Use3d)
		{
			player.voice3d = true;
		}
	},
	
	remove: function(player, notify)
	{
		let idx = this.listeners.indexOf(player);
			
		if(idx !== -1)
			this.listeners.splice(idx, 1);
			
		player.isListening = false;		
		
		if(notify)
		{
			mp.events.callRemote("RemoveVoiceListener", player);
		}
	}
};

mp.events.add("playerQuit", (player) =>
{
	if(player.isListening)
	{
		g_voiceMgr.remove(player, false);
	}
});

setInterval(() =>
{
	let localPlayer = mp.players.local;
	let localPos = localPlayer.position;
	
	mp.players.forEachInStreamRange(player =>
	{
		if(player != localPlayer)
		{
			if(!player.isListening)
			{
				const playerPos = player.position;		
				let dist = mp.game.system.vdist(playerPos.x, playerPos.y, playerPos.z, localPos.x, localPos.y, localPos.z);
				
				if(dist <= MaxRange)
				{
					g_voiceMgr.add(player);
				}
			}
		}
	});
	
	g_voiceMgr.listeners.forEach((player) =>
	{
		if(player.handle !== 0)
		{
			const playerPos = player.position;		
			let dist = mp.game.system.vdist(playerPos.x, playerPos.y, playerPos.z, localPos.x, localPos.y, localPos.z);
			
			if(dist > MaxRange)
			{
				g_voiceMgr.remove(player, true);
			}
			else if(!UseAutoVolume)
			{
				player.voiceVolume = 1 - (dist / MaxRange);
			}
		}
		else
		{
			g_voiceMgr.remove(player, true);
		}
	});
}, 500);