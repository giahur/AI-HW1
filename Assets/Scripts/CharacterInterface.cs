using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Provides an interface that allows the user to select tiles to perform pathfinding between.
/// Then moves a character between those tiles.
/// </summary>
public class CharacterInterface : MonoBehaviour
{
    // The tiles to peform pathfinding from and to.
    // Initialized to null.
    private GameObject from = null;
    private GameObject to = null;

    // Records whether search is currently being performed.
    private bool searching = false;

    // Links to the graph script.
    private Graph graph;

    // A prefab of the character.
    public GameObject characterPrefab;

    // The instantiated game object of the character.
    private GameObject characterGO;

    // The script attached to the character.
    private Character character;

    // The time in seconds to wait between actions during visualization.
    public float waitTime = 1f;

    // Allows the user to select what algorithm will be used for search.
    public SearchType searchType = SearchType.Dijkstra;

    // Allows the user to select what heuristic A* uses during search.
    public HeuristicType heuristicType = HeuristicType.Uniform;

    // Allows the user to select whether tiles will be colored.
    public bool colorTiles = true;

    // Allows the user to select whether costs will be displayed.
    public bool displayCosts = false;

    // Start is called before the first frame update
    void Start()
    {
        // Store the graph game object.
        GameObject graphGO = GameObject.Find("Graph");

        // Grab the graph script.
        graph = graphGO.GetComponent<Graph>();

        // Tell the graph script to generate a new graph.
        graph.makeGraph();

        // Insantiate the character game object at the top left corner of the screen.
        characterGO = Instantiate(characterPrefab, Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height, 1f)), Quaternion.identity);

        // Get the two renderers, one for the circle and the square.
        // Turn both of their colors blue so the character is visible as it pathfinds.
        SpriteRenderer[] characterRenderers = characterGO.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer characterRenderer in characterRenderers)
            characterRenderer.material.color = Color.blue;

        // Scale the character and place it in the middle of the top left square.
        characterGO.transform.localScale = new Vector3(1 / (float)graph.scale * .6f, 1 / (float)graph.scale * .6f, 1f);
        characterGO.transform.position += new Vector3((1 / (float)graph.scale) / 2f, ((1 / (float)graph.scale) / 2f) * -1f, 0f);

        // Grab the character script.
        character = characterGO.GetComponent<Character>();

        // Set the character's current tile.
        character.CurrentTile = graph.getTile(0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        // Run the core coroutine.
        StartCoroutine(HandleInput());
    }

    // A coroutine that handles input from the user and starts search.
    private IEnumerator HandleInput ()
    {
        // If the mouse has been clicked and there isn't a current search...
        if (Input.GetMouseButtonDown(0) && !searching)
        {
            // Grab the position that was clicked by the mouse.
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            // Use a raycast to determine whether a tile was clicked.
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

            // If a tile with a collider was clicked...
            if (hit.collider != null)
            {
                // Set the from tile to the character's current tile.
                from = character.CurrentTile;

                // Grab the renderer of the clicked tile.
                SpriteRenderer renderer = hit.collider.gameObject.GetComponentInChildren<SpriteRenderer>();

                // Turn the tile color to magenta to visualize the selection.
                renderer.material.color = Color.magenta;

                //  If the to tile is null and the current is different than the stored from tile...
                if (to == null && hit.collider.gameObject != from && !searching)
                {
                    // Set the to game object to the current tile.
                    to = hit.collider.gameObject;

                    // Store that we are currently searching.
                    searching = true;

                    // Create a stack to store the path found by the algorithm.
                    Stack<NodeRecord> path = new Stack<NodeRecord>();

                    // Start a new search coroutine based on the stored search type and heuristic.
                    // Also, print a line to the log stating what type of search has been started.
                    if (searchType == SearchType.Dijkstra)
                    {
                        Debug.Log("Dijkstra");
                        yield return StartCoroutine(Dijkstra.search(from, to, waitTime, colorTiles, displayCosts, path));
                    }
                    else if (searchType == SearchType.AStar)
                    {
                        if (heuristicType == HeuristicType.Uniform)
                        {
                            Debug.Log("A* Uniform");
                            yield return StartCoroutine(AStar.search(from, to, AStar.Uniform, waitTime, colorTiles, displayCosts, path));
                        }
                        else if (heuristicType == HeuristicType.Manhattan)
                        {
                            Debug.Log("A* Manhattan");
                            yield return StartCoroutine(AStar.search(from, to, AStar.Manhattan, waitTime, colorTiles, displayCosts, path));
                        }
                        else if (heuristicType == HeuristicType.CrossProduct)
                        {
                            Debug.Log("A* Cross Product");
                            yield return StartCoroutine(AStar.search(from, to, AStar.CrossProduct, waitTime, colorTiles, displayCosts, path));
                        }
                    }

                    // Pass the final path to the character.
                    character.Path = path;

                    // Search is now over.
                    searching = false;
                }

                // If both tiles are filled, this is a post-search click.
                // Reset the graph and prepare for a new search.
                else
                {
                    // Reset the graph color.
                    graph.resetColor();

                    // Reset the tile variables.
                    from = null;
                    to = null;
                }
            }
        }

        yield return null;
    }
}
