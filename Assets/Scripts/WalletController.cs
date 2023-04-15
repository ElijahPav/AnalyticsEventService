using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WalletController : MonoBehaviour
{
    private const string _coinsSpentAnalyticTypeName = "coinsSpent";

    [SerializeField] private Button _spendCoinsButton;
    [SerializeField] private AnalyticsEventService _analyticsEventService;

    private void Start()
    {
        _spendCoinsButton.onClick.AddListener(SpendCoins);
    }

    private void SpendCoins()
    {
        var coindCount = Random.Range(10, 20);
        _analyticsEventService.TrackEvent(_coinsSpentAnalyticTypeName, coindCount.ToString());
    }
}
