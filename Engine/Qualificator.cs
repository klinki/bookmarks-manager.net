using System.Collections.Generic;
using System.Linq;

namespace Engine
{
    public class BookmarkQualificator : AbstractBookmarksVisitor
    {
        protected Dictionary<string, List<Bookmark>> visitedBookmarks;

        public static string GetServerName(Bookmark bookmark)
        {
            int protocolSeparator = bookmark.Url.IndexOf("//");
            string withoutProtocol = bookmark.Url.Substring(protocolSeparator);
            int slash = withoutProtocol.IndexOf("/");

            return withoutProtocol.Substring(slash);
        }

        public override void Visit(Bookmark bookmark)
        {
            string serverName = GetServerName(bookmark);

            if (!this.visitedBookmarks.ContainsKey(serverName))
            {
                this.visitedBookmarks.Add(serverName, new List<Bookmark>());
            }

            this.visitedBookmarks[serverName].Add(bookmark);
        }

        public Dictionary<string, List<Bookmark>> QualifyByServer(BookmarkDirectory root)
        {
            this.visitedBookmarks = new Dictionary<string, List<Bookmark>>();
            root.Accept(this);

            return this.visitedBookmarks;
        }
    }
}
