using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Controller
{
    /// <summary>
    /// 블록 그리드 배치를 담당하는 핸들러
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
        /// 블록 위치 설정 - 그리드에 정렬
        /// </summary>
        public void SetBlockPosition(bool mouseUp = true)
        {
            // 블록 아래 방향으로 레이캐스트
            Ray ray = new Ray(transform.position, Vector3.down);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // 레이캐스트 히트 위치를 기준으로 좌표 계산
                Vector3 coordinate = hit.transform.position;

                // 마우스 업 상태일 때만 실제 위치 변경
                Vector3 targetPos = new Vector3(coordinate.x, transform.position.y, coordinate.z);
                if (mouseUp) transform.position = targetPos;

                // 그리드 좌표 계산
                centerPos.x = Mathf.Round(transform.position.x / blockDistance);
                centerPos.y = Mathf.Round(transform.position.z / blockDistance);

                // 보드 블록 상호작용 처리
                ProcessBoardBlockInteraction(hit, targetPos);
            }
            else
            {
                Debug.LogWarning("Nothing Detected");
            }
        }

        /// <summary>
        /// 보드 블록과의 상호작용 처리
        /// </summary>
        private void ProcessBoardBlockInteraction(RaycastHit hit, Vector3 targetPos)
        {
            if (hit.collider.TryGetComponent(out BoardBlockObject boardBlockObject))
            {
                // 모든 개별 블록의 좌표 설정
                foreach (var blockObject in dragHandler.blocks)
                {
                    blockObject.SetCoordinate(centerPos);
                }

                // 보드 블록과의 상호작용 처리
                foreach (var blockObject in dragHandler.blocks)
                {
                    boardBlockObject.CheckAdjacentBlock(blockObject, targetPos);
                    blockObject.CheckBelowBoardBlock(targetPos);
                }
            }
        }

        /// <summary>
        /// X축 중심 위치 계산
        /// </summary>
        public Vector3 GetCenterX()
        {
            if (dragHandler.blocks == null || dragHandler.blocks.Count == 0)
            {
                return Vector3.zero;
            }

            // X 좌표의 최소/최대값 계산
            float minX = float.MaxValue;
            float maxX = float.MinValue;

            foreach (var block in dragHandler.blocks)
            {
                float blockX = block.transform.position.x;

                if (blockX < minX) minX = blockX;
                if (blockX > maxX) maxX = blockX;
            }

            // 최소값과 최대값의 중간 계산
            return new Vector3((minX + maxX) / 2f, transform.position.y, 0);
        }

        /// <summary>
        /// Z축 중심 위치 계산
        /// </summary>
        public Vector3 GetCenterZ()
        {
            if (dragHandler.blocks == null || dragHandler.blocks.Count == 0)
            {
                return Vector3.zero;
            }

            // Z 좌표의 최소/최대값 계산
            float minZ = float.MaxValue;
            float maxZ = float.MinValue;

            foreach (var block in dragHandler.blocks)
            {
                float blockZ = block.transform.position.z;

                if (blockZ < minZ) minZ = blockZ;
                if (blockZ > maxZ) maxZ = blockZ;
            }

            // 최소값과 최대값의 중간 계산
            return new Vector3(transform.position.x, transform.position.y, (minZ + maxZ) / 2f);
        }

        /// <summary>
        /// 현재 중심 위치 반환
        /// </summary>
        public Vector2 GetCurrentCenterPos()
        {
            return centerPos;
        }
    }
}