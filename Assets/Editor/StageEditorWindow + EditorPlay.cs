// StageEditorWindow + EditorPlay.cs - 미리보기(Editor Play) 기능 구현
using UnityEditor;
using UnityEngine;
using Project.Scripts.Model;
using Project.Scripts.Controller;
using System.Collections.Generic;
using System;
using static ObjectPropertiesEnum;

namespace Project.Scripts.Editor
{
    

    public partial class StageEditorWindow
    {
        // 미리보기 모드 관련 변수들
        private Dictionary<Vector2Int, SimulationBoardBlock> simulationBoardBlocks = new Dictionary<Vector2Int, SimulationBoardBlock>();
        private List<SimulationBlock> simulationPlayingBlocks = new List<SimulationBlock>();
        private List<SimulationWall> simulationWalls = new List<SimulationWall>();
        private int selectedBlockIndex = -1;
        private bool isSimulating = false;
        private float lastMoveTime = 0f;
        private float moveCooldown = 0.15f; // 이동 간격 (초)
        private int blocksDestroyed = 0; // 파괴된 블록 수
        private bool gameWon = false; // 게임 승리 여부
        private string statusMessage = ""; // 상태 메시지

        // 에디터 상태 백업
        private List<BoardBlockData> backupBoardBlocks = new List<BoardBlockData>();
        private List<PlayingBlockData> backupPlayingBlocks = new List<PlayingBlockData>();
        private List<WallData> backupWalls = new List<WallData>();

        // 시뮬레이션 시작 메서드
        private void StartPreview()
        {
            // 이미 시뮬레이션 중이면 무시
            if (isSimulating)
                return;

            // 현재 상태 백업
            BackupCurrentState();

            // 미리보기 모드 설정
            previewMode = true;
            isSimulating = true;

            // 시뮬레이션 데이터 초기화
            InitializeSimulationData();

            // 현재 선택 상태 초기화
            currentPlayingBlockIndex = -1;
            currentWallIndex = -1;
            selectedBlockIndex = simulationPlayingBlocks.Count > 0 ? 0 : -1;

            // 첫 번째 블록 선택 상태로 설정
            if (selectedBlockIndex >= 0 && selectedBlockIndex < simulationPlayingBlocks.Count)
            {
                simulationPlayingBlocks[selectedBlockIndex].isSelected = true;
            }

            // 게임 승리 여부 초기화
            gameWon = false;
            blocksDestroyed = 0;
            statusMessage = "미리보기 모드: 화살표 키로 블록을 이동하세요. ESC로 종료합니다.";

            Repaint();
        }

        // 시뮬레이션 종료 메서드
        private void StopPreview()
        {
            // 미리보기 모드가 아니면 무시
            if (!isSimulating)
                return;

            // 미리보기 모드 종료
            previewMode = false;
            isSimulating = false;

            // 백업한 상태 복원
            RestoreState();

            // 선택 상태 초기화
            selectedBlockIndex = -1;

            // 시뮬레이션 데이터 정리
            simulationBoardBlocks.Clear();
            simulationPlayingBlocks.Clear();
            simulationWalls.Clear();

            Repaint();
        }

        // 현재 상태 백업
        private void BackupCurrentState()
        {
            backupBoardBlocks = new List<BoardBlockData>(boardBlocks);
            backupPlayingBlocks = new List<PlayingBlockData>(playingBlocks);
            backupWalls = new List<WallData>(walls);
        }

        // 백업한 상태 복원
        private void RestoreState()
        {
            boardBlocks = new List<BoardBlockData>(backupBoardBlocks);
            playingBlocks = new List<PlayingBlockData>(backupPlayingBlocks);
            walls = new List<WallData>(backupWalls);
        }

