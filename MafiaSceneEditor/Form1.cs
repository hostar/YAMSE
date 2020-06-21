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
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using MafiaSceneEditor.Diagram.Classes;
using System.Xml.Serialization;
using DiagramDesigner;
using MafiaSceneEditor.Diagram;

namespace MafiaSceneEditor
{
    public partial class MainForm : Form
    {
        private const int maxObjectNameLength = 50;
        readonly Scene2Data scene2Data = new Scene2Data();

        readonly MyDesigner myDesigner;
        readonly WpfHexaEditor.HexEditor hexEditor;

        readonly FastColoredTextBoxNS.FastColoredTextBox fctb;

        readonly System.Windows.Forms.Integration.ElementHost elementHostHexEditor;
        readonly System.Windows.Forms.Integration.ElementHost elementHostDiagramEditor;

        public MainForm()
        {
            InitializeComponent();

            // create script editor
            fctb = new FastColoredTextBoxNS.FastColoredTextBox
            {
                Parent = this,
                Location = new Point(250, 50),
                Size = new Size(1000, 500)
            };

            fctb.Hide();

            // create hex editor
            hexEditor = new WpfHexaEditor.HexEditor
            {
                ForegroundSecondColor = System.Windows.Media.Brushes.Blue,
                TypeOfCharacterTable = WpfHexaEditor.Core.CharacterTableType.Ascii
            };

            elementHostHexEditor = new System.Windows.Forms.Integration.ElementHost
            {
                Location = new Point(250, 50),
                Size = new Size(1000, 500)
            };
            elementHostHexEditor.Name = nameof(elementHostHexEditor);
            elementHostHexEditor.Child = hexEditor;
            elementHostHexEditor.Parent = this;

            System.Windows.Application app = new System.Windows.Application
            {
                MainWindow = new System.Windows.Window()
            };

            Controls.Add(elementHostHexEditor);

            // create diagram component

            myDesigner = new MyDesigner();
            elementHostDiagramEditor = new System.Windows.Forms.Integration.ElementHost
            {
                Location = new Point(250, 50),
                Size = new Size(1100, 500),
                Child = myDesigner,
                Anchor = AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top | AnchorStyles.Right
            };
            elementHostDiagramEditor.Name = nameof(elementHostDiagramEditor);
            elementHostDiagramEditor.Parent = this;

            Controls.Add(elementHostDiagramEditor);
            Invalidate();

            openToolStripMenuItem.Click += Scene2FileLoad;
            toolStripMenuItem1.Click += ShowScriptDependencyDiagram;
        }

