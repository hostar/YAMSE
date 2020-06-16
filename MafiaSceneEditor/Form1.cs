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
using WpfHexaEditor.Core.MethodExtention;

namespace MafiaSceneEditor
{
    public partial class Form1 : Form
    {
        private const int maxObjectNameLength = 50;
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

            elementHost.Location = new Point(250, maxObjectNameLength);
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

                treeView1.Nodes.Clear();

                byte[] tmpBuff = memoryStream.ToArray();

                bool headerParsed = false;
                bool objectsParsed = false;
                bool definitionsParsed = false;

                int i = 0;

                int objectID = 0;

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

                            scene2Data.standardObjectsStartPosition = i;
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
                        // parse dncs objects
                        if (tmpBuff[i] == 0x10 && tmpBuff[i + 1] == 0x40)
                        {
                            // standard object
                            Dnc currDnc = new Dnc
                            {
                                objectType = ObjectIDs.Unknown,
                            };

                            // get length
                            int lenCurr = BitConverter.ToInt32(tmpBuff.Skip(i).Skip(IdLen).Take(4).ToArray(), 0) - IdLen;

                            currDnc.rawData = tmpBuff.Skip(i).Skip(IdLen).Take(lenCurr).ToArray();
                            currDnc.objectType = GetObjectType(currDnc);
                            currDnc.name = GetNameByID(currDnc);
                            currDnc.ID = objectID;

                            scene2Data.objectsDncs.Add(currDnc);

                            objectID++;
                            i = i + IdLen + lenCurr;
                        }
                        else
                        {
                            i++;

                            if (!objectsParsed)
                            {
                                if (scene2Data.objectsDncs.Count != 0)
                                {
                                    objectsParsed = true;
                                    i--;
                                }
                            }
                            else
                            {
                                //i++;
                            }
                        }

                        // parse dncs definitions
                        //scene2Data.standardObjectsLength = BitConverter.ToInt32(arr, 0);

