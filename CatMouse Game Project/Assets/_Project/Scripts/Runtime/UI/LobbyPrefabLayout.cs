using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CatMouse.Game.UI
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class LobbyPrefabLayout : MonoBehaviour
    {
        [Header("기준")]
        [SerializeField, Min(1)] private int _gridUnit;
        [SerializeField] private Vector2 _referenceResolution;

        [Header("레이아웃 요소")]
        [SerializeField] private LobbyLayoutElement[] _elements = Array.Empty<LobbyLayoutElement>();

        [SerializeField] private Button[] _buttons = Array.Empty<Button>();

        [NonSerialized] private bool _isApplying;

        public int GridUnit => _gridUnit;
        public Vector2 ReferenceResolution => _referenceResolution;
        public IReadOnlyList<LobbyLayoutElement> Elements => _elements;
        public IReadOnlyList<Button> Buttons => _buttons;

        public void Initialize(int gridUnit, Vector2 referenceResolution)
        {
            _gridUnit = Mathf.Max(1, gridUnit);
            _referenceResolution = referenceResolution;
        }

        public void CaptureFromHierarchy()
        {
            RectTransform[] rectTransforms = GetComponentsInChildren<RectTransform>(true);
            List<LobbyLayoutElement> capturedElements = new(rectTransforms.Length);

            for (int index = 0; index < rectTransforms.Length; index++)
            {
                RectTransform rectTransform = rectTransforms[index];
                if (rectTransform == transform || FindLayoutOwner(rectTransform) != this)
                {
                    continue;
                }

                capturedElements.Add(LobbyLayoutElement.Capture(rectTransform, GetRelativePath(rectTransform)));
            }

            _elements = capturedElements.ToArray();
            _buttons = CaptureOwnedButtons();
            SnapLayoutValues();
        }

        public void ApplyLayout()
        {
            if (_isApplying)
            {
                return;
            }

            _isApplying = true;
            for (int index = 0; index < _elements.Length; index++)
            {
                _elements[index].Apply();
            }

            _isApplying = false;
        }

        public void SnapLayoutValues()
        {
            if (_gridUnit <= 0)
            {
                return;
            }

            _referenceResolution = Snap(_referenceResolution);
            for (int index = 0; index < _elements.Length; index++)
            {
                _elements[index].Snap(_gridUnit);
            }
        }

        private void OnValidate()
        {
            SnapLayoutValues();
        }

        private string GetRelativePath(Transform target)
        {
            List<string> segments = new();
            Transform current = target;
            while (current != null && current != transform)
            {
                segments.Add(current.name);
                current = current.parent;
            }

            segments.Reverse();
            return string.Join("/", segments);
        }

        private Button[] CaptureOwnedButtons()
        {
            Button[] buttons = GetComponentsInChildren<Button>(true);
            List<Button> ownedButtons = new(buttons.Length);

            for (int index = 0; index < buttons.Length; index++)
            {
                Button button = buttons[index];
                if (FindLayoutOwner(button.transform) == this)
                {
                    ownedButtons.Add(button);
                }
            }

            return ownedButtons.ToArray();
        }

        private static LobbyPrefabLayout FindLayoutOwner(Transform target)
        {
            for (Transform current = target; current != null; current = current.parent)
            {
                LobbyPrefabLayout layout = current.GetComponent<LobbyPrefabLayout>();
                if (layout != null)
                {
                    return layout;
                }
            }

            return null;
        }

        private Vector2 Snap(Vector2 value)
        {
            return new Vector2(Snap(value.x), Snap(value.y));
        }

        private float Snap(float value)
        {
            return Mathf.Round(value / _gridUnit) * _gridUnit;
        }
    }

    [Serializable]
    public sealed class LobbyLayoutElement
    {
        [SerializeField] private string _path;
        [SerializeField] private RectTransform _target;
        [SerializeField] private Vector2 _anchorMin;
        [SerializeField] private Vector2 _anchorMax;
        [SerializeField] private Vector2 _pivot;
        [SerializeField] private Vector2 _anchoredPosition;
        [SerializeField] private Vector2 _sizeDelta;

        public string Path => _path;
        public RectTransform Target => _target;

        public static LobbyLayoutElement Capture(RectTransform target, string path)
        {
            return new LobbyLayoutElement
            {
                _path = path,
                _target = target,
                _anchorMin = target.anchorMin,
                _anchorMax = target.anchorMax,
                _pivot = target.pivot,
                _anchoredPosition = target.anchoredPosition,
                _sizeDelta = target.sizeDelta,
            };
        }

        public void Apply()
        {
            if (_target == null)
            {
                return;
            }

            _target.anchorMin = _anchorMin;
            _target.anchorMax = _anchorMax;
            _target.pivot = _pivot;
            _target.anchoredPosition = _anchoredPosition;
            _target.sizeDelta = _sizeDelta;
        }

        public void Snap(int gridUnit)
        {
            _anchoredPosition = Snap(_anchoredPosition, gridUnit);
            _sizeDelta = Snap(_sizeDelta, gridUnit);
        }

        private static Vector2 Snap(Vector2 value, int gridUnit)
        {
            return new Vector2(Snap(value.x, gridUnit), Snap(value.y, gridUnit));
        }

        private static float Snap(float value, int gridUnit)
        {
            return Mathf.Round(value / gridUnit) * gridUnit;
        }
    }
}
