using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class WaterCombatController : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] private Animator _Anim;
    [SerializeField] private WaterBallControll waterBallController;
    [SerializeField] private WaterBender waterBenderController;
    [SerializeField] private WaterTubeController waterTubeController;

    [Header("Turn")]
    [SerializeField] private float _TurnSpeed = 12f;

    [Header("Hotkeys (do NOT use LMB/RMB/WASD)")]
    [SerializeField] private Key _waterBallKey = Key.Digit1;
    [SerializeField] private Key _waterBendKey = Key.Digit2;
    [SerializeField] private Key _waterTubeKey = Key.Digit3;

    [Header("Enable/Disable")]
    [SerializeField] private bool _enabled = true;

    private Vector3 _waterBallTarget;
    private Vector3 _waterBendTarget;
    private Vector3 _waterTubeTarget;

    private Coroutine _castRoutine;

    private void Update()
    {
        if (!_enabled) return;

        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb[_waterBallKey].wasPressedThisFrame)
            StartCast(CastType.WaterBall);

        if (kb[_waterBendKey].wasPressedThisFrame)
            StartCast(CastType.WaterBend);

        if (kb[_waterTubeKey].wasPressedThisFrame)
            StartCast(CastType.WaterTube);
    }

    public void SetEnabled(bool value)
    {
        _enabled = value;
        if (!value && _castRoutine != null)
        {
            StopCoroutine(_castRoutine);
            _castRoutine = null;
        }
    }

    private enum CastType { WaterBall, WaterBend, WaterTube }

    private void StartCast(CastType type)
    {
        if (_castRoutine != null)
        {
            StopCoroutine(_castRoutine);
            _castRoutine = null;
        }

        _castRoutine = StartCoroutine(CastRoutine(type));
    }

    private IEnumerator CastRoutine(CastType type)
    {
        // 1) берём точку под курсором (без клика мыши!)
        bool hasPoint = TryGetMouseWorldPoint(out Vector3 point);

        // 2) поворот к точке (если точка есть)
        if (hasPoint)
            yield return StartCoroutine(Coroutine_Turn(point));

        // 3) триггерим анимацию + сохраняем таргет для Animation Event
        switch (type)
        {
            case CastType.WaterBall:
                if (!waterBallController.WaterBallCreated())
                {
                    _Anim.SetTrigger("CreateWaterBall");
                }
                else if (hasPoint)
                {
                    _waterBallTarget = point;
                    _Anim.SetTrigger("ThrowWaterBall");
                }
                break;

            case CastType.WaterBend:
                if (hasPoint)
                {
                    _waterBendTarget = point;
                    _Anim.SetTrigger("WaterBend");
                }
                break;

            case CastType.WaterTube:
                if (hasPoint)
                {
                    _waterTubeTarget = point;
                    _Anim.SetTrigger("WaterTube");
                }
                break;
        }

        _castRoutine = null;
    }

    private bool TryGetMouseWorldPoint(out Vector3 point)
    {
        point = default;

        var cam = Camera.main;
        var mouse = Mouse.current;
        if (cam == null || mouse == null) return false;

        Vector2 screen = mouse.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(screen);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            point = hit.point;
            return true;
        }

        return false;
    }

    private IEnumerator Coroutine_Turn(Vector3 targetPoint)
    {
        Vector3 direction = (targetPoint - transform.position);
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f)
            yield break;

        direction.Normalize();

        Vector3 startForward = transform.forward;
        float angle = Vector3.Angle(startForward, direction);
        if (angle < 0.01f)
            yield break;

        _Anim.SetFloat("Turn", Vector3.Cross(startForward, direction).y);

        float t = 0f;
        while (t < 1f)
        {
            transform.forward = Vector3.Slerp(startForward, direction, t);
            t += Time.deltaTime * _TurnSpeed / Mathf.Max(angle, 0.001f);
            yield return null;
        }

        transform.forward = direction;
        _Anim.SetFloat("Turn", 0f);
    }

    // === Animation Events (оставляй как у тебя в клипах) ===
    private void AnimationCallback_CreateWaterBall()
    {
        if (!waterBallController.WaterBallCreated())
            waterBallController.CreateWaterBall();
    }

    private void AnimationCallback_ThrowBall()
    {
        if (waterBallController.WaterBallCreated())
            waterBallController.ThrowWaterBall(_waterBallTarget);
    }

    private void AnimationCallback_WaterBend()
    {
        waterBenderController.Attack(_waterBendTarget);
    }

    private void AnimationCallback_WaterTube()
    {
        waterTubeController.InstantiateWaterTube(_waterTubeTarget);
    }
}