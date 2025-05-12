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

        // ���� ��ǥ�� ���� ��ǥ�� ��ȯ�ϴ� ��ƿ��Ƽ �Լ�
        public Vector3 ToWorldPosition(float gridSize = 0.79f)
        {
            return new Vector3(x * gridSize, 0, y * gridSize);
        }
    }
}