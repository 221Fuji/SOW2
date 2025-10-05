using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackGroundManager : MonoBehaviour
{
    // 追従対象となるオブジェクト
    private Transform _target;

    private ParallaxBackground[] _pbgPrefabs;

    // x座標の制限範囲
    private float _minX;
    private float _maxX;

    private float _fixedY;

    public void InitializeBackGround(Transform target, float minBackGroundPos, float maxBackGroundPos, StageData stageData)
    {
        _target = target;
        _fixedY = transform.position.y;

        _minX = minBackGroundPos;
        _maxX = maxBackGroundPos;
        
        // 背景の生成
        stageData.GenerateBGG();
        _pbgPrefabs = stageData.GeneratePBG(transform);
        foreach (var backGround in _pbgPrefabs)
        {
            backGround.InitializeParallax(transform);
        }
    }

    private void LateUpdate()
    {
        
        if (_target == null) return;

        // ターゲットのx座標を取得
        float targetX = _target.position.x;

        // x座標を制限範囲内に収める
        float clampedX = Mathf.Clamp(targetX, _minX, _maxX);

        // 現在のx座標から滑らかに移動する
        float smoothX = Mathf.Lerp(transform.position.x, clampedX, 0.75f);

        // オブジェクトの位置を更新
        transform.position = new Vector3(smoothX, _fixedY);
    }

    public void ChangeBackGroundColor(Color color)
    {
        foreach(ParallaxBackground backGround in _pbgPrefabs)
        {
            backGround.GetComponent<SpriteRenderer>().color = color;
        }
    }
}
