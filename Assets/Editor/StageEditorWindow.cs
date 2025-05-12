// 1. StageEditorWindow.cs - 메인 클래스와 필드 선언
using UnityEditor;
using UnityEngine;
using Project.Scripts.Model;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UIElements;
using System;
using Project.Scripts.Controller;

namespace Project.Scripts.Editor
{
    public partial class StageEditorWindow : EditorWindow
    {
        #region 필드


        private enum BoardBlockMode
        {
            Rectangle,  // 사각형 모드
            FreeForm    // 자유 모양 모드
        }
        private BoardBlockMode boardBlockMode = BoardBlockMode.Rectangle;
        private bool isDraggingBoardFreeForm = false;
        private Vector2Int lastDraggedBoardCell = new Vector2Int(-1, -1);
        private List<Vector2Int> selectedBoardCells = new List<Vector2Int>();
        private enum PlayingBlockMode
        {
            Rectangle,  // 기존 사각형 모드
            FreeForm    // 자유 모양 모드
        }
        private PlayingBlockMode playingBlockMode = PlayingBlockMode.Rectangle;
        private List<Vector2Int> selectedCells = new List<Vector2Int>(); // 자유 모양용 선택된 셀

        private StageData currentStage;
        private Vector2 scrollPosition;
        private Rect gridViewRect;
        private Vector2Int currentMouseGridPos = new Vector2Int(-1, -1); 

        private bool isDraggingInFreeFormMode = false;  // 자유 모양 모드에서 드래그 중인지 여부
        private Vector2Int lastDraggedCell = new Vector2Int(-1, -1);  // 마지막으로 드래그된 셀 위치
        private bool isDraggingBoardBlock = false;
        private Vector2Int? boardDragStart = null;

        // 에디터 설정
        private int gridSize = 50; // 그리드 셀 크기 (픽셀)
        private int gridWidth = 10; // 그리드 너비
        private int gridHeight = 10; // 그리드 높이
       
        // 편집 도구
        private enum EditTool
        {
            BoardBlock,
            PlayingBlock,
            Wall,
            Gimmick,
            Erase
        }
        private EditTool currentTool = EditTool.BoardBlock;

        // 색상 선택
        private ColorType selectedColor = ColorType.Red;

        // 블록 사이즈
        private Vector2Int blockSize = new Vector2Int(1, 1);

        // 벽 방향
        private ObjectPropertiesEnum.WallDirection wallDirection = ObjectPropertiesEnum.WallDirection.Single_Up;
        private int wallLength = 1;
        private ColorType wallColor = ColorType.None;

        // 기믹 타입
        private string selectedGimmick = "None";

        // 저장된 블록 데이터
        private List<BoardBlockData> boardBlocks = new List<BoardBlockData>();
        private List<PlayingBlockData> playingBlocks = new List<PlayingBlockData>();
        private List<WallData> walls = new List<WallData>();

        // 미리보기 모드
        private bool previewMode = false;

        // 텍스처
        private Texture2D gridTexture;
        private Texture2D cellTexture;
        private Texture2D[] colorTextures;
        private Texture2D wallTexture;
        private Texture2D gimmickTexture;
        private Texture2D playingBlockPatternTexture;
        // 현재 작업 중인 블록
        private int currentPlayingBlockIndex = -1;
        private Vector2Int? startDragPosition = null;

        // JSON 변환 관련
        private string jsonFilePath = "";
        private bool showJsonPanel = false;

        #endregion

        #region Unity 메서드

        [MenuItem("Tools/스테이지 에디터")]
        public static void ShowWindow()
        {
            StageEditorWindow window = GetWindow<StageEditorWindow>("스테이지 에디터");
            window.minSize = new Vector2(800, 600);
            window.Show();
        }

        private void OnEnable()
        {
            InitializeTextures();
            LoadOrCreateStage();
        }

        private void OnGUI()
        {
            DrawToolbar();

            EditorGUILayout.BeginHorizontal();

            // 왼쪽 패널 - 속성
            EditorGUILayout.BeginVertical(GUILayout.Width(250));
            DrawPropertyPanel();
            EditorGUILayout.EndVertical();

            // 오른쪽 패널 - 그리드
            EditorGUILayout.BeginVertical();
            DrawGrid();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            // JSON 변환 패널
            if (showJsonPanel)
            {
                DrawJsonPanel();
            }

            HandleInput();

            if (GUI.changed)
            {
                Repaint();
            }
        }

        private void OnDisable()
        {
            SaveCurrentStage();
        }

        #endregion
    }
}