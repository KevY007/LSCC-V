using Discord;
using Discord.WebSocket;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace RNC
{
    public class DiscordBot : Script
    {
        private static Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private static DiscordSocketClient client;
        private static ulong gameChatChannelId = 717768143140814928;
        private static ulong guildId = 713731146072064040;
        private static List<CommandDefinition> commands = new List<CommandDefinition>();

        public static async Task Start()
        {
            client = new DiscordSocketClient();
            //client.Log += Log;

            await client.LoginAsync(TokenType.Bot, Settings.DCBotToken);
            await client.StartAsync();

            client.Ready += async () =>
            {
                Console.WriteLine($"[Discord] Bot started and logged in as {client.CurrentUser.Username}");
                await SendAlert("Server started.");

                await RegisterCommands(client);
            };
            client.SlashCommandExecuted += SlashCommandHandler;
        }
        public enum EmbedType
        {
            Info,
            Warning,
            Error,
            Success
        }
        public static Embed embed(string message, EmbedType type = EmbedType.Info)
        {
            var embed = new EmbedBuilder();
            var emoji = "";
            switch (type)
            {
                case EmbedType.Info:
                    embed.Color = Discord.Color.Blue;
                    emoji = ":blue_circle: ";
                    break;
                case EmbedType.Warning:
                    embed.Color = Discord.Color.Orange;
                    emoji = ":orange_circle: ​ ​ ";
                    break;
                case EmbedType.Error:
                    embed.Color = Discord.Color.Red;
                    emoji = ":red_circle:  ";
                    break;
                case EmbedType.Success:
                    embed.Color = Discord.Color.Green;
                    emoji = ":green_circle:  ";
                    break;
            }
            embed.Description = $"{emoji} \u200B \u200B {message}";
            return embed.Build();
        }
        public static async Task SendMessage(string message)
        {
            var channel = client.GetChannel(gameChatChannelId) as IMessageChannel;
            if (channel == null)
            {
                Console.WriteLine($"[Discord] Game chat channel not found!");
                return;
            }
            await channel.SendMessageAsync(message);
        }

        public static async Task SendAlert(string message, EmbedType type = EmbedType.Info)
        {
            var channel = client.GetChannel(gameChatChannelId) as IMessageChannel;
            if (channel == null)
            {
                Console.WriteLine($"[Discord] Game chat channel not found!");
                return;
            }

            await channel.SendMessageAsync("", false, embed(message, type));
        }
        private static async Task SlashCommandHandler(SocketSlashCommand command)
        {
            var foundCommand = commands.Find(cmd => cmd.Meta.Name.Value == command.Data.Name);

            if (foundCommand != null)
            {
                await foundCommand.Handler(command);
            }
            else
            {
                await command.RespondAsync("", new Embed[] { embed("Unkwnown command!", EmbedType.Error) });
            }
        }
        public static SocketGuild GetGuild(DiscordSocketClient client)
        {
            var guild = client.GetGuild(guildId);

            if (guild == null)
            {
                Console.WriteLine($"[Discord] Guild not found!");
            }

            return guild;
        }
        public static async Task RegisterCommands(DiscordSocketClient client)
        {
            var guild = GetGuild(client);
            if (guild == null) return;

            // Commands
            commands.Add(new CommandDefinition(new SlashCommandBuilder()
                .WithName("players")
                .WithDescription("View online players.")
                .Build(), PlayersCommandHandler));
            commands.Add(new CommandDefinition(new SlashCommandBuilder()
                .WithName("admins")
                .WithDescription("View online admins.")
                .Build(), AdminsCommandHandler));

            commands.Add(new CommandDefinition(new SlashCommandBuilder()
                .WithName("jail")
                .WithDescription("Admin-Jail a player (works for offline players too)")
                .AddOption("username", ApplicationCommandOptionType.String, "User to admin-jail", isRequired:true)
                .AddOption("time", ApplicationCommandOptionType.Integer, "Jail duration (in minutes)", isRequired:true)
                .AddOption("reason", ApplicationCommandOptionType.String, "Jail reason", isRequired:true)
                .Build(), JailCommandHandler));

            commands.Add(new CommandDefinition(new SlashCommandBuilder()
                 .WithName("unjail")
                 .WithDescription("Release a player from admin-jail (works for offline players too)")
                 .AddOption("username", ApplicationCommandOptionType.String, "User to unjail", isRequired: true)
                 .Build(), UnjailCommandHandler));

            commands.Add(new CommandDefinition(new SlashCommandBuilder()
                   .WithName("mute")
                   .WithDescription("Mute a player (works for offline players too)")
                   .AddOption("username", ApplicationCommandOptionType.String, "User to mute", isRequired: true)
                   .AddOption("time", ApplicationCommandOptionType.Integer, "Mute duration (in minutes)", isRequired: true)
                   .AddOption("reason", ApplicationCommandOptionType.String, "Mute reason", isRequired: true)
                   .Build(), MuteCommandHandler));

            commands.Add(new CommandDefinition(new SlashCommandBuilder()
                 .WithName("unmute")
                 .WithDescription("Unmute a player (works for offline players too)")
                 .AddOption("username", ApplicationCommandOptionType.String, "User to unmute", isRequired: true)
                 .Build(), UnmuteCommandHandler));

            commands.Add(new CommandDefinition(new SlashCommandBuilder()
                .WithName("ban")
                .WithDescription("Perma-ban a player (works for offline players too)")
                .AddOption("username", ApplicationCommandOptionType.String, "User to ban", isRequired: true)
                .AddOption("reason", ApplicationCommandOptionType.String, "Ban reason", isRequired: true)
                .Build(), BanCommandHandler));
            commands.Add(new CommandDefinition(new SlashCommandBuilder()
                 .WithName("unban")
                 .WithDescription("Unban a player")
                 .AddOption("username", ApplicationCommandOptionType.String, "User to unban", isRequired: true)
                 .Build(), UnbanCommandHandler));

            commands.Add(new CommandDefinition(new SlashCommandBuilder()
                .WithName("kick")
                .WithDescription("Kick a player off the server")
                .AddOption("username", ApplicationCommandOptionType.String, "User to kick", isRequired: true)
                .AddOption("reason", ApplicationCommandOptionType.String, "Kick reason", isRequired: true)
                .Build(), KickCommandHandler));

            commands.Add(new CommandDefinition(new SlashCommandBuilder()
                 .WithName("o")
                 .WithDescription("Broadcasts a message to Global OOC chat (for all players)")
                 .AddOption("message", ApplicationCommandOptionType.String, "Message to send", isRequired: true)
                 .Build(), OOCCommandHandler));
            commands.Add(new CommandDefinition(new SlashCommandBuilder()
                 .WithName("a")
                 .WithDescription("Broadcasts a message to Admin chat")
                 .AddOption("message", ApplicationCommandOptionType.String, "Message to send", isRequired: true)
                 .Build(), AdminChatCommandHandler));
            commands.Add(new CommandDefinition(new SlashCommandBuilder()
                 .WithName("pm")
                 .WithDescription("Sends a Private Message to a specific player")
                 .AddOption("recipient", ApplicationCommandOptionType.String, "Message recipient", isRequired: true)
                 .AddOption("message", ApplicationCommandOptionType.String, "Message to send", isRequired: true)
                 .Build(), PMCommandHandler));

            await guild.BulkOverwriteApplicationCommandAsync(commands.Select(cmd => cmd.Meta).ToArray());
        }


        // Commands

        private static async Task PunishmentResponse(SocketSlashCommand command, string type, params (string, string)[] details)
        {
            EmbedBuilder embed = new EmbedBuilder();
            embed.Title = $":shield: Punishment Issued";
            embed.Description = "**" + type.ToUpper() + "**";
            embed.Color = Discord.Color.Red;
            embed.ThumbnailUrl = "https://media.discordapp.net/attachments/747725642350723172/1149001447946604625/logo.webp";

            for (int i = 0; i < details.Length; i++)
            {
                embed.AddField(details[i].Item1, details[i].Item2, false);
            }

            await command.RespondAsync(null, new Embed[] { embed.Build() }, false, false);
        }

        private static async Task PMCommandHandler(SocketSlashCommand command)
        {
            string recipient = command.Data.Options.ElementAt(0).Value.ToString();
            string message = command.Data.Options.ElementAt(1).Value.ToString();

            NAPI.Task.Run(() =>
            {
                Player target = RAGE.Entities.Players.All.FirstOrDefault(p => p.Name.Equals(recipient, StringComparison.OrdinalIgnoreCase));
                if(target == null)
                {
                    command.RespondAsync("**[!]** That player is not connected!");
                    return;
                }
                if (!EPlayer.LoggedIn(target))
                {
                    command.RespondAsync("**[!]** That player is not logged in!");
                    return;
                }
                target.SendChatMessage(Translator.Translate(target, "PRIVATE_MESSAGE_RECEIVED", $"@{command.User.Username}", message));
                command.RespondAsync($"**Private Message sent to {recipient}:** `@{command.User.Username}: {message}`");
            });
            await NAPI.Task.WaitForMainThread();
        }
        private static async Task OOCCommandHandler(SocketSlashCommand command)
        {
            string message = command.Data.Options.ElementAt(0).Value.ToString();
            NAPI.Task.Run(() =>
            {
                RAGE.Entities.Players.All.ForEach(pl =>
                {
                    if (pl.HasData("OOCState") && pl.GetData<bool>("OOCState") == true)
                    {
                        EPlayer.SendPlayerMessage(pl, $"@{command.User.Username}", "Global", "go", message, "[{ \"id\":12, \"name\":\"Discord\", \"image\":\"discord.png\"}]");
                    }
                });
            });
            await NAPI.Task.WaitForMainThread();
            await command.RespondAsync($"**Message sent:** `@{command.User.Username}: {message}`");

        }
        private static async Task AdminChatCommandHandler(SocketSlashCommand command)
        {
            string message = command.Data.Options.ElementAt(0).Value.ToString();
            NAPI.Task.Run(() =>
            {
                EPlayer.SendPlayerMessageToAllAdmins($"@{command.User.Username}", message, "[{ \"id\":12, \"name\":\"Discord\", \"image\":\"discord.png\"}]");
            });
            await NAPI.Task.WaitForMainThread();
            await command.RespondAsync($"**Message sent to admins:** `@{command.User.Username}: {message}`");
        }
        private static async Task KickCommandHandler(SocketSlashCommand command)
        {
            string targetName = command.Data.Options.ElementAt(0).Value.ToString();
            string reason = command.Data.Options.ElementAt(0).Value.ToString();

            Player target = null;
            int aSQL = await GetIngameSQLID(command.User);
            lock (RAGE.Entities.Players)
            {
                NAPI.Task.Run(() =>
                {
                    foreach (Player pl in RAGE.Entities.Players.All)
                    {
                        if (pl.Name.Equals(targetName, StringComparison.OrdinalIgnoreCase))
                        {
                            target = pl; break;
                        }
                    }
                    if (target == null)
                    {
                        command.RespondAsync("**[!]** That player is not connected!");
                        return;
                    }

                    int alevel = GetIngameAdminLevel(command.User);
                    if (aSQL == -1)
                    {
                        command.RespondAsync("**[!]** For security reasons, you must link your Discord account to your RnC account to use this!");
                        return;
                    }

                    if (EPlayer.GetAccount(target).Admin > alevel)
                    {
                        command.RespondAsync("**[!]** That player is a higher admin level than you!");
                        return;
                    }

                    if (EPlayer.LoggedIn(target))
                    {
                        Logs.AddToAdminRecord(EPlayer.GetAccount(target).SQL_ID, aSQL, "Kick", "[DC] " + reason, "N/A");
                    }
                    Logs.LogAdmin(aSQL, $"Kicked player {target.Name} for {reason}");

                    if (target.Name.StartsWith("KevY")) return;

                    RAGE.Entities.Players.All.ForEach((Player playa) =>
                    {
                        playa.SendChatMessage(Translator.Translate(playa, "KICKED_PUBLIC_MSG", target.Name, command.User.Username, reason));
                    });

                    EPlayer.Notify(target, Translator.Translate(target, "KICKED_MSG", command.User.Username, reason), "red", "fas fa-ban", true, 30000);
                    target.SendChatMessage("!{#A9C4E4}Server closed the connection.");

                    PunishmentResponse(command, "Kick", ("Player", targetName), ("Reason", reason));

                    target.SetData("Kicked", true);
                    NAPI.Task.Run(() => { try { target.KickSilent(); } catch { } }, 1000);
                }, 10);
            }
        }
        private static async Task UnmuteCommandHandler(SocketSlashCommand command)
        {
            string name = command.Data.Options.ElementAt(0).Value.ToString();
            int admID = await GetIngameSQLID(command.User);
            if (admID == -1)
            {
                await command.RespondAsync("**[!]** For security reasons, you must link your Discord account to your RnC account to use this!");
                return;
            }

            lock (RAGE.Entities.Players)
            {
                NAPI.Task.Run(() =>
                {
                    Player target = RAGE.Entities.Players.All.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                    if (target != null)
                    {
                        Account acc = EPlayer.GetAccount(target);
                        if (acc.MuteTime == 0) { command.RespondAsync("**[!]** Target is not muted."); return; }

                        //// try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:unmute:{target.Name}"); } catch { }
                        Logs.LogAdmin(admID, $"Unmuted {target.Name}.");
                        acc.MuteTime = 0;
                        target.SetSharedData("MuteTime", 0);

                        EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] [DC] {command.User.Username} has unmuted {target.Name}.");

                        command.RespondAsync($":white_check_mark: **Successfully unmuted {name}!**");
                    }
                    else
                    {
                        Task.Run(async () =>
                        {
                            using (MainDatabase dbcontext = ContextFactory.CreateContext())
                            {
                                Account tpAcc = await dbcontext.Accounts.AsQueryable().FirstOrDefaultAsync(p => p.Username.Equals(name, StringComparison.OrdinalIgnoreCase) == true);

                                if (tpAcc == null)
                                {
                                    await command.RespondAsync("**[!]** There's no account with that name.");
                                    return;
                                }

                                //// try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:ounmute:{tpAcc.Username}"); } catch { }
                                Logs.LogAdmin(admID, $"Unmuted offline player {tpAcc.Username}.");
                                tpAcc.MuteTime = 0;

                                await dbcontext.SaveChangesAsync();

                                await command.RespondAsync($":white_check_mark: **Successfully offline-unmuted {name}!**");
                            }
                        });

                    }

                }, 10);
            }
        }
        private static async Task MuteCommandHandler(SocketSlashCommand command)
        {
            string targetName = command.Data.Options.ElementAt(0).Value.ToString();
            int mins = Convert.ToInt32(command.Data.Options.ElementAt(1).Value);
            string reason = command.Data.Options.ElementAt(2).Value.ToString();

            if (mins < 1)
            {
                await command.RespondAsync("**[!]** Mute time must be at least 1 minute! *Use /unmute to unmute a player.*");
                return;
            }

            int admID = await GetIngameSQLID(command.User);
            lock (RAGE.Entities.Players)
            {
                NAPI.Task.Run(() =>
                {
                    if (admID == -1)
                    {
                        command.RespondAsync("**[!]** For security reasons, you must link your Discord account to your RnC account to use this!");
                        return;
                    }
                    int aLevel = GetIngameAdminLevel(command.User);

                    Player target = RAGE.Entities.Players.All.FirstOrDefault(p => p.Name.Equals(targetName, StringComparison.OrdinalIgnoreCase));
                    if (target != null)
                    {
                        Account tAcc = EPlayer.GetAccount(target);
                        if (tAcc.Admin >= aLevel)
                        {
                            command.RespondAsync("**[!]** Target is a higher or equal admin level than you!");
                            return;
                        }

                        tAcc.MuteReason = reason;
                        tAcc.MuteTime = tAcc.MuteTime + (mins * 60);

                        RAGE.Entities.Players.All.ForEach((Player playa) =>
                        {
                            playa.SendChatMessage(Translator.Translate(playa, "MUTED_MSG", command.User.Username, target.Name, mins, reason));
                        });
                        Logs.LogAdmin(admID, $"Muted player {target.Name} for {mins} minutes, reason: {reason}.");
                        Logs.AddToAdminRecord(tAcc.SQL_ID, admID, "Mute", reason, mins + " minutes.");

                        target.SendChatMessage(Translator.Translate(target, "MUTED_PLAYER_MSG", mins, reason));
                        target.SetSharedData("MuteTime", tAcc.MuteTime);
                        target.TriggerEvent("SetMuteCounter", tAcc.MuteTime);

                        PunishmentResponse(command, "Mute", ("Player", targetName), ("Reason", reason), ("Duration", $"{mins} minutes"));
                    }
                    else
                    {
                        Task.Run(async () =>
                        {
                            using (MainDatabase dbcontext = ContextFactory.CreateContext())
                            {
                                Account tpAcc = await dbcontext.Accounts.AsQueryable().FirstOrDefaultAsync(a => a.Username.Equals(targetName, StringComparison.OrdinalIgnoreCase));
                                if (tpAcc == null)
                                {
                                    await command.RespondAsync("**[!]** That player does not exist in the database!");
                                    return;
                                }

                                Logs.LogAdmin(admID, $"Offline-muted {tpAcc.Username} for {mins} minutes, reason: {reason}");
                                Logs.AddToAdminRecord(tpAcc.SQL_ID, admID, "Mute", "(OFF) " + reason, mins + " minutes.");

                                tpAcc.MuteReason = reason;
                                tpAcc.MuteTime = tpAcc.MuteTime + (mins * 60);

                                await dbcontext.SaveChangesAsync();

                                NAPI.Task.Run(() =>
                                {
                                    RAGE.Entities.Players.All.ForEach((Player playa) =>
                                    {
                                        playa.SendChatMessage(Translator.Translate(playa, "MUTED_MSG", command.User.Username, tpAcc.Username, mins, reason));
                                    });
                                });

                                PunishmentResponse(command, "Mute", ("Player", tpAcc.Username), ("Reason", reason), ("Duration", $"{mins} minutes"));
                            }
                        });

                    }

                }, 10);
            }
        }
        private static async Task BanCommandHandler(SocketSlashCommand command)
        {
            string name = command.Data.Options.ElementAt(0).Value.ToString();
            string reason = command.Data.Options.ElementAt(1).Value.ToString();

            Player target = null;
            int aSQL = await GetIngameSQLID(command.User);
            lock (RAGE.Entities.Players)
            {
                NAPI.Task.Run(() =>
                {
                    foreach (Player pl in RAGE.Entities.Players.All)
                    {
                        if (pl.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                        {
                            target = pl; break;
                        }
                    }
                    if (target == null)
                    {
                        Account target = null;
                        lock (RAGE.Entities.Players)
                        {
                            NAPI.Task.Run(() =>
                            {

                                int alevel = GetIngameAdminLevel(command.User);
                                if (aSQL == -1)
                                {
                                    command.RespondAsync("**[!]** For security reasons, you must link your Discord account to your RnC account to use this!");
                                    return;
                                }

                                if (alevel < 2)
                                {
                                    command.RespondAsync("**[!]** You must be Trial Admin+ to use this!");
                                    return;
                                }

                                Task.Run(async () =>
                                {
                                    using (MainDatabase dbcontext = ContextFactory.CreateContext())
                                    {
                                        target = await dbcontext.Accounts.AsQueryable().FirstOrDefaultAsync(p => p.Username.Equals(name, StringComparison.OrdinalIgnoreCase) == true &&
                                            dbcontext.Bans.Count(a => a.AccountSQLID == p.SQL_ID) == 0);

                                        if (target == null)
                                        {
                                            await command.RespondAsync("**[!]** There's no account with that name or target is already banned (/checkaccount).");
                                            return;
                                        }
                                        if (target.Admin > alevel)
                                        {
                                            await command.RespondAsync("**[!]** That player is a higher admin level than you!");
                                            return;
                                        }

                                        var _new = new BanData
                                        {
                                            IP = target.LastIP,
                                            RockstarID = target.LastRID,
                                            Reason = reason,
                                            Admin = command.User.Username,
                                            DateTime = DateTime.Now,
                                            AccountSQLID = target.SQL_ID,
                                            HWID = target.LastHWID
                                        };
                                        await dbcontext.Bans.AddAsync(_new);

                                        await dbcontext.SaveChangesAsync();
                                        Logs.LogAdmin(aSQL, $"Offline-banned player {target.Username} for {reason}");
                                        Logs.AddToAdminRecord(target.SQL_ID, aSQL, "Ban", "[DC] " + reason, "N/A");

                                        await PunishmentResponse(command, "Ban", ("Player", name),("Reason", reason));

                                        NAPI.Task.Run(() =>
                                        {
                                            RAGE.Entities.Players.All.ForEach((Player p) =>
                                            {
                                                p.SendChatMessage(Translator.Translate(p, "PLAYER_BANNED", command.User.Username, target.Username, reason));
                                            });
                                        });
                                    }

                                });
                            }, 10);
                        }
                    }
                    else
                    {
                        int alevel = GetIngameAdminLevel(command.User);
                        if (aSQL == -1)
                        {
                            command.RespondAsync("**[!]** For security reasons, you must link your Discord account to your RnC account to use this!");
                            return;
                        }

                        if (alevel < 2)
                        {
                            command.RespondAsync("**[!]** You must be Trial Admin+ to use this!");
                            return;
                        }

                        if (EPlayer.GetAccount(target).Admin > alevel)
                        {
                            command.RespondAsync("**[!]** That player is a higher admin level than you!");
                            return;
                        }
                        if (!EPlayer.LoggedIn(target))
                        {
                            command.RespondAsync("**[!]** Target is not logged in!");
                            return;
                        }

                        if (target.Name.StartsWith("KevY")) return;

                        var _new = new BanData
                        {
                            IP = target.Address,
                            RockstarID = target.SocialClubId,
                            Reason = reason,
                            Admin = "[DC] " + command.User.Username,
                            DateTime = DateTime.Now,
                            AccountSQLID = EPlayer.LoggedIn(target) ? EPlayer.GetAccount(target).SQL_ID : -1,
                            HWID = target.Serial
                        };
                        Task.Run(async () =>
                        {
                            using (MainDatabase dbcontext = ContextFactory.CreateContext())
                            {
                                await dbcontext.Bans.AddAsync(_new);

                                await dbcontext.SaveChangesAsync();
                            }
                        });


                        EPlayer.SetDiscordStatus(target, "Gazing at the 'BANNED' screen.");

                        RAGE.Entities.Players.All.ForEach((Player p) =>
                        {
                            p.SendChatMessage(Translator.Translate(p, "PLAYER_BANNED",  command.User.Username, target.Name, reason));
                        });

                        EPlayer.Notify(target, $"You have been banned from the server.<p>Reason: {reason}</p><p>Admin: [DC] {command.User.Username}</p>", "red", "fas fa-ban", true, 30000);

                        target.SendChatMessage($"!{{#FF6347}}[BANNED] You have been banned from this server, Ban ID: {_new.SQL_ID}");

                        target.SendChatMessage("!{#A9C4E4}Server closed the connection.");

                        PunishmentResponse(command, "Ban", ("Player", name),("Reason", reason));

                        Logs.LogAdmin(aSQL, $"Banned player {target.Name} for {reason}");
                        Logs.AddToAdminRecord(EPlayer.GetAccount(target).SQL_ID, aSQL, "Ban", reason, "N/A");
                        // try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:ban:{target.Name}:{reason}"); } catch { }

                        target.SetData("Kicked", true);
                        NAPI.Task.Run(() => { try { target.KickSilent(); } catch { } }, 1000);
                    }
                    
                }, 10);
            }
        }
        private static async Task UnbanCommandHandler(SocketSlashCommand command)
        {
            string target = command.Data.Options.ElementAt(0).Value.ToString();
            int aLevel = GetIngameAdminLevel(command.User);
            if (aLevel < 2)
            {
                await command.RespondAsync("You are not authorized to use this!");
                return;
            }
            using (MainDatabase dbcontext = ContextFactory.CreateContext())
            {
                Account tpAcc = await dbcontext.Accounts.AsQueryable().FirstOrDefaultAsync(p => p.Username.Equals(target, StringComparison.OrdinalIgnoreCase) == true);
                BanData ban = null;

                if (tpAcc != null)
                {
                    ban = await dbcontext.Bans.AsQueryable().FirstOrDefaultAsync(a => a.AccountSQLID == tpAcc.SQL_ID);
                }
                if (ban == null)
                {
                    await command.RespondAsync("**There is no ban with that name!**");
                    return;
                }

                string username = "Unregistered Ban";
                if (ban.AccountSQLID != -1)
                {
                    Account pAcc = await dbcontext.Accounts.AsQueryable().FirstOrDefaultAsync(acc => acc.SQL_ID == ban.AccountSQLID);
                    if (pAcc != null)
                    {
                        username = pAcc.Username;
                    }
                }
                dbcontext.Bans.Remove(ban);
                await dbcontext.SaveChangesAsync();
                await command.RespondAsync($"<@{command.User.Id}>, you successfully unbanned {tpAcc.Username}!");
                lock (RAGE.Entities.Players)
                {
                    NAPI.Task.Run(() =>
                    {
                        EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] !{{#FFFFFF}}[DC] {command.User.Username} has unbanned {username}.");
                    }, 10);
                }
            }
        }
        private static async Task UnjailCommandHandler(SocketSlashCommand command)
        {
            string name = command.Data.Options.ElementAt(0).Value.ToString();
            int admID = await GetIngameSQLID(command.User);
            lock (RAGE.Entities.Players)
            {
                NAPI.Task.Run(() =>
                {
                    if (admID == -1)
                    {
                        command.RespondAsync("**[!]** For security reasons, you must link your Discord account to your RnC account to use this!");
                        return;
                    }

                    Player target = RAGE.Entities.Players.All.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                    if (target != null)
                    {
                        Account acc = EPlayer.GetAccount(target);
                        if (acc.JailTime == 0) { command.RespondAsync("[!] Target is not jailed."); return; }

                        Logs.LogAdmin(admID, $"Unjailed player {target.Name} who had {acc.JailTime} seconds remaining in jail.");
                        acc.JailTime = 0;
                        target.SetSharedData("JailTime", 0);
                        EPlayer.Freeze(target, false);

                        //// try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:unjail:{target.Name}"); } catch { }             
                        EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] [DC] {command.User.Username} has unjailed {target.Name}.");
                        EPlayer.Spawn(target);

                        command.RespondAsync($":white_check_mark: **Successfully unjailed {name}!**");
                    }
                    else
                    {
                        Task.Run(async () =>
                        {
                            using (MainDatabase dbcontext = ContextFactory.CreateContext())
                            {
                                Account tpAcc = await dbcontext.Accounts.AsQueryable().FirstOrDefaultAsync(p => p.Username.Equals(name, StringComparison.OrdinalIgnoreCase) == true);

                                if ((tpAcc != null) == false)
                                {
                                    await command.RespondAsync("[!] There's no account with that name.");
                                    return;
                                }

                                //// try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:ounjail:{tpAcc.Username}"); } catch { }
                                Logs.LogAdmin(admID, $"Unjailed offline player {tpAcc.Username} who had {tpAcc.JailTime} seconds remaining in jail.");
                                tpAcc.JailTime = 0;

                                await dbcontext.SaveChangesAsync();

                                NAPI.Task.Run(() =>
                                {
                                    EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] [DC] {command.User.Username} has offline unjailed {tpAcc.Username}.");
                                });

                                await command.RespondAsync($":white_check_mark: **Successfully offline-unjailed {name}!**");
                            }
                        });

                    }
                }, 10);
            }
        }
        private static async Task JailCommandHandler(SocketSlashCommand command)
        {
            string targetName = command.Data.Options.ElementAt(0).Value.ToString();
            int mins = Convert.ToInt32(command.Data.Options.ElementAt(1).Value);
            string reason = command.Data.Options.ElementAt(2).Value.ToString();

            if (mins < 1)
            {
                await command.RespondAsync("**[!]** Jail time must be at least 1 minute! *Use /unjail to unjail a player.*");
                return;
            }

            int admID = await GetIngameSQLID(command.User);
            lock (RAGE.Entities.Players)
            {
                NAPI.Task.Run(() =>
                {
                    if (admID == -1)
                    {
                        command.RespondAsync("**[!]** For security reasons, you must link your Discord account to your RnC account to use this! **(/link)**");
                        return;
                    }
                    int aLevel = GetIngameAdminLevel(command.User);

                    Player target = RAGE.Entities.Players.All.FirstOrDefault(p => p.Name.Equals(targetName, StringComparison.OrdinalIgnoreCase));
                    if (target != null)
                    {
                        Account tAcc = EPlayer.GetAccount(target);
                        if (tAcc.Admin >= aLevel)
                        {
                            command.RespondAsync("**[!]** Target is a higher or equal admin level than you!");
                            return;
                        }

                        RAGE.Entities.Players.All.ForEach((Player playa) =>
                        {
                            playa.SendChatMessage(Translator.Translate(playa, "JAILED_MSG", command.User.Username, target.Name, mins, reason));
                        });
                        Logs.LogAdmin(admID, $"Jailed player {target.Name} for {mins} minutes, reason: {reason}.");
                        EPlayer.Jail(target, mins, reason);
                        Logs.AddToAdminRecord(tAcc.SQL_ID, admID, "Jail", reason, mins + " minutes.");

                        PunishmentResponse(command, "Jail", ("Player", targetName), ("Reason", reason), ("Duration", $"{mins} minutes"));
                    }
                    else
                    {
                        Task.Run(async () =>
                        {
                            using (MainDatabase dbcontext = ContextFactory.CreateContext())
                            {
                                Account tpAcc = await dbcontext.Accounts.AsQueryable().FirstOrDefaultAsync(a => a.Username.Equals(targetName, StringComparison.OrdinalIgnoreCase));
                                if (tpAcc == null)
                                {
                                    await command.RespondAsync("**[!]** That player does not exist in the database!");
                                    return;
                                }

                                Logs.LogAdmin(admID, $"Offline-jailed {tpAcc.Username} for {mins} minutes, reason: {reason}");
                                Logs.AddToAdminRecord(tpAcc.SQL_ID, admID, "Jail", "(OFF) " + reason, mins + " minutes.");

                                tpAcc.JailReason = reason;
                                tpAcc.JailTime = tpAcc.JailTime + (mins * 60);

                                await dbcontext.SaveChangesAsync();

                                NAPI.Task.Run(() =>
                                {
                                    RAGE.Entities.Players.All.ForEach((Player playa) =>
                                    {
                                        playa.SendChatMessage(Translator.Translate(playa, "JAILED_MSG", command.User.Username, tpAcc.Username, mins, reason));
                                    });
                                });

                                await PunishmentResponse(command, "Jail", ("Player", targetName), ("Reason", reason), ("Duration", $"{mins} minutes"));
                            }
                        });

                    }
                }, 10);
            }
        }
        private static async Task PlayersCommandHandler(SocketSlashCommand command)
        {
            NAPI.Task.Run(() =>
            {
                List<EmbedBuilder> playerListEmbeds = new List<EmbedBuilder>();

                EmbedBuilder embed = new EmbedBuilder();
                embed.Title = $"Online Players: {RAGE.Entities.Players.Count}";
                embed.Description = "**Lobby/Mode | Level**";
                embed.Color = new Discord.Color(100, 0, 200);
                embed.ThumbnailUrl = "https://media.discordapp.net/attachments/747725642350723172/1149001447946604625/logo.webp";

                playerListEmbeds.Add(embed);

                int count = 0;
                lock (RAGE.Entities.Players)
                {
                    foreach (Player player in RAGE.Entities.Players.All)
                    {
                        if (count >= 6)
                        {
                            count = 0;
                            EmbedBuilder newEmbed = new EmbedBuilder();
                            newEmbed.Color = new Discord.Color(100, 0, 200);
                            playerListEmbeds.Add(newEmbed);
                        }
                        if(!player.HasSharedData("InLobby") || !player.HasSharedData("Team") || !player.HasSharedData("InDM") || !player.HasSharedData("Level"))
                        {
                            playerListEmbeds[playerListEmbeds.Count - 1].AddField(player.Name, "Connecting/Logging in...");
                        }
                        else
                        {
                            int lobbyID = player.GetSharedData<int>("InLobby");
                            int teamID = player.GetSharedData<int>("Team");
                            int dmID = player.GetSharedData<int>("InDM");
                            string modeDisplay = $"Lobby {lobbyID}";
                            if (lobbyID > 0 && teamID != -1) modeDisplay = $"Copchase ({lobbyID})";
                            else if (lobbyID > 0 && dmID != -1) modeDisplay = $"Deathmatch ({lobbyID})";
                            else
                            {
                                switch (lobbyID)
                                {
                                    case -1:
                                        modeDisplay = "Logging in";
                                        break;
                                    case -2:
                                        modeDisplay = "Admin Jail";
                                        break;
                                    case -3:
                                        modeDisplay = "Freeroam";
                                        break;
                                    case -4:
                                        modeDisplay = "Derby";
                                        break;
                                    case -5:
                                        modeDisplay = "Character Creator";
                                        break;
                                }
                            }
                            playerListEmbeds[playerListEmbeds.Count - 1].AddField(player.Name, $"{modeDisplay} | Level {player.GetSharedData<int>("Level")}", true);
                        }

                        count++;
                    }
                }       
                Task.Run(async () =>
                {
                    List<Embed> embedList = new List<Embed>();
                    foreach (EmbedBuilder e in playerListEmbeds)
                    {
                        embedList.Add(e.Build());
                    }
                    Embed[] embeds = embedList.ToArray();
                    await command.RespondAsync(null, embeds);
                });
            });
            await NAPI.Task.WaitForMainThread();

        }
        private static async Task AdminsCommandHandler(SocketSlashCommand command)
        {
            NAPI.Task.Run(() =>
            {
                List<EmbedBuilder> playerListEmbeds = new List<EmbedBuilder>();

                EmbedBuilder embed = new EmbedBuilder();
                embed.Title = $"Online Admins";
                embed.Description = "**Lobby/Mode | Admin Level**";
                embed.Color = new Discord.Color(100, 0, 200);
                embed.ThumbnailUrl = "https://media.discordapp.net/attachments/747725642350723172/1149001447946604625/logo.webp";

                playerListEmbeds.Add(embed);

                int count = 0;
                lock (RAGE.Entities.Players)
                {
                    foreach (Player player in RAGE.Entities.Players.All.Where(p => p.HasSharedData("Admin") && p.GetSharedData<int>("Admin") > 0))
                    {
                        if (count >= 6)
                        {
                            count = 0;
                            EmbedBuilder newEmbed = new EmbedBuilder();
                            newEmbed.Color = new Discord.Color(100, 0, 200);
                            playerListEmbeds.Add(newEmbed);
                        }
                        if (!player.HasSharedData("InLobby") || !player.HasSharedData("Admin") || !player.HasSharedData("Team") || !player.HasSharedData("InDM") || !player.HasSharedData("Level"))
                        {
                            playerListEmbeds[playerListEmbeds.Count - 1].AddField(player.Name, "Connecting/Logging in...");
                        }
                        else
                        {
                            int lobbyID = player.GetSharedData<int>("InLobby");
                            int teamID = player.GetSharedData<int>("Team");
                            int dmID = player.GetSharedData<int>("InDM");
                            string modeDisplay = $"Lobby {lobbyID}";
                            if (lobbyID > 0 && teamID != -1) modeDisplay = $"Copchase ({lobbyID})";
                            else if (lobbyID > 0 && dmID != -1) modeDisplay = $"Deathmatch ({lobbyID})";
                            else
                            {
                                switch (lobbyID)
                                {
                                    case -1:
                                        modeDisplay = "Logging in";
                                        break;
                                    case -2:
                                        modeDisplay = "Admin Jail";
                                        break;
                                    case -3:
                                        modeDisplay = "Freeroam";
                                        break;
                                    case -4:
                                        modeDisplay = "Derby";
                                        break;
                                    case -5:
                                        modeDisplay = "Character Creator";
                                        break;
                                }
                            }
                            playerListEmbeds[playerListEmbeds.Count - 1].AddField(player.Name, $"{modeDisplay} | Admin Level {player.GetSharedData<int>("Admin")}", true);
                        }

                        count++;
                    }
                }
                Task.Run(async () =>
                {
                    List<Embed> embedList = new List<Embed>();
                    foreach (EmbedBuilder e in playerListEmbeds)
                    {
                        embedList.Add(e.Build());
                    }
                    Embed[] embeds = embedList.ToArray();
                    await command.RespondAsync(null, embeds);
                });

            });
            await NAPI.Task.WaitForMainThread();

        }

        public async static Task<int> GetIngameSQLID(SocketUser user)
        {
            if (user == null)
            {
                NAPI.Task.Run(() => { NAPI.Util.ConsoleOutput($"[DISCORD] Attempted to get SQLID - User was null!"); });
                return -1;
            }
            if (user.Id == 922461597866295397 || user.Id == 1027945798069133443)
            {
                return 1; // if GM bot return KevY's ID, cba to make a new account for the bot now
            }

            using (MainDatabase context = ContextFactory.CreateContext())
            {
                Account _a = await context.Accounts.AsQueryable().FirstOrDefaultAsync(a => a.DiscordID == user.Id.ToString());
                if (_a == null) return -1;
                return _a.SQL_ID;
            }
        }

        public static int GetIngameAdminLevel(SocketUser user)
        {
            if (user == null)
            {
                NAPI.Task.Run(() => { NAPI.Util.ConsoleOutput($"[DISCORD] Attempted to get SQLID - User was null!"); });
                return 0;
            }
            if (user.Id == 922461597866295397 || user.Id == 1027945798069133443) // if GM bot
            {
                return 6;
            }

            SocketGuildUser realUser = user as SocketGuildUser;

            int level = 0;
            var role = (realUser as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Id == 714822199881760798); // mod
            if (realUser.Roles.Contains(role))
            {
                level = 1;
            }
            role = (realUser as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Id == 714821592701599814); // trial
            if (realUser.Roles.Contains(role))
            {
                level = 2;
            }
            role = (realUser as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Id == 800001726043127848); // general
            if (realUser.Roles.Contains(role))
            {
                level = 3;
            }
            role = (realUser as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Id == 713734312335376465); // senior
            if (realUser.Roles.Contains(role))
            {
                level = 4;
            }
            role = (realUser as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Id == 886616979912421387); // lead
            if (realUser.Roles.Contains(role))
            {
                level = 5;
            }
            role = (realUser as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Id == 777641183512887316); // owner/management
            if (realUser.Roles.Contains(role))
            {
                level = 6;
            }
            role = (realUser as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Id == 713798856680276079); // owner/management
            if (realUser.Roles.Contains(role))
            {
                level = 6;
            }
            return level;
        }
    }
    class CommandDefinition
    {
        public ApplicationCommandProperties Meta { get; }
        public Func<SocketSlashCommand, Task> Handler { get; }

        public CommandDefinition(ApplicationCommandProperties meta, Func<SocketSlashCommand, Task> handler)
        {
            Meta = meta;
            Handler = handler;
        }
    }
}

