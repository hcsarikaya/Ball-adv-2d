// 
// Level/BlockMarker.cs
// Attach this to every block prefab.
// Stores the block's level design properties.
// The LevelEditorWindow reads these to export JSON.
// 
using UnityEngine;

public class BlockMarker : MonoBehaviour
{
    public string blockType = "square";
    public int health = 3;

    
}