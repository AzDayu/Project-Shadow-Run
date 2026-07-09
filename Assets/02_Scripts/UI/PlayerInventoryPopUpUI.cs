using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class PlayerInventoryPopUpUI : UIBase
{
    [Header("Slot")]
    [SerializeField] private Transform SlotParent;

    [Header("Tooltip")]
    [SerializeField] private GameObject ItemTooltipPanel;
    [SerializeField] private RectTransform ItemTooltipRect;
    [SerializeField] private TMP_Text TextTooltipName;
    [SerializeField] private TMP_Text TextTooltipDescription;
    [SerializeField] private Vector2 TooltipOffset = new Vector2(20f, -20f);

    [Header("Context Menu")]
    [SerializeField] private GameObject ContextMenuPanel;
    [SerializeField] private RectTransform ContextMenuRect;
    [SerializeField] private Button ButtonUse;
    [SerializeField] private Button ButtonDrop;
    [SerializeField] private Button ButtonRegisterQuickSlot;
    [SerializeField] private Vector2 ContextMenuOffset = new Vector2(8f, -8f);

    private readonly List<PlayerInventorySlotUI> _slotUIList = new();

    private Canvas _rootCanvas;
    private RectTransform _canvasRect;

    private PlayerInventorySlotUI _selectedSlot;
    private PlayerInventorySlotUI _draggingSlot;
    private PlayerInventorySlotUI _contextTargetSlot;

    private void Awake()
    {
        _rootCanvas = GetComponentInParent<Canvas>();
        _canvasRect = _rootCanvas != null ? _rootCanvas.transform as RectTransform : null;

        InitSlots();
        InitContextButtons();

        HideItemTooltip(null);
        CloseContextMenu();
    }

    private void OnEnable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged += RefreshInventory;

        RefreshInventory();

        HideItemTooltip(null);
        CloseContextMenu();
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= RefreshInventory;

        HideItemTooltip(null);
        CloseContextMenu();
    }

    private void Update()
    {
        if (ItemTooltipPanel != null && ItemTooltipPanel.activeSelf)
        {
            SetPanelPositionClamped(ItemTooltipRect, Input.mousePosition + (Vector3)TooltipOffset);
        }

        if (ContextMenuPanel != null && ContextMenuPanel.activeSelf)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (!IsPointerOverObject(ContextMenuPanel))
                    CloseContextMenu();
            }
        }
    }

    private void InitSlots()
    {
        _slotUIList.Clear();

        PlayerInventorySlotUI[] slots =
            SlotParent.GetComponentsInChildren<PlayerInventorySlotUI>(true);

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].Init(i, this);
            _slotUIList.Add(slots[i]);
        }
    }

    private void InitContextButtons()
    {
        if (ButtonUse != null)
        {
            ButtonUse.onClick.RemoveAllListeners();
            ButtonUse.onClick.AddListener(OnClickUse);
        }

        if (ButtonDrop != null)
        {
            ButtonDrop.onClick.RemoveAllListeners();
            ButtonDrop.onClick.AddListener(OnClickDrop);
        }

        if (ButtonRegisterQuickSlot != null)
        {
            ButtonRegisterQuickSlot.onClick.RemoveAllListeners();
            ButtonRegisterQuickSlot.onClick.AddListener(OnClickRegisterQuickSlot);
        }
    }

    private void RefreshInventory()
    {
        if (InventoryManager.Instance == null)
            return;

        IReadOnlyList<ItemStack> itemStacks = InventoryManager.Instance.ItemList;

        for (int i = 0; i < _slotUIList.Count; i++)
        {
            if (i < itemStacks.Count)
                _slotUIList[i].SetItem(itemStacks[i]);
            else
                _slotUIList[i].Clear();
        }

        RefreshSelectedSlot();
    }

    private void RefreshSelectedSlot()
    {
        if (_selectedSlot == null)
            return;

        if (!_selectedSlot.HasItem)
        {
            _selectedSlot.SetSelected(false);
            _selectedSlot = null;
        }
    }

    public void SelectSlot(PlayerInventorySlotUI slot)
    {
        if (slot == null || !slot.HasItem)
            return;

        if (_selectedSlot != null)
            _selectedSlot.SetSelected(false);

        _selectedSlot = slot;
        _selectedSlot.SetSelected(true);

        Debug.Log($"선택 슬롯: {slot.SlotIndex}, 아이템: {slot.ItemStack.Item.ItemName}");
    }

    public void TryUseItem(PlayerInventorySlotUI slot)
    {
        if (slot == null || !slot.HasItem)
            return;

        SelectSlot(slot);

        ItemStack stack = slot.ItemStack;
        ItemData item = stack.Item;

        Debug.Log($"아이템 사용 시도: {item.ItemName}");

        // TODO:
        // if (item.ItemType == ItemType.Consumable)
        // {
        //     실제 아이템 사용 처리
        //     InventoryManager.Instance.TryRemoveItem(item.ItemId, 1);
        // }
    }

    public void OpenContextMenu(PlayerInventorySlotUI slot, Vector2 mousePosition)
    {
        if (slot == null || !slot.HasItem)
            return;

        _contextTargetSlot = slot;

        if (ContextMenuPanel == null || ContextMenuRect == null)
            return;

        ContextMenuPanel.SetActive(true);

        LayoutRebuilder.ForceRebuildLayoutImmediate(ContextMenuRect);

        SetPanelPositionClamped(ContextMenuRect, mousePosition + ContextMenuOffset);

        Debug.Log($"우클릭 메뉴 열기: {slot.ItemStack.Item.ItemName}");
    }

    private void CloseContextMenu()
    {
        _contextTargetSlot = null;

        if (ContextMenuPanel != null)
            ContextMenuPanel.SetActive(false);
    }

    public void ShowItemTooltip(PlayerInventorySlotUI slot, Vector2 mousePosition)
    {
        if (slot == null || !slot.HasItem)
            return;

        if (ItemTooltipPanel == null || ItemTooltipRect == null)
            return;

        ItemStack stack = slot.ItemStack;
        ItemData item = stack.Item;

        if (TextTooltipName != null)
            TextTooltipName.text = item.ItemName;

        if (TextTooltipDescription != null)
            TextTooltipDescription.text = item.ItemDescription;

        ItemTooltipPanel.SetActive(true);

        LayoutRebuilder.ForceRebuildLayoutImmediate(ItemTooltipRect);

        SetPanelPositionClamped(ItemTooltipRect, mousePosition + TooltipOffset);
    }

    public void HideItemTooltip(PlayerInventorySlotUI slot)
    {
        if (ItemTooltipPanel != null)
            ItemTooltipPanel.SetActive(false);
    }

    private void SetPanelPositionClamped(RectTransform panelRect, Vector2 screenPosition)
    {
        if (panelRect == null || _canvasRect == null || _rootCanvas == null)
            return;

        Camera uiCamera = null;

        if (_rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCamera = _rootCanvas.worldCamera;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRect,
            screenPosition,
            uiCamera,
            out Vector2 localPoint
        );

        panelRect.anchoredPosition = localPoint;

        Vector2 panelSize = panelRect.rect.size;
        Vector2 canvasSize = _canvasRect.rect.size;

        Vector2 clamped = panelRect.anchoredPosition;

        float minX = -canvasSize.x * 0.5f;
        float maxX = canvasSize.x * 0.5f - panelSize.x;

        float minY = -canvasSize.y * 0.5f + panelSize.y;
        float maxY = canvasSize.y * 0.5f;

        clamped.x = Mathf.Clamp(clamped.x, minX, maxX);
        clamped.y = Mathf.Clamp(clamped.y, minY, maxY);

        panelRect.anchoredPosition = clamped;
    }

    private bool IsPointerOverObject(GameObject targetObject)
    {
        if (EventSystem.current == null || targetObject == null)
            return false;

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject == targetObject ||
                result.gameObject.transform.IsChildOf(targetObject.transform))
            {
                return true;
            }
        }

        return false;
    }

    public void BeginDragSlot(PlayerInventorySlotUI slot, PointerEventData eventData)
    {
        if (slot == null || !slot.HasItem)
            return;

        _draggingSlot = slot;

        HideItemTooltip(slot);
        CloseContextMenu();

        Debug.Log($"드래그 시작: {slot.ItemStack.Item.ItemName}");
    }

    public void DragSlot(PlayerInventorySlotUI slot, PointerEventData eventData)
    {
        if (_draggingSlot == null)
            return;

        // TODO:
        // 드래그 아이콘을 만들면 여기서 eventData.position을 따라가게 처리.
    }

    public void EndDragSlot(PlayerInventorySlotUI slot, PointerEventData eventData)
    {
        if (_draggingSlot == null)
            return;

        Debug.Log($"드래그 종료: {_draggingSlot.ItemStack.Item.ItemName}");

        // TODO:
        // PlayerQuickSlotUI quickSlot =
        //     eventData.pointerEnter.GetComponentInParent<PlayerQuickSlotUI>();
        //
        // if (quickSlot != null)
        // {
        //     quickSlot.SetItem(_draggingSlot.ItemStack);
        // }

        _draggingSlot = null;
    }

    private void OnClickUse()
    {
        if (_contextTargetSlot == null || !_contextTargetSlot.HasItem)
            return;

        TryUseItem(_contextTargetSlot);
        CloseContextMenu();
    }

    private void OnClickDrop()
    {
        if (_contextTargetSlot == null || !_contextTargetSlot.HasItem)
            return;

        ItemData item = _contextTargetSlot.ItemStack.Item;

        Debug.Log($"아이템 버리기 요청: {item.ItemName}");

        // TODO:
        // InventoryManager.Instance.TryRemoveItem(item.ItemId, 1);
        // 월드 드랍 아이템 생성

        CloseContextMenu();
    }

    private void OnClickRegisterQuickSlot()
    {
        if (_contextTargetSlot == null || !_contextTargetSlot.HasItem)
            return;

        ItemData item = _contextTargetSlot.ItemStack.Item;

        Debug.Log($"퀵슬롯 등록 요청: {item.ItemName}");

        // TODO:
        // QuickSlotManager 또는 QuickSlot UI 쪽으로 등록 요청 연결

        CloseContextMenu();
    }
}