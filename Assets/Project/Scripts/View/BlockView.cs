using UnityEngine;
using DG.Tweening;
using Project.Scripts.Controller;
using Project.Scripts.Config;

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

        private Material dissolveMaterial;
        private Material originalMaterial;
        private bool isDissolving = false;
        private Tween dissolveTween;
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
            //InitializeOutline();

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

            // ������ Ʈ�� ����
            if (dissolveTween != null && dissolveTween.IsActive())
            {
                dissolveTween.Kill();
            }

            DOTween.Kill(transform);
        }
    }
}