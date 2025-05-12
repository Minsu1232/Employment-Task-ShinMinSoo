using UnityEngine;

namespace Project.Scripts.View
{
    /// <summary>
    /// ���� �ð��� ǥ���� ����ϴ� �� ������Ʈ
    /// </summary>
    [RequireComponent(typeof(WallObject))]
    public class WallView : MonoBehaviour
    {
        [Header("������ ����")]
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private GameObject arrowIndicator;

        private WallObject wallObject;
        private Material defaultMaterial;
        private Color originalColor;

        private void Awake()
        {
            wallObject = GetComponent<WallObject>();

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
        /// �� ��Ƽ���� ����
        /// </summary>
        public void SetMaterial(Material material, bool showArrow)
        {
            if (meshRenderer != null)
            {
                meshRenderer.material = material;
                defaultMaterial = material;
                originalColor = material.color;
            }

            // ȭ��ǥ ǥ�ñ� Ȱ��ȭ/��Ȱ��ȭ
            if (arrowIndicator != null)
            {
                arrowIndicator.SetActive(showArrow);
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
        /// �� �ı� �ִϸ��̼� (���� ����)
        /// </summary>
        public void PlayDestroyAnimation(System.Action onComplete = null)
        {
            // ������ ������ ���ҷ� �ı� ǥ��
            transform.localScale = Vector3.zero;

            // �ݹ� ����
            onComplete?.Invoke();
        }
    }
}