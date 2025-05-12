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
        [SerializeField] private float destroyAnimDuration = 1.0f;
        [SerializeField] private Ease destroyAnimEase = Ease.Linear;

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

            // 비주얼 루트 설정
            if (visualRoot == null)
            {
                visualRoot = transform.Find("VisualRoot")?.gameObject;

                // 만약 없다면 메시 렌더러가 있는 게임오브젝트를 루트로 사용
                if (visualRoot == null && meshRenderer != null)
                {
                    visualRoot = meshRenderer.gameObject;
                }
            }

            // 아웃라인 컴포넌트 초기화
            InitializeOutline();

            // 원본 색상 저장
            if (meshRenderer != null && meshRenderer.material != null)
            {
                originalColor = meshRenderer.material.color;
            }
        }

        /// <summary>
        /// 아웃라인 컴포넌트 초기화
        /// </summary>
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
                Debug.LogWarning("바운스 애니메이션을 위한 visualRoot가 설정되지 않았습니다.");
                return;
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
        /// 블록 강조 효과 재생
        /// </summary>
        public void PlayHighlightEffect(Color highlightColor, float duration = 0.5f)
        {
            if (meshRenderer == null || meshRenderer.material == null) return;

            // 이미 실행 중인 하이라이트 효과 취소
            meshRenderer.material.DOKill();

            // 하이라이트 애니메이션 시퀀스
            DOTween.Sequence()
                .Append(meshRenderer.material.DOColor(highlightColor, duration / 2))
                .Append(meshRenderer.material.DOColor(originalColor, duration / 2));
        }

        /// <summary>
        /// 블록 파괴 애니메이션
        /// </summary>
        public void PlayDestroyAnimation(Vector3 targetPosition, float duration = 1.0f, System.Action onComplete = null)
        {
            float animDuration = (duration > 0) ? duration : destroyAnimDuration;

            // 위치 이동 애니메이션
            transform.DOMove(targetPosition, animDuration)
                .SetEase(destroyAnimEase)
                .OnComplete(() => {
                    onComplete?.Invoke();
                });

            // 동시에 투명도 감소
            if (meshRenderer != null && meshRenderer.material != null)
            {
                Color startColor = meshRenderer.material.color;
                Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0);

                DOTween.To(() => startColor, x => meshRenderer.material.color = x, endColor, animDuration);
            }
        }

        /// <summary>
        /// 블록 선택 효과 재생
        /// </summary>
        public void PlaySelectionEffect()
        {
            // 아웃라인 표시
            ShowOutline(true);

            // 바운스 효과
            PlayBounceAnimation();

            // 잠시 후 아웃라인 숨김 (게임 요구사항에 따라 조정)
            Invoke(nameof(HideOutline), 0.5f);
        }

        /// <summary>
        /// 아웃라인 숨기기
        /// </summary>
        private void HideOutline()
        {
            ShowOutline(false);
        }

        /// <summary>
        /// 에셋 정리
        /// </summary>
        private void OnDestroy()
        {
            // 실행 중인 모든 트윈 종료
            if (bounceSequence != null && bounceSequence.IsActive())
            {
                bounceSequence.Kill();
            }

            if (meshRenderer != null && meshRenderer.material != null)
            {
                meshRenderer.material.DOKill();
            }

            DOTween.Kill(transform);
        }
    }
}