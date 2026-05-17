using UnityEngine;

namespace PushUslugi.Responsive
{
    [ExecuteAlways]
    public sealed class OrientationScaleSwitcher : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 landscapeScale = Vector3.one;
        [SerializeField] private Vector3 portraitScale = new Vector3(0.5f, 0.5f, 0.5f);
        [SerializeField] private bool applyInEditMode = true;

        private bool? lastIsPortrait;

        private void Reset()
        {
            target = transform;
        }

        private void OnEnable()
        {
            ApplyScale(true);
        }

        private void Start()
        {
            ApplyScale(true);
        }

        private void Update()
        {
            if (!Application.isPlaying && !applyInEditMode)
            {
                return;
            }

            ApplyScale(false);
        }

        private void OnValidate()
        {
            if (target == null)
            {
                target = transform;
            }

            ApplyScale(true);
        }

        private void ApplyScale(bool force)
        {
            var currentTarget = target != null ? target : transform;
            var isPortrait = IsPortraitOrientation();

            if (!force && lastIsPortrait.HasValue && lastIsPortrait.Value == isPortrait)
            {
                return;
            }

            currentTarget.localScale = isPortrait ? portraitScale : landscapeScale;
            lastIsPortrait = isPortrait;
        }

        private static bool IsPortraitOrientation()
        {
            if (Screen.height == Screen.width)
            {
                return Screen.orientation == ScreenOrientation.Portrait ||
                       Screen.orientation == ScreenOrientation.PortraitUpsideDown;
            }

            return Screen.height > Screen.width;
        }
    }
}
