function rgb(r, g, b){ // im lazy
	return ["rgb(",r,",",g,",",b,")"].join("");
}

function createUser(lid, name, level, ping, color_r, color_g, color_b) {
	//var playerCont = document.createElement("div");
	var trfirst = document.createElement("tr");
	var thLID = document.createElement("th");
	var tdName = document.createElement("td");
	var tdLevel = document.createElement("td");
	var tdPing = document.createElement("td");
	trfirst.setAttribute("id", name);

	var LID = document.createTextNode(lid);
	var Name = document.createTextNode(name);
	var Level = document.createTextNode(level);
	var Ping = document.createTextNode(ping);

	thLID.appendChild(LID);
	tdName.appendChild(Name);
	tdLevel.appendChild(Level);
	tdPing.appendChild(Ping);

	trfirst.appendChild(thLID);
	trfirst.appendChild(tdName);
	trfirst.appendChild(tdLevel);
	trfirst.appendChild(tdPing);

	document.getElementById("playershere").appendChild(trfirst);
	
	updateUser(lid, name, level, ping, color_r, color_g, color_b);
};

function removeUser(userRemove) {
	if (document.getElementById(userRemove) !== null) {
		var elmnt = document.getElementById(userRemove);
		elmnt.remove();

	} else {
		console.log('No user by that name!')
	}
};

function updateServerName(name) {
	if (document.getElementById("servername") !== null) {
		var element = document.getElementById("servername");
		if(element.innerHTML != name) element.innerHTML = name;
	} else {
		console.log('Failed to update server name.')
	}
};

function updateUser(userLID, userName, userLevel, userPing, color_r, color_g, color_b) {
	if (document.getElementById(userName) !== null) {
		var userElement = document.getElementById(userName);
		var editThLID = userElement.getElementsByTagName('th')[0];
		var editTdLevel = userElement.getElementsByTagName('td')[1];
		var editTdPing = userElement.getElementsByTagName('td')[2];
		editThLID.innerHTML = userLID;
		editTdLevel.innerHTML = userLevel;
		editTdPing.innerHTML = userPing;

		if(userElement && userElement.style) userElement.style.color = rgb(color_r, color_g, color_b);
	} else {
		console.log('No user by that name!')
	}
};