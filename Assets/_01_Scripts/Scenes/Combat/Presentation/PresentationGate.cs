using System.Collections;
using System.Collections.Generic;

public static class PresentationGate
{
    private static int nextToken = 1;
    private static readonly HashSet<int> completed = new();

    public static int NewToken() => nextToken++;

    public static void Complete(int token)
    {
        completed.Add(token);
    }

    public static IEnumerator Wait(int token)
    {
        while (!completed.Contains(token))
            yield return null;

        completed.Remove(token);
    }
}