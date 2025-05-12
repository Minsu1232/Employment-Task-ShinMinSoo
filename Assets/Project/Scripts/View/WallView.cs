using UnityEngine;
using DG.Tweening;

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

        [Header("ȿ�� ����")]
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
        /// �� �޽� ȿ�� ����
        /// </summary>
        public void StartPulseEffect()
        {
            if (isPulsing || meshRenderer == null || meshRenderer.material == null) return;

            isPulsing = true;

            // �޽� �ִϸ��̼� ������ ����
            pulseSequence = DOTween.Sequence();

            Color brightenedColor = new Color(
                originalColor.r + pulseIntensity,
                originalColor.g + pulseIntensity,
                originalColor.b + pulseIntensity,
                originalColor.a
            );

            pulseSequence.Append(DOTween.To(() => meshRenderer.material.color, x => meshRenderer.material.color = x, brightenedColor, pulseDuration / 2));
            pulseSequence.Append(DOTween.To(() => meshRenderer.material.color, x => meshRenderer.material.color = x, originalColor, pulseDuration / 2));

            pulseSequence.SetLoops(-1); // ���� �ݺ�
            pulseSequence.Play();
        }

        /// <summary>
        /// �� �޽� ȿ�� �ߴ�
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
        /// �� �ı� �ִϸ��̼�
        /// </summary>
        public void PlayDestroyAnimation(float duration, System.Action onComplete = null)
        {
            // �޽� ȿ�� �ߴ�
            StopPulseEffect();

            // ������ ���� �� ���̵� �ƿ�
            transform.DOScale(Vector3.zero, duration)
                .SetEase(Ease.InBack)
                .OnComplete(() => {
                    onComplete?.Invoke();
                });

            // ���ÿ� ���� ����
            if (meshRenderer != null && meshRenderer.material != null)
            {
                Color startColor = meshRenderer.material.color;
                Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0);

                DOTween.To(() => startColor, x => meshRenderer.material.color = x, endColor, duration);
            }
        }
    }
}