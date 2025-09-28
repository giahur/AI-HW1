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

    public float speed = 2f; 
    public float arriveThreshold = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update() {
        if (Path == null || Path.Count == 0) { 
            Debug.Log(Path.Count);
            return; 
        }
        NodeRecord nextRecord = Path.Peek();
        if (nextRecord == null || nextRecord.Tile == null) { 
            Debug.Log("2");
            Path.Pop(); 
            return;
        }
        Transform target = nextRecord.Tile.transform;
        var step =  speed * Time.deltaTime; // calculate distance to move
        transform.position = Vector3.MoveTowards(transform.position, target.position, step);
        if (Vector3.Distance(transform.position, target.position) <= arriveThreshold)
        {
            Debug.Log("3");
            transform.position = target.position;
            Debug.Log(target.position);
            CurrentTile = nextRecord.Tile;
            Path.Pop();
        } 
    }
}