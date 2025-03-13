using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    private Transform _backGroundTF; // 背景のTransform
    [SerializeField] private float _parallaxFactor; // パララックス効果の強さ（0に近いほど動かない）

    private Vector3 _previousBGpos; // 前の背景の座標

    public void InitializeParallax(Transform backGroundTF)
    {
        _backGroundTF = backGroundTF;
        _previousBGpos = _backGroundTF.position;
    }

    private void Update()
    {
        if (_backGroundTF == null) return;

        // カメラの移動量を計算
        Vector3 deltaMovement = _backGroundTF.position - _previousBGpos;

        // パララックス効果を調整（サイズが大きくなるほど背景の動きが抑制される）
        transform.localPosition += new Vector3(-deltaMovement.x * _parallaxFactor, 0);

        // カメラの現在位置を保存
        _previousBGpos = _backGroundTF.position;
    }
}
