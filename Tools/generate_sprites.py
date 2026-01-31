#!/usr/bin/env python3
"""
ChronoCiv Pixel Art Sprite Generator
Generates retro pixel art sprites for characters, buildings, and UI elements.

Usage: python3 generate_sprites.py [--output OUTPUT_DIR]
"""

import os
import json
from pathlib import Path

# Color palettes for different eras
PALETTES = {
    "stone_age": {
        "skin": ["#D4A574", "#C49A6C", "#B8956A"],
        "clothing": ["#8B4513", "#654321", "#4A3728"],
        "hair": ["#2C1810", "#3D2314", "#1A0F0A"],
        "terrain": ["#7CFC00", "#228B22", "#808080", "#8B7355"],
    },
    "ancient": {
        "skin": ["#F5D0A9", "#E5BE98", "#D4A574"],
        "clothing": ["#E6C288", "#D4B896", "#C4A876"],
        "hair": ["#1A1A1A", "#3D3D3D", "#4A4A4A"],
        "terrain": ["#DEB887", "#D2B48C", "#BC8F8F", "#8B7355"],
    },
    "classical": {
        "skin": ["#F5DEB3", "#FFE4C4", "#DEB887"],
        "clothing": ["#FFFFFF", "#F5F5DC", "#E8E8D8"],
        "hair": ["#FFD700", "#DAA520", "#B8860B"],
        "terrain": ["#F5F5F5", "#D3D3D3", "#C0C0C0", "#A9A9A9"],
    },
    "medieval": {
        "skin": ["#F5DEB3", "#DEB887", "#D2B48C"],
        "clothing": ["#8B0000", "#4A0080", "#000080", "#2F4F4F"],
        "hair": ["#2F2F2F", "#4A4A4A", "#696969"],
        "terrain": ["#8B4513", "#A0522D", "#6B8E23", "#556B2F"],
    },
    "renaissance": {
        "skin": ["#FFE4C4", "#FFDAB9", "#F5DEB3"],
        "clothing": ["#800020", "#4B0082", "#00008B", "#006400"],
        "hair": ["#2F2F2F", "#4A3728", "#8B4513"],
        "terrain": ["#DAA520", "#B8860B", "#CD853F", "#D2691E"],
    },
    "industrial": {
        "skin": ["#FFE4C4", "#F5DEB3", "#DEB887"],
        "clothing": ["#2F2F2F", "#4A4A4A", "#696969", "#808080"],
        "hair": ["#1A1A1A", "#2F2F2F", "#4A4A4A"],
        "terrain": ["#696969", "#808080", "#A9A9A9", "#C0C0C0"],
    },
    "modern": {
        "skin": ["#FFE4C4", "#F5DEB3", "#DEB887", "#C4A574"],
        "clothing": ["#000080", "#8B0000", "#2F4F4F", "#4A0080"],
        "hair": ["#1A1A1A", "#4A4A4A", "#8B4513", "#D2B48C"],
        "terrain": ["#808080", "#A9A9A9", "#C0C0C0", "#D3D3D3"],
    },
    "future": {
        "skin": ["#E8E8E8", "#D0D0D0", "#B8B8B8"],
        "clothing": ["#00FFFF", "#FF00FF", "#FFFF00", "#00FF00"],
        "hair": ["#00CED1", "#9400D3", "#00FF00", "#FF1493"],
        "terrain": ["#1a1a2e", "#16213e", "#0f3460", "#533483"],
    }
}

# Character animations
ANIMATIONS = ["idle", "walk", "run", "work", "attack", "sleep"]

# Directions for directional sprites
DIRECTIONS = ["up", "down", "left", "right"]

def create_pixel_image(width, height, pixels, palette=None):
    """Create a simple pixel representation using base64 PNG encoding."""
    from PIL import Image
    import base64
    
    if palette:
        color_map = {}
        for era_colors in palette.values():
            for color in era_colors:
                color_map[color] = color
        color_map["transparent"] = (0, 0, 0, 0)
        color_map["#000000"] = (0, 0, 0, 255)
        color_map["#FFFFFF"] = (255, 255, 255, 255)
        color_map["#2F2F2F"] = (47, 47, 47, 255)
        color_map["stone"] = (128, 128, 128, 255)
        color_map["terrain"] = (139, 69, 19, 255)
    else:
        color_map = {
            "transparent": (0, 0, 0, 0),
            "#8B4513": (139, 69, 19, 255),
            "#228B22": (34, 139, 34, 255),
            "#D2B48C": (210, 180, 140, 255),
            "#2F2F2F": (47, 47, 47, 255),
            "stone": (128, 128, 128, 255),
            "terrain": (139, 69, 19, 255),
        }
    
    img = Image.new('RGBA', (width, height), (0, 0, 0, 0))
    pixels_img = img.load()
    
    for y in range(height):
        for x in range(width):
            if y < len(pixels) and x < len(pixels[y]):
                color_name = pixels[y][x]
                if color_name in color_map:
                    pixels_img[x, y] = color_map[color_name]
    
    return img

def generate_npc_base_sprite(size=(16, 24)):
    """Generate base NPC sprite template."""
    width, height = size
    pixels = []
    
    for y in range(height):
        row = []
        for x in range(width):
            if y < 4:  # Head
                row.append("skin")
            elif y < 8:  # Neck
                row.append("skin")
            elif y < 16:  # Body
                row.append("clothing")
            else:  # Legs
                row.append("clothing")
        pixels.append(row)
    
    return pixels

