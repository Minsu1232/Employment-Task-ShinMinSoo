// 5. StageEditorWindow.Input.cs - 입력 처리 관련 메서드
using UnityEditor;
using UnityEngine;
using Project.Scripts.Model;
using System.Collections.Generic;

namespace Project.Scripts.Editor
{
    public partial class StageEditorWindow
    {
        #region 입력 처리


        private void HandleInput()
        {
            Event e = Event.current;

            // 현재 마우스 위치를 그리드 좌표로 업데이트 (모든 이벤트에서)
            if (gridViewRect.Contains(e.mousePosition))
            {
                Vector2 gridPos = e.mousePosition;
                gridPos -= gridViewRect.position;  // 그리드 뷰 위치 조정
                gridPos += scrollPosition;         // 스크롤 위치 반영

                currentMouseGridPos.x = Mathf.FloorToInt(gridPos.x / gridSize);
                currentMouseGridPos.y = Mathf.FloorToInt(gridPos.y / gridSize);

                // 그리드 범위 제한
                currentMouseGridPos.x = Mathf.Clamp(currentMouseGridPos.x, 0, gridWidth - 1);
                currentMouseGridPos.y = Mathf.Clamp(currentMouseGridPos.y, 0, gridHeight - 1);

                // 자유 모양 모드에서 드래그 중이면 셀 선택 처리 - 플레이 블록
                if (isDraggingInFreeFormMode && currentTool == EditTool.PlayingBlock &&
                    playingBlockMode == PlayingBlockMode.FreeForm)
                {
                    HandleFreeFormDrag(currentMouseGridPos);
                }

                // 자유 모양 모드에서 드래그 중이면 셀 선택 처리 - 보드 블록
                if (isDraggingBoardFreeForm && currentTool == EditTool.BoardBlock &&
                    boardBlockMode == BoardBlockMode.FreeForm)
                {
                    HandleBoardFreeFormDrag(currentMouseGridPos);
                }

                // 마우스가 움직일 때마다 그리드를 갱신 (드래그 시각화를 위해)
                if (e.type == EventType.MouseMove || e.type == EventType.MouseDrag)
                {
                    Repaint();
                }
            }

            // 키보드 단축키 처리
            if (e.type == EventType.KeyDown)
            {
                // 숫자 키로 도구 직접 선택 (1-5)
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
                mousePos -= gridViewRect.position;   // 스크롤뷰 내부 좌표로 보정
                mousePos += scrollPosition;          // 현재 스크롤 반영
                int gridX = Mathf.FloorToInt(mousePos.x / gridSize);
                int gridY = Mathf.FloorToInt(mousePos.y / gridSize);

                // 그리드 범위 체크
                if (gridX >= 0 && gridX < gridWidth && gridY >= 0 && gridY < gridHeight)
                {
                    // 왼쪽 마우스 버튼 클릭
                    if (e.button == 0)
                    {
                        // 도구에 따라 다른 액션 수행
                        switch (currentTool)
                        {
                            case EditTool.BoardBlock:
                                if (boardBlockMode == BoardBlockMode.Rectangle)
                                {
                                    // 사각형 모드 - 드래그 시작
                                    isDraggingBoardBlock = true;
                                    boardDragStart = new Vector2Int(gridX, gridY);
                                    // 클릭으로 단일 블록 배치는 유지
                                    PlaceBoardBlock(gridX, gridY);
                                }
                                else // FreeForm 모드
                                {
                                    // 자유 모양 모드 - 드래그 시작 및 셀 선택 처리
                                    isDraggingBoardFreeForm = true;
                                    Vector2Int cell = new Vector2Int(gridX, gridY);
                                    lastDraggedBoardCell = cell;

                                    // Shift 키를 누른 상태면 추가/제거
                                    if (e.shift)
                                    {
                                        if (selectedBoardCells.Contains(cell))
                                            selectedBoardCells.Remove(cell);
                                        else
                                            selectedBoardCells.Add(cell);
                                    }
                                    else // 일반 클릭이면 이전 선택 초기화하고 새로 선택
                                    {
                                        selectedBoardCells.Clear();
                                        selectedBoardCells.Add(cell);
                                    }
                                }
                                break;

                            case EditTool.PlayingBlock:
                                if (playingBlockMode == PlayingBlockMode.Rectangle)
                                {
                                    // 사각형 모드 - 드래그 시작
                                    startDragPosition = new Vector2Int(gridX, gridY);
                                    CheckPlayingBlockSelection(gridX, gridY);
                                }
                                else // FreeForm 모드
                                {
                                    // 자유 모양 모드 - 드래그 시작 및 셀 선택 처리
                                    isDraggingInFreeFormMode = true;
                                    Vector2Int cell = new Vector2Int(gridX, gridY);
                                    lastDraggedCell = cell;

                                    // 기존 블록 선택 체크
                                    int oldIndex = currentPlayingBlockIndex;
                                    CheckPlayingBlockSelection(gridX, gridY);

                                    // 블록이 선택되지 않은 경우만 셀 선택 처리
                                    if (currentPlayingBlockIndex < 0)
                                    {
                                        // Shift 키를 누른 상태면 추가/제거
                                        if (e.shift)
                                        {
                                            if (selectedCells.Contains(cell))
                                                selectedCells.Remove(cell);
                                            else
                                                selectedCells.Add(cell);
                                        }
                                        else // 일반 클릭이면 이전 선택 초기화하고 새로 선택
                                        {
                                            if (oldIndex < 0) // 이전에도 블록이 선택되지 않았을 때만
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
                // 자유 모양 모드에서 드래그 처리는 이미 상단에서 처리됨
                // 필요한 경우 여기에 추가 로직 추가
                e.Use();
            }
            else if (e.type == EventType.MouseUp && e.button == 0)
            {
                Vector2 mousePos = e.mousePosition;
                mousePos -= gridViewRect.position;   // 스크롤뷰 내부 좌표로 보정
                mousePos += scrollPosition;          // 현재 스크롤 반영
                int gridX = Mathf.FloorToInt(mousePos.x / gridSize);
                int gridY = Mathf.FloorToInt(mousePos.y / gridSize);

                // 사각형 모드 보드 블록 드래그 처리
                if (isDraggingBoardBlock && boardDragStart.HasValue && currentTool == EditTool.BoardBlock &&
                    boardBlockMode == BoardBlockMode.Rectangle)
                {
                    // 그리드 범위 체크
                    if (gridX >= 0 && gridX < gridWidth && gridY >= 0 && gridY < gridHeight)
                    {
                        // 드래그 영역 계산
                        int startX = Mathf.Min(boardDragStart.Value.x, gridX);
                        int startY = Mathf.Min(boardDragStart.Value.y, gridY);
                        int endX = Mathf.Max(boardDragStart.Value.x, gridX);
                        int endY = Mathf.Max(boardDragStart.Value.y, gridY);

                        // 영역 내 모든 셀에 보드 블록 배치
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

                // 보드 블록 자유 모양 모드 드래그 종료
                if (isDraggingBoardFreeForm && currentTool == EditTool.BoardBlock &&
                    boardBlockMode == BoardBlockMode.FreeForm)
                {
                    isDraggingBoardFreeForm = false;
                    lastDraggedBoardCell = new Vector2Int(-1, -1);
                }

                // 사각형 모드일 때 플레이 블록 생성 처리
                if (startDragPosition.HasValue && currentTool == EditTool.PlayingBlock &&
                    playingBlockMode == PlayingBlockMode.Rectangle)
                {
                    // 그리드 범위 체크
                    if (gridX >= 0 && gridX < gridWidth && gridY >= 0 && gridY < gridHeight)
                    {
                        // 최소 드래그 거리 확인
                        bool validDrag = (startDragPosition.Value.x != gridX || startDragPosition.Value.y != gridY);

                        // 드래그가 유효하거나, 블록이 선택되지 않은 경우에만 블록 생성
                        if (validDrag || currentPlayingBlockIndex < 0)
                        {
                            FinishDragAndPlacePlayingBlock(gridX, gridY);
                        }
                    }

                    startDragPosition = null;
                }

                // 자유 모양 모드 드래그 종료
                if (isDraggingInFreeFormMode && currentTool == EditTool.PlayingBlock &&
                    playingBlockMode == PlayingBlockMode.FreeForm)
                {
                    isDraggingInFreeFormMode = false;
                    lastDraggedCell = new Vector2Int(-1, -1);
                }

                e.Use();
            }
        }
        // 보드 블록 자유 모양 모드 드래그 처리 메서드
        private void HandleBoardFreeFormDrag(Vector2Int currentCell)
        {
            // 마지막으로 드래그된 셀과 동일하면 무시
            if (currentCell == lastDraggedBoardCell)
                return;

            // 현재 셀이 이미 선택되어 있지 않으면 선택
            if (!selectedBoardCells.Contains(currentCell))
            {
                selectedBoardCells.Add(currentCell);
            }

            // 마지막으로 드래그된 셀 업데이트
            lastDraggedBoardCell = currentCell;
        }
        // 보드 블록 자유 모양 생성 메서드
        private void CreateFreeFormBoardBlocks()
        {
            if (selectedBoardCells.Count == 0)
                return;

            // 선택된 모든 셀에 보드 블록 배치
            foreach (var cell in selectedBoardCells)
            {
                PlaceBoardBlock(cell.x, cell.y);
            }

            // 선택된 셀 초기화
            selectedBoardCells.Clear();
        }
        // 자유 모양 모드에서 드래그로 셀 선택 처리
        private void HandleFreeFormDrag(Vector2Int currentCell)
        {
            // 마지막으로 드래그된 셀과 동일하면 무시
            if (currentCell == lastDraggedCell)
                return;

            // 블록이 이미 선택된 상태면 무시
            if (currentPlayingBlockIndex >= 0)
                return;

            // 현재 셀이 이미 선택되어 있지 않으면 선택
            if (!selectedCells.Contains(currentCell))
            {
                selectedCells.Add(currentCell);
            }

            // 마지막으로 드래그된 셀 업데이트
            lastDraggedCell = currentCell;
        }

        private void PlaceBoardBlock(int x, int y)
        {
            // 기존 블록 확인
            BoardBlockData existingBlock = boardBlocks.Find(b => b.x == x && b.y == y);

            if (existingBlock != null)
            {
                // 기존 블록 수정
                existingBlock.ColorType = selectedColor;
            }
            else
            {
                // 새 블록 생성
                BoardBlockData newBlock = new BoardBlockData(x, y);
                newBlock.ColorType = selectedColor;
                boardBlocks.Add(newBlock);
            }
        }

        private void CheckPlayingBlockSelection(int x, int y)
        {
            // 플레이 블록 선택 체크
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

            // 선택된 블록이 없으면 새 블록 생성 모드로 전환
            currentPlayingBlockIndex = -1;
        }

        // 블록 선택 여부만 확인하는 별도 함수 추가
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
            // 이미 블록이 선택되어 있으면 새 블록을 생성하지 않음
            if (currentPlayingBlockIndex >= 0)
            {
                return;
            }

            // 드래그 영역 계산
            int startX = Mathf.Min(startDragPosition.Value.x, endX);
            int startY = Mathf.Min(startDragPosition.Value.y, endY);
            int width = Mathf.Abs(endX - startDragPosition.Value.x) + 1;
            int height = Mathf.Abs(endY - startDragPosition.Value.y) + 1;

            // 중심 위치 계산 (좌상단 기준)
            Vector2Int center = new Vector2Int(startX, startY);

            // 새 블록 생성
            PlayingBlockData newBlock = new PlayingBlockData();
            newBlock.center = center;
            newBlock.ColorType = selectedColor;
            newBlock.uniqueIndex = playingBlocks.Count;

            // 블록 모양 추가
            for (int x = 0; x < width; x++)
            { 
                for (int y = 0; y < height; y++)
                {
                    ShapeData shape = new ShapeData(x, y);
                    newBlock.AddShape(shape);
                }
            }

            // 기본 기믹 추가
            GimmickData gimmick = new GimmickData();
            gimmick.gimmickType = "None";
            newBlock.AddGimmick(gimmick);

            playingBlocks.Add(newBlock);
            currentPlayingBlockIndex = playingBlocks.Count - 1;
        }

        // 자유 모양 블록 생성 메서드
        private void CreateFreeFormPlayingBlock()
        {
            if (selectedCells.Count == 0)
                return;

            // 중심점 결정 (첫 번째 선택한 셀)
            Vector2Int center = selectedCells[0];

            // 새 블록 생성
            PlayingBlockData newBlock = new PlayingBlockData();
            newBlock.center = center;
            newBlock.ColorType = selectedColor;
            newBlock.uniqueIndex = playingBlocks.Count;

            // 선택된 모든 셀을 중심점 기준 오프셋으로 변환
            foreach (var cell in selectedCells)
            {
                ShapeData shape = new ShapeData(
                    cell.x - center.x,  // x 오프셋
                    cell.y - center.y   // y 오프셋
                );
                newBlock.AddShape(shape);
            }

            // 기본 기믹 추가
            GimmickData gimmick = new GimmickData();
            gimmick.gimmickType = "None";
            newBlock.AddGimmick(gimmick);

            playingBlocks.Add(newBlock);
            currentPlayingBlockIndex = playingBlocks.Count - 1;

            // 선택된 셀 초기화
            selectedCells.Clear();
        }

        private void PlaceWall(int x, int y)
        {
            // 기존 벽 확인
            WallData existingWall = walls.Find(w =>
                w.x == x && w.y == y && w.WallDirection == wallDirection);

            if (existingWall != null)
            {
                // 기존 벽 수정
                existingWall.Length = wallLength;
                existingWall.wallColor = wallColor;
            }
            else
            {
                // 새 벽 생성
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
            // 플레이 블록 찾기
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
                // 기존 기믹 확인
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
                    // 기존 기믹 제거 (토글 기능)
                    if (gimmickIndex >= 0)
                    {
                        targetBlock.gimmicks.RemoveAt(gimmickIndex);
                    }
                }
                else
                {
                    // 기믹 추가
                    GimmickData gimmick = new GimmickData();
                    gimmick.gimmickType = selectedGimmick;
                    targetBlock.AddGimmick(gimmick);
                }

                // 현재 선택된 블록 업데이트
                currentPlayingBlockIndex = blockIndex;
            }
        }

        private void EraseObject(int x, int y)
        {
            bool objectErased = false;

            // 보드 블록 삭제
            BoardBlockData boardBlock = boardBlocks.Find(b => b.x == x && b.y == y);
            if (boardBlock != null)
            {
                boardBlocks.Remove(boardBlock);
                objectErased = true;
            }

            // 벽 삭제
            WallData wall = walls.Find(w => w.x == x && w.y == y);
            if (wall != null)
            {
                walls.Remove(wall);
                objectErased = true;
            }

            // 플레이 블록 삭제 체크
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

            // 지운 객체가 없으면 알림
            if (!objectErased)
            {
                EditorUtility.DisplayDialog("알림", "선택한 위치에 지울 객체가 없습니다.", "확인");
            }
        }

        // 선택된 블록 이동 메서드
        private void MoveSelectedBlock(int deltaX, int deltaY)
        {
            if (currentPlayingBlockIndex >= 0 && currentPlayingBlockIndex < playingBlocks.Count)
            {
                var block = playingBlocks[currentPlayingBlockIndex];

                // 블록 중심 이동
                block.center.x += deltaX;
                block.center.y += deltaY;

                // 그리드 범위 체크 및 제한
                block.center.x = Mathf.Clamp(block.center.x, 0, gridWidth - 1);
                block.center.y = Mathf.Clamp(block.center.y, 0, gridHeight - 1);
            }
        }

        // 선택된 블록 회전 메서드
        private void RotateSelectedBlock(bool counterClockwise)
        {
            if (currentPlayingBlockIndex >= 0 && currentPlayingBlockIndex < playingBlocks.Count)
            {
                var block = playingBlocks[currentPlayingBlockIndex];

                // 모양 데이터 복사
                List<ShapeData> newShapes = new List<ShapeData>();

                foreach (var shape in block.shapes)
                {
                    int newX, newY;

                    if (counterClockwise)
                    {
                        // 반시계 회전 (90도)
                        newX = -shape.offset.y;
                        newY = shape.offset.x;
                    }
                    else
                    {
                        // 시계 회전 (90도)
                        newX = shape.offset.y;
                        newY = -shape.offset.x;
                    }

                    newShapes.Add(new ShapeData(newX, newY));
                }

                // 회전된 모양으로 업데이트
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