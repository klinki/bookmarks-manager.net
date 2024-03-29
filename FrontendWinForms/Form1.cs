﻿using BrightIdeasSoftware;
using Engine;
using Engine.Filters;
using Engine.Importers;
using Engine.Serializers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FrontendWinForms
{
    public partial class Form1 : Form
    {
        IBookmarkImporter importer;
        List<IBookmarkImporter> importers;

        List<BookmarkDirectory> rootNodes;

        BookmarkDirectory data;

        private ObjectListView listView1;
        private TreeListView treeListView1;

        public Form1()
        {
            InitializeComponent();

            this.rootNodes = new List<BookmarkDirectory>();

            this.treeListView1 = new TreeListView();
            this.treeListView1.Dock = DockStyle.Fill;

            this.splitContainer1.Panel1.Controls.Add(this.treeListView1);

            this.treeListView1.Roots = this.rootNodes;
            this.treeListView1.CanExpandGetter = x =>
            {
                if ((x as BookmarkNode).Type != BookmarkType.Directory)
                    return false;

                return (x as BookmarkDirectory).Children.Where(y => y.Type == BookmarkType.Directory).Count() > 0;
            };
            this.treeListView1.ChildrenGetter = x => new List<BookmarkNode>((x as BookmarkDirectory).Children.Where(y => y.Type == BookmarkType.Directory));

            this.treeListView1.AllColumns.Add(new OLVColumn("Name", "Name"));
            this.treeListView1.Columns.Add(this.treeListView1.AllColumns[0]);
            this.treeListView1.AllColumns[0].FillsFreeSpace = true;

            this.treeListView1.CellClick += TreeListView1_CellClick;

            this.listView1 = new FastObjectListView();
            this.listView1.Dock = DockStyle.Fill;

            this.splitContainer1.Panel2.Controls.Add(this.listView1);

            this.listView1.DoubleClick += ListBox1_DoubleClick;

            this.listView1.AllColumns.Add(new OLVColumn("Name", "Name"));
            this.listView1.AllColumns.Add(new OLVColumn("Url", "Url"));
            this.listView1.AllColumns.Add(new OLVColumn("Last Visited", "LastVisited"));
            this.listView1.AllColumns.Add(new OLVColumn("Date Added", "Created"));

            this.listView1.AllColumns[0].FreeSpaceProportion = 2;
            this.listView1.AllColumns[1].FreeSpaceProportion = 2;

//            this.listView1.AllColumns.ForEach(column => column.FillsFreeSpace = true);

            this.listView1.Columns.AddRange(new ColumnHeader[] {
                this.listView1.AllColumns[0],
                this.listView1.AllColumns[1],
                this.listView1.AllColumns[2],
                this.listView1.AllColumns[3]
            });

            this.listView1.ShowGroups = false;

            this.listView1.DragSource = new SimpleDragSource();
            this.listView1.DropSink = new RearrangingDropSink(false);

            this.listView1.ModelCanDrop += ListView1_ModelCanDrop;

            this.importers = new List<IBookmarkImporter>();

            this.importers.Add(new ChromeImporter());
            this.importers.Add(new FirefoxImporter());
            this.importers.Add(new XmarksImporter());
        }

        private void TreeListView1_CellClick(object sender, CellClickEventArgs e)
        {
            this.setListViewItems(e.Model as BookmarkDirectory);
        }

        private void ListView1_ModelCanDrop(object sender, ModelDropEventArgs e)
        {
            BookmarkNode node = e.TargetModel as BookmarkNode;

            if (node == null)
            {
                e.Effect = DragDropEffects.None;
            }
            else
            {
                if (node.Type == BookmarkType.Directory)
                {
                    e.Effect = DragDropEffects.Move;
                }
            }
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
            this.openFileDialog1.FileName = "";
            this.openFileDialog1.Filter = "All files (*.*)|*.*|Google Chrome Bookmarks (*.*)|*.*|Firefox Bookmarks (*.json)|*.json";

            if (this.openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.importFromFile(this.openFileDialog1.FileName);
            }
        }

        protected void importFromFile(string filePath)
        {
            importer = ImporterRegistry.GetInstance().GetImporterForFile(filePath);
            this.data = importer.FromFile(filePath);

            this.rootNodes.Add(this.data);
            this.treeListView1.SetObjects(this.rootNodes);
        }

        protected void setListViewItems(BookmarkDirectory directory)
        {
            if (directory != null)
            {
                this.listView1.SetObjects(directory.Children);
            }
        }

        private void findDuplicitiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BookmarkDeduplicator deduplicator = new BookmarkDeduplicator();
            IEnumerable<Bookmark> duplicities = deduplicator.GetDuplicateBookmarks(this.data);

            BookmarkDirectory duplicateRoot = new BookmarkDirectory("Duplicate Bookmarks");

            foreach (var duplicity in duplicities)
            {
                duplicateRoot.AddBookmark(duplicity);
            }

            this.data.AddChild(duplicateRoot);
            
            this.treeListView1.Refresh();

        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var serializer = new JsonSerializer();
                var data = serializer.Serialize(this.rootNodes);

                File.WriteAllText(this.saveFileDialog1.FileName, data);
            }
        }

        private void findSimilaritiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var qualifier = new SimilarityCalculator();

            var trieBasedQualifier = new TrieBasedSimilarityCalculator();
            var data = trieBasedQualifier.LexicographicallySort(trieBasedQualifier.GetList(this.data));

            var parallelResult = qualifier.QualifyByServerParallel(data);

            var result = qualifier.QualifyByServer(data);

            if (result == null)
            {

            }

        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] filePaths = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (string path in filePaths)
            {
                this.importFromFile(path);
            }
        }
    }
}