        private void ShowScriptDependencyDiagram(object sender, EventArgs e)
        {
            elementHostDiagramEditor.Show();
            elementHostDiagramEditor.BringToFront();
            this.Invalidate();

            Root root = new Root();

            List<RootConnection> tmpList;

            List<RootDesignerItem> designerItems = new List<RootDesignerItem>();
            Dictionary<string, string> guidsForNames = new Dictionary<string, string>();

            List<string> excludedNames = new List<string> { "Tommy" };

            Regex scriptExtract = new Regex("^(?!//)[ ]*findactor[ ]+([0-9]+),[ ]*\"([a-zA-Z0-9_-]+)\"");

            int left = 0;
            int top = 0;
            foreach (var script in scene2Data.objectDefinitionsDncs.Where(x => x.definitionType == DefinitionIDs.Script))
            {
                // get references from scripts
                // findactor xx, "name"
                string[] strings = GetStringFromScript(script).Split("\r\n");

                //listBoxOutput.Items.Add(script.name);

                bool hasAtLeastOneConnection = false;
                List<RootConnection> connectionsForOneItem = new List<RootConnection>();
                foreach (var str in strings)
                {
                    foreach (Match scriptMatchResult in scriptExtract.Matches(str))
                    {
                        if (scriptMatchResult?.Groups.Count == 3)
                        {
                            var sinkName = scriptMatchResult?.Groups[2].ToString();
                            //listBoxOutput.Items.Add($"  {sinkName}");

                            connectionsForOneItem.Add(new RootConnection 
                            {
                                PathFinder = "OrthogonalPathFinder",
                                Color = "#FF808080",
                                SinkConnectorName = "Top",
                                SinkArrowSymbol = "Arrow",
                                zIndex = 2,
                                StrokeThickness = 2,
                                SourceConnectorName = "Bottom",
                                SourceArrowSymbol = "None",
                                SourceID = script.name,
                                SinkID = sinkName,
                            });

                            if (!excludedNames.Contains(scriptMatchResult?.Groups[2].ToString()))
                            {
                                hasAtLeastOneConnection = true;
                            }
                        }
                    }
                }

                if (hasAtLeastOneConnection)
                {
                    var guid = AddToDesignerItems(designerItems, left, top, script);
                    guidsForNames.Add(script.name, guid);

                    if (root.Connections == null)
                    {
                        root.Connections = connectionsForOneItem.ToArray();
                    }
                    else
                    {
                        tmpList = root.Connections.ToList();
                        tmpList.AddRange(connectionsForOneItem);
                        root.Connections = tmpList.ToArray();
                    }

                    left += 100;

                    if (left == 500)
                    {
                        top += 100;
                        left = 0;
                    }
                }
            }

            root.DesignerItems = designerItems.ToArray();

            // filter out invalid connections
            List<string> connectionForDeletion = new List<string>();
            foreach (RootConnection item in root.Connections)
            {
                if (!guidsForNames.ContainsKey(item.SinkID))
                {
                    connectionForDeletion.Add(item.SinkID);
                    continue;
                }
                item.SourceID = guidsForNames[item.SourceID];
                item.SinkID = guidsForNames[item.SinkID];
            }

            tmpList = root.Connections.ToList();
            foreach (string item in connectionForDeletion)
            {
                foreach (var toDelete in root.Connections.Where(x => x.SinkID == item))
                {
                    tmpList.Remove(toDelete);
                }
            }
            root.Connections = tmpList.ToArray();

            MemoryStream memoryStream = new MemoryStream();
            XmlWriter xmlWriter = XmlWriter.Create(memoryStream, new XmlWriterSettings { Encoding = Encoding.UTF8 });

            XmlSerializer serializer = new XmlSerializer(typeof(Root));
            serializer.Serialize(xmlWriter, root);

            memoryStream.Position = 0;
            StreamReader reader = new StreamReader(memoryStream);

            //string diagram = reader.ReadToEnd();
            //File.WriteAllText(@"d:\xml\cc", diagram);

            myDesigner.MyDesignerCanvas.RestoreDiagram(XElement.Parse(reader.ReadToEnd()));
            
        }

        private static string AddToDesignerItems(List<RootDesignerItem> designerItems, int left, int top, Dnc script)
        {
            var guid = Guid.NewGuid().ToString();
            designerItems.Add(new RootDesignerItem
            {
                Content = Resources.Test1Content.Replace("Box_placeholder", script.name),
                Left = left,
                Top = top,
                Width = 100,
                Height = 100,
                ParentID = "00000000-0000-0000-0000-000000000000",
                ID = guid,
                IsGroup = false,
            });

            return guid;
        }

