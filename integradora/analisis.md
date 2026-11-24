vladimir garcia rojas
# I1 — Analizar y elegir

## a) Costo O de `PlanificadorIngenuo.Elegir`

El método recorre **todas las máscaras** de 0 a 2ⁿ − 1, es decir, los 2ⁿ subconjuntos posibles de paquetes. Para cada máscara hace un bucle interno de n pasos (revisa los n bits y suma kg y ganancia). Después reconstruye la lista elegida en O(n), lo cual es despreciable.

- Subconjuntos: 2ⁿ
- Trabajo por subconjunto: n
- **Costo total: O(n · 2ⁿ)** (exponencial).

Con n = 18 son unos 4,7 millones de operaciones; cada paquete extra duplica el tiempo, por eso no escala.

## b) Recursión «lo llevo o no lo llevo»

Cada llamada con n paquetes genera **dos** llamadas con n − 1 paquetes (una llevándolo, otra sin llevarlo) y hace trabajo constante:

T(n) = 2 T(n − 1) + 1

**Por qué no se aplica el teorema maestro de bolsillo:** ese teorema es para recurrencias de la forma T(n) = a·T(n/b) + f(n), donde el problema se **divide** entre una constante b > 1. Aquí el subproblema se reduce **restando** (n − 1), no dividiendo, así que no tiene la forma requerida.

**Cómo se resuelve:** por expansión.

T(n) = 2T(n−1) + 1
     = 4T(n−2) + 2 + 1
     = 8T(n−3) + 4 + 2 + 1
     = …
     = 2ᵏ T(n−k) + (2ᵏ − 1)

Con k = n y T(0) = 1: T(n) = 2ⁿ + 2ⁿ − 1 = 2ⁿ⁺¹ − 1.

**Costo: O(2ⁿ)** (el árbol de llamadas es un árbol binario de altura n). La profundidad de la pila es O(n).

## c) ¿Sirve un algoritmo voraz? Elección de estrategia

**El voraz por ganancia/kg no sirve.** Contraejemplo con el primer caso de las pruebas (capacidad 50 kg):

| Paquete | Kg | Ganancia | Ganancia/kg |
|---------|----|----------|-------------|
| P1 | 10 | 60 | 6 |
| P2 | 20 | 100 | 5 |
| P3 | 30 | 120 | 4 |

- **Voraz:** toma P1 (10 kg, 60) y luego P2 (20 kg, 100). Lleva 30 kg y gana **160**. P3 ya no cabe (30 + 30 = 60 > 50).
- **Óptimo:** P2 + P3 = 50 kg, ganancia **220**.

160 < 220, luego el voraz falla: la mejor razón ganancia/kg no garantiza el mejor uso de la capacidad, porque el paquete entero no se puede fraccionar.

**Estrategia elegida: programación dinámica (mochila 0/1).**

**Qué se repite:** en la recursión de la parte b) los mismos subproblemas aparecen una y otra vez. El estado es **(i, w)**: «mejor ganancia usando los primeros i paquetes con capacidad w». Hay solo (n + 1)(W + 1) estados distintos, aunque la recursión visita 2ⁿ caminos (**subproblemas superpuestos**). Además, la solución óptima contiene soluciones óptimas de subproblemas (**subestructura óptima**).

**Recurrencia:**

- dp[0][w] = 0 y dp[i][0] = 0
- Si kgᵢ > w: dp[i][w] = dp[i−1][w]
- Si no: dp[i][w] = max( dp[i−1][w], dp[i−1][w − kgᵢ] + gananciaᵢ )

La respuesta es dp[n][W]. Para saber **cuáles** paquetes se eligieron se recorre la tabla hacia atrás desde (n, W): si dp[i][w] ≠ dp[i−1][w], el paquete i se llevó y se resta su peso de w.

**Costo:**

- Tiempo: **O(n · W)**, con W la capacidad en kg.
- Memoria: O(n · W) con la tabla completa (necesaria para reconstruir los paquetes). Solo para la ganancia bastaría un arreglo de tamaño W + 1.
- Reconstrucción: O(n).
- Es **pseudopolinomial**: depende del valor de W, no solo del tamaño de la entrada. Es muy superior a O(n · 2ⁿ) mientras W sea moderado.

## d) Estructura para `EstaEnElViaje`

**Estructura: `HashSet<string>`** con los códigos de los paquetes elegidos, que se llena al final de `Elegir`.

| Opción | Construcción | `EstaEnElViaje` (1 consulta) | q consultas |
|--------|--------------|------------------------------|-------------|
| `List<string>` (ingenuo) | O(k) | **O(k)** (recorre la lista) | O(q · k) |
| `HashSet<string>` | O(k) promedio | **O(1)** promedio | O(q) |

donde k es la cantidad de paquetes elegidos.

**Por qué:** el `HashSet` calcula el hash del código y salta directo a su posición, sin recorrer los elementos. Como la consulta se hace **miles de veces**, pasar de O(k) a O(1) por consulta reduce el total de O(q · k) a O(q). El costo extra es solo la memoria O(k) y el llenado una vez en `Elegir`. En el peor caso (muchas colisiones) una consulta puede degradarse a O(k), pero con un buen hash eso no ocurre en la práctica.
