using UnityEngine;
using DG.Tweening;

namespace Project.Scripts.View
{
    /// <summary>
    /// 블록의 시각적 표현을 담당하는 뷰 컴포넌트
    /// </summary>
    [RequireComponent(typeof(BlockObject))]
    public class BlockView : MonoBehaviour
    {
        [Header("렌더링 참조")]
        [SerializeField] private SkinnedMeshRenderer meshRenderer;
        [SerializeField] private GameObject visualRoot;

        [Header("효과 설정")]
        [SerializeField] private float bounceHeight = 0.3f;
        [SerializeField] private float bounceDuration = 0.3f;

        private BlockObject blockObject;
        private Outline outlineComponent;
        private Color originalColor;
        private Sequence bounceSequence;

        private void Awake()
        {
            blockObject = GetComponent<BlockObject>();

            // 렌더러 참조 찾기
            if (meshRenderer == null)
            {
                meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            }

            // 아웃라인 컴포넌트 초기화
            InitializeOutline();

            // 원본 색상 저장
            if (meshRenderer != null && meshRenderer.material != null)
            {
                originalColor = meshRenderer.material.color;
            }
        }

        private void InitializeOutline()
        {
            outlineComponent = gameObject.GetComponent<Outline>();
            if (outlineComponent == null)
            {
                outlineComponent = gameObject.AddComponent<Outline>();
            }

            outlineComponent.OutlineMode = Outline.Mode.OutlineAll;
            outlineComponent.OutlineColor = Color.yellow;
            outlineComponent.OutlineWidth = 2f;
            outlineComponent.enabled = false;
        }

        /// <summary>
        /// 블록 머티리얼 설정
        /// </summary>
        public void SetMaterial(Material material)
        {
            if (meshRenderer != null)
            {
                meshRenderer.material = material;
                originalColor = material.color;
            }
        }

        /// <summary>
        /// 아웃라인 표시 여부 설정
        /// </summary>
        public void ShowOutline(bool show)
        {
            if (outlineComponent != null)
            {
                outlineComponent.enabled = show;
            }
        }

        /// <summary>
        /// 블록 색상 변경
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
            if (meshRenderer != null && meshRenderer.material != null)
            {
                meshRenderer.material.color = originalColor;
            }
        }

        /// <summary>
        /// 블록 바운스 효과
        /// </summary>
        public void PlayBounceAnimation()
        {
            if (visualRoot == null)
            {
                visualRoot = transform;
            }

            // 이전 애니메이션 정리
            if (bounceSequence != null && bounceSequence.IsActive())
            {
                bounceSequence.Kill();
            }

            // 바운스 애니메이션 시퀀스 생성
            bounceSequence = DOTween.Sequence();

            Vector3 originalPosition = visualRoot.transform.localPosition;
            Vector3 bouncePosition = originalPosition + new Vector3(0, bounceHeight, 0);

            bounceSequence.Append(visualRoot.transform.DOLocalMove(bouncePosition, bounceDuration / 2).SetEase(Ease.OutQuad));
            bounceSequence.Append(visualRoot.transform.DOLocalMove(originalPosition, bounceDuration / 2).SetEase(Ease.InQuad));

            bounceSequence.Play();
        }

        /// <summary>
        /// 블록 파괴 애니메이션
        /// </summary>
        public void PlayDestroyAnimation(Vector3 targetPosition, float duration, System.Action onComplete = null)
        {
            // 블록 이동 및 페이드 아웃 애니메이션
            transform.DOMove(targetPosition, duration)
                .SetEase(Ease.Linear)
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