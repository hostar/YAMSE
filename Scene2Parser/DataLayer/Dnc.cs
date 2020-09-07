using System;
using System.Collections.Generic;
using System.Text;
using YAMSE.Interfaces;

namespace YAMSE.DataLayer
{
    public class Dnc
    {
        public int ID;
        public byte[] objectIDArr = new byte[2];
        public DncType dncType = DncType.Unknown;

        public NodeType dncKind = NodeType.Unknown;

        public string Name { get; set; }

        public IDncProps DncProps { get; set; }

        public byte[] rawData { get; set; }
        public byte[] rawDataBackup { get; set; }
    }
}
