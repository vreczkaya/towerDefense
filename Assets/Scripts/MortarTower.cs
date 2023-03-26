using UnityEngine;

public class MortarTower : Tower
{
    [SerializeField, Range(0.5f, 2f)]
    private float _shootsPerSecond = 1f;

    [SerializeField, Range(0.5f, 3f)]
    private float _shellBlustRadius = 1f;

    [SerializeField, Range(1f, 100f)]
    private float _damage;

    [SerializeField]
    private Transform _mortar;

    public override TowerType Type => TowerType.Mortar;

    private float _launchSpeed;
    private float _launchProgress;

    private void Awake()
    {
        OnValidate();
    }

    private void OnValidate()
    {
        float x = _targetingRange + 0.25f;
        float y = -_mortar.position.y;
        _launchSpeed = Mathf.Sqrt(9.81f * (y + Mathf.Sqrt(x * x + y * y)));
    }

    public override void GameUpdate()
    {
        _launchProgress += Time.deltaTime * _shootsPerSecond;
        while (_launchProgress >= 1f)
        {
            if (IsAcquireTarget(out TargetPoint target))
            {
                Launch(target);
                _launchProgress -= 1f;
            }
            else
            {
                _launchProgress = 0.999f;
            }
        }
    }

    private void Launch(TargetPoint target)
    {
        Vector3 launchPoint = _mortar.position;
        Vector3 targetPoint = target.Position;
        targetPoint.y = 0f;

        Vector2 direction;
        direction.x = targetPoint.x - launchPoint.x;
        direction.y = targetPoint.z - launchPoint.z;

        float x = direction.magnitude;
        float y = -launchPoint.y;
        direction /= x;

        float g = 9.81f;
        float s = _launchSpeed;
        float s2 = s * s;

        float r = s2 * s2 - g * (g * x * x + 2f * y * s2);
        float tanTheta = (s2 + Mathf.Sqrt(r)) / (g * x);
        float cosTheta = Mathf.Cos(Mathf.Atan(tanTheta));
        float sinTheta = cosTheta * tanTheta;

        _mortar.localRotation = Quaternion.LookRotation(new Vector3(direction.x, tanTheta, direction.y));

        Game.SpawnShell().Initialize(launchPoint, targetPoint,
            new Vector3(s * cosTheta * direction.x, s * sinTheta, s * cosTheta * direction.y), _shellBlustRadius, _damage);
    }
}
