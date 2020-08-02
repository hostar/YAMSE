using ComponentFactory.Krypton.Navigator;
using ComponentFactory.Krypton.Toolkit;
using ComponentFactory.Krypton.Workspace;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
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

        public MainForm2()
        {
            InitializeComponent();
            SuspendLayout();
            ((ISupportInitialize)kryptonWorkspaceTreeView).BeginInit();
            //Controls.Add(kryptonWorkspaceTreeView);
            kryptonWorkspaceTreeView.Dock = DockStyle.Fill;
            ((ISupportInitialize)kryptonWorkspaceTreeView).EndInit();

            kryptonWorkspaceTreeView.Root.Children.Add(CreateCell(1, false, false));

            kryptonWorkspaceTreeView.AllowPageDrag = false;

            ((ISupportInitialize)kryptonWorkspaceContent).BeginInit();
            kryptonWorkspaceContent.Dock = DockStyle.Fill;
            ((ISupportInitialize)kryptonWorkspaceContent).EndInit();
            kryptonWorkspaceContent.Root.Children.Add(CreateCell(2));

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
            this.newToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.newToolStripMenuItem.Size = new System.Drawing.Size(181, 26);
            this.newToolStripMenuItem.Text = "&New";

            fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem });

            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(46, 24);
            this.fileToolStripMenuItem.Text = "&File";

            menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem });
        }

        private KryptonWorkspaceCell CreateCell(int numPages, bool allowDrag = true, bool allowDrop = true)
        {
            // Create new cell instance
            KryptonWorkspaceCell cell = new KryptonWorkspaceCell();

            // Do we need to set the star sizing value?
            /*
            if (!string.IsNullOrEmpty(starSize))
                cell.StarSize = starSize;
            */

            // Add requested number of pages
            for (int i = 0; i < numPages; i++)
            {
                cell.Pages.Add(CreatePage());
            }

            cell.AllowPageDrag = allowDrag;
            cell.PageDrop += Cell_PageDrop;

            return cell;
        }

        private void Cell_PageDrop(object sender, PageDropEventArgs e)
        {
            e.Cancel = true;
        }

        private KryptonPage CreatePage()
        {
            // Give each page a unique number
            string pageNumber = 1.ToString();

            // Create a new page and give it a name and image
            KryptonPage page = new KryptonPage();
            page.Text = "P" + pageNumber;
            page.TextTitle = "P" + pageNumber + " Title";
            page.TextDescription = "P" + pageNumber + " Description";
            //page.ImageSmall = imageList.Images[_count % imageList.Images.Count];
            page.MinimumSize = new Size(200, 250);

            // Create a rich text box with some sample text inside
            KryptonRichTextBox rtb = new KryptonRichTextBox();
            rtb.Text = "This page (" + page.Text + ") contains a rich text box control as example content. Your application could place anything you like here such as data entry controls, charts, data grids etc.\n\nTry dragging the page headers in order to rearrange the workspace layout.";
            rtb.Dock = DockStyle.Fill;
            rtb.StateCommon.Border.Draw = InheritBool.False;

            // Add rich text box as the contents of the page
            page.Padding = new Padding(5);
            page.Controls.Add(rtb);

            return page;
        }

        private void Page_DragDrop(object sender, DragEventArgs e)
        {
        }
    }
}
