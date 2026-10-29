from pathlib import Path
from PIL import Image
import json
for name in ['Underground','Underwater']:
 g=json.loads(Path(f'Tools/{name}-grid.json').read_text())
 Path(f'Assets/Sprites/Replica/{name}.csv').write_text('\n'.join(','.join(map(str,r)) for r in g))
# Remove the map's flat backdrop from collectables and enemies.
for name,ids,bg in [('Underground',[6],(0,0,0)),('Underwater',[13,14,21],(64,64,255))]:
 for i in ids:
  p=Path(f'Assets/Sprites/Replica/{name}_{i}.png'); im=Image.open(p).convert('RGBA'); bg=im.getpixel((0,0))[:3]
  im.putdata([(r,g,b,0 if (r,g,b)==bg else a) for r,g,b,a in im.getdata()]);im.save(p)
# Blooper is two tiles tall in the reference map.
im=Image.open('Assets/SpritesJuego/World2-2-reference.png').convert('RGBA')
for name,box in [('Blooper',(256+22*16,11*16,256+23*16,13*16))]:
 tile=im.crop(box);bg=tile.getpixel((0,0))[:3];tile.putdata([(r,g,b,0 if (r,g,b)==bg else a) for r,g,b,a in tile.getdata()]);tile.save(f'Assets/Sprites/Replica/{name}.png')
# Cheep-Cheep from the supplied enemy sheet (underwater palette).
im=Image.open('Assets/SpritesJuego/NES - Super Mario Bros. - Enemies & Bosses - Enemies & Bosses.png').convert('RGBA')
tile=im.crop((220,240,236,256));bg=tile.getpixel((0,0))[:3];tile.putdata([(r,g,b,0 if (r,g,b)==bg else a) for r,g,b,a in tile.getdata()]);tile.save('Assets/Sprites/Replica/CheepCheep.png')
