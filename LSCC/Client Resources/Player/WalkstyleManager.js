const {TranslateMessage: Translate} = require('./Translator');

const walkingStyles = [
    {Name: "Normal", AnimSet: null},
    {Name: "Brave", AnimSet: "move_m@brave"},
    {Name: "Confident", AnimSet: "move_m@confident"},
    {Name: "Drunk", AnimSet: "move_m@drunk@verydrunk"},
    {Name: "Fat", AnimSet: "move_m@fat@a"},
    {Name: "Gangster", AnimSet: "move_m@shadyped@a"},
    {Name: "Hurry", AnimSet: "move_m@hurry@a"},
    {Name: "Injured", AnimSet: "move_m@injured"},
    {Name: "Intimidated", AnimSet: "move_m@intimidation@1h"},
    {Name: "Quick", AnimSet: "move_m@quick"},
    {Name: "Sad", AnimSet: "move_m@sad@a"},
    {Name: "Tough", AnimSet: "move_m@tool_belt@a"}
];

mp.events.add("Walkstyle:RequestSetCS", (styleName) => {
    const walkstyle = walkingStyles.find(w => w.Name.toLowerCase() == styleName.toLowerCase());
    if(!walkstyle) return;

    mp.events.callRemote("Walkstyle:Set", walkstyle.AnimSet, true);
});

mp.events.add('entityStreamIn', (entity) => {
    if(entity.type !== 'player') return;

    setWalkstyle(entity, entity.getVariable("Walkstyle"));
});

mp.events.addDataHandler("Walkstyle", (entity, value, oldValue) => {
    if(entity.type !== 'player') return;
    mp.events.callRemote('ServerConsoleOutput', `[${mp.players.local.name}] ${entity.name} walkstyle changed to: ${value}`);

    setWalkstyle(entity, value);
});

let unfuckCounter = 0;
async function setWalkstyle(player, style) {
    if (!style || style == "null") {
        player.resetMovementClipset(1.0);
    } else {
        if (!mp.game.streaming.hasClipSetLoaded(style)) {
            mp.game.streaming.requestClipSet(style);
            for (let i = 0; !mp.game.streaming.hasClipSetLoaded(style) && i < 1500; i++) await mp.game.waitAsync(0);
        }
        player.setMovementClipset(style, 1.0);
    }
}
var IsInjuredWalkstyleSet = false;
mp.events.add("OneSecondEvent", () => {
    if(mp.players.local.getVariable("Team") != 1 && mp.players.local.getVariable("Team") != 0) return;
    const HP = mp.players.local.getHealth();

    if(HP > 50 && IsInjuredWalkstyleSet == true){
        IsInjuredWalkstyleSet = false;
        mp.events.callRemote("Walkstyle:Set", "null", false);
        return;    
    }
    if(HP <= 50 && IsInjuredWalkstyleSet == false){
        IsInjuredWalkstyleSet = true;
        mp.events.callRemote("Walkstyle:Set", "move_m@injured", false);
        mp.gui.chat.push(Translate("UNDER_50_HP_WARNING"));
        return;
    }
   
   
});