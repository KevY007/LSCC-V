const {Language:English} = require('./Languages/Client/Game/English');

function TranslateMessage(message_code, args){
    if(!args) args = [];

    var LANG = null;
    // switch case
    LANG = English;

    const keys = Object.keys(LANG);
    if(!keys.includes(message_code)){
        return `[ERROR] This message is not defined: ${message_code} | Language: English`;
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