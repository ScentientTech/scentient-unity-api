using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Scentient
{

    /// <summary>
    /// Uneeded and unused at the moment using Resources dir in the package
    /// 
    /// Was an IPreprocessBuildWithReport interface. Readd to reenable
    /// </summary>
    class EmbedScentTable 
    {
        public int callbackOrder { get { return 0; } }
        public void OnPreprocessBuild(BuildReport report)
        {
            var path = "Packages/scentient-unity-api/docs/scent-table_en.csv";
            var resourceDir = Path.Join(Application.dataPath, "Resources");
            var resourcePath = Path.Join(resourceDir, "scent-table_en.csv");

            if (!Directory.Exists(resourceDir))
            {
                Directory.CreateDirectory(resourceDir);
            }
            if (!File.Exists(resourcePath))
            {
                File.Copy(path, resourcePath);
                AssetDatabase.Refresh();
            }
            Debug.Log("MyCustomBuildProcessor.OnPreprocessBuild for target " + report.summary.platform + " at path " + report.summary.outputPath);
        }
    }
}