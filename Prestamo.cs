using System;

class Prestamo
{
    public int IdCliente { get; set; }
    public int IdLibro { get; set; }
    public DateTime FechaPrestamo { get; set; }
    public DateTime? FechaDevolucion { get; set; }

    public Prestamo(int idCliente, int idLibro)
    {
        IdCliente = idCliente;
        IdLibro = idLibro;
        FechaPrestamo = DateTime.Now;
        FechaDevolucion = null;
    }

    public void Devolver()
    {
        FechaDevolucion = DateTime.Now;
    }

    public override string ToString()
    {
        string estado = FechaDevolucion.HasValue ? $"Devuelto el {FechaDevolucion.Value}" : "Pendiente de devolución";
        return $"Cliente ID: {IdCliente}, Libro ID: {IdLibro}, Prestado el: {FechaPrestamo}, Estado: {estado}";
    }

}