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
            if (imagePath.StartsWith("images/")) imagePath = imagePath.Substring(7);
            string fileName = System.IO.Path.GetFileNameWithoutExtension(imagePath);
            Sprite imageSprite = Resources.Load<Sprite>($"Images/Events/{fileName}");
            if (imageSprite != null) {
                eventImage.sprite = imageSprite;
                eventImage.enabled = true;
            } else {
                eventImage.sprite = null;
                eventImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
                eventImage.enabled = true;
            }
        }
    }
}
