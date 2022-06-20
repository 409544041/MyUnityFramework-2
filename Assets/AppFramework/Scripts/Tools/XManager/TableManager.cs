using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableManager : SingletonMono<TableManager>
{
    private const string path_AppTableConfig = "App/Assets/AppTableConfig";
    private Dictionary<string, byte[]> m_TablePairs = new Dictionary<string, byte[]>();
    private AppTableConfig appTableConfig;
    private void Awake()
    {
        TextAsset config = AssetsManager.Instance.LoadAsset<TextAsset>(path_AppTableConfig);
        appTableConfig = XmlSerializeManager.ProtoDeSerialize<AppTableConfig>(config.bytes);
        for (int i = 0; i < appTableConfig.AppTable.Count; i++)
        {
            var bytes = AssetsManager.Instance.LoadAsset<TextAsset>(appTableConfig.AppTable[i].TablePath).bytes;
            if (m_TablePairs.ContainsKey(appTableConfig.AppTable[i].TableName))
            {
                m_TablePairs[appTableConfig.AppTable[i].TableName] = bytes;
            }
            else
            {
                m_TablePairs.Add(appTableConfig.AppTable[i].TableName, bytes);
            }
        }
    }
    public void InitManager()
    {
        transform.SetParent(App.app.transform);
    }
    public T GetTable<T>(string tableName) where T : Component
    {
        return XmlSerializeManager.ProtoDeSerialize<T>(m_TablePairs[tableName]);
    }
}
