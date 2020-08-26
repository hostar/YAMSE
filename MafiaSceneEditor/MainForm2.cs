using ComponentFactory.Krypton.Navigator;
using ComponentFactory.Krypton.Ribbon;
using ComponentFactory.Krypton.Toolkit;
using ComponentFactory.Krypton.Workspace;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using YAMSE.DataLayer;

namespace YAMSE
{
    public partial class MainForm2 : KryptonForm
    {
        KryptonManager kryptonManager = new KryptonManager();

        KryptonWorkspace kryptonWorkspaceTreeView = new KryptonWorkspace();
        KryptonWorkspace kryptonWorkspaceContent = new KryptonWorkspace();

        KryptonSplitContainer splitContainerInner = new KryptonSplitContainer();
        KryptonSplitContainer splitContainerOuter = new KryptonSplitContainer();

        KryptonRibbon kryptonRibbon = new KryptonRibbon();

        KryptonTreeView treeView1 = new KryptonTreeView();

        KryptonContextMenuItem kryptonContextMenuItem1 = new KryptonContextMenuItem();
        KryptonContextMenuItem kryptonContextMenuItem2 = new KryptonContextMenuItem();
        KryptonContextMenuItem kryptonContextMenuItem3 = new KryptonContextMenuItem();
        KryptonContextMenuItem kryptonContextMenuItem4 = new KryptonContextMenuItem();
        KryptonContextMenuItem kryptonContextMenuItemExit = new KryptonContextMenuItem();
        KryptonContextMenuSeparator kryptonContextMenuSeparator1 = new KryptonContextMenuSeparator();

        KryptonRibbonTab kryptonRibbonTab1 = new KryptonRibbonTab();
        KryptonRibbonGroup kryptonRibbonGroup1 = new KryptonRibbonGroup();
        KryptonRibbonGroupTriple kryptonRibbonGroupTriple1 = new KryptonRibbonGroupTriple();

        KryptonRibbonTab kryptonRibbonTab2 = new KryptonRibbonTab();
        KryptonRibbonGroup kryptonRibbonGroup2 = new KryptonRibbonGroup();
        KryptonRibbonGroupTriple kryptonRibbonGroupTriple2 = new KryptonRibbonGroupTriple();

        KryptonRibbonGroupButton kryptonRibbonGroupButtonShowDiagram = new KryptonRibbonGroupButton();
        KryptonRibbonGroupButton kryptonRibbonGroupButtonWorkspaceArrange = new KryptonRibbonGroupButton();

        KryptonListBox listBoxOutput = new KryptonListBox();

        TreeNode currentTreeNode;

        OpenFileDialog openFileDialog1 = new OpenFileDialog();

        List<string> activeDncs = new List<string>();

        //KryptonWorkspaceCell workspaceMain = new KryptonWorkspaceCell();

        bool isDirty = false;
        bool scene2FileLoaded = false;

        private Scene2Data scene2Data = new Scene2Data();

