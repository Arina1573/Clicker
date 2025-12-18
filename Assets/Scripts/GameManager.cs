using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using HexMapTools;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("UI Elements")]
    public TMP_Dropdown playersDropdown;
    public Button startButton;
    public TextMeshProUGUI currentPlayerText;
    public TextMeshProUGUI winnerText;
    public Button restartButton;
    
    [Header("Player Colors")]
    public Color[] playerColors = new Color[4]
    {
        Color.blue,
        Color.red,  
        Color.green,
        Color.yellow
    };
    
    private int totalPlayers = 2;
    private int currentPlayerIndex = 0;
    private List<int> activePlayers = new List<int>();
    private List<int> defeatedPlayers = new List<int>();
    public bool gameActive = false;
    private List<GameHexCell> allCells = new List<GameHexCell>();
    
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    
    void Start()
    {
        if (playersDropdown != null)
        {
            playersDropdown.options.Clear();
            playersDropdown.options.Add(new TMP_Dropdown.OptionData("2 игрока"));
            playersDropdown.options.Add(new TMP_Dropdown.OptionData("3 игрока"));
            playersDropdown.options.Add(new TMP_Dropdown.OptionData("4 игрока"));
            playersDropdown.value = 0;
            playersDropdown.interactable = true;
        }
        
        if (startButton != null)
        {
            startButton.onClick.AddListener(StartGame);
            startButton.interactable = true;
        }
        
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
            restartButton.interactable = false;
        }
        ResetUI();
    }
    
    void ResetUI()
    {
        if (playersDropdown != null)
        {
            playersDropdown.interactable = true;
            playersDropdown.enabled = true;
        }
        
        if (startButton != null)
        {
            startButton.interactable = true;
            startButton.enabled = true;
        }
        
        if (currentPlayerText != null)
        {
            currentPlayerText.text = "Current player: ";
            currentPlayerText.enabled = true;
        }
        
        if (winnerText != null)
        {
            winnerText.text = "Winner: ";
            winnerText.enabled = true;
        }
        
        if (restartButton != null)
        {
            restartButton.interactable = false;
            restartButton.enabled = true;
        }
    }
    
    public void StartGame()
    {
        totalPlayers = playersDropdown.value + 2;
        
        if (playersDropdown != null)
            playersDropdown.interactable = false;
        
        if (startButton != null)
            startButton.interactable = false;
        
        if (currentPlayerText != null)
            currentPlayerText.text = $"Current player: 1";
        
        if (winnerText != null)
            winnerText.text = "Winner: ";
        
        if (restartButton != null)
            restartButton.interactable = false;
        
        InitializePlayers();
        
        allCells.Clear();
        allCells.AddRange(FindObjectsOfType<GameHexCell>());
        
        SetupStartingCells();
        UpdateUI();
        
        gameActive = true;
    }
    
    void InitializePlayers()
    {
        activePlayers.Clear();
        defeatedPlayers.Clear();
        
        for (int i = 1; i <= totalPlayers; i++)
            activePlayers.Add(i);
        
        currentPlayerIndex = 0;
    }
    
    void SetupStartingCells()
    {
        List<GameHexCell> availableCells = new List<GameHexCell>(allCells);
        foreach (var cell in allCells)
        {
            cell.value = Random.Range(5, 16);
            cell.UpdateVisuals();
        }
        foreach (int playerId in activePlayers)
        {
            for (int i = 0; i < 3; i++)
            {
                if (availableCells.Count == 0) break;
                
                int index = Random.Range(0, availableCells.Count);
                GameHexCell cell = availableCells[index];
                cell.ownerId = playerId;
                cell.UpdateVisuals();
                
                availableCells.RemoveAt(index);
            }
        }
    }
    
    public void OnCellClicked(GameHexCell clickedCell, bool isOwnCell)
    {
        if (!gameActive) return;
        
        int currentPlayerId = GetCurrentPlayerId();
        
        if (isOwnCell)
        {
            clickedCell.ChangeValue(1);
            NextTurn();
        }
        else if (CanAttackCell(clickedCell, currentPlayerId))
        {
            clickedCell.Attack(1, currentPlayerId);
            NextTurn();
        }
        UpdateUI();
        CheckGameState();
    }

    bool CanAttackCell(GameHexCell targetCell, int attackerId)
    {
        HexCoordinates targetCoords = targetCell.coordinates;
        if (targetCoords == null) return false;
        HexCoordinates[] neighbors = HexUtility.GetNeighbours(targetCoords);

        foreach (var neighborCoords in neighbors)
        {
            if (SimpleGridCreator.cellMap.TryGetValue(neighborCoords, out GameHexCell neighborCell))
            {
                if (neighborCell.ownerId == attackerId)
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    void NextTurn()
    {
        if (activePlayers.Count == 0) return;
        
        currentPlayerIndex = (currentPlayerIndex + 1) % activePlayers.Count;
        
        int attempts = 0;
        while (defeatedPlayers.Contains(GetCurrentPlayerId()) && attempts < activePlayers.Count * 2)
        {
            currentPlayerIndex = (currentPlayerIndex + 1) % activePlayers.Count;
            attempts++;
        }
    }
    
    void CheckGameState()
    {
        List<int> playersToRemove = new List<int>();
        
        foreach (int playerId in activePlayers)
        {
            if (defeatedPlayers.Contains(playerId)) continue;
            
            bool hasCells = false;
            foreach (var cell in allCells)
                if (cell.ownerId == playerId) { hasCells = true; break; }
            
            if (!hasCells)
            {
                defeatedPlayers.Add(playerId);
                playersToRemove.Add(playerId);
            }
        }
        
        foreach (int playerId in playersToRemove)
            activePlayers.Remove(playerId);
        
        if (activePlayers.Count <= 1)
            GameOver();
    }
    
    void GameOver()
    {
        gameActive = false;
        currentPlayerText.text = "Game over";
        int winnerId = (activePlayers.Count > 0) ? activePlayers[0] : 0;
        
        winnerText.text = (winnerId > 0) ? $"Winner: player {winnerId}" : "Winner: nobody";
        winnerText.color = GetPlayerColor(winnerId);
        
        restartButton.interactable = true;
    }
    
    void UpdateUI()
    {
        if (currentPlayerText != null)
        {
            int currentId = GetCurrentPlayerId();
            currentPlayerText.text = $"Current player: {currentId}";
            currentPlayerText.color = GetPlayerColor(currentId);
        }
    }
    
    public int GetCurrentPlayerId()
    {
        if (activePlayers.Count == 0) return 0;
        if (currentPlayerIndex < 0 || currentPlayerIndex >= activePlayers.Count) 
            return activePlayers[0];
        
        return activePlayers[currentPlayerIndex];
    }
    
    public Color GetPlayerColor(int playerId)
    {
        if (playerId > 0 && playerId <= playerColors.Length)
            return playerColors[playerId - 1];
        return Color.white;
    }
    
    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

}
