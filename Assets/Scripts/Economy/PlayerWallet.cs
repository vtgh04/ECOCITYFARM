// --- File: PlayerWallet.cs (Final Version) ---

using UnityEngine;
using System;

public class PlayerWallet : MonoBehaviour
{
    public static PlayerWallet Instance { get; private set; }
    public event Action<int> OnMoneyChanged;

    [SerializeField] private int startingMoney = 100;
    private int _currentMoney;
    public int CurrentMoney => _currentMoney;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        _currentMoney = startingMoney;
        // Announce the starting money to all listeners
        OnMoneyChanged?.Invoke(_currentMoney);
    }

    public void AddMoney(int amount)
    {
        _currentMoney += amount;
        OnMoneyChanged?.Invoke(_currentMoney);
    }

    public bool SpendMoney(int amount)
    {
        if (amount <= _currentMoney)
        {
            _currentMoney -= amount;
            OnMoneyChanged?.Invoke(_currentMoney);
            return true;
        }
        return false;
    }
    public void SetMoney(int amount)
{
    _currentMoney = amount;
    OnMoneyChanged?.Invoke(_currentMoney);
}
}