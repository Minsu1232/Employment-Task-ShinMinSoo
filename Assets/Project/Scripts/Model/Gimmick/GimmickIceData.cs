using UnityEngine;

namespace Project.Scripts.Model
{
    /// <summary>
    /// 얼음 기믹 데이터 클래스
    /// </summary>
    [System.Serializable]
    public class GimmickIceData : GimmickData
    {
        [SerializeField] private int count;

        public int Count => count;

        public GimmickIceData() : base("Frozen")
        {
            count = 1;
        }

        public GimmickIceData(int count) : base("Frozen")
        {
            this.count = count;
        }

        public override ObjectPropertiesEnum.BlockGimmickType GetGimmickEnum()
        {
            return ObjectPropertiesEnum.BlockGimmickType.Frozen;
        }

        /// <summary>
        /// 얼음 카운트 감소
        /// </summary>
        /// <returns>카운트가 0이 되면 true 반환</returns>
        public bool DecreaseCount()
        {
            count--;
            return count <= 0;
        }
    }
}