        // 시뮬레이션 데이터 초기화
        private void InitializeSimulationData()
        {
            simulationBoardBlocks.Clear();
            simulationPlayingBlocks.Clear();
            simulationWalls.Clear();

            // 보드 블록 변환
            foreach (var boardBlock in boardBlocks)
            {
                Vector2Int position = new Vector2Int(boardBlock.x, boardBlock.y);
                SimulationBoardBlock simBlock = new SimulationBoardBlock(position, boardBlock.ColorType);
                simulationBoardBlocks[position] = simBlock;
            }

            // 플레이 블록 변환
            foreach (var playingBlock in playingBlocks)
            {
                Vector2Int center = new Vector2Int(playingBlock.center.x, playingBlock.center.y);
                SimulationBlock simBlock = new SimulationBlock(center, playingBlock.ColorType);

                // 모양 추가
                foreach (var shape in playingBlock.shapes)
                {
                    simBlock.shapes.Add(new Vector2Int(shape.offset.x, shape.offset.y));
                }

                simulationPlayingBlocks.Add(simBlock);
            }

            // 벽 변환
            foreach (var wall in walls)
            {
                Vector2Int position = new Vector2Int(wall.x, wall.y);
                SimulationWall simWall = new SimulationWall(
                    position,
                    wall.wallColor,
                    wall.WallDirection,
                    wall.Length,
                    wall.WallGimmickType
                );

                simulationWalls.Add(simWall);

                // 벽 정보를 보드 블록에 추가
                if (simulationBoardBlocks.TryGetValue(position, out SimulationBoardBlock boardBlock))
                {
                    boardBlock.wallColorTypes.Add(wall.wallColor);
                    boardBlock.wallIsHorizontal.Add(simWall.IsHorizontal());
                    boardBlock.wallLengths.Add(wall.Length);
                }
            }
        }

        // 게임 입력 처리
        private void HandleGameInput()
        {
            Event e = Event.current;

            // ESC 키로 미리보기 종료
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
            {
                StopPreview();
                e.Use();
                return;
            }

            // 게임에서 승리했으면 더 이상 입력 처리 안함
            if (gameWon)
                return;

            // 블록이 선택되지 않았으면 무시
            if (selectedBlockIndex < 0 || selectedBlockIndex >= simulationPlayingBlocks.Count)
                return;

            // 방향키 입력 처리 (이동 쿨다운 적용)
            if (e.type == EventType.KeyDown && Time.realtimeSinceStartup - lastMoveTime > moveCooldown)
            {
                bool moved = false;
                Vector2Int moveDirection = Vector2Int.zero;

                switch (e.keyCode)
                {
                    case KeyCode.UpArrow:
                        moveDirection = new Vector2Int(0, 1);
                        moved = true;
                        break;
                    case KeyCode.DownArrow:
                        moveDirection = new Vector2Int(0, -1);
                        moved = true;
                        break;
                    case KeyCode.LeftArrow:
                        moveDirection = new Vector2Int(-1, 0);
                        moved = true;
                        break;
                    case KeyCode.RightArrow:
                        moveDirection = new Vector2Int(1, 0);
                        moved = true;
                        break;
                    case KeyCode.Tab:
                        // 다음 블록 선택
                        SwitchSelectedBlock();
                        moved = true;
                        break;
                }

                if (moved)
                {
                    lastMoveTime = Time.realtimeSinceStartup;

                    // 블록 이동 또는 선택 변경 시 화면 갱신
                    if (moveDirection != Vector2Int.zero)
                    {
                        MoveSelectedBlock(moveDirection);
                    }

                    e.Use();
                    Repaint();
                }
            }
        }

        // 선택한 블록 이동
        private void MoveSelectedBlock(Vector2Int direction)
        {
            if (selectedBlockIndex < 0 || selectedBlockIndex >= simulationPlayingBlocks.Count)
                return;

            SimulationBlock block = simulationPlayingBlocks[selectedBlockIndex];

            // 이동 시도
            bool canMove = block.TryMove(direction, simulationBoardBlocks, simulationPlayingBlocks, gridWidth, gridHeight);

            if (canMove)
            {
                // 벽과의 충돌 검사
                CheckWallCollisions(block);
            }
        }

