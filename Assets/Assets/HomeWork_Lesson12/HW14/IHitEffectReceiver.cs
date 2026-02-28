using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHitEffectReceiver
{
    void PlayHitEffect(Vector3 fromPosition);
}