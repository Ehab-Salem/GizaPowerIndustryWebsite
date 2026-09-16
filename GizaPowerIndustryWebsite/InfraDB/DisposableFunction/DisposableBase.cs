using Microsoft.AspNetCore.Mvc;

namespace WInfraDB.DisposableFunction
{
    public abstract class ControllerDisposableBase : Controller, IDisposable
    {

        public enum NotificationType
        {
            error,
            success,
            warning,
            info
        }


        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // ✅ هنا تحرر الـ Managed Resources
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        public void Alert(string message, NotificationType notificationType)
        {
            var msg = "Swal.fire('" + notificationType.ToString().ToUpper() + "', '" + message + "','" + notificationType + "')" + "";
            TempData["notification"] = msg;
        }
    }


  

    
}
