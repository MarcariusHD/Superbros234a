# Cooperativo local y flor de fuego

En TitleScreen, usar arriba/abajo y Enter o Espacio para seleccionar uno o dos jugadores. También se puede hacer clic en cada opción.

| Acción | Mario | Luigi |
|---|---|---|
| Caminar | A / D | Flechas izquierda / derecha |
| Agacharse o entrar en tubo | S | Flecha abajo |
| Saltar o nadar | Espacio o Z | Ctrl derecho |
| Correr | Shift izquierdo o X | Shift derecho |
| Disparar | Shift izquierdo o X | Alt derecho |

El modo de un jugador conserva también las flechas para mover a Mario.

La cámara sigue al jugador que va más atrás. El adelantado queda limitado por el borde visible. Ambos comparten puntuación, monedas, tiempo y vidas: una muerte reinicia el nivel para los dos y descuenta una vida. Los tubos y el castillo trasladan al equipo. Los poderes se conservan al cambiar de nivel y se pierden al morir.

En cooperativo, los bloques de mejora entregan hongos hasta que Mario y Luigi sean grandes; solo entonces entregan flores. En un jugador se mantiene la regla de hongo para pequeño y flor para grande. Cada jugador tiene su propio estado de poder y un máximo de dos bolas de fuego simultáneas. Las bolas rebotan en el suelo, desaparecen ante paredes y derrotan enemigos; no dañan al compañero.

## Comprobación en Unity

Detener Play, esperar la importación y probar desde TitleScreen. No es necesario reconstruir los niveles.

1. Comprobar ambas opciones del menú con teclado y clic.
2. En dos jugadores, dejar a Luigi quieto y caminar con Mario hasta el borde: la cámara espera a Luigi. Repetir intercambiando los jugadores.
3. Recoger el hongo con cada jugador. Golpear otro bloque de mejora para obtener la flor; comprobar el cambio de aspecto y los disparos independientes.
4. Disparar contra el suelo, tubos, Goombas y Koopas. Comprobar el límite de dos disparos por jugador y que el compañero no recibe daño.
5. Probar la entrada y salida del atajo, la transición de nivel, el castillo y una muerte de cualquiera de los dos.

La compilación C# y los recursos se pueden comprobar sin abrir el editor. La física y los controles simultáneos requieren esta prueba en Play.

## Mundo 2-3: castillo y lava

Recorrido nuevo con ladrillos y lava de las referencias 1-4, 2-4 y 4-4. Incluye cuatro bloques de mejora, diez bloques de monedas (uno elevado), Goombas, Koopas, una planta, una burbuja de lava . Los enemigos están separados en grupos de hasta tres por una vista de ancho estándar.

Solo hay una plataforma móvil: cruza horizontalmente el lago grande y transporta a ambos jugadores. Los lagos causan muerte incluso con poderes; la burbuja hace daño normal.

Al final, saltar bajo el tubo invertido golpeando su boca desde abajo al saltar, o manteniendo Arriba (W con Mario; flecha arriba con Luigi). Ambos salen al exterior y caminan automáticamente al castillo. Hay seis fuegos artificiales y una pantalla de nivel completado; Enter regresa al menú.

Probar en Play el transporte horizontal, los saltos sobre lava, el bloque elevado y el final con cada personaje como primero en entrar. El nivel se crea al pulsar Play, sin reconstruir los demás niveles.

Correcciones de distribución: una sola superficie por lago, bloques de monedas a la altura 5 y el elevado a la altura 9, techo a la altura 11 unido al tubo invertido, sin barra de fuego y con escalones intermedios.

El tramo de la segunda poza queda libre de los escalones y bloques cercanos, y de las monedas a su derecha. La burbuja comienza oculta bajo la lava y se dibuja detrás de ella. Al salir al exterior, el borde izquierdo de cámara queda un bloque detrás del tubo y el desplazamiento continúa solo hacia delante.

## Atajo A/B del 1-1

En el cuarto tubo alto (A), centrarse encima y mantener Abajo: S con Mario o flecha abajo con Luigi. Se entra a la sala original de 19 monedas. Para regresar, avanzar contra la boca del tubo horizontal B manteniendo Derecha. La salida aparece en el tubo del tramo final, cerca de las escaleras. En cooperativo viajan ambos; se mantienen los poderes, puntos, monedas y vidas. La música cambia a subterránea dentro de la sala y regresa al tema del 1-1 al salir.

La sala se agrega automáticamente al cargar SampleScene, sin reconstruir niveles. Probar el trayecto iniciándolo con Mario y luego con Luigi; la cámara debe mostrar el tubo de regreso en ambos casos.
