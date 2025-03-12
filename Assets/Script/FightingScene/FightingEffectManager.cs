using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

/// <summary>
/// 対戦中のエフェクトを管理
/// </summary>
public class FightingEffectManager : MonoBehaviour
{
    [SerializeField] GameObject _effectObj;

    public void InitializeFEM(CharacterActions ca1, CharacterActions ca2)
    {
        ca1.OnEffect = OnEffect;
        ca2.OnEffect = OnEffect;
    }

    public void OnEffect(Vector2 effectPos, FightingEffect effectType)
    {
        switch (effectType) 
        {
            case FightingEffect.SmallHit:
                OnSmallHitEffect(effectPos);
                break;
            case FightingEffect.LargeHit:
                OnLargeHitEffect(effectPos);
                break;
            case FightingEffect.Guard:
                OnGuardEffect(effectPos);
                break;
            case FightingEffect.Break:
                OnBreakEffect(effectPos);
                break;
            case FightingEffect.RecoverBreak:
                OnRecoverBreakEffect(effectPos);
                break;
            default:
                break;
        }
    }

    private void OnGuardEffect(Vector2 effectPos)
    {
        GameObject effect = Instantiate(_effectObj, effectPos, Quaternion.identity);

        effect.GetComponent<Animator>().SetTrigger("GuardTrigger");

        CancellationTokenSource cts = new CancellationTokenSource();
        OffEffect(effect, 30, cts.Token).Forget();
    }

    private void OnSmallHitEffect(Vector2 effectPos)
    {
        GameObject effect = Instantiate(_effectObj, effectPos, Quaternion.identity);

        effect.GetComponent<Animator>().SetTrigger("SmallHitTrigger");

        CancellationTokenSource cts = new CancellationTokenSource();
        OffEffect(effect, 10, cts.Token).Forget();
    }

    private void OnLargeHitEffect(Vector2 effectPos)
    {
        OnSmallHitEffect(effectPos);

        GameObject effect = Instantiate(_effectObj, effectPos, Quaternion.identity);

        effect.GetComponent<Animator>().SetTrigger("LargeHitTrigger");

        CancellationTokenSource cts = new CancellationTokenSource();
        OffEffect(effect, 10, cts.Token).Forget();
    }

    private void OnBreakEffect(Vector2 effectPos)
    {
        GameObject effect = Instantiate(_effectObj, effectPos, Quaternion.identity);

        effect.GetComponent<Animator>().SetTrigger("BreakTrigger");

        CancellationTokenSource cts = new CancellationTokenSource();
        OffEffect(effect, 60, cts.Token).Forget();
    }

    private void OnRecoverBreakEffect(Vector2 effectPos)
    {
        GameObject effect = Instantiate(_effectObj, effectPos, Quaternion.identity);
        effect.GetComponent<Animator>().SetTrigger("RecoverBreakTrigger");

        CancellationTokenSource cts = new CancellationTokenSource();
        OffEffect(effect, 60, cts.Token).Forget();
    }

    private async UniTask OffEffect(GameObject effect, int activeFrame, CancellationToken token)
    {
        try
        {
            await FightingPhysics.DelayFrameWithTimeScale(activeFrame, cancellationToken: token);
        }
        catch(OperationCanceledException)
        {
            Debug.Log($"{effect.name}がキャンセル");
        }
        finally
        {
            if(effect) Destroy(effect);
        }
    }
}

public enum FightingEffect
{
    SmallHit,
    LargeHit,
    Guard,
    Break,
    RecoverBreak
}
