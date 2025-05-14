// 5. StageEditorWindow.Input.cs - �Է� ó�� ���� �޼���
using UnityEditor;
using UnityEngine;
using Project.Scripts.Model;
using System.Collections.Generic;
using static ObjectPropertiesEnum;

namespace Project.Scripts.Editor
{
    public partial class StageEditorWindow
    {
        #region �Է� ó��


        private void HandleInput()
        {
            Event e = Event.current;

            // ���� ���콺 ��ġ�� �׸��� ��ǥ�� ������Ʈ (��� �̺�Ʈ����)
            if (gridViewRect.Contains(e.mousePosition))
            {
                Vector2 gridPos = e.mousePosition;
                gridPos -= gridViewRect.position;  // �׸��� �� ��ġ ����
                gridPos += scrollPosition;         // ��ũ�� ��ġ �ݿ�

                // ������ �׸��� ��ǥ
                int editorGridX = Mathf.FloorToInt(gridPos.x / gridSize);
                int editorGridY = Mathf.FloorToInt(gridPos.y / gridSize);

                // ������ ��ǥ�� ���� ��ǥ�� ��ȯ�Ͽ� ����
                currentMouseGridPos.x = editorGridX;
                currentMouseGridPos.y = EditorToWorldY(editorGridY);

                // ���� ��� ��忡�� �巡�� ���̸� �� ���� ó�� - �÷��� ���
                if (isDraggingInFreeFormMode && currentTool == EditTool.PlayingBlock &&
                    playingBlockMode == PlayingBlockMode.FreeForm)
                {
                    HandleFreeFormDrag(currentMouseGridPos);
                }

                // ���� ��� ��忡�� �巡�� ���̸� �� ���� ó�� - ���� ���
                if (isDraggingBoardFreeForm && currentTool == EditTool.BoardBlock &&
                    boardBlockMode == BoardBlockMode.FreeForm)
                {
                    HandleBoardFreeFormDrag(currentMouseGridPos);
                }

                // ���콺�� ������ ������ �׸��带 ���� (�巡�� �ð�ȭ�� ����)
                if (e.type == EventType.MouseMove || e.type == EventType.MouseDrag)
                {
                    Repaint();
                }
            }

            // Ű���� ����Ű ó��
            if (e.type == EventType.KeyDown)
            {
                // ���� Ű�� ���� ���� ���� (1-5)
                if (e.keyCode >= KeyCode.Alpha1 && e.keyCode <= KeyCode.Alpha5)
                {
                    int toolIndex = (int)e.keyCode - (int)KeyCode.Alpha1;
                    if (toolIndex < System.Enum.GetValues(typeof(EditTool)).Length)
                    {
                        currentTool = (EditTool)toolIndex;
                        e.Use();
                        Repaint();
                        return;
                    }
                }
            }

            if (e.type == EventType.MouseDown)
            {
                Vector2 mousePos = e.mousePosition;
                mousePos -= gridViewRect.position;   // ��ũ�Ѻ� ���� ��ǥ�� ����
                mousePos += scrollPosition;          // ���� ��ũ�� �ݿ�

                // ������ ��ǥ ���
                int editorGridX = Mathf.FloorToInt(mousePos.x / gridSize);
                int editorGridY = Mathf.FloorToInt(mousePos.y / gridSize);

                // ���� ��ǥ ��� (�����)
                int worldGridX = editorGridX;
                int worldGridY = EditorToWorldY(editorGridY);

                // �׸��� ���� üũ
                if (editorGridX >= 0 && editorGridX < gridWidth && editorGridY >= 0 && editorGridY < gridHeight)
                {
                    // ���� ���콺 ��ư Ŭ��
                    if (e.button == 0)
                    {
                        // ������ ���� �ٸ� �׼� ����
                        switch (currentTool)
                        {
                            case EditTool.BoardBlock:
                                if (boardBlockMode == BoardBlockMode.Rectangle)
                                {
                                    // �簢�� ��� - �巡�� ����
                                    isDraggingBoardBlock = true;
                                    boardDragStart = new Vector2Int(editorGridX, editorGridY);
                                    // Ŭ������ ���� ��� ��ġ�� ����
                                    PlaceBoardBlock(worldGridX, worldGridY);
                                }
                                else // FreeForm ���
                                {
                                    // ���� ��� ��� - �巡�� ���� �� �� ���� ó��
                                    isDraggingBoardFreeForm = true;
                                    Vector2Int cell = new Vector2Int(worldGridX, worldGridY);
                                    lastDraggedBoardCell = cell;

                                    // Shift Ű�� ���� ���¸� �߰�/����
                                    if (e.shift)
                                    {
                                        if (selectedBoardCells.Contains(cell))
                                            selectedBoardCells.Remove(cell);
                                        else
                                            selectedBoardCells.Add(cell);
                                    }
                                    else // �Ϲ� Ŭ���̸� ���� ���� �ʱ�ȭ�ϰ� ���� ����
                                    {
                                        selectedBoardCells.Clear();
                                        selectedBoardCells.Add(cell);
                                    }
                                }
                                break;

                            case EditTool.PlayingBlock:
                                if (playingBlockMode == PlayingBlockMode.Rectangle)
                                {
                                    // �簢�� ��� - �巡�� ���� (������ ��ǥ ���)
                                    startDragPosition = new Vector2Int(editorGridX, editorGridY);
                                    CheckPlayingBlockSelection(worldGridX, worldGridY);
                                }
                                else // FreeForm ���
                                {
                                    // ���� ��� ��� - �巡�� ���� �� �� ���� ó��
                                    isDraggingInFreeFormMode = true;
                                    Vector2Int cell = new Vector2Int(worldGridX, worldGridY);
                                    lastDraggedCell = cell;

                                    // ���� ��� ���� üũ
                                    int oldIndex = currentPlayingBlockIndex;
                                    CheckPlayingBlockSelection(worldGridX, worldGridY);

                                    // ����� ���õ��� ���� ��츸 �� ���� ó��
                                    if (currentPlayingBlockIndex < 0)
                                    {
                                        // Shift Ű�� ���� ���¸� �߰�/����
                                        if (e.shift)
                                        {
                                            if (selectedCells.Contains(cell))
                                                selectedCells.Remove(cell);
                                            else
                                                selectedCells.Add(cell);
                                        }
                                        else // �Ϲ� Ŭ���̸� ���� ���� �ʱ�ȭ�ϰ� ���� ����
                                        {
                                            if (oldIndex < 0) // �������� ����� ���õ��� �ʾ��� ����
                                            {
                                                selectedCells.Clear();
                                                selectedCells.Add(cell);
                                            }
                                        }
                                    }
                                }
                                break;

                            case EditTool.Wall:
                                PlaceWall(worldGridX, worldGridY);
                                break;

                            case EditTool.Gimmick:
                                // ��� ���� ���� ��, ���� �� ���� ���� Ȯ��
                                CheckWallSelection(worldGridX, worldGridY);

                                // ���� ���õǾ����� ��� ����
                                if (currentWallIndex >= 0)
                                {
                                    PlaceWallGimmick(worldGridX, worldGridY);
                                }
                                else
                                {
                                    // ������ ��� ��� ó�� ����
                                    PlaceGimmick(worldGridX, worldGridY);
                                }
                                break;

                            case EditTool.Erase:
                                EraseObject(worldGridX, worldGridY);
                                break;
                        }
                        e.Use();
                    }
                }
            }
            else if (e.type == EventType.MouseDrag && e.button == 0)
            {
                // ���� ��� ��忡�� �巡�� ó���� �̹� ��ܿ��� ó����
                e.Use();
            }
            else if (e.type == EventType.MouseUp && e.button == 0)
            {
                Vector2 mousePos = e.mousePosition;
                mousePos -= gridViewRect.position;   // ��ũ�Ѻ� ���� ��ǥ�� ����
                mousePos += scrollPosition;          // ���� ��ũ�� �ݿ�

                // ������ ��ǥ ���
                int editorGridX = Mathf.FloorToInt(mousePos.x / gridSize);
                int editorGridY = Mathf.FloorToInt(mousePos.y / gridSize);

                // ���� ��ǥ ��� (�����)
                int worldGridX = editorGridX;
                int worldGridY = EditorToWorldY(editorGridY);

                // �簢�� ��� ���� ��� �巡�� ó��
                if (isDraggingBoardBlock && boardDragStart.HasValue && currentTool == EditTool.BoardBlock &&
                    boardBlockMode == BoardBlockMode.Rectangle)
                {
                    // �׸��� ���� üũ
                    if (editorGridX >= 0 && editorGridX < gridWidth && editorGridY >= 0 && editorGridY < gridHeight)
                    {
                        // �巡�� ���� ��� (������ ��ǥ)
                        int startEditorX = Mathf.Min(boardDragStart.Value.x, editorGridX);
                        int startEditorY = Mathf.Min(boardDragStart.Value.y, editorGridY);
                        int endEditorX = Mathf.Max(boardDragStart.Value.x, editorGridX);
                        int endEditorY = Mathf.Max(boardDragStart.Value.y, editorGridY);

                        // ���� �� ��� ���� ���� ��� ��ġ (���� ��ǥ�� ��ȯ�Ͽ� ����)
                        for (int x = startEditorX; x <= endEditorX; x++)
                        {
                            for (int y = startEditorY; y <= endEditorY; y++)
                            {
                                int worldY = EditorToWorldY(y);
                                PlaceBoardBlock(x, worldY);
                            }
                        }
                    }

                    isDraggingBoardBlock = false;
                    boardDragStart = null;
                }

                // ���� ��� ���� ��� ��� �巡�� ����
                if (isDraggingBoardFreeForm && currentTool == EditTool.BoardBlock &&
                    boardBlockMode == BoardBlockMode.FreeForm)
                {
                    isDraggingBoardFreeForm = false;
                    lastDraggedBoardCell = new Vector2Int(-1, -1);
                }

                // �簢�� ����� �� �÷��� ��� ���� ó��
                if (startDragPosition.HasValue && currentTool == EditTool.PlayingBlock &&
                    playingBlockMode == PlayingBlockMode.Rectangle)
                {
                    // �׸��� ���� üũ
                    if (editorGridX >= 0 && editorGridX < gridWidth && editorGridY >= 0 && editorGridY < gridHeight)
                    {
                        // �ּ� �巡�� �Ÿ� Ȯ��
                        bool validDrag = (startDragPosition.Value.x != editorGridX || startDragPosition.Value.y != editorGridY);

                        // �巡�װ� ��ȿ�ϰų�, ����� ���õ��� ���� ��쿡�� ��� ����
                        if (validDrag || currentPlayingBlockIndex < 0)
                        {
                            FinishDragAndPlacePlayingBlock(worldGridX, worldGridY);
                        }
                    }

                    startDragPosition = null;
                }

                // ���� ��� ��� �巡�� ����
                if (isDraggingInFreeFormMode && currentTool == EditTool.PlayingBlock &&
                    playingBlockMode == PlayingBlockMode.FreeForm)
                {
                    isDraggingInFreeFormMode = false;
                    lastDraggedCell = new Vector2Int(-1, -1);
                }

                e.Use();
            }
        }
        // ���� ��� ���� ��� ��� �巡�� ó�� �޼���
        private void HandleBoardFreeFormDrag(Vector2Int currentCell)
        {
            // ���������� �巡�׵� ���� �����ϸ� ����
            if (currentCell == lastDraggedBoardCell)
                return;

            // ���� ���� �̹� ���õǾ� ���� ������ ����
            if (!selectedBoardCells.Contains(currentCell))
            {
                selectedBoardCells.Add(currentCell);
            }

            // ���������� �巡�׵� �� ������Ʈ
            lastDraggedBoardCell = currentCell;
        }
        // ���� ��� ���� ��� ���� �޼���
        private void CreateFreeFormBoardBlocks()
        {
            if (selectedBoardCells.Count == 0)
                return;

            // ���õ� ��� ���� ���� ��� ��ġ
            foreach (var cell in selectedBoardCells)
            {
                PlaceBoardBlock(cell.x, cell.y);
            }

            // ���õ� �� �ʱ�ȭ
            selectedBoardCells.Clear();
        }
        // ���� ��� ��忡�� �巡�׷� �� ���� ó��
        // HandleFreeFormDrag �޼��� ����
        private void HandleFreeFormDrag(Vector2Int currentCell)
        {
            // ���������� �巡�׵� ���� �����ϸ� ����
            if (currentCell == lastDraggedCell)
                return;

            // ����� �̹� ���õ� ���¸� ����
            if (currentPlayingBlockIndex >= 0)
                return;

            // ���� ���� �̹� ���õǾ� ���� ������ ����
            if (!selectedCells.Contains(currentCell))
            {
                selectedCells.Add(currentCell);
            }

            // ���������� �巡�׵� �� ������Ʈ
            lastDraggedCell = currentCell;
        }

