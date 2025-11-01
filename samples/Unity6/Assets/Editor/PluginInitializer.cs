using UnityEditor;

namespace Assets.Editor
{
    [InitializeOnLoad]
    static class PluginInitializer
    {
        static PluginInitializer()
        {
            var importers = PluginImporter.GetAllImporters();
            K4AdotNetBackendSelector.UpdateImporters(importers);
            DistinctNativePluginSelector.UpdateImporters(importers);
        }
    }
}
