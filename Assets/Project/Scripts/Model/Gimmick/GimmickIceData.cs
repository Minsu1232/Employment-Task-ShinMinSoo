using UnityEngine;

namespace Project.Scripts.Model
{
    /// <summary>
    /// ���� ��� ������ Ŭ����
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
        /// ���� ī��Ʈ ����
        /// </summary>
        /// <returns>ī��Ʈ�� 0�� �Ǹ� true ��ȯ</returns>
        public bool DecreaseCount()
        {
            count--;
            return count <= 0;
        }
    }
}