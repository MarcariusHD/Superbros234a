from PIL import Image,ImageDraw
from pathlib import Path
import json
im=Image.open('Assets/SpritesJuego/NES - Super Mario Bros. - Stages - World 1-1.png').convert('RGBA').crop((0,0,3376,240))
out=Path('Assets/Sprites/Replica'); tiles={};grid=[]
for y in range(15):
 row=[]
 for x in range(211):
  tile=im.crop((x*16,y*16,x*16+16,y*16+16));k=tile.tobytes()
  if k not in tiles:tiles[k]=(len(tiles),tile)
  row.append(tiles[k][0])
 grid.append(row)
sheet=Image.new('RGB',(800,((len(tiles)+9)//10)*65),'#444444');d=ImageDraw.Draw(sheet)
for i,tile in tiles.values():
 x=i%10*80;y=i//10*65;sheet.paste(tile.resize((40,40)),(x,y));d.text((x,y+42),str(i),fill='white');tile.save(out/f'Overworld_{i}.png')
sheet.save('Tools/Overworld-tiles.png');(out/'Overworld.csv').write_text('\n'.join(','.join(map(str,r)) for r in grid))
print('Overworld tiles',len(tiles))
im=Image.open('Assets/SpritesJuego/NES - Super Mario Bros. - Playable Characters - Mario & Luigi.png');im.crop((0,0,282,66)).resize((1128,264)).save('Tools/Mario-frames.png')
im=Image.open('Assets/SpritesJuego/NES - Super Mario Bros. - Enemies & Bosses - Enemies & Bosses.png');im.crop((0,0,298,100)).resize((1192,400)).save('Tools/Enemy-frames.png')
