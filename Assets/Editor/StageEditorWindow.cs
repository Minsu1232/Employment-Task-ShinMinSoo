// 1. StageEditorWindow.cs - ���� Ŭ������ �ʵ� ����
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
        #region �ʵ�


        private enum BoardBlockMode
        {
            Rectangle,  // �簢�� ���
            FreeForm    // ���� ��� ���
        }
        private BoardBlockMode boardBlockMode = BoardBlockMode.Rectangle;
        private bool isDraggingBoardFreeForm = false;
        private Vector2Int lastDraggedBoardCell = new Vector2Int(-1, -1);
        private List<Vector2Int> selectedBoardCells = new List<Vector2Int>();
        private enum PlayingBlockMode
        {
            Rectangle,  // ���� �簢�� ���
            FreeForm    // ���� ��� ���
        }
        private PlayingBlockMode playingBlockMode = PlayingBlockMode.Rectangle;
        private List<Vector2Int> selectedCells = new List<Vector2Int>(); // ���� ���� ���õ� ��

        private StageData currentStage;
        private Vector2 scrollPosition;
        private Rect gridViewRect;
        private Vector2Int currentMouseGridPos = new Vector2Int(-1, -1); 

        private bool isDraggingInFreeFormMode = false;  // ���� ��� ��忡�� �巡�� ������ ����
        private Vector2Int lastDraggedCell = new Vector2Int(-1, -1);  // ���������� �巡�׵� �� ��ġ
        private bool isDraggingBoardBlock = false;
        private Vector2Int? boardDragStart = null;

        // ������ ����
        private int gridSize = 50; // �׸��� �� ũ�� (�ȼ�)
        private int gridWidth = 10; // �׸��� �ʺ�
        private int gridHeight = 10; // �׸��� ����
       
        // ���� ����
        private enum EditTool
        {
            BoardBlock,
            PlayingBlock,
            Wall,
            Gimmick,
            Erase
        }
        private EditTool currentTool = EditTool.BoardBlock;

        // ���� ����
        private ColorType selectedColor = ColorType.Red;

        // ��� ������
        private Vector2Int blockSize = new Vector2Int(1, 1);

        // �� ����
        private ObjectPropertiesEnum.WallDirection wallDirection = ObjectPropertiesEnum.WallDirection.Single_Up;
        private int wallLength = 1;
        private ColorType wallColor = ColorType.None;

        // ��� Ÿ��
        private string selectedGimmick = "None";

        // ����� ��� ������
        private List<BoardBlockData> boardBlocks = new List<BoardBlockData>();
        private List<PlayingBlockData> playingBlocks = new List<PlayingBlockData>();
        private List<WallData> walls = new List<WallData>();

        // �̸����� ���
        private bool previewMode = false;

        // �ؽ�ó
        private Texture2D gridTexture;
        private Texture2D cellTexture;
        private Texture2D[] colorTextures;
        private Texture2D wallTexture;
        private Texture2D gimmickTexture;
        private Texture2D playingBlockPatternTexture;
        // ���� �۾� ���� ���
        private int currentPlayingBlockIndex = -1;
        private Vector2Int? startDragPosition = null;

        // JSON ��ȯ ����
        private string jsonFilePath = "";
        private bool showJsonPanel = false;

        #endregion

        #region Unity �޼���

        [MenuItem("Tools/�������� ������")]
        public static void ShowWindow()
        {
            StageEditorWindow window = GetWindow<StageEditorWindow>("�������� ������");
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

            // ���� �г� - �Ӽ�
            EditorGUILayout.BeginVertical(GUILayout.Width(250));
            DrawPropertyPanel();
            EditorGUILayout.EndVertical();

            // ������ �г� - �׸���
            EditorGUILayout.BeginVertical();
            DrawGrid();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            // JSON ��ȯ �г�
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