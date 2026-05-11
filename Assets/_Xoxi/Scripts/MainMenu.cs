using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using YG;

public class MainMenu : MonoBehaviour
{
    public static MainMenu instance;

    public Text versionText;
    public Text companyText;

    public Dropdown langDropdown, difficultDropdown;
    public Text difDDLableText;
    public Toggle anim, sound, timer, markLastFigure;
    public Slider animSpeed;

    public static bool botEnable;
    public static turn playFigure;

    public static Action changeLanguageAction;
    [SerializeField] private Button menuButton;
    [SerializeField] private string menuSceneName = "MainMenuScene";
    [SerializeField] private string gameSceneName = "GameXoxi";

    private void Awake()
    {
        instance = this;

        versionText.text = Application.version;
        companyText.text = Application.companyName;

        menuButton.onClick.AddListener(() => SceneManager.LoadScene(menuSceneName));
    }

    private void Start()
    {
        Invoke(nameof(LoadDelay), 0.1f);
    }
    private void LoadDelay()
    {
        langDropdown.value = (int)Saver.saves._lang;
        difficultDropdown.value = (int)Saver.saves.difficult;
        anim.isOn = Saver.saves.anim;
        sound.isOn = Saver.saves.sound;
        timer.isOn = Saver.saves.timerTurn;
        markLastFigure.isOn = Saver.saves.markLastFigure;
        animSpeed.value = Saver.saves.animationSpeed;
        ChangeDropdownText();
    }

    private void ChangeDropdownText()
    {
        if (Saver.saves._lang == Lang.ru)
        {
            difficultDropdown.options[0].text = "Легко";
            difficultDropdown.options[1].text = "Средне";
            difficultDropdown.options[2].text = "Сложно";
        }
        else if (Saver.saves._lang == Lang.en)
        {
            difficultDropdown.options[0].text = "Easy";
            difficultDropdown.options[1].text = "Medium";
            difficultDropdown.options[2].text = "Hard";
        }
        difDDLableText.text = difficultDropdown.options[(int)Saver.saves.difficult].text;
    }
    public void PlayBtn(int index)
    {
        if (index == 0)
        {
            botEnable = false;
        }
        else if (index == 1)
        {
            botEnable = true;
        }

        SceneManager.LoadScene(gameSceneName);
    }

    public void ChooseFigure(int index)
    {
        playFigure = (turn)index;
    }

    public void changeLang(int index)
    {
        Saver.saves._lang = (Lang)index;
        changeLanguageAction?.Invoke();
        ChangeDropdownText();
        Saver.SaveProgress();
    }

    public void changeDifficult(int index)
    {
        Saver.saves.difficult = (Difficult)index;
        Saver.SaveProgress();
    }

    public void changeAnim(bool index)
    {
        Saver.saves.anim = index;
        Saver.SaveProgress();
    }

    public void changeAnimSpeed(float index)
    {
        Saver.saves.animationSpeed = index;
        Saver.SaveProgress();
    }

    public void changeSound(bool index)
    {
        Saver.saves.sound = index;
        Saver.SaveProgress();
    }

    public void changeTimer(bool index)
    {
        Saver.saves.timerTurn = index;
        Saver.SaveProgress();
    }

    public void changeMarkLast(bool index)
    {
        Saver.saves.markLastFigure = index;
        Saver.SaveProgress();
    }

    void OnDestroy()
    {
        menuButton.onClick.RemoveAllListeners();
    }
}
