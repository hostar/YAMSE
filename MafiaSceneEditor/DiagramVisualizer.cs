using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Windows.Media;
using YAMSE.Diagram;


using System.Linq;
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
using System.Windows.Controls;
using System.Runtime.InteropServices;
using ComponentFactory.Krypton.Toolkit;
using System.Windows.Input;

namespace YAMSE
{
    public partial class DiagramVisualizer : KryptonForm
    {
        readonly System.Windows.Forms.Integration.ElementHost elementHostDiagramEditor;
        readonly MyDesigner myDesigner = new MyDesigner();
        readonly MainForm2 mainForm;

        readonly KryptonContextMenu contextMenuNode = new KryptonContextMenu();

        DesignerItem currentNode = null;
        RootConnection[] rootConnections;

        public DiagramVisualizer(MainForm2 _mainForm)
        {
            InitializeComponent();

            elementHostDiagramEditor = new System.Windows.Forms.Integration.ElementHost
            {
                Location = new Point(250, 50),
                Size = new Size(1100, 500),
                Child = myDesigner,
                Anchor = AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top | AnchorStyles.Right
            };
            elementHostDiagramEditor.Name = nameof(elementHostDiagramEditor);
            elementHostDiagramEditor.Parent = this;

            elementHostDiagramEditor.Dock = DockStyle.Fill;
            Controls.Add(elementHostDiagramEditor);

            elementHostDiagramEditor.Show();
            elementHostDiagramEditor.BringToFront();

            mainForm = _mainForm;

            KryptonContextMenuItem kryptonContextMenuItemShowOnlyDirectlyConnected = new KryptonContextMenuItem();
            kryptonContextMenuItemShowOnlyDirectlyConnected.Text = "Show only directly connected";
            kryptonContextMenuItemShowOnlyDirectlyConnected.Click += KryptonContextMenuItemShowOnlyDirectlyConnected_Click;

            KryptonContextMenuItem kryptonContextMenuItemHideAllShowOnlyDirectlyConnected = new KryptonContextMenuItem();
            kryptonContextMenuItemHideAllShowOnlyDirectlyConnected.Text = "Hide all and show only directly connected";
            kryptonContextMenuItemHideAllShowOnlyDirectlyConnected.Click += KryptonContextMenuItemHideAllShowOnlyDirectlyConnected_Click;

            contextMenuNode.Items.Add(new KryptonContextMenuItems(new KryptonContextMenuItemBase[] { kryptonContextMenuItemShowOnlyDirectlyConnected, kryptonContextMenuItemHideAllShowOnlyDirectlyConnected }));
        }

        private void KryptonContextMenuItemHideAllShowOnlyDirectlyConnected_Click(object sender, EventArgs e)
        {
            Guid id = currentNode.ID;
            HideAllNodes(id);

            KryptonContextMenuItemShowOnlyDirectlyConnected_Click(sender, e);
        }

        private void KryptonContextMenuItemShowOnlyDirectlyConnected_Click(object sender, EventArgs e)
        {
            Guid id = currentNode.ID;
            string name = GetNameOfNode(currentNode);

            // get ID of current node
            foreach (var item in myDesigner.MyDesignerCanvas.Children)
            {
                if (item is DesignerItem designerItem)
                {
                    string tmpName = GetNameOfNode(designerItem);

                    bool found = false;
                    foreach (RootConnection conn in rootConnections)
                    {
                        if (conn.SinkID == id.ToString())
                        {
                            if (conn.SourceID == designerItem.ID.ToString())
                            {
                                found = true;
                                break;
                            }
                        }

                        if (conn.SourceID == id.ToString())
                        {
                            if (conn.SinkID == designerItem.ID.ToString())
                            {
                                found = true;
                                break;
                            }
                        }
                    }
                    if (found)
                    {
                        designerItem.Visibility = System.Windows.Visibility.Visible;
                    }
                }
            }
        }

