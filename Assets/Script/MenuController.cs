using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public GameObject optionsMenu;
    public Slider volumeSlider;

    void Start()
    {
        if (volumeSlider != null)
        {
            volumeSlider.value = PlayerPrefs.GetFloat("Volume", 1f);
            AudioListener.volume = volumeSlider.value;
        }
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(1);
    }

    public void ExitGame()
    {
        Debug.LogFormat("Exit");
        Application.Quit();
    }

    public void OpenOptions()
    {
        if (optionsMenu != null)
        {
            optionsMenu.SetActive(true);
        }
    }

    public void CloseOptions()
    {
        if (optionsMenu != null)
        {
            optionsMenu.SetActive(false);
        }
    }

    public void AdjustVolume()
    {
        if (volumeSlider != null)
        {
            AudioListener.volume = volumeSlider.value;
            PlayerPrefs.SetFloat("Volume", volumeSlider.value);
            PlayerPrefs.Save();
        }
    }
}

