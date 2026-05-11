using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YG;

public class Languages : MonoBehaviour
{
    [SerializeField] private string[] langsText; //������ ��������� ������� ������ ��������� � ��������� enum languages
    private Text textBox;

    private void Awake()
    {
        MainMenu.changeLanguageAction += LanguageChange;
    }

    private void Start()
    {
        textBox = GetComponent<Text>();
        Invoke(nameof(LanguageChange), 0.01f);
    }

    public void LanguageChange()
    {
        textBox.text = langsText[(int)Saver.saves._lang];
    }

    private void OnDestroy()
    {
        MainMenu.changeLanguageAction -= LanguageChange;
    }
}