        private void HideAllNodes(Guid id)
        {
            // hide everything
            foreach (var item in myDesigner.MyDesignerCanvas.Children)
            {
                if (item is DesignerItem designerItem)
                {
                    if (designerItem.ID != id)
                    {
                        designerItem.Visibility = System.Windows.Visibility.Hidden;
                    }
                }

                if (item is Connection conn)
                { // TODO: correctly hide all edges
                    if (conn.Sink.Connections.Any(x => x.ID == id))
                    {
                        conn.Visibility = System.Windows.Visibility.Hidden;
                    }
                    if (conn.Source.Connections.Any(x => x.ID == id))
                    {
                        conn.Visibility = System.Windows.Visibility.Hidden;
                    }
                }
            }
        }

        public void ShowScriptDependencyDiagram(Scene2Data scene2Data)
        {
            //var zoomSlider = myDesigner.ZoomBox.FindName("PART_ZoomSlider");
            var count = VisualTreeHelper.GetChildrenCount(myDesigner.ZoomBox);

            int iterator = 0;
            var dependencyObject = VisualTreeHelper.GetChild(myDesigner.ZoomBox, iterator);

            ((((dependencyObject as Border).Child as Expander).Header as Grid).Children[0] as Slider).Maximum = 100;
            //whatever.Child.Header.Children[0].Maximum

            Root root = new Root();

            List<RootConnection> tmpConnectionList;

            List<RootDesignerItem> designerItems = new List<RootDesignerItem>();
            Dictionary<string, string> guidsForNames = new Dictionary<string, string>();

            List<string> excludedNames = new List<string> { "Tommy" };

            Regex scriptExtract = new Regex("^(?!//)[ ]*findactor[ ]+([0-9]+),[ ]*\"([a-zA-Z 0-9_-~]+)\"");

            int left = 0;
            int top = 0;
            foreach (Dnc dnc in scene2Data.Sections.First(x => x.SectionType == NodeType.Definition).Dncs.Where(x => (x.dncType == DncType.Script) || (x.dncType == DncType.Enemy)))
            {
                // get references from scripts
                // findactor xx, "name"
                string[] strings = Scene2Parser.GetScriptFromDnc(dnc).Split("\r\n");

                //listBoxOutput.Items.Add(script.name);

                List<RootConnection> connectionsForOneItem = new List<RootConnection>();
                foreach (var str in strings)
                {
                    foreach (Match scriptMatchResult in scriptExtract.Matches(str))
                    {
                        if (scriptMatchResult?.Groups.Count == 3)
                        {
                            var sinkName = scriptMatchResult?.Groups[2].ToString();
                            if (dnc.Name != sinkName)
                            {
                                //listBoxOutput.Items.Add($"  {sinkName}");

                                connectionsForOneItem.Add(new RootConnection
                                {
                                    PathFinder = "OrthogonalPathFinder",
                                    Color = "#FF808080",
                                    SinkConnectorName = "Top",
                                    SinkArrowSymbol = "Arrow",
                                    zIndex = 0,
                                    ShowShadow = false,
                                    StrokeThickness = 2,
                                    SourceConnectorName = "Bottom",
                                    SourceArrowSymbol = "None",
                                    SourceID = dnc.Name,
                                    SinkID = sinkName,
                                });
                            }

                            /*
                            if (!excludedNames.Contains(scriptMatchResult?.Groups[2].ToString()))
                            {
                                hasAtLeastOneConnection = true;
                            }
                            */
                        }
                    }
                }                

                if (connectionsForOneItem.Count > 0)
                {
                    var guid = AddToDesignerItems(designerItems, left, top, dnc, connectionsForOneItem);
                    guidsForNames.Add(dnc.Name, guid);
                }

                if (root.Connections == null)
                {
                    root.Connections = connectionsForOneItem.ToArray();
                }
                else
                {
                    tmpConnectionList = root.Connections.ToList();
                    tmpConnectionList.AddRange(connectionsForOneItem);
                    root.Connections = tmpConnectionList.ToArray();
                }

                left += 150;

                if (left >= 1000)
                {
                    top += 120;
                    left = 0;
                }
            }

            // filter out invalid connections
            List<string> connectionForDeletion = new List<string>();

            if (root.Connections != null)
            {
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

                tmpConnectionList = root.Connections.ToList();
                foreach (string item in connectionForDeletion)
                {
                    foreach (var toDelete in root.Connections.Where(x => x.SinkID == item))
                    {
                        tmpConnectionList.Remove(toDelete);
                    }
                }
                root.Connections = tmpConnectionList.ToArray();

                // delete nodes without connection
                for (int i = 0; i < designerItems.Count; i++)
                {
                    var designerItem = designerItems[i];

                    if (!root.Connections.Any(x => x.SourceID == designerItem.ID) && !root.Connections.Any(x => x.SinkID == designerItem.ID))
                    {
                        designerItems.RemoveAt(i);
                    }
                }

                root.DesignerItems = designerItems.ToArray();

                // move nodes
                /*
                foreach (var grouping in root.Connections.GroupBy(x => x.SourceID))
                {
                    if (grouping.Count() > 2)
                    {
                        int topIterator = 0;
                        var srcNode = root.DesignerItems.First(x => x.ID == grouping.First().SourceID);
                        srcNode.Left += 600;

                        // listBoxOutput.Items.Add($"{guidsForNames.First(x => x.Value == srcNode.ID).Key}: {grouping.Count()}");
                        foreach (var connection in grouping)
                        {
                            // listBoxOutput.Items.Add(topIterator);
                            var tgtNode = root.DesignerItems.First(x => x.ID == connection.SinkID);
                            tgtNode.Left = srcNode.Left - 200;
                            tgtNode.Top = srcNode.Top + 150 + topIterator;
                            topIterator += 150;
                        }

                        // listBoxOutput.Items.Add($"---------------------");
                    }
                }*/
            }
            
            // optimize nodes position
            // listBoxOutput.Items.Add($"---TOP-----");
            /*
            var lastTop = 0;

            if (root.DesignerItems.Length == 0)
            {
                return;
            }

            var maxTop = root.DesignerItems.Max(x => x.Top);
            while (maxTop > 2000)
            {
                foreach (var item in root.DesignerItems.Where(x => x.Top == maxTop))
                {
                    // listBoxOutput.Items.Add($"{guidsForNames.First(x => x.Value == item.ID).Key} {item.Top}");
                    item.Top = (item.Top / 2000) * 100;

                    if (lastTop == item.Top)
                    {
                        item.Top += 100;
                    }

                    lastTop = item.Top;
                    // listBoxOutput.Items.Add($"-----------");
                }
                lastTop = 0;
                maxTop = root.DesignerItems.Max(x => x.Top);
            }*/

            // optimize nodes position - prevent overlaps
            
            foreach (var item in root.DesignerItems)
            {
                foreach (var itemInner in root.DesignerItems)
                {
                    if (itemInner.ID != item.ID)
                    {
                        if (IsNodeNear(itemInner.Left, item.Left) && IsNodeNear(itemInner.Top, item.Top))
                        {
                            itemInner.Top += 150;
                        }
                        /*
                        if (IsNodeNear(itemInner.Left, item.Left + (int)item.Width) || IsNodeNear(itemInner.Left + (int)itemInner.Width, item.Left))
                        {
                            itemInner.Top += 150;
                        }*/
                    }
                }
            }

            // optimize nodes position - check for long connections
            /*
            foreach (var item in root.Connections)
            {
                // left
                var leftSrc = root.DesignerItems.First(x => x.ID == item.SourceID).Left;
                var leftSink = root.DesignerItems.First(x => x.ID == item.SinkID).Left;

                if (Math.Abs(leftSrc - leftSink) > 500)
                {
                    root.DesignerItems.First(x => x.ID == item.SourceID).Left = root.DesignerItems.First(x => x.ID == item.SinkID).Left + 150;
                }
            }*/

            MemoryStream memoryStream = new MemoryStream();
            XmlWriter xmlWriter = XmlWriter.Create(memoryStream, new XmlWriterSettings { Encoding = Encoding.UTF8 });

            XmlSerializer serializer = new XmlSerializer(typeof(Root));
            serializer.Serialize(xmlWriter, root);

            memoryStream.Position = 0;
            StreamReader reader = new StreamReader(memoryStream);

            //string diagram = reader.ReadToEnd();
            //File.WriteAllText(@"d:\xml\cc", diagram);

            myDesigner.MyDesignerCanvas.RestoreDiagram(XElement.Parse(reader.ReadToEnd()));
            //myDesigner.MyDesignerCanvas.MouseDown += MyDesignerCanvas_MouseDown;
            myDesigner.MyDesignerCanvas.PreviewMouseDown += MyDesignerCanvas_MouseDown;
            //myDesigner.MyDesignerCanvas.ContextMenuOpening += MyDesignerCanvas_ContextMenuOpening;

            // save connections
            rootConnections = root.Connections.ToArray();
        }

