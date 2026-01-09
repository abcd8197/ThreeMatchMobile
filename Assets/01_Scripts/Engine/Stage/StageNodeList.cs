using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace ThreeMatch
{
    public class StageNodeList : MonoBehaviour
    {
        private static readonly List<Vector2> _nodeLocalPositions = new()
        {
            new Vector2(-1.2f, 1.517f), new Vector2(-0.25f, 2.6f), new Vector2(1.04f,3.15f), new Vector2(0.14f, 4.27f), new Vector2(-0.942f, 4.733f),
            new Vector2(0.1f, 5.7f), new Vector2(1.104f, 6.303f), new Vector2(0.26f, 7.25f), new Vector2(-0.7f, 7.75f), new Vector2(0.94f, 8.82f)
        };

        [SerializeField] private float ScrollSpeed = 7.0f;
        private SpriteRendererButton[] backgrounds;
        private List<StageNode> stageNodes = new();
        private AssetManager _assetManager;
        private float basicPosY;

        private void Awake()
        {
            _assetManager = Main.Instance.GetManager<AssetManager>();
            backgrounds = GetComponentsInChildren<SpriteRendererButton>();
            CreateNodes();

            foreach (var background in backgrounds)
                background.onDrag.AddListener(OnDragBackground);

            CalculateWorldPosition();
        }

        private void OnDestroy()
        {
            foreach (var background in backgrounds)
                background.onDrag.RemoveListener(OnDragBackground);
        }

        private void CalculateWorldPosition()
        {
            basicPosY = transform.position.y;
        }

        private void CreateNodes()
        {
            for (int i = 0; i < _nodeLocalPositions.Count * 2; i++)
            {
                StageNode node;
                if (i < _nodeLocalPositions.Count)
                    node = _assetManager.GetPrefabInstance(BundleGroup.defaultasset, "StageNode", false, backgrounds[0].transform).GetComponent<StageNode>();
                else
                    node = _assetManager.GetPrefabInstance(BundleGroup.defaultasset, "StageNode", false, backgrounds[1].transform).GetComponent<StageNode>();

                node.transform.localPosition = _nodeLocalPositions[i % _nodeLocalPositions.Count];
                stageNodes.Add(node.GetComponent<StageNode>());
            }
        }

        private void OnDragBackground(Vector2 delta)
        {
            Vector3 d = delta;
            d.x = 0;
            d.z = 0;
            transform.position += d * Time.unscaledDeltaTime * ScrollSpeed;

            if (transform.position.y > basicPosY)
                transform.position = Vector3.up * basicPosY;
        }

        private void UpdateNodeData()
        {

        }
    }
}
