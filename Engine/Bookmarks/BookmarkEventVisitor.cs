using System;

namespace Engine
{
    public class BookmarkEventVisitor : IBookmarkVisitor
    {
        public event EventHandler<BookmarkEventArgs<BookmarkDirectory>> OnBookmarkDirectoryOpen;
        public event EventHandler<BookmarkEventArgs<BookmarkDirectory>> OnBookmarkDirectoryClose;
        public event EventHandler<BookmarkEventArgs<Bookmark>> OnBookmarkOpen;

        public void Visit(BookmarkDirectory directory)
        {
            this.OnBookmarkDirectoryOpen?.Invoke(this, new BookmarkEventArgs<BookmarkDirectory>(directory));

            foreach (var node in directory.Children)
            {
                node.Accept(this);
            }

            this.OnBookmarkDirectoryClose?.Invoke(this, new BookmarkEventArgs<BookmarkDirectory>(directory));
        }

        public void Visit(Bookmark bookmark)
        {
            this.OnBookmarkOpen?.Invoke(this, new BookmarkEventArgs<Bookmark>(bookmark));
        }
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

    public interface IVisitable<T>
    {
        void Accept(T visitor);
    }

    public interface IBookmarkVisitor
    {
        void Visit(Bookmark bookmark);
        void Visit(BookmarkDirectory directory);
    }
}
