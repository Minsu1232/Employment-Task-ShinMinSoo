// 5. StageEditorWindow.Input.cs - 입력 처리 관련 메서드
using UnityEditor;
using UnityEngine;
using Project.Scripts.Model;
using System.Collections.Generic;
using static ObjectPropertiesEnum;

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

                // 에디터 그리드 좌표
                int editorGridX = Mathf.FloorToInt(gridPos.x / gridSize);
                int editorGridY = Mathf.FloorToInt(gridPos.y / gridSize);

                // 에디터 좌표를 월드 좌표로 변환하여 저장
                currentMouseGridPos.x = editorGridX;
                currentMouseGridPos.y = EditorToWorldY(editorGridY);

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

                // 에디터 좌표 계산
                int editorGridX = Mathf.FloorToInt(mousePos.x / gridSize);
                int editorGridY = Mathf.FloorToInt(mousePos.y / gridSize);

                // 월드 좌표 계산 (저장용)
                int worldGridX = editorGridX;
                int worldGridY = EditorToWorldY(editorGridY);

                // 그리드 범위 체크
                if (editorGridX >= 0 && editorGridX < gridWidth && editorGridY >= 0 && editorGridY < gridHeight)
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
                                    boardDragStart = new Vector2Int(editorGridX, editorGridY);
                                    // 클릭으로 단일 블록 배치는 유지
                                    PlaceBoardBlock(worldGridX, worldGridY);
                                }
                                else // FreeForm 모드
                                {
                                    // 자유 모양 모드 - 드래그 시작 및 셀 선택 처리
                                    isDraggingBoardFreeForm = true;
                                    Vector2Int cell = new Vector2Int(worldGridX, worldGridY);
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
                                    // 사각형 모드 - 드래그 시작 (에디터 좌표 사용)
                                    startDragPosition = new Vector2Int(editorGridX, editorGridY);
                                    CheckPlayingBlockSelection(worldGridX, worldGridY);
                                }
                                else // FreeForm 모드
                                {
                                    // 자유 모양 모드 - 드래그 시작 및 셀 선택 처리
                                    isDraggingInFreeFormMode = true;
                                    Vector2Int cell = new Vector2Int(worldGridX, worldGridY);
                                    lastDraggedCell = cell;

                                    // 기존 블록 선택 체크
                                    int oldIndex = currentPlayingBlockIndex;
                                    CheckPlayingBlockSelection(worldGridX, worldGridY);

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
                                PlaceWall(worldGridX, worldGridY);
                                break;

                            case EditTool.Gimmick:
                                // 기믹 도구 선택 시, 먼저 벽 선택 여부 확인
                                CheckWallSelection(worldGridX, worldGridY);

                                // 벽이 선택되었으면 기믹 적용
                                if (currentWallIndex >= 0)
                                {
                                    PlaceWallGimmick(worldGridX, worldGridY);
                                }
                                else
                                {
                                    // 기존의 블록 기믹 처리 유지
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
                // 자유 모양 모드에서 드래그 처리는 이미 상단에서 처리됨
                e.Use();
            }
            else if (e.type == EventType.MouseUp && e.button == 0)
            {
                Vector2 mousePos = e.mousePosition;
                mousePos -= gridViewRect.position;   // 스크롤뷰 내부 좌표로 보정
                mousePos += scrollPosition;          // 현재 스크롤 반영

                // 에디터 좌표 계산
                int editorGridX = Mathf.FloorToInt(mousePos.x / gridSize);
                int editorGridY = Mathf.FloorToInt(mousePos.y / gridSize);

                // 월드 좌표 계산 (저장용)
                int worldGridX = editorGridX;
                int worldGridY = EditorToWorldY(editorGridY);

                // 사각형 모드 보드 블록 드래그 처리
                if (isDraggingBoardBlock && boardDragStart.HasValue && currentTool == EditTool.BoardBlock &&
                    boardBlockMode == BoardBlockMode.Rectangle)
                {
                    // 그리드 범위 체크
                    if (editorGridX >= 0 && editorGridX < gridWidth && editorGridY >= 0 && editorGridY < gridHeight)
                    {
                        // 드래그 영역 계산 (에디터 좌표)
                        int startEditorX = Mathf.Min(boardDragStart.Value.x, editorGridX);
                        int startEditorY = Mathf.Min(boardDragStart.Value.y, editorGridY);
                        int endEditorX = Mathf.Max(boardDragStart.Value.x, editorGridX);
                        int endEditorY = Mathf.Max(boardDragStart.Value.y, editorGridY);

                        // 영역 내 모든 셀에 보드 블록 배치 (월드 좌표로 변환하여 저장)
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
                    if (editorGridX >= 0 && editorGridX < gridWidth && editorGridY >= 0 && editorGridY < gridHeight)
                    {
                        // 최소 드래그 거리 확인
                        bool validDrag = (startDragPosition.Value.x != editorGridX || startDragPosition.Value.y != editorGridY);

                        // 드래그가 유효하거나, 블록이 선택되지 않은 경우에만 블록 생성
                        if (validDrag || currentPlayingBlockIndex < 0)
                        {
                            FinishDragAndPlacePlayingBlock(worldGridX, worldGridY);
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
        // HandleFreeFormDrag 메서드 수정
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
            // 기존 코드 그대로 사용
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

            // 드래그 시작점은 에디터 좌표
            Vector2Int dragStart = startDragPosition.Value;

            // endY는 월드 좌표로 가정 (EditorToWorldY 함수로 이미 변환된 상태)
            // 에디터 좌표로 다시 변환
            int endEditorY = WorldToEditorY(endY);

            // 에디터 좌표 기준으로 영역 계산
            int startEditorX = Mathf.Min(dragStart.x, endX);
            int startEditorY = Mathf.Min(dragStart.y, endEditorY);
            int endEditorX = Mathf.Max(dragStart.x, endX);
            int endEditor_Y = Mathf.Max(dragStart.y, endEditorY);

            int width = endEditorX - startEditorX + 1;
            int height = endEditor_Y - startEditorY + 1;

            // 중심 위치 계산 - 드래그 영역의 실제 중심으로 계산
            // 1. 에디터 좌표에서 중심점 계산
            int centerEditorX = startEditorX + (width - 1) / 2;
            int centerEditorY = startEditorY + (height - 1) / 2;

            // 2. 에디터 좌표를 월드 좌표로 변환
            int centerWorldX = centerEditorX;
            int centerWorldY = EditorToWorldY(centerEditorY);

            // 새 블록 생성
            PlayingBlockData newBlock = new PlayingBlockData();
            newBlock.center = new Vector2Int(centerWorldX, centerWorldY);  // 월드 좌표로 중심 설정
            newBlock.ColorType = selectedColor;
            newBlock.uniqueIndex = playingBlocks.Count;

            // 블록 모양 추가 - 중심점 기준 오프셋 계산
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // 에디터 좌표에서 오프셋 계산
                    int offsetEditorX = (startEditorX + x) - centerEditorX;
                    int offsetEditorY = (startEditorY + y) - centerEditorY;

                    // Y 좌표만 변환 필요 (X는 동일)
                    int offsetWorldX = offsetEditorX;
                    int offsetWorldY = -offsetEditorY; // Y축 방향이 반대라서 부호 반전

                    ShapeData shape = new ShapeData(offsetWorldX, offsetWorldY);
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
        // CreateFreeFormPlayingBlock 메서드 수정
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
            // 이미 선택된 벽이 있으면 처리하지 않음 (기믹 도구를 사용중일 수 있음)
            if (currentTool == EditTool.Gimmick && currentWallIndex >= 0)
                return;

            // 기존 벽 확인
            WallData existingWall = walls.Find(w =>
                w.x == x && w.y == y && w.WallDirection == wallDirection);

            if (existingWall != null)
            {
                // 기존 벽이 있을 경우 선택 상태로 변경
                currentWallIndex = walls.IndexOf(existingWall);
            }
            else
            {
                // 새 벽 생성 - 기본적으로는 기믹 없이 생성
                WallData wall = new WallData(x, y, wallDirection, wallLength, wallColor, WallGimmickType.None);
                walls.Add(wall);
                currentWallIndex = walls.Count - 1;
            }
        }
        // 벽 선택 메서드 추가 (CheckWallSelection)
        private void CheckWallSelection(int x, int y)
        {
            // 기존 벽들 중에서 선택된 위치에 있는 벽 찾기
            for (int i = 0; i < walls.Count; i++)
            {
                var wall = walls[i];
                if (wall.x == x && wall.y == y)
                {
                    currentWallIndex = i;
                    return;
                }

                // 확장된 벽 영역도 검사
                if (wall.Length > 1)
                {
                    bool isExtendedWall = false;

                    // 벽 방향에 따라 확장 영역 계산
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
                            // 오른쪽으로 확장
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
                            // 아래쪽으로 확장
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

            // 선택된 벽이 없으면 -1로 설정
            currentWallIndex = -1;
        }
        private WallGimmickType ConvertToWallGimmickType(string gimmickType)
        {
            // 문자열 기반으로 WallGimmickType 변환
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
        // 벽에 기믹 추가하는 새 메서드
        private void PlaceWallGimmick(int x, int y)
        {
            if (currentWallIndex >= 0 && currentWallIndex < walls.Count)
            {
                var targetWall = walls[currentWallIndex];

                // 문자열 기반 기믹 타입 사용 또는 변환
                WallGimmickType wallGimmickType = ConvertToWallGimmickType(selectedGimmick);

                // 선택된 벽에 기믹 적용
                if (targetWall.WallGimmickType == wallGimmickType)
                {
                    // 같은 기믹 타입이면 제거 (토글)
                    targetWall.WallGimmickType = WallGimmickType.None;
                }
                else
                {
                    // 다른 기믹 타입이면 변경
                    targetWall.WallGimmickType = wallGimmickType;
                }
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