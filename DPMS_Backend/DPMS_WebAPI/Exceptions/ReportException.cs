namespace DPMS_WebAPI.Exceptions
{
    public class ReportException : Exception
    {
        public int StatusCode { get; }
        public int Status { get; }

        public ReportException(string message, int statusCode = 400) : base(message)
        {
            StatusCode = statusCode;
        }

        public ReportException(string message, int statusCode = 400, int status = -1) : base(message)
        {
            StatusCode = statusCode;
            Status = status;
        }

        public ReportException(string message) : base(message)
        {
            StatusCode = 400;
            Status = -1;
        }
    }

    public class ReportEmptyException : ReportException
    {
        public ReportEmptyException() : base("Không có dữ liệu in báo cáo")
        {
        }

        public ReportEmptyException(string message, int statusCode = 400) : base(message, statusCode)
        {
        }

        public ReportEmptyException(string message, int statusCode = 400, int status = -1) : base(message, statusCode,
            status)
        {
        }
    }
}