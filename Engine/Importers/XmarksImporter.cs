using System;

namespace Engine.Importers
{
    public class XmarksImporter : AbstractBookmarkImporter
    {
        public override bool canHandleFile(string path)
        {
            throw new NotImplementedException();
        }

        public override BookmarkDirectory fromFile(string path)
        {
            var root = this.createRootDirectory(path);

            return root;
        }
    }
}
