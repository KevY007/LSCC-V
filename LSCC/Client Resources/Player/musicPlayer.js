const localPlayer = mp.players.local;
let musicPlayer = mp.browsers.new('package://Player/musicPlayer/index.html');
let nextid = 0;
let activeSounds = {};

const soundManager = {
    add: function (id) {
        if (activeSounds[id]) 
        {
            if(musicPlayer) musicPlayer.execute(`playAudio("${id}", "${activeSounds[id].url}", "${activeSounds[id].volume}");`);
        }
    },
    remove: function (id) {
        if (musicPlayer) 
        {
            if (activeSounds[id]) activeSounds[id] = null;

            musicPlayer.execute(`stopAudio("${id}")`);
        }
    },
    setVolume: function (id, volume) {
        if (activeSounds[id])
        {
            activeSounds[id].volume = volume;
            if (musicPlayer) musicPlayer.execute(`setVolume("${id}", "${volume}")`);
        }
    },
    pauseToggle: function (id, pause) {
        if (musicPlayer) 
        {
            if (activeSounds[id]) activeSounds[id].paused = pause;
            musicPlayer.execute(`setPaused("${id}", "${pause}")`);
        }
    }
}

mp.events.add({
    'audioFinish': (soundID) => {
        soundManager.remove(soundID);
    },
    'audioError': (soundID, err) => {
        soundManager.remove(soundID);

        mp.gui.chat.push(`Sound Error: ${soundID}|${err}`);
    }
});

mp.game.audio.playSoundEx = function (url, volume = 1) {

    nextid++;

    activeSounds[nextid] = {
        id: nextid,
        url: url,
        volume: volume,
        paused: false 
    }
    
    soundManager.add(nextid);

    return nextid;
};

mp.game.audio.setSoundVolume = function (id, volume = 1) {
    if (activeSounds[id]) {
        soundManager.setVolume(id, volume);
    }
};

mp.game.audio.pauseSound = function (id, volume = 1) {
    if (activeSounds[id]) {
        soundManager.pauseToggle(id, pause);
    }
};

mp.game.audio.stopSoundEx = function (id, volume = 1) {
    soundManager.remove(id);
};