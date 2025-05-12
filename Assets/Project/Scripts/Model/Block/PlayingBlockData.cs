using Project.Scripts.Controller;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Model
{
    [System.Serializable]
    public class PlayingBlockData : BlockData
    {
        public Vector2Int center;
        public int uniqueIndex;
        public List<ShapeData> shapes = new List<ShapeData>();
        public List<GimmickData> gimmicks = new List<GimmickData>();

        public PlayingBlockData() : base() { }

        public PlayingBlockData(int x, int y, Vector2Int center, int uniqueIndex, ColorType colorType)
            : base(x, y, colorType)
        {
            this.center = center;
            this.uniqueIndex = uniqueIndex;
        }

        public void AddShape(ShapeData shape)
        {
            shapes.Add(shape);
        }

        public void AddGimmick(GimmickData gimmick)
        {
            gimmicks.Add(gimmick);
        }
    }
}