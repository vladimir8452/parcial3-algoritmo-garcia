// vladimir Garcia Rojas

using System.Diagnostics;

namespace Integradora.VarianteA;

public sealed record Paquete(string Codigo, int Kg, int Ganancia);

public interface IPlanificador
{
    
    (int ganancia, List<string> codigos) Elegir(Paquete[] paquetes, int capacidadKg);

    
    bool EstaEnElViaje(string codigo);
}


public sealed class PlanificadorIngenuo : IPlanificador
{
    private List<string> _elegidos = new();

    public (int ganancia, List<string> codigos) Elegir(Paquete[] paquetes, int capacidadKg)
    {
        int n = paquetes.Length, mejor = 0;
        long mejorMascara = 0;
        for (long mascara = 0; mascara < (1L << n); mascara++)
        {
            int kg = 0, ganancia = 0;
            for (int i = 0; i < n; i++)
                if ((mascara & (1L << i)) != 0) { kg += paquetes[i].Kg; ganancia += paquetes[i].Ganancia; }
            if (kg <= capacidadKg && ganancia > mejor) { mejor = ganancia; mejorMascara = mascara; }
        }
        _elegidos = new List<string>();
        for (int i = 0; i < n; i++) if ((mejorMascara & (1L << i)) != 0) _elegidos.Add(paquetes[i].Codigo);
        return (mejor, new List<string>(_elegidos));
    }

    public bool EstaEnElViaje(string codigo) => _elegidos.Contains(codigo);          
}


public sealed class PlanificadorMejorado : IPlanificador
{
    
    private HashSet<string> _elegidos = new();

    
    public (int ganancia, List<string> codigos) Elegir(Paquete[] paquetes, int capacidadKg)
    {
        int n = paquetes.Length;
        int W = Math.Max(0, capacidadKg);          
        var dp = new int[n + 1, W + 1];          

        for (int i = 1; i <= n; i++)
        {
            var p = paquetes[i - 1];
            for (int w = 0; w <= W; w++)
            {
                dp[i, w] = dp[i - 1, w];                         
                if (p.Kg <= w)                                   
                {
                    int con = dp[i - 1, w - p.Kg] + p.Ganancia;  
                    if (con > dp[i, w]) dp[i, w] = con;
                }
            }
        }

        
        var codigos = new List<string>();
        int cap = W;
        for (int i = n; i >= 1; i--)
        {
            if (dp[i, cap] != dp[i - 1, cap])
            {
                codigos.Add(paquetes[i - 1].Codigo);
                cap -= paquetes[i - 1].Kg;
            }
        }

        _elegidos = new HashSet<string>(codigos);
        return (dp[n, W], codigos);
    }

    public bool EstaEnElViaje(string codigo) => _elegidos.Contains(codigo);
}

public static class Programa
{
   
  
    private const int NBase = 22;

    private static Paquete[] Lote(params (string c, int kg, int g)[] datos) => datos.Select(d => new Paquete(d.c, d.kg, d.g)).ToArray();

    
    private static readonly (string que, Paquete[] paquetes, int capacidad, int esperada, string[] codigos)[] Casos =
    {
        ("el voraz se equivoca", Lote(("P1", 10, 60), ("P2", 20, 100), ("P3", 30, 120)), 50, 220, new[] { "P2", "P3" }),
        ("cuatro paquetes",      Lote(("P1", 2, 3), ("P2", 3, 4), ("P3", 4, 5), ("P4", 5, 6)), 8, 10, new[] { "P2", "P4" }),
        ("capacidad cero",       Lote(("P1", 5, 10), ("P2", 7, 12)), 0, 0, Array.Empty<string>()),
        ("sin paquetes",         Lote(), 30, 0, Array.Empty<string>()),
        ("nada cabe",            Lote(("P1", 40, 90), ("P2", 50, 100)), 30, 0, Array.Empty<string>()),
        ("entran todos",         Lote(("P1", 5, 10), ("P2", 7, 12), ("P3", 3, 4)), 100, 26, new[] { "P1", "P2", "P3" }),
    };

