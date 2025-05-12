using System;
using UnityEngine;

namespace Project.Scripts.Presenter
{
    /// <summary>
    /// ���� �̺�Ʈ �ý���
    /// </summary>
    public static class GameEvents
    {
        // Ű ���� �̺�Ʈ
        public static Action<int> OnKeyCollected;

        // �ڹ��� ���� �̺�Ʈ
        public static Action<int> OnLockUnlocked;

        // �� ���� �̺�Ʈ
        public static Action<int, int> OnStarCollected;

        // ���� ��� ���� �̺�Ʈ
        public static Action<GameObject, ColorType> OnMultipleBlockDestroyed;

        // ���� ���� ���� �̺�Ʈ
        public static Action OnLevelCompleted;
        public static Action OnGameOver;
    }
}