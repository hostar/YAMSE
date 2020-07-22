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
//using FastColoredTextBoxNS;
using ScintillaNET;

namespace MafiaSceneEditor
{
    public partial class MdiScriptEdit : Form
    {
        //private FastColoredTextBox textEditor;
        private Scintilla scintillaTextEditor;

        private Regex wordSplitRegex = new Regex("[a-zA-Z0-9_]+");
        private bool initDone = false;

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

            scintillaTextEditor = new Scintilla
            {
                //WrapMode = WrapMode.Word, 
                //IndentationGuides = IndentView.LookBoth, 
                //Parent = mainPanel, 
                Dock = DockStyle.Fill
            };

            scintillaTextEditor.Styles[ScintillaNET.Style.Default].Font = "Consolas";
            scintillaTextEditor.Styles[ScintillaNET.Style.Default].Size = 10;

            scintillaTextEditor.Lexer = Lexer.Null;

            /*
            scintillaTextEditor.Styles[1].ForeColor = System.Drawing.Color.Red;
            scintillaTextEditor.Styles[Style.Cpp.Identifier].ForeColor = IntToColor(0xD0DAE2);
            scintillaTextEditor.Styles[Style.Cpp.Comment].ForeColor = IntToColor(0x000000);
            scintillaTextEditor.Styles[Style.Cpp.CommentLine].ForeColor = IntToColor(0x000000);
            scintillaTextEditor.Styles[Style.Cpp.CommentDoc].ForeColor = IntToColor(0x000000);
            scintillaTextEditor.Styles[Style.Cpp.Number].ForeColor = IntToColor(0x000000);
            scintillaTextEditor.Styles[Style.Cpp.String].ForeColor = IntToColor(0x000000);
            scintillaTextEditor.Styles[Style.Cpp.Character].ForeColor = IntToColor(0x000000);
            scintillaTextEditor.Styles[Style.Cpp.Preprocessor].ForeColor = IntToColor(0x000000);
            scintillaTextEditor.Styles[Style.Cpp.Operator].ForeColor = IntToColor(0x000000);
            scintillaTextEditor.Styles[Style.Cpp.Regex].ForeColor = IntToColor(0x000000);
            scintillaTextEditor.Styles[Style.Cpp.CommentLineDoc].ForeColor = IntToColor(0x000000);
            scintillaTextEditor.Styles[Style.Cpp.Word].ForeColor = IntToColor(0x000000);
            scintillaTextEditor.Styles[Style.Cpp.Word2].ForeColor = IntToColor(0x000000);
            scintillaTextEditor.Styles[Style.Cpp.CommentDocKeyword].ForeColor = IntToColor(0x000000);
            scintillaTextEditor.Styles[Style.Cpp.CommentDocKeywordError].ForeColor = IntToColor(0x000000);
            scintillaTextEditor.Styles[Style.Cpp.GlobalClass].ForeColor = IntToColor(0x000000);
            */

            //scintillaTextEditor.AssignCmdKey(Keys.Control | Keys.C , Command.)
            //scintillaTextEditor.ExecuteCmd(ScintillaNET.Command.)

            scintillaTextEditor.Margins[0].Width = 32;

            scintillaTextEditor.MouseDwellTime = 500;
            scintillaTextEditor.DwellStart += ScintillaTextEditor_DwellStart;
            scintillaTextEditor.TextChanged += ScintillaTextEditor_TextChanged;

            //mainPanel.Controls.Add(textEditor);
            mainPanel.Controls.Add(scintillaTextEditor);

            tableLayoutPanel1.SetColumnSpan(mainPanel, 3);
            tableLayoutPanel1.SetRowSpan(mainPanel, 1);

            FormClosed += MdiScriptEdit_FormClosed;

            this.Show();
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
            scintillaTextEditor.CallTipShow(e.Position, "This feature is not implemented yet.");
        }

        private void MdiScriptEdit_FormClosed(object sender, FormClosedEventArgs e)
        {
            
        }

        public static System.Drawing.Color IntToColor(int rgb)
        {
            return System.Drawing.Color.FromArgb(255, (byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
        }

        private void ToolStripUndo_Click(object sender, EventArgs e)
        {
            //textEditor.Undo();
            scintillaTextEditor.Undo();
        }

        public void SetEditorText(string text)
        {
            //textEditor.Text = text;
            scintillaTextEditor.Text = text;
            initDone = true;
            try
            {
                SetStyle();
            }
            catch { }
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
            int index = 0;
            int indexInScintilla = startPosition;

            scintillaTextEditor.StartStyling(indexInScintilla);

            foreach (var word in textInput.Split(null))
            {
                Debug.WriteLine(word);
                var currIndex = scintillaTextEditor.Text.IndexOf(word, indexInScintilla);

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
            
        }
    }
}
