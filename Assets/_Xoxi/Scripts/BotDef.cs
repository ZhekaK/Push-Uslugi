using System.Collections.Generic;
using UnityEngine;

public class BotDef : MonoBehaviour
{
    public List<int> dungerCol = new List<int>();
    public List<int> dungerLine = new List<int>();

    public void BotLooseCheck()
    {
        ClearDangerCells();
        CheckCol();
    }

    public void CheckDungerCells()
    {
        if (Controller.instance.botAtc.winingCol.Count > 0)
        {
            for (int i = 0; i < Controller.instance.botAtc.winingCol.Count; i++)
            {
                dungerCol.Add(Controller.instance.botAtc.winingCol[i]);
                dungerLine.Add(Controller.instance.botAtc.winingLine[i]);
            }
        }

        for (int i = dungerCol.Count - 1; i >= 0; i--)
        {
            if (!IsInsideCell(dungerCol[i], dungerLine[i]) || Controller.instance.coloumn[dungerCol[i]].line[dungerLine[i]] != 0)
            {
                RemoveDangerAt(i);
            }
        }
    }
    public void CheckCol()
    {
        int winDetect = 0;
        for (int i = 0; i < Controller.instance.coloumn.Length; i++)
        {
            winDetect = 0;
            for (int j = 0; j < Controller.instance.coloumn[i].line.Length; j++)
            {
                if (Controller.instance.coloumn[i].line[j] == (int)Controller.instance.playerFigure + 1)
                {
                    winDetect++;
                    if (winDetect >= 3 && Controller.instance.coloumn[i].line[4] == 0 && Controller.instance.coloumn[i].line[j + 1] == 0)
                    {
                        Controller.instance.SetFigure(i);
                        Debug.LogWarning("Warning: <Coloumn> " + i);
                        //Debug.Log("Bot turned");
                        return;
                    }
                }
                else
                {
                    winDetect = 0;
                }
            }
        }

        CheckLines();
    }

