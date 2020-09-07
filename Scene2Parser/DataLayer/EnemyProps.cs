using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using YAMSE.DataLayer;
using YAMSE;
using YAMSE.Interfaces;

namespace YAMSE.DataLayer
{
    public class EnemyProps : IDncProps
    {
        private float agressivity;
        private int behavior1;
        private float behavior2;
        private float driving;
        private float energy;
        private float leftHand;
        private float rightHand;
        private float leftLeg;
        private float rightLeg;
        private float hearing;
        private float intelligence;
        private float mass;
        private float reactions;
        private float shooting;
        private float sight;
        private float speed;
        private float strength;
        private int voice;

        public float Agressivity { get => agressivity;
            set
            {
                agressivity = value;
                WriteToDnc(value, 42);
            }
        }

        public int Behavior1 { get => behavior1; 
            set
            {
                behavior1 = value;
                WriteToDnc(value, 2, false);
            }
        }

        public float Behavior2 { get => behavior2;
            set
            {
                behavior2 = value;
                WriteToDnc(value, 34);
            }
        }

        public float Driving { get => driving;
            set
            {
                driving = value;
                WriteToDnc(value, 62);
            }
        }

        public float Energy { get => energy;
            set
            {
                energy = value;
                WriteToDnc(value, 14);
            }
        }
        public float LeftHand { get => leftHand;
            set
            {
                leftHand = value;
                WriteToDnc(value, 18);
            }
        }

        public float RightHand { get => rightHand;
            set
            {
                rightHand = value;
                WriteToDnc(value, 22);
            }
        }

        public float LeftLeg { get => leftLeg;
            set
            {
                leftLeg = value;
                WriteToDnc(value, 26);
            }
        }

        public float RightLeg { get => rightLeg;
            set
            {
                rightLeg = value;
                WriteToDnc(value, 30);
            }
        }

        public float Hearing { get => hearing;
            set
            {
                hearing = value;
                WriteToDnc(value, 58);
            }
        }

        public float Intelligence { get => intelligence;
            set
            {
                intelligence = value;
                WriteToDnc(value, 46);
            }
        }

        public float Mass { get => mass;
            set
            {
                mass = value;
                WriteToDnc(value, 66);
            }
        }

        public float Reactions { get => reactions;
            set
            {
                reactions = value;
                WriteToDnc(value, 70);
            }
        }

        public float Shooting { get => shooting;
            set
            {
                shooting = value;
                WriteToDnc(value, 50);
            }
        }

        public float Sight { get => sight;
            set
            {
                sight = value;
                WriteToDnc(value, 54);
            }
        }

        public float Speed { get => speed;
            set
            {
                speed = value;
                WriteToDnc(value, 38);
            }
        }

        public float Strength { get => strength;
            set
            {
                strength = value;
                WriteToDnc(value, 10);
            }
        }

        public int Voice { get => voice;
            set
            {
                voice = value;
                WriteToDnc(value, 6, false);
            }
        }

        public int DataBegin { get; set; }

        private Dnc _dnc;

        public EnemyProps(Dnc dnc)
        {
            _dnc = dnc;
            DataBeginLocator();

            Agressivity = BitConverter.ToSingle(dnc.rawData.Skip(DataBegin + 42).Take(4).ToArray(), 0);
            Behavior1 = dnc.rawData.Skip(DataBegin + 2).Take(1).First();
            Behavior2 = BitConverter.ToSingle(dnc.rawData.Skip(DataBegin + 34).Take(4).ToArray(), 0);
            Driving = BitConverter.ToSingle(dnc.rawData.Skip(DataBegin + 62).Take(4).ToArray(), 0);
            Energy = BitConverter.ToSingle(dnc.rawData.Skip(DataBegin + 14).Take(4).ToArray(), 0);
            LeftHand = BitConverter.ToSingle(dnc.rawData.Skip(DataBegin + 18).Take(4).ToArray(), 0);
            RightHand = BitConverter.ToSingle(dnc.rawData.Skip(DataBegin + 22).Take(4).ToArray(), 0);
            LeftLeg = BitConverter.ToSingle(dnc.rawData.Skip(DataBegin + 26).Take(4).ToArray(), 0);
            RightLeg = BitConverter.ToSingle(dnc.rawData.Skip(DataBegin + 30).Take(4).ToArray(), 0);
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

        private void WriteToDnc(float value, int indexInArray, bool isFloat = true)
        {
            if (isFloat)
            {
                Array.Copy(BitConverter.GetBytes(value), 0, _dnc.rawData, DataBegin + indexInArray, 4);
            }
            else
            {
                Array.Copy(BitConverter.GetBytes((int)value).Take(1).ToArray(), 0, _dnc.rawData, DataBegin + indexInArray, 1);
            }
        }

        public int DataBeginLocator()
        {
            DataBegin = _dnc.rawData.FindIndexOf(new byte[] { 0x24, 0xAE }).FirstOrDefault() + 5;
            return DataBegin;
        }
    }
}
