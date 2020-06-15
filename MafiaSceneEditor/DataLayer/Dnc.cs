using System;
using System.Collections.Generic;
using System.Text;

namespace MafiaSceneEditor.DataLayer
{
    public class Dnc
    {
        public byte[] objectIDArr = new byte[2];
        public IDs    objectIDEnum = IDs.Unknown;

        public byte[] rawData;
    }
}