        private void MyDesignerCanvas_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (e.ClickCount == 2)
                { // handle node double click
                    string nodeName = GetNameOfNode(e);

                    TreeNode nodeToClick = null;
                    if (nodeName != string.Empty)
                    {
                        foreach (var node in mainForm.treeViewMain.Nodes)
                        {
                            var treeNode = node as TreeNode;
                            if (treeNode.Text == "Object definitions")
                            {
                                foreach (var item in treeNode.Nodes)
                                {
                                    var innerTreeNode = item as TreeNode;
                                    if (innerTreeNode.Text.Contains("Script") || innerTreeNode.Text.Contains("Enemy"))
                                    {
                                        foreach (var item2 in innerTreeNode.Nodes)
                                        {
                                            var innerTreeNode2 = item2 as TreeNode;
                                            if (innerTreeNode2.Text == nodeName)
                                            {
                                                nodeToClick = innerTreeNode2;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (nodeToClick != null)
                        {
                            mainForm.SelectedObjectChanged(nodeToClick);
                        }
                    }
                }
            }

            if (e.RightButton == MouseButtonState.Pressed)
            {
                e.Handled = true;
                currentNode = (DesignerItem)e.Source;
                contextMenuNode.Show(e.Source);
            }
        }

        private static string GetNameOfNode(MouseButtonEventArgs e)
        {
            return GetNameOfNode(e.Source as DesignerItem);
        }

        private static string GetNameOfNode(DesignerItem designerItem)
        {
            var grid = (designerItem.Content as Test1).Content as Grid;
            var nodeName = string.Empty;

            foreach (var item in grid.Children)
            {
                if (item is TextBlock textBlock)
                {
                    nodeName = textBlock.Text;
                    break;
                }
            }

            return nodeName;
        }

        private bool IsNodeNear(int a, int b)
        {
            return Math.Abs(a - b) < 20;
        }

        private static string AddToDesignerItems(List<RootDesignerItem> designerItems, int left, int top, Dnc dnc, List<RootConnection> connectionsForOneItem)
        {
            var guid = Guid.NewGuid().ToString();
            designerItems.Add(new RootDesignerItem
            {
                Content = Resources.Test1Content.Replace("Box_placeholder", dnc.Name).Replace("Num_connection", "Connections: " + connectionsForOneItem.Count.ToString()),
                Left = left,
                zIndex = 5,
                Top = top,
                Width = 100 + (dnc.Name.Length - 5) * 7,
                Height = 70,
                ParentID = "00000000-0000-0000-0000-000000000000",
                ID = guid,
                IsGroup = false,
            });

            return guid;
        }
    }
}
