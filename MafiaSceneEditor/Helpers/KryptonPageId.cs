﻿using ComponentFactory.Krypton.Navigator;
using ScintillaNET;
using System.Collections.Generic;
using YAMSE.DataLayer;

namespace YAMSE
{
    public enum PanelKind
    {
        Script,
        Enemy,
        Standard,
        Model,
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

        public IEnumerable<KryptonPageContainer> KryptonPageContainer { get; set; }

        public WpfHexaEditor.HexEditor HexEditor { get; set; }

        public KryptonPage KryptonPage { get; set; }
        public bool IsDirty { get; set; }

        public override string ToString()
        {
            return DncMethods.CreatePageID(dnc);
        }
    }
}
