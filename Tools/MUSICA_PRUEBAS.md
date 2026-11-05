# Pruebas de música (Abril) — feat/abril-musica

## Preparación (1 vez)
1. Abrir el proyecto en Unity 6000.3.16f1 y esperar a que importe los audios.
2. Menú **Tools > Abril > Configurar Musica** (ajusta los .wav y crea `Assets/Resources/MusicManager.prefab`).
3. Menú **Tools > Abril > Verificar Musica** → en la Console debe salir `[Musica] OK`.
   Si sale un warning, dice exactamente qué falta.

## Pruebas (Play en cada escena)
| # | Qué probar | Esperado |
|---|-----------|----------|
| 1 | Play desde **TitleScreen** | Suena Music_Title en loop, sin cortes al repetir |
| 2 | Iniciar juego → **SampleScene** (1-1) | Cambia a Music_Overworld con fade; en loop sin clic |
| 3 | Entrar a un tubo → **Underground** | Cambia a Music_Underground |
| 4 | Llegar a **Underwater** (2-2) | Cambia a Music_Underwater |
| 5 | Pantalla de intro del nivel (2 s) | La música se pausa y sigue al empezar |
| 6 | Tocar la **bandera** | La música se detiene (suena solo el jingle de la bandera) |
| 7 | **Morir** y reiniciar nivel | La música vuelve a empezar desde el principio |
| 8 | Quedarse sin vidas → **GameOver** | Suena Music_GameOver una sola vez (sin loop) |
| 9 | **World2_3Pending** | Silencio (no tiene pista) |
| 10 | Dejar un nivel 2 minutos | Sin cortes, sin bajón de FPS, sin errores en Console |

## SFX no tapados
- Con SFX ya puestos (moneda, salto, golpe de bloque), jugar 1 minuto en SampleScene.
- Los SFX se deben oír **claramente** encima de la música.
- Si la música tapa: seleccionar `Assets/Resources/MusicManager.prefab` → bajar **Music Volume**
  (por defecto 0.18; probar 0.12). Los SFX con `PlayClipAtPoint` son 3D y la cámara está lejos,
  por eso suenan más bajos: si los SFX quedan flojos, es mejor subirlos a ellos que subir la música.
- Si aún no hay SFX en `Assets/Audio/SFX`, repetir esta prueba cuando el equipo los suba.

## Antes de hacer commit
- Console sin errores rojos al jugar.
- `git status` muestra solo: `Assets/Audio/Music/*`, `Assets/Scripts/Audio/`, `Assets/Scripts/Editor/MusicSetupEditor.cs*`,
  `Assets/Resources/MusicManager.prefab*`, `Tools/*` y los `.meta` nuevos. **Nada de escenas modificadas.**

## Regenerar la música (opcional)
`python3 Tools/generate_music.py` (necesita numpy y scipy). Melodías originales, sin derechos de autor.
