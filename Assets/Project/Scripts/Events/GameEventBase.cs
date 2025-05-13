using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Events
{
    /// <summary>
    /// ScriptableObject ��� �̺�Ʈ ä���� �⺻ Ŭ����
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