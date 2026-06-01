using Blast.Core.Data;
using Blast.Level;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class GridEditorWindow : EditorWindow
{
    // ==================== DÜZENLEME MODLARI ====================
    private enum EditMode { Paint, Erase, Select }
    private EditMode currentMode = EditMode.Paint;

    // Seçim Verileri
    private Vector2Int selectStartCell = new Vector2Int(-1, -1);
    private Vector2Int selectEndCell = new Vector2Int(-1, -1);
    private bool isSelecting = false;

    // ==================== LEVEL KİMLİĞİ ====================
    private int levelNumber = 1;

    // ==================== GRID VERİSİ ====================
    private int columns = 10;
    private int rows = 30;
    private int visibleRows = 10;
    private int launchTrayCapacity = 5;

    private int[,] grid;
    private bool gridInitialized = false;

    // ==================== QUEUE VERİSİ ====================
    private List<ReserveColumnData> currentReserves = new List<ReserveColumnData>();
    private Vector2 scrollPosQueues;

    // ==================== BOYAMA ====================
    private int selectedColorIndex = 0;
    private bool isPainting = false;

    // ==================== GÖRSEL ====================
    private float cellSize = 24f;
    private Vector2 scrollPos;

 
    private static readonly string[] colorChars = { "R", "B", "G", "Y", "S" };
    private static readonly Color[] paletteColors =
    {
        new Color(0.90f, 0.22f, 0.22f), // Red
        new Color(0.22f, 0.42f, 0.90f), // Blue
        new Color(0.22f, 0.78f, 0.32f), // Green
        new Color(0.95f, 0.85f, 0.12f), // Yellow
        new Color(0.05f, 0.05f, 0.05f)  // Black (Siyah)
    };

    // Boş hücre rengi. Siyah küpten ayırt edilebilmesi için bilerek nötr-açık gri seçildi.
    private static readonly Color emptyCellColor = new Color(0.20f, 0.20f, 0.22f);

    // Tek doğruluk kaynağı: tüm renk sayısı / mod döngüleri buradan beslenir.
    private int ColorCount => paletteColors.Length;

    // ==================== KLASÖR YOLLARI ====================
    private const string JSON_FOLDER = "Assets/Levels";

    [MenuItem("Tools/Grid Level Editor")]
    public static void ShowWindow()
    {
        var window = GetWindow<GridEditorWindow>("JSON Level Editor");
        window.minSize = new Vector2(450, 550);
    }

    void OnEnable()
    {
        // Renk tanımlarının senkron olduğunu doğrula (enum'a ekleyip editör dizisini unutma vakası).
        if (colorChars.Length != paletteColors.Length ||
            colorChars.Length != Enum.GetValues(typeof(CubeColor)).Length)
        {
            Debug.LogWarning("[GridEditor] Renk tanımları senkron değil. " +
                "colorChars, paletteColors ve CubeColor enum'u aynı uzunlukta/sırada olmalı.");
        }

        if (!gridInitialized)
        {
            visibleRows = columns;
            ClearGrid();
        }
    }

    void ClearGrid()
    {
        grid = new int[rows, columns];
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < columns; c++)
                grid[r, c] = -1;

        gridInitialized = true;
        ClearSelection();
    }

    void ClearSelection()
    {
        selectStartCell = new Vector2Int(-1, -1);
        selectEndCell = new Vector2Int(-1, -1);
    }

    void OnGUI()
    {
        DrawLevelHeader();
        DrawToolbar();
        DrawColorPalette();
        DrawStats();
        EditorGUILayout.Space(4);
        DrawGridArea();
        EditorGUILayout.Space(4);
        DrawQueuesArea();
        EditorGUILayout.Space(4);
        DrawActionButtons();
    }

    // ==================== LEVEL HEADER ====================
    void DrawLevelHeader()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        EditorGUILayout.LabelField("Level:", GUILayout.Width(38));
        levelNumber = EditorGUILayout.IntField(levelNumber, GUILayout.Width(50));
        levelNumber = Mathf.Max(1, levelNumber);

        EditorGUILayout.LabelField(GetLevelName(), EditorStyles.boldLabel);

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("JSON Yükle", EditorStyles.toolbarButton, GUILayout.Width(75)))
            LoadLevel();

        EditorGUILayout.EndHorizontal();
    }

    // ==================== TOOLBAR ====================
    void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        EditorGUILayout.LabelField("Sütun:", GUILayout.Width(42));
        int newCols = EditorGUILayout.DelayedIntField(columns, GUILayout.Width(36));

        EditorGUILayout.LabelField("Satır:", GUILayout.Width(36));
        int newRows = EditorGUILayout.DelayedIntField(rows, GUILayout.Width(36));

        EditorGUILayout.LabelField("Görünür:", GUILayout.Width(52));
        visibleRows = EditorGUILayout.IntField(visibleRows, GUILayout.Width(36));
        visibleRows = Mathf.Clamp(visibleRows, 1, rows);

        EditorGUILayout.LabelField("Tepsi (Tray):", GUILayout.Width(70));
        launchTrayCapacity = EditorGUILayout.IntField(launchTrayCapacity, GUILayout.Width(36));

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Temizle", EditorStyles.toolbarButton, GUILayout.Width(52)))
        {
            if (EditorUtility.DisplayDialog("Temizle", "Tüm grid silinsin mi?", "Evet", "Hayır"))
            {
                ClearGrid();
                currentReserves.Clear();
            }
        }

        EditorGUILayout.EndHorizontal();

        newCols = Mathf.Clamp(newCols, 2, 20);
        newRows = Mathf.Clamp(newRows, 2, 500);

        if (newCols != columns || newRows != rows)
        {
            if (newCols != columns)
                visibleRows = newCols;

            ResizeGrid(newCols, newRows);
        }
    }

    // ==================== RENK PALETİ VE ARAÇLAR ====================
    void DrawColorPalette()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Renk:", GUILayout.Width(38));

        // Renk Butonları
        for (int i = 0; i < ColorCount; i++)
        {
            bool isSelected = (currentMode == EditMode.Paint) && (selectedColorIndex == i);
            var prevBg = GUI.backgroundColor;
            GUI.backgroundColor = isSelected ? Color.white : paletteColors[i];

            var style = new GUIStyle(GUI.skin.button);
            style.normal.textColor = isSelected ? paletteColors[i] : Color.white;
            if (isSelected) style.fontStyle = FontStyle.Bold;

            if (GUILayout.Button(colorChars[i], style, GUILayout.Width(36), GUILayout.Height(26)))
            {
                selectedColorIndex = i;
                currentMode = EditMode.Paint;
                ClearSelection();
            }
            GUI.backgroundColor = prevBg;
        }

        GUILayout.Space(8);

        // Silgi Butonu
        var prevBg2 = GUI.backgroundColor;
        GUI.backgroundColor = (currentMode == EditMode.Erase) ? Color.white : Color.gray;
        var eraserStyle = new GUIStyle(GUI.skin.button);
        if (currentMode == EditMode.Erase) eraserStyle.fontStyle = FontStyle.Bold;
        if (GUILayout.Button("Silgi", eraserStyle, GUILayout.Width(42), GUILayout.Height(26)))
        {
            currentMode = EditMode.Erase;
            ClearSelection();
        }

        GUILayout.Space(8);

        // Seçim Butonu
        GUI.backgroundColor = (currentMode == EditMode.Select) ? Color.cyan : Color.gray;
        var selectStyle = new GUIStyle(GUI.skin.button);
        if (currentMode == EditMode.Select) selectStyle.fontStyle = FontStyle.Bold;
        if (GUILayout.Button("Seçim", selectStyle, GUILayout.Width(50), GUILayout.Height(26)))
        {
            currentMode = EditMode.Select;
        }

        GUI.backgroundColor = prevBg2;

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    // ==================== İSTATİSTİKLER ====================
    void DrawStats()
    {
        // 1. Genel Grid İstatistikleri
        int empty = 0, total = 0;
        int[] counts = new int[ColorCount];
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                int val = grid[r, c];
                if (val >= 0 && val < ColorCount) { counts[val]++; total++; }
                else empty++;
            }
        }

        EditorGUILayout.BeginVertical(GUI.skin.box);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Tüm Grid Toplam: " + total, EditorStyles.miniBoldLabel, GUILayout.Width(125));
        for (int i = 0; i < ColorCount; i++)
        {
            var style = new GUIStyle(EditorStyles.miniLabel);
            style.normal.textColor = paletteColors[i];
            EditorGUILayout.LabelField($"{colorChars[i]}:{counts[i]}", style, GUILayout.Width(38));
        }
        EditorGUILayout.LabelField($"Boş:{empty}", EditorStyles.miniLabel, GUILayout.Width(50));
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        // 2. Seçili Alan İstatistikleri (Sadece seçim varsa gösterilir)
        if (currentMode == EditMode.Select && selectStartCell.x != -1)
        {
            int selEmpty = 0, selTotal = 0;
            int[] selCounts = new int[ColorCount];

            int minC = Mathf.Min(selectStartCell.x, selectEndCell.x);
            int maxC = Mathf.Max(selectStartCell.x, selectEndCell.x);
            int minR = Mathf.Min(selectStartCell.y, selectEndCell.y);
            int maxR = Mathf.Max(selectStartCell.y, selectEndCell.y);

            for (int r = minR; r <= maxR; r++)
            {
                for (int c = minC; c <= maxC; c++)
                {
                    if (r >= 0 && r < rows && c >= 0 && c < columns)
                    {
                        int val = grid[r, c];
                        if (val >= 0 && val < ColorCount) { selCounts[val]++; selTotal++; }
                        else selEmpty++;
                    }
                }
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Seçili Alan Toplam: " + selTotal, EditorStyles.miniBoldLabel, GUILayout.Width(125));
            for (int i = 0; i < ColorCount; i++)
            {
                var style = new GUIStyle(EditorStyles.miniLabel);
                style.normal.textColor = paletteColors[i];
                EditorGUILayout.LabelField($"{colorChars[i]}:{selCounts[i]}", style, GUILayout.Width(38));
            }
            EditorGUILayout.LabelField($"Boş:{selEmpty}", EditorStyles.miniLabel, GUILayout.Width(50));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();
    }

    // ==================== GRID ÇİZİMİ ====================
    void DrawGridArea()
    {
        if (!gridInitialized) return;

        float gridWidth = columns * cellSize + 40;
        float gridHeight = rows * cellSize;

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandHeight(true));
        Rect gridRect = GUILayoutUtility.GetRect(gridWidth, gridHeight);
        Event e = Event.current;

        for (int displayRow = 0; displayRow < rows; displayRow++)
        {
            int dataRow = rows - 1 - displayRow;

            // Görünür Sınır Çizgisi
            if (dataRow == visibleRows - 1)
            {
                float lineY = gridRect.y + displayRow * cellSize + cellSize;
                Handles.color = new Color(1f, 0.4f, 0.1f, 0.8f);
                Handles.DrawLine(
                    new Vector3(gridRect.x + 32, lineY),
                    new Vector3(gridRect.x + 32 + columns * cellSize, lineY));

                Rect lr = new Rect(gridRect.x + 34 + columns * cellSize, lineY - 8, 80, 16);
                var ls = new GUIStyle(EditorStyles.miniLabel);
                ls.normal.textColor = new Color(1f, 0.4f, 0.1f);
                EditorGUI.LabelField(lr, "▲ görünür", ls);
            }

            // Satır numarası
            Rect numRect = new Rect(gridRect.x, gridRect.y + displayRow * cellSize, 30, cellSize);
            var numStyle = new GUIStyle(EditorStyles.miniLabel);
            numStyle.alignment = TextAnchor.MiddleRight;
            EditorGUI.LabelField(numRect, dataRow.ToString(), numStyle);

            for (int col = 0; col < columns; col++)
            {
                Rect cellRect = new Rect(
                    gridRect.x + 32 + col * cellSize,
                    gridRect.y + displayRow * cellSize,
                    cellSize - 1, cellSize - 1);

                // Renk Çizimi
                int val = grid[dataRow, col];
                Color cc = (val >= 0 && val < ColorCount)
                    ? paletteColors[val]
                    : emptyCellColor;

                EditorGUI.DrawRect(cellRect, cc);

                // Sınır Çizgileri
                Handles.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
                Handles.DrawLine(new Vector3(cellRect.x, cellRect.y), new Vector3(cellRect.xMax, cellRect.y));
                Handles.DrawLine(new Vector3(cellRect.x, cellRect.y), new Vector3(cellRect.x, cellRect.yMax));

                // Harf İçi Çizim
                if (val >= 0 && cellSize >= 16)
                {
                    var cs = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
                    cs.normal.textColor = new Color(1, 1, 1, 0.6f);
                    cs.fontSize = Mathf.Max(8, (int)(cellSize * 0.38f));
                    EditorGUI.LabelField(cellRect, colorChars[val], cs);
                }

                // Seçim Vurgusu Çizimi (Highlight)
                if (currentMode == EditMode.Select && selectStartCell.x != -1)
                {
                    int minC = Mathf.Min(selectStartCell.x, selectEndCell.x);
                    int maxC = Mathf.Max(selectStartCell.x, selectEndCell.x);
                    int minR = Mathf.Min(selectStartCell.y, selectEndCell.y);
                    int maxR = Mathf.Max(selectStartCell.y, selectEndCell.y);

                    if (col >= minC && col <= maxC && dataRow >= minR && dataRow <= maxR)
                    {
                        EditorGUI.DrawRect(cellRect, new Color(0.2f, 0.7f, 1f, 0.4f)); // Yarı saydam mavi
                    }
                }

                // Mouse Etkileşimleri
                if (cellRect.Contains(e.mousePosition))
                {
                    if (e.type == EventType.MouseDown && e.button == 0)
                    {
                        if (currentMode == EditMode.Select)
                        {
                            isSelecting = true;
                            selectStartCell = new Vector2Int(col, dataRow);
                            selectEndCell = selectStartCell;
                        }
                        else
                        {
                            isPainting = true;
                            PaintCell(dataRow, col);
                        }
                        e.Use();
                    }
                    else if (e.type == EventType.MouseDrag && e.button == 0)
                    {
                        if (currentMode == EditMode.Select && isSelecting)
                        {
                            selectEndCell = new Vector2Int(col, dataRow);
                            Repaint(); // Sürüklerken anlık güncellenmesi için
                        }
                        else if (isPainting)
                        {
                            PaintCell(dataRow, col);
                        }
                        e.Use();
                    }
                }
            }
        }

        if (e.type == EventType.MouseUp && e.button == 0)
        {
            isPainting = false;
            isSelecting = false;
        }

        EditorGUILayout.EndScrollView();
    }

    void PaintCell(int row, int col)
    {
        grid[row, col] = (currentMode == EditMode.Erase) ? -1 : selectedColorIndex;
        Repaint();
    }

    // ==================== QUEUE ALANI ====================
    void DrawQueuesArea()
    {
        EditorGUILayout.LabelField("Shooter Kuyrukları (Grid sütun sayısından bağımsızdır):", EditorStyles.boldLabel);

        // --- Kuyruk İstatistikleri Başlangıç ---
        int[] qCounts = new int[ColorCount];
        int qTotalAmmo = 0;
        foreach (var reserveCol in currentReserves)
        {
            if (reserveCol.shooters != null)
            {
                foreach (var shooter in reserveCol.shooters)
                {
                    int cIdx = (int)shooter.color;
                    if (cIdx >= 0 && cIdx < ColorCount)
                    {
                        qCounts[cIdx] += shooter.ammo;
                        qTotalAmmo += shooter.ammo;
                    }
                }
            }
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Kuyruklardaki Toplam Mermi: " + qTotalAmmo, EditorStyles.miniBoldLabel, GUILayout.Width(170));
        for (int i = 0; i < ColorCount; i++)
        {
            var style = new GUIStyle(EditorStyles.miniLabel);
            style.normal.textColor = paletteColors[i];
            EditorGUILayout.LabelField($"{colorChars[i]}:{qCounts[i]}", style, GUILayout.Width(45));
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        // --- Kuyruk İstatistikleri Bitiş ---

        scrollPosQueues = EditorGUILayout.BeginScrollView(scrollPosQueues, GUILayout.Height(150));
        EditorGUILayout.BeginHorizontal();

        // Mevcut kuyrukları yan yana listele
        for (int c = 0; c < currentReserves.Count; c++)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(85));

            // Kuyruk Başlığı ve Silme
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Kuyruk {c}", EditorStyles.miniBoldLabel, GUILayout.Width(55));
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                currentReserves.RemoveAt(c);
                c--;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                continue;
            }
            EditorGUILayout.EndHorizontal();

            var shooters = currentReserves[c].shooters;
            if (shooters == null)
            {
                shooters = new List<ShooterSetupData>();
                currentReserves[c].shooters = shooters;
            }

            for (int i = 0; i < shooters.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                var currentShooter = shooters[i];

                var prevColor = GUI.backgroundColor;
                int colorInt = (int)currentShooter.color;
                GUI.backgroundColor = (colorInt >= 0 && colorInt < ColorCount) ? paletteColors[colorInt] : Color.white;

                if (GUILayout.Button(colorChars[colorInt], GUILayout.Width(22)))
                {
                    currentShooter.color = (CubeColor)((colorInt + 1) % ColorCount);
                }
                GUI.backgroundColor = prevColor;

                currentShooter.ammo = EditorGUILayout.DelayedIntField(currentShooter.ammo, GUILayout.Width(26));

                shooters[i] = currentShooter;

                if (GUILayout.Button("x", GUILayout.Width(18)))
                {
                    shooters.RemoveAt(i);
                    i--;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("+ Shooter", GUILayout.Width(80)))
            {
                shooters.Add(new ShooterSetupData { color = CubeColor.Red, ammo = 10 });
            }

            EditorGUILayout.EndVertical();
        }

        // Yeni Kuyruk Ekleme Butonu
        EditorGUILayout.BeginVertical(GUILayout.Width(90));
        if (GUILayout.Button("YENİ\nKUYRUK\nEKLE", GUILayout.Height(50)))
        {
            currentReserves.Add(new ReserveColumnData { shooters = new List<ShooterSetupData>() });
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndScrollView();
    }

    // ==================== JSON KAYDET / YÜKLE ====================
    void DrawActionButtons()
    {
        var style = new GUIStyle(GUI.skin.button);
        style.fontStyle = FontStyle.Bold;
        style.fixedHeight = 32;

        if (GUILayout.Button("JSON Olarak Kaydet", style))
            SaveLevelToJson();
    }

    void SaveLevelToJson()
    {
        EnsureFolders();

        string levelName = GetLevelName();
        string jsonPath = $"{JSON_FOLDER}/{levelName}.json";

        int topRow = rows - 1;
        while (topRow >= visibleRows && IsRowEmpty(topRow)) topRow--;
        int finalRowCount = topRow + 1;

        GridRow[] gridRows = new GridRow[finalRowCount];

        for (int r = 0; r < finalRowCount; r++)
        {
            int dataRow = finalRowCount - 1 - r;

            gridRows[r] = new GridRow();
            gridRows[r].colors = new CubeColor[columns];

            for (int c = 0; c < columns; c++)
            {
                int val = grid[dataRow, c];
                gridRows[r].colors[c] = val >= 0 ? (CubeColor)val : (CubeColor)(-1);
            }
        }

        LevelData levelData = new LevelData
        {
            columns = this.columns,
            totalRows = finalRowCount,
            visibleRows = this.visibleRows,
            launchTrayCapacity = this.launchTrayCapacity,
            rows = gridRows,
            reserveColumns = this.currentReserves
        };


        var settings = new JsonSerializerSettings { Formatting = Formatting.Indented };
        settings.Converters.Add(new StringEnumConverter());
        settings.Converters.Add(new InlineColorArrayConverter());

        string jsonContent = JsonConvert.SerializeObject(levelData, settings);
        File.WriteAllText(jsonPath, jsonContent);

        AssetDatabase.Refresh();
        Debug.Log($"[GridEditor] Başarıyla JSON olarak kaydedildi → {jsonPath}");
    }

    void LoadLevel()
    {
        string jsonPath = $"{JSON_FOLDER}/{GetLevelName()}.json";
        if (!File.Exists(jsonPath))
        {
            EditorUtility.DisplayDialog("Bulunamadı", $"{GetLevelName()}.json bulunamadı.", "Tamam");
            return;
        }

        string jsonContent = File.ReadAllText(jsonPath);
        var settings = new JsonSerializerSettings();
        settings.Converters.Add(new StringEnumConverter());
        LevelData data = JsonConvert.DeserializeObject<LevelData>(jsonContent, settings);

        columns = data.columns;
        visibleRows = data.visibleRows;
        launchTrayCapacity = data.launchTrayCapacity;
        int loadedRows = data.totalRows;

        if (loadedRows > rows) rows = loadedRows;

        ClearGrid();

        for (int r = 0; r < loadedRows; r++)
        {
            int dataRow = loadedRows - 1 - r;

            for (int c = 0; c < columns; c++)
            {
                CubeColor color = data.rows[r].colors[c];
                int val = (int)color;
                grid[dataRow, c] = val >= 0 && val < ColorCount ? val : -1;
            }
        }

        if (data.reserveColumns != null)
        {
            currentReserves = new List<ReserveColumnData>(data.reserveColumns);
        }
        else
        {
            currentReserves = new List<ReserveColumnData>();
        }

        Repaint();
        Debug.Log($"[GridEditor] JSON'dan yüklendi: {jsonPath}");
    }

    // ==================== YARDIMCI METOTLAR ====================
    string GetLevelName() => $"Level_{levelNumber:D3}";

    void ResizeGrid(int newCols, int newRows)
    {
        var old = grid;
        int oldRows = rows;
        int oldCols = columns;

        columns = newCols;
        rows = newRows;

        ClearGrid();

        for (int r = 0; r < Mathf.Min(oldRows, rows); r++)
            for (int c = 0; c < Mathf.Min(oldCols, columns); c++)
                grid[r, c] = old[r, c];
    }

    bool IsRowEmpty(int row)
    {
        for (int c = 0; c < columns; c++)
            if (grid[row, c] >= 0) return false;
        return true;
    }

    void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Levels"))
            AssetDatabase.CreateFolder("Assets", "Levels");
    }
}

// Newtonsoft kütüphanesine sadece CubeColor dizilerini tek satırda yazmasını söyler
public class InlineColorArrayConverter : Newtonsoft.Json.JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Blast.Core.Data.CubeColor[]);
    }

    public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
    {
        var array = (Blast.Core.Data.CubeColor[])value;

        var textWriter = writer as Newtonsoft.Json.JsonTextWriter;
        var previousFormatting = Newtonsoft.Json.Formatting.Indented;

        if (textWriter != null)
        {
            previousFormatting = textWriter.Formatting;
            textWriter.Formatting = Newtonsoft.Json.Formatting.None; // Yan yana (Inline) yazma moduna geç
        }

        writer.WriteStartArray();
        foreach (var color in array)
        {
            serializer.Serialize(writer, color);
        }
        writer.WriteEndArray();

        if (textWriter != null)
        {
            textWriter.Formatting = previousFormatting; // Eski formata geri dön
        }
    }

    public override object ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override bool CanRead => false;
}