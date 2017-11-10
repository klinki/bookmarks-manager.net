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

            if (protocolSeparator == -1)
            {
                return "";
            }

            string withoutProtocol = bookmark.Url.Substring(protocolSeparator + 2);
            int slash = withoutProtocol.IndexOf("/");

            if (slash == -1)
            {
                return "";
            }

            return withoutProtocol.Substring(0, slash);
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
