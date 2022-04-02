using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class FindSameFileNameWindowEditor : EditorWindow
{
    private static GUIStyle titleStyle;

    #region 2������ͬ���ļ�
    private string prefabsPath = "";
    private enum ObjType
    {
        All = 0,
        Prefab = 1,
        Texture = 2,
        AudioClip = 3,
        VideoClip = 4,
        Scene = 5,
        Material = 6,
        Model = 7,
        AnimationClip = 8,
        Shader = 9,
        TextAsset = 10,
        AnimatorController = 11,
        PhysicMaterial = 12,
        PhysicsMaterial2D = 13
    }

    private string[] items = {
        "t:Prefab t:Texture t:AudioClip t:Scene t:Material t:Model t:AnimationClip t:Shader t:TextAsset t:AnimatorController t:PhysicMaterial t:PhysicsMaterial2D",
        "t:Prefab",
        "t:Texture",
        "t:AudioClip",
        "t:VideoClip",
        "t:Scene",
        "t:Material",
        "t:Model",
        "t:AnimationClip",
        "t:Shader",
        "t:TextAsset",
        "t:AnimatorController",
        "t:PhysicMaterial",
        "t:PhysicsMaterial2D" };

    private ObjType objType = ObjType.All;

    private static Dictionary<string, string> FileNames = new Dictionary<string, string>();

    private string filesPath = "";
    #endregion

    [MenuItem("Tools/My ToolsWindow/FindSameFileName", false, 2)]
    public static void OpenWindow()
    {
        //texture = AssetDatabase.LoadAssetAtPath("Assets/Editor/Tools/Image/bg.png", typeof(Texture)) as Texture;
        titleStyle = new GUIStyle("OL Title")
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            fontSize = 12
        };

        FindSameFileNameWindowEditor window = GetWindow<FindSameFileNameWindowEditor>("Find Same File Name");
        window.Show();
    }
    private void OnGUI()
    {
        #region 2������ͬ���ļ�
        GUILayout.BeginVertical();

        objType = (ObjType)EditorGUILayout.EnumPopup("ObjectType", objType);

        EditorGUILayout.Space();

        filesPath = EditorGUILayout.TextField("Files Path", filesPath);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Browse", GUILayout.MaxWidth(75f)))
        {
            var newPath = EditorUtility.OpenFolderPanel("Files Folder", prefabsPath, string.Empty);
            if (!string.IsNullOrEmpty(newPath))
            {
                var gamePath = System.IO.Path.GetFullPath(".");
                gamePath = gamePath.Replace("\\", "/");
                if (newPath.StartsWith(gamePath) && newPath.Length > gamePath.Length)
                    newPath = newPath.Remove(0, gamePath.Length + 1);
                filesPath = newPath;
            }
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();
        if (GUILayout.Button("Find Same File Name(����ͬ���ļ�)"))
        {

            EditorApplication.delayCall += FindSameFileName;
        }

        GUILayout.EndVertical();
        #endregion

    }
    #region 2������ͬ���ļ�
    private void FindSameFileName()
    {
        string filter = items[(int)objType];
        string[] allPath = AssetDatabase.FindAssets(filter, new string[] { filesPath });
        for (int i = 0; i < allPath.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(allPath[i]);
            if (path.Contains("Plugins")) continue;
            var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (!FileNames.ContainsKey(obj.name))
            {
                FileNames.Add(obj.name, path);
            }
            else
            {
                Debug.LogFormat("�ظ��ļ�:{0}\n·��:{1}    {2}", obj.name, path, FileNames[obj.name]);
            }
        }
        Debug.Log("��������");
        FileNames.Clear();
    }
    #endregion
}
