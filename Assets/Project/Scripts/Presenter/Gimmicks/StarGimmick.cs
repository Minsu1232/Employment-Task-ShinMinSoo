using Project.Scripts.Model;
using UnityEngine;
using DG.Tweening;

namespace Project.Scripts.Presenter
{
    /// <summary>
    /// �� ��� ��������
    /// </summary>
    public class StarGimmick : GimmickPresenter
    {
        private GimmickStarBlockData starData;
        private int starId;
        private int pointValue;
        private GameObject starVisual;

        public override ObjectPropertiesEnum.BlockGimmickType GimmickType =>
            ObjectPropertiesEnum.BlockGimmickType.Star;

        public override void Initialize(GimmickData gimmickData, GameObject targetObject)
        {
            base.Initialize(gimmickData, targetObject);

            // Ư�� ��� �����ͷ� ĳ����
            if (gimmickData is GimmickStarBlockData)
            {
                starData = gimmickData as GimmickStarBlockData;
                starId = starData.StarId;
                pointValue = starData.PointValue;

                // �ð��� ǥ�� ����
                SetupVisuals();
            }
            else
            {
                Debug.LogError("StarGimmick�� �߸��� ������ Ÿ�� ������");
            }
        }

        private void SetupVisuals()
        {
            // �� �ð��� ǥ�� ����
            starVisual = new GameObject("StarVisual");
            starVisual.transform.SetParent(targetObject.transform);
            starVisual.transform.localPosition = new Vector3(0, 0.1f, 0);

            // �� ������Ʈ ���� (������ ���·�)
            GameObject starObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            starObj.transform.SetParent(starVisual.transform);
            starObj.transform.localPosition = Vector3.zero;
            starObj.transform.localScale = new Vector3(0.2f, 0.2f, 0.05f);

            // �� ��Ƽ���� ����
            Renderer renderer = starObj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.yellow;

                // �߱� ȿ��
                renderer.material.SetFloat("_EmissionColor", 1.0f);
                renderer.material.EnableKeyword("_EMISSION");
            }

            // �� �ִϸ��̼� ����
            StartStarAnimation();

            // ����Ʈ �ؽ�Ʈ ǥ�� (������)
            if (pointValue > 0)
            {
                GameObject textObj = new GameObject("PointText");
                textObj.transform.SetParent(starVisual.transform);
                textObj.transform.localPosition = new Vector3(0, 0.2f, 0);

                TextMesh textMesh = textObj.AddComponent<TextMesh>();
                textMesh.text = pointValue.ToString();
                textMesh.fontSize = 15;
                textMesh.alignment = TextAlignment.Center;
                textMesh.anchor = TextAnchor.MiddleCenter;
                textMesh.color = Color.yellow;
            }
        }

        private void StartStarAnimation()
        {
            // �� �ִϸ��̼�
            Sequence starAnimation = DOTween.Sequence();

            // Ȯ��/��� ȿ��
            starAnimation.Append(starVisual.transform.DOScale(Vector3.one * 1.2f, 0.5f).SetEase(Ease.InOutSine));
            starAnimation.Append(starVisual.transform.DOScale(Vector3.one * 0.8f, 0.5f).SetEase(Ease.InOutSine));

            // ȸ�� ȿ��
            starAnimation.Join(starVisual.transform.DOLocalRotate(new Vector3(0, 360, 0), 2f, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1));

            // ���� �ݺ�
            starAnimation.SetLoops(-1, LoopType.Restart);
            starAnimation.Play();
        }

        public override bool OnDestroyAttempt()
        {
            if (!isActive) return true;

            // �� ����� �׻� �ı� ����
            return true;
        }

        public override void OnDestroyed()
        {
            base.OnDestroyed();

            // �� ȹ�� ȿ��
            if (starVisual != null)
            {
                // �� Ȯ�� ȿ��
                starVisual.transform.DOScale(Vector3.one * 2f, 0.3f)
                    .OnComplete(() => {
                        starVisual.transform.DOScale(Vector3.zero, 0.3f);
                    });
            }

            // ���� �Ŵ����� �� ȹ�� �˸�
            GameEvents.OnStarCollected?.Invoke(starId, pointValue);
        }
    }
}