        public MainForm2()
        {
            // KryptonExplorer
            InitializeComponent();
            SuspendLayout();

            kryptonManager.GlobalPaletteMode = PaletteModeManager.Office2010Blue;

            ((ISupportInitialize)kryptonWorkspaceTreeView).BeginInit();
            //Controls.Add(kryptonWorkspaceTreeView);
            kryptonWorkspaceTreeView.Dock = DockStyle.Fill;
            ((ISupportInitialize)kryptonWorkspaceTreeView).EndInit();

            kryptonWorkspaceTreeView.Root.Children.Add(CreateCell("Objects", NavigatorMode.Panel));

            kryptonWorkspaceTreeView.AllowPageDrag = false;

            ((ISupportInitialize)kryptonWorkspaceContent).BeginInit();
            kryptonWorkspaceContent.Dock = DockStyle.Fill;

            kryptonWorkspaceContent.ContextMenus.ShowContextMenu = false;
            //workspaceMain.NavigatorMode = NavigatorMode.BarTabGroup;
            //cell.Pages.Add(CreatePage(pageName));

            //kryptonWorkspaceContent.Root.Children.Add(workspaceMain);

            kryptonWorkspaceContent.WorkspaceCellAdding += kryptonWorkspace_WorkspaceCellAdding;

            ((ISupportInitialize)kryptonWorkspaceContent).EndInit();

            //kryptonWorkspaceContent.Root.Children.Add(CreateCell(2));

            Controls.Add(splitContainerOuter);
            Controls.Add(kryptonRibbon);

            listBoxOutput.Dock = DockStyle.Fill;

            splitContainerInner.Dock = DockStyle.Fill;
            splitContainerInner.Panel1.Controls.Add(kryptonWorkspaceTreeView);
            splitContainerInner.Panel2.Controls.Add(kryptonWorkspaceContent);

            splitContainerOuter.Dock = DockStyle.Fill;
            splitContainerOuter.Orientation = Orientation.Horizontal;
            splitContainerOuter.Panel1.Controls.Add(splitContainerInner);
            splitContainerOuter.Panel2.Controls.Add(listBoxOutput);

            FormClosing += MainForm_Close;

            MinimumSize = new Size(500, 500);

            // this is essential, do not delete
            System.Windows.Application app = new System.Windows.Application
            {
                MainWindow = new System.Windows.Window()
            };

            InitRibbon();

            ResumeLayout();
        }

        private void InitRibbon()
        {
            ComponentResourceManager resources = new ComponentResourceManager(typeof(MainForm2));

            kryptonContextMenuItem1.Text = "New";
            kryptonContextMenuItem1.Image = (Image)resources.GetObject("ImageNew");

            kryptonContextMenuItem2.Text = "Open";
            kryptonContextMenuItem2.Image = (Image)resources.GetObject("ImageOpen");
            kryptonContextMenuItem2.Click += Scene2FileLoad;

            kryptonContextMenuItem3.Text = "Save";
            kryptonContextMenuItem3.Image = (Image)resources.GetObject("ImageSave");
            kryptonContextMenuItem3.Click += Scene2FileSave;

            kryptonContextMenuItem4.Text = "Save As";
            kryptonContextMenuItem4.Image = (Image)resources.GetObject("ImageSaveAs");

            kryptonContextMenuItemExit.Text = "Exit";
            kryptonContextMenuItemExit.Image = (Image)resources.GetObject("ImageExit");
            kryptonContextMenuItemExit.Click += (sender, e) => { Close(); };

            kryptonRibbon.HideRibbonSize = new Size(100, 250);
            kryptonRibbon.QATLocation = QATLocation.Hidden;

            kryptonRibbonTab1.Text = "Tools";
            kryptonRibbonTab2.Text = "Workspace";

            kryptonRibbon.RibbonAppButton.AppButtonMenuItems.AddRange(new KryptonContextMenuItemBase[] {
            kryptonContextMenuItem1,
            kryptonContextMenuItem2,
            kryptonContextMenuItem3,
            kryptonContextMenuItem4,
            kryptonContextMenuSeparator1,
            kryptonContextMenuItemExit});

            kryptonRibbon.RibbonTabs.AddRange(new KryptonRibbonTab[] {
            kryptonRibbonTab1,
            kryptonRibbonTab2});

            kryptonRibbonGroup1.DialogBoxLauncher = false;
            kryptonRibbonGroup1.TextLine1 = "Visualization";

            kryptonRibbonTab1.Groups.AddRange(new KryptonRibbonGroup[] {
            kryptonRibbonGroup1});

            kryptonRibbonGroup1.Items.AddRange(new KryptonRibbonGroupContainer[] {
            kryptonRibbonGroupTriple1});

            kryptonRibbonGroupButtonShowDiagram.TextLine1 = "Show diagram";

            kryptonRibbonGroupTriple1.Items.AddRange(new KryptonRibbonGroupItem[] {
            kryptonRibbonGroupButtonShowDiagram});

            kryptonRibbonGroup2.DialogBoxLauncher = false;
            kryptonRibbonGroup2.MinimumWidth = 200;
            kryptonRibbonGroup2.TextLine1 = "Arrange";

            kryptonRibbonGroupButtonWorkspaceArrange.Click += (sender, e) => { kryptonWorkspaceContent.ApplyGridPages(); };
            kryptonRibbonGroupButtonWorkspaceArrange.TextLine1 = "Grid";

            kryptonRibbonGroupTriple2.Items.AddRange(new KryptonRibbonGroupItem[] {
            kryptonRibbonGroupButtonWorkspaceArrange});

            kryptonRibbonGroup2.Items.AddRange(new KryptonRibbonGroupContainer[] {
            kryptonRibbonGroupTriple2});

            kryptonRibbonTab2.Groups.AddRange(new KryptonRibbonGroup[] {
            kryptonRibbonGroup2});
        }

