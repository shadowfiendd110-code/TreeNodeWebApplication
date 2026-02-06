using TreeNodeWebApi.Exceptions.Base;

namespace TreeNodeWebApi.Exceptions.Tree
{
    /// <summary>
    /// Исключение при отсутствии доступа
    /// </summary>
    public class ForbiddenException : ApiException
    {
        public ForbiddenException(string message = "Отсутствует доступ.")
            : base(StatusCodes.Status403Forbidden, message)
        {
        }
    }
}