        // 다음 블록 선택 (Tab 키)
        private void SwitchSelectedBlock()
        {
            if (simulationPlayingBlocks.Count == 0)
                return;

            // 현재 선택 블록 해제
            if (selectedBlockIndex >= 0 && selectedBlockIndex < simulationPlayingBlocks.Count)
            {
                simulationPlayingBlocks[selectedBlockIndex].isSelected = false;
            }

            // 다음 블록 선택
            selectedBlockIndex = (selectedBlockIndex + 1) % simulationPlayingBlocks.Count;
            simulationPlayingBlocks[selectedBlockIndex].isSelected = true;
        }

        // 벽과의 충돌 검사
        private void CheckWallCollisions(SimulationBlock block)
        {
            // 블록의 모든 부분에 대해 검사
            foreach (var shape in block.shapes)
            {
                Vector2Int blockPos = block.position + shape;

                // 보드 블록 가져오기
                if (simulationBoardBlocks.TryGetValue(blockPos, out SimulationBoardBlock boardBlock))
                {
                    // 벽 색상 및 방향 검사
                    for (int i = 0; i < boardBlock.wallColorTypes.Count; i++)
                    {
                        if (boardBlock.wallColorTypes[i] == block.colorType)
                        {
                            // 블록 크기 계산
                            int blockWidth = 0;
                            int blockHeight = 0;

                            int minX = int.MaxValue, maxX = int.MinValue;
                            int minY = int.MaxValue, maxY = int.MinValue;

                            foreach (var blockShape in block.shapes)
                            {
                                minX = Math.Min(minX, blockShape.x);
                                maxX = Math.Max(maxX, blockShape.x);
                                minY = Math.Min(minY, blockShape.y);
                                maxY = Math.Max(maxY, blockShape.y);
                            }

                            blockWidth = maxX - minX + 1;
                            blockHeight = maxY - minY + 1;

                            // 방향이 일치하고 크기가 일치하는지 확인
                            bool wallIsHorizontal = boardBlock.wallIsHorizontal[i];
                            int wallLength = boardBlock.wallLengths[i];

                            bool canDestroy = false;

                            if (wallIsHorizontal && blockWidth <= wallLength)
                            {
                                // 벽이 수평이고 블록의 너비가 벽 길이 이하인 경우
                                canDestroy = true;
                            }
                            else if (!wallIsHorizontal && blockHeight <= wallLength)
                            {
                                // 벽이 수직이고 블록의 높이가 벽 길이 이하인 경우
                                canDestroy = true;
                            }

                            if (canDestroy)
                            {
                                // 블록 파괴 처리
                                DestroyBlock(block);
                                return;
                            }
                        }
                    }
                }
            }
        }

        // 블록 파괴 처리
        private void DestroyBlock(SimulationBlock block)
        {
            // 블록 제거
            simulationPlayingBlocks.Remove(block);
            blocksDestroyed++;

            // 선택된 블록 초기화 및 다음 블록 선택
            selectedBlockIndex = simulationPlayingBlocks.Count > 0 ? 0 : -1;

            if (selectedBlockIndex >= 0)
            {
                simulationPlayingBlocks[selectedBlockIndex].isSelected = true;
            }

            // 모든 블록이 파괴되었는지 확인
            if (simulationPlayingBlocks.Count == 0)
            {
                gameWon = true;
                statusMessage = "축하합니다! 모든 블록을 파괴했습니다. ESC를 눌러 종료하세요.";
            }
            else
            {
                statusMessage = $"블록을 파괴했습니다! 남은 블록: {simulationPlayingBlocks.Count}개";
            }
        }

