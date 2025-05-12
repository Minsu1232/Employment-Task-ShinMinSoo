using Project.Scripts.Model;
using UnityEngine;

namespace Project.Scripts.Presenter
{
    /// <summary>
    /// 모든 기믹 프레젠터의 기본 인터페이스
    /// </summary>
    public interface IGimmickPresenter
    {
        void Initialize(GimmickData gimmickData, GameObject targetObject);
        void Activate();
        void Deactivate();
        bool IsActive { get; }
        ObjectPropertiesEnum.BlockGimmickType GimmickType { get; }
        bool OnBlockMove(Vector3 position);
        void OnBlockPlace(Vector3 position);
        bool OnDestroyAttempt();
        void OnDestroyed();
    }
}