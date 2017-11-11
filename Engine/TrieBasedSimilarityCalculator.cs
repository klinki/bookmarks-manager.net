using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Engine
{
    public class TrieBasedSimilarityCalculator : AbstractBookmarksVisitor
    {
        const int DISTANCE_THRESHOLD = 10;
        protected List<Bookmark> bookmarks;

        public TrieBasedSimilarityCalculator()
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

            this.bookmarks = this.LexicographicallySort(this.bookmarks);

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

        public List<Bookmark> GetList(BookmarkDirectory directory)
        {
            this.bookmarks = new List<Bookmark>();
            directory.Accept(this);

            return new List<Bookmark>(this.bookmarks);
        }

        public List<Bookmark> LexicographicallySort(List<Bookmark> bookmarks)
        {
            Trie<Bookmark> trie = new Trie<Bookmark>();

            foreach (Bookmark bookmark in bookmarks)
            {
                try
                {
                    trie.Insert(bookmark.Url, bookmark);
                }
                catch (DuplicateElementException e)
                {
                    Bookmark original = trie.Get(bookmark.Url);

                    if (original != null)
                    {
                        if (Math.Max(original.Tags.Count, bookmark.Tags.Count) > 0)
                        {
                            HashSet<Tag> mergedTags = new HashSet<Tag>();
                            mergedTags.UnionWith(original.Tags);
                            mergedTags.UnionWith(bookmark.Tags);

                            original.Tags = mergedTags.ToList();
                        }
                    }
                }
            }

            return trie.Select(item => item.Value).ToList();
        }
    }
}
