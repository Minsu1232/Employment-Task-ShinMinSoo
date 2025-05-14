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
    /// 스테이지 전환 및 초기화를 담당하는 컨트롤러
    /// </summary>
    public class StageController : MonoBehaviour, IGameEventListener<int>
    {
        public static StageController Instance { get; private set; }

        [SerializeField] private GameConfig gameConfig;

        // 참조 컴포넌트
        private BoardBuilder boardBuilder;
        private WallBuilder wallBuilder;
        private BlockFactory blockFactory;
        private VisualEffectManager visualEffectManager;
        private BlockDestroyManager blockDestroyManager;
        private CheckBlockGroupManager checkBlockManager;
        // 게임 상태
        private int nowStageIndex = 0;
        public int boardWidth { get; private set; }
        public int boardHeight { get; private set; }

        // 게임 오브젝트 참조
        private GameObject boardParent;
        private GameObject playingBlockParent;

        private void Awake()
        {
            // 싱글톤 설정
            if (Instance == null)
                Instance = this;
            else if (Instance != this)
                Destroy(gameObject);

            Application.targetFrameRate = 60;

            // 컴포넌트 초기화
            InitializeComponents();

            // 이벤트 등록
            RegisterEvents();
        }

        private void OnDestroy()
        {
            // 이벤트 해제
            UnregisterEvents();
        }

        private void Start()
        {
            Init();
        }

        /// <summary>
        /// 컴포넌트 초기화
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
        /// 이벤트 등록
        /// </summary>
        private void RegisterEvents()
        {
            gameConfig.gameEvents.onStageLoad.RegisterListener(this);
        }

        /// <summary>
        /// 이벤트 해제
        /// </summary>
        private void UnregisterEvents()
        {
            gameConfig.gameEvents.onStageLoad.UnregisterListener(this);
        }

        /// <summary>
        /// 스테이지 로드 이벤트 처리
        /// </summary>
        public void OnEventRaised(int stageIndex)
        {
            Init(stageIndex);
        }

        /// <summary>
        /// 게임 초기화 및 스테이지 로드
        /// </summary>
        public async void Init(int stageIdx = 0)
        {
            if (gameConfig.stageDatas == null || gameConfig.stageDatas.Length == 0)
            {
                Debug.LogError("StageData가 할당되지 않았습니다!");
                return;
            }

            // 이전 스테이지 정리
            CleanupCurrentStage();

            // 게임 오브젝트 초기화
            boardParent = new GameObject("BoardParent");
            boardParent.transform.SetParent(transform);

            playingBlockParent = new GameObject("PlayingBlockParent");
            playingBlockParent.transform.SetParent(transform);

            // 스테이지 생성
            var wallCoordInfo = await wallBuilder.CreateCustomWalls(stageIdx, boardParent);
            var boardInfo = await boardBuilder.CreateBoardAsync(stageIdx, wallCoordInfo, boardParent);

            // BlockDestroyManager 초기화 
            checkBlockManager.Initialize(gameConfig, boardInfo.boardBlockDic, boardInfo.boardWidth, boardInfo.boardHeight, gameConfig.stageDatas[stageIdx]);
            checkBlockManager.SetStandardBlockData(boardInfo.standardBlocks);

            await blockFactory.CreatePlayingBlocksAsync(stageIdx, boardInfo.boardBlockDic, boardInfo.boardWidth, boardInfo.boardHeight, playingBlockParent);

            // 보드 크기 정보 저장
            boardWidth = boardInfo.boardWidth;
            boardHeight = boardInfo.boardHeight;

            // 벽 오브젝트 등록 (스텐실 효과용)
            visualEffectManager.RegisterWalls(wallBuilder.walls);

            // 현재 스테이지 인덱스 설정
            nowStageIndex = stageIdx;
        }

        /// <summary>
        /// 현재 스테이지 정리
        /// </summary>
        private void CleanupCurrentStage()
        {
            if (boardParent != null)
                Destroy(boardParent);

            if (playingBlockParent != null)
                Destroy(playingBlockParent.gameObject);
        }

        /// <summary>
        /// 이전 레벨로 이동
        /// </summary>
        public void GoToPreviousLevel()
        {
            if (nowStageIndex == 0) return;

            gameConfig.gameEvents.onStageLoad.Raise(--nowStageIndex);
            StartCoroutine(Wait());
        }

        /// <summary>
        /// 다음 레벨로 이동
        /// </summary>
        public void GotoNextLevel()
        {
            if (nowStageIndex == gameConfig.stageDatas.Length - 1) return;

            gameConfig.gameEvents.onStageLoad.Raise(++nowStageIndex);
            StartCoroutine(Wait());
        }

        /// <summary>
        /// 카메라 위치 조정 대기
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