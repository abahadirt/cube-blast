using Blast.Core.Data;
using DG.Tweening;
using UnityEngine;
using Blast.GamePresentation.Contract;

namespace Blast.GameUnity.View
{
    public class BoardView : MonoBehaviour, IBoardView
    {
        [Header("Referanslar")]
        [SerializeField] private GameObject cubePrefab;
        [SerializeField] private SpriteRenderer gridBackground;
        [SerializeField] private CubeColorPalette palette;

        [Header("Görsel")]
        [SerializeField] private float gridWidth = 5f;
        [Range(0.1f, 1f)]
        [SerializeField] private float cubeSizeScale = 0.9f;
        [SerializeField] private float dropDuration = 0.15f;

        private CubeView[,] _visualGrid;
        private int _visibleRows;
        private int _columns;

        private float _cellSize;
        private float _cubeSize;
        private float _startOffsetX;
        private float _startOffsetY;

        public int VisibleRows => _visibleRows;

        public void Initialize(int columns, int visibleRows, CubeColor[,] colors, bool[,] activeFlags)
        {
            Debug.Log($"Initializing BoardView with columns: {columns}, visibleRows: {visibleRows}");
            _columns = columns;
            _visibleRows = visibleRows;

            _cellSize = gridWidth / _columns;
            _cubeSize = gridWidth * cubeSizeScale / _columns;
            _startOffsetX = (_columns - 1) * _cellSize / 2f;
            _startOffsetY = _cellSize / 2f;

            BuildVisualGrid(colors, activeFlags);
            AdjustGridBackground();
        }

        private void BuildVisualGrid(CubeColor[,] colors, bool[,] activeFlags)
        {
            _visualGrid = new CubeView[_visibleRows, _columns];
            for (int col = 0; col < _columns; col++)
            {
                for (int v = 0; v < _visibleRows; v++)
                {
                    Vector2 pos = GetCubePosition(col, v);
                    GameObject cubeObj = Instantiate(cubePrefab, pos, Quaternion.identity, transform);
                    cubeObj.transform.localScale = new Vector3(_cubeSize, _cubeSize, 1f);
                    cubeObj.name = $"Visual_{v}_{col}";

                    CubeView cube = cubeObj.GetComponent<CubeView>();
                    cube.Init(palette.Get(colors[v, col]));
                    cubeObj.SetActive(activeFlags[v, col]);

                    _visualGrid[v, col] = cube;
                }
            }
        }

        //TODO[P3]: dokill yerine aimation queue kullanılabilir...
        public void RemoveCubeFromBottom(int col, CubeColor? newTopColor)
        {
            CubeView recycled = _visualGrid[0, col];

            for (int v = 1; v < _visibleRows; v++)
            {
                _visualGrid[v - 1, col] = _visualGrid[v, col];
                Vector2 targetPos = GetCubePosition(col, v - 1);
                _visualGrid[v - 1, col].DOKill();
                _visualGrid[v - 1, col].transform.DOMove(targetPos, dropDuration);
            }

            recycled.transform.DOKill();
            if (newTopColor.HasValue)
            {
                recycled.SetColor(palette.Get(newTopColor.Value));
                recycled.transform.position = GetCubePosition(col, _visibleRows - 1);
                recycled.gameObject.SetActive(true);
            }
            else
            {
                recycled.gameObject.SetActive(false);
            }

            _visualGrid[_visibleRows - 1, col] = recycled;
        }

        public Vector3 GetBottomCubePosition(int col) => _visualGrid[0, col].transform.position;

        private Vector2 GetCubePosition(int col, int visualRow)
        {
            float x = (col * _cellSize) - _startOffsetX;
            float y = (visualRow * _cellSize) + _startOffsetY;
            return new Vector2(x, y);
        }

        private void AdjustGridBackground()
        {
            Debug.Log($"visible rows: {_visibleRows}");
            int displayRows = _visibleRows;
            float bgWidth = _columns * _cellSize + _cellSize * 0.3f;
            float bgHeight = displayRows * _cellSize + _cellSize * 0.3f;
            float bgY = (displayRows - 1) * _cellSize / 2f + _cellSize / 2f;

            gridBackground.transform.localPosition = new Vector3(0, bgY, 0);
            gridBackground.transform.localScale = new Vector3(bgWidth, bgHeight, 1f);
        }

        public (float startX, float startY, float offset) GetGridSpawnParameters()
        {
            // col=0 ve visualRow=0 olduğu için formülün sadeleşmiş hali:
            return (-_startOffsetX, _startOffsetY, _cellSize);
        }


    }
}