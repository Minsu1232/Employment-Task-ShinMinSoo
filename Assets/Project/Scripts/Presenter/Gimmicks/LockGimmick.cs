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