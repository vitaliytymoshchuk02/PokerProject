using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public List<Card> hand { get; private set; }
    public float startingStack = 100f;
    public float stack { get; set; }
    public float currentBet;
    public bool hasRaised;
    protected readonly string GlowFlickingTrigger = "GlowFlicking";
    protected readonly string RestState = "Rest";
    [SerializeField] protected Animator animator;
    [SerializeField] protected GameController controller;
    [SerializeField] protected Dealer dealer;

    [SerializeField] private GameObject callMessagePrefab;
    [SerializeField] private GameObject checkMessagePrefab;
    [SerializeField] private GameObject raiseMessagePrefab;
    [SerializeField] private GameObject foldMessagePrefab;
    [SerializeField] private GameObject allInMessagePrefab;
    [SerializeField] private GameObject betMessagePrefab;

    public Status status = Status.Active;
    public enum Status { Active, Folded, AllIn };
    public HandEvaluator.HandRank handRank;
    public Player()
    {
        hand = new List<Card>();
        stack = startingStack;
    }
    public void StartGlowFlicking()
    {
        if (animator == null) Debug.LogError($"Animator component not found on {gameObject.name}.");
        else animator.SetTrigger(GlowFlickingTrigger);
    }
    public void StopGlowFlicking()
    {
        animator.ResetTrigger(GlowFlickingTrigger);
        animator.Play(RestState);
    }
    public void ReceiveCard(Card card)
    {
        hand.Add(card);
    }

    public void ClearHand()
    {
        hand.Clear();
    }
    public void Call(int playerID)
    {
        MakeBet(dealer.currentBet - currentBet);
        controller.UpdateStack(playerID, stack);
        controller.ShowBet(playerID, currentBet);
        controller.ShowMessage(playerID, callMessagePrefab);
    }
    public void Raise(int playerID, float size)
    {
        MakeBet(size);
        controller.UpdateStack(playerID, stack);
        controller.ShowBet(playerID, currentBet);
        controller.ShowMessage(playerID, raiseMessagePrefab);
    }
    public void Check(int playerID)
    {
        controller.ShowMessage(playerID, checkMessagePrefab);
    }
    public void Fold(int playerID)
    {
        status = Status.Folded;
        dealer.activePlayersLeft--;
        controller.ShowFold(playerID);
        controller.ShowMessage(playerID, foldMessagePrefab);
    }
    public void Bet(int playerID, float size)
    {
        MakeBet(size);
        controller.UpdateStack(playerID, stack);
        controller.ShowBet(playerID, currentBet);
        controller.ShowMessage(playerID, betMessagePrefab);
    }
    public void MakeBet(float size)
    {
        size = (float)System.Math.Round(size, 1);
        if (size >= stack)
        {
            size = stack;
            status = Status.AllIn;
            dealer.allInPlayers++;
        }
        stack -= size;
        dealer.AddToPot(size);
        currentBet = currentBet + size;
        if (dealer.currentBet < currentBet) dealer.currentBet = currentBet;
    }
}
