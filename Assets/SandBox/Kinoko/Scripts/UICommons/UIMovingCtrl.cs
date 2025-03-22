using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

/// <summary>
/// 入力に応じて対象のボタン等を移動させていくクラス(※これが一番上になるように設計)
/// </summary>

public class UIMovingCtrl : MonoBehaviour
{
    [System.Serializable]
    public class Making
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
    [SerializeField] protected Vector2 _startPos = new Vector2(0, 0);


    public List<Making> OutMap { get { return _outMap; } }
    public Vector2 Forcus { get; protected set; } = new Vector2(0, 0);
    public Vector2 Search { get; protected set; } = new Vector2(-1, -1);
    //例外処理によってスキップされた際のみ、本来フォーカスされた座標が入る
    public Vector2 Casted { get; protected set; } = new Vector2(-1, -1);

    protected virtual void Awake()
    {
        
        DesignatedForcus(_startPos);
    }

    /// <summary>
    /// 特定な任意の座標にフォーカスを動かす
    /// </summary>
    public virtual void DesignatedForcus(Vector2 arrayPos)
    {
        if(Forcus.x == -1 && Forcus.y == -1)
        {
            Forcus = _startPos;
        }

        _outMap[(int)Forcus.x].ReturnList()[(int)Forcus.y].SeparateAction(this.transform.gameObject);
        Forcus = arrayPos;
        _outMap[(int)Forcus.x].ReturnList()[(int)Forcus.y].FocusedAction(this.transform.gameObject);
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
        if(Search.x == -1 && Search.y == -1) Search = new Vector2(Forcus.x,Forcus.y - 1);
        if(Search.y < 0)
        {
            Search = new Vector2(Search.x,_outMap[(int)Search.x].ReturnLength() - 1);
            this.ForcusUp();
            return;
        }

        if(_outMap[(int)Search.x].ReturnList()[(int)Search.y].MovingException(gameObject))
        {
            Casted = Search;
            Search = new Vector2(Search.x,Search.y - 1);
            this.ForcusUp();
            return;
        }

        if(Forcus == Search)
        {
            //もし最終的なフォーカスが今のフォーカスと同じ際の演出を書くならここに！(ボタンがブブッって感じで震えたり..)
            return;
        }
        _outMap[(int)Forcus.x].ReturnList()[(int)Forcus.y].SeparateAction(gameObject);

        Forcus = Search;
        Search = new Vector2(-1,-1);
        Casted = new Vector2(-1,-1);
        Debug.Log("forcus>>" + Forcus);
        
        //最終目的の選択後のアニメーション等、移動後の処理
        Vector2 _target = new Vector2(0,0);
        _outMap[(int)Forcus.x].ReturnList()[(int)Forcus.y].FocusedAction(this.transform.gameObject);
    }

    /// <summary>
    /// 下入力
    /// </summary>
    public virtual void ForcusDown(){
        if(Search.x == -1 && Search.y == -1) Search = new Vector2(Forcus.x,Forcus.y + 1);
        if(Search.y > _outMap[(int)Search.x].ReturnLength() - 1){
            Search = new Vector2(Search.x,0);
            this.ForcusDown();
            return;
        }

        if(_outMap[(int)Search.x].ReturnList()[(int)Search.y].MovingException(gameObject)){
            Casted = Search;
            Search = new Vector2(Search.x,Search.y + 1);
            this.ForcusDown();
            return;
        }
        
        if(Forcus == Search)
        {
            //もし最終的なフォーカスが今のフォーカスと同じ際の演出を書くならここに！(ボタンがブブッって感じで震えたり..)
            return;
        }

        _outMap[(int)Forcus.x].ReturnList()[(int)Forcus.y].SeparateAction(this.transform.gameObject);
        Debug.Log("以下処理");

        Forcus = Search;
        Search = new Vector2(-1,-1);
        Casted = new Vector2(-1,-1);
        Debug.Log("forcus>>" + Forcus);
        
        //最終目的の選択後のアニメーション等、移動後の処理
        Vector2 _target = new Vector2(0,0);
        _outMap[(int)Forcus.x].ReturnList()[(int)Forcus.y].FocusedAction(this.transform.gameObject);
    }

    /// <summary>
    /// 左入力
    /// </summary>
    public virtual void ForcusLeft(){
        if(Search.x == -1 && Search.y == -1) Search = new Vector2(Forcus.x - 1,Forcus.y);
        if(Search.x < 0){
            Search = new Vector2(_outMap.Count - 1, Search.y);
            this.ForcusLeft();
            return;
        }

        if(_outMap[(int)Search.x].ReturnList()[(int)Search.y].MovingException(gameObject)){
            Casted = Search;
            Search = new Vector2(Search.x - 1,Search.y);
            this.ForcusLeft();
            return;
        }

        if(Forcus == Search)
        {
            //もし最終的なフォーカスが今のフォーカスと同じ際の演出を書くならここに！(ボタンがブブッって感じで震えたり..)
            return;
        }

        _outMap[(int)Forcus.x].ReturnList()[(int)Forcus.y].SeparateAction(this.transform.gameObject);

        Forcus = Search;
        Search = new Vector2(-1,-1);
        Casted = new Vector2(-1,-1);
        Debug.Log("forcus>>" + Forcus);
        
        //最終目的の選択後のアニメーション等、移動後の処理
        Vector2 _target = new Vector2(0,0);
        _outMap[(int)Forcus.x].ReturnList()[(int)Forcus.y].FocusedAction(this.transform.gameObject);
    }

    /// <summary>
    /// 右入力
    /// </summary>
    public virtual void ForcusRight(){
        if(Search.x == -1 && Search.y == -1) Search = new Vector2(Forcus.x + 1,Forcus.y);
        if(Search.x > _outMap.Count - 1){
            Search = new Vector2(0,Search.y);
            this.ForcusRight();
            return;
        }
        Debug.Log(Search);

        if(_outMap[(int)Search.x].ReturnList()[(int)Search.y].MovingException(gameObject)){
            Casted = Search;
            Search = new Vector2(Search.x + 1,Search.y);
            this.ForcusRight();
            return;
        }

        if(Forcus == Search)
        {
            //もし最終的なフォーカスが今のフォーカスと同じ際の演出を書くならここに！(ボタンがブブッって感じで震えたり..)
            return;
        }
        
        _outMap[(int)Forcus.x].ReturnList()[(int)Forcus.y].SeparateAction(this.transform.gameObject);

        Forcus = Search;
        Search = new Vector2(-1,-1);
        Casted = new Vector2(-1,-1);
        Debug.Log("forcus>>" + Forcus);
        
        //最終目的の選択後のアニメーション等、移動後の処理
        Vector2 _target = new Vector2(0,0);
        _outMap[(int)Forcus.x].ReturnList()[(int)Forcus.y].FocusedAction(this.transform.gameObject);
    }

    /// <summary>
    /// 決定された時の処理
    /// </summary>
    public virtual void OnClick()
    {
        _outMap[(int)Forcus.x].ReturnList()[(int)Forcus.y].ClickedAction(this.transform.gameObject);
    }
}
