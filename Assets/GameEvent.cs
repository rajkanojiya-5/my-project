using System;

public enum HitResult { Perfect, Good, Ok }

public static class GameEvents
{
    public static Action<float> OnBeat;            // fired the instant a beat occurs
    public static Action<float> OnBeatApproaching; // fired slightly before, for visual lead-in

    public static Action<HitResult, float> OnHit;  // player scored a hit (with error in ms)

    public static Action OnMiss;                   // player pressed at the wrong time

    public static Action<int> OnComboChanged;
    public static Action<int> OnNitroLayerChanged;

    public static Action<float> OnNitroTick;// combo count changed (0 = reset)
}
