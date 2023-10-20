function adjustTextarea(e){
    e.style.height = "1px";
    e.style.height = (e.scrollHeight-25)+"px";
  }
var HelpBoxText = document.getElementById("help_box_text");
var HelpBoxWrapper = document.getElementById("help_box_wrapper");
var HelpBox = document.getElementById("help_box");
var HelpBoxClose = document.getElementById("help_box_close");
HelpBoxClose.addEventListener("click", (e) => hideHelpBox());

function showHelpBox(text){
    HelpBoxWrapper.style.visibility = "visible";

    HelpBoxText.innerHTML = text;
    adjustTextarea(HelpBoxText);
}
function hideHelpBox(){
    HelpBoxWrapper.style.visibility = "hidden";
}