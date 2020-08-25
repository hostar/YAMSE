using System;
using System.Collections.Generic;
using System.Text;

namespace YAMSE.DataLayer
{
    public class Dnc
    {
        public int ID;
        public byte[] objectIDArr = new byte[2];
        public DncType dncType = DncType.Unknown;

        public string name;

        public byte[] rawData;
        public byte[] rawDataBackup;
    }
}
