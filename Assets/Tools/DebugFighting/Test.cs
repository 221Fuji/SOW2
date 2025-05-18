using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

namespace Test
{
    public class Test : MonoBehaviour
    {
        private void Start()
        {
            DamegeEverySecond().Forget();
        }

        private async UniTask DamegeEverySecond()
        {
            while (true)
            {
                try
                {
                    await UniTask.Delay(1000);
                    Debug.Log($"1秒毎に{CalHugeDamage()}ダメージ！");
                }
                catch 
                {
                    return;
                }
            }
        }

        private BigInteger CalHugeDamage()
        {
            BigInteger hugeNum = BigInteger.Pow(10, 136);
            return hugeNum * 9999;
        }
    }
}

