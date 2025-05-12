using Project.Scripts.Model;
using UnityEngine;
using DG.Tweening;

namespace Project.Scripts.Presenter
{
    /// <summary>
    /// �ڹ��� ��� ��������
    /// </summary>
    public class LockGimmick : GimmickPresenter
    {
        private GimmickLockData lockData;
        private int lockId;
        private int remainingCount;
        private GameObject lockVisual;
        private TextMesh countText;

        // Ű ȹ�� �̺�Ʈ ���� ����
        private bool isSubscribed = false;

        public override ObjectPropertiesEnum.BlockGimmickType GimmickType =>
            ObjectPropertiesEnum.BlockGimmickType.Lock;

        public override void Initialize(GimmickData gimmickData, GameObject targetObject)
        {
            base.Initialize(gimmickData, targetObject);

            // Ư�� ��� �����ͷ� ĳ����
            if (gimmickData is GimmickLockData)
            {
                lockData = gimmickData as GimmickLockData;
                lockId = lockData.LockId;
                remainingCount = lockData.Count;

                // �ð��� ǥ�� ����
                SetupVisuals();

                // Ű ȹ�� �̺�Ʈ ����
                SubscribeToKeyEvents();
            }
            else
            {
                Debug.LogError("LockGimmick�� �߸��� ������ Ÿ�� ������");
            }
        }

        private void OnDestroy()
        {
            // �̺�Ʈ ���� ����
            UnsubscribeFromKeyEvents();
        }

        private void SetupVisuals()
        {
            // �ڹ��� �ð��� ǥ�� ����           
        }
              

        private void SubscribeToKeyEvents()
        {
            if (isSubscribed) return;

            // Ű ȹ�� �̺�Ʈ ����
            GameEvents.OnKeyCollected += OnKeyCollected;
            isSubscribed = true;
        }

        private void UnsubscribeFromKeyEvents()
        {
            if (!isSubscribed) return;

            // Ű ȹ�� �̺�Ʈ ���� ����
            GameEvents.OnKeyCollected -= OnKeyCollected;
            isSubscribed = false;
        }

        /// <summary>
        /// Ű ȹ�� �� ȣ��Ǵ� �޼���
        /// </summary>
        private void OnKeyCollected(int keyId)
        {
            // Ű ID�� ��ġ�ϴ� ���
            if (keyId == lockId)
            {
                DecreaseCount();
            }
        }

        /// <summary>
        /// �ڹ��� ī��Ʈ ����
        /// </summary>
        public void DecreaseCount()
        {
            if (remainingCount <= 0) return;

            remainingCount--;

            // ī��Ʈ �ؽ�Ʈ ������Ʈ
            if (countText != null)
            {
                countText.text = remainingCount.ToString();
            }

            // �ڹ��� ȿ��
            if (lockVisual != null)
            {
                // �ڹ��� ��鸲 ȿ��
                lockVisual.transform.DOShakePosition(0.5f, 0.1f, 10, 90);
            }

            // ��� ī��Ʈ�� 0�� �Ǿ����� Ȯ��
            if (remainingCount <= 0)
            {
                // �ڹ��� ����
                Unlock();
            }
        }

        /// <summary>
        /// �ڹ��� ����
        /// </summary>
        private void Unlock()
        {
            // �ڹ��� ���� ȿ��
            if (lockVisual != null)
            {
                // ���� ȿ�� �ִϸ��̼�
                lockVisual.transform.DOScale(Vector3.one * 1.5f, 0.3f)
                    .OnComplete(() => {
                        lockVisual.transform.DOScale(Vector3.zero, 0.3f);
                    });
            }

            // ���� �Ŵ����� �ڹ��� ���� �˸�
            GameEvents.OnLockUnlocked?.Invoke(lockId);

            // ��� ��Ȱ��ȭ
            Deactivate();
        }

        public override bool OnBlockMove(Vector3 position)
        {
            if (!isActive) return true;

            // �ڹ��谡 ��������� �̵� �Ұ�
            if (remainingCount > 0)
            {
                // �ڹ��� ��鸲 ȿ��
                if (lockVisual != null)
                {
                    lockVisual.transform.DOShakePosition(0.3f, 0.1f, 10, 90);
                }

                return false;
            }

            return true;
        }

        public override bool OnDestroyAttempt()
        {
            if (!isActive) return true;

            // �ڹ��谡 ��������� �ı� �Ұ�
            return remainingCount <= 0;
        }
    }
}