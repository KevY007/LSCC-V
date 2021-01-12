let carbrowser = null;
let carList = null;

mp.events.add("hideCarSelector", () =>
{
    if (carbrowser != null) {
        
        carbrowser.destroy();
        carbrowser = null;

        mp.gui.cursor.show(false, false);
        
        mp.events.call("TogglePlayerChat", true);
        return;
    }
});

mp.events.add("showCarSelector", (cars) => {

    mp.events.callLocal("hideCarSelector");

    mp.events.call("TogglePlayerChat", false);
    carList = JSON.parse(cars);
    carbrowser = mp.browsers.new("package://carselector/index.html");

    mp.gui.cursor.show(true, true);
});

mp.events.add("OnSelectCar", (team, carName) => {
    mp.events.call("hideCarSelector");
    mp.events.callRemote("OnCarSelectorSelect", team, carName);
});

mp.keys.bind(
    0x1B,
    false,
    () => {
        if(carbrowser != null) mp.events.call("hideCarSelector");
    }
);

mp.events.add('browserDomReady', (ebrowser) => {
    if(carbrowser != null && carbrowser === ebrowser)
    {
        mp.gui.cursor.show(true, true);

        // There exists a 'carList.Teams[i]' too now
        for (let i = 0; i < carList.Cars.length; i++) {
            carbrowser.execute(`addCar("` + carList.Cars[i] + `", "` + carList.Levels[i] + `", "` + carList.Images[i] + `", "` + carList.Teams[i] + `", ` + carList.Locked[i] + `);`);
        }
    }
});