using UnityEngine;

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

        private WallObject wallObject;
        private Material defaultMaterial;
        private Color originalColor;

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
        /// 벽 파괴 애니메이션 (간단 구현)
        /// </summary>
        public void PlayDestroyAnimation(System.Action onComplete = null)
        {
            // 간단한 스케일 감소로 파괴 표현
            transform.localScale = Vector3.zero;

            // 콜백 실행
            onComplete?.Invoke();
        }
    }
}