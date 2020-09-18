using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YAMSE.Interfaces;

namespace YAMSE.DataLayer
{
    public class ModelProps : StandardProps
    {
        private readonly Dnc _dnc;

        public ModelProps(Dnc dnc) : base(dnc)
        {
            _dnc = dnc;
        }

        public new void RevertData()
        {
            base.RevertData();
            // TODO: add parsing of model name and sector
        }

        public new void SaveData()
        {
            base.SaveData();
        }
    }
}
