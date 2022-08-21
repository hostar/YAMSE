using ComponentFactory.Krypton.Navigator;
using ComponentFactory.Krypton.Ribbon;
using ComponentFactory.Krypton.Toolkit;
using ComponentFactory.Krypton.Workspace;
using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using YAMSE.DataLayer;
using YAMSE.Helpers;
using ICSharpCode.AvalonEdit.Search;
using System.Diagnostics;
using ComponentFactory.Krypton.Docking;

namespace YAMSE
{
    public partial class MainForm2 : KryptonForm
    {
        readonly KryptonManager kryptonManager = new KryptonManager();
        readonly KryptonWorkspace kryptonWorkspaceTreeView = new KryptonWorkspace();
        readonly KryptonWorkspace kryptonWorkspaceContent = new KryptonWorkspace();
        readonly KryptonSplitContainer splitContainerInner = new KryptonSplitContainer();
        readonly KryptonSplitContainer splitContainerOuter = new KryptonSplitContainer();
        readonly KryptonRibbon kryptonRibbon = new KryptonRibbon();

        internal KryptonTreeView treeViewMain = new KryptonTreeView();
        internal KryptonTreeView treeViewMainScriptSearch = new KryptonTreeView();

        readonly KryptonContextMenuItem kryptonContextMenuItem1 = new KryptonContextMenuItem();
        readonly KryptonContextMenuItem kryptonContextMenuItem2 = new KryptonContextMenuItem();
        readonly KryptonContextMenuItem kryptonContextMenuItem3 = new KryptonContextMenuItem();
        readonly KryptonContextMenuItem kryptonContextMenuItem4 = new KryptonContextMenuItem();
        readonly KryptonContextMenuItem kryptonContextMenuItemAbout = new KryptonContextMenuItem();
        readonly KryptonContextMenuItem kryptonContextMenuItemExit = new KryptonContextMenuItem();

        readonly KryptonContextMenuSeparator kryptonContextMenuSeparator1 = new KryptonContextMenuSeparator();

        readonly KryptonRibbonTab kryptonRibbonTabTools = new KryptonRibbonTab();
        readonly KryptonRibbonGroup kryptonRibbonGroupVisualization = new KryptonRibbonGroup();
        readonly KryptonRibbonGroupTriple kryptonRibbonGroupTriple1 = new KryptonRibbonGroupTriple();

        readonly KryptonRibbonTab kryptonRibbonTabWorkspace = new KryptonRibbonTab();
        readonly KryptonRibbonGroup kryptonRibbonGroupArrange = new KryptonRibbonGroup();

        readonly KryptonRibbonGroupTriple kryptonRibbonGroupTriple2 = new KryptonRibbonGroupTriple();

        readonly KryptonRibbonGroup kryptonRibbonGroupExternalData = new KryptonRibbonGroup();
        readonly KryptonRibbonGroupTriple kryptonRibbonGroupTriple3 = new KryptonRibbonGroupTriple();
        readonly KryptonRibbonGroupTriple kryptonRibbonGroupTriple4 = new KryptonRibbonGroupTriple();

        readonly KryptonRibbonGroupButton kryptonRibbonGroupButtonShowDiagram = new KryptonRibbonGroupButton();

        readonly KryptonRibbonGroupButton kryptonRibbonGroupButtonLoadDefs = new KryptonRibbonGroupButton();

        readonly KryptonRibbonGroupButton kryptonRibbonGroupButtonStartGame = new KryptonRibbonGroupButton();
        readonly KryptonRibbonGroupButton kryptonRibbonGroupButtonStartMafiaCon = new KryptonRibbonGroupButton();

        readonly KryptonRibbonGroupButton kryptonRibbonGroupButtonSelectFileForComparison = new KryptonRibbonGroupButton();

        readonly KryptonRibbonGroupButton kryptonRibbonGroupButtonImportDnc = new KryptonRibbonGroupButton();

        readonly KryptonRibbonGroupButton kryptonRibbonGroupButtonWorkspaceArrange = new KryptonRibbonGroupButton();

        readonly KryptonRibbonQATButton kryptonQatButtonUndo = new KryptonRibbonQATButton();

        readonly KryptonListBox listBoxOutput = new KryptonListBox();
        readonly KryptonLabel outputLabel = new KryptonLabel { Text = "Output"};

        readonly Logging logging;

        TreeNode currentTreeNode;

        readonly OpenFileDialog openFileDialog1 = new OpenFileDialog();
        readonly SaveFileDialog saveFileDialogExport = new SaveFileDialog();

        readonly OpenFileDialog openFileDialog2 = new OpenFileDialog();
        readonly OpenFileDialog openFileDialogDnc = new OpenFileDialog();

        readonly OpenFileDialog openFileDialogSecond = new OpenFileDialog();

        //readonly List<string> activeDncs = new List<string>(); // TODO: transform this to class allowing to create and remove pages programmatically

        //KryptonWorkspaceCell workspaceMain = new KryptonWorkspaceCell();

        bool scene2FileLoaded = false;

        private Scene2Data scene2Data = new Scene2Data();
        private Scene2Data scene2DataSecond = new Scene2Data();

        private DiagramVisualizer diagramVisualizer;

        int _maxRecentDocs = 9;

        string fNameRecent = "\\recent.list";

        private int lastFound = 0;

        private string defFilePath = string.Empty;

        private KryptonContextMenu treeViewMenu = new KryptonContextMenu();

        private TreeNode currTreeNode;

        private ButtonSpecAny caseSensitive = new ButtonSpecAny();
        private ButtonSpecAny dismissResults = new ButtonSpecAny();

        private KryptonTextBox kryptonTextBoxObjectSearch = new KryptonTextBox() { Dock = DockStyle.Fill };
        private KryptonTextBox kryptonTextBoxScriptSearch = new KryptonTextBox() { Dock = DockStyle.Fill };

