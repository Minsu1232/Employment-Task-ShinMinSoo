using Project.Scripts.Controller;
using UnityEngine;

namespace Project.Scripts.Model
{
    [System.Serializable]
    public class BlockData : PositionData, IColorableData
    {
        [SerializeField] public ColorType colorType;

        public ColorType ColorType { get => colorType; set => colorType = value; }

        public BlockData() : base() { }

        public BlockData(int x, int y, ColorType colorType) : base(x, y)
        {
            this.colorType = colorType;
        }
    }
}