using UnityEditor;
using UnityEngine;
using Project.Scripts.Model;
using Project.Scripts.Controller;
using System.Collections.Generic;
using System;
using static ObjectPropertiesEnum;

namespace Project.Scripts.Editor
{
    // 플레이 모드에서 사용할 시뮬레이션 블록 클래스
    public class SimulationBlock
    {
        public Vector2Int position;
        public ColorType colorType;
        public List<Vector2Int> shapes = new List<Vector2Int>();
        public bool isSelected = false;

        public SimulationBlock(Vector2Int center, ColorType color)
        {
            position = center;
            colorType = color;
        }

        // 블록 이동 메서드
        public bool TryMove(Vector2Int direction, Dictionary<Vector2Int, SimulationBoardBlock> boardBlocks, List<SimulationBlock> otherBlocks, int boardWidth, int boardHeight)
        {
            // 새 위치 계산
            Vector2Int newPosition = position + direction;

            // 모든 모양에 대해 충돌 검사
            foreach (var shape in shapes)
            {
                Vector2Int newBlockPos = newPosition + shape;

                // 보드 경계 검사
                if (newBlockPos.x < 0 || newBlockPos.x >= boardWidth ||
                    newBlockPos.y < 0 || newBlockPos.y >= boardHeight)
                {
                    return false;
                }

                // 보드 블록 존재 여부 검사
                if (!boardBlocks.ContainsKey(newBlockPos))
                {
                    return false;
                }

                // 다른 플레이 블록과의 충돌 검사
                foreach (var otherBlock in otherBlocks)
                {
                    if (otherBlock == this) continue;

                    foreach (var otherShape in otherBlock.shapes)
                    {
                        if (newBlockPos == otherBlock.position + otherShape)
                        {
                            return false;
                        }
                    }
                }
            }

            // 이동 가능하면 위치 업데이트
            position = newPosition;
            return true;
        }
    }

    // 플레이 모드에서 사용할 시뮬레이션 보드 블록 클래스
    public class SimulationBoardBlock
    {
        public Vector2Int position;
        public ColorType colorType;
        public List<ColorType> wallColorTypes = new List<ColorType>();
        public List<bool> wallIsHorizontal = new List<bool>();
        public List<int> wallLengths = new List<int>();

        public SimulationBoardBlock(Vector2Int pos, ColorType color)
        {
            position = pos;
            colorType = color;
        }
    }

    // 플레이 모드에서 사용할 시뮬레이션 벽 클래스
    public class SimulationWall
    {
        public Vector2Int position;
        public ColorType colorType;
        public ObjectPropertiesEnum.WallDirection direction;
        public int length;
        public WallGimmickType gimmickType;

        public SimulationWall(Vector2Int pos, ColorType color, ObjectPropertiesEnum.WallDirection dir, int len, WallGimmickType gimmick)
        {
            position = pos;
            colorType = color;
            direction = dir;
            length = len;
            gimmickType = gimmick;
        }

        // 방향이 수평인지 확인
        public bool IsHorizontal()
        {
            return direction == ObjectPropertiesEnum.WallDirection.Single_Up ||
                   direction == ObjectPropertiesEnum.WallDirection.Single_Down ||
                   direction == ObjectPropertiesEnum.WallDirection.Left_Up ||
                   direction == ObjectPropertiesEnum.WallDirection.Left_Down ||
                   direction == ObjectPropertiesEnum.WallDirection.Right_Up ||
                   direction == ObjectPropertiesEnum.WallDirection.Right_Down ||
                   direction == ObjectPropertiesEnum.WallDirection.Open_Up ||
                   direction == ObjectPropertiesEnum.WallDirection.Open_Down;
        }

        // 방향이 Up 계열인지 확인
        public bool IsUpDirection()
        {
            return direction == ObjectPropertiesEnum.WallDirection.Single_Up ||
                   direction == ObjectPropertiesEnum.WallDirection.Left_Up ||
                   direction == ObjectPropertiesEnum.WallDirection.Right_Up ||
                   direction == ObjectPropertiesEnum.WallDirection.Open_Up;
        }

        // 방향이 Down 계열인지 확인
        public bool IsDownDirection()
        {
            return direction == ObjectPropertiesEnum.WallDirection.Single_Down ||
                   direction == ObjectPropertiesEnum.WallDirection.Left_Down ||
                   direction == ObjectPropertiesEnum.WallDirection.Right_Down ||
                   direction == ObjectPropertiesEnum.WallDirection.Open_Down;
        }

        // 방향이 Left 계열인지 확인
        public bool IsLeftDirection()
        {
            return direction == ObjectPropertiesEnum.WallDirection.Single_Left ||
                   direction == ObjectPropertiesEnum.WallDirection.Open_Left;
        }

        // 방향이 Right 계열인지 확인
        public bool IsRightDirection()
        {
            return direction == ObjectPropertiesEnum.WallDirection.Single_Right ||
                   direction == ObjectPropertiesEnum.WallDirection.Open_Right;
        }
    }
}