using ComponentFactory.Krypton.Navigator;
using ComponentFactory.Krypton.Toolkit;
using ComponentFactory.Krypton.Workspace;
using ComponentFactory.Krypton.Ribbon;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using YAMSE.DataLayer;
using System;
using System.IO;
using System.Linq;

namespace YAMSE
{
    public partial class MainForm2 : KryptonForm
    {
        KryptonWorkspace kryptonWorkspaceTreeView = new KryptonWorkspace();
        KryptonWorkspace kryptonWorkspaceContent = new KryptonWorkspace();

        private KryptonSplitContainer splitContainer = new KryptonSplitContainer();

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

        KryptonRibbonGroupButton kryptonRibbonGroupButtonShowDiagram = new KryptonRibbonGroupButton();

        KryptonListBox listBoxOutput = new KryptonListBox();

        OpenFileDialog openFileDialog1 = new OpenFileDialog();

        bool isDirty = false;
        bool scene2FileLoaded = false;

        private Scene2Data scene2Data = new Scene2Data();

        public MainForm2()
        {
            // KryptonExplorer
            InitializeComponent();
            SuspendLayout();
            ((ISupportInitialize)kryptonWorkspaceTreeView).BeginInit();
            //Controls.Add(kryptonWorkspaceTreeView);
            kryptonWorkspaceTreeView.Dock = DockStyle.Fill;
            ((ISupportInitialize)kryptonWorkspaceTreeView).EndInit();

            kryptonWorkspaceTreeView.Root.Children.Add(CreateCell("Objects", NavigatorMode.Panel));

            kryptonWorkspaceTreeView.AllowPageDrag = false;

            ((ISupportInitialize)kryptonWorkspaceContent).BeginInit();
            kryptonWorkspaceContent.Dock = DockStyle.Fill;
            ((ISupportInitialize)kryptonWorkspaceContent).EndInit();

            //kryptonWorkspaceContent.Root.Children.Add(CreateCell(2));

            Controls.Add(splitContainer);
            Controls.Add(kryptonRibbon);

            splitContainer.Dock = DockStyle.Fill;
            splitContainer.Panel1.Controls.Add(kryptonWorkspaceTreeView);
            splitContainer.Panel2.Controls.Add(kryptonWorkspaceContent);

            FormClosing += MainForm_Close;

            MinimumSize = new Size(500, 500);

            //InitMenus();
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

            kryptonContextMenuItem4.Text = "Save As";
            kryptonContextMenuItem4.Image = (Image)resources.GetObject("ImageSaveAs");

            kryptonContextMenuItemExit.Text = "Exit";
            kryptonContextMenuItemExit.Image = (Image)resources.GetObject("ImageExit");
            kryptonContextMenuItemExit.Click += ((sender, e) => { this.Close(); });

            kryptonRibbon.HideRibbonSize = new Size(100, 250);
            kryptonRibbon.QATLocation = QATLocation.Hidden;

            kryptonRibbonTab1.Text = "Tools";

            kryptonRibbon.RibbonAppButton.AppButtonMenuItems.AddRange(new KryptonContextMenuItemBase[] {
            kryptonContextMenuItem1,
            kryptonContextMenuItem2,
            kryptonContextMenuItem3,
            kryptonContextMenuItem4,
            kryptonContextMenuSeparator1,
            kryptonContextMenuItemExit});

            kryptonRibbon.RibbonTabs.AddRange(new KryptonRibbonTab[] {
            kryptonRibbonTab1});

            kryptonRibbonGroup1.DialogBoxLauncher = false;
            kryptonRibbonGroup1.TextLine1 = "Visualization";

            kryptonRibbonTab1.Groups.AddRange(new KryptonRibbonGroup[] {
            kryptonRibbonGroup1});

            kryptonRibbonGroup1.Items.AddRange(new KryptonRibbonGroupContainer[] {
            kryptonRibbonGroupTriple1});

            kryptonRibbonGroupButtonShowDiagram.TextLine1 = "Show diagram";

            kryptonRibbonGroupTriple1.Items.AddRange(new KryptonRibbonGroupItem[] {
            kryptonRibbonGroupButtonShowDiagram});
        }

        private void MainForm_Close(object sender, FormClosingEventArgs e)
        {
            if (KryptonMessageBox.Show("Are you sure you want to exit?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.No)
            {
                e.Cancel = true;
            }
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

            // Add rich text box as the contents of the page
            page.Padding = new Padding(5);
            page.Controls.Add(treeView1);

            return page;
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
                foreach (var item in scene2Data.objectsDncs.GroupBy(x => x.objectType))
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
    }
}
