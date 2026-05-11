using UnityEngine;
using UnityEngine.SceneManagement;


[System.Serializable]
public class PlayerSet
{
    public GameObject[] wallPrefabs;//플레이어가 사용할 벽
    public GameObject player; //플레이어가 사용할 말
}

public class GameManager: MonoBehaviour
{
    public static GameManager Instance;

    public enum GameState { Player1, Player2, GameOver }
    public GameState currentState;

    public PlayerSet Player1;
    public PlayerSet Player2;

    public bool hasActed = false; //행동 여부
    public GameObject selectedPiece;
    public int turnCount = 0;


    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ChangeTurn(GameState.Player1); //Player1부터 시작
    }


    //플레이어 행동 이후
    public void IsPlayerAction()
    {
        hasActed = true;
        EndTurn();
    }


    //턴 변경
    public void EndTurn()
    {
        if (currentState == GameState.Player1)
        {
            ChangeTurn(GameState.Player2);
        }
        else if (currentState == GameState.Player2)
        {
            ChangeTurn(GameState.Player1);
        }
    }

    public void ChangeTurn(GameState newState)
    {
        currentState = newState;
        hasActed = false;
        turnCount++;
        Debug.Log($"턴 수:{turnCount}, 현재: {currentState}");
    }


    //내 턴인지 확인
    public bool IsMyTurn(int ownerPlayerID)
    {
        if (hasActed) return false;

        if (currentState == GameState.Player1 && ownerPlayerID == 1) return true;
        if (currentState == GameState.Player2 && ownerPlayerID == 2) return true;

        return false;
    }


    //게임 끝
    public void EndGame(int winnerID)
    {
        Debug.Log($"게임 승리자: {winnerID}");

        //승리 UI 및 리셋
        ResetGame();
    }

    //게임이 끝났는지 판단
    public void IsEnd(PlayerPiece player)
    {
        Vector2Int currentGrid = BoardManager.Instance.WorldToGrid(player.transform.position);

        if (player.ownerPlayerID == 1 && currentGrid.y == BoardManager.Instance.dataSize - 1)
        {
            EndGame(1);
        }
        else if (player.ownerPlayerID == 2 && currentGrid.y == 0)
        {
            EndGame(2);
        }
    }


    //게임 리셋 : 현재 화면을 재로드
    public void ResetGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
