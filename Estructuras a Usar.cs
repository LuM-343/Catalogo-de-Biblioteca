using System;

namespace EstructurasSupersPros
{
    public class NodoBPlus<TKey, TValue> where TKey : IComparable<TKey>
    {
        public bool Hoja { get; set; }
        public int ConteoClaves { get; set; }
        public int ConteoHijos { get; set; }

        public TKey[] ClavesGuia { get; set; }
        public TValue[] Valores { get; set; }
        public NodoBPlus<TKey, TValue>[] Hijos { get; set; }

        public NodoBPlus<TKey, TValue> Siguiente { get; set; }
        public NodoBPlus<TKey, TValue> Padre { get; set; }

        public NodoBPlus(bool hoja, int maxCapacidad)
        {
            Hoja = hoja;
            ConteoClaves = 0;
            ConteoHijos = 0;

            // Se reserva espacio adicional (+1) para permitir sobreflujo temporal antes del split
            ClavesGuia = new TKey[maxCapacidad + 1];
            Hijos = new NodoBPlus<TKey, TValue>[maxCapacidad + 2];
            Valores = new TValue[maxCapacidad + 1];
        }
    }


    //IMPLEMENTACIÓN DE ARBOL GENERICO B+ PARA LIBROS Y CLIENTES
    public class ArbolBPlus<TKey, TValue> where TKey : IComparable<TKey>
    {
        public int Orden { get; }
        public int MaxClaves { get; }
        public int MinElementosHoja { get; }
        public int MinHijosInterno { get; }
        public NodoBPlus<TKey, TValue> Raiz { get; private set; }

        private readonly Func<TValue, TKey> _selectorClave;

        public ArbolBPlus(Func<TValue, TKey> selectorClave, int orden = 4)
        {
            if (orden < 3)
                throw new ArgumentException("El orden debe ser al menos 3.");

            Orden = orden;
            MaxClaves = orden - 1;
            MinElementosHoja = (int)Math.Ceiling((orden - 1) / 2.0);
            MinHijosInterno = (int)Math.Ceiling(orden / 2.0);
            _selectorClave = selectorClave ?? throw new ArgumentNullException(nameof(selectorClave));
            Raiz = new NodoBPlus<TKey, TValue>(hoja: true, MaxClaves);
        }

        private int BisectRightGuias(NodoBPlus<TKey, TValue> nodo, TKey clave)
        {
            int bajo = 0, alto = nodo.ConteoClaves;
            while (bajo < alto)
            {
                int medio = (bajo + alto) / 2;
                if (clave.CompareTo(nodo.ClavesGuia[medio]) < 0)
                    alto = medio;
                else
                    bajo = medio + 1;
            }
            return bajo;
        }

        private int BuscarIndiceValor(NodoBPlus<TKey, TValue> hoja, TKey clave)
        {
            int bajo = 0;
            int alto = hoja.ConteoClaves - 1;
            while (bajo <= alto)
            {
                int medio = (bajo + alto) / 2;
                int comparacion = _selectorClave(hoja.Valores[medio]).CompareTo(clave);

                if (comparacion == 0) return medio;
                if (comparacion < 0) bajo = medio + 1;
                else alto = medio - 1;
            }
            return ~bajo;
        }

        private NodoBPlus<TKey, TValue> BuscarHoja(TKey clave)
        {
            var nodo = Raiz;
            while (!nodo.Hoja)
            {
                int pos = BisectRightGuias(nodo, clave);
                nodo = nodo.Hijos[pos];
            }
            return nodo;
        }

        public TValue Buscar(TKey clave)
        {
            var hoja = BuscarHoja(clave);
            int idx = BuscarIndiceValor(hoja, clave);
            return (idx >= 0) ? hoja.Valores[idx] : default;
        }

        public bool Insertar(TValue valor)
        {
            TKey clave = _selectorClave(valor);
            var hoja = BuscarHoja(clave);
            int idx = BuscarIndiceValor(hoja, clave);

            if (idx >= 0) return false; // Clave duplicada

            int posInsercion = ~idx;
            for (int i = hoja.ConteoClaves; i > posInsercion; i--)
            {
                hoja.Valores[i] = hoja.Valores[i - 1];
            }
            hoja.Valores[posInsercion] = valor;
            hoja.ConteoClaves++;

            if (hoja.ConteoClaves > MaxClaves)
                DividirHoja(hoja);

            RecalcularGuias(Raiz);
            return true;
        }

