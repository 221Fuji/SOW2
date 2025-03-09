using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���͂ɉ����đΏۂ̃{�^�������ړ������Ă����N���X(�����ꂪ��ԏ�ɂȂ�悤�ɐ݌v)
/// </summary>

//MonoBehaviour�����I
public class UIMovingCtrl : MonoBehaviour
{
    UIPersonalAct[,] _map = new UIPersonalAct[,]{
        {new UIPersonalAct(),new UIPersonalAct(),new UIPersonalAct()},
        {new UIPersonalAct(),new UIPersonalAct(),new UIPersonalAct()}
    };

    Vector2 _forcus = new Vector2(0,2);
    Vector2 _search = new Vector2(-1,-1);

    /// <summary>
    /// �����
    /// </summary>
    public void ForcusUp()
    {
        if(_search.x == -1 && _search.y == -1) _search = new Vector2(_forcus.x,_forcus.y - 1);
        if(_search.y < 0){
            _search.y = _map.GetLength(1) - 1;
            this.ForcusUp();
            return;
        }

        if(_map[(int)_search.x,(int)_search.y].MovingException(this)){
            _search = new Vector2(_search.x,_search.y - 1);
            this.ForcusUp();
            return;
        }

        _forcus = _search;
        _search = new Vector2(-1,-1);
        Debug.Log("forcus>>" + _forcus);
        
        //�ŏI�ړI�̑I����̃A�j���[�V�������A�ړ���̏���
        Vector2 _target = new Vector2(0,0);
        _map[(int)_target.x,(int)_target.y].FocusedAction();
    }

    /// <summary>
    /// ������
    /// </summary>
    public void ForcusDown(){
        if(_search.x == -1 && _search.y == -1) _search = new Vector2(_forcus.x,_forcus.y + 1);
        if(_search.y > _map.GetLength(1) - 1){
            _search.y = 0;
            this.ForcusDown();
            return;
        }

        if(_map[(int)_search.x,(int)_search.y].MovingException(this)){
            _search = new Vector2(_search.x,_search.y + 1);
            this.ForcusDown();
            return;
        }

        _forcus = _search;
        _search = new Vector2(-1,-1);
        Debug.Log("forcus>>" + _forcus);
        
        //�ŏI�ړI�̑I����̃A�j���[�V�������A�ړ���̏���
        Vector2 _target = new Vector2(0,0);
        _map[(int)_target.x,(int)_target.y].FocusedAction();
    }

    /// <summary>
    /// ������
    /// </summary>
    public void ForcusLeft(){
        if(_search.x == -1 && _search.y == -1) _search = new Vector2(_forcus.x - 1,_forcus.y);
        if(_search.x < 0){
            _search.x = _map.GetLength(0) - 1;
            this.ForcusLeft();
            return;
        }

        if(_map[(int)_search.x,(int)_search.y].MovingException(this)){
            _search = new Vector2(_search.x - 1,_search.y);
            this.ForcusLeft();
            return;
        }

        _forcus = _search;
        _search = new Vector2(-1,-1);
        Debug.Log("forcus>>" + _forcus);
        
        //�ŏI�ړI�̑I����̃A�j���[�V�������A�ړ���̏���
        Vector2 _target = new Vector2(0,0);
        _map[(int)_target.x,(int)_target.y].FocusedAction();
    }

    /// <summary>
    /// �E����
    /// </summary>
    public void ForcusRight(){
        if(_search.x == -1 && _search.y == -1) _search = new Vector2(_forcus.x + 1,_forcus.y);
        if(_search.x > _map.GetLength(1) - 1){
            _search.x = 0;
            this.ForcusRight();
            return;
        }

        if(_map[(int)_search.x,(int)_search.y].MovingException(this)){
            _search = new Vector2(_search.x + 1,_search.y);
            this.ForcusRight();
            return;
        }

        _forcus = _search;
        _search = new Vector2(-1,-1);
        Debug.Log("forcus>>" + _forcus);
        
        //�ŏI�ړI�̑I����̃A�j���[�V�������A�ړ���̏���
        Vector2 _target = new Vector2(0,0);
        _map[(int)_target.x,(int)_target.y].FocusedAction();
    }
}
