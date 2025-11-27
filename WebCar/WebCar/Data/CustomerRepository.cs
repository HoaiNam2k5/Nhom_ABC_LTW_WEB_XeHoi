using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using WebCar.Models;
using WebCar.Models.ViewModels;

namespace WebCar.Data
{
    public class CustomerRepository
    {
        private readonly string _connectionString;

        public CustomerRepository()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["CARSALE_DB"].ConnectionString;
        }

        // =========================================
        // Register
        // =========================================
        public (bool Success, string Message, int? CustomerId) Register(RegisterViewModel model)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("SP_REGISTER_CUSTOMER", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@HOTEN", model.HoTen);
                    cmd.Parameters.AddWithValue("@EMAIL", model.Email);
                    cmd.Parameters.AddWithValue("@SDT", model.SDT);
                    cmd.Parameters.AddWithValue("@MATKHAU", model.Password);
                    cmd.Parameters.AddWithValue("@DIACHI", model.DiaChi ?? (object)DBNull.Value);

                    var resultParam = new SqlParameter("@RESULT", SqlDbType.Int) { Direction = ParameterDirection.Output };
                    var messageParam = new SqlParameter("@MESSAGE", SqlDbType.NVarChar, 200) { Direction = ParameterDirection.Output };
                    var makhParam = new SqlParameter("@MAKH", SqlDbType.Int) { Direction = ParameterDirection.Output };

                    cmd.Parameters.Add(resultParam);
                    cmd.Parameters.Add(messageParam);
                    cmd.Parameters.Add(makhParam);

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    int result = (int)resultParam.Value;
                    string message = messageParam.Value.ToString();
                    int? makh = makhParam.Value == DBNull.Value ? (int?)null : (int)makhParam.Value;

                    return (result == 1, message, makh);
                }
            }
        }

        // =========================================
        // Login
        // =========================================
        public (bool Success, string Message, CustomerViewModel Customer) Login(LoginViewModel model)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("SP_LOGIN_CUSTOMER", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@EMAIL", model.Email);
                    cmd.Parameters.AddWithValue("@MATKHAU", model.Password);
                    cmd.Parameters.AddWithValue("@IP", System.Web.HttpContext.Current?.Request.UserHostAddress ?? "Unknown");

                    var resultParam = new SqlParameter("@RESULT", SqlDbType.Int) { Direction = ParameterDirection.Output };
                    var messageParam = new SqlParameter("@MESSAGE", SqlDbType.NVarChar, 200) { Direction = ParameterDirection.Output };
                    var makhParam = new SqlParameter("@MAKH", SqlDbType.Int) { Direction = ParameterDirection.Output };
                    var hotenParam = new SqlParameter("@HOTEN", SqlDbType.NVarChar, 100) { Direction = ParameterDirection.Output };
                    var roleParam = new SqlParameter("@ROLENAME", SqlDbType.NVarChar, 30) { Direction = ParameterDirection.Output };

                    cmd.Parameters.Add(resultParam);
                    cmd.Parameters.Add(messageParam);
                    cmd.Parameters.Add(makhParam);
                    cmd.Parameters.Add(hotenParam);
                    cmd.Parameters.Add(roleParam);

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    int result = (int)resultParam.Value;
                    string message = messageParam.Value?.ToString() ?? "";

                    if (result == 1)
                    {
                        var customer = new CustomerViewModel
                        {
                            MaKH = makhParam.Value == DBNull.Value ? 0 : (int)makhParam.Value,
                            HoTen = hotenParam.Value?.ToString() ?? "",
                            Email = model.Email,
                            RoleName = roleParam.Value?.ToString() ?? "CUSTOMER"
                        };

                        return (true, message, customer);
                    }

                    return (false, message, null);
                }
            }
        }

        // =========================================
        // Logout
        // =========================================
        public (bool Success, string Message) Logout(int customerId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("SP_LOGOUT_CUSTOMER", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@MAKH", customerId);
                    cmd.Parameters.AddWithValue("@IP", System.Web.HttpContext.Current?.Request.UserHostAddress ?? "Unknown");

                    var resultParam = new SqlParameter("@RESULT", SqlDbType.Int) { Direction = ParameterDirection.Output };
                    var messageParam = new SqlParameter("@MESSAGE", SqlDbType.NVarChar, 200) { Direction = ParameterDirection.Output };

                    cmd.Parameters.Add(resultParam);
                    cmd.Parameters.Add(messageParam);

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    int result = (int)resultParam.Value;
                    string message = messageParam.Value.ToString();

                    return (result == 1, message);
                }
            }
        }

        // =========================================
        // Get Customer By Id
        // =========================================
        public CUSTOMER GetCustomerById(int customerId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = new SqlCommand("SELECT MAKH, HOTEN, EMAIL, SDT, DIACHI, NGAYDANGKY FROM CUSTOMER WHERE MAKH = @MAKH", conn);
                cmd.Parameters.AddWithValue("@MAKH", customerId);

                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new CUSTOMER
                        {
                            MAKH = reader.GetInt32(0),
                            HOTEN = reader.GetString(1),
                            EMAIL = reader.GetString(2),
                            SDT = reader.IsDBNull(3) ? "" : reader.GetString(3),
                            DIACHI = reader.IsDBNull(4) ? "" : reader.GetString(4),
                            NGAYDANGKY = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5)
                        };
                    }
                }
            }

            return null;
        }

        // =========================================
        // Update Customer
        // =========================================
        public (bool Success, string Message) UpdateCustomer(CUSTOMER customer)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    var cmd = new SqlCommand(@"
                        UPDATE CUSTOMER 
                        SET HOTEN = @HOTEN, SDT = @SDT, DIACHI = @DIACHI
                        WHERE MAKH = @MAKH", conn);

                    cmd.Parameters.AddWithValue("@HOTEN", customer.HOTEN);
                    cmd.Parameters.AddWithValue("@SDT", customer.SDT ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@DIACHI", customer.DIACHI ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@MAKH", customer.MAKH);

                    conn.Open();
                    int rows = cmd.ExecuteNonQuery();

                    if (rows > 0)
                        return (true, "Cập nhật thành công");
                    else
                        return (false, "Không tìm thấy tài khoản");
                }
            }
            catch (Exception ex)
            {
                return (false, "Lỗi: " + ex.Message);
            }
        }

        // =========================================
        // Change Password
        // =========================================
        public (bool Success, string Message) ChangePassword(int customerId, string oldPassword, string newPassword)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    // Verify old password
                    var checkCmd = new SqlCommand(@"
                        SELECT COUNT(*) FROM CUSTOMER 
                        WHERE MAKH = @MAKH 
                        AND MATKHAU = CONVERT(NVARCHAR(256), HASHBYTES('SHA2_256', @OldPassword), 2)", conn);

                    checkCmd.Parameters.AddWithValue("@MAKH", customerId);
                    checkCmd.Parameters.AddWithValue("@OldPassword", oldPassword);

                    conn.Open();
                    int count = (int)checkCmd.ExecuteScalar();

                    if (count == 0)
                    {
                        return (false, "Mật khẩu cũ không đúng");
                    }

                    // Update new password
                    var updateCmd = new SqlCommand(@"
                        UPDATE CUSTOMER 
                        SET MATKHAU = CONVERT(NVARCHAR(256), HASHBYTES('SHA2_256', @NewPassword), 2)
                        WHERE MAKH = @MAKH", conn);

                    updateCmd.Parameters.AddWithValue("@NewPassword", newPassword);
                    updateCmd.Parameters.AddWithValue("@MAKH", customerId);

                    int rows = updateCmd.ExecuteNonQuery();

                    if (rows > 0)
                        return (true, "Đổi mật khẩu thành công");
                    else
                        return (false, "Không thể đổi mật khẩu");
                }
            }
            catch (Exception ex)
            {
                return (false, "Lỗi: " + ex.Message);
            }
        }

        // =========================================
        // ✅ NEW: Get Total Customers
        // =========================================
        public int GetTotalCustomers()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = new SqlCommand("SELECT COUNT(*) FROM CUSTOMER", conn);
                conn.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        // =========================================
        // ✅ NEW: Get All Customers (with search)
        // =========================================
        public List<CUSTOMER> GetAllCustomers(string search = null)
        {
            var customers = new List<CUSTOMER>();

            using (var conn = new SqlConnection(_connectionString))
            {
                var sql = @"
                    SELECT MAKH, HOTEN, EMAIL, SDT, DIACHI, NGAYDANGKY
                    FROM CUSTOMER
                    WHERE 1=1";

                if (!string.IsNullOrEmpty(search))
                    sql += " AND (HOTEN LIKE @Search OR EMAIL LIKE @Search OR SDT LIKE @Search)";

                sql += " ORDER BY NGAYDANGKY DESC";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    if (!string.IsNullOrEmpty(search))
                        cmd.Parameters.AddWithValue("@Search", "%" + search + "%");

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            customers.Add(new CUSTOMER
                            {
                                MAKH = reader.GetInt32(0),
                                HOTEN = reader.GetString(1),
                                EMAIL = reader.GetString(2),
                                SDT = reader.IsDBNull(3) ? "" : reader.GetString(3),
                                DIACHI = reader.IsDBNull(4) ? "" : reader.GetString(4),
                                NGAYDANGKY = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5)
                            });
                        }
                    }
                }
            }

            return customers;
        }

        // =========================================
        // ✅ NEW: Get All Customers With Roles
        // =========================================
        public List<CustomerWithRole> GetAllCustomersWithRoles()
        {
            var customers = new List<CustomerWithRole>();

            using (var conn = new SqlConnection(_connectionString))
            {
                var sql = @"
                    SELECT c.MAKH, c. HOTEN, c.EMAIL, c.SDT, c.NGAYDANGKY, ar.ROLENAME
                    FROM CUSTOMER c
                    LEFT JOIN ACCOUNT_ROLE ar ON c. MAKH = ar.MAKH
                    ORDER BY c. NGAYDANGKY DESC";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            customers.Add(new CustomerWithRole
                            {
                                MAKH = reader.GetInt32(0),
                                HOTEN = reader.GetString(1),
                                EMAIL = reader.GetString(2),
                                SDT = reader.IsDBNull(3) ? "" : reader.GetString(3),
                                NGAYDANGKY = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
                                ROLENAME = reader.IsDBNull(5) ? "CUSTOMER" : reader.GetString(5)
                            });
                        }
                    }
                }
            }

            return customers;
        }

        // =========================================
        // ✅ NEW: Security Stats
        // =========================================
        public int GetFailedLoginCount()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = new SqlCommand(@"
                    SELECT COUNT(*) FROM AUDIT_LOG 
                    WHERE HANHDONG LIKE '%FAILED%' OR HANHDONG LIKE '%ACCESS_DENIED%'
                    AND NGAYGIO >= DATEADD(DAY, -7, GETDATE())", conn);

                conn.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        public int GetActiveSessionCount()
        {
            // Placeholder - implement session tracking if needed
            return 0;
        }
    }

    // =========================================
    // Helper Class
    // =========================================
    public class CustomerWithRole
    {
        public int MAKH { get; set; }
        public string HOTEN { get; set; }
        public string EMAIL { get; set; }
        public string SDT { get; set; }
        public DateTime? NGAYDANGKY { get; set; }
        public string ROLENAME { get; set; }
    }
}