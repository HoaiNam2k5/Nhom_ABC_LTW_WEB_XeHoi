using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using WebCar.Models;

namespace WebCar.Data
{
    public class OrderRepository
    {
        private readonly string _connectionString;

        public OrderRepository()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["CARSALE_DB"].ConnectionString;
        }

        // =========================================
        // Create Order (Using SP_CREATE_ORDER)
        // =========================================
        public (bool Success, string Message, int? OrderId) CreateOrder(int customerId, int carId, int quantity, string diachi, string sdt, string ghichu)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("SP_CREATE_ORDER", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@MAKH", customerId);
                    cmd.Parameters.AddWithValue("@MAXE", carId);
                    cmd.Parameters.AddWithValue("@SOLUONG", quantity);

                    var resultParam = new SqlParameter("@RESULT", SqlDbType.Int) { Direction = ParameterDirection.Output };
                    var messageParam = new SqlParameter("@MESSAGE", SqlDbType.NVarChar, 200) { Direction = ParameterDirection.Output };
                    var orderIdParam = new SqlParameter("@MADON", SqlDbType.Int) { Direction = ParameterDirection.Output };

                    cmd.Parameters.Add(resultParam);
                    cmd.Parameters.Add(messageParam);
                    cmd.Parameters.Add(orderIdParam);

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    int result = (int)resultParam.Value;
                    string message = messageParam.Value.ToString();
                    int? orderId = orderIdParam.Value == DBNull.Value ? (int?)null : (int)orderIdParam.Value;

                    return (result == 1, message, orderId);
                }
            }
        }
        // =========================================
        // Get Order By ID
        // =========================================
        public ORDER GetOrderById(int orderId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var sql = @"
            SELECT 
                o.MADON, o. MAKH, o.NGAYDAT, o.TONGTIEN, o.TRANGTHAI,
                c.HOTEN, c.EMAIL
            FROM ORDERS o
            INNER JOIN CUSTOMER c ON o.MAKH = c.MAKH
            WHERE o.MADON = @MADON";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@MADON", orderId);

                    conn.Open();

                    // ✅ DEBUG
                    System.Diagnostics.Debug.WriteLine($"Executing GetOrderById for OrderId: {orderId}");

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var order = new ORDER
                            {
                                MADON = reader.GetInt32(0),
                                MAKH = reader.GetInt32(1),
                                NGAYDAT = reader.IsDBNull(2) ? (DateTime?)null : reader.GetDateTime(2),
                                TONGTIEN = reader.IsDBNull(3) ? (decimal?)null : reader.GetDecimal(3),
                                TRANGTHAI = reader.IsDBNull(4) ? "" : reader.GetString(4),
                                CustomerName = reader.IsDBNull(5) ? "" : reader.GetString(5),
                                CustomerEmail = reader.IsDBNull(6) ? "" : reader.GetString(6)
                            };

                            // ✅ DEBUG
                            System.Diagnostics.Debug.WriteLine($"Order found: {order.MADON}, Customer: {order.CustomerName}");

                            return order;
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"No order found for OrderId: {orderId}");
                        }
                    }
                }
            }

            return null;
        }

        // =========================================
        // Get Order Details
        // =========================================
        public List<ORDER_DETAIL> GetOrderDetails(int orderId)
        {
            var details = new List<ORDER_DETAIL>();

            using (var conn = new SqlConnection(_connectionString))
            {
                var sql = @"
            SELECT 
                od.MADON, od. MAXE, od.SOLUONG, od.DONGIA,
                c.TENXE, c. HANGXE, c. HINHANH
            FROM ORDER_DETAIL od
            INNER JOIN CAR c ON od.MAXE = c.MAXE
            WHERE od.MADON = @MADON";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@MADON", orderId);

                    conn.Open();

                    // ✅ DEBUG
                    System.Diagnostics.Debug.WriteLine($"Executing GetOrderDetails for OrderId: {orderId}");

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var detail = new ORDER_DETAIL
                            {
                                MADON = reader.GetInt32(0),
                                MAXE = reader.GetInt32(1),
                                SOLUONG = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2),
                                DONGIA = reader.IsDBNull(3) ? (decimal?)null : reader.GetDecimal(3),
                                TENXE = reader.IsDBNull(4) ? "" : reader.GetString(4),
                                HANGXE = reader.IsDBNull(5) ? "" : reader.GetString(5),
                                HINHANH = reader.IsDBNull(6) ? "" : reader.GetString(6)
                            };

                            details.Add(detail);

                            // ✅ DEBUG
                            System.Diagnostics.Debug.WriteLine($"Detail: {detail.TENXE}, Qty: {detail.SOLUONG}, Price: {detail.DONGIA}");
                        }
                    }

                    System.Diagnostics.Debug.WriteLine($"Total details found: {details.Count}");
                }
            }

            return details;
        }

        // =========================================
        // Get Orders By Customer (Using SP)
        // =========================================
        public List<OrderViewModel> GetOrdersByCustomer(int customerId)
        {
            var orders = new List<OrderViewModel>();

            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("SP_GET_MY_ORDERS", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@MAKH", customerId);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            orders.Add(new OrderViewModel
                            {
                                MADON = reader.GetInt32(0),
                                NGAYDAT = reader.IsDBNull(1) ? (DateTime?)null : reader.GetDateTime(1),
                                TONGTIEN = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2),
                                TRANGTHAI = reader.IsDBNull(3) ? "" : reader.GetString(3),
                                MAXE = reader.GetInt32(4),
                                TENXE = reader.IsDBNull(5) ? "" : reader.GetString(5),
                                HANGXE = reader.IsDBNull(6) ? "" : reader.GetString(6),
                                SOLUONG = reader.IsDBNull(7) ? 0 : reader.GetInt32(7),
                                DONGIA = reader.IsDBNull(8) ? 0 : reader.GetDecimal(8)
                            });
                        }
                    }
                }
            }

            return orders;
        }

        // =========================================
        // Get All Orders (Admin) ✅ THIS IS THE MISSING METHOD
        // =========================================
        public List<OrderViewModel> GetAllOrders(string status = null)
        {
            var orders = new List<OrderViewModel>();

            using (var conn = new SqlConnection(_connectionString))
            {
                var sql = @"
                    SELECT 
                        o. MADON, 
                        o.NGAYDAT, 
                        o.TONGTIEN, 
                        o. TRANGTHAI,
                        od.MAXE, 
                        c.TENXE, 
                        c. HANGXE, 
                        od.SOLUONG, 
                        od.DONGIA,
                        cu.HOTEN, 
                        cu. EMAIL
                    FROM ORDERS o
                    INNER JOIN ORDER_DETAIL od ON o.MADON = od.MADON
                    INNER JOIN CAR c ON od.MAXE = c.MAXE
                    INNER JOIN CUSTOMER cu ON o. MAKH = cu.MAKH
                    WHERE 1=1";

                if (!string.IsNullOrEmpty(status))
                    sql += " AND o.TRANGTHAI = @Status";

                sql += " ORDER BY o.NGAYDAT DESC";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    if (!string.IsNullOrEmpty(status))
                        cmd.Parameters.AddWithValue("@Status", status);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            orders.Add(new OrderViewModel
                            {
                                MADON = reader.GetInt32(0),
                                NGAYDAT = reader.IsDBNull(1) ? (DateTime?)null : reader.GetDateTime(1),
                                TONGTIEN = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2),
                                TRANGTHAI = reader.IsDBNull(3) ? "" : reader.GetString(3),
                                MAXE = reader.GetInt32(4),
                                TENXE = reader.IsDBNull(5) ? "" : reader.GetString(5),
                                HANGXE = reader.IsDBNull(6) ? "" : reader.GetString(6),
                                SOLUONG = reader.IsDBNull(7) ? 0 : reader.GetInt32(7),
                                DONGIA = reader.IsDBNull(8) ? 0 : reader.GetDecimal(8),
                                CustomerName = reader.IsDBNull(9) ? "" : reader.GetString(9),
                                CustomerEmail = reader.IsDBNull(10) ? "" : reader.GetString(10)
                            });
                        }
                    }
                }
            }

            return orders;
        }

        // =========================================
        // Get Recent Orders
        // =========================================
        public List<OrderViewModel> GetRecentOrders(int count)
        {
            var orders = new List<OrderViewModel>();

            using (var conn = new SqlConnection(_connectionString))
            {
                var sql = $@"
                    SELECT TOP {count}
                        o.MADON, o.NGAYDAT, o. TONGTIEN, o. TRANGTHAI,
                        od.MAXE, c. TENXE, c.HANGXE, od.SOLUONG, od.DONGIA,
                        cu.HOTEN, cu.EMAIL
                    FROM ORDERS o
                    INNER JOIN ORDER_DETAIL od ON o.MADON = od. MADON
                    INNER JOIN CAR c ON od.MAXE = c.MAXE
                    INNER JOIN CUSTOMER cu ON o.MAKH = cu.MAKH
                    ORDER BY o.NGAYDAT DESC";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            orders.Add(new OrderViewModel
                            {
                                MADON = reader.GetInt32(0),
                                NGAYDAT = reader.IsDBNull(1) ? (DateTime?)null : reader.GetDateTime(1),
                                TONGTIEN = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2),
                                TRANGTHAI = reader.IsDBNull(3) ? "" : reader.GetString(3),
                                MAXE = reader.GetInt32(4),
                                TENXE = reader.IsDBNull(5) ? "" : reader.GetString(5),
                                HANGXE = reader.IsDBNull(6) ? "" : reader.GetString(6),
                                SOLUONG = reader.IsDBNull(7) ? 0 : reader.GetInt32(7),
                                DONGIA = reader.IsDBNull(8) ? 0 : reader.GetDecimal(8),
                                CustomerName = reader.IsDBNull(9) ? "" : reader.GetString(9),
                                CustomerEmail = reader.IsDBNull(10) ? "" : reader.GetString(10)
                            });
                        }
                    }
                }
            }

            return orders;
        }

        // =========================================
        // Update Order Status
        // =========================================
        public (bool Success, string Message) UpdateOrderStatus(int orderId, string newStatus)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    var sql = "UPDATE ORDERS SET TRANGTHAI = @Status WHERE MADON = @MADON";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Status", newStatus);
                        cmd.Parameters.AddWithValue("@MADON", orderId);

                        conn.Open();
                        int rows = cmd.ExecuteNonQuery();

                        if (rows > 0)
                            return (true, "Cập nhật trạng thái thành công");
                        else
                            return (false, "Không tìm thấy đơn hàng");
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, "Lỗi: " + ex.Message);
            }
        }

        // =========================================
        // Cancel Order
        // =========================================
        public (bool Success, string Message) CancelOrder(int orderId, int customerId)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    var sql = @"
                        UPDATE ORDERS 
                        SET TRANGTHAI = N'Đã hủy' 
                        WHERE MADON = @MADON 
                        AND MAKH = @MAKH 
                        AND TRANGTHAI IN (N'Chờ xử lý', N'Đang xử lý')";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@MADON", orderId);
                        cmd.Parameters.AddWithValue("@MAKH", customerId);

                        conn.Open();
                        int rows = cmd.ExecuteNonQuery();

                        if (rows > 0)
                            return (true, "Hủy đơn hàng thành công");
                        else
                            return (false, "Không thể hủy đơn hàng này");
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, "Lỗi: " + ex.Message);
            }
        }

        // =========================================
        // Get Statistics
        // =========================================
        public int GetTotalOrders()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = new SqlCommand("SELECT COUNT(*) FROM ORDERS", conn);
                conn.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        public decimal GetTotalRevenue()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = new SqlCommand("SELECT ISNULL(SUM(TONGTIEN), 0) FROM ORDERS WHERE TRANGTHAI != N'Đã hủy'", conn);
                conn.Open();
                return (decimal)cmd.ExecuteScalar();
            }
        }

        public int GetOrderCountByStatus(string status)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var sql = "SELECT COUNT(*) FROM ORDERS WHERE TRANGTHAI = @Status";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Status", status);
                    conn.Open();
                    return (int)cmd.ExecuteScalar();
                }
            }
        }
    }

    // =========================================
    // ViewModel for displaying orders
    // =========================================
    public class OrderViewModel
    {
        public int MADON { get; set; }
        public DateTime? NGAYDAT { get; set; }
        public decimal TONGTIEN { get; set; }
        public string TRANGTHAI { get; set; }
        public int MAXE { get; set; }
        public string TENXE { get; set; }
        public string HANGXE { get; set; }
        public int SOLUONG { get; set; }
        public decimal DONGIA { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }

        public string StatusBadgeClass
        {
            get
            {
                if (TRANGTHAI == "Chờ xử lý") return "badge bg-warning";
                if (TRANGTHAI == "Đang xử lý") return "badge bg-info";
                if (TRANGTHAI == "Đã giao") return "badge bg-success";
                if (TRANGTHAI == "Đã hủy") return "badge bg-danger";
                return "badge bg-secondary";
            }
        }

        public string FormattedDate => NGAYDAT?.ToString("dd/MM/yyyy HH:mm") ?? "N/A";
    }
}