using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using GTANetworkAPI;
using RNC.Data;

namespace RNC.Data
{
    public class EnterableBuildingData
    {
        public Vector3 Position { get; set; }
        public float Heading { get; set; }
        public string Name { get; set; }
        public int BlipID { get; set; }
        public EnterableBuildingData(Vector3 position, float heading, string name, int blipId)
        {
            this.Position = position;
            this.Heading = heading;
            this.Name = name;
            this.BlipID = blipId;
        }
    }
    public class EnterableBuildings : Script
    {
        public static EnterableBuildingData[] EnterableBuildingPositions =
        {        
            new EnterableBuildingData(new Vector3(433.919f, -981.88116f, 30.709455f), 91.243935f , "Police Station", 60),
            new EnterableBuildingData(new Vector3(1855.289f, 3683.2783f, 34.26754f), -155.12216f , "Police Station", 60),
            new EnterableBuildingData(new Vector3(-443.17432f, 6016.3916f, 31.71221f), -58.92256f , "Police Station", 60),

            new EnterableBuildingData(new Vector3(417.98846f, -807.51514f, 29.397846f), 89.17556f, "Clothing Store", 73),
            new EnterableBuildingData(new Vector3(82.95728f, -1391.6437f, 29.411314f), -91.942116f, "Clothing Store", 73),
            new EnterableBuildingData(new Vector3(127.474014f, -210.47804f, 54.538425f), -17.514898f, "Clothing Store", 73),
            new EnterableBuildingData(new Vector3(-3168.0598f, 1056.835f, 20.858864f), 158.76701f, "Clothing Store", 73),
            new EnterableBuildingData(new Vector3(1197.9128f, 2702.575f, 38.156094f), -167.03397f, "Clothing Store", 73),
            new EnterableBuildingData(new Vector3(617.9219f, 2750.137f, 42.088955f), 5.2716217f, "Clothing Store", 73),
            new EnterableBuildingData(new Vector3(1686.2122f, 4820.5693f, 41.9779f), -3.139387f, "Clothing Store", 73),
            new EnterableBuildingData(new Vector3(-1.8723875f, 6517.2583f, 31.483122f), -81.96693f, "Clothing Store", 73),
            new EnterableBuildingData(new Vector3(-1095.2634f, 2705.6094f, 19.08352f), -97.42532f, "Clothing Store", 73),
            new EnterableBuildingData(new Vector3(-717.02545f, -156.80402f, 36.988018f), -135.40097f, "Clothing Store", 73),
            new EnterableBuildingData(new Vector3(-155.85936f, -305.98184f, 39.289597f), -13.635981f, "Clothing Store", 73),
            new EnterableBuildingData(new Vector3(-817.6786f, -1079.5449f, 11.132536f), 108.381096f, "Clothing Store", 73),
            new EnterableBuildingData(new Vector3(-1201.8987f, -778.27277f, 17.337143f), 36.595222f, "Clothing Store", 73),
            new EnterableBuildingData(new Vector3(-1456.3527f, -231.9416f, 49.34221f), -108.30579f, "Clothing Store", 73),

            new EnterableBuildingData(new Vector3(151.08084f, -1037.2583f, 29.339083f), -18.368605f, "Bank", 108),
            new EnterableBuildingData(new Vector3(230.26367f, 214.54152f, 105.55165f), 107.37271f, "Bank", 108),
            new EnterableBuildingData(new Vector3(1175.447f, 2702.9802f, 38.172596f), 79.56133f, "Bank", 108),
            new EnterableBuildingData(new Vector3(-111.0951f, 6462.488f, 31.640772f), 131.73488f, "Bank", 108),
            new EnterableBuildingData(new Vector3(-2966.3823f, 483.12125f, 15.692721f), 97.38031f, "Bank", 108),

            new EnterableBuildingData(new Vector3(-53.19084f, -1757.1958f, 29.439632f), 138.3869f, "General Store", 52),
            new EnterableBuildingData(new Vector3(-712.0335f, -917.12573f, 19.214228f), -165.72762f, "General Store", 52),
            new EnterableBuildingData(new Vector3(376.45834f, 322.9743f, 103.57246f), 164.76015f, "General Store", 52),
            new EnterableBuildingData(new Vector3(29.014181f, -1350.0109f, 29.330671f), 178.37935f, "General Store", 52),
            new EnterableBuildingData(new Vector3(-1226.6317f, -902.0572f, 12.284421f), 34.853443f, "General Store", 52),
            new EnterableBuildingData(new Vector3(1159.586f, -327.39114f, 69.2069f), 26.24761f, "General Store", 52),
            new EnterableBuildingData(new Vector3(1141.9082f, -980.9195f, 46.197273f), -82.31642f, "General Store", 52),
            new EnterableBuildingData(new Vector3(-3038.0398f, 589.6614f, 7.8159347f), -75.56125f, "General Store", 52),
            new EnterableBuildingData(new Vector3(-3239.455f, 1004.35754f, 12.613415f), -97.95817f, "General Store", 52),
            new EnterableBuildingData(new Vector3(-2974.5254f, 390.84995f, 15.032467f), 92.103386f, "General Store", 52),
            new EnterableBuildingData(new Vector3(-1821.9016f, 787.904f, 138.1819f), -119.5243f, "General Store", 52),
            new EnterableBuildingData(new Vector3(2559.8208f, 385.24298f, 108.62105f), -103.74505f, "General Store", 52),
            new EnterableBuildingData(new Vector3(543.9823f, 2673.5598f, 42.150967f), -164.34106f, "General Store", 52),
            new EnterableBuildingData(new Vector3(1394.3987f, 3599.277f, 35.007645f), -156.69781f, "General Store", 52),
            new EnterableBuildingData(new Vector3(1965.6084f, 3740.0222f, 32.324284f), 145.27957f, "General Store", 52),
            new EnterableBuildingData(new Vector3(2682.9756f, 3282.0818f, 55.238586f), -22.671528f, "General Store", 52),
            new EnterableBuildingData(new Vector3(1698.2491f, 4929.7495f, 42.078136f), -173.00418f, "General Store", 52),
            new EnterableBuildingData(new Vector3(1730.7263f, 6410.941f, 35.00061f), 152.38426f, "General Store", 52),
            new EnterableBuildingData(new Vector3(-1491.7097f, -384.13174f, 40.05992f), 145.3827f, "General Store", 52),

            new EnterableBuildingData(new Vector3(1990.7975f, 3053.9634f, 47.214928f), -179.2838f, "Bar", 93),
            new EnterableBuildingData(new Vector3(-564.56793f, 275.46957f, 83.036514f), 116.935356f, "Bar", 93),

            new EnterableBuildingData(new Vector3(-3163.8137f, 1082.067f, 20.847738f), 156.68716f, "Weapon Store", 110),
            new EnterableBuildingData(new Vector3(1699.7227f, 3751.9797f, 34.5176f), 62.504383f, "Weapon Store", 110),
            new EnterableBuildingData(new Vector3(-324.67267f, 6075.518f, 31.243692f), -20.820875f, "Weapon Store", 110),
            new EnterableBuildingData(new Vector3(17.042889f, -1115.6641f, 29.791164f), -150.31505f, "Weapon Store", 110),
            new EnterableBuildingData(new Vector3(243.7973f, -45.14607f, 69.89652f), 113.76118f, "Weapon Store", 110),
            new EnterableBuildingData(new Vector3(-1314.8613f, -390.05228f, 36.58421f), 125.82767f, "Weapon Store", 110),
            new EnterableBuildingData(new Vector3(-1112.4161f, 2689.9128f, 18.588762f), -65.482925f, "Weapon Store", 110),
            new EnterableBuildingData(new Vector3(-663.95325f, -945.24023f, 21.783993f), 135.03511f, "Weapon Store", 110),
            new EnterableBuildingData(new Vector3(812.01636f, -2147.6223f, 29.50994f), 35.41494f, "Weapon Store", 110),
            new EnterableBuildingData(new Vector3(844.0101f, -1023.5221f, 27.98121f), -175.35904f, "Weapon Store", 110),
            new EnterableBuildingData(new Vector3(2569.5264f, 304.4676f, 108.60786f), 120.68825f, "Weapon Store", 110),

            new EnterableBuildingData(new Vector3(-1149.9208f, -1521.7838f, 10.628015f), 32.143345f, "Safehouse", 40),
            new EnterableBuildingData(new Vector3(-817.43414f, 177.93681f, 72.22407f), 115.10763f, "Safehouse", 40),
            new EnterableBuildingData(new Vector3(2451.903f, 4969.29f, 46.571575f), 23.83546f, "Safehouse", 40),
            new EnterableBuildingData(new Vector3(8.432799f, 540.177f, 176.0273f), 162.03288f, "Safehouse", 40),
            new EnterableBuildingData(new Vector3(-14.277575f, -1442.3053f, 31.099993f), -7.2073927f, "Safehouse", 40),

            new EnterableBuildingData(new Vector3(-29.886766f, -146.67f, 57.006653f), -14.317797f, "Hair Salon", 71),
            new EnterableBuildingData(new Vector3(132.33678f, -1712.7504f, 29.302402f), -25.616528f, "Hair Salon", 71),
            new EnterableBuildingData(new Vector3(1934.2506f, 3724.4253f, 32.802742f), 29.181149f, "Hair Salon", 71),
            new EnterableBuildingData(new Vector3(-282.37573f, 6233.015f, 31.489086f), -126.68627f, "Hair Salon", 71),
            new EnterableBuildingData(new Vector3(-1288.5099f, -1116.5143f, 6.980809f), -78.46615f, "Hair Salon", 71),
            new EnterableBuildingData(new Vector3(1206.4691f, -470.36188f, 66.18448f), -110.0705f, "Hair Salon", 71),
            new EnterableBuildingData(new Vector3(-823.9467f, -188.37604f, 37.62022f), 16.1521f, "Hair Salon", 71),

            new EnterableBuildingData(new Vector3(128.80002f, -1298.5297f, 29.232729f), 120.83149f, "Strip Club", 121),

            new EnterableBuildingData(new Vector3(320.9853f, 177.96117f, 103.54415f), 157.75287f, "Tattoo Shop", 75),
            new EnterableBuildingData(new Vector3(1859.6602f, 3748.8728f, 33.032528f), 84.75233f, "Tattoo Shop", 75),
            new EnterableBuildingData(new Vector3(1321.3888f, -1649.578f, 52.14558f), -146.76845f, "Tattoo Shop", 75),
            new EnterableBuildingData(new Vector3(-3167.3445f, 1073.9807f, 20.848097f), 167.05197f, "Tattoo Shop", 75),
            new EnterableBuildingData(new Vector3(-289.08853f, 6200.185f, 31.4653f), 129.98734f, "Tattoo Shop", 75),
            new EnterableBuildingData(new Vector3(-1155.1997f, -1422.9274f, 4.784532f), 33.00279f, "Tattoo Shop", 75),

            new EnterableBuildingData(new Vector3(-2342.5286f, 3266.2368f, 32.827637f), -112.88373f, "Fort Zancudo Tower", 181),

            new EnterableBuildingData(new Vector3(-1082.0801f, -260.31897f, 37.80518f), -157.7062f, "Lifeinvader Offices", 475),

            new EnterableBuildingData(new Vector3(-252.85185f, -2025.1189f, 29.946032f), -17.70971f, "Maze Bank Arena", 557),

            new EnterableBuildingData(new Vector3(736.1084f, 132.5129f, 80.72056f), -120.125916f, "LSDWP", 769),

            new EnterableBuildingData(new Vector3(717.91974f, -976.4302f, 24.92134f), -102.15795f, "Garment Factory", 473),

            new EnterableBuildingData(new Vector3(1064.7393f, -2004.7765f, 32.06369f), 29.043642f, "Steel Foundry", 473),

            new EnterableBuildingData(new Vector3(-609.8029f, -1609.7666f, 26.898926f), -1.0767382f, "Scrap Yard & Recycling Center", 473),

            new EnterableBuildingData(new Vector3(-1589.2687f, -3230.5068f, 26.312695f), 148.57982f, "Airport Terminal", 758),

            new EnterableBuildingData(new Vector3(-1388.5024f, -586.8714f, 30.218977f), 35.01387f, "Nightclub", 136),
        };
    }
    /*

BAHAMA MAMAS
codename Position log: { new Vector3(-1388.5024f, -586.8714f, 30.218977f), 35.01387f },

POLICE STATIONS:
codename Position log: { new Vector3(433.919f, -981.88116f, 30.709455f), 91.243935f },
codename Position log: { new Vector3(1855.289f, 3683.2783f, 34.26754f), -155.12216f },
codename Position log: { new Vector3(-443.17432f, 6016.3916f, 31.71221f), -58.92256f },


CLOTHING SHOPS:
codename Position log: { new Vector3(417.98846f, -807.51514f, 29.397846f), 89.17556f },
codename Position log: { new Vector3(82.95728f, -1391.6437f, 29.411314f), -91.942116f },
codename Position log: { new Vector3(127.474014f, -210.47804f, 54.538425f), -17.514898f },
codename Position log: { new Vector3(-3168.0598f, 1056.835f, 20.858864f), 158.76701f },
codename Position log: { new Vector3(1197.9128f, 2702.575f, 38.156094f), -167.03397f },
codename Position log: { new Vector3(617.9219f, 2750.137f, 42.088955f), 5.2716217f },
codename Position log: { new Vector3(1686.2122f, 4820.5693f, 41.9779f), -3.139387f },
codename Position log: { new Vector3(-1.8723875f, 6517.2583f, 31.483122f), -81.96693f },
codename Position log: { new Vector3(-1095.2634f, 2705.6094f, 19.08352f), -97.42532f },
codename Position log: { new Vector3(-717.02545f, -156.80402f, 36.988018f), -135.40097f },
codename Position log: { new Vector3(-155.85936f, -305.98184f, 39.289597f), -13.635981f },
codename Position log: { new Vector3(-817.6786f, -1079.5449f, 11.132536f), 108.381096f },    
codename Position log: { new Vector3(-1201.8987f, -778.27277f, 17.337143f), 36.595222f },
codename Position log: { new Vector3(-1456.3527f, -231.9416f, 49.34221f), -108.30579f },


BANKS:
codename Position log: { new Vector3(151.08084f, -1037.2583f, 29.339083f), -18.368605f },
codename Position log: { new Vector3(230.26367f, 214.54152f, 105.55165f), 107.37271f },
codename Position log: { new Vector3(1175.447f, 2702.9802f, 38.172596f), 79.56133f },
codename Position log: { new Vector3(-111.0951f, 6462.488f, 31.640772f), 131.73488f },
codename Position log: { new Vector3(-2966.3823f, 483.12125f, 15.692721f), 97.38031f },

STORES:
codename Position log: { new Vector3(-53.19084f, -1757.1958f, 29.439632f), 138.3869f },
codename Position log: { new Vector3(-712.0335f, -917.12573f, 19.214228f), -165.72762f },
codename Position log: { new Vector3(376.45834f, 322.9743f, 103.57246f), 164.76015f },
codename Position log: { new Vector3(29.014181f, -1350.0109f, 29.330671f), 178.37935f },
codename Position log: { new Vector3(-1226.6317f, -902.0572f, 12.284421f), 34.853443f },
codename Position log: { new Vector3(1159.586f, -327.39114f, 69.2069f), 26.24761f },
codename Position log: { new Vector3(1141.9082f, -980.9195f, 46.197273f), -82.31642f },
codename Position log: { new Vector3(-3038.0398f, 589.6614f, 7.8159347f), -75.56125f },
codename Position log: { new Vector3(-3239.455f, 1004.35754f, 12.613415f), -97.95817f },
codename Position log: { new Vector3(-2974.5254f, 390.84995f, 15.032467f), 92.103386f },
codename Position log: { new Vector3(-1821.9016f, 787.904f, 138.1819f), -119.5243f },
codename Position log: { new Vector3(2559.8208f, 385.24298f, 108.62105f), -103.74505f },
codename Position log: { new Vector3(543.9823f, 2673.5598f, 42.150967f), -164.34106f },
codename Position log: { new Vector3(1394.3987f, 3599.277f, 35.007645f), -156.69781f },
codename Position log: { new Vector3(1965.6084f, 3740.0222f, 32.324284f), 145.27957f },
codename Position log: { new Vector3(2682.9756f, 3282.0818f, 55.238586f), -22.671528f },
codename Position log: { new Vector3(1698.2491f, 4929.7495f, 42.078136f), -173.00418f },
codename Position log: { new Vector3(1730.7263f, 6410.941f, 35.00061f), 152.38426f },
codename Position log: { new Vector3(-1491.7097f, -384.13174f, 40.05992f), 145.3827f },

BARS:
codename Position log: { new Vector3(1990.7975f, 3053.9634f, 47.214928f), -179.2838f },
codename Position log: { new Vector3(-564.56793f, 275.46957f, 83.036514f), 116.935356f },

GUN STORES:
codename Position log: { new Vector3(-3163.8137f, 1082.067f, 20.847738f), 156.68716f },
codename Position log: { new Vector3(1699.7227f, 3751.9797f, 34.5176f), 62.504383f },
codename Position log: { new Vector3(-324.67267f, 6075.518f, 31.243692f), -20.820875f },
codename Position log: { new Vector3(17.042889f, -1115.6641f, 29.791164f), -150.31505f },
codename Position log: { new Vector3(243.7973f, -45.14607f, 69.89652f), 113.76118f },
codename Position log: { new Vector3(-1314.8613f, -390.05228f, 36.58421f), 125.82767f },
codename Position log: { new Vector3(-1112.4161f, 2689.9128f, 18.588762f), -65.482925f },
codename Position log: { new Vector3(-663.95325f, -945.24023f, 21.783993f), 135.03511f }, 
codename Position log: { new Vector3(812.01636f, -2147.6223f, 29.50994f), 35.41494f },
codename Position log: { new Vector3(844.0101f, -1023.5221f, 27.98121f), -175.35904f },
codename Position log: { new Vector3(2569.5264f, 304.4676f, 108.60786f), 120.68825f },


HOUSES:
codename Position log: { new Vector3(-1149.9208f, -1521.7838f, 10.628015f), 32.143345f },
codename Position log: { new Vector3(-817.43414f, 177.93681f, 72.22407f), 115.10763f },
codename Position log: { new Vector3(2451.903f, 4969.29f, 46.571575f), 23.83546f },
codename Position log: { new Vector3(8.432799f, 540.177f, 176.0273f), 162.03288f },
codename Position log: { new Vector3(-14.277575f, -1442.3053f, 31.099993f), -7.2073927f },


BARBER SHOP:
codename Position log: { new Vector3(-29.886766f, -146.67f, 57.006653f), -14.317797f },
codename Position log: { new Vector3(132.33678f, -1712.7504f, 29.302402f), -25.616528f },
codename Position log: { new Vector3(1934.2506f, 3724.4253f, 32.802742f), 29.181149f },
codename Position log: { new Vector3(-282.37573f, 6233.015f, 31.489086f), -126.68627f },
codename Position log: { new Vector3(-1288.5099f, -1116.5143f, 6.980809f), -78.46615f },
codename Position log: { new Vector3(1206.4691f, -470.36188f, 66.18448f), -110.0705f },
codename Position log: { new Vector3(-823.9467f, -188.37604f, 37.62022f), 16.1521f },


STRIP CLUB
codename Position log: { new Vector3(128.80002f, -1298.5297f, 29.232729f), 120.83149f },

TATTOO SHOP:
codename Position log: { new Vector3(320.9853f, 177.96117f, 103.54415f), 157.75287f },
codename Position log: { new Vector3(1859.6602f, 3748.8728f, 33.032528f), 84.75233f }
codename Position log: { new Vector3(1321.3888f, -1649.578f, 52.14558f), -146.76845f },
codename Position log: { new Vector3(-3167.3445f, 1073.9807f, 20.848097f), 167.05197f },
codename Position log: { new Vector3(-289.08853f, 6200.185f, 31.4653f), 129.98734f },
codename Position log: { new Vector3(-1155.1997f, -1422.9274f, 4.784532f), 33.00279f },


FORT ZANCUDO TOWER:
codename Position log: { new Vector3(-2342.5286f, 3266.2368f, 32.827637f), -112.88373f },

LIFE-INVADER OFFICE:
codename Position log: { new Vector3(-1082.0801f, -260.31897f, 37.80518f), -157.7062f },

MAZE BANK ARENA:
codename Position log: { new Vector3(-252.85185f, -2025.1189f, 29.946032f), -17.70971f },

LSDWP:
codename Position log: { new Vector3(736.1084f, 132.5129f, 80.72056f), -120.125916f },

GARMENT FACTORY:
codename Position log: { new Vector3(717.91974f, -976.4302f, 24.92134f), -102.15795f },

FACTORY FROM FINAL MISSION IDK:
codename Position log: { new Vector3(1064.7393f, -2004.7765f, 32.06369f), 29.043642f },

SCRAPYARD:
codename Position log: { new Vector3(-609.8029f, -1609.7666f, 26.898926f), -1.0767382f },

SOME AIRPORT ROOM:
codename Position log: { new Vector3(-1589.2687f, -3230.5068f, 26.312695f), 148.57982f },

*/
}
