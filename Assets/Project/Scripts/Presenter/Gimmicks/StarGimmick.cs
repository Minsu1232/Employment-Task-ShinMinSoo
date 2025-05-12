using Project.Scripts.Model;
using UnityEngine;
using DG.Tweening;

namespace Project.Scripts.Presenter
{
    /// <summary>
    /// 별 기믹 프레젠터
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

            // 특정 기믹 데이터로 캐스팅
            if (gimmickData is GimmickStarBlockData)
            {
                starData = gimmickData as GimmickStarBlockData;
                starId = starData.StarId;
                pointValue = starData.PointValue;

                // 시각적 표현 설정
                SetupVisuals();
            }
            else
            {
                Debug.LogError("StarGimmick에 잘못된 데이터 타입 제공됨");
            }
        }

        private void SetupVisuals()
        {
            // 별 시각적 표현 생성
            starVisual = new GameObject("StarVisual");
            starVisual.transform.SetParent(targetObject.transform);
            starVisual.transform.localPosition = new Vector3(0, 0.1f, 0);

            // 별 오브젝트 생성 (간단한 형태로)
            GameObject starObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            starObj.transform.SetParent(starVisual.transform);
            starObj.transform.localPosition = Vector3.zero;
            starObj.transform.localScale = new Vector3(0.2f, 0.2f, 0.05f);

            // 별 머티리얼 설정
            Renderer renderer = starObj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.yellow;

                // 발광 효과
                renderer.material.SetFloat("_EmissionColor", 1.0f);
                renderer.material.EnableKeyword("_EMISSION");
            }

            // 별 애니메이션 시작
            StartStarAnimation();

            // 포인트 텍스트 표시 (선택적)
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
            // 별 애니메이션
            Sequence starAnimation = DOTween.Sequence();

            // 확대/축소 효과
            starAnimation.Append(starVisual.transform.DOScale(Vector3.one * 1.2f, 0.5f).SetEase(Ease.InOutSine));
            starAnimation.Append(starVisual.transform.DOScale(Vector3.one * 0.8f, 0.5f).SetEase(Ease.InOutSine));

            // 회전 효과
            starAnimation.Join(starVisual.transform.DOLocalRotate(new Vector3(0, 360, 0), 2f, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1));

            // 무한 반복
            starAnimation.SetLoops(-1, LoopType.Restart);
            starAnimation.Play();
        }

        public override bool OnDestroyAttempt()
        {
            if (!isActive) return true;

            // 별 기믹은 항상 파괴 가능
            return true;
        }

        public override void OnDestroyed()
        {
            base.OnDestroyed();

            // 별 획득 효과
            if (starVisual != null)
            {
                // 별 확대 효과
                starVisual.transform.DOScale(Vector3.one * 2f, 0.3f)
                    .OnComplete(() => {
                        starVisual.transform.DOScale(Vector3.zero, 0.3f);
                    });
            }

            // 게임 매니저에 별 획득 알림
            GameEvents.OnStarCollected?.Invoke(starId, pointValue);
        }
    }
}