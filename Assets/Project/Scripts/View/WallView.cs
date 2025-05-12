using UnityEngine;
using DG.Tweening;

namespace Project.Scripts.View
{
    /// <summary>
    /// 벽의 시각적 표현을 담당하는 뷰 컴포넌트
    /// </summary>
    [RequireComponent(typeof(WallObject))]
    public class WallView : MonoBehaviour
    {
        [Header("렌더링 참조")]
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private GameObject arrowIndicator;

        [Header("효과 설정")]
        [SerializeField] private float pulseDuration = 1f;
        [SerializeField] private float pulseIntensity = 0.2f;

        private WallObject wallObject;
        private Material defaultMaterial;
        private Color originalColor;
        private Sequence pulseSequence;
        private bool isPulsing = false;

        private void Awake()
        {
            wallObject = GetComponent<WallObject>();

            // 렌더러 참조 찾기
            if (meshRenderer == null)
            {
                meshRenderer = GetComponent<MeshRenderer>();
            }

            // 원본 머티리얼 및 색상 저장
            if (meshRenderer != null && meshRenderer.material != null)
            {
                defaultMaterial = meshRenderer.material;
                originalColor = defaultMaterial.color;
            }
        }

        /// <summary>
        /// 벽 머티리얼 설정
        /// </summary>
        public void SetMaterial(Material material, bool showArrow)
        {
            if (meshRenderer != null)
            {
                meshRenderer.material = material;
                defaultMaterial = material;
                originalColor = material.color;
            }

            // 화살표 표시기 활성화/비활성화
            if (arrowIndicator != null)
            {
                arrowIndicator.SetActive(showArrow);
            }
        }

        /// <summary>
        /// 색상 변경
        /// </summary>
        public void SetColor(Color color)
        {
            if (meshRenderer != null && meshRenderer.material != null)
            {
                meshRenderer.material.color = color;
            }
        }

        /// <summary>
        /// 원래 색상으로 복원
        /// </summary>
        public void RestoreOriginalColor()
        {
            if (meshRenderer != null)
            {
                meshRenderer.material.color = originalColor;
            }
        }

        /// <summary>
        /// 벽 펄스 효과 시작
        /// </summary>
        public void StartPulseEffect()
        {
            if (isPulsing || meshRenderer == null || meshRenderer.material == null) return;

            isPulsing = true;

            // 펄스 애니메이션 시퀀스 생성
            pulseSequence = DOTween.Sequence();

            Color brightenedColor = new Color(
                originalColor.r + pulseIntensity,
                originalColor.g + pulseIntensity,
                originalColor.b + pulseIntensity,
                originalColor.a
            );

            pulseSequence.Append(DOTween.To(() => meshRenderer.material.color, x => meshRenderer.material.color = x, brightenedColor, pulseDuration / 2));
            pulseSequence.Append(DOTween.To(() => meshRenderer.material.color, x => meshRenderer.material.color = x, originalColor, pulseDuration / 2));

            pulseSequence.SetLoops(-1); // 무한 반복
            pulseSequence.Play();
        }

        /// <summary>
        /// 벽 펄스 효과 중단
        /// </summary>
        public void StopPulseEffect()
        {
            if (!isPulsing) return;

            if (pulseSequence != null && pulseSequence.IsActive())
            {
                pulseSequence.Kill();
            }

            RestoreOriginalColor();
            isPulsing = false;
        }

        /// <summary>
        /// 벽 파괴 애니메이션
        /// </summary>
        public void PlayDestroyAnimation(float duration, System.Action onComplete = null)
        {
            // 펄스 효과 중단
            StopPulseEffect();

            // 스케일 감소 및 페이드 아웃
            transform.DOScale(Vector3.zero, duration)
                .SetEase(Ease.InBack)
                .OnComplete(() => {
                    onComplete?.Invoke();
                });

            // 동시에 투명도 감소
            if (meshRenderer != null && meshRenderer.material != null)
            {
                Color startColor = meshRenderer.material.color;
                Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0);

                DOTween.To(() => startColor, x => meshRenderer.material.color = x, endColor, duration);
            }
        }
    }
}