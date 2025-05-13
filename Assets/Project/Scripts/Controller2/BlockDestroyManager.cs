using System.Collections.Generic;
using UnityEngine;
using Project.Scripts.Config;
using Project.Scripts.Events;
using Project.Scripts.Model;
using static Project.Scripts.Model.BoardBlockData;
using Project.Scripts.View;

namespace Project.Scripts.Controller
{
    /// <summary>
    /// 블록 파괴 관련 로직을 처리하는 매니저 클래스
    /// </summary>
    public class BlockDestroyManager : MonoBehaviour, IGameEventListener<(BoardBlockObject, BlockObject)>
    {
        // 설정 및 참조
        private GameConfig gameConfig;

        // 보드 데이터 참조
        private Dictionary<(int x, int y), BoardBlockObject> boardBlockDic;
        private Dictionary<int, List<BoardBlockObject>> checkBlockGroupDic = new Dictionary<int, List<BoardBlockObject>>();
        private Dictionary<(int, bool), BoardBlockObject> standardBlockDic = new Dictionary<(int, bool), BoardBlockObject>();

        // 보드 크기 정보
        private int boardWidth;
        private int boardHeight;

        /// <summary>
        /// GameConfig를 통한 초기화
        /// </summary>
        public void Initialize(GameConfig config, Dictionary<(int x, int y), BoardBlockObject> boardBlocks, int width, int height)
        {
            this.gameConfig = config;
            this.boardBlockDic = boardBlocks;
            this.boardWidth = width;
            this.boardHeight = height;

            // 블록 그룹 초기화
            checkBlockGroupDic.Clear();
            standardBlockDic.Clear();

            // 이벤트 등록
            RegisterEvents();

            // BoardBlockObject 이벤트 핸들러 등록
            RegisterBoardBlockEvents();
        }

        /// <summary>
        /// 이벤트 등록
        /// </summary>
        private void RegisterEvents()
        {
            if (gameConfig != null && gameConfig.gameEvents != null)
            {
                gameConfig.gameEvents.onCheckDestroy.RegisterListener(this);
            }
        }

        /// <summary>
        /// BoardBlockObject 정적 이벤트 등록
        /// </summary>
        private void RegisterBoardBlockEvents()
        {
            // 파괴 체크 이벤트 핸들러 등록
            BoardBlockObject.OnCheckDestroy += CheckCanDestroy;

            // 파티클 가져오기 이벤트 핸들러
            BoardBlockObject.OnGetDestroyParticle += GetDestroyParticle;

            // 재질 가져오기 이벤트 핸들러
            BoardBlockObject.OnGetMaterial += GetMaterial;

            // 보드 크기 가져오기 이벤트 핸들러
            BoardBlockObject.OnGetBoardSize += GetBoardSize;
        }

        /// <summary>
        /// 이벤트 해제
        /// </summary>
        private void OnDestroy()
        {
            if (gameConfig != null && gameConfig.gameEvents != null)
            {
                gameConfig.gameEvents.onCheckDestroy.UnregisterListener(this);
            }

            // 정적 이벤트 핸들러 해제
            BoardBlockObject.OnCheckDestroy -= CheckCanDestroy;
            BoardBlockObject.OnGetDestroyParticle -= GetDestroyParticle;
            BoardBlockObject.OnGetMaterial -= GetMaterial;
            BoardBlockObject.OnGetBoardSize -= GetBoardSize;
        }

        /// <summary>
        /// 체크 디스트로이 이벤트 처리
        /// </summary>
        public void OnEventRaised((BoardBlockObject, BlockObject) data)
        {
            var (boardBlock, block) = data;
            bool canDestroy = CheckCanDestroy(boardBlock, block);

            // 파괴 가능하면 블록 파괴 이벤트 발생
            if (canDestroy && block != null)
            {
                gameConfig.gameEvents.onBlockDestroy.Raise(block);
            }
        }

