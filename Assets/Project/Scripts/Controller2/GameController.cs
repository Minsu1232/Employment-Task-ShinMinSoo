using System.Threading.Tasks;
using UnityEngine;
using Project.Scripts.Model;

namespace Project.Scripts.Controller
{
    /// <summary>
    /// ������ ��ü �帧�� �����ϴ� ��Ʈ�ѷ�
    /// </summary>
    public class GameController : MonoBehaviour
    {
        [Header("��Ʈ�ѷ� ����")]
        [SerializeField] private BoardController boardController;
        [SerializeField] private BlockController blockController;
        [SerializeField] private WallController wallController;
        [SerializeField] private InputController inputController;
        [SerializeField] private LevelController levelController;

        [Header("���� ����")]
        [SerializeField] private int startLevel = 1;
        [SerializeField] private bool autoStartGame = true;

        private bool isGameInitialized = false;

        private void Awake()
        {
            // ��Ʈ�ѷ� �ڵ� ����
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

            // ��Ʈ�ѷ� �ʱ�ȭ
            boardController.Initialize(this);
            blockController.Initialize(this);
            wallController.Initialize(this);
            inputController.Initialize(this);
            levelController.Initialize(this);

            // ��Ʈ�ѷ� ���� ����
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
            // ������ �Ŵ��� �ʱ�ȭ
            if (DataManager.Instance == null)
            {
                Debug.LogError("DataManager �ν��Ͻ��� �����ϴ�!");
                return;
            }

            // ���� ���� �ε�
            await LoadLevelAsync(startLevel);
            isGameInitialized = true;
        }

        public async Task LoadLevelAsync(int levelIndex)
        {
            // ���� ���� ����
            ClearLevel();

            // ���� ������ �ε�
            Project.Scripts.Model.StageData stageData = await Project.Scripts.Model.DataManager.Instance.LoadStageDataAsync(levelIndex);

            if (stageData != null)
            {
                // �� ��Ʈ�ѷ� �ʱ�ȭ
                await InitializeLevelAsync(stageData);
                levelController.SetCurrentLevel(levelIndex);
            }
            else
            {
                Debug.LogError($"���� {levelIndex}�� �ε��� �� �����ϴ�.");
            }
        }

        public async Task InitializeLevelAsync(Project.Scripts.Model.StageData stageData)
        {
            // ���� ��� ���� ����:
            // 1. �� (���) ����
            // 2. ���� ����
            // 3. ��� ����

            // �� ����
            await wallController.CreateWallsAsync(stageData.walls);

            // ���� ����
            await boardController.CreateBoardAsync(stageData.boardBlocks);

            // ��� ����
            await blockController.CreateBlocksAsync(stageData.playingBlocks);

            // ī�޶� ��ġ ����
            AdjustCameraPosition();
        }

        public void ClearLevel()
        {
            // ��� ���� ������Ʈ ����
            wallController.ClearWalls();
            boardController.ClearBoard();
            blockController.ClearBlocks();
        }

        private void AdjustCameraPosition()
        {
            // ���� ũ�⿡ �°� ī�޶� ��ġ ����
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

        // ��Ʈ�ѷ� ���� getter �޼����
        public BoardController GetBoardController() => boardController;
        public BlockController GetBlockController() => blockController;
        public WallController GetWallController() => wallController;
        public InputController GetInputController() => inputController;
        public LevelController GetLevelController() => levelController;
    }
}