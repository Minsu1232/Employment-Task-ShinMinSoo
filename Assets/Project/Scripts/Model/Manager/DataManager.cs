using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Project.Scripts.Model
{
    public class DataManager : MonoBehaviour
    {
        private static DataManager instance;
        public static DataManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("DataManager");
                    instance = go.AddComponent<DataManager>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        // ĳ�̵� �������� ������
        private Dictionary<int, StageData> stageDataCache = new Dictionary<int, StageData>();

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        /// <summary>
        /// �������� ������ �񵿱� �ε�
        /// </summary>
        public async Task<StageData> LoadStageDataAsync(int stageIndex)
        {
            // ĳ�� Ȯ��
            if (stageDataCache.TryGetValue(stageIndex, out StageData cachedData))
            {
                return cachedData;
            }

            // ���ҽ����� �ε�
            string path = $"Data/StageData SO/StageData_{stageIndex}";
            StageData stageData = null;

            try
            {
                stageData = Resources.Load<StageData>(path);

                if (stageData == null)
                {
                    // JSON ���� �õ�
                    return await LoadStageDataFromJsonAsync($"Data/Json/Stage_{stageIndex}");
                }

                // ĳ�ÿ� ����
                stageDataCache[stageIndex] = stageData;
                return stageData;
            }
            catch (Exception ex)
            {
                Debug.LogError($"�������� ������ �ε� ����: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// JSON���� �������� ������ �ε�
        /// </summary>
        public async Task<StageData> LoadStageDataFromJsonAsync(string jsonPath)
        {
            try
            {
                TextAsset jsonFile = Resources.Load<TextAsset>(jsonPath);
                if (jsonFile == null)
                {
                    Debug.LogError($"JSON ������ ã�� �� �����ϴ�: {jsonPath}");
                    return null;
                }

                // JSON�� StageData�� ��ȯ
                StageData stageData = StageData.FromJson(jsonFile.text);

                // ĳ�ÿ� ����
                stageDataCache[stageData.stageIndex] = stageData;

                return stageData;
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSON �ε� ����: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// �������� ������ ����
        /// </summary>
        public void SaveStageData(StageData stageData, string path)
        {
#if UNITY_EDITOR
            // ������ ���� ����
            string directory = System.IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            // �������� ����
            UnityEditor.AssetDatabase.CreateAsset(stageData, path);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();

            // ĳ�� ������Ʈ
            stageDataCache[stageData.stageIndex] = stageData;
#endif
        }

        /// <summary>
        /// JSON���� �������� ������ ����
        /// </summary>
        public void SaveStageDataAsJson(StageData stageData, string path)
        {
            try
            {
                string json = stageData.ToJson();

#if UNITY_EDITOR
                System.IO.File.WriteAllText(path, json);
                UnityEditor.AssetDatabase.Refresh();
#endif

                // ĳ�� ������Ʈ
                stageDataCache[stageData.stageIndex] = stageData;
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSON ���� ����: {ex.Message}");
            }
        }
    }
}