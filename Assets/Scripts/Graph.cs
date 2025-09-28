using UnityEngine;

/// <summary>
/// Represents the underlying graph for the pathfinding task.
/// Stores references to the tile game objects that represents graph nodes in the game world.
/// Contains methods that initialize and reset the graph between searches.
/// </summary>
public class Graph : MonoBehaviour
{
    // A link to the tile prefab.
    public GameObject tile;

    // The scale of the tiles.
    // Ranges from 1 upwards. I wouldn't go higher than 6 or 7...
    public int scale = 1;

    // The probability of generating obstacles in the graph by dropping tiles.
    // Should be between 0 and 1.
    public float obstacleProbability = 0.1f;

    // A 2D game object array to store the game world tiles.
    private GameObject[,] graph;

    // Stores 1 / scale, which is used often during initialization.
    private float invertedScale;

    /// <summary>
    /// Initializes the tile game objects to represent the graph.
    /// Called at Start() from the interface class.
    /// </summary>
    public void makeGraph()
    {
        // Initializes a new random variable.
        System.Random random = new System.Random();

        // Sets the inverted scale.
        invertedScale = 1 / (float)scale;

        // Creates a new graph array based on the resolution (10:16) and the given scale.
        graph = new GameObject[10 * scale, 16 * scale];

        // Create the first tile at (0,0).
        // We never want an obstacle here because it is where we will spawn the character.
        createTile(0, 0);

        // Loop through the rows.
        for (int i = 0; i < (10 * scale); i++)
        {
            // Loop through the columns.
            for (int j = 0; j < (16 * scale); j++)
            {
                // Draw a random number and check that it is over the obstacle threshold.
                // Also check whether we have moved past (0,0).
                if ((float)random.NextDouble() > obstacleProbability && (i > 0 || j > 0))
                {
                    // Create a new tile at (i,j).
                    GameObject newTile = createTile(i, j);

                    // If there is at least one row above us...
                    if (i > 0)
                        // and the tile above us is not an empty obstacle...
                        if (graph[i - 1, j] != null)
                            // connect the current tile to the one above.
                            connectTiles(graph[i - 1, j], Direction.Down, newTile);

                    // Similarly, if there is at least one column to the left...
                    if (j > 0)
                        // and the tile to the left is not an empty obstacle...
                        if (graph[i, j - 1] != null)
                            // connect the current tile to the leftward one.
                            connectTiles(graph[i, j - 1], Direction.Right, newTile);
                }
            }
        }
    }

    /// <summary>
    /// Resets all the tiles to their original color once pathfinding is finished.
    /// </summary>
    public void resetColor ()
    {
        // Loops through each tile game object in the graph array.
        foreach (GameObject tile in graph)
        {
            // If the position is not a null obstacle...
            if (tile != null)
            {
                // Grab the renderer off the tile.
                SpriteRenderer rend = tile.GetComponentInChildren<SpriteRenderer>();

                // Grab the node script off the tile.
                Node tileNode = tile.GetComponent<Node>();

                // Change the renderer's color to the one stored in the script.
                rend.material.color = tileNode.OriginalColor;

                // Grab the text mesh off the tile.
                TextMesh text = tile.GetComponent<TextMesh>();

                // Reset its text to be blank.
                text.text = "";
            }
        }
    }

    // Returns the tile at a given (x,y) position.
    public GameObject getTile(int x, int y)
    {
        return graph[x, y];
    }

    // Creates a new tile game object at a given (x,y) position.
    private GameObject createTile (int x, int y)
    {
        // Instantiates a tile prefab at the top-left corner of the screen.
        GameObject newTile = Instantiate(tile, Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height, 1f)), Quaternion.identity);

        // Sets the tile's parent to be the graph game object.
        newTile.transform.SetParent(transform);

        // Sets the (x,y) position in the graph array to the new tile.
        graph[x, y] = newTile;

        // Sets the tile's name to reflect its position in the array.
        newTile.name = "Tile " + x.ToString() + "," + y.ToString();

        // Scales the tile according to the inverted scale variable.
        newTile.transform.localScale = new Vector3(invertedScale, invertedScale, 1f);

        // Centers the tile in the top-left tile position, instead of being halfway off-screen.
        newTile.transform.position += new Vector3(invertedScale / 2f, (invertedScale / 2f) * -1f, 0f);

        // Moves the tile from the top-left tile position, to the correct (x,y) position.
        newTile.transform.position += new Vector3(invertedScale * y, invertedScale * -x, 0f);

        // Determines whether the tile should be colored grey.
        // The values default to white.
        if ((x % 2 == 0 && y % 2 == 0) || (x % 2 == 1 && y % 2 == 1))
        {
            // Grab the node script.
            Node newNode = newTile.GetComponent<Node>();

            // Grab the renderer.
            SpriteRenderer rend = newTile.GetComponentInChildren<SpriteRenderer>();

            // Set both the original color variable and the renderer to grey.
            rend.material.color = Color.grey;
            newNode.OriginalColor = Color.grey;
        }

        // Return the generated tile.
        return newTile;
    }

    // Connect two tiles in a given direction.
    private void connectTiles (GameObject from, Direction direction, GameObject to)
    {
        // Grab the node scripts attached to the two tile game objects.
        Node fromNode = from.GetComponent<Node>();
        Node toNode = to.GetComponent<Node>();

        // The first direction is simple, add it to the from node.
        fromNode.Connections.Add(direction, to);

        // Find the opposite direction using if statements.
        // Then attach the from to the to in that direction.
        if (direction == Direction.Up)
        {
            toNode.Connections.Add(Direction.Down, from);
        }
        else if (direction == Direction.Down)
        {
            toNode.Connections.Add(Direction.Up, from);
        }
        else if (direction == Direction.Left)
        {
            toNode.Connections.Add(Direction.Right, from);
        }
        else if (direction == Direction.Right)
        {
            toNode.Connections.Add(Direction.Left, from);
        }
    }
}

/// <summary>
/// An enumeration of four directions: Up, Down, Left, and Right
/// </summary>
public enum Direction
{
    Up, Down, Left, Right
}
