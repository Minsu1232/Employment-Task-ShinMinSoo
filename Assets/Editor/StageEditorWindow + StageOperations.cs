// 6. StageEditorWindow.StageOperations.cs - �������� �۾� ���� �޼���
using UnityEditor;
using UnityEngine;
using Project.Scripts.Model;
using System.Collections.Generic;
using System.IO;
using System;
using Project.Scripts.Controller;

namespace Project.Scripts.Editor
{
    public partial class StageEditorWindow
    {
        #region �������� �۾�

        private void CreateNewStage()
        {
            currentStage = CreateInstance<StageData>();
            currentStage.stageIndex = 1;
            currentStage.boardBlocks = new List<BoardBlockData>();
            currentStage.playingBlocks = new List<PlayingBlockData>();
            currentStage.walls = new List<WallData>();

            boardBlocks = currentStage.boardBlocks;
            playingBlocks = currentStage.playingBlocks;
            walls = currentStage.walls;

            currentPlayingBlockIndex = -1;
        }

        private void LoadStage()
        {
            string path = EditorUtility.OpenFilePanel("�������� �ҷ�����", Application.dataPath, "asset");
            if (!string.IsNullOrEmpty(path))
            {
                // ��� ��η� ��ȯ
                path = "Assets" + path.Substring(Application.dataPath.Length);

                StageData loadedStage = AssetDatabase.LoadAssetAtPath<StageData>(path);

                if (loadedStage != null)
                {
                    currentStage = loadedStage;

                    // ������ ����
                    boardBlocks = new List<BoardBlockData>(currentStage.boardBlocks);
                    playingBlocks = new List<PlayingBlockData>(currentStage.playingBlocks);
                    walls = new List<WallData>(currentStage.walls);

                    // ���� ���������� ����
                    currentStage.boardBlocks = boardBlocks;
                    currentStage.playingBlocks = playingBlocks;
                    currentStage.walls = walls;

                    currentPlayingBlockIndex = -1;

                    Debug.Log("���������� ���������� �ҷ��Խ��ϴ�!");
                }
                else
                {
                    EditorUtility.DisplayDialog("����", "������ ������ �ҷ��� �� �����ϴ�.", "Ȯ��");
                }
            }
        }

        private void SaveStage()
        {
            string path = EditorUtility.SaveFilePanel("�������� ����", Application.dataPath, $"Stage_{currentStage.stageIndex}", "asset");
            if (!string.IsNullOrEmpty(path))
            {
                // ��� ��η� ��ȯ
                path = "Assets" + path.Substring(Application.dataPath.Length);

                // ���� �������� ������ ������Ʈ
                currentStage.boardBlocks = boardBlocks;
                currentStage.playingBlocks = playingBlocks;
                currentStage.walls = walls;

                // �̹� �����ϴ� �������� Ȯ��
                StageData existingAsset = AssetDatabase.LoadAssetAtPath<StageData>(path);

                if (existingAsset != null)
                {
                    // ���� ���� ������Ʈ
                    EditorUtility.CopySerialized(currentStage, existingAsset);
                    AssetDatabase.SaveAssets();
                }
                else
                {
                    // ���ο� ���� ����
                    AssetDatabase.CreateAsset(currentStage, path);
                    AssetDatabase.SaveAssets();
                }

                AssetDatabase.Refresh();

                EditorUtility.DisplayDialog("���� �Ϸ�", "���������� ���������� ����Ǿ����ϴ�.", "Ȯ��");
            }
        }

        private void SaveCurrentStage()
        {
            // �ڵ� ���� (�ʿ��ϸ� ����)
        }

        private void ChangeGridSize()
        {
            // �׸��� ũ�� ���� ���̾�α�
            // ���⼭�� �����ϰ� ������ ������ ����
            gridWidth = EditorUtility.DisplayDialogComplex("�׸��� ũ�� ����",
                "�׸��� ũ�⸦ �����Ͻðڽ��ϱ�?",
                "10x10", "15x15", "20x20");

            switch (gridWidth)
            {
                case 0: // 10x10
                    gridWidth = 10;
                    gridHeight = 10;
                    break;
                case 1: // 15x15
                    gridWidth = 15;
                    gridHeight = 15;
                    break;
                case 2: // 20x20
                    gridWidth = 20;
                    gridHeight = 20;
                    break;
            }
        }

        #endregion

        #region �̸����� ���

        private void StartPreview()
        {
            // �̸����� ��� ����
            // ���� ����
            EditorUtility.DisplayDialog("�̸����� ���", "�̸����� ���� ���� �������� �ʾҽ��ϴ�.", "Ȯ��");
            previewMode = false;
        }

        private void StopPreview()
        {
            // �̸����� ��� ����
            // ���� ����
        }

        #endregion

        #region JSON ��ȯ

