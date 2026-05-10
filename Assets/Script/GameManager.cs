using UnityEngine;



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
    public int turnCount = 1;


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
        Debug.Log($"턴 변경, 현재: {currentState}");
    }


    //내 턴인지 확인
    public bool IsMyTurn(int pieceOwnerID)
    {
        if (hasActed) return false;

        if (currentState == GameState.Player1 && pieceOwnerID == 1) return true;
        if (currentState == GameState.Player2 && pieceOwnerID == 2) return true;

        return false;
    }
}
