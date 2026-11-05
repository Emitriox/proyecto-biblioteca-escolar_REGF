using System;
using System.Collections.Generic;

namespace BibliotecaEscolar
{
    // Clase que representa un ejemplar individual de un libro
    public class Ejemplar
    {
        public int IdEjemplar { get; set; }
        public bool Disponible { get; set; }

        public Ejemplar(int id, bool disponible)
        {
            IdEjemplar = id;
            Disponible = disponible;
        }
    }

    // Clase que representa un Título en la biblioteca
    public class Titulo : IComparable<Titulo>
    {
        public string Isbn { get; set; }
        public string Nombre { get; set; }
        public string Autor { get; set; }
        public int PrestamosHistoricos { get; set; }
        public List<Ejemplar> Ejemplares { get; set; }

        public Titulo(string isbn, string nombre, string autor, int prestamosHistoricos, int cantidadEjemplares)
        {
            Isbn = isbn;
            Nombre = nombre;
            Autor = autor;
            PrestamosHistoricos = prestamosHistoricos;
            Ejemplares = new List<Ejemplar>();

            // Inicializar ejemplares irregulares sin celdas vacías
            for (int i = 1; i <= cantidadEjemplares; i++)
            {
                Ejemplares.Add(new Ejemplar(i, true));
            }
        }

        // Propiedad calculada para conocer cuántos ejemplares están disponibles actualmente
        public int EjemplaresDisponibles
        {
            get
            {
                int count = 0;
                foreach (var e in Ejemplares)
                {
                    if (e.Disponible) count++;
                }
                return count;
            }
        }

        // Implementación base para comparación por defecto (orden alfabético por ISBN o Título si se requiere)
        public int CompareTo(Titulo other)
        {
            if (other == null) return 1;
            return this.Isbn.CompareTo(other.Isbn);
        }
    }

    public class Biblioteca
    {
        private List<Titulo> catalogo;

        public Biblioteca()
        {
            catalogo = new List<Titulo>();
            CargarDatosPrueba();
        }

        // Requisito: Cargar al menos 8 títulos con sus ejemplares irregulares y préstamos históricos[cite: 1]
        private void CargarDatosPrueba()
        {
            catalogo.Add(new Titulo("978-968-16-0158-7", "Pedro Páramo", "Juan Rulfo", 12, 3));
            catalogo.Add(new Titulo("978-843-76-0494-7", "Cien años de soledad", "Gabriel García Márquez", 25, 5));
            catalogo.Add(new Titulo("978-950-07-2051-9", "El túnel", "Ernesto Sábato", 8, 1)); // 1 solo ejemplar
            catalogo.Add(new Titulo("978-607-31-3564-1", "Aura", "Carlos Fuentes", 12, 4)); // Empate en préstamos con Pedro Páramo
            catalogo.Add(new Titulo("978-842-04-7183-9", "La ciudad y los perros", "Mario Vargas Llosa", 19, 3));
            catalogo.Add(new Titulo("978-970-58-0178-2", "Ficciones", "Jorge Luis Borges", 15, 2));
            catalogo.Add(new Titulo("978-607-11-2895-9", "Las batallas en el desierto", "José Emilio Pacheco", 25, 3)); // Empate en préstamos con Cien años
            catalogo.Add(new Titulo("978-843-76-0495-4", "Rayuela", "Julio Cortázar", 7, 2));
        }

        public List<Titulo> ObtenerCatalogo() => catalogo;

        // Requisito: Ordenamiento por inserción manual (Descendente por Préstamos, Estable alfabéticamente en empate)[cite: 1]
        public void OrdenarPorInsercionPrestamos()
        {
            int n = catalogo.Count;
            for (int i = 1; i < n; i++)
            {
                Titulo key = catalogo[i];
                int j = i - 1;

                // Criterio 1: Mayor cantidad de préstamos históricos primero (>).
                // Criterio 2 (Estabilidad por empate): Si tienen los mismos préstamos, ordenar alfabéticamente por Nombre del título (Ascendente: key.Nombre < catalogo[j].Nombre).
                while (j >= 0 && (
                    catalogo[j].PrestamosHistoricos < key.PrestamosHistoricos ||
                    (catalogo[j].PrestamosHistoricos == key.PrestamosHistoricos && catalogo[j].Nombre.CompareTo(key.Nombre) > 0)
                ))
                {
                    catalogo[j + 1] = catalogo[j];
                    j--;
                }
                catalogo[j + 1] = key;
            }
        }

