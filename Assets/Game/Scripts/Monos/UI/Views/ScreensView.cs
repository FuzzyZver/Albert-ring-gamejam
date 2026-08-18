using System;
using UnityEngine;

/// <summary>
/// Все крупные экраны в одном месте. Складывай корни в массив в инспекторе,
/// система дальше переключает их по ScreenId и не знает про иерархию сцены.
/// </summary>
public class ScreensView : MonoBehaviour
{
    [Serializable]
    public class Entry
    {
        public ScreenId Id;
        public GameObject Root;
    }

    [SerializeField] private Entry[] _screens;

    public void Show(ScreenId id)
    {
        if (_screens == null) return;

        for (int i = 0; i < _screens.Length; i++)
        {
            var entry = _screens[i];
            if (entry != null && entry.Root != null)
                entry.Root.SetActive(entry.Id == id);
        }
    }
}