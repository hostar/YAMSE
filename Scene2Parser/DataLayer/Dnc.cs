using System;
using System.Collections.Generic;
using System.Text;

namespace YAMSE.DataLayer
{
    public class Dnc
    {
        public int ID;
        public byte[] objectIDArr = new byte[2];
        public ObjectIDs     objectType = ObjectIDs.Unknown;
        public DefinitionIDs definitionType = DefinitionIDs.Unknown;

        public string name;

        public byte[] rawData;
    }
}
