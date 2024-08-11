using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerControlUI : MonoBehaviour
{
    [SerializeField] private GameObject foldButton;
    [SerializeField] private GameObject callButton;
    [SerializeField] private TextMeshProUGUI callButtonSize;
    [SerializeField] private GameObject raiseButton;
    [SerializeField] private TextMeshProUGUI raiseButtonSize;
    [SerializeField] private GameObject checkButton;
    [SerializeField] private GameObject betButton;
    [SerializeField] private TextMeshProUGUI betButtonSize;
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI betText;

    [SerializeField] private Player mainPlayer;
    [SerializeField] private Dealer dealer;

    private Canvas canvas;

    void Start()
    {
        canvas = GetComponent<Canvas>();

        slider.onValueChanged.AddListener(delegate { UpdateBetSize(); });
        UpdateBetSize();
    }
    void UpdateBetSize()
    {
        betText.text = slider.value.ToString();
        betButtonSize.text = slider.value.ToString() + " $";
        raiseButtonSize.text = (mainPlayer.currentBet + slider.value).ToString() + " $";
    }
    public void ShowButtons(bool value)
    {
        if (value)
        {
            if (mainPlayer.currentBet < dealer.currentBet)
            {
                slider.minValue = dealer.currentBet;
                callButtonSize.text = dealer.currentBet.ToString() + " $";
                float raiseSize = dealer.currentBet * 2;
                if (raiseSize > mainPlayer.stack) slider.minValue = mainPlayer.stack;
                else slider.minValue = raiseSize;

                foldButton.SetActive(true);
                callButton.SetActive(true);
                raiseButton.SetActive(true);
            }
            else
            {
                slider.minValue = 2;
                if (mainPlayer.currentBet > 0)
                {
                    foldButton.SetActive(true);
                    checkButton.SetActive(true);
                    raiseButton.SetActive(true);
                }
                else
                {
                    foldButton.SetActive(true);
                    checkButton.SetActive(true);
                    betButton.SetActive(true);
                }
            }
            slider.maxValue = mainPlayer.stack;
            UpdateBetSize();
            slider.value = slider.minValue;
            canvas.enabled = true;
        }
        else
        {
            foldButton.SetActive(false);
            callButton.SetActive(false);
            raiseButton.SetActive(false);
            checkButton.SetActive(false);
            betButton.SetActive(false);

            canvas.enabled = false;
        }

    }
    public void OnPlayerAction(string action)
    {
        if (action == "Call") mainPlayer.Call(0);
        if (action == "Check") mainPlayer.Check(0);
        if (action == "Fold") mainPlayer.Fold(0);
        if (action == "Raise")
        {
            mainPlayer.Raise(0, slider.value);
            dealer.isRaised = true;
        }
        if (action == "Bet")
        {
            mainPlayer.Bet(0, slider.value);
            dealer.isRaised = true;
        }
        canvas.enabled = false;
        ShowButtons(false);
    }
}
