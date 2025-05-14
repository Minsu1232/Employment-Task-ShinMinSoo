using UnityEditor;
using UnityEngine;
using Project.Scripts.Model;
using Project.Scripts.Controller;
using System.Collections.Generic;
using System;
using static ObjectPropertiesEnum;

namespace Project.Scripts.Editor
{
    // �÷��� ��忡�� ����� �ùķ��̼� ��� Ŭ����
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

        // ��� �̵� �޼���
        public bool TryMove(Vector2Int direction, Dictionary<Vector2Int, SimulationBoardBlock> boardBlocks, List<SimulationBlock> otherBlocks, int boardWidth, int boardHeight)
        {
            // �� ��ġ ���
            Vector2Int newPosition = position + direction;

            // ��� ��翡 ���� �浹 �˻�
            foreach (var shape in shapes)
            {
                Vector2Int newBlockPos = newPosition + shape;

                // ���� ��� �˻�
                if (newBlockPos.x < 0 || newBlockPos.x >= boardWidth ||
                    newBlockPos.y < 0 || newBlockPos.y >= boardHeight)
                {
                    return false;
                }

                // ���� ��� ���� ���� �˻�
                if (!boardBlocks.ContainsKey(newBlockPos))
                {
                    return false;
                }

                // �ٸ� �÷��� ��ϰ��� �浹 �˻�
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

            // �̵� �����ϸ� ��ġ ������Ʈ
            position = newPosition;
            return true;
        }
    }

    // �÷��� ��忡�� ����� �ùķ��̼� ���� ��� Ŭ����
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

    // �÷��� ��忡�� ����� �ùķ��̼� �� Ŭ����
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

        // ������ �������� Ȯ��
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

        // ������ Up �迭���� Ȯ��
        public bool IsUpDirection()
        {
            return direction == ObjectPropertiesEnum.WallDirection.Single_Up ||
                   direction == ObjectPropertiesEnum.WallDirection.Left_Up ||
                   direction == ObjectPropertiesEnum.WallDirection.Right_Up ||
                   direction == ObjectPropertiesEnum.WallDirection.Open_Up;
        }

        // ������ Down �迭���� Ȯ��
        public bool IsDownDirection()
        {
            return direction == ObjectPropertiesEnum.WallDirection.Single_Down ||
                   direction == ObjectPropertiesEnum.WallDirection.Left_Down ||
                   direction == ObjectPropertiesEnum.WallDirection.Right_Down ||
                   direction == ObjectPropertiesEnum.WallDirection.Open_Down;
        }

        // ������ Left �迭���� Ȯ��
        public bool IsLeftDirection()
        {
            return direction == ObjectPropertiesEnum.WallDirection.Single_Left ||
                   direction == ObjectPropertiesEnum.WallDirection.Open_Left;
        }

        // ������ Right �迭���� Ȯ��
        public bool IsRightDirection()
        {
            return direction == ObjectPropertiesEnum.WallDirection.Single_Right ||
                   direction == ObjectPropertiesEnum.WallDirection.Open_Right;
        }
    }
}