﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    [SerializeField] private GridLayoutGroup grid;
    [SerializeField] private InventorySlot slotTemplate;

    private InventorySlot[,] slots;
    private InventorySlot hoveredSlot;
    private const int Columns = 5;
    private const int Rows = 5;

    public Item DraggedItem { get; set; }

    private void Awake()
    {
        foreach (Transform child in grid.transform)
        {
            Destroy(child.gameObject);
        }

        slots = new InventorySlot[Columns, Rows];
        slotTemplate.gameObject.SetActive(true);
        for (var y = 0; y < Rows; y++)
        {
            for (var x = 0; x < Columns; x++)
            {
                var slot = Instantiate(slotTemplate, grid.transform);
                slot.Position = new Vector2Int(x, y);
                slot.PointerEnter += () => Slot_PointerEnter(slot);
                slot.PointerDown += Slot_PointerDown;
                slots[x, y] = slot;
            }
        }
        slotTemplate.gameObject.SetActive(false);
    }

    private void Slot_PointerEnter(InventorySlot slot)
    {
        hoveredSlot = slot;
        foreach (var inventorySlot in slots)
        {
            inventorySlot.PointerExit -= Slot_PointerExit;
        }
        slot.PointerExit += Slot_PointerExit;
        if (DraggedItem != null)
        {
            HighlightDraggedItemSlots();
        }
        else if (slot.Item != null)
        {
            HighlightInventoryItem();
        }
    }

    private void HighlightInventoryItem()
    {
        var itemSlots = FindSlots(hoveredSlot.Item);
        foreach (var slot in itemSlots)
        {
            slot.HighlightHover();
        }
    }

    private void HighlightDraggedItemSlots()
    {
        ResetHighlight();
        var hoveredSlots = HoveredSlots.ToList();
        var isInvalidSelection = hoveredSlots.Any(s => s == null);
        hoveredSlots = hoveredSlots
            .Where(s => s != null)
            .ToList();
        foreach (var slot in hoveredSlots)
        {
            if (isInvalidSelection)
            {
                slot.HighlightInvalid();
            }
            else if (slot.Item != null)
            {
                slot.HighlightInvalid();
            }
            else
            {
                slot.HighlightHover();
            }
        }
    }

    private void ResetHighlight()
    {
        foreach (var slot in slots)
        {
            slot.ResetHighlight();
        }
    }

    private void Slot_PointerExit()
    {
        hoveredSlot = null;
        ResetHighlight();
    }

    private void Slot_PointerDown()
    {
        if (DraggedItem != null) return;
        if (hoveredSlot.Item == null) return;
        DraggedItem = hoveredSlot.Item;
        hoveredSlot.ItemView.StartDragging();
        foreach (var slot in FindSlots(hoveredSlot.Item))
        {
            slot.Item = null;
            slot.ItemView = null;
        }
        HighlightDraggedItemSlots();
    }

    public bool AcceptDraggedItem(InventoryItemView itemView)
    {
        if (!IsDraggedItemAcceptable)
        {
            DraggedItem = null;
            ResetHighlight();
            return false;
        }
        itemView.transform.position = hoveredSlot.transform.position;
        foreach (var slot in HoveredSlots)
        {
            slot.Item = DraggedItem;
            slot.ItemView = itemView;
        }

        DraggedItem = null;
        return true;
    }

    private bool IsDraggedItemAcceptable
    {
        get { return hoveredSlot != null && HoveredSlots.All(s => s != null && s.Item == null); }
    }

    private IEnumerable<InventorySlot> FindSlots(Item item)
    {
        return AllSlots.Where(s => s.Item == item);
    }

    private IEnumerable<InventorySlot> AllSlots
    {
        get { return slots.Cast<InventorySlot>(); }
    }

    private IEnumerable<InventorySlot> HoveredSlots
    {
        get
        {
            return DraggedItem.OccupiedSlotPositions
                .Select(p => GetSlot(p, hoveredSlot.Position))
                .ToList();
        }
    }

    private InventorySlot GetSlot(Vector2Int position, Vector2Int origin)
    {
        var finalPosition = origin + position;
        return AllSlots.FirstOrDefault(s => s.Position.Equals(finalPosition));
    }
}