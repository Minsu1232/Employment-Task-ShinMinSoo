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
            lockVisual = new GameObject("LockVisual");
            lockVisual.transform.SetParent(targetObject.transform);
            lockVisual.transform.localPosition = new Vector3(0, 0.1f, 0);

            // �ڹ��� ��� ����
            GameObject lockBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lockBody.transform.SetParent(lockVisual.transform);
            lockBody.transform.localPosition = Vector3.zero;
            lockBody.transform.localScale = new Vector3(0.2f, 0.2f, 0.05f);

            GameObject lockShackle = GameObject.CreatePrimitive(PrimitiveType.Torus);
            if (lockShackle == null) // �䷯���� �⺻ ������Ƽ�꿡 ���� ���
            {
                lockShackle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                lockShackle.transform.localScale = new Vector3(0.1f, 0.1f, 0.02f);
            }
            lockShackle.transform.SetParent(lockVisual.transform);
            lockShackle.transform.localPosition = new Vector3(0, 0.15f, 0);
            lockShackle.transform.localScale = new Vector3(0.1f, 0.1f, 0.02f);

            // ī��Ʈ �ؽ�Ʈ ����
            GameObject textObj = new GameObject("CountText");
            textObj.transform.SetParent(lockVisual.transform);
            textObj.transform.localPosition = new Vector3(0, 0, -0.03f);

            countText = textObj.AddComponent<TextMesh>();
            countText.text = remainingCount.ToString();
            countText.fontSize = 20;
            countText.alignment = TextAlignment.Center;
            countText.anchor = TextAnchor.MiddleCenter;
            countText.color = Color.white;

            // �ڹ��� ID�� ���� ���� ����
            Color lockColor = GetLockColor(lockId);

            // �ڹ��� ��Ƽ���� ����
            SetLockColor(lockBody, lockColor);
            SetLockColor(lockShackle, lockColor);

            // �ڹ��� �ִϸ��̼� ����
            StartLockAnimation();
        }

        private void SetLockColor(GameObject obj, Color color)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = color;

                // ��Ż�� ȿ��
                renderer.material.SetFloat("_Metallic", 0.8f);
                renderer.material.SetFloat("_Glossiness", 0.8f);
            }
        }

        private void StartLockAnimation()
        {
            // �ڹ��� �ִϸ��̼�
            Sequence lockAnimation = DOTween.Sequence();

            // �ణ�� ȸ�� �ִϸ��̼�
            lockAnimation.Append(lockVisual.transform.DOLocalRotate(new Vector3(0, 10, 0), 0.5f).SetEase(Ease.InOutSine));
            lockAnimation.Append(lockVisual.transform.DOLocalRotate(new Vector3(0, -10, 0), 0.5f).SetEase(Ease.InOutSine));
            lockAnimation.Append(lockVisual.transform.DOLocalRotate(Vector3.zero, 0.5f).SetEase(Ease.InOutSine));

            // �ִϸ��̼� ���� �ݺ�
            lockAnimation.SetLoops(-1);
            lockAnimation.Play();
        }

        private Color GetLockColor(int id)
        {
            // �ڹ��� ID�� ���� ���� ����
            switch (id % 5)
            {
                case 0: return new Color(1, 0.8f, 0); // �ݻ�
                case 1: return new Color(0.8f, 0.8f, 0.8f); // ����
                case 2: return new Color(0.6f, 0.3f, 0); // û����
                case 3: return new Color(0, 0.6f, 1); // �Ķ���
                case 4: return new Color(1, 0, 0.5f); // ��ȫ��
                default: return Color.white;
            }
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