using System;

namespace Engine.Importers
{
    public class XmarksImporter : AbstractBookmarkImporter
    {
        public override bool CanHandleFile(string path)
        {
            throw new NotImplementedException();
        }

        public override BookmarkDirectory FromFile(string path)
        {
            var root = this.CreateRootDirectory(path);

            return root;
        }
    }
}
