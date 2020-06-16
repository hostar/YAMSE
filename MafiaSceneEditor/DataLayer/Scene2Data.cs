﻿using System;
using System.Collections.Generic;
using System.Text;

namespace MafiaSceneEditor.DataLayer
{
    public class Scene2Data
    {
        byte[] scene2HeaderNewFile = new byte[]
            {
                0x53,0x4C,0x6C,0x00,0x00,0x00,0x01,0x00,0x0C,0x00,0x00,0x00,0x05,0x00,0x0A,0x00,
                0x00,0x00,0xFF,0xAF,0x0E,0x00,0x00,0x00,0xE7,0xB1,0xEB,0x74,0x02,0x00,0x00,0x00,
                0x00,0x32,0x12,0x00,0x00,0x00,0xBD,0xBC,0xBC,0x3E,0xAB,0xAA,0xAA,0x3E,0x8D,0x8C,
                0x8C,0x3E,0x10,0x30,0x0A,0x00,0x00,0x00,0x92,0x0A,0x86,0x3F,0x11,0x30,0x0A,0x00,
                0x00,0x00,0x00,0x00,0x96,0x43,0x11,0x32,0x0E,0x00,0x00,0x00,0x00,0x00,0x80,0x3F,
                0x00,0x00,0x7A,0x44
            };

        public List<Dnc> objectsDncs = new List<Dnc>();
        public List<Dnc> objectDefinitionsDncs = new List<Dnc>();

        public List<byte> rawDataHeader = new List<byte>();

        public byte[] standardObjectsHeader = new byte[] { 0x00,0x40 /*,0xFF,0x00,0x00,0x00 */ };

        public int standardObjectsStartPosition = 0;
        public int standardObjectsLength = 0;

        public int objectsDefinitionStartPosition = 0;
        public int objectsDefinitionLength = 0;
    }
}
