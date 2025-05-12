using DG.Tweening;
using UnityEngine;
using Project.Scripts.Controller;

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
        [SerializeField] private Color validPlacementColor = new Color(0, 1, 0, 0.3f);
        [SerializeField] private Color invalidPlacementColor = new Color(1, 0, 0, 0.3f);

        private BoardBlockObject boardBlockObject;
        private Material defaultMaterial;
        private Color originalColor;
        private Tween highlightTween;

        // 체크 블록 표시 오브젝트
        private GameObject checkIndicator;

        private void Awake()
        {
            boardBlockObject = GetComponent<BoardBlockObject>();

            // 렌더러 참조 찾기
            if (meshRenderer == null)
            {
                meshRenderer = GetComponent<MeshRenderer>();
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

            // 원본 머티리얼 및 색상 저장
            if (meshRenderer != null && meshRenderer.material != null)
            {
                defaultMaterial = meshRenderer.material;
                originalColor = defaultMaterial.color;
            }

            // 체크 블록 인디케이터 초기화
            InitializeCheckIndicator();
        }

        /// <summary>
        /// 체크 블록 인디케이터 초기화
        /// </summary>
        private void InitializeCheckIndicator()
        {
            if (boardBlockObject.isCheckBlock)
            {
                // 인디케이터 생성 (사용자 정의에 따라 조정 필요)
                checkIndicator = new GameObject("CheckIndicator");
                checkIndicator.transform.SetParent(transform);
                checkIndicator.transform.localPosition = new Vector3(0, 0.01f, 0);

                // 시각적 표시 (예: 작은 큐브 또는 이미지)
                var indicatorRenderer = checkIndicator.AddComponent<MeshRenderer>();
                var indicatorFilter = checkIndicator.AddComponent<MeshFilter>();

                // 기본 큐브 메시 사용 (실제 구현에서는 전용 메시나 스프라이트 사용 권장)
                indicatorFilter.mesh = CreateIndicatorMesh();

                // 인디케이터 머티리얼 설정
                indicatorRenderer.material = new Material(Shader.Find("Standard"));
                indicatorRenderer.material.color = new Color(1, 1, 0, 0.5f); // 반투명 노란색

                // 초기 상태는 비활성화
                checkIndicator.SetActive(false);
            }
        }

        /// <summary>
        /// 인디케이터 메시 생성 (간단한 평면)
        /// </summary>
        private Mesh CreateIndicatorMesh()
        {
            Mesh mesh = new Mesh();

            float size = 0.7f; // 보드 블록보다 약간 작게
            float height = 0.02f; // 높이

            Vector3[] vertices = new Vector3[4];
            vertices[0] = new Vector3(-size / 2, height, -size / 2);
            vertices[1] = new Vector3(size / 2, height, -size / 2);
            vertices[2] = new Vector3(size / 2, height, size / 2);
            vertices[3] = new Vector3(-size / 2, height, size / 2);

            int[] triangles = new int[6];
            triangles[0] = 0;
            triangles[1] = 1;
            triangles[2] = 2;
            triangles[3] = 0;
            triangles[4] = 2;
            triangles[5] = 3;

            Vector2[] uv = new Vector2[4];
            uv[0] = new Vector2(0, 0);
            uv[1] = new Vector2(1, 0);
            uv[2] = new Vector2(1, 1);
            uv[3] = new Vector2(0, 1);

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uv;
            mesh.RecalculateNormals();

            return mesh;
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

            // 색상 변경 애니메이션 시퀀스
            highlightTween = DOTween.Sequence()
                .Append(DOTween.To(() => meshRenderer.material.color, x => meshRenderer.material.color = x, highlightColor, highlightDuration / 2))
                .Append(DOTween.To(() => meshRenderer.material.color, x => meshRenderer.material.color = x, originalColor, highlightDuration / 2));

            highlightTween.Play();
        }

        /// <summary>
        /// 배치 가능 여부 시각적 표시
        /// </summary>
        public void ShowPlacementIndicator(bool isValid)
        {
            if (meshRenderer == null || meshRenderer.material == null) return;

            // 현재 실행 중인 트윈 중단
            meshRenderer.material.DOKill();

            // 색상 설정
            Color targetColor = isValid ? validPlacementColor : invalidPlacementColor;

            // 원래 색상 백업
            Color currentColor = meshRenderer.material.color;

            // 색상 변경 후 원래 색으로 복귀
            DOTween.Sequence()
                .Append(meshRenderer.material.DOColor(targetColor, 0.2f))
                .AppendInterval(0.3f)
                .Append(meshRenderer.material.DOColor(originalColor, 0.2f));
        }

        /// <summary>
        /// 블록 파괴 효과
        /// </summary>
        public void PlayDestroyEffect()
        {
            if (meshRenderer == null) return;

            // 섬광 효과
            Color flashColor = Color.white;

            DOTween.Sequence()
                .Append(meshRenderer.material.DOColor(flashColor, 0.1f))
                .Append(meshRenderer.material.DOColor(originalColor, 0.2f));

            // 크기 변화 효과
            if (visualRoot != null)
            {
                Vector3 originalScale = visualRoot.transform.localScale;

                DOTween.Sequence()
                    .Append(visualRoot.transform.DOScale(originalScale * 1.2f, 0.1f))
                    .Append(visualRoot.transform.DOScale(originalScale, 0.2f));
            }
        }

        /// <summary>
        /// 체크 블록 표시 효과
        /// </summary>
        public void ShowCheckBlockIndicator(bool show)
        {
            if (checkIndicator != null)
            {
                checkIndicator.SetActive(show);

                // 활성화 시 간단한 효과 추가
                if (show)
                {
                    checkIndicator.transform.localScale = Vector3.zero;
                    checkIndicator.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
                }
            }
        }

        /// <summary>
        /// 파괴 가능 표시 (색상 블록 일치 여부)
        /// </summary>
        public void ShowDestroyableIndicator(bool canDestroy, ColorType matchingColor)
        {
            if (checkIndicator == null || !boardBlockObject.isCheckBlock) return;

            // 인디케이터 활성화
            checkIndicator.SetActive(true);

            // 머티리얼 색상 설정
            var indicatorRenderer = checkIndicator.GetComponent<MeshRenderer>();
            if (indicatorRenderer != null)
            {
                Color indicatorColor;

                if (canDestroy)
                {
                    // 색상 매칭 표시 (색상 타입에 따른 색상 사용)
                    indicatorColor = GetColorFromType(matchingColor);
                    indicatorColor.a = 0.7f; // 반투명
                }
                else
                {
                    // 파괴 불가능 표시
                    indicatorColor = new Color(0.5f, 0.5f, 0.5f, 0.3f); // 회색 반투명
                }

                indicatorRenderer.material.DOKill();
                indicatorRenderer.material.DOColor(indicatorColor, 0.2f);

                // 펄스 효과
                if (canDestroy)
                {
                    DOTween.Sequence()
                        .Append(checkIndicator.transform.DOScale(Vector3.one * 1.1f, 0.3f))
                        .Append(checkIndicator.transform.DOScale(Vector3.one, 0.3f))
                        .SetLoops(2, LoopType.Restart);
                }
            }
        }

        /// <summary>
        /// 색상 타입에 따른 실제 Color 값 반환
        /// </summary>
        private Color GetColorFromType(ColorType colorType)
        {
            switch (colorType)
            {
                case ColorType.Red: return Color.red;
                case ColorType.Orange: return new Color(1.0f, 0.5f, 0.0f);
                case ColorType.Yellow: return Color.yellow;
                case ColorType.Green: return Color.green;
                case ColorType.Blue: return Color.blue;
                case ColorType.Purple: return new Color(0.5f, 0.0f, 0.5f);
                case ColorType.Gray: return Color.gray;
                case ColorType.Beige: return new Color(0.96f, 0.96f, 0.86f);
                default: return Color.white;
            }
        }

        /// <summary>
        /// 에셋 정리
        /// </summary>
        private void OnDestroy()
        {
            // 실행 중인 모든 트윈 종료
            if (highlightTween != null && highlightTween.IsActive())
            {
                highlightTween.Kill();
            }

            if (meshRenderer != null && meshRenderer.material != null)
            {
                meshRenderer.material.DOKill();
            }

            if (checkIndicator != null)
            {
                checkIndicator.transform.DOKill();
            }

            DOTween.Kill(transform);
        }
    }
}