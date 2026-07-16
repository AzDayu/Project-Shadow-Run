using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class PlayerInventoryTooltipController : MonoBehaviour
{
    [Header("Tooltip")]
    [SerializeField] private GameObject TooltipPanel;
    [SerializeField] private RectTransform TooltipRect;
    [SerializeField] private TMP_Text TextItemName;
    [SerializeField] private TMP_Text TextItemDescription;
    [SerializeField] private Vector2 TooltipOffset = new Vector2(20f, -20f);

    private Canvas _rootCanvas;
    private RectTransform _canvasRect;

    private void Awake()
    {
        _rootCanvas = GetComponentInParent<Canvas>();

        if (_rootCanvas != null)
        {
            _canvasRect = _rootCanvas.transform as RectTransform;
        }

        Hide();
    }

    private void Update()
    {
        if (TooltipPanel == null || !TooltipPanel.activeSelf || Mouse.current == null)
            return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();

        SetPositionClamped(mousePosition + TooltipOffset);
    }

    public void Show(ItemModel itemModel, Vector2 screenPosition)
    {
        if (itemModel == null)
            return;

        if (DataManager.Instance == null)
            return;

        ItemData itemData = DataManager.Instance.GetItemData(itemModel.ItemId);

        if (itemData == null)
            return;

        if (TooltipPanel == null || TooltipRect == null)
            return;

        if (TextItemName != null)
            TextItemName.text = itemData.Name;

        if (TextItemDescription != null)
            TextItemDescription.text = itemData.ItemDescription;

        TooltipPanel.SetActive(true);

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(TooltipRect);

        SetPositionClamped(screenPosition + TooltipOffset);
    }

    public void Hide()
    {
        if (TooltipPanel != null)
            TooltipPanel.SetActive(false);
    }

    private void SetPositionClamped(
        Vector2 screenPosition)
    {
        if (_rootCanvas == null || _canvasRect == null || TooltipRect == null)
            return;

        Camera uiCamera = null;

        if (_rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCamera = _rootCanvas.worldCamera;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRect,
            screenPosition,
            uiCamera,
            out Vector2 localPosition
        );

        Rect canvasArea = _canvasRect.rect;
        Rect tooltipArea = TooltipRect.rect;

        Vector2 canvasPivot = _canvasRect.pivot;
        Vector2 tooltipPivot = TooltipRect.pivot;

        float canvasMinX = -canvasArea.width * canvasPivot.x;
        float canvasMaxX = canvasArea.width * (1f - canvasPivot.x);
        float canvasMinY = -canvasArea.height * canvasPivot.y;
        float canvasMaxY = canvasArea.height * (1f - canvasPivot.y);
        float minX = canvasMinX + tooltipArea.width * tooltipPivot.x;
        float maxX = canvasMaxX - tooltipArea.width * (1f - tooltipPivot.x);
        float minY = canvasMinY + tooltipArea.height * tooltipPivot.y;
        float maxY = canvasMaxY - tooltipArea.height * (1f - tooltipPivot.y);

        localPosition.x = Mathf.Clamp(localPosition.x, minX, maxX);
        localPosition.y = Mathf.Clamp(localPosition.y, minY, maxY);

        TooltipRect.anchoredPosition = localPosition;
    }
}
