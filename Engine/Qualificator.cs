using System;
using System.Collections.Generic;

namespace Engine
{
    public class BookmarkQualificator : AbstractBookmarksVisitor
    {
        protected Dictionary<string, List<Bookmark>> visitedBookmarks;

        public static string GetServerName(Bookmark bookmark)
        {
            int protocolSeparator = bookmark.Url.IndexOf("//", 0, StringComparison.CurrentCulture);

            if (protocolSeparator == -1)
            {
                return "";
            }

            int slash = bookmark.Url.IndexOf('/', protocolSeparator + 2);

            if (slash == -1)
            {
                return "";
            }

            return bookmark.Url.Substring(protocolSeparator + 2, slash - protocolSeparator - 2);
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
