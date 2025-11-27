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
        // Get Total Orders
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

        // =========================================
        // Get Total Revenue
        // =========================================
        public decimal GetTotalRevenue()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = new SqlCommand("SELECT ISNULL(SUM(TONGTIEN), 0) FROM ORDERS WHERE TRANGTHAI != N'Đã hủy'", conn);
                conn.Open();
                return (decimal)cmd.ExecuteScalar();
            }
        }

        // =========================================
        // Get All Orders
        // =========================================
        public List<ORDER_VIEW> GetAllOrders(string status = null)
        {
            var orders = new List<ORDER_VIEW>();

            using (var conn = new SqlConnection(_connectionString))
            {
                var sql = @"
                    SELECT o. MADON, o.MAKH, o.NGAYDAT, o.TONGTIEN, o.TRANGTHAI,
                           c.HOTEN, c.EMAIL, c.SDT
                    FROM ORDERS o
                    INNER JOIN CUSTOMER c ON o. MAKH = c.MAKH
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
                            orders.Add(new ORDER_VIEW
                            {
                                MADON = reader.GetInt32(0),
                                MAKH = reader.GetInt32(1),
                                NGAYDAT = reader.GetDateTime(2),
                                TONGTIEN = reader.GetDecimal(3),
                                TRANGTHAI = reader.GetString(4),
                                CustomerName = reader.GetString(5),
                                CustomerEmail = reader.GetString(6),
                                CustomerPhone = reader.IsDBNull(7) ? "" : reader.GetString(7)
                            });
                        }
                    }
                }
            }

            return orders;
        }
    }
}