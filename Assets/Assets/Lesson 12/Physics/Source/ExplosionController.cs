using UnityEngine;

public class ExplosionController : MonoBehaviour
{
    [SerializeField] private ParticleSystem _sphere;
    [SerializeField] private ParticleSystem _sparks;

    private ParticleSystem.MainModule _sphereMain;
    private ParticleSystem.ShapeModule _sparksShape;

    public void ApplyRadius(float radius)
    {
        _sphereMain = _sphere.main;
        _sparksShape = _sparks.shape;

        _sphereMain.startSize = radius * 2f;
        _sparksShape.radius = radius * 4f / 3f;
    }

    public void Play()
    {
        _sphere.Play(true);
    }
}
