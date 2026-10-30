# Aplicar las correcciones

1. Detener Play y esperar a que Unity termine de compilar.
2. Ejecutar **Tools > Mario > Rebuild All Levels (1-1, 1-2, 2-2)**.
3. Pulsar Play. El generador configura SampleScene (1-1) como escena inicial del editor y del build.

El generador reconstruye los niveles guardados. Las ediciones manuales de esos niveles deben guardarse en una copia antes de reconstruir.

## Rutas

- Bandera del 1-1: Underground (1-2).
- Salida normal del 1-2: Underwater (2-2).
- Tres tubos del 1-2, de izquierda a derecha: 2-2, 1-2 y 2-3 pendiente. Entrar desde arriba con Abajo/S.
- Salida del 2-2: pantalla 2-3 pendiente.
- Pantalla pendiente: Enter o botón para regresar al 1-2 conservando puntos, monedas y vidas. No contiene un nivel 2-3 jugable.

## Comprobar en Play

- Goombas atraviesan pasillos de un bloque sin invertir dirección por el techo; rebotan ante paredes.
- Pisar Koopa lo convierte en caparazón; patearlo lo mueve, rebota en paredes y mata enemigos. No vuelve a crecer mientras tenga un techo encima.
- Peces y corales dañan al jugador; se respeta el periodo de invulnerabilidad.
- Castillo: 5 × 5 unidades. Poste y base: 1 × 11. Escaleras: textura de ladrillo, bloques de 1 × 1.
- La gran tubería vertical del 1-2 conserva la cuadrícula del mapa original, sin escala adicional.

## Verificación realizada fuera de Unity

Compilación C# con las referencias de Unity del proyecto, comprobación de los recortes originales, altura de las bocas de los tubos y espacio inicial de los Goombas. La prueba de física y recorrido en Play sigue siendo necesaria. El generador además valida sprites, hitboxes acuáticas y destinos de los tubos antes de guardar cada nivel.