                        if ((i >= scene2Data.standardObjectsLength) && !definitionsParsed)
                        {
                            if (scene2Data.objectsDefinitionStartPosition > 0)
                            {
                                if (i > (scene2Data.objectsDefinitionStartPosition + scene2Data.objectsDefinitionLength))
                                {
                                    definitionsParsed = true;
                                }
                            }

                            if (tmpBuff[i] == 0x20 && tmpBuff[i + 1] == 0xAE)
                            {
                                var arr = tmpBuff.Skip(i).Skip(2).Take(4).ToArray();

                                scene2Data.objectsDefinitionStartPosition = i;
                                scene2Data.objectsDefinitionLength = BitConverter.ToInt32(arr, 0);

                                i += 6;
                            }

                            // parse dncs object definitions
                            if (tmpBuff[i] == 0x21 && tmpBuff[i + 1] == 0xAE)
                            {
                                // standard object
                                Dnc currDnc = new Dnc
                                {
                                    objectType = ObjectIDs.Unknown,
                                };

                                // get length
                                int lenCurr = BitConverter.ToInt32(tmpBuff.Skip(i).Skip(IdLen).Take(4).ToArray(), 0) - IdLen;

                                currDnc.rawData = tmpBuff.Skip(i).Skip(IdLen).Take(lenCurr).ToArray();
                                currDnc.definitionType = GetObjectDefinitionType(currDnc);
                                currDnc.name = GetNameByDefinitionID(currDnc);
                                currDnc.ID = objectID;

                                scene2Data.objectDefinitionsDncs.Add(currDnc);

                                objectID++;
                                i = i + IdLen + lenCurr;
                                i--;
                            }
                        }
                    }
                }

                // put into treeview
                // objects
                TreeNode objectsTreeNode = new TreeNode("Objects");
                foreach (var item in scene2Data.objectsDncs.GroupBy(x => x.objectType))
                {
                    TreeNode treeNodeParent = new TreeNode(item.Key.ToString());

                    i = 0;

                    List<TreeNode> nodeList = new List<TreeNode>();
                    foreach (var dnc in item)
                    {
                        TreeNode treeNode = new TreeNode();
                        treeNode.Text = dnc.name; //$"{i} - {dnc.objectIDEnum}";
                        treeNode.Tag = new NodeTag 
                        { 
                            id = dnc.ID ,
                            nodeType = NodeType.Object
                        };

                        nodeList.Add(treeNode);
                        i++;
                    }

                    // sort nodes
                    nodeList = nodeList.OrderBy(x => x.Text).ToList();

                    foreach (var node2 in nodeList)
                    {
                        treeNodeParent.Nodes.Add(node2);
                    }

                    treeNodeParent.Text += $" [{nodeList.Count}]";
                    objectsTreeNode.Nodes.Add(treeNodeParent);
                }
                treeView1.Nodes.Add(objectsTreeNode);


                // definitions
                TreeNode defsTreeNode = new TreeNode("Object Definitions");
                foreach (var item in scene2Data.objectDefinitionsDncs.GroupBy(x => x.definitionType))
                {
                    TreeNode treeNodeParent = new TreeNode(item.Key.ToString());

                    i = 0;

                    List<TreeNode> nodeList = new List<TreeNode>();
                    foreach (var dnc in item)
                    {
                        TreeNode treeNode = new TreeNode();
                        treeNode.Text = dnc.name; //$"{i} - {dnc.objectIDEnum}";
                        treeNode.Tag = new NodeTag
                        {
                            id = dnc.ID,
                            nodeType = NodeType.Definition
                        };

                        nodeList.Add(treeNode);
                        i++;
                    }

                    // sort nodes
                    nodeList = nodeList.OrderBy(x => x.Text).ToList();

                    foreach (var node2 in nodeList)
                    {
                        treeNodeParent.Nodes.Add(node2);
                    }

                    treeNodeParent.Text += $" [{nodeList.Count}]";
                    defsTreeNode.Nodes.Add(treeNodeParent);
                }

                treeView1.Nodes.Add(defsTreeNode);
            }
        }

        private DefinitionIDs GetObjectDefinitionType(Dnc dnc)
        {
            if (dnc.rawData.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0x04 }).Any())
            {
                return DefinitionIDs.Car;
            }
            else
            {
                if (dnc.rawData.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0x14 }).Any())
                {
                    return DefinitionIDs.MovableBridge;
                }
                else
                {
                    if (dnc.rawData.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0x5 }).Any())
                    {
                        return DefinitionIDs.Script;
                    }
                    else
                    {
                        if (dnc.rawData.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0x23 }).Any())
                        {
                            return DefinitionIDs.PhysicalObject;
                        }
                        else
                        {
                            if (dnc.rawData.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0x6 }).Any())
                            {
                                return DefinitionIDs.Door;
                            }
                            else
                            {
                                if (dnc.rawData.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0x8 }).Any())
                                {
                                    return DefinitionIDs.Tram;
                                }
                                else
                                {
                                    if (dnc.rawData.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0x19 }).Any())
                                    {
                                        return DefinitionIDs.GasStation;
                                    }
                                    else
                                    {
                                        if (dnc.rawData.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0x12 }).Any())
                                        {
                                            return DefinitionIDs.PedestrianSetup;
                                        }
                                        else
                                        {
                                            if (dnc.rawData.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0x1B }).Any())
                                            {
                                                return DefinitionIDs.Enemy;
                                            }
                                            else
                                            {
                                                if (dnc.rawData.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0x16 }).Any())
                                                {
                                                    return DefinitionIDs.Plane;
                                                }
                                                else
                                                {
                                                    if (dnc.rawData.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0x2 }).Any())
                                                    {
                                                        return DefinitionIDs.Player;
                                                    }
                                                    else
                                                    {
                                                        if (dnc.rawData.FindIndexOf(new byte[] { 0x22, 0xAE, 0x0A, 0x00, 0x00, 0x00, 0xC }).Any())
                                                        {
                                                            return DefinitionIDs.TrafficSetup;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                return DefinitionIDs.Unknown;
            }
        }

        private ObjectIDs GetObjectType(Dnc dnc)
        {
            if (dnc.rawData[4] == 0x10)
            { // either LMAP or sector
                dnc.objectType = ObjectIDs.LMAP;
                dnc.name = GetNameByID(dnc);

                if (dnc.rawData.FindIndexOf(Encoding.ASCII.GetBytes("LMAP")).Any())
                { // is LMAP
                    return ObjectIDs.LMAP;
                }
                else
                {
                    if (dnc.rawData.FindIndexOf(new byte[] { 0x01, 0xB4, 0xF2 }).Any())
                    {
                        return ObjectIDs.Sector;
                    }
                    else
                    {
                        return ObjectIDs.Unknown;
                    }
                }
            }
            else
            {
                if (dnc.rawData.FindIndexOf(new byte[] { 0x11, 0x40, 0x0A, 0x00, 0x00, 0x00, 0x0C }).Any())
                {
                    return ObjectIDs.Occluder;
                }
                else
                {
                    if (dnc.rawData.FindIndexOf(new byte[] { 0x11, 0x40, 0x0A, 0x00, 0x00, 0x00, 0x09 }).Any())
                    {
                        return ObjectIDs.Model;
                    }
                    else
                    {
                        if (dnc.rawData.FindIndexOf(new byte[] { 0x11, 0x40, 0x0A, 0x00, 0x00, 0x00, 0x04 }).Any())
                        {
                            return ObjectIDs.Sound;
                        }
                        else
                        {
                            if (dnc.rawData.FindIndexOf(new byte[] { 0x11, 0x40, 0x0A, 0x00, 0x00, 0x00, 0x03 }).Any())
                            {
                                return ObjectIDs.Camera;
                            }
                            else
                            {
                                if (dnc.rawData.FindIndexOf(new byte[] { 0x11, 0x40, 0x0A, 0x00, 0x00, 0x00, 0x0E }).Any())
                                {
                                    return ObjectIDs.CityMusic;
                                }
                                else 
                                {
                                    if (dnc.rawData.FindIndexOf(new byte[] { 0x11, 0x40, 0x0A, 0x00, 0x00, 0x00, 0x02 }).Any())
                                    {
                                        return ObjectIDs.Light;
                                    }
                                }
                            }
                        }
                        return ObjectIDs.Standard;
                    }
                }
            }
        }

        private string GetNameByID(Dnc dnc)
        {
            switch (dnc.objectType)
            {
                case ObjectIDs.Unknown:
                    return "Unknown";
                case ObjectIDs.LMAP:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0xA).Take(maxObjectNameLength).ToArray());
                case ObjectIDs.Standard:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0x14).Take(maxObjectNameLength).ToArray());
                case ObjectIDs.Sector:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0xA).Take(maxObjectNameLength).ToArray());
                case ObjectIDs.Occluder:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0x14).Take(maxObjectNameLength).ToArray());
                case ObjectIDs.Model:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0x14).Take(maxObjectNameLength).ToArray());
                case ObjectIDs.Sound:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0x14).Take(maxObjectNameLength).ToArray());
                case ObjectIDs.Camera:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0x14).Take(maxObjectNameLength).ToArray());
                case ObjectIDs.CityMusic:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0x14).Take(maxObjectNameLength).ToArray());
                case ObjectIDs.Light:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0x14).Take(maxObjectNameLength).ToArray());
                default:
                    throw new InvalidOperationException(nameof(GetNameByID));
            }
        }

        private string GetNameByDefinitionID(Dnc dnc)
        {
            switch (dnc.definitionType)
            {
                case DefinitionIDs.Unknown:
                    return "Unknown";
                case DefinitionIDs.MovableBridge:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0xA).Take(maxObjectNameLength).ToArray());
                case DefinitionIDs.Car:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0xA).Take(maxObjectNameLength).ToArray());
                case DefinitionIDs.Script:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0xA).Take(maxObjectNameLength).ToArray());
                case DefinitionIDs.PhysicalObject:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0xA).Take(maxObjectNameLength).ToArray());
                case DefinitionIDs.Door:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0xA).Take(maxObjectNameLength).ToArray());
                case DefinitionIDs.Tram:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0xA).Take(maxObjectNameLength).ToArray());
                case DefinitionIDs.GasStation:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0xA).Take(maxObjectNameLength).ToArray());
                case DefinitionIDs.PedestrianSetup:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0xA).Take(maxObjectNameLength).ToArray());
                case DefinitionIDs.Enemy:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0xA).Take(maxObjectNameLength).ToArray());
                case DefinitionIDs.Plane:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0xA).Take(maxObjectNameLength).ToArray());
                case DefinitionIDs.Player:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0xA).Take(maxObjectNameLength).ToArray());
                case DefinitionIDs.TrafficSetup:
                    return GetCStringFromByteArray(dnc.rawData.Skip(0xA).Take(maxObjectNameLength).ToArray());
                default:
                    throw new InvalidOperationException(nameof(GetNameByDefinitionID));
            }
        }

        private string GetCStringFromByteArray(byte[] arr)
        {
            return Encoding.ASCII.GetString(arr, 0, Array.IndexOf(arr, (byte)0));
        }

        private void SelectedObjectChanged(TreeNode e)
        {
            //scene2Data.dncs

            if (e.Tag != null)
            {
                switch (((NodeTag)e.Tag).nodeType)
                {
                    case NodeType.Object:
                        hexEditor.Stream = new MemoryStream(scene2Data.objectsDncs.Where(x => x.ID == ((NodeTag)e.Tag).id).FirstOrDefault().rawData);
                        break;
                    case NodeType.Definition:
                        hexEditor.Stream = new MemoryStream(scene2Data.objectDefinitionsDncs.Where(x => x.ID == ((NodeTag)e.Tag).id).FirstOrDefault().rawData);
                        break;
                    case NodeType.InitScript:
                        break;
                    default:
                        break;
                }
                
                treeView1.Focus();
            }
        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            SelectedObjectChanged(e.Node);
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            SelectedObjectChanged(e.Node);
        }
    }
}
