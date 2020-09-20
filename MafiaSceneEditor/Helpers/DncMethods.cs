using ComponentFactory.Krypton.Toolkit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using YAMSE.DataLayer;
using YAMSE.Interfaces;

namespace YAMSE
{
    public delegate void CallbackSetPropValue(object value);
    public delegate void CallbackSetComponentValue(IDncProps prop, Control control);

    public class DncMethods
    {
        public static void BtnSaveClick(object sender, EventArgs eventArgs)
        {
            var pageId = ((KryptonButton)sender).Tag as KryptonPageId;

            switch (pageId.PanelKind)
            {
                case PanelKind.Script:
                    switch (pageId.Dnc.dncType)
                    {
                        case DncType.Script:
                        case DncType.InitScript:
                            Scene2Parser.UpdateStringInDnc(pageId.Dnc, pageId.ScintillaTextEditor.Text);
                            break;
                    }
                    break;
                case PanelKind.Hex:
                    pageId.Dnc.RawData = pageId.HexEditor.GetAllBytes(true);
                    break;
                case PanelKind.Enemy:
                    Scene2Parser.UpdateStringInEnemyDnc(pageId.Dnc, pageId.ScintillaTextEditor.Text);
                    pageId.Dnc.DncProps.SaveData();
                    break;
                case PanelKind.Standard:
                case PanelKind.Model:
                    pageId.Dnc.DncProps.SaveData();
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
                case PanelKind.Enemy:
                case PanelKind.Script:
                    var text = Scene2Parser.GetScriptFromDnc(pageId.Dnc, true);
                    pageId.ScintillaTextEditor.Text = text;
                    ScintillaTextHighlight(text, 0, pageId.ScintillaTextEditor);

                    pageId.Dnc.DncProps.RevertData();

                    var propGrid = pageId.KryptonPageContainer.First(x => x.ComponentType == ComponentType.PropertyGrid).Component as PropertyGrid;
                    propGrid.Refresh();

                    break;
                case PanelKind.Hex:
                    pageId.Dnc.RawDataBackup.CopyTo(pageId.Dnc.RawData, 0);

                    var tmpStream = new MemoryStream();
                    new MemoryStream(pageId.Dnc.RawData).CopyTo(tmpStream); // needed in order to allow expanding
                    //hexEditor.Stream = tmpStream;
                    break;
                case PanelKind.Standard:
                case PanelKind.Model:
                    pageId.Dnc.DncProps.RevertData();

                    foreach (var container in pageId.KryptonPageContainer.Where(x => x.ComponentType == ComponentType.TextBoxes))
                    {
                        container.SetComponentValue(pageId.Dnc.DncProps, container.Component);
                    }
                    break;
                default:
                    break;
            }
            
        }

        public static string CreatePageID(Dnc dnc)
        {
            return $"{dnc.dncKind} ; {dnc.dncType} ; {dnc.Name}";
        }

        public static void ScintillaTextHighlight(string textInput, int startPosition, ScintillaNET.Scintilla scintillaTextEditor)
        {
            int index = startPosition;

            scintillaTextEditor.StartStyling(index);

            foreach (var word in textInput.Split(null))
            {
                var wordLower = word.ToLower();
                if (string.IsNullOrWhiteSpace(wordLower))
                {
                    continue;
                }
                //Debug.WriteLine(word);

                var currIndex = scintillaTextEditor.Text.IndexOf(word, index, (scintillaTextEditor.Text.Length - index) <= 20 ? (scintillaTextEditor.Text.Length - index) : 20);

                int repeat = 0;
                while (currIndex == -1)
                {
                    index = scintillaTextEditor.Lines[scintillaTextEditor.LineFromPosition(index)].Position;
                    currIndex = scintillaTextEditor.Text.IndexOf(word, index);

                    repeat++;

                    if (repeat == 20)
                    {
                        break;
                    }
                }

                var toAdd = currIndex - index;

                scintillaTextEditor.StartStyling(currIndex);
                if (TextHighlight.Commands.Contains(wordLower))
                {
                    scintillaTextEditor.SetStyling(wordLower.Length, 1);
                }
                if (TextHighlight.Keywords.Contains(wordLower))
                {
                    scintillaTextEditor.SetStyling(wordLower.Length, 3);
                }
                if (TextHighlight.Declaration.Contains(wordLower))
                {
                    scintillaTextEditor.SetStyling(wordLower.Length, 2);
                }
                if (wordLower.StartsWith(TextHighlight.Comment))
                {
                    var currLineLen = scintillaTextEditor.Lines[scintillaTextEditor.LineFromPosition(currIndex)].Length;
                    scintillaTextEditor.SetStyling(currLineLen, 4);
                }

                index += wordLower.Length;
                index += toAdd;
            }
        }
    }
}
