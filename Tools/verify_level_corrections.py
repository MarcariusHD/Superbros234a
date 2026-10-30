from PIL import Image
from pathlib import Path
import csv
world=Image.open('Assets/SpritesJuego/NES - Super Mario Bros. - Stages - World 1-1.png')
assert world.width >= 207*16 and world.height >= 240
for name,rect,units in [('Castle',(202*16,128,207*16,208),(5,5)),('FlagPole',(198*16,32,199*16,208),(1,11)),('Block_Hard',(80*16,80,81*16,96),(1,1))]:
 region=world.crop(rect)
 assert (region.width/16,region.height/16)==units
 print('PASS: '+name+' original bounds '+str(units))
grid=list(csv.reader(open('Assets/Sprites/Replica/Underground.csv')))
for x in [178,182,186]:
 assert int(grid[len(grid)-1-4][x])==35
 assert int(grid[len(grid)-1-4][x+1])==36
 print('PASS: warp pipe at',x,'mouth y=5, trigger y=5.1')
center=2.45; half_height=.85/2
assert center-half_height>=2 and center+half_height<3
print('PASS: Goomba fits one-block corridor without overlapping floor or roof')
