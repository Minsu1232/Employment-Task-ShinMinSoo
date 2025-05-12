using DG.Tweening;
using UnityEngine;
using Project.Scripts.Controller;

namespace Project.Scripts.View
{
    /// <summary>
    /// ���� ����� �ð��� ǥ���� ����ϴ� �� ������Ʈ
    /// </summary>
    [RequireComponent(typeof(BoardBlockObject))]
    public class BoardBlockView : MonoBehaviour
    {
        [Header("������ ����")]
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private GameObject visualRoot;

        [Header("ȿ�� ����")]
        [SerializeField] private float highlightDuration = 0.5f;
        [SerializeField] private Color highlightColor = Color.white;
        [SerializeField] private Color validPlacementColor = new Color(0, 1, 0, 0.3f);
        [SerializeField] private Color invalidPlacementColor = new Color(1, 0, 0, 0.3f);

        private BoardBlockObject boardBlockObject;
        private Material defaultMaterial;
        private Color originalColor;
        private Tween highlightTween;

        // üũ ��� ǥ�� ������Ʈ
        private GameObject checkIndicator;

        private void Awake()
        {
            boardBlockObject = GetComponent<BoardBlockObject>();

            // ������ ���� ã��
            if (meshRenderer == null)
            {
                meshRenderer = GetComponent<MeshRenderer>();
            }

            // ���־� ��Ʈ ����
            if (visualRoot == null)
            {
                visualRoot = transform.Find("VisualRoot")?.gameObject;

                // ���� ���ٸ� �޽� �������� �ִ� ���ӿ�����Ʈ�� ��Ʈ�� ���
                if (visualRoot == null && meshRenderer != null)
                {
                    visualRoot = meshRenderer.gameObject;
                }
            }

            // ���� ��Ƽ���� �� ���� ����
            if (meshRenderer != null && meshRenderer.material != null)
            {
                defaultMaterial = meshRenderer.material;
                originalColor = defaultMaterial.color;
            }

            // üũ ��� �ε������� �ʱ�ȭ
            InitializeCheckIndicator();
        }

        /// <summary>
        /// üũ ��� �ε������� �ʱ�ȭ
        /// </summary>
        private void InitializeCheckIndicator()
        {
            if (boardBlockObject.isCheckBlock)
            {
                // �ε������� ���� (����� ���ǿ� ���� ���� �ʿ�)
                checkIndicator = new GameObject("CheckIndicator");
                checkIndicator.transform.SetParent(transform);
                checkIndicator.transform.localPosition = new Vector3(0, 0.01f, 0);

                // �ð��� ǥ�� (��: ���� ť�� �Ǵ� �̹���)
                var indicatorRenderer = checkIndicator.AddComponent<MeshRenderer>();
                var indicatorFilter = checkIndicator.AddComponent<MeshFilter>();

                // �⺻ ť�� �޽� ��� (���� ���������� ���� �޽ó� ��������Ʈ ��� ����)
                indicatorFilter.mesh = CreateIndicatorMesh();

                // �ε������� ��Ƽ���� ����
                indicatorRenderer.material = new Material(Shader.Find("Standard"));
                indicatorRenderer.material.color = new Color(1, 1, 0, 0.5f); // ������ �����

                // �ʱ� ���´� ��Ȱ��ȭ
                checkIndicator.SetActive(false);
            }
        }

