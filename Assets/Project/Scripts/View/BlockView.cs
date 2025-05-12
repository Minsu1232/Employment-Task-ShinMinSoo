using UnityEngine;
using DG.Tweening;

namespace Project.Scripts.View
{
    /// <summary>
    /// ����� �ð��� ǥ���� ����ϴ� �� ������Ʈ
    /// </summary>
    [RequireComponent(typeof(BlockObject))]
    public class BlockView : MonoBehaviour
    {
        [Header("������ ����")]
        [SerializeField] private SkinnedMeshRenderer meshRenderer;
        [SerializeField] private GameObject visualRoot;

        [Header("ȿ�� ����")]
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

            // ������ ���� ã��
            if (meshRenderer == null)
            {
                meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
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

            // �ƿ����� ������Ʈ �ʱ�ȭ
            InitializeOutline();

            // ���� ���� ����
            if (meshRenderer != null && meshRenderer.material != null)
            {
                originalColor = meshRenderer.material.color;
            }
        }

        /// <summary>
        /// �ƿ����� ������Ʈ �ʱ�ȭ
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
        /// ��� ��Ƽ���� ����
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
        /// �ƿ����� ǥ�� ���� ����
        /// </summary>
        public void ShowOutline(bool show)
        {
            if (outlineComponent != null)
            {
                outlineComponent.enabled = show;
            }
        }

        /// <summary>
        /// ��� ���� ����
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
            if (meshRenderer != null && meshRenderer.material != null)
            {
                meshRenderer.material.color = originalColor;
            }
        }

        /// <summary>
        /// ��� �ٿ ȿ��
        /// </summary>
        public void PlayBounceAnimation()
        {
            if (visualRoot == null)
            {
                Debug.LogWarning("�ٿ �ִϸ��̼��� ���� visualRoot�� �������� �ʾҽ��ϴ�.");
                return;
            }

            // ���� �ִϸ��̼� ����
            if (bounceSequence != null && bounceSequence.IsActive())
            {
                bounceSequence.Kill();
            }

            // �ٿ �ִϸ��̼� ������ ����
            bounceSequence = DOTween.Sequence();

            Vector3 originalPosition = visualRoot.transform.localPosition;
            Vector3 bouncePosition = originalPosition + new Vector3(0, bounceHeight, 0);

            bounceSequence.Append(visualRoot.transform.DOLocalMove(bouncePosition, bounceDuration / 2).SetEase(Ease.OutQuad));
            bounceSequence.Append(visualRoot.transform.DOLocalMove(originalPosition, bounceDuration / 2).SetEase(Ease.InQuad));

            bounceSequence.Play();
        }

        /// <summary>
        /// ��� ���� ȿ�� ���
        /// </summary>
        public void PlayHighlightEffect(Color highlightColor, float duration = 0.5f)
        {
            if (meshRenderer == null || meshRenderer.material == null) return;

            // �̹� ���� ���� ���̶���Ʈ ȿ�� ���
            meshRenderer.material.DOKill();

            // ���̶���Ʈ �ִϸ��̼� ������
            DOTween.Sequence()
                .Append(meshRenderer.material.DOColor(highlightColor, duration / 2))
                .Append(meshRenderer.material.DOColor(originalColor, duration / 2));
        }

        /// <summary>
        /// ��� �ı� �ִϸ��̼�
        /// </summary>
        public void PlayDestroyAnimation(Vector3 targetPosition, float duration = 1.0f, System.Action onComplete = null)
        {
            float animDuration = (duration > 0) ? duration : destroyAnimDuration;

            // ��ġ �̵� �ִϸ��̼�
            transform.DOMove(targetPosition, animDuration)
                .SetEase(destroyAnimEase)
                .OnComplete(() => {
                    onComplete?.Invoke();
                });

            // ���ÿ� ���� ����
            if (meshRenderer != null && meshRenderer.material != null)
            {
                Color startColor = meshRenderer.material.color;
                Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0);

                DOTween.To(() => startColor, x => meshRenderer.material.color = x, endColor, animDuration);
            }
        }

        /// <summary>
        /// ��� ���� ȿ�� ���
        /// </summary>
        public void PlaySelectionEffect()
        {
            // �ƿ����� ǥ��
            ShowOutline(true);

            // �ٿ ȿ��
            PlayBounceAnimation();

            // ��� �� �ƿ����� ���� (���� �䱸���׿� ���� ����)
            Invoke(nameof(HideOutline), 0.5f);
        }

        /// <summary>
        /// �ƿ����� �����
        /// </summary>
        private void HideOutline()
        {
            ShowOutline(false);
        }

        /// <summary>
        /// ���� ����
        /// </summary>
        private void OnDestroy()
        {
            // ���� ���� ��� Ʈ�� ����
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