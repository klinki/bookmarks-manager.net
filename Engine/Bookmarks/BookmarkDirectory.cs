using System.Collections.Generic;

namespace Engine
{
    public class BookmarkDirectory : BookmarkNode
    {
        public BookmarkDirectory(string name): this()
        {
            this.Name = name;            
        }

        public BookmarkDirectory()
        {
            this.Children = new List<BookmarkNode>();
        }

        public override BookmarkType Type { get { return BookmarkType.Directory; } }

        public List<BookmarkNode> Children 
        {
            get;
            set;
        }

        public void AddChild(BookmarkNode node)
        {
            this.Children.Add(node);
            node.Parent = this;
        }

        public void RemoveChild(BookmarkNode node)
        {
            this.Children.Remove(node);
            node.Parent = null;
        }

        public List<Bookmark> Bookmarks
        {
            get {
                return this.Children.FindAll(x => x.Type == BookmarkType.Bookmark)
                    .ConvertAll<Bookmark>(node => (Bookmark)node);
            }
        }

        public void AddBookmark(Bookmark bookmark)
        {
            this.AddChild(bookmark);
        }

        public void RemoveBookmark(Bookmark bookmark)
        {
            this.RemoveChild(bookmark);
        }

        public void AddDirectory(BookmarkDirectory directory)
        {
            this.AddChild(directory);
            directory.Parent = this;
        }

        public void RemoveDirectory(BookmarkDirectory directory)
        {
            this.RemoveChild(directory);
            directory.Parent = null;
        }

        public override void Accept(IBookmarkVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
