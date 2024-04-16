using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkManager : MonoBehaviour
{
    // Base URL for the HTTP server
    private string baseUrl = "http://localhost:5000/";

    // Method to send request to the server
    public void SendRequest(int grams, string protein, Action<string> onResponseReceived)
    {
        StartCoroutine(PerformRequest(grams, protein, onResponseReceived));
    }

    // Coroutine to perform the request
    private IEnumerator PerformRequest(int grams, string protein, Action<string> onResponseReceived)
    {
        string url = baseUrl + "predict"; // Assuming 'predict' is your endpoint
        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(new { grams = grams, protein = protein }));
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error: " + request.error);
        }
        else
        {
            Debug.Log("Response: " + request.downloadHandler.text);
            onResponseReceived?.Invoke(request.downloadHandler.text);
        }
    }
}
