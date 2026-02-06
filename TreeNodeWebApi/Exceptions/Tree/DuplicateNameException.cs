using TreeNodeWebApi.Exceptions.Base;

namespace TreeNodeWebApi.Exceptions.Tree
{
    /// <summary>
    /// Исключение при нарушении уникальности
    /// </summary>
    public class DuplicateNameException : ApiException
    {
        public DuplicateNameException(string message = "Нарушена уникальности имени.")
            : base(StatusCodes.Status400BadRequest, message)
        {
        }
    }
}
