using UnityEngine;

namespace Project.Scripts.Model
{
    /// <summary>
    /// 별 블록 기믹 데이터 클래스
    /// </summary>
    [System.Serializable]
    public class GimmickStarBlockData : GimmickData
    {
        [SerializeField] private int starId; // 별 ID (여러 별 구분용)
        [SerializeField] private int pointValue; // 별 획득 시 점수

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