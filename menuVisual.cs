using System;
using System.Drawing;
using System.Windows.Forms;
using EstructurasSuperPros; // Tu namespace unificado

namespace EstructurasSuperPros
{
    public class MenuVisual : Form
    {
        private ArbolBPlus<int, Libro> _catalogo;
        private MaxHeapLibros _maxHeapPrestamos;
        private MinHeapLibros _minHeapCopias;
        private Cliente[] _clientes;
        private int _conteoClientes;

        public MenuVisual()
        {
            // Inicializar las estructuras
            _catalogo = new ArbolBPlus<int, Libro>(libro => libro.Id, orden: 4);
            _maxHeapPrestamos = new MaxHeapLibros();
            _minHeapCopias = new MinHeapLibros();
            _clientes = new Cliente[10];
            _conteoClientes = 0;

            // Clientes de prueba
            GestorArchivos.CargarClientes("clientes.csv", ref _clientes, ref _conteoClientes);
            GestorArchivos.CargarLibros("libros.csv", _catalogo, _maxHeapPrestamos, _minHeapCopias);

            ConfigurarInterfaz();
        }

        private void ConfigurarInterfaz()
        {
            this.Text = "Sistema de Gestión de Biblioteca - Modo GUI (Bonus)";
            this.Size = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Contenedor de Pestañas
            TabControl pestañas = new TabControl();
            pestañas.Dock = DockStyle.Fill;

            // Pestaña 1: Clientes
            TabPage tabClientes = new TabPage("1. Clientes");
            ConfigurarTabClientes(tabClientes);

            // Pestaña 2: Catálogo
            TabPage tabCatalogo = new TabPage("2. Catálogo");
            ConfigurarTabCatalogo(tabCatalogo);

            // Pestaña 3: Préstamos
            TabPage tabPrestamos = new TabPage("3. Préstamos");
            ConfigurarTabPrestamos(tabPrestamos);

            pestañas.Controls.Add(tabClientes);
            pestañas.Controls.Add(tabCatalogo);
            pestañas.Controls.Add(tabPrestamos);

            this.Controls.Add(pestañas);
        }

        // ==========================================
        // DISEÑO DE LA PESTAÑA: CLIENTES
        // ==========================================
        private void ConfigurarTabClientes(TabPage tab)
        {
            Button btnVer = CrearBoton("Ver Clientes", 50, 30);
            btnVer.Click += (s, e) => MessageBox.Show($"Total de clientes: {_conteoClientes}", "Clientes");

            Button btnAgregar = CrearBoton("Agregar Cliente", 50, 80);
            btnAgregar.Click += (s, e) => MessageBox.Show("Aquí se abriría un cuadro para ingresar los datos del cliente.", "En construcción");

            Button btnTop = CrearBoton("Cliente con más préstamos", 50, 130);
            btnTop.Click += (s, e) => MessageBox.Show("Consultando cliente top...", "Estadísticas");

            tab.Controls.Add(btnVer);
            tab.Controls.Add(btnAgregar);
            tab.Controls.Add(btnTop);
        }

        // ==========================================
        // DISEÑO DE LA PESTAÑA: CATÁLOGO
        // ==========================================
        private void ConfigurarTabCatalogo(TabPage tab)
        {
            Button btnVer = CrearBoton("Ver Catálogo Completo", 50, 30);
            btnVer.Click += (s, e) => {
                Libro[] libros = _catalogo.Recorrer();
                MessageBox.Show($"Títulos registrados: {libros.Length}", "Árbol B+");
            };

            Button btnMaxHeap = CrearBoton("Libro Más Prestado (Max Heap)", 50, 80);
            btnMaxHeap.Click += (s, e) => {
                Libro top = _maxHeapPrestamos.ObtenerMaximo();
                MessageBox.Show(top != null ? top.Titulo : "Sin datos", "Max Heap");
            };

            Button btnMinHeap = CrearBoton("Libro con Menos Copias (Min Heap)", 50, 130);
            btnMinHeap.Click += (s, e) => {
                Libro min = _minHeapCopias.ObtenerMinimo();
                MessageBox.Show(min != null ? min.Titulo : "Sin datos", "Min Heap");
            };

            tab.Controls.Add(btnVer);
            tab.Controls.Add(btnMaxHeap);
            tab.Controls.Add(btnMinHeap);
        }

        // ==========================================
        // DISEÑO DE LA PESTAÑA: PRÉSTAMOS
        // ==========================================
        private void ConfigurarTabPrestamos(TabPage tab)
        {
            Button btnPrestar = CrearBoton("Prestar Libro", 50, 30);
            btnPrestar.Click += (s, e) => MessageBox.Show("Simulando préstamo... Recuerda usar _maxHeapPrestamos.Reconstruir() al final.", "Préstamos");

            Button btnDevolver = CrearBoton("Devolver Libro", 50, 80);
            btnDevolver.Click += (s, e) => MessageBox.Show("Simulando devolución... Recuerda usar _minHeapCopias.Reconstruir() al final.", "Devoluciones");

            tab.Controls.Add(btnPrestar);
            tab.Controls.Add(btnDevolver);
        }

        // Método utilitario para no repetir código creando botones
        private Button CrearBoton(string texto, int x, int y)
        {
            return new Button
            {
                Text = texto,
                Location = new Point(x, y),
                Size = new Size(350, 40),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
        }
    }
}