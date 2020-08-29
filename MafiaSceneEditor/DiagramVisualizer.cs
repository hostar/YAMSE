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

namespace YAMSE
{
    public partial class DiagramVisualizer : KryptonForm
    {
        readonly System.Windows.Forms.Integration.ElementHost elementHostDiagramEditor;
        readonly MyDesigner myDesigner = new MyDesigner();

        MainForm2 mainForm;

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
                }
            }

            // optimize nodes position
            // listBoxOutput.Items.Add($"---TOP-----");

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
            //myDesigner.MyDesignerCanvas.MouseDown += MyDesignerCanvas_MouseDown;
            myDesigner.MyDesignerCanvas.PreviewMouseDown += MyDesignerCanvas_MouseDown;
        }

        private void MyDesignerCanvas_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                var grid = ((e.Source as DesignerItem).Content as Test1).Content as Grid;
                var nodeName = string.Empty;
                foreach (var item in grid.Children)
                {
                    if (item is TextBlock textBlock)
                    {
                        nodeName = textBlock.Text;
                    }
                }

                TreeNode nodeToClick = null;
                if (nodeName != string.Empty)
                {
                    foreach (var node in mainForm.treeView1.Nodes)
                    {
                        var treeNode = node as TreeNode;
                        if (treeNode.Text == "Object definitions")
                        {
                            foreach (var item in treeNode.Nodes)
                            {
                                var innerTreeNode = item as TreeNode;
                                if (innerTreeNode.Text.Contains("Script"))
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

        private bool isNodeNear(int a, int b)
        {
            return Math.Abs(a - b) < 20;
        }

        private static string AddToDesignerItems(List<RootDesignerItem> designerItems, int left, int top, Dnc script)
        {
            var guid = Guid.NewGuid().ToString();
            designerItems.Add(new RootDesignerItem
            {
                Content = Resources.Test1Content.Replace("Box_placeholder", script.name),
                Left = left,
                Top = top,
                Width = 100 + (script.name.Length - 5) * 7,
                Height = 70,
                ParentID = "00000000-0000-0000-0000-000000000000",
                ID = guid,
                IsGroup = false,
            });

            return guid;
        }
    }
}
