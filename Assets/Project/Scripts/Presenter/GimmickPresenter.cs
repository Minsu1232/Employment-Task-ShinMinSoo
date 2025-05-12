using Project.Scripts.Model;
using UnityEngine;

namespace Project.Scripts.Presenter
{
    /// <summary>
    /// 기본 기믹 프레젠터 추상 클래스
    /// </summary>
    public abstract class GimmickPresenter : MonoBehaviour, IGimmickPresenter
    {
        protected GimmickData gimmickData;
        protected GameObject targetObject;
        protected bool isActive = true;

        public bool IsActive => isActive;
        public abstract ObjectPropertiesEnum.BlockGimmickType GimmickType { get; }

        public virtual void Initialize(GimmickData gimmickData, GameObject targetObject)
        {
            this.gimmickData = gimmickData;
            this.targetObject = targetObject;
        }

        public virtual void Activate()
        {
            isActive = true;
        }

        public virtual void Deactivate()
        {
            isActive = false;
        }

        /// <summary>
        /// 블록 이동 시도 시 호출
        /// </summary>
        /// <returns>이동 가능 여부</returns>
        public virtual bool OnBlockMove(Vector3 position)
        {
            // 기본 구현은 항상 이동 허용
            return true;
        }

        /// <summary>
        /// 블록 배치 시 호출
        /// </summary>
        public virtual void OnBlockPlace(Vector3 position)
        {
            // 기본 구현은 아무 작업 없음
        }

        /// <summary>
        /// 파괴 시도 시 호출
        /// </summary>
        /// <returns>파괴 가능 여부</returns>
        public virtual bool OnDestroyAttempt()
        {
            // 기본 구현은 항상 파괴 허용
            return true;
        }

        /// <summary>
        /// 파괴 성공 시 호출
        /// </summary>
        public virtual void OnDestroyed()
        {
            // 기본 구현은 아무 작업 없음
        }
    }
}