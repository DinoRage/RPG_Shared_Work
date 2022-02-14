using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimationEventListner : MonoBehaviour
{

    // On  + event causing error
    public UnityEvent OnHit = new UnityEvent();
    public UnityEvent OnShoot = new UnityEvent();
    public UnityEvent OnFootR = new UnityEvent();
    public UnityEvent OnFootL = new UnityEvent();
    public UnityEvent OnLand = new UnityEvent();
    public UnityEvent OnStrike = new UnityEvent();
    public UnityEvent OnWeaponSwitch = new UnityEvent();

    // enevnt Void 'event name,
    // On + 'event name' . Invoke
    public void Hit()
    {
        OnHit.Invoke();
    }
    public void Shoot()
    {
        OnShoot.Invoke();
    }
    public void FootR()
    {
        OnFootR.Invoke();
    }
    public void FootL()
    {
        OnFootL.Invoke();
    }
    public void Land()
    {
        OnLand.Invoke();
    }
    public void WeaponSwitch()
    {
        OnWeaponSwitch.Invoke();
    }
    public void Strike()
    {
        OnStrike.Invoke();
    }
    public void SendEvent()
    {

    }
   
}
