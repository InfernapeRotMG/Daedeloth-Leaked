#region

using db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wServer.networking;
using wServer.networking.svrPackets;
using wServer.realm.entities;
using wServer.realm.entities.player;
using wServer.realm.setpieces;
using wServer.realm.worlds;

#endregion

namespace wServer.realm.commands
{
    internal class Lock : Command
    {
        public Lock()
            : base("lock", Ranks.Player)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (args.Length != 1)
            {
                player.SendHelp("Usage: /lock <playername>");
                return false;
            }
            if (string.Equals(player.Name.ToLower(), args[0].ToLower()))
            {
                player.SendInfo("You are trying to lock yourself...Dumass.");
                return false;
            }
            foreach (KeyValuePair<string, Client> i in player.Manager.Clients.Where(i => i.Value.Player.Name.EqualsIgnoreCase(args[0])))
            {
                foreach (string l in player.Locked)
                {
                    Database db = new Database();
                    if (!l.Contains(i.Value.Account.AccountId))
                    {
                        db.AddLock(player.AccountId, i.Value.Account.AccountId);
                        player.Locked.Add(i.Value.Account.AccountId);
                        player.UpdateCount++;
                        return true;
                    }
                    db.RemoveLock(player.AccountId, i.Value.Account.AccountId);
                    player.Locked.Remove(i.Value.Account.AccountId);
                    player.UpdateCount++;
                    return true;
                }
            }
            player.SendError($"Player {args[0]} could not be found");
            return false;
        }
    }

    internal class RemoveTossEffCommand : Command
    {
        public RemoveTossEffCommand()
            : base("remtosseff", Ranks.Developer)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (args.Length != 2)
            {
                player.SendHelp("Usage: /tosseff <PlayerName> <Effectname or Effectnumber>");
                return false;
            }
            try
            {
                foreach (KeyValuePair<string, Client> i in player.Manager.Clients.Where(i => i.Value.Player.Name.EqualsIgnoreCase(args[0])))
                {
                    i.Value.Player.ApplyConditionEffect(new ConditionEffect
                    {
                        Effect =
                            (ConditionEffectIndex)Enum.Parse(typeof(ConditionEffectIndex), args[1].Trim(), true),
                        DurationMS = 0
                    });
                    player.SendInfo("Success!");
                }
            }
            catch
            {
                player.SendError("Invalid effect or player name! ");
                return false;
            }
            return true;
        }
    }

