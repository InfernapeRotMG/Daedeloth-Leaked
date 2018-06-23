﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using wServer.realm.entities;
using wServer.networking.svrPackets;
using wServer.realm.entities.player;
using wServer.networking;

namespace wServer.realm
{
    public class ChatManager
    {
        static ILog log = LogManager.GetLogger(typeof(ChatManager));

        RealmManager manager;
        public ChatManager(RealmManager manager)
        {
            this.manager = manager;
        }

        public void Say(Player src, string text)
        {
            src.Owner.BroadcastPacketSync(new TextPacket()
            {
                Name = (src.Client.Account.Rank >= 14 ? "#[Founder]" : src.Client.Account.Rank >= 13 ? "#[SpecialPlayer]" : src.Client.Account.Rank >= 12 ? "#[HeadDeveloper]" : src.Client.Account.Rank >= 11 ? "#[Developer]" : src.Client.Account.Rank >= 10 ? "#[HeadSpriter]" : src.Client.Account.Rank >= 9 ? "#[Staff]" : src.Client.Account.Rank >= 8 ? "#[Spriter]" : src.Client.Account.Rank >= 7 ? "#[Youtuber]" : src.Client.Account.Rank >= 6 ? "#[GoldDonor]" : src.Client.Account.Rank >= 5 ? "#[SilverDonor]" : src.Client.Account.Rank >= 4 ? "#[BronzeDonor]" : src.Client.Account.Rank >= 3 ? "#[AlphaTester]" : src.Client.Account.Rank >= 2 ? "#[TrustRWT]" : src.Client.Account.Rank >= 0 ? "#[Player]" : "") + src.Name,
                ObjectId = src.Id,
                Stars = src.Stars,
                BubbleTime = 10,
                Recipient = "",
                Text = text.ToSafeText(),
                CleanText = text.ToSafeText()
            }, p => !p.Ignored.Contains(src.AccountId));
            log.InfoFormat("[{0}({1})] <{2}> {3}", src.Owner.Name, src.Owner.Id, src.Name, text);
            src.Owner.ChatReceived(text);
        }

        public void SayGuild(Player src, string text)
        {
            foreach (Client i in src.Manager.Clients.Values)
            {
                if (String.Equals(src.Guild, i.Player.Guild))
                {
                    i.SendPacket(new TextPacket()
                    {
                        Name = src.ResolveGuildChatName(),
                        ObjectId = src.Id,
                        Stars = src.Stars,
                        BubbleTime = 10,
                        Recipient = "*Guild*",
                        Text = text.ToSafeText(),
                        CleanText = text.ToSafeText()
                    });
                }
            }
        }

        public void News(string text)
        {
            foreach (var i in manager.Clients.Values)
                i.SendPacket(new TextPacket()
                {
                    BubbleTime = 0,
                    Stars = -1,
                    Name = "@NEWS",
                    Text = text.ToSafeText()
                });
            log.InfoFormat("<NEWS> {0}", text);
        }

        public void Announce(string text)
        {
            foreach (var i in manager.Clients.Values)
                i.SendPacket(new TextPacket()
                {
                    BubbleTime = 0,
                    Stars = -1,
                    Name = "@Announcement",
                    Text = text.ToSafeText()
                });
            log.InfoFormat("<Announcement> {0}", text);
        }

        public void Oryx(World world, string text)
        {
            world.BroadcastPacket(new TextPacket()
            {
                BubbleTime = 0,
                Stars = -1,
                Name = "#Oryx the Mad God",
                Text = text.ToSafeText()
            }, null);
            log.InfoFormat("[{0}({1})] <Oryx the Mad God> {2}", world.Name, world.Id, text);
        }
    }
}
