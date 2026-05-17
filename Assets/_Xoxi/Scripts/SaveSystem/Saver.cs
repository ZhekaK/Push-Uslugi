using UnityEngine;
using YG;

public class SaveManager : MonoBehaviour
{
    private async void Awake()
    {
        bool serverReached = await Saver.LoadProgressAsync();

        if (serverReached)
            await Saver.SaveProgressAsync();
    }
}
