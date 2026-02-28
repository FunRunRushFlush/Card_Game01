using Game.Logging;
using UnityEngine;

public class LogTest : MonoBehaviour
{
    void Start()
    {
        Log.Info(LogCat.General, () => "Hello Logging", this);
        Log.Debug(LogCat.Net, () => "Net debug message", this);
        Log.Warn(LogCat.Save, () => "Save warning", this);
        //Log.Error(LogCat.AI, () => "AI error", this);
    }
}
