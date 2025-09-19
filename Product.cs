using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab05
{
    public class Product
    {
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; }
        public string CantidadPorUnidad { get; set; }
        public decimal PrecioUnidad { get; set; }
        public int UnidadesEnExistencia { get; set; }
        public string NombreCategoria { get; set; }
        public string NombreProveedor { get; set; }
    }
}
