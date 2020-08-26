using ComponentFactory.Krypton.Toolkit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using YAMSE.DataLayer;

namespace YAMSE
{
    public class DncMethods
    {
        public static void BtnSaveClick(object sender, EventArgs eventArgs)
        {
            var pageId = ((KryptonButton)sender).Tag as KryptonPageId;
            
            switch (pageId.PanelKind)
            {
                case PanelKind.Text:
                    Scene2Parser.UpdateStringInDnc(pageId.Dnc, pageId.ScintillaTextEditor.Text);
                    break;
                case PanelKind.Hex:
                    pageId.Dnc.rawData = pageId.HexEditor.GetAllBytes(true);
                    break;
                default:
                    break;
            }
            
        }

        public static void BtnRevertClick(object sender, EventArgs eventArgs)
        {
            var pageId = ((KryptonButton)sender).Tag as KryptonPageId;

            switch (pageId.PanelKind)
            {
                case PanelKind.Text:
                    var text = Scene2Parser.GetStringFromDnc(pageId.Dnc, true);
                    pageId.ScintillaTextEditor.Text = text;
                    ScintillaTextHighlight(text, 0, pageId.ScintillaTextEditor);
                    break;
                case PanelKind.Hex:
                    pageId.Dnc.rawDataBackup.CopyTo(pageId.Dnc.rawData, 0);

                    var tmpStream = new MemoryStream();
                    new MemoryStream(pageId.Dnc.rawData).CopyTo(tmpStream); // needed in order to allow expanding
                    //hexEditor.Stream = tmpStream;
                    break;
                default:
                    break;
            }
            
        }

        public static string CreatePageID(Dnc dnc)
        {
            return $"{dnc.dncType} ; {dnc.name}";
        }

        public static string CreatePageID(Dnc dnc, NodeType nodeType)
        {
            return $"{nodeType} ; {dnc.dncType} ; {dnc.name}";
        }

        public static void ScintillaTextHighlight(string textInput, int startPosition, ScintillaNET.Scintilla scintillaTextEditor)
        {
            int index = startPosition;

            scintillaTextEditor.StartStyling(index);

            foreach (var word in textInput.Split(null))
            {
                if (string.IsNullOrWhiteSpace(word))
                {
                    continue;
                }
                //Debug.WriteLine(word);

                var currIndex = scintillaTextEditor.Text.IndexOf(word, index, (scintillaTextEditor.Text.Length - index) <= 20 ? (scintillaTextEditor.Text.Length - index) : 20);
                while (currIndex == -1)
                {
                    index = scintillaTextEditor.Lines[scintillaTextEditor.LineFromPosition(index)].Position;
                    currIndex = scintillaTextEditor.Text.IndexOf(word, index);
                }

                var toAdd = currIndex - index;

                scintillaTextEditor.StartStyling(currIndex);
                if (TextHighlight.Commands.Contains(word))
                {
                    scintillaTextEditor.SetStyling(word.Length, 1);
                }
                if (TextHighlight.Keywords.Contains(word))
                {
                    scintillaTextEditor.SetStyling(word.Length, 3);
                }
                if (TextHighlight.Declaration.Contains(word))
                {
                    scintillaTextEditor.SetStyling(word.Length, 2);
                }
                if (word.StartsWith(TextHighlight.Comment))
                {
                    var currLineLen = scintillaTextEditor.Lines[scintillaTextEditor.LineFromPosition(currIndex)].Length;
                    scintillaTextEditor.SetStyling(currLineLen, 4);
                }

                index += word.Length;
                index += toAdd;
            }
        }
    }
}
