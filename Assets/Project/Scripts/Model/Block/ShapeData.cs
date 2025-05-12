using UnityEngine;

namespace Project.Scripts.Model
{
    [System.Serializable]
    public class ShapeData
    {
        public Vector2Int offset;

        public ShapeData() { }

        public ShapeData(int x, int y)
        {
            offset = new Vector2Int(x, y);
        }
    }
}