    private static void Probar(string titulo, Func<IPlanificador> fabrica)
    {
        int bien = 0, total = 0;
        try
        {
            foreach (var (que, paquetes, capacidad, esperada, codigos) in Casos)
            {
                var plan = fabrica();
                var (ganancia, elegidos) = plan.Elegir(paquetes, capacidad);
                total++;
                bool ok = ganancia == esperada && elegidos.OrderBy(x => x).SequenceEqual(codigos.OrderBy(x => x));
                if (ok) bien++;
                else Console.WriteLine($"   MAL en «{que}»: esperado {esperada} con [{string.Join(",", codigos)}], obtuvo {ganancia} con [{string.Join(",", elegidos)}]");
            }
            var p = fabrica();
            p.Elegir(Casos[0].paquetes, Casos[0].capacidad);
            total += 2;
            if (p.EstaEnElViaje("P2")) bien++; else Console.WriteLine("   MAL en «EstaEnElViaje(P2)»: esperado True");
            if (!p.EstaEnElViaje("P1")) bien++; else Console.WriteLine("   MAL en «EstaEnElViaje(P1)»: esperado False");
            Console.WriteLine($"{titulo}: {bien} de {total} pruebas correctas");
        }
        catch (NotImplementedException) { Console.WriteLine($"{titulo}: (sin escribir todavía)"); }
    }

    
    public static int EjecutarCarga(IPlanificador plan, int n)
    {
        var azar = new Random(n);
        var paquetes = Enumerable.Range(0, n).Select(i => new Paquete("C" + i, azar.Next(5, 60), azar.Next(10, 100))).ToArray();
        int capacidad = paquetes.Sum(p => p.Kg) / 3;
        var (ganancia, _) = plan.Elegir(paquetes, capacidad);
        return ganancia;
    }

    
    private static (double ms, int ganancia) MedirCarga(Func<IPlanificador> fabrica, int n)
    {
        EjecutarCarga(fabrica(), n);                       
        var tiempos = new double[3];
        int ganancia = 0;
        for (int k = 0; k < 3; k++)
        {
            var plan = fabrica();
            GC.Collect();
            var reloj = Stopwatch.StartNew();
            ganancia = EjecutarCarga(plan, n);
            reloj.Stop();
            tiempos[k] = reloj.Elapsed.TotalMilliseconds;
        }
        Console.WriteLine($"      n={n} corridas (ms): {string.Join(" | ", tiempos.Select(t => t.ToString("F3")))}");
        return (tiempos.Average(), ganancia);
    }

    
    private static void Medicion(int n)
    {
        Console.WriteLine("n | ingenuo ms | x anterior | mejorado ms | x anterior | misma ganancia");
        double? antI = null, antM = null;
        for (int tam = n; tam <= n + 6; tam += 2)
        {
            var (msI, gI) = MedirCarga(() => new PlanificadorIngenuo(), tam);
            var (msM, gM) = MedirCarga(() => new PlanificadorMejorado(), tam);
            string rI = antI is null ? "-" : $"x{msI / antI.Value:F2}";
            string rM = antM is null ? "-" : $"x{msM / antM.Value:F2}";
            Console.WriteLine($"{tam} | {msI:F2} | {rI} | {msM:F4} | {rM} | {(gI == gM ? "SI" : "NO (" + gI + " vs " + gM + ")")}");
            antI = msI; antM = msM;
        }
    }

    
    private static void MedirMil()
    {
        var azar = new Random(1000);
        var paquetes = Enumerable.Range(0, 1000)
            .Select(i => new Paquete("C" + i, azar.Next(5, 60), azar.Next(10, 100))).ToArray();

        new PlanificadorMejorado().Elegir(paquetes, 10000);   
        var tiempos = new double[3];
        int ganancia = 0;
        for (int k = 0; k < 3; k++)
        {
            var plan = new PlanificadorMejorado();
            GC.Collect();
            var reloj = Stopwatch.StartNew();
            ganancia = plan.Elegir(paquetes, 10000).ganancia;
            reloj.Stop();
            tiempos[k] = reloj.Elapsed.TotalMilliseconds;
        }
        Console.WriteLine($"1000 paquetes, capacidad 10000: ganancia {ganancia}, corridas (ms): " +
                          $"{string.Join(" | ", tiempos.Select(t => t.ToString("F2")))}, promedio {tiempos.Average():F2} ms");
    }

    public static void Main()
    {
        Probar("PlanificadorIngenuo ", () => new PlanificadorIngenuo());
        Probar("PlanificadorMejorado", () => new PlanificadorMejorado());

        Console.WriteLine();
        Medicion(NBase);
        Console.WriteLine();
        MedirMil();
    }
}
