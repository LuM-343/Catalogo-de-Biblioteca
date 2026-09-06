using System;
using EstructurasSuperPros;
class Program
{
    private static ArbolBPlus<int, Libro> _catalogo;
    private static MaxHeapLibros _maxHeapPrestamos;
    private static MinHeapLibros _minHeapCopias;

    // Arreglo nativo para almacenar clientes (sin usar List<T>)
    private static Cliente[] _clientes;
    private static int _conteoClientes;

    static void Main()
    {
        _catalogo = new ArbolBPlus<int, Libro>(libro => libro.Id, orden: 4);
        _maxHeapPrestamos = new MaxHeapLibros();
        _minHeapCopias = new MinHeapLibros();

        _clientes = new Cliente[10];
        _conteoClientes = 0;

        // Clientes iniciales de prueba
        RegistrarClienteDirecto(new Cliente(1, "Carlos Lopez", "5551-2345", "Zona 10"));
        RegistrarClienteDirecto(new Cliente(2, "Maria Morales", "5552-6789", "Zona 16"));

        bool salir = false;
        while (!salir)
        {
            Console.Clear();
            Console.WriteLine("==================================================");
            Console.WriteLine("   SISTEMA DE GESTION DE CATALOGO DE BIBLIOTECA   ");
            Console.WriteLine("==================================================");
            Console.WriteLine("1. Cargar libros desde archivo (.csv / .txt)");
            Console.WriteLine("2. Registrar nuevo libro manualmente");
            Console.WriteLine("3. Buscar libro por ID (Árbol B+)");
            Console.WriteLine("4. Listar catálogo ordenado por Título (Quicksort)");
            Console.WriteLine("5. Listar catálogo en orden de ID (Recorrido B+)");
            Console.WriteLine("6. Registrar Préstamo de Libro");
            Console.WriteLine("7. Registrar Devolución de Libro");
            Console.WriteLine("8. Reportes: Libro más prestado y con menor stock");
            Console.WriteLine("9. Ver Clientes y préstamos activos");
            Console.WriteLine("0. Salir");
            Console.WriteLine("==================================================");
            Console.Write("Seleccione una opción: ");

            string opcion = Console.ReadLine();
            Console.Clear();

            switch (opcion)
            {
                case "1":
                    CargarDesdeArchivo();
                    break;
                case "2":
                    RegistrarLibroManual();
                    break;
                case "3":
                    BuscarLibroPorId();
                    break;
                case "4":
                    ListarPorTitulo();
                    break;
                case "5":
                    ListarPorId();
                    break;
                case "6":
                    GestionarPrestamo();
                    break;
                case "7":
                    GestionarDevolucion();
                    break;
                case "8":
                    MostrarEstadisticas();
                    break;
                case "9":
                    MostrarClientes();
                    break;
                case "0":
                    salir = true;
                    Console.WriteLine("Saliendo del sistema...");
                    break;
                default:
                    Console.WriteLine("Opción no válida. Intente nuevamente.");
                    break;
            }

        }
    }

    private static void RegistrarClienteDirecto(Cliente cliente)
    {
        if (_conteoClientes >= _clientes.Length)
        {
            Cliente[] nuevo = new Cliente[_clientes.Length * 2];
            Array.Copy(_clientes, nuevo, _clientes.Length);
            _clientes = nuevo;
        }
        _clientes[_conteoClientes++] = cliente;
    }

    private static Cliente BuscarCliente(int id)
    {
        for (int i = 0; i < _conteoClientes; i++)
        {
            if (_clientes[i].Id == id) return _clientes[i];
        }
        return null;
    }

    private static void CargarDesdeArchivo()
    {
        Console.Write("Ingrese la ruta del archivo (.csv o .txt) [Por defecto: libros.csv]: ");
        string ruta = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(ruta)) ruta = "libros.csv";

