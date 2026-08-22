using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>Летопись: последние строки о том, что ты натворил.
/// Без неё половина матрицы срабатывает молча и выглядит как баг.</summary>
public class ChronicleView : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private int _maxLines = 6;

    private readonly List<string> _lines = new List<string>();

    public void Append(string line)
    {
        if (string.IsNullOrEmpty(line)) return;

        _lines.Add(line);
        while (_lines.Count > _maxLines) _lines.RemoveAt(0);

        if (_text != null) _text.text = string.Join("\n", _lines);
    }

    public void Clear()
    {
        _lines.Clear();
        if (_text != null) _text.text = string.Empty;
    }
}