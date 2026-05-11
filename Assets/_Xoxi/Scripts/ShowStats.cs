using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YG;

public class ShowStats : MonoBehaviour
{
    public DifficultStats[] blocks;
    public Text gamesInLocalMode;
    public Text winsCrossLocalMode;
    public Text winsNullsLocalMode;

    private void OnEnable()
    {
        for (int i = 0; i < blocks.Length; i++)
        {
            blocks[i].gamesAll.text = Saver.saves.gamesAll[i].ToString();
            blocks[i].forCross.text = Saver.saves.gamesForCross[i].ToString();
            blocks[i].forNulls.text = Saver.saves.gamesForNulls[i].ToString();
            blocks[i].cells.text = Saver.saves.cellsFilled[i].ToString();
            blocks[i].wins.text = Saver.saves.wins[i].ToString();
            blocks[i].loses.text = Saver.saves.loses[i].ToString();
        }

        gamesInLocalMode.text = Saver.saves.gamesInLocalMode.ToString();
        winsCrossLocalMode.text = Saver.saves.crossWinsInLocalMode.ToString();
        winsNullsLocalMode.text = Saver.saves.nullsWinsInLocalMode.ToString();
    }
}
