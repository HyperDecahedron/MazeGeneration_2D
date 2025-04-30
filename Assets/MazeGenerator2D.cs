using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MazeGenerator2D : MonoBehaviour
{
    [SerializeField]
    private MazeCell2D mazeCellPrefab;

    [SerializeField]
    private int mazeWidth;

    [SerializeField]
    private int mazeHeight;

    private MazeCell2D[,] mazeGrid;

    //public MazeCell[,] visitedMazeGrid;

    void Start()
    {
        mazeGrid = new MazeCell2D[mazeWidth, mazeHeight];

        for (int x = 0; x < mazeWidth; x++)
        {
            for (int y = 0; y < mazeHeight; y++)
            {
                mazeGrid[x, y] = Instantiate(mazeCellPrefab, new Vector3(2*x, 2*y, 0), Quaternion.identity);
            }
        }
         
        GenerateMaze(null, mazeGrid[0, 0]);

        // add entrance and exit
        mazeGrid[0, 0].ClearLeftWall();
        mazeGrid[mazeWidth - 1, mazeHeight - 1].ClearRightWall();

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

    private void GenerateMaze(MazeCell2D previousCell, MazeCell2D currentCell)
    {
        // this method will be called recursively to generate the maze
        currentCell.Visit();                        // visit current cell
        ClearWalls(previousCell, currentCell);      // knock down walls between previous and current

        // loops until there are no neighbours remaining
        MazeCell2D nextCell;

        do
        {
            nextCell = GetNextUnvisitedCell(currentCell);

            if (nextCell != null)
            {
                // recursively call the generate method.
                // it goes cell to cell until it gets to the point of no visited neighbours
                GenerateMaze(currentCell, nextCell); // yield return to call a corroutine
            }

        } while (nextCell != null);
    }

    private MazeCell2D GetNextUnvisitedCell(MazeCell2D currentCell)
    {
        // gets all unvisited neighbours and picks one of them as random
        var unvisitedCells = GetUnvisitedCells(currentCell);

        return unvisitedCells.OrderBy(_ => Random.Range(1, 10)).FirstOrDefault();
    }

    private IEnumerable<MazeCell2D> GetUnvisitedCells(MazeCell2D currentCell)
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

    private void ClearWalls(MazeCell2D previousCell, MazeCell2D currentCell)
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
            nextCell = GetNextUnvisitedCell(currentCell);

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
}
