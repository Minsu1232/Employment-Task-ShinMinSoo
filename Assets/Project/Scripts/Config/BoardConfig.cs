using UnityEngine;

namespace Project.Scripts.Config
{
    [CreateAssetMenu(fileName = "BoardConfig", menuName = "Config/BoardConfig")]
    public class BoardConfig : ScriptableObject
    {
        [Header("���� ������")]
        public GameObject boardBlockPrefab;

        [Header("���� ����")]
        public float blockDistance = 0.79f;

  
    }
}