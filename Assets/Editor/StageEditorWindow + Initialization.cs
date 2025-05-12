// 2. StageEditorWindow.Initialization.cs - �ʱ�ȭ ���� �޼���
using UnityEditor;
using UnityEngine;
using Project.Scripts.Model;
using System.Collections.Generic;
using System;
using Project.Scripts.Controller;

namespace Project.Scripts.Editor
{
    public partial class StageEditorWindow
    {
        #region �ʱ�ȭ

        private void InitializeTextures()
        {
            // �ؽ�ó ���� ����
            gridTexture = new Texture2D(1, 1);
            gridTexture.SetPixel(0, 0, new Color(0.2f, 0.2f, 0.2f, 1f));
            gridTexture.Apply();

            cellTexture = new Texture2D(1, 1);
            cellTexture.SetPixel(0, 0, new Color(0.3f, 0.3f, 0.3f, 1f));
            cellTexture.Apply();

            // ���� �ؽ�ó ����
            colorTextures = new Texture2D[Enum.GetValues(typeof(ColorType)).Length];
            for (int i = 0; i < colorTextures.Length; i++)
            {
                colorTextures[i] = new Texture2D(1, 1);

                Color color = Color.white;
                switch ((ColorType)i)
                {
                    case ColorType.None: color = new Color(0.5f, 0.5f, 0.5f, 0.5f); break;
                    case ColorType.Red: color = Color.red; break;
                    case ColorType.Orange: color = new Color(1f, 0.5f, 0f, 1f); break;
                    case ColorType.Yellow: color = Color.yellow; break;
                    case ColorType.Gray: color = Color.gray; break;
                    case ColorType.Purple: color = new Color(0.5f, 0f, 0.5f, 1f); break;
                    case ColorType.Beige: color = new Color(0.96f, 0.96f, 0.86f, 1f); break;
                    case ColorType.Blue: color = Color.blue; break;
                    case ColorType.Green: color = Color.green; break;
                }

                colorTextures[i].SetPixel(0, 0, color);
                colorTextures[i].Apply();
            }

            // �� �ؽ�ó
            wallTexture = new Texture2D(1, 1);
            wallTexture.SetPixel(0, 0, new Color(0.7f, 0.7f, 0.7f, 1f));
            wallTexture.Apply();

            // ��� �ؽ�ó
            gimmickTexture = new Texture2D(1, 1);
            gimmickTexture.SetPixel(0, 0, new Color(1f, 0.84f, 0f, 1f));
            gimmickTexture.Apply();

            // �÷��� ��� ���� �ؽ�ó ����
            playingBlockPatternTexture = new Texture2D(2, 2);
            playingBlockPatternTexture.SetPixel(0, 0, new Color(1f, 1f, 1f, 0.2f));
            playingBlockPatternTexture.SetPixel(1, 1, new Color(1f, 1f, 1f, 0.2f));
            playingBlockPatternTexture.SetPixel(0, 1, new Color(1f, 1f, 1f, 0.0f));
            playingBlockPatternTexture.SetPixel(1, 0, new Color(1f, 1f, 1f, 0.0f));
            playingBlockPatternTexture.wrapMode = TextureWrapMode.Repeat;
            playingBlockPatternTexture.Apply();
        }

        private void LoadOrCreateStage()
        {
            // ���� ���������� �ε��ϰų� ���� ����
            currentStage = CreateInstance<StageData>();
            currentStage.stageIndex = 1;
            currentStage.boardBlocks = new List<BoardBlockData>();
            currentStage.playingBlocks = new List<PlayingBlockData>();
            currentStage.walls = new List<WallData>();

            // ����, ���, �� ����Ʈ �ʱ�ȭ
            boardBlocks = currentStage.boardBlocks;
            playingBlocks = currentStage.playingBlocks;
            walls = currentStage.walls;
        }

        #endregion
    }
}