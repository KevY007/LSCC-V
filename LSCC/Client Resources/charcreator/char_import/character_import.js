var global = require('./globals');

mp.events.add('character_import_open', () => {
    if(global.CharacterImportCEF){
        global.CharacterImportCEF.destroy();
        global.CharacterImportCEF = null;
    }

    global.CharacterImportCEF = mp.browsers.new('package://charcreator/char_import/CEF/index.html');
    global.CharacterImportCEF.active = true;
    mp.gui.cursor.show(true, true);

    if(global.ServerUI) global.ServerUI.call('hud_toggle', false);
});
mp.events.add('character_import_close', () => {
    if(global.CharacterImportCEF){
        global.CharacterImportCEF.destroy();
        global.CharacterImportCEF = null;
    }
    mp.gui.cursor.show(false, false);
    if(global.ServerUI) global.ServerUI.call('hud_toggle', true);
});
mp.events.add('character_import_data', (gender, data) => {
    if(data.startsWith("There was")){
        mp.events.callLocal("HUDError", data);
        return;
    }
});