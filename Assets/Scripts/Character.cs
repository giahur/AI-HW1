using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Moves a character according to paths found by a pathfinding algorithm.
/// </summary>
public class Character : MonoBehaviour
{
    // The current tile the character is on.
    public GameObject CurrentTile { get; set; } = null;

    // The tile the character is moving to.
    public GameObject TargetTile { get; set; } = null;

    // The path the character is following.
    public Stack<NodeRecord> Path { get; set; } = new Stack<NodeRecord>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
