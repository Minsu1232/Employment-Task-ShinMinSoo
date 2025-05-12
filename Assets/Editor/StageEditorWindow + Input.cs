// 5. StageEditorWindow.Input.cs - �Է� ó�� ���� �޼���
using UnityEditor;
using UnityEngine;
using Project.Scripts.Model;
using System.Collections.Generic;

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

                currentMouseGridPos.x = Mathf.FloorToInt(gridPos.x / gridSize);
                currentMouseGridPos.y = Mathf.FloorToInt(gridPos.y / gridSize);

                // �׸��� ���� ����
                currentMouseGridPos.x = Mathf.Clamp(currentMouseGridPos.x, 0, gridWidth - 1);
                currentMouseGridPos.y = Mathf.Clamp(currentMouseGridPos.y, 0, gridHeight - 1);

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
                int gridX = Mathf.FloorToInt(mousePos.x / gridSize);
                int gridY = Mathf.FloorToInt(mousePos.y / gridSize);

                // �׸��� ���� üũ
                if (gridX >= 0 && gridX < gridWidth && gridY >= 0 && gridY < gridHeight)
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
                                    boardDragStart = new Vector2Int(gridX, gridY);
                                    // Ŭ������ ���� ��� ��ġ�� ����
                                    PlaceBoardBlock(gridX, gridY);
                                }
                                else // FreeForm ���
                                {
                                    // ���� ��� ��� - �巡�� ���� �� �� ���� ó��
                                    isDraggingBoardFreeForm = true;
                                    Vector2Int cell = new Vector2Int(gridX, gridY);
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
                                    // �簢�� ��� - �巡�� ����
                                    startDragPosition = new Vector2Int(gridX, gridY);
                                    CheckPlayingBlockSelection(gridX, gridY);
                                }
                                else // FreeForm ���
                                {
                                    // ���� ��� ��� - �巡�� ���� �� �� ���� ó��
                                    isDraggingInFreeFormMode = true;
                                    Vector2Int cell = new Vector2Int(gridX, gridY);
                                    lastDraggedCell = cell;

                                    // ���� ��� ���� üũ
                                    int oldIndex = currentPlayingBlockIndex;
                                    CheckPlayingBlockSelection(gridX, gridY);

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
                                PlaceWall(gridX, gridY);
                                break;

                            case EditTool.Gimmick:
                                PlaceGimmick(gridX, gridY);
                                break;

                            case EditTool.Erase:
                                EraseObject(gridX, gridY);
                                break;
                        }
                        e.Use();
                    }
                }
            }
            else if (e.type == EventType.MouseDrag && e.button == 0)
            {
                // ���� ��� ��忡�� �巡�� ó���� �̹� ��ܿ��� ó����
                // �ʿ��� ��� ���⿡ �߰� ���� �߰�
                e.Use();
            }
            else if (e.type == EventType.MouseUp && e.button == 0)
            {
                Vector2 mousePos = e.mousePosition;
                mousePos -= gridViewRect.position;   // ��ũ�Ѻ� ���� ��ǥ�� ����
                mousePos += scrollPosition;          // ���� ��ũ�� �ݿ�
                int gridX = Mathf.FloorToInt(mousePos.x / gridSize);
                int gridY = Mathf.FloorToInt(mousePos.y / gridSize);

                // �簢�� ��� ���� ��� �巡�� ó��
                if (isDraggingBoardBlock && boardDragStart.HasValue && currentTool == EditTool.BoardBlock &&
                    boardBlockMode == BoardBlockMode.Rectangle)
                {
                    // �׸��� ���� üũ
                    if (gridX >= 0 && gridX < gridWidth && gridY >= 0 && gridY < gridHeight)
                    {
                        // �巡�� ���� ���
                        int startX = Mathf.Min(boardDragStart.Value.x, gridX);
                        int startY = Mathf.Min(boardDragStart.Value.y, gridY);
                        int endX = Mathf.Max(boardDragStart.Value.x, gridX);
                        int endY = Mathf.Max(boardDragStart.Value.y, gridY);

                        // ���� �� ��� ���� ���� ��� ��ġ
                        for (int x = startX; x <= endX; x++)
                        {
                            for (int y = startY; y <= endY; y++)
                            {
                                PlaceBoardBlock(x, y);
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
                    if (gridX >= 0 && gridX < gridWidth && gridY >= 0 && gridY < gridHeight)
                    {
                        // �ּ� �巡�� �Ÿ� Ȯ��
                        bool validDrag = (startDragPosition.Value.x != gridX || startDragPosition.Value.y != gridY);

                        // �巡�װ� ��ȿ�ϰų�, ����� ���õ��� ���� ��쿡�� ��� ����
                        if (validDrag || currentPlayingBlockIndex < 0)
                        {
                            FinishDragAndPlacePlayingBlock(gridX, gridY);
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
            BoardBlockData existingBlock = boardBlocks.Find(b => b.x == x && b.y == y);

            if (existingBlock != null)
            {
                // ���� ��� ����
                existingBlock.ColorType = selectedColor;
            }
            else
            {
                // �� ��� ����
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

            // �巡�� ���� ���
            int startX = Mathf.Min(startDragPosition.Value.x, endX);
            int startY = Mathf.Min(startDragPosition.Value.y, endY);
            int width = Mathf.Abs(endX - startDragPosition.Value.x) + 1;
            int height = Mathf.Abs(endY - startDragPosition.Value.y) + 1;

            // �߽� ��ġ ��� (�»�� ����)
            Vector2Int center = new Vector2Int(startX, startY);

            // �� ��� ����
            PlayingBlockData newBlock = new PlayingBlockData();
            newBlock.center = center;
            newBlock.ColorType = selectedColor;
            newBlock.uniqueIndex = playingBlocks.Count;

            // ��� ��� �߰�
            for (int x = 0; x < width; x++)
            { 
                for (int y = 0; y < height; y++)
                {
                    ShapeData shape = new ShapeData(x, y);
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
            // ���� �� Ȯ��
            WallData existingWall = walls.Find(w =>
                w.x == x && w.y == y && w.WallDirection == wallDirection);

            if (existingWall != null)
            {
                // ���� �� ����
                existingWall.Length = wallLength;
                existingWall.wallColor = wallColor;
            }
            else
            {
                // �� �� ����
                WallData wall = new WallData();
                wall.x = x;
                wall.y = y;
                wall.WallDirection = wallDirection;
                wall.Length = wallLength;
                wall.wallColor = wallColor;

                walls.Add(wall);
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