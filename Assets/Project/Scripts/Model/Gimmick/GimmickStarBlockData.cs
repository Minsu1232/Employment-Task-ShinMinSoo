using UnityEngine;

namespace Project.Scripts.Model
{
    /// <summary>
    /// �� ��� ��� ������ Ŭ����
    /// </summary>
    [System.Serializable]
    public class GimmickStarBlockData : GimmickData
    {
        [SerializeField] private int starId; // �� ID (���� �� ���п�)
        [SerializeField] private int pointValue; // �� ȹ�� �� ����

        public int StarId => starId;
        public int PointValue => pointValue;

        public GimmickStarBlockData() : base("Star")
        {
            starId = 0;
            pointValue = 100;
        }

        public GimmickStarBlockData(int starId, int pointValue = 100) : base("Star")
        {
            this.starId = starId;
            this.pointValue = pointValue;
        }

        public override ObjectPropertiesEnum.BlockGimmickType GetGimmickEnum()
        {
            return ObjectPropertiesEnum.BlockGimmickType.Star;
        }
    }
}