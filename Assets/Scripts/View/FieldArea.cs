using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FieldArea : MonoBehaviour
{
    private readonly List<CardView> cards = new List<CardView>();
    public IReadOnlyList<CardView> Cards => cards.AsReadOnly();

    public void AddCard(CardView card)
    {
        cards.Add(card);
        card.transform.SetParent(transform, false);
    }

    public void RemoveCard(CardView card)
    {
        if (cards.Contains(card))
            cards.Remove(card);
    }

    public List<CardView> GetCardsByMonth(int month)
    {
        return cards.Where(c => c.Data.month == month).ToList();
    }

    public void SetSelectableByMonth(int month)
    {
        foreach (var c in cards)
        {
            bool match = c.Data.month == month;
            c.IsSelectable = match;
            c.SetHighlight(match);
        }
    }

    public void SetSelectableNone()
    {
        foreach (var c in cards)
        {
            c.IsSelectable = false;
            c.SetHighlight(false);
        }
    }

    // 同月4枚が場に揃っているかチェック（配りなおし判定用）
    public bool HasAllFourOfAMonth()
    {
        return cards.GroupBy(c => c.Data.month).Any(g => g.Count() >= 4);
    }
}
