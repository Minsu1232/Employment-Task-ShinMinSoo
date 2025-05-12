using UnityEngine;

namespace Project.Scripts.Model
{
    /// <summary>
    /// �ڹ��� ��� ������ Ŭ����
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
        /// �ڹ��� ī��Ʈ ����
        /// </summary>
        /// <returns>ī��Ʈ�� 0�� �Ǹ� true ��ȯ (��� ����)</returns>
        public bool DecreaseCount()
        {
            count--;
            return count <= 0;
        }
    }
}