def generate_building_sprite(building_type, size=(32, 32)):
    """Generate building sprite template."""
    width, height = size
    pixels = []
    
    for y in range(height):
        row = []
        for x in range(width):
            if y < 4:  # Roof - use terrain color as base
                row.append("terrain")
            elif y < height - 4:  # Walls - use stone/brown
                row.append("stone")
            else:  # Foundation - use dark stone
                row.append("#2F2F2F")
        pixels.append(row)
    
    return pixels

def generate_terrain_tile(size=(16, 16)):
    """Generate terrain tile sprite."""
    width, height = size
    pixels = []
    
    for y in range(height):
        row = []
        for x in range(width):
            row.append("terrain_base")
        pixels.append(row)
    
    return pixels

def generate_ui_icon(icon_type, size=(16, 16)):
    """Generate UI icon sprite."""
    width, height = size
    pixels = []
    
    for y in range(height):
        row = []
        for x in range(width):
            if y < 2 or y >= height - 2 or x < 2 or x >= width - 2:
                row.append("border")
            else:
                row.append("background")
        pixels.append(row)
    
    return pixels

def save_sprite_pil(img, filepath):
    """Save PIL image to file."""
    os.makedirs(os.path.dirname(filepath), exist_ok=True)
    img.save(filepath, 'PNG')
    print(f"Saved: {filepath}")

def generate_all_sprites(output_dir):
    """Generate all game sprites."""
    output_path = Path(output_dir)
    sprites_path = output_path / "Sprites"
    npc_path = sprites_path / "NPCs"
    building_path = sprites_path / "Buildings"
    terrain_path = sprites_path / "Terrain"
    ui_path = sprites_path / "UI"
    
    # Create directories
    for path in [npc_path, building_path, terrain_path, ui_path]:
        path.mkdir(parents=True, exist_ok=True)
    
    # Generate NPC sprites for each era
    print("Generating NPC sprites...")
    for era in PALETTES.keys():
        era_npc_path = npc_path / era
        era_npc_path.mkdir(exist_ok=True)
        
        for animation in ANIMATIONS:
            anim_path = era_npc_path / animation
            anim_path.mkdir(exist_ok=True)
            
            for direction in DIRECTIONS:
                base_pixels = generate_npc_base_sprite()
                filename = f"{direction}.png"
                filepath = anim_path / filename
                
                img = create_pixel_image(16, 24, base_pixels, PALETTES[era])
                save_sprite_pil(img, str(filepath))
    
    # Generate building sprites
    print("Generating building sprites...")
    buildings = ["cave", "tent", "campfire", "hut", "house", "castle", "palace", 
                 "factory", "skyscraper", "temple", "market"]
    
    for building in buildings:
        building_file = building_path / f"{building}.png"
        base_pixels = generate_building_sprite(building)
        img = create_pixel_image(32, 32, base_pixels)
        save_sprite_pil(img, str(building_file))
    
    # Generate terrain sprites
    print("Generating terrain sprites...")
    terrains = ["grassland", "forest", "desert", "ocean", "mountain", "snow", "swamp"]
    
    for terrain in terrains:
        terrain_file = terrain_path / f"{terrain}.png"
        base_pixels = generate_terrain_tile()
        img = create_pixel_image(16, 16, base_pixels)
        save_sprite_pil(img, str(terrain_file))
    
    # Generate UI icons
    print("Generating UI icons...")
    icons = ["food", "wood", "stone", "gold", "iron", "population", "science", 
             "culture", "faith", "happy", "military"]
    
    for icon in icons:
        icon_file = ui_path / f"{icon}.png"
        base_pixels = generate_ui_icon(icon)
        img = create_pixel_image(16, 16, base_pixels)
        save_sprite_pil(img, str(icon_file))
    
    print(f"\nSprite generation complete!")
    print(f"Sprites saved to: {output_path}")

def create_sprite_atlas_config(output_dir):
    """Create sprite atlas configuration JSON."""
    config = {
        "atlas_name": "ChronoCiv_Sprites",
        "sprites": [],
        "packing": {
            "power_of_two": True,
            "padding": 2,
            "extrude": 1
        }
    }
    
    output_path = Path(output_dir)
    config_file = output_path / "sprite_atlas_config.json"
    
    with open(config_file, 'w') as f:
        json.dump(config, f, indent=2)
    
    print(f"Sprite atlas config saved: {config_file}")

def main():
    """Main entry point."""
    import argparse
    
    parser = argparse.ArgumentParser(description="Generate pixel art sprites for ChronoCiv")
    parser.add_argument("--output", "-o", default="./Data/Art/Sprites",
                       help="Output directory for sprites")
    args = parser.parse_args()
    
    print("=" * 50)
    print("ChronoCiv Pixel Art Sprite Generator")
    print("=" * 50)
    
    generate_all_sprites(args.output)
    create_sprite_atlas_config(args.output)
    
    print("\nTo use these sprites in Unity:")
    print("1. Import the generated PNG files")
    print("2. Set texture type to 'Sprite (2D and UI)'")
    print("3. Set pixels per unit to 16 for characters, 32 for buildings")
    print("4. Create sprite atlases for better performance")

if __name__ == "__main__":
    main()

