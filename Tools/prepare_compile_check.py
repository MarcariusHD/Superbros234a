from pathlib import Path
import xml.etree.ElementTree as ET
refs={}
for name in ['Assembly-CSharp.csproj','Assembly-CSharp-Editor.csproj']:
 root=ET.parse(name).getroot()
 for h in root.findall('.//Reference/HintPath'):
  p=Path(h.text)
  if p.exists() and p.name not in ['Assembly-CSharp.dll','Assembly-CSharp-Editor.dll']: refs.setdefault(p.name, str(p.resolve()))
lines=['/nologo','/target:library','/nostdlib+','/langversion:9.0','/unsafe','/define:UNITY_EDITOR;UNITY_EDITOR_WIN;ENABLE_INPUT_SYSTEM;ENABLE_LEGACY_INPUT_MANAGER','/out:Logs/ReplicaValidation.dll']
lines += [' /reference:"'+p+'"' for p in sorted(refs.values())]
lines += ['"'+str(p.resolve())+'"' for p in Path('Assets/Scripts').rglob('*.cs')]
Path('Logs/ReplicaValidation.rsp').write_text('\n'.join(lines))
print(len(refs),'assembly references')

