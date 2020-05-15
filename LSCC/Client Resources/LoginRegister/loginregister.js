$('#loginButton').click(() => {

    mp.trigger('loginInformationToServer', $('#loginEmailText').val(), $('#loginPasswordText').val());
    document.getElementById("loginPasswordText").value = "";
    document.getElementById("loginEmailText").value = "";
});

$('#registerButton').click(() => {
    mp.trigger('registerInformationToServer', $('#registerEmailText').val(), $('#registerUsernameText').val(), $('#registerPasswordText').val(), $('#allcorrect').prop("checked"), $('#rules').prop("checked"));
    document.getElementById("registerEmailText").value = "";
    document.getElementById("registerUsernameText").value = "";
    document.getElementById("registerPasswordText").value = "";
});