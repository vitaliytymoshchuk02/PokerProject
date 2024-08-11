using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
public class Bot : Player
{
    [SerializeField] HandEvaluator handEvaluator;
    HandChart handChart;
    private string ranksString;
    private static readonly string[] ranks = { "2", "3", "4", "5", "6", "7", "8", "9", "T", "J", "Q", "K", "A" };
    public HandEvaluator.DrawRank drawRank;

    private void Start()
    {
        handChart = new HandChart();
    }
    public string Action(int botID)
    {
        List<Card> relevantCards = controller.GetRelevantCards(botID);

        switch (relevantCards.Count)
        {
            case 2: return PreFlopDecision(botID);
            case 5: return PostFlopDecision(botID);
            case 6: return PostFlopDecision(botID);
            case 7: return PostFlopDecision(botID);
            default: return null;
        }
    }
    private string FindValueInChart()
    {
        if (Array.IndexOf(ranks, hand[0].Rank) > Array.IndexOf(ranks, hand[1].Rank))
        {
            ranksString = hand[0].Rank + hand[1].Rank;
        }
        else
        {
            ranksString = hand[1].Rank + hand[0].Rank;
        }

        handChart.LoadHandChart(dealer.activePlayersLeft);
        if (handChart.handChart.ContainsKey(ranksString))
        {
            return handChart.handChart[ranksString];
        }
        else return null;
    }
    private string PreFlopDecision(int botID)
    {
        System.Random rand = new System.Random();
        int randomValue = rand.Next(100);

        string action = FindValueInChart();
        if (action == null) { Debug.LogError("Failed to find value in JSON file."); }

        string[] percentages = action.Split(' ');
        int raise = int.Parse(percentages[0]);
        int call = int.Parse(percentages[1]);

        if (currentBet < dealer.currentBet)
        {
            if (!hasRaised)
            {
                switch (randomValue)
                {
                    case int n when n < raise: { return HandlePreFlopRaise(botID); }
                    case int n when n < raise + call: { return HandlePreFlopCall(botID); }
                    default: { Fold(botID); return "Fold"; }
                }
            }
            switch (randomValue)
            {
                case int n when n < raise + call: { return HandlePreFlopCall(botID); }
                default: { Fold(botID); return "Fold"; }
            }
        }
        else
        {
            switch (randomValue)
            {
                case int n when n < raise: { Raise(botID, GetSizing()); hasRaised = true; return "Raise"; }
                default: { Check(botID); return "Check"; }
            }
        }
    }
    private string HandlePreFlopRaise(int botID)
    {
        switch (dealer.currentBet / stack)
        {
            case float n when n > 0.5f: return FrequencyCalculator(botID, 0, 50, 0);
            case float n when n > 0.25f: return FrequencyCalculator(botID, 0, 75, 0);
            case float n when n > 0.1f: return FrequencyCalculator(botID, 50, 50, 0);
            case float n when n > 0.05f: return FrequencyCalculator(botID, 90, 10, 0);
            default: Raise(botID, GetSizing()); hasRaised = true; return "Raise";
        }
    }
    private string HandlePreFlopCall(int botID)
    {
        switch (dealer.currentBet / stack)
        {
            case float n when n > 0.5f: return FrequencyCalculator(botID, 0, 20, 0);
            case float n when n > 0.25f: return FrequencyCalculator(botID, 0, 50, 0);
            case float n when n > 0.1f: return FrequencyCalculator(botID, 0, 90, 0);
            default: Call(botID); return "Call";
        }
    }
    private string PostFlopDecision(int botID)
    {
        handRank = handEvaluator.EvaluateBestHand(controller.GetRelevantCards(botID));
        drawRank = handEvaluator.EvaluateBestDraw(controller.GetRelevantCards(botID));

        switch ((int)handRank)
        {
            case int n when n > 3: return HandleStrongHand(botID, dealer.currentBet);
            case 0: return CheckFold(botID);
            default: return HandleWeakHand(botID, dealer.currentBet, CheckTable(handRank));
        }
    }
    private string CheckFold(int botID)
    {
        if (dealer.currentBet == currentBet)
        {
            Check(botID);
            return "Check";
        }


        if (drawRank != HandEvaluator.DrawRank.NoDraw)
        {
            if (controller.GetRelevantCards(botID).Count == 7)
            {
                switch (dealer.currentBet / stack)
                {
                    case float n when n > 0.2f: return FrequencyCalculator(botID, 0, 0, 0);
                    case float n when n > 0.1f: return FrequencyCalculator(botID, 0, 25, 0);
                    case float n when n > 0.05f: return FrequencyCalculator(botID, 0, 50, 0);
                    default: Call(botID); return "Call";
                }
            }
            
            if (stack / startingStack < 0.3) return FrequencyCalculator(botID, 0, 20, 0);
            switch (dealer.currentBet / stack)
            {
                case float n when n > 0.5f: return FrequencyCalculator(botID, 0, 5, 0);
                case float n when n > 0.25f: return FrequencyCalculator(botID, 0, 10, 0);
                case float n when n > 0.1f: return FrequencyCalculator(botID, 0, 30, 0);
                case float n when n > 0.05f: return FrequencyCalculator(botID, 0, 50, 0);
                default: Call(botID); return "Call";
            }
        }
        if (stack / startingStack < 0.3) return FrequencyCalculator(botID, 0, 0, 0);
        switch (dealer.currentBet / stack)
        {
            case float n when n > 0.5f: return FrequencyCalculator(botID, 0, 0, 0);
            case float n when n > 0.25f: return FrequencyCalculator(botID, 0, 5, 0);
            case float n when n > 0.1f: return FrequencyCalculator(botID, 0, 10, 0);
            case float n when n > 0.05f: return FrequencyCalculator(botID, 0, 20, 0);
            default: Call(botID); return "Call";
        }
    }
    private string HandleStrongHand(int botID, float bet)
    {
        if (currentBet < bet)
        {
            if (hasRaised)
            {
                switch (bet / stack)
                {
                    case float n when n > 0.5f: return FrequencyCalculator(botID, 0, 95, 0);
                    case float n when n > 0.25f: return FrequencyCalculator(botID, 50, 50, 0);
                    case float n when n > 0.1f: return FrequencyCalculator(botID, 90, 10, 0);
                    default: Raise(botID, GetSizing()); hasRaised = true; return "Raise";
                }
            }
            switch (bet / stack)
            {
                case float n when n > 0.5f: return FrequencyCalculator(botID, 80, 19, 0);
                case float n when n > 0.1f: return FrequencyCalculator(botID, 90, 10, 0);
                default: Raise(botID, GetSizing()); hasRaised = true; return "Raise";
            }
        }
        else
        {
            Bet(botID, GetSizing());
            hasRaised = true;
            return "Bet";
        }
    }
    private string HandleWeakHand(int botID, float bet, bool combinationOnTable)
    {
        if ((int)handRank == 3)
        {
            return HandleMediumHand(botID, bet, combinationOnTable);
        }
        else
        {
            return HandleLowHand(botID, bet, combinationOnTable);
        }
    }
    private string HandleMediumHand(int botID, float bet, bool combinationOnTable)
    {
        if (currentBet < bet)
        {
            if (combinationOnTable) { Fold(botID); return "Fold"; }
            if (hasRaised)
            {
                if (stack / startingStack < 0.3) return FrequencyCalculator(botID, 10, 90, 0);
                switch (bet / stack)
                {
                    case float n when n > 0.5f: return FrequencyCalculator(botID, 0, 80, 0);
                    case float n when n > 0.25f: return FrequencyCalculator(botID, 10, 85, 0);
                    case float n when n > 0.1f: return FrequencyCalculator(botID, 10, 90, 0);
                    default: Raise(botID, GetSizing()); hasRaised = true; return "Raise";
                }
            }
            if (stack / startingStack < 0.3) return FrequencyCalculator(botID, 50, 50, 0);
            switch (bet / stack)
            {
                case float n when n > 0.5f: return FrequencyCalculator(botID, 0, 90, 0);
                case float n when n > 0.25f: return FrequencyCalculator(botID, 30, 70, 0);
                case float n when n > 0.1f: return FrequencyCalculator(botID, 50, 50, 0);
                default: Raise(botID, GetSizing()); hasRaised = true; return "Raise";
            }
        }
        else
        {
            if (combinationOnTable) { Check(botID); return "Check"; }
            Bet(botID, GetSizing());
            hasRaised = true;
            return "Bet";
        }
    }
    private string HandleLowHand(int botID, float bet, bool combinationOnTable)
    {
        if (currentBet < bet)
        {
            if (combinationOnTable) { Fold(botID); return "Fold"; }
            if (hasRaised)
            {
                if (stack / startingStack < 0.1) return FrequencyCalculator(botID, 0, 70, 0);
                switch (bet / stack)
                {
                    case float n when n > 0.25f: return FrequencyCalculator(botID, 0, 30, 0);
                    case float n when n > 0.1f: return FrequencyCalculator(botID, 5, 90, 0);
                    default: Raise(botID, GetSizing()); hasRaised = true; return "Raise";
                }
            }

            if (stack / startingStack < 0.1) return FrequencyCalculator(botID, 50, 50, 0);
            switch (bet / stack)
            {
                case float n when n > 0.25f: return FrequencyCalculator(botID, 10, 40, 0);
                case float n when n > 0.1f: return FrequencyCalculator(botID, 10, 85, 0);
                default: Raise(botID, GetSizing()); hasRaised = true; return "Raise";
            };
        }
        else
        {
            if (combinationOnTable) { Check(botID); return "Check"; }
            Bet(botID, GetSizing());
            hasRaised = true;
            return "Bet";
        }
    }
    private string FrequencyCalculator(int botID, float raise, float call, float check)
    {
        System.Random rand = new System.Random();
        int randomValue = rand.Next(100);

        switch (randomValue)
        {
            case int n when n < raise: Raise(botID, GetSizing()); hasRaised = true; return "Raise";
            case int n when n < raise + call: Call(botID); return "Call";
            case int n when n < raise + call + check: Check(botID); return "Check";
            default: Fold(botID); return "Fold";
        }
    }
    private bool CheckTable(HandEvaluator.HandRank handRank)
    {
        if (handRank == HandEvaluator.HandRank.TwoPair)
        {
            var rankGroups = dealer.communityCards.GroupBy(card => card.Rank);
            return rankGroups.Count(group => group.Count() == 2) == 2;
        }
        if (handRank == HandEvaluator.HandRank.OnePair)
        {
            var rankGroups = dealer.communityCards.GroupBy(card => card.Rank);
            return rankGroups.Any(group => group.Count() == 2);
        }
        if (handRank == HandEvaluator.HandRank.ThreeOfAKind)
        {
            var rankGroups = dealer.communityCards.GroupBy(card => card.Rank);
            return rankGroups.Any(group => group.Count() == 3);
        }
        return false;
    }
    private int GetSizing()
    {
        int size;
        if(dealer.currentBet == 0)
        {
            switch (dealer.totalPot/stack)
            {
                case float n when n > 1: size = (int)Mathf.Floor(dealer.totalPot * 0.2f); break;
                case float n when n > 0.5: size = (int)Mathf.Floor(dealer.totalPot * 0.25f); break;
                case float n when n > 0.25: size = (int)Mathf.Floor(dealer.totalPot * 0.5f); break;
                case float n when n > 0.1: size = (int)Mathf.Floor(dealer.totalPot); break;
                default: size = (int)Mathf.Floor(dealer.totalPot * 1.5f); break;
            }
        }
        else
        {
            switch (dealer.currentBet/stack)
            {
                case float n when n > 0.05: size = (int)Mathf.Floor(dealer.currentBet * 2); break;
                default: size = (int)Mathf.Floor(dealer.currentBet * 3); break;
            }
        }
        if (size < 1) size = 1;
        return (int)Mathf.Min(size, stack);
    }
}