    public void CheckLines()
    {
        int winDetect = 0;
        for (int i = 0; i < 5; i++)
        {
            winDetect = 0;
            for (int j = 0; j < Controller.instance.coloumn.Length; j++)
            {
                if (Controller.instance.coloumn[j].line[i] == (int)Controller.instance.playerFigure + 1)
                {
                    winDetect++;
                    if (j + 1 < 9 && j - winDetect >= 0 && winDetect >= 2 && Controller.instance.coloumn[j + 1].line[i] == 0 && Controller.instance.coloumn[j - winDetect].line[i] == 0)
                    {
                        if (i == 0 || (i != 0 && Controller.instance.coloumn[j + 1].line[i - 1] != 0 && Controller.instance.coloumn[j - winDetect].line[i - 1] != 0))
                        {
                            if (i != 0 && j < 8 && Controller.instance.coloumn[j + 2].line[i - 1] != 0)
                            {
                                Controller.instance.SetFigure(j + 1);
                                Debug.LogWarning("Warning: <Line> " + (j + 1));
                                return;
                            }
                            else if (i != 0 && j > 2 && winDetect == 2 && Controller.instance.coloumn[j - winDetect - 1].line[i - 1] != 0)
                            {
                                Controller.instance.SetFigure(j + 1);
                                Debug.LogWarning("Warning: <Line> " + (j + 1));
                                return;
                            }
                            else
                            {
                                int tempChance = UnityEngine.Random.Range(0, 100);
                                if (tempChance < 50)
                                {
                                    Controller.instance.SetFigure(j - winDetect);
                                }
                                else
                                {
                                    Controller.instance.SetFigure(j + 1);
                                }
                                Debug.LogWarning("Warning: <Line> " + (j + 1));
                                return;
                            }
                        }
                        else if (Controller.instance.coloumn[j + 1].line[i - 1] == 0 || Controller.instance.coloumn[j - winDetect].line[i - 1] == 0)
                        {
                            dungerCol.Add(j + 1);
                            dungerLine.Add(i - 1);
                            dungerCol.Add(j - winDetect);
                            dungerLine.Add(i - 1);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                    else if (j + 1 <= 9 && winDetect >= 3 && Controller.instance.coloumn[j + 1].line[i] == 0)
                    {
                        if (i == 0 || (i != 0 && Controller.instance.coloumn[j + 1].line[i - 1] != 0))
                        {
                            Controller.instance.SetFigure(j + 1);
                            Debug.LogWarning("Warning: <Line> " + (j + 1));
                            return;
                        }
                        else if (Controller.instance.coloumn[j + 1].line[i - 1] == 0)
                        {
                            dungerCol.Add(j + 1);
                            dungerLine.Add(i - 1);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                    else if (j - winDetect >= 0 && winDetect >= 3 && Controller.instance.coloumn[j - winDetect].line[i] == 0)
                    {
                        if (i == 0 || (i != 0 && Controller.instance.coloumn[j - winDetect].line[i - 1] != 0))
                        {
                            Controller.instance.SetFigure(j - winDetect);
                            Debug.LogWarning("Warning: <Line> " + (j - winDetect));
                            //Debug.Log("Bot turned");
                            return;
                        }
                        else if (Controller.instance.coloumn[j - winDetect].line[i - 1] == 0)
                        {
                            dungerCol.Add(j - winDetect);
                            dungerLine.Add(i - 1);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                }
                else
                {
                    winDetect = 0;
                }
            }
        }

        for (int i = 0; i < 5; i++)
        {
            int a = 0;
            winDetect = 0;
            for (int j = 0; j < Controller.instance.coloumn.Length; j++)
            {
                if (Controller.instance.coloumn[j].line[i] == (int)Controller.instance.playerFigure + 1)
                {
                    winDetect++;
                    if (winDetect >= 1)
                    {
                        a = 0;
                    }
                }
                else
                {
                    a++;
                    if (a > 1)
                    {
                        winDetect = 0;
                    }
                    else if (j < 9 && j > 1 && Controller.instance.coloumn[j + 1].line[i] == (int)Controller.instance.playerFigure + 1 && Controller.instance.coloumn[j - 1].line[i] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[j - 2].line[i] == (int)Controller.instance.playerFigure + 1 && Controller.instance.coloumn[j].line[i] == 0)
                    {
                        if (i == 0 || (Controller.instance.coloumn[j].line[i - 1] != 0))
                        {
                            Controller.instance.SetFigure(j);
                            Debug.LogWarning("Warning: <Line> " + (j));
                            return;
                        }
                        else if (Controller.instance.coloumn[j].line[i - 1] == 0)
                        {
                            dungerCol.Add(j);
                            dungerLine.Add(i - 1);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                    else if (j > 0 && j < 8 && Controller.instance.coloumn[j + 1].line[i] == (int)Controller.instance.playerFigure + 1 && Controller.instance.coloumn[j - 1].line[i] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[j + 2].line[i] == (int)Controller.instance.playerFigure + 1 && Controller.instance.coloumn[j].line[i] == 0)
                    {
                        if (i == 0 || (Controller.instance.coloumn[j].line[i - 1] != 0))
                        {
                            Controller.instance.SetFigure(j);
                            Debug.LogWarning("Warning: <Line> " + (j));
                            return;
                        }
                        else if (Controller.instance.coloumn[j].line[i - 1] == 0)
                        {
                            dungerCol.Add(j);
                            dungerLine.Add(i - 1);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                    else if (j > 1 && j < 8 && Controller.instance.coloumn[j + 1].line[i] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[j - 1].line[i] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[j + 2].line[i] == 0 && Controller.instance.coloumn[j - 2].line[i] == 0
                        && Controller.instance.coloumn[j].line[i] == 0 && (i == 0 || i > 0 && Controller.instance.coloumn[j + 2].line[i - 1] != 0 && Controller.instance.coloumn[j - 2].line[i - 1] != 0))
                    {
                        if (i == 0 || (Controller.instance.coloumn[j].line[i - 1] != 0))
                        {
                            Controller.instance.SetFigure(j);
                            Debug.LogWarning("Warning: <Line> " + (j));
                            return;
                        }
                        else if (Controller.instance.coloumn[j].line[i - 1] == 0 && Controller.instance.coloumn[j + 2].line[i - 1] != 0 && Controller.instance.coloumn[j - 2].line[i - 1] != 0)
                        {
                            dungerCol.Add(j);
                            dungerLine.Add(i - 1);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                }
            }
        }

        CheckDiagonalsLeft();
    }

    public void CheckDiagonalsLeft()
    {
        int winDetect = 0;
        for (int j = 0; j < 4; j++)
        {
            if (Controller.instance.coloumn[j].line[j + 1] == (int)Controller.instance.playerFigure + 1)
            {
                winDetect++;
                if (winDetect == 2 && j == 1 && Controller.instance.coloumn[j + 2].line[j + 3] == (int)Controller.instance.playerFigure + 1 && Controller.instance.coloumn[j + 1].line[j + 2] == 0)
                {
                    if (Controller.instance.coloumn[j + 1].line[j + 1] != 0)
                    {
                        Controller.instance.SetFigure(j + 1);
                        Debug.LogWarning("Warning: <Diagonals> " + (j + 1));
                        return;
                    }
                    else if (Controller.instance.coloumn[j + 1].line[j + 1] == 0)
                    {
                        dungerCol.Add(j + 1);
                        dungerLine.Add(j + 1);
                        Debug.Log("DO NOT SET THIS");
                    }
                }
                if (winDetect == 2 && j == 3 && Controller.instance.coloumn[0].line[1] == (int)Controller.instance.playerFigure + 1 && Controller.instance.coloumn[1].line[2] == 0)
                {
                    if (Controller.instance.coloumn[1].line[1] != 0)
                    {
                        Controller.instance.SetFigure(1);
                        Debug.LogWarning("Warning: <Diagonals> " + (1));
                        return;
                    }
                    else if (Controller.instance.coloumn[1].line[1] == 0)
                    {
                        dungerCol.Add(1);
                        dungerLine.Add(1);
                        Debug.Log("DO NOT SET THIS");
                    }
                }
                if (winDetect == 3)
                {
                    if (j == 2 && Controller.instance.coloumn[3].line[4] == 0)
                    {
                        if (Controller.instance.coloumn[3].line[3] != 0)
                        {
                            Controller.instance.SetFigure(3);
                            Debug.LogWarning("Warning: <Diagonals> " + (3));
                            return;
                        }
                        else
                        {
                            dungerCol.Add(3);
                            dungerLine.Add(3);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                    if (j == 3 && Controller.instance.coloumn[0].line[1] == 0)
                    {
                        if (Controller.instance.coloumn[0].line[0] != 0)
                        {
                            Controller.instance.SetFigure(0);
                            Debug.LogWarning("Warning: <Diagonals> " + (0));
                            return;
                        }
                        else
                        {
                            dungerCol.Add(0);
                            dungerLine.Add(0);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                }

            }
            else
            {
                winDetect = 0;
            }
        }

        winDetect = 0;

        for (int j = 0; j < 4; j++)
        {
            if (Controller.instance.coloumn[j + 6].line[j] == (int)Controller.instance.playerFigure + 1)
            {
                winDetect++;
                if (winDetect == 2 && j == 1 && Controller.instance.coloumn[9].line[3] == (int)Controller.instance.playerFigure + 1 && Controller.instance.coloumn[8].line[2] == 0)
                {
                    if (Controller.instance.coloumn[8].line[1] != 0)
                    {
                        Controller.instance.SetFigure(8);
                        Debug.LogWarning("Warning: <Diagonals> " + (8));
                        return;
                    }
                    else if (Controller.instance.coloumn[8].line[1] == 0)
                    {
                        dungerCol.Add(8);
                        dungerLine.Add(1);
                        Debug.Log("DO NOT SET THIS");
                    }
                }
                if (winDetect == 2 && j == 3 && Controller.instance.coloumn[6].line[0] == (int)Controller.instance.playerFigure + 1 && Controller.instance.coloumn[7].line[1] == 0)
                {
                    if (Controller.instance.coloumn[7].line[0] != 0)
                    {
                        Controller.instance.SetFigure(7);
                        Debug.LogWarning("Warning: <Diagonals> " + (7));
                        return;
                    }
                    else if (Controller.instance.coloumn[7].line[0] == 0)
                    {
                        dungerCol.Add(7);
                        dungerLine.Add(0);
                        Debug.Log("DO NOT SET THIS");
                    }
                }
                if (winDetect == 3)
                {
                    if (j == 2 && Controller.instance.coloumn[9].line[3] == 0)
                    {
                        if (Controller.instance.coloumn[9].line[2] != 0)
                        {
                            Controller.instance.SetFigure(9);
                            Debug.LogWarning("Warning: <Diagonals> " + (9));
                            return;
                        }
                        else
                        {
                            dungerCol.Add(9);
                            dungerLine.Add(2);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                    if (j == 3 && Controller.instance.coloumn[6].line[0] == 0)
                    {
                        Controller.instance.SetFigure(6);
                        Debug.LogWarning("Warning: <Diagonals> " + (6));
                        return;
                    }
                }

            }
            else
            {
                winDetect = 0;
            }
        }

        for (int i = 0; i < Controller.instance.coloumn.Length - 4; i++)
        {
            winDetect = 0;
            for (int j = 0; j < 5; j++)
            {
                if (Controller.instance.coloumn[i + j].line[j] == (int)Controller.instance.playerFigure + 1)
                {
                    winDetect++;
                    if (winDetect == 2 && Controller.instance.coloumn[i + 0].line[0] == 0 && Controller.instance.coloumn[i + 4].line[4] == 0)
                    {
                        for (int o = 1; o < 4; o++)
                        {
                            if (Controller.instance.coloumn[i + o].line[o] == 0 && Controller.instance.coloumn[i + o].line[o - 1] != 0)
                            {
                                Controller.instance.SetFigure(i + o);
                                Debug.LogWarning("Warning: <Diagonals> " + (i + o));
                                return;
                            }
                            else if (Controller.instance.coloumn[i + o].line[o - 1] == 0)
                            {
                                dungerCol.Add(i + o);
                                dungerLine.Add(o - 1);
                                Debug.Log("DO NOT SET THIS");
                            }
                        }
                    }
                    if (winDetect == 3)
                    {
                        if (j < 4 && Controller.instance.coloumn[i + j + 1].line[j + 1] == 0 && Controller.instance.coloumn[i + j + 1].line[j + 1 - 1] != 0)
                        {
                            Controller.instance.SetFigure(i + j + 1);
                            Debug.LogWarning("Warning: <Diagonals> " + (i + j + 1));
                            return;
                        }
                        else if (j < 4 && Controller.instance.coloumn[i + j + 1].line[j + 1 - 1] == 0)
                        {
                            dungerCol.Add(i + j + 1);
                            dungerLine.Add(j + 1 - 1);
                            Debug.Log("DO NOT SET THIS");
                        }
                        if (j >= winDetect && Controller.instance.coloumn[i + j - winDetect].line[j - winDetect] == 0
                            && (j - winDetect == 0 || (j - winDetect != 0 && Controller.instance.coloumn[i + j - winDetect].line[j - winDetect - 1] != 0)))
                        {
                            Controller.instance.SetFigure(i + j - winDetect);
                            Debug.LogWarning("Warning: <Diagonals> " + (i + j - winDetect));
                            return;
                        }
                        else if (j > winDetect && Controller.instance.coloumn[i + j - winDetect].line[j - winDetect - 1] == 0)
                        {
                            dungerCol.Add(i + j - winDetect);
                            dungerLine.Add(j - winDetect - 1);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                }
                else
                {
                    winDetect = 0;
                    if (j > 0 && j < 3 && Controller.instance.coloumn[i + j - 1].line[j - 1] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[i + j + 1].line[j + 1] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[i + j + 2].line[j + 2] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[i + j].line[j] == 0)
                    {
                        if (Controller.instance.coloumn[i + j].line[j - 1] != 0)
                        {
                            Controller.instance.SetFigure(i + j);
                            Debug.LogWarning("Warning: <Diagonals> " + (i + j));
                            return;
                        }
                        else
                        {
                            dungerCol.Add(i + j);
                            dungerLine.Add(j - 1);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                    if (j > 1 && j < 4 && Controller.instance.coloumn[i + j - 1].line[j - 1] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[i + j + 1].line[j + 1] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[i + j - 2].line[j - 2] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[i + j].line[j] == 0)
                    {
                        if (Controller.instance.coloumn[i + j].line[j - 1] != 0)
                        {
                            Controller.instance.SetFigure(i + j);
                            Debug.LogWarning("Warning: <Diagonals> " + (i + j));
                            return;
                        }
                        else
                        {
                            dungerCol.Add(i + j);
                            dungerLine.Add(j - 1);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                    if (j > 1 && j < 3 && Controller.instance.coloumn[i + j + 1].line[j + 1] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[i + j - 1].line[j - 1] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[i + j - 2].line[j - 2] == 0
                        && Controller.instance.coloumn[i + j + 2].line[j + 2] == 0
                        && Controller.instance.coloumn[i + j].line[j] == 0)
                    {
                        if (Controller.instance.coloumn[i + j].line[j - 1] != 0)
                        {
                            Controller.instance.SetFigure(i + j);
                            Debug.LogWarning("Warning: <Diagonals> " + (i + j));
                            return;
                        }
                        else
                        {
                            dungerCol.Add(i + j);
                            dungerLine.Add(j - 1);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                }
            }
        }
        CheckDiagonalsRight();
    }

    public void CheckDiagonalsRight()
    {
        int winDetect = 0;
        for (int j = 0; j < 4; j++)
        {
            if (Controller.instance.coloumn[j].line[3 - j] == (int)Controller.instance.playerFigure + 1)
            {
                winDetect++;
                if (winDetect == 2 && j == 1 && Controller.instance.coloumn[3].line[0] == (int)Controller.instance.playerFigure + 1 && Controller.instance.coloumn[2].line[1] == 0)
                {
                    if (Controller.instance.coloumn[2].line[1 - 1] != 0)
                    {
                        Controller.instance.SetFigure(2);
                        Debug.LogWarning("Warning: <Diagonals> " + (2));
                        return;
                    }
                    else if (Controller.instance.coloumn[2].line[1 - 1] == 0)
                    {
                        dungerCol.Add(2);
                        dungerLine.Add(0);
                        Debug.Log("DO NOT SET THIS");
                    }
                }
                if (winDetect == 2 && j == 3 && Controller.instance.coloumn[0].line[3] == (int)Controller.instance.playerFigure + 1 && Controller.instance.coloumn[1].line[2] == 0)
                {
                    if (Controller.instance.coloumn[1].line[1] != 0)
                    {
                        Controller.instance.SetFigure(1);
                        Debug.LogWarning("Warning: <Diagonals> " + (1));
                        return;
                    }
                    else if (Controller.instance.coloumn[1].line[1] == 0)
                    {
                        dungerCol.Add(1);
                        dungerLine.Add(1);
                        Debug.Log("DO NOT SET THIS");
                    }
                }
                if (winDetect == 3)
                {
                    if (j == 2 && Controller.instance.coloumn[3].line[0] == 0)
                    {
                        Controller.instance.SetFigure(3);
                        Debug.LogWarning("Warning: <Diagonals> " + (3));
                        return;
                    }
                    if (j == 3 && Controller.instance.coloumn[0].line[3] == 0)
                    {
                        if (Controller.instance.coloumn[0].line[2] != 0)
                        {
                            Controller.instance.SetFigure(0);
                            Debug.LogWarning("Warning: <Diagonals> " + (0));
                            return;
                        }
                        else
                        {
                            dungerCol.Add(0);
                            dungerLine.Add(2);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                }

            }
            else
            {
                winDetect = 0;
            }
        }

        winDetect = 0;

        for (int j = 0; j < 4; j++)
        {
            if (Controller.instance.coloumn[j + 6].line[4 - j] == (int)Controller.instance.playerFigure + 1)
            {
                winDetect++;
                if (winDetect == 2 && j == 1 && Controller.instance.coloumn[9].line[1] == (int)Controller.instance.playerFigure + 1 && Controller.instance.coloumn[8].line[2] == 0)
                {
                    if (Controller.instance.coloumn[8].line[1] != 0)
                    {
                        Controller.instance.SetFigure(8);
                        Debug.LogWarning("Warning: <Diagonals> " + (8));
                        return;
                    }
                    else if (Controller.instance.coloumn[8].line[1] == 0)
                    {
                        dungerCol.Add(8);
                        dungerLine.Add(1);
                        Debug.Log("DO NOT SET THIS");
                    }
                }
                if (winDetect == 2 && j == 3 && Controller.instance.coloumn[6].line[4] == (int)Controller.instance.playerFigure + 1 && Controller.instance.coloumn[7].line[3] == 0)
                {
                    if (Controller.instance.coloumn[7].line[2] != 0)
                    {
                        Controller.instance.SetFigure(7);
                        Debug.LogWarning("Warning: <Diagonals> " + (7));
                        return;
                    }
                    else if (Controller.instance.coloumn[7].line[2] == 0)
                    {
                        dungerCol.Add(7);
                        dungerLine.Add(2);
                        Debug.Log("DO NOT SET THIS");
                    }
                }
                if (winDetect == 3)
                {
                    if (j == 2 && Controller.instance.coloumn[9].line[1] == 0)
                    {
                        if (Controller.instance.coloumn[9].line[0] != 0)
                        {
                            Controller.instance.SetFigure(9);
                            Debug.LogWarning("Warning: <Diagonals> " + (9));
                            return;
                        }
                        else
                        {
                            dungerCol.Add(9);
                            dungerLine.Add(0);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                    if (j == 3 && Controller.instance.coloumn[6].line[4] == 0)
                    {
                        if (Controller.instance.coloumn[6].line[3] != 0)
                        {
                            Controller.instance.SetFigure(6);
                            Debug.LogWarning("Warning: <Diagonals> " + (6));
                            return;
                        }
                        else
                        {
                            dungerCol.Add(6);
                            dungerLine.Add(3);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                }

            }
            else
            {
                winDetect = 0;
            }
        }


        for (int i = 4; i < Controller.instance.coloumn.Length; i++)
        {
            winDetect = 0;
            for (int j = 4; j >= 0; j--)
            {
                if (Controller.instance.coloumn[i - j].line[j] == (int)Controller.instance.playerFigure + 1)
                {
                    winDetect++;
                    if (winDetect == 2 && Controller.instance.coloumn[i - 0].line[0] == 0 && Controller.instance.coloumn[i - 4].line[4] == 0)
                    {
                        for (int o = 3; o > 0; o--)
                        {
                            if (Controller.instance.coloumn[i - o].line[o] == 0 && Controller.instance.coloumn[i - o].line[o - 1] != 0)
                            {
                                Controller.instance.SetFigure(i - o);
                                Debug.LogWarning("Warning: <Diagonals> " + (i - o));
                                return;
                            }
                            else if (Controller.instance.coloumn[i - o].line[o - 1] == 0)
                            {
                                dungerCol.Add(i - o);
                                dungerLine.Add(o - 1);
                                Debug.Log("DO NOT SET THIS");
                            }
                        }
                    }
                    if (winDetect == 3)
                    {
                        if (j > 0 && Controller.instance.coloumn[i - j + 1].line[j - 1] == 0 && (j == 1 || Controller.instance.coloumn[i - j + 1].line[j - 1 - 1] != 0))
                        {
                            Controller.instance.SetFigure(i - j + 1);
                            Debug.LogWarning("Warning: <Diagonals> " + (i - j + 1));
                            return;
                        }
                        else if (j > 1 && Controller.instance.coloumn[i - j + 1].line[j - 1 - 1] == 0)
                        {
                            dungerCol.Add(i - j + 1);
                            dungerLine.Add(j - 1 - 1);
                            Debug.Log("DO NOT SET THIS");
                        }
                        if (j + 1 < winDetect && Controller.instance.coloumn[i - j - winDetect].line[j + winDetect] == 0
                            && Controller.instance.coloumn[i - j - winDetect].line[j + winDetect - 1] != 0)
                        {
                            Controller.instance.SetFigure(i - j - winDetect);
                            Debug.LogWarning("Warning: <Diagonals> " + (i - j - winDetect));
                            return;
                        }
                        else if (j + 1 < winDetect && Controller.instance.coloumn[i - j - winDetect].line[j + winDetect - 1] == 0)
                        {
                            dungerCol.Add(i - j - winDetect);
                            dungerLine.Add(j + winDetect - 1);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                }
                else
                {
                    winDetect = 0;
                    if ((j == 2 || j == 1) && Controller.instance.coloumn[i - j + 1].line[j - 1] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[i - j - 1].line[j + 1] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[i - j - 2].line[j + 2] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[i - j].line[j] == 0)
                    {
                        if (Controller.instance.coloumn[i - j].line[j - 1] != 0)
                        {
                            Controller.instance.SetFigure(i - j);
                            Debug.LogWarning("Warning: <Diagonals> " + (i - j));
                            return;
                        }
                        else
                        {
                            dungerCol.Add(i - j);
                            dungerLine.Add(j - 1);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                    if ((j == 3 || j == 2) && Controller.instance.coloumn[i - j - 1].line[j + 1] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[i - j + 1].line[j - 1] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[i - j + 2].line[j - 2] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[i - j].line[j] == 0)
                    {
                        if (Controller.instance.coloumn[i - j].line[j - 1] != 0)
                        {
                            Controller.instance.SetFigure(i - j);
                            Debug.LogWarning("Warning: <Diagonals> " + (i - j));
                            return;
                        }
                        else
                        {
                            dungerCol.Add(i - j);
                            dungerLine.Add(j - 1);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                    if (j == 2 && Controller.instance.coloumn[i - j + 1].line[j - 1] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[i - j - 1].line[j + 1] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[i - j - 2].line[j + 2] == 0
                        && Controller.instance.coloumn[i - j + 2].line[j - 2] == 0
                        && Controller.instance.coloumn[i - j].line[j] == 0)
                    {
                        if (Controller.instance.coloumn[i - j].line[j - 1] != 0)
                        {
                            Controller.instance.SetFigure(i - j);
                            Debug.LogWarning("Warning: <Diagonals> " + (i - j));
                            return;
                        }
                        else
                        {
                            dungerCol.Add(i - j);
                            dungerLine.Add(j - 1);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                }
            }
        }

        ForkCheck();
    }

    public void ForkCheck()
    {
        for (int lines = 0; lines < 5; lines++)
        {
            for (int cols = 0; cols < Controller.instance.coloumn.Length; cols++)
            {
                //   x
                //  xo
                // ?oo
                //x?xx
                if (lines < 2 && cols > 1 && cols < 9)
                {
                    if (Controller.instance.coloumn[cols].line[lines] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[cols].line[lines + 2] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[cols + 1].line[lines + 3] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[cols + 1].line[lines] == (int)Controller.instance.playerFigure + 1)
                    {
                        if (Controller.instance.coloumn[cols - 2].line[lines] == 0 && (lines == 0 || Controller.instance.coloumn[cols - 2].line[lines - 1] != 0))
                        {
                            Controller.instance.SetFigure(cols - 2);
                            Debug.LogWarning("Warning: <FORK> " + (cols - 2));
                            return;
                        }
                        else if (lines != 0 && Controller.instance.coloumn[cols - 2].line[lines - 1] == 0)
                        {
                            dungerCol.Add(cols - 2);
                            dungerLine.Add(lines - 1);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                }
                if (lines < 2 && cols > 2 && cols < 9)
                {
                    if (Controller.instance.coloumn[cols].line[lines] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[cols].line[lines + 2] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[cols + 1].line[lines + 3] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[cols - 2].line[lines] == (int)Controller.instance.playerFigure + 1)
                    {
                        if (Controller.instance.coloumn[cols - 3].line[lines] == 0 && (lines == 0 || Controller.instance.coloumn[cols - 3].line[lines - 1] != 0))
                        {
                            Controller.instance.SetFigure(cols - 3);
                            Debug.LogWarning("Warning: <FORK> " + (cols - 3));
                            return;
                        }
                        else if (lines != 0 && Controller.instance.coloumn[cols - 3].line[lines - 1] == 0)
                        {
                            dungerCol.Add(cols - 3);
                            dungerLine.Add(lines - 1);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                }
                //то же самое, но в другую сторону
                if (lines < 2 && cols > 0 && cols < 8)
                {
                    if (Controller.instance.coloumn[cols].line[lines] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[cols].line[lines + 2] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[cols - 1].line[lines + 3] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[cols - 1].line[lines] == (int)Controller.instance.playerFigure + 1)
                    {
                        if (Controller.instance.coloumn[cols + 2].line[lines] == 0
                            && (lines == 0 || Controller.instance.coloumn[cols + 2].line[lines - 1] != 0))
                        {
                            Controller.instance.SetFigure(cols + 2);
                            Debug.LogWarning("Warning: <FORK> " + (cols + 2));
                            return;
                        }
                        else if (lines != 0 && Controller.instance.coloumn[cols + 2].line[lines - 1] == 0)
                        {
                            dungerCol.Add(cols + 2);
                            dungerLine.Add(lines - 1);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                }
                if (lines < 2 && cols > 0 && cols < 7)
                {
                    if (Controller.instance.coloumn[cols].line[lines] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[cols].line[lines + 2] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[cols - 1].line[lines + 3] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[cols + 2].line[lines] == (int)Controller.instance.playerFigure + 1)
                    {
                        if (Controller.instance.coloumn[cols + 3].line[lines] == 0
                            && (lines == 0 || Controller.instance.coloumn[cols + 3].line[lines - 1] != 0))
                        {
                            Controller.instance.SetFigure(cols + 3);
                            Debug.LogWarning("Warning: <FORK> " + (cols + 3));
                            return;
                        }
                        else if (lines != 0 && Controller.instance.coloumn[cols + 3].line[lines - 1] == 0)
                        {
                            dungerCol.Add(cols + 3);
                            dungerLine.Add(lines - 1);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                }


                //x?xx
                // ?oo
                //  xo
                //   x
                if (lines > 2 && cols > 1 && cols < 9)
                {
                    if (Controller.instance.coloumn[cols].line[lines] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[cols].line[lines - 2] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[cols + 1].line[lines - 3] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[cols + 1].line[lines] == (int)Controller.instance.playerFigure + 1)
                    {
                        if (Controller.instance.coloumn[cols - 2].line[lines] == 0 && Controller.instance.coloumn[cols - 2].line[lines - 1] != 0)
                        {
                            Controller.instance.SetFigure(cols - 2);
                            Debug.LogWarning("Warning: <FORK> " + (cols - 2));
                            return;
                        }
                        else if (Controller.instance.coloumn[cols - 2].line[lines - 1] == 0)
                        {
                            dungerCol.Add(cols - 2);
                            dungerLine.Add(lines - 1);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                }
                //то же самое, но в другую сторону
                if (lines > 2 && cols > 0 && cols < 8)
                {
                    if (Controller.instance.coloumn[cols].line[lines] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[cols].line[lines - 2] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[cols - 1].line[lines - 3] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[cols - 1].line[lines] == (int)Controller.instance.playerFigure + 1)
                    {
                        if (Controller.instance.coloumn[cols + 2].line[lines] == 0
                            && Controller.instance.coloumn[cols + 2].line[lines - 1] != 0)
                        {
                            Controller.instance.SetFigure(cols + 2);
                            Debug.LogWarning("Warning: <FORK> " + (cols + 2));
                            return;
                        }
                        else if (Controller.instance.coloumn[cols + 2].line[lines - 1] == 0)
                        {
                            dungerCol.Add(cols + 2);
                            dungerLine.Add(lines - 1);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                }


                //   x
                //  ?o
                //xx?x
                //xoxo
                if (lines < 2 && cols < 7)
                {
                    if (Controller.instance.coloumn[cols].line[lines] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[cols + 1].line[lines + 1] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[cols + 3].line[lines + 1] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[cols + 3].line[lines + 3] == (int)Controller.instance.playerFigure + 1)
                    {
                        if (Controller.instance.coloumn[cols].line[lines + 1] == 0)
                        {
                            Controller.instance.SetFigure(cols);
                            Debug.LogWarning("Warning: <FORK> " + (cols));
                            return;
                        }
                    }
                }
                //то же самое, но в другую сторону
                if (lines < 2 && cols > 2)
                {
                    if (Controller.instance.coloumn[cols].line[lines] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[cols - 1].line[lines + 1] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[cols - 3].line[lines + 1] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[cols - 3].line[lines + 3] == (int)Controller.instance.playerFigure + 1)
                    {
                        if (Controller.instance.coloumn[cols].line[lines + 1] == 0)
                        {
                            Controller.instance.SetFigure(cols);
                            Debug.LogWarning("Warning: <FORK> " + (cols));
                            return;
                        }
                    }
                }


                //xx?x
                //oo?x
                //ox o
                //xo x
                if (lines < 2 && cols < 7)
                {
                    if (Controller.instance.coloumn[cols].line[lines] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[cols].line[lines + 3] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[cols + 1].line[lines + 1] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[cols + 3].line[lines + 3] == (int)Controller.instance.playerFigure + 1)
                    {
                        if (Controller.instance.coloumn[cols + 1].line[lines + 3] == 0 && Controller.instance.coloumn[cols + 1].line[lines + 2] != 0)
                        {
                            Controller.instance.SetFigure(cols + 1);
                            Debug.LogWarning("Warning: <FORK> " + (cols + 1));
                            return;
                        }
                        else if (Controller.instance.coloumn[cols + 1].line[lines + 2] == 0)
                        {
                            dungerCol.Add(cols + 1);
                            dungerLine.Add(lines + 2);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                }
                if (lines < 2 && cols < 7)
                {
                    if (Controller.instance.coloumn[cols].line[lines] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[cols + 1].line[lines + 3] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[cols + 1].line[lines + 1] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[cols + 3].line[lines + 3] == (int)Controller.instance.playerFigure + 1)
                    {
                        if (Controller.instance.coloumn[cols].line[lines + 3] == 0 && Controller.instance.coloumn[cols].line[lines + 2] != 0)
                        {
                            Controller.instance.SetFigure(cols);
                            Debug.LogWarning("Warning: <FORK> " + (cols));
                            return;
                        }
                        else if (Controller.instance.coloumn[cols].line[lines + 2] == 0)
                        {
                            dungerCol.Add(cols);
                            dungerLine.Add(lines + 2);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                }
                //то же самое, но в другую сторону
                if (lines < 2 && cols > 2)
                {
                    if (Controller.instance.coloumn[cols].line[lines] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[cols].line[lines + 3] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[cols - 1].line[lines + 1] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[cols - 3].line[lines + 3] == (int)Controller.instance.playerFigure + 1)
                    {
                        if (Controller.instance.coloumn[cols - 1].line[lines + 3] == 0 && Controller.instance.coloumn[cols - 1].line[lines + 2] != 0)
                        {
                            Controller.instance.SetFigure(cols - 1);
                            Debug.LogWarning("Warning: <FORK> " + (cols + 1));
                            return;
                        }
                        else if (Controller.instance.coloumn[cols - 1].line[lines + 2] == 0)
                        {
                            dungerCol.Add(cols - 1);
                            dungerLine.Add(lines + 2);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                }
                if (lines < 2 && cols > 2)
                {
                    if (Controller.instance.coloumn[cols].line[lines] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[cols - 1].line[lines + 3] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[cols - 1].line[lines + 1] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[cols - 3].line[lines + 3] == (int)Controller.instance.playerFigure + 1)
                    {
                        if (Controller.instance.coloumn[cols].line[lines + 3] == 0 && Controller.instance.coloumn[cols].line[lines + 2] != 0)
                        {
                            Controller.instance.SetFigure(cols);
                            Debug.LogWarning("Warning: <FORK> " + (cols));
                            return;
                        }
                        else if (Controller.instance.coloumn[cols].line[lines + 2] == 0)
                        {
                            dungerCol.Add(cols);
                            dungerLine.Add(lines + 2);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                }


                // ?xxx
                //x?xx
                if (lines < 4 && cols > 1 && cols < 8)
                {
                    if (Controller.instance.coloumn[cols].line[lines] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[cols].line[lines + 1] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[cols + 1].line[lines] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[cols + 1].line[lines + 1] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[cols + 2].line[lines + 1] == (int)Controller.instance.playerFigure + 1)
                    {
                        if (Controller.instance.coloumn[cols - 2].line[lines] == 0 && (lines == 0 || Controller.instance.coloumn[cols - 2].line[lines - 1] != 0))
                        {
                            Controller.instance.SetFigure(cols - 2);
                            Debug.LogWarning("Warning: <FORK> " + (cols - 2));
                            return;
                        }
                        else if (lines != 0 && Controller.instance.coloumn[cols - 2].line[lines - 1] == 0)
                        {
                            dungerCol.Add(cols - 2);
                            dungerLine.Add(lines - 1);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                }
                //то же самое, но в другую сторону
                if (lines < 4 && cols > 1 && cols < 8)
                {
                    if (Controller.instance.coloumn[cols].line[lines] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[cols].line[lines + 1] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[cols - 1].line[lines] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[cols - 1].line[lines + 1] == (int)Controller.instance.playerFigure + 1
                        && Controller.instance.coloumn[cols - 2].line[lines + 1] == (int)Controller.instance.playerFigure + 1)
                    {
                        if (Controller.instance.coloumn[cols + 2].line[lines] == 0 && (lines == 0 || Controller.instance.coloumn[cols + 2].line[lines - 1] != 0))
                        {
                            Controller.instance.SetFigure(cols + 2);
                            Debug.LogWarning("Warning: <FORK> " + (cols + 2));
                            return;
                        }
                        else if (lines != 0 && Controller.instance.coloumn[cols + 2].line[lines - 1] == 0)
                        {
                            dungerCol.Add(cols + 2);
                            dungerLine.Add(lines - 1);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                }
            }
        }
        if (dungerCol.Count > 0 || Controller.instance.botAtc.winingCol.Count > 0)
        {
            CheckAllDunger();
        }
        Controller.instance.botAtc.CheckLinesPerspective();
    }

    public void AllClear()
    {
        if (Controller.instance.botAtc.fork && TrySetSafeFigure(Controller.instance.botAtc.forkCol, "All Clear: <For FORK Turn> "))
            return;

        if (TrySetSafeFigure(Controller.instance.lastSetCol, "All Clear: <On Player Figure> "))
            return;

        int columnsCount = Controller.instance.coloumn.Length;
        int startColumn = UnityEngine.Random.Range(0, columnsCount);

        for (int offset = 0; offset < columnsCount; offset++)
        {
            int column = (startColumn + offset) % columnsCount;
            if (TrySetSafeFigure(column, "All Clear: <Random> "))
                return;
        }

        Debug.Log("Ќ»„№я");
    }

    public void CheckAllDunger()
    {
        RemoveDuplicateDangerCells();

        int freeColumns = CountFreeColumns();
        int dangerColumns = CountDangerColumns();

        if (freeColumns == dangerColumns)
        {
            ClearDangerCells();
        }
    }

    private bool TrySetSafeFigure(int column, string messagePrefix)
    {
        if (!TryGetFirstFreeLine(column, out int line))
            return false;

        int topLine = Controller.instance.coloumn[column].line.Length - 1;
        if (IsDangerCell(column, line) || IsDangerCell(column, topLine))
            return false;

        Controller.instance.SetFigure(column);
        Debug.Log(messagePrefix + column);
        return true;
    }

    private bool TryGetFirstFreeLine(int column, out int line)
    {
        line = -1;
        if (!IsInsideColumn(column))
            return false;

        int[] lines = Controller.instance.coloumn[column].line;
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i] == 0)
            {
                line = i;
                return true;
            }
        }

        return false;
    }

    private bool IsDangerCell(int column, int line)
    {
        for (int i = 0; i < dungerCol.Count; i++)
        {
            if (dungerCol[i] == column && dungerLine[i] == line)
                return true;
        }

        return false;
    }

    private int CountFreeColumns()
    {
        int freeColumns = 0;
        for (int i = 0; i < Controller.instance.coloumn.Length; i++)
        {
            int topLine = Controller.instance.coloumn[i].line.Length - 1;
            if (Controller.instance.coloumn[i].line[topLine] == 0)
            {
                freeColumns++;
            }
        }

        return freeColumns;
    }

    private int CountDangerColumns()
    {
        bool[] usedColumns = new bool[Controller.instance.coloumn.Length];
        int dangerColumns = 0;

        for (int i = 0; i < dungerCol.Count; i++)
        {
            if (!IsInsideColumn(dungerCol[i]) || usedColumns[dungerCol[i]])
                continue;

            usedColumns[dungerCol[i]] = true;
            dangerColumns++;
        }

        return dangerColumns;
    }

    private void RemoveDuplicateDangerCells()
    {
        HashSet<int> seenCells = new HashSet<int>();
        for (int i = 0; i < dungerCol.Count; i++)
        {
            int key = dungerCol[i] * 10 + dungerLine[i];
            if (!seenCells.Add(key))
            {
                RemoveDangerAt(i);
                i--;
            }
        }
    }

    private void ClearDangerCells()
    {
        dungerCol.Clear();
        dungerLine.Clear();
    }

    private void RemoveDangerAt(int index)
    {
        dungerCol.RemoveAt(index);
        dungerLine.RemoveAt(index);
    }

    private bool IsInsideCell(int column, int line)
    {
        return IsInsideColumn(column) && line >= 0 && line < Controller.instance.coloumn[column].line.Length;
    }

    private bool IsInsideColumn(int column)
    {
        return column >= 0 && column < Controller.instance.coloumn.Length;
    }
}
