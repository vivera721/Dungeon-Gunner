using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[DisallowMultipleComponent]
public class ReloadWeaponEvent : MonoBehaviour
{
    public event Action<ReloadWeaponEvent, ReloadWeaponEventArgs> OnReloadWeapon;

    /// <summary>
    /// Specify the weapon to have it's clip reloaded. if the total ammo is also to be increased then specify the topUpAmmoPercent
    /// 클립을 재장전할 무기를 지정한다. 총 탄약도 증가해야 하는 경우 topUpAmmoPercent를 지정한다
    /// </summary>
    public void CallReloadWeaponEvent(Weapon weapon, int topUpAmmoPercent)
    {
        OnReloadWeapon?.Invoke(this, new ReloadWeaponEventArgs() { weapon = weapon, topUpAmmoPercent = topUpAmmoPercent });
    }
}

public class ReloadWeaponEventArgs : EventArgs
{
    public Weapon weapon;
    public int topUpAmmoPercent;
}
