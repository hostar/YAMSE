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
            DataBegin = _dnc.RawData.FindIndexOf(new byte[] { 0x24, 0xAE }).FirstOrDefault() + 2;
            return DataBegin;
        }

        public void SaveData()
        {
            Scene2Parser.WriteToDnc(_dnc, DataBegin, Voice, 9, false);
            Scene2Parser.WriteToDnc(_dnc, DataBegin, Strength, 13);
            Scene2Parser.WriteToDnc(_dnc, DataBegin, Speed, 41);
            Scene2Parser.WriteToDnc(_dnc, DataBegin, Sight, 57);
            Scene2Parser.WriteToDnc(_dnc, DataBegin, Shooting, 53);
            Scene2Parser.WriteToDnc(_dnc, DataBegin, Reactions, 73);
            Scene2Parser.WriteToDnc(_dnc, DataBegin, Mass, 69);
            Scene2Parser.WriteToDnc(_dnc, DataBegin, Intelligence, 49);
            Scene2Parser.WriteToDnc(_dnc, DataBegin, Hearing, 61);
            Scene2Parser.WriteToDnc(_dnc, DataBegin, Driving, 65);
            Scene2Parser.WriteToDnc(_dnc, DataBegin, Behavior2, 37);
            Scene2Parser.WriteToDnc(_dnc, DataBegin, Behavior1, 5, false);
            Scene2Parser.WriteToDnc(_dnc, DataBegin, Agressivity, 45);

            Scene2Parser.WriteToDnc(_dnc, DataBegin, EnemyEnergy.Energy, 17);
            Scene2Parser.WriteToDnc(_dnc, DataBegin, EnemyEnergy.LeftHand, 21);
            Scene2Parser.WriteToDnc(_dnc, DataBegin, EnemyEnergy.RightHand, 25);
            Scene2Parser.WriteToDnc(_dnc, DataBegin, EnemyEnergy.LeftLeg, 29);
            Scene2Parser.WriteToDnc(_dnc, DataBegin, EnemyEnergy.RightLeg, 33);
        }

        public void RevertData()
        {
            Agressivity = BitConverter.ToSingle(_dnc.RawData.Skip(DataBegin + 45).Take(4).ToArray(), 0);
            Behavior1 = _dnc.RawData.Skip(DataBegin + 5).Take(1).First();
            Behavior2 = BitConverter.ToSingle(_dnc.RawData.Skip(DataBegin + 37).Take(4).ToArray(), 0);
            Driving = BitConverter.ToSingle(_dnc.RawData.Skip(DataBegin + 65).Take(4).ToArray(), 0);

            EnemyEnergy = new EnemyEnergy(_dnc, DataBegin)
            {
                Energy = BitConverter.ToSingle(_dnc.RawData.Skip(DataBegin + 17).Take(4).ToArray(), 0),
                LeftHand = BitConverter.ToSingle(_dnc.RawData.Skip(DataBegin + 21).Take(4).ToArray(), 0),
                RightHand = BitConverter.ToSingle(_dnc.RawData.Skip(DataBegin + 25).Take(4).ToArray(), 0),
                LeftLeg = BitConverter.ToSingle(_dnc.RawData.Skip(DataBegin + 29).Take(4).ToArray(), 0),
                RightLeg = BitConverter.ToSingle(_dnc.RawData.Skip(DataBegin + 33).Take(4).ToArray(), 0),
            };

            Hearing = BitConverter.ToSingle(_dnc.RawData.Skip(DataBegin + 61).Take(4).ToArray(), 0);
            Intelligence = BitConverter.ToSingle(_dnc.RawData.Skip(DataBegin + 49).Take(4).ToArray(), 0);
            Mass = BitConverter.ToSingle(_dnc.RawData.Skip(DataBegin + 69).Take(4).ToArray(), 0);
            Reactions = BitConverter.ToSingle(_dnc.RawData.Skip(DataBegin + 73).Take(4).ToArray(), 0);
            Shooting = BitConverter.ToSingle(_dnc.RawData.Skip(DataBegin + 53).Take(4).ToArray(), 0);
            Sight = BitConverter.ToSingle(_dnc.RawData.Skip(DataBegin + 57).Take(4).ToArray(), 0);
            Speed = BitConverter.ToSingle(_dnc.RawData.Skip(DataBegin + 41).Take(4).ToArray(), 0);
            Strength = BitConverter.ToSingle(_dnc.RawData.Skip(DataBegin + 13).Take(4).ToArray(), 0);
            Voice = _dnc.RawData.Skip(DataBegin + 9).Take(1).First();
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
