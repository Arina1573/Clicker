using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using HexMapTools;

public class SimpleGridCreator : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject hexPrefab;
    
    [Header("GridSise")]
    public int gridWidth = 8;
    public int gridHeight = 6;
    
    [Header("Dist")]
    public float spacing = 1.8f;

    public static Dictionary<HexCoordinates, GameHexCell> cellMap = new Dictionary<HexCoordinates, GameHexCell>();
    
    void Start()
    {
        CreateGrid();
    }
    
    void CreateGrid()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        
        for (int row = 0; row < gridHeight; row++)
        {
            for (int col = 0; col < gridWidth; col++)
            {
                float xPos = col * spacing;
                float yPos = row * spacing * 0.86f; // 0.86 = √3/2
                
                if (row % 2 == 1)
                {
                    xPos += spacing * 0.5f;
                }

                Vector3 position = new Vector3(xPos, yPos, 0);
                
                GameObject hex = Instantiate(hexPrefab, position, Quaternion.identity, transform);
                hex.name = $"Hex_{col}_{row}";

                GameHexCell cell = hex.GetComponent<GameHexCell>();
                if (cell != null)
                {
                    HexCoordinates coordinates = HexCoordinates.FromOffsetCoordinates(col, row);
                    cell.coordinates = coordinates;
                
                    if (cellMap != null)
                    {
                    cellMap[coordinates] = cell;
                    }
                }
            }
        }
        CenterGrid();
    }
    
    void CenterGrid()
    {
        float totalWidth = (gridWidth - 1) * spacing;
        float totalHeight = (gridHeight - 1) * spacing * 0.86f;
        
        transform.position = new Vector3(-totalWidth/2, -totalHeight/2, 0);
    }
}