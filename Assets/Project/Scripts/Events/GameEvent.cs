using UnityEngine;
using System.Collections.Generic;

namespace Project.Scripts.Events
{
    [CreateAssetMenu(fileName = "GameEvent", menuName = "Events/Game Event")]
    public class GameEvent : GameEventBase
    {
        public void Raise()
        {
            for (int i = eventListeners.Count - 1; i >= 0; i--)
                eventListeners[i].OnEventRaised();
        }
    }
}