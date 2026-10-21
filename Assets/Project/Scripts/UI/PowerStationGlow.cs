using System.Collections;
using UnityEngine;
using Orbital.Core;
using Orbital.Activities;

namespace Orbital.UI
{
    /// <summary>Dims or brightens the Power station's indicator panels and local light as cells are placed.</summary>
    public class PowerStationGlow : MonoBehaviour
    {
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        [SerializeField] private PowerCellPuzzle _puzzle;
        [SerializeField] private Renderer[] _panels = System.Array.Empty<Renderer>();
        [SerializeField] private Light _light;
        [SerializeField] private Color _emissionColor = new Color(0f, 1.5f, 1.75f);
        [SerializeField] private float _maxLightIntensity = 1.2f;
        [SerializeField] private float _fadeSeconds = 1f;
        [SerializeField] private float _slotFadeSeconds = 0.5f;

        private StationGlowLogic _glow;
        private SocketGlowLogic[] _slotGlow;
        private MaterialPropertyBlock _block;
        private Coroutine _fade;

        private void Start()
        {
            _glow = new StationGlowLogic(_fadeSeconds);
            _slotGlow = new SocketGlowLogic[_panels.Length];
            for (int i = 0; i < _slotGlow.Length; i++) _slotGlow[i] = new SocketGlowLogic(_slotFadeSeconds);
            _block = new MaterialPropertyBlock();

            if (_puzzle != null) _puzzle.Tracker.Changed += OnSocketsChanged;

            ApplyLight(_glow.CurrentIntensity);
            for (int i = 0; i < _slotGlow.Length; i++) ApplyPanel(i, _slotGlow[i].CurrentIntensity);
        }

        private void OnDisable()
        {
            if (_puzzle != null) _puzzle.Tracker.Changed -= OnSocketsChanged;
            if (_fade != null)
            {
                StopCoroutine(_fade);
                _fade = null;
            }
        }

        private void OnSocketsChanged()
        {
            if (_fade != null) StopCoroutine(_fade);
            _fade = StartCoroutine(FadeToTarget());
        }

        private IEnumerator FadeToTarget()
        {
            while (true)
            {
                float lightTarget = StationGlowLogic.TargetIntensity(_puzzle.Tracker.FilledCount, _puzzle.Tracker.Total);
                float lightIntensity = _glow.Advance(_puzzle.Tracker.FilledCount, _puzzle.Tracker.Total, Time.deltaTime);
                ApplyLight(lightIntensity);
                bool settled = Mathf.Approximately(lightIntensity, lightTarget);

                for (int i = 0; i < _slotGlow.Length; i++)
                {
                    bool filled = _puzzle.Tracker.IsFilled(i);
                    float slotIntensity = _slotGlow[i].Advance(filled, Time.deltaTime);
                    ApplyPanel(i, slotIntensity);
                    settled &= Mathf.Approximately(slotIntensity, SocketGlowLogic.TargetIntensity(filled));
                }

                if (settled) break;
                yield return null;
            }

            _fade = null;
        }

        private void ApplyLight(float intensity)
        {
            if (_light != null) _light.intensity = intensity * _maxLightIntensity;
        }

        private void ApplyPanel(int index, float intensity)
        {
            var panel = _panels[index];
            if (panel == null) return;

            panel.GetPropertyBlock(_block);
            _block.SetColor(EmissionColorId, _emissionColor * intensity);
            panel.SetPropertyBlock(_block);
        }
    }
}
