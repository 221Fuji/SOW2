using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackGroundManager : MonoBehaviour
{
    // 追従対象となるオブジェクト
    private Transform _target;

    [SerializeField] private ParallaxBackground[] _pbg;

    // x座標の制限範囲
    private float _minX;
    private float _maxX;

    private float _fixedY;

    public void InitializeBackGround(Transform target, float minBackGroundPos, float maxBackGroundPos)
    {
        _target = target;
        _fixedY = transform.position.y;

        _minX = minBackGroundPos;
        _maxX = maxBackGroundPos;
        
        foreach (ParallaxBackground backGround in _pbg)
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
}
