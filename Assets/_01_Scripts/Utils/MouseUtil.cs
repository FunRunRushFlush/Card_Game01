using Game.Logging;
using UnityEngine;



public static class MouseUtil
{
    private static Camera _cached;

    private static Camera MainCam
    {
        get
        {
            // Unity "fake null" berücksichtigen:
            if (_cached == null)
                _cached = Camera.main;
            return _cached;
        }
    }

    public static Vector3 GetWorldMousePositionInWorldSpace(float zValue = 0f)
    {
        var cam = MainCam;
        if (cam == null)
        {
            Log.Warn(LogCat.General, () => "MouseUtil: No Camera.main found (tag MainCamera missing?)");
            return Vector3.zero;
        }

        var plane = new Plane(cam.transform.forward, new Vector3(0, 0, zValue));
        var ray = cam.ScreenPointToRay(Input.mousePosition);

        if (plane.Raycast(ray, out float distance))
            return ray.GetPoint(distance);

        Log.Error(LogCat.General, () => "MouseUtil: Could not raycast to plane.");
        return Vector3.zero;
    }
}
