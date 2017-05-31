using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System;

namespace Engine.Importers
{
    public class ChromeImporter : AbstractBookmarkImporter
    {
        static ChromeImporter()
        {
            ImporterRegistry.GetInstance().Register(new ChromeImporter());
        }

        public override bool canHandleFile(string path)
        {
            using (StreamReader streamReader = new StreamReader(path, Encoding.UTF8))
            using (JsonTextReader reader = new JsonTextReader(streamReader))
            {
                JsonSerializer serializer = new JsonSerializer();
                var o = (JToken)serializer.Deserialize(reader);

                return o["roots"] != null;
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

                if (o["roots"] == null)
                {
                    throw new InvalidDataException("Invalid input format");
                }

                List<BookmarkDirectory> directories = new JToken[] { o["roots"]["bookmark_bar"], o["roots"]["other"] }.ToList()
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
            if (type == "folder")
            {
                return this.GetDirectory(token);
            }
            else if (type == "url")
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
                foreach (var child in token["children"])
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
            bookmark.Url = token["url"].ToString();

            return bookmark;
        }

        protected void SetBookmarkNodeProperties(BookmarkNode node, JToken token)
        {
            node.Name = token["name"].ToString();
        }
    }
}
