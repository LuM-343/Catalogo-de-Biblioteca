using System;
using System.IO;

namespace EstructurasSuperPros
{
    public static class GestorArchivos
    {
        public static void CargarLibros(
            string rutaArchivo,
            ArbolBPlus<int, Libro> arbolBPlus,
            MaxHeapLibros maxHeap,
            MinHeapLibros minHeap)
        {
            if (string.IsNullOrWhiteSpace(rutaArchivo))
            {
                Console.WriteLine("[Error] La ruta del archivo no puede estar vacía.");
                return;
            }

            if (!File.Exists(rutaArchivo))
            {
                Console.WriteLine($"[Error] El archivo no existe en la ruta: {rutaArchivo}");
                return;
            }

            int insertados = 0;
            int omitidos = 0;
            int numeroLinea = 0;

            try
            {
                using (StreamReader lector = new StreamReader(rutaArchivo))
                {
                    string linea;
                    while ((linea = lector.ReadLine()) != null)
                    {
                        numeroLinea++;
                        linea = linea.Trim();

                        if (string.IsNullOrEmpty(linea))
                            continue;

                        // Detectar separador (coma o punto y coma)
                        char separador = linea.Contains(';') ? ';' : ',';
                        string[] partes = linea.Split(separador);

                        // Se requieren 8 columnas según el modelo de Libro
                        if (partes.Length < 8)
                        {
                            Console.WriteLine($"[Advertencia] Línea {numeroLinea} ignorada (columnas insuficientes).");
                            omitidos++;
                            continue;
                        }

                        // Ignorar encabezado si la primera columna no es numérica
                        if (!int.TryParse(partes[0].Trim(), out int id))
                        {
                            if (numeroLinea == 1) continue; // Encabezado detectado
                            Console.WriteLine($"[Advertencia] Línea {numeroLinea} ignorada (ID inválido).");
                            omitidos++;
                            continue;
                        }

                        string titulo = partes[1].Trim();
                        string autor = partes[2].Trim();

                        if (!int.TryParse(partes[3].Trim(), out int anio) ||
                            !int.TryParse(partes[5].Trim(), out int copiasTotales) ||
                            !int.TryParse(partes[6].Trim(), out int copiasDisponibles) ||
                            !int.TryParse(partes[7].Trim(), out int prestamosTotales))
                        {
                            Console.WriteLine($"[Advertencia] Línea {numeroLinea} ignorada (error en datos numéricos).");
                            omitidos++;
                            continue;
                        }

                        string genero = partes[4].Trim();

                        // Validar consistencia de copias
                        if (copiasDisponibles > copiasTotales)
                            copiasDisponibles = copiasTotales;

                        Libro nuevoLibro = new Libro(
                            id,
                            titulo,
                            autor,
                            anio,
                            genero,
                            copiasTotales,
                            copiasDisponibles,
                            prestamosTotales
                        );

                        // Inserción en el Árbol B+ (valida si la clave ya existe)
                        bool insertado = arbolBPlus.Insertar(nuevoLibro);

                        if (insertado)
                        {
                            // Poblar montículos con la misma referencia en memoria
                            maxHeap.Insertar(nuevoLibro);
                            minHeap.Insertar(nuevoLibro);
                            insertados++;
                        }
                        else
                        {
                            Console.WriteLine($"[Duplicado] El libro con ID {id} ya existe en el sistema.");
                            omitidos++;
                        }
                    }
                }

                Console.WriteLine("\n========================================");
                Console.WriteLine("       RESUMEN DE CARGA MASIVA          ");
                Console.WriteLine("========================================");
                Console.WriteLine($"* Libros cargados con éxito: {insertados}");
                Console.WriteLine($"* Registros omitidos:        {omitidos}");
                Console.WriteLine("========================================\n");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"[Error de E/S] No se pudo leer el archivo: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error inesperado] {ex.Message}");
            }
        }
    }
}