        GestorArchivos.CargarLibros(ruta, _catalogo, _maxHeapPrestamos, _minHeapCopias);
    }

    private static void RegistrarLibroManual()
    {
        Console.WriteLine("--- Registrar Nuevo Libro ---");
        Console.Write("ID (número entero único): ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("[Error] ID inválido.");
            return;
        }

        if (_catalogo.Buscar(id) != null)
        {
            Console.WriteLine($"[Error] Ya existe un libro registrado con el ID {id}.");
            return;
        }

        Console.Write("Título: ");
        string titulo = Console.ReadLine()?.Trim();
        Console.Write("Autor: ");
        string autor = Console.ReadLine()?.Trim();
        Console.Write("Año de Publicación: ");
        int.TryParse(Console.ReadLine(), out int anio);
        Console.Write("Género: ");
        string genero = Console.ReadLine()?.Trim();
        Console.Write("Copias Totales: ");
        int.TryParse(Console.ReadLine(), out int copias);

        Libro nuevo = new Libro(id, titulo, autor, anio, genero, copias, copias, 0);

        if (_catalogo.Insertar(nuevo))
        {
            _maxHeapPrestamos.Insertar(nuevo);
            _minHeapCopias.Insertar(nuevo);
            Console.WriteLine("[Éxito] Libro registrado e indexado correctamente en Árbol B+ y Heaps.");
        }
    }

    private static void BuscarLibroPorId()
    {
        Console.Write("Ingrese el ID del libro a consultar: ");
        if (int.TryParse(Console.ReadLine(), out int id))
        {
            Libro libro = _catalogo.Buscar(id);
            if (libro != null)
            {
                Console.WriteLine("\nLibro localizado (mediante búsqueda en Árbol B+):");
                libro.Informacion();
            }
            else
            {
                Console.WriteLine($"[Aviso] No existe un libro con el ID {id}.");
            }
        }
        else
        {
            Console.WriteLine("[Error] Formato de ID inválido.");
        }
    }

    private static void ListarPorId()
    {
        Libro[] libros = _catalogo.Recorrer();
        Console.WriteLine($"--- Catálogo de Libros ({libros.Length} títulos indexados por ID) ---");
        for (int i = 0; i < libros.Length; i++)
        {
            Console.WriteLine(libros[i]);
        }
    }

    private static void ListarPorTitulo()
    {
        Libro[] libros = _catalogo.Recorrer();
        if (libros.Length == 0)
        {
            Console.WriteLine("El catálogo está vacío.");
            return;
        }

        // Se clona el arreglo para ordenar sin alterar el orden de las hojas del Árbol B+
        Libro[] copia = new Libro[libros.Length];
        Array.Copy(libros, copia, libros.Length);

        QuicksortPorTitulo(copia, 0, copia.Length - 1);

        Console.WriteLine($"--- Catálogo Ordenado Alfabéticamente por Título ({copia.Length} títulos) ---");
        for (int i = 0; i < copia.Length; i++)
        {
            Console.WriteLine($"Título: {copia[i].Titulo,-30} | ID: {copia[i].Id,-5} | Autor: {copia[i].Autor,-20} | Stock: {copia[i].CopiasDisponibles}");
        }
    }

    private static void QuicksortPorTitulo(Libro[] arr, int bajo, int alto)
    {
        if (bajo < alto)
        {
            int p = ParticionarPorTitulo(arr, bajo, alto);
            QuicksortPorTitulo(arr, bajo, p - 1);
            QuicksortPorTitulo(arr, p + 1, alto);
        }
    }

    private static int ParticionarPorTitulo(Libro[] arr, int bajo, int alto)
    {
        string pivote = arr[alto].Titulo;
        int i = bajo - 1;

        for (int j = bajo; j < alto; j++)
        {
            if (string.Compare(arr[j].Titulo, pivote, StringComparison.OrdinalIgnoreCase) <= 0)
            {
                i++;
                Libro temp = arr[i];
                arr[i] = arr[j];
                arr[j] = temp;
            }
        }

        Libro temp2 = arr[i + 1];
        arr[i + 1] = arr[alto];
        arr[alto] = temp2;

        return i + 1;
    }

    private static void GestionarPrestamo()
    {
        Console.WriteLine("--- Registrar Préstamo de Libro ---");
        Console.Write("Ingrese el ID del cliente: ");
        if (!int.TryParse(Console.ReadLine(), out int clienteId)) return;

        Cliente cliente = BuscarCliente(clienteId);
        if (cliente == null)
        {
            Console.WriteLine("[Error] Cliente no encontrado.");
            return;
        }

        Console.Write("Ingrese el ID del libro a prestar: ");
        if (!int.TryParse(Console.ReadLine(), out int libroId)) return;

        Libro libro = _catalogo.Buscar(libroId);
        if (libro == null)
        {
            Console.WriteLine("[Error] El libro solicitado no existe en el catálogo.");
            return;
        }

        if (libro.CopiasDisponibles <= 0)
        {
            Console.WriteLine("[Denegado] No hay copias disponibles de este libro.");
            return;
        }

        if (!cliente.PrestarLibro(libro.Id))
        {
            Console.WriteLine("[Denegado] El cliente ya posee un ejemplar pendiente de este libro.");
            return;
        }

        libro.Prestar();

        // Sincronización obligatoria de los Heaps
        _maxHeapPrestamos.Reconstruir();
        _minHeapCopias.Reconstruir();

        Console.WriteLine($"[Préstamo Exitoso] El libro '{libro.Titulo}' ha sido asignado a {cliente.Nombre}.");
        Console.WriteLine($"Copias disponibles restantes: {libro.CopiasDisponibles}");
    }

    private static void GestionarDevolucion()
    {
        Console.WriteLine("--- Registrar Devolución de Libro ---");
        Console.Write("Ingrese el ID del cliente: ");
        if (!int.TryParse(Console.ReadLine(), out int clienteId)) return;

        Cliente cliente = BuscarCliente(clienteId);
        if (cliente == null)
        {
            Console.WriteLine("[Error] Cliente no encontrado.");
            return;
        }

        Console.Write("Ingrese el ID del libro a devolver: ");
        if (!int.TryParse(Console.ReadLine(), out int libroId)) return;

        Libro libro = _catalogo.Buscar(libroId);
        if (libro == null)
        {
            Console.WriteLine("[Error] El libro no existe en el sistema.");
            return;
        }

        if (!cliente.DevolverLibro(libro.Id))
        {
            Console.WriteLine("[Error] El cliente no tiene registrado este libro como pendiente de devolución.");
            return;
        }

        libro.Devolver();

        // Sincronización obligatoria de los Heaps
        _maxHeapPrestamos.Reconstruir();
        _minHeapCopias.Reconstruir();

        Console.WriteLine($"[Devolución Exitosa] Se reintegró una copia de '{libro.Titulo}'.");
        Console.WriteLine($"Copias disponibles actuales: {libro.CopiasDisponibles}");
    }

    private static void MostrarEstadisticas()
    {
        Console.WriteLine("==================================================");
        Console.WriteLine("            ESTADISTICAS DE LA BIBLIOTECA         ");
        Console.WriteLine("==================================================");

        Libro masPrestado = _maxHeapPrestamos.ObtenerMaximo();
        if (masPrestado != null)
        {
            Console.WriteLine($"[MAX HEAP] Título más prestado: '{masPrestado.Titulo}' con {masPrestado.PrestamosTotales} préstamos.");
        }
        else
        {
            Console.WriteLine("[MAX HEAP] Sin datos.");
        }

        Libro menosStock = _minHeapCopias.ObtenerMinimo();
        if (menosStock != null)
        {
            Console.WriteLine($"[MIN HEAP] Título con menor stock: '{menosStock.Titulo}' con {menosStock.CopiasDisponibles} copias disponibles.");
        }
        else
        {
            Console.WriteLine("[MIN HEAP] Sin datos.");
        }
        Console.WriteLine("==================================================");
    }

    private static void MostrarClientes()
    {
        Console.WriteLine("--- Registro de Clientes ---");
        for (int i = 0; i < _conteoClientes; i++)
        {
            Console.WriteLine(_clientes[i]);
            int[] pendientes = _clientes[i].ObtenerPrestamosPendientes();
            if (pendientes.Length > 0)
            {
                Console.Write("  -> IDs de libros en posesión: ");
                for (int j = 0; j < pendientes.Length; j++)
                {
                    Console.Write($"[{pendientes[j]}] ");
                }
                Console.WriteLine();
            }
        }
    }
}