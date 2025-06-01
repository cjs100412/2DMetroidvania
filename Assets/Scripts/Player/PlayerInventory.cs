using UnityEngine;
using System;

public class PlayerInventory : MonoBehaviour
{
    private int coinCount = 0;
    public int CoinCount { get { return coinCount; } set { coinCount = value; } }

    public Action<int> OnCoinChanged;
    // 코인 추가 (몬스터 처치, 아이템 줍기 등)
    public void AddCoins(int amount)
    {
        if (amount <= 0) return;
        coinCount += amount;
        Debug.Log($"Coins +{amount} → Total: {coinCount}");
        OnCoinChanged?.Invoke(coinCount);

        // GameManager와 동기화
        if (GameManager.I != null)
            GameManager.I.SetCoins(coinCount);
    }

    // 코인 소비 (상점 구매 등)
    public bool SpendCoins(int amount)
    {
        if (amount <= 0) return true;
        if (coinCount < amount)
        {
            Debug.LogWarning("Not enough coins!");
            return false;
        }
        coinCount -= amount;
        Debug.Log($"Coins -{amount} → Total: {coinCount}");
        OnCoinChanged?.Invoke(coinCount);

        // GameManager와 동기화
        if (GameManager.I != null)
            GameManager.I.SetCoins(coinCount);

        return true;
    }
}