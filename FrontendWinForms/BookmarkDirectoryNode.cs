using Engine;
using System.Collections.Generic;
using System.Windows.Forms;
using System;

namespace FrontendWinForms
{
    public class BookmarkDirectoryNode : TreeNode
    {
        public BookmarkDirectory BookmarkDirectory
        {
            get;
            private set;
        }

        public BookmarkDirectoryNode(BookmarkDirectory node)
        {
            this.BookmarkDirectory = node;
            this.Text = node.Name;
        }
    }

    public class BookmarkDirectoryNodeVisitor
    {
        private BookmarkEventVisitor eventVisitor;
        private BookmarkDirectoryNode currentDirectory;
        private Stack<BookmarkDirectoryNode> visitedStack;
        private List<BookmarkDirectoryNode> rootDirectories;

        public BookmarkDirectoryNodeVisitor()
        {
            this.eventVisitor = new BookmarkEventVisitor();
            this.eventVisitor.OnBookmarkDirectoryOpen += EventVisitor_OnBookmarkDirectoryOpen;
            this.eventVisitor.OnBookmarkDirectoryClose += EventVisitor_OnBookmarkDirectoryClose;
        }

        private void EventVisitor_OnBookmarkDirectoryOpen(object sender, BookmarkEventArgs<BookmarkDirectory> e)
        {
            var directory = new BookmarkDirectoryNode(e.Node);

            if (this.currentDirectory == null)
            {
                this.rootDirectories.Add(directory);
            }
            else
            {
                this.currentDirectory.Nodes.Add(directory);
            }

            this.currentDirectory = directory;
            this.visitedStack.Push(directory);
        }

        private void EventVisitor_OnBookmarkDirectoryClose(object sender, BookmarkEventArgs<BookmarkDirectory> e)
        {
            this.visitedStack.Pop();
            this.currentDirectory = this.visitedStack.Peek();
        }

        public List<BookmarkDirectoryNode> GetOnlyDirectories(BookmarkDirectory directory)
        {
            return this.GetOnlyDirectories(new BookmarkNode[] { directory });
        }

        public List<BookmarkDirectoryNode> GetOnlyDirectories(IEnumerable<BookmarkNode> nodes)
        {
            this.currentDirectory = null;
            this.visitedStack = new Stack<BookmarkDirectoryNode>();
            this.visitedStack.Push(null);
            this.rootDirectories = new List<BookmarkDirectoryNode>();

            foreach (var node in nodes)
            {
                node.Accept(this.eventVisitor);
            }

            return this.rootDirectories;
        }
    }
}
