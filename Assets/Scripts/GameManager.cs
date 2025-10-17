using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private ApiClient api;
    [SerializeField] private List<PlayerController> players;
    public string gameId;

    public void Start()
    {
        api.OnDataReceived += OnDataReceived;
    }

    public void GetPlayerData(int playerId)
    {
        StartCoroutine(api.GetPlayerData(gameId, playerId.ToString()));
    }

    public void OnDataReceived(int playerId, ServerData data)
    {
        Vector3 position = new Vector3(data.posX, data.posY, data.posZ);
        players[playerId].MovePlayer(position);
    }

    public void SendPlayerPosition(int playerId)
    {
        Vector3 position = players[playerId].GetPosition();
        ServerData data = new ServerData
        {
            posX = position.x,
            posY = position.y,
            posZ = position.z
        };
        StartCoroutine(api.PostPlayerData(gameId, playerId.ToString(), data));
    }
}
