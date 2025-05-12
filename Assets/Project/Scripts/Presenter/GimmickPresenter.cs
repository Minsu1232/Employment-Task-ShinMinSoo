using Project.Scripts.Model;
using UnityEngine;

namespace Project.Scripts.Presenter
{
    /// <summary>
    /// �⺻ ��� �������� �߻� Ŭ����
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
        /// ��� �̵� �õ� �� ȣ��
        /// </summary>
        /// <returns>�̵� ���� ����</returns>
        public virtual bool OnBlockMove(Vector3 position)
        {
            // �⺻ ������ �׻� �̵� ���
            return true;
        }

        /// <summary>
        /// ��� ��ġ �� ȣ��
        /// </summary>
        public virtual void OnBlockPlace(Vector3 position)
        {
            // �⺻ ������ �ƹ� �۾� ����
        }

        /// <summary>
        /// �ı� �õ� �� ȣ��
        /// </summary>
        /// <returns>�ı� ���� ����</returns>
        public virtual bool OnDestroyAttempt()
        {
            // �⺻ ������ �׻� �ı� ���
            return true;
        }

        /// <summary>
        /// �ı� ���� �� ȣ��
        /// </summary>
        public virtual void OnDestroyed()
        {
            // �⺻ ������ �ƹ� �۾� ����
        }
    }
}