        private void MainForm_Close(object sender, FormClosingEventArgs e)
        {
            if (KryptonMessageBox.Show("Are you sure you want to exit?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.No)
            {
                e.Cancel = true;
            }
        }

        private void kryptonWorkspace_WorkspaceCellAdding(object sender, WorkspaceCellEventArgs e)
        {
            // Do not show any navigator level buttons
            e.Cell.Button.CloseButtonDisplay = ButtonDisplay.Hide;
            e.Cell.Button.ButtonDisplayLogic = ButtonDisplayLogic.None;

            // Do not need the secondary header for header modes
            e.Cell.Header.HeaderVisibleSecondary = false;
        }

        private KryptonWorkspaceCell CreateCell(string pageName, NavigatorMode mode = NavigatorMode.BarTabGroup)
        {
            // Create new cell instance
            KryptonWorkspaceCell cell = new KryptonWorkspaceCell();

            // Do we need to set the star sizing value?
            /*
            if (!string.IsNullOrEmpty(starSize))
                cell.StarSize = starSize;
            */
            cell.NavigatorMode = mode;
            cell.Pages.Add(CreatePage(pageName));

            return cell;
        }

        private KryptonPage CreatePage(string pageName)
        {
            // Create a new page and give it a name and image
            KryptonPage page = new KryptonPage();
            page.Text = pageName;
            page.TextTitle = pageName;
            page.TextDescription = pageName;
            //page.ImageSmall = imageList.Images[_count % imageList.Images.Count];
            page.MinimumSize = new Size(200, 250);

            treeView1.Dock = DockStyle.Fill;

            treeView1.AfterSelect += (sender, e) => { SelectedObjectChanged(e.Node); };
            treeView1.NodeMouseClick += (sender, e) => { SelectedObjectChanged(e.Node); };

            // Add rich text box as the contents of the page
            page.Padding = new Padding(5);
            page.Controls.Add(treeView1);

            return page;
        }

        private KryptonPage CreatePage(Dnc dnc, PanelKind panelKind, string text = "")
        {
            string pageName = dnc.name;

            var pageId = new KryptonPageId { Dnc = dnc, PanelKind = panelKind };

            switch (panelKind)
            {
                case PanelKind.Text:
                    Scintilla scintillaTextEditor = new Scintilla
                    {
                        //WrapMode = WrapMode.Word, 
                        //IndentationGuides = IndentView.LookBoth, 
                        //Parent = mainPanel, 
                        Dock = DockStyle.Fill,
                        ScrollWidth = 200
                    };

                    scintillaTextEditor.Styles[Style.Default].Font = "Consolas";
                    scintillaTextEditor.Styles[Style.Default].Size = 10;

                    scintillaTextEditor.Lexer = Lexer.Null;

                    scintillaTextEditor.Text = text;

                    try
                    {
                        scintillaTextEditor.Styles[1].ForeColor = Color.Blue;
                        scintillaTextEditor.Styles[2].ForeColor = Color.Crimson;
                        scintillaTextEditor.Styles[3].ForeColor = Color.Blue;
                        scintillaTextEditor.Styles[4].ForeColor = Color.Green;
                        DncMethods.ScintillaTextHighlight(text, 0, scintillaTextEditor);
                    }
                    catch { }

                    scintillaTextEditor.TextChanged += (sender, eargs) =>
                    {
                        DncMethods.ScintillaTextHighlight(scintillaTextEditor.Lines[scintillaTextEditor.LineFromPosition(scintillaTextEditor.CurrentPosition)].Text, scintillaTextEditor.CurrentPosition, scintillaTextEditor);
                    };

                    pageId.ScintillaTextEditor = scintillaTextEditor;
                    return CreatePageInternal(pageName, pageId, scintillaTextEditor);
                case PanelKind.Hex:

                    var hexEditor = new WpfHexaEditor.HexEditor
                    {
                        ForegroundSecondColor = System.Windows.Media.Brushes.Blue,
                        TypeOfCharacterTable = WpfHexaEditor.Core.CharacterTableType.Ascii
                    };

                    var elementHostHexEditor = new System.Windows.Forms.Integration.ElementHost
                    {
                        Location = new Point(250, 50),
                        Size = new Size(1000, 500)
                    };
                    elementHostHexEditor.Name = nameof(elementHostHexEditor);
                    elementHostHexEditor.Child = hexEditor;

                    var tmpStream = new MemoryStream();
                    new MemoryStream(dnc.rawData).CopyTo(tmpStream); // needed in order to allow expanding
                    hexEditor.Stream = tmpStream;

                    pageId.HexEditor = hexEditor;

                    return CreatePageInternal(pageName, pageId, elementHostHexEditor);
                default:
                    throw new InvalidOperationException(nameof(CreatePage));
            }
        }

        private KryptonPage CreatePageInternal(string pageName, KryptonPageId pageId, Control mainComponent)
        {
            // Create a new page and give it a name and image
            KryptonPage page = new KryptonPage();
            page.Text = pageName;
            page.TextTitle = pageName;
            page.TextDescription = pageName;
            page.Tag = pageId;
            //page.ImageSmall = imageList.Images[_count % imageList.Images.Count];
            page.MinimumSize = new Size(200, 250);

            TableLayoutPanel tableLayoutPanel1 = new TableLayoutPanel();

            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 114F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 114F));

            tableLayoutPanel1.Controls.Add(CreateButton(pageId, DncMethods.BtnSaveClick, "Save"), 0, 1);
            tableLayoutPanel1.Controls.Add(CreateButton(pageId, DncMethods.BtnRevertClick, "Revert"), 2, 1);

            tableLayoutPanel1.Controls.Add(mainComponent, 0, 0);

            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 27);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            tableLayoutPanel1.Size = new Size(1149, 533);
            tableLayoutPanel1.TabIndex = 0;

