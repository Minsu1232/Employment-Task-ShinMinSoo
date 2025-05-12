using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Project.Scripts.Model;

namespace Project.Scripts.Controller
{
    /// <summary>
    /// 게임 보드 블록 관리를 담당하는 컨트롤러
    /// </summary>
    public class BoardController : MonoBehaviour
    {
        [Header("프리팹 참조")]
        [SerializeField] private GameObject boardBlockPrefab;

        [Header("보드 설정")]
        [SerializeField] private float blockDistance = 0.79f;

        private GameController gameController;
        private GameObject boardParent;

        // 보드 크기 프로퍼티
        public int BoardWidth { get; private set; }
        public int BoardHeight { get; private set; }

        // 데이터 컬렉션
        private Dictionary<(int x, int y), BoardBlockObject> boardBlockDic = new Dictionary<(int x, int y), BoardBlockObject>();
        private Dictionary<int, List<BoardBlockObject>> checkBlockGroupDic = new Dictionary<int, List<BoardBlockObject>>();

        // 외부에서 참조할 수 있는 프로퍼티
        public IReadOnlyDictionary<(int x, int y), BoardBlockObject> BoardBlocks => boardBlockDic;
        public IReadOnlyDictionary<int, List<BoardBlockObject>> CheckBlockGroups => checkBlockGroupDic;

        public void Initialize(GameController controller)
        {
            gameController = controller;
        }

        public async Task CreateBoardAsync(List<Project.Scripts.Model.BoardBlockData> boardBlocks)
        {
            // 이전 보드 제거
            ClearBoard();

            // 새 보드 부모 오브젝트 생성
            boardParent = new GameObject("BoardParent");
            boardParent.transform.SetParent(transform);

            // 보드 블록 생성
            await CreateBoardBlocksAsync(boardBlocks);

            // 체크 블록 그룹 설정
            SetupCheckBlockGroups();

            // 보드 크기 계산
            CalculateBoardSize();
        }

        private async Task CreateBoardBlocksAsync(List<Project.Scripts.Model.BoardBlockData> boardBlocks)
        {
            boardBlockDic.Clear();

            if (boardBlockPrefab == null)
            {
                Debug.LogError("boardBlockPrefab이 설정되지 않았습니다!");
                return;
            }

            foreach (var blockData in boardBlocks)
            {
                GameObject blockObj = Instantiate(boardBlockPrefab, boardParent.transform);
                blockObj.transform.localPosition = new Vector3(
                    blockData.X * blockDistance,
                    0,
                    blockData.Y * blockDistance
                );

                if (blockObj.TryGetComponent(out Project.Scripts.Model.BoardBlockObject boardBlock))
                {
                    // 보드 블록 초기화
                    boardBlock.SetupBlock(this, blockData);
                    boardBlockDic.Add((blockData.X, blockData.Y), boardBlock);
                }
            }

            await Task.Yield(); // 비동기 작업 포인트
        }

        private void SetupCheckBlockGroups()
        {
            // 체크 블록 그룹 설정 로직
            checkBlockGroupDic.Clear();

            // 표준 블록 딕셔너리 생성
            Dictionary<(int, bool), BoardBlockObject> standardBlockDic = new Dictionary<(int, bool), BoardBlockObject>();
            int standardBlockIndex = -1;

            // 1. 벽 좌표 정보 설정
            SetupWallCoordinateInfo();

            // 2. 표준 블록 설정
            SetupStandardBlocks(standardBlockDic, ref standardBlockIndex);

            // 3. 체크 블록 그룹 생성
            CreateCheckBlockGroups();
        }

        private void SetupWallCoordinateInfo()
        {
            // 벽 좌표 정보 설정 로직
            // (벽 컨트롤러와 연동)
        }

        private void SetupStandardBlocks(Dictionary<(int, bool), BoardBlockObject> standardBlockDic, ref int standardBlockIndex)
        {
            // 표준 블록 설정 로직
        }

        private void CreateCheckBlockGroups()
        {
            // 체크 블록 그룹 생성 로직
            int checkBlockIndex = -1;

            foreach (var blockPos in boardBlockDic.Keys)
            {
                BoardBlockObject boardBlock = boardBlockDic[blockPos];

                // 기존 코드 리팩토링
            }
        }

        private void CalculateBoardSize()
        {
            if (boardBlockDic.Count > 0)
            {
                BoardWidth = boardBlockDic.Keys.Max(k => k.x);
                BoardHeight = boardBlockDic.Keys.Max(k => k.y);
            }
            else
            {
                BoardWidth = 0;
                BoardHeight = 0;
            }
        }

        public void ClearBoard()
        {
            if (boardParent != null)
            {
                Destroy(boardParent);
            }

            boardBlockDic.Clear();
            checkBlockGroupDic.Clear();
        }

        // 블록 체크 및 배치 관련 메서드들
        public bool CheckCanDestroy(BoardBlockObject boardBlock, BlockObject block)
        {
            // 기존 코드 리팩토링
            return false; // 임시 반환값
        }

        public BoardBlockObject GetBoardBlockAt(int x, int y)
        {
            if (boardBlockDic.TryGetValue((x, y), out BoardBlockObject block))
            {
                return block;
            }
            return null;
        }

        public bool IsValidPosition(int x, int y)
        {
            return boardBlockDic.ContainsKey((x, y));
        }

        public List<BoardBlockObject> GetCheckBlockGroup(int groupIndex)
        {
            if (checkBlockGroupDic.TryGetValue(groupIndex, out List<BoardBlockObject> group))
            {
                return group;
            }
            return new List<BoardBlockObject>();
        }

        // 유틸리티 메서드
        public Vector3 GridToWorldPosition(int x, int y)
        {
            return new Vector3(x * blockDistance, 0, y * blockDistance);
        }

        public (int x, int y) WorldToGridPosition(Vector3 worldPos)
        {
            int x = Mathf.RoundToInt(worldPos.x / blockDistance);
            int y = Mathf.RoundToInt(worldPos.z / blockDistance);
            return (x, y);
        }
    }
}