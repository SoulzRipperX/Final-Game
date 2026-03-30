using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    private const string BgmVolumeKey = "BGM_VOLUME";
    private const string SfxVolumeKey = "SFX_VOLUME";

    public GameObject optionsMenu;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private AudioSource[] bgmSources;
    [SerializeField] private AudioSource[] sfxSources;

    void Start()
    {
        float bgmVolume = PlayerPrefs.GetFloat(BgmVolumeKey, 1f);
        float sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, 1f);

        if (bgmSlider != null)
        {
            bgmSlider.SetValueWithoutNotify(bgmVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.SetValueWithoutNotify(sfxVolume);
        }

        SetBgmVolume(bgmVolume);
        SetSfxVolume(sfxVolume);
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

    public void SetBgmVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(BgmVolumeKey, volume);
        PlayerPrefs.Save();
        ApplyVolumeToSources(bgmSources, volume);
    }

    public void SetSfxVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(SfxVolumeKey, volume);
        PlayerPrefs.Save();
        ApplyVolumeToSources(sfxSources, volume);
    }

    private static void ApplyVolumeToSources(AudioSource[] sources, float volume)
    {
        if (sources == null)
        {
            return;
        }

        foreach (AudioSource source in sources)
        {
            if (source != null)
            {
                source.volume = volume;
            }
        }
    }
}

