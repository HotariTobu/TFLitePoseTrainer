using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Assets.Editor
{
    class BuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            PluginInitializer.Initialize();
        }
    }
}