        private void PlaceBoardBlock(int x, int y)
        {
            // ���� ��� Ȯ��
            // ���� �ڵ� �״�� ���
            BoardBlockData existingBlock = boardBlocks.Find(b => b.x == x && b.y == y);

            if (existingBlock != null)
            {
                existingBlock.ColorType = selectedColor;
            }
            else
            {
                BoardBlockData newBlock = new BoardBlockData(x, y);
                newBlock.ColorType = selectedColor;
                boardBlocks.Add(newBlock);
            }
        }

        private void CheckPlayingBlockSelection(int x, int y)
        {
            // �÷��� ��� ���� üũ
            for (int i = 0; i < playingBlocks.Count; i++)
            {
                var block = playingBlocks[i];

                foreach (var shape in block.shapes)
                {
                    int blockX = block.center.x + shape.offset.x;
                    int blockY = block.center.y + shape.offset.y;

                    if (blockX == x && blockY == y)
                    {
                        currentPlayingBlockIndex = i;
                        return;
                    }
                }
            }

            // ���õ� ����� ������ �� ��� ���� ���� ��ȯ
            currentPlayingBlockIndex = -1;
        }

        // ��� ���� ���θ� Ȯ���ϴ� ���� �Լ� �߰�
        private bool IsBlockAtPosition(int x, int y)
        {
            foreach (var block in playingBlocks)
            {
                foreach (var shape in block.shapes)
                {
                    int blockX = block.center.x + shape.offset.x;
                    int blockY = block.center.y + shape.offset.y;

                    if (blockX == x && blockY == y)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void FinishDragAndPlacePlayingBlock(int endX, int endY)
        {
            // �̹� ����� ���õǾ� ������ �� ����� �������� ����
            if (currentPlayingBlockIndex >= 0)
            {
                return;
            }

            // �巡�� �������� ������ ��ǥ
            Vector2Int dragStart = startDragPosition.Value;

            // endY�� ���� ��ǥ�� ���� (EditorToWorldY �Լ��� �̹� ��ȯ�� ����)
            // ������ ��ǥ�� �ٽ� ��ȯ
            int endEditorY = WorldToEditorY(endY);

            // ������ ��ǥ �������� ���� ���
            int startEditorX = Mathf.Min(dragStart.x, endX);
            int startEditorY = Mathf.Min(dragStart.y, endEditorY);
            int endEditorX = Mathf.Max(dragStart.x, endX);
            int endEditor_Y = Mathf.Max(dragStart.y, endEditorY);

            int width = endEditorX - startEditorX + 1;
            int height = endEditor_Y - startEditorY + 1;

            // �߽� ��ġ ��� - �巡�� ������ ���� �߽����� ���
            // 1. ������ ��ǥ���� �߽��� ���
            int centerEditorX = startEditorX + (width - 1) / 2;
            int centerEditorY = startEditorY + (height - 1) / 2;

            // 2. ������ ��ǥ�� ���� ��ǥ�� ��ȯ
            int centerWorldX = centerEditorX;
            int centerWorldY = EditorToWorldY(centerEditorY);

            // �� ��� ����
            PlayingBlockData newBlock = new PlayingBlockData();
            newBlock.center = new Vector2Int(centerWorldX, centerWorldY);  // ���� ��ǥ�� �߽� ����
            newBlock.ColorType = selectedColor;
            newBlock.uniqueIndex = playingBlocks.Count;

            // ��� ��� �߰� - �߽��� ���� ������ ���
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // ������ ��ǥ���� ������ ���
                    int offsetEditorX = (startEditorX + x) - centerEditorX;
                    int offsetEditorY = (startEditorY + y) - centerEditorY;

                    // Y ��ǥ�� ��ȯ �ʿ� (X�� ����)
                    int offsetWorldX = offsetEditorX;
                    int offsetWorldY = -offsetEditorY; // Y�� ������ �ݴ�� ��ȣ ����

                    ShapeData shape = new ShapeData(offsetWorldX, offsetWorldY);
                    newBlock.AddShape(shape);
                }
            }

            // �⺻ ��� �߰�
            GimmickData gimmick = new GimmickData();
            gimmick.gimmickType = "None";
            newBlock.AddGimmick(gimmick);

            playingBlocks.Add(newBlock);
            currentPlayingBlockIndex = playingBlocks.Count - 1;
        }

        // ���� ��� ��� ���� �޼���
        // CreateFreeFormPlayingBlock �޼��� ����
        private void CreateFreeFormPlayingBlock()
        {
            if (selectedCells.Count == 0)
                return;

            // �߽��� ���� (ù ��° ������ ��)
            Vector2Int center = selectedCells[0];

            // �� ��� ����
            PlayingBlockData newBlock = new PlayingBlockData();
            newBlock.center = center;
            newBlock.ColorType = selectedColor;
            newBlock.uniqueIndex = playingBlocks.Count;

            // ���õ� ��� ���� �߽��� ���� ���������� ��ȯ
            foreach (var cell in selectedCells)
            {
                ShapeData shape = new ShapeData(
                    cell.x - center.x,  // x ������
                    cell.y - center.y   // y ������
                );
                newBlock.AddShape(shape);
            }

            // �⺻ ��� �߰�
            GimmickData gimmick = new GimmickData();
            gimmick.gimmickType = "None";
            newBlock.AddGimmick(gimmick);

            playingBlocks.Add(newBlock);
            currentPlayingBlockIndex = playingBlocks.Count - 1;

            // ���õ� �� �ʱ�ȭ
            selectedCells.Clear();
        }

        private void PlaceWall(int x, int y)
        {
            // �̹� ���õ� ���� ������ ó������ ���� (��� ������ ������� �� ����)
            if (currentTool == EditTool.Gimmick && currentWallIndex >= 0)
                return;

            // ���� �� Ȯ��
            WallData existingWall = walls.Find(w =>
                w.x == x && w.y == y && w.WallDirection == wallDirection);

            if (existingWall != null)
            {
                // ���� ���� ���� ��� ���� ���·� ����
                currentWallIndex = walls.IndexOf(existingWall);
            }
            else
            {
                // �� �� ���� - �⺻�����δ� ��� ���� ����
                WallData wall = new WallData(x, y, wallDirection, wallLength, wallColor, WallGimmickType.None);
                walls.Add(wall);
                currentWallIndex = walls.Count - 1;
            }
        }
        // �� ���� �޼��� �߰� (CheckWallSelection)
        private void CheckWallSelection(int x, int y)
        {
            // ���� ���� �߿��� ���õ� ��ġ�� �ִ� �� ã��
            for (int i = 0; i < walls.Count; i++)
            {
                var wall = walls[i];
                if (wall.x == x && wall.y == y)
                {
                    currentWallIndex = i;
                    return;
                }

                // Ȯ��� �� ������ �˻�
                if (wall.Length > 1)
                {
                    bool isExtendedWall = false;

                    // �� ���⿡ ���� Ȯ�� ���� ���
                    switch (wall.WallDirection)
                    {
                        case ObjectPropertiesEnum.WallDirection.Single_Up:
                        case ObjectPropertiesEnum.WallDirection.Single_Down:
                        case ObjectPropertiesEnum.WallDirection.Left_Up:
                        case ObjectPropertiesEnum.WallDirection.Left_Down:
                        case ObjectPropertiesEnum.WallDirection.Right_Up:
                        case ObjectPropertiesEnum.WallDirection.Right_Down:
                        case ObjectPropertiesEnum.WallDirection.Open_Up:
                        case ObjectPropertiesEnum.WallDirection.Open_Down:
                            // ���������� Ȯ��
                            for (int j = 1; j < wall.Length; j++)
                            {
                                if (wall.x + j == x && wall.y == y)
                                {
                                    isExtendedWall = true;
                                    break;
                                }
                            }
                            break;

                        case ObjectPropertiesEnum.WallDirection.Single_Left:
                        case ObjectPropertiesEnum.WallDirection.Single_Right:
                        case ObjectPropertiesEnum.WallDirection.Open_Left:
                        case ObjectPropertiesEnum.WallDirection.Open_Right:
                            // �Ʒ������� Ȯ��
                            for (int j = 1; j < wall.Length; j++)
                            {
                                if (wall.x == x && wall.y + j == y)
                                {
                                    isExtendedWall = true;
                                    break;
                                }
                            }
                            break;
                    }

                    if (isExtendedWall)
                    {
                        currentWallIndex = i;
                        return;
                    }
                }
            }

            // ���õ� ���� ������ -1�� ����
            currentWallIndex = -1;
        }
        private WallGimmickType ConvertToWallGimmickType(string gimmickType)
        {
            // ���ڿ� ������� WallGimmickType ��ȯ
            switch (gimmickType)
            {
                case "Star": return WallGimmickType.Star;
                case "Lock": return WallGimmickType.Lock;
                case "Key": return WallGimmickType.Key;
                case "Constraint": return WallGimmickType.Constraint;
                case "Multiple": return WallGimmickType.Multiple;
                case "Frozen": return WallGimmickType.Frozen;
                default: return WallGimmickType.None;
            }
        }
        // ���� ��� �߰��ϴ� �� �޼���
        private void PlaceWallGimmick(int x, int y)
        {
            if (currentWallIndex >= 0 && currentWallIndex < walls.Count)
            {
                var targetWall = walls[currentWallIndex];

                // ���ڿ� ��� ��� Ÿ�� ��� �Ǵ� ��ȯ
                WallGimmickType wallGimmickType = ConvertToWallGimmickType(selectedGimmick);

                // ���õ� ���� ��� ����
                if (targetWall.WallGimmickType == wallGimmickType)
                {
                    // ���� ��� Ÿ���̸� ���� (���)
                    targetWall.WallGimmickType = WallGimmickType.None;
                }
                else
                {
                    // �ٸ� ��� Ÿ���̸� ����
                    targetWall.WallGimmickType = wallGimmickType;
                }
            }
        }
        private void PlaceGimmick(int x, int y)
        {
            // �÷��� ��� ã��
            PlayingBlockData targetBlock = null;
            int blockIndex = -1;

            for (int i = 0; i < playingBlocks.Count; i++)
            {
                var block = playingBlocks[i];

                foreach (var shape in block.shapes)
                {
                    if (block.center.x + shape.offset.x == x && block.center.y + shape.offset.y == y)
                    {
                        targetBlock = block;
                        blockIndex = i;
                        break;
                    }
                }

                if (targetBlock != null) break;
            }

            if (targetBlock != null)
            {
                // ���� ��� Ȯ��
                bool hasGimmick = false;
                int gimmickIndex = -1;

                for (int i = 0; i < targetBlock.gimmicks.Count; i++)
                {
                    var gimmick = targetBlock.gimmicks[i];
                    if (gimmick.gimmickType == selectedGimmick)
                    {
                        hasGimmick = true;
                        gimmickIndex = i;
                        break;
                    }
                }

                if (hasGimmick)
                {
                    // ���� ��� ���� (��� ���)
                    if (gimmickIndex >= 0)
                    {
                        targetBlock.gimmicks.RemoveAt(gimmickIndex);
                    }
                }
                else
                {
                    // ��� �߰�
                    GimmickData gimmick = new GimmickData();
                    gimmick.gimmickType = selectedGimmick;
                    targetBlock.AddGimmick(gimmick);
                }

                // ���� ���õ� ��� ������Ʈ
                currentPlayingBlockIndex = blockIndex;
            }
        }

        private void EraseObject(int x, int y)
        {
            bool objectErased = false;

            // ���� ��� ����
            BoardBlockData boardBlock = boardBlocks.Find(b => b.x == x && b.y == y);
            if (boardBlock != null)
            {
                boardBlocks.Remove(boardBlock);
                objectErased = true;
            }

            // �� ����
            WallData wall = walls.Find(w => w.x == x && w.y == y);
            if (wall != null)
            {
                walls.Remove(wall);
                objectErased = true;
            }

            // �÷��� ��� ���� üũ
            for (int i = 0; i < playingBlocks.Count; i++)
            {
                var block = playingBlocks[i];

                bool shouldRemove = false;

                foreach (var shape in block.shapes)
                {
                    int blockX = block.center.x + shape.offset.x;
                    int blockY = block.center.y + shape.offset.y;

                    if (blockX == x && blockY == y)
                    {
                        shouldRemove = true;
                        break;
                    }
                }

                if (shouldRemove)
                {
                    playingBlocks.RemoveAt(i);

                    if (currentPlayingBlockIndex == i)
                    {
                        currentPlayingBlockIndex = -1;
                    }
                    else if (currentPlayingBlockIndex > i)
                    {
                        currentPlayingBlockIndex--;
                    }

                    objectErased = true;
                    break;
                }
            }

            // ���� ��ü�� ������ �˸�
            if (!objectErased)
            {
                EditorUtility.DisplayDialog("�˸�", "������ ��ġ�� ���� ��ü�� �����ϴ�.", "Ȯ��");
            }
        }

        // ���õ� ��� �̵� �޼���
        private void MoveSelectedBlock(int deltaX, int deltaY)
        {
            if (currentPlayingBlockIndex >= 0 && currentPlayingBlockIndex < playingBlocks.Count)
            {
                var block = playingBlocks[currentPlayingBlockIndex];

                // ��� �߽� �̵�
                block.center.x += deltaX;
                block.center.y += deltaY;

                // �׸��� ���� üũ �� ����
                block.center.x = Mathf.Clamp(block.center.x, 0, gridWidth - 1);
                block.center.y = Mathf.Clamp(block.center.y, 0, gridHeight - 1);
            }
        }

        // ���õ� ��� ȸ�� �޼���
        private void RotateSelectedBlock(bool counterClockwise)
        {
            if (currentPlayingBlockIndex >= 0 && currentPlayingBlockIndex < playingBlocks.Count)
            {
                var block = playingBlocks[currentPlayingBlockIndex];

                // ��� ������ ����
                List<ShapeData> newShapes = new List<ShapeData>();

                foreach (var shape in block.shapes)
                {
                    int newX, newY;

                    if (counterClockwise)
                    {
                        // �ݽð� ȸ�� (90��)
                        newX = -shape.offset.y;
                        newY = shape.offset.x;
                    }
                    else
                    {
                        // �ð� ȸ�� (90��)
                        newX = shape.offset.y;
                        newY = -shape.offset.x;
                    }

                    newShapes.Add(new ShapeData(newX, newY));
                }

                // ȸ���� ������� ������Ʈ
                block.shapes.Clear();
                foreach (var shape in newShapes)
                {
                    block.AddShape(shape);
                }
            }
        }

        #endregion
    }
}