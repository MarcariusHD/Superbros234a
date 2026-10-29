from PIL import Image
from pathlib import Path
import re,uuid
out=Path('Assets/Resources/MarioFrames');out.mkdir(parents=True,exist_ok=True)
source=Image.open('Assets/SpritesJuego/NES - Super Mario Bros. - Playable Characters - Mario & Luigi.png').convert('RGBA')
def save(tile,path):
 tile.putdata([(r,g,b,0 if (r,g,b) in [(146,144,255),(0,41,140)] else a) for r,g,b,a in tile.getdata()]);tile.save(path)
 template=Path('Assets/Sprites/Mario_Small_Idle.png.meta').read_text()
 meta=path.with_suffix('.png.meta')
 if not meta.exists():
  template=re.sub(r'guid: [a-f0-9]+','guid: '+uuid.uuid4().hex,template,count=1)
  meta.write_text(template)
for size,y,h in [('Small',8,16),('Big',32,32)]:
 for name,x in [('Idle',0),('Run1',20),('Run2',38),('Run3',56),('Skid',76),('Jump',96),('Swim1',174),('Swim2',192)]:
  save(source.crop((x,y,x+16,y+h)),out/f'{size}_{name}.png')
save(source.crop((116,8,132,24)),out/'Small_Die.png')
enemy=Image.open('Assets/SpritesJuego/NES - Super Mario Bros. - Enemies & Bosses - Enemies & Bosses.png').convert('RGBA')
for theme,x in [('Overworld',0),('Underground',74)]:
 for frame,dx in [('Walk1',0),('Walk2',18)]:save(enemy.crop((x+dx,16,x+dx+16,32)),out/f'Goomba_{theme}_{frame}.png')
 tile=Image.new('RGBA',(16,16));tile.paste(enemy.crop((x+36,24,x+52,32)),(0,8));save(tile,out/f'Goomba_{theme}_Squish.png')
# Contact sheet for checking exact frame boundaries.
names=['Small_Idle','Small_Run1','Small_Run2','Small_Run3','Small_Jump','Small_Die','Big_Idle','Big_Jump','Goomba_Underground_Walk1']
sheet=Image.new('RGBA',(len(names)*80,160),(70,70,70,255))
for i,name in enumerate(names):
 tile=Image.open(out/f'{name}.png');sheet.alpha_composite(tile.resize((64,tile.height*4)),(i*80,0))
sheet.convert('RGB').save('Tools/Animation-preview.png')
enemy.crop((218,100,298,273)).resize((320,692)).save('Tools/Water-enemy-detail.png')
