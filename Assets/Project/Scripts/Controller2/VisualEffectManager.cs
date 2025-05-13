using System.Collections.Generic;
using UnityEngine;
using Project.Scripts.Config;
using Project.Scripts.Controller;
using Project.Scripts.Events;
using DG.Tweening;
using static UnityEngine.ParticleSystem;

namespace Project.Scripts.View
{
    /// <summary>
    /// �ð� ȿ�� ������ ����ϴ� �Ŵ��� Ŭ����
    /// </summary>
    public class VisualEffectManager : MonoBehaviour,
        IGameEventListener<BlockObject>
    {
        private GameConfig gameConfig;
       
     



        // ������Ʈ ����
        private List<GameObject> walls = new List<GameObject>();
        private List<GameObject> blocks = new List<GameObject>();

        private VertexStencilEffectView vertexStencilEffectView;

        /// <summary>
        /// �ʱ�ȭ
        /// </summary>
        // ���� Initialize �޼��� ����
        public void Initialize(GameConfig config)
        {
            this.gameConfig = config;

            // ���ؽ� ���ٽ� ȿ�� �ʱ�ȭ
            vertexStencilEffectView = new VertexStencilEffectView();

            // ���̴� ��Ƽ���� ��������
            Material wallStencilMaterial = null;
            Material blockStencilMaterial = null;

            if (gameConfig != null && gameConfig.visualConfig != null)
            {
                // VisualConfig���� ��Ƽ���� ���� ��������
                if (gameConfig.visualConfig.vertexStencilDissolveMaterial != null)
                {
                    blockStencilMaterial = gameConfig.visualConfig.vertexStencilDissolveMaterial;
                }

                // �� ���ٽ� ��Ƽ���� ���� (���� URP/Lit �� �⺻ ���̴� ��� ����)
                Shader wallShader = Shader.Find("Custom/WallStencilWriter");
                if (wallShader != null)
                {
                    wallStencilMaterial = new Material(wallShader);
                }
            }

            // ���ٽ� ȿ�� �ʱ�ȭ
            vertexStencilEffectView.Initialize(wallStencilMaterial, blockStencilMaterial);

            // �̺�Ʈ ���
            RegisterEvents();
        }


        /// <summary>
        /// �̺�Ʈ ���
        /// </summary>
        private void RegisterEvents()
        {
            gameConfig.gameEvents.onBlockDragStart.RegisterListener(this);
            gameConfig.gameEvents.onBlockDestroy.RegisterListener(this);

            // BoardBlockObject�� ���� �̺�Ʈ �ڵ鷯 ����
            BoardBlockObject.OnGetDestroyParticle += GetDestroyParticle;
            BoardBlockObject.OnGetMaterial += GetMaterial;
            BoardBlockObject.OnGetBoardSize += GetBoardSize;
        }
        /// <summary>
        /// ������ ���̴� ��Ƽ���� �ν��Ͻ� ��ȯ
        /// </summary>
    

      
        // OnDestroy ����
        private void OnDestroy()
        {
            // �̺�Ʈ ����
            if (gameConfig != null)
            {
                gameConfig.gameEvents.onBlockDragStart.UnregisterListener(this);
                gameConfig.gameEvents.onBlockDestroy.UnregisterListener(this);
            }

            BoardBlockObject.OnGetDestroyParticle -= GetDestroyParticle;
            BoardBlockObject.OnGetMaterial -= GetMaterial;
            BoardBlockObject.OnGetBoardSize -= GetBoardSize;

            // ���ٽ� ȿ�� ����
            if (vertexStencilEffectView != null)
            {
                vertexStencilEffectView.Cleanup();
            }



        }

        /// <summary>
        /// ��� �巡�� ���� �̺�Ʈ ó��
        /// </summary>
        public void OnEventRaised(BlockObject block)
        {
            // ����� ���ٽ� ȿ�� ������� ���
            RegisterBlock(block.gameObject);
        }

        /// <summary>
        /// �� ������Ʈ ���
        /// </summary>
        public void RegisterWalls(List<GameObject> wallObjects)
        {
            walls.Clear();
            walls.AddRange(wallObjects);

            // ���ٽ� ����ũ ���� (���� ��� ��� ���)
            if (vertexStencilEffectView != null)
            {
                vertexStencilEffectView.SetupStencilMasking(walls, blocks);
            }

        }

        /// <summary>
        /// ��� ������Ʈ ���
        /// </summary>
        public void RegisterBlock(GameObject blockObject)
        {
            if (!blocks.Contains(blockObject))
            {
                blocks.Add(blockObject);

                // ���ٽ� ����ŷ ���� ������Ʈ
               
            }
        }
        // ����� ���� ������ �� ȣ���� �� �޼���
        public void ApplyWallClippingToBlock(GameObject block, Vector3 wallPosition, LaunchDirection direction)
        {
            if (vertexStencilEffectView == null || block == null) return;

            // �� ���⿡ ���� Ŭ���� ���� ���
            Vector3 clipNormal = vertexStencilEffectView.CalculateClipNormal(direction);

            // ����� �α� �߰�
            Debug.Log($"Wall Direction: {direction}, Clip Normal: {clipNormal}, Wall Position: {wallPosition}");

            // ��Ͽ� Ŭ���� ȿ�� ����
            vertexStencilEffectView.ApplyClippingToBlock(block, wallPosition, clipNormal);
        }
        /// <summary>
        /// ��Ͽ� ���ٽ� �б� ���� ����
        /// </summary>
        public void PrepareBlocksForStencilReading(List<GameObject> blocks)
        {
            if (vertexStencilEffectView == null) return;

            // ���ؽ� ���ٽ� ȿ���� ��� ����
            foreach (var block in blocks)
            {
                vertexStencilEffectView.PrepareBlockForStencilReading(block);
            }
        }
        /// <summary>
        /// �ı� ��ƼŬ ��ȯ (�̺�Ʈ �ڵ鷯)
        /// </summary>
        private ParticleSystem GetDestroyParticle()
        {
            return gameConfig.visualConfig.destroyParticlePrefab;
        }

        /// <summary>
        /// ��Ƽ���� ��ȯ (�̺�Ʈ �ڵ鷯)
        /// </summary>
        private Material GetMaterial(int index)
        {
            if (index >= 0 && index < gameConfig.wallConfig.wallMaterials.Length)
            {
                return gameConfig.wallConfig.wallMaterials[index];
            }
            return null;
        }

        /// <summary>
        /// ���� ũ�� ��ȯ (�̺�Ʈ �ڵ鷯)
        /// </summary>
        private Vector2Int GetBoardSize()
        {
            if (StageController.Instance != null)
            {
                return new Vector2Int(StageController.Instance.boardWidth, StageController.Instance.boardHeight);
            }
            return new Vector2Int(10, 10); // �⺻��
        }

        /// <summary>
        /// �ı� ��ƼŬ ����
        /// </summary>
        public ParticleSystem CreateParticleEffect(
    Vector3 position,       // �̹� ���� ��ġ
    Quaternion rotation,    // �̹� ���� ȸ��
    ColorType colorType,    // ����
    int blockLength)        // ��� ����
        {
            if (gameConfig == null || gameConfig.visualConfig == null ||
                gameConfig.visualConfig.destroyParticlePrefab == null)
            {
                Debug.LogWarning("��ƼŬ �������� �������� �ʾҽ��ϴ�.");
                return null;
            }

            // ��ƼŬ �ν��Ͻ� ���� - �̹� ���� ��ġ�� ȸ�� ���
            ParticleSystem particlePrefab = gameConfig.visualConfig.destroyParticlePrefab;
            ParticleSystem particle = Instantiate(particlePrefab, position, rotation);

            // ũ�� ���� - ��� ���� ���
            particle.transform.localScale = new Vector3(blockLength * 0.4f, 0.5f, blockLength * 0.4f);

            // ��ƼŬ ���� ����
            ParticleSystemRenderer[] renderers = particle.GetComponentsInChildren<ParticleSystemRenderer>();
            foreach (var renderer in renderers)
            {
                Material material = null;

                // ���� Ÿ�Կ� �´� ���� ��������
                if (gameConfig != null && gameConfig.wallConfig != null)
                {
                    int index = (int)colorType;
                    if (index >= 0 && index < gameConfig.wallConfig.wallMaterials.Length)
                    {
                        material = gameConfig.wallConfig.wallMaterials[index];
                    }
                }

                // ���� ����
                if (material != null)
                {
                    renderer.material = material;
                }
            }

            // �ڵ� ���� Ÿ�̸� ����
            float duration = particle.main.duration + particle.main.startLifetime.constant;
            Destroy(particle.gameObject, duration + 0.5f); // ���� �ð� �߰�

            return particle;
        }        
    }

}
