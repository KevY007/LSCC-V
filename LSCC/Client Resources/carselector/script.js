function addCar(carImage, carName, levelReq, carDesc, locked) {
    //Main card
    let card = document.createElement("div");
    card.classList.add("card", "custom-car-body");

    if(locked === false)
    {
        card.classList.add("unlocked-border");
    }
    else 
    {
        card.classList.add("locked-border");
    }
    //Car Image
    let imgElement = document.createElement("img");
    imgElement.src = carImage;
    imgElement.setAttribute("class", "card-img-top");
    card.appendChild(imgElement);
    //Card Body
    let cardbody = document.createElement("div");
    cardbody.setAttribute("class", "card-body");
    card.appendChild(cardbody);
    //Car Name & Level
    //Name
    let cardCarName = document.createElement("h5");
    cardCarName.setAttribute("class", "card-title");
    let cardCarNameText = document.createTextNode(carName + " ");
    cardCarName.appendChild(cardCarNameText);
    cardbody.appendChild(cardCarName);
    //Level
    let cardCarLevel = document.createElement("span");
    cardCarLevel.classList.add("badge", "badge-secondary");
    let cardCarLevelText = document.createTextNode(levelReq);
    cardCarLevel.appendChild(cardCarLevelText);
    cardCarName.appendChild(cardCarLevel);
    //Card Description
    let cardDesc = document.createElement("p");
    cardDesc.setAttribute("class", "card-text");
    let cardDescText =  document.createTextNode(carDesc);
    cardDesc.appendChild(cardDescText);
    cardbody.appendChild(cardDesc);
    //Card Button
    let cardButton = document.createElement("button");
    cardButton.type = "button";
    cardButton.classList.add("btn", "btn-primary");
    let cardButtonText = document.createTextNode("SELECT");
    cardButton.appendChild(cardButtonText);
    cardbody.appendChild(cardButton);

    if(locked === true)
    {
        cardButton.disabled = true;
    }
    else
    {
        cardButton.onclick = function()
        {
            mp.events.call("OnSelectCar", 1, carName);
        };
    }
    //Card END

    document.getElementById("carshere").appendChild(card);

}