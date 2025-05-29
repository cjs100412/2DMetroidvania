using UnityEngine;
using System;

public class PlayerInventory : MonoBehaviour
{
    // 현재 소지 코인 수
    private int coinCount = 0;
    public int CoinCount { get { return coinCount; } set { coinCount = value; } }

    // 코인 수 변경 시 외부에서 구독할 수 있는 이벤트
    public event Action<int> OnCoinChanged;

    /// <summary>
    /// 코인 추가 (몬스터 처치, 아이템 줍기 등)
    /// </summary>
    public void AddCoins(int amount)
    {
        if (amount <= 0) return;
        coinCount += amount;
        Debug.Log($"Coins +{amount} → Total: {coinCount}");
        OnCoinChanged?.Invoke(coinCount);
    }

    /// <summary>
    /// 코인 소비 (상점 구매 등). 충분치 않으면 false 반환
    /// </summary>
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
        return true;
    }
}
