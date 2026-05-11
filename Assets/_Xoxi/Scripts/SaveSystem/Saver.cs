using UnityEngine;
using YG;

public class SaveManager : MonoBehaviour
{
    private void Awake()
    {
        Saver.LoadProgress();
        Saver.SaveProgress();
    }
}
