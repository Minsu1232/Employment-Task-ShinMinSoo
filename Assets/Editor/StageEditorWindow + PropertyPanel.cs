// 4. StageEditorWindow.PropertyPanel.cs - �Ӽ� �г� ���� �޼���
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
        #region �Ӽ� �г�

        private void DrawPropertyPanel()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // �������� ����
            EditorGUILayout.LabelField("�������� ����", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("�������� ��ȣ:", GUILayout.Width(100));
            currentStage.stageIndex = EditorGUILayout.IntField(currentStage.stageIndex, GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("�׸��� ũ��:", GUILayout.Width(100));
            EditorGUILayout.LabelField($"{gridWidth}x{gridHeight}", GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("�׸��� ũ�� ����"))
            {
                // �׸��� ũ�� ���� ���̾�α�
                ChangeGridSize();
            }

            EditorGUILayout.Space(10);

            // ������ �Ӽ� �г�
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
            EditorGUILayout.LabelField("���� ��� �Ӽ�", EditorStyles.boldLabel);

            // ���� ����
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("����:", GUILayout.Width(100));
            selectedColor = (ColorType)EditorGUILayout.EnumPopup(selectedColor, GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();

            // ��� ���� UI �߰�
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("���� ���:", GUILayout.Width(100));

            EditorGUILayout.BeginHorizontal();
            bool newRectMode = GUILayout.Toggle(boardBlockMode == BoardBlockMode.Rectangle, "�簢��", EditorStyles.radioButton);
            bool newFreeFormMode = GUILayout.Toggle(boardBlockMode == BoardBlockMode.FreeForm, "���� ���", EditorStyles.radioButton);
            EditorGUILayout.EndHorizontal();

            // ��� ���� ���� �� ó��
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

            // ��庰 ���� �߰�
            if (boardBlockMode == BoardBlockMode.Rectangle)
            {
                EditorGUILayout.HelpBox("���� ����� ��ġ�Ϸ��� �׸��带 Ŭ���ϰų� �巡���ϼ���.", MessageType.Info);
            }
            else // FreeForm ���
            {
                EditorGUILayout.HelpBox("Ŭ���ϰų� �巡���Ͽ� ���� �����ϼ���. Shift+Ŭ������ ������ ����� �� �ֽ��ϴ�.", MessageType.Info);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField($"���õ� ��: {selectedBoardCells.Count}��");

                EditorGUILayout.BeginHorizontal();
                GUI.enabled = selectedBoardCells.Count > 0;
                if (GUILayout.Button("��� ����", GUILayout.Height(24)))
                {
                    CreateFreeFormBoardBlocks();
                }

                if (GUILayout.Button("���� �ʱ�ȭ", GUILayout.Height(24)))
                {
                    selectedBoardCells.Clear();
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space(10);

            // ���� ��� ���
            EditorGUILayout.LabelField("���� ��� ���", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"�� ���� ��� ��: {boardBlocks.Count}");

            // ���� ���
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
                EditorGUILayout.LabelField($"{colorCount.Value}��", GUILayout.Width(100));
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawPlayingBlockProperties()
        {
            EditorGUILayout.LabelField("�÷��� ��� �Ӽ�", EditorStyles.boldLabel);

            // ���� ����
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("����:", GUILayout.Width(100));
            selectedColor = (ColorType)EditorGUILayout.EnumPopup(selectedColor, GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();

            // ��� ���� UI �߰�
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("���� ���:", GUILayout.Width(100));

            EditorGUILayout.BeginHorizontal();
            bool newRectMode = GUILayout.Toggle(playingBlockMode == PlayingBlockMode.Rectangle, "1: �簢��", EditorStyles.radioButton);
            bool newFreeFormMode = GUILayout.Toggle(playingBlockMode == PlayingBlockMode.FreeForm, "2: ���� ���", EditorStyles.radioButton);
            EditorGUILayout.EndHorizontal();

            // ��� ���� ���� �� ó��
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

            // ��庰 ���� �߰�
            if (playingBlockMode == PlayingBlockMode.Rectangle)
            {
                EditorGUILayout.HelpBox("�÷��� ����� ������� �׸��带 �巡���Ͽ� ������ �����ϼ���.", MessageType.Info);
            }
            else // FreeForm ���
            {
                EditorGUILayout.HelpBox("���� ���� Ŭ���Ͽ� �����ϰ� Shift+Ŭ������ ���� ���� ������ �� �ֽ��ϴ�.", MessageType.Info);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField($"���õ� ��: {selectedCells.Count}��");

                EditorGUILayout.BeginHorizontal();
                GUI.enabled = selectedCells.Count > 0;
                if (GUILayout.Button("��� ����", GUILayout.Height(24)))
                {
                    CreateFreeFormPlayingBlock();
                }

                if (GUILayout.Button("���� �ʱ�ȭ", GUILayout.Height(24)))
                {
                    selectedCells.Clear();
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space(10);

            // ���� ���õ� �÷��� ��� ����
            if (currentPlayingBlockIndex >= 0 && currentPlayingBlockIndex < playingBlocks.Count)
            {
                var selectedBlock = playingBlocks[currentPlayingBlockIndex];
                EditorGUILayout.LabelField("���õ� ��� ����", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("�ε���:", GUILayout.Width(100));
                EditorGUILayout.LabelField($"{currentPlayingBlockIndex}", GUILayout.Width(100));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("�߽� ��ġ:", GUILayout.Width(100));
                EditorGUILayout.LabelField($"({selectedBlock.center.x}, {selectedBlock.center.y})", GUILayout.Width(100));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("����:", GUILayout.Width(100));
                selectedBlock.ColorType = (ColorType)EditorGUILayout.EnumPopup(selectedBlock.ColorType, GUILayout.Width(100));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("��� ��:", GUILayout.Width(100));
                EditorGUILayout.LabelField($"{selectedBlock.shapes.Count}��", GUILayout.Width(100));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("��� ��:", GUILayout.Width(100));
                EditorGUILayout.LabelField($"{selectedBlock.gimmicks.Count}��", GUILayout.Width(100));
                EditorGUILayout.EndHorizontal();

                // ���� �ɼ� (Ȯ��)
                EditorGUILayout.Space(5);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("��� �۾�:", EditorStyles.boldLabel);

                // ��� �̵� ��ư (�����¿�)
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("��", GUILayout.Width(40), GUILayout.Height(24)))
                {
                    MoveSelectedBlock(0, 1); // ���� �̵�
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("��", GUILayout.Width(40), GUILayout.Height(24)))
                {
                    MoveSelectedBlock(-1, 0); // �������� �̵�
                }

                if (GUILayout.Button("��", GUILayout.Width(40), GUILayout.Height(24)))
                {
                    MoveSelectedBlock(1, 0); // ���������� �̵�
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("��", GUILayout.Width(40), GUILayout.Height(24)))
                {
                    MoveSelectedBlock(0, -1); // �Ʒ��� �̵�
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                // ȸ�� ��ư
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("�������� ȸ��", GUILayout.Height(24)))
                {
                    RotateSelectedBlock(false); // ���� ȸ��
                }

                if (GUILayout.Button("���������� ȸ��", GUILayout.Height(24)))
                {
                    RotateSelectedBlock(true); // ������ ȸ��
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(5);

                // ��� ���� ��ư
                if (GUILayout.Button("�� ��� ����", GUILayout.Height(24)))
                {
                    if (EditorUtility.DisplayDialog("��� ����", "������ �� ����� �����Ͻðڽ��ϱ�?", "Ȯ��", "���"))
                    {
                        playingBlocks.RemoveAt(currentPlayingBlockIndex);
                        currentPlayingBlockIndex = -1;
                    }
                }
                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.HelpBox("����� �����Ϸ��� �׸��忡�� ����� Ŭ���ϼ���.", MessageType.Info);
            }

            EditorGUILayout.Space(10);

            // �÷��� ��� ���
            EditorGUILayout.LabelField("�÷��� ��� ���", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"�� �÷��� ��� ��: {playingBlocks.Count}");
        }

        private void DrawWallProperties()
        {
            EditorGUILayout.LabelField("�� �Ӽ�", EditorStyles.boldLabel);

            // �� ����
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("����:", GUILayout.Width(100));
            wallDirection = (ObjectPropertiesEnum.WallDirection)EditorGUILayout.EnumPopup(wallDirection, GUILayout.Width(130));
            EditorGUILayout.EndHorizontal();

            // �� ����
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("����:", GUILayout.Width(100));
            wallColor = (ColorType)EditorGUILayout.EnumPopup(wallColor, GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();

            // �� ����
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("����:", GUILayout.Width(100));
            wallLength = EditorGUILayout.IntField(wallLength, GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();

            wallLength = Mathf.Clamp(wallLength, 1, 10);

            EditorGUILayout.HelpBox("���� ��ġ�Ϸ��� �׸��带 Ŭ���ϼ���.", MessageType.Info);

            EditorGUILayout.Space(10);

            // ���� ���õ� �� ���� (�÷��� ��ϰ� ������ ���)
            if (currentWallIndex >= 0 && currentWallIndex < walls.Count)
            {
                var selectedWall = walls[currentWallIndex];
                EditorGUILayout.LabelField("���õ� �� ����", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("�ε���:", GUILayout.Width(100));
                EditorGUILayout.LabelField($"{currentWallIndex}", GUILayout.Width(100));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("��ġ:", GUILayout.Width(100));
                EditorGUILayout.LabelField($"({selectedWall.x}, {selectedWall.y})", GUILayout.Width(100));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("����:", GUILayout.Width(100));
                selectedWall.WallDirection = (ObjectPropertiesEnum.WallDirection)EditorGUILayout.EnumPopup(selectedWall.WallDirection, GUILayout.Width(130));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("����:", GUILayout.Width(100));
                selectedWall.wallColor = (ColorType)EditorGUILayout.EnumPopup(selectedWall.wallColor, GUILayout.Width(100));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("����:", GUILayout.Width(100));
                selectedWall.Length = EditorGUILayout.IntField(selectedWall.Length, GUILayout.Width(100));
                EditorGUILayout.EndHorizontal();

                // �� ���� ��, ���� ���� ��ư �߰�
                if (GUILayout.Button("���� ����", GUILayout.Height(24)))
                {
                    currentWallIndex = -1;
                }

                // �� ���� ��ư
                if (GUILayout.Button("�� �� ����", GUILayout.Height(24)))
                {
                    if (EditorUtility.DisplayDialog("�� ����", "������ �� ���� �����Ͻðڽ��ϱ�?", "Ȯ��", "���"))
                    {
                        walls.RemoveAt(currentWallIndex);
                        currentWallIndex = -1;
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("���� �����Ϸ��� �׸��忡�� ���� Ŭ���ϼ���.", MessageType.Info);
            }

            EditorGUILayout.Space(10);

            // �� ���
            EditorGUILayout.LabelField("�� ���", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"�� �� ��: {walls.Count}");

            // ���⺰ ���
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
                EditorGUILayout.LabelField($"{directionCount.Value}��", GUILayout.Width(100));
                EditorGUILayout.EndHorizontal();
            }

            // ��ͺ� ��� �߰�
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("�� ��� ���:", EditorStyles.boldLabel);

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
                EditorGUILayout.LabelField("������ ����� �����ϴ�.");
            }
            else
            {
                foreach (var gimmickCount in gimmickCounts)
                {
                    if (gimmickCount.Key != WallGimmickType.None)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField($"{gimmickCount.Key}:", GUILayout.Width(100));
                        EditorGUILayout.LabelField($"{gimmickCount.Value}��", GUILayout.Width(100));
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
        }

        private void DrawGimmickProperties()
        {
            EditorGUILayout.LabelField("��� �Ӽ�", EditorStyles.boldLabel);

            // ��� Ÿ��
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Ÿ��:", GUILayout.Width(100));
            selectedGimmick = EditorGUILayout.TextField(selectedGimmick, GUILayout.Width(150));
            EditorGUILayout.EndHorizontal();

            // �⺻ ��� Ÿ�� ��ư
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

            EditorGUILayout.HelpBox("����� �߰��Ϸ��� �÷��� ����� Ŭ���ϼ���.", MessageType.Info);

            EditorGUILayout.Space(10);

            // ��� ���
            EditorGUILayout.LabelField("��� ���", EditorStyles.boldLabel);

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
                EditorGUILayout.LabelField("����� �����ϴ�.");
            }
            else
            {
                foreach (var gimmickCount in gimmickCounts)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"{gimmickCount.Key}:", GUILayout.Width(100));
                    EditorGUILayout.LabelField($"{gimmickCount.Value}��", GUILayout.Width(100));
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        private void DrawEraseProperties()
        {
            EditorGUILayout.LabelField("����� ����", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("��ü�� ������� �׸��忡�� Ŭ���ϼ���.", MessageType.Info);

            // �ϰ� ���� ��ư
            if (GUILayout.Button("��� ���� ��� �����"))
            {
                if (EditorUtility.DisplayDialog("��� ���� ��� �����", "������ ��� ���� ����� ����ðڽ��ϱ�?", "Ȯ��", "���"))
                {
                    boardBlocks.Clear();
                }
            }

            if (GUILayout.Button("��� �÷��� ��� �����"))
            {
                if (EditorUtility.DisplayDialog("��� �÷��� ��� �����", "������ ��� �÷��� ����� ����ðڽ��ϱ�?", "Ȯ��", "���"))
                {
                    playingBlocks.Clear();
                    currentPlayingBlockIndex = -1;
                }
            }

            if (GUILayout.Button("��� �� �����"))
            {
                if (EditorUtility.DisplayDialog("��� �� �����", "������ ��� ���� ����ðڽ��ϱ�?", "Ȯ��", "���"))
                {
                    walls.Clear();
                }
            }

            if (GUILayout.Button("��� �����"))
            {
                if (EditorUtility.DisplayDialog("��� �����", "������ ��� ��ü�� ����ðڽ��ϱ�?", "Ȯ��", "���"))
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