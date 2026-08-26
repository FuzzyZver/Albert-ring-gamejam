using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Состояние, которое живёт дольше забега. Кладётся в Injects через
/// .Inject(new RealtimeData()) и потому не является статикой.
///
/// Открытые концовки сюда и просятся: их нельзя держать в компонентах,
/// потому что RunSetupSystem сносит забег целиком, а прогресс должен остаться.
/// </summary>
public class RealtimeData
{
    private const string Key = "AlbertsRing.Ending.";

    public bool Paused;

    private readonly HashSet<EndingId> _unlocked = new HashSet<EndingId>();

    public int UnlockedCount => _unlocked.Count;

    public bool IsUnlocked(EndingId id) => _unlocked.Contains(id);

    /// <summary>true, если концовка открыта впервые — чтобы эпилог мог это отметить.</summary>
    public bool Unlock(EndingId id)
    {
        if (!_unlocked.Add(id)) return false;

        PlayerPrefs.SetInt(Key + id, 1);
        PlayerPrefs.Save();
        return true;
    }

    public void Load()
    {
        _unlocked.Clear();

        foreach (EndingId id in System.Enum.GetValues(typeof(EndingId)))
            if (PlayerPrefs.GetInt(Key + id, 0) == 1) _unlocked.Add(id);
    }
}