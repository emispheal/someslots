using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BetMeter : GenericMeter
{
    int[] betMultiples = MainConfig.BET_MULTIPLES;
    int curBetMultiIdx = 0;

    public MainController mainController;

    public override void Start()
    {
        currentValue = betMultiples[curBetMultiIdx];
        base.Start();
        Debug.Log("Current Bet: $" + currentValue.ToString());
    }
    
    public void IncrementBet()
    {
        curBetMultiIdx = Mathf.Clamp(curBetMultiIdx + 1, 0, betMultiples.Length - 1);
        currentValue = betMultiples[curBetMultiIdx];
        SetString();
        mainController.SyncBet(currentValue);    
    }

    public void DecrementBet()
    {
        curBetMultiIdx = Mathf.Clamp(curBetMultiIdx - 1, 0, betMultiples.Length - 1);
        currentValue = betMultiples[curBetMultiIdx];
        SetString();
        mainController.SyncBet(currentValue);
    }

    public override void SetString()
    {
        meterString = "Current Bet: $" + currentValue.ToString();
        textObject.text = meterString;
    }
}
