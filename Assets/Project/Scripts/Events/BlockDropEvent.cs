using Project.Scripts.Events;
using UnityEngine;
namespace Project.Scripts.Events
{
    [CreateAssetMenu(fileName = "BlockDropEvent", menuName = "Events/Block Drop Event")]
    public class BlockDropEvent : GameEvent<BlockObject> { }
}
