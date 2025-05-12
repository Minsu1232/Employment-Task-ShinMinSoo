using UnityEngine;

namespace Project.Scripts.Controller
{
    /// <summary>
    /// ���� ������ ����ϴ� ��Ʈ�ѷ�
    /// </summary>
    public class LevelController : MonoBehaviour
    {
        [SerializeField] private int maxLevel = 5;

        private GameController gameController;

        // ���� ����
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
            // ���� ���� �� UI ������Ʈ �� ó��
            Debug.Log($"���� �����: {CurrentLevel}");
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