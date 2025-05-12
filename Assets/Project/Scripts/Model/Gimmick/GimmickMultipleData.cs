using Project.Scripts.Controller;
using UnityEngine;

namespace Project.Scripts.Model
{
    /// <summary>
    /// ���� ��� ������ Ŭ����
    /// </summary>
    [System.Serializable]
    public class GimmickMultipleData : GimmickData
    {
        [SerializeField] private ColorType colorType;

        public ColorType ColorType => colorType;

        public GimmickMultipleData() : base("Multiple") { }

        public GimmickMultipleData(ColorType colorType) : base("Multiple")
        {
            this.colorType = colorType;
        }

        public override ObjectPropertiesEnum.BlockGimmickType GetGimmickEnum()
        {
            return ObjectPropertiesEnum.BlockGimmickType.Multiple;
        }
    }
}