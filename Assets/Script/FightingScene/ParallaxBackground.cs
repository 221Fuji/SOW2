using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    private Transform _backGroundTF; // �w�i��Transform
    [SerializeField] private float _parallaxFactor; // �p�����b�N�X���ʂ̋����i0�ɋ߂��قǓ����Ȃ��j

    private Vector3 _previousBGpos; // �O�̔w�i�̍��W

    public void InitializeParallax(Transform backGroundTF)
    {
        _backGroundTF = backGroundTF;
        _previousBGpos = _backGroundTF.position;
    }

    private void Update()
    {
        if (_backGroundTF == null) return;

        // �J�����̈ړ��ʂ��v�Z
        Vector3 deltaMovement = _backGroundTF.position - _previousBGpos;

        // �p�����b�N�X���ʂ𒲐��i�T�C�Y���傫���Ȃ�قǔw�i�̓������}�������j
        transform.localPosition += new Vector3(-deltaMovement.x * _parallaxFactor, 0);

        // �J�����̌��݈ʒu��ۑ�
        _previousBGpos = _backGroundTF.position;
    }
}
