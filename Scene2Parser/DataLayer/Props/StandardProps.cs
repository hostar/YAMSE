using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YAMSE.Interfaces;

namespace YAMSE.DataLayer
{
    public class StandardProps : IDncProps
    {
        public int DataBegin { get; set; }

        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float PositionZ { get; set; }


        public float RotationX { get; set; }
        public float RotationY { get; set; }
        public float RotationZ { get; set; }

        public float ScalingX { get; set; }
        public float ScalingY { get; set; }
        public float ScalingZ { get; set; }

        public byte[] RawData { get; set; }

        private readonly Dnc _dnc;

        public StandardProps(Dnc dnc)
        {
            _dnc = dnc;
            DataBeginLocator();

            RevertData();
        }

        public int DataBeginLocator()
        {
            DataBegin = 23 + _dnc.Name.Length;
            return DataBegin;
        }

        public void RevertData()
        {
            PositionX = BitConverter.ToSingle(_dnc.RawData.Skip(DataBegin + 4).Take(4).ToArray(), 0);
            PositionY = BitConverter.ToSingle(_dnc.RawData.Skip(DataBegin + 8).Take(4).ToArray(), 0);
            PositionZ = BitConverter.ToSingle(_dnc.RawData.Skip(DataBegin + 12).Take(4).ToArray(), 0);

            RotationX = BitConverter.ToSingle(_dnc.RawData.Skip(DataBegin + 26).Take(4).ToArray(), 0);
            RotationY = BitConverter.ToSingle(_dnc.RawData.Skip(DataBegin + 30).Take(4).ToArray(), 0);
            RotationZ = BitConverter.ToSingle(_dnc.RawData.Skip(DataBegin + 34).Take(4).ToArray(), 0);

            ScalingX = BitConverter.ToSingle(_dnc.RawData.Skip(DataBegin + 44).Take(4).ToArray(), 0);
            ScalingY = BitConverter.ToSingle(_dnc.RawData.Skip(DataBegin + 48).Take(4).ToArray(), 0);
            ScalingZ = BitConverter.ToSingle(_dnc.RawData.Skip(DataBegin + 52).Take(4).ToArray(), 0);
        }

        public void SaveData()
        {
            Scene2Parser.WriteToDnc(_dnc, DataBegin, PositionX, 4);
            Scene2Parser.WriteToDnc(_dnc, DataBegin, PositionY, 8);
            Scene2Parser.WriteToDnc(_dnc, DataBegin, PositionZ, 12);

            Scene2Parser.WriteToDnc(_dnc, DataBegin, RotationX, 26);
            Scene2Parser.WriteToDnc(_dnc, DataBegin, RotationY, 30);
            Scene2Parser.WriteToDnc(_dnc, DataBegin, RotationZ, 34);

            Scene2Parser.WriteToDnc(_dnc, DataBegin, ScalingX, 44);
            Scene2Parser.WriteToDnc(_dnc, DataBegin, ScalingY, 48);
            Scene2Parser.WriteToDnc(_dnc, DataBegin, ScalingZ, 52);
        }
    }
}
