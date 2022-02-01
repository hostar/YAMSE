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

        readonly KryptonRibbonGroupButton kryptonRibbonGroupButtonImportDnc = new KryptonRibbonGroupButton();

        readonly KryptonRibbonGroupButton kryptonRibbonGroupButtonWorkspaceArrange = new KryptonRibbonGroupButton();

        readonly KryptonRibbonQATButton kryptonQatButtonUndo = new KryptonRibbonQATButton();

        readonly KryptonListBox listBoxOutput = new KryptonListBox();
        readonly KryptonLabel outputLabel = new KryptonLabel { Text = "Output"};

        TreeNode currentTreeNode;

        readonly OpenFileDialog openFileDialog1 = new OpenFileDialog();
        readonly SaveFileDialog saveFileDialog1 = new SaveFileDialog();

        readonly OpenFileDialog openFileDialog2 = new OpenFileDialog();
        readonly OpenFileDialog openFileDialogDnc = new OpenFileDialog();

        readonly List<string> activeDncs = new List<string>();

        readonly Color defaultColor = Color.FromArgb(221, 234, 247);

        //KryptonWorkspaceCell workspaceMain = new KryptonWorkspaceCell();

        bool scene2FileLoaded = false;

        private Scene2Data scene2Data = new Scene2Data();

        private DiagramVisualizer diagramVisualizer;

        int _maxRecentDocs = 9;

        string fNameRecent = "\\recent.list";

        private int lastFound = 0;

        private string defFilePath = string.Empty;
        private ImageElementGenerator imageElementGenerator;

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

            kryptonWorkspaceContent.ContextMenus.ShowContextMenu = false;

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
        }

        private void DuplicateDnc(object sender, EventArgs e)
        {
            var dnc = currTreeNode.Tag as Dnc;

            var newName = KryptonInputBox.Show("Enter new name", "Duplicate", dnc.Name);
            Dnc newDnc = Scene2Parser.DuplicateDnc(dnc, newName);

            var dncsInSection = scene2Data.Sections.First(x => x.SectionType == dnc.dncKind).Dncs;
            int highestIDinCat = dncsInSection.OrderBy(x => x.ID).Last().ID;
            newDnc.ID = highestIDinCat;

            dncsInSection.Add(newDnc);
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
            kryptonContextMenuItemExit});

            kryptonRibbon.RibbonTabs.AddRange(new KryptonRibbonTab[] {
            kryptonRibbonTabTools,
            kryptonRibbonTabWorkspace});

            kryptonRibbonGroupVisualization.DialogBoxLauncher = false;
            kryptonRibbonGroupVisualization.TextLine1 = "Visualization";

            kryptonRibbonGroupVisualization.Items.AddRange(new KryptonRibbonGroupContainer[] {kryptonRibbonGroupTriple1});

            kryptonRibbonGroupButtonShowDiagram.TextLine1 = "Show diagram";
            kryptonRibbonGroupButtonShowDiagram.Click += ShowScriptDependencyDiagram;

            kryptonRibbonGroupTriple1.Items.AddRange(new KryptonRibbonGroupItem[] {kryptonRibbonGroupButtonShowDiagram});

            kryptonRibbonGroupButtonLoadDefs.TextLine1 = "Load def file";
            kryptonRibbonGroupButtonLoadDefs.Click += LoadDefFile;

            kryptonRibbonGroupButtonStartGame.TextLine1 = "Start game";
            kryptonRibbonGroupButtonStartGame.Click += StartGame;

            kryptonRibbonGroupButtonStartMafiaCon.TextLine1 = "Start MafiaCon";
            kryptonRibbonGroupButtonStartMafiaCon.Click += StartMafiaCon;

            kryptonRibbonGroupButtonImportDnc.TextLine1 = "Import DNC";
            kryptonRibbonGroupButtonImportDnc.Click += ImportDnc;

            kryptonRibbonGroupTriple3.Items.AddRange(new KryptonRibbonGroupItem[] { kryptonRibbonGroupButtonImportDnc, kryptonRibbonGroupButtonLoadDefs, kryptonRibbonGroupButtonStartGame });
            kryptonRibbonGroupTriple4.Items.AddRange(new KryptonRibbonGroupItem[] { kryptonRibbonGroupButtonStartMafiaCon });

            kryptonRibbonGroupArrange.DialogBoxLauncher = false;
            kryptonRibbonGroupArrange.MinimumWidth = 200;
            kryptonRibbonGroupArrange.TextLine1 = "Arrange";

            kryptonRibbonGroupExternalData.DialogBoxLauncher = false;
            kryptonRibbonGroupExternalData.MinimumWidth = 200;
            kryptonRibbonGroupExternalData.TextLine1 = "Misc";

            kryptonRibbonGroupButtonWorkspaceArrange.Click += (sender, e) => { kryptonWorkspaceContent.ApplyGridPages(); };
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

                foreach (var item in treeViewMain.Nodes)
                {
                    if (item is TreeNode treeNode)
                    {
                        switch (currDnc.dncKind)
                        {
                            case NodeType.Object:
                                if (treeNode.Text == Scene2Parser.SectionNameObjects)
                                {
                                    CreateNewNode(currDnc, treeNode);
                                }
                                break;
                            case NodeType.Definition:
                                if (treeNode.Text == Scene2Parser.SectionNameDefs)
                                {
                                    CreateNewNode(currDnc, treeNode);
                                }
                                break;
                            case NodeType.InitScript:
                                if (treeNode.Text == Scene2Parser.SectionNameObjects)
                                {
                                    CreateNewNode(currDnc, treeNode);
                                }
                                break;
                            case NodeType.Unknown:
                                break;
                            default:
                                break;
                        }
                    }
                }

                var section = scene2Data.Sections.First(x => x.SectionType == currDnc.dncKind);

                if (section != null)
                {
                    section.Dncs.Add(currDnc);
                }
            }
        }

        private static void CreateNewNode(Dnc currDnc, TreeNode treeNode)
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
                        //pageId.ScintillaTextEditor.Undo();
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

                                if (currObjName.StartsWith(textToSearch))
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

        private KryptonPage CreatePage(Dnc dnc, PanelKind panelKind, string text = "")
        {
            string pageName = dnc.Name;

            var pageId = new KryptonPageId { Dnc = dnc, PanelKind = panelKind };

            List<KryptonPageContainer> kryptonPageContainer = new List<KryptonPageContainer>();

            TableLayoutPanel tableLayoutPanel;
            TextEditorWrapper textEditorWrapper;

            switch (panelKind)
            {
                case PanelKind.Script:
                    textEditorWrapper = CreateAvalonEdit(text, pageId, this);

                    if (imageElementGenerator != null)
                    {
                        textEditorWrapper.SetElementGenerator(imageElementGenerator);
                    }

                    pageId.TextEditor = textEditorWrapper;

                    kryptonPageContainer.Add(
                        new KryptonPageContainer
                        {
                            Column = 0,
                            ColumnSpan = 4,
                            Component = textEditorWrapper.ElementHost,
                            ComponentType = ComponentType.TextEditor,
                            RowSpan = 2
                        });

                    return CreatePageInternal(pageName, pageId, kryptonPageContainer);

                case PanelKind.Enemy:
                    textEditorWrapper = CreateAvalonEdit(text, pageId, this);

                    if (imageElementGenerator != null)
                    {
                        textEditorWrapper.SetElementGenerator(imageElementGenerator);
                    }

                    pageId.TextEditor = textEditorWrapper;

                    EnemyProps enemyProps = dnc.DncProps as EnemyProps;

                    PropertyGrid propertyGrid = new PropertyGrid() { SelectedObject = enemyProps, Dock = DockStyle.Fill };

                    kryptonPageContainer.Add(
                        new KryptonPageContainer
                        {
                            Column = 2,
                            ColumnSpan = 1,
                            Component = propertyGrid,
                            ComponentType = ComponentType.PropertyGrid,
                            RowSpan = 1
                        });
                    kryptonPageContainer.Add(
                        new KryptonPageContainer
                        {
                            Column = 0,
                            ColumnSpan = 2,
                            Component = textEditorWrapper.ElementHost,
                            ComponentType = ComponentType.TextEditor,
                            RowSpan = 1
                        });

                    return CreatePageInternal(pageName, pageId, kryptonPageContainer);

                case PanelKind.Hex:

                    var hexEditor = new WpfHexaEditor.HexEditor
                    {
                        ForegroundSecondColor = System.Windows.Media.Brushes.Blue,
                        TypeOfCharacterTable = WpfHexaEditor.Core.CharacterTableType.Ascii
                    };

                    var elementHostHexEditor = new System.Windows.Forms.Integration.ElementHost
                    {
                        Location = new Point(250, 50),
                        Size = new Size(1000, 500),
                        Dock = DockStyle.Fill
                    };
                    elementHostHexEditor.Name = nameof(elementHostHexEditor);
                    elementHostHexEditor.Child = hexEditor;

                    var tmpStream = new MemoryStream();
                    new MemoryStream(dnc.RawData).CopyTo(tmpStream); // needed in order to allow expanding
                    hexEditor.Stream = tmpStream;

                    pageId.HexEditor = hexEditor;

                    kryptonPageContainer.Add(
                        new KryptonPageContainer
                        {
                            Column = 0,
                            ColumnSpan = 3,
                            Component = elementHostHexEditor,
                            ComponentType = ComponentType.HexEditor,
                            RowSpan = 1
                        });

                    return CreatePageInternal(pageName, pageId, kryptonPageContainer);

                case PanelKind.Standard:
                    CreateDefaultTextBoxes(kryptonPageContainer, pageId, dnc);

                    tableLayoutPanel = new TableLayoutPanel
                    {
                        BackColor = Color.FromArgb(187, 206, 230), //defaultColor,
                        ColumnCount = 6,
                        RowCount = 3
                    };
                    tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
                    tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
                    tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

                    tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20));
                    tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));
                    tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20));
                    tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));
                    tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20));
                    tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));

                    return CreatePageInternal(pageName, pageId, kryptonPageContainer, tableLayoutPanel);

                case PanelKind.Model:
                    CreateDefaultTextBoxes(kryptonPageContainer, pageId, dnc);

                    ModelProps modelProps = dnc.DncProps as ModelProps;

                    KryptonPanel kryptonPanel = new KryptonPanel { Top = 500, Left = 0, Size = new Size(500, 200) };

                    List<KryptonPageContainer> kryptonPageContainer2 = new List<KryptonPageContainer>();
                    int row = 0;
                    int col = 0;
                    CreateLabel(kryptonPageContainer2, col, row, 1, "Sector");

                    col++;
                    CreateCheckBox(kryptonPageContainer2, col, row, string.Empty, modelProps.HaveSector,
                        (o, control) => { modelProps.HaveSector = (bool)o; }, (prop, control) => { (control as CheckBox).Checked = (prop as ModelProps).HaveSector; }, width: 16);

                    col++;
                    CreateTextBox(kryptonPageContainer2, col, row, modelProps.Sector, 
                        (o, control) => { modelProps.Sector = o.ToString(); }, (prop, control) => { control.Text = (prop as ModelProps).Sector.ToString(); }, 3, 278);

                    row++;
                    col = 0;
                    CreateLabel(kryptonPageContainer2, col, row, 1, "Model");

                    col++;
                    CreateTextBox(kryptonPageContainer2, col, row, modelProps.Model, 
                        (o, control) => { modelProps.Model = o.ToString(); }, (prop, control) => { control.Text = (prop as ModelProps).Model.ToString(); }, 3, 300);

                    TableLayoutPanel tableLayoutOptionalPanel = new TableLayoutPanel
                    {
                        BackColor = Color.FromArgb(187, 206, 230),
                        ColumnCount = 8,
                        RowCount = 2,
                        Dock = DockStyle.Fill
                    };

                    PutOnTableLayout(kryptonPageContainer2, tableLayoutOptionalPanel);
                    kryptonPanel.Controls.Add(tableLayoutOptionalPanel);

                    tableLayoutPanel = new TableLayoutPanel
                    {
                        BackColor = Color.FromArgb(187, 206, 230),
                        ColumnCount = 6,
                        RowCount = 5
                    };
                    tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
                    tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
                    tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

                    tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20));
                    tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));
                    tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20));
                    tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));
                    tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20));
                    tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));

                    return CreatePageInternal(pageName, pageId, kryptonPageContainer, tableLayoutPanel, kryptonPanel);

                default:
                    throw new InvalidOperationException(nameof(CreatePage));
            }

            static void CreateLabel(List<KryptonPageContainer> kryptonPageContainer, int col, int row, int colSpan, string text)
            {
                kryptonPageContainer.Add(
                                        new KryptonPageContainer
                                        {
                                            Column = col,
                                            ColumnSpan = colSpan,
                                            Component = new KryptonLabel() { Text = text },
                                            ComponentType = ComponentType.Label,
                                            RowSpan = 1,
                                            Row = row
                                        });
            }

            static void CreateTextBox(List<KryptonPageContainer> kryptonPageContainer, int col, int row, string init, CallbackSetPropValue setterFunction, CallbackSetComponentValue componentValueFunction, int colSpan = 1, int width = 100)
            {
                var textBox = new KryptonTextBox() { Text = init, Width = width };
                textBox.TextChanged += (sender, e) => 
                {

                    if (!string.IsNullOrWhiteSpace(textBox.Text))
                    {
                        setterFunction(textBox.Text, textBox);
                    }
                };

                kryptonPageContainer.Add(
                                        new KryptonPageContainer
                                        {
                                            Column = col,
                                            ColumnSpan = colSpan,
                                            Component = textBox,
                                            ComponentType = ComponentType.CheckBox,
                                            SetComponentValue = componentValueFunction,
                                            RowSpan = 1,
                                            Row = row
                                        });
            }

            static void CreateCheckBox(List<KryptonPageContainer> kryptonPageContainer, int col, int row, string initText, bool isChecked, CallbackSetPropValue setterFunction, CallbackSetComponentValue componentValueFunction, int colSpan = 1, int width = 100)
            {
                var checkBox = new CheckBox() { Text = initText, Checked = isChecked, Width = width };
                checkBox.CheckedChanged += (sender, e) =>
                {
                    setterFunction(checkBox.Checked, checkBox);
                };

                kryptonPageContainer.Add(
                                        new KryptonPageContainer
                                        {
                                            Column = col,
                                            ColumnSpan = colSpan,
                                            Component = checkBox,
                                            ComponentType = ComponentType.CheckBox,
                                            SetComponentValue = componentValueFunction,
                                            RowSpan = 1,
                                            Row = row
                                        });
            }

            static void AddAsterisk(Control control)
            {
                if (!control.Text.EndsWith('*'))
                {
                    control.Text += '*';
                }
            }

            static void CreateDefaultTextBoxes(List<KryptonPageContainer> kryptonPageContainer, KryptonPageId pageId, Dnc dnc)
            {
                int col = 0;
                int row = 0;

                StandardProps standardProps = dnc.DncProps as StandardProps;

                // position
                CreateLabel(kryptonPageContainer, col, row, 2, "Position");

                row++;
                CreateLabel(kryptonPageContainer, col, row, 1, "X");

                row++;

                CreateLabel(kryptonPageContainer, col, row, 1, "Y");

                row++;

                CreateLabel(kryptonPageContainer, col, row, 1, "Z");

                col++;
                row = 1;

                CreateTextBox(kryptonPageContainer, col, row, standardProps.PositionX.ToString(), (o, control) => { standardProps.PositionX = Convert.ToSingle(o); AddAsterisk(pageId.KryptonPage); }, (prop, control) => { control.Text = (prop as StandardProps).PositionX.ToString(); });

                row++;

                CreateTextBox(kryptonPageContainer, col, row, standardProps.PositionY.ToString(), (o, control) => { standardProps.PositionY = Convert.ToSingle(o); AddAsterisk(pageId.KryptonPage); }, (prop, control) => { control.Text = (prop as StandardProps).PositionY.ToString(); });

                row++;

                CreateTextBox(kryptonPageContainer, col, row, standardProps.PositionZ.ToString(), (o, control) => { standardProps.PositionZ = Convert.ToSingle(o); AddAsterisk(pageId.KryptonPage); }, (prop, control) => { control.Text = (prop as StandardProps).PositionZ.ToString(); });

                col++;
                row = 0;

                // rotation
                CreateLabel(kryptonPageContainer, col, row, 2, "Rotation");

                row++;
                CreateLabel(kryptonPageContainer, col, row, 1, "X");

                row++;
                CreateLabel(kryptonPageContainer, col, row, 1, "Y");

                row++;
                CreateLabel(kryptonPageContainer, col, row, 1, "Z");

                col++;

                row = 1;
                CreateTextBox(kryptonPageContainer, col, row, standardProps.RotationX.ToString(), (o, control) => { standardProps.RotationX = Convert.ToSingle(o); AddAsterisk(pageId.KryptonPage); }, (prop, control) => { control.Text = (prop as StandardProps).RotationX.ToString(); });

                row++;
                CreateTextBox(kryptonPageContainer, col, row, standardProps.RotationY.ToString(), (o, control) => { standardProps.RotationY = Convert.ToSingle(o); AddAsterisk(pageId.KryptonPage); }, (prop, control) => { control.Text = (prop as StandardProps).RotationY.ToString(); });

                row++;
                CreateTextBox(kryptonPageContainer, col, row, standardProps.RotationZ.ToString(), (o, control) => { standardProps.RotationZ = Convert.ToSingle(o); AddAsterisk(pageId.KryptonPage); }, (prop, control) => { control.Text = (prop as StandardProps).RotationZ.ToString(); });

                col++;
                row = 0;

                // scaling
                CreateLabel(kryptonPageContainer, col, row, 2, "Scaling");

                row++;
                CreateLabel(kryptonPageContainer, col, row, 1, "X");

                row++;
                CreateLabel(kryptonPageContainer, col, row, 1, "Y");

                row++;
                CreateLabel(kryptonPageContainer, col, row, 1, "Z");

                col++;

                row = 1;
                CreateTextBox(kryptonPageContainer, col, row, standardProps.ScalingX.ToString(), (o, control) => { standardProps.ScalingX = Convert.ToSingle(o); AddAsterisk(pageId.KryptonPage); }, (prop, control) => { control.Text = (prop as StandardProps).ScalingX.ToString(); });

                row++;
                CreateTextBox(kryptonPageContainer, col, row, standardProps.ScalingY.ToString(), (o, control) => { standardProps.ScalingY = Convert.ToSingle(o); AddAsterisk(pageId.KryptonPage); }, (prop, control) => { control.Text = (prop as StandardProps).ScalingY.ToString(); });

                row++;
                CreateTextBox(kryptonPageContainer, col, row, standardProps.ScalingZ.ToString(), (o, control) => { standardProps.ScalingZ = Convert.ToSingle(o); AddAsterisk(pageId.KryptonPage); }, (prop, control) => { control.Text = (prop as StandardProps).ScalingZ.ToString(); });
            }
        }

        private static TextEditorWrapper CreateAvalonEdit(string text, KryptonPageId pageId, Form parentForm)
        {
            ICSharpCode.AvalonEdit.TextEditor avalonEdit = new ICSharpCode.AvalonEdit.TextEditor();
            avalonEdit.Text = text;

            var avalonEditElementHost = new System.Windows.Forms.Integration.ElementHost
            {
                Dock = DockStyle.Fill,
                Child = avalonEdit,
                Anchor = AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top | AnchorStyles.Right
            };
            avalonEditElementHost.Name = nameof(avalonEditElementHost);
            avalonEditElementHost.Parent = parentForm;

            RichTextModel richTextModel = new RichTextModel();
            RichTextColorizer richTextColorizer = new RichTextColorizer(richTextModel);
            
            avalonEdit.TextArea.TextView.LineTransformers.Add(richTextColorizer);
            avalonEdit.ShowLineNumbers = true;

            SearchPanel.Install(avalonEdit);

            //HighlightingEngine highlightingEngine = new HighlightingEngine(highlightingRuleSet);


            //avalonEdit.SyntaxHighlighting.MainRuleSet.Rules.Add(new HighlightingRule() { Color = red, Regex = new System.Text.RegularExpressions.Regex("dim_flt") });

            avalonEdit.TextChanged += (sender, eargs) =>
            {
                if (!pageId.IsDirty)
                {
                    pageId.KryptonPage.Text += "*";
                    pageId.IsDirty = true;
                }

                //richTextModel.ApplyHighlighting(0, 20, new HighlightingColor() { Foreground = new SimpleHighlightingBrush((SWM.Color)SWM.ColorConverter.ConvertFromString("Red")) });
                //avalonEdit.TextArea.TextView.Redraw(0, 20);

                //avalonEdit.TextArea.TextView.InvalidateLayer(ICSharpCode.AvalonEdit.Rendering.KnownLayer.Caret);
                //avalonEdit.Document.Lines

                //DncMethods.ScintillaTextHighlight(scintillaTextEditor.Lines[scintillaTextEditor.LineFromPosition(scintillaTextEditor.CurrentPosition)].Text, scintillaTextEditor.CurrentPosition, scintillaTextEditor);

            };

            //avalonEdit.Document.

            //HighlightingManager.Instance.RegisterHighlighting("mafiascript", new string[] { ".bin" }, new MafiaHighlight());
            avalonEdit.SyntaxHighlighting = new MafiaHighlight();

            //avalonEdit.TextArea.TextView.BackgroundRenderers
            //avalonEdit.TextArea.TextView.ElementGenerators.
            //avalonEdit.

            //parentForm.Controls.Add(avalonEditElementHost);

            //avalonEditElementHost.Show();
            //avalonEditElementHost.BringToFront();
            return new TextEditorWrapper { Editor = avalonEdit, ElementHost = avalonEditElementHost };
        }

        private KryptonPage CreatePageInternal(string pageName, KryptonPageId pageId, IEnumerable<KryptonPageContainer> mainComponents, TableLayoutPanel tableLayoutPanel = null, KryptonPanel optionalPanel = null)
        {
            TableLayoutPanel kryptonBasePanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(187, 206, 230), //defaultColor,
                ColumnCount = 1,
                RowCount = 3
            };

            bool createdDefault = false;
            if (tableLayoutPanel == null)
            {
                createdDefault = true;
                tableLayoutPanel = CreateDefaultLayout();
            }

            if (optionalPanel == null)
            {
                //tableLayoutPanel.Size = new Size(1200, 500);

                if (createdDefault)
                {
                    kryptonBasePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 80));
                    kryptonBasePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
                }
                else
                {
                    kryptonBasePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 60));
                    kryptonBasePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 40));
                }
            }
            else
            {
                //tableLayoutPanel.Size = new Size(800, 300);
                kryptonBasePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
                kryptonBasePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
                kryptonBasePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 30));
            }

            kryptonBasePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            pageId.KryptonPageContainer = mainComponents;

            // Create a new page and give it a name and image
            KryptonPage page = new KryptonPage
            {
                Text = pageName,
                TextTitle = pageName,
                TextDescription = pageName,
                Tag = pageId,
                //page.ImageSmall = imageList.Images[_count % imageList.Images.Count];
                MinimumSize = new Size(200, 250)
            };

            pageId.KryptonPage = page;

            PutOnTableLayout(mainComponents, tableLayoutPanel);

            tableLayoutPanel.RowCount++;
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

            tableLayoutPanel.Dock = DockStyle.Fill;
            tableLayoutPanel.Location = new Point(0, 0);

            tableLayoutPanel.Name = nameof(tableLayoutPanel);
            tableLayoutPanel.TabIndex = 0;

            // Add rich text box as the contents of the page
            kryptonBasePanel.Padding = new Padding(5);
            
            kryptonBasePanel.Controls.Add(tableLayoutPanel, 0, 0);

            int kryptonButtonPanelRow = 2;
            if (optionalPanel != null)
            {
                kryptonBasePanel.Controls.Add(optionalPanel, 0, 1);
            }
            else
            {
                kryptonButtonPanelRow = 1;
            }

            KryptonPanel kryptonButtonPanel = new KryptonPanel() { Dock = DockStyle.Fill, BackColor = defaultColor, ForeColor = defaultColor };
            var btnSave = CreateButton(pageId, DncMethods.BtnSaveClick, "Save", 0, 50);
            kryptonButtonPanel.Controls.Add(btnSave);

            var btnRevert = CreateButton(pageId, DncMethods.BtnRevertClick, "Revert", 150, 50);
            kryptonButtonPanel.Controls.Add(btnRevert);

            kryptonBasePanel.Controls.Add(kryptonButtonPanel, 0, kryptonButtonPanelRow);

            page.Controls.Add(kryptonBasePanel);

            // Create a close button for the page
            ButtonSpecAny bsa = new ButtonSpecAny
            {
                Tag = pageId,
                Type = PaletteButtonSpecStyle.Close
            };
            bsa.Click += PageClose;
            page.ButtonSpecs.Add(bsa);

            //workspaceMain.Pages.Add(page);
            kryptonWorkspaceContent.FirstCell().Pages.Add(page);
            return page;
        }

        private static void PutOnTableLayout(IEnumerable<KryptonPageContainer> mainComponents, TableLayoutPanel tableLayoutPanel)
        {
            if (mainComponents == null)
            {
                return;
            }
            foreach (var pageContainer in mainComponents)
            {
                tableLayoutPanel.Controls.Add(pageContainer.Component, pageContainer.Column, pageContainer.Row);

                tableLayoutPanel.SetColumnSpan(pageContainer.Component, pageContainer.ColumnSpan);
                tableLayoutPanel.SetRowSpan(pageContainer.Component, pageContainer.RowSpan);
            }
        }

        private TableLayoutPanel CreateDefaultLayout()
        {
            TableLayoutPanel tableLayoutPanel1 = new TableLayoutPanel
            {
                BackColor = defaultColor,
                ColumnCount = 3,
                RowCount = 1
            };
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 114F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300F));

            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            return tableLayoutPanel1;
        }

        private Control CreateButton(KryptonPageId kryptonPageId, EventHandler eventHandler, string btnName, int x, int y)
        {
            KryptonButton btnObj = new KryptonButton
            {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                Location = new Point(x, y),
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
            var pageId = (sender as ButtonSpecAny).Tag as KryptonPageId;

            if (pageId.IsDirty)
            {
                if (KryptonMessageBox.Show("Close without saving?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    return;
                }
            }

            var page = pageId.KryptonPage;
            
            activeDncs.Remove(page.Tag.ToString());
            try
            {
                KryptonWorkspaceCell currCell = kryptonWorkspaceContent.FirstCell();
                while(!currCell.Pages.Contains(page))
                {
                    currCell = kryptonWorkspaceContent.NextCell(currCell);
                }
                currCell.Pages.Remove(page);
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void CloseAllPages()
        {
            KryptonWorkspaceCell currCell = kryptonWorkspaceContent.FirstCell();
            while (kryptonWorkspaceContent.NextCell(currCell) != null)
            {
                currCell.Pages.Clear();
                currCell = kryptonWorkspaceContent.NextCell(currCell);
            }
            currCell.Pages.Clear();

            activeDncs.Clear();
        }

        internal void SelectedObjectChanged(TreeNode e)
        {
            Dnc dnc;
            string currId = string.Empty;

            currentTreeNode = e;

            if (e.Tag != null)
            {
                dnc = e.Tag as Dnc;
                var nodeType = ((Dnc)e.Tag).dncKind;
                switch (nodeType)
                {
                    case NodeType.Object:
                        currId = DncMethods.CreatePageID(dnc);

                        if (activeDncs.Any(x => x == currId))
                        {
                            return;
                        }

                        switch (dnc.dncType)
                        {
                            case DncType.Standard:
                                activeDncs.Add(currId);
                                CreatePage(dnc, PanelKind.Standard);
                                break;
                            case DncType.Model:
                                activeDncs.Add(currId);
                                CreatePage(dnc, PanelKind.Model);
                                break;
                            default:
                                activeDncs.Add(currId);
                                CreatePage(dnc, PanelKind.Hex);
                                break;
                        }

                        break;
                    case NodeType.Definition:
                        currId = DncMethods.CreatePageID(dnc);

                        if (activeDncs.Any(x => x == currId))
                        {
                            return;
                        }

                        switch (dnc.dncType)
                        {
                            case DncType.Script:
                                //elementHostHexEditor.Hide();
                                activeDncs.Add(currId);
                                CreatePage(dnc, PanelKind.Script, Scene2Parser.GetScriptFromDnc(dnc));
                                break;
                            case DncType.Enemy:
                                activeDncs.Add(currId);
                                CreatePage(dnc, PanelKind.Enemy, Scene2Parser.GetScriptFromDnc(dnc));
                                break;
                            case DncType.PhysicalObject:
                            case DncType.Door:
                            case DncType.Tram:
                            case DncType.GasStation:
                            case DncType.PedestrianSetup:
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
                        currId = DncMethods.CreatePageID(dnc);

                        if (activeDncs.Any(x => x == currId))
                        {
                            return;
                        }

                        activeDncs.Add(currId);
                        CreatePage(dnc, PanelKind.Script, Scene2Parser.GetScriptFromDnc(dnc));
                        break;
                    default:
                        dnc = scene2Data.Sections.Where(x => x.SectionType == NodeType.Unknown).SelectMany(x => x.Dncs).Where(x => x.ID == ((Dnc)e.Tag).ID).FirstOrDefault();
                        currId = DncMethods.CreatePageID(dnc);

                        if (activeDncs.Any(x => x == currId))
                        {
                            return;
                        }

                        activeDncs.Add(currId);
                        CreatePage(dnc, PanelKind.Hex);
                        break;
                }

                treeViewMain.Focus();
            }
        }

        internal void ExportDnc(object sender, EventArgs e)
        {
            var dnc = currTreeNode.Tag as Dnc;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllBytes(saveFileDialog1.FileName, dnc.RawData);
            }
        }

        private void RenameDnc(object sender, EventArgs e)
        {
            var dnc = currTreeNode.Tag as Dnc;

            var newName = KryptonInputBox.Show("Enter new name", "Rename", dnc.Name);
            Scene2Parser.RenameDnc(dnc, newName);

            currTreeNode.Text = newName;
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
                            listBoxOutput.Items.Add($"Error when opening file: {ex.Message}");
                            return;
                        }
                        
                        openFileDialog1.FileName = recent.Text;

                        tmpStream.CopyTo(memoryStream);
                        tmpStream.Close();

                        Scene2LoadInternal(memoryStream);

                        CloseAllPages();

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
                listBoxOutput.Items.Add("Loading file...");

                MemoryStream memoryStream = new MemoryStream();
                Stream tmpStream = openFileDialog1.OpenFile();
                tmpStream.CopyTo(memoryStream);
                tmpStream.Close();

                Scene2LoadInternal(memoryStream);

                CloseAllPages();

                scene2FileLoaded = true;

                AddRecentFile(openFileDialog1.FileName);
            }
        }

        private void Scene2LoadInternal(MemoryStream memoryStream)
        {
            scene2Data = new Scene2Data();

            try
            {
                Scene2Parser.LoadScene(memoryStream, ref scene2Data, listBoxOutput.Items);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // put into treeview
            treeViewMain.Nodes.Clear();

            int i = 0;

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

                    // sort nodes
                    nodeList = nodeList.OrderBy(x => x.Text).ToList();

                    treeNodeParent.Nodes.AddRange(nodeList.ToArray());

                    treeNodeParent.Text += $" [{nodeList.Count}]";
                    objectsTreeNode.Nodes.Add(treeNodeParent);
                }
                treeViewMain.Nodes.Add(objectsTreeNode);

            }

            listBoxOutput.Items.Add("Loading of file done.");
        }

        private void Scene2FileSave(object sender, EventArgs e)
        {
            if (scene2FileLoaded)
            {
                var tmpStream = new FileStream(openFileDialog1.FileName, FileMode.Create);
                Scene2Parser.SaveScene(tmpStream, ref scene2Data, listBoxOutput.Items);
                tmpStream.Close();
            }
        }

        private void Scene2FileSaveAs(object sender, EventArgs e)
        {
            if (scene2FileLoaded)
            {
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    Scene2Parser.SaveScene(saveFileDialog1.OpenFile(), ref scene2Data, listBoxOutput.Items);
                }
            }
        }

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
                imageElementGenerator = new ImageElementGenerator(defFilePath);
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