        // 게임 뷰 그리기
        private void DrawGameView()
        {
            // 그리드 영역 가져오기
            Rect gameViewRect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            // 배경 그리기
            GUI.DrawTexture(gameViewRect, gridTexture);

            // 스크롤 영역 시작
            scrollPosition = GUI.BeginScrollView(gameViewRect, scrollPosition,
                new Rect(0, 0, gridWidth * gridSize, gridHeight * gridSize));

            // 보드 블록 그리기
            foreach (var boardBlock in simulationBoardBlocks.Values)
            {
                DrawSimulationBoardBlock(boardBlock);
            }

            // 벽 그리기
            foreach (var wall in simulationWalls)
            {
                DrawSimulationWall(wall);
            }

            // 플레이 블록 그리기
            foreach (var playingBlock in simulationPlayingBlocks)
            {
                DrawSimulationPlayingBlock(playingBlock);
            }

            GUI.EndScrollView();

            // 상태 메시지 표시
            Rect statusRect = new Rect(gameViewRect.x + 10, gameViewRect.y + 10, gameViewRect.width - 20, 25);
            GUI.Label(statusRect, statusMessage, new GUIStyle(GUI.skin.label)
            {
                normal = { textColor = Color.white },
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperLeft
            });
        }

        // 시뮬레이션 보드 블록 그리기
        private void DrawSimulationBoardBlock(SimulationBoardBlock block)
        {
            // 그리드 위치 계산
            int editorY = WorldToEditorY(block.position.y);
            Rect cellRect = new Rect(
                block.position.x * gridSize,
                editorY * gridSize,
                gridSize - 1,
                gridSize - 1
            );

            // 색상 설정 및 그리기
            Color guiColor = GUI.color;
            GUI.color = GetColorFromType(block.colorType);
            GUI.DrawTexture(cellRect, colorTextures[(int)block.colorType]);
            GUI.color = guiColor;
        }

        // 시뮬레이션 벽 그리기
        private void DrawSimulationWall(SimulationWall wall)
        {
            // 그리드 위치 계산
            int editorY = WorldToEditorY(wall.position.y);
            Rect cellRect = new Rect(
                wall.position.x * gridSize,
                editorY * gridSize,
                gridSize - 1,
                gridSize - 1
            );

            // 벽 영역 계산
            Rect wallRect = cellRect;

            // 벽 방향에 따라 영역 조정
            if (wall.IsUpDirection())
            {
                wallRect = new Rect(cellRect.x, cellRect.y, cellRect.width, cellRect.height * 0.1f);
            }
            else if (wall.IsDownDirection())
            {
                wallRect = new Rect(cellRect.x, cellRect.y + cellRect.height * 0.9f, cellRect.width, cellRect.height * 0.1f);
            }
            else if (wall.IsLeftDirection())
            {
                wallRect = new Rect(cellRect.x, cellRect.y, cellRect.width * 0.1f, cellRect.height);
            }
            else if (wall.IsRightDirection())
            {
                wallRect = new Rect(cellRect.x + cellRect.width * 0.9f, cellRect.y, cellRect.width * 0.1f, cellRect.height);
            }

            // 벽 색상 설정 및 그리기
            Color guiColor = GUI.color;

            if (wall.colorType != ColorType.None)
            {
                GUI.color = GetColorFromType(wall.colorType);
            }
            else
            {
                GUI.color = Color.gray;
            }

            GUI.DrawTexture(wallRect, wallTexture);

            // 길이 표시
            if (wall.length > 1)
            {
                GUI.color = Color.white;
                GUI.Label(new Rect(cellRect.x + 2, cellRect.y + 2, 30, 20),
                    $"L{wall.length}", new GUIStyle() { normal = { textColor = Color.white } });
            }

            // 기믹 있는 경우 표시
            if (wall.gimmickType != WallGimmickType.None)
            {
                DrawWallGimmick(cellRect, wall);
            }

            GUI.color = guiColor;

            // 확장 영역 그리기
            if (wall.length > 1)
            {
                DrawWallExtension(wall);
            }
        }

