"""Extract the supplied NES UI without opening Unity; mirrors OriginalSpriteSetup."""
from pathlib import Path
from PIL import Image
import uuid

root = Path(__file__).resolve().parents[1]
sheet = Image.open(root / 'Assets/SpritesJuego/NES - Super Mario Bros. - Miscellaneous - Title Screen, HUD and Miscellaneous.png').convert('RGBA')
output = root / 'Assets/Resources/ClassicUI'
output.mkdir(parents=True, exist_ok=True)

def save(name, image):
    image.save(output / (name + '.png'))
    meta = output / (name + '.png.meta')
    if not meta.exists():
        meta.write_text(f'''fileFormatVersion: 2
guid: {uuid.uuid4().hex}
TextureImporter:
  serializedVersion: 13
  mipmaps:
    enableMipMap: 0
  textureSettings:
    filterMode: 0
    wrapU: 1
    wrapV: 1
  nPOTScale: 0
  spriteMode: 1
  spritePixelsToUnits: 100
  alphaIsTransparency: 1
  textureType: 8
  maxTextureSize: 2048
  platformSettings:
  - serializedVersion: 4
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 2048
    textureFormat: -1
    textureCompression: 0
    overridden: 0
''')

font = Image.new('RGBA', (41 * 8, 8))
for i in range(41):
    x, y = (i * 9, 104) if i < 24 else ((i - 24) * 9, 113)
    glyph = sheet.crop((x, y, x + 8, y + 8))
    glyph.putdata([p if min(p[:3]) >= 230 else (0, 0, 0, 0) for p in glyph.getdata()])
    assert glyph.getbbox(), f'Empty glyph {i}'
    font.paste(glyph, (i * 8, 0))
save('Font', font)

for name, x, y in [('Title', 0, 192), ('LevelIntro', 0, 460), ('TimeUp', 264, 460), ('GameOver', 528, 460)]:
    image = sheet.crop((x, y, x + 256, y + 240))
    if name == 'Title':
        sky = image.getpixel((0, 0))
        image.putdata([(92, 148, 252, 255) if p == sky else p for p in image.getdata()])
    background = (92, 148, 252, 255) if name == 'Title' else (0, 0, 0, 255)
    image.paste(background, (0, 0, 256, 32))
    if name == 'Title':
        for rect in [(72, 144, 80, 152), (72, 160, 80, 168), (128, 184, 184, 192)]:
            image.paste(background, rect)
        image.paste(background, (88, 144, 200, 168))
        image.paste(background, (72, 184, 128, 192))
    elif name == 'LevelIntro':
        image.paste(background, (80, 76, 176, 96))
        image.paste(background, (120, 108, 160, 124))
    else:
        image.paste(background, (96, 108, 168, 120))
        image.paste(background, (48, 120, 208, 144))
    save(name, image)

for name, y in [('Coin', 156), ('Cursor', 440)]:
    image = sheet.crop((0, y, 8, y + 8))
    image.putdata([(0, 0, 0, 0) if p[:3] in [(146, 144, 255), (148, 148, 255), (0, 41, 140), (0, 40, 136)] else p for p in image.getdata()])
    save(name, image)
print('NES UI ready: 41 nonempty glyphs, four screens, coin and cursor.')
