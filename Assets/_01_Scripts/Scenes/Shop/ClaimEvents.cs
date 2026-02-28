using System;

public static class ClaimEvents
{
    public static event Action AnyClaimed;

    public static void RaiseAnyClaimed()
    {
        AnyClaimed?.Invoke();
    }
}