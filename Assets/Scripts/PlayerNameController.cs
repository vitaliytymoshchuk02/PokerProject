using TMPro;
using UnityEngine;

public class PlayerNameController : MonoBehaviour
{
    public TMP_InputField playerNameInput;
    private string playerName;
    [SerializeField] private GameController gameController;

    private void Start()
    {
        if (playerNameInput != null)
        {
            playerNameInput.text = PlayerPrefs.GetString("PlayerName");
            playerNameInput.characterLimit = 20;
            playerNameInput.onValueChanged.AddListener(UpdatePlayerName);
        }
    }

    private void UpdatePlayerName(string newName)
    {
        playerName = newName;
        SavePlayerName();
    }

    public void SavePlayerName()
    {
        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerPrefs.Save();

        if (gameController != null)
        {
            gameController.UpdatePlayerName();
        }
    }
}
