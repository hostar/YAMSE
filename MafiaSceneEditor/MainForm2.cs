using ComponentFactory.Krypton.Navigator;
using ComponentFactory.Krypton.Toolkit;
using ComponentFactory.Krypton.Workspace;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace YAMSE
{
    public partial class MainForm2 : Form
    {
        KryptonWorkspace kryptonWorkspaceTreeView = new KryptonWorkspace();
        KryptonWorkspace kryptonWorkspaceContent = new KryptonWorkspace();

        private SplitContainer splitContainer = new SplitContainer();
        private MenuStrip menuStrip1 = new System.Windows.Forms.MenuStrip();

        ToolStripMenuItem newToolStripMenuItem = new ToolStripMenuItem();
        ToolStripMenuItem fileToolStripMenuItem = new ToolStripMenuItem();

        KryptonTreeView treeView1 = new KryptonTreeView();

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
            Controls.Add(menuStrip1);

            splitContainer.Dock = DockStyle.Fill;
            splitContainer.Panel1.Controls.Add(kryptonWorkspaceTreeView);
            splitContainer.Panel2.Controls.Add(kryptonWorkspaceContent);

            InitMenus();

            ResumeLayout();
        }

        private void InitMenus()
        {
            newToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            newToolStripMenuItem.Name = "newToolStripMenuItem";
            newToolStripMenuItem.ShortcutKeys = (Keys.Control | Keys.N);
            newToolStripMenuItem.Size = new Size(181, 26);
            newToolStripMenuItem.Text = "&New";

            fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            newToolStripMenuItem });

            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new System.Drawing.Size(46, 24);
            fileToolStripMenuItem.Text = "&File";

            menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            fileToolStripMenuItem });
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
    }
}
