using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GetDropdownValue : MonoBehaviour
{
    [SerializeField] TMP_Dropdown dropdown;
    [SerializeField] List<Sprite> backgroundsList;
    [SerializeField] Image background;

    void Start()
    {
        if (PlayerPrefs.HasKey("Background"))
        {
            int savedIndex = PlayerPrefs.GetInt("Background");
            dropdown.value = savedIndex;
            dropdown.RefreshShownValue();
            ChangeBackground(savedIndex);
        }
        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }
    void OnDropdownValueChanged(int index)
    {
        ChangeBackground(index);
        PlayerPrefs.SetInt("Background", index);
        PlayerPrefs.Save();
    }
    void ChangeBackground(int index)
    {
        switch (index)
        {
            case 0: background.sprite = backgroundsList[0]; break;
            case 1: background.sprite = backgroundsList[1]; break;
            case 2: background.sprite = backgroundsList[2]; break;
            case 3: background.sprite = backgroundsList[3]; break;
        }
    }
}
