using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace Engine
{
    public interface BookmarkImporter
    {
        BookmarkDirectory fromFile(string path);
        bool canHandleFile(string path);
    }

    public abstract class AbstractBookmarkImporter : BookmarkImporter
    {
        public abstract BookmarkDirectory fromFile(string path);

        public abstract bool canHandleFile(string path);

        protected BookmarkDirectory createRootDirectory(string path)
        {
            int lastSlash = path.LastIndexOf(Path.DirectorySeparatorChar);
            string fileName = path.Substring(lastSlash + 1);

            return new BookmarkDirectory("Imported from " + fileName);
        }
    }
}