internal class TossEffCommand : Command
    {
        public TossEffCommand()
            : base("tosseff", Ranks.Developer)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (args.Length != 2)
            {
                player.SendHelp("Usage: /tosseff <PlayerName> <Effectname or Effectnumber>");
                return false;
            }
            try
            {
                foreach (KeyValuePair<string, Client> i in player.Manager.Clients.Where(i => i.Value.Player.Name.EqualsIgnoreCase(args[0])))
                {
                    i.Value.Player.ApplyConditionEffect(new ConditionEffect
                    {
                        Effect =
                            (ConditionEffectIndex)Enum.Parse(typeof(ConditionEffectIndex), args[1].Trim(), true),
                        DurationMS = -1
                    });
                    player.SendInfo("Success!");
                }
            }
            catch
            {
                player.SendError("Invalid effect or player name! ");
                return false;
            }
            return true;
        }
    }

    internal class SetGoldCommand : Command
    {
        public SetGoldCommand() : base("gold", Ranks.SpecialPlayer) { }
        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (string.IsNullOrEmpty(args[0]))
            {
                player.SendHelp("Usage: /gold <gold>");
                return false;
            }
            player.Manager.Database.DoActionAsync(db =>
            {
                var cmd = db.CreateQuery();
                cmd.CommandText = "UPDATE `stats` SET `credits`=@cre WHERE accId=@accId";
                cmd.Parameters.AddWithValue("@cre", args[0]);
                cmd.Parameters.AddWithValue("@accId", player.AccountId);
                if (cmd.ExecuteNonQuery() == 0)
                {
                    player.SendError("Error setting gold!");
                }
                else
                {
                    player.SendInfo("Success!");
                }
            });
            return true;
        }
    }
    internal class CFameCommand : Command
    {
        public CFameCommand()
        : base("cfame", Ranks.HeadDeveloper)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (args[0] == "")
            {
                player.SendHelp("Usage: /cfame <Fame Amount>");
                return false;
            }
            try
            {
                int newFame = Convert.ToInt32(args[0]);
                int newXP = Convert.ToInt32(newFame.ToString() + "000");
                player.Fame = newFame;
                player.Experience = newXP;
                player.SaveToCharacter();
                player.Client.Save();
                player.UpdateCount++;
                player.SendInfo("Updated Character Fame To: " + newFame);
            }
            catch
            {
                player.SendInfo("Error Setting Fame");
                return false;
            }
            return true;
        }
    }

    internal class GodCommand : Command
    {
        public GodCommand()
        : base("god", Ranks.HeadSpriter)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (player.HasConditionEffect(ConditionEffectIndex.Invincible))
            {
                player.ApplyConditionEffect(new ConditionEffect
                {
                    Effect = ConditionEffectIndex.Invincible,
                    DurationMS = 0
                });
                player.SendInfo("Godmode Off");
            }
            else
            {
                player.ApplyConditionEffect(new ConditionEffect
                {
                    Effect = ConditionEffectIndex.Invincible,
                    DurationMS = -1
                });
                player.SendInfo("Godmode On");
            }
            return true;
        }
    }
    internal class SetFameCommand : Command
    {
        public SetFameCommand() : base("fame", Ranks.HeadDeveloper) { }
        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (string.IsNullOrEmpty(args[0]))
            {
                player.SendHelp("Usage: /fame <fame ammount>");
                return false;
            }
            player.Manager.Database.DoActionAsync(db =>
            {
                var cmd = db.CreateQuery();
                cmd.CommandText = "UPDATE `stats` SET `fame`=@cre WHERE accId=@accId";
                cmd.Parameters.AddWithValue("@cre", args[0]);
                cmd.Parameters.AddWithValue("@accId", player.AccountId);
                if (cmd.ExecuteNonQuery() == 0)
                {
                    player.SendError("Error setting fame!");
                }
                else
                {
                    player.SendInfo("Success!");
                }
            });
            return true;
        }
    }
    internal class BanCommand : Command
    {
        public BanCommand() : 
            base("ban", permLevel: Ranks.Developer)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            var p = player.Manager.FindPlayer(args[1]);
            if (p == null)
            {
                player.SendError("Player not found");
                return false;
            }
            player.Manager.Database.DoActionAsync(db =>
            {
                var cmd = db.CreateQuery();
                cmd.CommandText = "UPDATE accounts SET banned=1 WHERE id=@accId;";
                cmd.Parameters.AddWithValue("@accId", p.AccountId);
                cmd.ExecuteNonQuery();
            });
            return true;
        }
    }
    internal class SpawnCommand : Command
    {
        public SpawnCommand()
            : base("spawn", Ranks.HeadSpriter)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            int num;
            if (args.Length > 0 && int.TryParse(args[0], out num)) //multi
            {
                string name = string.Join(" ", args.Skip(1).ToArray());
                ushort objType;
                //creates a new case insensitive dictionary based on the XmlDatas
                Dictionary<string, ushort> icdatas = new Dictionary<string, ushort>(
                    player.Manager.GameData.IdToObjectType,
                    StringComparer.OrdinalIgnoreCase);
                if (!icdatas.TryGetValue(name, out objType) ||
                    !player.Manager.GameData.ObjectDescs.ContainsKey(objType))
                {
                    player.SendInfo("Unknown entity!");
                    return false;
                }
                int c = int.Parse(args[0]);
                if (!(player.Client.Account.Rank > 2) && c > 200)
                {
                    player.SendError("Maximum spawn count is set to 200!");
                    return false;
                }
                if (player.Client.Account.Rank > 2 && c > 200)
                {
                    player.SendInfo("Bypass made!");
                }
                for (int i = 0; i < num; i++)
                {
                    Entity entity = Entity.Resolve(player.Manager, objType);
                    entity.Move(player.X, player.Y);
                    player.Owner.EnterWorld(entity);
                }
                player.SendInfo("Success!");
            }
            else
            {
                string name = string.Join(" ", args);
                ushort objType;
                //creates a new case insensitive dictionary based on the XmlDatas
                Dictionary<string, ushort> icdatas = new Dictionary<string, ushort>(
                    player.Manager.GameData.IdToObjectType,
                    StringComparer.OrdinalIgnoreCase);
                if (!icdatas.TryGetValue(name, out objType) ||
                    !player.Manager.GameData.ObjectDescs.ContainsKey(objType))
                {
                    player.SendHelp("Usage: /spawn <entityname>");
                    return false;
                }
                Entity entity = Entity.Resolve(player.Manager, objType);
                entity.Move(player.X, player.Y);
                player.Owner.EnterWorld(entity);
            }
            return true;
        }
    }

    internal class AddEffCommand : Command
    {
        public AddEffCommand()
            : base("addeff", Ranks.Developer)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (args.Length == 0)
            {
                player.SendHelp("Usage: /addeff <Effectname or Effectnumber>");
                return false;
            }
            try
            {
                player.ApplyConditionEffect(new ConditionEffect
                {
                    Effect = (ConditionEffectIndex)Enum.Parse(typeof(ConditionEffectIndex), args[0].Trim(), true),
                    DurationMS = -1
                });
                {
                    player.SendInfo("Success!");
                }
            }
            catch
            {
                player.SendError("Invalid effect!");
                return false;
            }
            return true;
        }
    }

    internal class RemoveEffCommand : Command
    {
        public RemoveEffCommand()
            : base("remeff", Ranks.Developer)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (args.Length == 0)
            {
                player.SendHelp("Usage: /remeff <Effectname or Effectnumber>");
                return false;
            }
            try
            {
                player.ApplyConditionEffect(new ConditionEffect
                {
                    Effect = (ConditionEffectIndex)Enum.Parse(typeof(ConditionEffectIndex), args[0].Trim(), true),
                    DurationMS = 0
                });
                player.SendInfo("Success!");
            }
            catch
            {
                player.SendError("Invalid effect!");
                return false;
            }
            return true;
        }
    }

    internal class GiveCommand : Command
    {
        public GiveCommand()
            : base("give", Ranks.HeadSpriter)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (args.Length == 0)
            {
                player.SendHelp("Usage: /give <Itemname>");
                return false;
            }
            string name = string.Join(" ", args.ToArray()).Trim();
            ushort objType;
            //creates a new case insensitive dictionary based on the XmlDatas
            Dictionary<string, ushort> icdatas = new Dictionary<string, ushort>(player.Manager.GameData.IdToObjectType,
                StringComparer.OrdinalIgnoreCase);
            if (!icdatas.TryGetValue(name, out objType))
            {
                player.SendError("Unknown type!");
                return false;
            }
            if (!player.Manager.GameData.Items[objType].Secret || player.Client.Account.Rank >= 4)
            {
                for (int i = 0; i < player.Inventory.Length; i++)
                    if (player.Inventory[i] == null)
                    {
                        player.Inventory[i] = player.Manager.GameData.Items[objType];
                        player.UpdateCount++;
                        player.SaveToCharacter();
                        player.SendInfo("Success!");
                        break;
                    }
            }
            else
            {
                player.SendError("Item cannot be given!");
                return false;
            }
            return true;
        }
    }
    class KillAll : Command
    {
        public KillAll() : base("killAll", permLevel: Ranks.SpecialPlayer) { }
        
        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            var iterations = 0;
            var lastKilled = -1;
            var killed = 0;

            var mobName = args.Aggregate((s, a) => string.Concat(s, " ", a));
            while (killed != lastKilled)
            {
                lastKilled = killed;
                foreach (var i in player.Owner.Enemies.Values.Where(e =>
                    e.ObjectDesc?.ObjectId != null && e.ObjectDesc.ObjectId.ContainsIgnoreCase(mobName)))
                {
                    i.Death(time);
                    killed++;
                }
                if (++iterations >= 5)
                    break;
            }

            player.SendInfo($"{killed} enemy killed!");
            return true;
        }
    }

    internal class Kick : Command
    {
        public Kick()
            : base("kick", Ranks.Spriter)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (args.Length == 0)
            {
                player.SendHelp("Usage: /kick <playername>");
                return false;
            }
            try
            {
                foreach (KeyValuePair<int, Player> i in player.Owner.Players)
                {
                    if (i.Value.Name.ToLower() == args[0].ToLower().Trim())
                    {
                        player.SendInfo("Player Disconnected");
                        i.Value.Client.Disconnect();
                    }
                }
            }
            catch
            {
                player.SendError("Cannot kick!");
                return false;
            }
            return true;
        }
    }

    internal class Mute : Command
    {
        public Mute()
            : base("mute", Ranks.Spriter)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (args.Length == 0)
            {
                player.SendHelp("Usage: /mute <playername>");
                return false;
            }
            try
            {
                foreach (KeyValuePair<int, Player> i in player.Owner.Players)
                {
                    if (i.Value.Name.ToLower() == args[0].ToLower().Trim())
                    {
                        i.Value.Muted = true;
                        i.Value.Manager.Database.DoActionAsync(db => db.MuteAccount(i.Value.AccountId));
                        player.SendInfo("Player Muted.");
                    }
                }
            }
            catch
            {
                player.SendError("Cannot mute!");
                return false;
            }
            return true;
        }
    }

    internal class Max : Command
    {
        public Max()
            : base("max", Ranks.HeadSpriter)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            try
            {
                player.Stats[0] = player.ObjectDesc.MaxHitPoints;
                player.Stats[1] = player.ObjectDesc.MaxMagicPoints;
                player.Stats[2] = player.ObjectDesc.MaxAttack;
                player.Stats[3] = player.ObjectDesc.MaxDefense;
                player.Stats[4] = player.ObjectDesc.MaxSpeed;
                player.Stats[5] = player.ObjectDesc.MaxHpRegen;
                player.Stats[6] = player.ObjectDesc.MaxMpRegen;
                player.Stats[7] = player.ObjectDesc.MaxDexterity;
                player.SaveToCharacter();
                player.Client.Save();
                player.UpdateCount++;
                player.SendInfo("Success");
            }
            catch
            {
                player.SendError("Error while maxing stats");
                return false;
            }
            return true;
        }
    }

    internal class Maxplayer : Command
    {
        public Maxplayer()
            : base("maxplayer", Ranks.HeadSpriter)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            try
            {
                foreach (KeyValuePair<int, Player> i in player.Owner.Players)
                {
                    if (i.Value.Name.ToLower() == args[0].ToLower().Trim())
                    {
                        player.Stats[0] = player.ObjectDesc.MaxHitPoints;
                        player.Stats[1] = player.ObjectDesc.MaxMagicPoints;
                        player.Stats[2] = player.ObjectDesc.MaxAttack;
                        player.Stats[3] = player.ObjectDesc.MaxDefense;
                        player.Stats[4] = player.ObjectDesc.MaxSpeed;
                        player.Stats[5] = player.ObjectDesc.MaxHpRegen;
                        player.Stats[6] = player.ObjectDesc.MaxMpRegen;
                        player.Stats[7] = player.ObjectDesc.MaxDexterity;
                        player.SaveToCharacter();
                        player.Client.Save();
                        player.UpdateCount++;
                        player.SendInfo("Success");
                    }
                }
            }
            catch
            {
                player.SendError("/Maxplayer <Player name>");
                return false;
            }
            return true;
        }
    }

    internal class UnMute : Command
    {
        public UnMute()
            : base("unmute", Ranks.Spriter)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (args.Length == 0)
            {
                player.SendHelp("Usage: /unmute <playername>");
                return false;
            }
            try
            {
                foreach (KeyValuePair<int, Player> i in player.Owner.Players)
                {
                    if (i.Value.Name.ToLower() == args[0].ToLower().Trim())
                    {
                        i.Value.Muted = true;
                        i.Value.Manager.Database.DoActionAsync(db => db.UnmuteAccount(i.Value.AccountId));
                        player.SendInfo("Player Unmuted.");
                    }
                }
            }
            catch
            {
                player.SendError("Cannot unmute!");
                return false;
            }
            return true;
        }
    }

    internal class OryxSay : Command
    {
        public OryxSay()
            : base("osay", Ranks.HeadDeveloper)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (args.Length == 0)
            {
                player.SendHelp("Usage: /oryxsay <saytext>");
                return false;
            }
            string saytext = string.Join(" ", args);
            player.SendEnemy("Oryx the Mad God", saytext);
            return true;
        }
    }

    internal class SWhoCommand : Command //get all players from all worlds (this may become too large!)
    {
        public SWhoCommand()
            : base("swho", Ranks.Player)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            StringBuilder sb = new StringBuilder("All conplayers: ");

            foreach (KeyValuePair<int, World> w in player.Manager.Worlds)
            {
                World world = w.Value;
                if (w.Key != 0)
                {
                    Player[] copy = world.Players.Values.ToArray();
                    if (copy.Length != 0)
                    {
                        for (int i = 0; i < copy.Length; i++)
                        {
                            sb.Append(copy[i].Name);
                            sb.Append(", ");
                        }
                    }
                }
            }
            string fixedString = sb.ToString().TrimEnd(',', ' '); //clean up trailing ", "s

            player.SendInfo(fixedString);
            return true;
        }
    }

    internal class Announcement : Command
    {
        public Announcement()
            : base("announce", Ranks.HeadDeveloper)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (args.Length == 0)
            {
                player.SendHelp("Usage: /announce <saytext>");
                return false;
            }
            string saytext = string.Join(" ", args);

            foreach (Client i in player.Manager.Clients.Values)
            {
                i.SendPacket(new TextPacket
                {
                    BubbleTime = 0,
                    Stars = -1,
                    Name = "@ANNOUNCEMENT",
                    Text = " " + saytext
                });
            }
            return true;
        }
    }

    internal class Summon : Command
    {
        public Summon()
            : base("summon", Ranks.Developer)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (player.Owner is Vault || player.Owner is PetYard)
            {
                player.SendInfo("You cant summon in this world.");
                return false;
            }
            foreach (KeyValuePair<string, Client> i in player.Manager.Clients)
            {
                if (i.Value.Player.Name.EqualsIgnoreCase(args[0]))
                {
                    Packet pkt;
                    if (i.Value.Player.Owner == player.Owner)
                    {
                        i.Value.Player.Move(player.X, player.Y);
                        pkt = new GotoPacket
                        {
                            ObjectId = i.Value.Player.Id,
                            Position = new Position(player.X, player.Y)
                        };
                        i.Value.Player.UpdateCount++;
                        player.SendInfo("Player summoned!");
                    }
                    else
                    {
                        pkt = new ReconnectPacket
                        {
                            GameId = player.Owner.Id,
                            Host = "",
                            IsFromArena = false,
                            Key = player.Owner.PortalKey,
                            KeyTime = -1,
                            Name = player.Owner.Name,
                            Port = -1
                        };
                        player.SendInfo("Player will connect to you now!");
                    }

                    i.Value.SendPacket(pkt);

                    return true;
                }
            }
            player.SendError(string.Format("Player '{0}' could not be found!", args));
            return false;
        }
    }

    internal class KillPlayerCommand : Command
    {
        public KillPlayerCommand()
            : base("kill", Ranks.HeadDeveloper)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            foreach (Client i in player.Manager.Clients.Values)
            {
                if (i.Account.Name.EqualsIgnoreCase(args[0]))
                {
                    i.Player.HP = 0;
                    i.Player.Death("Power of Fesal");
                    player.SendInfo("Player killed!");
                    return true;
                }
            }
            player.SendError(string.Format("Player '{0}' could not be found!", args));
            return false;
        }
    }
    internal class TqCommand : Command
    {
        public TqCommand()
            : base("tq", Ranks.AlphaTester)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (player.Quest == null)
            {
                player.SendError("Player does not have a quest!");
                return false;
            }
            player.Move(player.Quest.X + 0.5f, player.Quest.Y + 0.5f);
            if (player.Pet != null)
                player.Pet.Move(player.Quest.X + 0.5f, player.Quest.Y + 0.5f);
            player.UpdateCount++;
            player.Owner.BroadcastPacket(new GotoPacket
            {
                ObjectId = player.Id,
                Position = new Position
                {
                    X = player.Quest.X,
                    Y = player.Quest.Y
                }
            }, null);
            player.SendInfo("Success!");
            return true;
        }
    }
    internal class LevelCommand : Command
    {
        public LevelCommand()
            : base("level", Ranks.HeadSpriter)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    player.SendHelp("Use /level <ammount>");
                    return false;
                }
                if (args.Length == 1)
                {
                    player.Client.Character.Level = int.Parse(args[0]);
                    player.Client.Player.Level = int.Parse(args[0]);
                    player.UpdateCount++;
                    player.SendInfo("Success!");
                }
            }
            catch
            {
                player.SendError("Error!");
                return false;
            }
            return true;
        }
    }

    internal class SetCommand : Command
    {
        public SetCommand()
            : base("setStat", Ranks.Founder)
        {
        }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (args.Length == 2)
            {
                try
                {
                    string stat = args[0].ToLower();
                    int amount = int.Parse(args[1]);
                    switch (stat)
                    {
                        case "health":
                        case "hp":
                            player.Stats[0] = amount;
                            break;
                        case "mana":
                        case "mp":
                            player.Stats[1] = amount;
                            break;
                        case "atk":
                        case "attack":
                            player.Stats[2] = amount;
                            break;
                        case "def":
                        case "defence":
                            player.Stats[3] = amount;
                            break;
                        case "spd":
                        case "speed":
                            player.Stats[4] = amount;
                            break;
                        case "vit":
                        case "vitality":
                            player.Stats[5] = amount;
                            break;
                        case "wis":
                        case "wisdom":
                            player.Stats[6] = amount;
                            break;
                        case "dex":
                        case "dexterity":
                            player.Stats[7] = amount;
                            break;
                        default:
                            player.SendError("Invalid Stat");
                            player.SendHelp("Stats: Health, Mana, Attack, Defence, Speed, Vitality, Wisdom, Dexterity");
                            player.SendHelp("Shortcuts: Hp, Mp, Atk, Def, Spd, Vit, Wis, Dex");
                            return false;
                    }
                    player.SaveToCharacter();
                    player.Client.Save();
                    player.UpdateCount++;
                    player.SendInfo("Success");
                }
                catch
                {
                    player.SendError("Error while setting stat");
                    return false;
                }
                return true;
            }
            else if (args.Length == 3)
            {
                foreach (Client i in player.Manager.Clients.Values)
                {
                    if (i.Account.Name.EqualsIgnoreCase(args[0]))
                    {
                        try
                        {
                            string stat = args[1].ToLower();
                            int amount = int.Parse(args[2]);
                            switch (stat)
                            {
                                case "health":
                                case "hp":
                                    i.Player.Stats[0] = amount;
                                    break;
                                case "mana":
                                case "mp":
                                    i.Player.Stats[1] = amount;
                                    break;
                                case "atk":
                                case "attack":
                                    i.Player.Stats[2] = amount;
                                    break;
                                case "def":
                                case "defence":
                                    i.Player.Stats[3] = amount;
                                    break;
                                case "spd":
                                case "speed":
                                    i.Player.Stats[4] = amount;
                                    break;
                                case "vit":
                                case "vitality":
                                    i.Player.Stats[5] = amount;
                                    break;
                                case "wis":
                                case "wisdom":
                                    i.Player.Stats[6] = amount;
                                    break;
                                case "dex":
                                case "dexterity":
                                    i.Player.Stats[7] = amount;
                                    break;
                                default:
                                    player.SendError("Invalid Stat");
                                    player.SendHelp("Stats: Health, Mana, Attack, Defence, Speed, Vitality, Wisdom, Dexterity");
                                    player.SendHelp("Shortcuts: Hp, Mp, Atk, Def, Spd, Vit, Wis, Dex");
                                    return false;
                            }
                            i.Player.SaveToCharacter();
                            i.Player.Client.Save();
                            i.Player.UpdateCount++;
                            player.SendInfo("Success");
                        }
                        catch
                        {
                            player.SendError("Error while setting stat");
                            return false;
                        }
                        return true;
                    }
                }
                player.SendError(string.Format("Player '{0}' could not be found!", args));
                return false;
            }
            else
            {
                player.SendHelp("Usage: /setStat <Stat> <Amount>");
                player.SendHelp("or");
                player.SendHelp("Usage: /setStat <Player> <Stat> <Amount>");
                player.SendHelp("Shortcuts: Hp, Mp, Atk, Def, Spd, Vit, Wis, Dex");
                return false;
            }
        }
    }
    internal class ListCommands : Command
    {
        public ListCommands() : base("commands", permLevel: Ranks.Founder) { }

        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            Dictionary<string, Command> cmds = new Dictionary<string, Command>();
            Type t = typeof(Command);
            foreach (Type i in t.Assembly.GetTypes())
                if (t.IsAssignableFrom(i) && i != t)
                {
                    Command instance = (Command)Activator.CreateInstance(i);
                    cmds.Add(instance.CommandName, instance);
                }
            StringBuilder sb = new StringBuilder("");
            Command[] copy = cmds.Values.ToArray();
            for (int i = 0; i < copy.Length; i++)
            {
                if (i != 0) sb.Append(", ");
                sb.Append(copy[i].CommandName);
            }

            player.SendInfo(sb.ToString());
            return true;
        }

        internal class freeze : Command
        {
            public freeze()
                : base("freeze", Ranks.Developer)
            {
            }

            protected override bool Process(Player player, RealmTime time, string[] args)
            {
                if (args.Length == 0)
                {
                    player.SendHelp("Usage: /freeze <playername>");
                    return false;
                }
                foreach (KeyValuePair<string, Client> i in player.Manager.Clients)
                {
                    if (i.Value.Player.Name.EqualsIgnoreCase(args[0]))
                    {
                        i.Value.Player.ApplyConditionEffect(new ConditionEffect
                        {
                            Effect = ConditionEffectIndex.Paralyzed,
                            DurationMS = -1
                        });
                        i.Value.Player.ApplyConditionEffect(new ConditionEffect
                        {
                            Effect = ConditionEffectIndex.Paused,
                            DurationMS = -1
                        });
                        i.Value.Player.UpdateCount++;
                        player.SendInfo("Player frozen!");
                        return true;
                    }
                }
                player.SendError("Player could not be found");
                return false;
            }
        }

        internal class Heal : Command
        {
            public Heal()
                : base("heal", Ranks.HeadDeveloper)
            {
            }

            protected override bool Process(Player player, RealmTime time, string[] args)
            {
                if (args.Length == 0)
                {
                    player.SendHelp("Usage: /heal <playername>");
                    return false;
                }
                foreach (Client i in player.Manager.Clients.Values)
                {
                    if (i.Account.Name.EqualsIgnoreCase(args[0]))
                    {
                        int max = i.Player.MaxHp;
                        i.Player.HP += max;
                        player.SendInfo("Player healed!");
                        return true;
                    }
                }
                player.SendError("Player could not be found");
                return false;
            }
        }

        internal class VisitCommand : Command
        {
            public VisitCommand()
                : base("visit", Ranks.SpecialPlayer)
            {
            }

            protected override bool Process(Player player, RealmTime time, string[] args)
            {
                if (args.Length < 1)
                {
                    player.SendHelp("Usage: /visit <playername>");
                    return false;
                }
                else if (String.Equals(player.Name.ToLower(), args[0].ToLower()))
                {
                    player.SendInfo("You are You.");
                    return false;
                }
                foreach (KeyValuePair<string, Client> i in player.Manager.Clients)
                {
                     if (i.Value.Player.Name.EqualsIgnoreCase(args[0]))
                    {
                        if (player.Owner == i.Value.Player.Owner)
                        {
                            player.SendInfo("You are already in the same world as this Player.");
                            return false;
                        }
                        else
                        {
                            player.Client.Reconnect(new ReconnectPacket
                            {
                                GameId = i.Value.Player.Owner.Id,
                                Host = "",
                                IsFromArena = false,
                                Key = i.Value.Player.Owner.PortalKey,
                                KeyTime = -1,
                                Name = i.Value.Player.Owner.Name,
                                Port = -1
                            });
                        }
                        return true;
                    }
                }
                player.SendError("Player could not be found!");
                return false;
            }
        }

        //KOMUTU ÜSTE EKLE
    }
}