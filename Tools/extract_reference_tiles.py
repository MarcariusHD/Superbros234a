from PIL import Image, ImageDraw
from collections import Counter
import json
from pathlib import Path
out=Path('Assets/Sprites/Replica'); out.mkdir(exist_ok=True)
for name,path,box in [('Underground','Assets/SpritesJuego/NES - Super Mario Bros. - Stages - World 1-2.png',(0,272,3072,480)),('Underwater','Assets/SpritesJuego/World2-2-reference.png',(256,0,3328,240))]:
 im=Image.open(path).convert('RGBA').crop(box)
 tiles={}; grid=[]; counts=Counter()
 for y in range(im.height//16):
  row=[]
  for x in range(im.width//16):
   tile=im.crop((x*16,y*16,x*16+16,y*16+16)); key=tile.tobytes()
   if key not in tiles: tiles[key]=(len(tiles),tile)
   i=tiles[key][0];row.append(i);counts[i]+=1
  grid.append(row)
 sheet=Image.new('RGB',(800,((len(tiles)+9)//10)*65),'#555555');d=ImageDraw.Draw(sheet)
 for i,tile in tiles.values():
  x=(i%10)*80;y=(i//10)*65
  sheet.paste(tile.resize((40,40)),(x,y));d.text((x,y+41),f'{i} ({counts[i]})',fill='white')
 sheet.save(f'{name}-tiles.png')
 Path(f'Tools/{name}-grid.json').write_text(json.dumps(grid))
 for i,tile in tiles.values(): tile.save(out/f'{name}_{i}.png')
 print(name,len(tiles), im.size)
