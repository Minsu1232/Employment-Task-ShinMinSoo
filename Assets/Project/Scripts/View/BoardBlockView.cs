using UnityEngine;
using DG.Tweening;

namespace Project.Scripts.View
{
    /// <summary>
    /// 보드 블록의 시각적 표현을 담당하는 뷰 컴포넌트
    /// </summary>
    [RequireComponent(typeof(BoardBlockObject))]
    public class BoardBlockView : MonoBehaviour
    {
        [Header("렌더링 참조")]
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private GameObject visualRoot;

        [Header("효과 설정")]
        [SerializeField] private float highlightDuration = 0.5f;
        [SerializeField] private Color highlightColor = Color.white;

        private BoardBlockObject boardBlockObject;
        private Material defaultMaterial;
        private Color originalColor;
        private Tween highlightTween;

        private void Awake()
        {
            boardBlockObject = GetComponent<BoardBlockObject>();

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
        /// 보드 블록 머티리얼 설정
        /// </summary>
        public void SetMaterial(Material material)
        {
            if (meshRenderer != null)
            {
                meshRenderer.material = material;
                defaultMaterial = material;
                originalColor = material.color;
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
        /// 하이라이트 효과 (임시 색상 변경)
        /// </summary>
        public void PlayHighlightEffect()
        {
            if (meshRenderer == null || meshRenderer.material == null) return;

            // 이전 애니메이션 중단
            if (highlightTween != null && highlightTween.IsActive())
            {
                highlightTween.Kill();
            }

            // 원래 색상 저장
            Color targetColor = highlightColor;

            // 색상 변경 애니메이션 시퀀스
            highlightTween = DOTween.Sequence()
                .Append(DOTween.To(() => meshRenderer.material.color, x => meshRenderer.material.color = x, targetColor, highlightDuration / 2))
                .Append(DOTween.To(() => meshRenderer.material.color, x => meshRenderer.material.color = x, originalColor, highlightDuration / 2));

            highlightTween.Play();
        }

        /// <summary>
        /// 체크 블록 표시 효과
        /// </summary>
        public void ShowCheckBlockIndicator(bool show)
        {
            // 체크 블록 시각 효과
            // (예: 특별한 효과나 표시기 활성화/비활성화)
        }
    }
}