        /// <summary>
        /// �ε������� �޽� ���� (������ ���)
        /// </summary>
        private Mesh CreateIndicatorMesh()
        {
            Mesh mesh = new Mesh();

            float size = 0.7f; // ���� ��Ϻ��� �ణ �۰�
            float height = 0.02f; // ����

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
        /// ���� ��� ��Ƽ���� ����
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
        /// ���� ����
        /// </summary>
        public void SetColor(Color color)
        {
            if (meshRenderer != null && meshRenderer.material != null)
            {
                meshRenderer.material.color = color;
            }
        }

        /// <summary>
        /// ���� �������� ����
        /// </summary>
        public void RestoreOriginalColor()
        {
            if (meshRenderer != null)
            {
                meshRenderer.material.color = originalColor;
            }
        }

        /// <summary>
        /// ���̶���Ʈ ȿ�� (�ӽ� ���� ����)
        /// </summary>
        public void PlayHighlightEffect()
        {
            if (meshRenderer == null || meshRenderer.material == null) return;

            // ���� �ִϸ��̼� �ߴ�
            if (highlightTween != null && highlightTween.IsActive())
            {
                highlightTween.Kill();
            }

            // ���� ���� �ִϸ��̼� ������
            highlightTween = DOTween.Sequence()
                .Append(DOTween.To(() => meshRenderer.material.color, x => meshRenderer.material.color = x, highlightColor, highlightDuration / 2))
                .Append(DOTween.To(() => meshRenderer.material.color, x => meshRenderer.material.color = x, originalColor, highlightDuration / 2));

            highlightTween.Play();
        }

        /// <summary>
        /// ��ġ ���� ���� �ð��� ǥ��
        /// </summary>
        public void ShowPlacementIndicator(bool isValid)
        {
            if (meshRenderer == null || meshRenderer.material == null) return;

            // ���� ���� ���� Ʈ�� �ߴ�
            meshRenderer.material.DOKill();

            // ���� ����
            Color targetColor = isValid ? validPlacementColor : invalidPlacementColor;

            // ���� ���� ���
            Color currentColor = meshRenderer.material.color;

            // ���� ���� �� ���� ������ ����
            DOTween.Sequence()
                .Append(meshRenderer.material.DOColor(targetColor, 0.2f))
                .AppendInterval(0.3f)
                .Append(meshRenderer.material.DOColor(originalColor, 0.2f));
        }

        /// <summary>
        /// ��� �ı� ȿ��
        /// </summary>
        public void PlayDestroyEffect()
        {
            if (meshRenderer == null) return;

            // ���� ȿ��
            Color flashColor = Color.white;

            DOTween.Sequence()
                .Append(meshRenderer.material.DOColor(flashColor, 0.1f))
                .Append(meshRenderer.material.DOColor(originalColor, 0.2f));

            // ũ�� ��ȭ ȿ��
            if (visualRoot != null)
            {
                Vector3 originalScale = visualRoot.transform.localScale;

                DOTween.Sequence()
                    .Append(visualRoot.transform.DOScale(originalScale * 1.2f, 0.1f))
                    .Append(visualRoot.transform.DOScale(originalScale, 0.2f));
            }
        }

        /// <summary>
        /// üũ ��� ǥ�� ȿ��
        /// </summary>
        public void ShowCheckBlockIndicator(bool show)
        {
            if (checkIndicator != null)
            {
                checkIndicator.SetActive(show);

                // Ȱ��ȭ �� ������ ȿ�� �߰�
                if (show)
                {
                    checkIndicator.transform.localScale = Vector3.zero;
                    checkIndicator.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
                }
            }
        }

        /// <summary>
        /// �ı� ���� ǥ�� (���� ��� ��ġ ����)
        /// </summary>
        public void ShowDestroyableIndicator(bool canDestroy, ColorType matchingColor)
        {
            if (checkIndicator == null || !boardBlockObject.isCheckBlock) return;

            // �ε������� Ȱ��ȭ
            checkIndicator.SetActive(true);

            // ��Ƽ���� ���� ����
            var indicatorRenderer = checkIndicator.GetComponent<MeshRenderer>();
            if (indicatorRenderer != null)
            {
                Color indicatorColor;

                if (canDestroy)
                {
                    // ���� ��Ī ǥ�� (���� Ÿ�Կ� ���� ���� ���)
                    indicatorColor = GetColorFromType(matchingColor);
                    indicatorColor.a = 0.7f; // ������
                }
                else
                {
                    // �ı� �Ұ��� ǥ��
                    indicatorColor = new Color(0.5f, 0.5f, 0.5f, 0.3f); // ȸ�� ������
                }

                indicatorRenderer.material.DOKill();
                indicatorRenderer.material.DOColor(indicatorColor, 0.2f);

                // �޽� ȿ��
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
        /// ���� Ÿ�Կ� ���� ���� Color �� ��ȯ
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
        /// ���� ����
        /// </summary>
        private void OnDestroy()
        {
            // ���� ���� ��� Ʈ�� ����
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