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
    /// ��� �ı� ���� ������ ó���ϴ� �Ŵ��� Ŭ����
    /// </summary>
    public class BlockDestroyManager : MonoBehaviour, IGameEventListener<(BoardBlockObject, BlockObject)>
    {
        // ���� �� ����
        private GameConfig gameConfig;
        private VisualEffectManager visualEffectManager;

        /// <summary>
        /// GameConfig�� ���� �ʱ�ȭ
        /// </summary>
        public void Initialize(GameConfig config)
        {
            this.gameConfig = config;

            // �̺�Ʈ ���
            RegisterEvents();
        }

        /// <summary>
        /// �̺�Ʈ ���
        /// </summary>
        private void RegisterEvents()
        {
            if (gameConfig != null && gameConfig.gameEvents != null)
            {
                gameConfig.gameEvents.onCheckDestroy.RegisterListener(this);
            }
        }

        /// <summary>
        /// �̺�Ʈ ����
        /// </summary>
        private void OnDestroy()
        {
            if (gameConfig != null && gameConfig.gameEvents != null)
            {
                gameConfig.gameEvents.onCheckDestroy.UnregisterListener(this);
            }
        }

        /// <summary>
        /// üũ ��Ʈ���� �̺�Ʈ ó��
        /// </summary>
        public void OnEventRaised((BoardBlockObject, BlockObject) data)
        {
            var (boardBlock, block) = data;

            // CheckBlockGroupManager���� �ı� ���� ���� Ȯ��
            bool canDestroy = CheckBlockGroupManager.Instance.CheckCanDestroy(boardBlock, block);

            // �ı� �����ϸ� ��� �ı� �̺�Ʈ �߻�
            if (canDestroy && block != null)
            {
                gameConfig.gameEvents.onBlockDestroy.Raise(block);
            }
        }

        /// <summary>
        /// ��� ���� �ı� �޼��� (�ʿ�� �ܺο��� ȣ�� ����)
        /// </summary>
        public void DestroyBlockWithEffect(
            BlockObject block,
            Vector3 movePosition,     // �̵� ��ǥ ��ġ
            Vector3 effectPosition,   // �̹� ���� ��ƼŬ ��ġ
            LaunchDirection direction, // ���� (���� ����)
            ColorType colorType,      // ����
            Quaternion rotation)      // ȸ��
        {
            // ��� ��ȿ�� �˻�
            if (block == null || block.dragHandler == null) return;

            // VisualEffectManager ��������
            if (visualEffectManager == null)
            {
                visualEffectManager = StageController.Instance.GetVisualEffectManager();
            }

            // ��� �ı� �̺�Ʈ �߻�
            gameConfig.gameEvents.onBlockDestroy.Raise(block);

            // ��� �ı� ���� ����
            block.dragHandler.ReleaseInput();

            // ��� ����
            foreach (var blockObject in block.dragHandler.blocks)
            {
                if (blockObject.preBoardBlockObject != null)
                {
                    blockObject.preBoardBlockObject.playingBlock = null;
                }
                blockObject.ColliderOff();
            }

            // �巡�� �ڵ鷯 ��Ȱ��ȭ
            block.dragHandler.enabled = false;
            GameObject blockGroup = block.transform.parent.gameObject; // �θ������Ʈ�� ����

            // ���ؽ� ���ٽ� ȿ�� ���� (���� �߰�)
            if (visualEffectManager != null)
            {
                visualEffectManager.ApplyWallClippingToBlock(blockGroup, effectPosition, (global::LaunchDirection)direction);
            }

            // ��� ���� ��� (�ʿ��� ����)
            int blockLength = (direction == LaunchDirection.Up || direction == LaunchDirection.Down)
                        ? block.dragHandler.horizon    // ���� ���� �߻�� ���� ���� ���
                        : block.dragHandler.vertical;  // ���� ���� �߻�� ���� ���� ���

            // ��ƼŬ ���� �� �̵� �ִϸ��̼�
            if (visualEffectManager != null)
            {
                ParticleSystem particle = visualEffectManager.CreateParticleEffect(
                    effectPosition,    // �̹� ���� ��ġ 
                    rotation,          // �̹� ���� ȸ��
                    colorType,         // ����
                    blockLength        // ����
                );

                // ��� �̵� �ִϸ��̼� ����
                block.dragHandler.DestroyMove(movePosition, particle);
            }
            else
            {
                // VisualEffectManager�� ���� ��� �⺻ �̵� �ִϸ��̼� ����
                block.dragHandler.DestroyMove(movePosition, null);
            }
        }
    }
}