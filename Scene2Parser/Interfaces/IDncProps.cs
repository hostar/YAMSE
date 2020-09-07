using System;
using System.Collections.Generic;
using System.Text;

namespace YAMSE.Interfaces
{
    public interface IDncProps
    {
        int DataBegin { get; set; }

        int DataBeginLocator();
    }
}
