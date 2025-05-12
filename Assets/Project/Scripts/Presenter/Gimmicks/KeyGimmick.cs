using Project.Scripts.Model;
using UnityEngine;
using DG.Tweening;

namespace Project.Scripts.Presenter
{
    /// <summary>
    /// 열쇠 기믹 프레젠터
    /// </summary>
    public class KeyGimmick : GimmickPresenter
    {
        private GimmickKeyData keyData;
        private int keyId;
        private GameObject keyVisual;

        public override ObjectPropertiesEnum.BlockGimmickType GimmickType =>
            ObjectPropertiesEnum.BlockGimmickType.Key;

        public override void Initialize(GimmickData gimmickData, GameObject targetObject)
        {
            base.Initialize(gimmickData, targetObject);

            // 특정 기믹 데이터로 캐스팅
            if (gimmickData is GimmickKeyData)
            {
                keyData = gimmickData as GimmickKeyData;
                keyId = keyData.KeyId;

                // 시각적 표현 설정
                SetupVisuals();
            }
            else
            {
                Debug.LogError("KeyGimmick에 잘못된 데이터 타입 제공됨");
            }
        }

        private void SetupVisuals()
        {
            // 열쇠 시각적 표현 생성
            keyVisual = new GameObject("KeyVisual");
            keyVisual.transform.SetParent(targetObject.transform);
            keyVisual.transform.localPosition = new Vector3(0, 0.1f, 0);

            // 열쇠 모양 생성 (간단한 형태로)
            GameObject keyStem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            keyStem.transform.SetParent(keyVisual.transform);
            keyStem.transform.localPosition = new Vector3(0, 0, 0.1f);
            keyStem.transform.localRotation = Quaternion.Euler(90, 0, 0);
            keyStem.transform.localScale = new Vector3(0.05f, 0.15f, 0.05f);

            GameObject keyHead = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            keyHead.transform.SetParent(keyVisual.transform);
            keyHead.transform.localPosition = new Vector3(0, 0, 0.25f);
            keyHead.transform.localScale = new Vector3(0.15f, 0.05f, 0.15f);

            // 열쇠 ID에 따른 색상 설정
            Color keyColor = GetKeyColor(keyId);

            // 열쇠 머티리얼 설정
            SetKeyColor(keyStem, keyColor);
            SetKeyColor(keyHead, keyColor);

            // 열쇠 애니메이션 시작
            StartKeyAnimation();
        }

        private void SetKeyColor(GameObject obj, Color color)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = color;

                // 메탈릭 효과
                renderer.material.SetFloat("_Metallic", 0.8f);
                renderer.material.SetFloat("_Glossiness", 0.8f);
            }
        }

        private void StartKeyAnimation()
        {
            // 열쇠 부유 애니메이션
            Sequence keyAnimation = DOTween.Sequence();

            // 위아래로 움직임
            keyAnimation.Append(keyVisual.transform.DOLocalMoveY(0.15f, 0.5f).SetEase(Ease.InOutSine));
            keyAnimation.Append(keyVisual.transform.DOLocalMoveY(0.05f, 0.5f).SetEase(Ease.InOutSine));

            // 제자리 회전
            keyAnimation.Join(keyVisual.transform.DOLocalRotate(new Vector3(0, 360, 0), 2f, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1));

            // 위아래 부유 무한 반복
            keyAnimation.SetLoops(-1, LoopType.Restart);
            keyAnimation.Play();
        }

        private Color GetKeyColor(int id)
        {
            // 키 ID에 따른 색상 설정
            switch (id % 5)
            {
                case 0: return new Color(1, 0.8f, 0); // 금색
                case 1: return new Color(0.8f, 0.8f, 0.8f); // 은색
                case 2: return new Color(0.6f, 0.3f, 0); // 청동색
                case 3: return new Color(0, 0.6f, 1); // 파란색
                case 4: return new Color(1, 0, 0.5f); // 분홍색
                default: return Color.white;
            }
        }

        public override bool OnDestroyAttempt()
        {
            if (!isActive) return true;

            // 열쇠는 항상 파괴 가능
            return true;
        }

        public override void OnDestroyed()
        {
            base.OnDestroyed();

            // 게임 매니저에 키 획득 알림
            GameEvents.OnKeyCollected?.Invoke(keyId);

            // 키 파티클 효과
            if (keyVisual != null)
            {
                // 키 획득 효과
                keyVisual.transform.DOScale(Vector3.one * 1.5f, 0.3f)
                    .OnComplete(() => {
                        keyVisual.transform.DOScale(Vector3.zero, 0.3f);
                    });
            }
        }
    }
}