        private void ExportToJson()
        {
            if (string.IsNullOrEmpty(jsonFilePath))
            {
                EditorUtility.DisplayDialog("����", "���� ��θ� �Է����ּ���.", "Ȯ��");
                return;
            }

            // ���� �������� ������ ������Ʈ
            currentStage.boardBlocks = boardBlocks;
            currentStage.playingBlocks = playingBlocks;
            currentStage.walls = walls;

            // JSON ��ȯ
            StageJsonData jsonData = new StageJsonData
            {
                stageIndex = currentStage.stageIndex,
                boardBlocks = currentStage.boardBlocks,
                playingBlocks = currentStage.playingBlocks,
                walls = currentStage.walls
            };

            StageJsonWrapper wrapper = new StageJsonWrapper
            {
                Stage = jsonData
            };

            string json = JsonUtility.ToJson(wrapper, true);

            // ���� ����
            try
            {
                File.WriteAllText(jsonFilePath, json);
                EditorUtility.DisplayDialog("�������� �Ϸ�", $"JSON ������ ���������� �����߽��ϴ�:\n{jsonFilePath}", "Ȯ��");
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("����", $"���� ���� �� ������ �߻��߽��ϴ�:\n{e.Message}", "Ȯ��");
            }
        }

        private void ImportFromJson()
        {
            if (string.IsNullOrEmpty(jsonFilePath) || !File.Exists(jsonFilePath))
            {
                string path = EditorUtility.OpenFilePanel("JSON ���� ����", "", "json");
                if (string.IsNullOrEmpty(path))
                {
                    return;
                }

                jsonFilePath = path;
            }

            try
            {
                // JSON ���� �б�
                string json = File.ReadAllText(jsonFilePath);

                // JSON ��ȯ
                StageJsonWrapper wrapper = JsonUtility.FromJson<StageJsonWrapper>(json);

                if (wrapper == null || wrapper.Stage == null)
                {
                    EditorUtility.DisplayDialog("����", "��ȿ���� ���� JSON �����Դϴ�.", "Ȯ��");
                    return;
                }

                // �������� ������ ������Ʈ
                currentStage.stageIndex = wrapper.Stage.stageIndex;

                boardBlocks = wrapper.Stage.boardBlocks ?? new List<BoardBlockData>();
                playingBlocks = wrapper.Stage.playingBlocks ?? new List<PlayingBlockData>();
                walls = wrapper.Stage.walls ?? new List<WallData>();

                // ���� ���������� ������ ����
                currentStage.boardBlocks = boardBlocks;
                currentStage.playingBlocks = playingBlocks;
                currentStage.walls = walls;

                currentPlayingBlockIndex = -1;

                EditorUtility.DisplayDialog("�������� �Ϸ�", "JSON ������ ���������� �����Խ��ϴ�.", "Ȯ��");
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("����", $"���� �б� �� ������ �߻��߽��ϴ�:\n{e.Message}", "Ȯ��");
            }
        }

        #endregion

        #region ��ƿ��Ƽ �޼���

        private Color GetColorFromType(ColorType colorType)
        {
            switch (colorType)
            {
                case ColorType.None: return new Color(0.5f, 0.5f, 0.5f, 0.5f);
                case ColorType.Red: return Color.red;
                case ColorType.Orange: return new Color(1f, 0.5f, 0f, 1f);
                case ColorType.Yellow: return Color.yellow;
                case ColorType.Gray: return Color.gray;
                case ColorType.Purple: return new Color(0.5f, 0f, 0.5f, 1f);
                case ColorType.Beige: return new Color(0.96f, 0.96f, 0.86f, 1f);
                case ColorType.Blue: return Color.blue;
                case ColorType.Green: return Color.green;
                default: return Color.white;
            }
        }

        private string GetWallDirectionName(ObjectPropertiesEnum.WallDirection direction)
        {
            switch (direction)
            {
                case ObjectPropertiesEnum.WallDirection.Single_Up: return "����";
                case ObjectPropertiesEnum.WallDirection.Single_Down: return "�Ʒ���";
                case ObjectPropertiesEnum.WallDirection.Single_Left: return "����";
                case ObjectPropertiesEnum.WallDirection.Single_Right: return "������";
                case ObjectPropertiesEnum.WallDirection.Left_Up: return "����-��";
                case ObjectPropertiesEnum.WallDirection.Left_Down: return "����-�Ʒ�";
                case ObjectPropertiesEnum.WallDirection.Right_Up: return "������-��";
                case ObjectPropertiesEnum.WallDirection.Right_Down: return "������-�Ʒ�";
                case ObjectPropertiesEnum.WallDirection.Open_Up: return "���� ����";
                case ObjectPropertiesEnum.WallDirection.Open_Down: return "�Ʒ��� ����";
                case ObjectPropertiesEnum.WallDirection.Open_Left: return "���� ����";
                case ObjectPropertiesEnum.WallDirection.Open_Right: return "������ ����";
                default: return direction.ToString();
            }
        }

        #endregion
    }
}