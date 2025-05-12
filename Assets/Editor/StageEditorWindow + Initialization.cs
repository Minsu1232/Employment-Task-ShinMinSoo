// 2. StageEditorWindow.Initialization.cs - 초기화 관련 메서드
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
        #region 초기화

        private void InitializeTextures()
        {
            // 텍스처 생성 로직
            gridTexture = new Texture2D(1, 1);
            gridTexture.SetPixel(0, 0, new Color(0.2f, 0.2f, 0.2f, 1f));
            gridTexture.Apply();

            cellTexture = new Texture2D(1, 1);
            cellTexture.SetPixel(0, 0, new Color(0.3f, 0.3f, 0.3f, 1f));
            cellTexture.Apply();

            // 색상 텍스처 생성
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

            // 벽 텍스처
            wallTexture = new Texture2D(1, 1);
            wallTexture.SetPixel(0, 0, new Color(0.7f, 0.7f, 0.7f, 1f));
            wallTexture.Apply();

            // 기믹 텍스처
            gimmickTexture = new Texture2D(1, 1);
            gimmickTexture.SetPixel(0, 0, new Color(1f, 0.84f, 0f, 1f));
            gimmickTexture.Apply();

            // 플레이 블록 패턴 텍스처 생성
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
            // 현재 스테이지를 로드하거나 새로 생성
            currentStage = CreateInstance<StageData>();
            currentStage.stageIndex = 1;
            currentStage.boardBlocks = new List<BoardBlockData>();
            currentStage.playingBlocks = new List<PlayingBlockData>();
            currentStage.walls = new List<WallData>();

            // 보드, 블록, 벽 리스트 초기화
            boardBlocks = currentStage.boardBlocks;
            playingBlocks = currentStage.playingBlocks;
            walls = currentStage.walls;
        }

        #endregion
    }
}