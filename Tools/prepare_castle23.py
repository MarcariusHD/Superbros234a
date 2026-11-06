"""Extract castle art from the supplied 1-4, 2-4 and 4-4 level sheets."""
from pathlib import Path
from PIL import Image
import uuid
root=Path(__file__).resolve().parents[1]
out=root/'Assets/Resources/Castle23';out.mkdir(exist_ok=True)
def save(image,name,clear=False,pivot=.5):
    image=image.convert('RGBA')
    if clear:
        backgrounds={(146,144,255),(148,148,255),(0,41,140),(0,40,136),(92,148,252)}
        image.putdata([(0,0,0,0) if p[:3] in backgrounds else p for p in image.getdata()])
    assert name=="LavaFill" or len(set(image.getdata()))>1,name
    path=out/(name+'.png');path.parent.mkdir(parents=True,exist_ok=True);image.save(path)
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
  alignment: 9
  spritePivot: {{x: 0.5, y: {pivot}}}
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
maps={n:Image.open(root/f'Assets/SpritesJuego/NES - Super Mario Bros. - Stages - World {n}.png').convert('RGBA') for n in ['1-4','2-4','4-4']}
for name,level,x,y in [('Floor','1-4',16,208),('Ceiling','2-4',16,32),('Column','4-4',16,32)]:
    save(maps[level].crop((x,y,x+16,y+16)),name)
source=maps['1-4']
# Locate the actual lava surface by its red fill and pale crest, rather than editor annotations.
surfaces=[]
for y in range(32,240,16):
    for x in range(0,source.width,16):
        tile=source.crop((x,y,x+16,y+16));pixels=list(tile.getdata())
        red=sum(r>180 and g<90 and b<80 for r,g,b,a in pixels)
        pale=sum(min(r,g,b)>200 for r,g,b,a in pixels)
        if red>150 and 3<pale<70:surfaces.append(tile)
assert surfaces,'No lava surface in reference'
for i in range(2):save(surfaces[i],f'Lava/{i}')
save(Image.new('RGBA',(16,16),source.getpixel((216,232))),'LavaFill')
enemy=Image.open(root/'Assets/SpritesJuego/NES - Super Mario Bros. - Enemies & Bosses - Enemies & Bosses.png')
save(enemy.crop((36,344,52,368)),'Bubble',True)
for i,x in enumerate([146,164]):save(enemy.crop((x,138,x+16,162)),f'Plant/{i}',True)
for i,x in enumerate([148,166]):save(enemy.crop((x,16,x+16,32)),f'Goomba/{i}',True)
save(enemy.crop((184,24,200,32)),'GoombaSquish',True)
for name,x,y,h in [('Koopa1',146,112,24),('Koopa2',164,112,24),('KoopaShell',218,120,16)]:
    save(enemy.crop((x,y,x+16,y+h)),name,True,0)
objects=Image.open(root/'Assets/SpritesJuego/NES - Super Mario Bros. - Miscellaneous - Items, Objects and NPCs.png')
for i,x in enumerate([180,198,216]):save(objects.crop((x,64,x+16,80)),f'Firework/{i}',True)
for i,x in enumerate([180,190,200,210]):save(objects.crop((x,242,x+8,250)),f'Brick/{i}',True)
save(Image.open(root/'Assets/Sprites/Castle.png'),'Castle',False,0)
save(Image.open(root/'Assets/Sprites/Block_Ground.png'),'OutdoorGround')
save(Image.open(root/'Assets/Sprites/Item_Starman.png'),'CreditsStar')
print('Castle 2-3 art ready: tiles from all three maps, lava, enemies, castle and fireworks.')
