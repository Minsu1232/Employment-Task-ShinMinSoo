using UnityEngine;
using DG.Tweening;

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

        private BoardBlockObject boardBlockObject;
        private Material defaultMaterial;
        private Color originalColor;
        private Tween highlightTween;

        private void Awake()
        {
            boardBlockObject = GetComponent<BoardBlockObject>();

            // ������ ���� ã��
            if (meshRenderer == null)
            {
                meshRenderer = GetComponent<MeshRenderer>();
            }

            // ���� ��Ƽ���� �� ���� ����
            if (meshRenderer != null && meshRenderer.material != null)
            {
                defaultMaterial = meshRenderer.material;
                originalColor = defaultMaterial.color;
            }
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

            // ���� ���� ����
            Color targetColor = highlightColor;

            // ���� ���� �ִϸ��̼� ������
            highlightTween = DOTween.Sequence()
                .Append(DOTween.To(() => meshRenderer.material.color, x => meshRenderer.material.color = x, targetColor, highlightDuration / 2))
                .Append(DOTween.To(() => meshRenderer.material.color, x => meshRenderer.material.color = x, originalColor, highlightDuration / 2));

            highlightTween.Play();
        }

        /// <summary>
        /// üũ ��� ǥ�� ȿ��
        /// </summary>
        public void ShowCheckBlockIndicator(bool show)
        {
            // üũ ��� �ð� ȿ��
            // (��: Ư���� ȿ���� ǥ�ñ� Ȱ��ȭ/��Ȱ��ȭ)
        }
    }
}