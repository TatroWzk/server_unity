using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ApiClient : MonoBehaviour
{
    [Tooltip("Set this in the inspector (e.g. http://192.168.42.21:5005/server)")]
    public string baseUrl = "http://localhost:5005/server";

    // Kept because GameManager subscribes to this event.
    public event Action<int, ServerData> OnDataReceived;

    [Header("Auto Send Settings")]
    public string autoGameId = "defaultGame";
    public string autoPlayerId = "0";
    public Transform playerTransform; // optional; assign for stable behavior

    private Coroutine _autoSendCoroutine;

    private void Start()
    {
        _autoSendCoroutine = StartCoroutine(AutoSendRoutine());
    }

    private void OnDisable()
    {
        if (_autoSendCoroutine != null)
        {
            StopCoroutine(_autoSendCoroutine);
            _autoSendCoroutine = null;
        }
    }

    // AUTO SEND: build ServerData and call PostPlayerData each second.
    // We don't duplicate POST logic here; we call the existing PostPlayerData coroutine.
    private IEnumerator AutoSendRoutine()
    {
        while (true)
        {
            // Get current position
            Vector3 position = Vector3.zero;

            if (playerTransform != null)
            {
                position = playerTransform.position;
            }
            else
            {
                PlayerController pc = FindAnyObjectByType<PlayerController>();
                if (pc != null)
                {
                    position = pc.GetPosition();
                }
            }

            // Build ServerData (type defined elsewhere)
            ServerData data = new ServerData
            {
                posX = position.x,
                posY = position.y,
                posZ = position.z
            };

            // Fire-and-forget: start the POST coroutine but do not yield it here.
            StartCoroutine(PostPlayerData(autoGameId, autoPlayerId, data));

            // Minimal log indicating a send was requested (details and result are handled in PostPlayerData).
            Debug.Log($"Auto POST requested to {baseUrl}/{autoGameId}/{autoPlayerId}");

            // Wait 1 second and repeat
            yield return new WaitForSeconds(0.0000000000000000000000000000000000000000000000000000000000000000000000000000000001f);
        }
    }

    // --- existing functions kept intact because other scripts rely on them ---
    public IEnumerator GetPlayerData(string gameId, string playerId)
    {
        string url = $"{baseUrl}/{gameId}/{playerId}";

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"GET Error: {webRequest.error}");
                Debug.LogError($"Response: {webRequest.downloadHandler.text}");
            }
            else
            {
                Debug.Log($"GET Success: {webRequest.downloadHandler.text}");
                var data = JsonUtility.FromJson<ServerData>(webRequest.downloadHandler.text);
                OnDataReceived?.Invoke(Convert.ToInt16(playerId), data);
            }
        }
    }

    // POST request (kept as the single source of truth for POST behavior)
    public IEnumerator PostPlayerData(string gameId, string playerId, ServerData data)
    {
        string url = $"{baseUrl}/{gameId}/{playerId}";
        string jsonData = JsonUtility.ToJson(data);

        using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"POST Error: {webRequest.error}");
                Debug.LogError($"Response: {webRequest.downloadHandler.text}");
            }
            else
            {
                Debug.Log($"POST Success: {webRequest.downloadHandler.text}");
            }
        }
    }
}