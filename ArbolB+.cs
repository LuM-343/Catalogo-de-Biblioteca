using System;
using System.Collections.Generic;

namespace EstructurasBPlus
{
    public class NodoBPlus<TKey, TValue> where TKey : IComparable<TKey>
    {
        public bool Hoja { get; set; }
        public List<TKey> ClavesGuia { get; set; }              // Claves guía (nodos internos)
        public List<TValue> Valores { get; set; }               // Datos reales (solo en hojas)
        public List<NodoBPlus<TKey, TValue>> Hijos { get; set; }
        public NodoBPlus<TKey, TValue> Siguiente { get; set; }
        public NodoBPlus<TKey, TValue> Padre { get; set; }

        // Inicializa un nuevo nodo estableciendo si operará como hoja o nodo interno
        public NodoBPlus(bool hoja = true)
        {
            Hoja = hoja;
            ClavesGuia = new List<TKey>();
            Valores = new List<TValue>();
            Hijos = new List<NodoBPlus<TKey, TValue>>();
        }
    }

    // Estructura del Árbol B+
    public class ArbolBPlus<TKey, TValue> where TKey : IComparable<TKey>
    {
        public int Orden { get; }
        public int MaxClaves { get; }
        public int MinElementosHoja { get; }
        public int MinHijosInterno { get; }
        public NodoBPlus<TKey, TValue> Raiz { get; private set; }

        private readonly Func<TValue, TKey> _selectorClave;

        // Constructor: Configura los límites de capacidad y la función extractora de clave
        public ArbolBPlus(Func<TValue, TKey> selectorClave, int orden = 4)
        {
            if (orden < 3)
                throw new ArgumentException("El orden debe ser al menos 3.");

            Orden = orden;
            MaxClaves = orden - 1;
            MinElementosHoja = (int)Math.Ceiling((orden - 1) / 2.0);
            MinHijosInterno = (int)Math.Ceiling(orden / 2.0);
            Raiz = new NodoBPlus<TKey, TValue>(hoja: true);
            _selectorClave = selectorClave ?? throw new ArgumentNullException(nameof(selectorClave));
        }

        // Determina el índice del hijo por el cual descender evaluando las claves guía
        private int BisectRightGuias(List<TKey> guias, TKey clave)
        {
            int bajo = 0, alto = guias.Count;
            while (bajo < alto)
            {
                int medio = (bajo + alto) / 2;
                if (clave.CompareTo(guias[medio]) < 0)
                    alto = medio;
                else
                    bajo = medio + 1;
            }
            return bajo;
        }

        // Realiza una búsqueda binaria en una hoja; retorna el índice si existe o el complemento bitwise (~) para inserción ordenada
        private int BuscarIndiceValor(List<TValue> valores, TKey clave)
        {
            int bajo = 0;
            int alto = valores.Count - 1;
            while (bajo <= alto)
            {
                int medio = (bajo + alto) / 2;
                int comparacion = _selectorClave(valores[medio]).CompareTo(clave);

                if (comparacion == 0) return medio;
                if (comparacion < 0) bajo = medio + 1;
                else alto = medio - 1;
            }
            return ~bajo;
        }

        // Navega desde la raíz hacia abajo hasta localizar la hoja donde debería estar la clave
        private NodoBPlus<TKey, TValue> BuscarHoja(TKey clave)
        {
            var nodo = Raiz;
            while (!nodo.Hoja)
            {
                int pos = BisectRightGuias(nodo.ClavesGuia, clave);
                nodo = nodo.Hijos[pos];
            }
            return nodo;
        }

        // Busca un elemento por su clave exacta; devuelve el valor o default si no existe
        public TValue Buscar(TKey clave)
        {
            var hoja = BuscarHoja(clave);
            int idx = BuscarIndiceValor(hoja.Valores, clave);
            return (idx >= 0) ? hoja.Valores[idx] : default;
        }

        // Recupera en orden todos los elementos cuyas claves se encuentren dentro del rango [inicio, fin] recorriendo las hojas
        public List<TValue> BuscarPorRango(TKey inicio, TKey fin)
        {
            if (inicio.CompareTo(fin) > 0)
                (inicio, fin) = (fin, inicio);

            var hoja = BuscarHoja(inicio);
            var resultado = new List<TValue>();

            while (hoja != null)
            {
                foreach (var valor in hoja.Valores)
                {
                    TKey clave = _selectorClave(valor);
                    if (clave.CompareTo(inicio) >= 0 && clave.CompareTo(fin) <= 0)
                        resultado.Add(valor);
                    else if (clave.CompareTo(fin) > 0)
                        return resultado;
                }
                hoja = hoja.Siguiente;
            }
            return resultado;
        }

