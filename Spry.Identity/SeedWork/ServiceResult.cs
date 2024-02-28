namespace Spry.Identity.SeedWork
{
    public class ServiceResultBase
    {
        public int Id { get; set; }
        public string Message { get; set; } = default!;

        //public bool IsSuccessful { get; set; }
        public List<string> Errors { get; set; } = new List<string>();

        public bool HasErrors => Errors.Any();
    }

    public class ServiceResult<T> : ServiceResultBase
    {
        public T Entity { get; set; }

#nullable disable
        public ServiceResult()
        {

        }

        public ServiceResult(string errorMessage)
        {
            Errors.Add(errorMessage);
        }

        public ServiceResult(IList<string> errors)
        {
            Errors.AddRange(errors);
        }

        public ServiceResult(T entity)
        {
            Entity = entity;
        }
    }

    public class ServiceResult : ServiceResultBase
    {
        public object Entity { get; set; }

        public ServiceResult()
        {

        }

        public ServiceResult(string errorMessage)
        {
            Errors.Add(errorMessage);
        }

        public ServiceResult(IList<string> errors)
        {
            Errors.AddRange(errors);
        }

        public ServiceResult(object entity)
        {
            Entity = entity;
        }
    }
}
