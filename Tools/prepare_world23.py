"""Write the new 2-3 scene and resolve its existing Unity asset references."""
from pathlib import Path
import re,uuid
root=Path(__file__).resolve().parents[1]
def guid(path,importer):
    meta=root/(path+'.meta')
    if not meta.exists():meta.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\n'+importer)
    return re.search(r'^guid: (\w+)',meta.read_text(),re.M)[1]
script=guid('Assets/Scripts/Level/World23Builder.cs','MonoImporter:\n  serializedVersion: 2\n  defaultReferences: []\n  executionOrder: 0\n')
fields=[]
for field,path in {
    'goombaPrefab':'Assets/Prefabs/Enemies/Goomba.prefab',
    'koopaPrefab':'Assets/Prefabs/Enemies/Koopa.prefab',
    'coinPrefab':'Assets/Prefabs/Items/Coin.prefab',
    'mushroomPrefab':'Assets/Prefabs/Items/Mushroom.prefab',
    'groundSprite':'Assets/Sprites/Replica/Underground_0.png',
    'brickSprite':'Assets/Sprites/Replica/Underground_0.png',
    'questionSprite':'Assets/Sprites/Replica/Underground_31.png',
    'emptySprite':'Assets/Sprites/Block_Empty.png',
    'pipeSprite':'Assets/Sprites/Pipe.png',
    'liftSprite':'Assets/Sprites/LiftPlatform.png',
}.items():
    assert (root/path).exists(),path
    assetguid=re.search(r'^guid: (\w+)',(root/(path+'.meta')).read_text(),re.M)[1]
    fileid=re.search(r'--- !u!1 &(\d+)',(root/path).read_text())[1] if path.endswith('.prefab') else '21300000'
    fields.append(f'  {field}: {{fileID: {fileid}, guid: {assetguid}, type: {3 if path.endswith(".prefab") else 3}}}\n')
scene='''%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!1 &100000
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: 400000}
  - component: {fileID: 11400000}
  m_Layer: 0
  m_Name: World 2-3 Underground Course
  m_TagString: Untagged
  m_IsActive: 1
--- !u!4 &400000
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 100000}
  serializedVersion: 2
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_Children: []
  m_Father: {fileID: 0}
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 100000}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: SCRIPT, type: 3}
  m_Name: 
  m_EditorClassIdentifier: Assembly-CSharp::MarioBros.Level.World23Builder
'''.replace('SCRIPT',script)+''.join(fields)+'''--- !u!1660057539 &9223372036854775807
SceneRoots:
  m_ObjectHideFlags: 0
  m_Roots:
  - {fileID: 400000}
'''
(root/'Assets/Scenes/World2_3.unity').write_text(scene)
sceneguid=guid('Assets/Scenes/World2_3.unity','DefaultImporter:\n  externalObjects: {}\n')
settings=root/'ProjectSettings/EditorBuildSettings.asset'
text=settings.read_text()
if 'path: Assets/Scenes/World2_3.unity' not in text:
    text=text.replace('  m_configObjects:',f'  - enabled: 1\n    path: Assets/Scenes/World2_3.unity\n    guid: {sceneguid}\n  m_configObjects:')
    settings.write_text(text)
for name in ['Underground','Underwater']:
    p=root/f'Assets/Scenes/{name}.unity'
    s=p.read_text(encoding='utf-8-sig').replace('nextScene: World2_3Pending','nextScene: World2_3')
    s=s.replace('2-3\\nPENDIENTE','2-3')
    s=re.sub(r"'Warp 2-3\n\n\s+PENDIENTE'",'Warp 2-3',s)
    s=re.sub(r"'2-3\n\n\s+PENDIENTE'",'2-3',s)
    p.write_text(s,encoding='utf-8')
print('World2_3 scene created, registered and connected to Underground/Underwater.')
