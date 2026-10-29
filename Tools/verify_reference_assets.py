from pathlib import Path
from PIL import Image,ImageChops
import csv
cases=[('Overworld','NES - Super Mario Bros. - Stages - World 1-1.png',(0,0,3376,240)),('Underground','NES - Super Mario Bros. - Stages - World 1-2.png',(0,272,3072,480)),('Underwater','World2-2-reference.png',(256,0,3328,240))]
for name,file,box in cases:
 reference=Image.open(Path('Assets/SpritesJuego')/file).convert('RGBA').crop(box)
 grid=list(csv.reader(open(f'Assets/Sprites/Replica/{name}.csv')))
 bg=(0,0,0,255) if name=='Underground' else ((66,64,255,255) if name=='Underwater' else (92,148,252,255))
 rebuilt=Image.new('RGBA',reference.size,bg)
 for y,row in enumerate(grid):
  for x,idx in enumerate(row):rebuilt.alpha_composite(Image.open(f'Assets/Sprites/Replica/{name}_{idx}.png').convert('RGBA'),(x*16,y*16))
 assert ImageChops.difference(reference.convert('RGB'),rebuilt.convert('RGB')).getbbox() is None, name+' grid mismatch'
 print('PASS:',name,'reference matches grid pixel for pixel',len(grid[0]),'x',len(grid))
for name in ['Small_Idle','Small_Run1','Small_Run2','Small_Run3','Small_Jump','Small_Die','Big_Jump','Goomba_Underground_Walk1','Koopa_Walk1','CheepCheep1']:
 im=Image.open(f'Assets/Resources/MarioFrames/{name}.png')
 assert im.width==16 and im.getextrema()[3]==(0,255), name
 print('PASS:',name,'size and transparency')

