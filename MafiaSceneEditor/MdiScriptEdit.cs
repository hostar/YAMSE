using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using YAMSE.DataLayer;
//using FastColoredTextBoxNS;
using ScintillaNET;

namespace YAMSE
{
    public partial class MdiScriptEdit : Form
    {
        //private FastColoredTextBox textEditor;
        private Scintilla scintillaTextEditor;

        private Regex wordSplitRegex = new Regex("[a-zA-Z0-9_]+");
        private bool initDone = false;

        private MdiKind mdiKind;

        public Dnc Dnc;
        public Scene2Data Scene2Data;

        private System.Windows.Forms.Integration.ElementHost elementHostHexEditor;
        private WpfHexaEditor.HexEditor hexEditor;

        public MdiScriptEdit()
        {
            InitializeComponent();

            toolStripUndo.Image = Resources.undo;
            toolStripUndo.Click += ToolStripUndo_Click;

            /*
            textEditor = new FastColoredTextBoxNS.FastColoredTextBox
            {
                Parent = mainPanel,
                Dock = DockStyle.Fill
            };
            */

            tableLayoutPanel1.SetColumnSpan(mainPanel, 3);
            tableLayoutPanel1.SetRowSpan(mainPanel, 1);

            FormClosed += MdiScriptEdit_FormClosed;

            this.Show();
        }

        #region Public Methods
        public void SetEditorText(string text)
        {
            mdiKind = MdiKind.Text;
            CreateTextEditor();

            scintillaTextEditor.Show();
            //elementHostHexEditor.Hide();
            scintillaTextEditor.Text = text;
            initDone = true;
            try
            {
                SetStyle();
            }
            catch { }
        }

        public void SetHexEditorContent(Dnc dnc)
        {
            mdiKind = MdiKind.Hex;
            CreateHexEditor();

            //scintillaTextEditor.Hide();
            elementHostHexEditor.Show();

            var tmpStream = new MemoryStream();
            new MemoryStream(dnc.RawData).CopyTo(tmpStream); // needed in order to allow expanding
            hexEditor.Stream = tmpStream;
        }
        #endregion

        private void CreateHexEditor()
        {
            // create hex editor
            hexEditor = new WpfHexaEditor.HexEditor
            {
                ForegroundSecondColor = System.Windows.Media.Brushes.Blue,
                TypeOfCharacterTable = WpfHexaEditor.Core.CharacterTableType.Ascii,
                AllowExtend = true,
                AppendNeedConfirmation = false
            };

            elementHostHexEditor = new System.Windows.Forms.Integration.ElementHost
            {
                Dock = DockStyle.Fill
                //Location = new Point(250, 50),
                //Size = new Size(1000, 500)
            };
            elementHostHexEditor.Name = nameof(elementHostHexEditor);
            elementHostHexEditor.Child = hexEditor;
            elementHostHexEditor.Parent = this;

            mainPanel.Controls.Add(elementHostHexEditor);

            elementHostHexEditor.Hide();
        }

        private void CreateTextEditor()
        {
            scintillaTextEditor = new Scintilla
            {
                //WrapMode = WrapMode.Word, 
                //IndentationGuides = IndentView.LookBoth, 
                //Parent = mainPanel, 
                Dock = DockStyle.Fill,
                ScrollWidth = 200
            };

            scintillaTextEditor.Styles[ScintillaNET.Style.Default].Font = "Consolas";
            scintillaTextEditor.Styles[ScintillaNET.Style.Default].Size = 10;

            scintillaTextEditor.Lexer = Lexer.Null;

            //scintillaTextEditor.AssignCmdKey(Keys.Control | Keys.C , Command.)
            //scintillaTextEditor.ExecuteCmd(ScintillaNET.Command.)

            scintillaTextEditor.Margins[0].Width = 32;

            scintillaTextEditor.MouseDwellTime = 500;
            scintillaTextEditor.DwellStart += ScintillaTextEditor_DwellStart;
            scintillaTextEditor.TextChanged += ScintillaTextEditor_TextChanged;

            //mainPanel.Controls.Add(textEditor);
            mainPanel.Controls.Add(scintillaTextEditor);
        }

        private void ScintillaTextEditor_TextChanged(object sender, EventArgs e)
        {
            if (!initDone)
            {
                return;
            }
            ScintillaTextHighlight(scintillaTextEditor.Lines[scintillaTextEditor.LineFromPosition(scintillaTextEditor.CurrentPosition)].Text, scintillaTextEditor.CurrentPosition);
        }

        private void ScintillaTextEditor_DwellStart(object sender, DwellEventArgs e)
        {
            // scintillaTextEditor.CallTipShow(e.Position, "This feature is not implemented yet.");
        }

        private void MdiScriptEdit_FormClosed(object sender, FormClosedEventArgs e)
        {
            
        }

        private static System.Drawing.Color IntToColor(int rgb)
        {
            return System.Drawing.Color.FromArgb(255, (byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
        }

        private void ToolStripUndo_Click(object sender, EventArgs e)
        {
            //textEditor.Undo();
            scintillaTextEditor.Undo();
        }

        private void SetStyle()
        {
            scintillaTextEditor.Styles[1].ForeColor = System.Drawing.Color.Blue;
            scintillaTextEditor.Styles[2].ForeColor = System.Drawing.Color.Crimson;
            scintillaTextEditor.Styles[3].ForeColor = System.Drawing.Color.Blue;
            scintillaTextEditor.Styles[4].ForeColor = System.Drawing.Color.Green;
            ScintillaTextHighlight(scintillaTextEditor.Text, 0);
        }

        private void ScintillaTextHighlight(string textInput, int startPosition)
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
                while(currIndex == -1)
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

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void btnRevert_Click(object sender, EventArgs e)
        {
            switch (mdiKind)
            {
                case MdiKind.Text:
                    scintillaTextEditor.Text = Scene2Parser.GetScriptFromDnc(Dnc, true);
                    ScintillaTextHighlight(scintillaTextEditor.Text, 0);
                    break;
                case MdiKind.Hex:
                    Dnc.RawDataBackup.CopyTo(Dnc.RawData, 0);

                    var tmpStream = new MemoryStream();
                    new MemoryStream(Dnc.RawData).CopyTo(tmpStream); // needed in order to allow expanding
                    hexEditor.Stream = tmpStream;
                    break;
                default:
                    break;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            switch (mdiKind)
            {
                case MdiKind.Text:
                    Scene2Parser.UpdateStringInScriptDnc(Dnc, scintillaTextEditor.Text);
                    break;
                case MdiKind.Hex:
                    Dnc.RawData = hexEditor.GetAllBytes(true);
                    break;
                default:
                    break;
            }
        }
    }

    public enum MdiKind
    {
        Text,
        Hex
    }
}