        // 벽 확장 부분 그리기
        private void DrawWallExtension(SimulationWall wall)
        {
            bool isHorizontal = wall.IsHorizontal();

            for (int i = 1; i < wall.length; i++)
            {
                Vector2Int extPos;

                if (isHorizontal)
                {
                    // 수평 확장 (오른쪽으로)
                    extPos = new Vector2Int(wall.position.x + i, wall.position.y);
                }
                else
                {
                    // 수직 확장 (아래쪽으로)
                    extPos = new Vector2Int(wall.position.x, wall.position.y + i);
                }

                // 그리드 내 확인
                if (extPos.x >= 0 && extPos.x < gridWidth && extPos.y >= 0 && extPos.y < gridHeight)
                {
                    int editorY = WorldToEditorY(extPos.y);
                    Rect cellRect = new Rect(
                        extPos.x * gridSize,
                        editorY * gridSize,
                        gridSize - 1,
                        gridSize - 1
                    );

                    // 확장 영역 계산
                    Rect extRect = cellRect;

                    // 벽 방향에 따라 영역 조정
                    if (wall.IsUpDirection())
                    {
                        extRect = new Rect(cellRect.x, cellRect.y, cellRect.width, cellRect.height * 0.1f);
                    }
                    else if (wall.IsDownDirection())
                    {
                        extRect = new Rect(cellRect.x, cellRect.y + cellRect.height * 0.9f, cellRect.width, cellRect.height * 0.1f);
                    }
                    else if (wall.IsLeftDirection())
                    {
                        extRect = new Rect(cellRect.x, cellRect.y, cellRect.width * 0.1f, cellRect.height);
                    }
                    else if (wall.IsRightDirection())
                    {
                        extRect = new Rect(cellRect.x + cellRect.width * 0.9f, cellRect.y, cellRect.width * 0.1f, cellRect.height);
                    }

                    // 확장 부분 색상 설정 및 그리기
                    Color guiColor = GUI.color;

                    if (wall.colorType != ColorType.None)
                    {
                        GUI.color = GetColorFromType(wall.colorType);
                    }
                    else
                    {
                        GUI.color = Color.gray;
                    }

                    GUI.DrawTexture(extRect, wallTexture);

                    GUI.color = guiColor;
                }
            }
        }

        // 벽 기믹 그리기
        private void DrawWallGimmick(Rect cellRect, SimulationWall wall)
        {
            // 기본 값
            Rect gimmickRect = new Rect(
                cellRect.x + cellRect.width * 0.75f,
                cellRect.y + cellRect.height * 0.75f,
                cellRect.width * 0.2f,
                cellRect.height * 0.2f
            );

            // 벽 방향에 따라 기믹 아이콘 위치 조정
            if (wall.IsUpDirection())
            {
                gimmickRect = new Rect(
                    cellRect.x + cellRect.width * 0.75f,
                    cellRect.y + cellRect.height * 0.15f,
                    cellRect.width * 0.2f,
                    cellRect.height * 0.2f
                );
            }
            else if (wall.IsDownDirection())
            {
                gimmickRect = new Rect(
                    cellRect.x + cellRect.width * 0.75f,
                    cellRect.y + cellRect.height * 0.65f,
                    cellRect.width * 0.2f,
                    cellRect.height * 0.2f
                );
            }
            else if (wall.IsLeftDirection())
            {
                gimmickRect = new Rect(
                    cellRect.x + cellRect.width * 0.15f,
                    cellRect.y + cellRect.height * 0.75f,
                    cellRect.width * 0.2f,
                    cellRect.height * 0.2f
                );
            }
            else if (wall.IsRightDirection())
            {
                gimmickRect = new Rect(
                    cellRect.x + cellRect.width * 0.65f,
                    cellRect.y + cellRect.height * 0.75f,
                    cellRect.width * 0.2f,
                    cellRect.height * 0.2f
                );
            }

            // 기믹 아이콘 배경
            Color guiColor = GUI.color;
            GUI.color = Color.yellow;
            GUI.DrawTexture(gimmickRect, gimmickTexture);

            // 특정 기믹 타입에 따른 아이콘 변경
            string gimmickIcon = "";
            switch (wall.gimmickType)
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

            GUI.color = guiColor;
        }

