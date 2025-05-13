using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Controller
{
    /// <summary>
    /// ��� �׸��� ��ġ�� ����ϴ� �ڵ鷯
    /// </summary>
    [RequireComponent(typeof(BlockDragHandler))]
    public class BlockGridHandler : MonoBehaviour
    {
        private BlockDragHandler dragHandler;

        private Vector2 centerPos;
        private float blockDistance = 0.79f;

        private void Awake()
        {
            dragHandler = GetComponent<BlockDragHandler>();
        }

        /// <summary>
        /// ��� ��ġ ���� - �׸��忡 ����
        /// </summary>
        public void SetBlockPosition(bool mouseUp = true)
        {
            // ��� �Ʒ� �������� ����ĳ��Ʈ
            Ray ray = new Ray(transform.position, Vector3.down);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // ����ĳ��Ʈ ��Ʈ ��ġ�� �������� ��ǥ ���
                Vector3 coordinate = hit.transform.position;

                // ���콺 �� ������ ���� ���� ��ġ ����
                Vector3 targetPos = new Vector3(coordinate.x, transform.position.y, coordinate.z);
                if (mouseUp) transform.position = targetPos;

                // �׸��� ��ǥ ���
                centerPos.x = Mathf.Round(transform.position.x / blockDistance);
                centerPos.y = Mathf.Round(transform.position.z / blockDistance);

                // ���� ��� ��ȣ�ۿ� ó��
                ProcessBoardBlockInteraction(hit, targetPos);
            }
            else
            {
                Debug.LogWarning("Nothing Detected");
            }
        }

        /// <summary>
        /// ���� ��ϰ��� ��ȣ�ۿ� ó��
        /// </summary>
        private void ProcessBoardBlockInteraction(RaycastHit hit, Vector3 targetPos)
        {
            if (hit.collider.TryGetComponent(out BoardBlockObject boardBlockObject))
            {
                // ��� ���� ����� ��ǥ ����
                foreach (var blockObject in dragHandler.blocks)
                {
                    blockObject.SetCoordinate(centerPos);
                }

                // ���� ��ϰ��� ��ȣ�ۿ� ó��
                foreach (var blockObject in dragHandler.blocks)
                {
                    boardBlockObject.CheckAdjacentBlock(blockObject, targetPos);
                    blockObject.CheckBelowBoardBlock(targetPos);
                }
            }
        }

        /// <summary>
        /// X�� �߽� ��ġ ���
        /// </summary>
        public Vector3 GetCenterX()
        {
            if (dragHandler.blocks == null || dragHandler.blocks.Count == 0)
            {
                return Vector3.zero;
            }

            // X ��ǥ�� �ּ�/�ִ밪 ���
            float minX = float.MaxValue;
            float maxX = float.MinValue;

            foreach (var block in dragHandler.blocks)
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
            if (dragHandler.blocks == null || dragHandler.blocks.Count == 0)
            {
                return Vector3.zero;
            }

            // Z ��ǥ�� �ּ�/�ִ밪 ���
            float minZ = float.MaxValue;
            float maxZ = float.MinValue;

            foreach (var block in dragHandler.blocks)
            {
                float blockZ = block.transform.position.z;

                if (blockZ < minZ) minZ = blockZ;
                if (blockZ > maxZ) maxZ = blockZ;
            }

            // �ּҰ��� �ִ밪�� �߰� ���
            return new Vector3(transform.position.x, transform.position.y, (minZ + maxZ) / 2f);
        }

        /// <summary>
        /// ���� �߽� ��ġ ��ȯ
        /// </summary>
        public Vector2 GetCurrentCenterPos()
        {
            return centerPos;
        }
    }
}