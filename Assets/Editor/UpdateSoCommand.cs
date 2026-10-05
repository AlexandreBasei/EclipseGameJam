using UnityEditor;
using UnityEngine;

public static class UpdateSoCommand
{
    [MenuItem("Tools/ObjectSO/Remplir les noms")]
    private static void Fill()
    {

        var allObject = AssetDatabase.FindAssets("t:ObjectSo");

        int count = 0;
        foreach (string guid in AssetDatabase.FindAssets("t:CommandSo"))
        {
            string commandPath = AssetDatabase.GUIDToAssetPath(guid);
            var commandSo = AssetDatabase.LoadAssetAtPath<CommandSO>(commandPath);

            string objectPath = AssetDatabase.GUIDToAssetPath(allObject[count]);
            var objectSo = AssetDatabase.LoadAssetAtPath<ObjectSO>(objectPath);

            if (commandSo.clientName == commandSo.name) continue;

            commandSo.clientName = commandSo.name;
            commandSo.ObjectSO = objectSo;
            commandSo.moneyReward = objectSo.tags.Count() * 50;
            EditorUtility.SetDirty(commandSo);
            count++;
        }
        AssetDatabase.SaveAssets();
    }
}