        /// <summary>
        /// 표준 블록 정보 설정
        /// </summary>
        public void SetStandardBlockData(Dictionary<(int, bool), BoardBlockObject> standardBlocks)
        {
            this.standardBlockDic = standardBlocks;

            // 표준 블록과 체크 그룹 설정
            SetupStandardBlocks();
            CreateCheckBlockGroups();
        }

        /// <summary>
        /// 표준 블록 설정
        /// </summary>
        private void SetupStandardBlocks()
        {
            foreach (var kv in standardBlockDic)
            {
                BoardBlockObject boardBlockObject = kv.Value;
                for (int i = 0; i < boardBlockObject.colorType.Count; i++)
                {
                    if (kv.Key.Item2) // 가로 방향
                    {
                        for (int j = boardBlockObject.x + 1; j < boardBlockObject.x + boardBlockObject.len[i]; j++)
                        {
                            if (boardBlockDic.TryGetValue((j, boardBlockObject.y), out BoardBlockObject targetBlock))
                            {
                                targetBlock.colorType.Add(boardBlockObject.colorType[i]);
                                targetBlock.len.Add(boardBlockObject.len[i]);
                                targetBlock.isHorizon.Add(kv.Key.Item2);
                                targetBlock.isCheckBlock = true;
                            }
                        }
                    }
                    else // 세로 방향
                    {
                        for (int k = boardBlockObject.y + 1; k < boardBlockObject.y + boardBlockObject.len[i]; k++)
                        {
                            if (boardBlockDic.TryGetValue((boardBlockObject.x, k), out BoardBlockObject targetBlock))
                            {
                                targetBlock.colorType.Add(boardBlockObject.colorType[i]);
                                targetBlock.len.Add(boardBlockObject.len[i]);
                                targetBlock.isHorizon.Add(kv.Key.Item2);
                                targetBlock.isCheckBlock = true;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 체크 블록 그룹 생성
        /// </summary>
        private void CreateCheckBlockGroups()
        {
            int checkBlockIndex = -1;
            checkBlockGroupDic.Clear();

            foreach (var blockPos in boardBlockDic.Keys)
            {
                BoardBlockObject boardBlock = boardBlockDic[blockPos];

                for (int j = 0; j < boardBlock.colorType.Count; j++)
                {
                    if (boardBlock.isCheckBlock && boardBlock.colorType[j] != ColorType.None)
                    {
                        // 이 블록이 이미 그룹에 속해있는지 확인
                        if (boardBlock.checkGroupIdx.Count <= j)
                        {
                            if (boardBlock.isHorizon[j])
                            {
                                // 왼쪽 블록 확인
                                (int x, int y) leftPos = (boardBlock.x - 1, boardBlock.y);
                                if (boardBlockDic.TryGetValue(leftPos, out BoardBlockObject leftBlock) &&
                                    j < leftBlock.colorType.Count &&
                                    leftBlock.colorType[j] == boardBlock.colorType[j] &&
                                    leftBlock.checkGroupIdx.Count > j)
                                {
                                    int grpIdx = leftBlock.checkGroupIdx[j];
                                    checkBlockGroupDic[grpIdx].Add(boardBlock);
                                    boardBlock.checkGroupIdx.Add(grpIdx);
                                }
                                else
                                {
                                    checkBlockIndex++;
                                    checkBlockGroupDic.Add(checkBlockIndex, new List<BoardBlockObject>());
                                    checkBlockGroupDic[checkBlockIndex].Add(boardBlock);
                                    boardBlock.checkGroupIdx.Add(checkBlockIndex);
                                }
                            }
                            else
                            {
                                // 위쪽 블록 확인
                                (int x, int y) upPos = (boardBlock.x, boardBlock.y - 1);
                                if (boardBlockDic.TryGetValue(upPos, out BoardBlockObject upBlock) &&
                                    j < upBlock.colorType.Count &&
                                    upBlock.colorType[j] == boardBlock.colorType[j] &&
                                    upBlock.checkGroupIdx.Count > j)
                                {
                                    int grpIdx = upBlock.checkGroupIdx[j];
                                    checkBlockGroupDic[grpIdx].Add(boardBlock);
                                    boardBlock.checkGroupIdx.Add(grpIdx);
                                }
                                else
                                {
                                    checkBlockIndex++;
                                    checkBlockGroupDic.Add(checkBlockIndex, new List<BoardBlockObject>());
                                    checkBlockGroupDic[checkBlockIndex].Add(boardBlock);
                                    boardBlock.checkGroupIdx.Add(checkBlockIndex);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 파괴 파티클 반환 (이벤트 핸들러)
        /// </summary>
        private ParticleSystem GetDestroyParticle()
        {
            if (gameConfig != null && gameConfig.visualConfig != null)
            {
                return gameConfig.visualConfig.destroyParticlePrefab;
            }
            return null;
        }

        /// <summary>
        /// 재질 반환 (이벤트 핸들러)
        /// </summary>
        private Material GetMaterial(int index)
        {
            if (gameConfig != null && gameConfig.wallConfig != null)
            {
                if (index >= 0 && index < gameConfig.wallConfig.wallMaterials.Length)
                {
                    return gameConfig.wallConfig.wallMaterials[index];
                }
            }
            return null;
        }

        /// <summary>
        /// 보드 크기 반환 (이벤트 핸들러)
        /// </summary>
        private Vector2Int GetBoardSize()
        {
            return new Vector2Int(boardWidth, boardHeight);
        }

        /// <summary>
        /// 블록 파괴 가능 여부 확인 (이벤트 핸들러 및 내부 로직)
        /// </summary>
        public bool CheckCanDestroy(BoardBlockObject boardBlock, BlockObject block)
        {
            // 체크 그룹 유효성 검사
            foreach (var checkGroupIdx in boardBlock.checkGroupIdx)
            {
                if (!boardBlock.isCheckBlock && !checkBlockGroupDic.ContainsKey(checkGroupIdx)) return false;
            }

            // 플레이어 블록 범위 계산
            int pBlockminX = boardWidth;
            int pBlockmaxX = -1;
            int pBlockminY = boardHeight;
            int pBlockmaxY = -1;

            List<BlockObject> blocks = block.dragHandler.blocks;

            foreach (var playingBlock in blocks)
            {
                if (playingBlock.x <= pBlockminX) pBlockminX = (int)playingBlock.x;
                if (playingBlock.y <= pBlockminY) pBlockminY = (int)playingBlock.y;
                if (playingBlock.x >= pBlockmaxX) pBlockmaxX = (int)playingBlock.x;
                if (playingBlock.y >= pBlockmaxY) pBlockmaxY = (int)playingBlock.y;
            }

            List<BoardBlockObject> horizonBoardBlocks = new List<BoardBlockObject>();
            List<BoardBlockObject> verticalBoardBlocks = new List<BoardBlockObject>();

            foreach (var checkIndex in boardBlock.checkGroupIdx)
            {
                foreach (var boardBlockObj in checkBlockGroupDic[checkIndex])
                {
                    foreach (var horizon in boardBlockObj.isHorizon)
                    {
                        if (horizon) horizonBoardBlocks.Add(boardBlockObj);
                        else verticalBoardBlocks.Add(boardBlockObj);
                    }
                }
            }

            int matchingIndex = boardBlock.colorType.FindIndex(color => color == block.colorType);
            bool hor = boardBlock.isHorizon[matchingIndex];

            // Horizon
            if (hor)
            {
                int minX = boardWidth;
                int maxX = -1;
                foreach (var coordinate in horizonBoardBlocks)
                {
                    if (coordinate.x < minX) minX = (int)coordinate.x;
                    if (coordinate.x > maxX) maxX = (int)coordinate.x;
                }

                // 개별 좌표가 나갔는지 여부를 판단
                if (pBlockminX < minX - gameConfig.boardConfig.blockDistance / 2 ||
                    pBlockmaxX > maxX + gameConfig.boardConfig.blockDistance / 2)
                {
                    return false;
                }

                (int, int)[] blockCheckCoors = new (int, int)[horizonBoardBlocks.Count];

                for (int i = 0; i < horizonBoardBlocks.Count; i++)
                {
                    if (horizonBoardBlocks[i].y <= boardHeight / 2)
                    {
                        int maxY = -1;

                        for (int k = 0; k < block.dragHandler.blocks.Count; k++)
                        {
                            var currentBlock = block.dragHandler.blocks[k];

                            if (currentBlock.y == horizonBoardBlocks[i].y)
                            {
                                if (currentBlock.y > maxY)
                                {
                                    maxY = (int)currentBlock.y;
                                }
                            }
                        }

                        blockCheckCoors[i] = ((int)horizonBoardBlocks[i].x, maxY);

                        for (int l = blockCheckCoors[i].Item2; l <= horizonBoardBlocks[i].y; l++)
                        {
                            if (blockCheckCoors[i].Item1 < pBlockminX || blockCheckCoors[i].Item1 > pBlockmaxX)
                                continue;

                            (int, int) key = (blockCheckCoors[i].Item1, l);

                            if (boardBlockDic.ContainsKey(key) &&
                                boardBlockDic[key].playingBlock != null &&
                                boardBlockDic[key].playingBlock.colorType != boardBlock.horizonColorType)
                            {
                                return false;
                            }
                        }
                    }
                    else // up to downside
                    {
                        int minY = 100;

                        for (int k = 0; k < block.dragHandler.blocks.Count; k++)
                        {
                            var currentBlock = block.dragHandler.blocks[k];

                            if (currentBlock.y == horizonBoardBlocks[i].y)
                            {
                                if (currentBlock.y < minY)
                                {
                                    minY = (int)currentBlock.y;
                                }
                            }
                        }

                        blockCheckCoors[i] = ((int)horizonBoardBlocks[i].x, minY);

                        for (int l = blockCheckCoors[i].Item2; l >= horizonBoardBlocks[i].y; l--)
                        {
                            if (blockCheckCoors[i].Item1 < pBlockminX || blockCheckCoors[i].Item1 > pBlockmaxX)
                                continue;
                            (int, int) key = (blockCheckCoors[i].Item1, l);

                            if (boardBlockDic.ContainsKey(key) &&
                                boardBlockDic[key].playingBlock != null &&
                                boardBlockDic[key].playingBlock.colorType != boardBlock.horizonColorType)
                            {
                                return false;
                            }
                        }
                    }
                }

                return true;
            }
            // Vertical
            else
            {
                int minY = boardHeight;
                int maxY = -1;

                foreach (var coordinate in verticalBoardBlocks)
                {
                    if (coordinate.y < minY) minY = (int)coordinate.y;
                    if (coordinate.y > maxY) maxY = (int)coordinate.y;
                }

                if (pBlockminY < minY - gameConfig.boardConfig.blockDistance / 2 ||
                    pBlockmaxY > maxY + gameConfig.boardConfig.blockDistance / 2)
                {
                    return false;
                }

                (int, int)[] blockCheckCoors = new (int, int)[verticalBoardBlocks.Count];

                for (int i = 0; i < verticalBoardBlocks.Count; i++)
                {
                    //x exist in left
                    if (verticalBoardBlocks[i].x <= boardWidth / 2)
                    {
                        int maxX = int.MinValue;

                        for (int k = 0; k < block.dragHandler.blocks.Count; k++)
                        {
                            var currentBlock = block.dragHandler.blocks[k];

                            if (currentBlock.y == verticalBoardBlocks[i].y)
                            {
                                if (currentBlock.x > maxX)
                                {
                                    maxX = (int)currentBlock.x;
                                }
                            }
                        }

                        // 튜플에 y와 maxX를 저장
                        blockCheckCoors[i] = (maxX, (int)verticalBoardBlocks[i].y);

                        for (int l = blockCheckCoors[i].Item1; l >= verticalBoardBlocks[i].x; l--)
                        {
                            if (blockCheckCoors[i].Item2 < pBlockminY || blockCheckCoors[i].Item2 > pBlockmaxY)
                                continue;
                            (int, int) key = (l, blockCheckCoors[i].Item2);

                            if (boardBlockDic.ContainsKey(key) &&
                                boardBlockDic[key].playingBlock != null &&
                                boardBlockDic[key].playingBlock.colorType != boardBlock.verticalColorType)
                            {
                                return false;
                            }
                        }
                    }
                    else // x exist in right
                    {
                        int minX = 100;

                        for (int k = 0; k < block.dragHandler.blocks.Count; k++)
                        {
                            var currentBlock = block.dragHandler.blocks[k];

                            if (currentBlock.y == verticalBoardBlocks[i].y)
                            {
                                if (currentBlock.x < minX)
                                {
                                    minX = (int)currentBlock.x;
                                }
                            }
                        }

                        // 튜플에 y와 minX를 저장
                        blockCheckCoors[i] = (minX, (int)verticalBoardBlocks[i].y);

                        for (int l = blockCheckCoors[i].Item1; l <= verticalBoardBlocks[i].x; l++)
                        {
                            if (blockCheckCoors[i].Item2 < pBlockminY || blockCheckCoors[i].Item2 > pBlockmaxY)
                                continue;
                            (int, int) key = (l, blockCheckCoors[i].Item2);

                            if (boardBlockDic.ContainsKey(key) &&
                                boardBlockDic[key].playingBlock != null &&
                                boardBlockDic[key].playingBlock.colorType != boardBlock.verticalColorType)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 블록 직접 파괴 메서드 (필요시 외부에서 호출 가능)
        /// </summary>
        public void DestroyBlockWithEffect(
    BlockObject block,
    Vector3 movePosition,     // 이동 목표 위치
    Vector3 effectPosition,   // 이미 계산된 파티클 위치
    LaunchDirection direction, // 방향 (길이 계산용)
    ColorType colorType,      // 색상
    Quaternion rotation)      // 회전
        {
            if (block == null || block.dragHandler == null) return;

            // VisualEffectManager 가져오기
            VisualEffectManager visualEffectManager = StageController.Instance.GetVisualEffectManager();

            // 블록 파괴 이벤트 발생
            gameConfig.gameEvents.onBlockDestroy.Raise(block);

            // 블록 파괴 로직 실행
            block.dragHandler.ReleaseInput();

            foreach (var blockObject in block.dragHandler.blocks)
            {
                if (blockObject.preBoardBlockObject != null)
                {
                    blockObject.preBoardBlockObject.playingBlock = null;
                }
                blockObject.ColliderOff();
            }

            block.dragHandler.enabled = false;
            GameObject blockGroup = block.transform.parent.gameObject; // 블럭뭉치에 접근
            // 버텍스 스텐실 효과 적용 (새로 추가)
            if (visualEffectManager != null)
            {
                visualEffectManager.ApplyWallClippingToBlock(blockGroup, effectPosition, (global::LaunchDirection)direction);
            }
            // 블록 길이 계산 (필요한 정보)
            int blockLength = (direction == LaunchDirection.Up || direction == LaunchDirection.Down)
                        ? block.dragHandler.horizon    // 세로 방향 발사면 가로 길이 사용
                        : block.dragHandler.vertical;  // 가로 방향 발사면 세로 길이 사용

            // 파티클 생성 및 이동 애니메이션
            if (visualEffectManager != null)
            {
                ParticleSystem particle = visualEffectManager.CreateParticleEffect(
                    effectPosition,    // 이미 계산된 위치 
                    rotation,          // 이미 계산된 회전
                    colorType,         // 색상
                    blockLength        // 길이
                );

                // 블록 이동 애니메이션 실행
                block.dragHandler.DestroyMove(movePosition, particle);
            }
            else
            {
                // VisualEffectManager가 없는 경우 기본 이동 애니메이션 실행
                block.dragHandler.DestroyMove(movePosition, null);
            }
        }
       
        /// <summary>
        /// 새로운 스테이지 로딩을 위한 리셋
        /// </summary>
        public void Reset()
        {
            checkBlockGroupDic.Clear();
            standardBlockDic.Clear();
        }
    }
}