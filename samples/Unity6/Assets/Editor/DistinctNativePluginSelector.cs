using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

using UnityEditor;

namespace Assets.Editor
{
    static class DistinctNativePluginSelector
    {
        static readonly Regex PLUGIN_PATH_REGEX = new(@"native.+?\.dll");

        internal static void UpdateImporters(IEnumerable<PluginImporter> importers)
        {
            var pluginFilenameSet = new HashSet<string>();

            foreach (var importer in importers)
            {
                if (!PLUGIN_PATH_REGEX.IsMatch(importer.assetPath))
                {
                    continue;
                }

                if (!importer.GetEnabled())
                {
                    continue;
                }

                var pluginFilename = Path.GetFileName(importer.assetPath);
                if (!pluginFilenameSet.Contains(pluginFilename))
                {
                    pluginFilenameSet.Add(pluginFilename);
                    continue;
                }

                importer.SetEnabled(false);
            }
        }
    }
}
