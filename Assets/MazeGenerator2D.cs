using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MazeGenerator2D : MonoBehaviour
{
    // Public settings of the maze

    [SerializeField]
    private MazeCell2D mazeCellPrefab;

    [SerializeField]
    private int mazeWidth;

    [SerializeField]
    private int mazeHeight;

    [SerializeField]
    private bool changeInRealTime = false;

    [SerializeField]
    private bool watchGrow = false; // if we want to visualize the creation of the maze or not

    [SerializeField]
    private float waitSeconds = 0.05f; // how many seconds between generation steps. more seconds yield slower maze generation

    [SerializeField]
    private bool watchHumanEvaluation = false;

    private MazeCell2D[,] mazeGrid;

    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }

    [SerializeField]
    private Difficulty difficulty = Difficulty.Easy;


    void Start()
    {
        // Initializa Maze at the start
        InitializeMaze();

        // If the option change in real time is set to true, start changing after 3 seconds.
        if (changeInRealTime)
        {
            // clear all visited cells
            for (int x = 0; x < mazeWidth; x++)
            {
                for (int y = 0; y < mazeHeight; y++)
                {
                    mazeGrid[x, y].UnVisit();
                }
            }

            // wait 3 seconds for the degeneration
            StartCoroutine(WaitSeconds());
        }

    }

    public void InitializeMaze()
    {
        // First, destroy all objects if any
        if (mazeGrid != null)
        {
            for (int x = 0; x < mazeGrid.GetLength(0); x++)
            {
                for (int y = 0; y < mazeGrid.GetLength(1); y++)
                {
                    if (mazeGrid[x, y] != null)
                    {
                        Destroy(mazeGrid[x, y].gameObject);
                    }
                }
            }
        }

        // Initialize empty maze grid
        mazeGrid = new MazeCell2D[mazeWidth, mazeHeight];

        // Initialize a cell in each position of the maze grid
        for (int x = 0; x < mazeWidth; x++)
        {
            for (int y = 0; y < mazeHeight; y++)
            {
                mazeGrid[x, y] = Instantiate(mazeCellPrefab, new Vector3(2 * x, 2 * y, 0), Quaternion.identity);
            }
        }

        // Generate Maze according to difficulty
        if (difficulty == Difficulty.Easy)
        {
            // Binary Tree
            StartCoroutine(GenerateMaze_BinaryTree());
        }
        else if (difficulty == Difficulty.Medium)
        {
            // Aldous-Broder
            if(mazeWidth<9 && mazeHeight < 9)
            {
                StartCoroutine(GenerateMaze_AB());
            }
            else
            {
                Debug.Log("Please, reduce dimensions of the maze");
            }  
        }
        else if (difficulty == Difficulty.Hard)
        {
            // Recursive backtracker (Deep First Search)
            StartCoroutine(GenerateMaze_DFS(null, mazeGrid[0, 0]));
        }

        // Add entrance and exit to the maze
        mazeGrid[0, mazeHeight - 1].ClearLeftWall();
        mazeGrid[mazeWidth - 1, 0].ClearRightWall();
    }

    // Easy level: Binary Tree --------------------------------------------------------------------------------------------------------------------------

    private IEnumerator GenerateMaze_BinaryTree()
    {
        // For each cell in the grid
        for (int x = 0; x < mazeWidth; x++)
        {
            for (int y = mazeHeight-1; y >=0 ; y--)
            {
                // Current cell
                MazeCell2D currentCell = mazeGrid[x, y];

                // Randomly choose a direction (East or South)
                if (x < mazeWidth - 1) // cell can connect to East
                {
                    if (y > 0) // cell can connect to South
                    {
                        // Randomly decide to remove the right or top wall
                        bool removeEast = Random.Range(0, 2) == 0;

                        if (removeEast)
                        {
                            currentCell.ClearRightWall();       // Remove right wall on this cell 
                            mazeGrid[x + 1, y].ClearLeftWall(); // Remove the left wall on the right of the current cell 
                            currentCell.Visit();
                        }
                        else
                        {
                            currentCell.ClearBackWall();       // Remove back wall on this cell
                            mazeGrid[x, y - 1].ClearFrontWall();  // Remove front wall on the next cell
                            currentCell.Visit();
                        }
                    }
                    else
                    {
                        // Can only connect to East
                        currentCell.ClearRightWall();
                        mazeGrid[x + 1, y].ClearLeftWall();
                        currentCell.Visit();
                    }
                }
                else if (y>0) // Can only go South
                {
                    currentCell.ClearBackWall();
                    mazeGrid[x, y - 1].ClearFrontWall();
                    currentCell.Visit();
                }

                if(watchGrow)
                    yield return new WaitForSeconds(waitSeconds);
            }
        }

        // Visit last cell
        mazeGrid[mazeWidth - 1, 0].Visit();
    }

    // Medium level: Aldous-Broder ----------------------------------------------------------------------------------------------------------------------

    private IEnumerator GenerateMaze_AB()
    {
        // List with all unvisited cells
        List<Vector2Int> unvisitedCells = new List<Vector2Int>();
        for (int x = 0; x < mazeWidth; x++)
        {
            for (int y = 0; y < mazeHeight; y++)
            {
                unvisitedCells.Add(new Vector2Int(x, y));
            }
        }

        // Start root of the tree from a random cell
        Vector2Int currentCell = unvisitedCells[Random.Range(0, unvisitedCells.Count)];
        mazeGrid[currentCell.x, currentCell.y].Visit();
        unvisitedCells.Remove(currentCell); 

        // Direction vectors for movement (North, East, South, West)
        Vector2Int[] directions = new Vector2Int[]
        {
        new Vector2Int(0, 1), // North
        new Vector2Int(1, 0), // East
        new Vector2Int(0, -1), // South
        new Vector2Int(-1, 0) // West
        };

        // While there are unvisited cells
        while (unvisitedCells.Count > 0)
        {

            // Get a list of valid neighbors that are within the map limits
            List<Vector2Int> validNeighbors = new List<Vector2Int>(); // here I store exclusively the valid neighbours

            foreach (var dir in directions)
            {
                Vector2Int neighbor = currentCell + dir;

                if (neighbor.x >= 0 && neighbor.x < mazeWidth && neighbor.y >= 0 && neighbor.y < mazeHeight) // if the neighbour is within the limits...
                {
                    validNeighbors.Add(neighbor);
                }
            }

            // Choose one random neighbor and visit (if not visit)
            Vector2Int chosen_neighbor = validNeighbors[Random.Range(0, validNeighbors.Count)];

            if (unvisitedCells.Contains(chosen_neighbor)) { // if the neighbor hasn't been visited
                                                     
                ClearWalls_DFS(mazeGrid[currentCell.x, currentCell.y], mazeGrid[chosen_neighbor.x, chosen_neighbor.y]);  // Remove the wall between currentCell and neighbor

                mazeGrid[chosen_neighbor.x, chosen_neighbor.y].Visit(); // Mark the neighbor as visited
                unvisitedCells.Remove(chosen_neighbor);
            }

            currentCell = chosen_neighbor;

            if (watchGrow)
                yield return new WaitForSeconds(waitSeconds);
        }

        Debug.Log("Finished Aldous-Broder algorithm");
    }


    // Hard level: Recursive BackTracker (Deep First Search)---------------------------------------------------------------------------------------------

    private IEnumerator GenerateMaze_DFS(MazeCell2D previousCell, MazeCell2D currentCell)
    {
        // this method will be called recursively to generate the maze
        currentCell.Visit();                        // visit current cell
        ClearWalls_DFS(previousCell, currentCell);      // knock down walls between previous and current

        // loops until there are no neighbours remaining
        MazeCell2D nextCell;

        if (watchGrow)
            yield return new WaitForSeconds(waitSeconds);

        do
        {
            nextCell = GetNextUnvisitedCell_DFS(currentCell);

            if (nextCell != null)
            {
                // recursively call the generate method.
                // it goes cell to cell until it gets to the point of no visited neighbours
                yield return GenerateMaze_DFS(currentCell, nextCell); // yield return to call a corroutine
            }

        } while (nextCell != null);
    }

    private MazeCell2D GetNextUnvisitedCell_DFS(MazeCell2D currentCell)
    {
        // gets all unvisited neighbours and picks one of them as random
        var unvisitedCells = GetUnvisitedCells_DFS(currentCell);

        return unvisitedCells.OrderBy(_ => Random.Range(1, 10)).FirstOrDefault();
    }

    private IEnumerable<MazeCell2D> GetUnvisitedCells_DFS(MazeCell2D currentCell)
    {
        int x = (int)currentCell.transform.position.x/2; // each cell occupies 2 units
        int y = (int)currentCell.transform.position.y/2;

        // now we check if the neighbour is in the bounds of the grid
        // check right
        if (x + 1 < mazeWidth)
        {
            var cellToRight = mazeGrid[x + 1, y];

            if (cellToRight.IsVisited == false)
            {
                yield return cellToRight; // this adds the cell to the return collection, but will not exist this method, we can still check on the other directions
            }
        }

        // check left
        if (x - 1 >= 0)
        {
            var cellToLeft = mazeGrid[x - 1, y];

            if (cellToLeft.IsVisited == false)
            {
                yield return cellToLeft;
            }
        }

        // check front
        if (y + 1 < mazeHeight)
        {
            var cellToFront = mazeGrid[x, y + 1];

            if (cellToFront.IsVisited == false)
            {
                yield return cellToFront;
            }
        }

        // check back
        if (y - 1 >= 0)
        {
            var cellToBack = mazeGrid[x, y - 1];

            if (cellToBack.IsVisited == false)
            {
                yield return cellToBack;
            }
        }
    }

    private void ClearWalls_DFS(MazeCell2D previousCell, MazeCell2D currentCell)
    {
        if (previousCell == null)
        {
            return; // no walls to clear, just return
        }

        if (previousCell.transform.position.x < currentCell.transform.position.x)
        {
            // previous cell is on the left of the current cell
            previousCell.ClearRightWall();
            currentCell.ClearLeftWall();
            return;
        }

        if (previousCell.transform.position.x > currentCell.transform.position.x)
        {
            // previos cell is on the right of the current cell
            previousCell.ClearLeftWall();
            currentCell.ClearRightWall();
            return;
        }

        if (previousCell.transform.position.y < currentCell.transform.position.y)
        {
            // previos cell is on the back of the current cell
            previousCell.ClearFrontWall();
            currentCell.ClearBackWall();
            return;
        }

        if (previousCell.transform.position.y > currentCell.transform.position.y)
        {
            // previos cell is on the front of the current cell
            previousCell.ClearBackWall();
            currentCell.ClearFrontWall();
            return;
        }

    }

    // Real time evolution -------------------------------------------------------------------------------------

    public void SetVisitedMazeCell2D(int x, int y)
    {
        x = x / 2;
        y = y / 2; 
        if (x < mazeWidth && y < mazeHeight)
        {
            mazeGrid[x, y].Visit();
            Debug.Log("visited cell set");
        }
    }

    private void StartChange()
    {
        StartCoroutine(GenerateChange(null, null, mazeGrid[mazeWidth - 1, mazeHeight - 1]));
    }

    private IEnumerator GenerateChange(MazeCell2D previousCell2, MazeCell2D previousCell, MazeCell2D currentCell)
    {
        currentCell.Visit(); // visit current cell
        currentCell.ShowCorruptedBlock();

        ClearAndBuildWalls(previousCell2, previousCell, currentCell); // knock down walls between previous and current

        yield return new WaitForSeconds(1f);

        // loops until there are no neighbours remaining
        MazeCell2D nextCell;

        do
        {
            nextCell = GetNextUnvisitedCell_DFS(currentCell);

            if (nextCell != null)
            {
                // recursively call the generate method.
                // it goes cell to cell until it gets to the point of no visited neighbours
                //Debug.Log("Calling next change with: " + previousCell + ", " + currentCell + ", " + nextCell);
                yield return GenerateChange(previousCell, currentCell, nextCell); // yield return to call a corroutine
            }

        } while (nextCell != null);
    }

    private void ClearAndBuildWalls(MazeCell2D previousCell2, MazeCell2D previousCell, MazeCell2D currentCell)
    {
        //Debug.Log("Calling Clear Build Walls with: " + previousCell2 + "----" + previousCell + "---" + currentCell);
        if (previousCell == null)
        {
            return; // no walls to clear, just return
        }

        if (previousCell.transform.position.x < currentCell.transform.position.x)
        {
            if (previousCell2 != null)
            {
                int position = CheckPreviousCell2Position(previousCell2, previousCell);
                if (position == 1)
                {
                    previousCell.ShowBackWall();
                }
                else if (position == 4)
                {
                    previousCell.ShowFrontWall();
                    previousCell.ShowBackWall();
                }
                else if (position == 3)
                {
                    previousCell.ShowFrontWall();
                }
            }

            // previous cell is on the left of the current cell
            previousCell.ClearRightWall();
            currentCell.ClearLeftWall();
            return;
        }

        if (previousCell.transform.position.x > currentCell.transform.position.x)
        {

            if (previousCell2 != null)
            {
                int position = CheckPreviousCell2Position(previousCell2, previousCell);
                if (position == 1)
                {
                    previousCell.ShowBackWall();
                }
                else if (position == 2)
                {
                    previousCell.ShowFrontWall();
                    previousCell.ShowBackWall();
                }
                else if (position == 3)
                {
                    previousCell.ShowFrontWall();
                }
            }

            // previous cell is on the right of the current cell
            previousCell.ClearLeftWall();
            currentCell.ClearRightWall();
            return;
        }

        if (previousCell.transform.position.y < currentCell.transform.position.y)
        {

            if (previousCell2 != null)
            {
                int position = CheckPreviousCell2Position(previousCell2, previousCell);
                if (position == 4)
                {
                    previousCell.ShowRightWall();
                }
                else if (position == 3)
                {
                    previousCell.ShowLeftWall();
                    previousCell.ShowRightWall();
                }
                else if (position == 2)
                {
                    previousCell.ShowLeftWall();
                }
            }

            // previous cell is on the back of the current cell
            previousCell.ClearFrontWall();
            currentCell.ClearBackWall();
            return;
        }

        if (previousCell.transform.position.y > currentCell.transform.position.y)
        {

            if (previousCell2 != null)
            {
                int position = CheckPreviousCell2Position(previousCell2, previousCell);
                if (position == 4)
                {
                    previousCell.ShowRightWall();
                }
                else if (position == 1)
                {
                    previousCell.ShowLeftWall();
                    previousCell.ShowRightWall();
                }
                else if (position == 2)
                {
                    previousCell.ShowLeftWall();
                }
            }

            // previous cell is on the front of the current cell
            previousCell.ClearBackWall();
            currentCell.ClearFrontWall();
            return;
        }

    }

    private int CheckPreviousCell2Position(MazeCell2D cell1, MazeCell2D cell2)
    {
        int position = 0;

        if (cell1.transform.position.x < cell2.transform.position.x)
        {
            position = 4;
        }
        else if (cell1.transform.position.x > cell2.transform.position.x)
        {
            position = 2;
        }
        else if (cell1.transform.position.y < cell2.transform.position.y)
        {
            position = 3;
        }
        else if (cell1.transform.position.y > cell2.transform.position.y)
        {
            position = 1;
        }

        //Debug.Log(position);
        return position;
    }

    private IEnumerator WaitSeconds()
    {
        Debug.Log("Waiting...");
        yield return new WaitForSeconds(2f);
        Debug.Log("Starting change");
        StartChange();
    }

    // Difficulty Evaluation -------------------------------------------------------------------------------------

    public void StartEvaluation()
    {
        StartCoroutine(RunDifficultyEvaluation());
    }

    public IEnumerator RunDifficultyEvaluation()
    {
        // this function solves the maze as if it were a human.

        GameObject player = GameObject.FindGameObjectWithTag("Player"); // find gameobject player with tag "Player"

        // set position of the player to be the same position as the mazeCell in mazeGrid[0, mazeHeight-1]
        int player_x = 0;
        int player_y = mazeHeight - 1; // these are the positions of the player

        Vector3 startPos = mazeGrid[player_x, player_y].tfCenter.transform.position;
        player.transform.position = startPos;
        int totalSteps = 0; 

        // Initialize freeWalls array
        bool[] freeWalls = new bool[4]; // left, right, front, back
        int prevDir = 0; // 0: previous position is to the left, 1: right, 2: front, 3: back
        int quantityFreeWalls = 0;

        // Matrix of visited cells
        bool[,] visited = new bool[mazeWidth, mazeHeight];
        for (int x = 0; x < mazeWidth; x++)
        {
            for (int y = 0; y < mazeHeight; y++)
            {
                visited[x, y] = false;
            }
        }

        while (!(player_x == (mazeWidth - 1) && player_y == 0))
        {
            if(watchHumanEvaluation)
                yield return new WaitForSeconds(0.5f);

            // Reset wall flags
            for (int i = 0; i < 4; i++) freeWalls[i] = false;
            quantityFreeWalls = 0;

            // Calculate how many free walls there are
            if (!mazeGrid[player_x, player_y].leftWall.activeSelf && prevDir!=0)
            {
                freeWalls[0] = true;
                quantityFreeWalls++;
            }

            if (!mazeGrid[player_x, player_y].rightWall.activeSelf && prevDir!=1)
            {
                freeWalls[1] = true;
                quantityFreeWalls++;         
            }

            if (!mazeGrid[player_x, player_y].frontWall.activeSelf && prevDir!=2)
            {
                freeWalls[2] = true;
                quantityFreeWalls++;
            }

            if (!mazeGrid[player_x, player_y].backWall.activeSelf && prevDir!=3)
            {
                freeWalls[3] = true;
                quantityFreeWalls++;
            }

            if (quantityFreeWalls > 0)
            {
                if(quantityFreeWalls == 3)
                {
                    float p = Random.value;
                    float p2 = Random.value;

                    // 5a
                    if (freeWalls[2] && freeWalls[1] && freeWalls[3])
                    {
                        bool right = p < 0.61f;

                        if (right && player_x < mazeWidth - 1)
                        {
                            // move right
                            player_x++;
                            prevDir = 0; // prev pos to the left
                            totalSteps++;
                        }
                        else if (!right)
                        {
                            if(p2 < 0.5 && player_y < mazeHeight - 1)
                            {
                                // move front 
                                player_y++;
                                prevDir = 3; // prev pos to the back
                                totalSteps++;
                            }
                            else if (p2 < 0.5 && player_y > 0)
                            {
                                // move back
                                player_y--;
                                prevDir = 2; // prev pos to the front
                                totalSteps++;
                            }
                        }
                    }

                    // 5b
                    else if (freeWalls[0] && freeWalls[3] && freeWalls[1])
                    {
                        bool back = p < 0.58f;

                        if (back && player_y > 0)
                        {
                            // move back
                            player_y--;
                            prevDir = 2; // prev pos to the front
                            totalSteps++;
                        }
                        else if (!back)
                        {
                            if (p2 < 0.5 && player_x > 0)
                            {
                                // move left
                                player_x--;
                                prevDir = 1; // prev pos to the right
                                totalSteps++;
                            }
                            else if (p2 < 0.5 && player_x < mazeWidth - 1)
                            {
                                // move right
                                player_x++;
                                prevDir = 0; // prev pos to the left
                                totalSteps++;
                            }
                        }
                    }

                    else
                    {
                        // RETURN TO PREVIOUS BRANCH NODE
                    }
                }
                
                else if (quantityFreeWalls == 2)
                {
                    // two or more options
                    float p = Random.value;

                    // 1a
                    if (freeWalls[1] && freeWalls[3] && prevDir==0)
                    {  
                        bool right = p < 0.72f;

                        if (right && player_x < mazeWidth - 1)
                        {
                            // move right
                            player_x++;
                            prevDir = 0; // prev pos to the left
                            totalSteps++;
                        }
                        else if(!right && player_y > 0)
                        {
                            // move back
                            player_y--;
                            prevDir = 2; // prev pos to the front
                            totalSteps++;
                        }
                    }

                    // 1b
                    else if (freeWalls[0] && freeWalls[3] && prevDir == 1)
                    {
                        bool back = p < 0.85f;

                        if(back && player_y > 0)
                        {
                            // move back
                            player_y--;
                            prevDir = 2; // prev pos to the front
                            totalSteps++;
                        }
                        else if(!back && player_x > 0)
                        {
                            // move left
                            player_x--;
                            prevDir = 1; // prev pos to the right
                            totalSteps++;
                        }
                    }

                    // 2a
                    else if (freeWalls[1] && freeWalls[3] && prevDir == 2)
                    {
                        bool back = p < 0.62f;

                        if (back && player_y > 0)
                        {
                            // move back
                            player_y--;
                            prevDir = 2; // prev pos to the front
                            totalSteps++;
                        }
                        else if (!back && player_x < mazeWidth - 1)
                        {
                            // move right
                            player_x++;
                            prevDir = 0; // prev pos to the left
                            totalSteps++;
                        }
                    }

                    // 2b
                    else if (freeWalls[1] && freeWalls[2] && prevDir == 3)
                    {
                        bool right = p < 0.75f;

                        if (right && player_x < mazeWidth - 1)
                        {
                            // move right
                            player_x++;
                            prevDir = 0; // prev pos to the left
                            totalSteps++;
                        }
                        else if (!right && player_y < mazeHeight - 1)
                        {
                            // move front 
                            player_y++;
                            prevDir = 3; // prev pos to the back
                            totalSteps++;
                        }
                    }

                    // 3a
                    else if (freeWalls[2] && freeWalls[1] && prevDir == 0)
                    {
                        bool right = p < 0.87f;

                        if (right && player_x < mazeWidth - 1)
                        {
                            // move right
                            player_x++;
                            prevDir = 0; // prev pos to the left
                            totalSteps++;
                        }
                        else if (!right && player_y < mazeHeight - 1)
                        {
                            // move front 
                            player_y++;
                            prevDir = 3; // prev pos to the back
                            totalSteps++;
                        }
                    }

                    // 3b
                    else if (freeWalls[0] && freeWalls[1] && prevDir == 2)
                    {
                        bool right = p < 0.65f;

                        if (right && player_x < mazeWidth - 1)
                        {
                            // move right
                            player_x++;
                            prevDir = 0; // prev pos to the left
                            totalSteps++;
                        }
                        else if (!right && player_x > 0)
                        {
                            // move left
                            player_x--;
                            prevDir = 1; // prev pos to the right
                            totalSteps++;
                        }
                    }

                    // 4a
                    else if (freeWalls[2] && freeWalls[3] && prevDir == 0)
                    {
                        bool back = p < 0.80f;

                        if (back && player_y > 0)
                        {
                            // move back
                            player_y--;
                            prevDir = 2; // prev pos to the front
                            totalSteps++;
                        }
                        else if (!back && player_y < mazeHeight - 1)
                        {
                            // move front 
                            player_y++;
                            prevDir = 3; // prev pos to the back
                            totalSteps++;
                        }
                    }

                    // 4b
                    else if (freeWalls[0] && freeWalls[3] && prevDir == 2)
                    {
                        bool back = p < 0.82f;

                        if (back && player_y > 0)
                        {
                            // move back
                            player_y--;
                            prevDir = 2; // prev pos to the front
                            totalSteps++;
                        }
                        else if (!back && player_x > 0)
                        {
                            // move left
                            player_x--;
                            prevDir = 1; // prev pos to the right
                            totalSteps++;
                        }
                    }

                    else
                    {
                        // in any other case, go to the cell that was not visited and that has freeWalls

                        if (freeWalls[1] && !visited[player_x + 1, player_y])
                        {
                            // move right
                            player_x++;
                            prevDir = 0; // prev pos to the left
                            totalSteps++;
                        }
                        else if (freeWalls[3] && !visited[player_x, player_y - 1])
                        {
                            // move back
                            player_y--;
                            prevDir = 2; // prev pos to the front
                            totalSteps++;
                        }
                        else if (freeWalls[2] && !visited[player_x, player_y + 1])
                        {
                            // move front 
                            player_y++;
                            prevDir = 3; // prev pos to the back
                            totalSteps++;
                        }
                        else if (freeWalls[0] && !visited[player_x - 1, player_y])
                        {
                            // move left
                            player_x--;
                            prevDir = 1; // prev pos to the right
                            totalSteps++;
                        }

                        // if all cells were visited, just go to the one that is open
                        
                        else if (freeWalls[1] && player_x < mazeWidth - 1)
                        {
                            // move right
                            player_x++;
                            prevDir = 0; // prev pos to the left
                            totalSteps++;
                        }
                        else if (freeWalls[3] && player_y > 0)
                        {
                            // move back
                            player_y--;
                            prevDir = 2; // prev pos to the front
                            totalSteps++;
                        }
                        else if (freeWalls[2] && player_y < mazeHeight - 1)
                        {
                            // move front 
                            player_y++;
                            prevDir = 3; // prev pos to the back
                            totalSteps++;
                        }
                        else if (freeWalls[0] && player_x > 0)
                        {
                            // move left
                            player_x--;
                            prevDir = 1; // prev pos to the right
                            totalSteps++;
                        }
                       
                    }
                }
                
                else if (quantityFreeWalls == 1)
                {
                    // just move towards the cell without walls
                    if (freeWalls[0] && player_x > 0)
                    {
                        // move left
                        player_x--;
                        prevDir = 1; // prev pos to the right
                        totalSteps++;
                    }
                    else if (freeWalls[1] && player_x < mazeWidth - 1)
                    {
                        // move right
                        player_x++;
                        prevDir = 0; // prev pos to the left
                        totalSteps++;
                    }
                    else if (freeWalls[2] && player_y < mazeHeight - 1)
                    {
                        // move front 
                        player_y++;
                        prevDir = 3; // prev pos to the back
                        totalSteps++;
                    }
                    else if (freeWalls[3] && player_y > 0)
                    {
                        // move back
                        player_y--;
                        prevDir = 2; // prev pos to the front
                        totalSteps++;
                    }
                }

            }
            else
            {
                // move towards prev directions
                if (prevDir == 0 && player_x > 0)
                {
                    // move left
                    player_x--;
                    prevDir = 1; // prev pos to the right
                    totalSteps++;
                }
                else if (prevDir==1 && player_x < mazeWidth - 1)
                {
                    // move right
                    player_x++;
                    prevDir = 0; // prev pos to the left
                    totalSteps++;
                }
                else if (prevDir==2 && player_y < mazeHeight - 1)
                {
                    // move front 
                    player_y++;
                    prevDir = 3; // prev pos to the back
                    totalSteps++;
                }
                else if (prevDir==3 && player_y > 0)
                {
                    // move back
                    player_y--;
                    prevDir = 2; // prev pos to the front
                    totalSteps++;
                }

            }

            // update player's position to the position of the cell in the mazeGrid[player_x, player_y]
            player.transform.position = mazeGrid[player_x, player_y].tfCenter.transform.position;
            visited[player_x, player_y] = true;
        }

        Debug.Log("Maze completed. Total steps = " + totalSteps);
    }


    // TO DO: DEBUG && FINISH THE ELSE IN THE CASE WITH 3 FREE WALLS AS IN THE 2 FREE WALLS
    // REDUCE AND COMPACT THE CODE
    // Y HACER LO DE QUE SI ESTÁ A MENOS DE 5 CASILLAS DE LA SALIDA Y HAY CAMINO DIRECTO QUE VAYA HACIA ALLÍ

}
