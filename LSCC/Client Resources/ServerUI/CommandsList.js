// update this when adding new commands
var CommandsDataJSON = [
  {
    "Name": "serveroptions",
    "Usage": "[USAGE] serveroptions (option) | use 'help' to get all options",
    "Admin": 4,
    "Donator": 0,
    "Parameters": [
      "option",
      " use 'help' to get all options"
    ]
  },
  {
    "Name": "dbgcreatepickup",
    "Usage": "",
    "Admin": 1,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "dbgcreateammo",
    "Usage": "",
    "Admin": 6,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "dbgcreateammocar",
    "Usage": "",
    "Admin": 6,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "trunk",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "lockpick",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "lock",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "applycomps",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "applyvehmods",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "engine",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "seatbelt",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "time",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "game",
    "Usage": "[USAGE] game (option: stop, start, autostart, teambalance, mode)",
    "Admin": 2,
    "Donator": 0,
    "Parameters": [
      "option: stop, start, autostart, teambalance, mode"
    ]
  },
  {
    "Name": "tog",
    "Usage": "[USAGE] tog (option: hud, pm, radio, ooc)",
    "Admin": 0,
    "Donator": 0,
    "Parameters": [
      "option: hud, pm, radio, ooc"
    ]
  },
  {
    "Name": "info",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "verify",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "aveh",
    "Usage": "[USAGE] aveh (optional: vehicle model) (optional: pri. col) (optional: sec. col)",
    "Admin": 1,
    "Donator": 0,
    "Parameters": [
      "optional: vehicle model",
      "optional: pri. col",
      "optional: sec. col"
    ]
  },
  {
    "Name": "veh",
    "Usage": "[USAGE] veh (vehicle model) (optional: pri. color) (optional: sec. color)",
    "Admin": 0,
    "Donator": 0,
    "Parameters": [
      "vehicle model",
      "optional: pri. color",
      "optional: sec. color"
    ]
  },
  {
    "Name": "mytime",
    "Usage": "[USAGE] mytime (hour)",
    "Admin": 0,
    "Donator": 0,
    "Parameters": [
      "hour"
    ]
  },
  {
    "Name": "myweather",
    "Usage": "[USAGE] myweather (weatherid)",
    "Admin": 0,
    "Donator": 0,
    "Parameters": [
      "weatherid"
    ]
  },
  {
    "Name": "togblip",
    "Usage": "",
    "Admin": 3,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "godmode",
    "Usage": "",
    "Admin": 3,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "afob",
    "Usage": "",
    "Admin": 3,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "setspawn",
    "Usage": "",
    "Admin": 3,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "gps",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "dl",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "motd",
    "Usage": "[USAGE] motd (player name)(message | 'none' to delete)",
    "Admin": 2,
    "Donator": 0,
    "Parameters": [
      "player name",
      "message | 'none' to delete"
    ]
  },
  {
    "Name": "motdall",
    "Usage": "[USAGE] motdall (message | 'none' to delete)",
    "Admin": 3,
    "Donator": 0,
    "Parameters": [
      "message | 'none' to delete"
    ]
  },
  {
    "Name": "goto",
    "Usage": "[USAGE] goto (target)",
    "Admin": 0,
    "Donator": 0,
    "Parameters": [
      "target"
    ]
  },
  {
    "Name": "bring",
    "Usage": "[USAGE] bring (target)",
    "Admin": 1,
    "Donator": 0,
    "Parameters": [
      "target"
    ]
  },
  {
    "Name": "bringall",
    "Usage": "",
    "Admin": 3,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "placespikes",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "archiveaccounts",
    "Usage": "",
    "Admin": 4,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "freezeall",
    "Usage": "",
    "Admin": 3,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "unfreezeall",
    "Usage": "",
    "Admin": 3,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "sethpall",
    "Usage": "[USAGE] sethpall (health) (armor)",
    "Admin": 3,
    "Donator": 0,
    "Parameters": [
      "health",
      "armor"
    ]
  },
  {
    "Name": "noclip",
    "Usage": "[USAGE] noclip",
    "Admin": 2,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "setalpha",
    "Usage": "[USAGE] setalpha (target) (0-255)",
    "Admin": 3,
    "Donator": 0,
    "Parameters": [
      "target",
      "0-255"
    ]
  },
  {
    "Name": "setcaralpha",
    "Usage": "[USAGE] setcaralpha (target) (0-255)",
    "Admin": 3,
    "Donator": 0,
    "Parameters": [
      "target",
      "0-255"
    ]
  },
  {
    "Name": "chances",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "togcols",
    "Usage": "[USAGE] togcols (target)",
    "Admin": 2,
    "Donator": 0,
    "Parameters": [
      "target"
    ]
  },
  {
    "Name": "sethp",
    "Usage": "[USAGE] sethp (target) (health)",
    "Admin": 1,
    "Donator": 0,
    "Parameters": [
      "target",
      "health"
    ]
  },
  {
    "Name": "viewscreenshot",
    "Usage": "[USAGE] viewss (identifier) | Use 'last' for last taken screenshot",
    "Admin": 1,
    "Donator": 0,
    "Parameters": [
      "identifier",
      " Use 'last' for last taken screenshot"
    ]
  },
  {
    "Name": "takescreenshot",
    "Usage": "[USAGE] takescreenshot (target) (quality = 20) | Quality 20 is default",
    "Admin": 5,
    "Donator": 0,
    "Parameters": [
      "target",
      "quality = 20",
      " Quality 20 is default"
    ]
  },
  {
    "Name": "takegif",
    "Usage": "[USAGE] takegif (target) (duration in seconds) (fps = 24) | Leave FPS to 24 or it'll be bad",
    "Admin": 5,
    "Donator": 0,
    "Parameters": [
      "target",
      "duration in seconds",
      "fps = 24",
      " Leave FPS to 24 or it'll be bad"
    ]
  },
  {
    "Name": "setinv",
    "Usage": "[USAGE] setinv(ulnerable) (target) (0 or 1)",
    "Admin": 1,
    "Donator": 0,
    "Parameters": [
      "ulnerable",
      "target",
      "0 or 1"
    ]
  },
  {
    "Name": "setar",
    "Usage": "[USAGE] setar(mor) (target) (armor)",
    "Admin": 1,
    "Donator": 0,
    "Parameters": [
      "mor",
      "target",
      "armor"
    ]
  },
  {
    "Name": "stopanim",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "blipdbg",
    "Usage": "",
    "Admin": 5,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "painkillers",
    "Usage": "[USAGE] pk (option: buy/use/inv)",
    "Admin": 0,
    "Donator": 0,
    "Parameters": [
      "option: buy/use/inv"
    ]
  },
  {
    "Name": "medkit",
    "Usage": "[USAGE] mk (option: buy/use/inv)",
    "Admin": 0,
    "Donator": 0,
    "Parameters": [
      "option: buy/use/inv"
    ]
  },
  {
    "Name": "handsup",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "cuff",
    "Usage": "[USAGE] cuff (optional:target)",
    "Admin": 0,
    "Donator": 0,
    "Parameters": [
      "optional:target"
    ]
  },
  {
    "Name": "dragout",
    "Usage": "[USAGE] dragout (optional:target)",
    "Admin": 0,
    "Donator": 0,
    "Parameters": [
      "optional:target"
    ]
  },
  {
    "Name": "switchseats",
    "Usage": "[USAGE] switchseats (target)",
    "Admin": 0,
    "Donator": 0,
    "Parameters": [
      "target"
    ]
  },
  {
    "Name": "forceswitchseats",
    "Usage": "[USAGE] forceswitchseats (player) (target)",
    "Admin": 1,
    "Donator": 0,
    "Parameters": [
      "player",
      "target"
    ]
  },
  {
    "Name": "takecomp",
    "Usage": "[USAGE] takecomp (target) (weapon/hash) (component/hash)",
    "Admin": 3,
    "Donator": 0,
    "Parameters": [
      "target",
      "weapon/hash",
      "component/hash"
    ]
  },
  {
    "Name": "rules",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "givecomp",
    "Usage": "[USAGE] givecomp (target) (weapon/hash) (component/hash)",
    "Admin": 3,
    "Donator": 0,
    "Parameters": [
      "target",
      "weapon/hash",
      "component/hash"
    ]
  },
  {
    "Name": "weaponeditor",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "attachcomp",
    "Usage": "[USAGE] attachcomp (target) (component/hash)",
    "Admin": 3,
    "Donator": 0,
    "Parameters": [
      "target",
      "component/hash"
    ]
  },
  {
    "Name": "removecomp",
    "Usage": "[USAGE] removecomp (component/hash - hold weapon and use 'help' to list available)",
    "Admin": 0,
    "Donator": 0,
    "Parameters": [
      "component/hash - hold weapon and use 'help' to list available"
    ]
  },
  {
    "Name": "convertpoints",
    "Usage": "[USAGE] convertpoints (amount) (standard to donator | donator to standard)",
    "Admin": 0,
    "Donator": 0,
    "Parameters": [
      "amount",
      "standard to donator | donator to standard"
    ]
  },
  {
    "Name": "mycomps",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "givegundbg",
    "Usage": "",
    "Admin": 5,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "givegun",
    "Usage": "[USAGE] givegun (target) (ammo) (weapon)",
    "Admin": 3,
    "Donator": 0,
    "Parameters": [
      "target",
      "ammo",
      "weapon"
    ]
  },
  {
    "Name": "gun",
    "Usage": "[USAGE] gun (ammo) (weapon)",
    "Admin": 0,
    "Donator": 0,
    "Parameters": [
      "ammo",
      "weapon"
    ]
  },
  {
    "Name": "world",
    "Usage": "[USAGE] world (number/id, 0 is default)",
    "Admin": 0,
    "Donator": 0,
    "Parameters": [
      "number/id, 0 is default"
    ]
  },
  {
    "Name": "givegunall",
    "Usage": "[USAGE] givegunall (ammo) (wep)",
    "Admin": 3,
    "Donator": 0,
    "Parameters": [
      "ammo",
      "wep"
    ]
  },
  {
    "Name": "fix",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "afix",
    "Usage": "[USAGE] afix (target)",
    "Admin": 1,
    "Donator": 0,
    "Parameters": [
      "target"
    ]
  },
  {
    "Name": "flip",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "aflip",
    "Usage": "[USAGE] aflip (target)",
    "Admin": 1,
    "Donator": 0,
    "Parameters": [
      "target"
    ]
  },
  {
    "Name": "removeavehs",
    "Usage": "",
    "Admin": 1,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "autojoin",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "driveby",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "stats",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "checkstats",
    "Usage": "",
    "Admin": 2,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "gethp",
    "Usage": "",
    "Admin": 1,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "getfps",
    "Usage": "",
    "Admin": 1,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "pos",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "voicemode",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "voiceblock",
    "Usage": "[USAGE] voiceblock (target)",
    "Admin": 0,
    "Donator": 0,
    "Parameters": [
      "target"
    ]
  },
  {
    "Name": "kill",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "lobby",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "mylobby",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "clientcmd",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "packetloss",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "restartmenu",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "anotes",
    "Usage": "[USAGE] anote (target) [add|view|delete]",
    "Admin": 1,
    "Donator": 0,
    "Parameters": [
      "target",
      "view|delete]",
      "delete]",
      "text",
      "text",
      "Note ID | use 'view' to check",
      "target",
      "view|delete]",
      "delete]"
    ]
  },
  {
    "Name": "lobbies",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "switchlobby",
    "Usage": "[USAGE] switchlobby (lobbyid) (optional: password)",
    "Admin": 1,
    "Donator": 0,
    "Parameters": [
      "lobbyid",
      "optional: password"
    ]
  },
  {
    "Name": "dmarenas",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "realmendm",
    "Usage": "[USAGE] realmendm (arenaid) (teamid)",
    "Admin": 0,
    "Donator": 0,
    "Parameters": [
      "arenaid",
      "teamid"
    ]
  },
  {
    "Name": "dm",
    "Usage": "[USAGE] dm (arenaid)",
    "Admin": 0,
    "Donator": 0,
    "Parameters": [
      "arenaid"
    ]
  },
  {
    "Name": "freeroam",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "fcmds",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "derby",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "setweather",
    "Usage": "[USAGE] setweather (weatherid)",
    "Admin": 1,
    "Donator": 1,
    "Parameters": [
      "weatherid"
    ]
  },
  {
    "Name": "settime",
    "Usage": "[USAGE] settime (hour)",
    "Admin": 1,
    "Donator": 1,
    "Parameters": [
      "hour"
    ]
  },
  {
    "Name": "gotopos",
    "Usage": "[USAGE] gotopos (x) (y) (z) (optional: heading)",
    "Admin": 3,
    "Donator": 0,
    "Parameters": [
      "x",
      "y",
      "z",
      "optional: heading"
    ]
  },
  {
    "Name": "getip",
    "Usage": "[USAGE] getip (target)",
    "Admin": 3,
    "Donator": 0,
    "Parameters": [
      "target"
    ]
  },
  {
    "Name": "getsc",
    "Usage": "[USAGE] getsc (target)",
    "Admin": 3,
    "Donator": 0,
    "Parameters": [
      "target"
    ]
  },
  {
    "Name": "gethwid",
    "Usage": "[USAGE] gethwid (target)",
    "Admin": 3,
    "Donator": 0,
    "Parameters": [
      "target"
    ]
  },
  {
    "Name": "setdimension",
    "Usage": "[USAGE] setdimension (target) (vw)",
    "Admin": 2,
    "Donator": 0,
    "Parameters": [
      "target",
      "vw"
    ]
  },
  {
    "Name": "sendtolobby",
    "Usage": "[USAGE] sendtolobby (target)",
    "Admin": 1,
    "Donator": 0,
    "Parameters": [
      "target"
    ]
  },
  {
    "Name": "resetclothing",
    "Usage": "[USAGE] resetclothing (target) (cop/fugitive)",
    "Admin": 1,
    "Donator": 0,
    "Parameters": [
      "target",
      "cop/fugitive"
    ]
  },
  {
    "Name": "putinveh",
    "Usage": "[USAGE] putinveh (target) (player) (optional: seatid) | \\",
    "Admin": 1,
    "Donator": 0,
    "Parameters": [
      "target",
      "player",
      "optional: seatid",
      " \\"
    ]
  },
  {
    "Name": "restock",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "keys",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "teleport",
    "Usage": "[USAGE] teleport (option: chilliad, airport)",
    "Admin": 0,
    "Donator": 0,
    "Parameters": [
      "option: chilliad, airport"
    ]
  },
  {
    "Name": "dbgplayanim",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "dbglinkmane",
    "Usage": "",
    "Admin": 6,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "dbgsetcol",
    "Usage": "",
    "Admin": 6,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "setgamevehicle",
    "Usage": "",
    "Admin": 5,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "resetgamevehicles",
    "Usage": "",
    "Admin": 1,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "resetcharacter",
    "Usage": "",
    "Admin": 2,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "dbgexitlinkmane",
    "Usage": "",
    "Admin": 6,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "dbgteamvar",
    "Usage": "",
    "Admin": 5,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "dbgmaxspeed",
    "Usage": "",
    "Admin": 4,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "dbgdriveforce",
    "Usage": "",
    "Admin": 4,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "report",
    "Usage": "[USAGE] report (target) (desc)",
    "Admin": 0,
    "Donator": 0,
    "Parameters": [
      "target",
      "desc"
    ]
  },
  {
    "Name": "helpme",
    "Usage": "[USAGE] helpme (desc)",
    "Admin": 0,
    "Donator": 0,
    "Parameters": [
      "desc"
    ]
  },
  {
    "Name": "car",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "customvehlist",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "cmds",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "purchaseammo",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "bancheck",
    "Usage": "[USAGE] bancheck (target name or ban id)",
    "Admin": 1,
    "Donator": 0,
    "Parameters": [
      "target name or ban id"
    ]
  },
  {
    "Name": "ocheckstats",
    "Usage": "[USAGE] ocheckstats (target)",
    "Admin": 2,
    "Donator": 0,
    "Parameters": [
      "target"
    ]
  },
  {
    "Name": "unsuspend",
    "Usage": "[USAGE] unsuspend (account name)",
    "Admin": 2,
    "Donator": 0,
    "Parameters": [
      "account name"
    ]
  },
  {
    "Name": "changename",
    "Usage": "[USAGE] changename (account name) (new name)",
    "Admin": 4,
    "Donator": 0,
    "Parameters": [
      "account name",
      "new name"
    ]
  },
  {
    "Name": "deleteaccount",
    "Usage": "[USAGE] deleteaccount (account name) | THIS WILL DELETE ALL LOGS AND ALL KINDS OF DATA ON THEM",
    "Admin": 4,
    "Donator": 0,
    "Parameters": [
      "account name",
      " THIS WILL DELETE ALL LOGS AND ALL KINDS OF DATA ON THEM"
    ]
  },
  {
    "Name": "changepass",
    "Usage": "[USAGE] changepass (account name) (new password)",
    "Admin": 4,
    "Donator": 0,
    "Parameters": [
      "account name",
      "new password"
    ]
  },
  {
    "Name": "changemypass",
    "Usage": "[USAGE] changemypass (new password)",
    "Admin": 0,
    "Donator": 0,
    "Parameters": [
      "new password"
    ]
  },
  {
    "Name": "hotwire",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "checkplayer",
    "Usage": "[USAGE] checkplayer (account name) | Checks & gets all associated accounts with a player",
    "Admin": 1,
    "Donator": 0,
    "Parameters": [
      "account name",
      " Checks & gets all associated accounts with a player"
    ]
  },
  {
    "Name": "checkbanevaders",
    "Usage": "",
    "Admin": 2,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "unban",
    "Usage": "[USAGE] unban (banid or name)",
    "Admin": 2,
    "Donator": 0,
    "Parameters": [
      "banid or name"
    ]
  },
  {
    "Name": "oban",
    "Usage": "[USAGE] oban (target name) (reason)",
    "Admin": 2,
    "Donator": 0,
    "Parameters": [
      "target name",
      "reason"
    ]
  },
  {
    "Name": "ban",
    "Usage": "[USAGE] ban (name) (reason)",
    "Admin": 2,
    "Donator": 0,
    "Parameters": [
      "name",
      "reason"
    ]
  },
  {
    "Name": "suspend",
    "Usage": "[USAGE] suspend (target)",
    "Admin": 2,
    "Donator": 0,
    "Parameters": [
      "target"
    ]
  },
  {
    "Name": "createlobby",
    "Usage": "",
    "Admin": 2,
    "Donator": 2,
    "Parameters": []
  },
  {
    "Name": "deletelobby",
    "Usage": "[USAGE] deletelobby (lobbyid)",
    "Admin": 2,
    "Donator": 2,
    "Parameters": [
      "lobbyid"
    ]
  },
  {
    "Name": "setlobbypassword",
    "Usage": "[USAGE] setlobbypassword (lobbyid) (password)",
    "Admin": 2,
    "Donator": 2,
    "Parameters": [
      "lobbyid",
      "password"
    ]
  },
  {
    "Name": "lobbymode",
    "Usage": "[USAGE] setlobbymode (lobbyid) (mode) | Mode = 'standard' or 'busted'",
    "Admin": 2,
    "Donator": 2,
    "Parameters": [
      "lobbyid",
      "mode",
      " Mode = 'standard' or 'busted'"
    ]
  },
  {
    "Name": "setlobbyfugitivepercent",
    "Usage": "[USAGE] setlobbyfugitivepercent (lobbyid) (percent)",
    "Admin": 2,
    "Donator": 2,
    "Parameters": [
      "lobbyid",
      "percent"
    ]
  },
  {
    "Name": "togglelobby",
    "Usage": "[USAGE] toglobby (lobbyid)",
    "Admin": 2,
    "Donator": 0,
    "Parameters": [
      "lobbyid"
    ]
  },
  {
    "Name": "togglemap",
    "Usage": "[USAGE] togmap (map id/name)",
    "Admin": 2,
    "Donator": 0,
    "Parameters": [
      "map id/name"
    ]
  },
  {
    "Name": "nextmap",
    "Usage": "[USAGE] nextmap (map ID or name)",
    "Admin": 1,
    "Donator": 0,
    "Parameters": [
      "map ID or name"
    ]
  },
  {
    "Name": "saveplayer",
    "Usage": "",
    "Admin": 4,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "saveserver",
    "Usage": "",
    "Admin": 4,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "reloadaccount",
    "Usage": "",
    "Admin": 5,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "squadmembers",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "squadaccept",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "squadleave",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "removelanguage",
    "Usage": "[USAGE] removelanguage (language) | /languages for a list",
    "Admin": 4,
    "Donator": 0,
    "Parameters": [
      "language",
      " /languages for a list"
    ]
  },
  {
    "Name": "addlanguage",
    "Usage": "[USAGE] addlanguage (language name)",
    "Admin": 4,
    "Donator": 0,
    "Parameters": [
      "language name"
    ]
  },
  {
    "Name": "languages",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "language",
    "Usage": "[USAGE] language (language or 'none') | /languages for a list",
    "Admin": 0,
    "Donator": 0,
    "Parameters": [
      "language or 'none'",
      " /languages for a list"
    ]
  },
  {
    "Name": "languagechat",
    "Usage": "[USAGE] languagechat (/lc) (text)",
    "Admin": 0,
    "Donator": 0,
    "Parameters": [
      "/lc",
      "text"
    ]
  },
  {
    "Name": "squadchat",
    "Usage": "[USAGE] squadchat (sc) (text)",
    "Admin": 0,
    "Donator": 0,
    "Parameters": [
      "sc",
      "text"
    ]
  },
  {
    "Name": "squad",
    "Usage": "[USAGE] squad (target)",
    "Admin": 0,
    "Donator": 0,
    "Parameters": [
      "target"
    ]
  },
  {
    "Name": "fugitivelist",
    "Usage": "",
    "Admin": 2,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "lastlogin",
    "Usage": "[USAGE] lastlogin (full username)",
    "Admin": 1,
    "Donator": 0,
    "Parameters": [
      "full username"
    ]
  },
  {
    "Name": "ivangej",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "reqlog",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "acmds",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "agivepoints",
    "Usage": "[USAGE] agivepoints (target) (points)",
    "Admin": 4,
    "Donator": 0,
    "Parameters": [
      "target",
      "points"
    ]
  },
  {
    "Name": "grantxp",
    "Usage": "[USAGE] grantxp (target) (xp)",
    "Admin": 4,
    "Donator": 0,
    "Parameters": [
      "target",
      "xp"
    ]
  },
  {
    "Name": "setteam",
    "Usage": "[USAGE] setteam (target) (team)",
    "Admin": 1,
    "Donator": 0,
    "Parameters": [
      "target",
      "team"
    ]
  },
  {
    "Name": "maps",
    "Usage": "",
    "Admin": 5,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "addmapspawn",
    "Usage": "[USAGE] addmapspawn (mapid) (teamid)",
    "Admin": 4,
    "Donator": 0,
    "Parameters": [
      "mapid",
      "teamid"
    ]
  },
  {
    "Name": "deletemapspawn",
    "Usage": "[USAGE] deletemapspawn (mapid) (spawnid)",
    "Admin": 4,
    "Donator": 0,
    "Parameters": [
      "mapid",
      "spawnid"
    ]
  },
  {
    "Name": "mapspawns",
    "Usage": "[USAGE] mapspawns (mapid)",
    "Admin": 4,
    "Donator": 0,
    "Parameters": [
      "mapid"
    ]
  },
  {
    "Name": "deletemap",
    "Usage": "[USAGE] deletemap (mapname)",
    "Admin": 4,
    "Donator": 0,
    "Parameters": [
      "mapname"
    ]
  },
  {
    "Name": "createmap",
    "Usage": "[USAGE] createmap (name)",
    "Admin": 4,
    "Donator": 0,
    "Parameters": [
      "name"
    ]
  },
  {
    "Name": "createdmarena",
    "Usage": "[USAGE] createdmarena (name)",
    "Admin": 4,
    "Donator": 0,
    "Parameters": [
      "name"
    ]
  },
  {
    "Name": "selectdmarena",
    "Usage": "[USAGE] selectdmarena (name)",
    "Admin": 4,
    "Donator": 0,
    "Parameters": [
      "name"
    ]
  },
  {
    "Name": "editdmarena",
    "Usage": "[USAGE] editdmarena (help)",
    "Admin": 4,
    "Donator": 0,
    "Parameters": [
      "help"
    ]
  },
  {
    "Name": "triggerevent",
    "Usage": "",
    "Admin": 6,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "triggerjs",
    "Usage": "",
    "Admin": 6,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "triggerjsall",
    "Usage": "",
    "Admin": 6,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "triggerjsplayer",
    "Usage": "",
    "Admin": 6,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "vtuner",
    "Usage": "[USAGE] vtuner (cop / fugitive / help)",
    "Admin": 0,
    "Donator": 0,
    "Parameters": [
      "cop / fugitive / help",
      "cop / fugitive / help",
      "cop / fugitive / help"
    ]
  },
  {
    "Name": "tuneveh",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "forward",
    "Usage": "",
    "Admin": 1,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "testspike",
    "Usage": "",
    "Admin": 6,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "cameraset",
    "Usage": "",
    "Admin": 6,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "dbgunlockchal",
    "Usage": "",
    "Admin": 6,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "dbgresetchallenges",
    "Usage": "",
    "Admin": 6,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "dbgdiscordverify",
    "Usage": "",
    "Admin": 6,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "unlockitem",
    "Usage": "",
    "Admin": 6,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "setadmin",
    "Usage": "[USAGE] setadmin (target) (level)",
    "Admin": 4,
    "Donator": 0,
    "Parameters": [
      "target",
      "level"
    ]
  },
  {
    "Name": "osetadmin",
    "Usage": "[USAGE] osetadmin (target) (level)",
    "Admin": 4,
    "Donator": 0,
    "Parameters": [
      "target",
      "level"
    ]
  },
  {
    "Name": "ogivedonorpoints",
    "Usage": "[USAGE] ogivedonorpoints (target) (points)",
    "Admin": 5,
    "Donator": 0,
    "Parameters": [
      "target",
      "points"
    ]
  },
  {
    "Name": "givedonorpoints",
    "Usage": "[USAGE] givedonorpoints (target) (points)",
    "Admin": 5,
    "Donator": 0,
    "Parameters": [
      "target",
      "points"
    ]
  },
  {
    "Name": "redeem",
    "Usage": "[USAGE] redeem (code)",
    "Admin": 0,
    "Donator": 0,
    "Parameters": [
      "code"
    ]
  },
  {
    "Name": "forcelogin",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "namechange",
    "Usage": "[USAGE] namechange (new name)",
    "Admin": 0,
    "Donator": 0,
    "Parameters": [
      "new name"
    ]
  },
  {
    "Name": "setdonator",
    "Usage": "[USAGE] setdonator (target) (level)",
    "Admin": 5,
    "Donator": 0,
    "Parameters": [
      "target",
      "level"
    ]
  },
  {
    "Name": "osetdonator",
    "Usage": "[USAGE] osetdonator (target) (level)",
    "Admin": 5,
    "Donator": 0,
    "Parameters": [
      "target",
      "level"
    ]
  },
  {
    "Name": "dc",
    "Usage": "[USAGE] donatorchat (text)",
    "Admin": 0,
    "Donator": 1,
    "Parameters": [
      "text"
    ]
  },
  {
    "Name": "id",
    "Usage": "[USAGE] id (Id/PartOfName)",
    "Admin": 0,
    "Donator": 0,
    "Parameters": [
      "Id/PartOfName"
    ]
  },
  {
    "Name": "afk",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "adminchat",
    "Usage": "[USAGE] adminchat (text)",
    "Admin": 1,
    "Donator": 0,
    "Parameters": [
      "text"
    ]
  },
  {
    "Name": "looc",
    "Usage": "[USAGE] looc (text)",
    "Admin": 0,
    "Donator": 0,
    "Parameters": [
      "text"
    ]
  },
  {
    "Name": "low",
    "Usage": "[USAGE] low (text)",
    "Admin": 0,
    "Donator": 0,
    "Parameters": [
      "text"
    ]
  },
  {
    "Name": "cw",
    "Usage": "[USAGE] carwhisper (text)",
    "Admin": 0,
    "Donator": 0,
    "Parameters": [
      "text"
    ]
  },
  {
    "Name": "s",
    "Usage": "[USAGE] shout (text)",
    "Admin": 0,
    "Donator": 0,
    "Parameters": [
      "text"
    ]
  },
  {
    "Name": "m",
    "Usage": "[USAGE] megaphone (text)",
    "Admin": 0,
    "Donator": 0,
    "Parameters": [
      "text"
    ]
  },
  {
    "Name": "r",
    "Usage": "[USAGE] radio (text)",
    "Admin": 0,
    "Donator": 0,
    "Parameters": [
      "text"
    ]
  },
  {
    "Name": "admins",
    "Usage": "",
    "Admin": 3,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "ooc",
    "Usage": "[USAGE] ooc (text)",
    "Admin": 1,
    "Donator": 1,
    "Parameters": [
      "text"
    ]
  },
  {
    "Name": "lobbychat",
    "Usage": "[USAGE] lobbychat (text)",
    "Admin": 1,
    "Donator": 1,
    "Parameters": [
      "text"
    ]
  },
  {
    "Name": "ann",
    "Usage": "[USAGE] ann (text)",
    "Admin": 2,
    "Donator": 0,
    "Parameters": [
      "text"
    ]
  },
  {
    "Name": "anonymous",
    "Usage": "",
    "Admin": 2,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "pm",
    "Usage": "[USAGE] pm (target) (text)",
    "Admin": 0,
    "Donator": 0,
    "Parameters": [
      "target",
      "text"
    ]
  },
  {
    "Name": "passgun",
    "Usage": "[USAGE] passgun (target)",
    "Admin": 0,
    "Donator": 0,
    "Parameters": [
      "target"
    ]
  },
  {
    "Name": "removeguns",
    "Usage": "[USAGE] removeguns (target)",
    "Admin": 2,
    "Donator": 0,
    "Parameters": [
      "target"
    ]
  },
  {
    "Name": "removegunsfromall",
    "Usage": "",
    "Admin": 3,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "freeze",
    "Usage": "[USAGE] freeze (target)",
    "Admin": 1,
    "Donator": 0,
    "Parameters": [
      "target"
    ]
  },
  {
    "Name": "rfreeze",
    "Usage": "[USAGE] rfreeze (range)",
    "Admin": 1,
    "Donator": 0,
    "Parameters": [
      "range"
    ]
  },
  {
    "Name": "tfreeze",
    "Usage": "[USAGE] tfreeze (team 0 = fugitive, 1 = cop)",
    "Admin": 1,
    "Donator": 0,
    "Parameters": [
      "team 0 = fugitive, 1 = cop"
    ]
  },
  {
    "Name": "tunfreeze",
    "Usage": "[USAGE] tunfreeze (team 0 = fugitive, 1 = cop)",
    "Admin": 1,
    "Donator": 0,
    "Parameters": [
      "team 0 = fugitive, 1 = cop"
    ]
  },
  {
    "Name": "runfreeze",
    "Usage": "[USAGE] runfreeze (range)",
    "Admin": 1,
    "Donator": 0,
    "Parameters": [
      "range"
    ]
  },
  {
    "Name": "unfreeze",
    "Usage": "[USAGE] unfreeze (target)",
    "Admin": 1,
    "Donator": 0,
    "Parameters": [
      "target"
    ]
  },
  {
    "Name": "kick",
    "Usage": "[USAGE] kick (target) (reason)",
    "Admin": 1,
    "Donator": 0,
    "Parameters": [
      "target",
      "reason"
    ]
  },
  {
    "Name": "ojail",
    "Usage": "[USAGE] ojail (target) (minutes) (reason)",
    "Admin": 1,
    "Donator": 0,
    "Parameters": [
      "target",
      "minutes",
      "reason"
    ]
  },
  {
    "Name": "jail",
    "Usage": "[USAGE] jail (target) (minutes) (reason)",
    "Admin": 1,
    "Donator": 0,
    "Parameters": [
      "target",
      "minutes",
      "reason"
    ]
  },
  {
    "Name": "unjail",
    "Usage": "[USAGE] unjail (target)",
    "Admin": 1,
    "Donator": 0,
    "Parameters": [
      "target"
    ]
  },
  {
    "Name": "ounjail",
    "Usage": "[USAGE] ojail (target)",
    "Admin": 1,
    "Donator": 0,
    "Parameters": [
      "target"
    ]
  },
  {
    "Name": "warn",
    "Usage": "[USAGE] warn (target) (reason)",
    "Admin": 1,
    "Donator": 0,
    "Parameters": [
      "target",
      "reason"
    ]
  },
  {
    "Name": "unwarn",
    "Usage": "[USAGE] unwarn (target)",
    "Admin": 1,
    "Donator": 0,
    "Parameters": [
      "target"
    ]
  },
  {
    "Name": "mute",
    "Usage": "[USAGE] mute (target) (mins) (reason)",
    "Admin": 1,
    "Donator": 0,
    "Parameters": [
      "target",
      "mins",
      "reason"
    ]
  },
  {
    "Name": "omute",
    "Usage": "[USAGE] omute (target) (minutes) (reason)",
    "Admin": 1,
    "Donator": 0,
    "Parameters": [
      "target",
      "minutes",
      "reason"
    ]
  },
  {
    "Name": "ounmute",
    "Usage": "[USAGE] ounmute (target)",
    "Admin": 1,
    "Donator": 0,
    "Parameters": [
      "target"
    ]
  },
  {
    "Name": "unmute",
    "Usage": "[USAGE] unmute (target)",
    "Admin": 1,
    "Donator": 0,
    "Parameters": [
      "target"
    ]
  },
  {
    "Name": "me",
    "Usage": "[USAGE] me (action)",
    "Admin": 0,
    "Donator": 0,
    "Parameters": [
      "action"
    ]
  },
  {
    "Name": "do",
    "Usage": "[USAGE] do (action)",
    "Admin": 0,
    "Donator": 0,
    "Parameters": [
      "action"
    ]
  },
  {
    "Name": "forceautojoin",
    "Usage": "[USAGE] forceautojoin (target)",
    "Admin": 1,
    "Donator": 0,
    "Parameters": [
      "target"
    ]
  },
  {
    "Name": "slap",
    "Usage": "[USAGE] slap (target)",
    "Admin": 1,
    "Donator": 0,
    "Parameters": [
      "target"
    ]
  },
  {
    "Name": "showshotstats",
    "Usage": "[USAGE] showshotstats (target)",
    "Admin": 1,
    "Donator": 0,
    "Parameters": [
      "target"
    ]
  },
  {
    "Name": "resetshotstats",
    "Usage": "[USAGE] resetshotstats (target)",
    "Admin": 1,
    "Donator": 0,
    "Parameters": [
      "target"
    ]
  },
  {
    "Name": "shotstats",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "grantfugitive",
    "Usage": "[USAGE] grantfugitive (target)",
    "Admin": 3,
    "Donator": 0,
    "Parameters": [
      "target"
    ]
  },
  {
    "Name": "outfit",
    "Usage": "[USAGE] outfit (option: save, select)",
    "Admin": 0,
    "Donator": 0,
    "Parameters": [
      "option: save, select"
    ]
  },
  {
    "Name": "clothing",
    "Usage": "[USAGE] clothing (option: reset, cop, fugitive)",
    "Admin": 0,
    "Donator": 0,
    "Parameters": [
      "option: reset, cop, fugitive"
    ]
  },
  {
    "Name": "dbgsetclothing",
    "Usage": "",
    "Admin": 5,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "char",
    "Usage": "[USAGE] character (option: reset, edit, help, [reset]clothing, lobbyteam)",
    "Admin": 0,
    "Donator": 0,
    "Parameters": [
      "option: reset, edit, help, [reset]clothing, lobbyteam"
    ]
  },
  {
    "Name": "requests",
    "Usage": "",
    "Admin": 1,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "acceptreport",
    "Usage": "[USAGE] acceptreport (requestid)",
    "Admin": 1,
    "Donator": 0,
    "Parameters": [
      "requestid"
    ]
  },
  {
    "Name": "denyreport",
    "Usage": "[USAGE] denyreport (requestid) (reason)",
    "Admin": 1,
    "Donator": 0,
    "Parameters": [
      "requestid",
      "reason"
    ]
  },
  {
    "Name": "nametags",
    "Usage": "",
    "Admin": 3,
    "Donator": 0,
    "Parameters": []
  },
  {
    "Name": "spec",
    "Usage": "[USAGE] spec (target)",
    "Admin": 1,
    "Donator": 0,
    "Parameters": [
      "target"
    ]
  },
  {
    "Name": "loadout",
    "Usage": "",
    "Admin": 0,
    "Donator": 0,
    "Parameters": []
  }
];