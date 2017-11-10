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
    public partial class SimilarBookmarks : Form
    {
        protected BookmarksSimilarityResult data;
        protected ObjectListView listView1;


        public SimilarBookmarks()
        {
            InitializeComponent();


            this.listView1 = new FastObjectListView();
            this.listView1.Dock = DockStyle.Fill;

            this.Controls.Add(this.listView1);

            this.listView1.AllColumns.Add(new OLVColumn("Name", "Name"));
            this.listView1.AllColumns.Add(new OLVColumn("Url", "Url"));
            this.listView1.AllColumns.Add(new OLVColumn("Last Visited", "LastVisited"));
            this.listView1.AllColumns.Add(new OLVColumn("Date Added", "Created"));
            //this.listView1.AllColumns.Add(new OLVColumn("Duplicities", ))

            this.listView1.AllColumns[0].FreeSpaceProportion = 2;
            this.listView1.AllColumns[1].FreeSpaceProportion = 2;

            this.listView1.Columns.AddRange(new ColumnHeader[] {
                this.listView1.AllColumns[0],
                this.listView1.AllColumns[1],
                this.listView1.AllColumns[2],
                this.listView1.AllColumns[3]
            });


            this.listView1.FormatRow += ListView1_FormatRow;
        }

        private void ListView1_FormatRow(object sender, FormatRowEventArgs e)
        {

        }

        public void ShowSimilarBookmarks(BookmarksSimilarityResult data)
        {
            this.data = data;
            this.Show();
        }
    }
}
