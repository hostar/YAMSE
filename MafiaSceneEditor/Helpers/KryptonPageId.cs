using ScintillaNET;
using YAMSE.DataLayer;

namespace YAMSE
{
    public enum PanelKind
    {
        Script,
        Enemy,
        Hex
    }


    public class KryptonPageId
    {
        public PanelKind PanelKind { get; set; }

        private Dnc dnc;
        public Dnc Dnc 
        { 
            get => dnc;
            set 
            {
                pageId = DncMethods.CreatePageID(value);
                dnc = value;
            }
        }

        private string pageId = string.Empty;
        public string PageId
        {
            get => pageId;
        }

        public Scintilla ScintillaTextEditor { get; set; }

        public KryptonPageContainer KryptonPageContainer { get; set; }

        public WpfHexaEditor.HexEditor HexEditor { get; set; }

        public override string ToString()
        {
            return DncMethods.CreatePageID(dnc);
        }
    }
}
