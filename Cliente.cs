// Clase de Clientes
//Atributos: Id, nombre, celular, residencia, historial de prestamos devueltos [Libro], historial de prestamos pendientes [Libro]

class Cliente
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Celular { get; set; }
    public string Residencia { get; set; }
    public List<Libro> HistorialPrestamosDevueltos { get; set; }
    public List<Libro> HistorialPrestamosPendientes { get; set; }

    public Cliente(int id, string nombre, string celular, string residencia)
    {
        Id = id;
        Nombre = nombre;
        Celular = celular;
        Residencia = residencia;
        HistorialPrestamosDevueltos = new List<Libro>();
        HistorialPrestamosPendientes = new List<Libro>();
    }

    public override string ToString()
    {
        return $"ID: {Id}, Nombre: {Nombre}, Celular: {Celular}, Residencia: {Residencia}";
    }

    public void InformacionCliente()
    {
        Console.WriteLine($"ID: {Id}, Nombre: {Nombre}, Celular: {Celular}, Residencia: {Residencia}");
        Console.WriteLine("Historial de Prestamos Devueltos:");
        foreach (var libro in HistorialPrestamosDevueltos)
        {
            Console.WriteLine(libro.ToString());
        }
        Console.WriteLine("Historial de Prestamos Pendientes:");
        foreach (var libro in HistorialPrestamosPendientes)
        {
            Console.WriteLine(libro.ToString());
        }
    }

    public bool PrestarLibro(Libro libro)
    {
        if (HistorialPrestamosPendientes.Contains(libro))
        {
            return false; 
        }
        else
        {
            HistorialPrestamosPendientes.Add(libro);
            return true;
        }
    }

    public bool DevolverLibro(Libro libro)
    {
        if (HistorialPrestamosPendientes.Contains(libro))
        {
            HistorialPrestamosPendientes.Remove(libro);
            HistorialPrestamosDevueltos.Add(libro);
            return true;
        }
        else
        {
            return false;
        }
    }
}