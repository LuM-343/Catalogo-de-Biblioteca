using System;
using System.Drawing;
using System.Windows.Forms;
using EstructurasSuperPros;
// =====================================================================
// AVISO MEGA IMPORTANTE: LA PARTE DEL MENU VISUAL SE DESARROLLÓ CON IA
// NO SE ENCUENTRA PARA NADA EN UNA VERSIÓN FUNCIONAL, SOLO ESTA COMO
// REFERENCIA DE CÓMO PODRÍA SER UNA INTERFAZ GRÁFICA PARA EL PROYECTO.
// =====================================================================
namespace EstructurasSuperPros
{
    public class MenuVisual : Form
    {
        // Estructuras de datos
        private ArbolBPlus<int, Libro> _catalogo;
        private MaxHeapLibros _maxHeapPrestamos;
        private MinHeapLibros _minHeapCopias;
        private Cliente[] _clientes;
        private int _conteoClientes;

        // Controles de Interfaz
        private TabControl tabControl;
        
        // Controles Clientes
        private ListBox lstClientes;
        private TextBox txtCliId, txtCliNombre, txtCliCel, txtCliResidencia;

        // Controles Libros
        private ListBox lstLibros;
        private TextBox txtLibId, txtLibTitulo, txtLibAutor, txtLibAnio, txtLibGenero, txtLibCopias;

        // Controles Préstamos
        private ListBox lstPrestamos;
        private TextBox txtPrestCliId, txtPrestLibId;

        public MenuVisual()
        {
            // Inicialización de estructuras
            _catalogo = new ArbolBPlus<int, Libro>(libro => libro.Id, orden: 4);
            _maxHeapPrestamos = new MaxHeapLibros();
            _minHeapCopias = new MinHeapLibros();
            _clientes = new Cliente[10];
            _conteoClientes = 0;

            ConfigurarVentana();
            InicializarComponentes();
            ActualizarListas();
        }

