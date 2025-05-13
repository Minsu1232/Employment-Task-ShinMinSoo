using UnityEngine;

namespace Project.Scripts.Config
{
    [CreateAssetMenu(fileName = "WallConfig", menuName = "Config/WallConfig")]
    public class WallConfig : ScriptableObject
    {
        [Header("�� ������")]
        public GameObject[] wallPrefabs;

        [Header("�� ��Ƽ����")]
        public Material[] wallMaterials;
    }
}