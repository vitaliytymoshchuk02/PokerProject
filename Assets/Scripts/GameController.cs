using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public readonly int maxPlayers = 6;
    [SerializeField] private Dealer dealer;

    [SerializeField] private Chips chips;
    [SerializeField] private List<Player> players;
    [SerializeField] private List<GameObject> cardsOfPlayers;
    [SerializeField] private TextMeshProUGUI playerName;

    public readonly float smallBlind = 1;
    public readonly float bigBlind = 2;
    public readonly string currency = "$";

    [SerializeField] public Canvas actionCanvas;
    [SerializeField] private List<Sprite> cardSprites;
    [SerializeField] private Sprite backOfCard;

    [SerializeField] private TextMeshProUGUI totalPotText;
    private GameObject potImage;
    [SerializeField] private GameObject potPrefab;
    [SerializeField] private GameObject potPosition;

    [SerializeField] private List<TextMeshProUGUI> playersStacksTexts;

    [SerializeField] private List<Image> betPositions;
    [SerializeField] private List<GameObject> betBarPrefabs;
    [SerializeField] private PlayerControlUI playerControlUI;
    [SerializeField] private List<GameObject> buttonPositions;
    [SerializeField] private GameObject dealerButton;
    [SerializeField] HandEvaluator handEvaluator;

    void Start()
    {
        UpdatePlayerName();
        dealer.PrepareGame();
        StartGame();
    }
    public void StartGame()
    {
        StartCoroutine(dealer.PreFlop());
    }

    /*------------------------UtilMethods------------------------*/
    public void EvaluateHands()
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].status == Player.Status.Folded) continue;
            players[i].handRank = handEvaluator.EvaluateBestHand(GetRelevantCards(i));
        }
    }
    public List<Card> GetRelevantCards(int id)
    {
        return players[id].hand.ToList().Concat(dealer.communityCards).ToList();
    }
    public List<int> FindWinner()
    {
        List<int> listWinners = new List<int>();
        int winnerID = 0;

        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].status == Player.Status.Folded) continue;

            winnerID = i;
            break;
        }

        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].status == Player.Status.Folded || i == winnerID) continue;
            if ((int)players[winnerID].handRank < (int)players[i].handRank)
            {
                winnerID = i;
                listWinners.Clear();
            }
            else if ((int)players[winnerID].handRank == (int)players[i].handRank)
            {
                List<Card> list1 = GetRelevantCards(winnerID);
                Debug.Log($"Relevant cards of {winnerID}: {list1[0].Rank},{list1[1].Rank},{list1[2].Rank},{list1[3].Rank},{list1[4].Rank},{list1[5].Rank},{list1[6].Rank}");

                List<Card> list2 = GetRelevantCards(i);
                Debug.Log($"Relevant cards of {i}: {list2[0].Rank},{list2[1].Rank},{list2[2].Rank},{list2[3].Rank},{list2[4].Rank},{list2[5].Rank},{list2[6].Rank}");


                List<Card> bestList1 = handEvaluator.GetBestHand(list1);
                Debug.Log($"Best cards of {winnerID}: {bestList1[0].Rank},{bestList1[1].Rank},{bestList1[2].Rank},{bestList1[3].Rank},{bestList1[4].Rank}");

                List<Card> bestList2 = handEvaluator.GetBestHand(list2);
                Debug.Log($"Best cards of {i}: {bestList2[0].Rank},{bestList2[1].Rank},{bestList2[2].Rank},{bestList2[3].Rank},{bestList2[4].Rank}");

                int result = handEvaluator.CheckHigherCards(bestList1, bestList2);
                if (result == 1)
                {
                    Debug.Log($"Player {i} is now a winner");
                    winnerID = i;
                    listWinners.Clear();
                }
                else if (result == 0)
                {
                    if (!listWinners.Contains(winnerID))
                    {
                        listWinners.Add(winnerID);
                    }
                    listWinners.Add(i);
                }
            }
        }
        if (listWinners.Count > 0)
        {
            return listWinners;
        }
        return new List<int> { winnerID };
    }
    public Sprite FindSpriteByName(string name)
    {
        return cardSprites.Find(sprite => sprite.name == name);
    }
    public List<Player> GetPlayers() { return players; }
    public void EnablePlayerControl(bool value)
    {
        playerControlUI.ShowButtons(value);
    }
    private IEnumerator DeleteGameObject(GameObject gameObject)
    {
        yield return new WaitForSeconds(1);
        Destroy(gameObject);
    }
    public bool CheckAllPlayersCalled()
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].status == Player.Status.Folded || players[i].status == Player.Status.AllIn) continue;
            if (players[i].currentBet < dealer.currentBet)
            {
                return false;
            }
        }
        return true;
    }

    /*------------------------UpdateMethods------------------------*/
    public void UpdateStack(int id, float size)
    {
        playersStacksTexts[id].text = $"{size} {currency}";
    }
    public void UpdateTotalPotText(float totalPot)
    {
        totalPotText.text = "Total Pot : " + totalPot + " " + currency;
    }
    public void UpdatePotImage(float totalPot)
    {
        ClearBetImages();
        potImage.GetComponentInChildren<TextMeshProUGUI>().text = totalPot.ToString() + " " + currency;
        potImage.SetActive(true);
    }

    public void UpdatePlayerName()
    {
        if (PlayerPrefs.HasKey("PlayerName"))
        {
            string loadedName = PlayerPrefs.GetString("PlayerName");

            playerName.text = loadedName;
        }
        else
        {
            string defaultName = "Player";
            PlayerPrefs.SetString("PlayerName", defaultName);
            PlayerPrefs.Save();

            playerName.text = defaultName;
        }
    }

    /*------------------------ClearMethods------------------------*/
    private void ClearBetImages()
    {
        StartCoroutine(ClearBetImagesCoroutine());
    }
    private IEnumerator ClearBetImagesCoroutine()
    {
        float waitTime = 0.35f;

        List<Coroutine> animations = new List<Coroutine>();
        for (int i = 0; i < players.Count; i++)
        {
            var animationController = betPositions[i].gameObject.GetComponent<AnimationController>();
            animationController.SetTrigger("ToPot");
        }
        yield return new WaitForSeconds(waitTime);

        for (int i = 0; i < players.Count; i++)
        {
            betPositions[i].enabled = false;
            betBarPrefabs[i].SetActive(false);
        }
    }
    internal void ClearCurrentBets()
    {
        foreach (var player in players)
        {
            player.currentBet = 0;
            player.hasRaised = false;
        }
        dealer.currentBet = 0;
    }
    public void ClearAllHands()
    {
        for (int i = 0; i < players.Count; i++)
        {
            players[i].ClearHand();
            if (players[i].status == Player.Status.Folded) continue;
            ShowFold(i);
        }
    }
    public void ClearCommunityCards()
    {
        foreach (GameObject card in dealer.communityCardsGO) card.GetComponent<Image>().enabled = false;
    }

    /*------------------------ShowMethods------------------------*/
    public void ShowDealerButton(int positionID)
    {
        dealerButton.transform.position = buttonPositions[positionID].transform.position;
    }
    internal void ShowMessage(int playerID, GameObject action)
    {
        GameObject go = Instantiate(action, players[playerID].transform);
        StartCoroutine(DeleteGameObject(go));
    }
    public void ShowCommunityCard(GameObject go, Card card)
    {
        Image image = go.GetComponent<Image>();
        image.sprite = FindSpriteByName(card.ToString());
        image.enabled = true;

        go.GetComponent<AnimationController>().PlayAnimation("Deal");
    }
    public void ShowBlindBet(int id, float size)
    {
        betPositions[id].sprite = chips.GetChipsSprite(size);
        betPositions[id].enabled = true;

        betBarPrefabs[id].SetActive(true);
        betBarPrefabs[id].GetComponentInChildren<TextMeshProUGUI>().text = $"{size} {currency}";
    }
    public void ShowBet(int id, float size)
    {
        betPositions[id].sprite = chips.GetChipsSprite(size);
        betPositions[id].enabled = true;
        betPositions[id].gameObject.GetComponent<AnimationController>().PlayAnimation("Bet");

        betBarPrefabs[id].SetActive(true);
        betBarPrefabs[id].GetComponentInChildren<TextMeshProUGUI>().text = $"{size} {currency}";
    }
    public void ShowFold(int index)
    {
        Image[] cards = cardsOfPlayers[index].GetComponentsInChildren<Image>();

        for (int i = 0; i < cards.Length; i++)
        {
            cards[i].gameObject.GetComponent<AnimationController>().SetTrigger("Fold");
        }
    }
    internal void ShowWinner()
    {
        List<int> winners = FindWinner();

        if (winners.Count > 1)
        {
            float winnersPart = dealer.totalPot / winners.Count;
            List<GameObject> potImages = new List<GameObject> { potImage };
            for (int i = 1; i < winners.Count; i++) { potImages.Add(Instantiate(potImage, potImage.transform)); }
            for (int i = 0; i < winners.Count; i++)
            {
                potImages[i].GetComponent<Animator>().SetTrigger(winners[i].ToString());
                players[winners[i]].stack += winnersPart;
                UpdateStack(winners[i], players[winners[i]].stack);
                if (i != 0) StartCoroutine(DeleteGameObject(potImages[i]));
            }
        }
        else
        {
            potImage.GetComponent<Animator>().SetTrigger(winners[0].ToString());
            players[winners[0]].stack += dealer.totalPot / winners.Count;
            UpdateStack(winners[0], players[winners[0]].stack);
        }
    }
    public void ShowCardsFaceUp(int index)
    {
        Image[] cards = cardsOfPlayers[index].GetComponentsInChildren<Image>();

        for (int i = 0; i < cards.Length; i++)
        {
            cards[i].sprite = FindSpriteByName(players[index].hand[i].ToString());
            cards[i].gameObject.GetComponent<Animator>().SetTrigger("Deal");

            cards[i].enabled = true;
        }
    }
    public void ShowBotCardsFaceUp()
    {
        for (int i = 1; i < players.Count; i++)
        {
            if (players[i].status == Player.Status.Folded) continue;
            Image[] cardSlots = cardsOfPlayers[i].GetComponentsInChildren<Image>();

            for (int j = 0; j < cardSlots.Length; j++)
            {
                cardSlots[j].sprite = FindSpriteByName(players[i].hand[j].ToString());
            }
        }
    }
    public void ShowBotCardsFaceDown()
    {
        for (int i = 1; i < players.Count; i++)
        {
            if (players[i].status == Player.Status.Folded) continue;
            Image[] cardSlots = cardsOfPlayers[i].GetComponentsInChildren<Image>();

            for (int j = 0; j < cardSlots.Length; j++)
            {
                cardSlots[j].sprite = backOfCard;
                cardSlots[j].gameObject.GetComponent<Animator>().SetTrigger("Deal");

                cardSlots[j].enabled = true;
            }
        }
    }

    /*------------------------ResetMethods------------------------*/
    public void ResetCardPositions()
    {
        for (int i = 0; i < players.Count; i++)
        {
            Image[] cards = cardsOfPlayers[i].GetComponentsInChildren<Image>();

            for (int j = 0; j < cards.Length; j++)
            {
                cards[j].gameObject.GetComponent<Animator>().SetTrigger("Reset");
            }
        }
    }
    public void ResetPlayersStatus()
    {
        foreach (Player player in players)
        {
            if (player.stack > 0)
            {
                player.status = Player.Status.Active;
            }
            else player.status = Player.Status.Folded;
        }
    }
    public void ResetPotImage()
    {
        if (potImage != null)
        {
            Destroy(potImage.gameObject);
        }
        potImage = Instantiate(potPrefab, potPosition.transform);
        potImage.SetActive(false);
    }
    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void PauseGame()
    {
        if (Time.timeScale > 0)
        {
            Time.timeScale = 0;
        }
        else Time.timeScale = 1;
    }
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    internal string GetCurrentPlayerName()
    {
        return playerName.text;
    }
}
