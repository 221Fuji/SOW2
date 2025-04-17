using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DebugFighting
{
    /// <summary>
    /// �v���C���[�̓��͂�ۑ����čĐ�����N���X
    /// </summary>
    public class RecInput : MonoBehaviour
    {
        FightingInputReceiver _fir = new FightingInputReceiver();

        //�������͏�Ԃ��p�����Ă���t���[����,1or0or-1,jump,�ʏ�U���@��:�E�ɐi�݂Ȃ���W�����v���͂����ꍇ "0,1,true,false"
        List<string> _recInputArray = new List<string>();
        [SerializeField]private bool _recFrag = false;
        bool _startFrag = false;
        int _playIndex = 0;
        int _playFrame = 0;

        //���͊֌W�̕ϐ�
        float _WalkValue = 0f;
        bool _jump = false;
        bool _normalMove = false;


        // Start is called before the first frame update
        void Awake()
        {
            _fir = this.GetComponent<FightingInputReceiver>();
        }

        void Update()
        {
            //���΃{�^����Enter�ɂ��Ă邯�ǂ����͔��\����Ȃ̂ő��߂ɂȂ�Ƃ�����
            if (Input.GetKeyDown(KeyCode.Return))
            {
                SwitchRec();
            }
            if (Input.GetKeyDown(KeyCode.RightShift))
            {
                SwitchPlay();
            }


            if (_recFrag)
            {
                string _lastPattern = "";
                if (_recInputArray.Count != 0) _lastPattern = _recInputArray[_recInputArray.Count - 1];

                //string�^�Ƃ��ē��͏��𐬌`���Ă���
                string _nowPattern = "";
                //waklValue�̒l
                _nowPattern += _WalkValue.ToString() + ",";
                //jump�̒l
                _nowPattern += _jump.ToString() + ",";
                //normalMove�̒l
                _nowPattern += _normalMove.ToString();

                int _continueFrame = 0;
                if (_lastPattern != "")
                {
                    int _lastFrame = int.Parse(_lastPattern.Split(',')[0]);
                    string _purePattern = "";
                    for (int i = 1; i < _lastPattern.Split(",").Length; i++)
                    {
                        if (i == _lastPattern.Split(",").Length - 1)
                        {
                            _purePattern += _lastPattern.Split(",")[i];
                        }
                        else
                        {
                            _purePattern += _lastPattern.Split(",")[i] + ",";
                        }
                    }

                    if (_nowPattern == _purePattern)
                    {
                        _continueFrame += (_lastFrame + 1);
                    }
                }

                string _registerPattern = _continueFrame.ToString() + "," + _nowPattern;
                if (_continueFrame == 0)
                {
                    _recInputArray.Add(_registerPattern);
                }
                else
                {
                    _recInputArray[_recInputArray.Count - 1] = _registerPattern;
                }

            }
            else if (_startFrag)
            {

                string[] _strings = _recInputArray[_playIndex].Split(",");
                if (int.Parse(_strings[0]) + 1 == _playFrame)
                {
                    _playIndex++;
                    _playFrame = 0;
                    _strings = _recInputArray[_playIndex].Split(",");
                }
                string _purePattern = "";
                for (int i = 1; i < _strings.Length; i++)
                {
                    if (i == _strings.Length - 1)
                    {
                        _purePattern += _strings[i];
                    }
                    else
                    {
                        _purePattern += _strings[i] + ",";
                    }
                }

                TranditionInput(_purePattern);
                _playFrame++;

                if (_playIndex == _recInputArray.Count - 1 && int.Parse(_strings[0]) + 1 == _playFrame)
                {
                    Debug.Log("�Đ����I�����܂�");
                    _playIndex = 0;
                    _playFrame = 0;
                    _startFrag = false;
                }
            }

            _jump = false;
            _normalMove = false;
        }

        void TranditionInput(string _purePattern)
        {
            string[] strings = _purePattern.Split(",");
            //�ړ��̏o��
            _fir.WalkValue = int.Parse(strings[0]);
            //�W�����v
            if (strings[1] == "True")
            {
                _fir.JumpDelegate?.Invoke();
            }
            //�ʏ�U��
            if (strings[2] == "True")
            {
                _fir.NormalMove?.Invoke();
            }
        }

        void SwitchRec()
        {
            if (_recFrag) _recFrag = false;
            else
            {
                _recInputArray.Clear();
                _recFrag = true;
            }
        }
        void SwitchPlay()
        {
            if (_startFrag) _startFrag = false;
            else if (!_recFrag) _startFrag = true;
        }

        public void OnFourDirections(InputValue value)
        {
            //�������͂�1,-1,0�̂ǂꂩ
            if (value.Get<Vector2>().x > 0)
            {
                _WalkValue = 1f;
            }
            else if (value.Get<Vector2>().x < 0)
            {
                _WalkValue = -1f;
            }
            else
            {
                _WalkValue = 0f;
            }
        }

        public void OnJump(InputValue value)
        {
            _jump = true;
        }

        public void OnNomalMove()
        {
            _normalMove = true;
        }
    }
}

