using System.Collections.Generic;

namespace Engine
{
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

        public Tag()
        {
            this.Bookmarks = new List<Bookmark>();
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
}
