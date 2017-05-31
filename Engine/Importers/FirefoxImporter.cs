using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace Engine.Importers
{
    public class FirefoxImporter : AbstractBookmarkImporter
    {
        static FirefoxImporter()
        {
            ImporterRegistry.GetInstance().Register(new FirefoxImporter());
        }

        public override bool canHandleFile(string path)
        {
            using (StreamReader streamReader = new StreamReader(path, Encoding.UTF8))
            using (JsonTextReader reader = new JsonTextReader(streamReader))
            {
                JsonSerializer serializer = new JsonSerializer();
                var o = (JToken)serializer.Deserialize(reader);

                return o["guid"] != null;
            }
        }
        public override BookmarkDirectory fromFile(string path)
        {
            var root = this.createRootDirectory(path);

            using (StreamReader streamReader = new StreamReader(path, Encoding.UTF8))
            using (JsonTextReader reader = new JsonTextReader(streamReader))
            {
                JsonSerializer serializer = new JsonSerializer();
                var o = (JToken)serializer.Deserialize(reader);

                if (o["guid"] == null)
                {
                    throw new InvalidDataException("Invalid input format");
                }

                List<BookmarkDirectory> directories = o["children"]
                    .Where(x => x["type"].ToString() != "text/x-moz-place-separator")
                    .ToList()
                    .ConvertAll(r =>
                    {
                        return (BookmarkDirectory)this.GetNode(r, root);
                    });
            }

            return root;
        }

        protected BookmarkNode GetNode(JToken token, BookmarkDirectory parent)
        {
            var node = this.GetNode(token);
            parent.AddChild(node);

            return node;
        }

        protected BookmarkNode GetNode(JToken token)
        {
            var type = token["type"].ToString();
            if (type == "text/x-moz-place-container")
            {
                return this.GetDirectory(token);
            }
            else if (type == "text/x-moz-place")
            {
                return this.GetBookmark(token);
            }
            else
            {
                throw new System.Exception("Invalid type");
            }
        }

        protected BookmarkDirectory GetDirectory(JToken token)
        {
            var directory = new BookmarkDirectory();
            this.SetBookmarkNodeProperties(directory, token);

            if (token["children"] != null)
            {
                foreach (var child in token["children"].Where(x => x["type"].ToString() != "text/x-moz-place-separator"))
                {
                    BookmarkNode node = this.GetNode(child, directory);
                }
            }

            return directory;
        }

        protected Bookmark GetBookmark(JToken token)
        {
            var bookmark = new Bookmark();
            this.SetBookmarkNodeProperties(bookmark, token);
            bookmark.Id = int.Parse(token["id"].ToString());
            bookmark.Url = token["uri"].ToString();

            return bookmark;
        }

        protected void SetBookmarkNodeProperties(BookmarkNode node, JToken token)
        {
            node.Name = token["title"]?.ToString();
        }
    }
}
