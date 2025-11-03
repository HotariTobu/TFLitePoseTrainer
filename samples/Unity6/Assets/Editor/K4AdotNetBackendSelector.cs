using System.Collections.Generic;
using System.Text.RegularExpressions;

using UnityEditor;

namespace Assets.Editor
{
    static class K4AdotNetBackendSelector
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

        internal static void UpdateImporters(IEnumerable<PluginImporter> importers)
        {
            foreach (var importer in importers)
            {
                var deviceType = GetDeviceType(importer.assetPath);
                if (!deviceType.HasValue)
                {
                    continue;
                }

                var newEnabled = deviceType.Value == SELECTED_DEVICE_TYPE;
                if (importer.GetEnabled() == newEnabled)
                {
                    continue;
                }

                importer.SetEnabled(newEnabled);

                importer.SaveAndReimport();
            }
        }

        enum DeviceType
        {
            Azure,
            Femto,
        }
    }
}
