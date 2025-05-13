using UnityEngine;
using Project.Scripts.Events;
using Project.Scripts.Presenter;

namespace Project.Scripts.Config
{
    [CreateAssetMenu(fileName = "GameEvents", menuName = "Config/GameEvents")]
    public class GameEvents : ScriptableObject
    {
        [Header("���� �帧 �̺�Ʈ")]
        public GameEvent onGameStart;
        public GameEvent onGamePause;
        public GameEvent onGameResume;
        public GameEvent onGameOver;

        [Header("�������� �̺�Ʈ")]
        public StageLoadEvent onStageLoad;
        public GameEvent onStageComplete;

        [Header("��� �̺�Ʈ")]
        public BlockDestroyEvent onBlockDestroy;
        public CheckDestroyEvent onCheckDestroy;
        public BlockDragEvent onBlockDragStart;
        public BlockDragEvent onBlockDragEnd;
        public BlockDropEvent onBlockDrop;
    }
}