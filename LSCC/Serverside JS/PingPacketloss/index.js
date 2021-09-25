mp.events.add("UpdatePacketloss", (player) => {
    player.setVariable("Packetloss", player.packetLoss);
    player.setVariable("Ping", player.ping);
});