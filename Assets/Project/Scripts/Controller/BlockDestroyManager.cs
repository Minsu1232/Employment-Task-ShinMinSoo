using System.Collections.Generic;
using UnityEngine;
using Project.Scripts.Config;
using Project.Scripts.Events;
using Project.Scripts.Model;
using static Project.Scripts.Model.BoardBlockData;
using Project.Scripts.View;

namespace Project.Scripts.Controller
{
    /// <summary>
    /// 블록 파괴 관련 로직을 처리하는 매니저 클래스
    /// </summary>
    public class BlockDestroyManager : MonoBehaviour, IGameEventListener<(BoardBlockObject, BlockObject)>
    {
        // 설정 및 참조
        private GameConfig gameConfig;
        private VisualEffectManager visualEffectManager;

        /// <summary>
        /// GameConfig를 통한 초기화
        /// </summary>
        public void Initialize(GameConfig config)
        {
            this.gameConfig = config;

            // 이벤트 등록
            RegisterEvents();
        }

        /// <summary>
        /// 이벤트 등록
        /// </summary>
        private void RegisterEvents()
        {
            if (gameConfig != null && gameConfig.gameEvents != null)
            {
                gameConfig.gameEvents.onCheckDestroy.RegisterListener(this);
            }
        }

        /// <summary>
        /// 이벤트 해제
        /// </summary>
        private void OnDestroy()
        {
            if (gameConfig != null && gameConfig.gameEvents != null)
            {
                gameConfig.gameEvents.onCheckDestroy.UnregisterListener(this);
            }
        }

        /// <summary>
        /// 체크 디스트로이 이벤트 처리
        /// </summary>
        public void OnEventRaised((BoardBlockObject, BlockObject) data)
        {
            var (boardBlock, block) = data;

            // CheckBlockGroupManager에서 파괴 가능 여부 확인
            bool canDestroy = CheckBlockGroupManager.Instance.CheckCanDestroy(boardBlock, block);

            // 파괴 가능하면 블록 파괴 이벤트 발생
            if (canDestroy && block != null)
            {
                gameConfig.gameEvents.onBlockDestroy.Raise(block);
            }
        }

        /// <summary>
        /// 블록 직접 파괴 메서드 (필요시 외부에서 호출 가능)
        /// </summary>
        public void DestroyBlockWithEffect(
            BlockObject block,
            Vector3 movePosition,     // 이동 목표 위치
            Vector3 effectPosition,   // 이미 계산된 파티클 위치
            LaunchDirection direction, // 방향 (길이 계산용)
            ColorType colorType,      // 색상
            Quaternion rotation)      // 회전
        {
            // 블록 유효성 검사
            if (block == null || block.dragHandler == null) return;

            // VisualEffectManager 가져오기
            if (visualEffectManager == null)
            {
                visualEffectManager = StageController.Instance.GetVisualEffectManager();
            }

            // 블록 파괴 이벤트 발생
            gameConfig.gameEvents.onBlockDestroy.Raise(block);

            // 블록 파괴 로직 실행
            block.dragHandler.ReleaseInput();

            // 블록 정리
            foreach (var blockObject in block.dragHandler.blocks)
            {
                if (blockObject.preBoardBlockObject != null)
                {
                    blockObject.preBoardBlockObject.playingBlock = null;
                }
                blockObject.ColliderOff();
            }

            // 드래그 핸들러 비활성화
            block.dragHandler.enabled = false;
            GameObject blockGroup = block.transform.parent.gameObject; // 부모오브젝트에 접근

            // 버텍스 스텐실 효과 적용 (새로 추가)
            if (visualEffectManager != null)
            {
                visualEffectManager.ApplyWallClippingToBlock(blockGroup, effectPosition, (global::LaunchDirection)direction);
            }

            // 블록 길이 계산 (필요한 정보)
            int blockLength = (direction == LaunchDirection.Up || direction == LaunchDirection.Down)
                        ? block.dragHandler.horizon    // 세로 방향 발사면 가로 길이 사용
                        : block.dragHandler.vertical;  // 가로 방향 발사면 세로 길이 사용

            // 파티클 생성 및 이동 애니메이션
            if (visualEffectManager != null)
            {
                ParticleSystem particle = visualEffectManager.CreateParticleEffect(
                    effectPosition,    // 이미 계산된 위치 
                    rotation,          // 이미 계산된 회전
                    colorType,         // 색상
                    blockLength        // 길이
                );

                // 블록 이동 애니메이션 실행
                block.dragHandler.DestroyMove(movePosition, particle);
            }
            else
            {
                // VisualEffectManager가 없는 경우 기본 이동 애니메이션 실행
                block.dragHandler.DestroyMove(movePosition, null);
            }
        }
    }
}