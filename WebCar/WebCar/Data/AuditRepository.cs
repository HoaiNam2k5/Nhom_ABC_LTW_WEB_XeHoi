using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using WebCar.Models;

namespace WebCar.Data
{
    public class AuditRepository
    {
        private readonly string _connectionString;

        public AuditRepository()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["CARSALE_DB"].ConnectionString;
        }

        // =========================================
        // Get Audit Logs with Filters
        // =========================================
        public List<AUDIT_LOG> GetAuditLogs(string action = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var logs = new List<AUDIT_LOG>();

            using (var conn = new SqlConnection(_connectionString))
            {
                var sql = @"
                    SELECT al. MALOG, al.MATK, al.HANHDONG, al.BANGTACDONG, al.NGAYGIO, al.IP,
                           c.HOTEN, c.EMAIL
                    FROM AUDIT_LOG al
                    LEFT JOIN CUSTOMER c ON al. MATK = c.MAKH
                    WHERE 1=1";

                if (!string.IsNullOrEmpty(action))
                    sql += " AND al.HANHDONG LIKE @Action";

                if (fromDate.HasValue)
                    sql += " AND al.NGAYGIO >= @FromDate";

                if (toDate.HasValue)
                    sql += " AND al.NGAYGIO <= @ToDate";

                sql += " ORDER BY al.NGAYGIO DESC";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    if (!string.IsNullOrEmpty(action))
                        cmd.Parameters.AddWithValue("@Action", "%" + action + "%");

                    if (fromDate.HasValue)
                        cmd.Parameters.AddWithValue("@FromDate", fromDate.Value);

                    if (toDate.HasValue)
                        cmd.Parameters.AddWithValue("@ToDate", toDate.Value.AddDays(1));

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            logs.Add(new AUDIT_LOG
                            {
                                MALOG = reader.GetInt32(0),
                                MATK = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                                HANHDONG = reader.GetString(2),
                                BANGTACDONG = reader.IsDBNull(3) ? "" : reader.GetString(3),
                                NGAYGIO = reader.GetDateTime(4),
                                IP = reader.IsDBNull(5) ? "" : reader.GetString(5),
                                CustomerName = reader.IsDBNull(6) ? "System" : reader.GetString(6),
                                CustomerEmail = reader.IsDBNull(7) ? "" : reader.GetString(7)
                            });
                        }
                    }
                }
            }

            return logs;
        }

        // =========================================
        // Get Unique Actions
        // =========================================
        public List<string> GetUniqueActions()
        {
            var actions = new List<string>();

            using (var conn = new SqlConnection(_connectionString))
            {
                var sql = "SELECT DISTINCT HANHDONG FROM AUDIT_LOG ORDER BY HANHDONG";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            actions.Add(reader.GetString(0));
                        }
                    }
                }
            }

            return actions;
        }

        // =========================================
        // Get Security Events
        // =========================================
        public List<AUDIT_LOG> GetSecurityEvents()
        {
            return GetAuditLogs().Where(x =>
                x.HANHDONG.Contains("LOGIN") ||
                x.HANHDONG.Contains("LOGOUT") ||
                x.HANHDONG.Contains("ACCESS_DENIED") ||
                x.HANHDONG.Contains("REGISTER")
            ).ToList();
        }

        // =========================================
        // Statistics
        // =========================================
        public int GetLoginCount()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = new SqlCommand("SELECT COUNT(*) FROM AUDIT_LOG WHERE HANHDONG = 'LOGIN'", conn);
                conn.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        public int GetFailedLoginCount()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = new SqlCommand("SELECT COUNT(*) FROM AUDIT_LOG WHERE HANHDONG LIKE '%ACCESS_DENIED%'", conn);
                conn.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        public int GetTotalActionCount()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = new SqlCommand("SELECT COUNT(*) FROM AUDIT_LOG", conn);
                conn.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        public int GetUniqueUserCount()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = new SqlCommand("SELECT COUNT(DISTINCT MATK) FROM AUDIT_LOG WHERE MATK > 0", conn);
                conn.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        // =========================================
        // Chart Data
        // =========================================
        public Dictionary<string, int> GetLoginChartData()
        {
            var data = new Dictionary<string, int>();

            using (var conn = new SqlConnection(_connectionString))
            {
                var sql = @"
                    SELECT CONVERT(DATE, NGAYGIO) AS DateOnly, COUNT(*) AS LoginCount
                    FROM AUDIT_LOG
                    WHERE HANHDONG = 'LOGIN' AND NGAYGIO >= DATEADD(DAY, -7, GETDATE())
                    GROUP BY CONVERT(DATE, NGAYGIO)
                    ORDER BY DateOnly";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var date = reader.GetDateTime(0).ToString("dd/MM");
                            var count = reader.GetInt32(1);
                            data[date] = count;
                        }
                    }
                }
            }

            return data;
        }

        public Dictionary<string, int> GetActionChartData()
        {
            var data = new Dictionary<string, int>();

            using (var conn = new SqlConnection(_connectionString))
            {
                var sql = @"
                    SELECT TOP 10 HANHDONG, COUNT(*) AS ActionCount
                    FROM AUDIT_LOG
                    GROUP BY HANHDONG
                    ORDER BY ActionCount DESC";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            data[reader.GetString(0)] = reader.GetInt32(1);
                        }
                    }
                }
            }

            return data;
        }
        // =========================================
        // Get User Activity Logs
        // =========================================
        public List<AUDIT_LOG> GetUserActivityLogs(int customerId)
        {
            var logs = new List<AUDIT_LOG>();

            using (var conn = new SqlConnection(_connectionString))
            {
                var sql = @"
            SELECT al. MALOG, al.MATK, al.HANHDONG, al.BANGTACDONG, al.NGAYGIO, al.IP,
                   c.HOTEN, c.EMAIL
            FROM AUDIT_LOG al
            LEFT JOIN CUSTOMER c ON al. MATK = c.MAKH
            WHERE al.MATK = @CustomerId
            ORDER BY al.NGAYGIO DESC";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@CustomerId", customerId);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            logs.Add(new AUDIT_LOG
                            {
                                MALOG = reader.GetInt32(0),
                                MATK = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                                HANHDONG = reader.GetString(2),
                                BANGTACDONG = reader.IsDBNull(3) ? "" : reader.GetString(3),
                                NGAYGIO = reader.GetDateTime(4),
                                IP = reader.IsDBNull(5) ? "" : reader.GetString(5),
                                CustomerName = reader.IsDBNull(6) ? "" : reader.GetString(6),
                                CustomerEmail = reader.IsDBNull(7) ? "" : reader.GetString(7)
                            });
                        }
                    }
                }
            }

            return logs;
        }

        // =========================================
        // Get User Activity Summary
        // =========================================
        public Dictionary<string, int> GetUserActivitySummary(int customerId)
        {
            var summary = new Dictionary<string, int>();

            using (var conn = new SqlConnection(_connectionString))
            {
                var sql = @"
            SELECT HANHDONG, COUNT(*) AS ActionCount
            FROM AUDIT_LOG
            WHERE MATK = @CustomerId
            GROUP BY HANHDONG
            ORDER BY ActionCount DESC";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@CustomerId", customerId);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            summary[reader.GetString(0)] = reader.GetInt32(1);
                        }
                    }
                }
            }

            return summary;
        }

        // =========================================
        // Get Recent Login History
        // =========================================
        public List<AUDIT_LOG> GetRecentLogins(int customerId, int limit = 10)
        {
            var logs = new List<AUDIT_LOG>();

            using (var conn = new SqlConnection(_connectionString))
            {
                var sql = @"
            SELECT TOP (@Limit) 
                MALOG, MATK, HANHDONG, BANGTACDONG, NGAYGIO, IP
            FROM AUDIT_LOG
            WHERE MATK = @CustomerId 
            AND HANHDONG IN ('LOGIN', 'LOGOUT')
            ORDER BY NGAYGIO DESC";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@CustomerId", customerId);
                    cmd.Parameters.AddWithValue("@Limit", limit);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            logs.Add(new AUDIT_LOG
                            {
                                MALOG = reader.GetInt32(0),
                                MATK = reader.GetInt32(1),
                                HANHDONG = reader.GetString(2),
                                BANGTACDONG = reader.IsDBNull(3) ? "" : reader.GetString(3),
                                NGAYGIO = reader.GetDateTime(4),
                                IP = reader.IsDBNull(5) ? "" : reader.GetString(5)
                            });
                        }
                    }
                }
            }

            return logs;
        }
    }
}