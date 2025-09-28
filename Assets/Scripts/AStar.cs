using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Performs search using A*.
/// </summary>
public class AStar : MonoBehaviour
{
    // Colors for the different search categories.
    public static Color openColor = Color.cyan;
    public static Color closedColor = Color.blue;
    public static Color activeColor = Color.yellow;
    public static Color pathColor = Color.yellow;
    public static Color startEndColor = Color.magenta;

    // The stopwatch for timing search.
    private static Stopwatch watch = new Stopwatch();

    public static IEnumerator search(GameObject start, GameObject end, Heuristic heuristic, float waitTime, bool colorTiles = false, bool displayCosts = false, Stack<NodeRecord> path = null)
    {
        // Starts the stopwatch.
        watch.Start();
        
        path = new Stack<NodeRecord>();
        
        // Add your A* code here.!!!!!!!!!!!!!!!!!!!!
        // Initialize the record for the start node.
        NodeRecord startRecord = new NodeRecord{Tile = start, 
                                                previousNode = null, 
                                                costSoFar = 0,
                                                estimatedTotalCost = heuristic(start, start, end)};

        // Retrieves the number used to scale the game world tiles.
        float scale = startRecord.Tile.transform.localScale.x;
        
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
                float endNodeCost = currentRecord.costSoFar + scale; 
                NodeRecord endNodeRecord = null;
                float endNodeHeuristic;

                // If the node is closed we may have to skip or remove from the closed list.
                if (Contains(closed, connection)) 
                {  
                    //Here we find the record in the closed list corresponding to the endNode.
                    endNodeRecord = Find(closed, connection);

                    //If we didn’t find a shorter route, skip.
                    if (endNodeRecord.costSoFar <= endNodeCost) { continue; }

                    //Otherwise, remove it from the closed list.
                    closed.Remove(endNodeRecord);

                    //We can use the node’s old cost values to calculate its heuristic value without calling the possibly expensive function.
                    endNodeHeuristic = endNodeRecord.estimatedTotalCost - endNodeRecord.costSoFar;
                }

                //or if it is open and we’ve found a worse route.
                else if (Contains(open, connection)) { 
                    endNodeRecord = Find(open, connection); // Here we find the record in the open list corresponding to the endNode.
                    if (endNodeRecord.costSoFar <= endNodeCost) { continue; }
                    // Again, calculate heuristic.
                    endNodeHeuristic = endNodeRecord.estimatedTotalCost - endNodeRecord.costSoFar;
                }

                // Otherwise we know we’ve got an unvisited node, so make a record for it.
                else { 
                    endNodeRecord = new NodeRecord{ Tile = connection };
                    endNodeHeuristic = heuristic(start, connection, end);
                }

                // We’re here if we need to update the node. Update the cost, estimate, and connection.
                endNodeRecord.costSoFar = endNodeCost; 
                endNodeRecord.previousNode = currentRecord.Tile;
                endNodeRecord.estimatedTotalCost = endNodeCost + endNodeHeuristic;

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

        // Determine whether A* found a path and print it here.
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

    

    public delegate float Heuristic(GameObject start, GameObject tile, GameObject end);

    public static float Uniform (GameObject start, GameObject tile, GameObject end)
    {
        return 0f;
    }

    public static float Manhattan (GameObject start, GameObject tile, GameObject end)
    {
        Vector3 tilePos = tile.transform.position;
        Vector3 endPos = end.transform.position;
        return Mathf.Abs(tilePos.x - endPos.x) + Mathf.Abs(tilePos.y - endPos.y);
    }

    public static float CrossProduct (GameObject start, GameObject tile, GameObject end)
    {
        Vector3 startPos = start.transform.position;
        Vector3 tilePos = tile.transform.position;
        Vector3 endPos = end.transform.position;
        float cross = Mathf.Abs((tilePos.x - endPos.x)*(startPos.y-endPos.y)-(startPos.x-endPos.x)*(tilePos.y-endPos.y));
        return Manhattan(start, tile, end) + cross * 0.001f;
    }

    //helper methods
    private static NodeRecord SmallestElement(List<NodeRecord> list) {
        NodeRecord smallest = list[0];
        foreach (NodeRecord node in list)
            if (node.estimatedTotalCost < smallest.estimatedTotalCost) { smallest = node; }
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

// Extend NodeRecord class from Dijkstra
public partial class NodeRecord {
    public float estimatedTotalCost { get; set; } = float.MaxValue;
}