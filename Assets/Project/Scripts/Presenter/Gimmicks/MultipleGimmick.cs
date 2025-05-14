using Project.Scripts.Model;
using Project.Scripts.View;
using UnityEngine;
using DG.Tweening;
using Project.Scripts.Controller;

namespace Project.Scripts.Presenter
{
    /// <summary>
    /// ���� ��� ��������
    /// </summary>
    public class MultipleGimmick : GimmickPresenter
    {
        private GimmickMultipleData multipleData;
        private ColorType targetColorType;
        private Material originalMaterial;
        private Material multipleMaterial;
        private GameObject visualIndicator;

        public override ObjectPropertiesEnum.BlockGimmickType GimmickType =>
            ObjectPropertiesEnum.BlockGimmickType.Multiple;

        public override void Initialize(GimmickData gimmickData, GameObject targetObject)
        {
            base.Initialize(gimmickData, targetObject);

            // Ư�� ��� �����ͷ� ĳ����
            if (gimmickData is GimmickMultipleData)
            {
                multipleData = gimmickData as GimmickMultipleData;
                targetColorType = multipleData.ColorType;

                // �ð��� ǥ�� ����
                SetupVisuals();
            }
            else
            {
                Debug.LogError("MultipleGimmick�� �߸��� ������ Ÿ�� ������");
            }
        }

        private void SetupVisuals()
        {
            // ���� ��� ǥ�ñ� ����
            visualIndicator = new GameObject("MultipleIndicator");
            visualIndicator.transform.SetParent(targetObject.transform);
            visualIndicator.transform.localPosition = new Vector3(0, 0.1f, 0);

            //// ��� �� ������Ʈ ��������
            //BlockView blockView = targetObject.GetComponentInChildren<BlockView>();
            //if (blockView != null)
            //{
            //    SkinnedMeshRenderer renderer = blockView.GetComponentInChildren<SkinnedMeshRenderer>();
            //    if (renderer != null)
            //    {
            //        originalMaterial = renderer.material;

            //        // ���� ���� ȿ���� ���� �� ��Ƽ���� ����
            //        multipleMaterial = new Material(originalMaterial);
            //        multipleMaterial.color = GetColorForType(targetColorType);

            //        // ���� ��ȭ ȿ�� ����
            //        StartColorPulseEffect(renderer);
            //    }
            //}
        }

        private void StartColorPulseEffect(Renderer renderer)
        {
            if (renderer == null) return;

            // ���� ��ȭ ������ ����
            Sequence colorSequence = DOTween.Sequence();

            Color originalColor = originalMaterial.color;
            Color targetColor = GetColorForType(targetColorType);

            // ���� ��ȭ ȿ��
            colorSequence.Append(
                DOTween.To(() => renderer.material.color, x => renderer.material.color = x,
                    targetColor, 1.0f).SetEase(Ease.InOutSine));

            colorSequence.Append(
                DOTween.To(() => renderer.material.color, x => renderer.material.color = x,
                    originalColor, 1.0f).SetEase(Ease.InOutSine));

            // ���� �ݺ�
            colorSequence.SetLoops(-1);
            colorSequence.Play();
        }

        public override bool OnDestroyAttempt()
        {
            if (!isActive) return true;

            // ���� ����� �׻� �ı� ����
            return true;
        }

        public override void OnDestroyed()
        {
            base.OnDestroyed();

            // �ı� �� �߰� ��� ���� ����
            // �� �κ��� BlockController���� ó���ؾ� ��
            // ����� �̺�Ʈ�� �߻���Ű�� ���·� ����

            // ���� �Ŵ����� �˸�
            GameEvents.OnMultipleBlockDestroyed?.Invoke(targetObject, targetColorType);
        }

        private Color GetColorForType(ColorType colorType)
        {
            switch (colorType)
            {
                case ColorType.Red:
                    return new Color(1, 0, 0);
                case ColorType.Orange:
                    return new Color(1, 0.5f, 0);
                case ColorType.Yellow:
                    return new Color(1, 1, 0);
                case ColorType.Green:
                    return new Color(0, 1, 0);
                case ColorType.Blue:
                    return new Color(0, 0, 1);
                case ColorType.Purple:
                    return new Color(0.5f, 0, 0.5f);
                default:
                    return Color.white;
            }
        }
    }
}