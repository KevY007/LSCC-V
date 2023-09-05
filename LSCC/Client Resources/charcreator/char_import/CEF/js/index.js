var XMLInput = null;
var FloatingLabel = null;

var buttonMale = null;
var buttonFemale = null;
var buttonCancel = null;

var XMLData = "";

document.addEventListener("DOMContentLoaded", () => {
    XMLInput = document.getElementById("xml_data_input");
    FloatingLabel = document.getElementById("floating_label");

    buttonMale = document.getElementById("buttonMale");
    buttonFemale = document.getElementById("buttonFemale");
    buttonCancel = document.getElementById("buttonCancel");

    buttonMale.addEventListener("click", () => {onButtonPress(0)});
    buttonFemale.addEventListener("click", () => {onButtonPress(1)});
    buttonCancel.addEventListener("click", () => {onButtonPress(2)});

    XMLInput.addEventListener("input", onXMLInput);
});

function onXMLInput(e){
    updateFloatingLabelStyle();

    XMLData = XMLInput.value;
}
function updateFloatingLabelStyle(){
    if(XMLInput.value.length == 0){
        FloatingLabel.classList.add("floating-label");
        FloatingLabel.classList.remove("floating-label-top");
    }
    else{
        FloatingLabel.classList.add("floating-label-top");
        FloatingLabel.classList.remove("floating-label");
    }
}
function onButtonPress(type){
    if(type === 3){
        mp.trigger('character_import_close');
        return;
    }

    try{
        const XMLObject = parseXml(XMLData);
        const jsonResult = xml2json(XMLObject);
        const fixedJsonResult = jsonResult.replace("undefined", "");
        const JSONObject = JSON.parse(fixedJsonResult);

        if(JSONObject.html){
            mp.trigger('character_import_data', (type == 0 ? 'M' : 'F'), 'There was an error parsing the file (invalid XML)!');
        }
        else{
            mp.trigger('character_import_data', (type == 0 ? 'M' : 'F'), fixedJsonResult);
        }
    }
    catch{
        mp.trigger('character_import_data', (type == 0 ? 'M' : 'F'), 'There was a fatal error parsing the file (invalid XML or JSON)!');
    }
}