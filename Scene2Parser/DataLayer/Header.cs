using System;
using System.Collections.Generic;
using System.Text;

namespace YAMSE.DataLayer
{
    public class Header
    {
        public List<byte> Magic { get; set; } = new List<byte>(); // 2 bytes
        public List<byte> Size { get; set; } = new List<byte>(); // 4 bytes
        public List<byte> Content { get; set; } = new List<byte>();
    }
}
