using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Game : MonoBehaviour
{
    public int size = 16;
    public int MineCount; 

    private GameBoard gameBoard;
    private GameCells[,] gameCells;

    private bool gameOver;

    private void Awake()
    {
        gameBoard = GetComponentInChildren<GameBoard>();
    }
    // Start is called before the first frame update
    private void Start()
    {
        MineCount = size * 2;
        NewGame();
        
    }

    private void NewGame()
    {
        gameOver = false;
        gameCells = new GameCells[size, size];

        GenerateCells();
        GenerateMines();
        GenerateNumbers();

        Camera.main.transform.position = new Vector3(size/2f, size/2f, -10f);
        gameBoard.DrawMap(gameCells);
    }

    private void GenerateCells()
    {
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                GameCells gameCell = new GameCells();
                gameCell.position = new Vector3Int(x, y,0);
                gameCell.type = GameCells.Type.Empty;
                gameCells[x, y] = gameCell;
            }
        }
    }

    private void GenerateMines()
    {
        for (int i = 0; i < MineCount; i++)
        {
            int x = Random.Range(0, size);
            int y = Random.Range(0, size);

            while(gameCells[x, y].type == GameCells.Type.Mine)
            {
                x++;
                if(x >= size)
                {
                    x = 0;
                    y++;
                    if(y>=size)
                    {
                        y = 0;
                    }
                }
            }
            gameCells[x, y].type = GameCells.Type.Mine;
        }
    }

    private void GenerateNumbers()
    {
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                GameCells gameCell = gameCells[x,y];
                if(gameCell.type == GameCells.Type.Mine)
                {
                    continue;
                }
                gameCell.number = CountMines(x,y);

                if(gameCell.number > 0)
                {
                    gameCell.type= GameCells.Type.Number;
                }
                gameCells[x, y] = gameCell;
            }
        }
    }

    private int CountMines(int cellX,int cellY)
    {
        int count = 0;
        for (int adjacentX = -1; adjacentX <=1; adjacentX++)
        {
            for (int adjacentY = -1; adjacentY <=1; adjacentY++)
            {
                if(adjacentX == 0 && adjacentY == 0)
                {
                    continue;
                }
                int x = cellX + adjacentX;
                int y = cellY + adjacentY;

                if(x<0 || x >= size || y < 0 || y>= size)
                {
                    continue;
                }
                if (GetCell(x,y).type == GameCells.Type.Mine)
                {
                    count++;
                }
            }
        }
        return count;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            NewGame();
        }
        else if (!gameOver)
        {
            if (Input.GetMouseButtonDown(1))
            {
                flag();
            }
            else if (Input.GetMouseButtonDown(0))
            {
                RemoveTile();
            }
        }
        
    }

    private void flag()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 gameCellPosition = gameBoard.tilemap.WorldToCell(worldPosition);
        GameCells gameCell = GetCell((int)gameCellPosition.x, (int)gameCellPosition.y);

        if(gameCell.type == GameCells.Type.Invalid || gameCell.revealed)
        {
            return;
        }
        gameCell.flagged = !gameCell.flagged;
        gameCells[(int)gameCellPosition.x, (int)gameCellPosition.y] = gameCell;
        gameBoard.DrawMap(gameCells);
    }

    private GameCells GetCell(int x, int y)
    {
        if (isValid(x, y))
        {
            return gameCells[x,y];
        }
        else
        {
            return new GameCells();
        }
    }

    private bool isValid(int x, int y)
    {
        return x>=0 && x < size && y >= 0 && y<size;
    }

    private void RemoveTile()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 gameCellPosition = gameBoard.tilemap.WorldToCell(worldPosition);
        GameCells gameCell = GetCell((int)gameCellPosition.x, (int)gameCellPosition.y);

        if (gameCell.type == GameCells.Type.Invalid || gameCell.revealed || gameCell.flagged)
        {
            return;
        }

        switch (gameCell.type)
        {
            case GameCells.Type.Empty:
                Flood(gameCell);
                CheckWinCond();
                break;
            case GameCells.Type.Mine:
                Lose(gameCell);
                break;
            default:
                gameCell.revealed = true;
                gameCells[(int)gameCellPosition.x, (int)gameCellPosition.y] = gameCell;
                CheckWinCond();
                break;
        }

        if(gameCell.type == GameCells.Type.Empty)
        {
            Flood(gameCell);
        }
      
        gameBoard.DrawMap(gameCells);
    }

    private void CheckWinCond()
    {
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                GameCells gameCell = gameCells[x, y];
                if(gameCell.type != GameCells.Type.Mine && !gameCell.revealed)
                {
                    return;
                }
            }
        }
        Debug.Log("You Won !");
        gameOver = true;
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                GameCells gameCell = gameCells[x, y];
                if (gameCell.type == GameCells.Type.Mine)
                {
                    gameCell.revealed = true;
                    gameCells[x, y] = gameCell;
                }
            }
        }
    }

    private void Lose(GameCells gameCell)
    {
        Debug.Log("Game Over !");
        gameOver = true;
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                gameCell = gameCells[x, y];
                if(gameCell.type == GameCells.Type.Mine)
                {
                    gameCell.revealed = true;
                    gameCells[x,y] = gameCell;
                }
            }
        }
    }

    private void Flood(GameCells gameCell)
    {
        if (gameCell.revealed)
        {
            return;
        }
        if(gameCell.type == GameCells.Type.Mine || gameCell.type == GameCells.Type.Invalid)
        {
            return;
        }

        gameCell.revealed = true;
        gameCells[gameCell.position.x,gameCell.position.y] = gameCell;

        if(gameCell.type == GameCells.Type.Empty) 
        {
            Flood(GetCell(gameCell.position.x-1,gameCell.position.y));
            Flood(GetCell(gameCell.position.x + 1, gameCell.position.y));
            Flood(GetCell(gameCell.position.x, gameCell.position.y - 1));
            Flood(GetCell(gameCell.position.x, gameCell.position.y + 1));
        }
    }

    //private bool isATile(int x, int y)
    //{
    //    //if ()
    //    //{

    //    //}
    //    return false;
    //}
}
