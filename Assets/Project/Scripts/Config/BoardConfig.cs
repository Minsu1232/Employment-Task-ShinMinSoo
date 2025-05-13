using UnityEngine;

namespace Project.Scripts.Config
{
    [CreateAssetMenu(fileName = "BoardConfig", menuName = "Config/BoardConfig")]
    public class BoardConfig : ScriptableObject
    {
        [Header("보드 프리팹")]
        public GameObject boardBlockPrefab;

        [Header("보드 설정")]
        public float blockDistance = 0.79f;

  
    }
}