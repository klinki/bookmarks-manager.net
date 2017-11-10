using System.Collections.Generic;
using System.Linq;

namespace Engine
{
    public abstract class AbstractBookmarksVisitor : IBookmarkVisitor
    {
        public abstract void Visit(Bookmark bookmark);

        public void Visit(BookmarkDirectory directory)
        {
            directory.Children.ForEach(bookmark => bookmark.Accept(this));
        }
    }

    public class BookmarkDeduplicator : AbstractBookmarksVisitor
    {
        protected Dictionary<string, List<Bookmark>> visitedBookmarks;

        public override void Visit(Bookmark bookmark)
        {
            if (!this.visitedBookmarks.ContainsKey(bookmark.Url))
            {
                this.visitedBookmarks.Add(bookmark.Url, new List<Bookmark>());
            }

            this.visitedBookmarks[bookmark.Url].Add(bookmark);
        }

        public IEnumerable<Bookmark> GetDuplicateBookmarks(BookmarkDirectory root)
        {
            this.visitedBookmarks = new Dictionary<string, List<Bookmark>>();
            root.Accept(this);

            var duplicateBookmarksRoot = new BookmarkDirectory("Duplicate Bookmarks");

            return this.visitedBookmarks.Where(pair => pair.Value.Count > 1)
                .SelectMany(pair => pair.Value)
                .OrderBy(bookmark => bookmark.Url);
        }
    }
}
