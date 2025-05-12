using UnityEngine;

namespace Project.Scripts.Model
{
    /// <summary>
    /// 기본 기믹 데이터 클래스
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
        /// 기믹 타입 열거형 가져오기
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