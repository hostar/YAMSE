using ComponentFactory.Krypton.Toolkit;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using YAMSE.DataLayer;
using YAMSE.Interfaces;

namespace YAMSE
{
    public delegate void CallbackSetPropValue(object value, Control control);
    public delegate void CallbackSetComponentValue(IDncProps prop, Control control);

    public class DncMethods
    {
        public static void RemoveAsterisk(Control control)
        {
            if (control.Text.EndsWith('*'))
            {
                control.Text = control.Text.Substring(0, control.Text.Length - 1);
            }
        }

        public static Image PageKindToGlyph(PanelKind panelKind)
        {
            switch (panelKind)
            {
                case PanelKind.Script:
                    return Resources.script;
                case PanelKind.Enemy:
                    return Resources.enemy;
                case PanelKind.Standard:
                    return Resources.standard_object;
                case PanelKind.Model:
                    return Resources.model_object;
                case PanelKind.Hex:
                    return Resources.hex;
                default:
                    return Resources.hex;
            }
        }

        public static Image DrawText(string text, Font font, Color textColor, Color backColor)
        {
            //first, create a dummy bitmap just to get a graphics object
            Image img = new Bitmap(1, 1);
            Graphics drawing = Graphics.FromImage(img);

            //measure the string to see how big the image needs to be
            SizeF textSize = drawing.MeasureString(text, font);

            //free up the dummy image and old graphics object
            img.Dispose();
            drawing.Dispose();

            //create a new image of the right size
            img = new Bitmap((int)textSize.Width, (int)textSize.Height);

            drawing = Graphics.FromImage(img);

            //paint the background
            drawing.Clear(backColor);

            //create a brush for the text
            Brush textBrush = new SolidBrush(textColor);

            drawing.DrawString(text, font, textBrush, 0, 0);

            drawing.Save();

            textBrush.Dispose();
            drawing.Dispose();

            return img;

        }

        public static void BtnSaveClick(object sender, EventArgs eventArgs)
        {
            var pageId = ((KryptonButton)sender).Tag as KryptonPageId;

            //pageId.KryptonPage.Text = Active
            RemoveAsterisk(pageId.KryptonPage);

            pageId.IsDirty = false;

            switch (pageId.PanelKind)
            {
                case PanelKind.Script:
                    switch (pageId.Dnc.dncType)
                    {
                        case DncType.Script:
                            Scene2Parser.UpdateStringInScriptDnc(pageId.Dnc, pageId.TextEditor.GetText());
                            break;
                        case DncType.InitScript:
                            Scene2Parser.UpdateStringInInitScriptDnc(pageId.Dnc, pageId.TextEditor.GetText());
                            break;
                    }
                    break;
                case PanelKind.Hex:
                    pageId.Dnc.RawData = pageId.HexEditor.GetAllBytes(true);
                    break;
                case PanelKind.Enemy:
                    Scene2Parser.UpdateStringInEnemyDnc(pageId.Dnc, pageId.TextEditor.GetText());
                    pageId.Dnc.DncProps.SaveData();
                    break;
                case PanelKind.Standard:
                case PanelKind.Model:
                case PanelKind.Header:
                    pageId.Dnc.DncProps.SaveData();
                    break;
                default:
                    break;
            }
        }

        public static void BtnRevertClick(object sender, EventArgs eventArgs)
        {
            var pageId = ((KryptonButton)sender).Tag as KryptonPageId;

            pageId.IsDirty = false;

            switch (pageId.PanelKind)
            {
                case PanelKind.Enemy:
                case PanelKind.Script:
                    var text = Scene2Parser.GetScriptFromDnc(pageId.Dnc, true);
                    pageId.TextEditor.SetText(text);

                    if (pageId.Dnc.DncProps != null)
                    {
                        pageId.Dnc.DncProps.RevertData();
                    }

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

                    foreach (var container in pageId.KryptonPageContainer.Where(x => x.ComponentType == ComponentType.CheckBox))
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

        public static bool RawDataEqual(Dnc dnc1, Dnc dnc2)
        {
            if (dnc1.RawData.Length != dnc2.RawData.Length)
            {
                return false;
            }

            int len;
            if (dnc1.RawData.Length < dnc2.RawData.Length)
            {
                len = dnc1.RawData.Length;
            }
            else
            {
                len = dnc2.RawData.Length;
            }

            for (int i = 0; i < len; i++)
            {
                if (dnc1.RawData[i] != dnc2.RawData[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static int RawDataEqualGetDiffOffset(Dnc dnc1, Dnc dnc2)
        {
            int len;
            if (dnc1.RawData.Length < dnc2.RawData.Length)
            {
                len = dnc1.RawData.Length;
            }
            else
            {
                len = dnc2.RawData.Length;
            }

            for (int i = 0; i < len; i++)
            {
                if (dnc1.RawData[i] != dnc2.RawData[i])
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
