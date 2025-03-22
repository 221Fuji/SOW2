using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

/// <summary>
/// ���͂ɉ����đΏۂ̃{�^�������ړ������Ă����N���X(�����ꂪ��ԏ�ɂȂ�悤�ɐ݌v)
/// </summary>

//MonoBehaviour�����I
public class UIMovingCtrl : MonoBehaviour
{
    [System.Serializable]
    protected class Making
    {
        [SerializeField] protected List<UIPersonalAct> _inMap;
        public int ReturnLength(){
            return _inMap.Count;
        }
        public List<UIPersonalAct> ReturnList(){
            return _inMap;
        }
    }
    [SerializeField] protected List<Making> _outMap;
    [SerializeField] protected Vector2 _startPos = new Vector2(0,0);


    public Vector2 _forcus{get; protected set;} = new Vector2(0,0);
    public Vector2 _search{get; protected set;} = new Vector2(-1,-1);
    //��O�����ɂ���ăX�L�b�v���ꂽ�ۂ̂݁A�{���t�H�[�J�X���ꂽ���W������
    public Vector2 _casted{get; protected set;} = new Vector2(-1,-1);

    protected virtual void Awake()
    {
        
        DesignatedForcus(_startPos);
    }

    /// <summary>
    /// ����ȔC�ӂ̍��W�Ƀt�H�[�J�X�𓮂���
    /// </summary>
    public virtual void DesignatedForcus(Vector2 arrayPos)
    {
        if(_forcus.x == -1 && _forcus.y == -1)
        {
            _forcus = _startPos;
        }

        _outMap[(int)_forcus.x].ReturnList()[(int)_forcus.y].SeparateAction(this.transform.gameObject);
        _forcus = arrayPos;
        _outMap[(int)_forcus.x].ReturnList()[(int)_forcus.y].FocusedAction(this.transform.gameObject);
    }

    //y�������̗v�f����Ԃ����\�b�h
    public int ReturnArrayLength()
    {
        return _outMap[0].ReturnLength();
    }
    /// <summary>
    /// �����
    /// </summary>
    public virtual void ForcusUp()
    {
        if(_search.x == -1 && _search.y == -1) _search = new Vector2(_forcus.x,_forcus.y - 1);
        if(_search.y < 0)
        {
            _search = new Vector2(_search.x,_outMap[(int)_search.x].ReturnLength() - 1);
            this.ForcusUp();
            return;
        }

        if(_outMap[(int)_search.x].ReturnList()[(int)_search.y].MovingException(this.transform.gameObject))
        {
            _casted = _search;
            _search = new Vector2(_search.x,_search.y - 1);
            this.ForcusUp();
            return;
        }

        if(_forcus == _search)
        {
            //�����ŏI�I�ȃt�H�[�J�X�����̃t�H�[�J�X�Ɠ����ۂ̉��o�������Ȃ炱���ɁI(�{�^�����u�u�b���Ċ����Ők������..)
            return;
        }
        _outMap[(int)_forcus.x].ReturnList()[(int)_forcus.y].SeparateAction(this.transform.gameObject);

        _forcus = _search;
        _search = new Vector2(-1,-1);
        _casted = new Vector2(-1,-1);
        Debug.Log("forcus>>" + _forcus);
        
        //�ŏI�ړI�̑I����̃A�j���[�V�������A�ړ���̏���
        Vector2 _target = new Vector2(0,0);
        _outMap[(int)_forcus.x].ReturnList()[(int)_forcus.y].FocusedAction(this.transform.gameObject);
    }

