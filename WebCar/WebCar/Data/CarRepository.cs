using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using WebCar.Models;

namespace WebCar.Data
{
    public class CarRepository
    {
        private readonly string _connectionString;

        public CarRepository()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["CARSALE_DB"].ConnectionString;
        }

        // =========================================
        // Get All Cars with Filters
        // =========================================
        public List<CAR> GetAllCars(string searchTerm = null, string brand = null,
            decimal? minPrice = null, decimal? maxPrice = null, short? year = null)
        {
            var cars = new List<CAR>();

            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("SP_GET_ALL_CARS", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@SEARCH", (object)searchTerm ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@HANGXE", (object)brand ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@MIN_PRICE", (object)minPrice ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@MAX_PRICE", (object)maxPrice ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@NAMSX", (object)year ?? DBNull.Value);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cars.Add(MapCarFromReader(reader));
                        }
                    }
                }
            }

            return cars;
        }

        // =========================================
        // Get Car by ID
        // =========================================
        public CAR GetCarById(int carId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("SP_GET_CAR_BY_ID", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@MAXE", carId);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapCarFromReader(reader);
                        }
                    }
                }
            }

            return null;
        }

        // =========================================
        // Get Brands
        // =========================================
        public List<string> GetBrands()
        {
            var brands = new List<string>();

            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("SP_GET_BRANDS", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var brand = reader.IsDBNull(0) ? "" : reader.GetString(0);
                            if (!string.IsNullOrEmpty(brand))
                            {
                                brands.Add(brand);
                            }
                        }
                    }
                }
            }

            return brands;
        }

        // =========================================
        // Get Related Cars
        // =========================================
        public List<CAR> GetRelatedCars(int carId, string brand, int limit = 4)
        {
            var cars = new List<CAR>();

            using (var conn = new SqlConnection(_connectionString))
            {
                var sql = @"
                    SELECT TOP (@Limit) 
                        MAXE, TENXE, HANGXE, GIA, NAMSX, MOTA, HINHANH, TRANGTHAI
                    FROM CAR
                    WHERE HANGXE = @Brand 
                    AND MAXE != @CarId 
                    AND TRANGTHAI = N'Còn hàng'
                    ORDER BY MAXE DESC";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Brand", brand);
                    cmd.Parameters.AddWithValue("@CarId", carId);
                    cmd.Parameters.AddWithValue("@Limit", limit);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cars.Add(MapCarFromReader(reader));
                        }
                    }
                }
            }

            return cars;
        }

        // =========================================
        // ✅ NEW: Get Total Cars Count
        // =========================================
        public int GetTotalCars()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = new SqlCommand("SELECT COUNT(*) FROM CAR", conn);
                conn.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        // =========================================
        // Helper: Map CAR from DataReader
        // =========================================
        private CAR MapCarFromReader(SqlDataReader reader)
        {
            return new CAR
            {
                MAXE = reader.GetInt32(reader.GetOrdinal("MAXE")),
                TENXE = reader.IsDBNull(reader.GetOrdinal("TENXE")) ? "" : reader.GetString(reader.GetOrdinal("TENXE")),
                HANGXE = reader.IsDBNull(reader.GetOrdinal("HANGXE")) ? "" : reader.GetString(reader.GetOrdinal("HANGXE")),
                GIA = reader.IsDBNull(reader.GetOrdinal("GIA")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("GIA")),
                NAMSX = reader.IsDBNull(reader.GetOrdinal("NAMSX")) ? (short?)null : (short)reader.GetInt32(reader.GetOrdinal("NAMSX")),
                MOTA = reader.IsDBNull(reader.GetOrdinal("MOTA")) ? "" : reader.GetString(reader.GetOrdinal("MOTA")),
                HINHANH = reader.IsDBNull(reader.GetOrdinal("HINHANH")) ? "" : reader.GetString(reader.GetOrdinal("HINHANH")),
                TRANGTHAI = reader.IsDBNull(reader.GetOrdinal("TRANGTHAI")) ? "Còn hàng" : reader.GetString(reader.GetOrdinal("TRANGTHAI"))
            };
        }
        // =========================================
        // Create Car
        // =========================================
        public (bool Success, string Message) CreateCar(CAR car)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    var sql = @"
                INSERT INTO CAR (TENXE, HANGXE, GIA, NAMSX, MOTA, HINHANH, TRANGTHAI)
                VALUES (@TENXE, @HANGXE, @GIA, @NAMSX, @MOTA, @HINHANH, @TRANGTHAI)";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@TENXE", car.TENXE);
                        cmd.Parameters.AddWithValue("@HANGXE", car.HANGXE ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@GIA", car.GIA ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@NAMSX", car.NAMSX ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@MOTA", car.MOTA ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@HINHANH", car.HINHANH ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@TRANGTHAI", car.TRANGTHAI ?? "Còn hàng");

                        conn.Open();
                        int rows = cmd.ExecuteNonQuery();

                        if (rows > 0)
                            return (true, "Thêm xe thành công");
                        else
                            return (false, "Không thể thêm xe");
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, "Lỗi: " + ex.Message);
            }
        }

        // =========================================
        // Update Car
        // =========================================
        public (bool Success, string Message) UpdateCar(CAR car)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    var sql = @"
                UPDATE CAR 
                SET TENXE = @TENXE, 
                    HANGXE = @HANGXE, 
                    GIA = @GIA, 
                    NAMSX = @NAMSX, 
                    MOTA = @MOTA, 
                    HINHANH = @HINHANH, 
                    TRANGTHAI = @TRANGTHAI
                WHERE MAXE = @MAXE";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@MAXE", car.MAXE);
                        cmd.Parameters.AddWithValue("@TENXE", car.TENXE);
                        cmd.Parameters.AddWithValue("@HANGXE", car.HANGXE ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@GIA", car.GIA ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@NAMSX", car.NAMSX ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@MOTA", car.MOTA ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@HINHANH", car.HINHANH ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@TRANGTHAI", car.TRANGTHAI ?? "Còn hàng");

                        conn.Open();
                        int rows = cmd.ExecuteNonQuery();

                        if (rows > 0)
                            return (true, "Cập nhật thành công");
                        else
                            return (false, "Không tìm thấy xe");
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, "Lỗi: " + ex.Message);
            }
        }

        // =========================================
        // Delete Car
        // =========================================
        public (bool Success, string Message) DeleteCar(int carId)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    var sql = "DELETE FROM CAR WHERE MAXE = @MAXE";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@MAXE", carId);

                        conn.Open();
                        int rows = cmd.ExecuteNonQuery();

                        if (rows > 0)
                            return (true, "Xóa xe thành công");
                        else
                            return (false, "Không tìm thấy xe");
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, "Lỗi: " + ex.Message);
            }
        }
    }
}