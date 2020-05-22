var passElement = document.getElementById("passwordText");

mp.events.call("TogglePlayerChat", false);

passElement.addEventListener("keyup", function(e) {

  if (e.keyCode === 13) 
  {

    e.preventDefault();

    if(document.getElementById("loginButton")) document.getElementById("loginButton").click();
    else if(document.getElementById("registerButton")) document.getElementById("registerButton").click();
  }
});

$('#loginButton').click(() => {

    mp.trigger('loginInformationToServer', $('#emailText').val(), $('#passwordText').val());
    document.getElementById("emailText").value = "";
});

$('#registerButton').click(() => {


    mp.trigger('registerInformationToServer', $('#emailText').val(), $('#usernameText').val(), $('#passwordText').val(), $('#passwordTextConfirm').val());
    document.getElementById("emailText").value = "";
    document.getElementById("usernameText").value = "";
});