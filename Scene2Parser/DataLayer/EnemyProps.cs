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
        private float agressivity;
        private int behavior1;
        private float behavior2;
        private float driving;
        
        private float hearing;
        private float intelligence;
        private float mass;
        private float reactions;
        private float shooting;
        private float sight;
        private float speed;
        private float strength;
        private int voice;

        private int dataBegin;

        [TypeConverter(typeof(ExpandableObjectConverter))]
        public EnemyEnergy EnemyEnergy { get; set; }

        public float Agressivity { get => agressivity;
            set
            {
                agressivity = value;
                Scene2Parser.WriteToDnc(_dnc, DataBegin, value, 42);
            }
        }

        public int Behavior1 { get => behavior1; 
            set
            {
                behavior1 = value;
                Scene2Parser.WriteToDnc(_dnc, DataBegin, value, 2, false);
            }
        }

        public float Behavior2 { get => behavior2;
            set
            {
                behavior2 = value;
                Scene2Parser.WriteToDnc(_dnc, DataBegin, value, 34);
            }
        }

        public float Driving { get => driving;
            set
            {
                driving = value;
                Scene2Parser.WriteToDnc(_dnc, DataBegin, value, 62);
            }
        }

        

        public float Hearing { get => hearing;
            set
            {
                hearing = value;
                Scene2Parser.WriteToDnc(_dnc, DataBegin, value, 58);
            }
        }

        public float Intelligence { get => intelligence;
            set
            {
                intelligence = value;
                Scene2Parser.WriteToDnc(_dnc, DataBegin, value, 46);
            }
        }

        public float Mass { get => mass;
            set
            {
                mass = value;
                Scene2Parser.WriteToDnc(_dnc, DataBegin, value, 66);
            }
        }

        public float Reactions { get => reactions;
            set
            {
                reactions = value;
                Scene2Parser.WriteToDnc(_dnc, DataBegin, value, 70);
            }
        }

        public float Shooting { get => shooting;
            set
            {
                shooting = value;
                Scene2Parser.WriteToDnc(_dnc, DataBegin, value, 50);
            }
        }

        public float Sight { get => sight;
            set
            {
                sight = value;
                Scene2Parser.WriteToDnc(_dnc, DataBegin, value, 54);
            }
        }

        public float Speed { get => speed;
            set
            {
                speed = value;
                Scene2Parser.WriteToDnc(_dnc, DataBegin, value, 38);
            }
        }

        public float Strength { get => strength;
            set
            {
                strength = value;
                Scene2Parser.WriteToDnc(_dnc, DataBegin, value, 10);
            }
        }

        public int Voice { get => voice;
            set
            {
                voice = value;
                Scene2Parser.WriteToDnc(_dnc, DataBegin, value, 6, false);
            }
        }

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

        private Dnc _dnc;

        public EnemyProps(Dnc dnc)
        {
            _dnc = dnc;
            DataBeginLocator();

            Agressivity = BitConverter.ToSingle(dnc.rawData.Skip(DataBegin + 42).Take(4).ToArray(), 0);
            Behavior1 = dnc.rawData.Skip(DataBegin + 2).Take(1).First();
            Behavior2 = BitConverter.ToSingle(dnc.rawData.Skip(DataBegin + 34).Take(4).ToArray(), 0);
            Driving = BitConverter.ToSingle(dnc.rawData.Skip(DataBegin + 62).Take(4).ToArray(), 0);

            EnemyEnergy = new EnemyEnergy(_dnc, DataBegin)
            {
                Energy = BitConverter.ToSingle(dnc.rawData.Skip(DataBegin + 14).Take(4).ToArray(), 0),
                LeftHand = BitConverter.ToSingle(dnc.rawData.Skip(DataBegin + 18).Take(4).ToArray(), 0),
                RightHand = BitConverter.ToSingle(dnc.rawData.Skip(DataBegin + 22).Take(4).ToArray(), 0),
                LeftLeg = BitConverter.ToSingle(dnc.rawData.Skip(DataBegin + 26).Take(4).ToArray(), 0),
                RightLeg = BitConverter.ToSingle(dnc.rawData.Skip(DataBegin + 30).Take(4).ToArray(), 0),
            };

            Hearing = BitConverter.ToSingle(dnc.rawData.Skip(DataBegin + 58).Take(4).ToArray(), 0);
            Intelligence = BitConverter.ToSingle(dnc.rawData.Skip(DataBegin + 46).Take(4).ToArray(), 0);
            Mass = BitConverter.ToSingle(dnc.rawData.Skip(DataBegin + 66).Take(4).ToArray(), 0);
            Reactions = BitConverter.ToSingle(dnc.rawData.Skip(DataBegin + 70).Take(4).ToArray(), 0);
            Shooting = BitConverter.ToSingle(dnc.rawData.Skip(DataBegin + 50).Take(4).ToArray(), 0);
            Sight = BitConverter.ToSingle(dnc.rawData.Skip(DataBegin + 54).Take(4).ToArray(), 0);
            Speed = BitConverter.ToSingle(dnc.rawData.Skip(DataBegin + 38).Take(4).ToArray(), 0);
            Strength = BitConverter.ToSingle(dnc.rawData.Skip(DataBegin + 10).Take(4).ToArray(), 0);
            Voice = dnc.rawData.Skip(DataBegin + 6).Take(1).First();
        }

        public int DataBeginLocator()
        {
            DataBegin = _dnc.rawData.FindIndexOf(new byte[] { 0x24, 0xAE }).FirstOrDefault() + 5;
            return DataBegin;
        }
    }

    public class EnemyEnergy
    {
        private float energy;
        private float leftHand;
        private float rightHand;
        private float leftLeg;
        private float rightLeg;

        private Dnc _dnc;

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

        public float Energy
        {
            get => energy;
            set
            {
                energy = value;
                Scene2Parser.WriteToDnc(_dnc, DataBegin, value, 14);
            }
        }

        public float LeftHand
        {
            get => leftHand;
            set
            {
                leftHand = value;
                Scene2Parser.WriteToDnc(_dnc, DataBegin, value, 18);
            }
        }

        public float RightHand
        {
            get => rightHand;
            set
            {
                rightHand = value;
                Scene2Parser.WriteToDnc(_dnc, DataBegin, value, 22);
            }
        }

        public float LeftLeg
        {
            get => leftLeg;
            set
            {
                leftLeg = value;
                Scene2Parser.WriteToDnc(_dnc, DataBegin, value, 26);
            }
        }

        public float RightLeg
        {
            get => rightLeg;
            set
            {
                rightLeg = value;
                Scene2Parser.WriteToDnc(_dnc, DataBegin, value, 30);
            }
        }
    }
}
