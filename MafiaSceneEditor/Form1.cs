using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using MafiaSceneEditor.DataLayer;
using System.Windows.Interop;

namespace MafiaSceneEditor
{
    public partial class Form1 : Form
    {
        Scene2Data scene2Data = new Scene2Data();
        WpfHexaEditor.HexEditor hexEditor;

        public Form1()
        {
            InitializeComponent();

            // create hex editor
            var elementHost = new System.Windows.Forms.Integration.ElementHost();
            hexEditor = new WpfHexaEditor.HexEditor();

            hexEditor.ForegroundSecondColor = System.Windows.Media.Brushes.Blue;
            hexEditor.TypeOfCharacterTable = WpfHexaEditor.Core.CharacterTableType.Ascii;

            elementHost.Location = new Point(250, 50);
            elementHost.Size = new Size(1000, 500);
            elementHost.Name = "elementHost";
            elementHost.Child = hexEditor;
            elementHost.Parent = this;

            System.Windows.Application app = new System.Windows.Application();
            app.MainWindow = new System.Windows.Window();

            this.Controls.Add(elementHost);
            this.Invalidate();

            openToolStripMenuItem.Click += OpenToolStripMenuItem_Click;
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                MemoryStream memoryStream = new MemoryStream();
                openFileDialog1.OpenFile().CopyTo(memoryStream);

                byte[] tmpBuff = memoryStream.ToArray();

                bool headerParsed = false;
                int i = 0;

                const int IdLen = 2; // length of ID

                while(i < tmpBuff.Length)
                {
                    if (!headerParsed)
                    {
                        // parse header
                        if (tmpBuff[i] == 0 && tmpBuff[i + 1] == 0x40)
                        {
                            headerParsed = true;
                            scene2Data.rawDataHeader = tmpBuff.Take(i).ToList();

                            var arr = tmpBuff.Skip(i).Skip(2).Take(4).ToArray();
                            scene2Data.standardObjectsLength = BitConverter.ToInt32(arr, 0);

                            i += 4;
                        }
                        else
                        {
                            i++;
                        }
                    }
                    else
                    {
                        // parse dncs
                        if (tmpBuff[i] == 0x10 && tmpBuff[i + 1] == 0x40)
                        {
                            // standard object
                            Dnc currDnc = new Dnc
                            {
                                objectIDEnum = IDs.Unknown,
                                //objectIDArr = new byte[] { 0x10, 0x40 }
                            };

                            // get length
                            int lenCurr = BitConverter.ToInt32(tmpBuff.Skip(i).Skip(IdLen).Take(4).ToArray(), 0) - IdLen;

                            currDnc.rawData = tmpBuff.Skip(i).Skip(IdLen).Take(lenCurr).ToArray();

                            currDnc.objectIDEnum = (IDs)currDnc.rawData[4];

                            scene2Data.dncs.Add(currDnc);

                            i = i + IdLen + lenCurr;
                        }
                        else
                        {
                            i++;
                        }
                    }
                }

                i = 0;
                foreach (var item in scene2Data.dncs)
                {
                    listBox1.Items.Add($"{i} - {item.objectIDEnum}");
                    i++;
                }
            }
        }

        private void listBox1_Click(object sender, EventArgs e)
        {
            SelectedObjectChanged(sender);
        }

        private void SelectedObjectChanged(object sender)
        {
            //scene2Data.dncs
            hexEditor.Stream = new MemoryStream(scene2Data.dncs[(sender as ListBox).SelectedIndex].rawData);
            listBox1.Focus();
        }

        private void listBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            SelectedObjectChanged(sender);
        }
    }
}
