using System.Collections.Generic;

namespace Engine.Filters
{
    public class DirectoryFilter
    {
        private BookmarkEventVisitor eventVisitor;
        private BookmarkDirectory currentDirectory;
        private Stack<BookmarkDirectory> visitedStack;
        private List<BookmarkDirectory> rootDirectories;

        public DirectoryFilter()
        {
            this.eventVisitor = new BookmarkEventVisitor();
            this.eventVisitor.OnBookmarkDirectoryOpen += EventVisitor_OnBookmarkDirectoryOpen;
            this.eventVisitor.OnBookmarkDirectoryClose += EventVisitor_OnBookmarkDirectoryClose;
        }

        private void EventVisitor_OnBookmarkDirectoryOpen(object sender, BookmarkEventArgs<BookmarkDirectory> e)
        {
            var directory = new BookmarkDirectory(e.Node.Name);

            if (this.currentDirectory == null)
            {
                this.rootDirectories.Add(directory);
            }
            else
            {
                this.currentDirectory.Children.Add(directory);
            }

            this.currentDirectory = directory;
            this.visitedStack.Push(directory);
        }

        private void EventVisitor_OnBookmarkDirectoryClose(object sender, BookmarkEventArgs<BookmarkDirectory> e)
        {
            this.visitedStack.Pop();
            this.currentDirectory = this.visitedStack.Peek();
        }

        public List<BookmarkDirectory> GetOnlyDirectories(BookmarkDirectory directory)
        {
            return this.GetOnlyDirectories(new BookmarkNode[] { directory });
        }

        public List<BookmarkDirectory> GetOnlyDirectories(IEnumerable<BookmarkNode> nodes)
        {
            this.currentDirectory = null;
            this.visitedStack = new Stack<BookmarkDirectory>();
            this.visitedStack.Push(null);
            this.rootDirectories = new List<BookmarkDirectory>();

            foreach (var node in nodes)
            {
                node.Accept(this.eventVisitor);
            }

            return this.rootDirectories;
        }
    }
}
