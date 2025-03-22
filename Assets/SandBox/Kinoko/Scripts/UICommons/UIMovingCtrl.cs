using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

/// <summary>
/// 入力に応じて対象のボタン等を移動させていくクラス(※これが一番上になるように設計)
/// </summary>

//MonoBehaviour消す！
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
    //例外処理によってスキップされた際のみ、本来フォーカスされた座標が入る
    public Vector2 _casted{get; protected set;} = new Vector2(-1,-1);

    protected virtual void Awake()
    {
        
        DesignatedForcus(_startPos);
    }

    /// <summary>
    /// 特定な任意の座標にフォーカスを動かす
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

    //y軸方向の要素数を返すメソッド
    public int ReturnArrayLength()
    {
        return _outMap[0].ReturnLength();
    }
    /// <summary>
    /// 上入力
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
            //もし最終的なフォーカスが今のフォーカスと同じ際の演出を書くならここに！(ボタンがブブッって感じで震えたり..)
            return;
        }
        _outMap[(int)_forcus.x].ReturnList()[(int)_forcus.y].SeparateAction(this.transform.gameObject);

        _forcus = _search;
        _search = new Vector2(-1,-1);
        _casted = new Vector2(-1,-1);
        Debug.Log("forcus>>" + _forcus);
        
        //最終目的の選択後のアニメーション等、移動後の処理
        Vector2 _target = new Vector2(0,0);
        _outMap[(int)_forcus.x].ReturnList()[(int)_forcus.y].FocusedAction(this.transform.gameObject);
    }

    /// <summary>
    /// 下入力
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
            //もし最終的なフォーカスが今のフォーカスと同じ際の演出を書くならここに！(ボタンがブブッって感じで震えたり..)
            return;
        }

        _outMap[(int)_forcus.x].ReturnList()[(int)_forcus.y].SeparateAction(this.transform.gameObject);
        Debug.Log("以下処理");

        _forcus = _search;
        _search = new Vector2(-1,-1);
        _casted = new Vector2(-1,-1);
        Debug.Log("forcus>>" + _forcus);
        
        //最終目的の選択後のアニメーション等、移動後の処理
        Vector2 _target = new Vector2(0,0);
        _outMap[(int)_forcus.x].ReturnList()[(int)_forcus.y].FocusedAction(this.transform.gameObject);
    }

    /// <summary>
    /// 左入力
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
            //もし最終的なフォーカスが今のフォーカスと同じ際の演出を書くならここに！(ボタンがブブッって感じで震えたり..)
            return;
        }

        _outMap[(int)_forcus.x].ReturnList()[(int)_forcus.y].SeparateAction(this.transform.gameObject);

        _forcus = _search;
        _search = new Vector2(-1,-1);
        _casted = new Vector2(-1,-1);
        Debug.Log("forcus>>" + _forcus);
        
        //最終目的の選択後のアニメーション等、移動後の処理
        Vector2 _target = new Vector2(0,0);
        _outMap[(int)_forcus.x].ReturnList()[(int)_forcus.y].FocusedAction(this.transform.gameObject);
    }

    /// <summary>
    /// 右入力
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
            //もし最終的なフォーカスが今のフォーカスと同じ際の演出を書くならここに！(ボタンがブブッって感じで震えたり..)
            return;
        }
        
        _outMap[(int)_forcus.x].ReturnList()[(int)_forcus.y].SeparateAction(this.transform.gameObject);

        _forcus = _search;
        _search = new Vector2(-1,-1);
        _casted = new Vector2(-1,-1);
        Debug.Log("forcus>>" + _forcus);
        
        //最終目的の選択後のアニメーション等、移動後の処理
        Vector2 _target = new Vector2(0,0);
        _outMap[(int)_forcus.x].ReturnList()[(int)_forcus.y].FocusedAction(this.transform.gameObject);
    }

    /// <summary>
    /// 決定された時の処理
    /// </summary>
    public virtual void OnClick()
    {
        _outMap[(int)_forcus.x].ReturnList()[(int)_forcus.y].ClickedAction(this.transform.gameObject);
    }
}