        // Inserta un nuevo objeto de forma ordenada en la hoja adecuada, dividiendo nodos si ocurre desbordamiento
        public bool Insertar(TValue valor)
        {
            TKey clave = _selectorClave(valor);
            var hoja = BuscarHoja(clave);
            int idx = BuscarIndiceValor(hoja.Valores, clave);

            if (idx >= 0) return false; // Clave duplicada no permitida

            hoja.Valores.Insert(~idx, valor);

            if (hoja.Valores.Count > MaxClaves)
                DividirHoja(hoja);

            RecalcularGuias(Raiz);
            return true;
        }

        // Divide una hoja llena en dos mitades, actualiza la lista enlazada y propaga la clave guía hacia el padre
        private void DividirHoja(NodoBPlus<TKey, TValue> hoja)
        {
            int punto = (hoja.Valores.Count + 1) / 2;
            var nuevaHoja = new NodoBPlus<TKey, TValue>(hoja: true) { Padre = hoja.Padre };

            nuevaHoja.Valores.AddRange(hoja.Valores.GetRange(punto, hoja.Valores.Count - punto));
            hoja.Valores.RemoveRange(punto, hoja.Valores.Count - punto);

            nuevaHoja.Siguiente = hoja.Siguiente;
            hoja.Siguiente = nuevaHoja;

            InsertarEnPadre(hoja, _selectorClave(nuevaHoja.Valores[0]), nuevaHoja);
        }

        // Inserta la clave guía y el puntero al nuevo hijo en el nodo padre, crea una nueva raíz si la raíz actual se dividió
        private void InsertarEnPadre(NodoBPlus<TKey, TValue> izq, TKey guia, NodoBPlus<TKey, TValue> der)
        {
            if (izq == Raiz)
            {
                var nuevaRaiz = new NodoBPlus<TKey, TValue>(hoja: false);
                nuevaRaiz.ClavesGuia.Add(guia);
                nuevaRaiz.Hijos.Add(izq);
                nuevaRaiz.Hijos.Add(der);
                izq.Padre = nuevaRaiz;
                der.Padre = nuevaRaiz;
                Raiz = nuevaRaiz;
                return;
            }

            var padre = izq.Padre;
            int pos = padre.Hijos.IndexOf(izq);
            padre.ClavesGuia.Insert(pos, guia);
            padre.Hijos.Insert(pos + 1, der);
            der.Padre = padre;

            if (padre.ClavesGuia.Count > MaxClaves)
                DividirInterno(padre);
        }

        // Divide un nodo interno desbordado, subiendo la clave central al nivel superior y repartiendo hijos y guías
        private void DividirInterno(NodoBPlus<TKey, TValue> nodo)
        {
            int centro = nodo.ClavesGuia.Count / 2;
            TKey claveQueSube = nodo.ClavesGuia[centro];

            var nuevoInterno = new NodoBPlus<TKey, TValue>(hoja: false) { Padre = nodo.Padre };
            nuevoInterno.ClavesGuia.AddRange(nodo.ClavesGuia.GetRange(centro + 1, nodo.ClavesGuia.Count - (centro + 1)));
            nuevoInterno.Hijos.AddRange(nodo.Hijos.GetRange(centro + 1, nodo.Hijos.Count - (centro + 1)));

            foreach (var h in nuevoInterno.Hijos) h.Padre = nuevoInterno;

            nodo.ClavesGuia.RemoveRange(centro, nodo.ClavesGuia.Count - centro);
            nodo.Hijos.RemoveRange(centro + 1, nodo.Hijos.Count - (centro + 1));

            InsertarEnPadre(nodo, claveQueSube, nuevoInterno);
        }

        // Remueve un elemento a partir de su clave y dispara la reparación por underflow si la hoja queda con menos del mínimo
        public bool Eliminar(TKey clave)
        {
            var hoja = BuscarHoja(clave);
            int pos = BuscarIndiceValor(hoja.Valores, clave);

            if (pos < 0) return false;

            hoja.Valores.RemoveAt(pos);

            if (hoja != Raiz && hoja.Valores.Count < MinElementosHoja)
                RepararHoja(hoja);

            RecalcularGuias(Raiz);
            return true;
        }

        // Restaura el balance de una hoja desabastecida pidiendo prestado a un hermano adyacente o fusionándose con él
        private void RepararHoja(NodoBPlus<TKey, TValue> hoja)
        {
            var padre = hoja.Padre;
            int pos = padre.Hijos.IndexOf(hoja);
            var izq = (pos > 0) ? padre.Hijos[pos - 1] : null;
            var der = (pos + 1 < padre.Hijos.Count) ? padre.Hijos[pos + 1] : null;

            if (izq != null && izq.Valores.Count > MinElementosHoja)
            {
                var prestado = izq.Valores[^1];
                izq.Valores.RemoveAt(izq.Valores.Count - 1);
                hoja.Valores.Insert(0, prestado);
                return;
            }

            if (der != null && der.Valores.Count > MinElementosHoja)
            {
                var prestado = der.Valores[0];
                der.Valores.RemoveAt(0);
                hoja.Valores.Add(prestado);
                return;
            }

            if (izq != null)
            {
                izq.Valores.AddRange(hoja.Valores);
                izq.Siguiente = hoja.Siguiente;
                padre.Hijos.RemoveAt(pos);
                padre.ClavesGuia.RemoveAt(pos - 1);
                RepararInterno(padre);
            }
            else if (der != null)
            {
                hoja.Valores.AddRange(der.Valores);
                hoja.Siguiente = der.Siguiente;
                padre.Hijos.RemoveAt(pos + 1);
                padre.ClavesGuia.RemoveAt(pos);
                RepararInterno(padre);
            }
        }

