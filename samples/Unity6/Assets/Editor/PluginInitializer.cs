using UnityEditor;

namespace Assets.Editor
{
    static class PluginInitializer
    {
        [InitializeOnLoadMethod]
        internal static void Initialize()
        {
            var importers = PluginImporter.GetAllImporters();
            K4AdotNetBackendSelector.UpdateImporters(importers);
            DistinctNativePluginSelector.UpdateImporters(importers);
        }
    }
}
