using System;
using System.IO;

namespace EstructurasSuperPros
{
    public static class GestorArchivos
    {
        // ---------------------------------------------------------
        // CARGA DE LIBROS
        // ---------------------------------------------------------
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

            int insertados = 0, omitidos = 0, numeroLinea = 0;

            try
            {
                using (StreamReader lector = new StreamReader(rutaArchivo))
                {
                    string linea;
                    while ((linea = lector.ReadLine()) != null)
                    {
                        numeroLinea++;
                        linea = linea.Trim();
                        if (string.IsNullOrEmpty(linea)) continue;

                        char separador = linea.Contains(';') ? ';' : ',';
                        string[] partes = linea.Split(separador);

                        if (partes.Length < 8)
                        {
                            omitidos++;
                            continue;
                        }

                        if (!int.TryParse(partes[0].Trim(), out int id))
                        {
                            if (numeroLinea == 1) continue; // Encabezado
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
                            omitidos++;
                            continue;
                        }

                        string genero = partes[4].Trim();
                        if (copiasDisponibles > copiasTotales) copiasDisponibles = copiasTotales;

                        Libro nuevoLibro = new Libro(id, titulo, autor, anio, genero, copiasTotales, copiasDisponibles, prestamosTotales);

                        if (arbolBPlus.Insertar(nuevoLibro))
                        {
                            maxHeap.Insertar(nuevoLibro);
                            minHeap.Insertar(nuevoLibro);
                            insertados++;
                        }
                        else
                        {
                            omitidos++;
                        }
                    }
                }

                Console.WriteLine($"\n[Carga Libros] Éxito: {insertados} | Omitidos/Duplicados: {omitidos}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error de lectura] {ex.Message}");
            }
        }

        // ---------------------------------------------------------
        // CARGA DE CLIENTES
        // ---------------------------------------------------------
        public static void CargarClientes(
            string rutaArchivo,
            ref Cliente[] clientes,
            ref int conteoClientes)
        {
            if (string.IsNullOrWhiteSpace(rutaArchivo))
            {
                Console.WriteLine("[Error] La ruta no puede estar vacía.");
                return;
            }

            if (!File.Exists(rutaArchivo))
            {
                Console.WriteLine($"[Error] Archivo no encontrado: {rutaArchivo}");
                return;
            }

            int insertados = 0, omitidos = 0, numeroLinea = 0;

            try
            {
                using (StreamReader lector = new StreamReader(rutaArchivo))
                {
                    string linea;
                    while ((linea = lector.ReadLine()) != null)
                    {
                        numeroLinea++;
                        linea = linea.Trim();
                        if (string.IsNullOrEmpty(linea)) continue;

                        char separador = linea.Contains(';') ? ';' : ',';
                        string[] partes = linea.Split(separador);

                        // Formato requerido: Id, Nombre, Celular, Residencia
                        if (partes.Length < 4)
                        {
                            omitidos++;
                            continue;
                        }

                        if (!int.TryParse(partes[0].Trim(), out int id))
                        {
                            if (numeroLinea == 1) continue; // Encabezado
                            omitidos++;
                            continue;
                        }

                        // Verificar si el ID ya existe en el arreglo
                        bool existe = false;
                        for (int i = 0; i < conteoClientes; i++)
                        {
                            if (clientes[i].Id == id)
                            {
                                existe = true;
                                break;
                            }
                        }

                        if (existe)
                        {
                            omitidos++;
                            continue;
                        }

                        string nombre = partes[1].Trim();
                        string celular = partes[2].Trim();
                        string residencia = partes[3].Trim();

                        // Redimensionamiento manual de arreglo nativo
                        if (conteoClientes >= clientes.Length)
                        {
                            Cliente[] nuevo = new Cliente[clientes.Length * 2];
                            Array.Copy(clientes, nuevo, clientes.Length);
                            clientes = nuevo;
                        }

                        clientes[conteoClientes++] = new Cliente(id, nombre, celular, residencia);
                        insertados++;
                    }
                }

                Console.WriteLine($"\n[Carga Clientes] Éxito: {insertados} | Omitidos/Duplicados: {omitidos}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error de lectura] {ex.Message}");
            }
        }

        // ---------------------------------------------------------
        // REGISTRO DE ACCIONES DE PRÉSTAMO
        // ---------------------------------------------------------
        private static readonly string RutaRegistro = "registro_prestamos.txt";
        public static void RegistrarAccion(string accion, Cliente cliente, Libro libro)
        {
            try
            {
                string fecha = DateTime.Now.ToString("dd/MM/yyyy");
                string linea = $"{fecha} {cliente.Nombre} (id: {cliente.Id}) {accion} el libro {libro.Titulo} (id: {libro.Id})";
                
                File.AppendAllText(RutaRegistro, linea + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Aviso] No se pudo escribir en la bitácora: {ex.Message}");
            }
        }
    }
}