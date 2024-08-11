using System.Collections.Generic;

public class Deck
{
    private List<Card> cards;
    private static readonly string[] suits = { "H", "D", "C", "S" };
    private static readonly string[] ranks = { "2", "3", "4", "5", "6", "7", "8", "9", "T", "J", "Q", "K", "A" };
    public Deck()
    {
        cards = new List<Card>();
        foreach (var suit in suits)
        {
            foreach (var rank in ranks)
            {
                cards.Add(new Card(suit, rank));
            }
        }
    }
    public void Shuffle()
    {
        System.Random rng = new System.Random();
        int n = cards.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            Card value = cards[k];
            cards[k] = cards[n];
            cards[n] = value;
        }
    }
    public Card Deal()
    {
        if (cards.Count == 0)
            return null;
        Card card = cards[0];
        cards.RemoveAt(0);
        return card;
    }
}
