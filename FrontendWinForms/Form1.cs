using BrightIdeasSoftware;
using Engine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FrontendWinForms
{
    public partial class Form1 : Form
    {
        BookmarkImporter importer;

        BookmarkDirectory data;

        private ObjectListView listView1;


        public Form1()
        {
            InitializeComponent();

            this.treeView1.NodeMouseClick += TreeView1_NodeMouseClick;

            this.listView1 = new FastObjectListView();
            this.listView1.Dock = DockStyle.Fill;

            this.splitContainer1.Panel2.Controls.Add(this.listView1);

            this.listView1.DoubleClick += ListBox1_DoubleClick;

            this.listView1.AllColumns.Add(new OLVColumn("Name", "Name"));
            this.listView1.AllColumns.Add(new OLVColumn("Url", "Url"));

            this.listView1.Columns.AddRange(new ColumnHeader[] {
                this.listView1.AllColumns[0],
                this.listView1.AllColumns[1]
            });

            this.listView1.ShowGroups = false;
        }

        private void ListBox1_DoubleClick(object sender, EventArgs e)
        {
            if (this.listView1.SelectedItems.Count == 1)
            {
                var bookmarkNode = (BookmarkNode)this.listView1.SelectedItems[0].Tag;

                if (bookmarkNode.Type == BookmarkType.Directory)
                {
                    this.setListViewItems((BookmarkDirectory)bookmarkNode);
                }                
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                importer = new ChromeImporter();
                this.data = importer.fromFile(this.openFileDialog1.FileName);

                var visitor = new BookmarkDirectoryNodeVisitor();
                var directoryHierarchy = visitor.GetOnlyDirectories(this.data);

                this.treeView1.Nodes.AddRange(directoryHierarchy.ToArray());     
        
            }
        }

        private void TreeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            this.setListViewItems(((BookmarkDirectoryNode)e.Node).BookmarkDirectory);
        }

        protected void setListViewItems(BookmarkDirectory directory)
        {
            this.listView1.SetObjects(directory.Children);
            /*


            var listViewData = directory.Children.ConvertAll(node =>
            {
                var item = new ListViewItem();

                item.SubItems.Add(node.Name);

                if (node.Type == BookmarkType.Bookmark)
                    item.SubItems.Add((node as Bookmark).Url);

                item.Tag = node;

                return item;
            });

            this.listView1.Items.Clear();
            this.listView1.Items.AddRange(listViewData.ToArray());
            */
        }
    }
}
