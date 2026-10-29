from pathlib import Path
from PIL import Image
import re,uuid
im=Image.open('Assets/SpritesJuego/NES - Super Mario Bros. - Enemies & Bosses - Enemies & Bosses.png').convert('RGBA')
for name,x,y,h in [('Koopa_Walk1',0,112,24),('Koopa_Walk2',18,112,24),('Koopa_Shell',72,120,16)]:
 tile=im.crop((x,y,x+16,y+h));tile.putdata([(r,g,b,0 if (r,g,b) in [(146,144,255),(0,41,140)] else a) for r,g,b,a in tile.getdata()]);p=Path('Assets/Resources/MarioFrames')/(name+'.png');tile.save(p)
 meta=p.with_suffix('.png.meta');template=Path('Assets/Resources/MarioFrames/Small_Idle.png.meta').read_text()
 if not meta.exists():
  template=re.sub(r'guid: [a-f0-9]+','guid: '+uuid.uuid4().hex,template,count=1);template=template.replace('  alignment: 0\n  spritePivot: {x: 0.5, y: 0.5}', '  alignment: 9\n  spritePivot: {x: 0.5, y: 0}');meta.write_text(template)
 Image.open(p).resize((96,h*6)).save('Tools/'+name+'-preview.png')
