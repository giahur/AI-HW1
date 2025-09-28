using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Performs search using Dijkstra's algorithm.
/// </summary>
public class Dijkstra : MonoBehaviour
{
    // Colors for the different search categories.
    public static Color openColor = Color.cyan;
    public static Color closedColor = Color.blue;
    public static Color activeColor = Color.yellow;
    public static Color pathColor = Color.yellow;
    public static Color startEndColor = Color.magenta;

    // The stopwatch for timing search.
    private static Stopwatch watch = new Stopwatch();

    public static IEnumerator search(GameObject start, GameObject end, float waitTime, bool colorTiles = false, bool displayCosts = false, Stack<NodeRecord> path = null)
    {
        // Starts the stopwatch.
        watch.Start();

        path = new Stack<NodeRecord>();
        
        // Add your Dijkstra code here.!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        // Initialize the record for the start node.
        NodeRecord startRecord = new NodeRecord{Tile = start, 
                                                previousNode = null, 
                                                costSoFar = 0};

        // Initialize the open and closed lists
        List<NodeRecord> open = new List<NodeRecord>{ startRecord };
        List<NodeRecord> closed = new List<NodeRecord>();
        
        NodeRecord currentRecord = null;

        // Iterate through processing each node.
        while(open.Count > 0) {
            // Find the smallest element in the open list.
            currentRecord = SmallestElement(open); 

            // If coloring tiles, update the tile color.
            if (colorTiles) { currentRecord.ColorTile(activeColor); } 
            yield return new WaitForSeconds(waitTime); // Pause the animation to show the new active tile.

            // If it is the goal node, then terminate.
            if (currentRecord.Tile == end) { break; } 

            // Otherwise get its outgoing connections.
            Node currentNode = currentRecord.Tile.GetComponent<Node>(); 

            // Loop through each connection in turn.
            foreach(GameObject connection in currentNode.Connections.Values) {
                //Get the cost estimate for the end node.
                float endNodeCost = currentRecord.costSoFar + 1f; 
                NodeRecord endNodeRecord = null;

                // Skip if the node is closed.
                if (Contains(closed, connection)) { continue; } 

                //or if it is open and we’ve found a worse route.
                else if (Contains(open, connection)) { 
                    endNodeRecord = Find(open, connection); // Here we find the record in the open list corresponding to the endNode.
                    if (endNodeRecord.costSoFar <= endNodeCost) { continue; }
                }

                // Otherwise we know we’ve got an unvisited node, so make a record for it.
                else { endNodeRecord = new NodeRecord{ Tile = connection }; }

                // We’re here if we need to update the node. Update the cost and connection.
                endNodeRecord.costSoFar = endNodeCost; 
                endNodeRecord.previousNode = currentRecord.Tile;

                // If displaying costs, update the tile display.
                if(displayCosts) {endNodeRecord.Display(endNodeRecord.costSoFar); }
                // And add it to the open list.
                if(!Contains(open, connection)) { open.Add(endNodeRecord); }
                // If coloring tiles, update the open tile color.
                if (colorTiles) { endNodeRecord.ColorTile(openColor); }
                yield return new WaitForSeconds(waitTime); // Pause the animation to show the new open tile.
            }
            // We’ve finished looking at the connections for the current node, so add it to the closed list and remove it from the open list.
            open.Remove(currentRecord);
            closed.Add(currentRecord);
            
            //If coloring tiles, update the closed tile color.
            if (colorTiles) { currentRecord.ColorTile(closedColor); }
        }
    
        // Search has finished, so stop the timer.
        watch.Stop();

        // Print search statistics.
        UnityEngine.Debug.Log("Seconds Elapsed: " + (watch.ElapsedMilliseconds / 1000f).ToString());
        UnityEngine.Debug.Log("Nodes Expanded: " + closed.Count);

        // Reset the stopwatch.
        watch.Reset();

        // Determine whether Dijkstra found a path and print it here.
        // We’re here if we’ve either found the goal, or if we’ve no more nodes to search, find which.
        if (currentRecord.Tile != end) {UnityEngine.Debug.Log("Search Failed"); }            
        else {
            while (currentRecord.Tile != start) { //Work back along the path, accumulating connections.

                path.Push(currentRecord);
                currentRecord = Find(closed, currentRecord.previousNode);

                // If coloring tiles, update the path tile color.
                if (colorTiles) { currentRecord.ColorTile(pathColor); }
                yield return new WaitForSeconds(waitTime); //Pause the animation to show the new path tile.
            }
            //Print search statistics.
            UnityEngine.Debug.Log("Path Length: " + path.Count);

            if (colorTiles) {
                startRecord.ColorTile(startEndColor);
                NodeRecord endRecord = new NodeRecord  { Tile = end };
                endRecord.ColorTile(startEndColor); 
            }
        }
        // Since we’re using a co-routing, we just return null. The path will be stored in the stack variable we passed in.
        yield return null;
    }

    //helper methods
    private static NodeRecord SmallestElement(List<NodeRecord> list) {
        NodeRecord smallest = list[0];
        foreach (NodeRecord node in list)
            if (node.costSoFar < smallest.costSoFar) { smallest = node; }
        return smallest;
    }

    private static bool Contains(List<NodeRecord> list, GameObject tile) {
        foreach (NodeRecord node in list) {
            if (node.Tile == tile) { return true; }
        }
        return false;
    }

    private static NodeRecord Find(List<NodeRecord> list, GameObject tile) {
        foreach (NodeRecord node in list) {
            if (node.Tile == tile) { return node; }
        }
        return null;
    }
}

/// <summary>
/// A class for recording search statistics.
/// </summary>
public partial class NodeRecord
{
    // The tile game object.
    public GameObject Tile { get; set; } = null;

    // Set the other class properties here.!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    public GameObject previousNode;
    public float costSoFar;

    // Sets the tile's color.
    public void ColorTile (Color newColor)
    {
        SpriteRenderer renderer = Tile.GetComponentInChildren<SpriteRenderer>();
        renderer.material.color = newColor;
    }

    // Displays a string on the tile.
    public void Display (float value)
    {
        TextMesh text = Tile.GetComponent<TextMesh>();
        text.text = value.ToString();
    }
}
