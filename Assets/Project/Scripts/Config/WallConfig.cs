using UnityEngine;

namespace Project.Scripts.Config
{
    [CreateAssetMenu(fileName = "WallConfig", menuName = "Config/WallConfig")]
    public class WallConfig : ScriptableObject
    {
        [Header("벽 프리팹")]
        public GameObject[] wallPrefabs;

        [Header("벽 머티리얼")]
        public Material[] wallMaterials;
    }
}