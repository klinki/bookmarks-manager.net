using System;
using System.Collections.Generic;

namespace Engine
{
    public interface BookmarkVisitor
    {
        void Visit(Bookmark bookmark);
        void Visit(BookmarkDirectory directory);
    }

    public class BookmarkEventArgs<T> : EventArgs
    {
        public T Node
        {
            get;
            private set;
        }

        public BookmarkEventArgs(T node) : base()
        {
            this.Node = node;
        }
    }

    public class BookmarkEventVisitor : BookmarkVisitor
    {
        public event EventHandler<BookmarkEventArgs<BookmarkDirectory>> OnBookmarkDirectoryOpen;
        public event EventHandler<BookmarkEventArgs<BookmarkDirectory>> OnBookmarkDirectoryClose;
        public event EventHandler<BookmarkEventArgs<Bookmark>> OnBookmarkOpen;

        public void Visit(BookmarkDirectory directory)
        {
            if (this.OnBookmarkDirectoryOpen != null)
                this.OnBookmarkDirectoryOpen(this, new BookmarkEventArgs<BookmarkDirectory>(directory));

            foreach (var node in directory.Children)
            {
                node.Accept(this);
            }

            if (this.OnBookmarkDirectoryClose != null)
                this.OnBookmarkDirectoryClose(this, new BookmarkEventArgs<BookmarkDirectory>(directory));
        }

        public void Visit(Bookmark bookmark)
        {
            if (this.OnBookmarkOpen != null)
                this.OnBookmarkOpen(this, new BookmarkEventArgs<Bookmark>(bookmark));
        }
    }

    public interface Visitable<T>
    {
        void Accept(T visitor);        
    }

    public class Tag
    {
        public int Id
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public List<Bookmark> Bookmarks
        {
            get;
            set;
        }

        public void AddBookmark(Bookmark Bookmark)
        {
            this.Bookmarks.Add(Bookmark);
        }

        public void RemoveBookmark(Bookmark bookmark)
        {
            this.Bookmarks.Remove(bookmark);
        }
    }

    public enum BookmarkType
    {
        Bookmark,
        Directory
    }

    public abstract class BookmarkNode : Visitable<BookmarkVisitor>
    {
        public int Id 
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public int Created
        {
            get;
            set;
        }

        public int Modified
        {
            get;
            set;
        }

        public BookmarkDirectory Parent
        {
            get;
            set;
        }

        public abstract BookmarkType Type { get; }

        public abstract void Accept(BookmarkVisitor visitor);
    }

    public class Bookmark : BookmarkNode
    {
        public string Url
        {
            get;
            set;
        }

        public int LastVisited
        {
            get;
            set;
        }

        public int CountVisits
        {
            get;
            set;
        }

        public List<Tag> Tags
        {
            get;
            set;            
        }

        public void AddTag(Tag tag)
        {
            this.Tags.Add(tag);
            tag.AddBookmark(this);
        }

        public void RemoveTag(Tag tag)
        {
            this.Tags.Remove(tag);
            tag.RemoveBookmark(this);
        }

        public override BookmarkType Type { get { return BookmarkType.Bookmark; } }

        public override void Accept(BookmarkVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

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

        public override void Accept(BookmarkVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
