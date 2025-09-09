using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class OptionsManager : MonoBehaviour
{
    public Text masterLabel;
    public Text[] labels;
    public GameObject[] optionsCards;
    public Toggle[] difficultyToggles;
    public Text soundText;
    public Slider soundSlider;
    public Text musicText;
    public Slider musicSlider;
    public Toggle spatialAudio;
    public GameObject contentWindow;
    public Toggle cameraSway;
    public Toggle modernGraphics;
    public GameObject resButtonPrefab;
    List<GameObject> resButtons = new();


    Resolution[] resolutions;
    public int fontSize;
    int difficultyLevel = 0;

    public void StartUp()
    {
        OnOpen();
        StartCoroutine(_wait());
        resolutions = Screen.resolutions;
        for (int i = 0; i < resolutions.Length; i++)
        {
            GameObject newButton = Instantiate(resButtonPrefab, contentWindow.transform);
            newButton.GetComponentInChildren<Text>().text = resolutions[i].ToString();
            resButtons.Add(newButton);
        }
    }

    void OnOpen()
    {
        soundSlider.value = DataManager.data.soundVolume * 100;
        soundText.text = "" + soundSlider.value;
        musicSlider.value = DataManager.data.musicVolume * 100;
        musicText.text = "" + musicSlider.value;
        spatialAudio.SetIsOnWithoutNotify(DataManager.data.spatialAudio);
        cameraSway.SetIsOnWithoutNotify(cameraSway);
        modernGraphics.isOn = DataManager.data.enchancedMode;
        ChangeDifficulty(DataManager.data.difficulty);
    }

    public void OnClick(int menu)
    {
        MainMenuActions.menuBackground.ButtonSound(1);
        foreach (var card in optionsCards) { card.SetActive(false); } 
        optionsCards[menu].SetActive(true);
        OnOpen(); 
    }

    public void ChangeDifficulty(int difficulty)
    {
        difficultyLevel = difficulty;
        foreach (Toggle item in difficultyToggles) 
        { 
            item.SetIsOnWithoutNotify(false);
        } 
        difficultyToggles[difficulty].SetIsOnWithoutNotify(true);
        UpdateValues();
    }

    public void UpdateValues()
    {
        DataManager.data.soundVolume = soundSlider.value/100;
        soundText.text = ""+soundSlider.value;
        DataManager.data.musicVolume = musicSlider.value/100;
        musicText.text = ""+musicSlider.value;
        DataManager.data.spatialAudio = spatialAudio.isOn;
        //DataManager.data.resolution = selected resolution from list
        DataManager.data.cameraSway = cameraSway;
        MainMenuActions.menuBackground.musicAudio.volume = DataManager.data.musicVolume;
        DataManager.data.enchancedMode = modernGraphics.isOn;
        DataManager.data.difficulty = difficultyLevel;
    }

    IEnumerator _wait()
    {
        while (masterLabel.cachedTextGenerator.fontSizeUsedForBestFit == 0)
            yield return new WaitForSeconds(0);
        fontSize = masterLabel.cachedTextGenerator.fontSizeUsedForBestFit;
        for (int i = 0; i < labels.Length; i++)
        {
            labels[i].fontSize = fontSize;
        }
    }
}
