vladimir garcia rojas 
# I3 — Medir

## Protocolo

- Lenguaje: C# (.NET ___), compilado en **Release** (`dotnet run -c Release`).
- Equipo: ___ (CPU, RAM, sistema operativo).
- Carga: `EjecutarCarga(plan, n)`: n paquetes al azar con semilla fija y capacidad de un tercio del peso total. Es la misma carga para las dos soluciones.
- Por cada tamaño: **1 corrida de calentamiento** (descartada) + **3 corridas medidas**; se reporta el **promedio**. Tiempo medido con `Stopwatch`.
- Verificación: las dos soluciones devolvieron la **misma ganancia** en todos los tamaños (columna «Misma ganancia»).

## Tabla (cuatro tamaños: n, n+2, n+4, n+6)

| n | Ingenua (ms) | Veces vs. fila anterior | Mejorada (ms) | Veces vs. fila anterior | Misma ganancia |
|---|--------------|-------------------------|---------------|-------------------------|----------------|
| ___ | ___ | – | ___ | – | sí |
| ___ | ___ | ×___ | ___ | ×___ | sí |
| ___ | ___ | ×___ | ___ | ×___ | sí |
| ___ | ___ | ×___ | ___ | ×___ | sí |

Corridas individuales (ms), para mostrar la dispersión:

- n = ___: ingenua ___ / ___ / ___; mejorada ___ / ___ / ___
- (una línea por tamaño)

## Conclusión

*(Reescribe con tus números; esta es la estructura honesta.)*

- Cada +2 paquetes, la ingenua se multiplicó por ≈ ___ (la teoría dice ≈ 4, o un poco más, ≈ 4,4, por el factor n de O(n·2ⁿ)). Con tres corridas por tamaño la coincidencia es aproximada, no exacta.
- La mejorada casi no cambia entre tamaños: con la capacidad igual a un tercio del peso total, W crece con n y el costo O(n·W) crece más o menos como n², o sea ≈ ×1,2 a ×1,4 por cada +2. En los tamaños pequeños tarda fracciones de milisegundo, así que **el ruido (JIT, recolector de basura, otros procesos, resolución del reloj) es del mismo orden que la medida** y esas razones no son fiables.
- Tres corridas son pocas: sirven para ver el orden de magnitud y la tendencia (exponencial contra casi plana), no para afirmar el factor exacto. Con más corridas o con la mediana la estimación mejoraría.
- Lo que sí es sólido: con n = ___ la ingenua tarda ___ s y la mejorada ___ ms, una diferencia de varios órdenes de magnitud, mucho mayor que cualquier ruido.

## 1.000 paquetes y capacidad 10.000 (solo la mejorada)

Promedio de 3 corridas (tras 1 de calentamiento): **___ ms** (corridas: ___ / ___ / ___). La ganancia obtenida fue ___.

La tabla dp ocupa 1.001 × 10.001 ≈ 10 millones de enteros (≈ 40 MB), y el trabajo es de unos 10 millones de pasos.

## ¿Y con capacidad de 1.000.000.000 de kg?

Mi solución cuesta **O(n·W)** en tiempo y en memoria, y W pasaría de 10.000 a 10⁹, es decir, **100.000 veces más**. Con n = 1.000:

- **Memoria:** la tabla tendría ≈ 1.001 × 10⁹ ≈ 10¹² enteros, unos **4 TB**. No cabe en ninguna máquina normal, y además supera el máximo de elementos que .NET permite en un arreglo, así que fallaría de inmediato con `OutOfMemoryException`.
- **Tiempo:** aunque hubiera memoria, serían ≈ 10¹² pasos. Si 10⁷ pasos tardaron ___ ms (mi medición anterior), 10¹² tardarían unas 100.000 veces más: ___ minutos u horas.

**Por qué:** el algoritmo es **pseudopolinomial**. O(n·W) es polinomial en el *valor* de W, pero W se escribe con log₂ W bits (≈ 30 bits para 10⁹), así que el costo es **exponencial en el tamaño de la entrada**: cada dígito más en W multiplica el costo por 10. La programación dinámica gana a la fuerza bruta cuando W es moderado, pero no es magia: la mochila 0/1 es NP-completa.

**Qué se podría hacer:**

- Si W ≥ suma de todos los pesos, todo cabe: se lleva todo y no hace falta tabla. En ese caso conviene hacer `W = Math.Min(W, sumaKg)` antes de crear la tabla; con 1.000 paquetes de 5 a 59 kg, la suma ronda los 32.000 kg y la tabla sería de solo ≈ 32 millones de celdas.
- Si W es enorme pero las ganancias son pequeñas, hacer la DP sobre la ganancia (O(n · ΣGanancia)) en lugar del peso.
- Un arreglo 1D de tamaño W + 1 reduce la memoria, pero con W = 10⁹ sigue siendo ≈ 4 GB, y no permite reconstruir los paquetes elegidos.
- Con n pequeño, «meet in the middle» (O(2^(n/2))); con n y W grandes, algoritmos aproximados.
