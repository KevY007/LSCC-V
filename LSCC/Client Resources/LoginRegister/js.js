document.addEventListener("DOMContentLoaded", () => {
    setTimeout(() => {
       requestAutoLoginFromServer();
    }, 4000); // pro animation skill 8-)

    document.getElementById("btn_login").addEventListener("click", (e) => {onLoginPress(e)});
    document.getElementById("btn_register").addEventListener("click", (e) => {onRegisterPress(e)});
    document.getElementById("btn_forgotusername").addEventListener("click", (e) => {onForgotUsernamePress(e)});
    document.getElementById("btn_autologin").addEventListener("click", (e) => {onAutoLoginPress(e)});
});

var autoLoginData = undefined;

function requestAutoLoginFromServer(){
    mp.trigger('requestAutoLoginData');
}
function receiveAutoLoginFromServer(data){
    autoLoginData = data;
    if(autoLoginData !== '0'){
        document.getElementById("btn_autologin").innerHTML = document.getElementById("btn_autologin").innerHTML.replace("[USER]", `<strong>${data}</strong>`);
    }
    else{
        document.getElementsByClassName("autologin")[0].remove();
    }

    document.getElementById("loading_text").remove();
    document.getElementsByClassName("loading_screen")[0].remove();
    document.getElementById("logreg").className = "logreg_root";
}

function onLoginPress(event){
    let username = document.getElementById("username").value;
    let password = document.getElementById("password").value;
    
    mp.trigger("loginInformationToServer", username, password);
}
function onAutoLoginPress(event){
    mp.trigger("autologinToServer", autoLoginData);
}
function onRegisterPress(event){
    let username = document.getElementById("username").value;
    let password = document.getElementById("password").value;

     mp.trigger("registerInformationToServer", "", username, password, true, false) // email prazan jbg test purposes
}

function onForgotUsernamePress(event){
    mp.trigger("requestForgotUsernameServer");
}
function receiveForgotUsernameData(data){
    if(data.length > 0){
        for(let i = 0 ; i < data.length ; i++){
            document.getElementById("username_list").innerHTML += `<li>${data[i]}</li`;
        }
    }
    else{
        document.getElementById("forgot-username-list").innerHTML = `<p>No usernames could be found. Please open a ticket on our Discord server</p>`
    }
}