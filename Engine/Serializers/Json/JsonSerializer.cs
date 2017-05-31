using Engine.Serializers.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Serializers
{
    public class JsonSerializer
    {
        public string Serialize(BookmarkNode node)
        {
            return this.Serialize(new BookmarkNode[] { node });
        }

        public string Serialize(IEnumerable<BookmarkNode> nodes)
        {
            var anonymous = new
            {
                Type = "Data",
                Version = "0.1",
                Data = new List<BookmarkNode>(nodes)
            };

            return JsonConvert.SerializeObject(anonymous, Formatting.Indented, new JsonSerializerSettings() {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Converters = new JsonConverter[] { new BookmarkNodeJsonConverter(), new StringEnumConverter() }
            });
        }
    }
}
