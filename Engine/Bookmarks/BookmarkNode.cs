namespace Engine
{
    public abstract class BookmarkNode : IVisitable<IBookmarkVisitor>
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

        public abstract void Accept(IBookmarkVisitor visitor);
    }
}
