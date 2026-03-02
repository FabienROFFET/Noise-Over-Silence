#!/usr/bin/env python3
"""
Auto-generate all Noise Over Silence images from story JSON
Extracts image_prompt fields and generates 16-bit pixel art

Usage:
    python generate_all_images.py [path/to/your/story.json]
    
If no path provided, looks for episode01_STORY_en.json in current directory
"""

import json
import time
import requests
import sys
from pathlib import Path

# Configuration
COMFYUI_URL = "http://127.0.0.1:8188"

# Get JSON file path from command line argument or use default
if len(sys.argv) > 1:
    JSON_FILE = sys.argv[1]
else:
    JSON_FILE = "episode01_en.json"

# Unity project path - update this to your actual path
UNITY_OUTPUT_DIR = Path("C:/Users/fabien/Documents/GitHub/Noise-Over-Silence/NoiseOverSilent-NewUI/Assets/Resources/Images/Events")

# Style additions
STYLE_PREFIX = "16-bit pixel art, retro game graphics, detailed pixel work, "
STYLE_SUFFIX = ", limited color palette, nostalgic aesthetic, sharp pixels, video game art style"
NEGATIVE_PROMPT = "blurry, 3D render, photograph, smooth gradients, anti-aliasing, modern graphics, high resolution, photorealistic"

# ComfyUI workflow template
WORKFLOW_TEMPLATE = {
    "prompt": {
        "3": {
            "inputs": {
                "seed": 0,
                "steps": 25,
                "cfg": 7,
                "sampler_name": "dpmpp_2m",
                "scheduler": "karras",
                "denoise": 1,
                "model": ["4", 0],
                "positive": ["6", 0],
                "negative": ["7", 0],
                "latent_image": ["5", 0]
            },
            "class_type": "KSampler"
        },
        "4": {
            "inputs": {
                "ckpt_name": "dreamshaper_8.safetensors"
            },
            "class_type": "CheckpointLoaderSimple"
        },
        "5": {
            "inputs": {
                "width": 1024,
                "height": 576,
                "batch_size": 1
            },
            "class_type": "EmptyLatentImage"
        },
        "6": {
            "inputs": {
                "text": "",  # Will be filled with prompt
                "clip": ["4", 1]
            },
            "class_type": "CLIPTextEncode"
        },
        "7": {
            "inputs": {
                "text": NEGATIVE_PROMPT,
                "clip": ["4", 1]
            },
            "class_type": "CLIPTextEncode"
        },
        "8": {
            "inputs": {
                "samples": ["3", 0],
                "vae": ["4", 2]
            },
            "class_type": "VAEDecode"
        },
        "9": {
            "inputs": {
                "filename_prefix": "ep1_story_event",
                "images": ["8", 0]
            },
            "class_type": "SaveImage"
        }
    }
}


def load_story_json(filepath):
    """Load the story JSON file"""
    try:
        with open(filepath, 'r', encoding='utf-8') as f:
            return json.load(f)
    except FileNotFoundError:
        print(f"❌ Error: Could not find {filepath}")
        print(f"   Place your JSON file in the same directory as this script")
        sys.exit(1)
    except json.JSONDecodeError as e:
        print(f"❌ Error: Invalid JSON - {e}")
        sys.exit(1)


def extract_prompts(story_data):
    """Extract all image prompts from events"""
    prompts = []
    events = story_data.get('events', [])
    
    for event in events:
        event_id = event.get('id')
        image_prompt = event.get('image_prompt', '')
        image_link = event.get('image_link', '')  # Get the exact filename from JSON
        
        if image_prompt and image_link:
            # Extract filename without extension and path
            # "images/ep1_story_event1.png" -> "ep1_story_event1"
            filename = Path(image_link).stem
            
            # Add 16-bit pixel art style
            full_prompt = f"{STYLE_PREFIX}{image_prompt}{STYLE_SUFFIX}"
            
            prompts.append({
                'id': event_id,
                'filename': filename,  # Use exact filename from JSON
                'prompt': full_prompt
            })
    
    return prompts


