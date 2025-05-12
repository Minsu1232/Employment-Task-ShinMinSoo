using System;
using UnityEngine;

namespace Project.Scripts.Presenter
{
    /// <summary>
    /// 게임 이벤트 시스템
    /// </summary>
    public static class GameEvents
    {
        // 키 관련 이벤트
        public static Action<int> OnKeyCollected;

        // 자물쇠 관련 이벤트
        public static Action<int> OnLockUnlocked;

        // 별 관련 이벤트
        public static Action<int, int> OnStarCollected;

        // 다중 블록 관련 이벤트
        public static Action<GameObject, ColorType> OnMultipleBlockDestroyed;

        // 게임 진행 관련 이벤트
        public static Action OnLevelCompleted;
        public static Action OnGameOver;
    }
}