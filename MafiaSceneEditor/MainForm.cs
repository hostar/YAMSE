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
using YAMSE.DataLayer;
using System.Windows.Interop;
using WpfHexaEditor.Core.MethodExtention;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using YAMSE.Diagram.Classes;
using System.Xml.Serialization;
using DiagramDesigner;
using YAMSE.Diagram;
using System.Windows.Media;
using System.Windows.Controls;
using System.Runtime.InteropServices;

namespace YAMSE
{
    public partial class MainForm : Form
    {
        Scene2Data scene2Data = new Scene2Data();

        readonly MyDesigner myDesigner;
        readonly WpfHexaEditor.HexEditor hexEditor;

        //readonly FastColoredTextBoxNS.FastColoredTextBox fctb;

        readonly System.Windows.Forms.Integration.ElementHost elementHostHexEditor;
        readonly System.Windows.Forms.Integration.ElementHost elementHostDiagramEditor;

        private List<MdiScriptEdit> mdiForms = new List<MdiScriptEdit>();

        private TreeNode currentTreeNode;

        private bool scene2FileLoaded = false;

        public MainForm()
        {
            InitializeComponent();

            // create script editor
            /*
            fctb = new FastColoredTextBoxNS.FastColoredTextBox
            {
                Parent = this,
                Location = new Point(250, 50),
                Size = new Size(1000, 500)
            };

            fctb.Hide();
            */

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

            // this is essential, do not delete
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

            this.IsMdiContainer = true;
            //this.ClientRectangle.Left = 200;

            MdiClient client = this.Controls
                       .OfType<MdiClient>()
                       .FirstOrDefault();

            client.Anchor = mainPanel.Anchor;
            client.Dock = mainPanel.Dock;

            //client.Left = padding;
            client.Size = mainPanel.Size;
            client.Location = mainPanel.Location;

            mainPanel.SendToBack();

            elementHostHexEditor.Hide();
            elementHostDiagramEditor.Hide();

            openToolStripMenuItem.Click += Scene2FileLoad;
            saveToolStripMenuItem.Click += Scene2FileSave;
            saveAsToolStripMenuItem.Click += Scene2FileSaveAs;
            exitToolStripMenuItem.Click += ExitToolStripMenuItem_Click;
            toolStripMenuItem1.Click += ShowScriptDependencyDiagram;
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ShowScriptDependencyDiagram(object sender, EventArgs e)
        {
            if (!scene2FileLoaded)
            {
                MessageBox.Show("First open Scene2 file");
                return;
            }
            elementHostDiagramEditor.Show();
            elementHostDiagramEditor.BringToFront();
            Invalidate();

            //var zoomSlider = myDesigner.ZoomBox.FindName("PART_ZoomSlider");
            var count = VisualTreeHelper.GetChildrenCount(myDesigner.ZoomBox);

            int iterator = 0;
            var whatever = VisualTreeHelper.GetChild(myDesigner.ZoomBox, iterator);

            ((((whatever as Border).Child as Expander).Header as Grid).Children[0] as Slider).Maximum = 100;
            //whatever.Child.Header.Children[0].Maximum

            Root root = new Root();

            List<RootConnection> tmpList;

            List<RootDesignerItem> designerItems = new List<RootDesignerItem>();
            Dictionary<string, string> guidsForNames = new Dictionary<string, string>();

            List<string> excludedNames = new List<string> { "Tommy" };

            Regex scriptExtract = new Regex("^(?!//)[ ]*findactor[ ]+([0-9]+),[ ]*\"([a-zA-Z0-9_-]+)\"");

            int left = 0;
            int top = 0;
            foreach (var script in scene2Data.Sections.First(x => x.SectionType == NodeType.Definition).Dncs.Where(x => x.dncType == DncType.Script))
            {
                // get references from scripts
                // findactor xx, "name"
                string[] strings = Scene2Parser.GetStringFromDnc(script).Split("\r\n");

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
                                ShowShadow = false,
                                StrokeThickness = 2,
                                SourceConnectorName = "Bottom",
                                SourceArrowSymbol = "None",
                                SourceID = script.Name,
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
                    guidsForNames.Add(script.Name, guid);

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

                    left += 150;

                    if (left >= 1000)
                    {
                        top += 120;
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

            // move nodes
            foreach (var grouping in root.Connections.GroupBy(x => x.SourceID))
            {
                if (grouping.Count() > 2)
                {
                    int topIterator = 0;
                    var srcNode = root.DesignerItems.First(x => x.ID == grouping.First().SourceID);
                    srcNode.Left += 600;

                    listBoxOutput.Items.Add($"{guidsForNames.First(x => x.Value == srcNode.ID).Key}: {grouping.Count()}");
                    foreach (var connection in grouping)
                    {
                        listBoxOutput.Items.Add(topIterator);
                        var tgtNode = root.DesignerItems.First(x => x.ID == connection.SinkID);
                        tgtNode.Left = srcNode.Left - 200;
                        tgtNode.Top = srcNode.Top + 150 + topIterator;
                        topIterator += 150;
                    }

                    listBoxOutput.Items.Add($"---------------------");
                }
            }

            // optimize nodes position
            listBoxOutput.Items.Add($"---TOP-----");

            var lastTop = 0;

            var maxTop = root.DesignerItems.Max(x => x.Top);
            while (maxTop > 2000)
            {
                foreach (var item in root.DesignerItems.Where(x => x.Top == maxTop))
                {
                    listBoxOutput.Items.Add($"{guidsForNames.First(x => x.Value == item.ID).Key} {item.Top}");
                    item.Top = (item.Top / 2000)*100;

                    if (lastTop == item.Top)
                    {
                        item.Top += 100;
                    }

                    lastTop = item.Top;
                    listBoxOutput.Items.Add($"-----------");
                }
                lastTop = 0;
                maxTop = root.DesignerItems.Max(x => x.Top);
            }

            // optimize nodes position - prevent overlaps
            foreach (var item in root.DesignerItems)
            {
                foreach (var itemInner in root.DesignerItems)
                {
                    if (itemInner.ID != item.ID)
                    {
                        if (isNodeNear(itemInner.Left, item.Left) && isNodeNear(itemInner.Top, item.Top))
                        {
                            itemInner.Top += 150;
                        }
                    }
                }
            }

            // optimize nodes position - check for long conenctions
            foreach (var item in root.Connections)
            {
                // left
                var leftSrc = root.DesignerItems.First(x => x.ID == item.SourceID).Left;
                var leftSink = root.DesignerItems.First(x => x.ID == item.SinkID).Left;

                if (Math.Abs(leftSrc - leftSink) > 500)
                {
                    root.DesignerItems.First(x => x.ID == item.SourceID).Left = root.DesignerItems.First(x => x.ID == item.SinkID).Left + 150;
                }
            }

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

        private bool isNodeNear(int a, int b)
        {
            return Math.Abs(a - b) < 20;
        }

        private void GetObjectType(Dnc dnc)
        {
            dnc.RawData.FindIndexOf(Encoding.ASCII.GetBytes("LMAP")).Any();
        }
        private static string AddToDesignerItems(List<RootDesignerItem> designerItems, int left, int top, Dnc script)
        {
            var guid = Guid.NewGuid().ToString();
            designerItems.Add(new RootDesignerItem
            {
                Content = Resources.Test1Content.Replace("Box_placeholder", script.Name),
                Left = left,
                Top = top,
                Width = 100 + (script.Name.Length - 5) * 7,
                Height = 70,
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
                scene2FileLoaded = true;
                listBoxOutput.Items.Add("Loading file...");

                MemoryStream memoryStream = new MemoryStream();
                var tmpStream = openFileDialog1.OpenFile();
                tmpStream.CopyTo(memoryStream);
                tmpStream.Close();

                scene2Data = new Scene2Data();

                Scene2Parser.LoadScene(memoryStream, ref scene2Data, listBoxOutput.Items);

                // put into treeview
                // objects
                treeView1.Nodes.Clear();

                int i = 0;
                TreeNode objectsTreeNode = new TreeNode("Objects");
                foreach (var item in scene2Data.Sections.First(x => x.SectionType == NodeType.Object).Dncs.GroupBy(x => x.dncType))
                {
                    TreeNode treeNodeParent = new TreeNode(item.Key.ToString());

                    i = 0;

                    List<TreeNode> nodeList = new List<TreeNode>();
                    foreach (Dnc dnc in item)
                    {
                        TreeNode treeNode = new TreeNode
                        {
                            Text = dnc.Name,
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
                foreach (var item in scene2Data.Sections.First(x => x.SectionType == NodeType.Definition).Dncs.GroupBy(x => x.dncType))
                {
                    TreeNode treeNodeParent = new TreeNode(item.Key.ToString());

                    i = 0;

                    List<TreeNode> nodeList = new List<TreeNode>();
                    foreach (var dnc in item)
                    {
                        TreeNode treeNode = new TreeNode
                        {
                            Text = dnc.Name,
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
                foreach (var item in scene2Data.Sections.First(x => x.SectionType == NodeType.InitScript).Dncs.GroupBy(x => x.dncType))
                {
                    TreeNode treeNodeParent = new TreeNode(item.Key.ToString());

                    i = 0;

                    List<TreeNode> nodeList = new List<TreeNode>();
                    foreach (var dnc in item)
                    {
                        TreeNode treeNode = new TreeNode
                        {
                            Text = dnc.Name,
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
        private void Scene2FileSaveAs(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Scene2Parser.SaveScene(saveFileDialog1.OpenFile(), ref scene2Data, listBoxOutput.Items);
            }
        }

        private void Scene2FileSave(object sender, EventArgs e)
        {
            var tmpStream = new FileStream(openFileDialog1.FileName, FileMode.Create);
            Scene2Parser.SaveScene(tmpStream, ref scene2Data, listBoxOutput.Items);
            tmpStream.Close();
        }

        private void SelectedObjectChanged(TreeNode e)
        {
            Dnc dnc;

            if (currentTreeNode?.GetHashCode() == e.GetHashCode())
            {
                return;
            }
            currentTreeNode = e;

            if (e.Tag != null)
            {
                switch (((NodeTag)e.Tag).nodeType)
                {
                    case NodeType.Object:
                        //elementHostHexEditor.Show();
                        elementHostDiagramEditor.Hide();
                        //hexEditor.Stream = new MemoryStream(scene2Data.objectsDncs.Where(x => x.ID == ((NodeTag)e.Tag).id).FirstOrDefault().rawData);
                        dnc = scene2Data.Sections.First(x => x.SectionType == NodeType.Object).Dncs.Where(x => x.ID == ((NodeTag)e.Tag).id).FirstOrDefault();

                        if (mdiForms.Any(x => (string)x.Tag == CreateInnerFormTag(dnc)))
                        {
                            return;
                        }

                        CreateMdiForm(dnc);
                        break;
                    case NodeType.Definition:

                        dnc = scene2Data.Sections.First(x => x.SectionType == NodeType.Definition).Dncs.Where(x => x.ID == ((NodeTag)e.Tag).id).FirstOrDefault();

                        switch (dnc.dncType)
                        {
                            case DncType.Script:
                                elementHostHexEditor.Hide();
                                elementHostDiagramEditor.Hide();

                                if (mdiForms.Any(x => (string)x.Tag == CreateInnerFormTag(dnc)))
                                {
                                    return;
                                }

                                CreateMdiForm(dnc, Scene2Parser.GetStringFromDnc(dnc));
                                break;

                            case DncType.PhysicalObject:
                            case DncType.Door:
                            case DncType.Tram:
                            case DncType.GasStation:
                            case DncType.PedestrianSetup:
                            case DncType.Enemy:
                            case DncType.Plane:
                            case DncType.Player:
                            case DncType.TrafficSetup:
                            case DncType.Unknown:
                            case DncType.MovableBridge:
                            case DncType.Car:
                            default:
                                if (mdiForms.Any(x => (string)x.Tag == CreateInnerFormTag(dnc)))
                                {
                                    return;
                                }

                                CreateMdiForm(dnc);
                                break;
                        }
                        if (dnc.dncType == DncType.Script)
                        {
                            elementHostHexEditor.Hide();
                            elementHostDiagramEditor.Hide();

                            if (mdiForms.Any(x => (string)x.Tag == CreateInnerFormTag(dnc)))
                            {
                                return;
                            }

                            CreateMdiForm(dnc, Scene2Parser.GetStringFromDnc(dnc));
                        }
                        else
                        {
                            //hexEditor.Stream = new MemoryStream(dnc.rawData);
                            //elementHostHexEditor.Show();

                            //elementHostHexEditor.Hide();
                            //elementHostDiagramEditor.Hide();

                            if (mdiForms.Any(x => (string)x.Tag == CreateInnerFormTag(dnc)))
                            {
                                return;
                            }

                            CreateMdiForm(dnc);
                        }
                        
                        break;
                    case NodeType.InitScript:
                        dnc = scene2Data.Sections.First(x => x.SectionType == NodeType.InitScript).Dncs.Where(x => x.ID == ((NodeTag)e.Tag).id).FirstOrDefault();

                        //fctb.Text = GetStringFromInitScript(dnc);
                        elementHostHexEditor.Hide();
                        elementHostDiagramEditor.Hide();

                        if (mdiForms.Any(x => (string)x.Tag == CreateInnerFormTag(dnc)))
                        {
                            return;
                        }

                        CreateMdiForm(dnc, GetStringFromInitScript(dnc));
                        break;
                    default:
                        break;
                }
                
                treeView1.Focus();
            }
        }

        private void CreateMdiForm(Dnc dnc, string text)
        {
            var tmpForm = CreateMdiFormInner(dnc);

            tmpForm.SetEditorText(text);
            tmpForm.FormClosed += TmpForm_FormClosed;
            mdiForms.Add(tmpForm);

            tmpForm.BringToFront();
        }

        private void CreateMdiForm(Dnc dnc)
        {
            MdiScriptEdit tmpForm = CreateMdiFormInner(dnc);

            tmpForm.SetHexEditorContent(dnc);
            tmpForm.FormClosed += TmpForm_FormClosed;
            mdiForms.Add(tmpForm);

            tmpForm.BringToFront();
        }

        private MdiScriptEdit CreateMdiFormInner(Dnc dnc)
        {
            return new MdiScriptEdit { MdiParent = this, Width = 800, Height = 400, Visible = true, Text = dnc.Name, Tag = CreateInnerFormTag(dnc), Dnc = dnc, Scene2Data = scene2Data };
        }

        private void TmpForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            var foundForm = mdiForms.FirstOrDefault(x => x.Tag == (sender as MdiScriptEdit).Tag);
            foundForm?.Dispose();
            mdiForms.Remove(foundForm);
        }

        /// <summary>
        /// Creates a tag for MDI form for future identification
        /// </summary>
        /// <param name="dnc"></param>
        /// <returns></returns>
        private static string CreateInnerFormTag(Dnc dnc)
        {
            return $"{dnc.dncType} ; {dnc.Name}";
        }

        private static string GetStringFromInitScript(Dnc dnc)
        {
            return Encoding.UTF8.GetString(dnc.RawData.Skip(dnc.Name.Length + 13).ToArray());
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
