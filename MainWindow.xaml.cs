using Microsoft.Data.SqlClient;
using System.Text;
using System.Windows;
using System;
using System.Collections.Generic;


namespace Lab05
{
    public partial class MainWindow : Window
    {
        
        private string connectionString = "Data Source=DESKTOP-E6C62SS\\SQLEXPRESS;Initial Catalog=Neptuno;User Id=userTecsup;Password=120506;TrustServerCertificate=true";

        // OPCIÓN 2: Si prefieres autenticación de Windows (comenta la línea de arriba y usa esta)
        // private string connectionString = "Data Source=DESKTOP-E6C62SS\\SQLEXPRESS01;Initial Catalog=Neptuno;Integrated Security=true;TrustServerCertificate=true";

        public MainWindow()
        {
            InitializeComponent();
            // Probar conexión al iniciar
            TestConnection();
        }

        #region Métodos de Conexión

        /// <summary>
        /// Prueba la conexión a la base de datos al iniciar la aplicación
        /// </summary>
        private void TestConnection()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    MessageBox.Show("✅ Conexión exitosa a la base de datos Neptuno",
                                    "Conexión", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Error de conexión: {ex.Message}\n\n" +
                               "Verifica:\n" +
                               "1. Que SQL Server esté ejecutándose\n" +
                               "2. Que la base de datos 'Neptuno' exista\n" +
                               "3. La cadena de conexión en el código",
                               "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region PUNTO 7 - DataGrid Pedidos por Fecha (USP_DetallesPedidosPorFecha)

        /// <summary>
        /// Busca detalles de pedidos por rango de fechas usando USP_DetallesPedidosPorFecha
        /// Usa DataReader para leer los datos con parámetros de fecha
        /// </summary>
        private void btnBuscarPedidos_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validar que las fechas estén seleccionadas
                if (!dpFechaInicio.SelectedDate.HasValue || !dpFechaFin.SelectedDate.HasValue)
                {
                    MessageBox.Show("⚠️ Por favor selecciona ambas fechas (Inicio y Fin)",
                                   "Fechas Requeridas", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                DateTime fechaInicio = dpFechaInicio.SelectedDate.Value;
                DateTime fechaFin = dpFechaFin.SelectedDate.Value;

                // Validar que fecha inicio sea menor o igual a fecha fin
                if (fechaInicio > fechaFin)
                {
                    MessageBox.Show("⚠️ La fecha de inicio debe ser menor o igual a la fecha de fin",
                                   "Rango de Fechas Inválido", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                List<OrderDetail> pedidosDetalle = new List<OrderDetail>();
                decimal montoTotal = 0;
                int totalPedidosUnicos = 0;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Crear comando para ejecutar el stored procedure con parámetros de fecha
                    using (SqlCommand command = new SqlCommand("USP_DetallesPedidosPorFecha", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;

                        // Agregar parámetros de fecha al comando
                        command.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                        command.Parameters.AddWithValue("@FechaFin", fechaFin);

                        // Ejecutar y obtener DataReader
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            HashSet<int> pedidosUnicos = new HashSet<int>();

                            // Leer todos los registros con DataReader usando métodos seguros
                            while (reader.Read())
                            {
                                OrderDetail detalle = new OrderDetail
                                {
                                    IdPedido = Convert.ToInt32(reader["IdPedido"]),
                                    FechaPedido = Convert.ToDateTime(reader["FechaPedido"]),
                                    IdCliente = reader["IdCliente"]?.ToString() ?? "",
                                    NombreCliente = reader["NombreCliente"]?.ToString() ?? "",
                                    IdProducto = Convert.ToInt32(reader["idproducto"]),
                                    NombreProducto = reader["nombreProducto"]?.ToString() ?? "",
                                    PrecioUnidad = reader["preciounidad"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["preciounidad"]),
                                    Cantidad = Convert.ToInt32(reader["cantidad"]),
                                    Descuento = reader["descuento"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["descuento"]),
                                    SubTotal = reader["SubTotal"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["SubTotal"])
                                };

                                pedidosDetalle.Add(detalle);
                                montoTotal += detalle.SubTotal;
                                pedidosUnicos.Add(detalle.IdPedido);
                            }

                            totalPedidosUnicos = pedidosUnicos.Count;
                        }
                    }
                }

                // Asignar los datos al DataGrid
                dgPedidosDetalle.ItemsSource = pedidosDetalle;

                // Actualizar labels de resumen
                lblTotalPedidosTab.Text = $"Total de Pedidos: {totalPedidosUnicos}";
                lblTotalProductosTab.Text = $"Total de Productos: {pedidosDetalle.Count}";
                lblMontoTotalTab.Text = $"Monto Total: {montoTotal:C}";

                // Mostrar mensaje según el resultado
                if (pedidosDetalle.Count > 0)
                {
                    MessageBox.Show($"✅ Se encontraron {pedidosDetalle.Count} detalles de {totalPedidosUnicos} pedidos\n" +
                                   $"Período: {fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy}\n" +
                                   $"Monto total: {montoTotal:C}",
                                   "Búsqueda Completada", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"ℹ️ No se encontraron pedidos en el período seleccionado\n" +
                                   $"Período: {fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy}",
                                   "Sin Resultados", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Error al buscar pedidos por fecha: {ex.Message}",
                               "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Limpia los filtros de fecha y el DataGrid de pedidos
        /// </summary>
        private void btnLimpiarPedidos_Click(object sender, RoutedEventArgs e)
        {
            // Limpiar DatePickers (establecer fecha actual)
            dpFechaInicio.SelectedDate = DateTime.Now.AddMonths(-1); // Hace 1 mes
            dpFechaFin.SelectedDate = DateTime.Now;

            // Limpiar DataGrid
            dgPedidosDetalle.ItemsSource = null;

            // Limpiar labels de resumen
            lblTotalPedidosTab.Text = "";
            lblTotalProductosTab.Text = "";
            lblMontoTotalTab.Text = "";

            // Enfocar el primer campo
            dpFechaInicio.Focus();
        }

        #endregion

        #region PUNTO 8 - DataGrid Productos (relacionado con punto 3)

        /// <summary>
        /// Carga todos los productos usando el procedimiento USP_ListarProductos
        /// Usa DataReader para leer los datos
        /// </summary>
        private void btnCargarProductos_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<Product> productos = new List<Product>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Crear comando para ejecutar el stored procedure
                    using (SqlCommand command = new SqlCommand("USP_ListarProductos", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;

                        // Ejecutar y obtener DataReader
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            // Leer todos los registros con DataReader usando métodos seguros
                            while (reader.Read())
                            {
                                Product producto = new Product
                                {
                                    IdProducto = Convert.ToInt32(reader["idproducto"]),
                                    NombreProducto = reader["nombreProducto"]?.ToString() ?? "",
                                    CantidadPorUnidad = reader["cantidadPorUnidad"]?.ToString() ?? "",
                                    PrecioUnidad = reader["precioUnidad"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["precioUnidad"]),
                                    UnidadesEnExistencia = reader["unidadesEnExistencia"] == DBNull.Value ? 0 : Convert.ToInt32(reader["unidadesEnExistencia"]),
                                    NombreCategoria = reader["nombrecategoria"]?.ToString() ?? "",
                                    NombreProveedor = reader["NombreProveedor"]?.ToString() ?? ""
                                };

                                productos.Add(producto);
                            }
                        }
                    }
                }

                // Asignar los datos al DataGrid
                dgProductos.ItemsSource = productos;
                lblTotalProductos.Text = $"Total de productos: {productos.Count}";

                MessageBox.Show($"✅ Se cargaron {productos.Count} productos correctamente",
                               "Productos Cargados", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Error al cargar productos: {ex.Message}",
                               "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region PUNTO 9 - DataGrid Proveedores con Búsqueda (relacionado con punto 6)

        /// <summary>
        /// Busca proveedores usando el procedimiento USP_BuscarProveedores
        /// Usa DataReader para leer los datos con parámetros
        /// </summary>
        private void btnBuscarProveedores_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<Provider> proveedores = new List<Provider>();

                // Obtener valores de los TextBox
                string nombreContacto = txtNombreContacto.Text.Trim();
                string ciudad = txtCiudad.Text.Trim();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Crear comando para ejecutar el stored procedure con parámetros
                    using (SqlCommand command = new SqlCommand("USP_BuscarProveedores", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;

                        // Agregar parámetros al comando
                        command.Parameters.AddWithValue("@nombreContacto", nombreContacto);
                        command.Parameters.AddWithValue("@ciudad", ciudad);

                        // Ejecutar y obtener DataReader
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            // Leer todos los registros con DataReader usando métodos seguros
                            while (reader.Read())
                            {
                                Provider proveedor = new Provider
                                {
                                    IdProveedor = Convert.ToInt32(reader["idProveedor"]),
                                    NombreCompañia = reader["nombreCompañia"]?.ToString() ?? "",
                                    NombreContacto = reader["nombrecontacto"]?.ToString() ?? "",
                                    CargoContacto = reader["cargocontacto"]?.ToString() ?? "",
                                    Direccion = reader["direccion"]?.ToString() ?? "",
                                    Ciudad = reader["ciudad"]?.ToString() ?? "",
                                    Region = reader["region"]?.ToString() ?? "",
                                    Pais = reader["pais"]?.ToString() ?? "",
                                    Telefono = reader["telefono"]?.ToString() ?? ""
                                };

                                proveedores.Add(proveedor);
                            }
                        }
                    }
                }

                // Asignar los datos al DataGrid
                dgProveedores.ItemsSource = proveedores;
                lblResultadosProveedores.Text = $"Proveedores encontrados: {proveedores.Count}";

                // Mostrar mensaje según el resultado
                if (proveedores.Count > 0)
                {
                    MessageBox.Show($"✅ Se encontraron {proveedores.Count} proveedores",
                                   "Búsqueda Completada", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("ℹ️ No se encontraron proveedores con los criterios especificados",
                                   "Sin Resultados", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Error al buscar proveedores: {ex.Message}",
                               "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Limpia los campos de búsqueda y el DataGrid de proveedores
        /// </summary>
        private void btnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            // Limpiar campos de texto
            txtNombreContacto.Text = "";
            txtCiudad.Text = "";

            // Limpiar DataGrid
            dgProveedores.ItemsSource = null;
            lblResultadosProveedores.Text = "";

            // Enfocar el primer campo
            txtNombreContacto.Focus();
        }

        #endregion
    }
}