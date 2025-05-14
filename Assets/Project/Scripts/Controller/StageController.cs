using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using Project.Scripts.Config;
using Project.Scripts.Events;
using Project.Scripts.Model;
using Project.Scripts.View;

namespace Project.Scripts.Controller
{
    /// <summary>
    /// �������� ��ȯ �� �ʱ�ȭ�� ����ϴ� ��Ʈ�ѷ�
    /// </summary>
    public class StageController : MonoBehaviour, IGameEventListener<int>
    {
        public static StageController Instance { get; private set; }

        [SerializeField] private GameConfig gameConfig;

        // ���� ������Ʈ
        private BoardBuilder boardBuilder;
        private WallBuilder wallBuilder;
        private BlockFactory blockFactory;
        private VisualEffectManager visualEffectManager;
        private BlockDestroyManager blockDestroyManager;
        private CheckBlockGroupManager checkBlockManager;
        // ���� ����
        private int nowStageIndex = 0;
        public int boardWidth { get; private set; }
        public int boardHeight { get; private set; }

        // ���� ������Ʈ ����
        private GameObject boardParent;
        private GameObject playingBlockParent;

        private void Awake()
        {
            // �̱��� ����
            if (Instance == null)
                Instance = this;
            else if (Instance != this)
                Destroy(gameObject);

            Application.targetFrameRate = 60;

            // ������Ʈ �ʱ�ȭ
            InitializeComponents();

            // �̺�Ʈ ���
            RegisterEvents();
        }

        private void OnDestroy()
        {
            // �̺�Ʈ ����
            UnregisterEvents();
        }

        private void Start()
        {
            Init();
        }

        /// <summary>
        /// ������Ʈ �ʱ�ȭ
        /// </summary>
        private void InitializeComponents()
        {
            boardBuilder = GetComponent<BoardBuilder>() ?? gameObject.AddComponent<BoardBuilder>();
            wallBuilder = GetComponent<WallBuilder>() ?? gameObject.AddComponent<WallBuilder>();
            blockFactory = GetComponent<BlockFactory>() ?? gameObject.AddComponent<BlockFactory>();
            visualEffectManager = GetComponent<VisualEffectManager>() ?? gameObject.AddComponent<VisualEffectManager>();
            blockDestroyManager = GetComponent<BlockDestroyManager>() ?? gameObject.AddComponent<BlockDestroyManager>();
            checkBlockManager = GetComponent<CheckBlockGroupManager>() ?? gameObject.AddComponent<CheckBlockGroupManager>();

            boardBuilder.Initialize(gameConfig);
            wallBuilder.Initialize(gameConfig);
            blockFactory.Initialize(gameConfig);
            visualEffectManager.Initialize(gameConfig);
            blockDestroyManager.Initialize(gameConfig);
        }

        /// <summary>
        /// �̺�Ʈ ���
        /// </summary>
        private void RegisterEvents()
        {
            gameConfig.gameEvents.onStageLoad.RegisterListener(this);
        }

        /// <summary>
        /// �̺�Ʈ ����
        /// </summary>
        private void UnregisterEvents()
        {
            gameConfig.gameEvents.onStageLoad.UnregisterListener(this);
        }

        /// <summary>
        /// �������� �ε� �̺�Ʈ ó��
        /// </summary>
        public void OnEventRaised(int stageIndex)
        {
            Init(stageIndex);
        }

        /// <summary>
        /// ���� �ʱ�ȭ �� �������� �ε�
        /// </summary>
        public async void Init(int stageIdx = 0)
        {
            if (gameConfig.stageDatas == null || gameConfig.stageDatas.Length == 0)
            {
                Debug.LogError("StageData�� �Ҵ���� �ʾҽ��ϴ�!");
                return;
            }

            // ���� �������� ����
            CleanupCurrentStage();

            // ���� ������Ʈ �ʱ�ȭ
            boardParent = new GameObject("BoardParent");
            boardParent.transform.SetParent(transform);

            playingBlockParent = new GameObject("PlayingBlockParent");
            playingBlockParent.transform.SetParent(transform);

            // �������� ����
            var wallCoordInfo = await wallBuilder.CreateCustomWalls(stageIdx, boardParent);
            var boardInfo = await boardBuilder.CreateBoardAsync(stageIdx, wallCoordInfo, boardParent);

            // BlockDestroyManager �ʱ�ȭ 
            checkBlockManager.Initialize(gameConfig, boardInfo.boardBlockDic, boardInfo.boardWidth, boardInfo.boardHeight, gameConfig.stageDatas[stageIdx]);
            checkBlockManager.SetStandardBlockData(boardInfo.standardBlocks);

            await blockFactory.CreatePlayingBlocksAsync(stageIdx, boardInfo.boardBlockDic, boardInfo.boardWidth, boardInfo.boardHeight, playingBlockParent);

            // ���� ũ�� ���� ����
            boardWidth = boardInfo.boardWidth;
            boardHeight = boardInfo.boardHeight;

            // �� ������Ʈ ��� (���ٽ� ȿ����)
            visualEffectManager.RegisterWalls(wallBuilder.walls);

            // ���� �������� �ε��� ����
            nowStageIndex = stageIdx;
        }

        /// <summary>
        /// ���� �������� ����
        /// </summary>
        private void CleanupCurrentStage()
        {
            if (boardParent != null)
                Destroy(boardParent);

            if (playingBlockParent != null)
                Destroy(playingBlockParent.gameObject);
        }

        /// <summary>
        /// ���� ������ �̵�
        /// </summary>
        public void GoToPreviousLevel()
        {
            if (nowStageIndex == 0) return;

            gameConfig.gameEvents.onStageLoad.Raise(--nowStageIndex);
            StartCoroutine(Wait());
        }

        /// <summary>
        /// ���� ������ �̵�
        /// </summary>
        public void GotoNextLevel()
        {
            if (nowStageIndex == gameConfig.stageDatas.Length - 1) return;

            gameConfig.gameEvents.onStageLoad.Raise(++nowStageIndex);
            StartCoroutine(Wait());
        }

        /// <summary>
        /// ī�޶� ��ġ ���� ���
        /// </summary>
        private IEnumerator Wait()
        {
            yield return null;

            Vector3 camTr = Camera.main.transform.position;
            Camera.main.transform.position = new Vector3(1.5f + 0.5f * (boardWidth - 4), camTr.y, camTr.z);
        }
        public VisualEffectManager GetVisualEffectManager()
        {
            return visualEffectManager;
        }
        public BlockDestroyManager GetBlockDestroyManager()
        {
            return blockDestroyManager;
        }

    }
}