using System;
using System.IO;
namespace EstructurasSuperPros
{
    // Montículo Máximo para gestionar el libro con mayor cantidad de préstamos
    public class MaxHeapLibros
    {
        private Libro[] _elementos;
        public int Conteo { get; private set; }

        public MaxHeapLibros(int capacidadInicial = 20)
        {
            _elementos = new Libro[capacidadInicial];
            Conteo = 0;
        }

        private void AsegurarCapacidad()
        {
            if (Conteo >= _elementos.Length)
            {
                Libro[] nuevo = new Libro[_elementos.Length * 2];
                Array.Copy(_elementos, nuevo, _elementos.Length);
                _elementos = nuevo;
            }
        }

        private void Intercambiar(int i, int j)
        {
            Libro temp = _elementos[i];
            _elementos[i] = _elementos[j];
            _elementos[j] = temp;
        }

        public void Insertar(Libro libro)
        {
            if (libro == null) return;
            AsegurarCapacidad();
            _elementos[Conteo] = libro;
            Flotar(Conteo);
            Conteo++;
        }

        private void Flotar(int indice)
        {
            while (indice > 0)
            {
                int padre = (indice - 1) / 2;
                if (_elementos[indice].PrestamosTotales > _elementos[padre].PrestamosTotales)
                {
                    Intercambiar(indice, padre);
                    indice = padre;
                }
                else
                {
                    break;
                }
            }
        }

        private void Hundir(int indice)
        {
            while (true)
            {
                int mayor = indice;
                int izq = 2 * indice + 1;
                int der = 2 * indice + 2;

                if (izq < Conteo && _elementos[izq].PrestamosTotales > _elementos[mayor].PrestamosTotales)
                    mayor = izq;

                if (der < Conteo && _elementos[der].PrestamosTotales > _elementos[mayor].PrestamosTotales)
                    mayor = der;

                if (mayor != indice)
                {
                    Intercambiar(indice, mayor);
                    indice = mayor;
                }
                else
                {
                    break;
                }
            }
        }

        public Libro ObtenerMaximo()
        {
            if (Conteo == 0) return null;
            return _elementos[0];
        }

        public Libro ExtraerMaximo()
        {
            if (Conteo == 0) return null;
            Libro raiz = _elementos[0];
            _elementos[0] = _elementos[Conteo - 1];
            _elementos[Conteo - 1] = null;
            Conteo--;
            if (Conteo > 0)
                Hundir(0);

            return raiz;
        }

        public bool EliminarPorId(int libroId)
        {
            int indice = -1;
            for (int i = 0; i < Conteo; i++)
            {
                if (_elementos[i].Id == libroId)
                {
                    indice = i;
                    break;
                }
            }

            if (indice == -1) return false;

            _elementos[indice] = _elementos[Conteo - 1];
            _elementos[Conteo - 1] = null;
            Conteo--;

            if (indice < Conteo)
                Reconstruir();

            return true;
        }

        // Se invoca tras mutar 'PrestamosTotales' al prestar un libro
        public void Reconstruir()
        {
            for (int i = (Conteo / 2) - 1; i >= 0; i--)
            {
                Hundir(i);
            }
        }

        public void ImprimirArbol()
        {
            if (Conteo == 0)
            {
                Console.WriteLine("El montículo está vacío.");
                return;
            }

            for (int i = 0; i < Conteo; i++)
            {
                Console.WriteLine($"[{i}] ID: {_elementos[i].Id} | Préstamos: {_elementos[i].PrestamosTotales} | {_elementos[i].Titulo}");
            }
        }
    }

    // Montículo Mínimo para gestionar el libro con menor disponibilidad de copias
    public class MinHeapLibros
    {
        private Libro[] _elementos;
        public int Conteo { get; private set; }

        public MinHeapLibros(int capacidadInicial = 20)
        {
            _elementos = new Libro[capacidadInicial];
            Conteo = 0;
        }

        private void AsegurarCapacidad()
        {
            if (Conteo >= _elementos.Length)
            {
                Libro[] nuevo = new Libro[_elementos.Length * 2];
                Array.Copy(_elementos, nuevo, _elementos.Length);
                _elementos = nuevo;
            }
        }

        private void Intercambiar(int i, int j)
        {
            Libro temp = _elementos[i];
            _elementos[i] = _elementos[j];
            _elementos[j] = temp;
        }

        public void Insertar(Libro libro)
        {
            if (libro == null) return;
            AsegurarCapacidad();
            _elementos[Conteo] = libro;
            Flotar(Conteo);
            Conteo++;
        }

        private void Flotar(int indice)
        {
            while (indice > 0)
            {
                int padre = (indice - 1) / 2;
                if (_elementos[indice].CopiasDisponibles < _elementos[padre].CopiasDisponibles)
                {
                    Intercambiar(indice, padre);
                    indice = padre;
                }
                else
                {
                    break;
                }
            }
        }

        private void Hundir(int indice)
        {
            while (true)
            {
                int menor = indice;
                int izq = 2 * indice + 1;
                int der = 2 * indice + 2;

                if (izq < Conteo && _elementos[izq].CopiasDisponibles < _elementos[menor].CopiasDisponibles)
                    menor = izq;

                if (der < Conteo && _elementos[der].CopiasDisponibles < _elementos[menor].CopiasDisponibles)
                    menor = der;

                if (menor != indice)
                {
                    Intercambiar(indice, menor);
                    indice = menor;
                }
                else
                {
                    break;
                }
            }
        }

        public Libro ObtenerMinimo()
        {
            if (Conteo == 0) return null;
            return _elementos[0];
        }

        public Libro ExtraerMinimo()
        {
            if (Conteo == 0) return null;
            Libro raiz = _elementos[0];
            _elementos[0] = _elementos[Conteo - 1];
            _elementos[Conteo - 1] = null;
            Conteo--;
            if (Conteo > 0)
                Hundir(0);

            return raiz;
        }

        public bool EliminarPorId(int libroId)
        {
            int indice = -1;
            for (int i = 0; i < Conteo; i++)
            {
                if (_elementos[i].Id == libroId)
                {
                    indice = i;
                    break;
                }
            }

            if (indice == -1) return false;

            _elementos[indice] = _elementos[Conteo - 1];
            _elementos[Conteo - 1] = null;
            Conteo--;

            if (indice < Conteo)
                Reconstruir();

            return true;
        }

        // Se invoca tras alterar 'CopiasDisponibles' en préstamos o devoluciones
        public void Reconstruir()
        {
            for (int i = (Conteo / 2) - 1; i >= 0; i--)
            {
                Hundir(i);
            }
        }

        public void ImprimirArbol()
        {
            if (Conteo == 0)
            {
                Console.WriteLine("El montículo está vacío.");
                return;
            }

            for (int i = 0; i < Conteo; i++)
            {
                Console.WriteLine($"[{i}] ID: {_elementos[i].Id} | Copias Disp: {_elementos[i].CopiasDisponibles} | {_elementos[i].Titulo}");
            }
        }
    }
}