        // 시뮬레이션 플레이 블록 그리기
        private void DrawSimulationPlayingBlock(SimulationBlock block)
        {
            // 블록의 모든 모양 그리기
            foreach (var shape in block.shapes)
            {
                Vector2Int blockPos = block.position + shape;

                // 그리드 내 확인
                if (blockPos.x >= 0 && blockPos.x < gridWidth && blockPos.y >= 0 && blockPos.y < gridHeight)
                {
                    int editorY = WorldToEditorY(blockPos.y);
                    Rect cellRect = new Rect(
                        blockPos.x * gridSize,
                        editorY * gridSize,
                        gridSize - 1,
                        gridSize - 1
                    );

                    // 색상 설정 및 그리기
                    Color guiColor = GUI.color;
                    GUI.color = GetColorFromType(block.colorType);
                    GUI.DrawTexture(cellRect, colorTextures[(int)block.colorType]);

                    // 테두리 그리기
                    DrawPlayingBlockBorder(cellRect);

                    // 선택된 블록 하이라이트
                    if (block.isSelected)
                    {
                        GUI.color = new Color(1f, 1f, 1f, 0.3f);
                        GUI.DrawTextureWithTexCoords(cellRect, playingBlockPatternTexture,
                            new Rect(0, 0, cellRect.width / 4, cellRect.height / 4));
                    }

                    // 블록 중심 표시 (0, 0 오프셋)
                    if (shape.x == 0 && shape.y == 0)
                    {
                        GUI.color = new Color(1f, 1f, 1f, 0.7f);
                        GUI.DrawTexture(new Rect(cellRect.x + cellRect.width * 0.25f, cellRect.y + cellRect.height * 0.25f,
                            cellRect.width * 0.5f, cellRect.height * 0.5f),
                            EditorGUIUtility.whiteTexture);
                    }

                    GUI.color = guiColor;
                }
            }
        }

        // 게임 컨트롤 패널 그리기
        private void DrawGameControlPanel()
        {
            EditorGUILayout.LabelField("게임 컨트롤", EditorStyles.boldLabel);

            // 컨트롤 설명
            EditorGUILayout.HelpBox(
                "화살표 키: 블록 이동\n" +
                "Tab: 다음 블록 선택\n" +
                "ESC: 미리보기 종료",
                MessageType.Info
            );

            EditorGUILayout.Space(10);

            // 게임 상태 정보
            EditorGUILayout.LabelField("게임 상태", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("남은 블록:", GUILayout.Width(100));
            EditorGUILayout.LabelField($"{simulationPlayingBlocks.Count}개", GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("파괴한 블록:", GUILayout.Width(100));
            EditorGUILayout.LabelField($"{blocksDestroyed}개", GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();

            // 현재 선택된 블록 정보
            if (selectedBlockIndex >= 0 && selectedBlockIndex < simulationPlayingBlocks.Count)
            {
                var selectedBlock = simulationPlayingBlocks[selectedBlockIndex];

                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("선택된 블록", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("위치:", GUILayout.Width(100));
                EditorGUILayout.LabelField($"({selectedBlock.position.x}, {selectedBlock.position.y})", GUILayout.Width(100));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("색상:", GUILayout.Width(100));
                EditorGUILayout.LabelField($"{selectedBlock.colorType}", GUILayout.Width(100));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("블록 크기:", GUILayout.Width(100));
                EditorGUILayout.LabelField($"{selectedBlock.shapes.Count}칸", GUILayout.Width(100));
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(10);

            // 미리보기 종료 버튼
            if (GUILayout.Button("미리보기 종료", GUILayout.Height(30)))
            {
                StopPreview();
            }
        }
    }
}