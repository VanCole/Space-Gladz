using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCore : MonoBehaviour
{
    protected Vector3 shakeOffset = Vector3.zero;
    private Coroutine shakeCoroutine = null;
    private bool shakeActivated = false;
    
    private Coroutine slowCoroutine = null;
    private bool slowActivated = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12))
        {
            Shake(!shakeActivated);
        }

        if (Input.GetKeyDown(KeyCode.F11))
        {
            Shake(true, 0.8f, 0.8f, 0.0f, 1.0f);
        }

        if (Input.GetKeyDown(KeyCode.F10))
        {
            SlowMotion(0.5f, 2.0f, 0.0f, 0.5f);
        }
    }


    /// <summary>
    /// Shake the camera. Stop previous shaking.
    /// </summary>
    /// <param name="_activate">Active or deactive the shaking manually.</param>
    /// <param name="_amplitude">Set the maximum amplitude of the shaking in world unit.</param>
    /// <param name="_duration">Duration of the shaking in seconds. Deactivate automatically.</param>
    /// <param name="_fadeIn">Normalized duration of fade in to smooth the beggining. Between 0 and 1.</param>
    /// <param name="_fadeOut">Normalized duration of fade out to smooth the ending. Between 0 and 1.</param>
    public void Shake(bool _activate = true, float _amplitude = 1.0f, float _duration = -1.0f, float _fadeIn = 0.0f, float _fadeOut = 0.0f)
    {
        //Debug.Log("Shake : " + _activate);
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }
        shakeActivated = _activate;
        if (_activate)
        {
            shakeCoroutine = StartCoroutine(ShakeCoroutine(_amplitude, _duration, _fadeIn, _fadeOut));
        }
    }

    /// <summary>
    /// Apply a sow motion to the game.
    /// </summary>
    /// <param name="_factor">Slow factor. Value of 1 is normal speed.</param>
    /// <param name="_duration">Duration of the slow in seconds.</param>
    /// <param name="_fadeIn">Normalized duration of fade in to smooth the beggining. Between 0 and 1.</param>
    /// <param name="_fadeOut">Normalized duration of fade out to smooth the ending. Between 0 and 1.</param>
    public void SlowMotion(float _factor = 1.0f, float _duration = -1.0f, float _fadeIn = 0.0f, float _fadeOut = 0.0f)
    {
        //Debug.Log("Slow Motion : " + _factor);
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }
        slowActivated = _factor != 1.0f;
        slowCoroutine = StartCoroutine(SlowMotionCoroutine(_factor, _duration, _fadeIn, _fadeOut));
    }


    private IEnumerator ShakeCoroutine(float _amplitude, float _duration, float _fadeIn, float _fadeOut)
    {

        float timer = 0.0f;
        float amp = 1.0f, ampIn = 0.0f, ampOut = 0.0f;
        Vector2 rand = Vector2.zero;

        _fadeIn = Mathf.Clamp(_fadeIn, 0.0f, 1.0f);
        _fadeOut = Mathf.Clamp(_fadeOut, 0.0f, 1.0f);

        while (shakeActivated)
        {
            if (_duration > 0)
            {
                timer += Time.deltaTime / _duration;

                if (timer >= 1.0f)
                {
                    shakeActivated = false;
                }

                ampIn = (_fadeIn > 0.0f) ? timer / _fadeIn : 1.0f;
                ampOut = (_fadeOut > 0.0f) ? (1.0f - timer) / _fadeOut : 1.0f;

                amp = Mathf.Min(ampIn, ampOut);
            }


            rand.x = Random.Range(-1.0f, 1.0f);
            rand.y = Random.Range(-1.0f, 1.0f);
            rand.Normalize();

            shakeOffset = _amplitude * amp * (transform.right * rand.x + transform.up * rand.y);


            yield return 0;
        }

        shakeCoroutine = null;
    }


    private IEnumerator SlowMotionCoroutine(float _factor, float _duration, float _fadeIn, float _fadeOut)
    {

        float timer = 0.0f;
        float amp = 1.0f, ampIn = 0.0f, ampOut = 0.0f;
        Vector2 rand = Vector2.zero;

        _fadeIn = Mathf.Clamp(_fadeIn, 0.0f, 1.0f);
        _fadeOut = Mathf.Clamp(_fadeOut, 0.0f, 1.0f);

        while (slowActivated)
        {
            if (_duration > 0)
            {
                timer += Time.unscaledDeltaTime / _duration;

                if (timer >= 1.0f)
                {
                    slowActivated = false;
                }

                ampIn = (_fadeIn > 0.0f) ? timer / _fadeIn : 1.0f;
                ampOut = (_fadeOut > 0.0f) ? (1.0f - timer) / _fadeOut : 1.0f;

                amp = Mathf.Min(ampIn, ampOut);
            }

            float timeScale = 1.0f + (_factor - 1.0f) * amp;
            if (timeScale > 0.1f)
            {
                Time.timeScale = timeScale;
            }
            
            yield return 0;
        }

        slowCoroutine = null;
    }
}
