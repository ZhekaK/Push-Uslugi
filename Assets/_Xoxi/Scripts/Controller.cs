using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using YG;

public enum turn
{
    cross,
    nulls
}
public enum Lang
{
    ru,
    en
}
public enum Difficult
{
    easy,
    medium,
    hard
}
//[ExecuteAlways]
public class Controller : MonoBehaviour
{
    public static Controller instance;

    public Cell[] coloumn = new Cell[10];
    public Transform[] spawnPoints = new Transform[10];
    public Button[] buttons;
    public GameObject[] Figures = new GameObject[2];
    public GameObject winLine;
    [HideInInspector] public BotDef botDef;
    [HideInInspector] public BotAtc botAtc;
    [HideInInspector] public float fallingSpeed;
    [HideInInspector] public bool animate;
    [Space]
    public bool bot, BotVsBot;
    public bool maxDifficult;
    public float[] secondsCount;
    [HideInInspector] public turn playerFigure;
    [HideInInspector] public turn botFigure;
    [Space]
    [SerializeField] private GameObject[] figuresForShow;
    [SerializeField] private GameObject winPanel, crossWinText, NullsWinText;
    [SerializeField] private Text timerText, turnCounterText;
    [SerializeField] private Image turnTimer;
    [SerializeField] private GameObject MarkFX;
    [SerializeField] private AudioSource _as;
    [SerializeField] private AudioClip sound1, sound2;
    [HideInInspector] public turn currentTurn;
    [HideInInspector] public int turnNumber;
    [HideInInspector] public int lastSetCol;
    [HideInInspector] public int lastSetLine;
    [HideInInspector] public bool win;

    private float timerTurnValue = 1f;
    private GameObject tempMark;
    private Tween dropTween;
    private float time;

    private void Awake()
    {
        instance = this;

        bot = MainMenu.botEnable;
        playerFigure = MainMenu.playFigure;
        botFigure = OppositeTurn(playerFigure);
        animate = Saver.saves.anim;
        fallingSpeed = Saver.saves.animationSpeed;
    }

    private void Start()
    {
        botDef = GetComponent<BotDef>();
        botAtc = GetComponent<BotAtc>();

        StartCoroutine(StartTimer());

        if (Saver.saves.timerTurn)
        {
            StartCoroutine(TimerTurnCircle());
        }

        if (bot && playerFigure == turn.nulls)
        {
            SetFigure(UnityEngine.Random.Range(0, 10));
        }
    }

    public void SetFigure(int btn)
    {
        //int cellIndex = 0;
        timerTurnValue = 1;
        if (Saver.saves.sound && currentTurn != botFigure)
        {
            PlaySound(sound1);
        }
        for (int i = 0; i < coloumn[btn].line.Length; i++)
        {
            if (coloumn[btn].line[i] == 0)
            {
                coloumn[btn].line[i] = (int)currentTurn + 1;

                if (animate)
                {
                    SetButtonsInteractable(false);
                    StartCoroutine(AnimateFigure(btn, i));
                }
                else
                {
                    coloumn[btn].figure[i] = Instantiate(Figures[(int)currentTurn], coloumn[btn].spawnPointsCell[i].position, Quaternion.identity, coloumn[btn].spawnPointsCell[i]);
                    FinishTurn(btn, i, false);
                }
                break;
            }
        }
    }

    private IEnumerator AnimateFigure(int col, int line)
    {
        GameObject temp = Instantiate(Figures[(int)currentTurn], spawnPoints[col].position, Quaternion.identity, coloumn[col].spawnPointsCell[line]);
        coloumn[col].figure[line] = temp;

        Vector3 targetPosition = coloumn[col].spawnPointsCell[line].position;
        float duration = GetDropDuration(temp.transform.position, targetPosition);

        dropTween = temp.transform
            .DOMove(targetPosition, duration)
            .SetEase(Ease.OutCubic)
            .SetLink(temp);

        yield return dropTween.WaitForCompletion();
        dropTween = null;

        SetButtonsInteractable(true);
        FinishTurn(col, line, true);
    }

    private float GetDropDuration(Vector3 from, Vector3 to)
    {
        float speed = Mathf.Max(fallingSpeed, 0.01f) * 100f;
        return Mathf.Clamp(Vector3.Distance(from, to) / speed, 0.08f, 0.8f);
    }

