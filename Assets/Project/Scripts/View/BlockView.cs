using UnityEngine;
using DG.Tweening;
using Project.Scripts.Controller;
using Project.Scripts.Config;

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

        private Material dissolveMaterial;
        private Material originalMaterial;
        private bool isDissolving = false;
        private Tween dissolveTween;
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
            //InitializeOutline();

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

            // 디졸브 트윈 정리
            if (dissolveTween != null && dissolveTween.IsActive())
            {
                dissolveTween.Kill();
            }

            DOTween.Kill(transform);
        }
    }
}