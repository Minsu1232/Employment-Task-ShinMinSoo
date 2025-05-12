using UnityEngine;

namespace Project.Scripts.Controller
{
    /// <summary>
    /// 레벨 관리를 담당하는 컨트롤러
    /// </summary>
    public class LevelController : MonoBehaviour
    {
        [SerializeField] private int maxLevel = 5;

        private GameController gameController;

        // 현재 레벨
        public int CurrentLevel { get; private set; } = 0;
        public int MaxLevel => maxLevel;

        public void Initialize(GameController controller)
        {
            gameController = controller;
        }

        public void SetCurrentLevel(int level)
        {
            if (level > 0)
            {
                CurrentLevel = level;
                OnLevelChanged();
            }
        }

        private void OnLevelChanged()
        {
            // 레벨 변경 시 UI 업데이트 등 처리
            Debug.Log($"레벨 변경됨: {CurrentLevel}");
        }

        public bool CanGoToNextLevel()
        {
            return CurrentLevel < maxLevel;
        }

        public bool CanGoToPreviousLevel()
        {
            return CurrentLevel > 1;
        }
    }
}