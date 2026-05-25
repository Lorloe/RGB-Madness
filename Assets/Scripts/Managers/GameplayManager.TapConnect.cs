using System.Collections.Generic;
using UnityEngine;

public partial class GameplayManager
{
    private const string TapConnectLabel = "TAP CONNECT";

    [Header("Tap Connect Event")]
    [SerializeField] private int _minResolvedBlocksBetweenTapConnect = 8;
    [SerializeField] private int _maxResolvedBlocksBetweenTapConnect = 14;
    [SerializeField] private int _minTapConnectBlocks = 4;
    [SerializeField] private int _maxTapConnectBlocks = 7;

    private readonly List<BlockButton> _laneButtons = new();
    private int _blocksUntilNextTapConnectEvent;
    private int _remainingTapConnectBlocks;
    private bool _isTapConnectActive;

    private bool IsTapConnectActive => _isTapConnectActive;
    private bool ShouldForceShortBlocks => _isTapConnectActive;

    private void InitializeTapConnect()
    {
        CacheTapConnectLaneButtons();
        ScheduleNextTapConnectEvent();
        SetLaneButtonsVisible(true);
    }

    private void NotifyTapConnectBlockResolved()
    {
        if (_isTapConnectActive)
        {
            _remainingTapConnectBlocks--;
            if (_remainingTapConnectBlocks <= 0)
            {
                DeactivateTapConnectEvent();
            }

            return;
        }

        _blocksUntilNextTapConnectEvent--;
        if (_blocksUntilNextTapConnectEvent <= 0)
        {
            ActivateTapConnectEvent();
        }
    }

    public void HandleTapConnectClick(FloatingBlock clickedBlock)
    {
        if (_hasGameFinished || !_isTapConnectActive)
        {
            return;
        }

        if (clickedBlock == null || clickedBlock != _currentBlock)
        {
            TriggerGameOver();
            return;
        }

        ResolveCurrentBlock();
    }

    private void ResetTapConnectState()
    {
        if (_isTapConnectActive)
        {
            DeactivateTapConnectEvent();
            return;
        }

        SetLaneButtonsVisible(true);
    }

    private void ActivateTapConnectEvent()
    {
        _isTapConnectActive = true;
        _remainingTapConnectBlocks = Random.Range(
            Mathf.Max(1, Mathf.Min(_minTapConnectBlocks, _maxTapConnectBlocks)),
            Mathf.Max(_minTapConnectBlocks, _maxTapConnectBlocks) + 1);
        SetLaneButtonsVisible(false);
    }

    private void DeactivateTapConnectEvent()
    {
        _isTapConnectActive = false;
        _remainingTapConnectBlocks = 0;
        ScheduleNextTapConnectEvent();
        SetLaneButtonsVisible(true);
    }

    private void ScheduleNextTapConnectEvent()
    {
        int minBlocks = Mathf.Max(1, Mathf.Min(_minResolvedBlocksBetweenTapConnect, _maxResolvedBlocksBetweenTapConnect));
        int maxBlocks = Mathf.Max(minBlocks, Mathf.Max(_minResolvedBlocksBetweenTapConnect, _maxResolvedBlocksBetweenTapConnect));
        _blocksUntilNextTapConnectEvent = Random.Range(minBlocks, maxBlocks + 1);
    }

    private void CacheTapConnectLaneButtons()
    {
        _laneButtons.Clear();
        _laneButtons.AddRange(FindObjectsByType<BlockButton>(FindObjectsSortMode.None));
    }

    private void SetLaneButtonsVisible(bool isVisible)
    {
        if (_laneButtons.Count == 0)
        {
            CacheTapConnectLaneButtons();
        }

        foreach (BlockButton laneButton in _laneButtons)
        {
            if (laneButton != null)
            {
                laneButton.gameObject.SetActive(isVisible);
            }
        }
    }
}
