using System.Threading.Tasks;
using UnityEngine;
using Project.Scripts.Model;

namespace Project.Scripts.Controller
{
    /// <summary>
    /// 게임의 전체 흐름을 제어하는 컨트롤러
    /// </summary>
    public class GameController : MonoBehaviour
    {
        [Header("컨트롤러 참조")]
        [SerializeField] private BoardController boardController;
        [SerializeField] private BlockController blockController;
        [SerializeField] private WallController wallController;
        [SerializeField] private InputController inputController;
        [SerializeField] private LevelController levelController;

        [Header("게임 설정")]
        [SerializeField] private int startLevel = 1;
        [SerializeField] private bool autoStartGame = true;

        private bool isGameInitialized = false;

        private void Awake()
        {
            // 컨트롤러 자동 생성
            if (boardController == null)
                boardController = GetComponentInChildren<BoardController>() ?? gameObject.AddComponent<BoardController>();

            if (blockController == null)
                blockController = GetComponentInChildren<BlockController>() ?? gameObject.AddComponent<BlockController>();

            if (wallController == null)
                wallController = GetComponentInChildren<WallController>() ?? gameObject.AddComponent<WallController>();

            if (inputController == null)
                inputController = GetComponentInChildren<InputController>() ?? gameObject.AddComponent<InputController>();

            if (levelController == null)
                levelController = GetComponentInChildren<LevelController>() ?? gameObject.AddComponent<LevelController>();

            // 컨트롤러 초기화
            boardController.Initialize(this);
            blockController.Initialize(this);
            wallController.Initialize(this);
            inputController.Initialize(this);
            levelController.Initialize(this);

            // 컨트롤러 참조 설정
            inputController.SetBlockController(blockController);
            blockController.SetBoardController(boardController);
        }

        private void Start()
        {
            if (autoStartGame)
            {
                InitializeGame();
            }
        }

        public async void InitializeGame()
        {
            // 데이터 매니저 초기화
            if (DataManager.Instance == null)
            {
                Debug.LogError("DataManager 인스턴스가 없습니다!");
                return;
            }

            // 시작 레벨 로드
            await LoadLevelAsync(startLevel);
            isGameInitialized = true;
        }

        public async Task LoadLevelAsync(int levelIndex)
        {
            // 현재 레벨 정리
            ClearLevel();

            // 레벨 데이터 로드
            Project.Scripts.Model.StageData stageData = await Project.Scripts.Model.DataManager.Instance.LoadStageDataAsync(levelIndex);

            if (stageData != null)
            {
                // 각 컨트롤러 초기화
                await InitializeLevelAsync(stageData);
                levelController.SetCurrentLevel(levelIndex);
            }
            else
            {
                Debug.LogError($"레벨 {levelIndex}를 로드할 수 없습니다.");
            }
        }

        public async Task InitializeLevelAsync(Project.Scripts.Model.StageData stageData)
        {
            // 게임 요소 생성 순서:
            // 1. 벽 (경계) 생성
            // 2. 보드 생성
            // 3. 블록 생성

            // 벽 생성
            await wallController.CreateWallsAsync(stageData.walls);

            // 보드 생성
            await boardController.CreateBoardAsync(stageData.boardBlocks);

            // 블록 생성
            await blockController.CreateBlocksAsync(stageData.playingBlocks);

            // 카메라 위치 조정
            AdjustCameraPosition();
        }

        public void ClearLevel()
        {
            // 모든 게임 오브젝트 제거
            wallController.ClearWalls();
            boardController.ClearBoard();
            blockController.ClearBlocks();
        }

        private void AdjustCameraPosition()
        {
            // 보드 크기에 맞게 카메라 위치 조정
            if (Camera.main != null)
            {
                int boardWidth = boardController.BoardWidth;
                Vector3 camPos = Camera.main.transform.position;
                Camera.main.transform.position = new Vector3(1.5f + 0.5f * (boardWidth - 4), camPos.y, camPos.z);
            }
        }

        public async Task GoToNextLevelAsync()
        {
            int nextLevel = levelController.CurrentLevel + 1;
            await LoadLevelAsync(nextLevel);
        }

        public async Task GoToPreviousLevelAsync()
        {
            int prevLevel = levelController.CurrentLevel - 1;
            if (prevLevel > 0)
            {
                await LoadLevelAsync(prevLevel);
            }
        }

        // 컨트롤러 참조 getter 메서드들
        public BoardController GetBoardController() => boardController;
        public BlockController GetBlockController() => blockController;
        public WallController GetWallController() => wallController;
        public InputController GetInputController() => inputController;
        public LevelController GetLevelController() => levelController;
    }
}