using UnityEngine;

namespace Project.Scripts.Model
{
    /// <summary>
    /// 자물쇠 기믹 데이터 클래스
    /// </summary>
    [System.Serializable]
    public class GimmickLockData : GimmickData
    {
        [SerializeField] private int lockId;
        [SerializeField] private int count;

        public int LockId => lockId;
        public int Count => count;

        public GimmickLockData() : base("Lock")
        {
            lockId = 0;
            count = 1;
        }

        public GimmickLockData(int lockId, int count = 1) : base("Lock")
        {
            this.lockId = lockId;
            this.count = count;
        }

        public override ObjectPropertiesEnum.BlockGimmickType GetGimmickEnum()
        {
            return ObjectPropertiesEnum.BlockGimmickType.Lock;
        }

        /// <summary>
        /// 자물쇠 카운트 감소
        /// </summary>
        /// <returns>카운트가 0이 되면 true 반환 (잠금 해제)</returns>
        public bool DecreaseCount()
        {
            count--;
            return count <= 0;
        }
    }
}