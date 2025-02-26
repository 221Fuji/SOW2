using UnityEngine;
using System.Collections;
using Cysharp.Threading.Tasks;
using System.Threading;
using TMPro;

public class FightingCameraManager : MonoBehaviour
{
    [SerializeField] private float _minCameraSize;
    [SerializeField] private float _maxCameraSize;
    [SerializeField] private Vector2 _stageBoundsMin;
    [SerializeField] private Vector2 _stageBoundsMax;

    [SerializeField] private BackGroundManager _backGroundManager;
    [SerializeField] private Transform _leftWall;
    [SerializeField] private Transform _rightWall;

    private Camera _cam;
    private Transform _player1Pos;
    private Transform _player2Pos;

    public void InitializeCamera(Transform player1, Transform player2)
    {
        _cam = GetComponent<Camera>();
        _player1Pos = player1;
        _player2Pos = player2;

        float maxBackGroundPos = _stageBoundsMax.x - _maxCameraSize * _cam.aspect;
        float minBackGroundPos = _stageBoundsMin.x + _maxCameraSize * _cam.aspect;
        _backGroundManager.InitializeBackGround(transform, minBackGroundPos, maxBackGroundPos);
    }

    private void Update()
    {
        if (_player1Pos == null || _player2Pos == null || _cam == null) return;

        Vector3 midpoint = (_player1Pos.position + _player2Pos.position) / 2f;
        float distance = Vector3.Distance(_player1Pos.position, _player2Pos.position);

        float targetSize = (distance - _minCameraSize) / 6 + _minCameraSize;
        targetSize = Mathf.Clamp(targetSize, _minCameraSize, _maxCameraSize);

        float cameraHalfHeight = targetSize;
        float cameraHalfWidth = cameraHalfHeight * _cam.aspect;

        float clampedX = Mathf.Clamp(midpoint.x, _stageBoundsMin.x + cameraHalfWidth, _stageBoundsMax.x - cameraHalfWidth);
        float clampedY = Mathf.Clamp(midpoint.y, _stageBoundsMin.y + cameraHalfHeight, 1);

        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
        _cam.orthographicSize = targetSize;

        UpdateWalls(cameraHalfWidth, clampedY);
    }

    private void UpdateWalls(float cameraHalfWidth, float cameraY)
    {
        if (_leftWall != null)
        {
            _leftWall.position = new Vector3(transform.position.x - cameraHalfWidth, cameraY);
        }
        if (_rightWall != null)
        {
            _rightWall.position = new Vector3(transform.position.x + cameraHalfWidth, cameraY);
        }

        StageParameter.SetCurrentWallPos(_rightWall.position.x, _leftWall.position.x);
    }
}

