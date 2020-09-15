using ScintillaNET;
using System.Windows.Forms;
using YAMSE.DataLayer;

namespace YAMSE
{
    public class KryptonPageContainer
    {
        public Control Component { get; set; }
        public int Column { get; set; }
        public int ColumnSpan { get; set; }
        public int RowSpan { get; set; }
        public int Row { get; set; } = 0;
    }
}
