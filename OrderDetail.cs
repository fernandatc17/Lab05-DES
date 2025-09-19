using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab05
{
    public class OrderDetail
    {
        public int IdPedido { get; set; }
        public DateTime FechaPedido { get; set; }
        public string IdCliente { get; set; }
        public string NombreCliente { get; set; }
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; }
        public decimal PrecioUnidad { get; set; }
        public int Cantidad { get; set; }
        public decimal Descuento { get; set; }
        public decimal SubTotal { get; set; }
    }
}
