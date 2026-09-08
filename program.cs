using System;
using System.Windows.Forms;
using EstructurasSuperPros;

class Program
{
    private static ArbolBPlus<int, Libro> _catalogo;
    private static MaxHeapLibros _maxHeapPrestamos;
    private static MinHeapLibros _minHeapCopias;

    // Arreglo nativo dinámico para clientes (sin colecciones nativas de .NET)
    private static Cliente[] _clientes;
    private static int _conteoClientes;
    [STAThread]

    static void Main()
    {
        Console.WriteLine("========================================");
        Console.WriteLine("¿En qué modo deseas iniciar el sistema?");
        Console.WriteLine("1. Modo Consola");
        Console.WriteLine("2. Modo Interfaz Gráfica (Solo como demostración, no funcional)");
        Console.WriteLine("========================================");
        Console.Write("Elige una opción: ");
        
        string modo = Console.ReadLine();

        if (modo == "2")
        {
            // Arranca el modo visual
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MenuVisual()); 
        }
        else
        {
            // Arranca el modo consola 
            IniciarConsolaOriginal(); 
        }
    }

    //Iniciar con consola
    private static void IniciarConsolaOriginal()
    {
        _catalogo = new ArbolBPlus<int, Libro>(libro => libro.Id, orden: 4);
        _maxHeapPrestamos = new MaxHeapLibros();
        _minHeapCopias = new MinHeapLibros();
        _clientes = new Cliente[10];
        _conteoClientes = 0;

        Console.WriteLine("Iniciando menú de consola...");
        bool salir = false;
        while (!salir)
        {
            Console.Clear();
            Console.WriteLine("\n==================================================");
            Console.WriteLine("     SISTEMA DE GESTION DE BIBLIOTECA CENTRAL    ");
            Console.WriteLine("==================================================");
            Console.WriteLine("1. Gestión de Clientes");
            Console.WriteLine("2. Gestión de Catálogo de Libros");
            Console.WriteLine("3. Gestión de Préstamos");
            Console.WriteLine("0. Salir");
            Console.WriteLine("==================================================");
            Console.Write("Seleccione un módulo: ");

            string opcion = Console.ReadLine();

            switch (opcion)
            {
                case "1":
                    MenuGestionClientes();
                    break;
                case "2":
                    MenuGestionCatalogo();
                    break;
                case "3":
                    MenuGestionPrestamos();
                    break;
                case "0":
                    salir = true;
                    Console.WriteLine("Cerrando el sistema...");
                    break;
                default:
                    Console.WriteLine("[Aviso] Opción no válida.");
                    break;
            }
        }
    }
    
    // =========================================================================
    // 1. SUBMENÚ: GESTIÓN CLIENTES
    // =========================================================================
    private static void MenuGestionClientes()
    {
        bool volver = false;
        while (!volver)
        {   

            Console.WriteLine("\n----------------------------------------");
            Console.WriteLine("           GESTION DE CLIENTES          ");
            Console.WriteLine("----------------------------------------");
            Console.WriteLine("1. Ver clientes");
            Console.WriteLine("2. Agregar cliente");
            Console.WriteLine("3. Eliminar cliente");
            Console.WriteLine("4. Ver cliente con más préstamos");
            Console.WriteLine("5. Cargar registro de clientes desde archivo (.csv / .txt)");
            Console.WriteLine("6. Exportar registro de clientes a archivo (.csv)");
            Console.WriteLine("0. Volver al menú principal");
            Console.WriteLine("----------------------------------------");
            Console.Write("Seleccione una opción: ");

            switch (Console.ReadLine())
            {
                case "1":
                    VerClientes();
                    break;
                case "2":
                    AgregarCliente();
                    break;
                case "3":
                    EliminarCliente();
                    break;
                case "4":
                    VerClienteConMasPrestamos();
                    break;
                case "5":
                    CargarClientesDesdeArchivo();
                    break;
                case "6":
                    ExportarClientesAArchivo();
                    break;
                case "0":
                    volver = true;
                    break;
                default:
                    Console.WriteLine("[Aviso] Opción inválida.");
                    break;
            }
        }
    }

    private static void VerClientes()
    {
        Console.WriteLine("\n--- Listado de Clientes Registrados ---");
        if (_conteoClientes == 0)
        {
            Console.WriteLine("No hay clientes registrados en el sistema.");
            return;
        }

        for (int i = 0; i < _conteoClientes; i++)
        {
            Console.WriteLine(_clientes[i]);
            int[] pendientes = _clientes[i].ObtenerPrestamosPendientes();
            if (pendientes.Length > 0)
            {
                Console.Write("  -> IDs de libros activos: ");
                for (int j = 0; j < pendientes.Length; j++)
                    Console.Write($"[{pendientes[j]}] ");
                Console.WriteLine();
            }
        }
    }

    private static void AgregarCliente()
    {
        Console.WriteLine("\n--- Registrar Nuevo Cliente ---");
        Console.Write("ID del cliente: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("[Error] El ID debe ser numérico.");
            return;
        }

        if (BuscarCliente(id) != null)
        {
            Console.WriteLine($"[Error] Ya existe un cliente con el ID {id}.");
            return;
        }

        Console.Write("Nombre completo: ");
        string nombre = Console.ReadLine()?.Trim();
        Console.Write("Celular: ");
        string celular = Console.ReadLine()?.Trim();
        Console.Write("Residencia / Zona: ");
        string residencia = Console.ReadLine()?.Trim();

        Cliente nuevo = new Cliente(id, nombre, celular, residencia);
        RegistrarClienteDirecto(nuevo);
        Console.WriteLine($"[Éxito] Cliente '{nombre}' registrado correctamente.");
    }

    private static void EliminarCliente()
    {
        Console.WriteLine("\n--- Eliminar Cliente ---");
        Console.Write("Ingrese el ID del cliente a dar de baja: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("[Error] ID inválido.");
            return;
        }

        int pos = -1;
        for (int i = 0; i < _conteoClientes; i++)
        {
            if (_clientes[i].Id == id)
            {
                pos = i;
                break;
            }
        }

        if (pos == -1)
        {
            Console.WriteLine($"[Error] No se encontró ningún cliente con el ID {id}.");
            return;
        }

        // No se puede borrar cliente si tiene libros prestados
        if (_clientes[pos].ObtenerPrestamosPendientes().Length > 0)
        {
            Console.WriteLine("[Denegado] El cliente tiene libros pendientes de devolución. No se puede eliminar.");
            return;
        }

        // Compactación manual de arreglo nativo
        for (int i = pos; i < _conteoClientes - 1; i++)
            _clientes[i] = _clientes[i + 1];

        _clientes[--_conteoClientes] = null;
        Console.WriteLine($"[Éxito] El cliente con ID {id} fue eliminado del sistema.");
    }

    private static void VerClienteConMasPrestamos()
    {
        Console.WriteLine("\n--- Cliente con Mayor Actividad de Préstamos ---");
        if (_conteoClientes == 0)
        {
            Console.WriteLine("No hay clientes registrados.");
            return;
        }

        Cliente topCliente = null;
        int maxPrestamos = -1;

        for (int i = 0; i < _conteoClientes; i++)
        {
            // Suma de pendientes + devueltos para el histórico acumulado
            int totalHistorial = _clientes[i].ObtenerPrestamosPendientes().Length + _clientes[i].ObtenerPrestamosDevueltos().Length;
            if (totalHistorial > maxPrestamos)
            {
                maxPrestamos = totalHistorial;
                topCliente = _clientes[i];
            }
        }

        if (topCliente != null)
        {
            Console.WriteLine($"Cliente: {topCliente.Nombre} (ID: {topCliente.Id})");
            Console.WriteLine($"Total de préstamos procesados: {maxPrestamos} (Activos: {topCliente.ObtenerPrestamosPendientes().Length})");
        }
    }

    private static void ExportarClientesAArchivo()
    {
        Console.Write("\nIngrese nombre/ruta del archivo destino [Enter para 'clientes.csv']: ");
        string ruta = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(ruta)) ruta = "clientes.csv";

        GestorArchivos.ExportarClientes(ruta, _clientes, _conteoClientes);
    }

        private static void CargarClientesDesdeArchivo()
    {
        Console.Write("\nIngrese la ruta del archivo (.csv o .txt) [Enter para 'clientes.csv']: ");
        string ruta = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(ruta)) ruta = "clientes.csv";

        GestorArchivos.CargarClientes(ruta, ref _clientes, ref _conteoClientes);
    }

    // =========================================================================
    // 2. SUBMENÚ: GESTIÓN CATÁLOGO
    // =========================================================================
    private static void MenuGestionCatalogo()
    {
        bool volver = false;
        while (!volver)
        {
            Console.WriteLine("\n----------------------------------------");
            Console.WriteLine("           GESTION DE CATALOGO          ");
            Console.WriteLine("----------------------------------------");
            Console.WriteLine("1. Ver libros (Catálogo completo)");
            Console.WriteLine("2. Buscar libros por género");
            Console.WriteLine("3. Buscar libros por ID (Árbol B+)");
            Console.WriteLine("4. Ver libro más prestado (Max Heap)");
            Console.WriteLine("5. Ver libro con menos copias disponibles (Min Heap)");
            Console.WriteLine("6. Agregar libro");
            Console.WriteLine("7. Eliminar libro");
            Console.WriteLine("8. Cargar catálogo de libros desde archivo (.csv / .txt)");
            Console.WriteLine("9. Exportar catálogo de libros a archivo (.csv)");
            Console.WriteLine("0. Volver al menú principal");
            Console.WriteLine("----------------------------------------");
            Console.Write("Seleccione una opción: ");

            switch (Console.ReadLine())
            {
                case "1":
                    ListarLibros();
                    break;
                case "2":
                    BuscarLibrosPorGenero();
                    break;
                case "3":
                    BuscarLibroPorId();
                    break;
                case "4":
                    VerLibroMasPrestado();
                    break;
                case "5":
                    VerLibroMenosCopias();
                    break;
                case "6":
                    RegistrarLibroManual();
                    break;
                case "7":
                    EliminarLibroCatalogo();
                    break;
                case "8":
                    CargarLibrosDesdeArchivo();
                    break;
                case "9":
                    ExportarLibrosAArchivo();
                    break; 
                case "0":
                    volver = true;
                    break;
                default:
                    Console.WriteLine("[Aviso] Opción inválida.");
                    break;
            }
        }
    }

    private static void ListarLibros()
    {
        Libro[] libros = _catalogo.Recorrer();
        if (libros.Length == 0)
        {
            Console.WriteLine("\nEl catálogo está vacío.");
            return;
        }

        Console.WriteLine($"\n--- Catálogo de Libros ({libros.Length} títulos) ---");
        for (int i = 0; i < libros.Length; i++)
        {
            Console.WriteLine(libros[i]);
        }
    }

    private static void BuscarLibrosPorGenero()
    {
        Console.Write("\nIngrese el género literario a buscar: ");
        string genero = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(genero))
        {
            Console.WriteLine("[Error] Debe ingresar un género válido.");
            return;
        }

        Libro[] libros = _catalogo.Recorrer();
        int encontrados = 0;

        Console.WriteLine($"\n--- Libros bajo el género '{genero}' ---");
        for (int i = 0; i < libros.Length; i++)
        {
            if (libros[i].Genero.Equals(genero, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine(libros[i]);
                encontrados++;
            }
        }

        if (encontrados == 0)
            Console.WriteLine($"No se hallaron libros asociados al género '{genero}'.");
        else
            Console.WriteLine($"Total encontrados: {encontrados}");
    }

    private static void BuscarLibroPorId()
    {
        Console.Write("\nIngrese el ID del libro a buscar: ");
        if (int.TryParse(Console.ReadLine(), out int id))
        {
            Libro libro = _catalogo.Buscar(id);
            if (libro != null)
            {
                Console.WriteLine("\n[Libro Localizado en Árbol B+]");
                libro.Informacion();
            }
            else
            {
                Console.WriteLine($"[Aviso] No existe un libro registrado con el ID {id}.");
            }
        }
        else
        {
            Console.WriteLine("[Error] ID inválido.");
        }
    }

    private static void VerLibroMasPrestado()
    {
        Console.WriteLine("\n--- Título Más Prestado (Consulta O(1) en Max Heap) ---");
        Libro libro = _maxHeapPrestamos.ObtenerMaximo();
        if (libro != null)
            Console.WriteLine($"'{libro.Titulo}' (ID: {libro.Id}) | Préstamos totales: {libro.PrestamosTotales}");
        else
            Console.WriteLine("El montículo de préstamos está vacío.");
    }

    private static void VerLibroMenosCopias()
    {
        Console.WriteLine("\n--- Título con Menor Stock (Consulta O(1) en Min Heap) ---");
        Libro libro = _minHeapCopias.ObtenerMinimo();
        if (libro != null)
            Console.WriteLine($"'{libro.Titulo}' (ID: {libro.Id}) | Copias disponibles: {libro.CopiasDisponibles} de {libro.CopiasTotales}");
        else
            Console.WriteLine("El montículo de copias está vacío.");
    }

    private static void RegistrarLibroManual()
    {
        Console.WriteLine("\n--- Agregar Libro al Catálogo ---");
        Console.Write("ID único: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("[Error] ID inválido.");
            return;
        }

        if (_catalogo.Buscar(id) != null)
        {
            Console.WriteLine($"[Error] Ya existe un libro con el ID {id}.");
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
            Console.WriteLine($"[Éxito] Libro '{titulo}' insertado en Árbol B+ y montículos.");
        }
    }

    private static void EliminarLibroCatalogo()
    {
        Console.WriteLine("\n--- Eliminar Libro del Catálogo ---");
        Console.Write("Ingrese el ID del libro a eliminar: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("[Error] ID inválido.");
            return;
        }

        Libro libro = _catalogo.Buscar(id);
        if (libro == null)
        {
            Console.WriteLine($"[Error] El libro con ID {id} no existe en el catálogo.");
            return;
        }

        // Validación: No dar de baja si hay copias prestadas actualmente
        if (libro.CopiasDisponibles < libro.CopiasTotales)
        {
            Console.WriteLine("[Denegado] Hay ejemplares prestados actualmente. Deben ser devueltos antes de eliminar el título.");
            return;
        }

        // Eliminación en cascada en las 3 estructuras principales
        bool borradoBPlus = _catalogo.Eliminar(id);
        bool borradoMax = _maxHeapPrestamos.EliminarPorId(id);
        bool borradoMin = _minHeapCopias.EliminarPorId(id);

        if (borradoBPlus)
        {
            Console.WriteLine($"[Éxito] El libro '{libro.Titulo}' fue eliminado del Árbol B+ y desindexado de ambos montículos.");
        }
        else
        {
            Console.WriteLine("[Error] No se pudo concretar la eliminación en el árbol.");
        }
    }

    private static void CargarLibrosDesdeArchivo()
    {
        Console.Write("\nIngrese la ruta del archivo (.csv o .txt) [Enter para 'libros.csv']: ");
        string ruta = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(ruta)) ruta = "libros.csv";

        GestorArchivos.CargarLibros(ruta, _catalogo, _maxHeapPrestamos, _minHeapCopias);
    }
    private static void ExportarLibrosAArchivo()
    {
        Console.Write("\nIngrese nombre/ruta del archivo destino [Enter para 'libros.csv']: ");
        string ruta = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(ruta)) ruta = "libros.csv";

        GestorArchivos.ExportarLibros(ruta, _catalogo);
    }

    // =========================================================================
    // 3. SUBMENÚ: GESTIÓN PRÉSTAMOS
    // =========================================================================
    private static void MenuGestionPrestamos()
    {
        bool volver = false;
        while (!volver)
        {
            Console.WriteLine("\n----------------------------------------");
            Console.WriteLine("           GESTION DE PRESTAMOS         ");
            Console.WriteLine("----------------------------------------");
            Console.WriteLine("1. Prestar libro");
            Console.WriteLine("2. Regresar libro (Devolución)");
            Console.WriteLine("3. Ver préstamos activos");
            Console.WriteLine("0. Volver al menú principal");
            Console.WriteLine("----------------------------------------");
            Console.Write("Seleccione una opción: ");

            switch (Console.ReadLine())
            {
                case "1":
                    GestionarPrestamo();
                    break;
                case "2":
                    GestionarDevolucion();
                    break;
                case "3":
                    VerPrestamosActivos();
                    break;
                case "0":
                    volver = true;
                    break;
                default:
                    Console.WriteLine("[Aviso] Opción inválida.");
                    break;
            }
        }
    }

    private static void GestionarPrestamo()
    {
        Console.WriteLine("\n--- Registrar Préstamo ---");
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
            Console.WriteLine("[Error] Libro no encontrado en el catálogo.");
            return;
        }

        if (libro.CopiasDisponibles <= 0)
        {
            Console.WriteLine("[Denegado] No hay copias disponibles.");
            return;
        }

        if (!cliente.PrestarLibro(libro.Id))
        {
            Console.WriteLine("[Denegado] El cliente ya tiene este libro en calidad de préstamo.");
            return;
        }

        libro.Prestar();
        GestorArchivos.RegistrarAccion("presto", cliente, libro); //Guargar registro de préstamo en archivo

        // Rebalanceo Bottom-up de los montículos
        _maxHeapPrestamos.Reconstruir();
        _minHeapCopias.Reconstruir();

        Console.WriteLine($"[Préstamo Exitoso] Libro '{libro.Titulo}' asignado a {cliente.Nombre}.");
        Console.WriteLine($"Copias disponibles restantes: {libro.CopiasDisponibles}");
    }

    private static void GestionarDevolucion()
    {
        Console.WriteLine("\n--- Registrar Devolución de Libro ---");
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
            Console.WriteLine("[Error] El libro no existe en el catálogo.");
            return;
        }

        if (!cliente.DevolverLibro(libro.Id))
        {
            Console.WriteLine("[Error] El cliente no posee este libro pendiente de devolución.");
            return;
        }

        libro.Devolver();
        GestorArchivos.RegistrarAccion("regreso", cliente, libro); //Guargar registro de devolución en archivo

        // Rebalanceo Bottom-up de los montículos
        _maxHeapPrestamos.Reconstruir();
        _minHeapCopias.Reconstruir();

        Console.WriteLine($"[Devolución Exitosa] Se reingresó una copia de '{libro.Titulo}'.");
        Console.WriteLine($"Copias disponibles actuales: {libro.CopiasDisponibles}");
    }

    private static void VerPrestamosActivos()
    {
        Console.WriteLine("\n--- Relación General de Préstamos Activos ---");
        int totalPrestamos = 0;

        for (int i = 0; i < _conteoClientes; i++)
        {
            int[] pendientes = _clientes[i].ObtenerPrestamosPendientes();
            if (pendientes.Length > 0)
            {
                Console.WriteLine($"\nCliente: {_clientes[i].Nombre} (ID: {_clientes[i].Id}) | Contacto: {_clientes[i].Celular}");
                for (int j = 0; j < pendientes.Length; j++)
                {
                    Libro lib = _catalogo.Buscar(pendientes[j]);
                    string titulo = lib != null ? lib.Titulo : "Título no encontrado";
                    Console.WriteLine($"  - [Libro ID: {pendientes[j]}] {titulo}");
                    totalPrestamos++;
                }
            }
        }

        if (totalPrestamos == 0)
            Console.WriteLine("No existen préstamos activos en este momento.");
        else
            Console.WriteLine($"\nTotal de libros en préstamo actualmente: {totalPrestamos}");
    }

    // =========================================================================
    // UTILIDADES GENERALES
    // =========================================================================
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

}