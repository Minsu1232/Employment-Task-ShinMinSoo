namespace Project.Scripts.Model
{
    
    using UnityEngine;

    [System.Serializable]
    public abstract class PositionData : BaseData, IPositionData
    {
        public int x;
        public int y;

        public int X { get => x; set => x = value; }
        public int Y { get => y; set => y = value; }

        public PositionData() { }

        public PositionData(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        // 격자 좌표를 월드 좌표로 변환하는 유틸리티 함수
        public Vector3 ToWorldPosition(float gridSize = 0.79f)
        {
            return new Vector3(x * gridSize, 0, y * gridSize);
        }
    }
}