            tableLayoutPanel1.SetColumnSpan(mainComponent, 3);
            tableLayoutPanel1.SetRowSpan(mainComponent, 1);

            // Create a close button for the page
            ButtonSpecAny bsa = new ButtonSpecAny();
            bsa.Tag = page;
            bsa.Type = PaletteButtonSpecStyle.Close;
            bsa.Click += PageClose;
            page.ButtonSpecs.Add(bsa);

            // Add rich text box as the contents of the page
            page.Padding = new Padding(5);
            //page.Controls.Add(scintillaTextEditor);
            page.Controls.Add(tableLayoutPanel1);

            //workspaceMain.Pages.Add(page);
            kryptonWorkspaceContent.FirstCell().Pages.Add(page);
            return page;
        }

        private Control CreateButton(KryptonPageId kryptonPageId, EventHandler eventHandler, string btnName)
        {
            KryptonButton btnObj = new KryptonButton
            {
                Anchor = AnchorStyles.Bottom,
                Location = new Point(3, 486),
                Name = $"btn{btnName}",
                Size = new Size(108, 44),
                TabIndex = 1,
                Text = btnName,
                Tag = kryptonPageId,
                PaletteMode = PaletteMode.Office2010Blue
            };
            btnObj.Click += eventHandler;

            return btnObj;
        }

