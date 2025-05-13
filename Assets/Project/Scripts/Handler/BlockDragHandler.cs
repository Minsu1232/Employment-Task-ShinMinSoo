using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Project.Scripts.Controller
{
    /// <summary>
    /// ��� �巡�� �ڵ鷯 - ������ Ŭ����
    /// </summary>
    public class BlockDragHandler : MonoBehaviour
    {
        // ��� �Ӽ� ����
        public int horizon = 1;
        public int vertical = 1;
        public int uniqueIndex;
        public List<ObjectPropertiesEnum.BlockGimmickType> gimmickType = new List<ObjectPropertiesEnum.BlockGimmickType>();
        public List<BlockObject> blocks = new List<BlockObject>();
        public List<Vector2> blockOffsets = new List<Vector2>();
        public bool Enabled = true;

        // ���� ����
        public bool IsDragging { get; set; } = false;
        public Collider col { get; set; }

        // ������Ʈ ����
        private BlockInputHandler inputHandler;
        private BlockPhysicsHandler physicsHandler;
        private BlockGridHandler gridHandler;

        // �̺�Ʈ ��������Ʈ
        public delegate void BlockDestroyedHandler(BlockObject block);
        public BlockDestroyedHandler OnBlockDestroyed;

        private void Awake()
        {
            // �ʿ��� ������Ʈ �߰� �� ���� ����
            inputHandler = GetComponent<BlockInputHandler>() ?? gameObject.AddComponent<BlockInputHandler>();
            physicsHandler = GetComponent<BlockPhysicsHandler>() ?? gameObject.AddComponent<BlockPhysicsHandler>();
            gridHandler = GetComponent<BlockGridHandler>() ?? gameObject.AddComponent<BlockGridHandler>();
        }

        /// <summary>
        /// X�� �߽� ��ġ ���
        /// </summary>
        public Vector3 GetCenterX()
        {
            if (gridHandler != null)
            {
                return gridHandler.GetCenterX();
            }

            // GridHandler�� ���� ��� ���� ���� ���
            if (blocks == null || blocks.Count == 0)
            {
                return Vector3.zero; // ����Ʈ�� ��������� �⺻�� ��ȯ
            }

            // X ��ǥ�� �ּ�/�ִ밪 ���
            float minX = float.MaxValue;
            float maxX = float.MinValue;

            foreach (var block in blocks)
            {
                float blockX = block.transform.position.x;

                if (blockX < minX) minX = blockX;
                if (blockX > maxX) maxX = blockX;
            }

            // �ּҰ��� �ִ밪�� �߰� ���
            return new Vector3((minX + maxX) / 2f, transform.position.y, 0);
        }

        /// <summary>
        /// Z�� �߽� ��ġ ���
        /// </summary>
        public Vector3 GetCenterZ()
        {
            if (gridHandler != null)
            {
                return gridHandler.GetCenterZ();
            }

            // GridHandler�� ���� ��� ���� ���� ���
            if (blocks == null || blocks.Count == 0)
            {
                return Vector3.zero; // ����Ʈ�� ��������� �⺻�� ��ȯ
            }

            // Z ��ǥ�� �ּ�/�ִ밪 ���
            float minZ = float.MaxValue;
            float maxZ = float.MinValue;

            foreach (var block in blocks)
            {
                float blockZ = block.transform.position.z;

                if (blockZ < minZ) minZ = blockZ;
                if (blockZ > maxZ) maxZ = blockZ;
            }

            // �ּҰ��� �ִ밪�� �߰� ���
            return new Vector3(transform.position.x, transform.position.y, (minZ + maxZ) / 2f);
        }

        /// <summary>
        /// ���� ���� ��� ���� ����
        /// </summary>
        private void ClearPreboardBlockObjects()
        {
            foreach (var b in blocks)
            {
                if (b.preBoardBlockObject != null)
                {
                    b.preBoardBlockObject.playingBlock = null;
                }
            }
        }

        /// <summary>
        /// ��� �ı� �ִϸ��̼�
        /// </summary>
        public void DestroyMove(Vector3 pos, ParticleSystem particle)
        {
            ClearPreboardBlockObjects();

            // �ִϸ��̼� ���� �� ���� �۾�
            transform.DOMove(pos, 1f).SetEase(Ease.Linear)
                .OnComplete(() => {
                    if (particle != null)
                    {
                        Destroy(particle.gameObject);
                    }

                    // ��� �ı� �̺�Ʈ ȣ��
                    OnBlockDestroyed?.Invoke(blocks.Count > 0 ? blocks[0] : null);

                    Destroy(gameObject);
                });
        }

        /// <summary>
        /// �Է� ���� ����
        /// </summary>
        public void ReleaseInput()
        {
            if (inputHandler != null)
            {
                inputHandler.ReleaseInput();
            }
        }

        /// <summary>
        /// ���ҽ� ����
        /// </summary>
        private void OnDisable()
        {
            transform.DOKill(true);
        }

        /// <summary>
        /// ���ҽ� ����
        /// </summary>
        private void OnDestroy()
        {
            transform.DOKill(true);
        }
    }
}