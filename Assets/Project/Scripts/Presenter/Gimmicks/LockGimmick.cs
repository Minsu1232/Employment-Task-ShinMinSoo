using Project.Scripts.Model;
using UnityEngine;
using DG.Tweening;

namespace Project.Scripts.Presenter
{
    /// <summary>
    /// 자물쇠 기믹 프레젠터
    /// </summary>
    public class LockGimmick : GimmickPresenter
    {
        private GimmickLockData lockData;
        private int lockId;
        private int remainingCount;
        private GameObject lockVisual;
        private TextMesh countText;

        // 키 획득 이벤트 구독 상태
        private bool isSubscribed = false;

        public override ObjectPropertiesEnum.BlockGimmickType GimmickType =>
            ObjectPropertiesEnum.BlockGimmickType.Lock;

        public override void Initialize(GimmickData gimmickData, GameObject targetObject)
        {
            base.Initialize(gimmickData, targetObject);

            // 특정 기믹 데이터로 캐스팅
            if (gimmickData is GimmickLockData)
            {
                lockData = gimmickData as GimmickLockData;
                lockId = lockData.LockId;
                remainingCount = lockData.Count;

                // 시각적 표현 설정
                SetupVisuals();

                // 키 획득 이벤트 구독
                SubscribeToKeyEvents();
            }
            else
            {
                Debug.LogError("LockGimmick에 잘못된 데이터 타입 제공됨");
            }
        }

        private void OnDestroy()
        {
            // 이벤트 구독 해제
            UnsubscribeFromKeyEvents();
        }

        private void SetupVisuals()
        {
            // 자물쇠 시각적 표현 생성
            lockVisual = new GameObject("LockVisual");
            lockVisual.transform.SetParent(targetObject.transform);
            lockVisual.transform.localPosition = new Vector3(0, 0.1f, 0);

            // 자물쇠 모양 생성
            GameObject lockBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lockBody.transform.SetParent(lockVisual.transform);
            lockBody.transform.localPosition = Vector3.zero;
            lockBody.transform.localScale = new Vector3(0.2f, 0.2f, 0.05f);

            GameObject lockShackle = GameObject.CreatePrimitive(PrimitiveType.Torus);
            if (lockShackle == null) // 토러스가 기본 프리미티브에 없는 경우
            {
                lockShackle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                lockShackle.transform.localScale = new Vector3(0.1f, 0.1f, 0.02f);
            }
            lockShackle.transform.SetParent(lockVisual.transform);
            lockShackle.transform.localPosition = new Vector3(0, 0.15f, 0);
            lockShackle.transform.localScale = new Vector3(0.1f, 0.1f, 0.02f);

            // 카운트 텍스트 생성
            GameObject textObj = new GameObject("CountText");
            textObj.transform.SetParent(lockVisual.transform);
            textObj.transform.localPosition = new Vector3(0, 0, -0.03f);

            countText = textObj.AddComponent<TextMesh>();
            countText.text = remainingCount.ToString();
            countText.fontSize = 20;
            countText.alignment = TextAlignment.Center;
            countText.anchor = TextAnchor.MiddleCenter;
            countText.color = Color.white;

            // 자물쇠 ID에 따른 색상 설정
            Color lockColor = GetLockColor(lockId);

            // 자물쇠 머티리얼 설정
            SetLockColor(lockBody, lockColor);
            SetLockColor(lockShackle, lockColor);

            // 자물쇠 애니메이션 시작
            StartLockAnimation();
        }

        private void SetLockColor(GameObject obj, Color color)
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

        private void StartLockAnimation()
        {
            // 자물쇠 애니메이션
            Sequence lockAnimation = DOTween.Sequence();

            // 약간의 회전 애니메이션
            lockAnimation.Append(lockVisual.transform.DOLocalRotate(new Vector3(0, 10, 0), 0.5f).SetEase(Ease.InOutSine));
            lockAnimation.Append(lockVisual.transform.DOLocalRotate(new Vector3(0, -10, 0), 0.5f).SetEase(Ease.InOutSine));
            lockAnimation.Append(lockVisual.transform.DOLocalRotate(Vector3.zero, 0.5f).SetEase(Ease.InOutSine));

            // 애니메이션 무한 반복
            lockAnimation.SetLoops(-1);
            lockAnimation.Play();
        }

        private Color GetLockColor(int id)
        {
            // 자물쇠 ID에 따른 색상 설정
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

        private void SubscribeToKeyEvents()
        {
            if (isSubscribed) return;

            // 키 획득 이벤트 구독
            GameEvents.OnKeyCollected += OnKeyCollected;
            isSubscribed = true;
        }

        private void UnsubscribeFromKeyEvents()
        {
            if (!isSubscribed) return;

            // 키 획득 이벤트 구독 해제
            GameEvents.OnKeyCollected -= OnKeyCollected;
            isSubscribed = false;
        }

        /// <summary>
        /// 키 획득 시 호출되는 메서드
        /// </summary>
        private void OnKeyCollected(int keyId)
        {
            // 키 ID가 일치하는 경우
            if (keyId == lockId)
            {
                DecreaseCount();
            }
        }

        /// <summary>
        /// 자물쇠 카운트 감소
        /// </summary>
        public void DecreaseCount()
        {
            if (remainingCount <= 0) return;

            remainingCount--;

            // 카운트 텍스트 업데이트
            if (countText != null)
            {
                countText.text = remainingCount.ToString();
            }

            // 자물쇠 효과
            if (lockVisual != null)
            {
                // 자물쇠 흔들림 효과
                lockVisual.transform.DOShakePosition(0.5f, 0.1f, 10, 90);
            }

            // 모든 카운트가 0이 되었는지 확인
            if (remainingCount <= 0)
            {
                // 자물쇠 해제
                Unlock();
            }
        }

        /// <summary>
        /// 자물쇠 해제
        /// </summary>
        private void Unlock()
        {
            // 자물쇠 해제 효과
            if (lockVisual != null)
            {
                // 해제 효과 애니메이션
                lockVisual.transform.DOScale(Vector3.one * 1.5f, 0.3f)
                    .OnComplete(() => {
                        lockVisual.transform.DOScale(Vector3.zero, 0.3f);
                    });
            }

            // 게임 매니저에 자물쇠 해제 알림
            GameEvents.OnLockUnlocked?.Invoke(lockId);

            // 기믹 비활성화
            Deactivate();
        }

        public override bool OnBlockMove(Vector3 position)
        {
            if (!isActive) return true;

            // 자물쇠가 잠겨있으면 이동 불가
            if (remainingCount > 0)
            {
                // 자물쇠 흔들림 효과
                if (lockVisual != null)
                {
                    lockVisual.transform.DOShakePosition(0.3f, 0.1f, 10, 90);
                }

                return false;
            }

            return true;
        }

        public override bool OnDestroyAttempt()
        {
            if (!isActive) return true;

            // 자물쇠가 잠겨있으면 파괴 불가
            return remainingCount <= 0;
        }
    }
}