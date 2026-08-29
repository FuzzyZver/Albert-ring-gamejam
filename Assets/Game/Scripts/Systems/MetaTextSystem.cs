using UnityEngine;
using Leopotam.Ecs;
using UnityEngine.InputSystem;

/// <summary>
/// Держит источник между наведением и уходом. Текст перечитывает каждый кадр:
/// прогресс стройки под курсором обновляется сам.
/// </summary>
public class MetaTextSystem : Injects, IEcsInitSystem, IEcsRunSystem
{
    private EcsFilter<MetaTextViewRef, MetaTextAttribute> _windows;
    private EcsFilter<MetaTextEnterEvent> _entered;
    private EcsFilter<MetaTextExitEvent> _exited;

    public void Init()
    {
        var prefab = GameConfig.PrefabsConfig.MetaTextView;
        if (prefab == null) return;

        var view = Object.Instantiate(prefab, UI.InstantiateParent);
        view.Hide();

        var entity = EcsWorld.NewEntity();
        entity.Get<MetaTextViewRef>().View = view;
        entity.Get<MetaTextAttribute>();
    }

    public void Run()
    {
        foreach (var w in _windows)
        {
            var view = _windows.Get1(w).View;
            ref var state = ref _windows.Get2(w);
            if (view == null) continue;

            Track(ref state);

            if (!TryGetMeta(ref state, out var meta))
            {
                state.HoverTime = 0f;
                if (state.Shown) Clear(ref state, view);
                continue;
            }

            state.HoverTime += Time.unscaledDeltaTime;

            if (state.HoverTime < GameConfig.SettingsConfig.MetaTextDelay)
            {
                if (state.Shown) Clear(ref state, view);   // сюда попадём только при смене источника
                continue;
            }

            if (!state.Shown || state.ShownTitle != meta.Title || state.ShownBody != meta.Body)
            {
                view.SetText(meta);
                view.Show();
                state.Shown = true;
                state.ShownTitle = meta.Title;
                state.ShownBody = meta.Body;
            }

            view.Place(Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero);
        }
    }

    private static bool TryGetMeta(ref MetaTextAttribute state, out MetaText meta)
    {
        meta = default;

        if (state.Source == null) return false;

        if (state.Behaviour == null || !state.Behaviour.isActiveAndEnabled)
        {
            Debug.Log("MetaText: источник умер или выключен");   // временно
            return false;
        }

        meta = state.Source.Meta;
        if (meta.IsEmpty)
        {
            Debug.Log($"MetaText: пустой текст у {state.Behaviour.GetType().Name}");   // временно
            return false;
        }

        return true;
    }

    private void Hide(ref MetaTextAttribute state, MetaTextView view)
    {
        state.Shown = false;
        state.ShownTitle = null;
        state.ShownBody = null;
        view.Hide();
    }

    private void Clear(ref MetaTextAttribute state, MetaTextView view)
    {
        state.Source = null;
        state.Behaviour = null;
        state.HoverTime = 0f;
        Hide(ref state, view);
    }

    /// <summary>Уходы разбираем раньше наведений: при переходе между соседними
    /// элементами оба евента живут в одном кадре, и порядок их создания не гарантирован.</summary>
    private void Track(ref MetaTextAttribute state)
    {
        foreach (var i in _exited)
            if (ReferenceEquals(_exited.Get1(i).Source, state.Source)) state.Source = null;

        foreach (var i in _entered)
        {
            var source = _entered.Get1(i).Source;
            if (ReferenceEquals(source, state.Source)) continue;   // тот же — не сбрасываем

            state.Source = source;
            state.Behaviour = source as MonoBehaviour;
            state.HoverTime = 0f;
        }
    }
}
