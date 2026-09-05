using System;

public class Cliente
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Celular { get; set; }
    public string Residencia { get; set; }

    // Almacenamos directamente los IDs (int) de los libros prestados y devueltos
    private int[] _prestamosPendientes;
    private int _conteoPendientes;

    private int[] _prestamosDevueltos;
    private int _conteoDevueltos;

    public Cliente(int id, string nombre, string celular, string residencia, int capacidadInicial = 10)
    {
        Id = id;
        Nombre = nombre;
        Celular = celular;
        Residencia = residencia;

        _prestamosPendientes = new int[capacidadInicial];
        _conteoPendientes = 0;

        _prestamosDevueltos = new int[capacidadInicial];
        _conteoDevueltos = 0;
    }

    private void RedimensionarSiEsNecesario(ref int[] arreglo, int conteo)
    {
        if (conteo >= arreglo.Length)
        {
            int[] nuevo = new int[arreglo.Length * 2];
            Array.Copy(arreglo, nuevo, arreglo.Length);
            arreglo = nuevo;
        }
    }

    public bool PrestarLibro(int libroId)
    {
        for (int i = 0; i < _conteoPendientes; i++)
        {
            if (_prestamosPendientes[i] == libroId)
                return false; // Ya lo tiene prestado
        }

        RedimensionarSiEsNecesario(ref _prestamosPendientes, _conteoPendientes);
        _prestamosPendientes[_conteoPendientes++] = libroId;
        return true;
    }

    public bool DevolverLibro(int libroId)
    {
        int pos = -1;
        for (int i = 0; i < _conteoPendientes; i++)
        {
            if (_prestamosPendientes[i] == libroId)
            {
                pos = i;
                break;
            }
        }

        if (pos == -1) return false;

        // Remover de pendientes compactando el arreglo
        for (int i = pos; i < _conteoPendientes - 1; i++)
            _prestamosPendientes[i] = _prestamosPendientes[i + 1];
        _prestamosPendientes[--_conteoPendientes] = 0;

        // Agregar a devueltos
        RedimensionarSiEsNecesario(ref _prestamosDevueltos, _conteoDevueltos);
        _prestamosDevueltos[_conteoDevueltos++] = libroId;
        return true;
    }

    public int[] ObtenerPrestamosPendientes()
    {
        int[] copia = new int[_conteoPendientes];
        Array.Copy(_prestamosPendientes, copia, _conteoPendientes);
        return copia;
    }

    public int[] ObtenerPrestamosDevueltos()
    {
        int[] copia = new int[_conteoDevueltos];
        Array.Copy(_prestamosDevueltos, copia, _conteoDevueltos);
        return copia;
    }

    public override string ToString()
    {
        return $"ID: {Id} | Nombre: {Nombre} | Celular: {Celular} | Residencia: {Residencia} | Activos: {_conteoPendientes}";
    }
}