        private void PageClose(object sender, EventArgs e)
        {
            KryptonPage page = (sender as ButtonSpecAny).Tag as KryptonPage;
            activeDncs.Remove(page.Tag.ToString());
            kryptonWorkspaceContent.FirstCell().Pages.Remove(page);
        }

        private void SelectedObjectChanged(TreeNode e)
        {
            Dnc dnc;
            string currId = string.Empty;

            /*
            if (currentTreeNode?.GetHashCode() == e.GetHashCode())
            {
                return;
            }
            */
            currentTreeNode = e;

            if (e.Tag != null)
            {
                var nodeType = ((NodeTag)e.Tag).nodeType;
                switch (nodeType)
                {
                    case NodeType.Object:
                        //elementHostHexEditor.Show();
                        //elementHostDiagramEditor.Hide();
                        //hexEditor.Stream = new MemoryStream(scene2Data.objectsDncs.Where(x => x.ID == ((NodeTag)e.Tag).id).FirstOrDefault().rawData);

                        dnc = scene2Data.Sections.First(x => x.SectionType == NodeType.Object).Dncs.Where(x => x.ID == ((NodeTag)e.Tag).id).FirstOrDefault();
                        currId = DncMethods.CreatePageID(dnc, nodeType);

                        if (activeDncs.Any(x => x == currId))
                        {
                            return;
                        }

                        activeDncs.Add(currId);
                        CreatePage(dnc, PanelKind.Hex);
                        break;
                    case NodeType.Definition:

                        dnc = scene2Data.Sections.First(x => x.SectionType == NodeType.Definition).Dncs.Where(x => x.ID == ((NodeTag)e.Tag).id).FirstOrDefault();
                        currId = DncMethods.CreatePageID(dnc, nodeType);

                        if (activeDncs.Any(x => x == currId))
                        {
                            return;
                        }

                        switch (dnc.dncType)
                        {
                            case DncType.Script:
                                //elementHostHexEditor.Hide();
                                activeDncs.Add(currId);
                                CreatePage(dnc, PanelKind.Text, Scene2Parser.GetStringFromDnc(dnc));
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
                                activeDncs.Add(currId);
                                CreatePage(dnc, PanelKind.Hex);
                                break;
                        }

                        break;
                    case NodeType.InitScript:
                        dnc = scene2Data.Sections.First(x => x.SectionType == NodeType.InitScript).Dncs.Where(x => x.ID == ((NodeTag)e.Tag).id).FirstOrDefault();

                        currId = DncMethods.CreatePageID(dnc, nodeType);

                        if (activeDncs.Any(x => x == currId))
                        {
                            return;
                        }

                        activeDncs.Add(currId);
                        CreatePage(dnc, PanelKind.Text, Scene2Parser.GetStringFromInitScript(dnc));
                        break;
                    default:
                        dnc = scene2Data.Sections.Where(x => x.SectionType == NodeType.Unknown).SelectMany(x => x.Dncs).Where(x => x.ID == ((NodeTag)e.Tag).id).FirstOrDefault();
                        currId = DncMethods.CreatePageID(dnc, nodeType);

                        if (activeDncs.Any(x => x == currId))
                        {
                            return;
                        }

                        activeDncs.Add(currId);
                        CreatePage(dnc, PanelKind.Hex);
                        break;
                }

                treeView1.Focus();
            }
        }

