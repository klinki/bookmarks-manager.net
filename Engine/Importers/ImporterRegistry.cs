using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Importers
{
    public class ImporterRegistry
    {
        protected static ImporterRegistry instance;
        protected HashSet<IBookmarkImporter> registeredImporters;

        public static ImporterRegistry GetInstance()
        {
            if (instance == null)
            {
                instance = new ImporterRegistry();
            }

            return instance;
        }

        protected ImporterRegistry()
        {
            this.registeredImporters = new HashSet<IBookmarkImporter>();
        }

        public ImporterRegistry Register(IBookmarkImporter importer)
        {
            this.registeredImporters.Add(importer);

            return this;
        }

        public IBookmarkImporter GetImporterForFile(string filePath)
        {
            foreach (var importer in this.registeredImporters)
            {
                if (importer.CanHandleFile(filePath))
                {
                    return importer;
                }
            }

            return null;
        }
    }
}
