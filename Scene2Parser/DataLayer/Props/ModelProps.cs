using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using YAMSE.Interfaces;

namespace YAMSE.DataLayer
{
    public class ModelProps : StandardProps
    {
        private bool haveSector;

        public string Model { get; set; }
        public string Sector { get; set; }
        public bool HaveSector { get => haveSector;
            set
            {
                haveSector = value;
            }
        }

        private readonly Dnc _dnc;

        public ModelProps(Dnc dnc) : base(dnc)
        {
            _dnc = dnc;

            DataBeginLocator();

            RevertData();
        }

        public new void RevertData()
        {
            base.RevertData();

            // search for sector
            HaveSector = _dnc.RawData.Skip(DataBegin + 76).ToArray().FindIndexOf(new byte[] { 0, 0, 0, 0x10, 0 }).Any();

            if (HaveSector)
            {
                Sector = Scene2Parser.GetStringFromDnc(_dnc, DataBegin, 86);
                Model = Scene2Parser.GetStringFromDnc(_dnc, DataBegin, Sector.Length + 93);
            }
            else
            {
                Model = Scene2Parser.GetStringFromDnc(_dnc, DataBegin, 80);
            }            
        }

        public override void SaveData()
        {
            if (HaveSector)
            {
                _dnc.RawData = _dnc.RawData.Concat(Encoding.UTF8.GetBytes(new string('\0', Sector.Length + 20))).ToArray();

                var totSectorLen = Sector.Length + 13;
                var sectorArr = BitConverter.GetBytes(totSectorLen).Concat(new byte[] { 0x10, 0 }).Concat(BitConverter.GetBytes(Sector.Length + 7)).Concat(Encoding.UTF8.GetBytes(Sector)).Concat(new byte[] { 0, 0x12, 0x20 }).ToArray();
                var modelArr = BitConverter.GetBytes(Model.Length + 7).Concat(Encoding.UTF8.GetBytes(Model)).Concat(new byte[] { 0 }).ToArray();

                Scene2Parser.WriteArrayToDnc(sectorArr, DataBegin + 76, _dnc);
                Scene2Parser.WriteArrayToDnc(modelArr, DataBegin + Sector.Length + 89, _dnc);
            }
            else
            {
                var modelArr = BitConverter.GetBytes(Model.Length + 7).Concat(Encoding.UTF8.GetBytes(Model)).Concat(new byte[] { 0 }).ToArray();

                modelArr = modelArr.Concat(Encoding.UTF8.GetBytes(new string('\0', _dnc.RawData.Length - (DataBegin + 76) - modelArr.Length))).ToArray();
                Scene2Parser.WriteArrayToDnc(modelArr, DataBegin + 76, _dnc);
            }

            // cut zeros at the end
            Scene2Parser.CutZerosAtEndOfArray(_dnc);

            base.SaveData();
        }
    }
}
