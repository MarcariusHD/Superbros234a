"""Extract original coin and broken-brick frames at their native pixel size."""
from pathlib import Path
from PIL import Image
import uuid

root = Path(__file__).resolve().parents[1]
sheet = Image.open(root / 'Assets/SpritesJuego/NES - Super Mario Bros. - Miscellaneous - Items, Objects and NPCs.png').convert('RGBA')
output = root / 'Assets/Resources/ClassicEffects'
for group, count in [('Coin', 4), ('Brick', 8)]:
    folder = output / group
    folder.mkdir(parents=True, exist_ok=True)
    for i in range(count):
        x = 180 + (i % 4) * 10
        y = 36 if group == 'Coin' else 26 + (108 if i >= 4 else 0)
        image = sheet.crop((x, y, x + 8, y + (16 if group == 'Coin' else 8)))
        image.putdata([(0, 0, 0, 0) if p[:3] in [(146, 144, 255), (0, 41, 140)] else p for p in image.getdata()])
        if group == 'Coin':
            canvas = Image.new('RGBA', (16, 16));canvas.paste(image, (4, 0));image = canvas
        assert image.getbbox(), (group, i)
        path = folder / f'{i}.png';image.save(path)
        meta = path.with_suffix('.png.meta')
        if not meta.exists():
            meta.write_text(f'''fileFormatVersion: 2
guid: {uuid.uuid4().hex}
TextureImporter:
  serializedVersion: 13
  mipmaps:
    enableMipMap: 0
  textureSettings:
    filterMode: 0
  nPOTScale: 0
  spriteMode: 1
  spritePixelsToUnits: 16
  alphaIsTransparency: 1
  textureType: 8
  platformSettings:
  - serializedVersion: 4
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 2048
    textureFormat: -1
    textureCompression: 0
    overridden: 0
''')
print('Extracted 4 coin frames and 8 brick fragment frames.')
