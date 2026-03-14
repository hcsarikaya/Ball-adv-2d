// ─────────────────────────────────────────────

#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class LevelEditorWindow : EditorWindow
{
    // ── Level Settings ────────────────────────
    string _levelId = "1-01";
    string _mode = "advancing";
    int _columns = 11;
    float _cellSize = 1f;

    // ── Block Prefabs ─────────────────────────
    GameObject _squarePrefab;
    GameObject _trianglePrefab;
    GameObject _dynamitePrefab;
    GameObject _spikePrefab;
    GameObject _metalPrefab;

    // ── Grid Preview ──────────────────────────
    bool _showGrid = true;
    Color _gridColor = new Color(0.4f, 0.8f, 1f, 0.3f);

    // ── Scroll ────────────────────────────────
    Vector2 _scroll;

    // ─────────────────────────────────────────
    [MenuItem("Tools/Level Editor")]
    public static void OpenWindow()
    {
        var window = GetWindow<LevelEditorWindow>("Level Editor");
        window.minSize = new Vector2(300, 400);
    }

    // ─────────────────────────────────────────
    void OnGUI()
    {
        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        DrawLevelSettings();
        EditorGUILayout.Space(10);
        DrawPrefabSettings();
        EditorGUILayout.Space(10);
        DrawGridSettings();
        EditorGUILayout.Space(10);
        DrawBlockList();
        EditorGUILayout.Space(10);
        DrawActions();

        EditorGUILayout.EndScrollView();
    }

    // ── Section: Prefab Settings ──────────────
    void DrawPrefabSettings()
    {
        EditorGUILayout.LabelField("Block Prefabs", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        _squarePrefab = (GameObject)EditorGUILayout.ObjectField("Square", _squarePrefab, typeof(GameObject), false);
        _trianglePrefab = (GameObject)EditorGUILayout.ObjectField("Triangle", _trianglePrefab, typeof(GameObject), false);
        _dynamitePrefab = (GameObject)EditorGUILayout.ObjectField("Dynamite", _dynamitePrefab, typeof(GameObject), false);
        _spikePrefab = (GameObject)EditorGUILayout.ObjectField("Spike", _spikePrefab, typeof(GameObject), false);
        _metalPrefab = (GameObject)EditorGUILayout.ObjectField("Metal", _metalPrefab, typeof(GameObject), false);

        EditorGUI.indentLevel--;
    }

    // ── Section: Level Settings ───────────────
    void DrawLevelSettings()
    {
        EditorGUILayout.LabelField("Level Settings", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        _levelId = EditorGUILayout.TextField("Level ID", _levelId);

        // Mode dropdown
        int modeIndex = _mode == "advancing" ? 0 : 1;
        modeIndex = EditorGUILayout.Popup("Mode", modeIndex, new[] { "Advancing", "Fixed" });
        _mode = modeIndex == 0 ? "advancing" : "fixed";

        EditorGUI.indentLevel--;
    }

    void DrawGridSettings()
    {
        EditorGUILayout.LabelField("Grid Settings", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        _columns = EditorGUILayout.IntSlider("Columns", _columns, 3, 15);
        _showGrid = EditorGUILayout.Toggle("Show Grid", _showGrid);

        if (_showGrid)
            _gridColor = EditorGUILayout.ColorField("Grid Color", _gridColor);

        // Read-only: show computed cell size for reference
        Camera cam = Camera.main;
        if (cam != null)
        {
            float fitCellSize = (cam.orthographicSize * cam.aspect * 2f) / _columns;
            EditorGUILayout.LabelField("Cell Size (auto)", $"{fitCellSize:F3} units");
        }

        EditorGUI.indentLevel--;
    }

    // ── Section: Block List ───────────────────
    void DrawBlockList()
    {
        var markers = FindBlockMarkers();

        EditorGUILayout.LabelField($"Blocks in Scene ({markers.Count})", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        if (markers.Count == 0)
        {
            EditorGUILayout.HelpBox(
                "No blocks found. Add GameObjects with a BlockMarker component to the scene.",
                MessageType.Info
            );
        }
        else
        {
            // Header row
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Name", GUILayout.Width(120));
            EditorGUILayout.LabelField("Type", GUILayout.Width(70));
            EditorGUILayout.LabelField("Health", GUILayout.Width(50));
            EditorGUILayout.LabelField("Col/Row", GUILayout.Width(80));
            EditorGUILayout.LabelField("Rotation", GUILayout.Width(60));
            EditorGUILayout.EndHorizontal();

            foreach (var marker in markers)
            {
                var (col, row) = WorldToGrid(marker.transform.position);
                float rotation = marker.transform.eulerAngles.z;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(marker.gameObject.name, GUILayout.Width(120));
                EditorGUILayout.LabelField(marker.blockType, GUILayout.Width(70));
                EditorGUILayout.LabelField(marker.health.ToString(), GUILayout.Width(50));
                EditorGUILayout.LabelField($"{col}, {row}", GUILayout.Width(80));
                EditorGUILayout.LabelField($"{rotation:F0}°", GUILayout.Width(60));

                // Click to select in scene
                if (GUILayout.Button("Select", GUILayout.Width(55)))
                    Selection.activeGameObject = marker.gameObject;

                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUI.indentLevel--;
    }

    // ── Section: Action Buttons ───────────────
    void DrawActions()
    {
        var markers = FindBlockMarkers();

        EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

        // Snap all blocks to grid
        if (GUILayout.Button("⊞  Snap All Blocks to Grid", GUILayout.Height(30)))
            SnapAllToGrid(markers);

        EditorGUILayout.Space(4);

        // Export
        GUI.backgroundColor = new Color(0.4f, 0.9f, 0.4f);
        if (GUILayout.Button("Export Level to JSON", GUILayout.Height(36)))
            ExportLevel(markers);

        EditorGUILayout.Space(4);

        // Test 
        GUI.backgroundColor = new Color(0.4f, 0.7f, 1f);
        if (GUILayout.Button("▶  Test Level", GUILayout.Height(36)))
            TestLevel();

        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space(4);

        // Import
        if (GUILayout.Button("Import JSON → Place Blocks", GUILayout.Height(30)))
            ImportLevel();
    }

    // ─────────────────────────────────────────
    // Camera Bounds Helper
    // ─────────────────────────────────────────

    // Cell size is derived from camera width / column count so the
    // grid fills the full camera width exactly. Origin starts at
    // the bottom-left of the camera view.
    (float originX, float originY, float fitCellSize) GetCameraAlignedGrid()
    {
        Camera cam = Camera.main;
        if (cam == null)
            return (-(_columns * _cellSize) / 2f, 0f, _cellSize);

        float camHalfHeight = cam.orthographicSize;
        float camHalfWidth = camHalfHeight * cam.aspect;
        float fitCellSize = (camHalfWidth * 2f) / _columns;
        float originX = cam.transform.position.x - camHalfWidth;
        float originY = cam.transform.position.y - camHalfHeight;

        return (originX, originY, fitCellSize);
    }

    // ─────────────────────────────────────────
    // Grid Helpers
    // ─────────────────────────────────────────

    (int col, int row) WorldToGrid(Vector3 worldPos)
    {
        var (originX, originY, fitCellSize) = GetCameraAlignedGrid();

        int col = Mathf.RoundToInt((worldPos.x - originX - fitCellSize / 2f) / fitCellSize);
        int row = Mathf.RoundToInt((worldPos.y - originY - fitCellSize / 2f) / fitCellSize);

        return (col, row);
    }

    Vector3 GridToWorld(int col, int row)
    {
        var (originX, originY, fitCellSize) = GetCameraAlignedGrid();

        float x = originX + fitCellSize / 2f + col * fitCellSize;
        float y = originY + fitCellSize / 2f + row * fitCellSize;

        return new Vector3(x, y, 0f);
    }

    void SnapAllToGrid(List<BlockMarker> markers)
    {
        foreach (var marker in markers)
        {
            var (col, row) = WorldToGrid(marker.transform.position);
            marker.transform.position = GridToWorld(col, row);
        }

        Debug.Log($"[LevelEditor] Snapped {markers.Count} blocks to grid.");
    }

    // ─────────────────────────────────────────
    // Export
    // ─────────────────────────────────────────

    void ExportLevel(List<BlockMarker> markers)
    {
        if (markers.Count == 0)
        {
            EditorUtility.DisplayDialog("Export Failed", "No blocks in scene to export.", "OK");
            return;
        }

        // Build level data
        var level = new LevelData
        {
            levelId = _levelId,
            mode = _mode,
            columns = _columns,
            cellSize = _cellSize,
        };

        foreach (var marker in markers)
        {
            var (col, row) = WorldToGrid(marker.transform.position);
            level.blocks.Add(new BlockData
            {
                blockType = marker.blockType,
                col = col,
                row = row,
                health = marker.health,
                rotation = marker.transform.eulerAngles.z,
            });
        }

        // Serialize to JSON
        string json = JsonUtility.ToJson(level, prettyPrint: true);
        string folder = Path.Combine(Application.streamingAssetsPath, "Levels");
        string filePath = Path.Combine(folder, $"{_levelId}.json");

        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        File.WriteAllText(filePath, json);
        AssetDatabase.Refresh();

        Debug.Log($"[LevelEditor] Exported → {filePath}");
        EditorUtility.DisplayDialog("Export Complete", $"Saved to:\nStreamingAssets/Levels/{_levelId}.json", "OK");
    }

    // ─────────────────────────────────────────
    // Test
    // ─────────────────────────────────────────

    void TestLevel()
    {
        // Auto-export first so the latest layout is saved
        ExportLevel(FindBlockMarkers());

        // Store which level to load so GameManager picks it up on Play
        EditorPrefs.SetString("TestLevelId", _levelId);

        EditorApplication.isPlaying = true;
    }

    // ─────────────────────────────────────────
    // Import  —  load a JSON and place blocks
    // ─────────────────────────────────────────

    void ImportLevel()
    {
        string path = EditorUtility.OpenFilePanel("Open Level JSON",
            Path.Combine(Application.streamingAssetsPath, "Levels"), "json");

        if (string.IsNullOrEmpty(path)) return;

        string json = File.ReadAllText(path);
        LevelData level = JsonUtility.FromJson<LevelData>(json);

        // Apply settings
        _levelId = level.levelId;
        _mode = level.mode;
        _columns = level.columns;

        // Clear existing markers
        foreach (var marker in FindBlockMarkers())
            DestroyImmediate(marker.gameObject);

        // Spawn blocks using correct prefab per type
        foreach (var blockData in level.blocks)
        {
            GameObject prefab = GetPrefabForType(blockData.blockType);

            GameObject go;

            if (prefab != null)
            {
                go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            }
            else
            {
                // Fallback: plain GameObject if prefab not assigned
                Debug.LogWarning($"[LevelEditor] No prefab assigned for type '{blockData.blockType}', using empty GameObject.");
                go = new GameObject(blockData.blockType);
                go.AddComponent<BlockMarker>();
            }

            go.name = $"{blockData.blockType}_{blockData.col}_{blockData.row}";
            go.transform.position = GridToWorld(blockData.col, blockData.row);
            go.transform.rotation = Quaternion.Euler(0, 0, blockData.rotation);

            // Apply saved health to the BlockMarker
            var marker = go.GetComponent<BlockMarker>();
            if (marker != null)
                marker.health = blockData.health;
        }

        Debug.Log($"[LevelEditor] Imported {level.blocks.Count} blocks from {Path.GetFileName(path)}");
    }

    GameObject GetPrefabForType(string blockType)
    {
        return blockType switch
        {
            "square" => _squarePrefab,
            "triangle" => _trianglePrefab,
            "dynamite" => _dynamitePrefab,
            "spike" => _spikePrefab,
            "metal" => _metalPrefab,
            _ => null
        };
    }

    // ─────────────────────────────────────────
    // Scene Grid Preview  —  drawn in Scene view
    // ─────────────────────────────────────────

    void OnEnable() => SceneView.duringSceneGui += DrawSceneGrid;
    void OnDisable() => SceneView.duringSceneGui -= DrawSceneGrid;

    void DrawSceneGrid(SceneView sceneView)
    {
        if (!_showGrid) return;

        var (originX, originY, fitCellSize) = GetCameraAlignedGrid();

        // How many rows fit in the camera height
        Camera cam = Camera.main;
        float camH = cam != null ? cam.orthographicSize * 2f : 20f;
        int rows = Mathf.CeilToInt(camH / fitCellSize);

        float gridW = _columns * fitCellSize;
        float gridH = rows * fitCellSize;

        Handles.color = _gridColor;

        // Vertical lines — span full camera height
        for (int col = 0; col <= _columns; col++)
        {
            float x = originX + col * fitCellSize;
            Handles.DrawLine(
                new Vector3(x, originY, 0),
                new Vector3(x, originY + gridH, 0)
            );
        }

        // Horizontal lines — span full camera width
        for (int row = 0; row <= rows; row++)
        {
            float y = originY + row * fitCellSize;
            Handles.DrawLine(
                new Vector3(originX, y, 0),
                new Vector3(originX + gridW, y, 0)
            );
        }

        // Camera boundary highlight
        Handles.color = new Color(1f, 0.8f, 0.2f, 0.8f);
        Handles.DrawWireCube(
            cam != null ? cam.transform.position : Vector3.zero,
            new Vector3(gridW, camH, 0)
        );

        sceneView.Repaint();
    }

    // ─────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────

    List<BlockMarker> FindBlockMarkers()
    {
        return new List<BlockMarker>(FindObjectsByType<BlockMarker>(FindObjectsSortMode.None));
    }
}
#endif