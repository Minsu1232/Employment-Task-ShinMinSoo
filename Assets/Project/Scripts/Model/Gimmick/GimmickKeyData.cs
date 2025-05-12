using UnityEngine;

namespace Project.Scripts.Model
{
    /// <summary>
    /// ø≠ºË ±‚πÕ µ•¿Ã≈Õ ≈¨∑°Ω∫
    /// </summary>
    [System.Serializable]
    public class GimmickKeyData : GimmickData
    {
        [SerializeField] private int keyId;

        public int KeyId => keyId;

        public GimmickKeyData() : base("Key")
        {
            keyId = 0;
        }

        public GimmickKeyData(int keyId) : base("Key")
        {
            this.keyId = keyId;
        }

        public override ObjectPropertiesEnum.BlockGimmickType GetGimmickEnum()
        {
            return ObjectPropertiesEnum.BlockGimmickType.Key;
        }
    }
}