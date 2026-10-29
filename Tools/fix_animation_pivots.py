from pathlib import Path
from PIL import Image
import re,uuid
for p in Path('Assets/Resources/MarioFrames').glob('Big_*.png.meta'):
 s=p.read_text();s=s.replace('  alignment: 0\n  spritePivot: {x: 0.5, y: 0.5}', '  alignment: 9\n  spritePivot: {x: 0.5, y: 0.25}');p.write_text(s)
im=Image.open('Assets/SpritesJuego/NES - Super Mario Bros. - Enemies & Bosses - Enemies & Bosses.png').convert('RGBA')
for i,x in enumerate([292,310]):
 tile=im.crop((x,164,x+16,180));tile.putdata([(r,g,b,0 if (r,g,b) in [(146,144,255),(0,41,140)] else a) for r,g,b,a in tile.getdata()])
 p=Path(f'Assets/Resources/MarioFrames/CheepCheep{i+1}.png');tile.save(p)
 template=Path('Assets/Resources/MarioFrames/Small_Idle.png.meta').read_text();meta=p.with_suffix('.png.meta')
 if not meta.exists():meta.write_text(re.sub(r'guid: [a-f0-9]+','guid: '+uuid.uuid4().hex,template,count=1))
 if i==0:tile.save('Assets/Sprites/Replica/CheepCheep.png')
print('World 1-1 sky:',Image.open('Assets/SpritesJuego/NES - Super Mario Bros. - Stages - World 1-1.png').getpixel((0,0)))
