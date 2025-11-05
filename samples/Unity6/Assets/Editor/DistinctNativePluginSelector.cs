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
            var pluginKeySet = new HashSet<(PluginImporterExtension.BuildArchitecture?, string filename)>();

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

                var buildArchitecture = importer.GetBuildArchitecture();
                var pluginFilename = Path.GetFileName(importer.assetPath);

                var pluginKey = (buildArchitecture, pluginFilename);
                if (!pluginKeySet.Contains(pluginKey))
                {
                    pluginKeySet.Add(pluginKey);
                    continue;
                }

                importer.SetEnabled(false);

                importer.SaveAndReimport();
            }
        }
    }
}
