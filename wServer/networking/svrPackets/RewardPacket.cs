using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace wServer.networking.svrPackets
{
    class CrateOpenPacket : ServerPacket
    {
        public uint ItemId { get; set; }

        public override PacketID ID
        {
            get { return PacketID.LOOTOPEN; }
        }
        public override Packet CreateInstance() { return new CrateOpenPacket(); }

        protected override void Read(Client psr, NReader rdr)
        {
            ItemId = rdr.ReadUInt16();
        }

        protected override void Write(Client psr, NWriter wtr)
        {
            wtr.Write(ItemId);
        }
    }
}