        private void DividirHoja(NodoBPlus<TKey, TValue> hoja)
        {
            int punto = (hoja.ConteoClaves + 1) / 2;
            var nuevaHoja = new NodoBPlus<TKey, TValue>(hoja: true, MaxClaves) { Padre = hoja.Padre };

            int elementosNuevos = hoja.ConteoClaves - punto;
            for (int i = 0; i < elementosNuevos; i++)
            {
                nuevaHoulaArrayCopy(hoja.Valores, punto + i, nuevaHoja.Valores, i);
                nuevaHoja.ConteoClaves++;
                hoja.Valores[punto + i] = default;
            }
            hoja.ConteoClaves = punto;

            nuevaHoja.Siguiente = hoja.Siguiente;
            hoja.Siguiente = nuevaHoja;

            InsertarEnPadre(hoja, _selectorClave(nuevaHoja.Valores[0]), nuevaHoja);
        }

        private static void nuevaHoulaArrayCopy(TValue[] origen, int idxOrigen, TValue[] destino, int idxDestino)
        {
            destino[idxDestino] = origen[idxOrigen];
        }

        private void InsertarEnPadre(NodoBPlus<TKey, TValue> izq, TKey guia, NodoBPlus<TKey, TValue> der)
        {
            if (izq == Raiz)
            {
                var nuevaRaiz = new NodoBPlus<TKey, TValue>(hoja: false, MaxClaves);
                nuevaRaiz.ClavesGuia[0] = guia;
                nuevaRaiz.ConteoClaves = 1;
                nuevaRaiz.Hijos[0] = izq;
                nuevaRaiz.Hijos[1] = der;
                nuevaRaiz.ConteoHijos = 2;

                izq.Padre = nuevaRaiz;
                der.Padre = nuevaRaiz;
                Raiz = nuevaRaiz;
                return;
            }

            var padre = izq.Padre;
            int pos = Array.IndexOf(padre.Hijos, izq, 0, padre.ConteoHijos);

            for (int i = padre.ConteoClaves; i > pos; i--)
                padre.ClavesGuia[i] = padre.ClavesGuia[i - 1];
            padre.ClavesGuia[pos] = guia;
            padre.ConteoClaves++;

            for (int i = padre.ConteoHijos; i > pos + 1; i--)
                padre.Hijos[i] = padre.Hijos[i - 1];
            padre.Hijos[pos + 1] = der;
            padre.ConteoHijos++;

            der.Padre = padre;

            if (padre.ConteoClaves > MaxClaves)
                DividirInterno(padre);
        }

        private void DividirInterno(NodoBPlus<TKey, TValue> nodo)
        {
            int centro = nodo.ConteoClaves / 2;
            TKey claveQueSube = nodo.ClavesGuia[centro];

            var nuevoInterno = new NodoBPlus<TKey, TValue>(hoja: false, MaxClaves) { Padre = nodo.Padre };

            int j = 0;
            for (int i = centro + 1; i < nodo.ConteoClaves; i++)
            {
                nuevoInterno.ClavesGuia[j++] = nodo.ClavesGuia[i];
                nuevoInterno.ConteoClaves++;
                nodo.ClavesGuia[i] = default;
            }

            j = 0;
            for (int i = centro + 1; i < nodo.ConteoHijos; i++)
            {
                nuevoInterno.Hijos[j] = nodo.Hijos[i];
                nuevoInterno.Hijos[j].Padre = nuevoInterno;
                nuevoInterno.ConteoHijos++;
                nodo.Hijos[i] = null;
                j++;
            }

            nodo.ClavesGuia[centro] = default;
            nodo.ConteoClaves = centro;
            nodo.ConteoHijos = centro + 1;

            InsertarEnPadre(nodo, claveQueSube, nuevoInterno);
        }

        public bool Eliminar(TKey clave)
        {
            var hoja = BuscarHoja(clave);
            int pos = BuscarIndiceValor(hoja, clave);

            if (pos < 0) return false;

            for (int i = pos; i < hoja.ConteoClaves - 1; i++)
                hoja.Valores[i] = hoja.Valores[i + 1];

            hoja.Valores[hoja.ConteoClaves - 1] = default;
            hoja.ConteoClaves--;

            if (hoja != Raiz && hoja.ConteoClaves < MinElementosHoja)
                RepararHoja(hoja);

            RecalcularGuias(Raiz);
            return true;
        }

