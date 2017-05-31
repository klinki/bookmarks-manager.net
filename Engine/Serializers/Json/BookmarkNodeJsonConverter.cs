using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Serializers.Json
{
    public class BookmarkNodeJsonConverter : JsonConverter
    {
        public static string BookmarkManagerType = "BookmarkManagerJsonFile";

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(BookmarkNode);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            // return serializer.Deserialize<Tycoon>(reader);

            var o = (JToken)serializer.Deserialize(reader);

            if (o["type"]?.ToString() != "Data")
            {
                throw new InvalidCastException();
            }


            List<BookmarkNode> nodes = o["Data"]
                .ToList()
                .ConvertAll(x => this.GetNode(x));

            return nodes;
        }

        protected BookmarkNode GetNode(JToken token, BookmarkDirectory parent)
        {
            var node = this.GetNode(token);
            parent.AddChild(node);

            return node;
        }

        protected BookmarkNode GetNode(JToken token)
        {
            var type = token["Type"].ToString();
            if (type == "Directory")
            {
                return this.GetDirectory(token);
            }
            else if (type == "Bookmark")
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

            if (token["Children"] != null)
            {
                foreach (var child in token["Children"])
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

            bookmark.Url = token["Url"].ToString();
            bookmark.LastVisited = int.Parse(token["LastVisited"].ToString());
            bookmark.CountVisits = int.Parse(token["CountVisits"].ToString());

            return bookmark;
        }

        protected void SetBookmarkNodeProperties(BookmarkNode node, JToken token)
        {
            node.Id = int.Parse(token["Id"].ToString());
            node.Name = token["Name"]?.ToString();
            node.Created = int.Parse(token["Created"].ToString());
            node.Modified = int.Parse(token["Modified"].ToString());
        }

        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            BookmarkNode node = value as BookmarkNode;

            if (node.Type == BookmarkType.Bookmark)
            {
                Bookmark bookmark = node as Bookmark;

                serializer.Serialize(writer, new {
                    bookmark.Id,
                    bookmark.Type,
                    bookmark.Name,
                    bookmark.Url,
                    bookmark.Created,
                    bookmark.Modified,
                    bookmark.LastVisited,
                    bookmark.CountVisits,
                    bookmark.Tags
                });
            }
            else
            {
                BookmarkDirectory directory = node as BookmarkDirectory;

                serializer.Serialize(writer, new
                {
                    directory.Id,
                    directory.Type,
                    directory.Name,
                    directory.Created,
                    directory.Modified,
                    directory.Children
                });
            }
        }
    }
}
