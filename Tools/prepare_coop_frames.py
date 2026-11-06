"""Extract Luigi, fire suits, flowers and projectiles from the supplied NES sheets."""
from pathlib import Path
from PIL import Image
import uuid

root=Path(__file__).resolve().parents[1]
characters=Image.open(root/'Assets/SpritesJuego/NES - Super Mario Bros. - Playable Characters - Mario & Luigi.png').convert('RGBA')
objects=Image.open(root/'Assets/SpritesJuego/NES - Super Mario Bros. - Miscellaneous - Items, Objects and NPCs.png').convert('RGBA')
def save(image, name, big=False):
    background=image.getpixel((0,0))
    colors={(146,144,255),(148,148,255),(0,41,140),(0,40,136)}
    image.putdata([(0,0,0,0) if p[:3] in colors else p for p in image.getdata()])
    assert image.getbbox(),name
    path=root/'Assets/Resources/CoopFrames'/name
    path.parent.mkdir(parents=True,exist_ok=True);image.save(path)
    meta=Path(str(path)+'.meta')
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
  alignment: 9
  spritePivot: {{x: 0.5, y: {0.25 if big else 0.5}}}
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
for variant,dx,dy in [('Luigi',288,0),('FireMario',0,108),('FireLuigi',0,108)]:
    for size,y,h in [('Small',8,16),('Big',32,32)]:
        for name,x in [('Idle',0),('Run1',20),('Run2',38),('Run3',56),('Jump',96),('Climb',136),('Swim1',174),('Swim2',192)]:
            save(characters.crop((dx+x,dy+y,dx+x+16,dy+y+h)),f'{variant}/{size}_{name}.png',size=='Big')
    save(characters.crop((dx+116,dy+32,dx+132,dy+64)),f'{variant}/Big_Crouch.png',True)
    save(characters.crop((dx+116,dy+8,dx+132,dy+24)),f'{variant}/Small_Die.png')
for i,x in enumerate([32,50,68,86]):
    save(objects.crop((x,8,x+16,24)),f'Flower/{i}.png')
for i,x in enumerate([180,190,200,210]):
    save(objects.crop((x,54,x+8,62)),f'Fireball/{i}.png')
print('Prepared 54 character frames, four flowers and four fireballs.')
