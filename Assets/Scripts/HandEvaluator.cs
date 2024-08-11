using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HandEvaluator : MonoBehaviour
{
    private static readonly string[] suits = { "H", "D", "C", "S" };
    private static readonly string[] ranks = { "2", "3", "4", "5", "6", "7", "8", "9", "T", "J", "Q", "K", "A" };

    public enum HandRank
    {
        HighCard = 0,
        OnePair,
        TwoPair,
        ThreeOfAKind,
        Straight,
        Flush,
        FullHouse,
        FourOfAKind,
        StraightFlush,
        RoyalFlush
    }
    public enum DrawRank
    {
        NoDraw,
        StraightDraw,
        FlushDraw
    }
    public static IEnumerable<IEnumerable<T>> GetCombinations<T>(IEnumerable<T> list, int length)
    {
        if (length == 0) return new List<List<T>> { new List<T>() };
        return list.SelectMany((element, index) =>
            GetCombinations(list.Skip(index + 1), length - 1).Select(c => (new List<T> { element }).Concat(c)));
    }
    public HandRank EvaluateBestHand(List<Card> hand)
    {
        var combinations = GetCombinations(hand, 5);
        HandRank bestHandRank = HandRank.HighCard;

        foreach (var combination in combinations)
        {
            HandRank currentHandRank = EvaluateHand(combination.ToList());
            if (currentHandRank > bestHandRank)
            {
                bestHandRank = currentHandRank;
            }
        }

        return bestHandRank;
    }
    public List<Card> GetBestHand(List<Card> hand)
    {
        var combinations = GetCombinations(hand, 5).ToList();
        HandRank bestHandRank = HandRank.HighCard;
        int bestHandIndex = 0;

        for (int i = 0; i < combinations.Count(); i++)
        {
            List<Card> list = combinations[i].ToList();
            HandRank currentHandRank = EvaluateHand(list);
            if (currentHandRank > bestHandRank)
            {
                bestHandRank = currentHandRank;
                bestHandIndex = i;
            }
            if(currentHandRank == bestHandRank)
            {
                List<Card> list1 = combinations[bestHandIndex].ToList();
                List<Card> list2 = combinations[i].ToList();
                if (CheckHigherCards(list1, list2) == 1)
                {
                    bestHandIndex = i;
                }
            }
        }
        List<Card> bestHand = combinations[bestHandIndex].ToList();
        return bestHand;
    }
    public DrawRank EvaluateBestDraw(List<Card> hand)
    {
        var combinations = GetCombinations(hand, 5);
        DrawRank bestDrawRank = DrawRank.NoDraw;

        foreach (var combination in combinations)
        {
            DrawRank currentDrawRank = EvaluateDraw(combination.ToList());
            if (currentDrawRank > bestDrawRank)
            {
                bestDrawRank = currentDrawRank;
            }
        }
        return bestDrawRank;
    }
    public HandRank EvaluateHand(List<Card> hand)
    {
        if (IsRoyalFlush(hand)) return HandRank.RoyalFlush;
        if (IsStraightFlush(hand)) return HandRank.StraightFlush;
        if (IsFourOfAKind(hand)) return HandRank.FourOfAKind;
        if (IsFullHouse(hand)) return HandRank.FullHouse;
        if (IsFlush(hand)) return HandRank.Flush;
        if (IsStraight(hand)) return HandRank.Straight;
        if (IsThreeOfAKind(hand)) return HandRank.ThreeOfAKind;
        if (IsTwoPair(hand)) return HandRank.TwoPair;
        if (IsOnePair(hand)) return HandRank.OnePair;
        return HandRank.HighCard;
    }
    public DrawRank EvaluateDraw(List<Card> hand)
    {
        if(IsStraightDraw(hand)) return DrawRank.StraightDraw;
        if(IsFlushDraw(hand)) return DrawRank.FlushDraw;
        return DrawRank.NoDraw;
    }
    private bool IsRoyalFlush(List<Card> hand)
    {
        return IsStraightFlush(hand) && hand.All(card => Array.IndexOf(ranks, card.Rank) >= Array.IndexOf(ranks, "10"));
    }
    private bool IsStraightFlush(List<Card> hand)
    {
        return IsFlush(hand) && IsStraight(hand);
    }
    private bool IsFourOfAKind(List<Card> hand)
    {
        var rankGroups = hand.GroupBy(card => card.Rank);
        return rankGroups.Any(group => group.Count() == 4);
    }
    private bool IsFullHouse(List<Card> hand)
    {
        var rankGroups = hand.GroupBy(card => card.Rank);
        return rankGroups.Any(group => group.Count() == 3) && rankGroups.Any(group => group.Count() == 2);
    }
    private bool IsFlush(List<Card> hand)
    {
        return hand.GroupBy(card => card.Suit).Count() == 1;
    }
    private bool IsFlushDraw(List<Card> hand)
    {
        var suitGroups = hand.GroupBy(card => card.Suit);
        return suitGroups.Any(group => group.Count() == 4);
    }
    private bool IsStraight(List<Card> hand)
    {
        var sortedRanks = hand.Select(card => card.Rank).OrderBy(rank => Array.IndexOf(ranks, rank)).ToList();
        if (sortedRanks.Last() == "A" && sortedRanks.First() == "2")
        {
            // Handle A-2-3-4-5 as a valid straight (wheel)
            sortedRanks.Remove(sortedRanks.Last());
        }
        for (int i = 1; i < sortedRanks.Count; i++)
        {
            if (Array.IndexOf(ranks, sortedRanks[i]) != Array.IndexOf(ranks, sortedRanks[i - 1]) + 1)
            {
                return false;
            }
        }
        return true;
    }
    private bool IsStraightDraw(List<Card> hand)
    {
        var sortedRanks = hand.Select(card => card.Rank).OrderBy(rank => Array.IndexOf(ranks, rank)).ToList();
        if (sortedRanks.Last() == "A" && sortedRanks.First() == "2")
        {
            // Handle A-2-3-4-5 as a valid straight (wheel)
            sortedRanks.Remove(sortedRanks.Last());
        }
        for (int i = 0; i < sortedRanks.Count - 3; i++)
        {
            int consecutiveCount = 1;
            for (int j = i + 1; j < sortedRanks.Count; j++)
            {
                if (Array.IndexOf(ranks, sortedRanks[j]) == Array.IndexOf(ranks, sortedRanks[j - 1]) + 1)
                {
                    consecutiveCount++;
                    if (consecutiveCount == 4)
                    {
                        return true;
                    }
                }
                else if (Array.IndexOf(ranks, sortedRanks[j]) != Array.IndexOf(ranks, sortedRanks[j - 1]))
                {
                    break;
                }
            }
        }
        return false;
    }
    private bool IsThreeOfAKind(List<Card> hand)
    {
        var rankGroups = hand.GroupBy(card => card.Rank);
        return rankGroups.Any(group => group.Count() == 3);
    }
    private bool IsTwoPair(List<Card> hand)
    {
        var rankGroups = hand.GroupBy(card => card.Rank);
        return rankGroups.Count(group => group.Count() == 2) == 2;
    }
    private bool IsOnePair(List<Card> hand)
    {
        var rankGroups = hand.GroupBy(card => card.Rank);
        return rankGroups.Any(group => group.Count() == 2);
    }
    public static int GetRankIndex(string rank)
    {
        return Array.IndexOf(ranks, rank);
    }
    internal int CheckHigherCards(List<Card> cards1, List<Card> cards2)
    {
        var sortedCards1 = cards1.OrderByDescending(card => GetRankIndex(card.Rank)).ToList();
        var sortedCards2 = cards2.OrderByDescending(card => GetRankIndex(card.Rank)).ToList();

        for (int i = 0; i < sortedCards1.Count; i++)
        {
            int rankIndex1 = GetRankIndex(sortedCards1[i].Rank);
            int rankIndex2 = GetRankIndex(sortedCards2[i].Rank);

            if (rankIndex1 > rankIndex2) return -1;
            if (rankIndex1 < rankIndex2) return 1;
        }
        return 0;
    }
}