def queue_prompt(prompt_data):
    """Send prompt to ComfyUI"""
    workflow = WORKFLOW_TEMPLATE.copy()
    workflow['prompt']['6']['inputs']['text'] = prompt_data['prompt']
    workflow['prompt']['9']['inputs']['filename_prefix'] = prompt_data['filename']
    
    try:
        response = requests.post(
            f"{COMFYUI_URL}/prompt",
            json={"prompt": workflow['prompt']}
        )
        
        if response.status_code == 200:
            return response.json()
        else:
            print(f"❌ Error queuing prompt: {response.status_code}")
            return None
            
    except requests.exceptions.ConnectionError:
        print(f"❌ Error: Cannot connect to ComfyUI at {COMFYUI_URL}")
        print(f"   Make sure ComfyUI is running!")
        sys.exit(1)


def wait_for_completion(prompt_id):
    """Wait for image generation to complete"""
    while True:
        try:
            response = requests.get(f"{COMFYUI_URL}/history/{prompt_id}")
            if response.status_code == 200:
                history = response.json()
                if prompt_id in history:
                    return True
            time.sleep(1)
        except:
            time.sleep(1)


def main():
    print("=" * 60)
    print("  NOISE OVER SILENCE - AUTO IMAGE GENERATOR")
    print("  16-bit Pixel Art Style")
    print("=" * 60)
    print()
    
    # Show configuration
    print(f"📝 JSON File: {JSON_FILE}")
    print(f"📁 Output Folder: {UNITY_OUTPUT_DIR}")
    print(f"🌐 ComfyUI URL: {COMFYUI_URL}")
    print()
    
    # Check if ComfyUI is running
    try:
        requests.get(COMFYUI_URL)
    except:
        print("❌ Error: ComfyUI is not running!")
        print(f"   Start ComfyUI first, then run this script")
        print(f"   Expected URL: {COMFYUI_URL}")
        sys.exit(1)
    
    print("✅ ComfyUI is running")
    print()
    
    # Load story data
    print(f"📖 Loading story from: {JSON_FILE}")
    story_data = load_story_json(JSON_FILE)
    
    # Extract prompts
    prompts = extract_prompts(story_data)
    print(f"✅ Found {len(prompts)} events with image prompts")
    print()
    
    # Create Unity output directory
    UNITY_OUTPUT_DIR.mkdir(parents=True, exist_ok=True)
    print(f"📁 Output folder: {UNITY_OUTPUT_DIR}")
    print()
    
    # Generate images
    print("🎨 Starting image generation...")
    print(f"   Style: 16-bit pixel art")
    print(f"   Size: 1024x576")
    print(f"   Estimated time: {len(prompts) * 20 // 60} minutes")
    print()
    
    for i, prompt_data in enumerate(prompts, 1):
        print(f"[{i}/{len(prompts)}] Generating: {prompt_data['filename']}.png")
        print(f"   Prompt: {prompt_data['prompt'][:80]}...")
        
        # Queue the prompt
        result = queue_prompt(prompt_data)
        
        if result and 'prompt_id' in result:
            # Wait for completion
            print(f"   ⏳ Generating... ", end='', flush=True)
            wait_for_completion(result['prompt_id'])
            print("✅ Done!")
            
            # Copy to Unity folder
            # ComfyUI saves to: output/ep1_story_event1_00001_.png
            # We need to find and copy it
            print(f"   📋 Copying to Unity folder...")
        else:
            print(f"   ❌ Failed to queue")
        
        print()
    
    print("=" * 60)
    print("✅ ALL IMAGES GENERATED!")
    print(f"   Saved to: {UNITY_OUTPUT_DIR}")
    print(f"   Files: ep1_story_event1.png ... ep1_story_event{len(prompts)}.png")
    print()
    print("🎮 Ready to use in Unity!")
    print("=" * 60)


if __name__ == "__main__":
    main()