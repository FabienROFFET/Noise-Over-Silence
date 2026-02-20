// ============================================================
// PROJECT : Noise Over Silence
// FILE    : TypewriterEffect.cs
// PATH    : Assets/Scripts/UI/
// CREATED : 2026-02-16
// VERSION : 1.1
// CHANGES : v1.1 - 2026-02-16 - Slower speed (15 chars/sec) for visibility
//           v1.0 - 2026-02-16 - Initial version
// DESC    : Animates text character-by-character with typing sound
// ============================================================

using System.Collections;
using UnityEngine;
using TMPro;

namespace NoiseOverSilent.UI
{
    public class TypewriterEffect : MonoBehaviour
    {
        [SerializeField] private float charsPerSecond = 15f; // Slower = more visible
        [SerializeField] private AudioSource typingAudio;
        
        private TextMeshProUGUI textComponent;
        private Coroutine typeCoroutine;

        private void Awake()
        {
            textComponent = GetComponent<TextMeshProUGUI>();
        }

        public void TypeText(string fullText, System.Action onComplete = null)
        {
            if (typeCoroutine != null)
                StopCoroutine(typeCoroutine);
            
            typeCoroutine = StartCoroutine(TypeTextCoroutine(fullText, onComplete));
        }

        public void SkipToEnd()
        {
            if (typeCoroutine != null)
            {
                StopCoroutine(typeCoroutine);
                typeCoroutine = null;
            }
        }

        private IEnumerator TypeTextCoroutine(string fullText, System.Action onComplete)
        {
            textComponent.text = "";
            textComponent.maxVisibleCharacters = 0;
            
            // Set the full text first (for layout)
            textComponent.text = fullText;
            
            int totalChars = fullText.Length;
            float delay = 1f / charsPerSecond;
            
            for (int i = 0; i <= totalChars; i++)
            {
                textComponent.maxVisibleCharacters = i;
                
                // Play typing sound occasionally (not every char, too annoying)
                if (i % 3 == 0 && typingAudio != null && !typingAudio.isPlaying)
                {
                    typingAudio.pitch = Random.Range(0.95f, 1.05f);
                    typingAudio.Play();
                }
                
                yield return new WaitForSeconds(delay);
            }
            
            typeCoroutine = null;
            onComplete?.Invoke();
        }
    }
}