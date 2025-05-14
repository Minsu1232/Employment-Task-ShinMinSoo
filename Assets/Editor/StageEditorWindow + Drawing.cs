// 3. StageEditorWindow.Drawing.cs - UI 그리기 관련 메서드
using UnityEditor;
using UnityEngine;
using Project.Scripts.Model;
using System.Collections.Generic;
using Project.Scripts.Controller;
using static ObjectPropertiesEnum;

namespace Project.Scripts.Editor
{
    public partial class StageEditorWindow
    {
        #region 그리기 메서드

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            // 파일 메뉴
            if (GUILayout.Button("새 스테이지", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                if (EditorUtility.DisplayDialog("새 스테이지", "새 스테이지를 만드시겠습니까? 저장되지 않은 변경사항은 사라집니다.", "확인", "취소"))
                {
                    CreateNewStage();
                }
            }

            if (GUILayout.Button("불러오기", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                LoadStage();
            }

            if (GUILayout.Button("저장", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                SaveStage();
            }

            // JSON 변환 버튼
            if (GUILayout.Button("JSON 변환", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                showJsonPanel = !showJsonPanel;
            }

            GUILayout.FlexibleSpace();

            // 도구 선택 (단축키 힌트 추가)
            string[] toolLabels = new string[] { "1: 보드 블록", "2: 플레이 블록", "3: 벽", "4: 기믹", "5: 지우기" };

            // 툴바 스타일 생성 및 조정
            GUIStyle toolbarStyle = new GUIStyle(EditorStyles.toolbarButton);
            

            // 배경색 저장
            Color origBgColor = GUI.backgroundColor;

            // 커스텀 툴바 그리기
            int newToolIndex = GUILayout.Toolbar(
                (int)currentTool,
                toolLabels,
                toolbarStyle,
                GUILayout.Width(450)
            );

            // 도구 변경 감지
            if (newToolIndex != (int)currentTool)
            {
                currentTool = (EditTool)newToolIndex;
                GUI.FocusControl(null); // 포커스 초기화
            }

            // 배경색 복원
            GUI.backgroundColor = origBgColor;

            GUILayout.FlexibleSpace();

            // 그리드 크기 조정
            EditorGUILayout.LabelField("그리드 크기:", GUILayout.Width(60));

            if (GUILayout.Button("+", EditorStyles.toolbarButton, GUILayout.Width(20)))
            {
                gridSize = Mathf.Min(gridSize + 5, 80);
            }

            if (GUILayout.Button("-", EditorStyles.toolbarButton, GUILayout.Width(20)))
            {
                gridSize = Mathf.Max(gridSize - 5, 30);
            }

            // 미리보기 모드
            GUI.changed = false;
            bool newPreviewMode = GUILayout.Toggle(previewMode, "미리보기", EditorStyles.toolbarButton, GUILayout.Width(80));
            if (newPreviewMode != previewMode)
            {
                previewMode = newPreviewMode;
                if (previewMode)
                {
                    StartPreview();
                }
                else
                {
                    StopPreview();
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawGrid()
        {
            gridViewRect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            // 그리드 배경
            GUI.DrawTexture(gridViewRect, gridTexture);

            // 스크롤 영역 시작
            scrollPosition = GUI.BeginScrollView(gridViewRect, scrollPosition,
                new Rect(0, 0, gridWidth * gridSize, gridHeight * gridSize));

            // 그리드 셀 그리기
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    Rect cellRect = new Rect(x * gridSize, y * gridSize, gridSize - 1, gridSize - 1);
                    GUI.DrawTexture(cellRect, cellTexture);

                   

                    // 보드 블록 그리기
                    DrawBoardBlocksOnGrid(x, y, cellRect);

                    // 플레이 블록 그리기
                    DrawPlayingBlocksOnGrid(x, y, cellRect);
                    // 벽 그리기
                    DrawwallsOnGrid(x, y, cellRect);
                }
            }

            // 마우스 호버 및 선택 셀 효과
            DrawHoverAndSelectionEffects();

            // 보드 블록 사각형 드래그 영역 표시
            if (boardDragStart.HasValue && currentTool == EditTool.BoardBlock &&
     boardBlockMode == BoardBlockMode.Rectangle && isDraggingBoardBlock)
            {
                // 드래그 시작점과 현재 마우스 위치 (에디터 좌표)
                Vector2Int dragStart = boardDragStart.Value;

                // currentMouseGridPos는 월드 좌표일 수 있으므로 에디터 좌표로 변환
                int currentEditorMouseY = WorldToEditorY(currentMouseGridPos.y);
                Vector2Int currentEditorMouse = new Vector2Int(currentMouseGridPos.x, currentEditorMouseY);

                // 드래그 영역 계산 (에디터 좌표)
                int startX = Mathf.Min(dragStart.x, currentEditorMouse.x);
                int startY = Mathf.Min(dragStart.y, currentEditorMouse.y);
                int endX = Mathf.Max(dragStart.x, currentEditorMouse.x);
                int endY = Mathf.Max(dragStart.y, currentEditorMouse.y);

                // 시각적 효과 (모든 셀 및 테두리 그리기)
                for (int x = startX; x <= endX; x++)
                {
                    for (int y = startY; y <= endY; y++)
                    {
                        if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
                        {
                            Rect cellRect = new Rect(x * gridSize, y * gridSize, gridSize - 1, gridSize - 1);

                            // 배경 색상 (선택된 색상 반투명)
                            Color prevColor = GUI.color;
                            Color selectedBlockColor = GetColorFromType(selectedColor);
                            GUI.color = new Color(selectedBlockColor.r, selectedBlockColor.g, selectedBlockColor.b, 0.3f);
                            GUI.DrawTexture(cellRect, EditorGUIUtility.whiteTexture);

                            // 테두리 (더 진한 색상)
                            if (x == startX || x == endX || y == startY || y == endY)
                            {
                                GUI.color = new Color(selectedBlockColor.r, selectedBlockColor.g, selectedBlockColor.b, 0.8f);
                                float borderWidth = 2f;

                                // 상단 테두리
                                if (y == startY)
                                    GUI.DrawTexture(new Rect(cellRect.x, cellRect.y, cellRect.width, borderWidth), EditorGUIUtility.whiteTexture);

                                // 왼쪽 테두리
                                if (x == startX)
                                    GUI.DrawTexture(new Rect(cellRect.x, cellRect.y, borderWidth, cellRect.height), EditorGUIUtility.whiteTexture);

                                // 하단 테두리
                                if (y == endY)
                                    GUI.DrawTexture(new Rect(cellRect.x, cellRect.y + cellRect.height - borderWidth, cellRect.width, borderWidth), EditorGUIUtility.whiteTexture);

                                // 오른쪽 테두리
                                if (x == endX)
                                    GUI.DrawTexture(new Rect(cellRect.x + cellRect.width - borderWidth, cellRect.y, borderWidth, cellRect.height), EditorGUIUtility.whiteTexture);
                            }

                            GUI.color = prevColor;
                        }
                    }
                }
            }

            // 보드 블록 자유 모양 모드에서 선택된 셀 표시
            if (currentTool == EditTool.BoardBlock && boardBlockMode == BoardBlockMode.FreeForm)
            {
                foreach (var cell in selectedBoardCells)
                {
                    // 여기서 cell 좌표는 월드 좌표일 수 있음
                    // 에디터 좌표로 변환하여 표시
                    int editorX = cell.x;
                    int editorY = WorldToEditorY(cell.y);
                    if (editorX >= 0 && editorX < gridWidth && editorY >= 0 && editorY < gridHeight)
                    {
                        Rect cellRect = new Rect(
                            editorX * gridSize,
                            editorY * gridSize,
                            gridSize - 1,
                            gridSize - 1
                        );

                        // 선택된 셀 표시
                        Color prevColor = GUI.color;

                        // 선택된 색상 기반 표시
                        Color selectedBlockColor = GetColorFromType(selectedColor);

                        // 배경 (반투명)
                        GUI.color = new Color(selectedBlockColor.r, selectedBlockColor.g, selectedBlockColor.b, 0.3f);
                        GUI.DrawTexture(cellRect, EditorGUIUtility.whiteTexture);

                        // 테두리 (더 진한 색상)
                        GUI.color = new Color(selectedBlockColor.r, selectedBlockColor.g, selectedBlockColor.b, 0.8f);
                        float borderWidth = 2f;
                        GUI.DrawTexture(new Rect(cellRect.x, cellRect.y, cellRect.width, borderWidth), EditorGUIUtility.whiteTexture);
                        GUI.DrawTexture(new Rect(cellRect.x, cellRect.y, borderWidth, cellRect.height), EditorGUIUtility.whiteTexture);
                        GUI.DrawTexture(new Rect(cellRect.x, cellRect.y + cellRect.height - borderWidth, cellRect.width, borderWidth), EditorGUIUtility.whiteTexture);
                        GUI.DrawTexture(new Rect(cellRect.x + cellRect.width - borderWidth, cellRect.y, borderWidth, cellRect.height), EditorGUIUtility.whiteTexture);

                        GUI.color = prevColor;
                    }
                }
            }

            // 현재 드래그 영역 표시 - 플레이 블록 사각형 모드일 때
            if (startDragPosition.HasValue && currentTool == EditTool.PlayingBlock && playingBlockMode == PlayingBlockMode.Rectangle)
            {
                // 드래그 시작점(에디터 좌표)
                Vector2Int dragStart = startDragPosition.Value;

                // 현재 마우스 위치(에디터 좌표로 변환)
                int currentEditorMouseY = WorldToEditorY(currentMouseGridPos.y);
                Vector2Int currentEditorMouse = new Vector2Int(currentMouseGridPos.x, currentEditorMouseY);

                // 드래그 영역 계산
                int startX = Mathf.Min(dragStart.x, currentEditorMouse.x);
                int startY = Mathf.Min(dragStart.y, currentEditorMouse.y);
                int endX = Mathf.Max(dragStart.x, currentEditorMouse.x);
                int endY = Mathf.Max(dragStart.y, currentEditorMouse.y);

                // 시각적 효과 (모든 셀 및 테두리 그리기)
                for (int x = startX; x <= endX; x++)
                {
                    for (int y = startY; y <= endY; y++)
                    {
                        if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
                        {
                            Rect cellRect = new Rect(x * gridSize, y * gridSize, gridSize - 1, gridSize - 1);

                            // 배경 색상 (파란색 반투명)
                            Color prevColor = GUI.color;
                            GUI.color = new Color(0.3f, 0.6f, 1.0f, 0.25f);
                            GUI.DrawTexture(cellRect, EditorGUIUtility.whiteTexture);

                            // 테두리 (더 진한 파란색)
                            if (x == startX || x == endX || y == startY || y == endY)
                            {
                                GUI.color = new Color(0.3f, 0.6f, 1.0f, 0.8f);
                                float borderWidth = 2f;

                                // 상단 테두리
                                if (y == startY)
                                    GUI.DrawTexture(new Rect(cellRect.x, cellRect.y, cellRect.width, borderWidth), EditorGUIUtility.whiteTexture);

                                // 왼쪽 테두리
                                if (x == startX)
                                    GUI.DrawTexture(new Rect(cellRect.x, cellRect.y, borderWidth, cellRect.height), EditorGUIUtility.whiteTexture);

                                // 하단 테두리
                                if (y == endY)
                                    GUI.DrawTexture(new Rect(cellRect.x, cellRect.y + cellRect.height - borderWidth, cellRect.width, borderWidth), EditorGUIUtility.whiteTexture);

                                // 오른쪽 테두리
                                if (x == endX)
                                    GUI.DrawTexture(new Rect(cellRect.x + cellRect.width - borderWidth, cellRect.y, borderWidth, cellRect.height), EditorGUIUtility.whiteTexture);
                            }

                            GUI.color = prevColor;
                        }
                    }
                }
            }

            // 플레이 블록 자유 모양 모드에서 선택된 셀 표시
            if (currentTool == EditTool.PlayingBlock && playingBlockMode == PlayingBlockMode.FreeForm)
            {
                foreach (var cell in selectedCells)
                {
                    // 월드 좌표를 에디터 좌표로 변환
                    int editorX = cell.x;
                    int editorY = WorldToEditorY(cell.y);

                    if (editorX >= 0 && editorX < gridWidth && editorY >= 0 && editorY < gridHeight)
                    {
                        Rect cellRect = new Rect(
                            editorX * gridSize,
                            editorY * gridSize,
                            gridSize - 1,
                            gridSize - 1
                        );

                        // 선택된 셀 표시
                        Color prevColor = GUI.color;

                        // 배경 (반투명 녹색)
                        GUI.color = new Color(0.3f, 0.8f, 0.3f, 0.3f);
                        GUI.DrawTexture(cellRect, EditorGUIUtility.whiteTexture);

                        // 테두리 (더 진한 녹색)
                        GUI.color = new Color(0.3f, 0.8f, 0.3f, 0.8f);
                        float borderWidth = 2f;
                        GUI.DrawTexture(new Rect(cellRect.x, cellRect.y, cellRect.width, borderWidth), EditorGUIUtility.whiteTexture);
                        GUI.DrawTexture(new Rect(cellRect.x, cellRect.y, borderWidth, cellRect.height), EditorGUIUtility.whiteTexture);
                        GUI.DrawTexture(new Rect(cellRect.x, cellRect.y + cellRect.height - borderWidth, cellRect.width, borderWidth), EditorGUIUtility.whiteTexture);
                        GUI.DrawTexture(new Rect(cellRect.x + cellRect.width - borderWidth, cellRect.y, borderWidth, cellRect.height), EditorGUIUtility.whiteTexture);

                        // 첫 번째 선택한 셀(중심)은 특별한 표시 추가
                        if (selectedCells.Count > 0 && cell == selectedCells[0])
                        {
                            // 중심점 표시 (작은 사각형)
                            GUI.color = new Color(0.3f, 0.8f, 0.3f, 0.9f);
                            float centerSize = cellRect.width * 0.3f;
                            Rect centerRect = new Rect(
                                cellRect.x + (cellRect.width - centerSize) / 2,
                                cellRect.y + (cellRect.height - centerSize) / 2,
                                centerSize,
                                centerSize
                            );
                            GUI.DrawTexture(centerRect, EditorGUIUtility.whiteTexture);
                        }

                        GUI.color = prevColor;
                    }
                }
            }

            GUI.EndScrollView();
        }

        private void DrawBoardBlocksOnGrid(int x, int y, Rect cellRect)
        {
            foreach (var block in boardBlocks)
            {
                // 여기서 block.y를 에디터 좌표로 변환해서 비교
                if (block.x == x && WorldToEditorY(block.y) == y)
                {
                    Color guiColor = GUI.color;
                    GUI.color = GetColorFromType(block.ColorType);
                    GUI.DrawTexture(cellRect, colorTextures[(int)block.ColorType]);
                    GUI.color = guiColor;

                    // 좌표 표시
                    GUI.Label(new Rect(cellRect.x + 2, cellRect.y + 2, 30, 20),
                        $"{block.x},{block.y}", new GUIStyle() { normal = { textColor = Color.white } });
                }
            }
        }
        private void DrawPlayingBlocksOnGrid(int x, int y, Rect cellRect)
        {
            for (int blockIndex = 0; blockIndex < playingBlocks.Count; blockIndex++)
            {
                var block = playingBlocks[blockIndex];
                foreach (var shape in block.shapes)
                {
                    int blockX = block.center.x + shape.offset.x;
                    int blockY = block.center.y + shape.offset.y;

                    // 월드 좌표를 에디터 좌표로 변환하여 비교
                    int editorBlockY = WorldToEditorY(blockY);

                    if (blockX == x && editorBlockY == y)
                    {
                        // 기존 그리기 코드...
                        Color guiColor = GUI.color;

                        // 플레이 블록 색상 그리기
                        GUI.color = GetColorFromType(block.ColorType);
                        GUI.DrawTexture(cellRect, colorTextures[(int)block.ColorType]);

                        // 테두리 추가
                        DrawPlayingBlockBorder(cellRect);

                        // 현재 선택된 블록이면 하이라이트
                        if (blockIndex == currentPlayingBlockIndex)
                        {
                            GUI.color = new Color(1f, 1f, 1f, 0.3f);
                            GUI.DrawTextureWithTexCoords(cellRect, playingBlockPatternTexture,
                                new Rect(0, 0, cellRect.width / 4, cellRect.height / 4));
                        }

                        // 블록 중심 표시
                        if (shape.offset.x == 0 && shape.offset.y == 0)
                        {
                            GUI.DrawTexture(new Rect(cellRect.x + cellRect.width * 0.25f, cellRect.y + cellRect.height * 0.25f,
                                cellRect.width * 0.5f, cellRect.height * 0.5f),
                                EditorGUIUtility.whiteTexture);

                            // 인덱스 표시
                            GUI.Label(new Rect(cellRect.x + cellRect.width * 0.4f, cellRect.y + cellRect.height * 0.4f,
                                cellRect.width * 0.2f, cellRect.height * 0.2f),
                                $"{blockIndex}", new GUIStyle() { normal = { textColor = Color.white } });
                        }

                        // 기믹 표시
                        if (block.gimmicks != null && block.gimmicks.Count > 0)
                        {
                            foreach (var gimmick in block.gimmicks)
                            {
                                if (gimmick.gimmickType != "None")
                                {
                                    GUI.color = Color.yellow;
                                    GUI.DrawTexture(new Rect(cellRect.x + cellRect.width * 0.75f, cellRect.y,
                                        cellRect.width * 0.25f, cellRect.height * 0.25f),
                                        gimmickTexture);

                                    // 기믹 타입 표시
                                    GUI.Label(new Rect(cellRect.x + cellRect.width * 0.6f, cellRect.y,
                                        cellRect.width * 0.4f, cellRect.height * 0.2f),
                                        gimmick.gimmickType.Substring(0, 1),
                                        new GUIStyle() { normal = { textColor = Color.black } });
                                }
                            }
                        }

                        GUI.color = guiColor;
                    }
                }
            }
        }

        private void DrawPlayingBlockBorder(Rect cellRect)
        {
            // 플레이 블록 테두리 그리기
            Color prevColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, 0.9f); // 흰색 테두리

            float borderWidth = 2f; // 두꺼운 테두리
            float inset = 3f;      // 약간 안쪽으로

            // 테두리 사각형 (안쪽에 더 작은 사각형)
            Rect borderRect = new Rect(
                cellRect.x + inset,
                cellRect.y + inset,
                cellRect.width - (inset * 2),
                cellRect.height - (inset * 2)
            );

            // 상하좌우 테두리 그리기
            GUI.DrawTexture(new Rect(borderRect.x, borderRect.y, borderRect.width, borderWidth), EditorGUIUtility.whiteTexture);
            GUI.DrawTexture(new Rect(borderRect.x, borderRect.y, borderWidth, borderRect.height), EditorGUIUtility.whiteTexture);
            GUI.DrawTexture(new Rect(borderRect.x, borderRect.y + borderRect.height - borderWidth, borderRect.width, borderWidth), EditorGUIUtility.whiteTexture);
            GUI.DrawTexture(new Rect(borderRect.x + borderRect.width - borderWidth, borderRect.y, borderWidth, borderRect.height), EditorGUIUtility.whiteTexture);

            GUI.color = prevColor;
        }
        private void DrawwallsOnGrid(int x, int y, Rect cellRect)
        {
            // 현재 셀에 대한 벽 정보 처리
            foreach (var wall in walls)
            {
                // 벽의 시작점인지 확인 (데이터의 월드 Y를 에디터 Y로 변환하여 비교)
                bool isWallStartCell = (wall.x == x && WorldToEditorY(wall.y) == y);

                // 벽의 확장 영역 정보
                bool isWallExtension = false;

                // 확장된 벽인 경우 확장 영역 계산
                if (wall.Length > 1)
                {
                    // 벽 방향에 따라 확장 셀 확인
                    switch (wall.WallDirection)
                    {
                        // 위쪽/아래쪽 타입들은 오른쪽으로 확장 (수평 확장)
                        case ObjectPropertiesEnum.WallDirection.Single_Up:
                        case ObjectPropertiesEnum.WallDirection.Single_Down:
                        case ObjectPropertiesEnum.WallDirection.Left_Up:
                        case ObjectPropertiesEnum.WallDirection.Left_Down:
                        case ObjectPropertiesEnum.WallDirection.Right_Up:
                        case ObjectPropertiesEnum.WallDirection.Right_Down:
                        case ObjectPropertiesEnum.WallDirection.Open_Up:
                        case ObjectPropertiesEnum.WallDirection.Open_Down:
                            for (int i = 1; i < wall.Length; i++)
                            {
                                if (wall.x + i == x && WorldToEditorY(wall.y) == y)
                                {
                                    isWallExtension = true;
                                    break;
                                }
                            }
                            break;

                        // 왼쪽/오른쪽 타입들은 아래쪽으로 확장 (수직 확장)
                        case ObjectPropertiesEnum.WallDirection.Single_Left:
                        case ObjectPropertiesEnum.WallDirection.Single_Right:
                        case ObjectPropertiesEnum.WallDirection.Open_Left:
                        case ObjectPropertiesEnum.WallDirection.Open_Right:
                            for (int i = 1; i < wall.Length; i++)
                            {
                                if (wall.x == x && WorldToEditorY(wall.y + i) == y)
                                {
                                    isWallExtension = true;
                                    break;
                                }
                            }
                            break;
                    }
                }

                // 1. 벽의 시작점이면 벽 및 기믹 그리기
                if (isWallStartCell)
                {
                    // 벽 그리기
                    DrawWallSegment(x, y, cellRect, wall, false);

                    // 벽 기믹 그리기 (시작점에만)
                    if (wall.WallGimmickType != WallGimmickType.None)
                    {
                        DrawWallGimmick(cellRect, wall);
                    }
                }
                // 2. 벽의 확장 영역이면 해당 부분만 그리기
                else if (isWallExtension)
                {
                    // 확장 영역 벽 그리기
                    DrawWallSegment(x, y, cellRect, wall, true);
                }
            }
        }

        // 벽 세그먼트 그리기 헬퍼 메서드
        private void DrawWallSegment(int x, int y, Rect cellRect, WallData wall, bool isExtension)
        {
            // 벽 방향에 따라 그리기 영역 계산
            Rect wallRect = cellRect;

            switch (wall.WallDirection)
            {
                // 위쪽 타입들 (상단에 벽)
                case ObjectPropertiesEnum.WallDirection.Single_Up:
                case ObjectPropertiesEnum.WallDirection.Left_Up:
                case ObjectPropertiesEnum.WallDirection.Right_Up:
                case ObjectPropertiesEnum.WallDirection.Open_Up:
                    wallRect = new Rect(cellRect.x, cellRect.y, cellRect.width, cellRect.height * 0.1f);
                    break;

                // 아래쪽 타입들 (하단에 벽)
                case ObjectPropertiesEnum.WallDirection.Single_Down:
                case ObjectPropertiesEnum.WallDirection.Left_Down:
                case ObjectPropertiesEnum.WallDirection.Right_Down:
                case ObjectPropertiesEnum.WallDirection.Open_Down:
                    wallRect = new Rect(cellRect.x, cellRect.y + cellRect.height * 0.9f, cellRect.width, cellRect.height * 0.1f);
                    break;

                // 왼쪽 타입들 (좌측에 벽)
                case ObjectPropertiesEnum.WallDirection.Single_Left:
                case ObjectPropertiesEnum.WallDirection.Open_Left:
                    wallRect = new Rect(cellRect.x, cellRect.y, cellRect.width * 0.1f, cellRect.height);
                    break;

                // 오른쪽 타입들 (우측에 벽)
                case ObjectPropertiesEnum.WallDirection.Single_Right:
                case ObjectPropertiesEnum.WallDirection.Open_Right:
                    wallRect = new Rect(cellRect.x + cellRect.width * 0.9f, cellRect.y, cellRect.width * 0.1f, cellRect.height);
                    break;
            }

            // 벽 색상 설정 및 그리기
            Color guiColor = GUI.color;

            // 벽 색상
            if (wall.wallColor != ColorType.None)
            {
                GUI.color = GetColorFromType(wall.wallColor);
            }
            else
            {
                GUI.color = Color.gray;
            }

            // 벽 텍스처 그리기
            GUI.DrawTexture(wallRect, wallTexture);

            // 벽의 시작점에만 길이 표시
            if (!isExtension && wall.Length > 1)
            {
                // 길이 텍스트 표시
                GUI.color = Color.white;
                GUI.Label(new Rect(cellRect.x + 2, cellRect.y + 2, 30, 20),
                    $"L{wall.Length}", new GUIStyle() { normal = { textColor = Color.white } });

                // 확장 방향 화살표
                string directionSymbol = "";
                Vector2 symbolPos = Vector2.zero;

                switch (wall.WallDirection)
                {
                    // 위쪽/아래쪽 타입들은 오른쪽으로 확장
                    case ObjectPropertiesEnum.WallDirection.Single_Up:
                    case ObjectPropertiesEnum.WallDirection.Single_Down:
                    case ObjectPropertiesEnum.WallDirection.Left_Up:
                    case ObjectPropertiesEnum.WallDirection.Left_Down:
                    case ObjectPropertiesEnum.WallDirection.Right_Up:
                    case ObjectPropertiesEnum.WallDirection.Right_Down:
                    case ObjectPropertiesEnum.WallDirection.Open_Up:
                    case ObjectPropertiesEnum.WallDirection.Open_Down:
                        directionSymbol = "→"; // 오른쪽 화살표
                        symbolPos = new Vector2(cellRect.x + cellRect.width - 20, cellRect.y + cellRect.height * 0.5f - 10);
                        break;

                    // 왼쪽/오른쪽 타입들은 아래쪽으로 확장
                    case ObjectPropertiesEnum.WallDirection.Single_Left:
                    case ObjectPropertiesEnum.WallDirection.Single_Right:
                    case ObjectPropertiesEnum.WallDirection.Open_Left:
                    case ObjectPropertiesEnum.WallDirection.Open_Right:
                        directionSymbol = "↑"; // 위 화살표
                        symbolPos = new Vector2(cellRect.x + cellRect.width * 0.5f - 10, cellRect.y + cellRect.height - 20);
                        break;
                }

                // 화살표 표시
                GUI.Label(new Rect(symbolPos.x, symbolPos.y, 20, 20),
                    directionSymbol, new GUIStyle()
                    {
                        normal = { textColor = Color.white },
                        fontSize = 15,
                        fontStyle = FontStyle.Bold,
                        alignment = TextAnchor.MiddleCenter
                    });
            }

            // 선택된 벽 하이라이트
            if (walls.IndexOf(wall) == currentWallIndex)
            {
                // 벽 주변에 하이라이트 표시
                GUI.color = new Color(1f, 1f, 0.5f, 0.5f);

                // 벽 방향에 따라 하이라이트 위치 조정
                float highlightThickness = 2f;
                switch (wall.WallDirection)
                {
                    case ObjectPropertiesEnum.WallDirection.Single_Up:
                    case ObjectPropertiesEnum.WallDirection.Left_Up:
                    case ObjectPropertiesEnum.WallDirection.Right_Up:
                    case ObjectPropertiesEnum.WallDirection.Open_Up:
                        // 위쪽 벽의 경우 하이라이트
                        GUI.DrawTexture(new Rect(
                            cellRect.x,
                            cellRect.y + cellRect.height * 0.1f,
                            cellRect.width,
                            highlightThickness),
                            EditorGUIUtility.whiteTexture);
                        break;

                    case ObjectPropertiesEnum.WallDirection.Single_Down:
                    case ObjectPropertiesEnum.WallDirection.Left_Down:
                    case ObjectPropertiesEnum.WallDirection.Right_Down:
                    case ObjectPropertiesEnum.WallDirection.Open_Down:
                        // 아래쪽 벽의 경우 하이라이트
                        GUI.DrawTexture(new Rect(
                            cellRect.x,
                            cellRect.y + cellRect.height * 0.9f - highlightThickness,
                            cellRect.width,
                            highlightThickness),
                            EditorGUIUtility.whiteTexture);
                        break;

                    case ObjectPropertiesEnum.WallDirection.Single_Left:
                    case ObjectPropertiesEnum.WallDirection.Open_Left:
                        // 왼쪽 벽의 경우 하이라이트
                        GUI.DrawTexture(new Rect(
                            cellRect.x + cellRect.width * 0.1f,
                            cellRect.y,
                            highlightThickness,
                            cellRect.height),
                            EditorGUIUtility.whiteTexture);
                        break;

                    case ObjectPropertiesEnum.WallDirection.Single_Right:
                    case ObjectPropertiesEnum.WallDirection.Open_Right:
                        // 오른쪽 벽의 경우 하이라이트
                        GUI.DrawTexture(new Rect(
                            cellRect.x + cellRect.width * 0.9f - highlightThickness,
                            cellRect.y,
                            highlightThickness,
                            cellRect.height),
                            EditorGUIUtility.whiteTexture);
                        break;
                }
            }

            // GUI 색상 복원
            GUI.color = guiColor;
        }

        // 벽 기믹 그리기 헬퍼 메서드
        private void DrawWallGimmick(Rect cellRect, WallData wall)
        {
            if (wall.WallGimmickType == WallGimmickType.None)
                return;

            Color guiColor = GUI.color;

            // 벽 방향에 따라 기믹 아이콘 위치 조정
            Rect gimmickRect;

            switch (wall.WallDirection)
            {
                case ObjectPropertiesEnum.WallDirection.Single_Up:
                case ObjectPropertiesEnum.WallDirection.Left_Up:
                case ObjectPropertiesEnum.WallDirection.Right_Up:
                case ObjectPropertiesEnum.WallDirection.Open_Up:
                    // 위쪽 벽의 경우 아이콘 위치
                    gimmickRect = new Rect(
                        cellRect.x + cellRect.width * 0.75f,
                        cellRect.y + cellRect.height * 0.15f,
                        cellRect.width * 0.2f,
                        cellRect.height * 0.2f
                    );
                    break;
                case ObjectPropertiesEnum.WallDirection.Single_Down:
                case ObjectPropertiesEnum.WallDirection.Left_Down:
                case ObjectPropertiesEnum.WallDirection.Right_Down:
                case ObjectPropertiesEnum.WallDirection.Open_Down:
                    // 아래쪽 벽의 경우 아이콘 위치
                    gimmickRect = new Rect(
                        cellRect.x + cellRect.width * 0.75f,
                        cellRect.y + cellRect.height * 0.65f,
                        cellRect.width * 0.2f,
                        cellRect.height * 0.2f
                    );
                    break;
                case ObjectPropertiesEnum.WallDirection.Single_Left:
                case ObjectPropertiesEnum.WallDirection.Open_Left:
                    // 왼쪽 벽의 경우 아이콘 위치
                    gimmickRect = new Rect(
                        cellRect.x + cellRect.width * 0.15f,
                        cellRect.y + cellRect.height * 0.75f,
                        cellRect.width * 0.2f,
                        cellRect.height * 0.2f
                    );
                    break;
                case ObjectPropertiesEnum.WallDirection.Single_Right:
                case ObjectPropertiesEnum.WallDirection.Open_Right:
                    // 오른쪽 벽의 경우 아이콘 위치
                    gimmickRect = new Rect(
                        cellRect.x + cellRect.width * 0.65f,
                        cellRect.y + cellRect.height * 0.75f,
                        cellRect.width * 0.2f,
                        cellRect.height * 0.2f
                    );
                    break;
                default:
                    // 기본 위치
                    gimmickRect = new Rect(
                        cellRect.x + cellRect.width * 0.75f,
                        cellRect.y + cellRect.height * 0.75f,
                        cellRect.width * 0.2f,
                        cellRect.height * 0.2f
                    );
                    break;
            }

            // 기믹 아이콘 배경
            GUI.color = Color.yellow;
            GUI.DrawTexture(gimmickRect, gimmickTexture);

            // 특정 기믹 타입에 따른 아이콘 변경
            string gimmickIcon = "";
            switch (wall.WallGimmickType)
            {
                case WallGimmickType.Star:
                    gimmickIcon = "★";
                    break;
                case WallGimmickType.Lock:
                    gimmickIcon = "🔒";
                    break;
                case WallGimmickType.Key:
                    gimmickIcon = "🔑";
                    break;
                case WallGimmickType.Constraint:
                    gimmickIcon = "⚓";
                    break;
                case WallGimmickType.Multiple:
                    gimmickIcon = "×";
                    break;
                case WallGimmickType.Frozen:
                    gimmickIcon = "❄";
                    break;
                default:
                    gimmickIcon = "G";
                    break;
            }

            // 기믹 아이콘 표시
            GUI.color = Color.black;
            GUI.Label(
                gimmickRect,
                gimmickIcon,
                new GUIStyle()
                {
                    normal = { textColor = Color.black },
                    fontSize = 12,
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold
                }
            );

            // GUI 색상 복원
            GUI.color = guiColor;
        }

        private void DrawHoverAndSelectionEffects()
        {
            // 마우스 위치를 그리드 좌표로 변환
            Vector2 mousePos = Event.current.mousePosition;
            mousePos += scrollPosition;

            int gridX = Mathf.FloorToInt(mousePos.x / gridSize);
            int gridY = Mathf.FloorToInt(mousePos.y / gridSize);

            // 그리드 범위 체크
            if (gridX >= 0 && gridX < gridWidth && gridY >= 0 && gridY < gridHeight)
            {
                // 마우스 호버 효과
                Rect hoverRect = new Rect(gridX * gridSize, gridY * gridSize, gridSize - 1, gridSize - 1);

                Color prevColor = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, 0.2f);
                GUI.DrawTexture(hoverRect, EditorGUIUtility.whiteTexture);
                GUI.color = prevColor;

                // 도구 힌트 표시
                string hint = "";
                switch (currentTool)
                {
                    case EditTool.BoardBlock:
                        hint = $"보드 블록 ({selectedColor})";
                        break;
                    case EditTool.PlayingBlock:
                        hint = $"플레이 블록 ({selectedColor})";
                        break;
                    case EditTool.Wall:
                        hint = $"벽 (방향: {GetWallDirectionName(wallDirection)}, 길이: {wallLength})";
                        break;
                    case EditTool.Gimmick:
                        hint = $"기믹 ({selectedGimmick})";
                        break;
                    case EditTool.Erase:
                        hint = "지우기";
                        break;
                }

                GUI.Label(new Rect(hoverRect.x, hoverRect.y - 20, 200, 20),
           $"{gridX}, {gridY} - {hint}",
           new GUIStyle() { normal = { textColor = Color.white } });
            }
        }

        private void DrawJsonPanel()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField("JSON 변환", EditorStyles.boldLabel);

            // JSON 파일 경로
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("파일 경로:", GUILayout.Width(100));
            jsonFilePath = EditorGUILayout.TextField(jsonFilePath);

            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                string selectedPath = EditorUtility.SaveFilePanel("JSON 파일 경로", "", "Stage", "json");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    jsonFilePath = selectedPath;
                }
            }
            EditorGUILayout.EndHorizontal();

            // 내보내기/가져오기 버튼
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("JSON으로 내보내기"))
            {
                ExportToJson();
            }

            if (GUILayout.Button("JSON에서 가져오기"))
            {
                ImportFromJson();
            }

            EditorGUILayout.EndHorizontal();

            // ScriptableObject 내보내기/가져오기
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("ScriptableObject 변환", EditorStyles.boldLabel);

            if (GUILayout.Button("ScriptableObject로 저장"))
            {
                SaveStage();
            }

            if (GUILayout.Button("ScriptableObject에서 불러오기"))
            {
                LoadStage();
            }

            // 닫기 버튼
            if (GUILayout.Button("닫기"))
            {
                showJsonPanel = false;
            }

            EditorGUILayout.EndVertical();
        }

        #endregion
    }
}