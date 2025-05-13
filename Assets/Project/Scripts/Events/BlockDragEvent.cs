using Project.Scripts.Controller;
using UnityEngine;

namespace Project.Scripts.Events
{
    [CreateAssetMenu(fileName = "BlockDragEvent", menuName = "Events/Block Drag Event")]
    public class BlockDragEvent : GameEvent<BlockObject> { }
}