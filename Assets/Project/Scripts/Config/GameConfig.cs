using UnityEngine;
using Project.Scripts.Model;

namespace Project.Scripts.Config
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Config/GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [Header("���� ����")]
        public StageData[] stageDatas;

        [Header("������Ʈ ����")]
        public BoardConfig boardConfig;
        public BlockConfig blockConfig;
        public WallConfig wallConfig;
        public VisualConfig visualConfig;

        [Header("���� �̺�Ʈ")]
        public GameEvents gameEvents;
    }
}