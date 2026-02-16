// ============================================================
// PROJECT : Noise Over Silence
// FILE    : ImageDisplay.cs
// PATH    : Assets/Scripts/UI/
// CREATED : 2026-02-14
// AUTHOR  : Noise Over Silence Dev Team
// DESC    : Displays fullscreen event images loaded from
//           Assets/Resources/Images/Events/.
//           Strips "images/" prefix and file extension
//           before calling Resources.Load.
// ============================================================

using UnityEngine;
using UnityEngine.UI;

namespace NoiseOverSilent.UI
{
    public class ImageDisplay : MonoBehaviour
    {
        [SerializeField] private Image eventImage;

        /// <summary>
        /// Loads and displays a sprite from Resources/Images/Events/.
        /// imagePath format: "images/ep1_event1.png"
        /// </summary>
        public void DisplayImage(string imagePath)
        {
            if (eventImage == null)
            {
                Debug.LogWarning("[ImageDisplay] eventImage reference is null.");
                return;
            }

            if (string.IsNullOrEmpty(imagePath))
            {
                SetPlaceholder();
                return;
            }

            // Strip "images/" prefix
            string path = imagePath;
            if (path.StartsWith("images/"))
                path = path.Substring(7);

            // Strip file extension
            int dot = path.LastIndexOf('.');
            if (dot >= 0)
                path = path.Substring(0, dot);

            // Load from Assets/Resources/Images/Events/
            Sprite sprite = Resources.Load<Sprite>("Images/Events/" + path);

            if (sprite != null)
            {
                eventImage.sprite = sprite;
                eventImage.color  = Color.white;
                Debug.Log($"[ImageDisplay] Loaded: Images/Events/{path}");
            }
            else
            {
                Debug.LogWarning($"[ImageDisplay] Not found: Images/Events/{path}");
                SetPlaceholder();
            }
        }

        /// <summary>Shows a dark placeholder when no image is available.</summary>
        private void SetPlaceholder()
        {
            eventImage.sprite = null;
            eventImage.color  = new Color(0.08f, 0.08f, 0.08f, 1f);
        }
    }
}
