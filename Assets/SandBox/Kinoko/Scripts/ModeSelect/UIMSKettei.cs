using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMSKettei : MonoBehaviour
{
    [SerializeField] private GameObject _lineFlame;

    public void StartAnim()
    {
        Animator animator = _lineFlame.GetComponent<Animator>();
        animator.SetBool("Action", true);
    }

    public void ResetAnim()
    {
        Animator animator = _lineFlame.GetComponent<Animator>();
        animator.SetBool("Action",false);
    }
}
