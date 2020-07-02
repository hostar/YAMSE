using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using FastColoredTextBoxNS;

namespace MafiaSceneEditor
{
    public partial class MdiScriptEdit : Form
    {
        private FastColoredTextBox textEditor;
        public MdiScriptEdit()
        {
            InitializeComponent();

            toolStripUndo.Image = Resources.undo;
            toolStripUndo.Click += ToolStripUndo_Click;

            textEditor = new FastColoredTextBoxNS.FastColoredTextBox
            {
                Parent = mainPanel,
                Dock = DockStyle.Fill
            };

            mainPanel.Controls.Add(textEditor);

            tableLayoutPanel1.SetColumnSpan(mainPanel, 3);
            tableLayoutPanel1.SetRowSpan(mainPanel, 1);

            FormClosed += MdiScriptEdit_FormClosed;

            this.Show();
        }

        private void MdiScriptEdit_FormClosed(object sender, FormClosedEventArgs e)
        {
            
        }

        private void ToolStripUndo_Click(object sender, EventArgs e)
        {
            textEditor.Undo();
        }

        public void SetEditorText(string text)
        {
            textEditor.Text = text;
        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }
    }
}
