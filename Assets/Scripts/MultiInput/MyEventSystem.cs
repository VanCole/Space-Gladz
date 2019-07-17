using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;

public class MyEventSystem : EventSystem
{
    protected override void OnEnable()
    {
        // do not assign EventSystem.current
    }

    protected override void Update()
    {
        EventSystem originalCurrent = EventSystem.current;
        current = this; // in order to avoid reimplementing half of the EventSystem class, just temporarily assign this EventSystem to be the globally current one
        base.Update();
        current = originalCurrent;
    }

}
