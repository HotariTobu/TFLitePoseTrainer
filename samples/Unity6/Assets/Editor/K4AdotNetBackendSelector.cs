using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using UnityEditor;

[InitializeOnLoad]
class K4AdotNetBackendSelector : AssetPostprocessor
{
    static readonly Regex K4A_PATH_REGEX = new(@"K4AdotNet(?:-(?<device_type>\w+))?");

#if AZURE
    const DeviceType SELECTED_DEVICE_TYPE = DeviceType.Azure;
#else
    const DeviceType SELECTED_DEVICE_TYPE = DeviceType.Femto;
#endif

    static DeviceType? GetDeviceType(string assetPath)
    {
        var match = K4A_PATH_REGEX.Match(assetPath);
        if (!match.Success)
        {
            return null;
        }

        if (match.Groups["device_type"].Success)
        {
            return DeviceType.Femto;
        }

        return DeviceType.Azure;
    }

    static bool GetEnabled(PluginImporter importer)
    {
        var compatibles = new bool[] {
            importer.GetCompatibleWithAnyPlatform(),
            importer.GetCompatibleWithEditor(),
            importer.GetCompatibleWithPlatform(BuildTarget.StandaloneWindows),
            importer.GetCompatibleWithPlatform(BuildTarget.StandaloneWindows64),
        };
        return compatibles.Any(compatible => compatible);
    }

    static void SetEnabled(PluginImporter importer, bool enabled)
    {
        importer.SetCompatibleWithAnyPlatform(false);
        importer.SetCompatibleWithEditor(enabled);
        importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows, enabled);
        importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows64, enabled);
    }

    static void UpdateImporters(IEnumerable<PluginImporter> importers)
    {
        foreach (var importer in importers)
        {
            var deviceType = GetDeviceType(importer.assetPath);
            if (!deviceType.HasValue)
            {
                continue;
            }

            var newEnabled = deviceType.Value == SELECTED_DEVICE_TYPE;
            if (GetEnabled(importer) == newEnabled)
            {
                continue;
            }

            SetEnabled(importer, newEnabled);

            importer.SaveAndReimport();
        }
    }

    static K4AdotNetBackendSelector()
    {
        var importers = PluginImporter.GetAllImporters();
        UpdateImporters(importers);
    }

    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
    {
        if (!didDomainReload)
        {
            return;
        }

        var importers = from assetPath in importedAssets
                        let importer = AssetImporter.GetAtPath(assetPath) as PluginImporter
                        where importer is not  null
                        select importer;
        UpdateImporters(importers);
    }

    enum DeviceType
    {
        Azure,
        Femto,
    }
}
