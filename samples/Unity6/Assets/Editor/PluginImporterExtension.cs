using UnityEditor;

namespace Assets.Editor
{
    static class PluginImporterExtension
    {
        internal static bool GetEnabled(this PluginImporter importer)
        {
            return importer.GetCompatibleWithEditor() ||
                   importer.GetCompatibleWithPlatform(BuildTarget.StandaloneWindows) ||
                   importer.GetCompatibleWithPlatform(BuildTarget.StandaloneWindows64);
        }

        internal static void SetEnabled(this PluginImporter importer, bool enabled)
        {
            importer.SetCompatibleWithAnyPlatform(false);
            importer.SetCompatibleWithEditor(enabled);
            importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows, enabled);
            importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows64, enabled);
        }
    }
}
