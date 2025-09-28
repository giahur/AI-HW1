using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a node and stores graph information.
/// Is associated with a game world tile.
/// </summary>
public class Node : MonoBehaviour
{
    // A dictionary that stores up to four connections associated with Up, Down, Left, and Right.
    public Dictionary<Direction, GameObject> Connections { get; set; } = new Dictionary<Direction, GameObject>();

    // Stores the original color assigned to the tile, so the color can be reset after search.
    public Color OriginalColor { get; set; } = Color.white;

    // A method for debugging connections.
    // Also demonstrated how to enumerate the connected game world tiles in Connections.
    public void printConnections()
    {
        // Iterates through the different values in the direction enum (Up, Down, Left, Right).
        foreach (Direction direction in Enum.GetValues(typeof(Direction)))
            // If there is a connection in that direction, prints this tile's name, the connection direction, and the connected tile's name.
            if (Connections.ContainsKey(direction)) Debug.Log(name + " " + direction + " " + Connections[direction].name);
    }
}
