using System;

namespace WebCar.Models
{
    public class ORDER_VIEW
    {
        public int MADON { get; set; }
        public int MAKH { get; set; }
        public DateTime NGAYDAT { get; set; }
        public decimal TONGTIEN { get; set; }
        public string TRANGTHAI { get; set; }

        // Additional properties
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        public int MAXE { get; set; }
        public string TENXE { get; set; }
        public string HANGXE { get; set; }
        public int SOLUONG { get; set; }
        public decimal DONGIA { get; set; }
    }
}