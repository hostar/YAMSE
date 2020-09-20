using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YAMSE.Interfaces;

namespace YAMSE.DataLayer
{
    public class ModelProps : StandardProps
    {
        public string Model { get; set; }
        public string Sector { get; set; }
        public bool HaveSector { get; set; }

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

            if (_dnc.Name == "9lamphl1744")
            {

            }

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

        public new void SaveData()
        {
            base.SaveData();
        }
    }
}
