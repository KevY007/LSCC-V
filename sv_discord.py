import discord
import requests, json 
import time
import asyncio


lastUpReport = time.time()

lastDataUpdate = time.time()-5
lastServData = -1

lastChannelUpdate = time.time()-50
lastChannelPlayers = -1

lastMentionTime = time.time()-900
lastMentionPlayers = 0

currentPlayers = 0

client = discord.Client()

async def update_task():
    await client.wait_until_ready()

    global lastChannelUpdate
    global lastChannelPlayers
    global currentPlayers
    global lastServData
    global lastDataUpdate
    global lastMentionTime
    global lastMentionPlayers

    print('[Updater] Started updater task!')
    while not client.is_closed():
        
        if(currentPlayers <= 1 and lastMentionTime > time.time()):
            lastMentionTime = time.time() - 600
            print('[Updater/Notifier] Player are less than/equal to one and time is a lot. Resetting time to notify on next >= 2 players.')

        if(time.time() - lastMentionTime >= 900):
            if(currentPlayers >= 2):
                if(lastMentionPlayers == 0 or currentPlayers >= (lastMentionPlayers * 2)):
                    print('[Updater/Notifier] Notifying players of ingame players due to PB double/ no last player mentions.')
                    notifyChan = client.get_channel(796389588880588830) # game-notify chan.
                    await notifyChan.send(f'<@&{796386517525725224}> **There are {currentPlayers} players ingame.** \n')

                    lastMentionTime = time.time()
                    lastMentionPlayers = currentPlayers
                else:
                    print('[Updater/Notifier] Have less players than last mention, reducing threshold by half but increasing time to half an hour.')
                    lastMentionPlayers = lastMentionPlayers / 2
                    lastMentionTime = time.time() + (1800 - 900)

        if(time.time() - lastChannelUpdate >= 60):
            
            if(time.time() - lastDataUpdate >= 10):
                    req = requests.get(url='https://cdn.rage.mp/master/')
                    lastServData = req.json()
                    lastDataUpdate = time.time()
                    currentPlayers = lastServData["51.210.171.99:22005"]["players"]
                    print('[Updater] Refreshing data list')
            
            if("51.210.171.99:22005" in lastServData):
                if(lastChannelPlayers != currentPlayers):
                    pCount = client.get_channel(717405966123139104) # Player count chanID
                    await pCount.edit(name=f'Players Online: {currentPlayers}')
                    lastChannelPlayers = currentPlayers
            else:
                pCount = client.get_channel(717405966123139104) # Player count chanID
                await pCount.edit(name=f'Server is offline')
                lastChannelPlayers = -1

            print('[Updater] Players count updated.')
            lastChannelUpdate = time.time()
        
        await asyncio.sleep(5) # task runs every 5 seconds
    
    print('[Updater] Updater exiting...')


@client.event
async def on_ready():
    print('Bot is now ready! ' + client.user.name + '\n===============================================\n')
    client.loop.create_task(update_task())

@client.event
async def on_message(message):
    if message.author == client.user:
        return

    if message.content == "!up":
        global lastUpReport
        global lastDataUpdate
        global lastServData
        global currentPlayers

        if(time.time() - lastUpReport < 2):
            return

        if(time.time() - lastDataUpdate >= 10):
            req = requests.get(url='https://cdn.rage.mp/master/')
            lastServData = req.json()
            lastDataUpdate = time.time()
            currentPlayers = lastServData["51.210.171.99:22005"]["players"]
            print('[Cmd/Up] Refreshing data list')

        lastUpReport = time.time()

        print('[Cmd/Up] Taking !up cmd from: ' + message.author.name)
        if("51.210.171.99:22005" in lastServData):
            await message.channel.send(f':white_check_mark:  The server is online with **{currentPlayers}** players! (Peak since restart: **{lastServData["51.210.171.99:22005"]["peak"]}**)')
        else:
            await message.channel.send(":negative_squared_cross_mark:  **The server appears to be offline or shut down for maintenance!**")
        

client.run('NjAyNTQ1Mzc0MDY1OTgzNTgw.XtaCEQ.clpqVL5c0X_38xkT-mYTr4_N9a4')