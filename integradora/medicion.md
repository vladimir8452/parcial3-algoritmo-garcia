Vladimir Garcia Rojas

# I3 — Medir

## Protocolo

- Lenguaje: C# (.NET 10.0.403), compilado y ejecutado en **Release** (`dotnet run -c Release`).
- Equipo: Intel Core i7-11700F @ 2,50 GHz, 8 GB de RAM, Windows 10 Enterprise LTSC.
- Carga: `EjecutarCarga(plan, n)`: n paquetes al azar con semilla fija y capacidad de un tercio del peso total. Es la misma carga para las dos soluciones.
- Por cada tamaño: **1 corrida de calentamiento** (descartada) + **3 corridas medidas**; se reporta el **promedio**. Tiempo medido con `Stopwatch`.
- Verificación: las dos soluciones devolvieron la **misma ganancia** en los cuatro tamaños (columna «Misma ganancia»). Ambas pasan las 8 de 8 pruebas.

## Tabla (cuatro tamaños: n, n+2, n+4, n+6)

| n | Ingenua (ms) | Veces vs. fila anterior | Mejorada (ms) | Veces vs. fila anterior | Misma ganancia |
|---|--------------|-------------------------|---------------|-------------------------|----------------|
| 22 | 237,16 | – | 0,7758 | – | sí |
| 24 | 951,30 | ×4,01 | 0,0519 | ×0,07 | sí |
| 26 | 3.933,41 | ×4,13 | 0,0441 | ×0,85 | sí |
| 28 | 16.094,58 | ×4,09 | 0,0498 | ×1,13 | sí |

Corridas individuales (ms), para mostrar la dispersión:

- n = 22: ingenua 241,058 / 236,098 / 234,333; mejorada 0,086 / 2,176 / 0,065
- n = 24: ingenua 946,742 / 946,821 / 960,340; mejorada 0,048 / 0,053 / 0,054
- n = 26: ingenua 3.949,891 / 3.925,472 / 3.924,867; mejorada 0,045 / 0,043 / 0,044
- n = 28: ingenua 16.233,822 / 15.883,204 / 16.166,720; mejorada 0,050 / 0,055 / 0,045

## Conclusión

- **Ingenua:** cada +2 paquetes el tiempo se multiplicó por ≈ 4 (×4,01, ×4,13 y ×4,09), como predice O(n·2ⁿ): dos paquetes más son 2² = 4 veces más subconjuntos. La teoría con el factor n daría ≈ ×4,3; lo medido queda un poco por debajo, y no puedo afirmar la causa con solo tres corridas (puede deberse a que el costo por subconjunto no crece exactamente con n). Las tres corridas de cada tamaño son muy parecidas entre sí (por ejemplo, 16,2 / 15,9 / 16,2 s en n = 28), así que en la ingenua el ruido es pequeño y la tendencia exponencial es clara.
- **Mejorada:** casi no cambia entre tamaños, y sus razones (×0,07, ×0,85, ×1,13) **no son fiables**. Tarda unas 0,05 ms (50 microsegundos), y a esa escala el ruido (JIT, recolector de basura, otros procesos, resolución del reloj) es del mismo orden que la medida. El caso n = 22 lo muestra: una corrida aislada de 2,176 ms, contra 0,086 y 0,065 ms de las otras dos, subió el promedio a 0,7758 ms y por eso la fila siguiente parece «bajar ×0,07». La teoría (O(n·W) con W ∝ n, o sea ≈ n²) predice un crecimiento suave, de ≈ ×1,2 por cada +2, que estas mediciones son demasiado pequeñas para distinguir.
- **Limitaciones:** tres corridas son pocas. Sirven para ver el orden de magnitud y la tendencia (exponencial contra casi plana), no para dar el factor exacto. Con más corridas o con la mediana, el valor atípico de n = 22 no habría distorsionado el promedio.
- **Lo que sí es sólido:** con n = 28 la ingenua tarda ≈ 16,1 s y la mejorada ≈ 0,05 ms, una diferencia de más de cinco órdenes de magnitud, mucho mayor que cualquier ruido, y las dos dan la misma ganancia.

## 1.000 paquetes y capacidad 10.000 (solo la mejorada)

Promedio de 3 corridas (tras 1 de calentamiento): **25,84 ms** (corridas: 26,26 / 25,79 / 25,48). La ganancia obtenida fue 32.197.

La tabla dp ocupa 1.001 × 10.001 ≈ 10 millones de enteros (≈ 40 MB), y el trabajo es de unos 10 millones de pasos.

## ¿Y con capacidad de 1.000.000.000 de kg?

Mi solución cuesta **O(n·W)** en tiempo y en memoria, y W pasaría de 10.000 a 10⁹, es decir, **100.000 veces más**. Con n = 1.000:

- **Memoria:** la tabla tendría ≈ 1.001 × 10⁹ ≈ 10¹² enteros, unos **4 TB**. No cabe en ninguna máquina normal, y además supera el máximo de elementos que .NET permite en un arreglo, así que fallaría de inmediato con `OutOfMemoryException`.
- **Tiempo:** aunque hubiera memoria, serían ≈ 10¹² pasos. Como 10⁷ pasos tardaron 25,84 ms (medición anterior), 10¹² tardarían unas 100.000 veces más: ≈ 2.584 s, es decir, **≈ 43 minutos**, y eso es una cota optimista porque supone que el acceso a memoria sería igual de rápido.

**Por qué:** el algoritmo es **pseudopolinomial**. O(n·W) es polinomial en el *valor* de W, pero W se escribe con log₂ W bits (≈ 30 bits para 10⁹), así que el costo es **exponencial en el tamaño de la entrada**: cada dígito más en W multiplica el costo por 10. La programación dinámica gana a la fuerza bruta cuando W es moderado, pero no es magia: la mochila 0/1 es NP-completa.

**Qué se podría hacer:**

- Si W ≥ suma de todos los pesos, todo cabe: se lleva todo y no hace falta tabla. En ese caso conviene hacer `W = Math.Min(W, sumaKg)` antes de crear la tabla; con 1.000 paquetes de 5 a 59 kg, la suma ronda los 32.000 kg y la tabla sería de solo ≈ 32 millones de celdas.
- Si W es enorme pero las ganancias son pequeñas, hacer la DP sobre la ganancia (O(n · ΣGanancia)) en lugar del peso.
- Un arreglo 1D de tamaño W + 1 reduce la memoria, pero con W = 10⁹ sigue siendo ≈ 4 GB, y no permite reconstruir los paquetes elegidos.
- Con n pequeño, «meet in the middle» (O(2^(n/2))); con n y W grandes, algoritmos aproximados.