        private void Scene2FileLoad(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                scene2FileLoaded = true;
                listBoxOutput.Items.Add("Loading file...");

                MemoryStream memoryStream = new MemoryStream();
                Stream tmpStream = openFileDialog1.OpenFile();
                tmpStream.CopyTo(memoryStream);
                tmpStream.Close();

                scene2Data = new Scene2Data();

                Scene2Parser.LoadScene(memoryStream, ref scene2Data, listBoxOutput.Items);

                // put into treeview
                // objects
                treeView1.Nodes.Clear();

                int i = 0;

                foreach (Scene2Section section in scene2Data.Sections)
                {
                    TreeNode objectsTreeNode = new TreeNode(section.SectionName);

                    foreach (IGrouping<DncType, Dnc> item in section.Dncs.GroupBy(x => x.dncType))
                    {
                        TreeNode treeNodeParent = new TreeNode(item.Key.ToString());

                        if (item.Key == DncType.Unknown)
                        {
                            treeNodeParent.Text += $" {section.SectionName}";
                        }

                        i = 0;

                        List<TreeNode> nodeList = new List<TreeNode>();
                        foreach (Dnc dnc in item)
                        {
                            TreeNode treeNode = new TreeNode
                            {
                                Text = dnc.name,
                                Tag = new NodeTag
                                {
                                    id = dnc.ID,
                                    nodeType = section.SectionType
                                }
                            };

                            nodeList.Add(treeNode);
                            i++;
                        }

                        // sort nodes
                        nodeList = nodeList.OrderBy(x => x.Text).ToList();

                        treeNodeParent.Nodes.AddRange(nodeList.ToArray());

                        treeNodeParent.Text += $" [{nodeList.Count}]";
                        objectsTreeNode.Nodes.Add(treeNodeParent);
                    }
                    treeView1.Nodes.Add(objectsTreeNode);

                }

                /*
                foreach (IGrouping<ObjectIDs, Dnc> item in scene2Data.objectsDncs.GroupBy(x => x.objectType))
                {
                    TreeNode treeNodeParent = new TreeNode(item.Key.ToString());

                    i = 0;

                    List<TreeNode> nodeList = new List<TreeNode>();
                    foreach (Dnc dnc in item)
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

                    foreach (TreeNode node2 in nodeList)
                    {
                        treeNodeParent.Nodes.Add(node2);
                    }

                    treeNodeParent.Text += $" [{nodeList.Count}]";
                    objectsTreeNode.Nodes.Add(treeNodeParent);
                }
                treeView1.Nodes.Add(objectsTreeNode);


                // definitions
                TreeNode defsTreeNode = new TreeNode("Object Definitions");
                foreach (IGrouping<DefinitionIDs, Dnc> item in scene2Data.objectDefinitionsDncs.GroupBy(x => x.definitionType))
                {
                    TreeNode treeNodeParent = new TreeNode(item.Key.ToString());

                    i = 0;

                    List<TreeNode> nodeList = new List<TreeNode>();
                    foreach (Dnc dnc in item)
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

                    foreach (TreeNode node2 in nodeList)
                    {
                        treeNodeParent.Nodes.Add(node2);
                    }

                    treeNodeParent.Text += $" [{nodeList.Count}]";
                    defsTreeNode.Nodes.Add(treeNodeParent);
                }

                treeView1.Nodes.Add(defsTreeNode);

                // init scripts
                TreeNode initScriptTreeNode = new TreeNode("Init script");
                foreach (IGrouping<DefinitionIDs, Dnc> item in scene2Data.initScriptsDncs.GroupBy(x => x.definitionType))
                {
                    TreeNode treeNodeParent = new TreeNode(item.Key.ToString());

                    i = 0;

                    List<TreeNode> nodeList = new List<TreeNode>();
                    foreach (Dnc dnc in item)
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

                    foreach (TreeNode node2 in nodeList)
                    {
                        treeNodeParent.Nodes.Add(node2);
                    }

                    treeNodeParent.Text += $" [{nodeList.Count}]";
                    initScriptTreeNode.Nodes.Add(treeNodeParent);
                }

                treeView1.Nodes.Add(initScriptTreeNode);
                */

                listBoxOutput.Items.Add("Loading of file done.");
            }
        }

        private void Scene2FileSave(object sender, EventArgs e)
        {
            var tmpStream = new FileStream(openFileDialog1.FileName, FileMode.Create);
            Scene2Parser.SaveScene(tmpStream, ref scene2Data, listBoxOutput.Items);
            tmpStream.Close();
        }
    }
}
