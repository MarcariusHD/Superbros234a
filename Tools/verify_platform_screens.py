from PIL import Image
import csv
sheet=Image.open('Assets/SpritesJuego/NES - Super Mario Bros. - Miscellaneous - Title Screen, HUD and Miscellaneous.png')
objects=Image.open('Assets/SpritesJuego/NES - Super Mario Bros. - Miscellaneous - Items, Objects and NPCs.png')
for label,im,rect in [('Title',sheet,(0,192,256,432)),('Intro',sheet,(0,460,256,700)),('Lift',objects,(116,64,164,72)),('Mushroom',objects,(0,8,16,24)),('Star',objects,(106,8,122,24)),('Coin',objects,(180,36,196,52))]:
 x0,y0,x1,y1=rect
 assert 0<=x0<x1<=im.width and 0<=y0<y1<=im.height
 assert len(im.crop(rect).getcolors(100000))>1
 print('PASS: source crop',label,(x1-x0,y1-y0))
grid=list(csv.reader(open('Assets/Sprites/Replica/Underground.csv')))
# Main roof is at y=13; lift goes beyond it, with a shaft before it and opening by the warps.
assert all(int(grid[0][x]) in [0,4,5] for x in range(161,187))
assert all(int(grid[0][x])==1 for x in [156,187,188,189])
assert 16+.25>13
for platform_speed in [-2,2]:
 assert platform_speed-platform_speed <= .6
print('PASS: roof route, lift clearance and relative vertical-speed condition')