        private void RepararHoja(NodoBPlus<TKey, TValue> hoja)
        {
            var padre = hoja.Padre;
            int pos = Array.IndexOf(padre.Hijos, hoja, 0, padre.ConteoHijos);
            var izq = (pos > 0) ? padre.Hijos[pos - 1] : null;
            var der = (pos + 1 < padre.ConteoHijos) ? padre.Hijos[pos + 1] : null;

            if (izq != null && izq.ConteoClaves > MinElementosHoja)
            {
                var prestado = izq.Valores[izq.ConteoClaves - 1];
                izq.Valores[izq.ConteoClaves - 1] = default;
                izq.ConteoClaves--;

                for (int i = hoja.ConteoClaves; i > 0; i--)
                    hoja.Valores[i] = hoja.Valores[i - 1];
                hoja.Valores[0] = prestado;
                hoja.ConteoClaves++;
                return;
            }

            if (der != null && der.ConteoClaves > MinElementosHoja)
            {
                var prestado = der.Valores[0];
                for (int i = 0; i < der.ConteoClaves - 1; i++)
                    der.Valores[i] = der.Valores[i + 1];
                der.Valores[der.ConteoClaves - 1] = default;
                der.ConteoClaves--;

                hoja.Valores[hoja.ConteoClaves++] = prestado;
                return;
            }

            if (izq != null)
            {
                for (int i = 0; i < hoja.ConteoClaves; i++)
                    izq.Valores[izq.ConteoClaves++] = hoja.Valores[i];

                izq.Siguiente = hoja.Siguiente;

                for (int i = pos; i < padre.ConteoHijos - 1; i++)
                    padre.Hijos[i] = padre.Hijos[i + 1];
                padre.Hijos[padre.ConteoHijos - 1] = null;
                padre.ConteoHijos--;

                for (int i = pos - 1; i < padre.ConteoClaves - 1; i++)
                    padre.ClavesGuia[i] = padre.ClavesGuia[i + 1];
                padre.ClavesGuia[padre.ConteoClaves - 1] = default;
                padre.ConteoClaves--;

                RepararInterno(padre);
            }
            else if (der != null)
            {
                for (int i = 0; i < der.ConteoClaves; i++)
                    hoja.Valores[hoja.ConteoClaves++] = der.Valores[i];

                hoja.Siguiente = der.Siguiente;

                for (int i = pos + 1; i < padre.ConteoHijos - 1; i++)
                    padre.Hijos[i] = padre.Hijos[i + 1];
                padre.Hijos[padre.ConteoHijos - 1] = null;
                padre.ConteoHijos--;

                for (int i = pos; i < padre.ConteoClaves - 1; i++)
                    padre.ClavesGuia[i] = padre.ClavesGuia[i + 1];
                padre.ClavesGuia[padre.ConteoClaves - 1] = default;
                padre.ConteoClaves--;

                RepararInterno(padre);
            }
        }