    private void FinishTurn(int col, int line, bool playDropSound)
    {
        int figure = (int)currentTurn + 1;

        if (line >= 3)
        {
            CheckWinColoum(col, line, figure);
        }
        CheckWinLines(col, line, figure);
        CheckWinDiagonalsRight(col, line, figure);
        CheckWinDiagonalsLeft(col, line, figure);

        currentTurn = OppositeTurn(currentTurn);
        UpdateTurnPreview();

        turnNumber++;
        lastSetCol = col;
        lastSetLine = line;
        turnCounterText.text = turnNumber.ToString();

        MarkLastFigure(col, line);

        if (playDropSound && Saver.saves.sound && currentTurn != botFigure)
        {
            PlaySound(sound2);
        }

        StartBotTurnIfNeeded();
    }

    private void UpdateTurnPreview()
    {
        bool crossTurn = currentTurn == turn.cross;
        figuresForShow[0].SetActive(crossTurn);
        figuresForShow[1].SetActive(!crossTurn);
    }

    private void MarkLastFigure(int col, int line)
    {
        if (!Saver.saves.markLastFigure)
            return;

        if (tempMark != null)
        {
            Destroy(tempMark);
        }
        tempMark = Instantiate(MarkFX, coloumn[col].spawnPointsCell[line].position, Quaternion.identity, coloumn[col].spawnPointsCell[line]);
    }

    private void StartBotTurnIfNeeded()
    {
        if (currentTurn == playerFigure || !bot || win)
            return;

        botAtc.BotTurnStart();
        if (BotVsBot)
        {
            playerFigure = OppositeTurn(playerFigure);
            botFigure = OppositeTurn(botFigure);
        }
    }

