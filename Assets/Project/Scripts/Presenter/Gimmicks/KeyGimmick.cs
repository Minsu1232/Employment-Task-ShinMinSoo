using Project.Scripts.Model;
using UnityEngine;
using DG.Tweening;

namespace Project.Scripts.Presenter
{
    /// <summary>
    /// ���� ��� ��������
    /// </summary>
    public class KeyGimmick : GimmickPresenter
    {
        private GimmickKeyData keyData;
        private int keyId;
        private GameObject keyVisual;

        public override ObjectPropertiesEnum.BlockGimmickType GimmickType =>
            ObjectPropertiesEnum.BlockGimmickType.Key;

        public override void Initialize(GimmickData gimmickData, GameObject targetObject)
        {
            base.Initialize(gimmickData, targetObject);

            // Ư�� ��� �����ͷ� ĳ����
            if (gimmickData is GimmickKeyData)
            {
                keyData = gimmickData as GimmickKeyData;
                keyId = keyData.KeyId;

                // �ð��� ǥ�� ����
                SetupVisuals();
            }
            else
            {
                Debug.LogError("KeyGimmick�� �߸��� ������ Ÿ�� ������");
            }
        }

        private void SetupVisuals()
        {
            // ���� �ð��� ǥ�� ����
            keyVisual = new GameObject("KeyVisual");
            keyVisual.transform.SetParent(targetObject.transform);
            keyVisual.transform.localPosition = new Vector3(0, 0.1f, 0);

            // ���� ��� ���� (������ ���·�)
            GameObject keyStem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            keyStem.transform.SetParent(keyVisual.transform);
            keyStem.transform.localPosition = new Vector3(0, 0, 0.1f);
            keyStem.transform.localRotation = Quaternion.Euler(90, 0, 0);
            keyStem.transform.localScale = new Vector3(0.05f, 0.15f, 0.05f);

            GameObject keyHead = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            keyHead.transform.SetParent(keyVisual.transform);
            keyHead.transform.localPosition = new Vector3(0, 0, 0.25f);
            keyHead.transform.localScale = new Vector3(0.15f, 0.05f, 0.15f);

            // ���� ID�� ���� ���� ����
            Color keyColor = GetKeyColor(keyId);

            // ���� ��Ƽ���� ����
            SetKeyColor(keyStem, keyColor);
            SetKeyColor(keyHead, keyColor);

            // ���� �ִϸ��̼� ����
            StartKeyAnimation();
        }

        private void SetKeyColor(GameObject obj, Color color)
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

        private void StartKeyAnimation()
        {
            // ���� ���� �ִϸ��̼�
            Sequence keyAnimation = DOTween.Sequence();

            // ���Ʒ��� ������
            keyAnimation.Append(keyVisual.transform.DOLocalMoveY(0.15f, 0.5f).SetEase(Ease.InOutSine));
            keyAnimation.Append(keyVisual.transform.DOLocalMoveY(0.05f, 0.5f).SetEase(Ease.InOutSine));

            // ���ڸ� ȸ��
            keyAnimation.Join(keyVisual.transform.DOLocalRotate(new Vector3(0, 360, 0), 2f, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1));

            // ���Ʒ� ���� ���� �ݺ�
            keyAnimation.SetLoops(-1, LoopType.Restart);
            keyAnimation.Play();
        }

        private Color GetKeyColor(int id)
        {
            // Ű ID�� ���� ���� ����
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

        public override bool OnDestroyAttempt()
        {
            if (!isActive) return true;

            // ����� �׻� �ı� ����
            return true;
        }

        public override void OnDestroyed()
        {
            base.OnDestroyed();

            // ���� �Ŵ����� Ű ȹ�� �˸�
            GameEvents.OnKeyCollected?.Invoke(keyId);

            // Ű ��ƼŬ ȿ��
            if (keyVisual != null)
            {
                // Ű ȹ�� ȿ��
                keyVisual.transform.DOScale(Vector3.one * 1.5f, 0.3f)
                    .OnComplete(() => {
                        keyVisual.transform.DOScale(Vector3.zero, 0.3f);
                    });
            }
        }
    }
}