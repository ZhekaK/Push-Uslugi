using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PushPelmesh.App.Auth;
using UnityEngine.SceneManagement;
using System;

public class main : MonoBehaviour
{
    public GameObject napitok;
    public float gradus;
    public InputField inputG;
    string gradusTS;
    public float time;
    public GameObject viborNapitka;
    public Text viborNapitkaT;
    public GameObject vremya;
    public InputField inputTime;
    string timeT;
    public GameObject error;
    public GameObject settings;
    public Text nameT;
    public InputField inputName;
    public Text weightT;
    string weightTS;
    public InputField inputWeight;
    public float quality;
    string qualityT;
    public InputField inputQ;
    public GameObject stThree;
    public GameObject errorTwo;
    public Text finalText;
    public float sum;
    float UniversalKoef = 100f;
    public GameObject final;
    public GameObject table;
    public GameObject stageFour;
    public int circle = 0;
    public GameObject moreeDrink;
    public float alcoScore;
    public float alcoScoreTwo;
    public float alcoScoreThree;
    public float alcoScoreFour;
    public float alcoScoreFive;
    public GameObject other;

    public void reset()
    {
        alcoScore = 0;
        alcoScoreTwo = 0;
        alcoScoreThree = 0;
        alcoScoreFour = 0;
        alcoScoreFive = 0;
        gradus = 0;
        time = 0;
        quality = 0;
        sum = 0;
        circle = 0;
        viborNapitka.SetActive(true);
        vremya.SetActive(false);
        stThree.SetActive(false);
        stageFour.SetActive(false);
        final.SetActive(false);

    }
    public void closePanel()
    {
        napitok.SetActive(false);
    }
    public void openPanel()
    {
        napitok.SetActive(true);
    }
    public void pivo()
    {
        napitok.SetActive(false);
        gradus = 4.5f;
        viborNapitkaT.text = "Пиво";
    }
    public void vodka()
    {
        napitok.SetActive(false);
        gradus = 40;
        viborNapitkaT.text = "Водка";
    }
    public void vino()
    {
        napitok.SetActive(false);
        gradus = 15;
        viborNapitkaT.text = "Вино";
    }
    public void whiteVine()
    {
        napitok.SetActive(false);
        gradus = 7;
        viborNapitkaT.text = "Белое Вино";
    }
    public void absent()
    {
        napitok.SetActive(false);
        gradus = 70;
        viborNapitkaT.text = "Абсент";
    }
    public void shampain()
    {
        napitok.SetActive(false);
        gradus = 12;
        viborNapitkaT.text = "Шампунь";
    }
    public void liker()
    {
        napitok.SetActive(false);
        gradus = 20;
        viborNapitkaT.text = "Ликер";
    }
    public void otherr()
    {
        napitok.SetActive(false);
        other.SetActive(true);
    }
    public void znachenie()
    {
        if (inputG.text == "")
        {
            inputG.text = "1";
        }
        else
        {
            gradusTS = inputG.text;
        }
        gradus = System.Single.Parse(gradusTS);
        viborNapitkaT.text = gradusTS + " градусов";
        other.SetActive(false);
    }
    public void vvodVremeni()
    {
        if (inputTime.text == "")
        {
            inputTime.text = "1";
        }
        else
        {
            timeT = inputTime.text;
        }
        time = System.Single.Parse(timeT);
        vremya.SetActive(false);
        stageFour.SetActive(true);
    }
    public void confirmOne()
    {
        if (gradus != 0)
        {
            viborNapitka.SetActive(false);
            stThree.SetActive(true);
        }
        else
        {
            error.SetActive(true);
        }
    }
    public void closeError()
    {
        error.SetActive(false);
    }
    public void StageF()
    {
        stThree.SetActive(false);
        if (circle == 0)
        {
            vremya.SetActive(true);
        }
        else
        {
            stageFour.SetActive(true);
        }
        if (inputQ.text == "")
        {
            inputQ.text = "0";
        }
        else
        {
            qualityT = inputQ.text;
        }
        quality = System.Single.Parse(qualityT);
        if (circle == 0)
        {
            alcoScore = gradus * quality;
        }
        if (circle == 1)
        {
            alcoScoreTwo = gradus * quality;
        }
        if (circle == 2)
        {
            alcoScoreThree = gradus * quality;
        }
        if (circle == 3)
        {
            alcoScoreFour = gradus * quality;
        }
        if (circle == 4)
        {
            alcoScoreFive = gradus * quality;
            moreeDrink.SetActive(false);
        }
        else
        {
            moreeDrink.SetActive(true);
        }
    }
    public void vvodQuality()
    {
        if (SessionManager.CurrentProfile.weightKg > 1)
        {
            sum = (alcoScore + alcoScoreTwo + alcoScoreThree + alcoScoreFour + alcoScoreFive) / (SessionManager.CurrentProfile.weightKg * UniversalKoef * time);
            stageFour.SetActive(false);
            final.SetActive(true);
            finalText.text = sum.ToString();
            circle = 0;
        }
        else
        {
            errorTwo.SetActive(true);
        }

    }
    public void closeErrorTwo()
    {
        errorTwo.SetActive(false);
    }
    public void tables()
    {
        table.SetActive(true);
    }
    public void OnMain()
    {
        table.SetActive(false);
        final.SetActive(false);
        viborNapitka.SetActive(true);
    }
    public void moreDrink()
    {
        circle++;
        stageFour.SetActive(false);
        viborNapitka.SetActive(true);
    }
    public void exit(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
