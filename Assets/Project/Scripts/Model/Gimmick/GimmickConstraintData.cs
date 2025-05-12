using UnityEngine;

namespace Project.Scripts.Model
{
    /// <summary>
    /// ø≠ºË ±‚πÕ µ•¿Ã≈Õ ≈¨∑°Ω∫
    /// </summary>
    [System.Serializable]
    public class GimmickConstraintData : GimmickData
    {
        [SerializeField] private bool isWidth;

        public bool IsWidth => isWidth;

        public GimmickConstraintData() : base("Constraint")
        {
            isWidth = false;
        }

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