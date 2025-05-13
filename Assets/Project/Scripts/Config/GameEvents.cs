using UnityEngine;
using Project.Scripts.Events;
using Project.Scripts.Presenter;

namespace Project.Scripts.Config
{
    [CreateAssetMenu(fileName = "GameEvents", menuName = "Config/GameEvents")]
    public class GameEvents : ScriptableObject
    {
        [Header("게임 흐름 이벤트")]
        public GameEvent onGameStart;
        public GameEvent onGamePause;
        public GameEvent onGameResume;
        public GameEvent onGameOver;

        [Header("스테이지 이벤트")]
        public StageLoadEvent onStageLoad;
        public GameEvent onStageComplete;

        [Header("블록 이벤트")]
        public BlockDestroyEvent onBlockDestroy;
        public CheckDestroyEvent onCheckDestroy;
        public BlockDragEvent onBlockDragStart;
        public BlockDragEvent onBlockDragEnd;
        public BlockDropEvent onBlockDrop;
    }
}