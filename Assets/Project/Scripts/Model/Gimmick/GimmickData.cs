using UnityEngine;

namespace Project.Scripts.Model
{
    /// <summary>
    /// �⺻ ��� ������ Ŭ����
    /// </summary>
    [System.Serializable]
    public class GimmickData : IGimmickData
    {
         public string gimmickType;

        public string GimmickType { get => gimmickType; set => gimmickType = value; }

        public GimmickData()
        {
            gimmickType = "None";
        }

        public GimmickData(string type)
        {
            gimmickType = type;
        }

        /// <summary>
        /// ��� Ÿ�� ������ ��������
        /// </summary>
        public virtual ObjectPropertiesEnum.BlockGimmickType GetGimmickEnum()
        {
            if (System.Enum.TryParse(gimmickType, out ObjectPropertiesEnum.BlockGimmickType result))
            {
                return result;
            }
            return ObjectPropertiesEnum.BlockGimmickType.None;
        }
    }
}