        public MainForm2()
        {
            // KryptonExplorer
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");

            InitializeComponent();
            SuspendLayout();

            kryptonManager.GlobalPaletteMode = PaletteModeManager.Office2010Blue;

            ((ISupportInitialize)kryptonWorkspaceTreeView).BeginInit();

            kryptonWorkspaceTreeView.Dock = DockStyle.Fill;
            ((ISupportInitialize)kryptonWorkspaceTreeView).EndInit();

            kryptonWorkspaceTreeView.Root.Children.Add(CreateCell("Objects", NavigatorMode.Panel));

            kryptonWorkspaceTreeView.AllowPageDrag = false;

            ((ISupportInitialize)kryptonWorkspaceContent).BeginInit();
            kryptonWorkspaceContent.Dock = DockStyle.Fill;

            kryptonWorkspaceContent.ContextMenus.ShowContextMenu = true;

            kryptonWorkspaceContent.WorkspaceCellAdding += kryptonWorkspace_WorkspaceCellAdding;

            ((ISupportInitialize)kryptonWorkspaceContent).EndInit();

            Controls.Add(splitContainerOuter);
            Controls.Add(kryptonRibbon);

            listBoxOutput.Dock = DockStyle.Fill;

            /// context menu
            KryptonContextMenuItems kryptonContextMenuItems = new KryptonContextMenuItems();

            KryptonContextMenuItems options = new KryptonContextMenuItems();
            treeViewMenu.Items.Clear();
            treeViewMenu.Items.Add(options);

            KryptonContextMenuItem exportContextMenuItem = new KryptonContextMenuItem("Export as DNC") { Enabled = true };
            exportContextMenuItem.Click += ExportDnc;
            options.Items.Add(exportContextMenuItem);

            KryptonContextMenuItem renameContextMenuItem = new KryptonContextMenuItem("Rename") { Enabled = true };
            renameContextMenuItem.Click += RenameDnc;
            options.Items.Add(renameContextMenuItem);

            KryptonContextMenuItem duplicateContextMenuItem = new KryptonContextMenuItem("Duplicate") { Enabled = true };
            duplicateContextMenuItem.Click += DuplicateDnc;
            options.Items.Add(duplicateContextMenuItem);

            KryptonContextMenuItem removeContextMenuItem = new KryptonContextMenuItem("Remove") { Enabled = true };
            removeContextMenuItem.Click += RemoveDnc;
            options.Items.Add(removeContextMenuItem);

            splitContainerInner.Dock = DockStyle.Fill;
            splitContainerInner.SeparatorStyle = SeparatorStyle.HighProfile;

            Label kryptonLabelObjectSearch = new Label { Text = "Search\r\nobjects:", AutoSize = true, Dock = DockStyle.Fill};
            kryptonLabelObjectSearch.Font = new Font("Segoe UI", 6F, GraphicsUnit.Point);

            Label kryptonLabelScriptSearch = new Label { Text = "Search\r\nscripts:", AutoSize = true, Dock = DockStyle.Fill };
            kryptonLabelScriptSearch.Font = new Font("Segoe UI", 6.5F, GraphicsUnit.Point);
            
            kryptonTextBoxObjectSearch.TextChanged += (sender, e) => { lastFound = 0; };
            kryptonTextBoxObjectSearch.AllowButtonSpecToolTips = true;

            caseSensitive.ToolTipBody = "Case sensitive";
            caseSensitive.ToolTipStyle = LabelStyle.ToolTip;
            caseSensitive.Checked = ButtonCheckState.Unchecked;
            caseSensitive.Text = "Cc";
            kryptonTextBoxObjectSearch.ButtonSpecs.Add(caseSensitive);

            KryptonButton kryptonButtonObjectSearch = new KryptonButton() { Width = 25 };
            kryptonButtonObjectSearch.Values.Image = Resources.FindSmall;

            kryptonButtonObjectSearch.Click += SearchObjectButtonClick;
            kryptonButtonObjectSearch.Tag = kryptonTextBoxObjectSearch;

            kryptonTextBoxObjectSearch.KeyPress += (sender, e) => {
                if (e.KeyChar == 13)
                {
                    SearchObjectButtonClick(kryptonButtonObjectSearch, null);
                    kryptonTextBoxObjectSearch.Focus();
                }
            };

            // --------------
            KryptonButton kryptonButtonScriptSearch = new KryptonButton() { Width = 25 };

            kryptonTextBoxScriptSearch.KeyPress += (sender, e) => {
                if (e.KeyChar == 13)
                {
                    SearchScriptsButtonClick(kryptonButtonScriptSearch, null);
                    kryptonTextBoxScriptSearch.Focus();
                }
            };

            kryptonButtonScriptSearch.Values.Image = Resources.FindSmall;

            kryptonButtonScriptSearch.Click += SearchScriptsButtonClick;
            kryptonButtonScriptSearch.Tag = kryptonTextBoxScriptSearch;
            kryptonTextBoxScriptSearch.AllowButtonSpecToolTips = true;

            dismissResults.Type = PaletteButtonSpecStyle.Close;
            dismissResults.ToolTipBody = "Dismiss results";
            dismissResults.ToolTipStyle = LabelStyle.ToolTip;
            dismissResults.Checked = ButtonCheckState.NotCheckButton;
            dismissResults.Click += (sender, e) => {
                treeViewMainScriptSearch.Hide();
                treeViewMain.Show();
            };

            kryptonTextBoxScriptSearch.ButtonSpecs.Add(dismissResults);

            TableLayoutPanel tableLayoutPanelTreeView = new TableLayoutPanel
            {
                BackColor = Color.FromArgb(221, 234, 247),
                ColumnCount = 3,
                RowCount = 2,
                Dock = DockStyle.Fill,
                Width = 300
            };
            tableLayoutPanelTreeView.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            tableLayoutPanelTreeView.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            tableLayoutPanelTreeView.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

            tableLayoutPanelTreeView.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50));
            tableLayoutPanelTreeView.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80));
            tableLayoutPanelTreeView.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 30));

            tableLayoutPanelTreeView.Controls.Add(kryptonWorkspaceTreeView, 0, 0);

            tableLayoutPanelTreeView.Controls.Add(kryptonLabelObjectSearch, 0, 1);
            tableLayoutPanelTreeView.Controls.Add(kryptonTextBoxObjectSearch, 1, 1);
            tableLayoutPanelTreeView.Controls.Add(kryptonButtonObjectSearch, 2, 1);

            tableLayoutPanelTreeView.Controls.Add(kryptonLabelScriptSearch, 0, 2);
            tableLayoutPanelTreeView.Controls.Add(kryptonTextBoxScriptSearch, 1, 2);
            tableLayoutPanelTreeView.Controls.Add(kryptonButtonScriptSearch, 2, 2);

            tableLayoutPanelTreeView.SetColumnSpan(kryptonWorkspaceTreeView, 3);

            splitContainerInner.Panel1.Controls.Add(tableLayoutPanelTreeView);
            splitContainerInner.Panel2.Controls.Add(kryptonWorkspaceContent);

            splitContainerOuter.Dock = DockStyle.Fill;
            splitContainerOuter.SplitterDistance = 400;

            splitContainerOuter.Orientation = Orientation.Horizontal;
            splitContainerOuter.Panel1.Controls.Add(splitContainerInner);

            TableLayoutPanel tableLayoutPanelOutput = new TableLayoutPanel
            {
                BackColor = Color.FromArgb(221, 234, 247),
                ColumnCount = 1,
                RowCount = 2,
                Dock = DockStyle.Fill
            };
            tableLayoutPanelOutput.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tableLayoutPanelOutput.RowStyles.Add(new RowStyle(SizeType.Percent, 80F));

            tableLayoutPanelOutput.Controls.Add(outputLabel, 0, 0);
            tableLayoutPanelOutput.Controls.Add(listBoxOutput, 0, 1);

            splitContainerOuter.Panel2.Controls.Add(tableLayoutPanelOutput);

            splitContainerOuter.SeparatorStyle = SeparatorStyle.HighProfile;

            FormClosing += MainForm_Close;

            MinimumSize = new Size(600, 600);

            // this is essential, do not delete
            System.Windows.Application app = new System.Windows.Application
            {
                MainWindow = new System.Windows.Window()
            };

            InitRibbon();

            ResumeLayout();

            diagramVisualizer = new DiagramVisualizer(this);

            ActiveDncs.SetObjects(this, kryptonWorkspaceContent);

            logging = new Logging(listBoxOutput.Items);
        }

        private void DuplicateDnc(object sender, EventArgs e)
        {
            var dnc = currTreeNode.Tag as Dnc;

            var newName = KryptonInputBox.Show("Enter new name", "Duplicate", dnc.Name);
            if (newName != string.Empty)
            {
                Dnc newDnc = Scene2Parser.DuplicateDnc(dnc, newName);

                var dncsInSection = scene2Data.Sections.First(x => x.SectionType == dnc.dncKind).Dncs;
                int highestIDinCat = dncsInSection.OrderBy(x => x.ID).Last().ID;
                newDnc.ID = highestIDinCat;

                dncsInSection.Add(newDnc);

                InsertIntoTree(newDnc);
            }
        }

        private void RemoveDnc(object sender, EventArgs e)
        {
            var dnc = currTreeNode.Tag as Dnc;

            var dncsInSection = scene2Data.Sections.First(x => x.SectionType == dnc.dncKind).Dncs;

            dncsInSection.Remove(dnc);

            ActiveDncs.Remove(dnc);

            foreach (var item in treeViewMain.Nodes)
            {
                if (item is TreeNode treeNode)
                {
                    foreach (var itemIn in treeNode.Nodes)
                    {
                        if (itemIn is TreeNode treeNodeIn)
                        {
                            if (treeNodeIn.Name == dnc.dncType.ToString())
                            {
                                foreach (var toSearch in treeNodeIn.Nodes)
                                {
                                    if (toSearch is TreeNode toSearchIn)
                                    {
                                        if (toSearchIn.Text == dnc.Name)
                                        {
                                            treeNodeIn.Nodes.Remove(toSearchIn);
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
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
            kryptonContextMenuItem4.Click += Scene2FileSaveAs;

            kryptonContextMenuItemExit.Text = "Exit";
            kryptonContextMenuItemExit.Image = (Image)resources.GetObject("ImageExit");
            kryptonContextMenuItemExit.Click += (sender, e) => { Close(); };

            kryptonContextMenuItemAbout.Text = "About";
            kryptonContextMenuItemAbout.Image = Resources.help;
            kryptonContextMenuItemAbout.Click += (sender, e) => { KryptonMessageBox.Show(Resources.AboutFormContent); };

            kryptonRibbon.HideRibbonSize = new Size(100, 250);
            kryptonRibbon.QATLocation = QATLocation.Below;
            kryptonRibbon.QATUserChange = false;

            kryptonQatButtonUndo.Text = "Undo";
            kryptonQatButtonUndo.Image = Resources.undo2;

            kryptonQatButtonUndo.Click += KryptonQatButtonUndo_Click;

            kryptonRibbon.QATButtons.Add(kryptonQatButtonUndo);

            kryptonRibbonTabTools.Text = "Tools";
            kryptonRibbonTabWorkspace.Text = "Workspace";

            kryptonRibbon.RibbonAppButton.AppButtonMenuItems.AddRange(new KryptonContextMenuItemBase[] {
            /*kryptonContextMenuItem1,*/
            kryptonContextMenuItem2,
            kryptonContextMenuItem3,
            kryptonContextMenuItem4,
            kryptonContextMenuSeparator1,
            kryptonContextMenuItemAbout,
            kryptonContextMenuItemExit});

            kryptonRibbon.RibbonTabs.AddRange(new KryptonRibbonTab[] {
            kryptonRibbonTabTools,
            kryptonRibbonTabWorkspace});

            kryptonRibbonGroupVisualization.DialogBoxLauncher = false;
            kryptonRibbonGroupVisualization.TextLine1 = "Visualization";

            kryptonRibbonGroupVisualization.Items.AddRange(new KryptonRibbonGroupContainer[] {kryptonRibbonGroupTriple1});

            kryptonRibbonGroupButtonShowDiagram.TextLine1 = "Show diagram";
            kryptonRibbonGroupButtonShowDiagram.ImageLarge = Resources.hierarchical_structure;
            kryptonRibbonGroupButtonShowDiagram.Click += ShowScriptDependencyDiagram;

            kryptonRibbonGroupTriple1.Items.AddRange(new KryptonRibbonGroupItem[] {kryptonRibbonGroupButtonShowDiagram});

            kryptonRibbonGroupButtonLoadDefs.TextLine1 = "Load def file";
            kryptonRibbonGroupButtonLoadDefs.ImageLarge = Resources.language;
            kryptonRibbonGroupButtonLoadDefs.Click += LoadDefFile;

            kryptonRibbonGroupButtonStartGame.TextLine1 = "Start game";
            kryptonRibbonGroupButtonStartGame.Click += StartGame;

            kryptonRibbonGroupButtonStartMafiaCon.TextLine1 = "Start MafiaCon";
            kryptonRibbonGroupButtonStartMafiaCon.Click += StartMafiaCon;

            kryptonRibbonGroupButtonImportDnc.TextLine1 = "Import DNC";
            kryptonRibbonGroupButtonImportDnc.Click += ImportDnc;

            kryptonRibbonGroupButtonSelectFileForComparison.TextLine1 = "Load scene file for compare";
            kryptonRibbonGroupButtonSelectFileForComparison.ImageLarge = Resources.compare;
            kryptonRibbonGroupButtonSelectFileForComparison.Click += SecondScene2FileLoad;

            kryptonRibbonGroupTriple3.Items.AddRange(new KryptonRibbonGroupItem[] { kryptonRibbonGroupButtonImportDnc, kryptonRibbonGroupButtonLoadDefs, kryptonRibbonGroupButtonStartGame });
            kryptonRibbonGroupTriple4.Items.AddRange(new KryptonRibbonGroupItem[] { kryptonRibbonGroupButtonStartMafiaCon, kryptonRibbonGroupButtonSelectFileForComparison });

            kryptonRibbonGroupArrange.DialogBoxLauncher = false;
            kryptonRibbonGroupArrange.MinimumWidth = 200;
            kryptonRibbonGroupArrange.TextLine1 = "Arrange";

            kryptonRibbonGroupExternalData.DialogBoxLauncher = false;
            kryptonRibbonGroupExternalData.MinimumWidth = 200;
            kryptonRibbonGroupExternalData.TextLine1 = "Misc";

            kryptonRibbonGroupButtonWorkspaceArrange.Click += (sender, e) => { kryptonWorkspaceContent.ApplyGridPages(); };
            kryptonRibbonGroupButtonWorkspaceArrange.ImageLarge = Resources.grid;
            kryptonRibbonGroupButtonWorkspaceArrange.TextLine1 = "Grid";

            kryptonRibbonGroupTriple2.Items.AddRange(new KryptonRibbonGroupItem[] {
            kryptonRibbonGroupButtonWorkspaceArrange});

            kryptonRibbonGroupArrange.Items.AddRange(new KryptonRibbonGroupContainer[] {
            kryptonRibbonGroupTriple2});

            kryptonRibbonGroupExternalData.Items.AddRange(new KryptonRibbonGroupContainer[] {
            kryptonRibbonGroupTriple3, kryptonRibbonGroupTriple4});

            kryptonRibbonTabWorkspace.Groups.AddRange(new KryptonRibbonGroup[] {
            kryptonRibbonGroupArrange});

            kryptonRibbonTabTools.Groups.AddRange(new KryptonRibbonGroup[] { kryptonRibbonGroupVisualization, kryptonRibbonGroupExternalData });

            var fullPathRecent = Directory.GetCurrentDirectory() + fNameRecent;

            if (File.Exists(fullPathRecent))
            {
                foreach (var path in File.ReadAllLines(fullPathRecent))
                {
                    AddRecentFile(path);
                }
            }
        }

        private void ImportDnc(object sender, EventArgs e)
        {
            if (openFileDialogDnc.ShowDialog() == DialogResult.OK)
            {
                var bytes = File.ReadAllBytes(openFileDialogDnc.FileName);
                Dnc currDnc = new Dnc
                {
                    dncType = DncType.Unknown,
                    dncKind = NodeType.Unknown,
                    RawData = bytes.Skip(2).ToArray(),
                    RawDataBackup = bytes.Skip(2).ToArray(),
                    objectIDArr = bytes.Take(2).ToArray()
                };

                currDnc.dncType = Scene2Parser.GetObjectDefinitionType(currDnc);
                if (currDnc.dncType == DncType.Unknown)
                {
                    currDnc.dncType = Scene2Parser.GetObjectType(currDnc);
                    if (currDnc.dncType == DncType.Unknown)
                    {
                        currDnc.dncType = Scene2Parser.TestIfInitScript(currDnc);
                        if (currDnc.dncType != DncType.Unknown)
                        {
                            currDnc.dncKind = NodeType.InitScript;
                        }
                        else
                        {
                            KryptonMessageBox.Show("Unknown DNC type. Cannot import.");
                        }
                    }
                    else
                    {
                        currDnc.dncKind = NodeType.Object;
                    }
                }
                else
                {
                    currDnc.dncKind = NodeType.Definition;
                }

                currDnc.Name = Scene2Parser.GetNameOfDnc(currDnc);
                Scene2Parser.PopulateProps(currDnc);

                var section = scene2Data.Sections.First(x => x.SectionType == currDnc.dncKind);

                if (section != null)
                {
                    var existingDnc = section.Dncs.FirstOrDefault(x => x.Name == currDnc.Name);
                    if (existingDnc != null)
                    { // dnc with same name already exist
                        section.Dncs.Remove(existingDnc);
                        ReplaceInTree(currDnc);
                    }
                    else
                    {
                        InsertIntoTree(currDnc);
                    }
                    section.Dncs.Add(currDnc);
                }
                KryptonMessageBox.Show($"Import successful - name: {currDnc.Name} , type: {currDnc.dncType}");
            }
        }

        private void ReplaceInTree(Dnc dnc)
        {
            foreach (var item in treeViewMain.Nodes)
            {
                if (item is TreeNode treeNode)
                {
                    switch (dnc.dncKind)
                    {
                        case NodeType.Object:
                            if (treeNode.Text == Scene2Parser.SectionNameObjects)
                            {
                                ReplaceNode(dnc, treeNode);
                                return;
                            }
                            break;
                        case NodeType.Definition:
                            if (treeNode.Text == Scene2Parser.SectionNameDefs)
                            {
                                ReplaceNode(dnc, treeNode);
                                return;
                            }
                            break;
                        case NodeType.InitScript:
                            if (treeNode.Text == Scene2Parser.SectionNameObjects)
                            {
                                ReplaceNode(dnc, treeNode);
                                return;
                            }
                            break;
                        case NodeType.Unknown:
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void InsertIntoTree(Dnc dnc)
        {
            foreach (var item in treeViewMain.Nodes)
            {
                if (item is TreeNode treeNode)
                {
                    switch (dnc.dncKind)
                    {
                        case NodeType.Object:
                            if (treeNode.Text == Scene2Parser.SectionNameObjects)
                            {
                                CreateAndEnsure(dnc, treeNode);
                                return;
                            }
                            break;
                        case NodeType.Definition:
                            if (treeNode.Text == Scene2Parser.SectionNameDefs)
                            {
                                CreateAndEnsure(dnc, treeNode);
                                return;
                            }
                            break;
                        case NodeType.InitScript:
                            if (treeNode.Text == Scene2Parser.SectionNameObjects)
                            {
                                CreateAndEnsure(dnc, treeNode);
                                return;
                            }
                            break;
                        case NodeType.Unknown:
                            break;
                        default:
                            break;
                    }
                }
            }

            static void CreateAndEnsure(Dnc dnc, TreeNode treeNode)
            {
                var treeNodeRet = CreateNewNode(dnc, treeNode);
                treeNodeRet.EnsureVisible();
            }
        }

        private static TreeNode CreateNewNode(Dnc currDnc, TreeNode treeNode)
        {
            foreach (var itemIn in treeNode.Nodes)
            {
                if (itemIn is TreeNode treeNodeIn)
                {
                    if (treeNodeIn.Name == currDnc.dncType.ToString())
                    {
                        TreeNode treeNodeNew = new TreeNode
                        {
                            Text = currDnc.Name,
                            Tag = currDnc
                        };
                        treeNodeIn.Nodes.Add(treeNodeNew);

                        List<TreeNode> treeNodes = new List<TreeNode>();
                        foreach (var node in treeNodeIn.Nodes)
                        {
                            treeNodes.Add((TreeNode)node);
                        }
                        treeNodes = treeNodes.OrderBy(x => x.Text).ToList();

                        treeNodeIn.Nodes.Clear();

                        treeNodeIn.Nodes.AddRange(treeNodes.ToArray());
                        return treeNodeNew;
                    }
                }
            }
            return null;
        }

        private static void ReplaceNode(Dnc currDnc, TreeNode treeNode)
        {
            foreach (var itemIn in treeNode.Nodes)
            {
                if (itemIn is TreeNode treeNodeIn)
                {
                    if (treeNodeIn.Name == currDnc.dncType.ToString())
                    {
                        foreach (var node in treeNodeIn.Nodes)
                        {
                            if (node is TreeNode treeNode2)
                            {
                                if (treeNode2.Text == currDnc.Name)
                                {
                                    Dnc dnc = (Dnc)treeNode2.Tag;
                                    currDnc.ID = dnc.ID;
                                    treeNode2.Tag = currDnc;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void KryptonQatButtonUndo_Click(object sender, EventArgs e)
        {
            if (kryptonWorkspaceContent.ActivePage != null)
            {
                KryptonPageId pageId = kryptonWorkspaceContent.ActivePage.Tag as KryptonPageId;

                switch (pageId.PanelKind)
                {
                    case PanelKind.Script:
                        pageId.TextEditor.Undo();
                        break;
                    case PanelKind.Hex:
                        pageId.HexEditor.Undo();
                        break;
                    default:
                        break;
                }
            }
        }

        private void MainForm_Close(object sender, FormClosingEventArgs e)
        {
            if (KryptonMessageBox.Show("Are you sure you want to exit?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.No)
            {
                e.Cancel = true;
            }
            else
            {
                File.WriteAllText(Directory.GetCurrentDirectory() + fNameRecent, string.Join(Environment.NewLine, kryptonRibbon.RibbonAppButton.AppButtonRecentDocs.Select(x => x.Text)));
            }
        }

        private void SearchObjectButtonClick(object sender, EventArgs e)
        {
            KryptonTextBox kryptonTextBoxSearch = (KryptonTextBox)(sender as KryptonButton).Tag;

            if (kryptonTextBoxSearch.Text == string.Empty)
            {
                return;
            }

            string textToSearch = kryptonTextBoxSearch.Text;

            bool found = false;

            int sectionId = 0;

            foreach (var item in treeViewMain.Nodes)
            {
                if (item is TreeNode treeNode)
                {
                    foreach (var node in treeNode.Nodes)
                    {
                        if (node is TreeNode treeNode2)
                        {
                            sectionId++;
                            int objectId = 0;
                            foreach (var node2 in treeNode2.Nodes)
                            {
                                objectId++;

                                string currObjName = string.Empty;
                                if (caseSensitive.Checked == ButtonCheckState.Checked)
                                { // perform case sensitive search
                                    currObjName = (node2 as TreeNode).Text;
                                }
                                else
                                {
                                    currObjName = (node2 as TreeNode).Text.ToLower();
                                    textToSearch = textToSearch.ToLower();
                                }

                                if (currObjName.Contains(textToSearch))
                                {
                                    var foundNode = node2 as TreeNode;

                                    if (lastFound < (sectionId * 100000) + objectId)
                                    {
                                        lastFound = (sectionId * 100000) + objectId;
                                        found = true;

                                        treeNode2.Expand();
                                        foundNode.EnsureVisible();
                                        treeViewMain.SelectedNode = foundNode;
                                        break;
                                    }
                                }
                            }
                        }
                        if (found)
                        {
                            break;
                        }
                    }
                }
                if (found)
                {
                    break;
                }
            }

            if (!found)
            {
                KryptonMessageBox.Show("End of file reached.");
            }
        }

        private void SearchScriptsButtonClick(object sender, EventArgs e)
        {
            if (scene2Data.Sections.Count == 0)
            {
                return;
            }

            if (kryptonTextBoxScriptSearch.Text == string.Empty)
            {
                return;
            }

            treeViewMain.Hide();
            treeViewMainScriptSearch.Show();
            treeViewMainScriptSearch.Nodes.Clear();

            var dncs = scene2Data.Sections.First(x => x.SectionType == NodeType.Definition).Dncs.Where(x => (x.dncType == DncType.Script) || (x.dncType == DncType.Enemy));
            foreach (var dnc in dncs)
            {
                if (Scene2Parser.GetScriptFromDnc(dnc).Contains(kryptonTextBoxScriptSearch.Text))
                {
                    int lNumber = 0;
                    var split = Scene2Parser.GetScriptFromDnc(dnc).Split(Environment.NewLine);
                    for (int i = 0; i < split.Length; i++)
                    {
                        if (split[i].Contains(kryptonTextBoxScriptSearch.Text))
                        {
                            lNumber = i + 1;
                        }
                    }
                    TreeNode treeNode = new TreeNode
                    {
                        Text = dnc.Name + $" ({lNumber})",
                        Tag = dnc
                    };

                    treeViewMainScriptSearch.Nodes.Add(treeNode);
                }
            }
        }

        private void kryptonWorkspace_WorkspaceCellAdding(object sender, WorkspaceCellEventArgs e)
        {
            // Do not show any navigator level buttons
            e.Cell.Button.CloseButtonDisplay = ButtonDisplay.Hide;
            e.Cell.Button.ButtonDisplayLogic = ButtonDisplayLogic.None;

            // Do not need the secondary header for header modes
            e.Cell.Header.HeaderVisibleSecondary = false;
            e.Cell.CloseAction += (sender, e) =>
            {
                var x = e.Item.Tag;
                if (x is KryptonPageId pageId)
                {
                    ActiveDncs.Remove(pageId.Dnc);
                }
            };
        }

        private KryptonWorkspaceCell CreateCell(string pageName, NavigatorMode mode = NavigatorMode.BarTabGroup)
        {
            // Create new cell instance
            KryptonWorkspaceCell cell = new KryptonWorkspaceCell
            {

                // Do we need to set the star sizing value?
                /*
                if (!string.IsNullOrEmpty(starSize))
                    cell.StarSize = starSize;
                */
                NavigatorMode = mode
            };
            cell.Pages.Add(CreatePage(pageName));

            return cell;
        }

        private KryptonPage CreatePage(string pageName)
        {
            // Create a new page and give it a name and image
            KryptonPage page = new KryptonPage
            {
                Text = pageName,
                TextTitle = pageName,
                TextDescription = pageName,
                //page.ImageSmall = imageList.Images[_count % imageList.Images.Count];
                MinimumSize = new Size(200, 250)
            };

            treeViewMain.Dock = DockStyle.Fill;

            treeViewMain.AfterSelect += (sender, e) => { SelectedObjectChanged(e.Node); };
            treeViewMain.NodeMouseClick += (sender, e) => {
                if ((e.Button == MouseButtons.Right) && (e.Node.Tag != null))
                {
                    currTreeNode = e.Node;
                    treeViewMenu.Show(treeViewMain, System.Windows.Forms.Cursor.Position);
                }
                else
                {
                    SelectedObjectChanged(e.Node);
                }
            };

            page.Padding = new Padding(5);
            page.Controls.Add(treeViewMain);

            treeViewMainScriptSearch.Dock = DockStyle.Fill;

            treeViewMainScriptSearch.AfterSelect += (sender, e) => { SelectedObjectChanged(e.Node); };

            page.Controls.Add(treeViewMainScriptSearch);

            return page;
        }

        internal void SelectedObjectChanged(TreeNode e)
        {
            currentTreeNode = e;

            if (e.Tag != null)
            {
                Dnc dnc = e.Tag as Dnc;
                ActiveDncs.Add(dnc);
                //dnc = scene2Data.Sections.Where(x => x.SectionType == NodeType.Unknown).SelectMany(x => x.Dncs).Where(x => x.ID == ((Dnc)e.Tag).ID).FirstOrDefault();

                treeViewMain.Focus();
            }
        }

        internal void ExportDnc(object sender, EventArgs e)
        {
            var dnc = currTreeNode.Tag as Dnc;

            saveFileDialogExport.FileName = $"{dnc.Name}.dnc";
            if (saveFileDialogExport.ShowDialog() == DialogResult.OK)
            {
                var tmpArr = new byte[dnc.RawData.Length + dnc.objectIDArr.Length];
                dnc.objectIDArr.CopyTo(tmpArr, 0);
                dnc.RawData.CopyTo(tmpArr, dnc.objectIDArr.Length);
                File.WriteAllBytes(saveFileDialogExport.FileName, tmpArr);
            }
        }

        private void RenameDnc(object sender, EventArgs e)
        {
            var dnc = currTreeNode.Tag as Dnc;

            var newName = KryptonInputBox.Show("Enter new name", "Rename", dnc.Name);
            if (newName != string.Empty)
            {
                Scene2Parser.RenameDnc(dnc, newName);
                currTreeNode.Text = newName;
            }
        }

        private void AddRecentFile(string filename)
        {
            // Search for an existing entry for that filename
            KryptonRibbonRecentDoc recentDoc = null;
            foreach (KryptonRibbonRecentDoc entry in kryptonRibbon.RibbonAppButton.AppButtonRecentDocs)
                if (entry.Text.Equals(filename))
                {
                    recentDoc = entry;
                    break;
                }

            // If no existing entry then create a new one
            if (recentDoc == null)
            {
                recentDoc = new KryptonRibbonRecentDoc();
                recentDoc.Click += (sender, e) => 
                    {
                        var recent = sender as KryptonRibbonRecentDoc;
                        MemoryStream memoryStream = new MemoryStream();

                        Stream tmpStream = null;
                        try
                        {
                            tmpStream = File.OpenRead(recent.Text);
                        }
                        catch (Exception ex)
                        {
                            logging.Add($"Error when opening file: {ex.Message}");
                            return;
                        }
                        
                        openFileDialog1.FileName = recent.Text;

                        tmpStream.CopyTo(memoryStream);
                        tmpStream.Close();

                        scene2Data = Scene2LoadInternal(memoryStream);
                        PutIntoTreeview(scene2Data);

                        ActiveDncs.RemoveAll();

                        SetFormTitle(openFileDialog1.FileName);

                        scene2FileLoaded = true;
                    };
                recentDoc.Text = filename;
            }

            // Remove entry from current list and insert at the top
            kryptonRibbon.RibbonAppButton.AppButtonRecentDocs.Remove(recentDoc);
            kryptonRibbon.RibbonAppButton.AppButtonRecentDocs.Insert(0, recentDoc);

            // Restrict list to just 9 entries
            if (kryptonRibbon.RibbonAppButton.AppButtonRecentDocs.Count > _maxRecentDocs)
            {
                for (int i = kryptonRibbon.RibbonAppButton.AppButtonRecentDocs.Count; i > _maxRecentDocs; i--)
                {
                    kryptonRibbon.RibbonAppButton.AppButtonRecentDocs.RemoveAt(kryptonRibbon.RibbonAppButton.AppButtonRecentDocs.Count - 1);
                }
            }
        }

        private void Scene2FileLoad(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                logging.Add("Loading file...");

                MemoryStream memoryStream = new MemoryStream();
                Stream tmpStream = openFileDialog1.OpenFile();
                tmpStream.CopyTo(memoryStream);
                tmpStream.Close();

                scene2Data = Scene2LoadInternal(memoryStream);
                PutIntoTreeview(scene2Data);

                ActiveDncs.RemoveAll();

                scene2FileLoaded = true;

                AddRecentFile(openFileDialog1.FileName);
                SetFormTitle(openFileDialog1.FileName);

            }
        }

        private void SetFormTitle(string title)
        {
            Text = $"YAMSE ({title})";
        }

        private void SecondScene2FileLoad(object sender, EventArgs e)
        {
            if (!scene2FileLoaded)
            {
                KryptonMessageBox.Show("First open Scene2 file.");
                return;
            }

            if (openFileDialogSecond.ShowDialog() == DialogResult.OK)
            {
                logging.Add("Loading file...");

                MemoryStream memoryStream = new MemoryStream();
                Stream tmpStream = openFileDialogSecond.OpenFile();
                tmpStream.CopyTo(memoryStream);
                tmpStream.Close();

                scene2DataSecond = Scene2LoadInternal(memoryStream);

                Compare();
            }
        }

        private Scene2Data Scene2LoadInternal(MemoryStream memoryStream)
        {
            var scene2Data = new Scene2Data();

            try
            {
                Scene2Parser.LoadScene(memoryStream, ref scene2Data, logging);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return scene2Data;
            }
            return scene2Data;
        }

        private void PutIntoTreeview(Scene2Data scene2Data)
        {
            // put into treeview
            treeViewMain.Nodes.Clear();

            int i = 0;

            // add header
            TreeNode objectsTreeNodeHeader = new TreeNode("Header") { Tag = scene2Data.Header.Content };
            treeViewMain.Nodes.Add(objectsTreeNodeHeader);

            foreach (Scene2Section section in scene2Data.Sections)
            {
                TreeNode objectsTreeNode = new TreeNode(section.SectionName);

                foreach (IGrouping<DncType, Dnc> item in section.Dncs.GroupBy(x => x.dncType))
                {
                    TreeNode treeNodeParent = new TreeNode(item.Key.ToString());
                    treeNodeParent.Name = item.Key.ToString();

                    if (item.Key == DncType.Unknown)
                    {
                        treeNodeParent.Text += $" {section.SectionName}";
                    }

                    i = 0;

                    List<TreeNode> nodeList = new List<TreeNode>();
                    i = AddAllItems(i, item, nodeList);

                    // sort nodes
                    nodeList = nodeList.OrderBy(x => x.Text).ToList();

                    treeNodeParent.Nodes.AddRange(nodeList.ToArray());

                    treeNodeParent.Text += $" [{nodeList.Count}]";
                    objectsTreeNode.Nodes.Add(treeNodeParent);
                }
                treeViewMain.Nodes.Add(objectsTreeNode);

            }

            logging.Add("Loading of file done.");

            static int AddAllItems(int i, IGrouping<DncType, Dnc> item, List<TreeNode> nodeList)
            {
                foreach (Dnc dnc in item)
                {
                    TreeNode treeNode = new TreeNode
                    {
                        Text = dnc.Name,
                        Tag = dnc
                    };

                    nodeList.Add(treeNode);
                    i++;
                }

                return i;
            }
        }

        private void Scene2FileSave(object sender, EventArgs e)
        {
            if (scene2FileLoaded)
            {
                var tmpStream = new FileStream(openFileDialog1.FileName, FileMode.Create);
                Scene2Parser.SaveScene(tmpStream, ref scene2Data, logging);
                tmpStream.Close();
            }
        }

        private void Scene2FileSaveAs(object sender, EventArgs e)
        {
            if (scene2FileLoaded)
            {
                if (saveFileDialogExport.ShowDialog() == DialogResult.OK)
                {
                    Scene2Parser.SaveScene(saveFileDialogExport.OpenFile(), ref scene2Data, logging);
                }
            }
        }

        #region Compare
        private void Compare()
        {
            List<string> diffs = new List<string>();
            var orderedOriginal = scene2Data.Sections.OrderBy(x => x.SectionName).ToList();
            var orderedSecond = scene2DataSecond.Sections.OrderBy(x => x.SectionName).ToList();

            var equalityComparer = new EqualityComparer();

            int len = 0;
            if (orderedOriginal.Count < orderedSecond.Count)
            {
                len = orderedOriginal.Count;
            }
            else
            {
                len = orderedSecond.Count;
            }

            for (int i = 0; i < len; i++)
            {
                diffs.Add(orderedOriginal[i].SectionName);
                diffs.Add("------------------------");

                var orderedDncs1 = orderedOriginal[i].Dncs.OrderBy(x => x.dncKind).ToList();
                var orderedDncs2 = orderedSecond[i].Dncs.OrderBy(x => x.dncKind).ToList();

                int lenDnc = 0;
                List<Dnc> longerDnc;
                
                IEnumerable<Dnc> except_result;
                if (orderedDncs1.Count < orderedDncs2.Count)
                {
                    lenDnc = orderedDncs1.Count;
                    longerDnc = orderedDncs2;
                    except_result = longerDnc.Except(orderedDncs1, equalityComparer);
                }
                else
                {
                    lenDnc = orderedDncs2.Count;
                    longerDnc = orderedDncs1;
                    except_result = longerDnc.Except(orderedDncs2, equalityComparer);
                }

                foreach (var item in except_result.OrderBy(x => x.Name))
                {
                    diffs.Add($"  {item.Name} ({item.dncType})");
                }

                /*
                for (int j = 0; j < lenDnc; j++)
                {
                    if (orderedDncs1[j].Name == orderedDncs2[j].Name)
                    {
                        if (!DncMethods.RawDataEqual(orderedDncs1[j], orderedDncs2[j]))
                        {
                            diffs.Add($"{orderedDncs1[j].Name} - different data length");
                        }
                    }
                    else
                    {
                        diffs.Add($"{orderedDncs1[j].Name} - different names");
                    }
                }

                int diffLen = Math.Abs(orderedDncs1.Count - orderedDncs2.Count);
                for (int k = lenDnc; k < lenDnc + diffLen; k++)
                {
                    diffs.Add($"{longerDnc[k].Name} - missing in the other");
                }
                */

                diffs.Add("------------------------");
            }

            if (diffs.Count > 0)
            {
                KryptonTaskDialog.Show("", "Differences found:", string.Join(Environment.NewLine, diffs), MessageBoxIcon.Information, TaskDialogButtons.OK);
            }
            else
            {
                KryptonMessageBox.Show("No difference found.");
            }
        }
        #endregion

        #region Diagram
        private void ShowScriptDependencyDiagram(object sender, EventArgs e)
        {
            if (!scene2FileLoaded)
            {
                KryptonMessageBox.Show("First open Scene2 file.");
                return;
            }

            diagramVisualizer = new DiagramVisualizer(this);
            diagramVisualizer.Show();
            diagramVisualizer.ShowScriptDependencyDiagram(scene2Data);
        }
        #endregion

        #region Def
        private void LoadDefFile(object sender, EventArgs e)
        {
            if (openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                defFilePath = openFileDialog2.FileName;
                ActiveDncs.SetElementGenerator(defFilePath);
            }
        }
        #endregion

        private void StartMafiaCon(object sender, EventArgs e)
        {
            StartExternalAppInParent("MafiaCon.exe");
        }

        private void StartGame(object sender, EventArgs e)
        {
            StartExternalAppInParent("Game.exe");
        }

        private void StartExternalAppInParent(string appName)
        {
            if (openFileDialog1.FileName != string.Empty)
            {
                string scenePath = openFileDialog1.FileName;
                scenePath = Path.GetDirectoryName(scenePath);

                while (!File.Exists(scenePath + $"\\{appName}"))
                {
                    if (scenePath.Length <= 4)
                    {
                        KryptonMessageBox.Show($"{appName} was not found in parent folder of currently opened scene file.");
                        return;
                    }
                    scenePath = Directory.GetParent(scenePath).FullName;
                }

                ProcessStartInfo processStartInfo = new ProcessStartInfo($"{scenePath}\\{appName}")
                {
                    WorkingDirectory = scenePath,
                };
                Process.Start(processStartInfo);
            }
            else
            {
                KryptonMessageBox.Show("Open scene file first.");
            }
        }
    }
}
