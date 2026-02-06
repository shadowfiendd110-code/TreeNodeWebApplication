using TreeNodeWebApi.Exceptions.Base;

namespace TreeNodeWebApi.Exceptions.Tree
{
    /// <summary>
    /// Исключение при ненайденном ресурсе
    /// </summary>
    public class NotFoundException : ApiException
    {
        /// <summary>
        /// Инициализация исключения.
        /// </summary>
        /// <param name="message">Сообщение об ошибке.</param>
        public NotFoundException(string message = "Ресурс не найден.")
            : base(StatusCodes.Status404NotFound, message)
        {
        }
    }
}
