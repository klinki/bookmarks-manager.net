using Microsoft.VisualStudio.TestTools.UnitTesting;
using FrontendWinForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using Engine;

namespace FrontendWinForms.Tests
{
    [TestClass()]
    public class BookmarkDirectoryNodeVisitorTests
    {
        [TestMethod()]
        public void GetOnlyDirectoriesTest()
        {
            BookmarkDirectory root = new BookmarkDirectory();
            root.Name = "Root";

            var firstChild = new BookmarkDirectory();
            firstChild.Name = "First Child";

            var secondChild = new BookmarkDirectory();
            secondChild.Name = "Second Child";

            root.AddChild(firstChild);
            root.AddChild(secondChild);


            BookmarkDirectoryNodeVisitor visitor = new BookmarkDirectoryNodeVisitor();

            List<BookmarkDirectoryNode> treeNodes = visitor.GetOnlyDirectories(root);

            Assert.AreEqual(1, treeNodes.Count);

            Assert.AreEqual(root.Name, treeNodes[0].Text);
            Assert.AreEqual(2, treeNodes[0].Nodes.Count);        
        }
    }
}