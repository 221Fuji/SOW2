using FMOD.Studio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonMonoBihaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T I;

    /// <summary>
    /// 古いインスタンスが残る
    /// </summary>
    protected virtual void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if(I == null)
        {
            I = this as T;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
