using Project.Scripts.Events;
using UnityEngine;
namespace Project.Scripts.Events
{
    [CreateAssetMenu(fileName = "StageLoadEvent", menuName = "Events/Stage Load Event")]
    public class StageLoadEvent : GameEvent<int> { }
}
