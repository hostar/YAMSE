using ComponentFactory.Krypton.Navigator;
using ComponentFactory.Krypton.Ribbon;
using ComponentFactory.Krypton.Toolkit;
using ComponentFactory.Krypton.Workspace;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Input;
using YAMSE.DataLayer;
using YAMSE.Diagram.Classes;

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
        readonly KryptonContextMenuItem kryptonContextMenuItem1 = new KryptonContextMenuItem();
        readonly KryptonContextMenuItem kryptonContextMenuItem2 = new KryptonContextMenuItem();
        readonly KryptonContextMenuItem kryptonContextMenuItem3 = new KryptonContextMenuItem();
        readonly KryptonContextMenuItem kryptonContextMenuItem4 = new KryptonContextMenuItem();
        readonly KryptonContextMenuItem kryptonContextMenuItemExit = new KryptonContextMenuItem();

        readonly KryptonContextMenuSeparator kryptonContextMenuSeparator1 = new KryptonContextMenuSeparator();

        readonly KryptonRibbonTab kryptonRibbonTab1 = new KryptonRibbonTab();
        readonly KryptonRibbonGroup kryptonRibbonGroup1 = new KryptonRibbonGroup();
        readonly KryptonRibbonGroupTriple kryptonRibbonGroupTriple1 = new KryptonRibbonGroupTriple();

        readonly KryptonRibbonTab kryptonRibbonTab2 = new KryptonRibbonTab();
        readonly KryptonRibbonGroup kryptonRibbonGroup2 = new KryptonRibbonGroup();
        readonly KryptonRibbonGroupTriple kryptonRibbonGroupTriple2 = new KryptonRibbonGroupTriple();

        readonly KryptonRibbonGroupButton kryptonRibbonGroupButtonShowDiagram = new KryptonRibbonGroupButton();
        readonly KryptonRibbonGroupButton kryptonRibbonGroupButtonWorkspaceArrange = new KryptonRibbonGroupButton();

        readonly KryptonRibbonQATButton kryptonQatButtonUndo = new KryptonRibbonQATButton();

        readonly KryptonListBox listBoxOutput = new KryptonListBox();
        readonly KryptonLabel outputLabel = new KryptonLabel { Text = "Output"};

        TreeNode currentTreeNode;

        readonly OpenFileDialog openFileDialog1 = new OpenFileDialog();
        readonly SaveFileDialog saveFileDialog1 = new SaveFileDialog();

        readonly List<string> activeDncs = new List<string>();

        //KryptonWorkspaceCell workspaceMain = new KryptonWorkspaceCell();

        bool scene2FileLoaded = false;

        private Scene2Data scene2Data = new Scene2Data();

        private DiagramVisualizer diagramVisualizer;

        int _maxRecentDocs = 9;

        string fNameRecent = "\\recent.list";

        readonly Color defaultColor = Color.FromArgb(221, 234, 247);

        private int lastFound = 0;

        private KryptonContextMenu treeViewMenu = new KryptonContextMenu();

        private TreeNode currTreeNode;

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

            KryptonContextMenuItem duplicateContextMenuItem = new KryptonContextMenuItem("Duplicate (not implemented yet)") { Enabled = false };
            //floatingItem.Click += new EventHandler(OnDropDownFloatingClicked);
            options.Items.Add(duplicateContextMenuItem);

            splitContainerInner.Dock = DockStyle.Fill;
            splitContainerInner.SeparatorStyle = SeparatorStyle.HighProfile;

            Label kryptonLabelSearch = new Label { Text = "Search:", AutoSize = false, Dock = DockStyle.None };
            kryptonLabelSearch.Font = new Font("Segoe UI", 6.5F, GraphicsUnit.Point);

            KryptonTextBox kryptonTextBoxSearch = new KryptonTextBox() { Dock = DockStyle.Fill };
            kryptonTextBoxSearch.TextChanged += (sender, e) => { lastFound = 0; };

            KryptonButton kryptonButtonSearch = new KryptonButton() { Width = 25 };
            kryptonButtonSearch.Values.Image = Resources.FindSmall;

            kryptonButtonSearch.Click += SearchButtonClick;
            kryptonButtonSearch.Tag = kryptonTextBoxSearch;

            kryptonTextBoxSearch.KeyPress += (sender, e) => {
                if (e.KeyChar == 13)
                {
                    SearchButtonClick(kryptonButtonSearch, null);
                    kryptonTextBoxSearch.Focus();
                }
            };

            TableLayoutPanel tableLayoutPanelTreeView = new TableLayoutPanel
            {
                BackColor = Color.FromArgb(221, 234, 247),
                ColumnCount = 3,
                RowCount = 2,
                Dock = DockStyle.Fill,
                Width = 300
            };
            tableLayoutPanelTreeView.RowStyles.Add(new RowStyle(SizeType.Percent, 90));
            tableLayoutPanelTreeView.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

            tableLayoutPanelTreeView.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50));
            tableLayoutPanelTreeView.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80));
            tableLayoutPanelTreeView.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 30));

            tableLayoutPanelTreeView.Controls.Add(kryptonWorkspaceTreeView, 0, 0);

            tableLayoutPanelTreeView.Controls.Add(kryptonLabelSearch, 0, 1);
            tableLayoutPanelTreeView.Controls.Add(kryptonTextBoxSearch, 1, 1);
            tableLayoutPanelTreeView.Controls.Add(kryptonButtonSearch, 2, 1);

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

            kryptonRibbonTab1.Text = "Tools";
            kryptonRibbonTab2.Text = "Workspace";

            kryptonRibbon.RibbonAppButton.AppButtonMenuItems.AddRange(new KryptonContextMenuItemBase[] {
            /*kryptonContextMenuItem1,*/
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

            kryptonRibbonTab1.Groups.AddRange(new KryptonRibbonGroup[] {kryptonRibbonGroup1});

            kryptonRibbonGroup1.Items.AddRange(new KryptonRibbonGroupContainer[] {kryptonRibbonGroupTriple1});

            kryptonRibbonGroupButtonShowDiagram.TextLine1 = "Show diagram";
            kryptonRibbonGroupButtonShowDiagram.Click += ShowScriptDependencyDiagram;

            kryptonRibbonGroupTriple1.Items.AddRange(new KryptonRibbonGroupItem[] {kryptonRibbonGroupButtonShowDiagram});

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

            var fullPathRecent = Directory.GetCurrentDirectory() + fNameRecent;

            if (File.Exists(fullPathRecent))
            {
                foreach (var path in File.ReadAllLines(fullPathRecent))
                {
                    AddRecentFile(path);
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
                        pageId.ScintillaTextEditor.Undo();
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

        private void SearchButtonClick(object sender, EventArgs e)
        {
            KryptonTextBox kryptonTextBoxSearch = (KryptonTextBox)(sender as KryptonButton).Tag;

            if (kryptonTextBoxSearch.Text == string.Empty)
            {
                return;
            }

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
                                if ((node2 as TreeNode).Text.StartsWith(kryptonTextBoxSearch.Text))
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

            return page;
        }

        private KryptonPage CreatePage(Dnc dnc, PanelKind panelKind, string text = "")
        {
            string pageName = dnc.Name;

            var pageId = new KryptonPageId { Dnc = dnc, PanelKind = panelKind };

            List<KryptonPageContainer> kryptonPageContainer = new List<KryptonPageContainer>();

            Scintilla scintillaTextEditor;

            TableLayoutPanel tableLayoutPanel;

            switch (panelKind)
            {
                case PanelKind.Script:
                    scintillaTextEditor = CreateScintilla(text, pageId);

                    pageId.ScintillaTextEditor = scintillaTextEditor;

                    kryptonPageContainer.Add(
                        new KryptonPageContainer
                        {
                            Column = 0,
                            ColumnSpan = 4,
                            Component = scintillaTextEditor,
                            ComponentType = ComponentType.TextEditor,
                            RowSpan = 2
                        });

                    return CreatePageInternal(pageName, pageId, kryptonPageContainer);

                case PanelKind.Enemy:
                    scintillaTextEditor = CreateScintilla(text, pageId);

                    pageId.ScintillaTextEditor = scintillaTextEditor;

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
                            Component = scintillaTextEditor,
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
                    CreateDefaultTextBoxes(kryptonPageContainer, dnc);

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
                    CreateDefaultTextBoxes(kryptonPageContainer, dnc);

                    ModelProps modelProps = dnc.DncProps as ModelProps;

                    KryptonPanel kryptonPanel = new KryptonPanel { Top = 500, Left = 0, Size = new Size(500, 200) };

                    List<KryptonPageContainer> kryptonPageContainer2 = new List<KryptonPageContainer>();
                    int row = 0;
                    int col = 0;
                    CreateLabel(kryptonPageContainer2, col, row, 1, "Sector");

                    col++;
                    CreateCheckBox(kryptonPageContainer2, col, row, string.Empty, modelProps.HaveSector,
                        (o) => { modelProps.HaveSector = (bool)o; }, (prop, control) => { (control as CheckBox).Checked = (prop as ModelProps).HaveSector; }, width: 16);

                    col++;
                    CreateTextBox(kryptonPageContainer2, col, row, modelProps.Sector, 
                        (o) => { modelProps.Sector = o.ToString(); }, (prop, control) => { control.Text = (prop as ModelProps).Sector.ToString(); }, 3, 278);

                    row++;
                    col = 0;
                    CreateLabel(kryptonPageContainer2, col, row, 1, "Model");

                    col++;
                    CreateTextBox(kryptonPageContainer2, col, row, modelProps.Model, 
                        (o) => { modelProps.Model = o.ToString(); }, (prop, control) => { control.Text = (prop as ModelProps).Model.ToString(); }, 3, 300);

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
                        setterFunction(textBox.Text);
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
                    setterFunction(checkBox.Checked);
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

            static void CreateDefaultTextBoxes(List<KryptonPageContainer> kryptonPageContainer, Dnc dnc)
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

                CreateTextBox(kryptonPageContainer, col, row, standardProps.PositionX.ToString(), (o) => { standardProps.PositionX = Convert.ToSingle(o); }, (prop, control) => { control.Text = (prop as StandardProps).PositionX.ToString(); });

                row++;

                CreateTextBox(kryptonPageContainer, col, row, standardProps.PositionY.ToString(), (o) => { standardProps.PositionY = Convert.ToSingle(o); }, (prop, control) => { control.Text = (prop as StandardProps).PositionY.ToString(); });

                row++;

                CreateTextBox(kryptonPageContainer, col, row, standardProps.PositionZ.ToString(), (o) => { standardProps.PositionZ = Convert.ToSingle(o); }, (prop, control) => { control.Text = (prop as StandardProps).PositionZ.ToString(); });

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
                CreateTextBox(kryptonPageContainer, col, row, standardProps.RotationX.ToString(), (o) => { standardProps.RotationX = Convert.ToSingle(o); }, (prop, control) => { control.Text = (prop as StandardProps).RotationX.ToString(); });

                row++;
                CreateTextBox(kryptonPageContainer, col, row, standardProps.RotationY.ToString(), (o) => { standardProps.RotationY = Convert.ToSingle(o); }, (prop, control) => { control.Text = (prop as StandardProps).RotationY.ToString(); });

                row++;
                CreateTextBox(kryptonPageContainer, col, row, standardProps.RotationZ.ToString(), (o) => { standardProps.RotationZ = Convert.ToSingle(o); }, (prop, control) => { control.Text = (prop as StandardProps).RotationZ.ToString(); });

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
                CreateTextBox(kryptonPageContainer, col, row, standardProps.ScalingX.ToString(), (o) => { standardProps.ScalingX = Convert.ToSingle(o); }, (prop, control) => { control.Text = (prop as StandardProps).ScalingX.ToString(); });

                row++;
                CreateTextBox(kryptonPageContainer, col, row, standardProps.ScalingY.ToString(), (o) => { standardProps.ScalingY = Convert.ToSingle(o); }, (prop, control) => { control.Text = (prop as StandardProps).ScalingY.ToString(); });

                row++;
                CreateTextBox(kryptonPageContainer, col, row, standardProps.ScalingZ.ToString(), (o) => { standardProps.ScalingZ = Convert.ToSingle(o); }, (prop, control) => { control.Text = (prop as StandardProps).ScalingZ.ToString(); });
            }
        }

        private static Scintilla CreateScintilla(string text, KryptonPageId pageId)
        {
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
                pageId.IsDirty = true;
                DncMethods.ScintillaTextHighlight(scintillaTextEditor.Lines[scintillaTextEditor.LineFromPosition(scintillaTextEditor.CurrentPosition)].Text, scintillaTextEditor.CurrentPosition, scintillaTextEditor);
            };
            return scintillaTextEditor;
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

            /*
            if (currentTreeNode?.GetHashCode() == e.GetHashCode())
            {
                return;
            }
            */
            currentTreeNode = e;

            if (e.Tag != null)
            {
                var nodeType = ((Dnc)e.Tag).dncKind;
                switch (nodeType)
                {
                    case NodeType.Object:
                        //elementHostHexEditor.Show();
                        //elementHostDiagramEditor.Hide();
                        //hexEditor.Stream = new MemoryStream(scene2Data.objectsDncs.Where(x => x.ID == ((NodeTag)e.Tag).id).FirstOrDefault().rawData);

                        dnc = scene2Data.Sections.First(x => x.SectionType == NodeType.Object).Dncs.Where(x => x.ID == ((Dnc)e.Tag).ID).FirstOrDefault();
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

                        dnc = scene2Data.Sections.First(x => x.SectionType == NodeType.Definition).Dncs.Where(x => x.ID == ((Dnc)e.Tag).ID).FirstOrDefault();
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
                        dnc = scene2Data.Sections.First(x => x.SectionType == NodeType.InitScript).Dncs.Where(x => x.ID == ((Dnc)e.Tag).ID).FirstOrDefault();

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
                        Stream tmpStream = File.OpenRead(recent.Text);
                        
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
    }
}
