using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AnalyticsEventService : MonoBehaviour
{
    private const string _eventDataFileName = "/AnalyticsEventsData.json";
    private const string _analyticsLink = "";
    private const int _cooldownBeforeSend = 5000;

    private bool _isAnalyticSendingInProgress;
    private Button button;

    private CancellationTokenSource _cancellationTokenSource;
    private List<AnalyticsEvent> _analyticsEvents;

    private void Start()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _analyticsEvents = new List<AnalyticsEvent>();
        _isAnalyticSendingInProgress = false;

        LoadAnalyticsDataFormPrevousSession();
    }

    private void LoadAnalyticsDataFormPrevousSession()
    {
        try
        {
            var savedEventsData = JsonConvert.DeserializeObject<AnalyticsEvent[]>(
                File.ReadAllText(Application.streamingAssetsPath + _eventDataFileName));
            _analyticsEvents.AddRange(savedEventsData);
            File.Delete(Application.streamingAssetsPath + _eventDataFileName);

        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    public void TrackEvent(string type, string data)
    {
        _analyticsEvents.Add(new AnalyticsEvent(type, data));

        if (!_isAnalyticSendingInProgress)
        {
            SendEventsToServer();
        }
    }

    private async void SendEventsToServer()
    {
        var token = _cancellationTokenSource.Token;
        _isAnalyticSendingInProgress = true;
        UnityWebRequest.Result requestResult;
        int countOfSendedEvents;

        do
        {
            await UniTask.Delay(_cooldownBeforeSend, cancellationToken: token);
            countOfSendedEvents = _analyticsEvents.Count;
            var analiticsDataJson = JsonConvert.SerializeObject(_analyticsEvents);
            var request = UnityWebRequest.Post(_analyticsLink, analiticsDataJson);
            await request.SendWebRequest();
            token.ThrowIfCancellationRequested();

            requestResult = request.result;
            request.Dispose();
        } while (requestResult == UnityWebRequest.Result.Success
        && !token.IsCancellationRequested);

        _isAnalyticSendingInProgress = false;
        if (requestResult == UnityWebRequest.Result.Success)
        {
            _analyticsEvents.RemoveAt(countOfSendedEvents - 1);
        }
    }

    private void OnDestroy()
    {
        _cancellationTokenSource?.Cancel();
        if (_analyticsEvents != null && _analyticsEvents.Count != 0)
        {
            File.WriteAllText(Application.streamingAssetsPath + _eventDataFileName,
                JsonConvert.SerializeObject(_analyticsEvents));
        }
    }
}
