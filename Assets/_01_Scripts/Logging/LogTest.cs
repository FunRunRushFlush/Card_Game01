using Game.Logging;
using UnityEngine;

public class LogTest : MonoBehaviour
{
    void Start()
    {
        Log.Info(LogArea.General, () => "Hello Logging", this);
        Log.Debug(LogArea.Combat, () => "Net debug message", this);
        Log.Warn(LogArea.Core, () => "Save warning", this);
        //Log.Error(LogCat.AI, () => "AI error", this);
    }
}
