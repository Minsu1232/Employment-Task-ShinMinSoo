using Project.Scripts.Controller;
using UnityEngine;

namespace Project.Scripts.Events
{
    [CreateAssetMenu(fileName = "CheckDestroyEvent", menuName = "Events/Check Destroy Event")]
    public class CheckDestroyEvent : GameEvent<(BoardBlockObject boardBlock, BlockObject block)> { }
}