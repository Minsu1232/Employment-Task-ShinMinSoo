using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Events
{
    /// <summary>
    /// ScriptableObject 기반 이벤트 채널의 기본 클래스
    /// </summary>
    public abstract class GameEventBase : ScriptableObject
    {
        protected readonly List<IGameEventListener> eventListeners = new List<IGameEventListener>();

        public void RegisterListener(IGameEventListener listener)
        {
            if (!eventListeners.Contains(listener))
                eventListeners.Add(listener);
        }

        public void UnregisterListener(IGameEventListener listener)
        {
            if (eventListeners.Contains(listener))
                eventListeners.Remove(listener);
        }
    }

    public interface IGameEventListener
    {
        void OnEventRaised();
    }
}