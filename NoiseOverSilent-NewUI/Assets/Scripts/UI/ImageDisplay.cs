using UnityEngine;
using UnityEngine.UI;

namespace NoiseOverSilent.UI
{
    public class ImageDisplay : MonoBehaviour
    {
        [SerializeField] private Image eventImage;

        public void DisplayImage(string imagePath)
        {
            if (eventImage == null) return;

            if (string.IsNullOrEmpty(imagePath))
            {
                eventImage.color = new Color(0.08f, 0.08f, 0.08f, 1f);
                return;
            }

            // Strip "images/" prefix and file extension
            string path = imagePath;
            if (path.StartsWith("images/"))
                path = path.Substring(7);

            int dot = path.LastIndexOf('.');
            if (dot >= 0)
                path = path.Substring(0, dot);

            // Load from Assets/Resources/Images/Events/
            Sprite sprite = Resources.Load<Sprite>("Images/Events/" + path);

            if (sprite != null)
            {
                eventImage.sprite = sprite;
                eventImage.color = Color.white;
            }
            else
            {
                Debug.LogWarning($"Image not found: Images/Events/{path}");
                eventImage.sprite = null;
                eventImage.color = new Color(0.08f, 0.08f, 0.08f, 1f);
            }
        }
    }
}
