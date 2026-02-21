// ============================================================
// PROJECT : Noise Over Silence
// FILE    : TypewriterEffect.cs
// PATH    : Assets/Scripts/UI/
// CREATED : 2026-02-16
// VERSION : 1.4
// CHANGES : v1.4 - 2026-02-21 - Speed increased to 30 chars/sec (2x faster)
//           v1.3 - 2026-02-21 - More detailed debug logs
//           v1.2 - 2026-02-21 - Added debug log
//           v1.1 - 2026-02-16 - Slower speed (15 chars/sec) for visibility
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
        [SerializeField] private float charsPerSecond = 30f; // 2x faster (was 15)
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
            
            Debug.Log($"[TypewriterEffect v1.2] Starting typewriter for {fullText.Length} chars at {charsPerSecond} chars/sec");
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
            Debug.Log($"[TypewriterEffect v1.3] Coroutine started! Text length: {fullText.Length}");
            
            textComponent.text = "";
            textComponent.maxVisibleCharacters = 0;
            
            // Set the full text first (for layout)
            textComponent.text = fullText;
            
            int totalChars = fullText.Length;
            float delay = 1f / charsPerSecond;
            
            Debug.Log($"[TypewriterEffect v1.3] Delay per char: {delay}s (speed: {charsPerSecond} chars/sec)");
            
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
            
            Debug.Log($"[TypewriterEffect v1.3] Typing complete!");
            typeCoroutine = null;
            onComplete?.Invoke();
        }
    }
}