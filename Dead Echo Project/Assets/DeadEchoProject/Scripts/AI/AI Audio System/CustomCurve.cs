using UnityEngine;

[CreateAssetMenu(fileName = "New Custom Event Curve", menuName = "Dead Echo/Audio/Custom Audio Event")]
public class CustomCurve : ScriptableObject
{
    [SerializeField] AnimationCurve _curve = new AnimationCurve(new Keyframe(0,0), new Keyframe(1,0));

    public float Evaluate (float t) => _curve.Evaluate(t);
}