//Estructura de arbol B+ para almacenar los libros de la biblioteca
using System;

class ArbolBPlus
{
    private NodoBPlus raiz {get;set;}
    private int orden {get;set;}
    private int maxClaves {get; set;}
    private int minClavesHoja{get;set;}
    private int minHijosInterno{get;set;}

    private class NodoBPlus
    {
        public List<Libro> claves;
        public List<NodoBPlus> hijos;
        public bool esHoja;

        public NodoBPlus(bool esHoja)
        {
            this.esHoja = esHoja;
            claves = new List<Libro>();
            hijos = new List<NodoBPlus>();
        }
    }

    public ArbolBPlus(int orden)
    {
        this.orden = orden;
        raiz = null;
        maxClaves = orden - 1;
        minClavesHoja = (int)Math.Ceiling((double)orden / 2) - 1;
        minHijosInterno = (int)Math.Ceiling((double)orden / 2);
    }

/* 
Función en python para buscar la hoja donde se encuentra una clave en un árbol B+:
def _buscar_hoja(self, clave):
        nodo = self.raiz                                   # Comienza en la raíz.

        while not nodo.hoja:                               # Continúa mientras no sea hoja.
            posicion = bisect_right(nodo.claves, clave)    # Determina el rango de la clave.
            nodo = nodo.hijos[posicion]                    # Baja al hijo correspondiente.

        return nodo  */
    public NodoBPlus BuscarHoja(int clave)
    {
        if (raiz == null) return null;

        NodoBPlus nodo = raiz;

        while (!nodo.esHoja)
        {
            int posicion = nodo.claves.FindIndex(c => c.Id > clave);
            if (posicion == -1)
            {
                posicion = nodo.claves.Count;
            }

            nodo = nodo.hijos[posicion];
        }

        return nodo;
    }

    public NodoBPlus Buscar(int clave)
    {
        NodoBPlus hoja = BuscarHoja(clave);
        if (hoja != null)
        {
            return hoja.claves.Find(c => c.Id == clave) != null ? hoja : null;
        }
        return null;
    }

    public void Insertar(Libro libro)
    {
        if (raiz == null)
        {
            raiz = new NodoBPlus(true);
            raiz.claves.Add(libro);
        }
        else
        {
            if (raiz.claves.Count == maxClaves)
            {
                NodoBPlus nuevaRaiz = new NodoBPlus(false);
                nuevaRaiz.hijos.Add(raiz);
                DividirHijo(nuevaRaiz, 0, raiz);
                raiz = nuevaRaiz;
            }
            InsertarNoLleno(raiz, libro);
        }
    }

    private void DividirHijo(NodoBPlus padre, int indice, NodoBPlus hijo)
    {
        NodoBPlus nuevoHijo = new NodoBPlus(hijo.esHoja);
        int mitad = maxClaves / 2;

        for (int i = 0; i < mitad; i++)
        {
            nuevoHijo.claves.Add(hijo.claves[mitad + i]);
        }

        if (!hijo.esHoja)
        {
            for (int i = 0; i <= mitad; i++)
            {
                nuevoHijo.hijos.Add(hijo.hijos[mitad + i]);
            }
        }

        hijo.claves.RemoveRange(mitad, hijo.claves.Count - mitad);
        if (!hijo.esHoja)
        {
            hijo.hijos.RemoveRange(mitad + 1, hijo.hijos.Count - (mitad + 1));
        }

        padre.hijos.Insert(indice + 1, nuevoHijo);
        padre.claves.Insert(indice, hijo.claves[mitad]);
    }

    // Métodos para insertar, buscar y eliminar libros en el árbol B+
    // Implementación de la estructura del árbol B+ y sus operaciones
}