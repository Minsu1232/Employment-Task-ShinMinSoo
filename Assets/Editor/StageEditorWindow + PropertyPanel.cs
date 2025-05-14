// 4. StageEditorWindow.PropertyPanel.cs - 속성 패널 관련 메서드
using UnityEditor;
using UnityEngine;
using Project.Scripts.Model;
using System.Collections.Generic;
using System;
using Project.Scripts.Controller;
using static ObjectPropertiesEnum;

namespace Project.Scripts.Editor
{
    public partial class StageEditorWindow
    {
        #region 속성 패널

        private void DrawPropertyPanel()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // 스테이지 설정
            EditorGUILayout.LabelField("스테이지 설정", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("스테이지 번호:", GUILayout.Width(100));
            currentStage.stageIndex = EditorGUILayout.IntField(currentStage.stageIndex, GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("그리드 크기:", GUILayout.Width(100));
            EditorGUILayout.LabelField($"{gridWidth}x{gridHeight}", GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("그리드 크기 변경"))
            {
                // 그리드 크기 변경 다이얼로그
                ChangeGridSize();
            }

            EditorGUILayout.Space(10);

            // 도구별 속성 패널
            switch (currentTool)
            {
                case EditTool.BoardBlock:
                    DrawBoardBlockProperties();
                    break;
                case EditTool.PlayingBlock:
                    DrawPlayingBlockProperties();
                    break;
                case EditTool.Wall:
                    DrawWallProperties();
                    break;
                case EditTool.Gimmick:
                    DrawGimmickProperties();
                    break;
                case EditTool.Erase:
                    DrawEraseProperties();
                    break;
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawBoardBlockProperties()
        {
            EditorGUILayout.LabelField("보드 블록 속성", EditorStyles.boldLabel);

            // 색상 선택
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("색상:", GUILayout.Width(100));
            selectedColor = (ColorType)EditorGUILayout.EnumPopup(selectedColor, GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();

            // 모드 선택 UI 추가
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("생성 모드:", GUILayout.Width(100));

            EditorGUILayout.BeginHorizontal();
            bool newRectMode = GUILayout.Toggle(boardBlockMode == BoardBlockMode.Rectangle, "사각형", EditorStyles.radioButton);
            bool newFreeFormMode = GUILayout.Toggle(boardBlockMode == BoardBlockMode.FreeForm, "자유 모양", EditorStyles.radioButton);
            EditorGUILayout.EndHorizontal();

            // 모드 변경 감지 및 처리
            if (newRectMode && boardBlockMode != BoardBlockMode.Rectangle)
            {
                boardBlockMode = BoardBlockMode.Rectangle;
                selectedBoardCells.Clear(); 
            }
            else if (newFreeFormMode && boardBlockMode != BoardBlockMode.FreeForm)
            {
                boardBlockMode = BoardBlockMode.FreeForm; 
                selectedBoardCells.Clear();
            }

            // 모드별 설명 추가
            if (boardBlockMode == BoardBlockMode.Rectangle)
            {
                EditorGUILayout.HelpBox("보드 블록을 배치하려면 그리드를 클릭하거나 드래그하세요.", MessageType.Info);
            }
            else // FreeForm 모드
            {
                EditorGUILayout.HelpBox("클릭하거나 드래그하여 셀을 선택하세요. Shift+클릭으로 선택을 취소할 수 있습니다.", MessageType.Info);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField($"선택된 셀: {selectedBoardCells.Count}개");

                EditorGUILayout.BeginHorizontal();
                GUI.enabled = selectedBoardCells.Count > 0;
                if (GUILayout.Button("블록 생성", GUILayout.Height(24)))
                {
                    CreateFreeFormBoardBlocks();
                }

                if (GUILayout.Button("선택 초기화", GUILayout.Height(24)))
                {
                    selectedBoardCells.Clear();
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space(10);

            // 보드 블록 통계
            EditorGUILayout.LabelField("보드 블록 통계", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"총 보드 블록 수: {boardBlocks.Count}");

            // 색상별 통계
            Dictionary<ColorType, int> colorCounts = new Dictionary<ColorType, int>();
            foreach (var block in boardBlocks)
            {
                if (!colorCounts.ContainsKey(block.ColorType))
                {
                    colorCounts[block.ColorType] = 0;
                }
                colorCounts[block.ColorType]++;
            }

            foreach (var colorCount in colorCounts)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"{colorCount.Key}:", GUILayout.Width(100));
                EditorGUILayout.LabelField($"{colorCount.Value}개", GUILayout.Width(100));
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawPlayingBlockProperties()
        {
            EditorGUILayout.LabelField("플레이 블록 속성", EditorStyles.boldLabel);

            // 색상 선택
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("색상:", GUILayout.Width(100));
            selectedColor = (ColorType)EditorGUILayout.EnumPopup(selectedColor, GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();

            // 모드 선택 UI 추가
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("생성 모드:", GUILayout.Width(100));

            EditorGUILayout.BeginHorizontal();
            bool newRectMode = GUILayout.Toggle(playingBlockMode == PlayingBlockMode.Rectangle, "1: 사각형", EditorStyles.radioButton);
            bool newFreeFormMode = GUILayout.Toggle(playingBlockMode == PlayingBlockMode.FreeForm, "2: 자유 모양", EditorStyles.radioButton);
            EditorGUILayout.EndHorizontal();

            // 모드 변경 감지 및 처리
            if (newRectMode && playingBlockMode != PlayingBlockMode.Rectangle)
            {
                playingBlockMode = PlayingBlockMode.Rectangle;
                selectedCells.Clear();
            }
            else if (newFreeFormMode && playingBlockMode != PlayingBlockMode.FreeForm)
            {
                playingBlockMode = PlayingBlockMode.FreeForm;
                selectedCells.Clear();
            }

            // 모드별 설명 추가
            if (playingBlockMode == PlayingBlockMode.Rectangle)
            {
                EditorGUILayout.HelpBox("플레이 블록을 만들려면 그리드를 드래그하여 영역을 선택하세요.", MessageType.Info);
            }
            else // FreeForm 모드
            {
                EditorGUILayout.HelpBox("개별 셀을 클릭하여 선택하고 Shift+클릭으로 여러 셀을 선택할 수 있습니다.", MessageType.Info);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField($"선택된 셀: {selectedCells.Count}개");

                EditorGUILayout.BeginHorizontal();
                GUI.enabled = selectedCells.Count > 0;
                if (GUILayout.Button("블록 생성", GUILayout.Height(24)))
                {
                    CreateFreeFormPlayingBlock();
                }

                if (GUILayout.Button("선택 초기화", GUILayout.Height(24)))
                {
                    selectedCells.Clear();
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space(10);

            // 현재 선택된 플레이 블록 정보
            if (currentPlayingBlockIndex >= 0 && currentPlayingBlockIndex < playingBlocks.Count)
            {
                var selectedBlock = playingBlocks[currentPlayingBlockIndex];
                EditorGUILayout.LabelField("선택된 블록 정보", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("인덱스:", GUILayout.Width(100));
                EditorGUILayout.LabelField($"{currentPlayingBlockIndex}", GUILayout.Width(100));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("중심 위치:", GUILayout.Width(100));
                EditorGUILayout.LabelField($"({selectedBlock.center.x}, {selectedBlock.center.y})", GUILayout.Width(100));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("색상:", GUILayout.Width(100));
                selectedBlock.ColorType = (ColorType)EditorGUILayout.EnumPopup(selectedBlock.ColorType, GUILayout.Width(100));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("블록 수:", GUILayout.Width(100));
                EditorGUILayout.LabelField($"{selectedBlock.shapes.Count}개", GUILayout.Width(100));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("기믹 수:", GUILayout.Width(100));
                EditorGUILayout.LabelField($"{selectedBlock.gimmicks.Count}개", GUILayout.Width(100));
                EditorGUILayout.EndHorizontal();

                // 편집 옵션 (확장)
                EditorGUILayout.Space(5);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("블록 작업:", EditorStyles.boldLabel);

                // 블록 이동 버튼 (상하좌우)
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("↑", GUILayout.Width(40), GUILayout.Height(24)))
                {
                    MoveSelectedBlock(0, 1); // 위로 이동
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("←", GUILayout.Width(40), GUILayout.Height(24)))
                {
                    MoveSelectedBlock(-1, 0); // 왼쪽으로 이동
                }

                if (GUILayout.Button("→", GUILayout.Width(40), GUILayout.Height(24)))
                {
                    MoveSelectedBlock(1, 0); // 오른쪽으로 이동
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("↓", GUILayout.Width(40), GUILayout.Height(24)))
                {
                    MoveSelectedBlock(0, -1); // 아래로 이동
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                // 회전 버튼
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("왼쪽으로 회전", GUILayout.Height(24)))
                {
                    RotateSelectedBlock(false); // 왼쪽 회전
                }

                if (GUILayout.Button("오른쪽으로 회전", GUILayout.Height(24)))
                {
                    RotateSelectedBlock(true); // 오른쪽 회전
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(5);

                // 블록 삭제 버튼
                if (GUILayout.Button("이 블록 삭제", GUILayout.Height(24)))
                {
                    if (EditorUtility.DisplayDialog("블록 삭제", "정말로 이 블록을 삭제하시겠습니까?", "확인", "취소"))
                    {
                        playingBlocks.RemoveAt(currentPlayingBlockIndex);
                        currentPlayingBlockIndex = -1;
                    }
                }
                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.HelpBox("블록을 선택하려면 그리드에서 블록을 클릭하세요.", MessageType.Info);
            }

            EditorGUILayout.Space(10);

            // 플레이 블록 통계
            EditorGUILayout.LabelField("플레이 블록 통계", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"총 플레이 블록 수: {playingBlocks.Count}");
        }

        private void DrawWallProperties()
        {
            EditorGUILayout.LabelField("벽 속성", EditorStyles.boldLabel);

            // 벽 방향
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("방향:", GUILayout.Width(100));
            wallDirection = (ObjectPropertiesEnum.WallDirection)EditorGUILayout.EnumPopup(wallDirection, GUILayout.Width(130));
            EditorGUILayout.EndHorizontal();

            // 벽 색상
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("색상:", GUILayout.Width(100));
            wallColor = (ColorType)EditorGUILayout.EnumPopup(wallColor, GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();

            // 벽 길이
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("길이:", GUILayout.Width(100));
            wallLength = EditorGUILayout.IntField(wallLength, GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();

            wallLength = Mathf.Clamp(wallLength, 1, 10);

            EditorGUILayout.HelpBox("벽을 배치하려면 그리드를 클릭하세요.", MessageType.Info);

            EditorGUILayout.Space(10);

            // 현재 선택된 벽 정보 (플레이 블록과 동일한 방식)
            if (currentWallIndex >= 0 && currentWallIndex < walls.Count)
            {
                var selectedWall = walls[currentWallIndex];
                EditorGUILayout.LabelField("선택된 벽 정보", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("인덱스:", GUILayout.Width(100));
                EditorGUILayout.LabelField($"{currentWallIndex}", GUILayout.Width(100));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("위치:", GUILayout.Width(100));
                EditorGUILayout.LabelField($"({selectedWall.x}, {selectedWall.y})", GUILayout.Width(100));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("방향:", GUILayout.Width(100));
                selectedWall.WallDirection = (ObjectPropertiesEnum.WallDirection)EditorGUILayout.EnumPopup(selectedWall.WallDirection, GUILayout.Width(130));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("색상:", GUILayout.Width(100));
                selectedWall.wallColor = (ColorType)EditorGUILayout.EnumPopup(selectedWall.wallColor, GUILayout.Width(100));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("길이:", GUILayout.Width(100));
                selectedWall.Length = EditorGUILayout.IntField(selectedWall.Length, GUILayout.Width(100));
                EditorGUILayout.EndHorizontal();

                // 벽 선택 시, 선택 해제 버튼 추가
                if (GUILayout.Button("선택 해제", GUILayout.Height(24)))
                {
                    currentWallIndex = -1;
                }

                // 벽 삭제 버튼
                if (GUILayout.Button("이 벽 삭제", GUILayout.Height(24)))
                {
                    if (EditorUtility.DisplayDialog("벽 삭제", "정말로 이 벽을 삭제하시겠습니까?", "확인", "취소"))
                    {
                        walls.RemoveAt(currentWallIndex);
                        currentWallIndex = -1;
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("벽을 선택하려면 그리드에서 벽을 클릭하세요.", MessageType.Info);
            }

            EditorGUILayout.Space(10);

            // 벽 통계
            EditorGUILayout.LabelField("벽 통계", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"총 벽 수: {walls.Count}");

            // 방향별 통계
            Dictionary<ObjectPropertiesEnum.WallDirection, int> directionCounts = new Dictionary<ObjectPropertiesEnum.WallDirection, int>();
            foreach (var wall in walls)
            {
                if (!directionCounts.ContainsKey(wall.WallDirection))
                {
                    directionCounts[wall.WallDirection] = 0;
                }
                directionCounts[wall.WallDirection]++;
            }

            foreach (var directionCount in directionCounts)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"{GetWallDirectionName(directionCount.Key)}:", GUILayout.Width(100));
                EditorGUILayout.LabelField($"{directionCount.Value}개", GUILayout.Width(100));
                EditorGUILayout.EndHorizontal();
            }

            // 기믹별 통계 추가
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("벽 기믹 통계:", EditorStyles.boldLabel);

            Dictionary<WallGimmickType, int> gimmickCounts = new Dictionary<WallGimmickType, int>();
            foreach (var wall in walls)
            {
                if (!gimmickCounts.ContainsKey(wall.WallGimmickType))
                {
                    gimmickCounts[wall.WallGimmickType] = 0;
                }
                gimmickCounts[wall.WallGimmickType]++;
            }

            if (gimmickCounts.Count == 0 || (gimmickCounts.Count == 1 && gimmickCounts.ContainsKey(WallGimmickType.None)))
            {
                EditorGUILayout.LabelField("설정된 기믹이 없습니다.");
            }
            else
            {
                foreach (var gimmickCount in gimmickCounts)
                {
                    if (gimmickCount.Key != WallGimmickType.None)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField($"{gimmickCount.Key}:", GUILayout.Width(100));
                        EditorGUILayout.LabelField($"{gimmickCount.Value}개", GUILayout.Width(100));
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
        }

        private void DrawGimmickProperties()
        {
            EditorGUILayout.LabelField("기믹 속성", EditorStyles.boldLabel);

            // 기믹 타입
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("타입:", GUILayout.Width(100));
            selectedGimmick = EditorGUILayout.TextField(selectedGimmick, GUILayout.Width(150));
            EditorGUILayout.EndHorizontal();

            // 기본 기믹 타입 버튼
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Star", GUILayout.Width(60)))
            {
                selectedGimmick = "Star";
            }

            if (GUILayout.Button("Lock", GUILayout.Width(60)))
            {
                selectedGimmick = "Lock";
            }

            if (GUILayout.Button("Key", GUILayout.Width(60)))
            {
                selectedGimmick = "Key";
            }

            if (GUILayout.Button("Ice", GUILayout.Width(60)))
            {
                selectedGimmick = "Ice";
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.HelpBox("기믹을 추가하려면 플레이 블록을 클릭하세요.", MessageType.Info);

            EditorGUILayout.Space(10);

            // 기믹 통계
            EditorGUILayout.LabelField("기믹 통계", EditorStyles.boldLabel);

            Dictionary<string, int> gimmickCounts = new Dictionary<string, int>();
            foreach (var block in playingBlocks)
            {
                foreach (var gimmick in block.gimmicks)
                {
                    if (gimmick.gimmickType != "None")
                    {
                        if (!gimmickCounts.ContainsKey(gimmick.gimmickType))
                        {
                            gimmickCounts[gimmick.gimmickType] = 0;
                        }
                        gimmickCounts[gimmick.gimmickType]++;
                    }
                }
            }

            if (gimmickCounts.Count == 0)
            {
                EditorGUILayout.LabelField("기믹이 없습니다.");
            }
            else
            {
                foreach (var gimmickCount in gimmickCounts)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"{gimmickCount.Key}:", GUILayout.Width(100));
                    EditorGUILayout.LabelField($"{gimmickCount.Value}개", GUILayout.Width(100));
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        private void DrawEraseProperties()
        {
            EditorGUILayout.LabelField("지우기 도구", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("객체를 지우려면 그리드에서 클릭하세요.", MessageType.Info);

            // 일괄 삭제 버튼
            if (GUILayout.Button("모든 보드 블록 지우기"))
            {
                if (EditorUtility.DisplayDialog("모든 보드 블록 지우기", "정말로 모든 보드 블록을 지우시겠습니까?", "확인", "취소"))
                {
                    boardBlocks.Clear();
                }
            }

            if (GUILayout.Button("모든 플레이 블록 지우기"))
            {
                if (EditorUtility.DisplayDialog("모든 플레이 블록 지우기", "정말로 모든 플레이 블록을 지우시겠습니까?", "확인", "취소"))
                {
                    playingBlocks.Clear();
                    currentPlayingBlockIndex = -1;
                }
            }

            if (GUILayout.Button("모든 벽 지우기"))
            {
                if (EditorUtility.DisplayDialog("모든 벽 지우기", "정말로 모든 벽을 지우시겠습니까?", "확인", "취소"))
                {
                    walls.Clear();
                }
            }

            if (GUILayout.Button("모두 지우기"))
            {
                if (EditorUtility.DisplayDialog("모두 지우기", "정말로 모든 객체를 지우시겠습니까?", "확인", "취소"))
                {
                    boardBlocks.Clear();
                    playingBlocks.Clear();
                    walls.Clear();
                    currentPlayingBlockIndex = -1;
                }
            }
        }

        #endregion
    }
}