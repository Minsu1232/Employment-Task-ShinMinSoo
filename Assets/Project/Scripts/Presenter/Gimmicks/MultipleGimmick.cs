using Project.Scripts.Model;
using Project.Scripts.View;
using UnityEngine;
using DG.Tweening;
using Project.Scripts.Controller;

namespace Project.Scripts.Presenter
{
    /// <summary>
    /// 다중 기믹 프레젠터
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

            // 특정 기믹 데이터로 캐스팅
            if (gimmickData is GimmickMultipleData)
            {
                multipleData = gimmickData as GimmickMultipleData;
                targetColorType = multipleData.ColorType;

                // 시각적 표현 설정
                SetupVisuals();
            }
            else
            {
                Debug.LogError("MultipleGimmick에 잘못된 데이터 타입 제공됨");
            }
        }

        private void SetupVisuals()
        {
            // 다중 기믹 표시기 생성
            visualIndicator = new GameObject("MultipleIndicator");
            visualIndicator.transform.SetParent(targetObject.transform);
            visualIndicator.transform.localPosition = new Vector3(0, 0.1f, 0);

            //// 블록 뷰 컴포넌트 가져오기
            //BlockView blockView = targetObject.GetComponentInChildren<BlockView>();
            //if (blockView != null)
            //{
            //    SkinnedMeshRenderer renderer = blockView.GetComponentInChildren<SkinnedMeshRenderer>();
            //    if (renderer != null)
            //    {
            //        originalMaterial = renderer.material;

            //        // 다중 색상 효과를 위한 새 머티리얼 생성
            //        multipleMaterial = new Material(originalMaterial);
            //        multipleMaterial.color = GetColorForType(targetColorType);

            //        // 색상 변화 효과 시작
            //        StartColorPulseEffect(renderer);
            //    }
            //}
        }

        private void StartColorPulseEffect(Renderer renderer)
        {
            if (renderer == null) return;

            // 색상 변화 시퀀스 생성
            Sequence colorSequence = DOTween.Sequence();

            Color originalColor = originalMaterial.color;
            Color targetColor = GetColorForType(targetColorType);

            // 색상 변화 효과
            colorSequence.Append(
                DOTween.To(() => renderer.material.color, x => renderer.material.color = x,
                    targetColor, 1.0f).SetEase(Ease.InOutSine));

            colorSequence.Append(
                DOTween.To(() => renderer.material.color, x => renderer.material.color = x,
                    originalColor, 1.0f).SetEase(Ease.InOutSine));

            // 무한 반복
            colorSequence.SetLoops(-1);
            colorSequence.Play();
        }

        public override bool OnDestroyAttempt()
        {
            if (!isActive) return true;

            // 다중 기믹은 항상 파괴 가능
            return true;
        }

        public override void OnDestroyed()
        {
            base.OnDestroyed();

            // 파괴 시 추가 블록 생성 로직
            // 이 부분은 BlockController에서 처리해야 함
            // 현재는 이벤트만 발생시키는 형태로 구현

            // 게임 매니저에 알림
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