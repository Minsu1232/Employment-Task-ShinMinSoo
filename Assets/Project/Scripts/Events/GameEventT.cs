using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Events
{
    /// <summary>
    /// 제네릭 타입 매개변수를 갖는 이벤트
    /// </summary>
    public abstract class GameEvent<T> : ScriptableObject
    {
        private readonly List<IGameEventListener<T>> eventListeners = new List<IGameEventListener<T>>();

        public void RegisterListener(IGameEventListener<T> listener)
        {
            if (!eventListeners.Contains(listener))
                eventListeners.Add(listener);
        }

        public void UnregisterListener(IGameEventListener<T> listener)
        {
            if (eventListeners.Contains(listener))
                eventListeners.Remove(listener);
        }

        public void Raise(T data)
        {
            for (int i = eventListeners.Count - 1; i >= 0; i--)
                eventListeners[i].OnEventRaised(data);
        }
    }

    public interface IGameEventListener<T>
    {
        void OnEventRaised(T data);
    }
}