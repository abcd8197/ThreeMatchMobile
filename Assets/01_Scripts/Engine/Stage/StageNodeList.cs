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

        private float ScrollSpeed = 7.0f;
        private SpriteRendererButton[] backgrounds;
        private List<int> _loopCounts = new();
        private List<StageNode> stageNodes = new();
        private AssetManager _assetManager;

        private float _basicPosY;
        private float _backgroundHeight;
        private float _maxY;
        private float _minY;

        private void Awake()
        {
            _assetManager = Main.Instance.GetManager<AssetManager>();
            backgrounds = GetComponentsInChildren<SpriteRendererButton>();
            CreateNodes();

            foreach (var background in backgrounds)
            {
                background.onDrag.AddListener(OnDragBackground);
                _loopCounts.Add(0);
            }

            CalculateWorldPosition();
        }

        private void OnDestroy()
        {
            foreach (var background in backgrounds)
                background.onDrag.RemoveListener(OnDragBackground);
        }

        private void CalculateWorldPosition()
        {
            _basicPosY = transform.position.y;
            _backgroundHeight = Camera.main.orthographicSize * 2;
            _maxY = (_backgroundHeight * (backgrounds.Length - 1)) + _basicPosY;
            _minY = -_backgroundHeight + _basicPosY;
        }

        private void CreateNodes()
        {
            var stageManager = Main.Instance.GetManager<StageManager>();
            for (int i = 0; i < _nodeLocalPositions.Count * backgrounds.Length; i++)
            {
                StageNode node;
                int backgroundIndex = Mathf.FloorToInt(i / _nodeLocalPositions.Count);
                node = _assetManager.GetPrefabInstance(BundleGroup.defaultasset, "StageNode", false, backgrounds[backgroundIndex].transform).GetComponent<StageNode>();
                node.transform.localPosition = _nodeLocalPositions[i % _nodeLocalPositions.Count];
                node.SetData(i, stageManager.IsEnterableStage(i));
                stageNodes.Add(node.GetComponent<StageNode>());
            }
        }

        private void OnDragBackground(Vector2 delta)
        {
            Vector3 d = delta;
            d.x = 0;
            d.z = 0;
            transform.position += d * Time.unscaledDeltaTime * ScrollSpeed;

            if (transform.position.y > _basicPosY)
                transform.position = Vector3.up * _basicPosY;

            bool hasChanged = false;

            foreach (var background in backgrounds)
            {
                if (background.transform.position.y < _minY)
                {
                    background.transform.position += Vector3.up * backgrounds.Length * _backgroundHeight;
                    _loopCounts[background.transform.GetSiblingIndex()]++;
                    hasChanged = true;
                }
                else if (background.transform.position.y > _maxY)
                {
                    background.transform.position -= Vector3.up * backgrounds.Length * _backgroundHeight;
                    _loopCounts[background.transform.GetSiblingIndex()]--;
                    hasChanged = true;
                }
            }

            if (hasChanged)
                UpdateNodeData();
        }

        private void UpdateNodeData()
        {
            int stage = 0;
            int nodePerBackground = _nodeLocalPositions.Count;
            int backgroundCount = backgrounds.Length;
            var stageManager = Main.Instance.GetManager<StageManager>();

            for (int i = 0; i < backgroundCount; i++)
            {
                for (int j = 0; j < nodePerBackground; j++)
                {
                    int nodeIndex = (i * nodePerBackground) + j;
                    stage = j + (i * nodePerBackground) + (_loopCounts[i] * nodePerBackground * backgroundCount);

                    stageNodes[nodeIndex].SetData(stage, stageManager.IsEnterableStage(stage));
                }
            }
        }
    }
}
