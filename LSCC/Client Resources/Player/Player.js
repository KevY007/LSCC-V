let camera = null;
const player = mp.players.local;
const timerBarLib = require("./Player/Bar.js");

let chaseProgBar = null;
let chaseProgText = null;
let collisionText = null;
let collisionTime = 0;
let chaseInterval = null;
let chaseTimeCur = 0;
let chaseTimeMax = 600;


// F/G Entering Tweak
mp.events.add('render', () =>
{
	const controls = mp.game.controls;
	
	controls.enableControlAction(0, 23, true);
	controls.disableControlAction(0, 58, true);
	
	if(controls.isDisabledControlJustPressed(0, 58))
	{
		let position = mp.players.local.position;		
		let vehHandle = mp.game.vehicle.getClosestVehicle(position.x, position.y, position.z, 5, 0, 70);
		
		let vehicle = mp.vehicles.atHandle(vehHandle);
		
		if(vehicle
			&& vehicle.isAnySeatEmpty()
			&& vehicle.getSpeed() < 5)
		{
			mp.players.local.taskEnterVehicle(vehicle.handle, 5000, 0, 2, 1, 0);
		}
	}
});

function createCam(x, y, z, rx, ry, rz, viewangle) {
    camera = mp.cameras.new("Cam", { x, y, z }, { x: rx, y: ry, z: rz }, viewangle);
    camera.setActive(true);
    mp.game.cam.renderScriptCams(true, true, 20000000000000000000000000, false, false);
}

function destroyCam() {
    if (!camera) return;
    camera.setActive(false);
    mp.game.cam.renderScriptCams(false, true, 0, true, true);
    camera.destroy();
    camera = null;
}

function TimeFormat(time)
{   
    var hrs = ~~(time / 3600);
    var mins = ~~((time % 3600) / 60);
    var secs = ~~time % 60;

    var ret = "";

    if (hrs > 0) {
        ret += "" + hrs + ":" + (mins < 10 ? "0" : "");
    }

    ret += "" + mins + ":" + (secs < 10 ? "0" : "");
    ret += "" + secs;
    return ret;
}

function updateChaseTimer()
{
    if(chaseProgBar != null && chaseProgText != null)
    {
        chaseTimeCur += 1;

        chaseProgBar.progress = chaseTimeCur / chaseTimeMax;
        chaseProgText.text = TimeFormat(chaseTimeMax - chaseTimeCur);

        chaseProgText.visible = true;
        chaseProgBar.visible = true;

        if(chaseTimeCur >= chaseTimeMax)
        {
            chaseProgText.visible = false;
            chaseProgBar.visible = false;

            clearInterval(chaseInterval);
            chaseInterval = null;
        }
    }

    if(collisionText != null)
    {
        if(collisionTime > 0)
        {
            collisionTime--;
            collisionText.text = TimeFormat(collisionTime);

            collisionText.visible = true;
        }
        else
        {
            collisionText.visible = false;
        }
    }
    return;
}


mp.events.add(
    {
        "ToggleChaseCollisions": (toggle) => {
            if(toggle)
            {
                mp.vehicles.forEach(target => {
                    mp.vehicles.forEach(target2 => {
                        target.setNoCollision(target2.handle, true);
                        target2.setNoCollision(target.handle, true);

                        target2.setInvincible(false);
                        target.setInvincible(false);
                    });
                });
            }
            else
            {
                mp.vehicles.forEach(target => {
                    mp.vehicles.forEach(target2 => {
                        target.setNoCollision(target2.handle, false);
                        target2.setNoCollision(target.handle, false);

                        target.setCanBeDamaged(true);
                        target2.setCanBeDamaged(true);
                        target.setCanBreak(true);
                        target2.setCanBreak(true);

                        target2.setInvincible(true);
                        target.setInvincible(true);
                    });
                });
            }
        },

        "SetActorInvulnerable": (toggle) => {
            if(toggle)
            {
                player.setInvincible(true);
            }
            else
            {
                player.setInvincible(false);
            }
        },

        "CanFallOutOfCar": (toggle) => {
            if(toggle)
            {
                player.canKnockOffVehicle(true);
            }
            else
            {
                player.canKnockOffVehicle(false);
            }
        },

        "StartChaseTimer": (iTimeCur, iTimeMax, collisionDuration) => {

            chaseTimeCur = iTimeCur - 1;
            chaseTimeMax = iTimeMax;
            collisionTime = collisionDuration;
            updateChaseTimer();
            
            if(chaseInterval != null) clearInterval(chaseInterval);

            chaseInterval = setInterval(updateChaseTimer, 1000);
        },

        "HideChaseTimer": () => {
            chaseProgBar.visible = false;
            chaseProgText.visible = false;
            collisionText.visible = false;
            clearInterval(chaseInterval);
            chaseInterval = null;
        },

        "playerEnterVehicle": (vehicle, seat) => {
            vehicle.setInvincible(false);
        },

        "InitChaseTimer": () => {

            if(chaseProgBar == null)
            {
                chaseProgBar = new timerBarLib.TimerBar("PROGRESS", true);
                chaseProgBar.pbarFgColor = [64, 115, 222, 255];
                chaseProgBar.pbarBgColor = [25, 53, 112, 255];
                chaseProgBar.progress = 1.0;
                chaseProgBar.visible = false;
            }

            if(collisionText == null)
            {
                collisionText = new timerBarLib.TimerBar("COLLISIONS ENABLE");
                collisionText.text = "0:30";
                collisionText.visible = false;
            }

            if(chaseProgText == null)
            {
                chaseProgText = new timerBarLib.TimerBar("TIME LEFT");
                chaseProgText.text = "10:00";
                chaseProgText.visible = false;
            }
        },

        "DestroyPlayerCamera": () => destroyCam(),

        "SetPlayerFrozen": (status) => {
            player.freezePosition(status);

            if(player.vehicle)
            {
                player.vehicle.freezePosition(status);
            }
        },

        "SpectatePlayerCamera": (target) => {
            if (!camera) return;

            camera.attachTo(target.handle, 0, 0, 2.0, false);
            mp.game.cam.renderScriptCams(true, false, 0, true, false);

            camera.setActive(true);
        },

        "CreatePlayerCamera": (x, y, z, rx, ry, rz, viewangle) => {
            createCam(x, y, z, rx, ry, rz, viewangle);
        },

        "SetDiscordStatus": (serverName, status) => {
            mp.discord.update(serverName, status);
        },
    });