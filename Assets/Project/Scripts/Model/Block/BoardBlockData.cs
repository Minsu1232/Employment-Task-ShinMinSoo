using Project.Scripts.Controller;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Model
{
    [System.Serializable]
    public class BoardBlockData : BlockData
    {
        [SerializeField] private List<ColorType> colorTypes = new List<ColorType>();
        [SerializeField] private List<int> dataTypes = new List<int>();
        [SerializeField] private bool isCheckBlock;

        public List<ColorType> ColorTypes => colorTypes;
        public List<int> DataTypes => dataTypes;
        public bool IsCheckBlock => isCheckBlock;

        public BoardBlockData() : base() { }

        public BoardBlockData(int x, int y) : base(x, y, ColorType.None)
        {
        }

        public void AddColorType(ColorType type, int dataType)
        {
            colorTypes.Add(type);
            dataTypes.Add(dataType);
        }

        public void SetIsCheckBlock(bool value)
        {
            isCheckBlock = value;
        }
        public enum DestroyWallDirection
        {
            None = 0,
            Up = 1,
            Down = 2,
            Left = 3,
            Right = 4
        }
    }
}