using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCameraShaker : Shaker
{
    public static MainCameraShaker instance;
    protected override void Awake()
    {
        base.Awake();
        instance = this;
    }
}
