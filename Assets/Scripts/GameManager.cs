using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private GameObject floor;

    [SerializeField]
    private GameObject tilePrefab;
    [SerializeField]
    private Transform tileContainer;

    [SerializeField]
    private GameObject dropPrefab;
    [SerializeField]
    private Transform dropContainer;

    [SerializeField]
    private GameObject mainCamera;

    private int size = 8;

    [SerializeField]
    private Drop[,] board;
    private int nbFalling;

    private string[] colors;
    [SerializeField]
    private Sprite[] sprites;

    private void Start()
    {
        board = new Drop[size, size];
        colors = new string[] { "red", "yellow", "green", "blue" };

        SetFloor();
        InitBoard();

        mainCamera.transform.position = new Vector3(0.5f * (size - 1), 0.5f * (size - 1), -1);
    }

    private void SetFloor() {
        floor.transform.position = new Vector2(0.5f * (size - 1), -1);
        floor.transform.localScale = new Vector2(0.5f * size, 0.5f);
    }

    private void InitBoard()
    {
        GameObject t;
        Vector2 pos;
        Drop d;

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                pos = new Vector2(i, j);
                t = Instantiate(tilePrefab, pos, Quaternion.identity);
                t.transform.parent = tileContainer;

                t = Instantiate(dropPrefab, pos, Quaternion.identity);
                t.transform.parent = dropContainer;

                d = t.GetComponent<Drop>();
                d.SetCoordinates(i, j);
                SetInitialColor(d, i, j);

                board[i, j] = d;
            }
        }
    }

    private void SetInitialColor(Drop drop, int i, int j)
    {
        List<string> possibleColors = new List<string>(colors);

        string left1 = GetDrop(i - 1, j);
        string left2 = GetDrop(i - 2, j);
        if (left2 != null && left1 == left2)
        {
            possibleColors.Remove(left2);
        }

        string down1 = GetDrop(i, j - 1);
        string down2 = GetDrop(i, j - 2);
        if (down2 != null && down1 == down2)
        {
            possibleColors.Remove(down2);
        }

        SetColor(drop, possibleColors[Random.Range(0, possibleColors.Count)]);
    }

    private void SetColor(Drop drop, string color) {
        drop.color = color;

        Sprite newSprite = sprites[0];

        switch (drop.color)
        {
            case "red":
                newSprite = sprites[0];
                break;
            case "yellow":
                newSprite = sprites[1];
                break;
            case "green":
                newSprite = sprites[2];
                break;
            case "blue":
                newSprite = sprites[3];
                break;
            default:
                newSprite = null;
                break;
        }

        drop.GetComponent<SpriteRenderer>().sprite = newSprite;
    }

    public string GetDrop(int i, int j)
    {
        if (i < 0 || i >= size || j < 0 || j >= size || !board[i, j])
        {
            return null;
        }

        return board[i, j].color;
    }

    public void DropHasLanded() {
        nbFalling -= 1;
    }

    public void Swap(Drop drop1, Drop drop2)
    {
        if (!drop1 || !drop2)
        {
            return;
        }

        string temp = drop1.color;
        SetColor(drop1, drop2.color);
        SetColor(drop2, temp);

        if (!ValidMove())
        {
            SetColor(drop2, drop1.color);
            SetColor(drop1, temp);
        }
        else
        {
            StartCoroutine(FallingRoutine());
        }
    }

    private bool ValidMove()
    {
        HashSet<Drop> matchedDrops = new HashSet<Drop>();
        HashSet<Drop> horizontalMatches = new HashSet<Drop>();
        HashSet<Drop> verticalMatches = new HashSet<Drop>();

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                string currentColor = GetDrop(i, j);
                if (currentColor == null) {
                    continue;
                }

                horizontalMatches = FindHorizontalMatches(i, j, currentColor);
                if (horizontalMatches.Count >= 2)
                {
                    matchedDrops.UnionWith(horizontalMatches);
                    matchedDrops.Add(board[i, j]);
                }

                verticalMatches = FindVerticalMatches(i, j, currentColor);
                if (verticalMatches.Count >= 2)
                {
                    matchedDrops.UnionWith(verticalMatches);
                    matchedDrops.Add(board[i, j]);
                }
            }
        }

        foreach (Drop drop in matchedDrops)
        {
            if (drop)
            {
                board[drop.i, drop.j] = null;
                Destroy(drop.gameObject);
            }
        }

        return matchedDrops.Count > 0;
    }

    HashSet<Drop> FindHorizontalMatches(int col, int row, string color)
    {
        HashSet<Drop> matches = new HashSet<Drop>();

        for (int i = col + 1; i < size; i++)
        {
            string nextDrop = GetDrop(i, row);
            
            if (nextDrop != color)
            {
                break;
            }
            matches.Add(board[i, row]);
        }
        return matches;
    }

    HashSet<Drop> FindVerticalMatches(int col, int row, string color)
    {
        HashSet<Drop> matches = new HashSet<Drop>();

        for (int j = row + 1; j < size; j++)
        {
            string nextDrop = GetDrop(col, j);
            if (nextDrop != color)
            {
                break;
            }
            matches.Add(board[col, j]);
        }
        return matches;
    }

    private IEnumerator FallingRoutine()
    {
        DropDrops();

        while (nbFalling > 0)
        {
            yield return new WaitForSeconds(0.5f);
        }

        if (ValidMove())
        {
            StartCoroutine(FallingRoutine());
        }
    }

    void DropDrops()
    {
        Drop currentDrop;
        Drop fallingDrop;

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                currentDrop = board[i, j];
                if (!currentDrop) {
                    for (int k = j; k < size; k++) {
                        fallingDrop = board[i, k];
                        if (fallingDrop) {
                            fallingDrop.Fall(j);
                            board[i, j] = fallingDrop;
                            board[i, k] = null;

                            nbFalling += 1;
                            break;
                        }
                    }
                }
            }
        }
    }

    private void PrintBoard() {
        string message;
        string color;

        for (int j = 0; j < size; j++) {
            message = "";
            for (int i = 0; i < size - 1; i++)
            {
                color = GetDrop(i, j);
                if (color == null)
                {
                    message += "empty, ";
                }
                else
                {
                    message += color + ", ";
                }
            }

            color = GetDrop(size - 1, j);
            if (color == null)
            {
                message += "empty";
            }
            else
            {
                message += color;
            }

            Debug.Log(message);
        }
    }
}
