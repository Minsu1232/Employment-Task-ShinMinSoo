// 6. StageEditorWindow.StageOperations.cs - 스테이지 작업 관련 메서드
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
        #region 스테이지 작업

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
            string path = EditorUtility.OpenFilePanel("스테이지 불러오기", Application.dataPath, "asset");
            if (!string.IsNullOrEmpty(path))
            {
                // 상대 경로로 변환
                path = "Assets" + path.Substring(Application.dataPath.Length);

                StageData loadedStage = AssetDatabase.LoadAssetAtPath<StageData>(path);

                if (loadedStage != null)
                {
                    currentStage = loadedStage;

                    // 데이터 복사
                    boardBlocks = new List<BoardBlockData>(currentStage.boardBlocks);
                    playingBlocks = new List<PlayingBlockData>(currentStage.playingBlocks);
                    walls = new List<WallData>(currentStage.walls);

                    // 현재 스테이지에 연결
                    currentStage.boardBlocks = boardBlocks;
                    currentStage.playingBlocks = playingBlocks;
                    currentStage.walls = walls;

                    currentPlayingBlockIndex = -1;

                    Debug.Log("스테이지를 성공적으로 불러왔습니다!");
                }
                else
                {
                    EditorUtility.DisplayDialog("오류", "선택한 파일을 불러올 수 없습니다.", "확인");
                }
            }
        }

        private void SaveStage()
        {
            string path = EditorUtility.SaveFilePanel("스테이지 저장", Application.dataPath, $"Stage_{currentStage.stageIndex}", "asset");
            if (!string.IsNullOrEmpty(path))
            {
                // 상대 경로로 변환
                path = "Assets" + path.Substring(Application.dataPath.Length);

                // 현재 스테이지 데이터 업데이트
                currentStage.boardBlocks = boardBlocks;
                currentStage.playingBlocks = playingBlocks;
                currentStage.walls = walls;

                // 이미 존재하는 에셋인지 확인
                StageData existingAsset = AssetDatabase.LoadAssetAtPath<StageData>(path);

                if (existingAsset != null)
                {
                    // 기존 에셋 업데이트
                    EditorUtility.CopySerialized(currentStage, existingAsset);
                    AssetDatabase.SaveAssets();
                }
                else
                {
                    // 새로운 에셋 생성
                    AssetDatabase.CreateAsset(currentStage, path);
                    AssetDatabase.SaveAssets();
                }

                AssetDatabase.Refresh();

                EditorUtility.DisplayDialog("저장 완료", "스테이지가 성공적으로 저장되었습니다.", "확인");
            }
        }

        private void SaveCurrentStage()
        {
            // 자동 저장 (필요하면 구현)
        }

        private void ChangeGridSize()
        {
            // 그리드 크기 변경 다이얼로그
            // 여기서는 간단하게 임의의 값으로 설정
            gridWidth = EditorUtility.DisplayDialogComplex("그리드 크기 변경",
                "그리드 크기를 변경하시겠습니까?",
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

        #region 미리보기 모드

        private void StartPreview()
        {
            // 미리보기 모드 시작
            // 구현 예정
            EditorUtility.DisplayDialog("미리보기 모드", "미리보기 모드는 아직 구현되지 않았습니다.", "확인");
            previewMode = false;
        }

        private void StopPreview()
        {
            // 미리보기 모드 종료
            // 구현 예정
        }

        #endregion

        #region JSON 변환

        private void ExportToJson()
        {
            if (string.IsNullOrEmpty(jsonFilePath))
            {
                EditorUtility.DisplayDialog("오류", "파일 경로를 입력해주세요.", "확인");
                return;
            }

            // 현재 스테이지 데이터 업데이트
            currentStage.boardBlocks = boardBlocks;
            currentStage.playingBlocks = playingBlocks;
            currentStage.walls = walls;

            // JSON 변환
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

            // 파일 저장
            try
            {
                File.WriteAllText(jsonFilePath, json);
                EditorUtility.DisplayDialog("내보내기 완료", $"JSON 파일을 성공적으로 저장했습니다:\n{jsonFilePath}", "확인");
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("오류", $"파일 저장 중 오류가 발생했습니다:\n{e.Message}", "확인");
            }
        }

        private void ImportFromJson()
        {
            if (string.IsNullOrEmpty(jsonFilePath) || !File.Exists(jsonFilePath))
            {
                string path = EditorUtility.OpenFilePanel("JSON 파일 열기", "", "json");
                if (string.IsNullOrEmpty(path))
                {
                    return;
                }

                jsonFilePath = path;
            }

            try
            {
                // JSON 파일 읽기
                string json = File.ReadAllText(jsonFilePath);

                // JSON 변환
                StageJsonWrapper wrapper = JsonUtility.FromJson<StageJsonWrapper>(json);

                if (wrapper == null || wrapper.Stage == null)
                {
                    EditorUtility.DisplayDialog("오류", "유효하지 않은 JSON 형식입니다.", "확인");
                    return;
                }

                // 스테이지 데이터 업데이트
                currentStage.stageIndex = wrapper.Stage.stageIndex;

                boardBlocks = wrapper.Stage.boardBlocks ?? new List<BoardBlockData>();
                playingBlocks = wrapper.Stage.playingBlocks ?? new List<PlayingBlockData>();
                walls = wrapper.Stage.walls ?? new List<WallData>();

                // 현재 스테이지에 데이터 연결
                currentStage.boardBlocks = boardBlocks;
                currentStage.playingBlocks = playingBlocks;
                currentStage.walls = walls;

                currentPlayingBlockIndex = -1;

                EditorUtility.DisplayDialog("가져오기 완료", "JSON 파일을 성공적으로 가져왔습니다.", "확인");
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("오류", $"파일 읽기 중 오류가 발생했습니다:\n{e.Message}", "확인");
            }
        }

        #endregion

        #region 유틸리티 메서드

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
                case ObjectPropertiesEnum.WallDirection.Single_Up: return "위쪽";
                case ObjectPropertiesEnum.WallDirection.Single_Down: return "아래쪽";
                case ObjectPropertiesEnum.WallDirection.Single_Left: return "왼쪽";
                case ObjectPropertiesEnum.WallDirection.Single_Right: return "오른쪽";
                case ObjectPropertiesEnum.WallDirection.Left_Up: return "왼쪽-위";
                case ObjectPropertiesEnum.WallDirection.Left_Down: return "왼쪽-아래";
                case ObjectPropertiesEnum.WallDirection.Right_Up: return "오른쪽-위";
                case ObjectPropertiesEnum.WallDirection.Right_Down: return "오른쪽-아래";
                case ObjectPropertiesEnum.WallDirection.Open_Up: return "위쪽 열림";
                case ObjectPropertiesEnum.WallDirection.Open_Down: return "아래쪽 열림";
                case ObjectPropertiesEnum.WallDirection.Open_Left: return "왼쪽 열림";
                case ObjectPropertiesEnum.WallDirection.Open_Right: return "오른쪽 열림";
                default: return direction.ToString();
            }
        }

        #endregion
    }
}