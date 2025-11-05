using System.Text.RegularExpressions;

using UnityEditor;

namespace Assets.Editor
{
    static class PluginImporterExtension
    {
        static readonly Regex BUILD_ARCHITECTURE_REGEX = new(@"x86|x64");

        internal static BuildArchitecture? GetBuildArchitecture(this PluginImporter importer)
        {
            var match = BUILD_ARCHITECTURE_REGEX.Match(importer.assetPath);
            if (!match.Success)
            {
                return null;
            }

            return match.Value switch
            {
                "x86" => BuildArchitecture.X86,
                "x64" => BuildArchitecture.X64,
                _ => null,
            };
        }

        internal static bool GetEnabled(this PluginImporter importer)
        {
            if (importer.GetCompatibleWithAnyPlatform() || importer.GetCompatibleWithEditor())
            {
                return true;
            }

            var buildArchitecture = importer.GetBuildArchitecture();
            if (!buildArchitecture.HasValue)
            {
                return false;
            }

            return buildArchitecture.Value switch
            {
                BuildArchitecture.X86 => importer.GetCompatibleWithPlatform(BuildTarget.StandaloneWindows),
                BuildArchitecture.X64 => importer.GetCompatibleWithPlatform(BuildTarget.StandaloneWindows64),
                _ => importer.GetCompatibleWithPlatform(BuildTarget.StandaloneWindows) &&
                     importer.GetCompatibleWithPlatform(BuildTarget.StandaloneWindows64),
            };
        }

        internal static void SetEnabled(this PluginImporter importer, bool enabled)
        {
            importer.SetCompatibleWithAnyPlatform(false);
            importer.SetCompatibleWithEditor(enabled);

            var buildArchitecture = importer.GetBuildArchitecture();
            if (!buildArchitecture.HasValue)
            {
                return;
            }

            switch (buildArchitecture.Value)
            {
                case BuildArchitecture.X86:
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows, enabled);
                    break;
                case BuildArchitecture.X64:
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows64, enabled);
                    break;
                default:
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows, enabled);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows64, enabled);
                    break;
            }
        }

        internal enum BuildArchitecture
        {
            X86,
            X64,
        }
    }
}