        private void Scene2FileLoad(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                listBoxOutput.Items.Add("Loading file...");
                MemoryStream memoryStream = new MemoryStream();
                openFileDialog1.OpenFile().CopyTo(memoryStream);

                treeView1.Nodes.Clear();

                byte[] tmpBuff = memoryStream.ToArray();

                bool headerParsed = false;
                bool objectsParsed = false;
                bool definitionsParsed = false;

                int i = 0;
                int objectID = 0;

                // loading
                bool loadingHeaderShown = false;
                bool loadingObjectsShown = false;
                bool loadingObjectsDefinitionsShown = false;
                bool loadingInitScriptsShown = false;

                const int IdLen = 2; // length of ID

                while (i < tmpBuff.Length)
                {
                    if (!headerParsed)
                    {
                        // parse header
                        if (!loadingHeaderShown)
                        {
                            listBoxOutput.Items.Add("Loading header...");
                            loadingHeaderShown = true;
                        }
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
                        if (!loadingObjectsShown)
                        {
                            listBoxOutput.Items.Add("Loading objects...");
                            loadingObjectsShown = true;
                        }
                        if (tmpBuff[i] == 0x10 && tmpBuff[i + 1] == 0x40)
                        {                            
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
                                    objectID = 0;
                                    i--;
                                }
                            }
                            else
                            {
                                //i++;
                            }
                        }

                        // parse dncs definitions
                        if ((i >= scene2Data.standardObjectsLength) && !definitionsParsed)
                        {
                            if (scene2Data.objectsDefinitionStartPosition > 0)
                            {
                                if (i > (scene2Data.objectsDefinitionStartPosition + scene2Data.objectsDefinitionLength))
                                {
                                    definitionsParsed = true;
                                    objectID = 0;
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
                            if (!loadingObjectsDefinitionsShown)
                            {
                                listBoxOutput.Items.Add("Loading object definitions...");
                                loadingObjectsDefinitionsShown = true;
                            }
                            if (tmpBuff[i] == 0x21 && tmpBuff[i + 1] == 0xAE)
                            {
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

                        if (definitionsParsed)
                        {
                            if (tmpBuff.Length <= i)
                            {
                                break;
                            }

                            // init scripts
                            if (!loadingInitScriptsShown)
                            {
                                listBoxOutput.Items.Add("Loading init scripts...");
                                loadingInitScriptsShown = true;
                            }
                            if (tmpBuff[i] == 0x51 && tmpBuff[i + 1] == 0xAE)
                            {
                                Dnc currDnc = new Dnc
                                {
                                    objectType = ObjectIDs.Unknown,
                                };

                                // get length
                                int lenCurr = BitConverter.ToInt32(tmpBuff.Skip(i).Skip(IdLen).Take(4).ToArray(), 0) - IdLen;

                                currDnc.rawData = tmpBuff.Skip(i).Skip(IdLen).Take(lenCurr).ToArray();
                                currDnc.definitionType = DefinitionIDs.InitScript;
                                currDnc.name = GetNameByDefinitionID(currDnc);
                                currDnc.ID = objectID;

                                scene2Data.initScriptsDncs.Add(currDnc);

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
                        TreeNode treeNode = new TreeNode
                        {
                            Text = dnc.name,
                            Tag = new NodeTag
                            {
                                id = dnc.ID,
                                nodeType = NodeType.Object
                            }
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
                        TreeNode treeNode = new TreeNode
                        {
                            Text = dnc.name,
                            Tag = new NodeTag
                            {
                                id = dnc.ID,
                                nodeType = NodeType.Definition
                            }
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

                // init scripts
                TreeNode initScriptTreeNode = new TreeNode("Init script");
                foreach (var item in scene2Data.initScriptsDncs.GroupBy(x => x.definitionType))
                {
                    TreeNode treeNodeParent = new TreeNode(item.Key.ToString());

                    i = 0;

                    List<TreeNode> nodeList = new List<TreeNode>();
                    foreach (var dnc in item)
                    {
                        TreeNode treeNode = new TreeNode
                        {
                            Text = dnc.name,
                            Tag = new NodeTag
                            {
                                id = dnc.ID,
                                nodeType = NodeType.InitScript
                            }
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
                    initScriptTreeNode.Nodes.Add(treeNodeParent);
                }

                treeView1.Nodes.Add(initScriptTreeNode);

                listBoxOutput.Items.Add("Loading of file done.");
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
                case DefinitionIDs.InitScript:

                    var len = dnc.rawData[5];

                    return Encoding.ASCII.GetString(dnc.rawData, 0x9, len);

                    //return GetCStringFromByteArray(dnc.rawData.Skip(0x9).Take(maxObjectNameLength).ToArray());


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
            Dnc dnc;

            if (e.Tag != null)
            {
                switch (((NodeTag)e.Tag).nodeType)
                {
                    case NodeType.Object:
                        fctb.Hide();
                        elementHostHexEditor.Show();
                        elementHostDiagramEditor.Hide();
                        hexEditor.Stream = new MemoryStream(scene2Data.objectsDncs.Where(x => x.ID == ((NodeTag)e.Tag).id).FirstOrDefault().rawData);
                        break;
                    case NodeType.Definition:

                        dnc = scene2Data.objectDefinitionsDncs.Where(x => x.ID == ((NodeTag)e.Tag).id).FirstOrDefault();

                        if (dnc.definitionType == DefinitionIDs.Script)
                        {
                            fctb.Text = GetStringFromScript(dnc);
                            elementHostHexEditor.Hide();
                            elementHostDiagramEditor.Hide();
                            fctb.Show();
                        }
                        else
                        {
                            hexEditor.Stream = new MemoryStream(dnc.rawData);
                            fctb.Hide();
                        }
                        
                        break;
                    case NodeType.InitScript:
                        dnc = scene2Data.initScriptsDncs.Where(x => x.ID == ((NodeTag)e.Tag).id).FirstOrDefault();

                        fctb.Text = GetStringFromScript(dnc);
                        elementHostHexEditor.Hide();
                        elementHostDiagramEditor.Hide();
                        fctb.Show();
                        break;
                    default:
                        break;
                }
                
                treeView1.Focus();
            }
        }

        private static string GetStringFromScript(Dnc dnc)
        {
            return Encoding.UTF8.GetString(dnc.rawData.Skip(dnc.name.Length + 41).ToArray());
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
