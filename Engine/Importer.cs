using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace Engine
{
    public interface IBookmarkImporter
    {
        BookmarkDirectory FromFile(string path);
        bool CanHandleFile(string path);
    }

    public abstract class AbstractBookmarkImporter : IBookmarkImporter
    {
        public abstract BookmarkDirectory FromFile(string path);

        public abstract bool CanHandleFile(string path);

        protected BookmarkDirectory CreateRootDirectory(string path)
        {
            int lastSlash = path.LastIndexOf(Path.DirectorySeparatorChar);
            string fileName = path.Substring(lastSlash + 1);

            return new BookmarkDirectory("Imported from " + fileName);
        }
    }
}