        // Ordenamiento auxiliar por ISBN para la búsqueda binaria
        public List<Titulo> ObtenerCopiaOrdenadaPorIsbn(out List<Titulo> copia)
        {
            copia = new List<Titulo>(catalogo);
            int n = copia.Count;
            for (int i = 1; i < n; i++)
            {
                Titulo key = copia[i];
                int j = i - 1;
                while (j >= 0 && copia[j].Isbn.CompareTo(key.Isbn) > 0)
                {
                    copia[j + 1] = copia[j];
                    j--;
                }
                copia[j + 1] = key;
            }
            return copia;
        }

        // Requisito: Búsqueda binaria propia por ISBN[cite: 1]
        public Titulo BuscarBinarioPorIsbn(List<Titulo> listaOrdenada, string isbnBusqueda, out int comparaciones)
        {
            comparaciones = 0;
            int izquierda = 0;
            int derecha = listaOrdenada.Count - 1;

            while (izquierda <= derecha)
            {
                comparaciones++;
                int medio = izquierda + (derecha - izquierda) / 2;
                int comparacion = string.Compare(listaOrdenada[medio].Isbn, isbnBusqueda, StringComparison.Ordinal);

                if (comparacion == 0)
                {
                    return listaOrdenada[medio];
                }
                else if (comparacion < 0)
                {
                    izquierda = medio + 1;
                }
                else
                {
                    derecha = medio - 1;
                }
            }
            return null; // No encontrado
        }

        // Requisito: Registrar un préstamo que reduzca la disponibilidad y actualice el contador histórico[cite: 1]
        public bool RegistrarPrestamo(string isbn)
        {
            Titulo titulo = catalogo.Find(t => t.Isbn == isbn);
            if (titulo == null)
            {
                Console.WriteLine("Error: El libro con ISBN especificado no existe en el catálogo.");
                return false;
            }

            // Buscar un ejemplar disponible
            Ejemplar ejemplarDisponible = titulo.Ejemplares.Find(e => e.Disponible);
            if (ejemplarDisponible == null)
            {
                Console.WriteLine($"Lo siento, no quedan ejemplares disponibles del título: '{titulo.Nombre}'.");
                return false;
            }

            ejemplarDisponible.Disponible = false;
            titulo.PrestamosHistoricos++;
            Console.WriteLine($"¡Préstamo exitoso! Se prestó el ejemplar #{ejemplarDisponible.IdEjemplar} de '{titulo.Nombre}'.");
            return true;
        }

        public void MostrarCatalogo()
        {
            Console.WriteLine($"{"ISBN",-20} | {"Título",-35} | {"Préstamos",-10} | {"Disponibles/Total",-18}");
            Console.WriteLine(new string('-', 90));
            foreach (var t in catalogo)
            {
                Console.WriteLine($"{t.Isbn,-20} | {t.Nombre,-35} | {t.PrestamosHistoricos,-10} | {t.EjemplaresDisponibles} / {t.Ejemplares.Count,-14}");
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Biblioteca biblio = new Biblioteca();

            Console.WriteLine("=== CATÁLOGO INICIAL (Sin ordenar) ===");
            biblio.MostrarCatalogo();

            // Ordenar por inserción (Préstamos Descendente + Estabilidad Alfabética)
            biblio.OrdenarPorInsercionPrestamos();
            Console.WriteLine("\n=== CATÁLOGO ORDENADO POR PRÉSTAMOS (Mayor a Menor) ===");
            biblio.MostrarCatalogo();

            Console.WriteLine("\n=== SIMULACIÓN DE 3 PRÉSTAMOS (Día 3 y 6) ===");
            biblio.RegistrarPrestamo("978-843-76-0494-7"); // Cien años de soledad
            biblio.RegistrarPrestamo("978-950-07-2051-9"); // El túnel (ejemplar único)
            biblio.RegistrarPrestamo("978-950-07-2051-9"); // Segundo intento en El túnel (Debe fallar por no haber copias)

            Console.WriteLine("\n=== CATÁLOGO DESPUÉS DE LOS PRÉSTAMOS ===");
            biblio.MostrarCatalogo();
        }
    }
}