module.exports = [
  {
    start: `player.TriggerEvent("pushMsgToChat"`,
    end: `);`,
  },
  {
    start: `EPlayer.SendMessage(`,
    end: `);`,
  },
  {
    start: `EPlayer.SendMessageToAll(`,
    end: `);`,
  },
  {
    start: `EPlayer.SendMessageToAllAdmins(`,
    end: `);`,
  },
  {
    start: `Logs.LogAdmin(`,
    end: `);`,
  },
  {
    start: `Logs.LogSession(`,
    end: `);`,
  },
  {
    start: `Logs.AddToAdminRecord(`,
    end: `);`,
  },
  {
    start: `pushMessageToChat(`,
    end: `);`,
  },
];
