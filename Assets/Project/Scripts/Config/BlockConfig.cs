using UnityEngine;

namespace Project.Scripts.Config
{
    [CreateAssetMenu(fileName = "BlockConfig", menuName = "Config/BlockConfig")]
    public class BlockConfig : ScriptableObject
    {
        [Header("블록 프리팹")]
        public GameObject blockGroupPrefab;
        public GameObject blockPrefab;

        [Header("블록 머티리얼")]
        public Material[] blockMaterials;
        public Material[] testBlockMaterials;

        [Header("블록 물리 설정")]
        public float maxSpeed = 20f;
        public float moveSpeed = 25f;
        public float followSpeed = 30f;
        public float collisionResetTime = 0.1f;
    }
}