using UnityEngine;
using Project.Scripts.Model;

namespace Project.Scripts.Config
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Config/GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [Header("게임 설정")]
        public StageData[] stageDatas;

        [Header("컴포넌트 설정")]
        public BoardConfig boardConfig;
        public BlockConfig blockConfig;
        public WallConfig wallConfig;
        public VisualConfig visualConfig;

        [Header("게임 이벤트")]
        public GameEvents gameEvents;
    }
}