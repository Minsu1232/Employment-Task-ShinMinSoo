using UnityEngine;

namespace Project.Scripts.Model
{
    /// <summary>
    /// ���� ��� ������ Ŭ����
    /// </summary>
    [System.Serializable]
    public class GimmickConstraintData : GimmickData
    {
        [SerializeField] private bool isWidth;

        public bool IsWidth => isWidth;

        public GimmickConstraintData() : base("Constraint") { }

        public GimmickConstraintData(bool isWidth) : base("Constraint")
        {
            this.isWidth = isWidth;
        }

        public override ObjectPropertiesEnum.BlockGimmickType GetGimmickEnum()
        {
            return ObjectPropertiesEnum.BlockGimmickType.Constraint;
        }
    }
}