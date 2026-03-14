// 
// Level/LevelData.cs
// Data classes that map directly to/from JSON
// 
using System.Collections.Generic;

[System.Serializable]
public class LevelData
{
    public string levelId;
    public string mode;        // "advancing" or "fixed"
    public int columns;     // grid width, set per level
    public float cellSize;    // world units per cell
    public List<BlockData> blocks = new List<BlockData>();
}

[System.Serializable]
public class BlockData
{
    public string blockType;  // "square", "triangle", "dynamite", "spike", "metal"
    public int col;
    public int row;
    public int health;
    public float rotation;   // 0, 90, 180, 270  for angled blocks
}