using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System;
using Engine.Serializers.Json;

namespace Engine.Importers
{
    public class BookmarksManagerImporter : AbstractBookmarkImporter
    {
        static BookmarksManagerImporter()
        {
            ImporterRegistry.GetInstance().Register(new BookmarksManagerImporter());
        }

        public override bool canHandleFile(string path)
        {
            using (StreamReader streamReader = new StreamReader(path, Encoding.UTF8))
            using (JsonTextReader reader = new JsonTextReader(streamReader))
            {
                JsonSerializer serializer = new JsonSerializer();
                var o = (JToken)serializer.Deserialize(reader);

                return o["Type"]?.ToString() == BookmarkNodeJsonConverter.BookmarkManagerType;
            }
        }

        public override BookmarkDirectory fromFile(string path)
        {
            var root = this.createRootDirectory(path);

            using (StreamReader streamReader = new StreamReader(path, Encoding.UTF8))
            {
                string data = streamReader.ReadToEnd();

                var deserialized = JsonConvert.DeserializeObject(data, typeof(BookmarkNode), new JsonConverter[] { new BookmarkNodeJsonConverter() });
                
            }

            return root;
        }
    }
}