    /// <summary>
    /// ������
    /// </summary>
    public virtual void ForcusDown(){
        if(_search.x == -1 && _search.y == -1) _search = new Vector2(_forcus.x,_forcus.y + 1);
        if(_search.y > _outMap[(int)_search.x].ReturnLength() - 1){
            _search = new Vector2(_search.x,0);
            this.ForcusDown();
            return;
        }

        if(_outMap[(int)_search.x].ReturnList()[(int)_search.y].MovingException(this.transform.gameObject)){
            _casted = _search;
            _search = new Vector2(_search.x,_search.y + 1);
            this.ForcusDown();
            return;
        }
        
        if(_forcus == _search)
        {
            //�����ŏI�I�ȃt�H�[�J�X�����̃t�H�[�J�X�Ɠ����ۂ̉��o�������Ȃ炱���ɁI(�{�^�����u�u�b���Ċ����Ők������..)
            return;
        }

        _outMap[(int)_forcus.x].ReturnList()[(int)_forcus.y].SeparateAction(this.transform.gameObject);
        Debug.Log("�ȉ�����");

        _forcus = _search;
        _search = new Vector2(-1,-1);
        _casted = new Vector2(-1,-1);
        Debug.Log("forcus>>" + _forcus);
        
        //�ŏI�ړI�̑I����̃A�j���[�V�������A�ړ���̏���
        Vector2 _target = new Vector2(0,0);
        _outMap[(int)_forcus.x].ReturnList()[(int)_forcus.y].FocusedAction(this.transform.gameObject);
    }

    /// <summary>
    /// ������
    /// </summary>
    public virtual void ForcusLeft(){
        if(_search.x == -1 && _search.y == -1) _search = new Vector2(_forcus.x - 1,_forcus.y);
        if(_search.x < 0){
            _search = new Vector2(_outMap.Count - 1, _search.y);
            this.ForcusLeft();
            return;
        }

        if(_outMap[(int)_search.x].ReturnList()[(int)_search.y].MovingException(this.transform.gameObject)){
            _casted = _search;
            _search = new Vector2(_search.x - 1,_search.y);
            this.ForcusLeft();
            return;
        }

        if(_forcus == _search)
        {
            //�����ŏI�I�ȃt�H�[�J�X�����̃t�H�[�J�X�Ɠ����ۂ̉��o�������Ȃ炱���ɁI(�{�^�����u�u�b���Ċ����Ők������..)
            return;
        }

        _outMap[(int)_forcus.x].ReturnList()[(int)_forcus.y].SeparateAction(this.transform.gameObject);

        _forcus = _search;
        _search = new Vector2(-1,-1);
        _casted = new Vector2(-1,-1);
        Debug.Log("forcus>>" + _forcus);
        
        //�ŏI�ړI�̑I����̃A�j���[�V�������A�ړ���̏���
        Vector2 _target = new Vector2(0,0);
        _outMap[(int)_forcus.x].ReturnList()[(int)_forcus.y].FocusedAction(this.transform.gameObject);
    }

    /// <summary>
    /// �E����
    /// </summary>
    public virtual void ForcusRight(){
        if(_search.x == -1 && _search.y == -1) _search = new Vector2(_forcus.x + 1,_forcus.y);
        if(_search.x > _outMap.Count - 1){
            _search = new Vector2(0,_search.y);
            this.ForcusRight();
            return;
        }
        Debug.Log(_search);

        if(_outMap[(int)_search.x].ReturnList()[(int)_search.y].MovingException(this.transform.gameObject)){
            _casted = _search;
            _search = new Vector2(_search.x + 1,_search.y);
            this.ForcusRight();
            return;
        }

        if(_forcus == _search)
        {
            //�����ŏI�I�ȃt�H�[�J�X�����̃t�H�[�J�X�Ɠ����ۂ̉��o�������Ȃ炱���ɁI(�{�^�����u�u�b���Ċ����Ők������..)
            return;
        }
        
        _outMap[(int)_forcus.x].ReturnList()[(int)_forcus.y].SeparateAction(this.transform.gameObject);

        _forcus = _search;
        _search = new Vector2(-1,-1);
        _casted = new Vector2(-1,-1);
        Debug.Log("forcus>>" + _forcus);
        
        //�ŏI�ړI�̑I����̃A�j���[�V�������A�ړ���̏���
        Vector2 _target = new Vector2(0,0);
        _outMap[(int)_forcus.x].ReturnList()[(int)_forcus.y].FocusedAction(this.transform.gameObject);
    }

    /// <summary>
    /// ���肳�ꂽ���̏���
    /// </summary>
    public virtual void OnClick()
    {
        _outMap[(int)_forcus.x].ReturnList()[(int)_forcus.y].ClickedAction(this.transform.gameObject);
    }
}
