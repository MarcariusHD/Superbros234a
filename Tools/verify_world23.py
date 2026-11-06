"""Validate the scene's actual asset references and critical course coordinates."""
from pathlib import Path
import re
root=Path(__file__).resolve().parents[1]
scene=(root/'Assets/Scenes/World2_3.unity').read_text()
metas={}
for folder in ['Assets/Scripts/Level','Assets/Prefabs','Assets/Sprites']:
    for p in (root/folder).rglob('*.meta'):
        match=re.search(r'^guid: (\w+)',p.read_text(),re.M)
        if match:metas[match[1]]=Path(str(p)[:-5])
for guid in re.findall(r'guid: (\w+)',scene):
    assert guid in metas and metas[guid].exists(),guid
print('PASS: builder script, four prefabs and all scene sprites resolve.')
settings=(root/'ProjectSettings/EditorBuildSettings.asset').read_text()
assert 'path: Assets/Scenes/World2_3.unity' in settings
for name in ['Underground','Underwater']:
    text=(root/f'Assets/Scenes/{name}.unity').read_text(encoding='utf-8-sig')
    assert 'nextScene: World2_3\n' in text
    assert 'nextScene: World2_3Pending' not in text
print('PASS: both entrances route to the registered playable scene.')
layout=(root/'Assets/Scripts/Level/World23Builder.cs').read_text()
pits=[(int(a),int(b)) for a,b in re.findall(r'x>=(\d+)&&x<=(\d+)',layout)]
enemies=[int(x) for x in re.findall(r'Enemy\((?:goombaPrefab|koopaPrefab),(\d+)\)',layout)]
assert len(enemies)==5
assert all(not any(a<=x<=b for a,b in pits) for x in enemies)
assert all(not any(a<=x<=b for a,b in pits) for x in [3,4,132,135,138,153,168])
assert len(re.findall(r'LiftAt\([0-9]',layout))==1
assert 'AddComponent<HorizontalPlatform>()' in layout and 'AddComponent<LoopingLift>()' not in layout
assert 'left=95.5f' in layout and 'right=107.5f' in layout
assert 'Lava(94,108)' in layout
upgrades=re.search(r'new\[\]\{([^}]+)\}\)Question\(x,5,true\)',layout)[1].split(',')
assert len(upgrades)==4
assert 'Question(45,9,false)' in layout
assert 'Question(x,5,false)' in layout
assert '14,22,40,47,79,85,89,123,128' in layout
assert 'new[]{34,110}' in layout and 'new[]{17,42,81,119}' in layout
assert 'FirebarAt' not in layout
positions=sorted(enemies+[30,74])
assert max(sum(x<=p<=x+27 for p in positions) for x in positions)<=3
assert 'Visual(Resources.Load<Sprite>("Castle23/LavaFill")' in layout
assert 'fill.AddComponent<CastleLava>' not in layout
ceiling=int(re.search(r'CeilingRow=(\d+)',layout)[1])
mouth=float(re.search(r'PipeMouthY=([\d.]+)',layout)[1])
assert ceiling==11 and mouth==ceiling-2
assert 'new Vector2(138,CeilingRow-1)' in layout
assert 'flipY=true' in layout and '-2/pipeBounds.y' not in layout
assert 'World23PipeContact' in layout
print('PASS: single lava surface, lowered coin blocks, lower ceiling, connected pipe and added terrain obstacles.')
finish=(root/'Assets/Scripts/Level/World23Finish.cs').read_text()
assert 'PlayerIdentity.InputFor(health).Up' in finish
assert 'EnterUp(' in finish and 'Emerge(' in finish
assert 'undersideContact||rising||PlayerIdentity.InputFor(health).Up' in finish
assert 'OnCollisionEnter2D' in finish and 'entrance.TryEnter(collision.collider,true)' in finish
assert 'initialCenter=152-1+camera.orthographicSize*camera.aspect' in finish
assert 'SetRegion(initialCenter,Mathf.Max(initialCenter,168),initialCenter)' in finish and 'i<6' in finish
for aspect in [4/3,16/9,21/9]:
    half=7.5*aspect;center=152-1+half
    assert abs((center-half)-(152-1))<1e-6
print('PASS: exit camera left edge stays one tile behind the pipe for different aspect ratios.')
assert 'NIVEL 2-3 COMPLETADO' in finish
assert 'outdoorRoot.SetActive(false)' in layout and 'RevealOutdoor()' in finish
for aspect in [4/3,16/9,21/9]:
    right_edge=(148-7.5*aspect)+7.5*aspect
    assert abs(right_edge-148)<1e-6
animation=(root/'Assets/Scripts/Player/PlayerAnimation.cs').read_text(encoding='utf-8-sig')
assert 'playerMovement.IsUnderwater && !playerMovement.IsGrounded' in animation
for name in ['Marco Calderon','Abril Cayllagua','Edson Chuquimia','Limber Soto']:
    assert f'NesText.Create(frame,"{name}"' in finish
assert 'Resources.Load<Sprite>("Castle23/CreditsStar")' in finish
assert (root/'Assets/Resources/Castle23/CreditsStar.png').exists()
print('PASS: indoor/outdoor separation, grounded underwater pose and four credits with Marco star.')
print('PASS: upside-down pipe, outdoor castle walk, six fireworks and completion screen connected.')
from PIL import Image
for name,level,rect in [('Floor','1-4',(16,208,32,224)),('Ceiling','2-4',(16,32,32,48)),('Column','4-4',(16,32,32,48))]:
    reference=Image.open(root/f'Assets/SpritesJuego/NES - Super Mario Bros. - Stages - World {level}.png').convert('RGBA').crop(rect)
    actual=Image.open(root/f'Assets/Resources/Castle23/{name}.png').convert('RGBA')
    assert reference.tobytes()==actual.tobytes(),name
for folder in ['Lava','Firework','Plant','Goomba','Brick']:
    for path in (root/f'Assets/Resources/Castle23/{folder}').glob('*.png'):
        assert Image.open(path).convert('RGBA').getbbox(),path
print('PASS: exact art crops from 1-4, 2-4 and 4-4; nonempty castle effects and enemy frames.')
fill=Image.open(root/'Assets/Resources/Castle23/LavaFill.png').convert('RGBA')
assert len(set(fill.getdata()))==1
r,g,b,a=fill.getpixel((0,0));assert r>180 and g<90 and b<80 and a==255
# Spanish labels have separate 64px columns, with padding even for MONEDAS.
heads=[('MARIO',32),('MONEDAS',96),('MUNDO',160),('TIEMPO',224),('VIDAS',288)]
for (word,x),(other,y) in zip(heads,heads[1:]):assert x+len(word)*4+8<=y-len(other)*4
bubble=(root/'Assets/Scripts/Level/CastleHazard.cs').read_text()
assert 'sprite.sortingOrder=-2;sprite.enabled=false' in bubble
assert 'sprite.enabled=transform.position.y+sprite.bounds.extents.y>2' in bubble
print('PASS: lava bubble is occluded while submerged; Spanish HUD columns remain separate.')
controls=(root/'Assets/Scripts/Player/PlayerIdentity.cs').read_text()
fire=next(line for line in controls.splitlines() if 'public bool FireDown' in line)
assert 'K.rightAltKey.wasPressedThisFrame' in fire and 'rightShiftKey' not in fire
print('PASS: Luigi fires with Right Alt; Right Shift remains running only.')
