using MediatR;

namespace DPMS_WebAPI.Events.EventModels
{
    public abstract class BaseNotification<T> : INotification
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public T Data { get; set; }

        protected BaseNotification(T data)
        {
            Data = data;
        }
    }
}
