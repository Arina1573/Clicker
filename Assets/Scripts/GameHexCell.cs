using UnityEngine;
using TMPro;
using HexMapTools;

public class GameHexCell : MonoBehaviour
{
    [Header("Game Data")]
    public int value = 10;
    public int ownerId = 0;
    public HexCoordinates coordinates;
    
    [Header("Components")]
    public SpriteRenderer spriteRenderer;
    public TextMeshPro valueText;

    public void Initialize(HexCoordinates coords)
    {
        coordinates = coords;
    }
    
    void Start()
    {
        UpdateVisuals();
    }
    
    void OnMouseDown()
    {
        if (GameManager.Instance != null && GameManager.Instance.gameActive)
        {
            bool isOwnCell = (ownerId == GameManager.Instance.GetCurrentPlayerId());
            GameManager.Instance.OnCellClicked(this, isOwnCell);
        }
    }
    
    public void ChangeValue(int amount)
    {
        value += amount;
        value = Mathf.Max(value, 1);
        UpdateVisuals();
    }
    
    public void Attack(int damage, int attackerId)
    {
        if (ownerId == 0 || ownerId != attackerId)
        {
            value -= damage;
            
            if (value <= 0)
            {
                value = 1;
                ownerId = attackerId;
            }
            UpdateVisuals();
        }
    }
    
    public void UpdateVisuals()
    {
        if (valueText != null)
            valueText.text = value.ToString();
        UpdateColor();
    }
    
    void UpdateColor()
    {
        if (spriteRenderer != null)
        {
            Color testColor = new Color(0.75f, 0.75f, 0.77f);
            if (ownerId == 1)
                testColor = Color.blue;
            else if (ownerId == 2)
                testColor = Color.red;
            else if (ownerId == 3)
                testColor = Color.green;
            else if (ownerId == 4)
                testColor = Color.yellow;
            
            spriteRenderer.material.color = testColor;
        }
    }
}