        // Restaura el balance de un nodo interno con escasez de hijos mediante rotación o fusión con nodos hermanos
        private void RepararInterno(NodoBPlus<TKey, TValue> nodo)
        {
            if (nodo == Raiz)
            {
                if (nodo.ClavesGuia.Count == 0)
                {
                    Raiz = nodo.Hijos[0];
                    Raiz.Padre = null;
                }
                return;
            }

            if (nodo.Hijos.Count >= MinHijosInterno) return;

            var padre = nodo.Padre;
            int pos = padre.Hijos.IndexOf(nodo);
            var izq = (pos > 0) ? padre.Hijos[pos - 1] : null;
            var der = (pos + 1 < padre.Hijos.Count) ? padre.Hijos[pos + 1] : null;

            if (izq != null && izq.Hijos.Count > MinHijosInterno)
            {
                var hijoMovido = izq.Hijos[^1];
                izq.Hijos.RemoveAt(izq.Hijos.Count - 1);
                hijoMovido.Padre = nodo;
                TKey nuevaGuia = izq.ClavesGuia[^1];
                izq.ClavesGuia.RemoveAt(izq.ClavesGuia.Count - 1);

                nodo.Hijos.Insert(0, hijoMovido);
                nodo.ClavesGuia.Insert(0, padre.ClavesGuia[pos - 1]);
                padre.ClavesGuia[pos - 1] = nuevaGuia;
                return;
            }

            if (der != null && der.Hijos.Count > MinHijosInterno)
            {
                var hijoMovido = der.Hijos[0];
                der.Hijos.RemoveAt(0);
                hijoMovido.Padre = nodo;

                nodo.Hijos.Add(hijoMovido);
                nodo.ClavesGuia.Add(padre.ClavesGuia[pos]);
                padre.ClavesGuia[pos] = der.ClavesGuia[0];
                der.ClavesGuia.RemoveAt(0);
                return;
            }

            if (izq != null)
            {
                izq.ClavesGuia.Add(padre.ClavesGuia[pos - 1]);
                padre.ClavesGuia.RemoveAt(pos - 1);
                izq.ClavesGuia.AddRange(nodo.ClavesGuia);
                foreach (var h in nodo.Hijos) h.Padre = izq;
                izq.Hijos.AddRange(nodo.Hijos);
                padre.Hijos.RemoveAt(pos);
                RepararInterno(padre);
            }
            else if (der != null)
            {
                nodo.ClavesGuia.Add(padre.ClavesGuia[pos]);
                padre.ClavesGuia.RemoveAt(pos);
                nodo.ClavesGuia.AddRange(der.ClavesGuia);
                foreach (var h in der.Hijos) h.Padre = nodo;
                nodo.Hijos.AddRange(der.Hijos);
                padre.Hijos.RemoveAt(pos + 1);
                RepararInterno(padre);
            }
        }

        // Recorre la rama izquierda de un subárbol hasta la primera hoja para extraer su clave mínima
        private TKey MinimoSubarbol(NodoBPlus<TKey, TValue> nodo)
        {
            while (!nodo.Hoja) nodo = nodo.Hijos[0];
            return _selectorClave(nodo.Valores[0]);
        }

        // Actualiza de forma recursiva ascendente las claves guía internas tras mutaciones estructurales
        private void RecalcularGuias(NodoBPlus<TKey, TValue> nodo)
        {
            if (nodo.Hoja) return;
            foreach (var h in nodo.Hijos) RecalcularGuias(h);
            nodo.ClavesGuia.Clear();
            for (int i = 1; i < nodo.Hijos.Count; i++)
                nodo.ClavesGuia.Add(MinimoSubarbol(nodo.Hijos[i]));
        }

        // Recorre secuencialmente todas las hojas mediante sus punteros 'Siguiente' para obtener la colección completa ordenada
        public List<TValue> Recorrer()
        {
            var nodo = Raiz;
            while (!nodo.Hoja) nodo = nodo.Hijos[0];
            var resultado = new List<TValue>();
            while (nodo != null)
            {
                resultado.AddRange(nodo.Valores);
                nodo = nodo.Siguiente;
            }
            return resultado;
        }
    }
}