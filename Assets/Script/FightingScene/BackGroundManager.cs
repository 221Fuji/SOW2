using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackGroundManager : MonoBehaviour
{
    // �Ǐ]�ΏۂƂȂ�I�u�W�F�N�g
    private Transform _target;

    [SerializeField] private ParallaxBackground[] _pbg;

    // x���W�̐����͈�
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

        // �^�[�Q�b�g��x���W���擾
        float targetX = _target.position.x;

        // x���W�𐧌��͈͓��Ɏ��߂�
        float clampedX = Mathf.Clamp(targetX, _minX, _maxX);

        // ���݂�x���W���犊�炩�Ɉړ�����
        float smoothX = Mathf.Lerp(transform.position.x, clampedX, 0.75f);

        // �I�u�W�F�N�g�̈ʒu���X�V
        transform.position = new Vector3(smoothX, _fixedY);
    }
}
