using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using YAMSE.DataLayer;
using YAMSE;
using YAMSE.Interfaces;
using System.ComponentModel;

namespace YAMSE.DataLayer
{
    public class EnemyProps : IDncProps
    {
        private int dataBegin;

        [TypeConverter(typeof(ExpandableObjectConverter))]
        public EnemyEnergy EnemyEnergy { get; set; }

        public float Agressivity { get; set; }

        public int Behavior1 { get; set; }

        public float Behavior2 { get; set; }

        public float Driving { get; set; }


        public float Hearing { get; set; }

        public float Intelligence { get; set; }

        public float Mass { get; set; }

        public float Reactions { get; set; }

        public float Shooting { get; set; }

        public float Sight { get; set; }

        public float Speed { get; set; }

        public float Strength { get; set; }

        public int Voice { get; set; }

        [Browsable(false)]
        public int DataBegin { get => dataBegin;
            set
            {
                dataBegin = value;
                if (EnemyEnergy != null)
                {
                    EnemyEnergy.DataBegin = value;
                }
            }
        }

        [Browsable(false)]
        public byte[] RawData { get; set; }

        private readonly Dnc _dnc;

        public EnemyProps(Dnc dnc)
        {
            _dnc = dnc;
            DataBeginLocator();

            RevertData();
        }

        public int DataBeginLocator()
        {
            DataBegin = _dnc.RawData.FindIndexOf(new byte[] { 0x24, 0xAE }).FirstOrDefault() + 5;
            return DataBegin;
        }

        public void SaveData()
        {
            Scene2Parser.WriteToDnc(_dnc, DataBegin, Voice, 6, false);
            Scene2Parser.WriteToDnc(_dnc, DataBegin, Strength, 10);
            Scene2Parser.WriteToDnc(_dnc, DataBegin, Speed, 38);
            Scene2Parser.WriteToDnc(_dnc, DataBegin, Sight, 54);
            Scene2Parser.WriteToDnc(_dnc, DataBegin, Shooting, 50);
            Scene2Parser.WriteToDnc(_dnc, DataBegin, Reactions, 70);
            Scene2Parser.WriteToDnc(_dnc, DataBegin, Mass, 66);
            Scene2Parser.WriteToDnc(_dnc, DataBegin, Intelligence, 46);
            Scene2Parser.WriteToDnc(_dnc, DataBegin, Hearing, 58);
            Scene2Parser.WriteToDnc(_dnc, DataBegin, Driving, 62);
            Scene2Parser.WriteToDnc(_dnc, DataBegin, Behavior2, 34);
            Scene2Parser.WriteToDnc(_dnc, DataBegin, Behavior1, 2, false);
            Scene2Parser.WriteToDnc(_dnc, DataBegin, Agressivity, 42);

            Scene2Parser.WriteToDnc(_dnc, DataBegin, EnemyEnergy.Energy, 14);
            Scene2Parser.WriteToDnc(_dnc, DataBegin, EnemyEnergy.LeftHand, 18);
            Scene2Parser.WriteToDnc(_dnc, DataBegin, EnemyEnergy.RightHand, 22);
            Scene2Parser.WriteToDnc(_dnc, DataBegin, EnemyEnergy.LeftLeg, 26);
            Scene2Parser.WriteToDnc(_dnc, DataBegin, EnemyEnergy.RightLeg, 30);
        }

        public void RevertData()
        {
            Agressivity = BitConverter.ToSingle(_dnc.RawData.Skip(DataBegin + 42).Take(4).ToArray(), 0);
            Behavior1 = _dnc.RawData.Skip(DataBegin + 2).Take(1).First();
            Behavior2 = BitConverter.ToSingle(_dnc.RawData.Skip(DataBegin + 34).Take(4).ToArray(), 0);
            Driving = BitConverter.ToSingle(_dnc.RawData.Skip(DataBegin + 62).Take(4).ToArray(), 0);

            EnemyEnergy = new EnemyEnergy(_dnc, DataBegin)
            {
                Energy = BitConverter.ToSingle(_dnc.RawData.Skip(DataBegin + 14).Take(4).ToArray(), 0),
                LeftHand = BitConverter.ToSingle(_dnc.RawData.Skip(DataBegin + 18).Take(4).ToArray(), 0),
                RightHand = BitConverter.ToSingle(_dnc.RawData.Skip(DataBegin + 22).Take(4).ToArray(), 0),
                LeftLeg = BitConverter.ToSingle(_dnc.RawData.Skip(DataBegin + 26).Take(4).ToArray(), 0),
                RightLeg = BitConverter.ToSingle(_dnc.RawData.Skip(DataBegin + 30).Take(4).ToArray(), 0),
            };

            Hearing = BitConverter.ToSingle(_dnc.RawData.Skip(DataBegin + 58).Take(4).ToArray(), 0);
            Intelligence = BitConverter.ToSingle(_dnc.RawData.Skip(DataBegin + 46).Take(4).ToArray(), 0);
            Mass = BitConverter.ToSingle(_dnc.RawData.Skip(DataBegin + 66).Take(4).ToArray(), 0);
            Reactions = BitConverter.ToSingle(_dnc.RawData.Skip(DataBegin + 70).Take(4).ToArray(), 0);
            Shooting = BitConverter.ToSingle(_dnc.RawData.Skip(DataBegin + 50).Take(4).ToArray(), 0);
            Sight = BitConverter.ToSingle(_dnc.RawData.Skip(DataBegin + 54).Take(4).ToArray(), 0);
            Speed = BitConverter.ToSingle(_dnc.RawData.Skip(DataBegin + 38).Take(4).ToArray(), 0);
            Strength = BitConverter.ToSingle(_dnc.RawData.Skip(DataBegin + 10).Take(4).ToArray(), 0);
            Voice = _dnc.RawData.Skip(DataBegin + 6).Take(1).First();
        }
    }

    public class EnemyEnergy
    {
        private readonly Dnc _dnc;

        public override string ToString()
        {
            return $"{Energy}; {LeftHand}; {RightHand}; {LeftLeg}; {RightLeg}";
        }

        [Browsable(false)]
        public int DataBegin { get; set; }

        public EnemyEnergy(Dnc dnc, int dataBegin)
        {
            _dnc = dnc;
            DataBegin = dataBegin;
        }

        public float Energy { get; set; }

        public float LeftHand { get; set; }

        public float RightHand { get; set; }

        public float LeftLeg { get; set; }

        public float RightLeg { get; set; }
    }
}
