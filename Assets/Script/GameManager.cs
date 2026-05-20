using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class PlayerData
{
    public int ownerID;
    public int targetY; //승리 지점
    public Vector2Int currentGridPos; //현재 위치
    public GameObject playerPiece;
}

public class GameManager: MonoBehaviour
{
    public static GameManager Instance;

    public List<PlayerData> players = new List<PlayerData>();
    public PlayerData player1;
    public PlayerData player2;

    public bool hasActed = false; //행동 여부
    public GameObject selectedPiece;

    public int turnCount = 0;
    public int currentTurnID => turnCount % 2;

    [SerializeField] TMP_Text text;

    void Awake()
    {
        Instance = this;

        //플레이어 기본 데이터
        ResetPlayers();
    }

    void Update()
    {
        if(currentTurnID == 0){
            text.text = $"Turn : {turnCount}\n<color=#FF6B6B>Player {currentTurnID+1}</color>";    
        }else{
            text.text = $"Turn : {turnCount}\n<color=#4CAF50>Player {currentTurnID+1}</color>";
        }
        
    }

    public void ResetPlayers()
    {
        players.Clear();

        player1 = new PlayerData
        {
            ownerID = 0,
            targetY = BoardManager.Instance.boardSize * 2 - 1,
            currentGridPos = new Vector2Int(9, 1),
            playerPiece = GameObject.Find("player_1")
        };

        player2 = new PlayerData
        {
            ownerID = 1,
            targetY = 1,
            currentGridPos = new Vector2Int(11, 19),
            playerPiece = GameObject.Find("player_2")
        };

        players.Add(player1);
        players.Add(player2);
    }


    //턴 변경
    public void EndTurn()
    {
        turnCount++;
        hasActed = false;
        Debug.Log($"턴 수:{turnCount}, 현재: Player{currentTurnID + 1}");
    }


    //내 턴인지 확인
    public bool IsMyTurn(int ownerPlayerID)
    {
        if (hasActed) return false;

        return currentTurnID == ownerPlayerID;
    }


    //게임 끝
    public void EndGame(int winnerID)
    {
        Debug.Log($"게임 승리자: Player{winnerID + 1}");

        //승리자에 따라 다른 게임 결과 창(Scene) 로드
        if(winnerID == 0)
        {
            P1Win();
        }
        else
        {
            P2Win();
        }
    }

    //게임이 끝났는지 판단
    public void IsEnd(int id, Vector2Int currentGrid)
    {
        if (currentGrid.y == players[id].targetY)
        {
            EndGame(id);
        }
    }


    //게임 리셋 : 현재 화면을 재로드
    public void ResetGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    //플레이어 1 우승 결과 씬
    public void P1Win()
    {
        SceneManager.LoadScene("Player1Win");
    }
    //플레이어 2 우승 결과 씬
    public void P2Win()
    {
        SceneManager.LoadScene("Player2Win");
    }
}
