// 3. StageEditorWindow.Drawing.cs - UI �׸��� ���� �޼���
using UnityEditor;
using UnityEngine;
using Project.Scripts.Model;
using System.Collections.Generic;
using Project.Scripts.Controller;

namespace Project.Scripts.Editor
{
    public partial class StageEditorWindow
    {
        #region �׸��� �޼���

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            // ���� �޴�
            if (GUILayout.Button("�� ��������", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                if (EditorUtility.DisplayDialog("�� ��������", "�� ���������� ����ðڽ��ϱ�? ������� ���� ��������� ������ϴ�.", "Ȯ��", "���"))
                {
                    CreateNewStage();
                }
            }

            if (GUILayout.Button("�ҷ�����", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                LoadStage();
            }

            if (GUILayout.Button("����", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                SaveStage();
            }

            // JSON ��ȯ ��ư
            if (GUILayout.Button("JSON ��ȯ", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                showJsonPanel = !showJsonPanel;
            }

            GUILayout.FlexibleSpace();

            // ���� ���� (����Ű ��Ʈ �߰�)
            string[] toolLabels = new string[] { "1: ���� ���", "2: �÷��� ���", "3: ��", "4: ���", "5: �����" };

            // ���� ��Ÿ�� ���� �� ����
            GUIStyle toolbarStyle = new GUIStyle(EditorStyles.toolbarButton);
            

            // ���� ����
            Color origBgColor = GUI.backgroundColor;

            // Ŀ���� ���� �׸���
            int newToolIndex = GUILayout.Toolbar(
                (int)currentTool,
                toolLabels,
                toolbarStyle,
                GUILayout.Width(450)
            );

            // ���� ���� ����
            if (newToolIndex != (int)currentTool)
            {
                currentTool = (EditTool)newToolIndex;
                GUI.FocusControl(null); // ��Ŀ�� �ʱ�ȭ
            }

            // ���� ����
            GUI.backgroundColor = origBgColor;

            GUILayout.FlexibleSpace();

            // �׸��� ũ�� ����
            EditorGUILayout.LabelField("�׸��� ũ��:", GUILayout.Width(60));

            if (GUILayout.Button("+", EditorStyles.toolbarButton, GUILayout.Width(20)))
            {
                gridSize = Mathf.Min(gridSize + 5, 80);
            }

            if (GUILayout.Button("-", EditorStyles.toolbarButton, GUILayout.Width(20)))
            {
                gridSize = Mathf.Max(gridSize - 5, 30);
            }

            // �̸����� ���
            GUI.changed = false;
            bool newPreviewMode = GUILayout.Toggle(previewMode, "�̸�����", EditorStyles.toolbarButton, GUILayout.Width(80));
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

            // �׸��� ���
            GUI.DrawTexture(gridViewRect, gridTexture);

            // ��ũ�� ���� ����
            scrollPosition = GUI.BeginScrollView(gridViewRect, scrollPosition,
                new Rect(0, 0, gridWidth * gridSize, gridHeight * gridSize));

            // �׸��� �� �׸���
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    Rect cellRect = new Rect(x * gridSize, y * gridSize, gridSize - 1, gridSize - 1);
                    GUI.DrawTexture(cellRect, cellTexture);

                    // �� �׸���
                    DrawwallsOnGrid(x, y, cellRect);

                    // ���� ��� �׸���
                    DrawBoardBlocksOnGrid(x, y, cellRect);

                    // �÷��� ��� �׸���
                    DrawPlayingBlocksOnGrid(x, y, cellRect);
                }
            }

            // ���콺 ȣ�� �� ���� �� ȿ��
            DrawHoverAndSelectionEffects();

            // ���� ��� �簢�� �巡�� ���� ǥ��
            if (boardDragStart.HasValue && currentTool == EditTool.BoardBlock &&
                boardBlockMode == BoardBlockMode.Rectangle && isDraggingBoardBlock)
            {
                // �巡�� ���� ���
                int startX = Mathf.Min(boardDragStart.Value.x, currentMouseGridPos.x);
                int startY = Mathf.Min(boardDragStart.Value.y, currentMouseGridPos.y);
                int endX = Mathf.Max(boardDragStart.Value.x, currentMouseGridPos.x);
                int endY = Mathf.Max(boardDragStart.Value.y, currentMouseGridPos.y);

                // �ð��� ȿ�� (��� �� �� �׵θ� �׸���)
                for (int x = startX; x <= endX; x++)
                {
                    for (int y = startY; y <= endY; y++)
                    {
                        if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
                        {
                            Rect cellRect = new Rect(x * gridSize, y * gridSize, gridSize - 1, gridSize - 1);

                            // ��� ���� (���õ� ���� ������)
                            Color prevColor = GUI.color;
                            Color selectedBlockColor = GetColorFromType(selectedColor);
                            GUI.color = new Color(selectedBlockColor.r, selectedBlockColor.g, selectedBlockColor.b, 0.3f);
                            GUI.DrawTexture(cellRect, EditorGUIUtility.whiteTexture);

                            // �׵θ� (�� ���� ����)
                            if (x == startX || x == endX || y == startY || y == endY)
                            {
                                GUI.color = new Color(selectedBlockColor.r, selectedBlockColor.g, selectedBlockColor.b, 0.8f);
                                float borderWidth = 2f;

                                // ��� �׵θ�
                                if (y == startY)
                                    GUI.DrawTexture(new Rect(cellRect.x, cellRect.y, cellRect.width, borderWidth), EditorGUIUtility.whiteTexture);

                                // ���� �׵θ�
                                if (x == startX)
                                    GUI.DrawTexture(new Rect(cellRect.x, cellRect.y, borderWidth, cellRect.height), EditorGUIUtility.whiteTexture);

                                // �ϴ� �׵θ�
                                if (y == endY)
                                    GUI.DrawTexture(new Rect(cellRect.x, cellRect.y + cellRect.height - borderWidth, cellRect.width, borderWidth), EditorGUIUtility.whiteTexture);

                                // ������ �׵θ�
                                if (x == endX)
                                    GUI.DrawTexture(new Rect(cellRect.x + cellRect.width - borderWidth, cellRect.y, borderWidth, cellRect.height), EditorGUIUtility.whiteTexture);
                            }

                            GUI.color = prevColor;
                        }
                    }
                }
            }

            // ���� ��� ���� ��� ��忡�� ���õ� �� ǥ��
            if (currentTool == EditTool.BoardBlock && boardBlockMode == BoardBlockMode.FreeForm)
            {
                foreach (var cell in selectedBoardCells)
                {
                    if (cell.x >= 0 && cell.x < gridWidth && cell.y >= 0 && cell.y < gridHeight)
                    {
                        Rect cellRect = new Rect(
                            cell.x * gridSize,
                            cell.y * gridSize,
                            gridSize - 1,
                            gridSize - 1
                        );

                        // ���õ� �� ǥ��
                        Color prevColor = GUI.color;

                        // ���õ� ���� ��� ǥ��
                        Color selectedBlockColor = GetColorFromType(selectedColor);

                        // ��� (������)
                        GUI.color = new Color(selectedBlockColor.r, selectedBlockColor.g, selectedBlockColor.b, 0.3f);
                        GUI.DrawTexture(cellRect, EditorGUIUtility.whiteTexture);

                        // �׵θ� (�� ���� ����)
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

            // ���� �巡�� ���� ǥ�� - �÷��� ��� �簢�� ����� ��
            if (startDragPosition.HasValue && currentTool == EditTool.PlayingBlock && playingBlockMode == PlayingBlockMode.Rectangle)
            {
                // �巡�� ���� ��� - ����� ���콺 ��ġ ���
                int startX = Mathf.Min(startDragPosition.Value.x, currentMouseGridPos.x);
                int startY = Mathf.Min(startDragPosition.Value.y, currentMouseGridPos.y);
                int endX = Mathf.Max(startDragPosition.Value.x, currentMouseGridPos.x);
                int endY = Mathf.Max(startDragPosition.Value.y, currentMouseGridPos.y);

                // �ð��� ȿ�� (��� �� �� �׵θ� �׸���)
                for (int x = startX; x <= endX; x++)
                {
                    for (int y = startY; y <= endY; y++)
                    {
                        if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
                        {
                            Rect cellRect = new Rect(x * gridSize, y * gridSize, gridSize - 1, gridSize - 1);

                            // ��� ���� (�Ķ��� ������)
                            Color prevColor = GUI.color;
                            GUI.color = new Color(0.3f, 0.6f, 1.0f, 0.25f);
                            GUI.DrawTexture(cellRect, EditorGUIUtility.whiteTexture);

                            // �׵θ� (�� ���� �Ķ���)
                            if (x == startX || x == endX || y == startY || y == endY)
                            {
                                GUI.color = new Color(0.3f, 0.6f, 1.0f, 0.8f);
                                float borderWidth = 2f;

                                // ��� �׵θ�
                                if (y == startY)
                                    GUI.DrawTexture(new Rect(cellRect.x, cellRect.y, cellRect.width, borderWidth), EditorGUIUtility.whiteTexture);

                                // ���� �׵θ�
                                if (x == startX)
                                    GUI.DrawTexture(new Rect(cellRect.x, cellRect.y, borderWidth, cellRect.height), EditorGUIUtility.whiteTexture);

                                // �ϴ� �׵θ�
                                if (y == endY)
                                    GUI.DrawTexture(new Rect(cellRect.x, cellRect.y + cellRect.height - borderWidth, cellRect.width, borderWidth), EditorGUIUtility.whiteTexture);

                                // ������ �׵θ�
                                if (x == endX)
                                    GUI.DrawTexture(new Rect(cellRect.x + cellRect.width - borderWidth, cellRect.y, borderWidth, cellRect.height), EditorGUIUtility.whiteTexture);
                            }

                            GUI.color = prevColor;
                        }
                    }
                }
            }

            // �÷��� ��� ���� ��� ��忡�� ���õ� �� ǥ��
            if (currentTool == EditTool.PlayingBlock && playingBlockMode == PlayingBlockMode.FreeForm)
            {
                foreach (var cell in selectedCells)
                {
                    if (cell.x >= 0 && cell.x < gridWidth && cell.y >= 0 && cell.y < gridHeight)
                    {
                        Rect cellRect = new Rect(
                            cell.x * gridSize,
                            cell.y * gridSize,
                            gridSize - 1,
                            gridSize - 1
                        );

                        // ���õ� �� ǥ��
                        Color prevColor = GUI.color;

                        // ��� (������ ���)
                        GUI.color = new Color(0.3f, 0.8f, 0.3f, 0.3f);
                        GUI.DrawTexture(cellRect, EditorGUIUtility.whiteTexture);

                        // �׵θ� (�� ���� ���)
                        GUI.color = new Color(0.3f, 0.8f, 0.3f, 0.8f);
                        float borderWidth = 2f;
                        GUI.DrawTexture(new Rect(cellRect.x, cellRect.y, cellRect.width, borderWidth), EditorGUIUtility.whiteTexture);
                        GUI.DrawTexture(new Rect(cellRect.x, cellRect.y, borderWidth, cellRect.height), EditorGUIUtility.whiteTexture);
                        GUI.DrawTexture(new Rect(cellRect.x, cellRect.y + cellRect.height - borderWidth, cellRect.width, borderWidth), EditorGUIUtility.whiteTexture);
                        GUI.DrawTexture(new Rect(cellRect.x + cellRect.width - borderWidth, cellRect.y, borderWidth, cellRect.height), EditorGUIUtility.whiteTexture);

                        // ù ��° ������ ��(�߽�)�� Ư���� ǥ�� �߰�
                        if (selectedCells.Count > 0 && cell == selectedCells[0])
                        {
                            // �߽��� ǥ�� (���� �簢��)
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
                if (block.x == x && block.y == y && block.ColorType != ColorType.None)
                {
                    Color guiColor = GUI.color;
                    GUI.color = GetColorFromType(block.ColorType);
                    GUI.DrawTexture(cellRect, colorTextures[(int)block.ColorType]);
                    GUI.color = guiColor;

                    // ��ǥ ǥ��
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
                    if (blockX == x && blockY == y)
                    {
                        Color guiColor = GUI.color;

                        // �÷��� ��� ���� �׸���
                        GUI.color = GetColorFromType(block.ColorType);
                        GUI.DrawTexture(cellRect, colorTextures[(int)block.ColorType]);

                        // �׵θ� �߰� - �޼��� ȣ�� �߰�
                        DrawPlayingBlockBorder(cellRect);

                        // ���� ���õ� ����̸� ���̶���Ʈ
                        if (blockIndex == currentPlayingBlockIndex)
                        {
                            GUI.color = new Color(1f, 1f, 1f, 0.3f);
                            GUI.DrawTextureWithTexCoords(cellRect, playingBlockPatternTexture,
                                new Rect(0, 0, cellRect.width / 4, cellRect.height / 4)); // �ݺ� ũ�� ����
                        }
                       
                        // ��� �߽� ǥ��
                        if (shape.offset.x == 0 && shape.offset.y == 0)
                        {
                            GUI.DrawTexture(new Rect(cellRect.x + cellRect.width * 0.25f, cellRect.y + cellRect.height * 0.25f,
                                cellRect.width * 0.5f, cellRect.height * 0.5f),
                                EditorGUIUtility.whiteTexture);

                            // �ε��� ǥ��
                            GUI.Label(new Rect(cellRect.x + cellRect.width * 0.4f, cellRect.y + cellRect.height * 0.4f,
                                cellRect.width * 0.2f, cellRect.height * 0.2f),
                                $"{blockIndex}", new GUIStyle() { normal = { textColor = Color.white } });
                        }

                        // ��� ǥ��
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

                                    // ��� Ÿ�� ǥ��
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
            // �÷��� ��� �׵θ� �׸���
            Color prevColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, 0.9f); // ��� �׵θ�

            float borderWidth = 2f; // �β��� �׵θ�
            float inset = 3f;      // �ణ ��������

            // �׵θ� �簢�� (���ʿ� �� ���� �簢��)
            Rect borderRect = new Rect(
                cellRect.x + inset,
                cellRect.y + inset,
                cellRect.width - (inset * 2),
                cellRect.height - (inset * 2)
            );

            // �����¿� �׵θ� �׸���
            GUI.DrawTexture(new Rect(borderRect.x, borderRect.y, borderRect.width, borderWidth), EditorGUIUtility.whiteTexture);
            GUI.DrawTexture(new Rect(borderRect.x, borderRect.y, borderWidth, borderRect.height), EditorGUIUtility.whiteTexture);
            GUI.DrawTexture(new Rect(borderRect.x, borderRect.y + borderRect.height - borderWidth, borderRect.width, borderWidth), EditorGUIUtility.whiteTexture);
            GUI.DrawTexture(new Rect(borderRect.x + borderRect.width - borderWidth, borderRect.y, borderWidth, borderRect.height), EditorGUIUtility.whiteTexture);

            GUI.color = prevColor;
        }
        private void DrawwallsOnGrid(int x, int y, Rect cellRect)
        {
            foreach (var wall in walls)
            {
                if (wall.x == x && wall.y == y)
                {
                    // �� ���⿡ ���� �׸���
                    Rect wallRect = cellRect;

                    switch (wall.WallDirection)
                    {
                        case ObjectPropertiesEnum.WallDirection.Single_Up:
                            wallRect = new Rect(cellRect.x, cellRect.y, cellRect.width, cellRect.height * 0.1f);
                            break;
                        case ObjectPropertiesEnum.WallDirection.Single_Down:
                            wallRect = new Rect(cellRect.x, cellRect.y + cellRect.height * 0.9f, cellRect.width, cellRect.height * 0.1f);
                            break;
                        case ObjectPropertiesEnum.WallDirection.Single_Left:
                            wallRect = new Rect(cellRect.x, cellRect.y, cellRect.width * 0.1f, cellRect.height);
                            break;
                        case ObjectPropertiesEnum.WallDirection.Single_Right:
                            wallRect = new Rect(cellRect.x + cellRect.width * 0.9f, cellRect.y, cellRect.width * 0.1f, cellRect.height);
                            break;
                        default:
                            // �ٸ� �� ���⿡ ���� ó��
                            break;
                    }

                    Color guiColor = GUI.color;

                    // �� ����
                    if (wall.wallColor != ColorType.None)
                    {
                        GUI.color = GetColorFromType(wall.wallColor);
                    }
                    else
                    {
                        GUI.color = Color.gray;
                    }

                    GUI.DrawTexture(wallRect, wallTexture);

                    // ���� ǥ��
                    if (wall.Length > 1)
                    {
                        GUI.Label(new Rect(cellRect.x + 2, cellRect.y + 2, 30, 20),
                            $"L{wall.Length}", new GUIStyle() { normal = { textColor = Color.white } });
                    }

                    GUI.color = guiColor;
                }
            }
        }

        private void DrawHoverAndSelectionEffects()
        {
            // ���콺 ��ġ�� �׸��� ��ǥ�� ��ȯ
            Vector2 mousePos = Event.current.mousePosition;
            mousePos += scrollPosition;

            int gridX = Mathf.FloorToInt(mousePos.x / gridSize);
            int gridY = Mathf.FloorToInt(mousePos.y / gridSize);

            // �׸��� ���� üũ
            if (gridX >= 0 && gridX < gridWidth && gridY >= 0 && gridY < gridHeight)
            {
                // ���콺 ȣ�� ȿ��
                Rect hoverRect = new Rect(gridX * gridSize, gridY * gridSize, gridSize - 1, gridSize - 1);

                Color prevColor = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, 0.2f);
                GUI.DrawTexture(hoverRect, EditorGUIUtility.whiteTexture);
                GUI.color = prevColor;

                // ���� ��Ʈ ǥ��
                string hint = "";
                switch (currentTool)
                {
                    case EditTool.BoardBlock:
                        hint = $"���� ��� ({selectedColor})";
                        break;
                    case EditTool.PlayingBlock:
                        hint = $"�÷��� ��� ({selectedColor})";
                        break;
                    case EditTool.Wall:
                        hint = $"�� (����: {GetWallDirectionName(wallDirection)}, ����: {wallLength})";
                        break;
                    case EditTool.Gimmick:
                        hint = $"��� ({selectedGimmick})";
                        break;
                    case EditTool.Erase:
                        hint = "�����";
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

            EditorGUILayout.LabelField("JSON ��ȯ", EditorStyles.boldLabel);

            // JSON ���� ���
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("���� ���:", GUILayout.Width(100));
            jsonFilePath = EditorGUILayout.TextField(jsonFilePath);

            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                string selectedPath = EditorUtility.SaveFilePanel("JSON ���� ���", "", "Stage", "json");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    jsonFilePath = selectedPath;
                }
            }
            EditorGUILayout.EndHorizontal();

            // ��������/�������� ��ư
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("JSON���� ��������"))
            {
                ExportToJson();
            }

            if (GUILayout.Button("JSON���� ��������"))
            {
                ImportFromJson();
            }

            EditorGUILayout.EndHorizontal();

            // ScriptableObject ��������/��������
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("ScriptableObject ��ȯ", EditorStyles.boldLabel);

            if (GUILayout.Button("ScriptableObject�� ����"))
            {
                SaveStage();
            }

            if (GUILayout.Button("ScriptableObject���� �ҷ�����"))
            {
                LoadStage();
            }

            // �ݱ� ��ư
            if (GUILayout.Button("�ݱ�"))
            {
                showJsonPanel = false;
            }

            EditorGUILayout.EndVertical();
        }

        #endregion
    }
}