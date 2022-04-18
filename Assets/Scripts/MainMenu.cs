using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private TMP_Text highScoreText;
    [SerializeField] private TMP_Text energyText;
    [SerializeField] private int maxEnergy;
    [SerializeField] private int energyRechargeDuration;
    [SerializeField] private AndroidNotificationHandler androidNotificationHandler;
    [SerializeField] private IOSNotificationHandler iOSNotificationHandler;
    [SerializeField] private Button playButton;

    private int energy;

    private const string ENERGY_KEY = "Energy";
    private const string ENERGY_READY_KEY = "EnergyReady";

    private void Start()
    {
        OnApplicationFocus(true);
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus) return;

        CancelInvoke();

        Application.targetFrameRate = 60;
        int highScore = PlayerPrefs.GetInt(ScoreSystem.HIGH_SCORE_KEY, 0);
        highScoreText.text = $"High Score:\n{highScore}";

        energy = PlayerPrefs.GetInt(ENERGY_KEY, maxEnergy);
        if(energy == 0)
        {
            string energyReadyString = PlayerPrefs.GetString(ENERGY_READY_KEY, string.Empty);

            if(energyReadyString == string.Empty) return;

            DateTime energyReady = DateTime.Parse(energyReadyString);

            if(DateTime.Now > energyReady)
            {
                energy = maxEnergy;
                PlayerPrefs.SetInt(ENERGY_KEY, energy);
            } 
            else 
            {
                playButton.interactable = false;
                Invoke(nameof(EnergyRecharged), (energyReady - DateTime.Now).Seconds);
            }
        }

        energyText.text = $"Play ({energy})";
    }

    private void EnergyRecharged()
    {
        playButton.interactable = true;
        energy = maxEnergy;
        PlayerPrefs.SetInt(ENERGY_KEY, energy);
        energyText.text = $"Play ({energy})";
    }

    public void Play()
    {
        if (energy < 1) return;

        energy--;

        PlayerPrefs.SetInt(ENERGY_KEY, energy);

        if (energy == 0)
        {
            DateTime energyReady = DateTime.Now.AddMinutes(energyRechargeDuration);
            PlayerPrefs.SetString(ENERGY_READY_KEY, energyReady.ToString());

#if UNITY_ANDROID
            androidNotificationHandler.ScheduleNotification(energyReady);
#elif UNITY_IOS
            iOSNotificationHandler.ScheduleNotification(energyRechargeDuration);
#endif
        }

        SceneManager.LoadScene(1);
    }
}
