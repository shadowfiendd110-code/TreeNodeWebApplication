using TreeNodeWebApi.Exceptions.Base;

namespace TreeNodeWebApi.Exceptions.Tree
{
    /// <summary>
    /// Исключение при циклической ссылке
    /// </summary>
    public class CyclicReferenceException : ApiException
    {
        public CyclicReferenceException(string message = "Циклическая ссылка.")
            : base(StatusCodes.Status400BadRequest, message)
        {
        }
    }
}
