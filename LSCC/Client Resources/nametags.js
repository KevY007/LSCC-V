const maxDistance = 25*25;
const width = 0.03;
const height = 0.0065;
const border = 0.001;
const defaultcolor = [255,255,255,255];

let showHPStatus = 0;

mp.nametags.enabled = false;

mp.events.add('playerNametagSet', (target, r, g, b, a, armour) => {
    target.col = true;
    target.col_r = r;
    target.col_g = g;
    target.col_b = b;
    target.col_a = a;

    if(armour)
    {
        target.setArmour(armour);
    }
});

// HPBarStatus (0) := Never show HP & AR bars. (Lobby)
// HPBarStatus (1) := Always show HP & AR bars. (Copchase?)
// HPBarStatus (2) := Show only on aim. (DM/Freeroam?)

mp.events.add('showHPBars', (status) => {
    showHPStatus = status;
});

mp.events.add('render', (nametags) => {
    const graphics = mp.game.graphics;
    const screenRes = graphics.getScreenResolution(0, 0);
	
    nametags.forEach(nametag => {
        let [player, x, y, distance] = nametag;
		
        if(distance <= maxDistance) {	   
            let scale = (distance / maxDistance);
            if(scale < 0.6) scale = 0.6;
			
            var health = player.getHealth() / 100;
		   
            var armour = player.getArmour() / 100;
			
            y -= scale * (0.005 * (screenRes.y / 1080));
            
            if(player.col)
            {
                mp.game.graphics.drawText(player.name.replace('_', ' '), [x, y],
                {
                font: 4,
                color: [player.col_r, player.col_g, player.col_b, player.col_a],
                scale: [0.4, 0.4],
                outline: true
                });
            }
            else
            {
                mp.game.graphics.drawText(player.name.replace('_', ' '), [x, y],
                {
                font: 4,
                color: defaultcolor,
                scale: [0.4, 0.4],
                outline: true
                });
            }
            

            if(showHPStatus == 1 || (mp.game.player.isFreeAimingAtEntity(player.handle) && showHPStatus == 2))
            {

                let y2 = y + 0.056;
                
                graphics.drawRect(x, y2, width + border * 2, height + border * 2, 0, 0, 0, 200);
                graphics.drawRect(x, y2, width, height, 176, 12, 12, 255);
                graphics.drawRect(x - width / 2 * (1 - health), y2, width * health, height, 245, 73, 73, 200);
                
                if(armour > 0) 
                {
                    y2 -= 0.016;
                    graphics.drawRect(x, y2, width + border * 2, height + border * 2, 0, 0, 0, 200);
                    graphics.drawRect(x, y2, width, height, 143, 143, 143, 255);
                    graphics.drawRect(x - width / 2 * (1 - armour), y2, width * armour, height, 255, 255, 255, 200);
                }
            }
        }
    })
})