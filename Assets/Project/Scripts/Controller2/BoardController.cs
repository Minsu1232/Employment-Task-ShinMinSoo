using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Project.Scripts.Model;

namespace Project.Scripts.Controller
{
    /// <summary>
    /// ���� ���� ��� ������ ����ϴ� ��Ʈ�ѷ�
    /// </summary>
    public class BoardController : MonoBehaviour
    {
        [Header("������ ����")]
        [SerializeField] private GameObject boardBlockPrefab;

        [Header("���� ����")]
        [SerializeField] private float blockDistance = 0.79f;

        private GameController gameController;
        private GameObject boardParent;

        // ���� ũ�� ������Ƽ
        public int BoardWidth { get; private set; }
        public int BoardHeight { get; private set; }

        // ������ �÷���
        private Dictionary<(int x, int y), BoardBlockObject> boardBlockDic = new Dictionary<(int x, int y), BoardBlockObject>();
        private Dictionary<int, List<BoardBlockObject>> checkBlockGroupDic = new Dictionary<int, List<BoardBlockObject>>();

        // �ܺο��� ������ �� �ִ� ������Ƽ
        public IReadOnlyDictionary<(int x, int y), BoardBlockObject> BoardBlocks => boardBlockDic;
        public IReadOnlyDictionary<int, List<BoardBlockObject>> CheckBlockGroups => checkBlockGroupDic;

        public void Initialize(GameController controller)
        {
            gameController = controller;
        }

        public async Task CreateBoardAsync(List<Project.Scripts.Model.BoardBlockData> boardBlocks)
        {
            // ���� ���� ����
            ClearBoard();

            // �� ���� �θ� ������Ʈ ����
            boardParent = new GameObject("BoardParent");
            boardParent.transform.SetParent(transform);

            // ���� ��� ����
            await CreateBoardBlocksAsync(boardBlocks);

            // üũ ��� �׷� ����
            SetupCheckBlockGroups();

            // ���� ũ�� ���
            CalculateBoardSize();
        }

        private async Task CreateBoardBlocksAsync(List<Project.Scripts.Model.BoardBlockData> boardBlocks)
        {
            boardBlockDic.Clear();

            if (boardBlockPrefab == null)
            {
                Debug.LogError("boardBlockPrefab�� �������� �ʾҽ��ϴ�!");
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
                    // ���� ��� �ʱ�ȭ
                    boardBlock.SetupBlock(this, blockData);
                    boardBlockDic.Add((blockData.X, blockData.Y), boardBlock);
                }
            }

            await Task.Yield(); // �񵿱� �۾� ����Ʈ
        }

        private void SetupCheckBlockGroups()
        {
            // üũ ��� �׷� ���� ����
            checkBlockGroupDic.Clear();

            // ǥ�� ��� ��ųʸ� ����
            Dictionary<(int, bool), BoardBlockObject> standardBlockDic = new Dictionary<(int, bool), BoardBlockObject>();
            int standardBlockIndex = -1;

            // 1. �� ��ǥ ���� ����
            SetupWallCoordinateInfo();

            // 2. ǥ�� ��� ����
            SetupStandardBlocks(standardBlockDic, ref standardBlockIndex);

            // 3. üũ ��� �׷� ����
            CreateCheckBlockGroups();
        }

        private void SetupWallCoordinateInfo()
        {
            // �� ��ǥ ���� ���� ����
            // (�� ��Ʈ�ѷ��� ����)
        }

        private void SetupStandardBlocks(Dictionary<(int, bool), BoardBlockObject> standardBlockDic, ref int standardBlockIndex)
        {
            // ǥ�� ��� ���� ����
        }

        private void CreateCheckBlockGroups()
        {
            // üũ ��� �׷� ���� ����
            int checkBlockIndex = -1;

            foreach (var blockPos in boardBlockDic.Keys)
            {
                BoardBlockObject boardBlock = boardBlockDic[blockPos];

                // ���� �ڵ� �����丵
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

        // ��� üũ �� ��ġ ���� �޼����
        public bool CheckCanDestroy(BoardBlockObject boardBlock, BlockObject block)
        {
            // ���� �ڵ� �����丵
            return false; // �ӽ� ��ȯ��
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

        // ��ƿ��Ƽ �޼���
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