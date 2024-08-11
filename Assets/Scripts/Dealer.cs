using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Dealer : MonoBehaviour
{
    [SerializeField] GameController controller;
    private Deck deck;

    public float totalPot;
    public float currentBet;
    public int activePlayersLeft;
    public int allInPlayers;

    [SerializeField] private int idButton = 2;

    private List<Player> players;

    public List<Card> communityCards;
    [SerializeField] public List<GameObject> communityCardsGO;

    public int currentID;
    public bool isRaised;

    private void ChangeButton()
    {
        idButton++;
        for (int i = idButton; i < idButton + players.Count; i++)
        {
            int id = i % players.Count;
            if (players[id].status == Player.Status.Folded) continue;

            idButton = id;
            break;
        }
    }
    public void PrepareGame()
    {
        PrepareDeck();
        players = controller.GetPlayers();
        allInPlayers = 0;
        activePlayersLeft = players.Count(player => player.status == Player.Status.Active);
        totalPot = 0;
        controller.UpdateTotalPotText(totalPot);
        ChangeButton();
        controller.ShowDealerButton(idButton);
        controller.ResetPotImage();

        communityCards = new List<Card>();

        BetBlinds();
        DealInitialCards();
    }
    public void PrepareDeck()
    {
        deck = new Deck();
        deck.Shuffle();
    }
    private void BetBlinds()
    {
        currentID = idButton + 1;
        if (activePlayersLeft <= 2)
        {
            for (int i = currentID; i < currentID + players.Count; i++)
            {
                int id = i % players.Count;
                if (players[id].status == Player.Status.Folded) continue;

                players[id].MakeBet(controller.bigBlind);
                controller.ShowBlindBet(id, controller.bigBlind);
                controller.UpdateStack(id, players[id].stack);
                currentID = id;
                break;
            }
        }
        else
        {
            for (int i = currentID; i < currentID + players.Count; i++)
            {
                int id = i % players.Count;
                if (players[id].status == Player.Status.Folded) continue;

                players[id].MakeBet(controller.smallBlind);
                controller.ShowBlindBet(id, controller.smallBlind);
                controller.UpdateStack(id, players[id].stack);
                currentID = id + 1;
                break;
            }
            for (int i = currentID; i < currentID + players.Count; i++)
            {
                int id = i % players.Count;
                if (players[id].status == Player.Status.Folded) continue;

                players[id].MakeBet(controller.bigBlind);
                controller.ShowBlindBet(id, controller.bigBlind);
                controller.UpdateStack(id, players[id].stack);
                currentID = id;
                break;
            }
        }
        currentBet = controller.bigBlind;
    }
    void DealInitialCards()
    {
        foreach (var player in players)
        {
            if (player.status != Player.Status.Folded)
            {
                player.ReceiveCard(deck.Deal());
                player.ReceiveCard(deck.Deal());
            }
        }
        controller.ShowCardsFaceUp(0);
        controller.ShowBotCardsFaceDown();
    }
    private void DealFlop()
    {
        DealCommunityCard(communityCardsGO[0]);
        DealCommunityCard(communityCardsGO[1]);
        DealCommunityCard(communityCardsGO[2]);

        currentID = idButton;
        isRaised = false;
        StartCoroutine(Flop());
    }
    private void DealTurn()
    {
        DealCommunityCard(communityCardsGO[3]);

        currentID = idButton;
        isRaised = false;
        StartCoroutine(Turn());
    }
    private void DealRiver()
    {
        DealCommunityCard(communityCardsGO[4]);

        currentID = idButton;
        isRaised = false;
        StartCoroutine(River());
    }
    public IEnumerator PreFlop()
    {
        yield return new WaitForSeconds(1);
        yield return StartCoroutine(BettingRound());
        yield return StartCoroutine(TransitionPreFlop());
    }
    public IEnumerator Flop()
    {
        yield return new WaitForSeconds(1);
        yield return StartCoroutine(BettingRound());
        yield return StartCoroutine(TransitionFlop());
    }
    public IEnumerator Turn()
    {
        yield return new WaitForSeconds(1);
        yield return StartCoroutine(BettingRound());
        yield return StartCoroutine(TransitionTurn());
    }
    public IEnumerator River()
    {
        yield return new WaitForSeconds(1);
        yield return StartCoroutine(BettingRound());
        yield return StartCoroutine(TransitionRiver());
    }
    private IEnumerator TransitionPreFlop()
    {
        for (int i = 10; i > 0; i--)
        {
            if (activePlayersLeft == 1)
            {
                StartCoroutine(PreShowdownWin()); break;
            }
            if (activePlayersLeft == allInPlayers || activePlayersLeft - 1 == allInPlayers)
            {
                if (controller.CheckAllPlayersCalled())
                {
                    DealCommunityCard(communityCardsGO[0]);
                    DealCommunityCard(communityCardsGO[1]);
                    DealCommunityCard(communityCardsGO[2]);
                    yield return new WaitForSeconds(1);

                    DealCommunityCard(communityCardsGO[3]);
                    yield return new WaitForSeconds(1);

                    DealCommunityCard(communityCardsGO[4]);
                    yield return new WaitForSeconds(1);

                    controller.UpdatePotImage(totalPot);
                    controller.ClearCurrentBets();
                    yield return new WaitForSeconds(1);
                    ShowDown();
                    break;
                }
                else
                {
                    yield return StartCoroutine(RaisedBettingRound());
                    continue;
                }
            }
            else if (isRaised)
            {
                yield return StartCoroutine(RaisedBettingRound());
                continue;
            }
            else
            {
                yield return new WaitForSeconds(1);
                controller.UpdatePotImage(totalPot);
                controller.ClearCurrentBets();
                yield return new WaitForSeconds(1);
                DealFlop();
                break;
            }
        }
    }
    private IEnumerator TransitionFlop()
    {
        for (int i = 10; i > 0; i--)
        {
            if (activePlayersLeft == 1)
            {
                StartCoroutine(PreShowdownWin()); break;
            }
            if (activePlayersLeft == allInPlayers || activePlayersLeft - 1 == allInPlayers)
            {
                if (controller.CheckAllPlayersCalled())
                {
                    DealCommunityCard(communityCardsGO[3]);
                    yield return new WaitForSeconds(1);

                    DealCommunityCard(communityCardsGO[4]);
                    yield return new WaitForSeconds(1);
                    controller.UpdatePotImage(totalPot);
                    controller.ClearCurrentBets();
                    yield return new WaitForSeconds(1);
                    ShowDown();
                    break;
                }
                else
                {
                    yield return StartCoroutine(RaisedBettingRound());
                    continue;
                }
            }
            else if (isRaised)
            {
                yield return StartCoroutine(RaisedBettingRound());
                continue;
            }
            else
            {
                yield return new WaitForSeconds(1);
                controller.UpdatePotImage(totalPot);
                controller.ClearCurrentBets();
                yield return new WaitForSeconds(1);
                DealTurn();
                break;
            }
        }
    }
    private IEnumerator TransitionTurn()
    {
        for (int i = 10; i > 0; i--)
        {
            if (activePlayersLeft == 1)
            {
                StartCoroutine(PreShowdownWin()); break;
            }
            if (activePlayersLeft == allInPlayers || activePlayersLeft - 1 == allInPlayers)
            {
                if (controller.CheckAllPlayersCalled())
                {  
                    DealCommunityCard(communityCardsGO[4]);
                    yield return new WaitForSeconds(1);
                    
                    controller.UpdatePotImage(totalPot);
                    controller.ClearCurrentBets();
                    yield return new WaitForSeconds(1);
                    ShowDown();
                    break;
                }
                else
                {
                    yield return StartCoroutine(RaisedBettingRound());
                    continue;
                }
            }
            else if (isRaised)
            {
                yield return StartCoroutine(RaisedBettingRound());
                continue;
            }
            else
            {
                yield return new WaitForSeconds(1);
                controller.UpdatePotImage(totalPot);
                controller.ClearCurrentBets();
                yield return new WaitForSeconds(1);
                DealRiver();
                break;
            }
        }
    }
    private IEnumerator TransitionRiver()
    {
        for (int i = 10; i > 0; i--)
        {
            if (activePlayersLeft == 1) { StartCoroutine(PreShowdownWin()); break; }
            if (activePlayersLeft == allInPlayers || activePlayersLeft - 1 == allInPlayers)
            {
                if (controller.CheckAllPlayersCalled())
                {
                    yield return new WaitForSeconds(1);
                    controller.UpdatePotImage(totalPot);
                    controller.ClearCurrentBets();
                    yield return new WaitForSeconds(1);
                    ShowDown();
                    break;
                }
                else
                {
                    yield return StartCoroutine(RaisedBettingRound());
                    continue;
                }
            }
            else if (isRaised)
            {
                yield return StartCoroutine(RaisedBettingRound());
                continue;
            }
            else
            {
                yield return new WaitForSeconds(1);
                controller.UpdatePotImage(totalPot);
                controller.ClearCurrentBets();
                yield return new WaitForSeconds(1);
                ShowDown();
                break;
            }
        }
    }
    private void ShowDown()
    {
        controller.ShowBotCardsFaceUp();
        controller.EvaluateHands();
        controller.ShowWinner();
        StartCoroutine(PrepareNextRound());
    }
    void DealCommunityCard(GameObject go)
    {
        Card card = deck.Deal();
        communityCards.Add(card);

        controller.ShowCommunityCard(go, card);
    }
    private IEnumerator RaisedBettingRound()
    {
        isRaised = false;
        for (int i = currentID; i < currentID + players.Count; i++)
        {
            if (activePlayersLeft == 1 || activePlayersLeft == allInPlayers) { break; }
            int id = (i + 1) % players.Count;
            if (players[id].status == Player.Status.Folded || players[id].status == Player.Status.AllIn) continue;
            if (id != currentID)
            {
                players[id].StartGlowFlicking();
                if (players[id] is Bot)
                {
                    float waitTime = UnityEngine.Random.Range(1.0f, 3.0f);
                    yield return new WaitForSeconds(3);
                    Bot bot = (Bot)players[id];
                    string action = bot.Action(id);
                    bot.StopGlowFlicking();
                    if (action == "Raise" || action == "Bet")
                    {
                        isRaised = true;
                        currentID = id;
                        break;
                    }
                }
                else
                {
                    controller.EnablePlayerControl(true);
                    yield return new WaitUntil(() => controller.actionCanvas.enabled == false);
                    players[id].StopGlowFlicking();
                    if (isRaised)
                    {
                        currentID = id;
                        break;
                    }
                }
            }
            else { break; }
        }
    }
    public IEnumerator BettingRound()
    {
        isRaised = false;
        for (int i = currentID; i < currentID + players.Count; i++)
        {
            if (activePlayersLeft == 1 || activePlayersLeft == allInPlayers) { break; }

            int id = (i + 1) % players.Count;
            if (players[id].status == Player.Status.Folded || players[id].status == Player.Status.AllIn) continue;
            players[id].StartGlowFlicking();
            if (players[id] is Bot)
            {
                float waitTime = UnityEngine.Random.Range(1.0f, 3.0f);
                yield return new WaitForSeconds(waitTime);
                Bot bot = (Bot)players[id];
                string action = bot.Action(id);
                players[id].StopGlowFlicking();
                if (action == "Raise" || action == "Bet")
                {
                    isRaised = true;
                    currentID = id;
                    break;
                }
            }
            else
            {
                controller.EnablePlayerControl(true);
                yield return new WaitUntil(() => controller.actionCanvas.enabled == false);
                players[id].StopGlowFlicking();
                if (isRaised)
                {
                    currentID = id;
                    break;
                }
            }
        }
    }
    public void AddToPot(float size)
    {
        totalPot += size;
        controller.UpdateTotalPotText(totalPot);
    }
    private IEnumerator PrepareNextRound()
    {
        if (players[0].stack == 0)
        {
            /*Game is over*/
        }
        yield return new WaitForSeconds(3);
        controller.ClearAllHands();
        controller.ClearCommunityCards();
        controller.ResetPlayersStatus();
        controller.ResetCardPositions();
        yield return new WaitForSeconds(1);
        PrepareGame();
        controller.StartGame();
    }
    private IEnumerator PreShowdownWin()
    {
        yield return new WaitForSeconds(1);
        controller.UpdatePotImage(totalPot);
        controller.ClearCurrentBets();
        controller.ShowWinner();
        StartCoroutine(PrepareNextRound());
    }
}
