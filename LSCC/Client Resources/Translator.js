const {Language:English} = require('./Languages/Client/Game/English');
const {Language:Spanish} = require('./Languages/Client/Game/Spanish');
const {Language:French} = require('./Languages/Client/Game/French');
const {Language:Vietnamese} = require('./Languages/Client/Game/Vietnamese');
const {Language:Croatian} = require('./Languages/Client/Game/Croatian');
var global = require('./globals');

var CURRENT_LANGUAGE = 0;
const VALID_LANGUAGES = [0,1,2,3,4];
mp.events.add("Translator:ChangeLanguage", (languageID, onJoin) => {
  const realLanguageID = Number(languageID);
  if(!VALID_LANGUAGES.includes(realLanguageID)){
    CURRENT_LANGUAGE = 0;
    mp.storage.data.language = 0;
    mp.gui.chat.push(`Your language (${realLanguageID}) was invalid! Setting you back to English.`);
    return;
  }
  mp.storage.data.language = realLanguageID;
  mp.storage.flush();
  CURRENT_LANGUAGE = realLanguageID;
  
  if(onJoin === false){
    let notifyscoreboardstr = `toast('${Translate("CS_TEST", [mp.players.local.name])}', '', 'nothing', 6500, 2);`;
    if(global.ServerUI) global.ServerUI.execute(notifyscoreboardstr);

    mp.events.callLocal("ToggleTooltips", !mp.storage.data.menu.tooltips);
    setTimeout(() => {
        mp.events.callLocal("ToggleTooltips", !mp.storage.data.menu.tooltips);
    }, 100);
  }
  mp.events.callRemote("Translator:SetLanguageServer", realLanguageID, !onJoin);
  updateNametagsMessages();
  setTimeout(() => {
    updateUILanguage();
  }, 100);

});

function getLanguageObject(){
    var LANG = null;

    switch(CURRENT_LANGUAGE){
        case 0: LANG = English; break;
        case 1: LANG = Spanish; break;
        case 2: LANG = French; break;
        case 3: LANG = Croatian; break;
        case 4: LANG = Vietnamese; break;
        default: LANG = English; break;
    }
    return LANG;
}
function updateUILanguage(){
    if(!global.ServerUI) return;
    if(global.ServerUI === undefined || global.ServerUI === null) return;

    //switch(CURRENT_LANGUAGE){
    //    default: if(global.ServerUI) global.ServerUI.execute(`gm.$refs.helpers.setLocale("en")`); break;
    //    case 0: if(global.ServerUI) global.ServerUI.execute(`gm.$refs.helpers.setLocale("en")`); break;
    //    case 1: if(global.ServerUI) global.ServerUI.execute(`gm.$refs.helpers.setLocale("es")`); break;
    //    case 2: if(global.ServerUI) global.ServerUI.execute(`gm.$refs.helpers.setLocale("fr")`); break;
    //    case 3: if(global.ServerUI) global.ServerUI.execute(`gm.$refs.helpers.setLocale("hr")`); break;
    //    case 4: if(global.ServerUI) global.ServerUI.execute(`gm.$refs.helpers.setLocale("vi")`); break;
    //}
}
mp.events.add("Translator:UpdateNametagMessagesManual", () => {
    updateNametagsMessages();
})
function updateNametagsMessages(){
    const LANG = getLanguageObject();
    var NAMETAG_MESSAGES = {
        "PLAYER_SHOT": LANG.PLAYER_SHOT,
        "TASE_PROTECTION": LANG.TASE_PROTECTION,
        "LADDER_PROTECTION": LANG.LADDER_PROTECTION,
        "PLAYER_INJURED": LANG.PLAYER_INJURED,
        "ENEMY_INJURED": LANG.ENEMY_INJURED
    }
    mp.events.callLocal("Translator:UpdateNametagMessages", JSON.stringify(NAMETAG_MESSAGES));
}

function TranslateMessage(message_code, args){
    if(!args) args = [];

    const LANG = getLanguageObject();

    const keys = Object.keys(LANG);
    if(!keys.includes(message_code)){
        return `[ERROR] This message is not defined: ${message_code} | Language: ${CURRENT_LANGUAGE}`;
    }
    const unformattedMsg = LANG[message_code];
    const toReturn = ApplyStringArguments(unformattedMsg, args);
    return toReturn;
}

function ApplyStringArguments(text, args){
    if(args.length == 0) return text;

    var currentArgIdx = 0;
    var isFormat = false;
    var toReturn = "";
    for(let i = 0 ; i < text.length ; i++){
        if (text[i] == '{')
        {
            if (text[i + 1] != '#')
            {
                isFormat = true;
            }
        }

        if (!isFormat) toReturn += text[i];

        if (text[i] == '}' && isFormat)
        {
            isFormat = false;

            if (currentArgIdx < args.length)
            {
                toReturn += args[currentArgIdx];
                currentArgIdx++;
            }
            else
            {
                toReturn += "{MISSING TRANSLATION ARGUMENT}";
            }

        }
    }
    return toReturn;
}

exports.TranslateMessage = TranslateMessage;