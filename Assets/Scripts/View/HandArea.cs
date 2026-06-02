using System.Collections.Generic;
using UnityEngine;

public class HandArea : MonoBehaviour
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
        cards.Remove(card);
    }

    public void SetAllSelectable(bool selectable)
    {
        foreach (var c in cards)
        {
            c.IsSelectable = selectable;
            c.SetHighlight(selectable);
        }
    }

    public void SetSelectableNone()
    {
        SetAllSelectable(false);
    }
}