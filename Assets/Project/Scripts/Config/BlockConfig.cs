using UnityEngine;

namespace Project.Scripts.Config
{
    [CreateAssetMenu(fileName = "BlockConfig", menuName = "Config/BlockConfig")]
    public class BlockConfig : ScriptableObject
    {
        [Header("��� ������")]
        public GameObject blockGroupPrefab;
        public GameObject blockPrefab;

        [Header("��� ��Ƽ����")]
        public Material[] blockMaterials;
        public Material[] testBlockMaterials;

        [Header("��� ���� ����")]
        public float maxSpeed = 20f;
        public float moveSpeed = 25f;
        public float followSpeed = 30f;
        public float collisionResetTime = 0.1f;
    }
}