        private void RepararInterno(NodoBPlus<TKey, TValue> nodo)
        {
            if (nodo == Raiz)
            {
                if (nodo.ConteoClaves == 0 && nodo.ConteoHijos > 0)
                {
                    Raiz = nodo.Hijos[0];
                    Raiz.Padre = null;
                }
                return;
            }

            if (nodo.ConteoHijos >= MinHijosInterno) return;

            var padre = nodo.Padre;
            int pos = Array.IndexOf(padre.Hijos, nodo, 0, padre.ConteoHijos);
            var izq = (pos > 0) ? padre.Hijos[pos - 1] : null;
            var der = (pos + 1 < padre.ConteoHijos) ? padre.Hijos[pos + 1] : null;

            if (izq != null && izq.ConteoHijos > MinHijosInterno)
            {
                var hijoMovido = izq.Hijos[izq.ConteoHijos - 1];
                izq.Hijos[izq.ConteoHijos - 1] = null;
                izq.ConteoHijos--;
                hijoMovido.Padre = nodo;

                TKey guiaMovida = izq.ClavesGuia[izq.ConteoClaves - 1];
                izq.ClavesGuia[izq.ConteoClaves - 1] = default;
                izq.ConteoClaves--;

                for (int i = nodo.ConteoHijos; i > 0; i--)
                    nodo.Hijos[i] = nodo.Hijos[i - 1];
                nodo.Hijos[0] = hijoMovido;
                nodo.ConteoHijos++;

                for (int i = nodo.ConteoClaves; i > 0; i--)
                    nodo.ClavesGuia[i] = nodo.ClavesGuia[i - 1];
                nodo.ClavesGuia[0] = padre.ClavesGuia[pos - 1];
                nodo.ConteoClaves++;

                padre.ClavesGuia[pos - 1] = guiaMovida;
                return;
            }

            if (der != null && der.ConteoHijos > MinHijosInterno)
            {
                var hijoMovido = der.Hijos[0];
                for (int i = 0; i < der.ConteoHijos - 1; i++)
                    der.Hijos[i] = der.Hijos[i + 1];
                der.Hijos[der.ConteoHijos - 1] = null;
                der.ConteoHijos--;
                hijoMovido.Padre = nodo;

                nodo.Hijos[nodo.ConteoHijos++] = hijoMovido;
                nodo.ClavesGuia[nodo.ConteoClaves++] = padre.ClavesGuia[pos];

                padre.ClavesGuia[pos] = der.ClavesGuia[0];
                for (int i = 0; i < der.ConteoClaves - 1; i++)
                    der.ClavesGuia[i] = der.ClavesGuia[i + 1];
                der.ClavesGuia[der.ConteoClaves - 1] = default;
                der.ConteoClaves--;
                return;
            }

            if (izq != null)
            {
                izq.ClavesGuia[izq.ConteoClaves++] = padre.ClavesGuia[pos - 1];
                for (int i = 0; i < nodo.ConteoClaves; i++)
                    izq.ClavesGuia[izq.ConteoClaves++] = nodo.ClavesGuia[i];

                for (int i = 0; i < nodo.ConteoHijos; i++)
                {
                    nodo.Hijos[i].Padre = izq;
                    izq.Hijos[izq.ConteoHijos++] = nodo.Hijos[i];
                }

                for (int i = pos - 1; i < padre.ConteoClaves - 1; i++)
                    padre.ClavesGuia[i] = padre.ClavesGuia[i + 1];
                padre.ClavesGuia[padre.ConteoClaves - 1] = default;
                padre.ConteoClaves--;

                for (int i = pos; i < padre.ConteoHijos - 1; i++)
                    padre.Hijos[i] = padre.Hijos[i + 1];
                padre.Hijos[padre.ConteoHijos - 1] = null;
                padre.ConteoHijos--;

                RepararInterno(padre);
            }
            else if (der != null)
            {
                nodo.ClavesGuia[nodo.ConteoClaves++] = padre.ClavesGuia[pos];
                for (int i = 0; i < der.ConteoClaves; i++)
                    nodo.ClavesGuia[nodo.ConteoClaves++] = der.ClavesGuia[i];

                for (int i = 0; i < der.ConteoHijos; i++)
                {
                    der.Hijos[i].Padre = nodo;
                    nodo.Hijos[nodo.ConteoHijos++] = der.Hijos[i];
                }

                for (int i = pos; i < padre.ConteoClaves - 1; i++)
                    padre.ClavesGuia[i] = padre.ClavesGuia[i + 1];
                padre.ClavesGuia[padre.ConteoClaves - 1] = default;
                padre.ConteoClaves--;

                for (int i = pos + 1; i < padre.ConteoHijos - 1; i++)
                    padre.Hijos[i] = padre.Hijos[i + 1];
                padre.Hijos[padre.ConteoHijos - 1] = null;
                padre.ConteoHijos--;

                RepararInterno(padre);
            }
        }

        private TKey MinimoSubarbol(NodoBPlus<TKey, TValue> nodo)
        {
            while (!nodo.Hoja) nodo = nodo.Hijos[0];
            return _selectorClave(nodo.Valores[0]);
        }

        private void RecalcularGuias(NodoBPlus<TKey, TValue> nodo)
        {
            if (nodo.Hoja) return;
            for (int i = 0; i < nodo.ConteoHijos; i++)
                RecalcularGuias(nodo.Hijos[i]);

            nodo.ConteoClaves = 0;
            for (int i = 1; i < nodo.ConteoHijos; i++)
            {
                nodo.ClavesGuia[nodo.ConteoClaves++] = MinimoSubarbol(nodo.Hijos[i]);
            }
        }

        public int ContarTotal()
        {
            int total = 0;
            var nodo = Raiz;
            while (!nodo.Hoja) nodo = nodo.Hijos[0];
            while (nodo != null)
            {
                total += nodo.ConteoClaves;
                nodo = nodo.Siguiente;
            }
            return total;
        }

        public TValue[] Recorrer()
        {
            TValue[] salida = new TValue[ContarTotal()];
            int idx = 0;
            var nodo = Raiz;
            while (!nodo.Hoja) nodo = nodo.Hijos[0];
            while (nodo != null)
            {
                for (int i = 0; i < nodo.ConteoClaves; i++)
                    salida[idx++] = nodo.Valores[i];
                nodo = nodo.Siguiente;
            }
            return salida;
        }
    }

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