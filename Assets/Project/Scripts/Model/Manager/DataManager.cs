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

        // 캐싱된 스테이지 데이터
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
        /// 스테이지 데이터 비동기 로드
        /// </summary>
        public async Task<StageData> LoadStageDataAsync(int stageIndex)
        {
            // 캐시 확인
            if (stageDataCache.TryGetValue(stageIndex, out StageData cachedData))
            {
                return cachedData;
            }

            // 리소스에서 로드
            string path = $"Data/StageData SO/StageData_{stageIndex}";
            StageData stageData = null;

            try
            {
                stageData = Resources.Load<StageData>(path);

                if (stageData == null)
                {
                    // JSON 파일 시도
                    return await LoadStageDataFromJsonAsync($"Data/Json/Stage_{stageIndex}");
                }

                // 캐시에 저장
                stageDataCache[stageIndex] = stageData;
                return stageData;
            }
            catch (Exception ex)
            {
                Debug.LogError($"스테이지 데이터 로드 오류: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// JSON에서 스테이지 데이터 로드
        /// </summary>
        public async Task<StageData> LoadStageDataFromJsonAsync(string jsonPath)
        {
            try
            {
                TextAsset jsonFile = Resources.Load<TextAsset>(jsonPath);
                if (jsonFile == null)
                {
                    Debug.LogError($"JSON 파일을 찾을 수 없습니다: {jsonPath}");
                    return null;
                }

                // JSON을 StageData로 변환
                StageData stageData = StageData.FromJson(jsonFile.text);

                // 캐시에 저장
                stageDataCache[stageData.stageIndex] = stageData;

                return stageData;
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSON 로드 오류: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 스테이지 데이터 저장
        /// </summary>
        public void SaveStageData(StageData stageData, string path)
        {
#if UNITY_EDITOR
            // 저장할 폴더 생성
            string directory = System.IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            // 에셋으로 저장
            UnityEditor.AssetDatabase.CreateAsset(stageData, path);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();

            // 캐시 업데이트
            stageDataCache[stageData.stageIndex] = stageData;
#endif
        }

        /// <summary>
        /// JSON으로 스테이지 데이터 저장
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

                // 캐시 업데이트
                stageDataCache[stageData.stageIndex] = stageData;
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSON 저장 오류: {ex.Message}");
            }
        }
    }
}