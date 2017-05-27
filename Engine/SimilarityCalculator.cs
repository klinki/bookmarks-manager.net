using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine
{
    public class SimilarityCalculator : AbstractBookmarksVisitor
    {
        const int DISTANCE_THRESHOLD = 10;
        protected List<Bookmark> bookmarks;

        public override void Visit(Bookmark bookmark)
        {
            this.bookmarks.Add(bookmark);
        }

        protected int GetDistance(string s, string t)
            {
            // degenerate cases
            if (s == t) return 0;
            if (s.Length == 0) return t.Length;
            if (t.Length == 0) return s.Length;

            // create two work vectors of integer distances
            int[] v0 = new int[t.Length + 1];
            int[] v1 = new int[t.Length + 1];

            // initialize v0 (the previous row of distances)
            // this row is A[0][i]: edit distance for an empty s
            // the distance is just the number of characters to delete from t
            for (int i = 0; i < v0.Length; i++)
                v0[i] = i;

            for (int i = 0; i < s.Length; i++)
            {
                // calculate v1 (current row distances) from the previous row v0

                // first element of v1 is A[i+1][0]
                //   edit distance is delete (i+1) chars from s to match empty t
                v1[0] = i + 1;

                // use formula to fill in the rest of the row
                for (int j = 0; j < t.Length; j++)
                {
                    var cost = (s[i] == t[j]) ? 0 : 1;
                    v1[j + 1] = Math.Min(Math.Min(v1[j] + 1, v0[j + 1] + 1), v0[j] + cost);
                }

                // copy v1 (current row) to v0 (previous row) for next iteration
                for (int j = 0; j < v0.Length; j++)
                    v0[j] = v1[j];
            }

            return v1[t.Length];
        }

        public Dictionary<string, SortedSet<Bookmark>> QualifyByServer(BookmarkDirectory root)
        {
            root.Accept(this);

            var possibleDuplicates = new Dictionary<string, SortedSet<Bookmark>>();

            for (int i = 0; i < this.bookmarks.Count - 1; i++) {
                for (int j = i; j < this.bookmarks.Count; j++) {
                    Bookmark a = this.bookmarks[i];
                    Bookmark b = this.bookmarks[j];
                    int distance = this.GetDistance(a.Url, b.Url);

                    if (distance < DISTANCE_THRESHOLD) {
                        var serverUrl = BookmarkQualificator.GetServerName(a);
                        
                        if (!possibleDuplicates.ContainsKey(serverUrl)) {
                            possibleDuplicates.Add(serverUrl, new SortedSet<Bookmark>());
                        }

                        possibleDuplicates[serverUrl].Add(a);
                        possibleDuplicates[serverUrl].Add(b);
                    }
                }
            }


            return possibleDuplicates;
        }
    }
}
