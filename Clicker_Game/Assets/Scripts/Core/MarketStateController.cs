using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MarketStateController : MonoBehaviour
{
    [Header("====시장 상태 원본 데이터====")]
    [Tooltip("시장 상태 목록\n약세장 / 보합장 / 강세장")]
    [SerializeField] private MarketStateDataSO[] _marketStateDefinitions;
    
    [Tooltip("게임 시작 시 적용할 기본 시장 상태\n시작 상태는 보합장")]
    [SerializeField] private MarketStateDataSO _startState;
    
    [Header("====시장 상태 변경 설정====")]
    [Tooltip("시장 상태 변경 판정을 수행하는 주기(초)")]
    [SerializeField, Min(1f)] private float _changeInterval = 20f;
    
    public event Action StateChanged;
    
    public string CurrentStateId => _currentState != null ? _currentState.Id : string.Empty;
    public string CurrentStateName => _currentState != null ? _currentState.Name : "보합장";
    public double CurrentIncomeMultiplier => _currentState != null ? _currentState.IncomeMultiplier : 1d;
    
    private MarketStateDataSO _currentState;
    private float _elapsedTime;
    private int _consecutiveSameStateCount;
    
    private void Awake() => InitializeState();
    
    private void Update() => ProcessStateTimer(Time.deltaTime);
    
    private void InitializeState()
    {
        _currentState = ResolveStartState();
        _elapsedTime = 0f;
        _consecutiveSameStateCount = _currentState != null ? 1 : 0;
    }
    
    public void ResetStateForLoad() => InitializeState();

    private MarketStateDataSO ResolveStartState()
    {
        if (_startState != null) return _startState;
        if (_marketStateDefinitions != null && _marketStateDefinitions.Length > 0)
            return _marketStateDefinitions[0];
        
        return null;
    }

    private void ProcessStateTimer(float deltaTime)
    {
        if (_marketStateDefinitions == null || _marketStateDefinitions.Length == 0)
            return;

        _elapsedTime += deltaTime;

        while (_elapsedTime >= _changeInterval)
        {
            _elapsedTime -= _changeInterval;
            ChangeState();
        }
    }
    
    private void ChangeState()
    {
        List<MarketStateDataSO> candidates = BuildCandidateStates();
        if (candidates.Count == 0) return;
        
        MarketStateDataSO nextState = candidates[Random.Range(0, candidates.Count)];
        ApplyState(nextState);
    }

    private List<MarketStateDataSO> BuildCandidateStates()
    {
        List<MarketStateDataSO> candidates = new();

        foreach (MarketStateDataSO state in _marketStateDefinitions)
        {
            if (state == null) continue;

            bool shouldExcludeCurrent =
                _currentState != null &&
                state == _currentState &&
                _consecutiveSameStateCount >= 2;

            if (shouldExcludeCurrent) continue;
            
            candidates.Add(state);
        }
        
        return candidates;
    }

    private void ApplyState(MarketStateDataSO nextState)
    {
        if (nextState == null) return;

        if (nextState == _currentState)
            _consecutiveSameStateCount++;
        else
            _consecutiveSameStateCount = 1;

        _currentState = nextState;
        StateChanged?.Invoke();
    }
}