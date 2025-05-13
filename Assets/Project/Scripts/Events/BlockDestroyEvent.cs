using UnityEngine;

namespace Project.Scripts.Events
{
    [CreateAssetMenu(fileName = "BlockDestroyEvent", menuName = "Events/Block Destroy Event")]
    public class BlockDestroyEvent : GameEvent<BlockObject> { }
}