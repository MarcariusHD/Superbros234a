"""Extract the original 1-1 bonus room, excluding its A/B editor annotations."""
from pathlib import Path
from PIL import Image
import uuid
root=Path(__file__).resolve().parents[1]
source=Image.open(root/'Assets/SpritesJuego/NES - Super Mario Bros. - Stages - World 1-1.png').convert('RGBA')
folder=root/'Assets/Resources/BonusRoom';folder.mkdir(exist_ok=True)
coins=0
for y in range(13):
    for x in range(16):
        solid=y<2 or (x==0 and y>=2) or (y==12 and 4<=x<=10) or (4<=x<=10 and 2<=y<=4) or x==15 or (x>=13 and 2<=y<=3)
        coin=(y in [5,7] and 4<=x<=10) or (y==9 and 5<=x<=9)
        if not(solid or coin):continue
        px,py=768+x*16,272+(12-y)*16
        image=source.crop((px,py,px+16,py+16))
        path=folder/f'Tile_{x}_{y}.png';image.save(path)
        coins+=int(coin)
        meta=Path(str(path)+'.meta')
        if not meta.exists():meta.write_text(f'''fileFormatVersion: 2
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
assert coins==19
for path,extra in [(root/'Assets/Scripts/Level/BonusRoomBuilder.cs','MonoImporter:\n  serializedVersion: 2\n  defaultReferences: []\n'),(folder,'folderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n')]:
    meta=Path(str(path)+'.meta')
    if not meta.exists():meta.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\n'+extra)
print('Original bonus room extracted: 19 coins, solid floor, brick platform and exit pipe.')
