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
    carbrowser = mp.browsers.new("package://carselector/index.html");

    carList = JSON.parse(cars);
    mp.gui.cursor.show(true, true);
});

mp.events.add("OnSelectCar", (carName) => {
    mp.events.call("hideCarSelector");
    mp.events.callRemote("OnCarSelectorSelect", carName);
});

mp.events.add('browserDomReady', (ebrowser) => {
    if(carbrowser != null && carbrowser === ebrowser)
    {
        mp.gui.cursor.show(true, true);

        for (let i = 0; i < carList.Cars.length; i++) {
            carbrowser.execute(`addCar("` + carList.Images[i] + `", "` + carList.Cars[i] + `", "` + carList.Levels[i] + `", "` + carList.Desc[i] + `", ` + carList.Locked[i] + `);`);
        }
    }
});