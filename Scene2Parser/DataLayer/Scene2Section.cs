using System;
using System.Collections.Generic;
using System.Text;

namespace YAMSE.DataLayer
{
    public class Scene2Section
    {
        public string SectionName { get; set; }

        public int Position { get; set; }

        public List<Dnc> Dncs { get; set; } = new List<Dnc>();

        public NodeType SectionType { get; set; }

        public int SectionStart { get; set; }

        public int SectionLength { get; set; }

        public int SectionEnd 
        { 
            get 
            {
                return SectionStart + SectionLength;
            }
        }
    }
}