        private void ConfigurarVentana()
        {
            this.Text = "Sistema de Gestión de Biblioteca - Modo Visual";
            this.Size = new Size(850, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void InicializarComponentes()
        {
            tabControl = new TabControl { Dock = DockStyle.Fill };

            TabPage tabClientes = new TabPage("Gestión de Clientes");
            TabPage tabLibros = new TabPage("Gestión de Catálogo");
            TabPage tabPrestamos = new TabPage("Gestión de Préstamos");

            ConfigurarPestanaClientes(tabClientes);
            ConfigurarPestanaLibros(tabLibros);
            ConfigurarPestanaPrestamos(tabPrestamos);

            tabControl.TabPages.Add(tabClientes);
            tabControl.TabPages.Add(tabLibros);
            tabControl.TabPages.Add(tabPrestamos);

            this.Controls.Add(tabControl);
        }

        // =========================================================
        // PESTAÑA: CLIENTES
        // =========================================================
        private void ConfigurarPestanaClientes(TabPage tab)
        {
            int y = 20;
            CrearCampoTexto(tab, "ID Cliente:", ref txtCliId, 20, ref y);
            CrearCampoTexto(tab, "Nombre:", ref txtCliNombre, 20, ref y);
            CrearCampoTexto(tab, "Celular:", ref txtCliCel, 20, ref y);
            CrearCampoTexto(tab, "Residencia:", ref txtCliResidencia, 20, ref y);

            Button btnAgregar = new Button { Text = "Agregar Cliente", Location = new Point(20, y += 30), Width = 150 };
            btnAgregar.Click += BtnAgregarCliente_Click;
            tab.Controls.Add(btnAgregar);

            Button btnEliminar = new Button { Text = "Eliminar (por ID)", Location = new Point(20, y += 35), Width = 150 };
            btnEliminar.Click += BtnEliminarCliente_Click;
            tab.Controls.Add(btnEliminar);

            Button btnTopCliente = new Button { Text = "Cliente + Préstamos", Location = new Point(20, y += 35), Width = 150 };
            btnTopCliente.Click += BtnTopCliente_Click;
            tab.Controls.Add(btnTopCliente);

            Button btnCargar = new Button { Text = "Cargar CSV", Location = new Point(20, y += 45), Width = 70 };
            btnCargar.Click += (s, e) => { GestorArchivos.CargarClientes("clientes.csv", ref _clientes, ref _conteoClientes); ActualizarListas(); };
            tab.Controls.Add(btnCargar);

            Button btnExportar = new Button { Text = "Exportar", Location = new Point(100, y), Width = 70 };
            btnExportar.Click += (s, e) => { GestorArchivos.ExportarClientes("clientes.csv", _clientes, _conteoClientes); MessageBox.Show("Exportado a clientes.csv"); };
            tab.Controls.Add(btnExportar);

            lstClientes = new ListBox { Location = new Point(200, 20), Size = new Size(600, 380) };
            tab.Controls.Add(lstClientes);
        }

        private void BtnAgregarCliente_Click(object sender, EventArgs e)
        {
            if (int.TryParse(txtCliId.Text, out int id) && !string.IsNullOrWhiteSpace(txtCliNombre.Text))
            {
                if (BuscarCliente(id) != null) { MessageBox.Show("El ID ya existe."); return; }
                RegistrarClienteDirecto(new Cliente(id, txtCliNombre.Text, txtCliCel.Text, txtCliResidencia.Text));
                ActualizarListas();
                LimpiarTextos(txtCliId, txtCliNombre, txtCliCel, txtCliResidencia);
            }
            else MessageBox.Show("Revise los datos ingresados.");
        }

        private void BtnEliminarCliente_Click(object sender, EventArgs e)
        {
            if (int.TryParse(txtCliId.Text, out int id))
            {
                int pos = -1;
                for (int i = 0; i < _conteoClientes; i++) if (_clientes[i].Id == id) { pos = i; break; }
                
                if (pos == -1) { MessageBox.Show("Cliente no encontrado."); return; }
                if (_clientes[pos].ObtenerPrestamosPendientes().Length > 0) { MessageBox.Show("El cliente tiene préstamos activos."); return; }

                for (int i = pos; i < _conteoClientes - 1; i++) _clientes[i] = _clientes[i + 1];
                _clientes[--_conteoClientes] = null;
                ActualizarListas();
            }
        }

        private void BtnTopCliente_Click(object sender, EventArgs e)
        {
            if (_conteoClientes == 0) return;
            Cliente top = null;
            int max = -1;
            for (int i = 0; i < _conteoClientes; i++)
            {
                int total = _clientes[i].ObtenerPrestamosPendientes().Length + _clientes[i].ObtenerPrestamosDevueltos().Length;
                if (total > max) { max = total; top = _clientes[i]; }
            }
            MessageBox.Show($"Mejor Cliente: {top.Nombre}\nPréstamos Históricos: {max}", "Top Cliente");
        }

        // =========================================================
        // PESTAÑA: CATÁLOGO DE LIBROS
        // =========================================================
        private void ConfigurarPestanaLibros(TabPage tab)
        {
            int y = 20;
            CrearCampoTexto(tab, "ID Libro:", ref txtLibId, 20, ref y);
            CrearCampoTexto(tab, "Título:", ref txtLibTitulo, 20, ref y);
            CrearCampoTexto(tab, "Autor:", ref txtLibAutor, 20, ref y);
            CrearCampoTexto(tab, "Año:", ref txtLibAnio, 20, ref y);
            CrearCampoTexto(tab, "Género:", ref txtLibGenero, 20, ref y);
            CrearCampoTexto(tab, "Copias Totales:", ref txtLibCopias, 20, ref y);

            Button btnAgregar = new Button { Text = "Agregar Libro", Location = new Point(20, y += 30), Width = 150 };
            btnAgregar.Click += BtnAgregarLibro_Click;
            tab.Controls.Add(btnAgregar);

            Button btnEliminar = new Button { Text = "Eliminar (por ID)", Location = new Point(20, y += 35), Width = 150 };
            btnEliminar.Click += BtnEliminarLibro_Click;
            tab.Controls.Add(btnEliminar);

            Button btnEstadisticas = new Button { Text = "Ver Estadísticas Heap", Location = new Point(20, y += 35), Width = 150 };
            btnEstadisticas.Click += BtnEstadisticas_Click;
            tab.Controls.Add(btnEstadisticas);

            Button btnCargar = new Button { Text = "Cargar CSV", Location = new Point(20, y += 45), Width = 70 };
            btnCargar.Click += (s, e) => { GestorArchivos.CargarLibros("libros.csv", _catalogo, _maxHeapPrestamos, _minHeapCopias); ActualizarListas(); };
            tab.Controls.Add(btnCargar);

            Button btnExportar = new Button { Text = "Exportar", Location = new Point(100, y), Width = 70 };
            btnExportar.Click += (s, e) => { GestorArchivos.ExportarLibros("libros.csv", _catalogo); MessageBox.Show("Exportado a libros.csv"); };
            tab.Controls.Add(btnExportar);

            lstLibros = new ListBox { Location = new Point(200, 20), Size = new Size(600, 380) };
            tab.Controls.Add(lstLibros);
        }

        private void BtnAgregarLibro_Click(object sender, EventArgs e)
        {
            if (int.TryParse(txtLibId.Text, out int id) && int.TryParse(txtLibAnio.Text, out int anio) && int.TryParse(txtLibCopias.Text, out int copias))
            {
                if (_catalogo.Buscar(id) != null) { MessageBox.Show("El ID del libro ya existe."); return; }
                Libro nuevo = new Libro(id, txtLibTitulo.Text, txtLibAutor.Text, anio, txtLibGenero.Text, copias, copias, 0);
                if (_catalogo.Insertar(nuevo))
                {
                    _maxHeapPrestamos.Insertar(nuevo);
                    _minHeapCopias.Insertar(nuevo);
                    ActualizarListas();
                    LimpiarTextos(txtLibId, txtLibTitulo, txtLibAutor, txtLibAnio, txtLibGenero, txtLibCopias);
                }
            }
            else MessageBox.Show("Revise los datos ingresados.");
        }

        private void BtnEliminarLibro_Click(object sender, EventArgs e)
        {
            if (int.TryParse(txtLibId.Text, out int id))
            {
                Libro lib = _catalogo.Buscar(id);
                if (lib == null) { MessageBox.Show("Libro no encontrado."); return; }
                if (lib.CopiasDisponibles < lib.CopiasTotales) { MessageBox.Show("Hay copias prestadas. No se puede eliminar."); return; }

                _catalogo.Eliminar(id);
                _maxHeapPrestamos.EliminarPorId(id);
                _minHeapCopias.EliminarPorId(id);
                ActualizarListas();
            }
        }

        private void BtnEstadisticas_Click(object sender, EventArgs e)
        {
            Libro max = _maxHeapPrestamos.ObtenerMaximo();
            Libro min = _minHeapCopias.ObtenerMinimo();
            string msg = $"Más prestado: {(max != null ? max.Titulo : "N/A")}\nMenos copias disponibles: {(min != null ? min.Titulo : "N/A")}";
            MessageBox.Show(msg, "Estadísticas Heaps");
        }

        // =========================================================
        // PESTAÑA: PRÉSTAMOS
        // =========================================================
        private void ConfigurarPestanaPrestamos(TabPage tab)
        {
            int y = 20;
            CrearCampoTexto(tab, "ID Cliente:", ref txtPrestCliId, 20, ref y);
            CrearCampoTexto(tab, "ID Libro:", ref txtPrestLibId, 20, ref y);

            Button btnPrestar = new Button { Text = "Prestar Libro", Location = new Point(20, y += 30), Width = 150 };
            btnPrestar.Click += BtnPrestar_Click;
            tab.Controls.Add(btnPrestar);

            Button btnDevolver = new Button { Text = "Devolver Libro", Location = new Point(20, y += 35), Width = 150 };
            btnDevolver.Click += BtnDevolver_Click;
            tab.Controls.Add(btnDevolver);

            lstPrestamos = new ListBox { Location = new Point(200, 20), Size = new Size(600, 380) };
            tab.Controls.Add(lstPrestamos);
        }

        private void BtnPrestar_Click(object sender, EventArgs e)
        {
            if (int.TryParse(txtPrestCliId.Text, out int cliId) && int.TryParse(txtPrestLibId.Text, out int libId))
            {
                Cliente cli = BuscarCliente(cliId);
                Libro lib = _catalogo.Buscar(libId);
                if (cli == null || lib == null) { MessageBox.Show("Cliente o Libro no encontrados."); return; }
                
                if (lib.CopiasDisponibles <= 0) { MessageBox.Show("No hay copias disponibles."); return; }
                if (!cli.PrestarLibro(libId)) { MessageBox.Show("El cliente ya tiene este libro."); return; }

                lib.Prestar();
                GestorArchivos.RegistrarAccion("presto", cli, lib);
                _maxHeapPrestamos.Reconstruir();
                _minHeapCopias.Reconstruir();
                ActualizarListas();
            }
        }

        private void BtnDevolver_Click(object sender, EventArgs e)
        {
            if (int.TryParse(txtPrestCliId.Text, out int cliId) && int.TryParse(txtPrestLibId.Text, out int libId))
            {
                Cliente cli = BuscarCliente(cliId);
                Libro lib = _catalogo.Buscar(libId);
                if (cli == null || lib == null) { MessageBox.Show("Cliente o Libro no encontrados."); return; }

                if (!cli.DevolverLibro(libId)) { MessageBox.Show("El cliente no tiene prestado este libro."); return; }
                
                lib.Devolver();
                GestorArchivos.RegistrarAccion("regreso", cli, lib);
                _maxHeapPrestamos.Reconstruir();
                _minHeapCopias.Reconstruir();
                ActualizarListas();
            }
        }

        // =========================================================
        // UTILIDADES Y ACTUALIZACIÓN
        // =========================================================
        private void ActualizarListas()
        {
            // Actualizar Clientes
            lstClientes.Items.Clear();
            for (int i = 0; i < _conteoClientes; i++) lstClientes.Items.Add(_clientes[i].ToString());

            // Actualizar Libros
            lstLibros.Items.Clear();
            Libro[] libros = _catalogo.Recorrer();
            foreach (var lib in libros) lstLibros.Items.Add(lib.ToString());

            // Actualizar Préstamos Activos
            lstPrestamos.Items.Clear();
            for (int i = 0; i < _conteoClientes; i++)
            {
                int[] pendientes = _clientes[i].ObtenerPrestamosPendientes();
                foreach (int pId in pendientes)
                {
                    Libro lib = _catalogo.Buscar(pId);
                    lstPrestamos.Items.Add($"[{_clientes[i].Nombre}] tiene prestado -> {(lib != null ? lib.Titulo : "ID: " + pId)}");
                }
            }
        }

        private void CrearCampoTexto(TabPage tab, string textoLabel, ref TextBox txtBox, int x, ref int y)
        {
            Label lbl = new Label { Text = textoLabel, Location = new Point(x, y), Width = 150 };
            tab.Controls.Add(lbl);
            y += 20;
            txtBox = new TextBox { Location = new Point(x, y), Width = 150 };
            tab.Controls.Add(txtBox);
            y += 25;
        }

        private void LimpiarTextos(params TextBox[] textboxes)
        {
            foreach (var tb in textboxes) tb.Clear();
        }

        private Cliente BuscarCliente(int id)
        {
            for (int i = 0; i < _conteoClientes; i++)
                if (_clientes[i].Id == id) return _clientes[i];
            return null;
        }

        private void RegistrarClienteDirecto(Cliente cliente)
        {
            if (_conteoClientes >= _clientes.Length)
            {
                Cliente[] nuevo = new Cliente[_clientes.Length * 2];
                Array.Copy(_clientes, nuevo, _clientes.Length);
                _clientes = nuevo;
            }
            _clientes[_conteoClientes++] = cliente;
        }
    }
}