using UnityEngine;
using UnityEngine.Events;

namespace Orbital.Interaction
{
    /// <summary>Reads a lever handle's rotation and reports it as a normalized 0..1 value.</summary>
    public class LeverReader : MonoBehaviour
    {
        [SerializeField] private Transform _handle;
        [SerializeField] private Vector3 _localAxis = Vector3.right;
        [SerializeField] private float _minAngle;
        [SerializeField] private float _maxAngle;
        [SerializeField] private UnityEvent<float> _onChanged;

        private float _lastValue = -1f;

        private void Update()
        {
            Vector3 reference = PerpendicularTo(_localAxis);
            Vector3 rotatedReference = _handle.localRotation * reference;
            float angle = Vector3.SignedAngle(reference, rotatedReference, _localAxis);

            float value = Mathf.Clamp01(Mathf.InverseLerp(_minAngle, _maxAngle, angle));

            if (Mathf.Abs(value - _lastValue) > 0.01f)
            {
                _lastValue = value;
                _onChanged?.Invoke(value);
            }
        }

        private static Vector3 PerpendicularTo(Vector3 axis)
        {
            Vector3 candidate = Mathf.Abs(Vector3.Dot(axis.normalized, Vector3.up)) > 0.9f
                ? Vector3.forward
                : Vector3.up;
            return Vector3.ProjectOnPlane(candidate, axis).normalized;
        }
    }
}
