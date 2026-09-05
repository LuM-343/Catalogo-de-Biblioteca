//Clase principal de libro

class Libro
{
    public int Id { get; set; }
    public string Titulo { get; set; }
    public string Autor { get; set; }
    public int AnioPublicacion { get; set; }
    public string Genero { get; set; }
    public int CopiasTotales {get; set;}
    public int CopiasDisponibles {get; set;}
    public int PrestamosTotales {get; set;}

    public Libro(int id, string titulo, string autor, int anioPublicacion, string genero, int copiasTotales, int copiasDisponibles, int prestamosTotales)
    {
        Id = id;
        Titulo = titulo;
        Autor = autor;
        AnioPublicacion = anioPublicacion;
        Genero = genero;
        CopiasTotales = copiasTotales;
        CopiasDisponibles = copiasDisponibles;
        PrestamosTotales = prestamosTotales;
    }

    public override string ToString()
    {
        return $"ID: {Id}, Título: {Titulo}, Autor: {Autor}, Año de Publicación: {AnioPublicacion}, Género: {Genero}";
    }

    public int CompareTo(Libro otro)
    {
        if (otro == null) return 1;
        return Id.CompareTo(otro.Id);
    }

    public int AgregarCopias(int cantidad)
    {
        CopiasTotales += cantidad;
        CopiasDisponibles += cantidad;
        return CopiasTotales;
    }

    public int QuitarCopias(int cantidad)
    {
        if (cantidad <= CopiasDisponibles)
        {
            CopiasTotales -= cantidad;
            CopiasDisponibles -= cantidad;
            return CopiasTotales;
        }
        else
        {
            return -1; 
        }
    }

    public int PrestarLibro()
    {
        if (CopiasDisponibles > 0)
        {
            CopiasDisponibles--;
            PrestamosTotales++;
            return CopiasDisponibles;
        }
        else
        {
            return -1; 
        }
    }

    public int DevolverLibro()
    {
        if (CopiasDisponibles < CopiasTotales)
        {
            CopiasDisponibles++;
            return CopiasDisponibles;
        }
        else
        {
            return -1; 
        }
    }

    public void Informacion()
    {
        Console.WriteLine($"ID: {Id}");
        Console.WriteLine($"Título: {Titulo}");
        Console.WriteLine($"Autor: {Autor}");
        Console.WriteLine($"Año de Publicación: {AnioPublicacion}");
        Console.WriteLine($"Género: {Genero}");
        Console.WriteLine($"Copias Totales: {CopiasTotales}");
        Console.WriteLine($"Copias Disponibles: {CopiasDisponibles}");
        Console.WriteLine($"Préstamos Totales: {PrestamosTotales}");
    }
}