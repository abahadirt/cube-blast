using Blast.Core.Data;
using Blast.GamePresentation.Contract;
using Blast.GameUnity.Registry;
using DG.Tweening;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


namespace Blast.GameUnity.View
{
    public class ShooterReserveView : MonoBehaviour, IShooterReserveView
    {


        private List<List<ShooterView>> _reserveColumns;
        private ShooterViewRegistry _registry;

        [SerializeField]
        private ShooterView _shooterPrefab;
        [SerializeField]
        private CubeColorPalette _palette;

        [Header("Layout")]
        [Tooltip("NOT: Shooter prefab scale deðiþtirilirse bu deðer de deðiþmeli.")]
        [SerializeField] private Vector2 spacing = new Vector2(0.8f, 0.8f);

        [Header("Animation")]
        [SerializeField] private float _repositionDuration = 0.15f;
        [SerializeField] private Ease _repositionEase = Ease.OutQuad;

        private float _startX;
        private float _totalWidth;
        public void Construct(ShooterViewRegistry registry)
        {
            _registry = registry;
        }


        // inspector icin child olayýný sonra eklicem bi olayý yok
        //TODO[P2] :
        //1. ShooterData almamalý
        //2. baþka yaklaþýmlar da incelenmeli. (registrynin logice taþýnmasý(?)[X] gibi)
        // shooterdata yerine ilerde sadece gerekli parametreler (color, ammo) verilirse baya ok.
        public void BuildColumns(IReadOnlyList<IReadOnlyList<ShooterData>> columnsData)
        {
            InitLayout(columnsData.Count);
            Debug.Log($"[ShooterReserveView] Building {columnsData.Count} columns from data.");
            _reserveColumns = new List<List<ShooterView>>(columnsData.Count);

            for (int col = 0; col < columnsData.Count; col++)
            {
                Debug.Log($"[ShooterReserveView] Building shooter col of size {columnsData[col].Count}");
                var reserveCol = new List<ShooterView>();
                for (int row = 0; row < columnsData[col].Count; row++)
                {
                    ShooterData data = columnsData[col][row];

                    // 1. Prefabý Yarat ve Pozisyonunu Ayarla
                    Vector3 spawnPos = GetElementPosition(col, row);
                    ShooterView newShooter = Instantiate(_shooterPrefab, spawnPos, Quaternion.identity, transform);

                    // 2. Rengini/Yazýsýný Ayarla
                    newShooter.SetVisuals(_palette.Get(data.Color), data.Ammo);

                    // 3. EN ÖNEMLÝ ADIM: Memura (Registry) Datadaki ID ile kaydet!
                    _registry.Register(data.Id, newShooter);

                    // 4. Kaydýrma animasyonlarý için kendi fiziksel listene de ekle
                    reserveCol.Add(newShooter);
                }
                _reserveColumns.Add(reserveCol);
            }
        }




        private void InitLayout(int colCount)
        {
            _totalWidth = (colCount - 1) * spacing.x;
            _startX = -_totalWidth / 2f;
        }


        private Vector3 GetElementPosition(int col, int row)
        {
            return transform.position + new Vector3(
                _startX + col * spacing.x,
                -row * spacing.y,
                0f
            );
        }


        /// <summary>
        /// World-space X koordinatýna en yakýn (boþ olmayan) sütunun indeksini döner.
        /// Eþleþme yoksa -1.
        /// </summary>
        public int GetColumnIndexFromWorldX(float worldX)
        {
            if (_reserveColumns == null || _reserveColumns.Count == 0) return -1;

            float localX = worldX - transform.position.x;
            int closestIndex = -1;
            float closestDist = float.MaxValue;

            for (int i = 0; i < _reserveColumns.Count; i++)
            {
                if (_reserveColumns[i].Count == 0) continue;

                float columnX = _startX + i * spacing.x;
                float dist = Mathf.Abs(columnX - localX);

                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestIndex = i;
                }
            }

            return closestIndex;
        }


        private bool IsValidColumn(int columnIndex)
        {
            return _reserveColumns != null && columnIndex >= 0 && columnIndex < _reserveColumns.Count;
        }


        /// <summary>
        /// Sütunun en üstündeki slot'u layout takibinden çýkarýr ve transform'unu döner.
        /// Kalan elemanlar yukarý doðru animasyonla kayar.
        /// Destroy etmez — çaðýran (Adapter), shooter'ý tahtaya fýrlatma vb. amaçla
        /// kullanmaya devam edebilsin diye yaþamaya býrakýr.
        /// </summary>
        public void DetachFirstInColumn(int columnIndex)
        {
            if (!IsValidColumn(columnIndex) || _reserveColumns[columnIndex].Count == 0)
                return;

            _reserveColumns[columnIndex].RemoveAt(0);
        }


        public void PlayShiftAnimation(int columnIndex)
        {
            var column = _reserveColumns[columnIndex];
            for (int row = 0; row < column.Count; row++)
            {
                var t = column[row].transform;
                if (t == null) continue;

                t.DOKill(); //SILINECEK ! ! ! ! !
                t.DOMove(GetElementPosition(columnIndex, row), _repositionDuration)
                 .SetEase(_repositionEase);
            }
        }


    }


}

