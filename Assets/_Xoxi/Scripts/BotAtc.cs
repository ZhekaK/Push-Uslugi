using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YG;

public class BotAtc : MonoBehaviour
{
    public List<int> winingCol = new List<int>();
    public List<int> winingLine = new List<int>();
    public bool fork;
    public int forkCol;

    public void BotTurnStart()
    {
        winingCol.Clear();
        winingLine.Clear();
        fork = false;
        forkCol = 0;


        if (Controller.instance.maxDifficult)
        {
            CheckCol();
        }
        else
        {
            int randomAction = Random.Range(0, 100);
            switch (Saver.saves.difficult)
            {
                case Difficult.easy:
                    if (randomAction < 40)
                    {
                        AllClear();
                    }
                    else
                    {
                        CheckCol();
                    }
                    break;

                case Difficult.medium:
                    if (randomAction <= 15)
                    {
                        AllClear();
                    }
                    else
                    {
                        CheckCol();
                    }
                    break;

                case Difficult.hard:
                    CheckCol();
                    break;
            }
        }
    }

    public void CheckDungerCells()
    {
        for (int i = winingCol.Count - 1; i >= 0; i--)
        {
            if (!IsInsideCell(winingCol[i], winingLine[i]) || Controller.instance.coloumn[winingCol[i]].line[winingLine[i]] != 0)
            {
                RemoveWinningAt(i);
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
                if (Controller.instance.coloumn[i].line[j] == (int)Controller.instance.botFigure + 1)
                {
                    winDetect++;
                    if (winDetect >= 3 && Controller.instance.coloumn[i].line[4] == 0 && Controller.instance.coloumn[i].line[j + 1] == 0)
                    {
                        Controller.instance.SetFigure(i);
                        Debug.LogWarning("Warning: <Col> " + (j + 1));
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
                if (Controller.instance.coloumn[j].line[i] == (int)Controller.instance.botFigure + 1)
                {
                    winDetect++;
                    if (j + 1 <= 9 && winDetect >= 3 && Controller.instance.coloumn[j + 1].line[i] == 0)
                    {
                        if (i == 0 || (i != 0 && Controller.instance.coloumn[j + 1].line[i - 1] != 0))
                        {
                            Controller.instance.SetFigure(j + 1);
                            return;
                        }
                        else if (Controller.instance.coloumn[j + 1].line[i - 1] == 0)
                        {
                            winingCol.Add(j + 1);
                            winingLine.Add(i - 1);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                    else if (j - winDetect >= 0 && winDetect >= 3 && Controller.instance.coloumn[j - winDetect].line[i] == 0)
                    {
                        if (i == 0 || (i != 0 && Controller.instance.coloumn[j - winDetect].line[i - 1] != 0))
                        {
                            Controller.instance.SetFigure(j - winDetect);
                            //Debug.Log("Bot turned");
                            return;
                        }
                        else if (Controller.instance.coloumn[j - winDetect].line[i - 1] == 0)
                        {
                            winingCol.Add(j - winDetect);
                            winingLine.Add(i - 1);
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
                if (Controller.instance.coloumn[j].line[i] == (int)Controller.instance.botFigure + 1)
                {
                    winDetect++;
                    if (winDetect == 1)
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
                    else if (j < 9 && j > 1 && Controller.instance.coloumn[j + 1].line[i] == (int)Controller.instance.botFigure + 1 && Controller.instance.coloumn[j - 1].line[i] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[j - 2].line[i] == (int)Controller.instance.botFigure + 1 && Controller.instance.coloumn[j].line[i] == 0)
                    {
                        if (i == 0 || (Controller.instance.coloumn[j].line[i - 1] != 0))
                        {
                            Controller.instance.SetFigure(j);
                            Debug.LogWarning("Warning: <Line> " + (j));
                            return;
                        }
                        else if (Controller.instance.coloumn[j].line[i - 1] == 0)
                        {
                            winingCol.Add(j);
                            winingLine.Add(i - 1);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                    else if (j > 0 && j < 8 && Controller.instance.coloumn[j + 1].line[i] == (int)Controller.instance.botFigure + 1 && Controller.instance.coloumn[j - 1].line[i] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[j + 2].line[i] == (int)Controller.instance.botFigure + 1 && Controller.instance.coloumn[j].line[i] == 0)
                    {
                        if (i == 0 || (Controller.instance.coloumn[j].line[i - 1] != 0))
                        {
                            Controller.instance.SetFigure(j);
                            Debug.LogWarning("Warning: <Line> " + (j));
                            return;
                        }
                        else if (Controller.instance.coloumn[j].line[i - 1] == 0)
                        {
                            winingCol.Add(j);
                            winingLine.Add(i - 1);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                }
            }
        }

        CheckDiagonalsLeft();
    }

    public void CheckLinesPerspective()
    {
        int winDetect = 0;
        for (int i = 0; i < 5; i++)
        {
            winDetect = 0;
            for (int j = 0; j < Controller.instance.coloumn.Length; j++)
            {
                if (Controller.instance.coloumn[j].line[i] == (int)Controller.instance.botFigure + 1)
                {
                    winDetect++;
                    if (j + 1 < 9 && j - winDetect >= 0 && winDetect >= 2 && Controller.instance.coloumn[j + 1].line[i] == 0 && Controller.instance.coloumn[j - winDetect].line[i] == 0)
                    {
                        if (i == 0 || (i != 0 && Controller.instance.coloumn[j + 1].line[i - 1] != 0 && Controller.instance.coloumn[j - winDetect].line[i - 1] != 0))
                        {
                            if (i != 0 && j < 8 && Controller.instance.coloumn[j + 2].line[i - 1] != 0)
                            {
                                if (Controller.instance.botDef.dungerCol.Count > 0)
                                {
                                    for (int s = 0; s < Controller.instance.botDef.dungerCol.Count; s++)
                                    {
                                        if (Controller.instance.botDef.dungerCol[s] == j + 1 && Controller.instance.botDef.dungerLine[s] == i)
                                        {
                                            break;
                                        }
                                        else if (s == Controller.instance.botDef.dungerCol.Count - 1)
                                        {
                                            Controller.instance.SetFigure(j + 1);
                                            Debug.LogWarning("Warning: <Line> " + (j + 1));
                                            return;
                                        }
                                    }
                                }
                                else
                                {
                                    Controller.instance.SetFigure(j + 1);
                                    Debug.LogWarning("Warning: <Line> " + (j + 1));
                                    return;
                                }
                            }
                            else if (i != 0 && j > 2 && winDetect == 2 && Controller.instance.coloumn[j - winDetect - 1].line[i - 1] != 0)
                            {
                                if (Controller.instance.botDef.dungerCol.Count > 0)
                                {
                                    for (int s = 0; s < Controller.instance.botDef.dungerCol.Count; s++)
                                    {
                                        if (Controller.instance.botDef.dungerCol[s] == j + 1 && Controller.instance.botDef.dungerLine[s] == i)
                                        {
                                            break;
                                        }
                                        else if (s == Controller.instance.botDef.dungerCol.Count - 1)
                                        {
                                            Controller.instance.SetFigure(j + 1);
                                            Debug.LogWarning("Warning: <Line> " + (j + 1));
                                            return;
                                        }
                                    }
                                }
                                else
                                {
                                    Controller.instance.SetFigure(j + 1);
                                    Debug.LogWarning("Warning: <Line> " + (j + 1));
                                    return;
                                }
                            }
                            else
                            {
                                int tempChance = Random.Range(0, 100);
                                if (tempChance < 50)
                                {
                                    if (Controller.instance.botDef.dungerCol.Count > 0)
                                    {
                                        for (int s = 0; s < Controller.instance.botDef.dungerCol.Count; s++)
                                        {
                                            if (Controller.instance.botDef.dungerCol[s] == j - winDetect && Controller.instance.botDef.dungerLine[s] == i)
                                            {
                                                break;
                                            }
                                            else if (s == Controller.instance.botDef.dungerCol.Count - 1)
                                            {
                                                Controller.instance.SetFigure(j - winDetect);
                                                Debug.LogWarning("Warning: <Line> " + (j - winDetect));
                                                return;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Controller.instance.SetFigure(j - winDetect);
                                        Debug.LogWarning("Warning: <Line> " + (j - winDetect));
                                        return;
                                    }
                                }
                                else
                                {
                                    if (Controller.instance.botDef.dungerCol.Count > 0)
                                    {
                                        for (int s = 0; s < Controller.instance.botDef.dungerCol.Count; s++)
                                        {
                                            if (Controller.instance.botDef.dungerCol[s] == j + 1 && Controller.instance.botDef.dungerLine[s] == i)
                                            {
                                                break;
                                            }
                                            else if (s == Controller.instance.botDef.dungerCol.Count - 1)
                                            {
                                                Controller.instance.SetFigure(j + 1);
                                                Debug.LogWarning("Warning: <Line> " + (j + 1));
                                                return;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Controller.instance.SetFigure(j + 1);
                                        Debug.LogWarning("Warning: <Line> " + (j + 1));
                                        return;
                                    }
                                }
                                return;
                            }
                        }
                        else if (Controller.instance.coloumn[j + 1].line[i - 1] == 0 || Controller.instance.coloumn[j - winDetect].line[i - 1] == 0)
                        {
                            winingCol.Add(j + 1);
                            winingLine.Add(i - 1);
                            winingCol.Add(j - winDetect);
                            winingLine.Add(i - 1);
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
                if (Controller.instance.coloumn[j].line[i] == (int)Controller.instance.botFigure + 1)
                {
                    winDetect++;
                    if (winDetect == 1)
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
                    else if (j > 1 && j < 8 && Controller.instance.coloumn[j + 1].line[i] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[j - 1].line[i] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[j + 2].line[i] == 0 && Controller.instance.coloumn[j - 2].line[i] == 0
                        && Controller.instance.coloumn[j].line[i] == 0 && (i == 0 || i > 0 && Controller.instance.coloumn[j + 2].line[i - 1] != 0 && Controller.instance.coloumn[j - 2].line[i - 1] != 0))
                    {
                        if (i == 0 || (Controller.instance.coloumn[j].line[i - 1] != 0))
                        {
                            if (Controller.instance.botDef.dungerCol.Count > 0)
                            {
                                for (int s = 0; s < Controller.instance.botDef.dungerCol.Count; s++)
                                {
                                    if (Controller.instance.botDef.dungerCol[s] == j && Controller.instance.botDef.dungerLine[s] == i)
                                    {
                                        break;
                                    }
                                    else if (s == Controller.instance.botDef.dungerCol.Count - 1)
                                    {
                                        Controller.instance.SetFigure(j);
                                        Debug.LogWarning("Warning: <Line> " + (j));
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                Controller.instance.SetFigure(j);
                                Debug.LogWarning("Warning: <Line> " + (j));
                                return;
                            }
                        }
                        else if (Controller.instance.coloumn[j].line[i - 1] == 0 && Controller.instance.coloumn[j + 2].line[i - 1] != 0 && Controller.instance.coloumn[j - 2].line[i - 1] != 0)
                        {
                            winingCol.Add(j);
                            winingLine.Add(i - 1);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                }
            }
        }

        CheckDiagonalsLeftPerspective();
    }

    public void CheckDiagonalsLeft()
    {
        int winDetect = 0;
        for (int j = 0; j < 4; j++)
        {
            if (Controller.instance.coloumn[j].line[j + 1] == (int)Controller.instance.botFigure + 1)
            {
                winDetect++;
                if (winDetect == 2 && j == 1 && Controller.instance.coloumn[j + 2].line[j + 3] == (int)Controller.instance.botFigure + 1 && Controller.instance.coloumn[j + 1].line[j + 2] == 0)
                {
                    if (Controller.instance.coloumn[j + 1].line[j + 1] != 0)
                    {
                        Controller.instance.SetFigure(j + 1);
                        Debug.LogWarning("Warning: <Diagonals> " + (j + 1));
                        return;
                    }
                    else if (Controller.instance.coloumn[j + 1].line[j + 1] == 0)
                    {
                        winingCol.Add(j + 1);
                        winingLine.Add(j + 1);
                        Debug.Log("DO NOT SET THIS");
                    }
                }
                if (winDetect == 2 && j == 3 && Controller.instance.coloumn[0].line[1] == (int)Controller.instance.botFigure + 1 && Controller.instance.coloumn[1].line[2] == 0)
                {
                    if (Controller.instance.coloumn[1].line[1] != 0)
                    {
                        Controller.instance.SetFigure(1);
                        Debug.LogWarning("Warning: <Diagonals> " + (1));
                        return;
                    }
                    else if (Controller.instance.coloumn[1].line[1] == 0)
                    {
                        winingCol.Add(1);
                        winingLine.Add(1);
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
                            winingCol.Add(3);
                            winingLine.Add(3);
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
                            winingCol.Add(0);
                            winingLine.Add(0);
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
            if (Controller.instance.coloumn[j + 6].line[j] == (int)Controller.instance.botFigure + 1)
            {
                winDetect++;
                if (winDetect == 2 && j == 1 && Controller.instance.coloumn[9].line[3] == (int)Controller.instance.botFigure + 1 && Controller.instance.coloumn[8].line[2] == 0)
                {
                    if (Controller.instance.coloumn[8].line[1] != 0)
                    {
                        Controller.instance.SetFigure(8);
                        Debug.LogWarning("Warning: <Diagonals> " + (8));
                        return;
                    }
                    else if (Controller.instance.coloumn[8].line[1] == 0)
                    {
                        winingCol.Add(8);
                        winingLine.Add(1);
                        Debug.Log("DO NOT SET THIS");
                    }
                }
                if (winDetect == 2 && j == 3 && Controller.instance.coloumn[6].line[0] == (int)Controller.instance.botFigure + 1 && Controller.instance.coloumn[7].line[1] == 0)
                {
                    if (Controller.instance.coloumn[7].line[0] != 0)
                    {
                        Controller.instance.SetFigure(7);
                        Debug.LogWarning("Warning: <Diagonals> " + (7));
                        return;
                    }
                    else if (Controller.instance.coloumn[7].line[0] == 0)
                    {
                        winingCol.Add(7);
                        winingLine.Add(0);
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
                            winingCol.Add(9);
                            winingLine.Add(2);
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
                if (Controller.instance.coloumn[i + j].line[j] == (int)Controller.instance.botFigure + 1)
                {
                    winDetect++;
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
                            winingCol.Add(i + j + 1);
                            winingLine.Add(j + 1 - 1);
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
                            winingCol.Add(i + j - winDetect);
                            winingLine.Add(j - winDetect - 1);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                }
                else
                {
                    winDetect = 0;
                    if (j > 0 && j < 3 && Controller.instance.coloumn[i + j - 1].line[j - 1] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[i + j + 1].line[j + 1] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[i + j + 2].line[j + 2] == (int)Controller.instance.botFigure + 1
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
                            winingCol.Add(i + j);
                            winingLine.Add(j - 1);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                    if (j > 1 && j < 4 && Controller.instance.coloumn[i + j - 1].line[j - 1] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[i + j + 1].line[j + 1] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[i + j - 2].line[j - 2] == (int)Controller.instance.botFigure + 1
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
                            winingCol.Add(i + j);
                            winingLine.Add(j - 1);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                }
            }
        }
        CheckDiagonalsRight();
    }

    public void CheckDiagonalsLeftPerspective()
    {
        int winDetect = 0;

        for (int i = 0; i < Controller.instance.coloumn.Length - 4; i++)
        {
            winDetect = 0;
            for (int j = 0; j < 5; j++)
            {
                if (Controller.instance.coloumn[i + j].line[j] == (int)Controller.instance.botFigure + 1)
                {
                    winDetect++;
                    if (winDetect == 2 && Controller.instance.coloumn[i + 0].line[0] == 0 && Controller.instance.coloumn[i + 4].line[4] == 0)
                    {
                        for (int o = 1; o < 4; o++)
                        {
                            if (Controller.instance.coloumn[i + o].line[o] == 0 && Controller.instance.coloumn[i + o].line[o - 1] != 0)
                            {
                                if (Controller.instance.botDef.dungerCol.Count > 0)
                                {
                                    for (int s = 0; s < Controller.instance.botDef.dungerCol.Count; s++)
                                    {
                                        if (Controller.instance.botDef.dungerCol[s] == i + o && Controller.instance.botDef.dungerLine[s] == o)
                                        {
                                            break;
                                        }
                                        else if (s == Controller.instance.botDef.dungerCol.Count - 1)
                                        {
                                            Controller.instance.SetFigure(i + o);
                                            Debug.LogWarning("Warning: <Line> " + (i + o));
                                            return;
                                        }
                                    }
                                }
                                else
                                {
                                    Controller.instance.SetFigure(i + o);
                                    Debug.LogWarning("Warning: <Line> " + (i + o));
                                    return;
                                }
                            }
                            else if (Controller.instance.coloumn[i + o].line[o - 1] == 0)
                            {
                                winingCol.Add(i + o);
                                winingLine.Add(o - 1);
                                Debug.Log("DO NOT SET THIS");
                            }
                        }
                    }
                }
                else
                {
                    winDetect = 0;
                    if (j > 1 && j < 3 && Controller.instance.coloumn[i + j + 1].line[j + 1] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[i + j - 1].line[j - 1] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[i + j - 2].line[j - 2] == 0
                        && Controller.instance.coloumn[i + j + 2].line[j + 2] == 0
                        && Controller.instance.coloumn[i + j].line[j] == 0)
                    {
                        if (Controller.instance.coloumn[i + j].line[j - 1] != 0)
                        {
                            if (Controller.instance.botDef.dungerCol.Count > 0)
                            {
                                for (int s = 0; s < Controller.instance.botDef.dungerCol.Count; s++)
                                {
                                    if (Controller.instance.botDef.dungerCol[s] == i + j && Controller.instance.botDef.dungerLine[s] == j)
                                    {
                                        break;
                                    }
                                    else if (s == Controller.instance.botDef.dungerCol.Count - 1)
                                    {
                                        Controller.instance.SetFigure(i + j);
                                        Debug.LogWarning("Warning: <Line> " + (i + j));
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                Controller.instance.SetFigure(i + j);
                                Debug.LogWarning("Warning: <Line> " + (i + j));
                                return;
                            }
                        }
                        else
                        {
                            winingCol.Add(i + j);
                            winingLine.Add(j - 1);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                }
            }
        }

        CheckDiagonalsRightPerspective();
    }

    public void CheckDiagonalsRight()
    {
        int winDetect = 0;
        for (int j = 0; j < 4; j++)
        {
            if (Controller.instance.coloumn[j].line[3 - j] == (int)Controller.instance.botFigure + 1)
            {
                winDetect++;
                if (winDetect == 2 && j == 1 && Controller.instance.coloumn[3].line[0] == (int)Controller.instance.botFigure + 1 && Controller.instance.coloumn[2].line[1] == 0)
                {
                    if (Controller.instance.coloumn[2].line[1 - 1] != 0)
                    {
                        Controller.instance.SetFigure(2);
                        Debug.LogWarning("Warning: <Diagonals> " + (2));
                        return;
                    }
                    else if (Controller.instance.coloumn[2].line[1 - 1] == 0)
                    {
                        winingCol.Add(2);
                        winingLine.Add(0);
                        Debug.Log("DO NOT SET THIS");
                    }
                }
                if (winDetect == 2 && j == 3 && Controller.instance.coloumn[0].line[3] == (int)Controller.instance.botFigure + 1 && Controller.instance.coloumn[1].line[2] == 0)
                {
                    if (Controller.instance.coloumn[1].line[1] != 0)
                    {
                        Controller.instance.SetFigure(1);
                        Debug.LogWarning("Warning: <Diagonals> " + (1));
                        return;
                    }
                    else if (Controller.instance.coloumn[1].line[1] == 0)
                    {
                        winingCol.Add(1);
                        winingLine.Add(1);
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
                            winingCol.Add(0);
                            winingLine.Add(2);
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
            if (Controller.instance.coloumn[j + 6].line[4 - j] == (int)Controller.instance.botFigure + 1)
            {
                winDetect++;
                if (winDetect == 2 && j == 1 && Controller.instance.coloumn[9].line[1] == (int)Controller.instance.botFigure + 1 && Controller.instance.coloumn[8].line[2] == 0)
                {
                    if (Controller.instance.coloumn[8].line[1] != 0)
                    {
                        Controller.instance.SetFigure(8);
                        Debug.LogWarning("Warning: <Diagonals> " + (8));
                        return;
                    }
                    else if (Controller.instance.coloumn[8].line[1] == 0)
                    {
                        winingCol.Add(8);
                        winingLine.Add(1);
                        Debug.Log("DO NOT SET THIS");
                    }
                }
                if (winDetect == 2 && j == 3 && Controller.instance.coloumn[6].line[4] == (int)Controller.instance.botFigure + 1 && Controller.instance.coloumn[7].line[3] == 0)
                {
                    if (Controller.instance.coloumn[7].line[2] != 0)
                    {
                        Controller.instance.SetFigure(7);
                        Debug.LogWarning("Warning: <Diagonals> " + (7));
                        return;
                    }
                    else if (Controller.instance.coloumn[7].line[2] == 0)
                    {
                        winingCol.Add(7);
                        winingLine.Add(2);
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
                            winingCol.Add(9);
                            winingLine.Add(0);
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
                            winingCol.Add(6);
                            winingLine.Add(3);
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
                if (Controller.instance.coloumn[i - j].line[j] == (int)Controller.instance.botFigure + 1)
                {
                    winDetect++;
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
                            winingCol.Add(i - j + 1);
                            winingLine.Add(j - 1 - 1);
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
                            winingCol.Add(i - j - winDetect);
                            winingLine.Add(j + winDetect - 1);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                }
                else
                {
                    winDetect = 0;
                    if ((j == 2 || j == 1) && Controller.instance.coloumn[i - j + 1].line[j - 1] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[i - j - 1].line[j + 1] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[i - j - 2].line[j + 2] == (int)Controller.instance.botFigure + 1
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
                            winingCol.Add(i - j);
                            winingLine.Add(j - 1);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                    if ((j == 3 || j == 2) && Controller.instance.coloumn[i - j - 1].line[j + 1] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[i - j + 1].line[j - 1] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[i - j + 2].line[j - 2] == (int)Controller.instance.botFigure + 1
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
                            winingCol.Add(i - j);
                            winingLine.Add(j - 1);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                }
            }
        }

        Controller.instance.botDef.BotLooseCheck();
    }

    public void CheckDiagonalsRightPerspective()
    {
        int winDetect = 0;

        for (int i = 4; i < Controller.instance.coloumn.Length; i++)
        {
            winDetect = 0;
            for (int j = 4; j >= 0; j--)
            {
                if (Controller.instance.coloumn[i - j].line[j] == (int)Controller.instance.botFigure + 1)
                {
                    winDetect++;
                    if (winDetect == 2 && Controller.instance.coloumn[i - 0].line[0] == 0 && Controller.instance.coloumn[i - 4].line[4] == 0)
                    {
                        for (int o = 3; o > 0; o--)
                        {
                            if (Controller.instance.coloumn[i - o].line[o] == 0 && Controller.instance.coloumn[i - o].line[o - 1] != 0)
                            {
                                if (Controller.instance.botDef.dungerCol.Count > 0)
                                {
                                    for (int s = 0; s < Controller.instance.botDef.dungerCol.Count; s++)
                                    {
                                        if (Controller.instance.botDef.dungerCol[s] == i - o && Controller.instance.botDef.dungerLine[s] == o)
                                        {
                                            break;
                                        }
                                        else if (s == Controller.instance.botDef.dungerCol.Count - 1)
                                        {
                                            Controller.instance.SetFigure(i - o);
                                            Debug.LogWarning("Warning: <Line> " + (i - o));
                                            return;
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                    }
                                }
                                else
                                {
                                    Controller.instance.SetFigure(i - o);
                                    Debug.LogWarning("Warning: <Line> " + (i - o));
                                    return;
                                }
                            }
                            else if (Controller.instance.coloumn[i - o].line[o - 1] == 0)
                            {
                                winingCol.Add(i - o);
                                winingLine.Add(o - 1);
                                Debug.Log("DO NOT SET THIS");
                            }
                        }
                    }
                }
                else
                {
                    winDetect = 0;
                    if (j == 2 && Controller.instance.coloumn[i - j + 1].line[j - 1] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[i - j - 1].line[j + 1] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[i - j - 2].line[j + 2] == 0
                        && Controller.instance.coloumn[i - j + 2].line[j - 2] == 0
                        && Controller.instance.coloumn[i - j].line[j] == 0)
                    {
                        if (Controller.instance.coloumn[i - j].line[j - 1] != 0)
                        {
                            if (Controller.instance.botDef.dungerCol.Count > 0)
                            {
                                for (int s = 0; s < Controller.instance.botDef.dungerCol.Count; s++)
                                {
                                    if (Controller.instance.botDef.dungerCol[s] == i - j && Controller.instance.botDef.dungerLine[s] == j)
                                    {
                                        break;
                                    }
                                    else if (s == Controller.instance.botDef.dungerCol.Count - 1)
                                    {
                                        Controller.instance.SetFigure(i - j);
                                        Debug.LogWarning("Warning: <Line> " + (i - j));
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                Controller.instance.SetFigure(i - j);
                                Debug.LogWarning("Warning: <Line> " + (i - j));
                                return;
                            }
                        }
                        else
                        {
                            winingCol.Add(i - j);
                            winingLine.Add(j - 1);
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
                    if (Controller.instance.coloumn[cols].line[lines] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[cols].line[lines + 2] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[cols + 1].line[lines + 3] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[cols + 1].line[lines] == (int)Controller.instance.botFigure + 1)
                    {
                        if (Controller.instance.coloumn[cols - 2].line[lines] == 0 && (lines == 0 || Controller.instance.coloumn[cols - 2].line[lines - 1] != 0))
                        {
                            if (Controller.instance.botDef.dungerCol.Count > 0)
                            {
                                for (int s = 0; s < Controller.instance.botDef.dungerCol.Count; s++)
                                {
                                    if (Controller.instance.botDef.dungerCol[s] == cols - 2 && Controller.instance.botDef.dungerLine[s] == lines)
                                    {
                                        break;
                                    }
                                    else if (s == Controller.instance.botDef.dungerCol.Count - 1)
                                    {
                                        Controller.instance.SetFigure(cols - 2);
                                        fork = true;
                                        forkCol = cols - 1;
                                        Debug.LogWarning("Warning: <FORK> " + (cols - 2));
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                Controller.instance.SetFigure(cols - 2);
                                fork = true;
                                forkCol = cols - 1;
                                Debug.LogWarning("Warning: <FORK> " + (cols - 2));
                                return;
                            }
                        }
                        else if (lines != 0 && Controller.instance.coloumn[cols - 2].line[lines - 1] == 0)
                        {
                            winingCol.Add(cols - 2);
                            winingLine.Add(lines - 1);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                }
                if (lines < 2 && cols > 2 && cols < 9)
                {
                    if (Controller.instance.coloumn[cols].line[lines] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[cols].line[lines + 2] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[cols + 1].line[lines + 3] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[cols - 2].line[lines] == (int)Controller.instance.botFigure + 1)
                    {
                        if (Controller.instance.coloumn[cols - 3].line[lines] == 0 && (lines == 0 || Controller.instance.coloumn[cols - 3].line[lines - 1] != 0))
                        {
                            if (Controller.instance.botDef.dungerCol.Count > 0)
                            {
                                for (int s = 0; s < Controller.instance.botDef.dungerCol.Count; s++)
                                {
                                    if (Controller.instance.botDef.dungerCol[s] == cols - 3 && Controller.instance.botDef.dungerLine[s] == lines)
                                    {
                                        break;
                                    }
                                    else if (s == Controller.instance.botDef.dungerCol.Count - 1)
                                    {
                                        fork = true;
                                        forkCol = cols - 1;
                                        Controller.instance.SetFigure(cols - 3);
                                        Debug.LogWarning("Warning: <FORK> " + (cols - 3));
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                fork = true;
                                forkCol = cols - 1;
                                Controller.instance.SetFigure(cols - 3);
                                Debug.LogWarning("Warning: <FORK> " + (cols - 3));
                                return;
                            }
                        }
                        else if (lines != 0 && Controller.instance.coloumn[cols - 3].line[lines - 1] == 0)
                        {
                            winingCol.Add(cols - 3);
                            winingLine.Add(lines - 1);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                }
                //�� �� �����, �� � ������ �������
                if (lines < 2 && cols > 0 && cols < 8)
                {
                    if (Controller.instance.coloumn[cols].line[lines] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[cols].line[lines + 2] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[cols - 1].line[lines + 3] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[cols - 1].line[lines] == (int)Controller.instance.botFigure + 1)
                    {
                        if (Controller.instance.coloumn[cols + 2].line[lines] == 0
                            && (lines == 0 || Controller.instance.coloumn[cols + 2].line[lines - 1] != 0))
                        {
                            if (Controller.instance.botDef.dungerCol.Count > 0)
                            {
                                for (int s = 0; s < Controller.instance.botDef.dungerCol.Count; s++)
                                {
                                    if (Controller.instance.botDef.dungerCol[s] == cols + 2 && Controller.instance.botDef.dungerLine[s] == lines)
                                    {
                                        break;
                                    }
                                    else if (s == Controller.instance.botDef.dungerCol.Count - 1)
                                    {
                                        Controller.instance.SetFigure(cols + 2);
                                        fork = true;
                                        forkCol = cols + 1;
                                        Debug.LogWarning("Warning: <FORK> " + (cols + 2));
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                Controller.instance.SetFigure(cols + 2);
                                fork = true;
                                forkCol = cols + 1;
                                Debug.LogWarning("Warning: <FORK> " + (cols + 2));
                                return;
                            }
                        }
                        else if (lines != 0 && Controller.instance.coloumn[cols + 2].line[lines - 1] == 0)
                        {
                            winingCol.Add(cols + 2);
                            winingLine.Add(lines - 1);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                }
                if (lines < 2 && cols > 0 && cols < 7)
                {
                    if (Controller.instance.coloumn[cols].line[lines] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[cols].line[lines + 2] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[cols - 1].line[lines + 3] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[cols + 2].line[lines] == (int)Controller.instance.botFigure + 1)
                    {
                        if (Controller.instance.coloumn[cols + 3].line[lines] == 0
                            && (lines == 0 || Controller.instance.coloumn[cols + 3].line[lines - 1] != 0))
                        {
                            if (Controller.instance.botDef.dungerCol.Count > 0)
                            {
                                for (int s = 0; s < Controller.instance.botDef.dungerCol.Count; s++)
                                {
                                    if (Controller.instance.botDef.dungerCol[s] == cols + 3 && Controller.instance.botDef.dungerLine[s] == lines)
                                    {
                                        break;
                                    }
                                    else if (s == Controller.instance.botDef.dungerCol.Count - 1)
                                    {
                                        fork = true;
                                        forkCol = cols + 1;
                                        Controller.instance.SetFigure(cols + 3);
                                        Debug.LogWarning("Warning: <FORK> " + (cols + 3));
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                fork = true;
                                forkCol = cols + 1;
                                Controller.instance.SetFigure(cols + 3);
                                Debug.LogWarning("Warning: <FORK> " + (cols + 3));
                                return;
                            }
                        }
                        else if (lines != 0 && Controller.instance.coloumn[cols + 3].line[lines - 1] == 0)
                        {
                            winingCol.Add(cols + 3);
                            winingLine.Add(lines - 1);
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
                    if (Controller.instance.coloumn[cols].line[lines] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[cols].line[lines - 2] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[cols + 1].line[lines - 3] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[cols + 1].line[lines] == (int)Controller.instance.botFigure + 1)
                    {
                        if (Controller.instance.coloumn[cols - 2].line[lines] == 0 && Controller.instance.coloumn[cols - 2].line[lines - 1] != 0)
                        {
                            if (Controller.instance.botDef.dungerCol.Count > 0)
                            {
                                for (int s = 0; s < Controller.instance.botDef.dungerCol.Count; s++)
                                {
                                    if (Controller.instance.botDef.dungerCol[s] == cols - 2 && Controller.instance.botDef.dungerLine[s] == lines)
                                    {
                                        break;
                                    }
                                    else if (s == Controller.instance.botDef.dungerCol.Count - 1)
                                    {
                                        Controller.instance.SetFigure(cols - 2);
                                        fork = true;
                                        forkCol = cols - 1;
                                        Debug.LogWarning("Warning: <FORK> " + (cols - 2));
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                Controller.instance.SetFigure(cols - 2);
                                fork = true;
                                forkCol = cols - 1;
                                Debug.LogWarning("Warning: <FORK> " + (cols - 2));
                                return;
                            }
                        }
                        else if (Controller.instance.coloumn[cols - 2].line[lines - 1] == 0)
                        {
                            winingCol.Add(cols - 2);
                            winingLine.Add(lines - 1);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                }
                //�� �� �����, �� � ������ �������
                if (lines > 2 && cols > 0 && cols < 8)
                {
                    if (Controller.instance.coloumn[cols].line[lines] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[cols].line[lines - 2] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[cols - 1].line[lines - 3] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[cols - 1].line[lines] == (int)Controller.instance.botFigure + 1)
                    {
                        if (Controller.instance.coloumn[cols + 2].line[lines] == 0
                            && Controller.instance.coloumn[cols + 2].line[lines - 1] != 0)
                        {
                            if (Controller.instance.botDef.dungerCol.Count > 0)
                            {
                                for (int s = 0; s < Controller.instance.botDef.dungerCol.Count; s++)
                                {
                                    if (Controller.instance.botDef.dungerCol[s] == cols + 2 && Controller.instance.botDef.dungerLine[s] == lines)
                                    {
                                        break;
                                    }
                                    else if (s == Controller.instance.botDef.dungerCol.Count - 1)
                                    {
                                        Controller.instance.SetFigure(cols + 2);
                                        fork = true;
                                        forkCol = cols + 1;
                                        Debug.LogWarning("Warning: <FORK> " + (cols + 2));
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                Controller.instance.SetFigure(cols + 2);
                                fork = true;
                                forkCol = cols + 1;
                                Debug.LogWarning("Warning: <FORK> " + (cols + 2));
                                return;
                            }
                        }
                        else if (Controller.instance.coloumn[cols + 2].line[lines - 1] == 0)
                        {
                            winingCol.Add(cols + 2);
                            winingLine.Add(lines - 1);
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
                    if (Controller.instance.coloumn[cols].line[lines] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[cols + 1].line[lines + 1] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[cols + 3].line[lines + 1] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[cols + 3].line[lines + 3] == (int)Controller.instance.botFigure + 1)
                    {
                        if (Controller.instance.coloumn[cols].line[lines + 1] == 0)
                        {
                            if (Controller.instance.botDef.dungerCol.Count > 0)
                            {
                                for (int s = 0; s < Controller.instance.botDef.dungerCol.Count; s++)
                                {
                                    if (Controller.instance.botDef.dungerCol[s] == cols && Controller.instance.botDef.dungerLine[s] == lines + 1)
                                    {
                                        break;
                                    }
                                    else if (s == Controller.instance.botDef.dungerCol.Count - 1)
                                    {
                                        Controller.instance.SetFigure(cols);
                                        fork = true;
                                        forkCol = cols + 2;
                                        Debug.LogWarning("Warning: <FORK> " + (cols));
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                Controller.instance.SetFigure(cols);
                                fork = true;
                                forkCol = cols + 2;
                                Debug.LogWarning("Warning: <FORK> " + (cols));
                                return;
                            }
                        }
                    }
                }
                //�� �� �����, �� � ������ �������
                if (lines < 2 && cols > 2)
                {
                    if (Controller.instance.coloumn[cols].line[lines] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[cols - 1].line[lines + 1] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[cols - 3].line[lines + 1] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[cols - 3].line[lines + 3] == (int)Controller.instance.botFigure + 1)
                    {
                        if (Controller.instance.coloumn[cols].line[lines + 1] == 0)
                        {
                            if (Controller.instance.botDef.dungerCol.Count > 0)
                            {
                                for (int s = 0; s < Controller.instance.botDef.dungerCol.Count; s++)
                                {
                                    if (Controller.instance.botDef.dungerCol[s] == cols && Controller.instance.botDef.dungerLine[s] == lines + 1)
                                    {
                                        break;
                                    }
                                    else if (s == Controller.instance.botDef.dungerCol.Count - 1)
                                    {
                                        Controller.instance.SetFigure(cols);
                                        fork = true;
                                        forkCol = cols - 2;
                                        Debug.LogWarning("Warning: <FORK> " + (cols));
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                Controller.instance.SetFigure(cols);
                                fork = true;
                                forkCol = cols - 2;
                                Debug.LogWarning("Warning: <FORK> " + (cols));
                                return;
                            }
                        }
                    }
                }


                //xx?x
                //oo?x
                //ox o
                //xo x
                if (lines < 2 && cols < 7)
                {
                    if (Controller.instance.coloumn[cols].line[lines] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[cols].line[lines + 3] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[cols + 1].line[lines + 1] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[cols + 3].line[lines + 3] == (int)Controller.instance.botFigure + 1)
                    {
                        if (Controller.instance.coloumn[cols + 1].line[lines + 3] == 0 && Controller.instance.coloumn[cols + 1].line[lines + 2] != 0)
                        {
                            if (Controller.instance.botDef.dungerCol.Count > 0)
                            {
                                for (int s = 0; s < Controller.instance.botDef.dungerCol.Count; s++)
                                {
                                    if (Controller.instance.botDef.dungerCol[s] == cols + 1 && Controller.instance.botDef.dungerLine[s] == lines + 3)
                                    {
                                        break;
                                    }
                                    else if (s == Controller.instance.botDef.dungerCol.Count - 1)
                                    {
                                        Controller.instance.SetFigure(cols + 1);
                                        fork = true;
                                        forkCol = cols + 2;
                                        Debug.LogWarning("Warning: <FORK> " + (cols + 1));
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                Controller.instance.SetFigure(cols + 1);
                                fork = true;
                                forkCol = cols + 2;
                                Debug.LogWarning("Warning: <FORK> " + (cols + 1));
                                return;
                            }
                        }
                        else if (Controller.instance.coloumn[cols + 1].line[lines + 2] == 0)
                        {
                            winingCol.Add(cols + 1);
                            winingLine.Add(lines + 2);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                }
                if (lines < 2 && cols < 7)
                {
                    if (Controller.instance.coloumn[cols].line[lines] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[cols + 1].line[lines + 3] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[cols + 1].line[lines + 1] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[cols + 3].line[lines + 3] == (int)Controller.instance.botFigure + 1)
                    {
                        if (Controller.instance.coloumn[cols].line[lines + 3] == 0 && Controller.instance.coloumn[cols].line[lines + 2] != 0)
                        {
                            if (Controller.instance.botDef.dungerCol.Count > 0)
                            {
                                for (int s = 0; s < Controller.instance.botDef.dungerCol.Count; s++)
                                {
                                    if (Controller.instance.botDef.dungerCol[s] == cols && Controller.instance.botDef.dungerLine[s] == lines + 3)
                                    {
                                        break;
                                    }
                                    else if (s == Controller.instance.botDef.dungerCol.Count - 1)
                                    {
                                        Controller.instance.SetFigure(cols);
                                        fork = true;
                                        forkCol = cols + 2;
                                        Debug.LogWarning("Warning: <FORK> " + (cols));
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                Controller.instance.SetFigure(cols);
                                fork = true;
                                forkCol = cols + 2;
                                Debug.LogWarning("Warning: <FORK> " + (cols));
                                return;
                            }
                        }
                        else if (Controller.instance.coloumn[cols].line[lines + 2] == 0)
                        {
                            winingCol.Add(cols);
                            winingLine.Add(lines + 2);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                }
                //�� �� �����, �� � ������ �������
                if (lines < 2 && cols > 2)
                {
                    if (Controller.instance.coloumn[cols].line[lines] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[cols].line[lines + 3] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[cols - 1].line[lines + 1] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[cols - 3].line[lines + 3] == (int)Controller.instance.botFigure + 1)
                    {
                        if (Controller.instance.coloumn[cols - 1].line[lines + 3] == 0 && Controller.instance.coloumn[cols - 1].line[lines + 2] != 0)
                        {
                            if (Controller.instance.botDef.dungerCol.Count > 0)
                            {
                                for (int s = 0; s < Controller.instance.botDef.dungerCol.Count; s++)
                                {
                                    if (Controller.instance.botDef.dungerCol[s] == cols - 1 && Controller.instance.botDef.dungerLine[s] == lines + 3)
                                    {
                                        break;
                                    }
                                    else if (s == Controller.instance.botDef.dungerCol.Count - 1)
                                    {
                                        Controller.instance.SetFigure(cols - 1);
                                        fork = true;
                                        forkCol = cols - 2;
                                        Debug.LogWarning("Warning: <FORK> " + (cols - 1));
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                Controller.instance.SetFigure(cols - 1);
                                fork = true;
                                forkCol = cols - 2;
                                Debug.LogWarning("Warning: <FORK> " + (cols - 1));
                                return;
                            }
                        }
                        else if (Controller.instance.coloumn[cols - 1].line[lines + 2] == 0)
                        {
                            winingCol.Add(cols - 1);
                            winingLine.Add(lines + 2);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                }
                if (lines < 2 && cols > 2)
                {
                    if (Controller.instance.coloumn[cols].line[lines] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[cols - 1].line[lines + 3] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[cols - 1].line[lines + 1] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[cols - 3].line[lines + 3] == (int)Controller.instance.botFigure + 1)
                    {
                        if (Controller.instance.coloumn[cols].line[lines + 3] == 0 && Controller.instance.coloumn[cols].line[lines + 2] != 0)
                        {
                            if (Controller.instance.botDef.dungerCol.Count > 0)
                            {
                                for (int s = 0; s < Controller.instance.botDef.dungerCol.Count; s++)
                                {
                                    if (Controller.instance.botDef.dungerCol[s] == cols && Controller.instance.botDef.dungerLine[s] == lines + 3)
                                    {
                                        break;
                                    }
                                    else if (s == Controller.instance.botDef.dungerCol.Count - 1)
                                    {
                                        Controller.instance.SetFigure(cols);
                                        fork = true;
                                        forkCol = cols - 2;
                                        Debug.LogWarning("Warning: <FORK> " + (cols));
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                Controller.instance.SetFigure(cols);
                                fork = true;
                                forkCol = cols - 2;
                                Debug.LogWarning("Warning: <FORK> " + (cols));
                                return;
                            }
                        }
                        else if (Controller.instance.coloumn[cols].line[lines + 2] == 0)
                        {
                            winingCol.Add(cols);
                            winingLine.Add(lines + 2);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                }


                // ?xxx
                //x?xx
                if (lines < 4 && cols > 1 && cols < 8)
                {
                    if (Controller.instance.coloumn[cols].line[lines] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[cols].line[lines + 1] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[cols + 1].line[lines] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[cols + 1].line[lines + 1] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[cols + 2].line[lines + 1] == (int)Controller.instance.botFigure + 1)
                    {
                        if (Controller.instance.coloumn[cols - 2].line[lines] == 0 && (lines == 0 || Controller.instance.coloumn[cols - 2].line[lines - 1] != 0))
                        {

                            if (Controller.instance.botDef.dungerCol.Count > 0)
                            {
                                for (int s = 0; s < Controller.instance.botDef.dungerCol.Count; s++)
                                {
                                    if (Controller.instance.botDef.dungerCol[s] == cols - 2 && Controller.instance.botDef.dungerLine[s] == lines)
                                    {
                                        break;
                                    }
                                    else if (s == Controller.instance.botDef.dungerCol.Count - 1)
                                    {
                                        fork = true;
                                        forkCol = cols - 1;
                                        Controller.instance.SetFigure(cols - 2);
                                        Debug.LogWarning("Warning: <FORK> " + (cols - 2));
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                fork = true;
                                forkCol = cols - 1;
                                Controller.instance.SetFigure(cols - 2);
                                Debug.LogWarning("Warning: <FORK> " + (cols - 2));
                                return;
                            }
                        }
                        else if (lines != 0 && Controller.instance.coloumn[cols - 2].line[lines - 1] == 0)
                        {
                            winingCol.Add(cols - 2);
                            winingLine.Add(lines - 1);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                }
                //�� �� �����, �� � ������ �������
                if (lines < 4 && cols > 1 && cols < 8)
                {
                    if (Controller.instance.coloumn[cols].line[lines] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[cols].line[lines + 1] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[cols - 1].line[lines] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[cols - 1].line[lines + 1] == (int)Controller.instance.botFigure + 1
                        && Controller.instance.coloumn[cols - 2].line[lines + 1] == (int)Controller.instance.botFigure + 1)
                    {
                        if (Controller.instance.coloumn[cols + 2].line[lines] == 0 && (lines == 0 || Controller.instance.coloumn[cols + 2].line[lines - 1] != 0))
                        {
                            if (Controller.instance.botDef.dungerCol.Count > 0)
                            {
                                for (int s = 0; s < Controller.instance.botDef.dungerCol.Count; s++)
                                {
                                    if (Controller.instance.botDef.dungerCol[s] == cols + 2 && Controller.instance.botDef.dungerLine[s] == lines)
                                    {
                                        break;
                                    }
                                    else if (s == Controller.instance.botDef.dungerCol.Count - 1)
                                    {
                                        fork = true;
                                        forkCol = cols + 1;
                                        Controller.instance.SetFigure(cols + 2);
                                        Debug.LogWarning("Warning: <FORK> " + (cols + 2));
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                fork = true;
                                forkCol = cols + 1;
                                Controller.instance.SetFigure(cols + 2);
                                Debug.LogWarning("Warning: <FORK> " + (cols + 2));
                                return;
                            }
                        }
                        else if (lines != 0 && Controller.instance.coloumn[cols + 2].line[lines - 1] == 0)
                        {
                            winingCol.Add(cols + 2);
                            winingLine.Add(lines - 1);
                            Debug.Log("DO NOT SET THIS");
                        }
                    }
                }
            }
        }
        AllClear();
    }

    public void AllClear()
    {
        if (winingCol.Count > 0)
        {
            CheckAllDunger();
            CheckDungerCells();
        }
        if (Controller.instance.botDef.dungerCol.Count > 0 || winingCol.Count > 0)
        {
            Controller.instance.botDef.CheckDungerCells();
            Controller.instance.botDef.CheckAllDunger();
        }

        if (Controller.instance.turnNumber < 50)
        {
            Controller.instance.botDef.AllClear();
        }
        else
        {
            Debug.Log("�����");
        }
    }

    public void CheckAllDunger()
    {
        RemoveDuplicateWinningCells();

        int freeColumns = CountFreeColumns();
        if (freeColumns == winingCol.Count)
        {
            winingCol.Clear();
            winingLine.Clear();
        }
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

    private void RemoveDuplicateWinningCells()
    {
        HashSet<int> seenCells = new HashSet<int>();
        for (int i = 0; i < winingCol.Count; i++)
        {
            int key = winingCol[i] * 10 + winingLine[i];
            if (!seenCells.Add(key))
            {
                RemoveWinningAt(i);
                i--;
            }
        }
    }

    private void RemoveWinningAt(int index)
    {
        winingCol.RemoveAt(index);
        winingLine.RemoveAt(index);
    }

    private bool IsInsideCell(int column, int line)
    {
        return column >= 0 && column < Controller.instance.coloumn.Length && line >= 0 && line < Controller.instance.coloumn[column].line.Length;
    }
}