    private void SetButtonsInteractable(bool interactable)
    {
        for (int j = 0; j < buttons.Length; j++)
        {
            buttons[j].interactable = interactable;
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (_as == null || clip == null)
            return;

        _as.clip = clip;
        _as.Play();
    }

    private static turn OppositeTurn(turn value)
    {
        return (turn)(1 - (int)value);
    }

    private IEnumerator TimerTurnCircle()
    {
        while (true)
        {
            if (Saver.saves.difficult == Difficult.easy)
                timerTurnValue -= (float)0.1f / secondsCount[0];
            else if (Saver.saves.difficult == Difficult.medium)
                timerTurnValue -= (float)0.1f / secondsCount[1];
            else if (Saver.saves.difficult == Difficult.hard)
                timerTurnValue -= (float)0.1f / secondsCount[2];
            turnTimer.fillAmount = timerTurnValue;

            if (timerTurnValue <= 0 && !win)
            {
                currentTurn = OppositeTurn(currentTurn);
                Debug.Log("WINNER: " + currentTurn.ToString());
                win = true;
                WinNotify();
                break;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator StartTimer()
    {
        while (true)
        {
            time += Time.deltaTime;
            UpdateTimeText();
            yield return null;
        }
    }
    private void UpdateTimeText()
    {
        float minutes = Mathf.FloorToInt(time / 60);
        float seconds = Mathf.FloorToInt(time % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    #region CheckWin
    private void CheckWinDiagonalsRight(int col, int currentCell, int currentFigure)
    {
        int winDetect = 0;
        int verticalIndex = -3;
        for (int i = col - 3; i < col + 4; i++)
        {
            if (currentCell + verticalIndex >= 0 && currentCell + verticalIndex <= 4 && i >= 0 && i <= 9)
            {
                if (coloumn[i].line[currentCell + verticalIndex] == currentFigure)
                {
                    winDetect++;
                }
                else
                {
                    winDetect = 0;
                }
                if (winDetect == 4)
                {
                    GameObject temp = Instantiate(winLine, coloumn[i].spawnPointsCell[currentCell + verticalIndex].position + new Vector3(-1.527f, -1.527f, 0), Quaternion.Euler(new Vector3(0, 0, 45f)), coloumn[i].spawnPointsCell[currentCell + verticalIndex]);
                    temp.transform.localScale += new Vector3(150, 0, 0);
                    Debug.Log("WINNER: " + currentTurn.ToString());
                    win = true;
                    WinNotify();
                    break;
                }
            }
            verticalIndex++;
        }
    }

    private void CheckWinDiagonalsLeft(int col, int currentCell, int currentFigure)
    {
        int winDetect = 0;
        int verticalIndex = 3;
        for (int i = col - 3; i < col + 4; i++)
        {
            if (currentCell + verticalIndex >= 0 && currentCell + verticalIndex <= 4 && i >= 0 && i <= 9)
            {
                if (coloumn[i].line[currentCell + verticalIndex] == currentFigure)
                {
                    winDetect++;
                }
                else
                {
                    winDetect = 0;
                }
                if (winDetect == 4)
                {
                    GameObject temp = Instantiate(winLine, coloumn[i].spawnPointsCell[currentCell + verticalIndex].position + new Vector3(-1.527f, 1.527f, 0), Quaternion.Euler(new Vector3(0, 0, -45f)), coloumn[i].spawnPointsCell[currentCell + verticalIndex]);
                    temp.transform.localScale += new Vector3(150, 0, 0);
                    Debug.Log("WINNER: " + currentTurn.ToString());
                    win = true;
                    WinNotify();
                    break;
                }
            }
            verticalIndex--;
        }
    }

    private void CheckWinLines(int col, int currentCell, int currentFigure)
    {
        int winDetect = 0;
        for (int i = col - 3; i < col + 4; i++)
        {
            if (i >= 0 && i <= 9)
            {
                if (coloumn[i].line[currentCell] == currentFigure)
                {
                    winDetect++;
                }
                else
                {
                    winDetect = 0;
                }
                if (winDetect == 4)
                {
                    Instantiate(winLine, coloumn[i].spawnPointsCell[currentCell].position + new Vector3(-1.527f, 0, 0), Quaternion.identity, coloumn[i].spawnPointsCell[currentCell]);
                    Debug.Log("WINNER: " + currentTurn.ToString());
                    win = true;
                    WinNotify();
                    break;
                }
            }
        }
    }

    private void CheckWinColoum(int col, int currentCell, int currentFigure)
    {
        if (coloumn[col].line[currentCell - 1] == currentFigure && coloumn[col].line[currentCell - 2] == currentFigure && coloumn[col].line[currentCell - 3] == currentFigure)
        {
            Instantiate(winLine, coloumn[col].spawnPointsCell[currentCell].position + new Vector3(0, -1.527f, 0), Quaternion.Euler(new Vector3(0, 0, 90f)), coloumn[col].spawnPointsCell[currentCell]);

            Debug.Log("WINNER: " + currentTurn.ToString());
            win = true;
            WinNotify();
        }
    }
    #endregion

    private void WinNotify()
    {
        if (win)
        {
            StopAllCoroutines();
            dropTween?.Kill();
            dropTween = null;
            winPanel.SetActive(true);
            if (currentTurn == turn.cross)
            {
                crossWinText.SetActive(true);
                NullsWinText.SetActive(false);
            }
            else if (currentTurn == turn.nulls)
            {
                crossWinText.SetActive(false);
                NullsWinText.SetActive(true);
            }
            if (bot)
            {
                SetButtonsInteractable(false);

                Saver.saves.gamesAll[(int)Saver.saves.difficult]++;
                if (playerFigure == turn.cross)
                {
                    Saver.saves.gamesForCross[(int)Saver.saves.difficult]++;
                }
                else
                {
                    Saver.saves.gamesForNulls[(int)Saver.saves.difficult]++;
                }
                Saver.saves.cellsFilled[(int)Saver.saves.difficult] += (turnNumber + 1);
                if (currentTurn == playerFigure)
                {
                    Saver.saves.wins[(int)Saver.saves.difficult]++;
                }
                else
                {
                    Saver.saves.loses[(int)Saver.saves.difficult]++;
                }
            }
            else
            {
                Saver.saves.gamesInLocalMode++;
                if (currentTurn == turn.cross)
                {
                    Saver.saves.crossWinsInLocalMode++;
                }
                else
                {
                    Saver.saves.nullsWinsInLocalMode++;
                }
            }
            Saver.SaveProgress();
        }
    }

    public void ReloadSceneButton(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    [ContextMenu("Find")]
    public void FindAllObj()
    {
        for (int i = 0; i < coloumn.Length; i++)
        {
            for (int j = 0; j < coloumn[i].spawnPointsCell.Length; j++)
            {
                coloumn[i].spawnPointsCell[j] = GameObject.Find("Canvas/GameSpace/Line (" + (j + 1) + ")/Image (" + (i + 1) + ")").transform;
            }
        }

        //test = GameObject.Find("Canvas/GameSpace/Line (5)/Image (3)");
    }

    [ContextMenu("Revert")]
    public void DebugRevert()
    {
        coloumn[lastSetCol].line[lastSetLine] = 0;
        Destroy(coloumn[lastSetCol].figure[lastSetLine]);
        coloumn[lastSetCol].figure[lastSetLine] = null;
        currentTurn = OppositeTurn(currentTurn);
        turnNumber -= 1;
    }
}
