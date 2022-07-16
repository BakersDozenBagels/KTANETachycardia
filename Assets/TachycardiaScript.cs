using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TachycardiaScript : MonoBehaviour
{
    [SerializeField]
    private KMSelectable _heart;
    [SerializeField]
    private Renderer _heartRenderer;
    [SerializeField]
    private Material _heartMat, _HLMat;
    [SerializeField]
    private TachycardiaEnemyScript _enemy;
    [SerializeField]
    private KMAudio _audio;
    [SerializeField]
    private KMBombModule _module;
    [SerializeField]
    private KMBombInfo _info;

    private bool _defib = false, _isSolved = false;
    private Coroutine _routine = null;
    private float _lastHeld = -1f;
    private static int _idc, _strikesToIgnore = 0;
    private int _id = ++_idc, _strikes = 0;
    private float _nextAttack;
    private Action _onStrike = () => { };

    private void Start()
    {
        Debug.LogFormat("[Tachycardia #{0}] Good luck. You'll need it...", _id);
        _heart.OnHighlight += () => { _heartRenderer.material = _HLMat; };
        _heart.OnHighlightEnded += () => { _heartRenderer.material = _heartMat; };

        StartCoroutine(PlaySounds());
        StartCoroutine(SendEnemies());

        _heart.OnInteract += Hold;
        _heart.OnInteractEnded += Release;
    }

    private IEnumerator SendEnemies()
    {
        _nextAttack = 90f * UnityEngine.Random.Range(0.8f, 1.2f);
        yield return new WaitForSeconds(_nextAttack);
        while(!_isSolved)
        {
            _routine = StartCoroutine(MoveEnemy());
            _nextAttack = Mathf.Max(7f, UnityEngine.Random.Range(8f, 12f) * _info.GetTime() / 60f);
            yield return new WaitForSeconds(_nextAttack);
        }
    }

    private void Update()
    {
        if(_info.GetStrikes() > _strikes && _strikesToIgnore == 0)
            _onStrike();
        _strikes = _info.GetStrikes();
    }

    private IEnumerator WaitShortly(int count)
    {
        yield return null;
        _strikesToIgnore = 1;
        for(int i = 0; i < count; i++)
            _module.HandleStrike();
        yield return null;
        _strikesToIgnore = 0;
    }

    private void Release()
    {
        if(_lastHeld == -1f)
            return;

        if(Time.time - _lastHeld >= 5f)
        {
            Debug.LogFormat("[Tachycardia #{0}] Enter panic mode.", _id);
            _module.HandlePass();
            _audio.PlaySoundAtTransform("HeartBeat", _heart.transform);
            _isSolved = true;
            _nextAttack = float.PositiveInfinity;
            _onStrike += () => StartCoroutine(WaitShortly(2));

            _defib = false;
            if(_routine != null)
            {
                StopCoroutine(_routine);
                _routine = null;
            }
            _enemy.gameObject.SetActive(false);
        }
        _lastHeld = -1f;
    }

    private bool Hold()
    {
        _heart.AddInteractionPunch(1f);
        _audio.PlaySoundAtTransform("Correct", _heart.transform);

        if(_isSolved)
            return false;

        _lastHeld = Time.time;

        if(!_defib)
        {
            _module.HandleStrike();
            return false;
        }

        _defib = false;
        if(_routine != null)
        {
            StopCoroutine(_routine);
            _routine = null;
        }
        _enemy.gameObject.SetActive(false);

        return false;
    }

    private IEnumerator PlaySounds()
    {
        yield return null;
        while(true)
        {
            _audio.PlaySoundAtTransform("HeartMonitor", _heart.transform);
            yield return new WaitForSeconds(60.995f);
        }
    }

    private IEnumerator MoveEnemy()
    {
        const float DELAY = 7f;
        _audio.PlaySoundAtTransform("HeartBeat2", _heart.transform);
        _enemy.gameObject.SetActive(true);
        float time = Time.time;
        while(Time.time - time < DELAY)
        {
            _enemy.SetMotion((DELAY - Time.time + time) / DELAY);
            if((DELAY - Time.time + time) / DELAY < 0.5f)
                _defib = true;
            yield return null;
        }
        _enemy.gameObject.SetActive(false);
        if(_defib)
        {
            _module.HandleStrike();
            _defib = false;
        }
    }
}
