using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engine
{
    public class BookmarkSimilarityIndexComparer : IEqualityComparer<BookmarkSimilarityIndex>
    {
        public bool Equals(BookmarkSimilarityIndex x, BookmarkSimilarityIndex y)
        {
            if (object.ReferenceEquals(x, y)) return true;

            if (x == null || y == null) return false;

            return (x.From == y.From && x.To == y.To) || (x.From == y.To && x.To == y.From);
        }

        public int GetHashCode(BookmarkSimilarityIndex obj)
        {
            return (((long)obj.From.GetHashCode()) + ((long)obj.To.GetHashCode())).GetHashCode();
        }
    }

    public class BookmarkSimilarityIndex
    {
        public BookmarkSimilarityIndex(Bookmark from, Bookmark to, int dist)
        {
            this.From = from;
            this.To = to;
            this.Distance = dist;
        }

        public Bookmark From
        {
            get;
            set;
        }

        public Bookmark To
        {
            get;
            set;
        }

        public int Distance
        {
            get;
            set;
        }
    }

    public class BookmarksSimilarityResult
    {
        public ICollection<BookmarkSimilarityIndex> SimilarBookmarks
        {
            get;
            set;
        }

        public IDictionary<string, SortedSet<Bookmark>> BookmarksByServer
        {
            get;
            set;
        }

        public Dictionary<Bookmark, ICollection<BookmarkSimilarityIndex>> PrecalculatedSimilarities
        {
            get;
            private set;
        }

        public void PrecalculateSimilarities()
        {
            this.PrecalculatedSimilarities = new Dictionary<Bookmark, ICollection<BookmarkSimilarityIndex>>();

            foreach (var similarityIndex in this.SimilarBookmarks)
            {
                this.AddBookmarkToList(similarityIndex.From, similarityIndex);
                this.AddBookmarkToList(similarityIndex.To, similarityIndex);
            }
        }

        protected void AddBookmarkToList(Bookmark node, BookmarkSimilarityIndex index)
        {
            if (!this.PrecalculatedSimilarities.ContainsKey(node))
            {
                this.PrecalculatedSimilarities.Add(node, new List<BookmarkSimilarityIndex>());
            }

            this.PrecalculatedSimilarities[node].Add(index);
        }
    }

    public class SimilarityCalculator : AbstractBookmarksVisitor
    {
        const int DISTANCE_THRESHOLD = 10;
        protected List<Bookmark> bookmarks;

        public SimilarityCalculator()
        {
            this.bookmarks = new List<Bookmark>();
        }

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

        protected BookmarksSimilarityResult FindDuplicities(List<Bookmark> bookmarks, int startFrom = 0, int to = -1)
        {
            var result = new BookmarksSimilarityResult
            {
                SimilarBookmarks = new HashSet<BookmarkSimilarityIndex>(new BookmarkSimilarityIndexComparer())
            };

            var possibleDuplicates = result.BookmarksByServer = new Dictionary<string, SortedSet<Bookmark>>();

            to = (to == -1) ? bookmarks.Count - 1 : to;

            for (int i = startFrom; i < to; i++)
            {
                for (int j = i + 1; j < to + 1; j++)
                {
                    Bookmark a = bookmarks[i];
                    Bookmark b = bookmarks[j];

                    if (BookmarkQualificator.GetServerName(a) != BookmarkQualificator.GetServerName(b))
                    {
                        continue;
                    }

                    int distance = this.GetDistance(a.Url, b.Url);

                    if (distance < DISTANCE_THRESHOLD)
                    {
                        var serverUrl = BookmarkQualificator.GetServerName(a);

                        if (!possibleDuplicates.ContainsKey(serverUrl))
                        {
                            possibleDuplicates.Add(serverUrl, new SortedSet<Bookmark>());
                        }

                        possibleDuplicates[serverUrl].Add(a);
                        possibleDuplicates[serverUrl].Add(b);

                        result.SimilarBookmarks.Add(new BookmarkSimilarityIndex(a, b, distance));
                    }
                }
            }

            return result;
        }

        public BookmarksSimilarityResult QualifyByServer(BookmarkDirectory root, int startFrom = 0, int to = -1)
        {
            root.Accept(this);
            return this.FindDuplicities(this.bookmarks);
        }

        public BookmarksSimilarityResult QualifyByServerParallel(BookmarkDirectory root)
        {
            this.bookmarks = new List<Bookmark>();
            root.Accept(this);

            var rangePartitioner = Partitioner.Create(0, this.bookmarks.Count);

            var dictionary = new ConcurrentDictionary<string, SortedSet<Bookmark>>();

            var totalResult = new BookmarksSimilarityResult()
            {
                SimilarBookmarks = new ConcurrentSet<BookmarkSimilarityIndex>(),
                BookmarksByServer = dictionary
            };

            Parallel.ForEach(rangePartitioner, (range, loopState) =>
            {
                var result = this.FindDuplicities(this.bookmarks, range.Item1, range.Item2 - 1);

                foreach (var keyValuePair in result.BookmarksByServer)
                {
                    dictionary.AddOrUpdate(keyValuePair.Key, key => keyValuePair.Value, (key, oldValue) => {
                        foreach (var bookmark in keyValuePair.Value)
                        {
                            oldValue.Add(bookmark);
                        }

                        return oldValue;
                    });
                }

                foreach (var similarityIndex in result.SimilarBookmarks)
                {
                    totalResult.SimilarBookmarks.Add(similarityIndex);
                }
            });

            return totalResult;
        }
    }
}
