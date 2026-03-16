using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace NoiseOverSilent.UI
{
    /// <summary>
    /// HotspotManager v1.0 - 2026-03-17
    /// Manages clickable hotspot circles on event images
    /// Shows text popups when clicked
    /// </summary>
    public class HotspotManager : MonoBehaviour
    {
        private RectTransform imageContainer;
        private GameObject hotspotPrefab;
        private GameObject textPopup;
        private TextMeshProUGUI popupText;
        private List<GameObject> activeHotspots = new List<GameObject>();
        
        public void Initialize(RectTransform imageContainerRect, Transform canvasTransform)
        {
            imageContainer = imageContainerRect;
            
            CreateHotspotPrefab(canvasTransform);
            CreateTextPopup(canvasTransform);
            
            Debug.Log("[HotspotManager v1.0] Initialized");
        }
        
        private void CreateHotspotPrefab(Transform parent)
        {
            hotspotPrefab = new GameObject("HotspotPrefab");
            hotspotPrefab.transform.SetParent(parent, false);
            hotspotPrefab.SetActive(false);
            
            RectTransform rect = hotspotPrefab.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(40, 40);
            
            // Circle background
            Image circle = hotspotPrefab.AddComponent<Image>();
            circle.sprite = CreateCircleSprite();
            circle.color = new Color(0.22f, 0.65f, 1f, 0.6f);
            circle.raycastTarget = true;
            
            // Make clickable
            Button btn = hotspotPrefab.AddComponent<Button>();
            
            // Add pulsing animation component
            hotspotPrefab.AddComponent<HotspotPulse>();
        }
        
        private void CreateTextPopup(Transform parent)
        {
            textPopup = new GameObject("HotspotTextPopup");
            textPopup.transform.SetParent(parent, false);
            
            RectTransform rect = textPopup.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(400, 150);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            
            // Background with border
            Image bg = textPopup.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.05f, 0.05f, 0.95f);
            
            Outline outline = textPopup.AddComponent<Outline>();
            outline.effectColor = new Color(0.22f, 0.65f, 1f, 1f);
            outline.effectDistance = new Vector2(2, -2);
            
            // Text content
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(textPopup.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(15, 15);
            textRect.offsetMax = new Vector2(-15, -15);
            
            popupText = textObj.AddComponent<TextMeshProUGUI>();
            popupText.fontSize = 14;
            popupText.color = new Color(0.78f, 0.82f, 0.85f, 1f);
            popupText.alignment = TextAlignmentOptions.TopLeft;
            popupText.textWrappingMode = TMPro.TextWrappingModes.Normal;
            
            textPopup.SetActive(false);
        }
        
        public void ShowHotspots(List<Data.Hotspot> hotspots)
        {
            ClearHotspots();
            
            if (hotspots == null || hotspots.Count == 0)
            {
                Debug.Log("[HotspotManager v1.0] No hotspots for this event");
                return;
            }
            
            foreach (var hotspot in hotspots)
            {
                CreateHotspot(hotspot);
            }
            
            Debug.Log($"[HotspotManager v1.0] Showing {hotspots.Count} hotspot(s)");
        }
        
        private void CreateHotspot(Data.Hotspot data)
        {
            GameObject hotspot = Instantiate(hotspotPrefab, imageContainer);
            hotspot.name = $"Hotspot_{data.x}_{data.y}";
            hotspot.SetActive(true);
            
            RectTransform rect = hotspot.GetComponent<RectTransform>();
            
            // Set anchor to position (normalized 0-1)
            rect.anchorMin = new Vector2(data.x, data.y);
            rect.anchorMax = new Vector2(data.x, data.y);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(data.radius * 2, data.radius * 2);
            
            // Set click handler
            Button btn = hotspot.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnHotspotClicked(data.text, rect));
            
            activeHotspots.Add(hotspot);
        }
        
        private void OnHotspotClicked(string text, RectTransform hotspotRect)
        {
            Debug.Log($"[HotspotManager v1.0] Hotspot clicked: {text.Substring(0, Mathf.Min(50, text.Length))}...");
            
            popupText.text = text;
            
            // Position popup above the hotspot
            RectTransform popupRect = textPopup.GetComponent<RectTransform>();
            
            // Get hotspot world position
            Vector3[] corners = new Vector3[4];
            hotspotRect.GetWorldCorners(corners);
            Vector2 hotspotWorldPos = (corners[0] + corners[2]) / 2;
            
            // Position popup above hotspot
            popupRect.position = hotspotWorldPos + new Vector2(0, 120);
            
            // Make sure popup stays on screen
            Canvas.ForceUpdateCanvases();
            Vector3[] popupCorners = new Vector3[4];
            popupRect.GetWorldCorners(popupCorners);
            
            // Clamp to screen bounds
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
            
            if (popupCorners[2].x > screenWidth)
            {
                popupRect.position += new Vector3(screenWidth - popupCorners[2].x - 10, 0, 0);
            }
            if (popupCorners[0].x < 0)
            {
                popupRect.position += new Vector3(-popupCorners[0].x + 10, 0, 0);
            }
            if (popupCorners[2].y > screenHeight)
            {
                popupRect.position += new Vector3(0, screenHeight - popupCorners[2].y - 10, 0);
            }
            
            textPopup.SetActive(true);
            
            // Auto-hide after delay
            StopAllCoroutines();
            StartCoroutine(AutoHidePopup(5f));
        }
        
        private IEnumerator AutoHidePopup(float delay)
        {
            yield return new WaitForSeconds(delay);
            textPopup.SetActive(false);
        }
        
        public void HidePopup()
        {
            textPopup.SetActive(false);
            StopAllCoroutines();
        }
        
        public void ClearHotspots()
        {
            foreach (var hotspot in activeHotspots)
            {
                if (hotspot != null) Destroy(hotspot);
            }
            activeHotspots.Clear();
            HidePopup();
        }
        
        private Sprite CreateCircleSprite()
        {
            int size = 64;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];
            
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float radius = size / 2f - 2f;
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    pixels[y * size + x] = distance < radius ? Color.white : Color.clear;
                }
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            texture.filterMode = FilterMode.Bilinear;
            
            return Sprite.Create(
                texture,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f),
                100f
            );
        }
    }
    
    /// <summary>
    /// HotspotPulse v1.0 - 2026-03-17
    /// Animates hotspot circles with pulsing glow effect
    /// </summary>
    public class HotspotPulse : MonoBehaviour
    {
        private Image image;
        private float timer = 0f;
        private float speed = 2f;
        
        void Start()
        {
            image = GetComponent<Image>();
            if (image == null)
            {
                Debug.LogWarning("[HotspotPulse] No Image component found!");
                enabled = false;
            }
        }
        
        void Update()
        {
            if (image == null) return;
            
            timer += Time.deltaTime * speed;
            float alpha = Mathf.Lerp(0.3f, 0.8f, (Mathf.Sin(timer) + 1f) / 2f);
            
            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }
    }
}